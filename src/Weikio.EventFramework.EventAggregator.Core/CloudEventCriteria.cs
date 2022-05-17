using System;
using System.Collections.Generic;
using CloudNative.CloudEvents;

namespace Weikio.EventFramework.EventAggregator.Core
{
    public class CloudEventCriteria : IEquatable<CloudEventCriteria>
    {
        public bool Equals(CloudEventCriteria other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Type == other.Type && Source == other.Source && Subject == other.Subject && DataContentType == other.DataContentType;
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

            return Equals((CloudEventCriteria) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Type, Source, Subject, DataContentType);
        }

        private sealed class CloudEventCriteriaEqualityComparer : IEqualityComparer<CloudEventCriteria>
        {
            public bool Equals(CloudEventCriteria x, CloudEventCriteria y)
            {
                if (ReferenceEquals(x, y))
                {
                    return true;
                }

                if (ReferenceEquals(x, null))
                {
                    return false;
                }

                if (ReferenceEquals(y, null))
                {
                    return false;
                }

                if (x.GetType() != y.GetType())
                {
                    return false;
                }

                return x.Type == y.Type && x.Source == y.Source && x.Subject == y.Subject && x.DataContentType == y.DataContentType;
            }

            public int GetHashCode(CloudEventCriteria obj)
            {
                return HashCode.Combine(obj.Type, obj.Source, obj.Subject, obj.DataContentType);
            }
        }

        public static IEqualityComparer<CloudEventCriteria> CloudEventCriteriaComparer { get; } = new CloudEventCriteriaEqualityComparer();

        public string Type { get; set; }
        public string Source { get; set; }
        public string Subject { get; set; }
        public string DataContentType { get; set; }

        public bool CanHandle(CloudEvent cloudEvent)
        {
            if (!string.IsNullOrWhiteSpace(Type) && !string.Equals(cloudEvent.Type, Type, StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(Source) && !string.Equals(cloudEvent.Source.ToString(), Source, StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(Subject) && !string.Equals(cloudEvent.Subject, Subject, StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(DataContentType) &&
                !string.Equals(cloudEvent.DataContentType.ToString(), DataContentType, StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            return true;
        }

        public static CloudEventCriteria Empty { get; } = new CloudEventCriteria()
        {
            Type = string.Empty, DataContentType = null, Source = string.Empty, Subject = string.Empty
        };
    }
}
