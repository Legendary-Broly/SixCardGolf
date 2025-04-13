using System.Collections.Generic;

public interface IDeckSystem
{
    string DrawCard();
    string PeekTopDiscard();
    string TakeDiscardCard();
    void PlaceInDiscardPile(string cardValue);
    void LockDiscardPile();
    void UnlockDiscardPile();
    IEnumerable<string> GetDiscardPileState();
}
