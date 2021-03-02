namespace Weikio.EventFramework.Channels.Dataflow
{
    public class DataflowChannelBuilder : IChannelBuilder
    {
        public IChannel Create(string channelName = ChannelName.Default)
        {
            return new DataflowChannel(channelName);
        }
    }
}
