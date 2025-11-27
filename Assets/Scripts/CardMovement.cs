using UnityEngine;
using UnityEngine.EventSystems;

public class CardMovement : MonoBehaviour,IBeginDragHandler,IDragHandler,IEndDragHandler
{
    private Transform originalParent;
    private CanvasGroup canvasGroup;
    private CardDisplay cardDisplay;


    public bool isDragging = false;
    private void Awake()
    {
        cardDisplay = GetComponent<CardDisplay>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = false;

        if (!GameManager.Instance.isPlaningPhase) 
        {
            UIManager.Instance.ShowFeedback("WAIT FOR TURN!", Color.yellow);
            return;
            
        }

        int limit = GameManager.Instance.availableCost;

        if (cardDisplay.data.cost > limit)
        {
            UIManager.Instance.ShowFeedback($"NOT ENOUGH COST!", Color.red);
            return;
            
        }

        isDragging = true;

        originalParent = transform.parent;
        transform.SetParent(transform.root);
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        if (canvasGroup.blocksRaycasts) return;
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        canvasGroup.blocksRaycasts = true;

        if (transform.parent == originalParent) return;

        if (eventData.position.y > Screen.height * 0.4f)
        {
            PlayCard();
        }
        else
        {
            ReturnToHand();
        }
    }

    void PlayCard()
    {
       

        if (GameManager.Instance.playedArea != null)
        {
            transform.SetParent(GameManager.Instance.playedArea);

            transform.localPosition = Vector3.zero;
            GameManager.Instance.myPlayedCardIds.Add(cardDisplay.data.id);

            GameManager.Instance.SpendCost(cardDisplay.data.cost);
        }
        else
        {
            Destroy(gameObject);
        }

        this.enabled = false;
    }

    void ReturnToHand()
    {
       
        transform.SetParent(originalParent);

        transform.localPosition = Vector3.zero;
    }
}
