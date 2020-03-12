using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour {

    const string ON = "On";
    const string OFF = "Off";
    const string SET_KEY = "[Press Key]";

    public Transform canvasTranform;
    public CanvasGroup canvasGroup;

    [Header("Graphic")]
    public Button fullscreenButton;
    public Button resolutionButton;
    public Button textureQualityButton;
    public Button antiAliasingButton;
    public Button vSyncButtonSlider;

    [Header("Controls [ElementID = InputKey ID]")]
    public Text[] controlInputText;
    public Slider mouseSensitivity;
    Dictionary<InputKey, Text> inputKeysTexts = new Dictionary<InputKey, Text>();

    [Header("Audio")]
    public Slider audioVolumeSlider;

    GameSettings newGameSettings;
    delegate void OnKeyPressed(int key);
    OnKeyPressed onKeyPressed;
    InputKey changePretenderKey = InputKey.None;
    bool waitingForKey;

    private void Start() {
        for (int i = 0; i < controlInputText.Length; i++)
            inputKeysTexts.Add((InputKey)i, controlInputText[i]);
    }
    private void OnEnable() {
        newGameSettings = new GameSettings();
        newGameSettings.getCopyOfGameSettings();

        for (int i = 0; i < controlInputText.Length; i++)
            controlInputText[i].text = Controls.inputKeyCode[i].ToString();

        resolutionButton.GetComponentInChildren<Text>().text = GameSettings.resolutions[newGameSettings.resolutionIndex].width + " x " + GameSettings.resolutions[newGameSettings.resolutionIndex].height;
        fullscreenButton.GetComponentInChildren<Text>().text = newGameSettings.fullscreen ? ON : OFF;
        textureQualityButton.GetComponentInChildren<Text>().text = newGameSettings.textureQualities[newGameSettings.textureQuality];
        antiAliasingButton.GetComponentInChildren<Text>().text = newGameSettings.antialiasing != 0 ? newGameSettings.antialiasingValues[newGameSettings.antialiasing] + "X" : OFF;
        vSyncButtonSlider.GetComponentInChildren<Text>().text = newGameSettings.vSyncOptions[newGameSettings.vSync];
        audioVolumeSlider.value = newGameSettings.audioVolume;
        mouseSensitivity.value = newGameSettings.mouseSensitivity;
    }

    #region ClickEventCallMethods
    public void OnFullscreenChange() {

        newGameSettings.fullscreen = !newGameSettings.fullscreen;
        fullscreenButton.GetComponentInChildren<Text>().text = newGameSettings.fullscreen ? ON : OFF;
    }
    public void OnResolutionChange() {

        newGameSettings.resolutionIndex = getValueDependOnButton(newGameSettings.resolutionIndex, GameSettings.resolutions.Length - 1);
        resolutionButton.GetComponentInChildren<Text>().text = GameSettings.resolutions[newGameSettings.resolutionIndex].width + " x " + GameSettings.resolutions[newGameSettings.resolutionIndex].height;
    }
    public void OnTextureQualityChange() {

        newGameSettings.textureQuality = getValueDependOnButton(newGameSettings.textureQuality, 2);
        textureQualityButton.GetComponentInChildren<Text>().text = newGameSettings.textureQualities[newGameSettings.textureQuality];
    }
    public void OnAntiAliasingChange() {
        newGameSettings.antialiasing = getValueDependOnButton(newGameSettings.antialiasing, 3);
        antiAliasingButton.GetComponentInChildren<Text>().text = newGameSettings.antialiasing != 0 ? newGameSettings.antialiasingValues[newGameSettings.antialiasing] + "X" : OFF;
    }
    public void OnVSyncChange() {

        newGameSettings.vSync = getValueDependOnButton(newGameSettings.vSync, 2);
        vSyncButtonSlider.GetComponentInChildren<Text>().text = newGameSettings.vSyncOptions[newGameSettings.vSync];
    }
    public void OnMouseSensitivityChange() {
        newGameSettings.mouseSensitivity = mouseSensitivity.value;
    }
    public void OnAudioVolumeChange() {
        newGameSettings.audioVolume = audioVolumeSlider.value;
        SoundsManager.volume = audioVolumeSlider.value;
    }

    public void OnApplyButton() {
        applyChanges();
        NotificationBox.drawNotificationBox(canvasTranform, "Settings have been applied");
    }

    public void OnBackButton() {
        if (newGameSettings.wereChangesMade())
            NotificationBox.drawNotificationBox(canvasTranform, "Do you want to apply changes?", NotificationBox.Type.YES_NO ,applyChangesAndCloseMenu, discardChangesAndClose);
        else
            closeMenu();
    }

    #endregion
    #region NotificationBoxFunctionCalls

    public void applyChanges() {
        GameSettings.gameSettings = newGameSettings;
        newGameSettings.saveSettings();

        Screen.SetResolution(GameSettings.resolutions[newGameSettings.resolutionIndex].width, GameSettings.resolutions[newGameSettings.resolutionIndex].height, newGameSettings.fullscreen);
        QualitySettings.masterTextureLimit = newGameSettings.textureQuality;
        QualitySettings.vSyncCount = newGameSettings.vSync;
        QualitySettings.antiAliasing = newGameSettings.antialiasingValues[newGameSettings.antialiasing];
        SoundsManager.volume = audioVolumeSlider.value;

        for (int i = 0; i < Controls.inputKeyCode.Length; i++)
            Controls.inputKeyCode[i] = (KeyCode)newGameSettings.inputKeyCode[i];
    }

    public void closeMenu() {
        if (Player.myPlayer == null)
            StartCoroutine(PlayerUI.fadeOutCanvasGroup(canvasGroup, true));
        else
            PlayerUI.instance.onOptionsCloseClick();
    }

    public void discardChangesAndClose() {
        SoundsManager.volume = GameSettings.gameSettings.audioVolume;
        closeMenu();
    }

    public void applyChangesAndCloseMenu() {
        applyChanges();
        closeMenu();
    }

    #endregion
    #region ChangeKeyLogic

    public void OnChangeKeyClick(int key) {

        changePretenderKey = (InputKey)key;
        waitingForKey = true;

        /* Delegate */
        onKeyPressed = null; //Clear delegate
        onKeyPressed += OnChangeKeyPressed;

        inputKeysTexts[changePretenderKey].text = SET_KEY;
    }

    public void OnChangeKeyPressed(int keyboardKeyCodeId) {

        /* pressed key is illegal */
        if (Array.IndexOf(Controls.illegalKeys, (KeyCode)keyboardKeyCodeId) != -1)
        {
            string illegalKeys = "";
            foreach (KeyCode illegalKey in Controls.illegalKeys)
                illegalKeys += illegalKey.ToString() + ", ";

            NotificationBox.drawNotificationBox(canvasTranform, "Illegal key detected. \nDo not use those keys: " + illegalKeys);
            inputKeysTexts[changePretenderKey].text = ((KeyCode)newGameSettings.inputKeyCode[(int)changePretenderKey]).ToString();
            waitingForKey = false;
        }
        else
        {
            /* checking for same assign */
            for (int i = 0; i < newGameSettings.inputKeyCode.Length; i++)
            {
                if (newGameSettings.inputKeyCode[i] == keyboardKeyCodeId)
                {
                    InputKey foundKey = (InputKey)i;
                    newGameSettings.inputKeyCode[i] = (int)InputKey.None;
                    inputKeysTexts[foundKey].text = KeyCode.None.ToString();
                    break;
                }
            }

            /* Setting new key value */
            newGameSettings.inputKeyCode[(int)changePretenderKey] = keyboardKeyCodeId;
            inputKeysTexts[changePretenderKey].text = ((KeyCode)(keyboardKeyCodeId)).ToString();
            changePretenderKey = InputKey.None;
            waitingForKey = false;  
        }
    }

    private void Update() {
        if (waitingForKey)
        {
            foreach (KeyCode vKey in Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKey(vKey))
                {
                    onKeyPressed((int)vKey);
                }
            }
        }
    }

    int getValueDependOnButton(int value, int max) {
        if (Input.GetMouseButton(1)) {
            if (value == 0) value = max;
            else value-= 1;
        }
        else if (Input.GetMouseButton(0))  {
            if (value ==  max) value = 0;
            else value+= 1;
        }
        return value;
    }
    #endregion
}
public class GameSettings
{
    static string dataPath = Application.persistentDataPath + "/gamesettings.json";
    public static GameSettings gameSettings;
    public static Resolution[] resolutions;

