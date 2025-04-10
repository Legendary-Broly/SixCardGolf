using UnityEngine;

public class TurnCoordinator : MonoBehaviour
{
    [SerializeField] private AITurnController aiTurnController;
    [SerializeField] private PlayerTurnController playerTurnController;

    private bool isPlayerTurn = true;
    private bool gameStarted = false;
    private bool finalTurnTriggered = false;
    private bool finalTurnInProgress = false;

    public void EnableGameStart()
    {
        gameStarted = true;
        BeginPlayerTurn();
    }

    public void BeginPlayerTurn()
    {
        if (!gameStarted) return;
        isPlayerTurn = true;
        playerTurnController.BeginTurn();
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
            finalTurnInProgress = true;

        aiTurnController.StartAITurn();
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
            finalTurnInProgress = true;

        playerTurnController.BeginTurn();
    }

    public void TriggerFinalTurn()
    {
        if (!finalTurnTriggered)
            finalTurnTriggered = true;
    }

    private void EndRound()
    {
        Debug.Log("Round over. Final scoring...");
        // TODO: Trigger round end behavior, reveal cards, show casino, etc.
    }
}
