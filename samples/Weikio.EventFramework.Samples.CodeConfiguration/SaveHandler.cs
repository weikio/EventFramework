﻿using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Weikio.EventFramework.Abstractions;

namespace Weikio.EventFramework.Samples.CodeConfiguration
{
    public class LoadHandler
    {
        public Task LoadFile(CloudEvent cloudEvent)
        {
            return Task.CompletedTask;
        }
    }

    public class SaveHandler
    {
        public Task Test(CloudEvent cloudEvent, string eventType = "new-file")
        {
            return Task.CompletedTask;
        }

        public Task NewFile(CloudEvent cloudEvent)
        {
            return Task.CompletedTask;
        }
        
        // public Task SaveFile(CloudEvent cloudEvent, string eventType = "new-file")
        // {
        //     return Task.CompletedTask;
        // }
        //
        // public Task DeleteFile(CloudEvent cloudEvent, string eventType = "delete-file")
        // {
        //     return Task.CompletedTask;
        // }

        public Task CanHandle(ICloudEventContext cloudEventContext)
        {
            throw new System.NotImplementedException();
        }

        public Task Handle(ICloudEventContext cloudEventContext)
        {
            throw new System.NotImplementedException();
        }
    }
}
