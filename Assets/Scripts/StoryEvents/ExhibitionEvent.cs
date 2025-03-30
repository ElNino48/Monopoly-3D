public class ExhibitionEvent : StoryEvent
{
    public override string EventName => "Выставка";
    private MonopolyNode randomPurchasableNode;
    private float originalPriceMofidier;

    public override void OnEventStart()
    {
        randomPurchasableNode = MonopolyBoard.instance.GetRandomPurchasableNode();
        originalPriceMofidier = randomPurchasableNode.PriceModifier;
        randomPurchasableNode.PriceModifier = 0;
    }

    public override void OnEventEnd()
    {
        randomPurchasableNode.PriceModifier += originalPriceMofidier;
        randomPurchasableNode = null;
    }
}
