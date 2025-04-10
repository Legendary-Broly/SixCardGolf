using System.Collections.Generic;
using UnityEngine;

public class DeckManager : MonoBehaviour, IDeckSystem
{
    [SerializeField] private DeckConfigSO deckConfig;

    private Stack<string> drawPile = new();
    private Stack<string> discardPile = new();

    private void Awake()
    {
        BuildDeck();
        ShuffleDrawPile();
    }

    private void BuildDeck()
    {
        List<string> tempList = new();

        foreach (var entry in deckConfig.StandardCards)
        {
            for (int i = 0; i < entry.Count; i++)
            {
                tempList.Add(entry.Value);
            }
        }

        drawPile = new Stack<string>(tempList);
    }

    private void ShuffleDrawPile()
    {
        List<string> tempList = new(drawPile);
        drawPile.Clear();

        for (int i = 0; i < tempList.Count; i++)
        {
            int j = Random.Range(i, tempList.Count);
            (tempList[i], tempList[j]) = (tempList[j], tempList[i]);
        }

        foreach (var card in tempList)
        {
            drawPile.Push(card);
        }
    }

    public string DrawCard()
    {
        if (drawPile.Count == 0) ShuffleDiscardIntoDraw();
        return drawPile.Count > 0 ? drawPile.Pop() : null;
    }

    public string PeekTopDiscard()
    {
        return discardPile.Count > 0 ? discardPile.Peek() : null;
    }

    public string TakeDiscardCard()
    {
        return discardPile.Count > 0 ? discardPile.Pop() : null;
    }

    public void PlaceInDiscardPile(string cardValue)
    {
        discardPile.Push(cardValue);
    }

    private void ShuffleDiscardIntoDraw()
    {
        var temp = new List<string>(discardPile);
        discardPile.Clear();
        ShuffleList(temp);
        foreach (var card in temp)
            drawPile.Push(card);
    }

    private void ShuffleList(List<string> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int j = Random.Range(i, list.Count);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
