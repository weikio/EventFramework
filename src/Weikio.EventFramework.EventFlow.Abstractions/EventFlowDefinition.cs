using System;

namespace Weikio.EventFramework.EventFlow.Abstractions
{
    public class EventFlowDefinition
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Version Version { get; set; }

        public EventFlowDefinition()
        {
        }

        public EventFlowDefinition(string name, Version version) : this(name, string.Empty, version)
        {
        }

        public EventFlowDefinition(string name, string description, Version version)
        {
            Name = name;
            Description = description;
            Version = version;
        }

        public override string ToString()
        {
            return $"{Name}: {Version} {Description}".Trim();
        }
        
        public static implicit operator EventFlowDefinition(string name)
        {
            return new EventFlowDefinition(name, Version.Parse("1.0.0"));
        }
        
        public static implicit operator EventFlowDefinition((string Name, Version Version) nameAndVersion)
        {
            return new EventFlowDefinition(nameAndVersion.Name, nameAndVersion.Version);
        }

        public static implicit operator EventFlowDefinition((string Name, string Version) nameAndVersion)
        {
            return new EventFlowDefinition(nameAndVersion.Name, Version.Parse(nameAndVersion.Version));
        }
        protected bool Equals(EventFlowDefinition other)
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

            return Equals((EventFlowDefinition)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Version);
        }
    }
}
