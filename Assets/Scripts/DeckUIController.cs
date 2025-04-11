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

        discardText.text = $"Discard: {deck.PeekTopDiscard() ?? "Empty"}";
        // Optionally: maintain previous drawn card instead of overwriting
    }

    private void OnDrawClicked()
    {
        string drawn = deck.DrawCard();
        drawnCardText.text = $"Drawn: {drawn}";
    }
    public void UpdateDrawnCardText(string cardValue)
    {
        drawnCardText.text = $"Drawn: {cardValue}";
    }

    private void OnDiscardClicked()
    {
        string top = deck.PeekTopDiscard();
        drawnCardText.text = $"Drawn: {top}";
    }

    public void UpdateDiscardText(string cardValue)
    {
        discardText.text = $"Discard: {cardValue}";
    }

}
