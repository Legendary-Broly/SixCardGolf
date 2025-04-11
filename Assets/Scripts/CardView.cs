using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardView : MonoBehaviour
{
    [SerializeField] private Image cardFaceBackground; // this should be CardFace's Image component
    [SerializeField] private TextMeshProUGUI cardText;

    public void UpdateVisual(CardModel model)
    {
        Debug.Log($"[CardView] UpdateVisual called | IsFaceUp: {model.IsFaceUp} | Value: {model.Value}");

        // Unconditionally apply values for clarity
        cardText.text = model.IsFaceUp ? model.Value : "";
        cardFaceBackground.color = model.IsFaceUp ? Color.white : Color.gray;

        Debug.Log($"[CardView] Color now: {cardFaceBackground.color} | Text now: {cardText.text}");
    }

}

