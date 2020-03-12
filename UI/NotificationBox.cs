using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NotificationBox : MonoBehaviour {

    public delegate void onClickDelegate();

    public GameObject typeYesNo;
    public GameObject typeOK;
    public Text titleText;
    public Text descriptionText;

    GameObject notificationBox;
    onClickDelegate onPositiveClick;
    onClickDelegate onNegativeClick;

    public void Update() {
        if(Input.GetKeyDown(KeyCode.Escape)) {
            Destroy();
        }
    }

    /* For OK alertbox type onNegativeClick = null */
    public static void drawNotificationBox(Transform parent, string description, Type type = Type.OK, onClickDelegate onPositiveClick = null, onClickDelegate onNegativeClick = null, string title = "Notification") {

        GameObject notificationBox = Resources.Load("notificationBox") as GameObject;
        GameObject itemGO = Instantiate(notificationBox, parent, false) as GameObject;
        NotificationBox item = itemGO.GetComponent<NotificationBox>();

        if (item != null) {
            item.Setup(parent, description, type, onPositiveClick, onNegativeClick, title);
        }
    }

    public void Setup(Transform parent, string description, Type type, onClickDelegate onPositiveClick, onClickDelegate onNegativeClick, string title ) {

        descriptionText.text = description;
        titleText.text = title;

        if (onPositiveClick != null)
            this.onPositiveClick += onPositiveClick;

        if (type == Type.YES_NO)
        {
            if (onNegativeClick != null)
                this.onNegativeClick += onNegativeClick;

            typeYesNo.SetActive(true);
            typeOK.SetActive(false);
        }
    }

    public void Destroy() {
        Object.Destroy(gameObject);
    }

    public void onPositiveClicked() {
        if (onPositiveClick != null) onPositiveClick();
        Destroy();
    }

    public void onNegativeClicked() {
        if (onNegativeClick != null) onNegativeClick();
        Destroy();
    }

    public enum Type
    {
        OK,
        YES_NO
    };
}
