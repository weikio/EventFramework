using System;

public class StatusMessage
{
    public DateTime MessageTime { get; }
    public string Message { get; }

    public StatusMessage(DateTime messageTime, string message)
    {
        MessageTime = messageTime;
        Message = message;
    }
}