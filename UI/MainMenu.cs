using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{

    public static MainMenu instance;
    ConnectionManager connectionManager;
    public InputField SoldierNameInput;
    public Text SoldierNamePlaceHolder;
    public AudioClip mainMenuSound;
    public GameObject menu;

    [Header("Canvas Groups")]
    public GameObject mainMenuGO;
    public GameObject joinGameGO;
    public GameObject optionsGO;

    [Header("Join Game")]
    public Text connectingState;

    public Canvas canvas;

    string playerName;
    bool connectingToSrv = false;

    void Start()
    {
        DontDestroyOnLoad(gameObject);
        DontDestroyOnLoad(canvas);
        connectionManager = GetComponent<ConnectionManager>();

        playerName = PlayerPrefs.GetString("playerNick", "Unknown Soldier");
        SoldierNamePlaceHolder.text = playerName;
        SoldierNameInput.text = playerName;
        SoundsManager.localAS.clip = mainMenuSound;
        SoundsManager.localAS.loop = true;
       // SoundsManager.localAS.Play();
        instance = this;
    }

    void Update()
    {
        if(connectingToSrv)
            connectingState.text = PhotonNetwork.connectionStateDetailed.ToString();

        if (PhotonNetwork.connectionStateDetailed == ClientState.Joined && connectingToSrv)
        {
            connectingToSrv = false;   
            PlayerUI.instance.OnConnectedToServer();
            Destroy(canvas.gameObject);
            Destroy(gameObject);
        }
    }

    public void Connect()
    {
        PhotonNetwork.sendRate = 30;
        PhotonNetwork.sendRateOnSerialize = 30;
        PhotonNetwork.ConnectUsingSettings("v_0.0");

    }

    void OnJoinedLobby()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }

    public void joinGameButton()
    {
        if (SoldierNameInput.text.Length < 4)
            return;

        connectingToSrv = true;

        Destroy(GetComponent<SoundsManager>());
        joinGameGO.SetActive(true);
        StartCoroutine(PlayerUI.fadeInCanvasGroup(joinGameGO.GetComponent<CanvasGroup>()));

        PlayerPrefs.SetString("playerNick", SoldierNameInput.text);
        TestScript.initalized = true;
        PhotonNetwork.playerName = SoldierNameInput.text;
        Connect();
    }
    public void optionsButton()
    {
        optionsGO.SetActive(true);
        StartCoroutine(PlayerUI.fadeInCanvasGroup(optionsGO.GetComponent<CanvasGroup>()));
    }

    public void creditsButton() {
        NotificationBox.drawNotificationBox(canvas.transform, "Coming soon :)");
    }

    public void leaveGameButton() {
        NotificationBox.drawNotificationBox(canvas.transform, "Do you really want to quit?", NotificationBox.Type.YES_NO, leaveGame);
    }

    public void leaveGame() {
        Application.Quit();
    }
}
