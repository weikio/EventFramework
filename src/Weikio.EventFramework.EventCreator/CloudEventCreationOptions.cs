﻿using System;
using CloudNative.CloudEvents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Weikio.EventFramework.EventCreator
{
    public class CloudEventCreationOptions
    {
        public string EventTypeName { get; set; }
        public CloudEventsSpecVersion SpecVersion { get; set; } = CloudEventsSpecVersion.V1_0;
        public Uri Source { get; set; } = new Uri("http://localhost/eventframework");
        public Func<CloudEventCreationOptions, IServiceProvider, object, string> GetEventTypeName { get; set; } = (options, provider, o) =>
        {
            if (!string.IsNullOrWhiteSpace(options.EventTypeName))
            {
                return options.EventTypeName;
            }

            return o.GetType().Name;
        };

        public string Subject { get; set; } = string.Empty;
        public string DataContentType { get; set; } = "Application/Json";

        public Func<CloudEventCreationOptions, IServiceProvider, object, string> GetDataContentType { get; set; } =
            (options, provider, o) => options.DataContentType;

        public Func<CloudEventCreationOptions, IServiceProvider, object, string> GetSubject { get; set; } = (options, provider, o) => options.Subject;
        public Func<CloudEventCreationOptions, IServiceProvider, object, string> GetId { get; set; } = (options, provider, o) => Guid.NewGuid().ToString();
        public Func<CloudEventCreationOptions, IServiceProvider, object, Uri> GetSource { get; set; } = (options, provider, o) =>
        {
            if (options?.Source != null)
            {
                return options.Source;
            }

            var optionsMonitor = provider.GetRequiredService<IOptionsMonitor<CloudEventCreationOptions>>();
            var defaultOptions = optionsMonitor.CurrentValue;

            return defaultOptions.Source;
        };

        public Func<CloudEventCreationOptions, IServiceProvider, object, ICloudEventExtension[]> GetExtensions { get; set; } =
            (options, provider, o) => Array.Empty<ICloudEventExtension>();
    }
}