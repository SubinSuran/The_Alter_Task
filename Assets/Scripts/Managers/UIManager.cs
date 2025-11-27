using UnityEngine;
using TMPro; 
using System.Collections.Generic;
using Photon.Pun;

public class UIManager : MonoBehaviourPun
{
    public static UIManager Instance;

    [Header("Score UI")]
    public TMP_Text p1ScoreText;
    public TMP_Text p2ScoreText;
    public TMP_Text timerText;
    public TMP_Text centerFeedbackText;
    public TMP_Text powerText;

    [Header("Panels")]
    public GameObject gameOverPanel;
    public TMP_Text resultText;

    [Header("Card Areas")]
    public Transform handArea;
    public Transform playedArea;
    public Transform opponentPlayedArea;
    public GameObject cardPrefab; 

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }


    public void UpdateScoreUI(int p1, int p2)
    {
        int myActorNumber = PhotonNetwork.LocalPlayer.ActorNumber;

        if (myActorNumber == 1)
        {
            if (p1ScoreText) p1ScoreText.text = $"YOU: {p1}";
            if (p2ScoreText) p2ScoreText.text = $"OPPONENT: {p2}";
        }
        else
        { 
            if (p1ScoreText) p1ScoreText.text = $"OPPONENT: {p1}";
            if (p2ScoreText) p2ScoreText.text = $"YOU: {p2}";
        }
    }

    public void UpdateTimer(float timeRemaining)
    {
        if (timerText)
            timerText.text = Mathf.CeilToInt(timeRemaining).ToString();
    }

    public void HideTimer()
    {
        if (timerText) timerText.text = "";
    }

    public void ShowGameOver(string result)
    {
        if (gameOverPanel)
        {
            gameOverPanel.SetActive(true);
            resultText.text = result;
        }
    }


    public void ShowFeedback(string message, Color color)
    {
        if (centerFeedbackText)
        {
            centerFeedbackText.text = message;
            centerFeedbackText.color = color;
            centerFeedbackText.gameObject.SetActive(true);

            CancelInvoke(nameof(HideFeedback));

            Invoke(nameof(HideFeedback), 1.5f);
        }
    }

    public void UpdatePowerUI(int current, int max)
    {
        if (powerText) powerText.text = $"COST: {current} / {max}";
    }

    void HideFeedback()
    {
        if (centerFeedbackText)
            centerFeedbackText.gameObject.SetActive(false);
    }
    public void SpawnCardInHand(CardData data, int cardId, List<int> handIdsList)
    {
        GameObject newCard = Instantiate(cardPrefab, handArea);
        CardDisplay display = newCard.GetComponent<CardDisplay>();
        display.Setup(data);
    }

    public void SpawnOpponentCards(List<int> cardIds)
    {
        foreach (int id in cardIds)
        {
            GameObject newCard = Instantiate(cardPrefab, opponentPlayedArea);
            CardDisplay display = newCard.GetComponent<CardDisplay>();
            CardData data = CardDatabase.Instance.GetCardById(id);
            display.Setup(data);
            Destroy(newCard.GetComponent<CardMovement>()); 
        }
    }

    public void ClearTable()
    {
        foreach (Transform child in playedArea) Destroy(child.gameObject);
        foreach (Transform child in opponentPlayedArea) Destroy(child.gameObject);
    }
}