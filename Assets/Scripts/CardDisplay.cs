using TMPro;
using UnityEngine;

public class CardDisplay : MonoBehaviour
{
    [Header("UI Reference")]
    public TMP_Text nameText;
    public TMP_Text costText;
    public TMP_Text powerText;
    public TMP_Text descriptionText;

    [Header("Data")]
    public CardData data;
    
    public void Setup(CardData data)
    {
        this.data = data;
        nameText.text = data.name;
        costText.text = data.cost.ToString();
        powerText.text = data.power.ToString();

        if (data.ability != null)
        {
            descriptionText.text = $"{data.ability.type} ({data.ability.value})";
        }
        else
        {
            descriptionText.text = "No ability";
            descriptionText.text = "No ability";
        }
    }
}
