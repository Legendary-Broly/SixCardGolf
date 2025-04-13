using System.Collections.Generic;
using UnityEngine;

public static class ScoreCalculator
{
    private static readonly Dictionary<string, int> CardPoints = new()
    {
        { "A", 1 }, { "2", 2 }, { "3", 3 }, { "4", 4 }, { "5", 5 },
        { "6", 6 }, { "7", 7 }, { "8", 8 }, { "9", 9 }, { "10", 10 },
        { "J", 11 }, { "Q", 12 }, { "K", 0 }, { "JOKER", -2 }
    };

    public static int GetCardPointValue(string value)
    {
        if (string.IsNullOrEmpty(value)) return 0;

        if (!CardPoints.TryGetValue(value, out int val))
        {
            Debug.LogWarning($"[ScoreCalculator] Unknown card value: {value}");
            return 0;
        }

        return val;
    }

    public static int CalculateScore(List<string> values)
    {
        int total = 0;
        foreach (var value in values)
        {
            total += GetCardPointValue(value);
        }
        return total;
    }

    public static int GetGridScore(CardModel[] cards)
    {
        if (cards == null || cards.Length < 6)
        {
            Debug.LogError("[ScoreCalculator] Invalid card array. Ensure there are exactly 6 cards.");
            return 0;
        }

        Debug.Log("[ScoreCalculator] Starting grid score calculation. Grid state:");
        for (int i = 0; i < cards.Length; i++)
        {
            var card = cards[i];
            if (card == null)
            {
                Debug.LogWarning($"[ScoreCalculator] Card at index {i} is null.");
            }
            else
            {
                Debug.Log($"[ScoreCalculator] Card at index {i}: Value={card.Value}, IsFaceUp={card.IsFaceUp}");
            }
        }

        int score = 0;
        for (int col = 0; col < 3; col++)
        {
            var top = cards[col];
            var bottom = cards[col + 3];

            if (top == null || bottom == null)
            {
                Debug.LogWarning($"[ScoreCalculator] Null card detected at column {col}.");
                continue;
            }

            Debug.Log($"[ScoreCalculator] Evaluating column {col}: TopCard={top.Value}, BottomCard={bottom.Value}, TopFaceUp={top.IsFaceUp}, BottomFaceUp={bottom.IsFaceUp}");

            // Normalize values for comparison
            string topValueNormalized = top.Value?.Trim().ToUpper();
            string bottomValueNormalized = bottom.Value?.Trim().ToUpper();

            Debug.Log($"[ScoreCalculator] Normalized values for column {col}: TopCardNormalized={topValueNormalized}, BottomCardNormalized={bottomValueNormalized}");

            bool isTopFaceUp = top.IsFaceUp;
            bool isBottomFaceUp = bottom.IsFaceUp;
            bool isMatch = topValueNormalized == bottomValueNormalized;

            Debug.Log($"[ScoreCalculator] Match condition for column {col}: IsTopFaceUp={isTopFaceUp}, IsBottomFaceUp={isBottomFaceUp}, IsMatch={isMatch}");

            if (isTopFaceUp && isBottomFaceUp && isMatch)
            {
                Debug.Log($"[ScoreCalculator] Matched column detected at index {col}: Top={top.Value}, Bottom={bottom.Value}");
                continue; // Skip adding points for matched columns
            }

            if (isTopFaceUp)
            {
                int topValue = GetCardPointValue(top.Value);
                Debug.Log($"[ScoreCalculator] Adding top card value at column {col}: {top.Value} = {topValue}");
                score += topValue;
            }

            if (isBottomFaceUp)
            {
                int bottomValue = GetCardPointValue(bottom.Value);
                Debug.Log($"[ScoreCalculator] Adding bottom card value at column {col}: {bottom.Value} = {bottomValue}");
                score += bottomValue;
            }
        }

        Debug.Log($"[ScoreCalculator] Total grid score: {score}");
        return score;
    }
}