    public bool fullscreen;
    public int textureQuality;
    public int antialiasing;
    public int vSync;
    public int resolutionIndex;
    public float mouseSensitivity;
    public float audioVolume;
    public int[] inputKeyCode; 

    public Dictionary<int, string> textureQualities = new Dictionary<int, string>()
    {
        {0, "Low"},
        {1, "Medium"},
        {2, "High"}
    };
    public Dictionary<int, string> vSyncOptions = new Dictionary<int, string>()
    {
        {0, "Don't sync"},
        {1, "Every V Blank"},
        {2, "Every Second V Blank"}
    };
    public int[] antialiasingValues = { 0, 2, 4, 8 };

    /* Create copy of real gameSettings to store temporary changes which user has done */
    public GameSettings getCopyOfGameSettings() {
        GameSettings realGS = gameSettings;
        fullscreen = realGS.fullscreen;
        textureQuality = realGS.textureQuality;
        antialiasing = realGS.antialiasing;
        vSync = realGS.vSync;
        resolutionIndex = realGS.resolutionIndex;
        mouseSensitivity = realGS.mouseSensitivity;
        audioVolume = realGS.audioVolume;
        inputKeyCode = realGS.inputKeyCode;
        return realGS;
    }

    public bool wereChangesMade() {
        for (int i = 0; i < Controls.inputKeyCode.Length; i++)
            if (inputKeyCode[i] != (int)Controls.inputKeyCode[i]) return true;

        GameSettings realGS = gameSettings;
        return !(fullscreen == realGS.fullscreen &&
        textureQuality == realGS.textureQuality &&
        antialiasing == realGS.antialiasing &&
        vSync == realGS.vSync &&
        resolutionIndex == realGS.resolutionIndex &&
        mouseSensitivity == realGS.mouseSensitivity &&
        audioVolume == realGS.audioVolume);
    }

