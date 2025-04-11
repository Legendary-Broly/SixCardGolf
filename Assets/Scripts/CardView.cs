using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardView : MonoBehaviour
{
    [SerializeField] private Image cardFaceBackground;
    [SerializeField] private TextMeshProUGUI cardText;

    public void UpdateVisual(CardModel model)
    {
        Debug.Log($"[CardView] UpdateVisual called | IsFaceUp: {model.IsFaceUp} | Value: {model.Value}");

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

        Debug.Log($"[CardView] Color set to: {cardFaceBackground.color} | Text: {cardText.text}");
    }
}
