using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Microsoft.Extensions.Logging;
using Weikio.EventFramework.EventCreator;
using Weikio.EventFramework.Tests.Shared;
using Xunit.Abstractions;

namespace Weikio.EventFramework.Channels.Dataflow.UnitTests
{
    public abstract class TestBase
    {
        protected  readonly ITestOutputHelper _output;
        protected ILoggerFactory _loggerFactory;

        protected TestBase(ITestOutputHelper output)
        {
            _output = output;

            _loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddXUnit(output);
            });
        }
        
        protected  static List<InvoiceCreated> CreateObjects(int count = 500)
        {
            var evs = new List<InvoiceCreated>();

            for (var i = 0; i < count; i++)
            {
                evs.Add(new InvoiceCreated() { Index = i });
            }

            return evs;
        }

        protected  static List<InvoiceCreated> CreateManyObjects()
        {
            return CreateObjects(50000);
        }

        protected  static List<CloudEvent> CreateEvents()
        {
            return CreateObjects().Select(x => CloudEventCreator.Create(x)).ToList();
        }

        protected async Task ContinueWhen(Func<bool> probe, string assertErrorMessage = null, TimeSpan? timeout = null)
        {
            if (timeout == null)
            {
                timeout = TimeSpan.FromSeconds(3);
            }

            var cts = new CancellationTokenSource();
            cts.CancelAfter(timeout.GetValueOrDefault());

            var success = false;

            while (cts.IsCancellationRequested == false)
            {
                success = probe();

                if (success)
                {
                    break;
                }

                if (cts.IsCancellationRequested)
                {
                    break;
                }

                await Task.Delay(TimeSpan.FromMilliseconds(50));
            }
        }
    }
}
