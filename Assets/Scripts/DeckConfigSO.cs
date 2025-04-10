using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DeckConfig", menuName = "SixCardGolf/DeckConfig")]
public class DeckConfigSO : ScriptableObject
{
    [System.Serializable]
    public class CardEntry
    {
        public string Value;
        public int Count;
    }

    public List<CardEntry> StandardCards = new()
    {
        new CardEntry { Value = "A", Count = 4 },
        new CardEntry { Value = "2", Count = 4 },
        new CardEntry { Value = "3", Count = 4 },
        new CardEntry { Value = "4", Count = 4 },
        new CardEntry { Value = "5", Count = 4 },
        new CardEntry { Value = "6", Count = 4 },
        new CardEntry { Value = "7", Count = 4 },
        new CardEntry { Value = "8", Count = 4 },
        new CardEntry { Value = "9", Count = 4 },
        new CardEntry { Value = "10", Count = 4 },
        new CardEntry { Value = "J", Count = 4 },
        new CardEntry { Value = "Q", Count = 4 },
        new CardEntry { Value = "K", Count = 4 },
        new CardEntry { Value = "JOKER", Count = 2 }
    };
}
