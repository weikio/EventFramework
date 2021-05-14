using Weikio.EventFramework.Channels.Abstractions;

namespace Weikio.EventFramework.Channels.Dataflow
{
    public class DataflowChannelFactory : IChannelFactory
    {
        public IChannel Create()
        {
            return null;
            // return new DataflowChannel();
        }
    }
}
