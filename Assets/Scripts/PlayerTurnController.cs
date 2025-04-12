﻿using UnityEngine;

public class PlayerTurnController : MonoBehaviour, IGameActions, ICardInteractionHandler
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
        if (string.IsNullOrEmpty(drawnCard)) return;

        hasDrawn = true;
        turnCoordinator.SetPhase(TurnPhase.ActionPhase);

        GameEvents.CardDrawn(drawnCard);
        Debug.Log("Player drew: " + drawnCard);
    }

    public void DrawCardFromDiscard()
    {
        if (hasDrawn || turnCoordinator.CurrentPhase != TurnPhase.DrawPhase) return;

        drawnCard = deck.TakeDiscardCard();
        Debug.Log($"[Player] TakeDiscardCard() returned: {drawnCard}");

        if (string.IsNullOrEmpty(drawnCard))
        {
            Debug.LogWarning("[Player] TakeDiscardCard() failed — discard pile likely empty.");
            return;
        }

        hasDrawn = true;
        turnCoordinator.SetPhase(TurnPhase.ActionPhase);

        GameEvents.CardDrawn(drawnCard);
    }

    public void DiscardDrawnCard()
    {
        if (!hasDrawn || string.IsNullOrEmpty(drawnCard) || turnCoordinator.CurrentPhase != TurnPhase.ActionPhase) return;

        deck.PlaceInDiscardPile(drawnCard);
        GameEvents.CardDiscarded(drawnCard);
        GameEvents.CardDrawn("");

        drawnCard = null;
        hasDrawn = false;

        EndTurn();
    }

    public void HandleCardClick(CardController card)
    {
        int index = grid.GetCardControllers().IndexOf(card);
        if (index == -1) return;

        if (!string.IsNullOrEmpty(drawnCard))
        {
            // Swap logic
            var outgoing = grid.GetCardModels()[index].Value;
            grid.ReplaceCard(index, drawnCard);

            // Manually flip the card face-up if it wasn’t already
            var model = grid.GetCardModels()[index];
            var controller = grid.GetCardControllers()[index];

            if (!model.IsFaceUp)
            {
                model.IsFaceUp = true;
                controller.FlipCard();
            }

            deck.PlaceInDiscardPile(outgoing);
            GameEvents.CardDiscarded(outgoing); // update DiscardCardDisplay
            GameEvents.CardDrawn("");           // clear DrawnCardDisplay

            drawnCard = null;
            hasDrawn = false;

            EndTurn();
        }
        else
        {
            // Flip logic
            FlipCard(index);
        }
    }

    public void FlipCard(int index)
    {
        if (hasDrawn || turnCoordinator.CurrentPhase != TurnPhase.DrawPhase) return;

        var model = grid.GetCardModels()[index];
        var controller = grid.GetCardControllers()[index];

        if (model.IsFaceUp)
        {
            Debug.Log($"[FlipCard] Ignored flip — Card at index {index} is already face-up.");
            return;
        }

        model.IsFaceUp = true;
        controller.FlipCard();

        Debug.Log($"[FlipCard] Flipped card at index {index} - New Value: {model.Value}");

        EndTurn();
    }

    public void ReplaceCardAt(int index, string value)
    {
        grid.ReplaceCard(index, value);
        GameEvents.CardDrawn("");
    }

    private void EndTurn()
    {
        turnCoordinator.EndPlayerTurn();
    }
}
