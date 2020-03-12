using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClassManager : MonoBehaviour {

    public Text className;

    public Image primaryWeaponImage;
    public Image secondaryWeaponImage;
    public Text primaryWeaponName;
    public Text secondaryWeaponName;

	void Start () {
        /* Add default classes*/
        new WeaponClass(AvailableWeapon.acr, AvailableWeapon.deagle);
        new WeaponClass(AvailableWeapon.m4a1s, AvailableWeapon.deagle);
        new WeaponClass(AvailableWeapon.g36c, AvailableWeapon.deagle);

        /* Set default class (1) visible */
        showClass(0);
    }

    public void showClass(int classID) {
        primaryWeaponImage.sprite = PlayerUI.weaponImageSprites[WeaponClass.classes[classID].primaryWeapon];
        secondaryWeaponImage.sprite = PlayerUI.weaponImageSprites[WeaponClass.classes[classID].secondaryWeapon];
        primaryWeaponName.text = Weapon.availableWeapons[WeaponClass.classes[classID].primaryWeapon].fullName;
        secondaryWeaponName.text = Weapon.availableWeapons[WeaponClass.classes[classID].secondaryWeapon].fullName;
        className.text = "Class " + (classID + 1);
    }

    public void spawnWithClass(int classID) {

        Player.myPlayer.weaponClass = WeaponClass.classes[classID];

        if (!Player.myPlayer.inGame)
            GameManager.instance.spawnPlayer();
        else
            PlayerUI.drawScreenMessage("Your class will change on next spawn");

        PlayerUI.instance.switchMenu(null, gameObject);
    }
}

public class WeaponClass
{
    public static List<WeaponClass> classes = new List<WeaponClass>();
    public AvailableWeapon primaryWeapon;
    public AvailableWeapon secondaryWeapon;

    public WeaponClass(AvailableWeapon primaryWeapon, AvailableWeapon secondaryWeapon) {
        this.primaryWeapon = primaryWeapon;
        this.secondaryWeapon = secondaryWeapon;
        classes.Add(this);
    }
}
