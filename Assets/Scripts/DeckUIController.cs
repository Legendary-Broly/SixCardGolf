using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DeckUIController : MonoBehaviour
{
    [SerializeField] private Button drawPileButton;
    [SerializeField] private Button discardPileButton;
    [SerializeField] private TextMeshProUGUI discardText;
    [SerializeField] private TextMeshProUGUI drawnCardText;
    [SerializeField] private MonoBehaviour deckRef; // Must implement IDeckSystem

    private IDeckSystem deck;

    private void Awake()
    {
        deck = deckRef as IDeckSystem;
        drawPileButton.onClick.AddListener(OnDrawClicked);
        discardPileButton.onClick.AddListener(OnDiscardClicked);
    }

    private void Update()
    {
        if (deck == null) return;

        // Only update the discard pile text in real-time
        discardText.text = $"Discard: {deck.PeekTopDiscard() ?? "Empty"}";
    }

    private void OnDrawClicked()
    {
        string drawn = deck.DrawCard();
        UpdateDrawnCardText(drawn);
    }

    private void OnDiscardClicked()
    {
        string top = deck.PeekTopDiscard();
        UpdateDrawnCardText(top);
    }

    public void UpdateDrawnCardText(string cardValue)
    {
        drawnCardText.text = $"Drawn: {cardValue}";
    }

    public void UpdateDiscardText(string cardValue)
    {
        discardText.text = $"Discard: {cardValue}";
    }
}
