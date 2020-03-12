using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class buttonEvent : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    public Text myText;

    void Start()
    {
        myText = GetComponentInChildren<Text>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        myText.text = "Hovering";
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        myText.text = "Not Hovering";
    }

    public void OnSubmit(PointerEventData eventData)
    {
        Debug.Log("test");
    }
}