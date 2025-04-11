using System.Collections.Generic;

public interface ICardGrid
{
    CardModel[] GetCardModels();
    void ReplaceCard(int index, string newValue);
    int FindHighestPointFaceUp();
    List<string> GetFlippedCardValues(); // 🔹 New for ScoreDisplay
}
