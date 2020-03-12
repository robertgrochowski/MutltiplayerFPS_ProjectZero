using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TestScript : MonoBehaviour {

    public static bool initalized = false;


    void Start () {

        if(!initalized)
        {
           SceneManager.LoadScene(0);
        }
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
