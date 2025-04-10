using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;

    [SerializeField] private AIManager aiManager;
    [SerializeField] private CardManager cardManager;

    private bool isPlayerTurn = true;
    private bool gameStarted = false;

    private bool finalTurnTriggered = false;
    private bool finalTurnInProgress = false;

    private void Awake()
    {
        Instance = this;
    }

    public void EnableGameStart()
    {
        gameStarted = true;
        BeginPlayerTurn();
    }

    public void BeginPlayerTurn()
    {
        if (!gameStarted) return;
        isPlayerTurn = true;
        cardManager.StartPlayerTurn();
    }

    public void EndPlayerTurn()
    {
        if (finalTurnTriggered && finalTurnInProgress)
        {
            EndRound();
            return;
        }

        isPlayerTurn = false;

        if (finalTurnTriggered && !finalTurnInProgress)
        {
            finalTurnInProgress = true;
        }

        aiManager.StartAITurn();
    }

    public void EndAITurn()
    {
        if (!gameStarted) return;

        if (finalTurnTriggered && finalTurnInProgress)
        {
            EndRound();
            return;
        }

        isPlayerTurn = true;

        if (finalTurnTriggered && !finalTurnInProgress)
        {
            finalTurnInProgress = true;
        }

        cardManager.StartPlayerTurn();
    }

    public void TriggerFinalTurn()
    {
        if (finalTurnTriggered) return;
        finalTurnTriggered = true;
        Debug.Log("[TurnManager] Final turn triggered.");
    }

    private void EndRound()
    {
        Debug.Log("[TurnManager] Round over. Flipping all cards...");

        cardManager.RevealAllPlayerCards();
        aiManager.RevealAllCards();

        // Future: scoring, chip rewards, round transition
    }

    public bool GameStarted => gameStarted;
}
