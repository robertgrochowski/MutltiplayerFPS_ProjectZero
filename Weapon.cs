using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour  {

    //Wszystkie bronie dostepne tylko na serwerze! W przypadku clienta, jest tu bron, ktora posiada w ekwipunku (lokalnie)
    public static Dictionary<AvailableWeapon, Weapon> availableWeapons = new Dictionary<AvailableWeapon, Weapon>();

    public static Weapon getWeapon(AvailableWeapon weapon) {
        return availableWeapons[weapon];
    }

    public static void giveWeapon(Player player, AvailableWeapon weaponEnum) {
        //Jeden gracz który dostaje calluje na innych
        //Callowane na pojedynczych graczach
        var weaponInGame = new WeaponInGame();
        weaponInGame.id = WeaponInGame.lastID++;
        Weapon weapon = Weapon.availableWeapons[weaponEnum];
        weaponInGame.weapon = weapon;
        weaponInGame.mag = weapon.magCapacity;
        weaponInGame.ammo = weapon.ammo;

        if (weaponInGame.weapon.weaponType == Weapon.WeaponType.main)
            player.gameObject.GetComponent<WeaponsManager>().primaryWeapon = weaponInGame;
        else if (weaponInGame.weapon.weaponType == Weapon.WeaponType.additional)
            player.gameObject.GetComponent<WeaponsManager>().secondaryWeapon = weaponInGame;

        player.gameObject.GetComponent<WeaponsManager>().changeWeaponRPC((int)weaponInGame.weapon.Enum, player.photonPlayer);
    }

    public static void reloadWeaponProperties(AvailableWeapon weaponID, Weapon weapon)
    {
        weapon.fireRate = 1f / (weapon.RPM / 60f);
        availableWeapons[weaponID] = weapon;
        Debug.Log("Properties reloaded for " + weapon.name);
    }


    public void initalizeWeapon() {
        fireRate = 1f / (RPM / 60f);
        //Debug.Log("Add weapon: " + Enum);
        availableWeapons.Add(Enum, this); 
    }

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
    public float takeTime;
    public float kickback;
    public float kickup;
    public float recoil;
    public Vector3 aimPosition;
    public Vector3 hipPosition;
    [SerializeField]
    private int RPM = 1;
    [HideInInspector]
    public float fireRate;

    public enum FiringMode
    {
        singleFire,
        semiAuto,
        fullAuto
    }
    public enum WeaponType
    {
        main,
        additional,
        knife
    }
}

public class WeaponInGame
{
    public static int lastID = 0; //Tylko na masterclient
    public Player owner;
    public Weapon weapon;
    public int id;
    public int mag;
    public int ammo;
    public WeaponStatus status = WeaponStatus.ready;
}


public enum WeaponStatus {
    ready, reloading
}
public enum AvailableWeapon {
    none=-1,
    acr,
    m4a1s,
    knife,
    deagle,
    g36c,
}
public enum MeansOfDeath
{
    falldamage,
    suicide,
    kill
}