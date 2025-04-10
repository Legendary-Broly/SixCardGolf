using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardManager : MonoBehaviour
{
    [SerializeField] private List<CardBehavior> allCards; // Player grid
    [SerializeField] private List<CardBehavior> aiCards;  // AI grid
    [SerializeField] private Button drawPileButton;
    [SerializeField] private Button discardPileButton;
    [SerializeField] private TextMeshProUGUI discardText;
    [SerializeField] private TextMeshProUGUI drawnCardText;

    private List<string> deck = new List<string>();
    private string drawnCard = null;
    private string topDiscardCard = null;

    private void Start()
    {
        BuildDeck();
        ShuffleDeck();
        DealInitialCards();

        FlipRandomCards(allCards, 2);
        FlipRandomCards(aiCards, 2);

        topDiscardCard = deck[0];
        deck.RemoveAt(0);

        drawPileButton.onClick.AddListener(OnDrawPileClicked);
        discardPileButton.onClick.AddListener(OnDiscardPileClicked);

        UpdateDiscardVisual();
        UpdateDrawnCardVisual();

        Invoke(nameof(MarkGameStarted), 0.2f);
    }

    private void MarkGameStarted()
    {
        TurnManager.Instance.EnableGameStart();
    }

    private void BuildDeck()
    {
        string[] values = new string[]
        {
            "A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K"
        };

        foreach (string value in values)
        {
            for (int i = 0; i < 4; i++)
                deck.Add(value);
        }

        deck.Add("JOKER");
        deck.Add("JOKER");
    }

    private void ShuffleDeck()
    {
        for (int i = 0; i < deck.Count; i++)
        {
            int randomIndex = Random.Range(i, deck.Count);
            string temp = deck[i];
            deck[i] = deck[randomIndex];
            deck[randomIndex] = temp;
        }
    }

    private void DealInitialCards()
    {
        int totalCards = allCards.Count + aiCards.Count;

        for (int i = 0; i < allCards.Count; i++)
        {
            string value = deck[i];
            allCards[i].SetCardValue(value);
        }

        for (int i = 0; i < aiCards.Count; i++)
        {
            string value = deck[allCards.Count + i];
            aiCards[i].SetCardValue(value);
        }

        deck.RemoveRange(0, totalCards);
    }

    private void OnDrawPileClicked()
    {
        DrawCardFromDeck();
    }

    private void OnDiscardPileClicked()
    {
        if (drawnCard == null && topDiscardCard != null)
        {
            drawnCard = topDiscardCard;
            topDiscardCard = null;
            UpdateDiscardVisual();
            UpdateDrawnCardVisual();
        }
        else if (drawnCard != null)
        {
            PlaceInDiscardPile(drawnCard);
            ClearDrawnCard();
            TurnManager.Instance.EndPlayerTurn();
        }
    }

    public void DrawCardFromDeck()
    {
        if (deck.Count == 0 || drawnCard != null) return;

        drawnCard = deck[0];
        deck.RemoveAt(0);
        UpdateDrawnCardVisual();
    }

    public bool HasDrawnCard()
    {
        return drawnCard != null;
    }

    public string GetDrawnCard()
    {
        return drawnCard;
    }

    public void ClearDrawnCard()
    {
        drawnCard = null;
        UpdateDrawnCardVisual();
    }

    public void PlaceInDiscardPile(string value)
    {
        topDiscardCard = value;
        UpdateDiscardVisual();
    }

    public string PeekTopDiscard()
    {
        return topDiscardCard;
    }

    public string TakeDiscardCard()
    {
        string temp = topDiscardCard;
        topDiscardCard = null;
        UpdateDiscardVisual();
        return temp;
    }

    private void UpdateDiscardVisual()
    {
        if (discardText != null)
            discardText.text = topDiscardCard != null ? topDiscardCard : "";
    }

    private void UpdateDrawnCardVisual()
    {
        if (drawnCardText != null)
            drawnCardText.text = drawnCard != null ? drawnCard : "";
    }

    private void FlipRandomCards(List<CardBehavior> cards, int count)
    {
        List<int> indices = new List<int>();
        for (int i = 0; i < cards.Count; i++) indices.Add(i);

        for (int i = 0; i < count && indices.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, indices.Count);
            int selected = indices[randomIndex];
            indices.RemoveAt(randomIndex);
            cards[selected].FlipCard();
        }
    }

    public void StartPlayerTurn()
    {
        // Enable player interaction, if applicable
    }

    public void NotifyPlayerCardFlipped()
    {
        if (AllPlayerCardsFaceUp())
        {
            TurnManager.Instance.TriggerFinalTurn();
        }
    }

    public void RevealAllPlayerCards()
    {
        foreach (CardBehavior card in allCards)
        {
            if (!card.isFaceUp)
                card.FlipCard();
        }
    }

    private bool AllPlayerCardsFaceUp()
    {
        foreach (CardBehavior card in allCards)
        {
            if (!card.isFaceUp)
                return false;
        }
        return true;
    }

    public List<CardBehavior> GetPlayerCards()
    {
        return allCards;
    }
}
