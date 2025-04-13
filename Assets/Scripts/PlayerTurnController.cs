using UnityEngine;
using System.Linq;

public class PlayerTurnController : MonoBehaviour, IGameActions, ICardInteractionHandler
{
    [SerializeField] private MonoBehaviour gridRef; // Must implement ICardGrid
    [SerializeField] private MonoBehaviour deckRef; // Must implement IDeckSystem

    private ICardGrid grid;
    private IDeckSystem deck;
    private TurnCoordinator turnCoordinator;

    private string drawnCard = null;
    private bool hasDrawn = false;
    private bool usingDiscard = false;
    private bool isFirstTurn = true;

    private void Awake()
    {
        grid = gridRef as ICardGrid;
        deck = deckRef as IDeckSystem;
        turnCoordinator = FindFirstObjectByType<TurnCoordinator>();
        Debug.Log($"[PlayerTurnController] Deck reference type: {deck?.GetType().Name}");
    }

    public void BeginTurn()
    {
        hasDrawn = false;
        drawnCard = null;
        usingDiscard = false;
        turnCoordinator.SetPhase(TurnPhase.DrawPhase);
        Debug.Log("Player turn started.");
        Debug.Log("[PlayerTurnController] BeginTurn called. hasDrawn: " + hasDrawn + ", drawnCard: " + drawnCard + ", usingDiscard: " + usingDiscard);

        if (isFirstTurn)
        {
            isFirstTurn = false;
        }
    }

    public void DrawCardFromDeck()
    {
        if (hasDrawn || turnCoordinator.CurrentPhase != TurnPhase.DrawPhase) return;

        drawnCard = deck.DrawCard();
        if (string.IsNullOrEmpty(drawnCard)) return;

        hasDrawn = true;
        usingDiscard = false;
        turnCoordinator.SetPhase(TurnPhase.ActionPhase);

        GameEvents.CardDrawn(drawnCard);
        Debug.Log("Player drew: " + drawnCard);
    }

    public void DrawCardFromDiscard()
    {
        if (hasDrawn || turnCoordinator.CurrentPhase != TurnPhase.DrawPhase) return;

        drawnCard = deck.PeekTopDiscard(); // Only peek — don't pop yet
        if (string.IsNullOrEmpty(drawnCard))
        {
            return;
        }

        deck.LockDiscardPile();

        hasDrawn = true;
        usingDiscard = true;
        turnCoordinator.SetPhase(TurnPhase.ActionPhase);

        GameEvents.CardDrawn(drawnCard);
        Debug.Log("Player peeked discard: " + drawnCard);
    }

    public void DiscardDrawnCard()
    {
        if (!hasDrawn || string.IsNullOrEmpty(drawnCard) || turnCoordinator.CurrentPhase != TurnPhase.ActionPhase) return;

        deck.PlaceInDiscardPile(drawnCard);
        GameEvents.CardDiscarded(drawnCard);
        GameEvents.CardDrawn("");

        drawnCard = null;
        hasDrawn = false;
        usingDiscard = false;

        EndTurn();
    }

    public void HandleCardClick(CardController card)
    {
        int index = grid.GetCardControllers().IndexOf(card);
        if (index == -1) return;

        if (hasDrawn && !string.IsNullOrEmpty(drawnCard))
        {
            // Swap logic
            var outgoing = grid.GetCardModels()[index].Value;

            // Replace the grid card with the drawn card
            grid.ReplaceCard(index, drawnCard);

            // Force face-up if it was face-down
            var model = grid.GetCardModels()[index];
            var controller = grid.GetCardControllers()[index];

            if (!model.IsFaceUp)
            {
                Debug.Log($"[PlayerTurnController] Setting IsFaceUp to true for card at index {index}");
                model.IsFaceUp = true;
                controller.FlipCard();
            }

            // Place outgoing card into discard
            deck.PlaceInDiscardPile(outgoing);
            GameEvents.CardDiscarded(outgoing);
            GameEvents.CardDrawn(""); // Clear drawn card display

            // Finalize state
            if (usingDiscard)
            {
                deck.TakeDiscardCard(); // Now remove from discard pile
                usingDiscard = false;
            }

            drawnCard = null;
            hasDrawn = false;

            EndTurn();
        }
        else
        {
            // Prevent flip if a drawn card is held
            if (hasDrawn)
            {
                Debug.Log("[Player] Cannot flip — must place drawn card first.");
                return;
            }

            FlipCard(index);
        }
    }

    public void FlipCard(int index)
    {
        if (turnCoordinator.CurrentPhase != TurnPhase.DrawPhase) return;

        var model = grid.GetCardModels()[index];
        var controller = grid.GetCardControllers()[index];

        if (model.IsFaceUp)
        {
            Debug.Log($"[FlipCard] Ignored flip — Card at index {index} is already face-up.");
            return;
        }

        Debug.Log($"[PlayerTurnController] Setting IsFaceUp to true for card at index {index}");
        model.IsFaceUp = true;
        controller.FlipCard();

        Debug.Log($"[FlipCard] Flipped card at index {index} - New Value: {model.Value}");

        EndTurn();
    }

    public string ReplaceCardAt(int index, string value)
    {
        var outgoing = grid.GetCardModels()[index].Value;
        grid.ReplaceCard(index, value);
        GameEvents.CardDrawn("");
        return outgoing;
    }

    private void EndTurn()
    {
        Debug.Log("[PlayerTurnController] EndTurn called. isFirstTurn: " + isFirstTurn);

        if (isFirstTurn || (!hasDrawn && !grid.GetCardModels().Any(card => card.IsFaceUp)))
        {
            Debug.Log("[PlayerTurnController] Preventing premature turn end. isFirstTurn: " + isFirstTurn + ", hasDrawn: " + hasDrawn);
            return;
        }

        drawnCard = null;
        hasDrawn = false;
        usingDiscard = false;

        turnCoordinator.EndPlayerTurn();
    }
}
