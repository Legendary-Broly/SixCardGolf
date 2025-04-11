using UnityEngine;

[RequireComponent(typeof(CardView))]
public class CardController : MonoBehaviour
{
    public CardModel Model { get; private set; }

    private CardView view;
    private ICardInteractionHandler interactionHandler;

    private void Awake()
    {
        view = GetComponent<CardView>();
    }

    public void Initialize(string value, bool isFaceUp, ICardInteractionHandler handler)
    {
        Model = new CardModel(value, isFaceUp);
        interactionHandler = handler;
        view.UpdateVisual(Model);
    }

    public void FlipCard()
    {
        // Just reapply the current visual state
        view.UpdateVisual(Model);
    }

    public void SetCardValue(string newValue)
    {
        if (Model == null)
            Model = new CardModel(newValue, true);
        else
            Model.Value = newValue;

        if (view == null)
            view = GetComponent<CardView>();

        view.UpdateVisual(Model);
    }
    public bool IsFaceUp => Model != null && Model.IsFaceUp;

    public void OnCardClicked()
    {
        interactionHandler?.HandleCardClick(this);
    }
}
