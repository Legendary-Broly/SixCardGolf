public interface IGameActions
{
    void DrawCardFromDeck();
    void DrawCardFromDiscard();
    void ReplaceCardAt(int index, string value);
    void DiscardDrawnCard();
}
