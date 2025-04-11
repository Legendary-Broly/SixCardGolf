using System.Collections.Generic;

public interface ICardGrid
{
    CardModel[] GetCardModels();
    void ReplaceCard(int index, string newValue);
    int FindHighestPointFaceUp();
    List<string> GetFlippedCardValues();
    List<CardController> GetCardControllers(); // 🔧 REQUIRED for flip visuals
}
