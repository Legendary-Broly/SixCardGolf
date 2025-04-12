using System;

public static class GameEvents
{
    public static event Action<string> OnCardDrawn;
    public static event Action<string> OnCardDiscarded;
    public static event Action OnDiscardPileUpdated;

    public static void CardDrawn(string cardValue)
    {
        OnCardDrawn?.Invoke(cardValue);
    }

    public static void CardDiscarded(string cardValue)
    {
        OnCardDiscarded?.Invoke(cardValue);
    }

    public static void DiscardPileUpdated()
    {
        OnDiscardPileUpdated?.Invoke();
    }
}
