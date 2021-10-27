using System;
using System.Threading.Tasks;

namespace Weikio.EventFramework.Channels.Dataflow.Abstractions
{
    public class ChannelComponent<TOutput>
    {
        public Func<TOutput, Task<TOutput>> Func { get; protected set; }
        public Predicate<TOutput> Predicate { get; protected set; } = ev => true; 

        protected ChannelComponent() { }

        public ChannelComponent(Func<TOutput, TOutput> func, Predicate<TOutput> predicate = null) : this(output => Task.FromResult(func(output)), predicate)
        {
        }

        public ChannelComponent(Func<TOutput, Task<TOutput>> func, Predicate<TOutput> predicate = null)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            Func = func;
            Predicate = predicate ?? (ev => true);
        }

        public static implicit operator Func<TOutput, Task<TOutput>>(ChannelComponent<TOutput> component)
        {
            return component.Func;
        }

        public static implicit operator Predicate<TOutput>(ChannelComponent<TOutput> component)
        {
            return component.Predicate;
        }

        public static implicit operator ChannelComponent<TOutput>(Func<TOutput, TOutput> func)
        {
            return new ChannelComponent<TOutput>(func);
        }

        public static implicit operator ChannelComponent<TOutput>((Func<TOutput, TOutput> Func, Predicate<TOutput> Predicate) def)
        {
            return new ChannelComponent<TOutput>(def.Func, def.Predicate);
        }
    }
}
