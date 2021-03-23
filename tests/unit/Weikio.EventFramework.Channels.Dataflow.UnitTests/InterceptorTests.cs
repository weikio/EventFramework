using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Transactions;
using CloudNative.CloudEvents;
using Weikio.EventFramework.Abstractions;
using Weikio.EventFramework.Channels.Dataflow.CloudEvents;
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
            var options = new CloudEventsDataflowChannelOptions() { Name = "name", };

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

            var options = new CloudEventsDataflowChannelOptions() { Name = "name", };

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
        public async Task CanAddInterfacePreAdapters()
        {
            var options = new CloudEventsDataflowChannelOptions() { Name = "name", };

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
        public async Task CanAddInterfacePostAdapters()
        {
            var options = new CloudEventsDataflowChannelOptions() { Name = "name", };

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
        public async Task CanAddInterfacePreComponents()
        {
            var options = new CloudEventsDataflowChannelOptions() { Name = "name", };
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
        public async Task CanAddInterfacePreEndpoints()
        {
            var options = new CloudEventsDataflowChannelOptions() { Name = "name", };

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
            var options = new CloudEventsDataflowChannelOptions() { Name = "name", };
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
            var options = new CloudEventsDataflowChannelOptions() { Name = "name", };
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

        public class MyInterceptor : IDataflowChannelInterceptor
        {
            public List<object> Objs { get; set; } = new();
            public object Received = null;

            public Task<object> Intercept(object obj)
            {
                Objs.Add(obj);
                Received = obj;

                return Task.FromResult(obj);
            }
        }

        public class TransformInterceptor : IDataflowChannelInterceptor
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

        public class ModifierInterceptor : IDataflowChannelInterceptor
        {
            public Task<object> Intercept(object obj)
            {
                var custCreated = new CustomerCreated(Guid.NewGuid(), "test", "test");

                return Task.FromResult<object>(custCreated);
            }
        }

        public class BeforeReceiveInterceptor : IDataflowChannelInterceptor<object, CloudEvent>
        {
            public List<object> InterceptedObjectsBeforeReceive { get; set; } = new();

            public Task OnPreReceive(object obj)
            {
                InterceptedObjectsBeforeReceive.Add(obj);

                return Task.CompletedTask;
            }
        }

        public class BeforeAdaptersInterceptor : IDataflowChannelInterceptor<object, CloudEvent>
        {
            public List<object> InterceptedObjectsBeforeReceive { get; set; } = new();

            public Task OnPreReceive(object obj)
            {
                InterceptedObjectsBeforeReceive.Add(obj);

                return Task.CompletedTask;
            }
        }

        public class BeforeComponentsInterceptor : IDataflowChannelInterceptor<object, CloudEvent>
        {
            public List<object> InterceptedObjectsBeforeReceive { get; set; } = new();

            public Task OnPreReceive(object obj)
            {
                InterceptedObjectsBeforeReceive.Add(obj);

                return Task.CompletedTask;
            }
        }

        public class BeforeEndspointsInterceptor : IDataflowChannelInterceptor<object, CloudEvent>
        {
            public List<object> InterceptedObjectsBeforeReceive { get; set; } = new();

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
