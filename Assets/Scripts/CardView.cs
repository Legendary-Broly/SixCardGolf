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

        cardText.ForceMeshUpdate();

        // 🔄 Force UI refresh by enabling/disabling the GameObject
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(cardText.rectTransform);
    }

}

