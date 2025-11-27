using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro;
public class NetworkConnector : MonoBehaviourPunCallbacks
{
    public Button findMatchButton;
    public TMP_Text statusText;
    void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;

       
        findMatchButton.onClick.AddListener(SearchForMatch);

        findMatchButton.interactable = false;

        statusText.text = "Connecting to Server...";

        if (PhotonNetwork.IsConnectedAndReady)
        {
            PhotonNetwork.JoinLobby();
        }
        else
        {
          
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    void SearchForMatch()
    {
        findMatchButton.interactable = false;
        statusText.text = "Searching for Room...";
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnConnectedToMaster()
    {
        statusText.text = "Connected. Joining Lobby...";
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {

        statusText.text = "Ready to Play";
        Debug.Log("Joined Lobby. Waiting for user input.");

        findMatchButton.interactable = true; 
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {

        RoomOptions options = new RoomOptions();
        options.MaxPlayers = 2;

        PhotonNetwork.CreateRoom("null", options);
    }

    public override void OnJoinedRoom()
    {
        statusText.text = "Waiting for Opponent...";

        
        CheckForGameStart();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        CheckForGameStart();
    }

    public void CheckForGameStart()
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            statusText.text = "Starting Game...";

            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.LoadLevel("Game");
            }
        }
    }
}