using System;
using System.Collections.Generic;
using Weikio.EventFramework.Channels.Dataflow.Abstractions;
using Weikio.EventFramework.EventSource.Abstractions;

namespace Weikio.EventFramework.IntegrationFlow
{
    public abstract class IntegrationFlowBase<TOutput>
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public Action<EventSourceInstanceOptions> ConfigureEventSourceInstanceOptions { get; set; }
        public List<ChannelComponent<TOutput>> Components { get; set; } = new List<ChannelComponent<TOutput>>();
        public List<Endpoint<TOutput>> Endpoints { get; set; } = new List<Endpoint<TOutput>>();
        public List<(InterceptorTypeEnum InterceptorType, IChannelInterceptor Interceptor)> Interceptors { get; set; } =
            new List<(InterceptorTypeEnum InterceptorType, IChannelInterceptor Interceptor)>();
        public object Configuration { get; set; }
        
        public IntegrationFlowDefinition Definition { get; set; }
    }

    public class IntegrationFlowDefinition
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Version Version { get; set; }

        public IntegrationFlowDefinition()
        {
        }

        public IntegrationFlowDefinition(string name, Version version) : this(name, string.Empty, version)
        {
        }

        public IntegrationFlowDefinition(string name, string description, Version version)
        {
            Name = name;
            Description = description;
            Version = version;
        }

        public override string ToString()
        {
            return $"{Name}: {Version} {Description}".Trim();
        }
        
        public static implicit operator IntegrationFlowDefinition(string name)
        {
            return new IntegrationFlowDefinition(name, Version.Parse("1.0.0.0"));
        }
        
        public static implicit operator IntegrationFlowDefinition((string Name, Version Version) nameAndVersion)
        {
            return new IntegrationFlowDefinition(nameAndVersion.Name, nameAndVersion.Version);
        }

        public static implicit operator IntegrationFlowDefinition((string Name, string Version) nameAndVersion)
        {
            return new IntegrationFlowDefinition(nameAndVersion.Name, Version.Parse(nameAndVersion.Version));
        }
        protected bool Equals(IntegrationFlowDefinition other)
        {
            return Name == other.Name && Equals(Version, other.Version);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((IntegrationFlowDefinition)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Version);
        }
    }
}
