﻿using UnityEngine;

public class PlayerTurnController : MonoBehaviour
{
    [SerializeField] private MonoBehaviour gridRef; // Must implement ICardGrid
    [SerializeField] private MonoBehaviour deckRef; // Must implement IDeckSystem

    private ICardGrid grid;
    private IDeckSystem deck;

    private string drawnCard = null;
    private bool hasDrawn = false;
    private TurnCoordinator turnCoordinator;

    private void Awake()
    {
        grid = gridRef as ICardGrid;
        deck = deckRef as IDeckSystem;
        turnCoordinator = FindFirstObjectByType<TurnCoordinator>();
    }

    public void BeginTurn()
    {
        hasDrawn = false;
        drawnCard = null;
        turnCoordinator.SetPhase(TurnPhase.DrawPhase);

        Debug.Log("Player turn started.");
    }

    public void DrawCardFromDeck()
    {
        if (hasDrawn || turnCoordinator.CurrentPhase != TurnPhase.DrawPhase) return;

        drawnCard = deck.DrawCard();
        hasDrawn = true;
        turnCoordinator.SetPhase(TurnPhase.ActionPhase);

        Debug.Log("Player drew: " + drawnCard);
    }

    public void DrawCardFromDiscard()
    {
        if (hasDrawn || turnCoordinator.CurrentPhase != TurnPhase.DrawPhase) return;

        drawnCard = deck.TakeDiscardCard();
        hasDrawn = true;
        turnCoordinator.SetPhase(TurnPhase.ActionPhase);

        Debug.Log("Player took discard: " + drawnCard);
    }

    public void ReplaceCard(int index)
    {
        if (!hasDrawn || string.IsNullOrEmpty(drawnCard) || turnCoordinator.CurrentPhase != TurnPhase.ActionPhase) return;

        var oldCard = grid.GetCardModels()[index].Value;
        grid.ReplaceCard(index, drawnCard);
        deck.PlaceInDiscardPile(oldCard);

        drawnCard = null;
        hasDrawn = false;

        EndTurn();
    }

    public void FlipCard(int index)
    {
        if (hasDrawn || turnCoordinator.CurrentPhase != TurnPhase.DrawPhase) return;

        var model = grid.GetCardModels()[index];
        if (model.IsFaceUp)
        {
            Debug.Log($"[FlipCard] Ignored flip — Card at index {index} is already face-up.");
            return;
        }

        model.IsFaceUp = true;
        Debug.Log($"[FlipCard] Flipped card at index {index} - New Value: {model.Value}");

        EndTurn();
    }

    public void DiscardDrawnCard()
    {
        if (!hasDrawn || string.IsNullOrEmpty(drawnCard) || turnCoordinator.CurrentPhase != TurnPhase.ActionPhase) return;

        deck.PlaceInDiscardPile(drawnCard);

        drawnCard = null;
        hasDrawn = false;

        EndTurn();
    }

    private void EndTurn()
    {
        turnCoordinator.EndPlayerTurn();
    }
}
