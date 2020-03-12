using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundsManager : Photon.MonoBehaviour {

    public static SoundsManager instance;

    public static Dictionary<AvailableWeapon, AudioClip> weaponsShootSounds = new Dictionary<AvailableWeapon, AudioClip>();
    public static Dictionary<AvailableWeapon, AudioClip> weaponsReloadSounds = new Dictionary<AvailableWeapon, AudioClip>();
    public static Dictionary<AvailableWeapon, AudioClip> weaponsDrawSounds = new Dictionary<AvailableWeapon, AudioClip>();

    public static AudioSource localAS;
    private static float Volume = 1f;

    public static float volume
    {
        set
        {
            Volume = value;
            if(localAS != null) localAS.volume = value;
        }
        get
        {
            return Volume;
        }
    }


    private void Awake() {
        localAS = gameObject.AddComponent<AudioSource>();
        localAS.volume = Volume;
    }

    private void Start() {
        instance = this;
    }

    public static void Initialize() {

        foreach (var weapon in Weapon.availableWeapons.Values) {
            if(weapon.Enum == AvailableWeapon.knife)
            {
                weaponsDrawSounds.Add(weapon.Enum, Resources.Load<AudioClip>("knife/knife_stab1"));
                continue;
            }
            weaponsShootSounds.Add(weapon.Enum, Resources.Load<AudioClip>("WeaponsShotSounds/" + weapon.Enum));
            weaponsReloadSounds.Add(weapon.Enum, Resources.Load<AudioClip>("WeaponsShotSounds/" + weapon.Enum + "_reload"));
            weaponsDrawSounds.Add(weapon.Enum, Resources.Load<AudioClip>("WeaponsShotSounds/" + weapon.Enum + "_draw"));
        }
        int i = 0;
        foreach (var sound in weaponsShootSounds) { 
            if (sound.Value == null) Debug.LogWarning("Nie znaleziono dzwieku broni: "+Weapon.availableWeapons[(AvailableWeapon)i].name);
            i++;
        }
    }

    [PunRPC]
    void playShootSoundRPC(PhotonMessageInfo pmi) {
        Player player = Player.FindPlayer(pmi.sender);
        PlayerGO playerGO = player.gameObject.GetComponent<PlayerGO>();
        playerGO.weaponShootSoundAS.Stop();
       /* Transform al = CamerasManager.cameras[CameraEnum.sceneCamera].transform;

        if (Player.myPlayer.gameObject != null)
            al = Player.myPlayer.gameObject.transform;*/

        playerGO.weaponShootSoundAS.PlayOneShot(weaponsShootSounds[playerGO.GetComponent<WeaponsManager>().activeWeapon.weapon.Enum]);
    }

    [PunRPC]
    void playFootstepSoundRPC(string surface, float speed, PhotonMessageInfo pmi) {
        Player player = Player.FindPlayer(pmi.sender);
        if (player == null || player.gameObject == null) return;
        FootSteps footsteps = player.gameObject.GetComponent<FootSteps>();
        footsteps.playFootstepSound(surface, speed);

        //playerGO.FootSteps.playFootstepSound(surface, speed);
    }
}
