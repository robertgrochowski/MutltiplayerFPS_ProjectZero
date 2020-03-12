using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSwing : MonoBehaviour {

    float speed = 10f;
    float multiplier = 0.003f;

    public static Vector3 kickBack;

    void LateUpdate() {
        Vector3 swing = new Vector3(Mathf.Clamp(transform.localPosition.x - Input.GetAxis("Mouse X") * multiplier, -0.1f, 0.1f), transform.localPosition.y - -Input.GetAxis("Mouse Y") * multiplier, 0);
        kickBack = Vector3.Lerp(new Vector3(kickBack.x, kickBack.y, Mathf.Clamp(kickBack.z, -0.1f, 0f)), Vector3.zero, Time.deltaTime * speed * 0.5f);
        transform.localPosition = Vector3.Lerp(swing + kickBack, Vector3.zero, Time.deltaTime * speed);
    }
}
