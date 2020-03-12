using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour {

    public int quality = 1;

    public static PlayerUI instance;

    public Canvas canvas;
    public GameObject chatMessageItem;
    public GameObject playerKillItem;
    public GameObject hitDirectionItem;
    public GameObject screenMessageItem;
    public GameObject lowerScreenMessageItem;
    public GameObject ingamePanel;
    public GameObject joinedServerPanel;
    public GameObject classManagerPanel;
    public GameObject optionsPanel;
    public Transform lowerScreenInfoTransform;
    public Transform screenMessageTransform;


    public Sprite fallDamageHudIcon;

    public static GameObject activeMenu;
    public static bool menuLock = false;

    /* HUD */
    public static GameObject HUD;
    public static Text clipAmmo;
    public static Text ammo;
    public static Text health;
    public static Text weaponName;
    public static Image hitmark;
    public static Image bloodDefocus;
    public static GameObject scoreboard;
    public static GameObject messageInput;


    public static List<GameObject> activeMessages;

    public static bool HUDEnabled = false;
    public static bool isTyping = false;
    public static bool isCursorEnabled = true;
    public static Dictionary<AvailableWeapon, Sprite> weaponHudSprites = new Dictionary<AvailableWeapon, Sprite>();
    public static Dictionary<AvailableWeapon, Sprite> weaponImageSprites = new Dictionary<AvailableWeapon, Sprite>();

    private void Awake() {
        instance = this;
        activeMessages = new List<GameObject>();
    }

    public static void Initialize() {
        foreach (var weapon in Weapon.availableWeapons.Values)
        {
            weaponHudSprites.Add(weapon.Enum, Resources.Load<Sprite>("UI/HUD/hud_" + weapon.Enum));
            weaponImageSprites.Add(weapon.Enum, Resources.Load<Sprite>("UI/WeaponImages/weapon_" + weapon.Enum));
        } 
    }

    public static void enableHUD(bool enable) {

        if (HUD == null)
        {
            HUD = GameObject.Find("IngameCanvas").transform.Find("HUD").gameObject;
            HUD.transform.SetSiblingIndex(0);
            scoreboard = HUD.transform.Find("Scoreboard").gameObject;
            messageInput = HUD.transform.Find("messageInput").gameObject;
            clipAmmo = HUD.transform.Find("clip").GetComponent<Text>();
            ammo = HUD.transform.Find("ammo").GetComponent<Text>();
            health = HUD.transform.Find("health").GetComponent<Text>();
            weaponName = HUD.transform.Find("weapon_name").GetComponent<Text>();
            hitmark = HUD.transform.Find("hitmark").GetComponent<Image>();
            bloodDefocus = HUD.transform.Find("bloodDefocus").GetComponent<Image>();
            hitmark.gameObject.SetActive(true);
            bloodDefocus.gameObject.SetActive(true);
            weaponName.CrossFadeAlpha(0, 0, true);
            hitmark.CrossFadeAlpha(0, 0, true);
            bloodDefocus.CrossFadeAlpha(0, 0, true);
        }

        if(enable) HUD.GetComponent<CanvasGroup>().alpha = 1f;
        else       HUD.GetComponent<CanvasGroup>().alpha = 0f;  

        HUDEnabled = enable;
    }

    void Update () {
        if (Input.GetKeyDown(KeyCode.Escape) && !menuLock)
        {
            if (activeMenu != null) /* Some menu is opened */
                switchMenu(null, activeMenu);
            else 
                switchMenu(ingamePanel); /*No menu is opened */
        }

        if (!HUDEnabled) return;

		if(Input.GetKeyDown(KeyCode.Tab)) {
            scoreboard.SetActive(true);
        }
        else if (Input.GetKeyUp(KeyCode.Tab)) {
            scoreboard.SetActive(false);
        }
        else if(Input.GetKeyDown(KeyCode.T) && !isTyping) {
            messageInput.SetActive(true);
            isTyping = true;
            messageInput.GetComponent<InputField>().ActivateInputField();
            messageInput.GetComponent<InputField>().Select();
        }
        else if(Input.GetKeyDown(KeyCode.Escape) && isTyping) {
            isTyping = false;
            messageInput.SetActive(false);
        }
        else if(Input.GetKeyDown(KeyCode.Return) && isTyping) {
            ChatManager.instance.sendMessage(messageInput.GetComponent<InputField>().text);
            messageInput.GetComponent<InputField>().text = "";
            isTyping = false;
            messageInput.SetActive(false);  
        }

        if(Input.GetKeyDown(KeyCode.M))
        {
            Debug.Log(QualitySettings.masterTextureLimit);
            //QualitySettings.masterTextureLimit = quality;
        }
    }

    private void LateUpdate() {
        //if (!Application.isEditor) return;
        if (!isCursorEnabled)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public static void drawScreenMessage(string message) {
        GameObject itemGO = Instantiate<GameObject>(instance.screenMessageItem, instance.screenMessageTransform, false);
        ScreenMessageItem item = itemGO.GetComponent<ScreenMessageItem>();

        if (item != null)
        {
            item.Setup(message);
        }
    }

    public static void drawLowerScreenMessage(string message) {

        GameObject itemGO = Instantiate<GameObject>(instance.lowerScreenMessageItem, instance.lowerScreenInfoTransform, false);
        LowerScreenInfoItem item = itemGO.GetComponent<LowerScreenInfoItem>();

        if (item != null)
        {
            item.SetupMessage(message);
        }
    }

    public static void drawHitIndicator(Vector3 target)
    {
        GameObject itemGO = Instantiate<GameObject>(instance.hitDirectionItem, HUD.transform, false);
        HitDirectionIndicator item = itemGO.GetComponent<HitDirectionIndicator>();

        if (item != null) {
            item.Setup(target);
        }
    }
  
    public static void drawKillInfo(string killer, string victim, int weapon, int meansOfDeath) {
        GameObject itemGO = Instantiate<GameObject>(instance.playerKillItem, instance.lowerScreenInfoTransform, false);
        LowerScreenInfoItem item = itemGO.GetComponent<LowerScreenInfoItem>();

        if (item != null) {
            item.SetupKillInfo(killer, victim, (AvailableWeapon)weapon, (MeansOfDeath)meansOfDeath);
        }
    }

    public static void enableCursor(bool enable) 
    {
        if (!enable)
            Cursor.lockState = CursorLockMode.Locked;
        else
            Cursor.lockState = CursorLockMode.None;

        isCursorEnabled = enable;
    }

    /* inGame Menu */

    public void onChangeClassClick() {
        switchMenu(classManagerPanel, ingamePanel);
    }

    public void onOptionsClick() {
        switchMenu(optionsPanel, ingamePanel);
    }

    public void onOptionsCloseClick() {
        switchMenu(ingamePanel, optionsPanel);
    }
    
    /* Join Game */

    public void OnConnectedToServer() {
        menuLock = true;
        switchMenu(joinedServerPanel);
    }

    public void onJoinGameClicked() {
        switchMenu(classManagerPanel, joinedServerPanel);
    }

    public void onLeaveServerClicked() {
        NotificationBox.drawNotificationBox(canvas.transform, "Do you really want to leave the server?", NotificationBox.Type.YES_NO, GameManager.leaveServer);
    }

    public void switchMenu(GameObject turnON, GameObject turnOFF=null) {
        activeMenu = turnON;

        if (turnON != null) { turnON.SetActive(true); resetButtonsImages(turnON); }
        if (turnOFF != null) turnOFF.SetActive(false);

        if (turnON == null)             showGame(true);
        else if(turnON == ingamePanel)  showGame(false);
    }

    public void showGame(bool show) {
        enableCursor(!show);
        enableHUD(show);
        if (Player.myPlayer.gameObject != null)
            Player.myPlayer.gameObject.GetComponent<PlayerGO>().weaponSwing.enabled = show;

        if (!show && !WeaponsManager.instance.weaponDisabled)
        {
            FPSController.instance.weaponAnimations.CrossFade(FPSController.idleAnimation);
            FPSController.instance.cameraAnimations.CrossFade(FPSController.idleAnimation);
        }
    }

    public void resetButtonsImages(GameObject buttonsParent) {
        Button[] buttons = buttonsParent.GetComponentsInChildren<Button>();

        foreach (var button in buttons)
        {
            Image image = button.GetComponent<Image>();
            if(image != null) image.enabled = false;
        }
            
    }

    public static IEnumerator fadeOutCanvasGroup(CanvasGroup canvasGroup, bool disable = false) {
        float time = .5f;
        while (canvasGroup.alpha > 0)
        {
            canvasGroup.alpha -= Time.deltaTime / time;
            yield return null;
        }
        canvasGroup.gameObject.SetActive(!disable);
    }
    public static IEnumerator fadeInCanvasGroup(CanvasGroup canvasGroup) {
        float time = .5f;
        while (canvasGroup.alpha < 1f)
        {
            canvasGroup.alpha += Time.deltaTime / time;
            yield return null;
        }
    }

    public static IEnumerator fadeOutAfter(Text text, float waitSeconds=5f, float fadeSeconds=1f) {
        yield return new WaitForSeconds(waitSeconds);
        text.CrossFadeAlpha(0f, fadeSeconds, false);
        Debug.Log("koniec");
    }
}
