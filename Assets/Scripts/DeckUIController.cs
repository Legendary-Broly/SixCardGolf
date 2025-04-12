using UnityEngine;

public class DeckUIController : MonoBehaviour
{
    [SerializeField] private GameObject drawnCardDisplay;
    [SerializeField] private GameObject discardCardDisplay;
    [SerializeField] private GameObject cardPrefab;

    private CardController drawnCardController;
    private CardController discardCardController;

    private void Awake()
    {
        InitializeCardDisplays();
    }

    private void InitializeCardDisplays()
    {
        if (drawnCardDisplay.transform.childCount > 0)
            Destroy(drawnCardDisplay.transform.GetChild(0).gameObject);

        if (discardCardDisplay.transform.childCount > 0)
            Destroy(discardCardDisplay.transform.GetChild(0).gameObject);

        var drawnGO = Instantiate(cardPrefab, drawnCardDisplay.transform);
        drawnCardController = drawnGO.GetComponent<CardController>();

        var discardGO = Instantiate(cardPrefab, discardCardDisplay.transform);
        discardCardController = discardGO.GetComponent<CardController>();

        var deck = FindFirstObjectByType<DeckManager>();
        if (deck != null)
        {
            string top = deck.PeekTopDiscard();
            if (!string.IsNullOrEmpty(top))
            {
                UpdateDiscardCard(top);
            }
        }
    }

    public void UpdateDrawnCard(string value)
    {
        if (drawnCardController != null && !string.IsNullOrEmpty(value))
        {
            drawnCardController.Initialize(value, true, null);
        }
    }

    public void UpdateDiscardCard(string value)
    {
        if (discardCardController != null && !string.IsNullOrEmpty(value))
        {
            discardCardController.Initialize(value, true, null);
        }
    }
}
