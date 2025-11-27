using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Photon.Pun;
public class NetworkMessenger: MonoBehaviourPun
{
    public static NetworkMessenger Instance;
    public event Action<string> OnGameStart;
    public event Action<string> OnTurnStart;
    public event Action<string> OnPlayerEndedTurn;
    public event Action<string> OnRevealCards;
    public event Action<string> OnGameEnd;


    private void Awake()
    {
        if (Instance == null) 
            Instance = this;
        else Destroy(gameObject);
    }

    public void BroadCastEvent(string actionType, object dataPayload)
    {
       
        var message = new NetworkMessage
        {
            action = actionType,
            payload = dataPayload
        };

        string jsonString = JsonConvert.SerializeObject(message);

        photonView.RPC(nameof(ReceiveMessageRPC), RpcTarget.All, jsonString);

    }

    [PunRPC]
    private void ReceiveMessageRPC(string jsonString)
    {
        JObject root = JObject.Parse(jsonString);
        string action = root["action"].ToString();

        switch (action)
        {
            case "gameStart":
                OnGameStart?.Invoke(jsonString);
                break;
            case "turnStart":
                OnTurnStart?.Invoke(jsonString);
                break;
            case "endTurn": 
                OnPlayerEndedTurn?.Invoke(jsonString); 
                break;
            case "revealCards":
                OnRevealCards?.Invoke(jsonString); 
                break;
            case "gameEnd":
                OnGameEnd?.Invoke(jsonString); 
                break;
            default:
                break;
        }
    }

}

[Serializable]
public class NetworkMessage
{
    public string action;
    public object payload;
}