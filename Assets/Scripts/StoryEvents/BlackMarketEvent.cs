using UnityEngine;

public class BlackMarketEvent : StoryEvent
{
    public override string EventName => "Чёрный рынок";

    public override void OnEventStart()
    {
        MonopolyNode.GlobalPriceModifier = 2f;
        Debug.Log("OnEventStart = Black market");
    }

    public override void OnEventEnd()
    {
        MonopolyNode.GlobalPriceModifier = 1f;
        Debug.Log("OnEventEnd = Black market");
    }
}
