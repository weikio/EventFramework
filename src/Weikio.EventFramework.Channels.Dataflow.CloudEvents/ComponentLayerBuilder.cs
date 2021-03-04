using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks.Dataflow;
using CloudNative.CloudEvents;

namespace Weikio.EventFramework.Channels.Dataflow.CloudEvents
{
    public class ComponentLayerBuilder
    {
        public DataflowLayerGeneric<CloudEvent, CloudEvent> Build(DataflowChannelOptionsBase<object, CloudEvent> options)
        {
            var builder = new SequentialLayerBuilder<CloudEvent>();

            // // Todo: Interceptors should be created on DataflowChannelBuilder
            // var preInterceptorComponent = new DataflowChannelComponent<CloudEvent>(async ev =>
            // {
            //     foreach (var interceptor in options.Interceptors.Where(x => x.Item1 == InterceptorTypeEnum.PreComponents))
            //     {
            //         ev = (CloudEvent) await interceptor.Interceptor.Intercept(ev);
            //     }
            //
            //     return ev;
            // });
            //
            // var postInterceptorComponent = new DataflowChannelComponent<CloudEvent>(async ev =>
            // {
            //     foreach (var interceptor in options.Interceptors.Where(x => x.Item1 == InterceptorTypeEnum.PostComponent))
            //     {
            //         ev = (CloudEvent) await interceptor.Interceptor.Intercept(ev);
            //     }
            //
            //     return ev;
            // });

            // var allComponents = new List<DataflowChannelComponent<CloudEvent>> { preInterceptorComponent };
            // allComponents.AddRange(options.Components);
            // allComponents.Add(postInterceptorComponent);

            return builder.Build(options.Components);
        }
    }
}
