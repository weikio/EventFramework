using System;
using System.Threading.Tasks;

namespace Weikio.EventFramework.Channels.Dataflow.Abstractions
{
    public class Endpoint<TOutput>
    {
        public Func<TOutput, Task> Func { get; protected set; }
        public Predicate<TOutput> Predicate { get; protected set; } = ev => true;

        protected Endpoint()
        {
        }

        public Endpoint(Func<TOutput, Task> func, Predicate<TOutput> predicate = null)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            Func = func;
            Predicate = predicate ?? (ev => true);
        }

        public static implicit operator Func<TOutput, Task>(Endpoint<TOutput> component)
        {
            return component.Func;
        }

        public static implicit operator Predicate<TOutput>(Endpoint<TOutput> component)
        {
            return component.Predicate;
        }

        public static implicit operator Endpoint<TOutput>(Func<TOutput, Task> func)
        {
            return new Endpoint<TOutput>(func);
        }

        public static implicit operator Endpoint<TOutput>((Func<TOutput, Task> Func, Predicate<TOutput> Predicate) def)
        {
            return new Endpoint<TOutput>(def.Func, def.Predicate);
        }
    }
}
