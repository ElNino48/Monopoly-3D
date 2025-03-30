using UnityEngine;

public class ColdTimesEvent : StoryEvent
{
    public override string EventName => "Заморозки";
    public override int EventDuration => 3;

    public override void OnEventStart()
    {
        MonopolyNode.GlobalPriceModifier = 0.5f;
        Debug.Log("OnEventStart = Cold times");
    }

    public override void OnEventEnd()
    {
        MonopolyNode.GlobalPriceModifier = 1f;
        Debug.Log("OnEventEnd = Cold times");
    }
}
