using UnityEngine;

public class DeckUIController : MonoBehaviour
{
    [Header("Card Display References")]
    [SerializeField] private CardController drawnCardController;
    [SerializeField] private CardController discardCardController;

    public void UpdateDrawnCard(string value)
    {
        if (drawnCardController == null)
        {
            Debug.LogError("[DeckUI] DrawnCardController reference is missing.");
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

        Debug.Log($"[DeckUI] Updating discard card to: {value}");
        discardCardController.Initialize(value, true, null);
    }
}
