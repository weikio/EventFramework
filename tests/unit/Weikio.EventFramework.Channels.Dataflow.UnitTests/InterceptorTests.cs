using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Transactions;
using CloudNative.CloudEvents;
using Weikio.EventFramework.Abstractions;
using Weikio.EventFramework.Channels.Abstractions;
using Weikio.EventFramework.Channels.Dataflow.Abstractions;
using Weikio.EventFramework.Channels.CloudEvents;
using Weikio.EventFramework.EventCreator;
using Weikio.EventFramework.Tests.Shared;
using Xunit;
using Xunit.Abstractions;

namespace Weikio.EventFramework.Channels.Dataflow.UnitTests
{
    public class InterceptorTests : TestBase
    {
        public InterceptorTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task CanAddInterceptorPreReceive()
        {
            var options = new CloudEventsChannelOptions() { Name = "name", };

            var interceptor = new MyInterceptor();
            options.Interceptors.Add((InterceptorTypeEnum.PreReceive, interceptor));

            await using (var channel = new CloudEventsChannel(options))
            {
                await channel.Send(new InvoiceCreated());
            }

            Assert.Single(interceptor.Objs);
        }

        [Fact]
        public async Task PreReceiveInterceptorCanModify()
        {
            CloudEvent<CustomerCreated> receivedEvent = null;

            var options = new CloudEventsChannelOptions() { Name = "name", };

            Task Receive(CloudEvent ev)
            {
                receivedEvent = ev.To<CustomerCreated>();

                return Task.CompletedTask;
            }

            options.Endpoints.Add((Receive, null));
            var interceptor = new ModifierInterceptor();
            options.Interceptors.Add((InterceptorTypeEnum.PreReceive, interceptor));

            await using (var channel = new CloudEventsChannel(options))
            {
                await channel.Send(new InvoiceCreated());
            }

            Assert.Equal("test", receivedEvent.Object.FirstName);
        }

        [Fact]
        public async Task CanAddInterceptorPreAdapters()
        {
            var options = new CloudEventsChannelOptions() { Name = "name", };

            var interceptor = new ModifierInterceptor();
            var interceptor2 = new MyInterceptor();

            options.Interceptors.Add((InterceptorTypeEnum.PreAdapters, interceptor));
            options.Interceptors.Add((InterceptorTypeEnum.PostAdapters, interceptor2));

            await using (var channel = new CloudEventsChannel(options))
            {
                await channel.Send(new InvoiceCreated());
            }

            var msg = (CloudEvent) interceptor2.Received;
            var ev = msg.To<CustomerCreated>();

            Assert.Equal("test", ev.Object.FirstName);
        }

        [Fact]
        public async Task CanAddInterceptorPostAdapters()
        {
            var options = new CloudEventsChannelOptions() { Name = "name", };

            var interceptor = new TransformInterceptor(o =>
            {
                var customerCreated = new CustomerCreated(Guid.NewGuid(), "test", "test");

                return Task.FromResult<object>(CloudEventCreator.Create(customerCreated));
            });
            
            options.Interceptors.Add((InterceptorTypeEnum.PostAdapters, interceptor));

            CloudEvent received = null;
            
            options.Endpoint = ev =>
            {
                received = ev;
            };
            
            await using (var channel = new CloudEventsChannel(options))
            {
                await channel.Send(new InvoiceCreated());
            }

            var ev2 = received.To<CustomerCreated>();

            Assert.Equal("test", ev2.Object.FirstName);
        }

        [Fact]
        public async Task CanAddInterceptorPreComponents()
        {
            var options = new CloudEventsChannelOptions() { Name = "name", };
            CloudEvent<CustomerCreated> receivedEvent = null;

            options.Endpoint = ev =>
            {
                receivedEvent = ev.To<CustomerCreated>();
            };

            var interceptor = new TransformInterceptor(o => Task.FromResult<object>(CloudEventCreator.Create(new CustomerCreated(Guid.NewGuid(), "test", "test"))));
            
            options.Interceptors.Add((InterceptorTypeEnum.PreComponents, interceptor));

            await using (var channel = new CloudEventsChannel(options))
            {
                await channel.Send(new InvoiceCreated());
            }

            Assert.Equal("test", receivedEvent.Object.FirstName);
        }

        [Fact]
        public async Task CanAddInterceptorPreEndpoints()
        {
            var options = new CloudEventsChannelOptions() { Name = "name", };

            var interceptor = new MyInterceptor();
            options.Interceptors.Add((InterceptorTypeEnum.PreEndpoints, interceptor));

            await using (var channel = new CloudEventsChannel(options))
            {
                await channel.Send(new InvoiceCreated());
            }

            Assert.Single(interceptor.Objs);
        }

        [Fact]
        public async Task CanAddInterceptorRuntime()
        {
            var options = new CloudEventsChannelOptions() { Name = "name", };
            var counter = 0;

            options.Endpoint = ev =>
            {
                counter += 1;
            };
            
            var interceptor = new MyInterceptor();

            await using (var channel = new CloudEventsChannel(options))
            {
                await channel.Send(new InvoiceCreated());
                await ContinueWhen(() => counter == 1);

                channel.AddInterceptor((InterceptorTypeEnum.PostReceive, interceptor));
                await channel.Send(new InvoiceCreated());
                await ContinueWhen(() => counter == 2);
            }

            Assert.Single(interceptor.Objs);
        }

