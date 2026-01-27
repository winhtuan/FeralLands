using UnityEngine;
using UnityEngine.EventSystems;

public class UIHoverSound : MonoBehaviour, IPointerEnterHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        AudioUI.Instance.PlayHover();
    }
}
