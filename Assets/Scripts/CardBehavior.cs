using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardBehavior : MonoBehaviour
{
    public bool isFaceUp = false;
    public string cardValue = "";

    private Image cardFaceImage;
    private TextMeshProUGUI cardText;
    private CardManager cardManager;
    private TurnManager turnManager;

    private void Awake()
    {
        cardFaceImage = GetComponentInChildren<Image>();
        cardText = GetComponentInChildren<TextMeshProUGUI>();
        UpdateCardVisual();
    }

    private void Start()
    {
        cardManager = FindFirstObjectByType<CardManager>();
        turnManager = FindFirstObjectByType<TurnManager>();
        UpdateCardVisual();
    }

    public void FlipCard()
    {
        isFaceUp = true;
        UpdateCardVisual();
    }

    // Add debug logs for cardValue modifications
    public void SetCardValue(string value)
    {
        Debug.Log($"[CardBehavior] Changing cardValue from {cardValue} to {value}");
        cardValue = value;
        UpdateCardVisual();
    }

    private void UpdateCardVisual()
    {
        if (isFaceUp)
        {
            cardFaceImage.color = Color.white;
            if (cardText != null)
                cardText.text = cardValue;
        }
        else
        {
            cardFaceImage.color = Color.gray;
            if (cardText != null)
                cardText.text = "";
        }
    }

    public void OnCardClicked()
    {
        if (cardManager == null) return;

        if (cardManager.HasDrawnCard())
        {
            string replacedValue = cardValue;

            SetCardValue(cardManager.GetDrawnCard());
            if (!isFaceUp)
                FlipCard();

            cardManager.PlaceInDiscardPile(replacedValue);
            cardManager.ClearDrawnCard();

            if (turnManager != null && turnManager.GameStarted)
            {
                Debug.Log("Flip + Swap triggered AI turn.");
                turnManager.EndPlayerTurn();
            }
        }
        else if (!isFaceUp)
        {
            FlipCard();

            if (turnManager != null && turnManager.GameStarted)
            {
                Debug.Log("Flip triggered AI turn.");
                turnManager.EndPlayerTurn();
            }
        }
    }
}
