using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Weikio.EventFramework.EventSource.EventSourceWrapping;

namespace Weikio.EventFramework.EventSource
{
    public class EsInstance
    {
        public Guid Id { get; }
        public EventSource EventSource { get; }
        public EventSourceStatus Status { get; }
        public TimeSpan? PollingFrequency { get; }

        public string CronExpression { get; }

        public MulticastDelegate Configure { get; }

        public EsInstance(EventSource eventSource, TimeSpan? pollingFrequency, string cronExpression, MulticastDelegate configure)
        {
            EventSource = eventSource;
            PollingFrequency = pollingFrequency;
            CronExpression = cronExpression;
            Configure = configure;
            Status = new EventSourceStatus();
            Id = Guid.NewGuid();
        }
        
        public CancellationTokenSource CancellationTokenSource { get; set; }

    }
    
    public class EventSource
    {
        public EventSourceDefinition EventSourceDefinition { get; }
        public MulticastDelegate Action { get; }
        public Type EventSourceType { get; }
        public object Instance { get; }

        public EventSource(EventSourceDefinition eventSourceDefinition, MulticastDelegate action = null, Type eventSourceType = null, object instance = null)
        {
            EventSourceDefinition = eventSourceDefinition;
            Action = action;
            EventSourceType = eventSourceType;
            Instance = instance;
        }
    }

    public class EventSourceCatalog : List<EventSource>, IEventSourceCatalog
    {
        public Task Initialize(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public List<EventSourceDefinition> List()
        {
            return this.Select(x => x.EventSourceDefinition).ToList();
        }

        public EventSource Get(EventSourceDefinition definition)
        {
            return this.FirstOrDefault(x => x.EventSourceDefinition == definition);
        }

        public EventSource Get(string name, Version version)
        {
            return Get(new EventSourceDefinition(name, version));
        }

        public EventSource Get(string name)
        {
            return Get(name, Version.Parse("1.0.0.0"));
        }
    }

    public interface IEventSourceCatalog
    {
        Task Initialize(CancellationToken cancellationToken);
        List<EventSourceDefinition> List();
        EventSource Get(EventSourceDefinition definition);
        EventSource Get(string name, Version version);
        EventSource Get(string name);
    }
    
    public class EventSourceProvider : List<EventSourceCatalog>
    {
        private readonly ILogger<EventSourceProvider> _logger;

        public EventSourceProvider(IEnumerable<EventSourceCatalog> catalogs, ILogger<EventSourceProvider> logger)
        {
            _logger = logger;
            AddRange(catalogs);
        }
    }
    
    public class EventSourceDefinition
    {


        public EventSourceDefinition()
        {
        }

        public EventSourceDefinition(string name, Version version)
        {
            Name = name;
            Version = version;
        }

        public string Name { get; }
        public Version Version { get; }
        public string Description { get; set; }
        public string ProductVersion { get; set; }

        public override string ToString()
        {
            return $"{Name}: {Version} {GetMoreVersionDetails()}".Trim();
        }

        private string GetMoreVersionDetails()
        {
            if (string.IsNullOrWhiteSpace(Description) && string.IsNullOrWhiteSpace(ProductVersion))
            {
                return string.Empty;
            }

            var result = new StringBuilder("(");

            if (string.IsNullOrWhiteSpace(Description))
            {
                result.Append(ProductVersion);
            }
            else if (string.IsNullOrWhiteSpace(ProductVersion))
            {
                result.Append(Description);
            }
            else
            {
                result.Append($"{ProductVersion}, {Description}");
            }
            
            result.Append(")");

            return result.ToString();
        }

        private bool Equals(EventSourceDefinition other)
        {
            return Name == other.Name && Version.Equals(other.Version);
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

            return Equals((EventSourceDefinition) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Version);
        }

        public static bool operator ==(EventSourceDefinition left, EventSourceDefinition right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(EventSourceDefinition left, EventSourceDefinition right)
        {
            return !Equals(left, right);
        }
    }
}
