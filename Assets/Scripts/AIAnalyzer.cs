using System.Collections.Generic;

public class AIAnalyzer
{
    private readonly Dictionary<string, int> cardPoints = new()
    {
        { "A", 1 }, { "2", 2 }, { "3", 3 }, { "4", 4 }, { "5", 5 },
        { "6", 6 }, { "7", 7 }, { "8", 8 }, { "9", 9 }, { "10", 10 },
        { "J", 11 }, { "Q", 12 }, { "K", 0 }, { "JOKER", -2 }
    };

    public int CalculateGridScore(CardModel[] grid)
    {
        int score = 0;
        foreach (var card in grid)
        {
            if (card.IsFaceUp && cardPoints.TryGetValue(card.Value, out var val))
                score += val;
        }
        return score;
    }
    public int CountMatchedColumns(List<CardModel> cards)
    {
        int matches = 0;

        for (int i = 0; i < 3; i++)
        {
            var top = cards[i];
            var bottom = cards[i + 3];

            bool bothFaceUp = top.IsFaceUp && bottom.IsFaceUp;
            bool isMatch = (top.Value == bottom.Value || top.Value == "JOKER" || bottom.Value == "JOKER");

            if (bothFaceUp && isMatch)
                matches++;
        }

        return matches;
    }

    public int FindMatchableCard(CardModel[] grid, string value)
    {
        for (int i = 0; i < grid.Length; i++)
        {
            if (grid[i].IsFaceUp && grid[i].Value == value && !IsColumnPaired(grid, i))
                return GetColumnMate(grid, i);
        }
        return -1;
    }

    private bool IsColumnPaired(CardModel[] grid, int index)
    {
        int col = index % 3;
        return grid[col].Value == grid[col + 3].Value;
    }

    private int GetColumnMate(CardModel[] grid, int index)
    {
        int mate = (index < 3) ? index + 3 : index - 3;
        return !grid[mate].IsFaceUp ? mate : -1;
    }
    public int FindThirdMatchColumn(List<CardModel> cards, string incomingValue)
    {
        int matchedCount = CountMatchedColumns(cards);
        if (matchedCount < 2) return -1;

        for (int col = 0; col < 3; col++)
        {
            var top = cards[col];
            var bottom = cards[col + 3];

            bool isMatched = (top.Value == bottom.Value || top.Value == "JOKER" || bottom.Value == "JOKER");
            bool isFullyFaceUp = top.IsFaceUp && bottom.IsFaceUp;

            if (isMatched && isFullyFaceUp) continue;

            // Find the missing piece of this column
            if (top.IsFaceUp && !bottom.IsFaceUp && (top.Value == incomingValue || top.Value == "JOKER" || incomingValue == "JOKER"))
                return col + 3;

            if (!top.IsFaceUp && bottom.IsFaceUp && (bottom.Value == incomingValue || bottom.Value == "JOKER" || incomingValue == "JOKER"))
                return col;
        }

        return -1;
    }

}
