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
}
