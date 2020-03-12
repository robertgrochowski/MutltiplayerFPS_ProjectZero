using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;

public class GameManager : Photon.MonoBehaviour {

    public static GameManager instance;

    void Start () {
        instance = this;
    }

    public static void leaveServer() {
        Player.Clear();
        PhotonNetwork.Disconnect();
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    public void spawnPlayer() //Local run
    {
        
        Player.myPlayer.inGame = true;   
        Vector3 spawnOrigin = Vector3.zero;
        Transform spawny =  GameObject.Find("map_" + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name + "/SpawnPoints/" + Player.myPlayer.team.ToString()).transform;
        spawnOrigin = spawny.GetChild(Random.Range(0, spawny.childCount)).position;
        CamerasManager.cameras[CameraEnum.sceneCamera].gameObject.SetActive(false);
        GameObject myPlayerGameObject = PhotonNetwork.Instantiate("newPlayer_new", spawnOrigin, Quaternion.identity, 0);
        WeaponClass weaponClass = Player.myPlayer.weaponClass;
        myPlayerGameObject.GetComponent<FPSController>().enabled = true;
        myPlayerGameObject.GetComponent<PlayerGO>().enabled = true;
        myPlayerGameObject.GetComponent<WeaponsManager>().enabled = true;
        myPlayerGameObject.GetComponent<HealthRegen>().enabled = true;
        myPlayerGameObject.transform.FindChild("CameraHolder").gameObject.SetActive(true);

        /*hud attach*/
        PlayerUI.enableHUD(true);

        /*Attatch weapons*/
        Object[] gameObjects = Resources.LoadAll("Weapons", typeof(GameObject));
        Transform weaponChild = myPlayerGameObject.transform.Find("CameraHolder/Camera/WeaponHolder/Weapon");
        foreach (GameObject gameObject in gameObjects)
        {
            GameObject weapon = Instantiate(gameObject, weaponChild) as GameObject;
            weapon.transform.name = weapon.GetComponent<Weapon>().name;
        }
        /* RPC */
        photonView.RPC("spawnPlayerRPC", PhotonTargets.All, PhotonNetwork.player, myPlayerGameObject.GetComponent<PhotonView>().viewID, (int)weaponClass.primaryWeapon, (int)weaponClass.secondaryWeapon); //Spawnimy dla innych graczy i siebie
        PlayerUI.menuLock = false;
    }
    
    [PunRPC]
    void spawnPlayerRPC(PhotonPlayer photonPlayer, int photonViewID, int primaryWeapon, int secondaryWeapon, PhotonMessageInfo pmi) {
        Player newPlayer = Player.FindPlayer(photonPlayer);

        GameObject newPlayerGameObject = PhotonView.Find(photonViewID).gameObject;
        newPlayerGameObject.name = "Player_" + newPlayer.nick;

        newPlayer.gameObject = newPlayerGameObject;
        newPlayer.Health = 100f;
        newPlayerGameObject.GetComponent<PlayerGO>().myPlayer = newPlayer;

        if (newPlayer == Player.myPlayer)
        {
            newPlayer.gameObject.transform.Find("Model/soldier_mesh").gameObject.SetActive(false);
            newPlayer.gameObject.transform.Find("Model/soldier:Hips").gameObject.SetActive(false);
        }
        else
            newPlayer.gameObject.transform.Find("Model/soldier_mesh").GetComponent<Renderer>().material.mainTexture = Resources.Load<Texture2D>("Player/Player_" + newPlayer.team.ToString());

        Weapon.giveWeapon(newPlayer, (AvailableWeapon)secondaryWeapon);
        Weapon.giveWeapon(newPlayer, (AvailableWeapon)primaryWeapon);
    }

    IEnumerator RespawnPlayer(float time, GameObject player) {
        yield return new WaitForSeconds(time);
        PhotonNetwork.Destroy(player);
        spawnPlayer();
    }

    [PunRPC]
    void addPlayerHealthRPC(PhotonPlayer playerToChange, int health)
    {
        Player player = Player.FindPlayer(playerToChange);
        player.Health += health;
        player.Health = Mathf.Clamp(player.Health, 0, 100f);
    }

    [PunRPC]
    void makePlayerDamageRPC(PhotonPlayer victimPhotonPlayer, int weapon, int menasOfDeath, int damage, PhotonMessageInfo pmi)
    {
        bool dead = false;
        PhotonPlayer attacker = pmi.sender;
        Player victim = Player.FindPlayer(victimPhotonPlayer);

        if (victim.Health <= 0)
            dead = true;

        addPlayerHealthRPC(victimPhotonPlayer, -damage);

        if (victim.Health == 0f && !dead) {
            killVictimRPC(victimPhotonPlayer, pmi.sender, weapon, menasOfDeath);
        }
        else if(!dead) {
            victim.gameObject.GetComponent<PlayerGO>().playerAnim.SetTrigger("Hit"); //TODO?

            if (victim == Player.myPlayer)
            {
                Transform kickGO = victim.gameObject.GetComponent<WeaponsManager>().kickGO;
                kickGO.localRotation = Quaternion.Euler(kickGO.localRotation.eulerAngles - new Vector3(Random.Range(0.5f, 2f), Random.Range(-1f, 1f), 0));

                PlayerUI.drawHitIndicator(Player.FindPlayer(attacker).gameObject.transform.position);
            }
        }
    }

    [PunRPC]
    void killVictimRPC(PhotonPlayer victimPhotonPlayer, PhotonPlayer killerPhotonPlayer, int weapon, int menasOfDeath) //TODO
    {
        Player victim = Player.FindPlayer(victimPhotonPlayer);
        Player killer = Player.FindPlayer(killerPhotonPlayer);

        if (killer != victim)
        {
            killer.kills++;
            killer.score += 10;
            victim.deaths++;
        }

        victim.Health = 0;

        if (victim == Player.myPlayer) {
            PlayerUI.bloodDefocus.CrossFadeAlpha(0, 0, false);
            Camera.main.transform.parent.transform.parent.transform.parent.gameObject.SetActive(false);
            CamerasManager.cameras[CameraEnum.sceneCamera].gameObject.SetActive(true);
            victim.gameObject.transform.Find("Model/soldier_mesh").gameObject.SetActive(true);
            victim.gameObject.transform.Find("Model/soldier:Hips").gameObject.SetActive(true);
            StartCoroutine(RespawnPlayer(5f, victim.gameObject));
        }
        victim.gameObject.GetComponent<Hitbox>().Activate(true, killer.gameObject.transform.forward, 12f);

        PlayerUI.drawKillInfo(killer.nick, victim.nick, weapon, menasOfDeath);
        Debug.Log("killVictimRPC@GameManager: " + victimPhotonPlayer.NickName + " Killed");
    }
}
