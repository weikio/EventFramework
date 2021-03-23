﻿using System;
using System.Threading.Tasks;
using CloudNative.CloudEvents;

namespace Weikio.EventFramework.Channels.Dataflow
{
    public class Endpoint<TOutput>
    {
        public Func<TOutput, Task> Func { get; private set; }
        public Predicate<TOutput> Predicate { get; private set; }

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
            return new(func);
        }

        public static implicit operator Endpoint<TOutput>((Func<TOutput, Task> Func, Predicate<TOutput> Predicate) def)
        {
            return new(def.Func, def.Predicate);
        }
    }
}
