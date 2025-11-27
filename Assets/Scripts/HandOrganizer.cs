using UnityEngine;
using UnityEngine.UI;

public class HandOrganizer : MonoBehaviour
{
    public float handWidth = 800f; 
    public float cardWidth = 150f;
    public float maxSpacing = 10f; 

    private HorizontalLayoutGroup layoutGroup;
    private RectTransform rectTransform;

    void Awake()
    {
        layoutGroup = GetComponent<HorizontalLayoutGroup>();
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        int count = transform.childCount;

        if (count <= 0) return;

        float totalWidthNeeded = count * cardWidth;

        if (totalWidthNeeded <= handWidth)
        {
            layoutGroup.spacing = maxSpacing;
        }
        else
        {
            float excessWidth = totalWidthNeeded - handWidth;
            float spacingReduction = excessWidth / (count - 1);

            layoutGroup.spacing = -spacingReduction;
        }
    }
}