    public static void loadSettings() 
    {
        resolutions = Screen.resolutions;

        if (File.Exists(dataPath)) {
            GameSettings gs = JsonUtility.FromJson<GameSettings>(File.ReadAllText(dataPath));
            for (int i = 0; i < Controls.inputKeyCode.Length; i++)
                Controls.inputKeyCode[i] = (KeyCode)gs.inputKeyCode[i];

            SoundsManager.volume = gs.audioVolume;
            gameSettings = gs;
        }
        else
        {
            gameSettings = new GameSettings();
            gameSettings.loadDefaultSettings();
            Controls.loadDefaultInputKeyCodes();
            gameSettings.inputKeyCode = new int[Controls.inputKeyCode.Length];

            for (int i = 0; i < Controls.inputKeyCode.Length; i++)
                gameSettings.inputKeyCode[i] = (int)Controls.inputKeyCode[i];
        }    
    }

    public void saveSettings() {
        string jsonData = JsonUtility.ToJson(this, false);
        File.WriteAllText(dataPath, jsonData);
    }

    public void loadDefaultSettings() {
        fullscreen = Screen.fullScreen;

        if (!fullscreen)
            resolutionIndex = Application.isEditor ? 0 : getWindowedResolutionId(Screen.width, Screen.height);
        else
            resolutionIndex = Application.isEditor ? 0 : Array.IndexOf(resolutions, Screen.currentResolution);

        textureQuality = QualitySettings.masterTextureLimit;
        antialiasing = Array.IndexOf(antialiasingValues, QualitySettings.antiAliasing);
        vSync = QualitySettings.vSyncCount;
        mouseSensitivity = 0.5f;
        audioVolume = 1f;
    }

    int getWindowedResolutionId(int width, int height) {

        for(int i = 0; i < resolutions.Length; i++)
        {
            if (resolutions[i].width == width && resolutions[i].height == height)
                return i;
        }
        return -1;
    }
}
public class Controls
{
    public static KeyCode[] illegalKeys = { KeyCode.Escape, KeyCode.Tab, KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.Space, KeyCode.Mouse0, KeyCode.Mouse1, KeyCode.R };
    public static KeyCode[] inputKeyCode = new KeyCode[Enum.GetNames(typeof(InputKey)).Length-1];
    public static int sprintKey = (int)InputKey.sprint;
    public static int knifeKey = (int)InputKey.knife;
    public static int crouchKey = (int)InputKey.crouch;
    public static int proneKey = (int)InputKey.prone;

    public static void loadDefaultInputKeyCodes() {
        inputKeyCode[sprintKey] = KeyCode.LeftShift;
        inputKeyCode[knifeKey] = KeyCode.E;
        inputKeyCode[crouchKey] = KeyCode.C;
        inputKeyCode[proneKey] = KeyCode.LeftControl;
    }
}
public enum InputKey
{  /* Do not change the order! */
    None = -1,
    sprint,
    knife,
    crouch,
    prone
};