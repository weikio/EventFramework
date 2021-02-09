using System;
using System.Text;

namespace Weikio.EventFramework.EventSource.Abstractions
{
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
        
        public static implicit operator EventSourceDefinition(string name)
        {
            return new EventSourceDefinition(name, Version.Parse("1.0.0.0"));
        }
        
        public static implicit operator EventSourceDefinition((string Name, Version Version) nameAndVersion)
        {
            return new EventSourceDefinition(nameAndVersion.Name, nameAndVersion.Version);
        }

        public static implicit operator EventSourceDefinition((string Name, string Version) nameAndVersion)
        {
            return new EventSourceDefinition(nameAndVersion.Name, Version.Parse(nameAndVersion.Version));
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
