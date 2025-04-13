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
        Debug.Log("[ScoreManager] Update called. Calculating score.");
        int score = CalculateScore();
        Debug.Log($"[ScoreManager] Calculated score: {score}");
        scoreText.text = "Score: " + score;
    }

    private int CalculateScore()
    {
        Debug.Log("[ScoreManager] Starting score calculation.");
        int total = 0;

        for (int col = 0; col < 3; col++)
        {
            CardBehavior top = gridCards[col];
            CardBehavior bottom = gridCards[col + 3];

            Debug.Log($"[ScoreManager] Evaluating column {col}: RawTopCard={top.cardValue}, RawBottomCard={bottom.cardValue}");

            // Normalize values for comparison
            string topValueNormalized = top.cardValue?.Trim().ToUpper();
            string bottomValueNormalized = bottom.cardValue?.Trim().ToUpper();

            Debug.Log($"[ScoreManager] Normalized values for column {col}: TopCardNormalized={topValueNormalized}, BottomCardNormalized={bottomValueNormalized}");

            bool bothFaceUp = top.isFaceUp && bottom.isFaceUp;
            bool match = (topValueNormalized == bottomValueNormalized) ||
                         (topValueNormalized == "JOKER" || bottomValueNormalized == "JOKER");

            Debug.Log($"[ScoreManager] Match condition for column {col}: BothFaceUp={bothFaceUp}, Match={match}");

            if (bothFaceUp && match)
            {
                Debug.Log($"[ScoreManager] Matched column detected at index {col}: Top={top.cardValue}, Bottom={bottom.cardValue}");
                continue; // Skip adding points for matched columns
            }

            if (top.isFaceUp && cardPoints.ContainsKey(top.cardValue))
            {
                int topValue = cardPoints[top.cardValue];
                Debug.Log($"[ScoreManager] Adding top card value at column {col}: {top.cardValue} = {topValue}");
                total += topValue;
            }

            if (bottom.isFaceUp && cardPoints.ContainsKey(bottom.cardValue))
            {
                int bottomValue = cardPoints[bottom.cardValue];
                Debug.Log($"[ScoreManager] Adding bottom card value at column {col}: {bottom.cardValue} = {bottomValue}");
                total += bottomValue;
            }
        }

        Debug.Log($"[ScoreManager] Total score calculated: {total}");
        return total;
    }
}
