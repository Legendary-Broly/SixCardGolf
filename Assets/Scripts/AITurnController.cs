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
            return;
        }

        StartAITurn();
    }

    public void StartAITurn()
    {
        if (isAITurnActive)
        {
            return;
        }

        isAITurnActive = true;
        StartCoroutine(ExecuteTurn());
    }

    private IEnumerator ExecuteTurn()
    {
        yield return new WaitForSeconds(1f);

        var gridCards = grid.GetCardModels().ToList();
        string discardValue = deck.PeekTopDiscard();

        List<int> matchedIndices = analyzer.GetMatchedColumnIndices(gridCards);

        // Step 1: Complete third match column using discard
        int thirdMatchIndex = analyzer.FindThirdMatchColumn(gridCards, discardValue);
        if (thirdMatchIndex != -1)
        {
            string outgoing = gridCards[thirdMatchIndex].Value;
            ReplaceCardAt(thirdMatchIndex, discardValue);
            deck.TakeDiscardCard(); // Remove from discard AFTER using

            // Ensure the value of the swapped face-down card is placed into the discard pile
            if (!gridCards[thirdMatchIndex].IsFaceUp)
            {
                deck.PlaceInDiscardPile(outgoing);
                GameEvents.CardDiscarded(outgoing);
            }

            // Ensure the outgoing card is placed into the discard pile
            deck.PlaceInDiscardPile(outgoing);
            GameEvents.CardDiscarded(outgoing);
            EndAITurn();
            yield break;
        }

        // Step 1: Prioritize creating a match using the discard card
        int matchableIndex = analyzer.FindMatchableColumnIndex(gridCards, discardValue);
        if (matchableIndex != -1)
        {
            string outgoing = ReplaceCardAt(matchableIndex, discardValue);
            deck.TakeDiscardCard();

            // Ensure the value of the swapped face-down card is placed into the discard pile
            if (!gridCards[matchableIndex].IsFaceUp)
            {
                deck.PlaceInDiscardPile(outgoing);
                GameEvents.CardDiscarded(outgoing);
            }

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

            // Ensure the outgoing card is placed into the discard pile
            deck.PlaceInDiscardPile(outgoing);
            GameEvents.CardDiscarded(outgoing);
            EndAITurn();
            yield break;
        }

        // Ensure AI only draws from the draw pile if 5 out of 6 cards are face-up
        int faceUpCount = analyzer.CountFaceUpCards(gridCards);
        if (faceUpCount < 5)
        {
            // Step 6: Flip a card if fewer than 5 cards are face-up
            int flipIndex = analyzer.SelectRandomFaceDownIndex(gridCards);
            if (flipIndex != -1)
            {
                FlipCard(flipIndex);
                EndAITurn();
                yield break;
            }

            EndAITurn();
            yield break;
        }

        // Step 4: Draw and handle Joker
        string drawn = deck.DrawCard();

        if (drawn == "JOKER")
        {
            int replaceIndex = analyzer.FindWorstReplaceableCard(gridCards, matchedIndices);
            if (replaceIndex == -1)
                replaceIndex = analyzer.SelectRandomFaceDownIndex(gridCards);

            if (replaceIndex != -1)
            {
                string outgoing = gridCards[replaceIndex].Value;
                ReplaceCardAt(replaceIndex, drawn);

                // Ensure the outgoing card is placed into the discard pile
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

            // Ensure the outgoing card is placed into the discard pile
            deck.PlaceInDiscardPile(outgoing);
            GameEvents.CardDiscarded(outgoing);
            EndAITurn();
            yield break;
        }

        // Step 2 (continued): Match drawn card
        int matchIndex = analyzer.FindMatchableColumnIndex(gridCards, drawn);
        if (matchIndex != -1)
        {
            string outgoing = gridCards[matchIndex].Value;
            ReplaceCardAt(matchIndex, drawn);

            // Ensure the outgoing card is placed into the discard pile
            deck.PlaceInDiscardPile(outgoing);
            GameEvents.CardDiscarded(outgoing);
            EndAITurn();
            yield break;
        }

        // Step 5 (continued): Replace worst if drawn is better..
        worstIndex = analyzer.FindWorstReplaceableCard(gridCards, matchedIndices);
        if (worstIndex != -1 && analyzer.IsBetterByThreshold(drawn, gridCards[worstIndex].Value))
        {
            string outgoing = gridCards[worstIndex].Value;
            ReplaceCardAt(worstIndex, drawn);

            // Ensure the outgoing card is placed into the discard pile
            deck.PlaceInDiscardPile(outgoing);
            GameEvents.CardDiscarded(outgoing);
            EndAITurn();
            yield break;
        }

        // Step 7: Fallback — discard drawn card if no other moves are valid
        if (!string.IsNullOrEmpty(drawn))
        {
            deck.PlaceInDiscardPile(drawn);
            GameEvents.CardDiscarded(drawn);
            EndAITurn();
            yield break;
        }

        EndAITurn();
    }

    public void DrawCardFromDeck()
    {
        drawnCard = deck.DrawCard();
        GameEvents.CardDrawn(drawnCard);
    }

    public void DrawCardFromDiscard()
    {
        drawnCard = deck.TakeDiscardCard();
        GameEvents.CardDrawn(drawnCard);
        GameEvents.CardDiscarded(null);
    }

    public string ReplaceCardAt(int index, string value)
    {
        var outgoing = grid.GetCardModels()[index].Value;
        grid.ReplaceCard(index, value);
        EnsureFaceUp(index);
        return outgoing;
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
        isAITurnActive = false;
        FindFirstObjectByType<TurnCoordinator>()?.EndAITurn();
    }

    public void HandleCardClick(CardController card) { }
}
