using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class MouseLook
{
    public float maxSensitivity = 4f;
    public bool clampVerticalRotation = true;
    public float MinimumX = -90F;
    public float MaximumX = 90F;
    public bool smooth;
    public float smoothTime = 5f;
    public bool lockCursor = true;

    public bool lock360Rotation = false;
    float max = 360;
    float min = 0;

    private Quaternion m_CharacterTargetRot;
    private Quaternion m_CameraTargetRot;

    public void Init(Transform character, Transform camera)
    {
        m_CharacterTargetRot = character.localRotation;
        m_CameraTargetRot = camera.localRotation;
    }


    public void LookRotation(Transform character, Transform camera)
    {
        float yRot = CrossPlatformInputManager.GetAxis("Mouse X") * maxSensitivity * GameSettings.gameSettings.mouseSensitivity;
        float xRot = CrossPlatformInputManager.GetAxis("Mouse Y") * maxSensitivity * GameSettings.gameSettings.mouseSensitivity;

        m_CharacterTargetRot *= Quaternion.Euler (0f, yRot, 0f);
        m_CameraTargetRot *= Quaternion.Euler (-xRot, 0f, 0f);

        if(clampVerticalRotation)
            m_CameraTargetRot = ClampRotationAroundXAxis (m_CameraTargetRot);

            camera.localRotation = m_CameraTargetRot;

        /* lock rotation to 180 angle - (ladder) */
        if (lock360Rotation)
        {
            float currentY = character.localRotation.eulerAngles.y;
            float candidateY = m_CharacterTargetRot.eulerAngles.y;

            if ((candidateY >= max && candidateY <= min && max < min) || (max > min && (candidateY >= max || candidateY <= min)))
            {
                if(yRot > 0)
                    m_CharacterTargetRot = Quaternion.Euler(new Vector3(0, max, 0));
                else if(yRot < 0)
                    m_CharacterTargetRot = Quaternion.Euler(new Vector3(0, min, 0));
            }
        }
        character.localRotation = m_CharacterTargetRot;
    }

    
    public void Lock360Rotation(bool Lock, float centerAngle = -1f) 
    {
        if(Lock)
        {
            max = centerAngle + 90;
            min = centerAngle - 90;
            if (max > 360)
                max -= 360;
            if(min  < 0)
                min += 360;
        }

        lock360Rotation = Lock;
    }

    Quaternion ClampRotationAroundXAxis(Quaternion q)
    {
        q.x /= q.w;
        q.y /= q.w;
        q.z /= q.w;
        q.w = 1.0f;

        float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan (q.x);

        angleX = Mathf.Clamp (angleX, MinimumX, MaximumX);

        q.x = Mathf.Tan (0.5f * Mathf.Deg2Rad * angleX);

        return q;
    }

}
