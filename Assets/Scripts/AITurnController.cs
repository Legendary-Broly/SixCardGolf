using System.Collections;
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
        StartCoroutine(ExecuteTurn());
    }

    private IEnumerator ExecuteTurn()
    {
        yield return new WaitForSeconds(1f);

        string discardValue = deck.PeekTopDiscard();
        var gridCards = grid.GetCardModels();
        int visibleScore = ScoreCalculator.GetGridScore(gridCards);

        int matchIndex = analyzer.FindMatchableCard(gridCards, discardValue);

        if (matchIndex != -1)
        {
            grid.ReplaceCard(matchIndex, deck.TakeDiscardCard());
        }
        else
        {
            string drawn = deck.DrawCard();
            int highIndex = grid.FindHighestPointFaceUp();
            if (highIndex != -1 && IsWorthReplacing(gridCards[highIndex], drawn))
            {
                grid.ReplaceCard(highIndex, drawn);
            }
            else
            {
                deck.PlaceInDiscardPile(drawn);
            }
        }
    }

    private bool IsWorthReplacing(CardModel current, string drawn)
    {
        return current.IsFaceUp && drawn != null &&
               ScoreCalculator.GetCardPointValue(drawn) + 2 <= ScoreCalculator.GetCardPointValue(current.Value);
    }

}
