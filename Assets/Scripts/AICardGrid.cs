using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AICardGrid : MonoBehaviour, ICardGrid
{
    [SerializeField] private List<CardController> cardControllers;

    public CardModel[] GetCardModels()
    {
        return cardControllers.Select(c => c.Model).ToArray();
    }

    public void ReplaceCard(int index, string newValue)
    {
        if (index < 0 || index >= cardControllers.Count) return;
        cardControllers[index].SetCardValue(newValue);
    }

    public int FindHighestPointFaceUp()
    {
        return cardControllers
            .Where(c => c.IsFaceUp)
            .Select(c => ScoreCalculator.GetCardPointValue(c.Model.Value))
            .DefaultIfEmpty(0)
            .Max();
    }

    public List<string> GetFlippedCardValues()
    {
        return cardControllers
            .Where(card => card.IsFaceUp)
            .Select(card => card.Model.Value)
            .ToList();
    }
    public List<CardController> GetCardControllers()
    {
        return cardControllers;
    }

}
