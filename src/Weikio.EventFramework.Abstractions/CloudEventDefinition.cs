namespace Weikio.EventFramework.Abstractions
{
    public class CloudEventDefinition
    {
        public string Type { get; set; }
        public string Source { get; set; }
        public string SpecVersion { get; set; } = "1.0";
        public string DataContentType { get; set; } = "application/json";
        public string DataSchema { get; set; }
    }
}
