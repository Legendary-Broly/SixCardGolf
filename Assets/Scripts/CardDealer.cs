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
        var shuffled = new List<CardModel>(cards);
        for (int i = 0; i < shuffled.Count; i++)
        {
            var temp = shuffled[i];
            int randomIndex = Random.Range(i, shuffled.Count);
            shuffled[i] = shuffled[randomIndex];
            shuffled[randomIndex] = temp;
        }

        for (int i = 0; i < count && i < shuffled.Count; i++)
        {
            shuffled[i].IsFaceUp = true;
        }
    }

    private void SignalGameStart()
    {
        FindFirstObjectByType<TurnCoordinator>()?.EnableGameStart();
    }
}
