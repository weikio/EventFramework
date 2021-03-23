namespace Weikio.EventFramework.Channels
{
    public interface IOutgoingChannel : IChannel
    {
    }

    // public static class ChannelBuilderExtensions
    // {
    //     public static ChannelBuilder Channel(this ChannelBuilder channelBuilder, string channelName)
    //     {
    //         
    //     }
    //
    //     public static ChannelBuilder Transform(this ChannelBuilder channelBuilder, Func<CloudEvent, CloudEvent> transform)
    //     {
    //         
    //     }
    //     public static ChannelBuilder Filter(this ChannelBuilder channelBuilder, Predicate<CloudEvent> predicate)
    //     {
    //         
    //     }
    // }

    // public class ChannelBuilder
    // {
    //     private bool _isPubSub = true;
    //     private List<IDataflowBlock> _blocks = new List<IDataflowBlock>();
    //
    //     public static ChannelBuilder Build()
    //     {
    //         return new ChannelBuilder();
    //     }
    //
    //     public IChannel Create()
    //     {
    //         var logger = new TransformBlock<CloudEvent, CloudEvent>(ev =>
    //         {
    //             Debug.WriteLine(ev.ToJson());
    //
    //             return ev;
    //         });
    //     }
    //
    //     public void AddComponent(IDataflowBlock block)
    //     {
    //         
    //     }
    // }
}
