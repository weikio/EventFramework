using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventFrameworkTestBed;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Weikio.EventFramework.Abstractions;
using Weikio.EventFramework.IntegrationFlow.CloudEvents;
using Weikio.EventFramework.IntegrationTests.EventSource.Sources;
using Weikio.EventFramework.IntegrationTests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace Weikio.EventFramework.IntegrationTests.IntegrationFlow
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

            var flowBuilder = IntegrationFlowBuilder.From<NumberEventSource>()
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

            var manager = server.GetRequiredService<ICloudEventsIntegrationFlowManager>();
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

            var flowBuilder = IntegrationFlowBuilder.From<NumberEventSource>(options =>
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

            var manager = server.GetRequiredService<ICloudEventsIntegrationFlowManager>();
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

            var flowBuilder = IntegrationFlowBuilder.From<NumberEventSource>(options =>
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

            var manager = server.GetRequiredService<ICloudEventsIntegrationFlowManager>();
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
    }
}
