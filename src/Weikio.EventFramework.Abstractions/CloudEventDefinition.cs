﻿using System;
using System.Collections.Generic;
using System.Net.Mime;

namespace Weikio.EventFramework.Abstractions
{
    public class CloudEventDefinition
    {
        public string Type { get; set; }
        public string Source { get; set; }
        public string SpecVersion { get; set; } = "1.0";
        public string DataContentType { get; set; } = "application/json";
        public Uri DataSchemaUri { get; set; }
        public string DataSchema { get; set; }
        public List<string> Tags { get; set; } = new List<string>();

        public CloudEventDefinition()
        {
        }

        public CloudEventDefinition(string type, string source, Uri dataSchemaUri = null, string dataSchema = null, string specVersion = null,
            string contentType = null, List<string> tags = null)
        {
            Type = type;
            Source = source;
            SpecVersion = specVersion ?? "1.0";
            DataSchemaUri = dataSchemaUri;
            DataSchema = dataSchema;
            DataContentType = contentType ?? "application/json";
            Tags = tags ?? new List<string>();
        }

        protected bool Equals(CloudEventDefinition other)
        {
            return Type == other.Type && Source == other.Source && SpecVersion == other.SpecVersion;
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

            return Equals((CloudEventDefinition) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Type, Source, SpecVersion);
        }

        public static bool operator ==(CloudEventDefinition left, CloudEventDefinition right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(CloudEventDefinition left, CloudEventDefinition right)
        {
            return !Equals(left, right);
        }
    }
}
