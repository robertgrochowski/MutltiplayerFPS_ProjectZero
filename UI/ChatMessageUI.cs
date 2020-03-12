using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChatMessageUI : MonoBehaviour {

    [SerializeField]
    Text messageText;

    public void Setup(string playerName, string message) {
        messageText.CrossFadeAlpha(0f, 0f, false);
        messageText.CrossFadeAlpha(1f, 0.4f, false);
        messageText.text = playerName + ": " + message;
        StartCoroutine(fadeAndDestroy());
    }

    IEnumerator fadeAndDestroy() {
        yield return new WaitForSeconds(10f);
        messageText.CrossFadeAlpha(0f, 2f, false);
        yield return new WaitForSeconds(2f);
        Destroy(this.gameObject);
    }
}
