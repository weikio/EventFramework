using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using EventFrameworkTestBed;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Weikio.EventFramework.Abstractions;
using Weikio.EventFramework.EventFlow.CloudEvents;
using Weikio.EventFramework.IntegrationTests.EventSource.Sources;
using Weikio.EventFramework.IntegrationTests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace Weikio.EventFramework.IntegrationTests.EventFlow
{
    public class BranchTests : IntegrationFlowTestBase
    {
        public BranchTests(WebApplicationFactory<Startup> factory, ITestOutputHelper output) : base(factory, output)
        {
        }

        [Fact]
        public async Task CanBranch()
        {
            var server = Init();
            var dividableByTwo = new List<int>();
            var defaultCounter = new List<int>();

            var flowBuilder = EventFlowBuilder.From<NumberEventSource>()
                .Branch((ev =>
                {
                    var number = ev.To<CounterEvent>().Object.CurrentCount;

                    if ((decimal)number % 2 == 0)
                    {
                        return true;
                    }

                    return false;
                }, flow =>
                {
                    flow.Handle(ev =>
                    {
                        dividableByTwo.Add(ev.To<CounterEvent>().Object.CurrentCount);
                    });
                }))
                .Handle(ev =>
                {
                    defaultCounter.Add(ev.To<CounterEvent>().Object.CurrentCount);
                });

            var flow = await flowBuilder.Build(server);

            var manager = server.GetRequiredService<ICloudEventFlowManager>();
            await manager.Execute(flow);

            await ContinueWhen(() => dividableByTwo.Count > 0 && defaultCounter.Count > 0, timeout: TimeSpan.FromSeconds(5));

            foreach (var i in dividableByTwo)
            {
                Assert.Equal(0, (decimal)i % 2);
            }

            foreach (var i in defaultCounter)
            {
                Assert.NotEqual(0, (decimal)i % 2);
            }
        }

        [Fact]
        public async Task CanMultiBranch()
        {
            var server = Init();
            var dividableByTwo = new List<int>();
            var dividableByThree = new List<int>();
            var defaultCounter = new List<int>();

            var flowBuilder = EventFlowBuilder.From<NumberEventSource>(options =>
                {
                    options.PollingFrequency = TimeSpan.FromSeconds(1);
                    options.Autostart = true;
                })
                .Branch((ev =>
                    {
                        var number = ev.To<CounterEvent>().Object.CurrentCount;

                        if ((decimal)number % 2 == 0)
                        {
                            return true;
                        }

                        return false;
                    }, flow =>
                    {
                        flow.Handle(ev =>
                        {
                            dividableByTwo.Add(ev.To<CounterEvent>().Object.CurrentCount);
                        });
                    }),
                    (ev =>
                    {
                        var number = ev.To<CounterEvent>().Object.CurrentCount;

                        if ((decimal)number % 3 == 0)
                        {
                            return true;
                        }

                        return false;
                    }, flow =>
                    {
                        flow.Handle(ev =>
                        {
                            dividableByThree.Add(ev.To<CounterEvent>().Object.CurrentCount);
                        });
                    }))
                .Handle(ev =>
                {
                    defaultCounter.Add(ev.To<CounterEvent>().Object.CurrentCount);
                });

            var flow = await flowBuilder.Build(server);

            var manager = server.GetRequiredService<ICloudEventFlowManager>();
            await manager.Execute(flow);

            await ContinueWhen(() => dividableByTwo.Count > 0 && dividableByThree.Count > 0 && defaultCounter.Count > 0, timeout: TimeSpan.FromSeconds(5));

            foreach (var i in dividableByTwo)
            {
                Assert.Equal(0, (decimal)i % 2);
            }

            foreach (var i in dividableByThree)
            {
                Assert.Equal(0, (decimal)i % 3);
            }

            foreach (var i in defaultCounter)
            {
                Assert.NotEqual(0, (decimal)i % 2);
                Assert.NotEqual(0, (decimal)i % 3);
            }
        }

        [Fact]
        public async Task CanBranchInMultiplePoints()
        {
            var server = Init();
            var dividableByTwo = new List<int>();
            var dividableByThree = new List<int>();
            var defaultCounter = new List<int>();

            var flowBuilder = EventFlowBuilder.From<NumberEventSource>(options =>
                {
                    options.PollingFrequency = TimeSpan.FromSeconds(1);
                    options.Autostart = true;
                })
                .Branch((ev =>
                {
                    var number = ev.To<CounterEvent>().Object.CurrentCount;

                    if ((decimal)number % 2 == 0)
                    {
                        return true;
                    }

                    return false;
                }, flow =>
                {
                    flow.Handle(ev =>
                    {
                        dividableByTwo.Add(ev.To<CounterEvent>().Object.CurrentCount);
                    });
                }))
                .Branch(
                    (ev =>
                    {
                        var number = ev.To<CounterEvent>().Object.CurrentCount;

                        if ((decimal)number % 3 == 0)
                        {
                            return true;
                        }

                        return false;
                    }, flow =>
                    {
                        flow.Handle(ev =>
                        {
                            dividableByThree.Add(ev.To<CounterEvent>().Object.CurrentCount);
                        });
                    }))
                .Handle(ev =>
                {
                    defaultCounter.Add(ev.To<CounterEvent>().Object.CurrentCount);
                });

            var flow = await flowBuilder.Build(server);

            var manager = server.GetRequiredService<ICloudEventFlowManager>();
            await manager.Execute(flow);

            await ContinueWhen(() => dividableByTwo.Count > 0 && dividableByThree.Count > 0 && defaultCounter.Count > 0, timeout: TimeSpan.FromSeconds(5));

            foreach (var i in dividableByTwo)
            {
                Assert.Equal(0, (decimal)i % 2);
            }

            foreach (var i in dividableByThree)
            {
                Assert.Equal(0, (decimal)i % 3);
            }

            foreach (var i in defaultCounter)
            {
                Assert.NotEqual(0, (decimal)i % 2);
                Assert.NotEqual(0, (decimal)i % 3);
            }
        }

        [Fact]
        public async Task CanRunComplexFlowWithBranch()
        {
            var server = Init();
            var logger = server.GetRequiredService<ILogger<BranchTests>>();
            var allRun = false;
            var okbranchCount = 0;
            var mainBranchCount = 0;

            var flowBuilder = EventFlowBuilder.From<NumberEventSource>(options =>
                {
                    options.Autostart = true;
                    options.PollingFrequency = TimeSpan.FromSeconds(1);
                })
                .Branch((ev =>
                {
                    var status = ev.To<CounterEvent>();

                    logger.LogInformation($"Should branch? {status.Id}: {status.Object.CurrentCount} ");
                    return status.Object.CurrentCount == 2;
                }, okBranch =>
                {
                    okBranch.Handle(ev =>
                        {
                            var status = ev.To<CounterEvent>();

                            logger.LogInformation($"Running OK branch: {status.Id}: {status.Object.CurrentCount} ");
                        })
                        .Transform(ev =>
                        {
                            var status = ev.To<CounterEvent>();

                            logger.LogInformation($"OK branch transform: {status.Id}: {status.Object.CurrentCount} ");

                            return ev;
                        })
                        .Handle(ev =>
                        {
                            okbranchCount += 1;

                            var status = ev.To<CounterEvent>();

                            logger.LogInformation($"OK branch last handle: {status.Id}: {status.Object.CurrentCount} ");
                        });
                }))
                .Transform(ev =>
                {
                    mainBranchCount += 1;
                    var status = ev.To<CounterEvent>();

                    logger.LogInformation($"Running main branch: {status.Id}: {status.Object.CurrentCount} ");

                    return ev;
                })
                .Handle(ev =>
                {
                    var status = ev.To<CounterEvent>();

                    logger.LogInformation($"End of main branch: {status.Id}: {status.Object.CurrentCount} ");
                });
            var flow = await flowBuilder.Build(server);
            var manager = server.GetRequiredService<ICloudEventFlowManager>();
            await manager.Execute(flow);

            await Task.Delay(TimeSpan.FromSeconds(5));
            
            Assert.Equal(1, okbranchCount);
        }
    }
}
