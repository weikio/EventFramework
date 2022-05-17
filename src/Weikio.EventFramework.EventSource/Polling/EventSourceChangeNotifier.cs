namespace Weikio.EventFramework.EventSource.Polling
{
    public class EventSourceChangeNotifier
    {
        private readonly EventSourceChangeToken _changeToken;

        public EventSourceChangeNotifier(EventSourceChangeToken changeToken)
        {
            _changeToken = changeToken;
        }

        public void Notify()
        {
            _changeToken.TokenSource.Cancel();
        }
    }
}