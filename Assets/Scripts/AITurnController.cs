using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AITurnController : MonoBehaviour, IGameActions, ICardInteractionHandler
{
    [Header("Injected Interface References")]
    [SerializeField] private MonoBehaviour gridRef;
    [SerializeField] private MonoBehaviour deckRef;

    private ICardGrid grid;
    private IDeckSystem deck;
    private AIAnalyzer analyzer = new();
    private string drawnCard;

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

        List<int> matchedIndices = analyzer.GetMatchedColumnIndices(gridCards);

        // Step 1: Complete third match column using discard
        int thirdMatchIndex = analyzer.FindThirdMatchColumn(gridCards, discardValue);
        if (thirdMatchIndex != -1)
        {
            string outgoing = gridCards[thirdMatchIndex].Value;
            Debug.Log("[AI] Completing third match column using discard.");
            string taken = deck.TakeDiscardCard();
            ReplaceCardAt(thirdMatchIndex, taken);
            GameEvents.CardDiscarded(outgoing);
            EndAITurn();
            yield break;
        }

        // Step 2: Match a column with discard
        int matchIndex = analyzer.FindMatchableColumnIndex(gridCards, discardValue);
        if (matchIndex != -1)
        {
            string outgoing = gridCards[matchIndex].Value;
            Debug.Log("[AI] Matching column using discard card.");
            string taken = deck.TakeDiscardCard();
            ReplaceCardAt(matchIndex, taken);
            GameEvents.CardDiscarded(outgoing);
            EndAITurn();
            yield break;
        }

        // Step 5: Replace worst card if discard is 2+ pts better
        int worstIndex = analyzer.FindWorstReplaceableCard(gridCards, matchedIndices);
        if (worstIndex != -1 && analyzer.IsBetterByThreshold(discardValue, gridCards[worstIndex].Value))
        {
            string outgoing = gridCards[worstIndex].Value;
            Debug.Log("[AI] Replacing worst face-up card with discard (2+ pts better).");
            string taken = deck.TakeDiscardCard();
            ReplaceCardAt(worstIndex, taken);
            GameEvents.CardDiscarded(outgoing);
            EndAITurn();
            yield break;
        }

        // Step 4: Draw and handle JOKER
        string drawn = deck.DrawCard();
        Debug.Log($"[AI] Drew card: {drawn}");

        if (drawn == "JOKER")
        {
            int replaceIndex = analyzer.FindWorstReplaceableCard(gridCards, matchedIndices);
            if (replaceIndex == -1)
                replaceIndex = analyzer.SelectRandomFaceDownIndex(gridCards);

            if (replaceIndex != -1)
            {
                string outgoing = gridCards[replaceIndex].Value;
                Debug.Log("[AI] Replacing card with JOKER.");
                ReplaceCardAt(replaceIndex, drawn);
                GameEvents.CardDiscarded(outgoing);
                EndAITurn();
                yield break;
            }
        }

        // Step 1 (continued): Complete third match with drawn card
        int drawnMatchIndex = analyzer.FindThirdMatchColumn(gridCards, drawn);
        if (drawnMatchIndex != -1)
        {
            string outgoing = gridCards[drawnMatchIndex].Value;
            Debug.Log("[AI] Completing third match column using drawn card.");
            ReplaceCardAt(drawnMatchIndex, drawn);
            GameEvents.CardDiscarded(outgoing);
            EndAITurn();
            yield break;
        }

        // Step 2 (continued): Match drawn card
        matchIndex = analyzer.FindMatchableColumnIndex(gridCards, drawn);
        if (matchIndex != -1)
        {
            string outgoing = gridCards[matchIndex].Value;
            Debug.Log("[AI] Matching column using drawn card.");
            ReplaceCardAt(matchIndex, drawn);
            GameEvents.CardDiscarded(outgoing);
            EndAITurn();
            yield break;
        }

        // Step 5 (continued): Replace worst if drawn card is better
        worstIndex = analyzer.FindWorstReplaceableCard(gridCards, matchedIndices);
        if (worstIndex != -1 && analyzer.IsBetterByThreshold(drawn, gridCards[worstIndex].Value))
        {
            string outgoing = gridCards[worstIndex].Value;
            Debug.Log("[AI] Replacing worst face-up card with drawn card (2+ pts better).");
            ReplaceCardAt(worstIndex, drawn);
            GameEvents.CardDiscarded(outgoing);
            EndAITurn();
            yield break;
        }

        // Step 6: Flip a card if no valid play
        int faceUpCount = analyzer.CountFaceUpCards(gridCards);
        if (faceUpCount < 5)
        {
            int flipIndex = analyzer.SelectRandomFaceDownIndex(gridCards);
            if (flipIndex != -1)
            {
                Debug.Log("[AI] Flipping a face-down card due to no better move.");
                FlipCard(flipIndex);
                EndAITurn();
                yield break;
            }
        }

        // Step 7: Fallback — discard drawn card
        Debug.Log("[AI] No suitable move. Discarding drawn card.");
        deck.PlaceInDiscardPile(drawn);
        GameEvents.CardDiscarded(drawn);
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

    private void FlipCard(int index)
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

    public void HandleCardClick(CardController card) { /* unused for AI */ }
}
