using UnityEngine;

[System.Serializable]
public class CardModel
{
    public string Value;
    public bool IsFaceUp;

    public CardModel(string value, bool isFaceUp = false)
    {
        Value = value;
        IsFaceUp = isFaceUp;
    }
}
