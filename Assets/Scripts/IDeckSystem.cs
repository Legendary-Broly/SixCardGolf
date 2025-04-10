public interface IDeckSystem
{
    string DrawCard();
    string PeekTopDiscard();
    string TakeDiscardCard();
    void PlaceInDiscardPile(string cardValue);
}
