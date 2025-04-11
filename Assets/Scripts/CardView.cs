using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardView : MonoBehaviour
{
    [SerializeField] private Image cardImage;
    [SerializeField] private TextMeshProUGUI cardText;

    public void UpdateVisual(CardModel model)
    {
        if (model.IsFaceUp)
        {
            cardImage.enabled = false;
            cardImage.color = Color.white;
            cardImage.enabled = true;
            cardText.text = model.Value;
        }
        else
        {
            cardImage.color = Color.gray;
            cardText.text = "";
        }
    }
}
