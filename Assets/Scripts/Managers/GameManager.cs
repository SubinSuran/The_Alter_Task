using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Newtonsoft.Json.Linq;


public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;

    [Header("Game State")]
    public int currentTurn = 0;
    public int availableCost = 0;
    public float turnTimer = 30f;
    public bool isPlaningPhase = false;

    [Header("Scoring")]
    public int p1Score = 0;
    public int p2Score = 0;

    [Header("Network State")]
    public int playersReadyCount = 0;
    public List<int> myPlayedCardIds = new List<int>();
    public Dictionary<int, List<int>> serverPendingCards = new Dictionary<int, List<int>>();

    [Header("Card Data")]
    public List<int> deck = new List<int>();
    public List<int> myHandIds = new List<int>();

    
    public Transform playedArea; 

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public override void OnEnable()
    {
        base.OnEnable();
        if (NetworkMessenger.Instance == null) return;
        NetworkMessenger.Instance.OnGameStart += HandleGameStart;
        NetworkMessenger.Instance.OnTurnStart += HandleTurnStart;
        NetworkMessenger.Instance.OnRevealCards += HandleReveal;
        NetworkMessenger.Instance.OnGameEnd += HandleGameEnd;
        NetworkMessenger.Instance.OnPlayerEndedTurn += HandlePlayerEndedTurn;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        if (NetworkMessenger.Instance == null) return;
        NetworkMessenger.Instance.OnGameStart -= HandleGameStart;
        NetworkMessenger.Instance.OnTurnStart -= HandleTurnStart;
        NetworkMessenger.Instance.OnRevealCards -= HandleReveal;
        NetworkMessenger.Instance.OnGameEnd -= HandleGameEnd;
        NetworkMessenger.Instance.OnPlayerEndedTurn -= HandlePlayerEndedTurn;
    }

    void Start()
    {
        if (PhotonNetwork.IsMasterClient) StartCoroutine(StartMatch());
    }

    void Update()
    {
        if (isPlaningPhase)
        {
            turnTimer -= Time.deltaTime;
           
            UIManager.Instance.UpdateTimer(turnTimer);

            if (turnTimer <= 0) OnEndTurnButton();
        }
        else
        {
            UIManager.Instance.HideTimer();
        }
    }

    IEnumerator StartMatch()
    {
        yield return new WaitForSeconds(2f);
        var startData = new
        {
            totalTurns = 6,
            p1Name = PhotonNetwork.PlayerList[0].NickName,
            p2Name = PhotonNetwork.PlayerList[1].NickName
        };
        if (NetworkMessenger.Instance != null)
            NetworkMessenger.Instance.BroadCastEvent("gameStart", startData);
    }

    IEnumerator StartTurn(int turnNumber)
    {
        yield return new WaitForSeconds(1f);
        var turnData = new { turnNumber = turnNumber };
        NetworkMessenger.Instance.BroadCastEvent("turnStart", turnData);
    }

    IEnumerator WaitAndNextTurn()
    {
        yield return new WaitForSeconds(3.0f);
        if (currentTurn >= 6) NetworkMessenger.Instance.BroadCastEvent("gameEnd", null);
        else StartCoroutine(StartTurn(currentTurn + 1));
    }

    void HandleGameStart(string json)
    {
        p1Score = 0; p2Score = 0; currentTurn = 0;
        UIManager.Instance.UpdateScoreUI(0, 0);

        InitializeDeck();
        DrawCard(3);

        if (PhotonNetwork.IsMasterClient) StartCoroutine(StartTurn(1));
    }

    void HandleTurnStart(string json)
    {
        JObject root = JObject.Parse(json);
        currentTurn = (int)root["payload"]["turnNumber"];
        availableCost = currentTurn;

        playersReadyCount = 0;
        myPlayedCardIds.Clear();

        UIManager.Instance.ClearTable();

        UIManager.Instance.ShowFeedback($"TURN {currentTurn} YOU HAVE {availableCost} COST", Color.green);

        UIManager.Instance.UpdatePowerUI(availableCost, currentTurn);

        DrawCard(1);
        turnTimer = 30f;
        isPlaningPhase = true;
    }

    void HandlePlayerEndedTurn(string json)
    {
        JObject root = JObject.Parse(json);
        int playerId = (int)root["payload"]["playerId"];
        List<int> incomingCards = root["payload"]["playedCards"].ToObject<List<int>>();

        if (PhotonNetwork.IsMasterClient)
        {
            if (serverPendingCards.ContainsKey(playerId)) serverPendingCards[playerId] = incomingCards;
            else serverPendingCards.Add(playerId, incomingCards);

            playersReadyCount++;
            if (playersReadyCount >= 2)
            {
                var revealData = new
                {
                    p1Cards = serverPendingCards.ContainsKey(1) ? serverPendingCards[1] : new List<int>(),
                    p2Cards = serverPendingCards.ContainsKey(2) ? serverPendingCards[2] : new List<int>()
                };
                playersReadyCount = 0;
                serverPendingCards.Clear();
                NetworkMessenger.Instance.BroadCastEvent("revealCards", revealData);
            }
        }
    }

    void HandleReveal(string json)
    {
        JObject root = JObject.Parse(json);
        List<int> p1IDs = root["payload"]["p1Cards"].ToObject<List<int>>();
        List<int> p2IDs = root["payload"]["p2Cards"].ToObject<List<int>>();

        int myActorNum = PhotonNetwork.LocalPlayer.ActorNumber;
        List<int> enemyCards = (myActorNum == 1) ? p2IDs : p1IDs;

        UIManager.Instance.SpawnOpponentCards(enemyCards);

        int p1RoundGain = 0;
        int p2RoundGain = 0;
        ResolveAbilities(p1IDs, ref p1RoundGain, ref p2RoundGain);
        ResolveAbilities(p2IDs, ref p2RoundGain, ref p1RoundGain);

        p1Score += p1RoundGain;
        p2Score += p2RoundGain;
        if (p1Score < 0) p1Score = 0;
        if (p2Score < 0) p2Score = 0;

        UIManager.Instance.UpdateScoreUI(p1Score, p2Score);

        if (PhotonNetwork.IsMasterClient) StartCoroutine(WaitAndNextTurn());
    }

    void HandleGameEnd(string json)
    {
        int myActorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
        string result = "DRAW";
        if (p1Score > p2Score) result = (myActorNumber == 1) ? "VICTORY!" : "DEFEAT";
        else if (p2Score > p1Score) result = (myActorNumber == 2) ? "VICTORY!" : "DEFEAT";

        UIManager.Instance.ShowGameOver(result);
    }

    void InitializeDeck()
    {
        deck.Clear();
        for (int i = 1; i <= 12; i++) deck.Add(i);
        for (int i = 0; i < deck.Count; i++)
        {
            int temp = deck[i];
            int rnd = UnityEngine.Random.Range(i, deck.Count);
            deck[i] = deck[rnd];
            deck[rnd] = temp;
        }
    }

    void DrawCard(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            if (deck.Count == 0) return;
            int nextCardId = deck[0];
            deck.RemoveAt(0);
            myHandIds.Add(nextCardId);

            CardData data = CardDatabase.Instance.GetCardById(nextCardId);

            if (data != null)
                UIManager.Instance.SpawnCardInHand(data, nextCardId, myHandIds);
        }
    }

    void ResolveAbilities(List<int> cardIds, ref int myRoundScore, ref int enemyRoundScore)
    {
        foreach (int id in cardIds)
        {
            CardData card = CardDatabase.Instance.GetCardById(id);
            if (card == null) continue;
            int currentCardPower = card.power;
            if (card.ability != null && card.ability.type == "DoublePower")
                currentCardPower *= card.ability.value;
            myRoundScore += currentCardPower;
            if (card.ability != null)
            {
                switch (card.ability.type)
                {
                    case "GainPoints": myRoundScore += card.ability.value; break;
                    case "StealPoints":
                        myRoundScore += card.ability.value;
                        enemyRoundScore -= card.ability.value;
                        break;
                }
            }
        }
    }

    public void OnEndTurnButton()
    {
        if (!isPlaningPhase) return;
        isPlaningPhase = false;
        var data = new
        {
            playerId = PhotonNetwork.LocalPlayer.ActorNumber,
            playedCards = myPlayedCardIds
        };
        NetworkMessenger.Instance.BroadCastEvent("endTurn", data);
    }

    public void SpendCost(int amount)
    {
        availableCost -= amount;
        UIManager.Instance.ShowFeedback($"SPENT {amount} COST\n{availableCost} COST LEFT", Color.yellow);
        UIManager.Instance.UpdatePowerUI(availableCost, currentTurn);
    }
    public void LeaveGame()
    {
        PhotonNetwork.AutomaticallySyncScene = false;
        if (PhotonNetwork.InRoom) PhotonNetwork.LeaveRoom();
        else UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    public override void OnLeftRoom()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
}