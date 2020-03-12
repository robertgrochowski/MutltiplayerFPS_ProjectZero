using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamerasManager : MonoBehaviour {

    public static Dictionary<CameraEnum, Camera> cameras = new Dictionary<CameraEnum, Camera>();
	
    void OnLevelWasLoaded(int level) {
        if (level == 0) return;
        cameras.Clear();

        Camera sceneCamera = GameObject.Find("WorldCamera").GetComponent<Camera>();
        if (!sceneCamera) Debug.LogError("scene camera not found");

        cameras.Add(CameraEnum.sceneCamera, sceneCamera);

    }
}
public enum CameraEnum { sceneCamera }
