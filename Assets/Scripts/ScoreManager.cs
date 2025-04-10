using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] private List<CardBehavior> gridCards;
    [SerializeField] private TextMeshProUGUI scoreText;

    private Dictionary<string, int> cardPoints = new Dictionary<string, int>()
    {
        { "A", 1 }, { "2", 2 }, { "3", 3 }, { "4", 4 }, { "5", 5 },
        { "6", 6 }, { "7", 7 }, { "8", 8 }, { "9", 9 }, { "10", 10 },
        { "J", 11 }, { "Q", 12 }, { "K", 0 }, { "JOKER", -2 }
    };

    private void Update()
    {
        int score = CalculateScore();
        scoreText.text = "Score: " + score;
    }

    private int CalculateScore()
    {
        int total = 0;

        for (int col = 0; col < 3; col++)
        {
            CardBehavior top = gridCards[col];
            CardBehavior bottom = gridCards[col + 3];

            bool bothFaceUp = top.isFaceUp && bottom.isFaceUp;
            bool match = top.cardValue == bottom.cardValue;

            if (bothFaceUp && match)
            {
                // Cancelled vertical pair
                continue;
            }

            if (top.isFaceUp && cardPoints.ContainsKey(top.cardValue))
                total += cardPoints[top.cardValue];

            if (bottom.isFaceUp && cardPoints.ContainsKey(bottom.cardValue))
                total += cardPoints[bottom.cardValue];
        }

        return total;
    }
}
