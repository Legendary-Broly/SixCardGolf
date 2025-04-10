public interface ICardGrid
{
    CardModel[] GetCardModels();
    void ReplaceCard(int index, string newValue);
    int FindHighestPointFaceUp();
}
