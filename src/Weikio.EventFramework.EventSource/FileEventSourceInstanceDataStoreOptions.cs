using System;
using System.IO;
using System.Reflection;
using Weikio.EventFramework.EventSource.Abstractions;

namespace Weikio.EventFramework.EventSource
{
    public class FileEventSourceInstanceDataStoreOptions
    {
        public Func<EventSourceInstance, string> GetRootPath { get; set; } = eventSourceInstance =>
        {
            var entryAssembly = Assembly.GetEntryAssembly().GetName().Name ?? "";
            var result = Path.Combine(Path.GetTempPath(), "eventframework", entryAssembly, eventSourceInstance.Id);

            return result;
        };
    }
}