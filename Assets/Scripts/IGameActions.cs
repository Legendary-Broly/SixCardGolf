public interface IGameActions
{
    void DrawCardFromDeck();
    void DrawCardFromDiscard();
    string ReplaceCardAt(int index, string value);
    void DiscardDrawnCard();
}
