﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CardDealer : MonoBehaviour
{
    [SerializeField] private MonoBehaviour playerGridRef; // Must implement ICardGrid
    [SerializeField] private MonoBehaviour aiGridRef;     // Must implement ICardGrid
    [SerializeField] private DeckManager deck;
    [SerializeField] private PlayerTurnController playerTurnController;
    [SerializeField] private AITurnController aiTurnController;
    [SerializeField] private DeckUIController deckUI;

    private ICardGrid playerGrid;
    private ICardGrid aiGrid;

    private void Awake()
    {
        playerGrid = playerGridRef as ICardGrid;
        aiGrid = aiGridRef as ICardGrid;
    }

    private void Start()
    {
        DealToGrid(playerGrid);
        DealToGrid(aiGrid);

        FlipRandomCards(playerGrid, 2);
        FlipRandomCards(aiGrid, 2);

        // ✅ Initialize discard pile with top draw card
        string firstDiscard = deck.DrawCard();
        deck.PlaceInDiscardPile(firstDiscard);
        deckUI.UpdateDiscardCard(firstDiscard);

        SignalGameStart();
    }

    private void DealToGrid(ICardGrid grid)
    {
        var isPlayer = grid == playerGrid;

        var controllers = grid.GetCardControllers();

        for (int i = 0; i < 6; i++)
        {
            var controller = controllers[i];
            string value = deck.DrawCard();

            var handler = isPlayer
                ? playerTurnController
                : aiTurnController as ICardInteractionHandler;

            controller.Initialize(value, false, handler);
        }
    }

    private void FlipRandomCards(ICardGrid grid, int count)
    {
        var models = grid.GetCardModels();
        var controllers = grid.GetCardControllers();

        if (models == null || controllers == null)
        {
            return;
        }

        var indices = new List<int> { 0, 1, 2, 3, 4, 5 };

        for (int i = 0; i < count && indices.Count > 0; i++)
        {
            int idx = indices[Random.Range(0, indices.Count)];
            indices.Remove(idx);

            if (models[idx] == null)
            {
                continue;
            }

            models[idx].IsFaceUp = true;
            controllers[idx].FlipCard();
        }
    }

    private void SignalGameStart()
    {
        FindFirstObjectByType<TurnCoordinator>()?.EnableGameStart();
    }
}
