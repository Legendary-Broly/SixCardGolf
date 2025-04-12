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
        int score = 0;
        for (int col = 0; col < 3; col++)
        {
            var top = cards[col];
            var bottom = cards[col + 3];

            if (top.IsFaceUp && bottom.IsFaceUp && top.Value == bottom.Value)
            {
                continue; // vertical match cancels
            }

            if (top.IsFaceUp)
                score += GetCardPointValue(top.Value);

            if (bottom.IsFaceUp)
                score += GetCardPointValue(bottom.Value);
        }

        return score;
    }
}
