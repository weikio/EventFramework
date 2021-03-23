using System;
using System.Threading.Tasks;
using CloudNative.CloudEvents;

namespace Weikio.EventFramework.Channels.Dataflow
{
    public class DataflowChannelComponent<TOutput>
    {
        public Func<TOutput, Task<TOutput>> Func { get; private set; }
        public Predicate<TOutput> Predicate { get; private set; }

        public DataflowChannelComponent(Func<TOutput, TOutput> func, Predicate<TOutput> predicate = null) : this(output => Task.FromResult(func(output)), predicate)
        {
        }
        
        public DataflowChannelComponent(Func<TOutput, Task<TOutput>> func, Predicate<TOutput> predicate = null)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            Func = func;
            Predicate = predicate ?? (ev => true);
        }

        public static implicit operator Func<TOutput, Task<TOutput>>(DataflowChannelComponent<TOutput> component)
        {
            return component.Func;
        }

        public static implicit operator Predicate<TOutput>(DataflowChannelComponent<TOutput> component)
        {
            return component.Predicate;
        }

        public static implicit operator DataflowChannelComponent<TOutput>(Func<TOutput, TOutput> func)
        {
            return new DataflowChannelComponent<TOutput>(func);
        }

        public static implicit operator DataflowChannelComponent<TOutput>((Func<TOutput, TOutput> Func, Predicate<TOutput> Predicate) def)
        {
            return new DataflowChannelComponent<TOutput>(def.Func, def.Predicate);
        }
    }
}
