namespace Weikio.EventFramework.Channels.Dataflow
{
    public class BatchItem
    {
        public object Object { get; set; }
        public int Sequence { get; set; }

        public BatchItem(object o, int sequence)
        {
            Object = o;
            Sequence = sequence;
        }
    }
}
