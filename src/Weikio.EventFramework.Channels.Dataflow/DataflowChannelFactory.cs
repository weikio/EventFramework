namespace Weikio.EventFramework.Channels.Dataflow
{
    public class DataflowChannelFactory : IChannelFactory
    {
        public IChannel Create()
        {
            return new DataflowChannel();
        }
    }
}
