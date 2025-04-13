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
    private bool gameStarted = false;
    private bool isAITurnActive = false;

    private void Awake()
    {
        grid = gridRef as ICardGrid;
        deck = deckRef as IDeckSystem;
    }

    private void OnEnable()
    {
        GameEvents.OnDiscardPileUpdated += HandleDiscardPileUpdated;
    }

    private void OnDisable()
    {
        GameEvents.OnDiscardPileUpdated -= HandleDiscardPileUpdated;
    }

    public void StartGame()
    {
        gameStarted = true;
    }

    private void HandleDiscardPileUpdated()
    {
        if (!gameStarted)
        {
            Debug.Log("[AITurnController] Ignoring discard pile update before game start.");
            return;
        }

        StartAITurn();
    }

    public void StartAITurn()
    {
        if (isAITurnActive)
        {
            Debug.LogWarning("[AITurnController] StartAITurn called while AI turn is already active.");
            return;
        }

        isAITurnActive = true;
        Debug.Log("[AITurnController] StartAITurn called.");
        Debug.Log("[AI] Starting AI turn...");
        StartCoroutine(ExecuteTurn());
    }

    private IEnumerator ExecuteTurn()
    {
        yield return new WaitForSeconds(1f);

        var gridCards = grid.GetCardModels().ToList();
        string discardValue = deck.PeekTopDiscard();
        Debug.Log($"[AI] Grid has {gridCards.Count} cards. Top of discard: {discardValue}");

        Debug.Log("[AITurnController] Analyzer instance: " + (analyzer != null ? "Initialized" : "Null"));
        Debug.Log("[AITurnController] GridCards type: " + gridCards.GetType());

        Debug.Log("[AI Decision] Evaluating whether to draw a card. Current grid state: " + string.Join(", ", gridCards.Select(card => card.Value)));
        Debug.Log("[AI Decision] Top of discard pile: " + discardValue);

        List<int> matchedIndices = analyzer.GetMatchedColumnIndices(gridCards);

        // Step 1: Complete third match column using discard
        int thirdMatchIndex = analyzer.FindThirdMatchColumn(gridCards, discardValue);
        if (thirdMatchIndex != -1)
        {
            string outgoing = gridCards[thirdMatchIndex].Value;
            ReplaceCardAt(thirdMatchIndex, discardValue);
            deck.TakeDiscardCard(); // Remove from discard AFTER using
            deck.PlaceInDiscardPile(outgoing); // New top
            GameEvents.CardDiscarded(outgoing);
            EndAITurn();
            yield break;
        }

        // Step 2: Match column with discard
        int matchIndex = analyzer.FindMatchableColumnIndex(gridCards, discardValue);
        if (matchIndex != -1)
        {
            string outgoing = gridCards[matchIndex].Value;
            ReplaceCardAt(matchIndex, discardValue);
            deck.TakeDiscardCard();
            deck.PlaceInDiscardPile(outgoing);
            GameEvents.CardDiscarded(outgoing);
            EndAITurn();
            yield break;
        }

        // Step 5: Replace worst card if discard is better
        int worstIndex = analyzer.FindWorstReplaceableCard(gridCards, matchedIndices);
        if (worstIndex != -1 && analyzer.IsBetterByThreshold(discardValue, gridCards[worstIndex].Value))
        {
            string outgoing = gridCards[worstIndex].Value;
            ReplaceCardAt(worstIndex, discardValue);
            deck.TakeDiscardCard();
            deck.PlaceInDiscardPile(outgoing);
            GameEvents.CardDiscarded(outgoing);
            EndAITurn();
            yield break;
        }

        // Step 4: Draw and handle Joker
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
                ReplaceCardAt(replaceIndex, drawn);
                deck.PlaceInDiscardPile(outgoing);
                GameEvents.CardDiscarded(outgoing);
                EndAITurn();
                yield break;
            }
        }

        // Step 1 (continued): Match third column with drawn card
        int drawnMatchIndex = analyzer.FindThirdMatchColumn(gridCards, drawn);
        if (drawnMatchIndex != -1)
        {
            string outgoing = gridCards[drawnMatchIndex].Value;
            ReplaceCardAt(drawnMatchIndex, drawn);
            deck.PlaceInDiscardPile(outgoing);
            GameEvents.CardDiscarded(outgoing);
            EndAITurn();
            yield break;
        }

        // Step 2 (continued): Match drawn card
        matchIndex = analyzer.FindMatchableColumnIndex(gridCards, drawn);
        if (matchIndex != -1)
        {
            string outgoing = gridCards[matchIndex].Value;
            ReplaceCardAt(matchIndex, drawn);
            deck.PlaceInDiscardPile(outgoing);
            GameEvents.CardDiscarded(outgoing);
            EndAITurn();
            yield break;
        }

        // Step 5 (continued): Replace worst if drawn is better
        worstIndex = analyzer.FindWorstReplaceableCard(gridCards, matchedIndices);
        if (worstIndex != -1 && analyzer.IsBetterByThreshold(drawn, gridCards[worstIndex].Value))
        {
            string outgoing = gridCards[worstIndex].Value;
            ReplaceCardAt(worstIndex, drawn);
            deck.PlaceInDiscardPile(outgoing);
            GameEvents.CardDiscarded(outgoing);
            EndAITurn();
            yield break;
        }

        // Ensure AI only draws from the draw pile if 5 out of 6 cards are face-up
        int faceUpCount = analyzer.CountFaceUpCards(gridCards);
        if (faceUpCount < 5)
        {
            Debug.Log("[AI Decision] Drawing from the draw pile is not allowed as fewer than 5 cards are face-up.");
            yield break;
        }

        // Step 6: Flip a card if fewer than 5 cards are face-up
        int flipIndex = analyzer.SelectRandomFaceDownIndex(gridCards);
        if (flipIndex != -1)
        {
            Debug.Log("[AI] Flipping a face-down card as no other moves are valid.");
            FlipCard(flipIndex);
            EndAITurn();
            yield break;
        }

        Debug.Log("[AI Decision] No valid moves available. Ending turn.");
        EndAITurn();
        yield break;

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
        GameEvents.CardDiscarded(null);
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
            GameEvents.CardDrawn("");
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
        isAITurnActive = false;
        FindFirstObjectByType<TurnCoordinator>()?.EndAITurn();
    }

    public void HandleCardClick(CardController card) { }
}
