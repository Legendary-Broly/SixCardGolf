using UnityEngine;

public class AICardGrid : MonoBehaviour, ICardGrid
{
    [SerializeField] private CardController[] cardControllers;

    public CardModel[] GetCardModels()
    {
        var models = new CardModel[cardControllers.Length];
        for (int i = 0; i < cardControllers.Length; i++)
        {
            models[i] = cardControllers[i].Model;
        }
        return models;
    }

    public void ReplaceCard(int index, string newValue)
    {
        cardControllers[index].SetCardValue(newValue);
        cardControllers[index].FlipCard();
    }

    public int FindHighestPointFaceUp()
    {
        int maxPoints = -1;
        int index = -1;

        for (int i = 0; i < cardControllers.Length; i++)
        {
            var card = cardControllers[i].Model;
            if (card.IsFaceUp)
            {
                int points = ScoreCalculator.GetCardPoints(card.Value);
                if (points > maxPoints)
                {
                    maxPoints = points;
                    index = i;
                }
            }
        }

        return index;
    }
}
