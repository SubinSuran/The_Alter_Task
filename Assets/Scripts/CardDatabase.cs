using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
public class CardDatabase : MonoBehaviour
{

    public static CardDatabase Instance;

    public List<CardData> allCards = new List<CardData>();
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        StartCoroutine(LoadCards());
    }

    IEnumerator LoadCards()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "cards.json");
        string jsonResult = "";

        if (filePath.Contains("://") || filePath.Contains("///"))
        {
            UnityWebRequest www = UnityWebRequest.Get(filePath);
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.Success)
            {
                jsonResult = www.downloadHandler.text;
            }
            else
            {
                Debug.LogError("ERROR: " + www.error);
            }
        }

        else
        {
            if (File.Exists(filePath))
            {
                jsonResult = File.ReadAllText(filePath);
            }
        }

        if (!string.IsNullOrEmpty(jsonResult))
        {

            CardCollection collection = JsonConvert.DeserializeObject<CardCollection>(jsonResult);

            if (collection == null)
            {
                yield break;
            }

            if (collection.cards == null)
            {
                yield break;
            }

            allCards = new List<CardData>(collection.cards);
        }
        else
        {
            Debug.LogError("JSON File was empty");
        }
    }


    public CardData GetCardById(int id)
    {
        return allCards.Find(x => x.id == id);
    }
}
