﻿using System;
using CloudNative.CloudEvents;
using Weikio.EventFramework.EventFlow.CloudEvents.Components;

// ReSharper disable once CheckNamespace
namespace Weikio.EventFramework.EventFlow.CloudEvents
{
    public static class EventFlowBranchBuilderExtensions
    {
        public static EventFlowBuilder Branch(this EventFlowBuilder builder,
            params (Predicate<CloudEvent> Predicate, Action<EventFlowBuilder> BuildBranch)[] branches)
        {
            var componentBuilder = new BranchComponentBuilder(branches);
            builder.Component(componentBuilder);

            return builder;
        }
    }
}
