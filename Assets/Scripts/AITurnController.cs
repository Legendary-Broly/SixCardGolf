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
        yield return new WaitForSeconds(1f);

        var gridCards = grid.GetCardModels().ToList(); // Convert array to List<CardModel>
        var discardValue = deck.PeekTopDiscard();
        Debug.Log($"[AI] Grid has {gridCards.Count()} cards. Top of discard: {discardValue}");

        // 🧠 Step 1: Complete a third vertical match column using DISCARD
        int thirdMatchIndex = analyzer.FindThirdMatchColumn(gridCards, discardValue);
        if (thirdMatchIndex != -1)
        {
            Debug.Log($"[AI] Completing third vertical match using discard card.");
            grid.ReplaceCard(thirdMatchIndex, deck.TakeDiscardCard());
            EnsureFaceUp(thirdMatchIndex);
            EndAITurn();
            yield break;
        }

        // --- fallback: draw a card
        string drawn = deck.DrawCard();
        Debug.Log($"[AI] Drew card: {drawn}");

        // 🧠 Step 1 (continued): Complete third match with DRAWN card
        thirdMatchIndex = analyzer.FindThirdMatchColumn(gridCards, drawn);
        if (thirdMatchIndex != -1)
        {
            Debug.Log($"[AI] Completing third vertical match using drawn card.");
            grid.ReplaceCard(thirdMatchIndex, drawn);
            EnsureFaceUp(thirdMatchIndex);
            EndAITurn();
            yield break;
        }

        // [Step 2 and beyond will be added here in future steps]

        Debug.Log("[AI] Ending AI turn.");
        deck.PlaceInDiscardPile(drawn); // fallback discard
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
    private void EndAITurn()
    {
        Debug.Log("[AI] Ending AI turn.");
        deck.PlaceInDiscardPile(deck.DrawCard());
        FindFirstObjectByType<TurnCoordinator>()?.EndAITurn();
    }
}
