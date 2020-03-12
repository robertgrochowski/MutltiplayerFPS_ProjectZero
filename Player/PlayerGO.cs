using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGO : MonoBehaviour {

    public Player myPlayer;
    public Transform weaponHolderTP;
    public Transform weaponHolderFP;
    public WeaponSwing weaponSwing;
    public Animator playerAnim;

    //Audio
    public AudioSource footstepsAS;
    public AudioSource weaponShootSoundAS;
    public AudioSource weaponsReloadSoundsAS;
    public AudioSource weaponsDrawSoundsAS;
    public AudioSource localAS;
    public Transform audioTR;

    public void Start () {
    
    }
	void Update () {

       //if (myPlayer.gameObject.GetComponent<FPSController>().run) playerAnim.SetFloat("WeaponRunSpeed", 1.3f);
        //else playerAnim.SetFloat("WeaponRunSpeed", 1f);
    }
}
