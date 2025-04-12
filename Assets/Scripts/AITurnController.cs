using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AITurnController : MonoBehaviour, IGameActions, ICardInteractionHandler
{
    [Header("Injected Interface References")]
    [SerializeField] private MonoBehaviour gridRef; // Must implement ICardGrid
    [SerializeField] private MonoBehaviour deckRef; // Must implement IDeckSystem

    private ICardGrid grid;
    private IDeckSystem deck;
    private AIAnalyzer analyzer = new();

    private string drawnCard;
    public void HandleCardClick(CardController card)
    {
        // AI never handles manual clicks, leave empty
    }
    private void Awake()
    {
        grid = gridRef as ICardGrid;
        deck = deckRef as IDeckSystem;
    }

    public void StartAITurn()
    {
        Debug.Log("[AI] Starting AI turn...");
        StartCoroutine(ExecuteTurn());
    }

    private IEnumerator ExecuteTurn()
    {
        yield return new WaitForSeconds(1f);

        var gridCards = grid.GetCardModels().ToList();
        var discardValue = deck.PeekTopDiscard();

        Debug.Log($"[AI] Grid has {gridCards.Count} cards. Top of discard: {discardValue}");

        // Step 1: Try to complete a third vertical match column with DISCARD
        int thirdMatchIndex = analyzer.FindThirdMatchColumn(gridCards, discardValue);
        if (thirdMatchIndex != -1)
        {
            Debug.Log("[AI] Completing third match column using discard.");
            ReplaceCardAt(thirdMatchIndex, deck.TakeDiscardCard());
            GameEvents.CardDiscarded(discardValue);
            EndAITurn();
            yield break;
        }

        // Step 5: Replace worst face-up card if discard is 2+ points better
        List<int> matchedIndices = analyzer.GetMatchedColumnIndices(gridCards);
        int worstIndex = analyzer.FindWorstReplaceableCard(gridCards, matchedIndices);

        if (worstIndex != -1 && analyzer.IsBetterByThreshold(discardValue, gridCards[worstIndex].Value))
        {
            Debug.Log("[AI] Replacing worst face-up card with discard (2+ pts better).");
            ReplaceCardAt(worstIndex, deck.TakeDiscardCard());
            GameEvents.CardDiscarded(discardValue);
            EndAITurn();
            yield break;
        }

        // Step 1 (continued): Try with DRAWN card
        DrawCardFromDeck();

        thirdMatchIndex = analyzer.FindThirdMatchColumn(gridCards, drawnCard);
        if (thirdMatchIndex != -1)
        {
            Debug.Log("[AI] Completing third match column using drawn card.");
            ReplaceCardAt(thirdMatchIndex, drawnCard);
            GameEvents.CardDiscarded(gridCards[thirdMatchIndex].Value);
            drawnCard = null;
            EndAITurn();
            yield break;
        }

        // Fallback: discard the drawn card
        DiscardDrawnCard();
        EndAITurn();
    }

    public void DrawCardFromDeck()
    {
        drawnCard = deck.DrawCard();
        Debug.Log($"[AI] Drew card: {drawnCard}");
        GameEvents.CardDrawn(drawnCard);
    }

    public void DrawCardFromDiscard()
    {
        drawnCard = deck.TakeDiscardCard();
        Debug.Log($"[AI] Took discard: {drawnCard}");
        GameEvents.CardDrawn(drawnCard);
        GameEvents.CardDiscarded(null); // Clear discard UI
    }

    public void ReplaceCardAt(int index, string value)
    {
        grid.ReplaceCard(index, value);
        EnsureFaceUp(index);
    }

    public void DiscardDrawnCard()
    {
        if (!string.IsNullOrEmpty(drawnCard))
        {
            deck.PlaceInDiscardPile(drawnCard);
            GameEvents.CardDiscarded(drawnCard);
            GameEvents.CardDrawn(""); // Clear drawn UI
            drawnCard = null;
        }
    }

    private void EnsureFaceUp(int index)
    {
        var model = grid.GetCardModels()[index];
        var controller = grid.GetCardControllers()[index];

        if (!model.IsFaceUp)
        {
            model.IsFaceUp = true;
            controller.FlipCard();
        }
    }

    private void EndAITurn()
    {
        Debug.Log("[AI] Ending AI turn.");
        FindFirstObjectByType<TurnCoordinator>()?.EndAITurn();
    }
}
