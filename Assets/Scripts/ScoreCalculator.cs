using System.Collections.Generic;

public static class ScoreCalculator
{
    private static readonly Dictionary<string, int> CardPoints = new()
    {
        { "A", 1 }, { "2", 2 }, { "3", 3 }, { "4", 4 }, { "5", 5 },
        { "6", 6 }, { "7", 7 }, { "8", 8 }, { "9", 9 }, { "10", 10 },
        { "J", 11 }, { "Q", 12 }, { "K", 0 }, { "JOKER", -2 }
    };

    public static int GetCardPoints(string value)
    {
        return CardPoints.TryGetValue(value, out var score) ? score : 99;
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
                // Matched column = 0
                continue;
            }

            if (top.IsFaceUp)
                score += GetCardPoints(top.Value);
            if (bottom.IsFaceUp)
                score += GetCardPoints(bottom.Value);
        }

        return score;
    }
}