        [Fact]
        public async Task CanRemoveIntercept()
        {
            var options = new CloudEventsChannelOptions() { Name = "name", };
            var counter = 0;

            options.Endpoint = ev =>
            {
                counter += 1;
            };
            
            var interceptor = new MyInterceptor();

            await using (var channel = new CloudEventsChannel(options))
            {
                await channel.Send(new InvoiceCreated());
                await ContinueWhen(() => counter == 1);

                channel.AddInterceptor((InterceptorTypeEnum.PostReceive, interceptor));
                await channel.Send(new InvoiceCreated());
                await ContinueWhen(() => counter == 2);
                
                channel.RemoveInterceptor((InterceptorTypeEnum.PostReceive, interceptor));
                await channel.Send(new InvoiceCreated());
                await ContinueWhen(() => counter == 3);
            }

            Assert.Single(interceptor.Objs);
        }
        
                
        [Fact]
        public async Task CanAddInterceptorPreEachComponent()
        {
            var options = new CloudEventsChannelOptions() { Name = "name", };

            options.Components.Add(new CloudEventsComponent(ev =>
            {
                return Task.FromResult(ev);
            }));
            
            options.Components.Add(new CloudEventsComponent(ev =>
            {
                return Task.FromResult(ev);
            }));

            var interceptor = new CounterInterceptor();
            
            options.Interceptors.Add((InterceptorTypeEnum.PreComponent, interceptor));

            await using (var channel = new CloudEventsChannel(options))
            {
                await channel.Send(new InvoiceCreated());
            }

            Assert.Equal(2, interceptor.Counter);
        }
        
        [Fact]
        public async Task CanAddInterceptorAfterEachComponent()
        {
            var options = new CloudEventsChannelOptions() { Name = "name", };

            options.Components.Add(new CloudEventsComponent(ev =>
            {
                return Task.FromResult(ev);
            }));
            
            options.Components.Add(new CloudEventsComponent(ev =>
            {
                return Task.FromResult(ev);
            }));

            var interceptor = new CounterInterceptor();
            
            options.Interceptors.Add((InterceptorTypeEnum.PreComponent, interceptor));
            options.Interceptors.Add((InterceptorTypeEnum.PostComponent, interceptor));

            await using (var channel = new CloudEventsChannel(options))
            {
                await channel.Send(new InvoiceCreated());
            }

            Assert.Equal(4, interceptor.Counter);
        }

        public class MyInterceptor : IChannelInterceptor
        {
            public List<object> Objs { get; set; } = new List<object>();
            public object Received = null;

            public Task<object> Intercept(object obj)
            {
                Objs.Add(obj);
                Received = obj;

                return Task.FromResult(obj);
            }
        }
        
        public class CounterInterceptor : IChannelInterceptor
        {
            public int Counter { get; private set; }

            public CounterInterceptor()
            {
            }

            public Task<object> Intercept(object obj)
            {
                Counter += 1;

                return Task.FromResult(obj);
            }
        }

        public class TransformInterceptor : IChannelInterceptor
        {
            private readonly Func<object, Task<object>> _func;

            public TransformInterceptor(Func<object, Task<object>> func)
            {
                _func = func;
            }

            public Task<object> Intercept(object obj)
            {
                return _func.Invoke(obj);
            }
        }

        public class ModifierInterceptor : IChannelInterceptor
        {
            public Task<object> Intercept(object obj)
            {
                var custCreated = new CustomerCreated(Guid.NewGuid(), "test", "test");

                return Task.FromResult<object>(custCreated);
            }
        }

        public class BeforeReceiveInterceptor : IDataflowChannelInterceptor<object, CloudEvent>
        {
            public List<object> InterceptedObjectsBeforeReceive { get; set; } = new List<object>();

            public Task OnPreReceive(object obj)
            {
                InterceptedObjectsBeforeReceive.Add(obj);

                return Task.CompletedTask;
            }
        }

        public class BeforeAdaptersInterceptor : IDataflowChannelInterceptor<object, CloudEvent>
        {
            public List<object> InterceptedObjectsBeforeReceive { get; set; } = new List<object>();

            public Task OnPreReceive(object obj)
            {
                InterceptedObjectsBeforeReceive.Add(obj);

                return Task.CompletedTask;
            }
        }

        public class BeforeComponentsInterceptor : IDataflowChannelInterceptor<object, CloudEvent>
        {
            public List<object> InterceptedObjectsBeforeReceive { get; set; } = new List<object>();

            public Task OnPreReceive(object obj)
            {
                InterceptedObjectsBeforeReceive.Add(obj);

                return Task.CompletedTask;
            }
        }

        public class BeforeEndspointsInterceptor : IDataflowChannelInterceptor<object, CloudEvent>
        {
            public List<object> InterceptedObjectsBeforeReceive { get; set; } = new List<object>();

            public Task OnPreReceive(object obj)
            {
                InterceptedObjectsBeforeReceive.Add(obj);

                return Task.CompletedTask;
            }

            public InvoiceCreated Hello()
            {
                return default;
            }

            public List<object> InterceptedObjectsBeforeReceive2 { get; set; } = default;
        }
    }
}
