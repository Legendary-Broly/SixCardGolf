using UnityEngine;
using System.Collections.Generic;

public class CardDealer : MonoBehaviour
{
    [SerializeField] private MonoBehaviour playerGridRef; // Must implement ICardGrid
    [SerializeField] private MonoBehaviour aiGridRef;     // Must implement ICardGrid
    [SerializeField] private DeckManager deck;

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

        deck.PlaceInDiscardPile(deck.DrawCard());

        Invoke(nameof(SignalGameStart), 0.2f);
    }

    private void DealToGrid(ICardGrid grid)
    {
        for (int i = 0; i < 6; i++)
        {
            grid.ReplaceCard(i, deck.DrawCard());
        }
    }

    private void FlipRandomCards(ICardGrid grid, int count)
    {
        var cards = grid.GetCardModels();
        var indices = new List<int> { 0, 1, 2, 3, 4, 5 };
        for (int i = 0; i < count; i++)
        {
            int idx = indices[Random.Range(0, indices.Count)];
            indices.Remove(idx);
            cards[idx].IsFaceUp = true;
        }
    }

    private void SignalGameStart()
    {
        FindFirstObjectByType<TurnCoordinator>()?.EnableGameStart();
    }
}
