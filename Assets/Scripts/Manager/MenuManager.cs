using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System.Diagnostics;


public class MenuManager : MonoBehaviourPunCallbacks, ILobbyCallbacks
{
    public string playerName;
    public GameObject nameInput;
    [Header("Screens")]
    public GameObject mainScreen;
    public GameObject createRoomScreen;
    public GameObject lobbyScreen;
    public GameObject lobbyBrowserScreen;
    public GameObject IntroScreen;
    public GameObject AudioScreen;

    [Header("Main Screen")]
    public Button createRoomButton;
    public Button findRoomButton;

    [Header("Lobby")]
    public TextMeshProUGUI playerListText;
    public TextMeshProUGUI roomInfoText;
    public Button startGameButton;

    [Header("Lobby Browser")]
    public RectTransform roomListContainer;
    public GameObject roomButtonPrefabs;

    private List<GameObject> roomButtons = new List<GameObject>();
    private List<RoomInfo> roomList = new List<RoomInfo>();

    [Header("Intro & Introduction")]
    public Button openIntroButton;
    public Button closeIntroButton;
    [Header("Audio Screen")]
    public Button openAudioButton;
    public Button closeAudioButton;
    public Button IncreaseMusic;
    public Button DecreaseMusic;
    public Button IncreaseSound;
    public Button DecreaseSound;
    // Start is called before the first frame update
    void Start()
    {
        createRoomButton.interactable = false;
        findRoomButton.interactable = false;

        Cursor.lockState = CursorLockMode.None;

        if(PhotonNetwork.InRoom)
        {
            PhotonNetwork.CurrentRoom.IsVisible = true;
            PhotonNetwork.CurrentRoom.IsOpen = true;
        }
        if (PlayerPrefs.HasKey("Name"))
        {
            playerName = PlayerPrefs.GetString("Name");
            PhotonNetwork.NickName = playerName;
            nameInput.SetActive(false);
        }
        else
        {
            nameInput.SetActive(true);
        }

    }
    public override void OnConnectedToMaster()
    {
        createRoomButton.interactable = true;
        findRoomButton.interactable = true;
        
    }

    public void SetScreen(GameObject screen)
    {
        mainScreen.SetActive(false);
        createRoomScreen.SetActive(false);
        lobbyScreen.SetActive(false);
        lobbyBrowserScreen.SetActive(false);
        IntroScreen.SetActive(false);
        AudioScreen.SetActive(false);
        screen.SetActive(true);

        if (screen == lobbyBrowserScreen)
            UpdateLobbyBrowserUI();
    }
    public void OnIntroScreen()
    {
        IntroScreen.SetActive(true);
    }
    public void OnAudioScreen()
    {
        AudioScreen.SetActive(true);
    }
    public void OutAudioScreen()
    {
        AudioScreen.SetActive(false);
    }

    public void OutIntroScreen()
    {
        IntroScreen.SetActive(false);
    }
    public void OnBackToMainScreen()
    {
         SetScreen(mainScreen);
    }
    public void OnScreenRoomButton()
    {
        AudioManager.instance.PlaySFX(1);
        if (playerName.Length < 2)
        {
            return;
        }
        else
            SetScreen(createRoomScreen);
    }
    public void OnFindRoomButton()
    {
        AudioManager.instance.PlaySFX(1);
        if (playerName.Length < 2)
        {
            return;
        }
        else
            SetScreen(lobbyBrowserScreen);
    }

    public void OnCreateRoom(TMP_InputField roomNameInput)
    {
        if (PhotonNetwork.NickName.Length < 2)
        {
            return;
        }
        else
            NetworkManager.instance.CreateRoom(roomNameInput.text);
        AudioManager.instance.PlaySFX(1);
    }

    public void OnPlayerNameChanged(TMP_InputField playerNameInput)
    {
        playerName = playerNameInput.text;
        if (playerName.Length >= 2)
            PlayerPrefs.SetString("Name", playerName);
        AudioManager.instance.PlaySFX(1);
    }

    public void OnRefreshButton()
    {
        UpdateLobbyBrowserUI();
        AudioManager.instance.PlaySFX(1);
    }

    public override void OnJoinedRoom()
    {
        SetScreen(lobbyScreen);
        photonView.RPC("UpdateLobbyUI", RpcTarget.All);
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdateLobbyUI();
    }
    [PunRPC]
    void UpdateLobbyUI()
    {
        startGameButton.interactable = PhotonNetwork.IsMasterClient;
        playerListText.text = "";
        foreach (Player player in PhotonNetwork.PlayerList)
            playerListText.text += player.NickName + "\n";

        roomInfoText.text = "<b> RoomName </b> \n" + PhotonNetwork.CurrentRoom.Name;
    }
    public void OnstartGameButton()
    {
        AudioManager.instance.PlaySFX(1);
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;
        NetworkManager.instance.photonView.RPC("ChangeScene", RpcTarget.All, "Game");
    }
    public void OnLeaveLobbyButton()
    {
        AudioManager.instance.PlaySFX(1);
        PhotonNetwork.LeaveRoom();
        SetScreen(mainScreen);
        AudioManager.instance.PlaySFX(1);
    }

    GameObject CreateRoomButton()
    {
        GameObject buttonObject = Instantiate(roomButtonPrefabs, roomListContainer.transform);
        roomButtons.Add(buttonObject);
        return buttonObject;
    }
    void UpdateLobbyBrowserUI()
    {
        foreach(GameObject button in roomButtons)
        {
            button.SetActive(false);
        }

        for(int x = 0; x < roomList.Count ; x++)
        {
            GameObject button = (x >= roomButtons.Count) ? CreateRoomButton() : roomButtons[x];

            button.SetActive(true);
            button.transform.Find("Room Name Text").GetComponent<TextMeshProUGUI>().text = roomList[x].Name;
            button.transform.Find("Player Counter Text").GetComponent<TextMeshProUGUI>().text = roomList[x].PlayerCount + " / " + roomList[x].MaxPlayers;

            Button buttoncomp = button.GetComponent<Button>();
            string roomName = roomList[x].Name;

            buttoncomp.onClick.RemoveAllListeners();
            buttoncomp.onClick.AddListener(() => { OnJoinRoomButton(roomName); });
            
        } 
    }
    public void OnJoinRoomButton(string roomName)
    {
        NetworkManager.instance.JoinRoom(roomName);
    }

    public override void OnRoomListUpdate(List<RoomInfo> allRooms)
    {
        roomList = allRooms; ;
    }
}
