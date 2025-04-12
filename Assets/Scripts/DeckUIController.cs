using UnityEngine;

public class DeckUIController : MonoBehaviour
{
    [Header("Card Display References")]
    [SerializeField] private CardController drawnCardController;
    [SerializeField] private CardController discardCardController;

    private void OnEnable()
    {
        GameEvents.OnCardDrawn += UpdateDrawnCard;
        GameEvents.OnCardDiscarded += UpdateDiscardCard;
    }

    private void OnDisable()
    {
        GameEvents.OnCardDrawn -= UpdateDrawnCard;
        GameEvents.OnCardDiscarded -= UpdateDiscardCard;
    }

    public void UpdateDrawnCard(string value)
    {
        if (drawnCardController == null)
        {
            Debug.LogError("[DeckUI] DrawnCardController reference is missing.");
            return;
        }

        if (string.IsNullOrEmpty(value))
        {
            drawnCardController.Initialize("", false, null); // clear
            return;
        }

        Debug.Log($"[DeckUI] Updating drawn card to: {value}");
        drawnCardController.Initialize(value, true, null);
    }

    public void UpdateDiscardCard(string value)
    {
        if (discardCardController == null)
        {
            Debug.LogError("[DeckUI] DiscardCardController reference is missing.");
            return;
        }

        if (string.IsNullOrEmpty(value))
        {
            discardCardController.Initialize("", false, null); // clear
            return;
        }

        Debug.Log($"[DeckUI] Updating discard card to: {value}");
        discardCardController.Initialize(value, true, null);
    }
}
