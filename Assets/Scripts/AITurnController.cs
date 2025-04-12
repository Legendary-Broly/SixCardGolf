using System.Collections;
using System.Linq;
using UnityEngine;

public class AITurnController : MonoBehaviour
{
    [Header("Injected Interface References")]
    [SerializeField] private MonoBehaviour gridRef; // Must implement ICardGrid
    [SerializeField] private MonoBehaviour deckRef; // Must implement IDeckSystem

    private ICardGrid grid;
    private IDeckSystem deck;
    private AIAnalyzer analyzer = new();

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
        yield return new WaitForSeconds(1f); // Optional pacing delay

        var gridCards = grid.GetCardModels(); // ✅ Correct method call
        var discardValue = deck.PeekTopDiscard();

        Debug.Log($"[AI] Grid has {gridCards.Count()} cards. Top of discard: {discardValue}");

        int matchIndex = analyzer.FindMatchableCard(gridCards, discardValue);

        // ✅ FIXED: Use gridCards.Count instead of grid.GetCardModels.Count
        if (matchIndex >= 0 && matchIndex < gridCards.Count())
        {
            Debug.Log($"[AI] Using discard card to replace index {matchIndex}.");
            grid.ReplaceCard(matchIndex, deck.TakeDiscardCard());
            EnsureFaceUp(matchIndex);
        }
        else
        {
            string drawn = deck.DrawCard();
            Debug.Log($"[AI] Drew card: {drawn}");

            int highIndex = grid.FindHighestPointFaceUp();

            // ✅ FIXED: Use gridCards.Count here too
            if (highIndex != -1 && highIndex < gridCards.Count())
            {
                var candidate = gridCards[highIndex];

                if (IsWorthReplacing(candidate, drawn))
                {
                    Debug.Log($"[AI] Replacing card at index {highIndex} with drawn card.");
                    grid.ReplaceCard(highIndex, drawn);
                    EnsureFaceUp(highIndex);
                }
                else
                {
                    Debug.Log("[AI] Drawn card not worth keeping — discarding it.");
                    deck.PlaceInDiscardPile(drawn);
                }
            }
            else
            {
                Debug.Log("[AI] No valid face-up card to replace — discarding drawn card.");
                deck.PlaceInDiscardPile(drawn);
            }
        }

        Debug.Log("[AI] Ending AI turn.");
        FindFirstObjectByType<TurnCoordinator>()?.EndAITurn();
    }

    private bool IsWorthReplacing(CardModel current, string drawn)
    {
        return current.IsFaceUp && drawn != null &&
               ScoreCalculator.GetCardPointValue(drawn) + 2 <= ScoreCalculator.GetCardPointValue(current.Value);
    }

    private void EnsureFaceUp(int index)
    {
        var model = grid.GetCardModels()[index];           // ✅ CALL method
        var controller = grid.GetCardControllers()[index]; // ✅ CALL method

        if (!model.IsFaceUp)
        {
            model.IsFaceUp = true;
            controller.FlipCard();
        }
    }
}
