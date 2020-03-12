using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HitDirectionIndicator : MonoBehaviour {

    Vector3 targetVector;
    RectTransform rectTransform;
    Image image;
	
	// Update is called once per frame
	void Update () {

        if (targetVector == Vector3.zero) return;
        Transform playerTransform = FPSController.instance.transform;
        float angle = getAngleOnScreen(playerTransform.position, playerTransform.forward, playerTransform.right, targetVector);

        float r = (Screen.height / 2) * 0.8f;
        float x = Screen.width / 2 + Mathf.Cos(angle * Mathf.Deg2Rad) * r;
        float y = Screen.height / 2 + Mathf.Sin(angle * Mathf.Deg2Rad) * r;

        rectTransform.position = new Vector2(x, Screen.height - y);
        rectTransform.rotation = Quaternion.Euler(0, 0, 270 - angle);

        if(Player.myPlayer.Health <= 0)
        {
            StopCoroutine(fadeAndDestroy());
            Destroy(this.gameObject);
        }
    }

    public void Setup(Vector3 target)
    {
        rectTransform = this.gameObject.transform.GetComponent<RectTransform>();
        image = this.gameObject.transform.GetComponent<Image>();
        targetVector = target;
        StartCoroutine(fadeAndDestroy());
    }

    IEnumerator fadeAndDestroy()
    {
        yield return new WaitForSeconds(3f);
        image.CrossFadeAlpha(0f, 2f, false);
        yield return new WaitForSeconds(2f);
        Destroy(this.gameObject);
    }

    private float getAngleOnScreen(Vector3 player, Vector3 forward, Vector3 right, Vector3 target)
    {
        player.y = 0; target.y = 0;
        Vector3 displacement = player - target;

        float forwardAngle = Vector3.Angle(displacement, forward);
        float rightAngle = Vector3.Angle(displacement, right);

        if (rightAngle >= 90) // Shot come from left side
            forwardAngle = 360 - forwardAngle;

        forwardAngle += 90; //?

        return forwardAngle;
    }
}
