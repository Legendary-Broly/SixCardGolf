using UnityEngine;

public class PlayerTurnController : MonoBehaviour
{
    [SerializeField] private MonoBehaviour gridRef; // Must implement ICardGrid
    [SerializeField] private MonoBehaviour deckRef; // Must implement IDeckSystem

    private ICardGrid grid;
    private IDeckSystem deck;

    private string drawnCard = null;
    private bool hasDrawn = false;

    private void Awake()
    {
        grid = gridRef as ICardGrid;
        deck = deckRef as IDeckSystem;
    }

    public void BeginTurn()
    {
        hasDrawn = false;
        drawnCard = null;
        Debug.Log("Player turn started.");
    }

    public void DrawCardFromDeck()
    {
        if (hasDrawn) return;

        drawnCard = deck.DrawCard();
        hasDrawn = true;
        Debug.Log("Player drew: " + drawnCard);
    }

    public void DrawCardFromDiscard()
    {
        if (hasDrawn) return;

        drawnCard = deck.TakeDiscardCard();
        hasDrawn = true;
        Debug.Log("Player took discard: " + drawnCard);
    }

    public void ReplaceCard(int index)
    {
        if (!hasDrawn || string.IsNullOrEmpty(drawnCard)) return;

        var oldCard = grid.GetCardModels()[index].Value;
        grid.ReplaceCard(index, drawnCard);
        deck.PlaceInDiscardPile(oldCard);

        drawnCard = null;
        hasDrawn = false;

        EndTurn();
    }

    public void FlipCard(int index)
    {
        if (hasDrawn) return;

        var card = grid.GetCardModels()[index];
        card.IsFaceUp = true;

        EndTurn();
    }

    public void DiscardDrawnCard()
    {
        if (!hasDrawn || string.IsNullOrEmpty(drawnCard)) return;

        deck.PlaceInDiscardPile(drawnCard);
        drawnCard = null;
        hasDrawn = false;

        EndTurn();
    }

    private void EndTurn()
    {
        Object.FindFirstObjectByType<TurnCoordinator>().EndPlayerTurn();
    }
}
