namespace Weikio.EventFramework.EventCreator
{
    public interface ICloudEventCreatorOptionsProvider
    {
        CloudEventCreationOptions Get(string optionsName);
    }
}
