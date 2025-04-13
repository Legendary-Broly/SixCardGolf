using UnityEngine;

public class TurnCoordinator : MonoBehaviour
{
    [SerializeField] private AITurnController aiTurnController;
    [SerializeField] private PlayerTurnController playerTurnController;

    private bool isPlayerTurn = true;
    private bool gameStarted = false;
    private bool finalTurnTriggered = false;
    private bool finalTurnInProgress = false;
    private bool isFirstTurn = true;

    public TurnPhase CurrentPhase { get; private set; } = TurnPhase.Waiting;

    public void EnableGameStart()
    {
        Debug.Log("[TurnCoordinator] EnableGameStart called. gameStarted: " + gameStarted);
        gameStarted = true;
        aiTurnController.StartGame();
        BeginPlayerTurn();
    }

    public void BeginPlayerTurn()
    {
        if (!gameStarted) return;

        isPlayerTurn = true;
        CurrentPhase = TurnPhase.DrawPhase;
        Debug.Log("Player Turn Started - Phase: DrawPhase");

        playerTurnController.BeginTurn();
    }

    public void EndPlayerTurn()
    {
        Debug.Log("[TurnCoordinator] EndPlayerTurn called. isPlayerTurn: " + isPlayerTurn + ", gameStarted: " + gameStarted);
        Debug.Log("[TurnCoordinator] EndPlayerTurn called. isFirstTurn: " + isFirstTurn + ", finalTurnTriggered: " + finalTurnTriggered + ", finalTurnInProgress: " + finalTurnInProgress);

        if (isFirstTurn)
        {
            Debug.Log("[TurnCoordinator] First turn completed. Transitioning to AI turn.");
            isFirstTurn = false;
            aiTurnController.StartAITurn();
            return;
        }

        if (finalTurnTriggered && finalTurnInProgress)
        {
            EndRound();
            return;
        }

        isPlayerTurn = false;
        CurrentPhase = TurnPhase.Waiting;

        if (finalTurnTriggered && !finalTurnInProgress)
            finalTurnInProgress = true;

        aiTurnController.StartAITurn();
    }

    public void EndAITurn()
    {
        if (!gameStarted) return;

        Debug.Log("[TurnCoordinator] EndAITurn called. Transitioning to Player turn.");

        if (finalTurnTriggered && finalTurnInProgress)
        {
            EndRound();
            return;
        }

        isPlayerTurn = true;
        CurrentPhase = TurnPhase.DrawPhase;

        if (finalTurnTriggered && !finalTurnInProgress)
            finalTurnInProgress = true;

        playerTurnController.BeginTurn();
    }

    public void TriggerFinalTurn()
    {
        if (!finalTurnTriggered)
            finalTurnTriggered = true;
    }
    public void SetPhase(TurnPhase newPhase)
    {
        CurrentPhase = newPhase;
        Debug.Log($"Turn Phase changed to: {CurrentPhase}");
    }

    private void EndRound()
    {
        Debug.Log("Round over. Final scoring...");
        CurrentPhase = TurnPhase.End;

        // TODO: Trigger round end behavior, reveal cards, show casino, etc.
    }
}
