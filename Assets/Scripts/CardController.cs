using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CardView))]
public class CardController : MonoBehaviour, IPointerClickHandler
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
        if (!Model.IsFaceUp)
        {
            Model.IsFaceUp = true;
            Debug.Log($"[FlipCard] Flipping card with value: {Model.Value}");
            view.UpdateVisual(Model);
        }
    }

    public void SetCardValue(string newValue)
    {
        if (Model == null)
            Model = new CardModel(newValue, false);
        else
            Model.Value = newValue;

        if (view == null)
            view = GetComponent<CardView>();

        view.UpdateVisual(Model);
    }

    public bool IsFaceUp => Model != null && Model.IsFaceUp;

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"[OnPointerClick] Card clicked and flipped. Now face up? {!IsFaceUp}");
        interactionHandler?.HandleCardClick(this);
    }
}
