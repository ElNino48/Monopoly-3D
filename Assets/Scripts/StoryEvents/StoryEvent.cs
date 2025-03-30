public abstract class StoryEvent
{
    public abstract string EventName { get; }
    public virtual int EventDuration { get; } = 1;
    public int CurrentDuration { get; set; } = 0;

    public abstract void OnEventStart();
    public abstract void OnEventEnd();
}
