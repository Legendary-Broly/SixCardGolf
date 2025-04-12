using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardView : MonoBehaviour
{
    [SerializeField] private Image cardFaceBackground;
    [SerializeField] private TextMeshProUGUI cardText;

    private void Awake()
    {
        // Ensure both fields are hooked up even in runtime-spawned prefabs
        if (cardFaceBackground == null)
            cardFaceBackground = GetComponentInChildren<Image>();

        if (cardText == null)
            cardText = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void UpdateVisual(CardModel model)
    {
        //Debug.Log($"[CardView] UpdateVisual called | IsFaceUp: {model.IsFaceUp} | Value: {model.Value}");

        if (model.IsFaceUp)
        {
            cardFaceBackground.color = Color.white;
            cardText.text = model.Value;
        }
        else
        {
            cardFaceBackground.color = Color.gray;
            cardText.text = "";
        }

        //Debug.Log($"[CardView] Color now: {cardFaceBackground.color} | Text now: {cardText.text}");
    }
}
