using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScreenMessageItem : MonoBehaviour {

    public Text content;

    public void Setup(string message) {

        if(PlayerUI.activeMessages.Count >= 7)
        {
            GameObject lastMessage = PlayerUI.activeMessages[0];
            PlayerUI.activeMessages.Remove(lastMessage);
            Destroy(lastMessage);
        }

        PlayerUI.activeMessages.Add(gameObject);

        content.CrossFadeAlpha(0, 0, false);
        content.CrossFadeAlpha(1, 0.5f, false);
        content.text = message;
        gameObject.transform.SetSiblingIndex(0);
        StartCoroutine(fadeAndDestroy());
    }

    IEnumerator fadeAndDestroy() {
        yield return new WaitForSeconds(5f);
        content.CrossFadeAlpha(0f, 2f, false);
        yield return new WaitForSeconds(2f);
        PlayerUI.activeMessages.Remove(gameObject);
        Destroy(gameObject);
    }
}
