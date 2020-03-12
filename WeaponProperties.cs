using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponProperties : MonoBehaviour {

    /* Global Values */
    public AvailableWeapon Enum;
    public string name;
    public string fullName;
    public int magCapacity;
    public int ammo;
    public int damage;
    public Weapon.FiringMode firingMode;
    public Weapon.WeaponType weaponType;
    public Team allowedTeam;
    public float reloadTime;
    public float movementSpeed;

    /* Local Values */
    public float fireRate;
    public float takeTime;
    public float kickback;
    public float kickup;
    public float recoil;
    public Vector3 aimPosition;
    public Vector3 hipPosition;
    

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
