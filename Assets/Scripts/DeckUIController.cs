using UnityEngine;

public class DeckUIController : MonoBehaviour
{
    [SerializeField] private Transform drawnCardDisplay;
    [SerializeField] private Transform discardCardDisplay;
    [SerializeField] private GameObject cardPrefab;

    private GameObject currentDrawnCard;
    private GameObject currentDiscardCard;

    public void UpdateDrawnCard(string value)
    {
        Debug.Log($"[DeckUI] UpdateDrawnCard called with value: {value}");
        UpdateCard(ref currentDrawnCard, drawnCardDisplay, value, true);
    }

    public void UpdateDiscardCard(string value)
    {
        Debug.Log($"[DeckUI] UpdateDiscardCard called with value: {value}");
        UpdateCard(ref currentDiscardCard, discardCardDisplay, value, true);
    }

    private void UpdateCard(ref GameObject cardObject, Transform displayParent, string value, bool isFaceUp)
    {
        if (cardObject != null)
        {
            Destroy(cardObject);
        }

        cardObject = Instantiate(cardPrefab, displayParent);
        cardObject.transform.localPosition = Vector3.zero;

        var controller = cardObject.GetComponent<CardController>();
        if (controller == null)
        {
            Debug.LogError("[DeckUI] Missing CardController on instantiated card.");
            return;
        }

        controller.Initialize(value, isFaceUp, null);

        Debug.Log($"[DeckUI] CardController initialized with value: {value} | FaceUp: {isFaceUp}");
    }
}
