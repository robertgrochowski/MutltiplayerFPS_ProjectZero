using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonLogic : MonoBehaviour {

    public AudioClip mouseOverClip;
    public AudioClip mouseClickClip;

    public Color defaultColor = Color.white;
    public Color fadeColor = new Color(1f, 1f, 1f,0.25f);
    Text activeButtonText;
    bool active = false;
    float passedTime = 0f;

    private void Update() {

        if (!active) passedTime = 0f;
        else
        {
            if (passedTime == 0f)
            {
                StartCoroutine(fadeTextColor(activeButtonText));
                passedTime += Time.deltaTime;
            }
            else if (passedTime >= .8f) passedTime = 0f;
            else passedTime += Time.deltaTime;
        }
    }

    public void buttonMouseOver(GameObject button)
    {
        SoundsManager.localAS.PlayOneShot(mouseOverClip, 0.85f);
        button.GetComponentInChildren<Image>().enabled = true;
        activeButtonText = button.GetComponentInChildren<Text>();
        fadeActivate(true, activeButtonText);
    }
    public void buttonMouseOut(GameObject button)
    {
        button.GetComponentInChildren<Image>().enabled = false;
        fadeActivate(false, activeButtonText);
    }
    public void buttonMouseClick(GameObject button) {
        SoundsManager.localAS.PlayOneShot(mouseClickClip, 0.85f);
    }
    void fadeActivate(bool activate, Text text) {

        active = activate;

        if(!activate)
        {
            StopCoroutine(fadeTextColor(text));
            activeButtonText.CrossFadeColor(defaultColor, 0f, false, true);
            passedTime = 0f;
        }
    }

    IEnumerator fadeTextColor(Text text) {
        text.CrossFadeColor(fadeColor, .4f, false, true);
        yield return new WaitForSeconds(.4f);
        text.CrossFadeColor(defaultColor, .4f, false, true);
        yield return new WaitForSeconds(.4f);
    }
}
