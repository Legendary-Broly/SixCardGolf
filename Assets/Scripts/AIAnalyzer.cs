using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

        // Check for matched columns and treat them as having a value of 0
        var matchedColumns = GetMatchedColumnIndices(grid.ToList());
        foreach (var card in grid)
        {
            if (matchedColumns.Contains(grid.ToList().IndexOf(card)))
                continue; // Skip matched cards

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
    public List<int> GetMatchedColumnIndices(List<CardModel> cards)
    {
        var matched = new List<int>();

        for (int i = 0; i < 3; i++)
        {
            var top = cards[i];
            var bottom = cards[i + 3];

            bool bothFaceUp = top.IsFaceUp && bottom.IsFaceUp;
            bool isMatch = top.Value == bottom.Value || top.Value == "JOKER" || bottom.Value == "JOKER";

            if (bothFaceUp && isMatch)
            {
                matched.Add(i);
                matched.Add(i + 3);
            }
        }

        return matched;
    }
    public int FindMatchableColumnIndex(List<CardModel> cards, string incoming)
    {
        for (int col = 0; col < 3; col++)
        {
            int top = col;
            int bottom = col + 3;

            CardModel topCard = cards[top];
            CardModel bottomCard = cards[bottom];

            // Declaring and initializing variables before usage
            bool topMatches = topCard.IsFaceUp && (topCard.Value == incoming || topCard.Value == "JOKER" || incoming == "JOKER");
            bool bottomMatches = bottomCard.IsFaceUp && (bottomCard.Value == incoming || bottomCard.Value == "JOKER" || incoming == "JOKER");

            Debug.Log($"[AIAnalyzer] Evaluating column {col}: TopCard={topCard.Value}, BottomCard={bottomCard.Value}, Incoming={incoming}");
            Debug.Log($"[AIAnalyzer] TopMatches={topMatches}, BottomMatches={bottomMatches}");

            // Check if either top or bottom is face-up and matches the incoming
            if (topMatches && !bottomCard.IsFaceUp)
                return bottom;

            // If bottom is face-up and top is face-down
            if (bottomMatches && !topCard.IsFaceUp)
                return top;
        }

        return -1; // no matchable column
    }

    public int CountFaceUpCards(List<CardModel> cards)
    {
        return cards.Count(card => card.IsFaceUp);
    }

    public int SelectRandomFaceDownIndex(List<CardModel> cards)
    {
        var faceDownIndices = new List<int>();
        for (int i = 0; i < cards.Count; i++)
        {
            if (!cards[i].IsFaceUp)
            {
                faceDownIndices.Add(i);
            }
        }

        if (faceDownIndices.Count == 0) return -1;

        int randomIndex = UnityEngine.Random.Range(0, faceDownIndices.Count);
        return faceDownIndices[randomIndex];
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
    public int FindWorstReplaceableCard(List<CardModel> cards, List<int> matchedIndices)
    {
        int worstIndex = -1;
        int worstValue = -1;

        for (int i = 0; i < cards.Count; i++)
        {
            var card = cards[i];

            if (!card.IsFaceUp || matchedIndices.Contains(i))
                continue;

            int val = ScoreCalculator.GetCardPointValue(card.Value);
            if (val > worstValue)
            {
                worstValue = val;
                worstIndex = i;
            }
        }

        return worstIndex;
    }
    public bool IsBetterByThreshold(string incoming, string existing, int threshold = 2)
    {
        if (string.IsNullOrEmpty(incoming) || string.IsNullOrEmpty(existing))
            return false;

        int incomingVal = ScoreCalculator.GetCardPointValue(incoming);
        int existingVal = ScoreCalculator.GetCardPointValue(existing);

        return (existingVal - incomingVal) >= threshold;
    }

    public bool CanFlipCardSafely(List<CardModel> cards, int index)
    {
        if (index < 0 || index >= cards.Count || cards[index].IsFaceUp)
            return false;

        // Simulate flipping the card
        var simulatedCard = cards[index];
        simulatedCard.IsFaceUp = true;

        // Check if flipping improves score or matches
        int currentScore = CalculateGridScore(cards.ToArray());
        int newScore = CalculateGridScore(cards.ToArray());

        // Restore original state
        simulatedCard.IsFaceUp = false;

        return newScore >= currentScore;
    }
}
