using UnityEngine;
using System.Collections.Generic;

public class HealthManager : MonoBehaviour
{
    public GameState gameState;
    public GameObject heartPrefab;
    public PlayerController player;
    List<HealthHeart> hearts = new List<HealthHeart>();

    private void OnEnable()
    {
        PlayerController.OnPlayerDamaged += DrawHearts;
        RoomManager.OnGenerationComplete += DrawHearts;
    }

    private void OnDisable()
    {
        PlayerController.OnPlayerDamaged -= DrawHearts;
        RoomManager.OnGenerationComplete -= DrawHearts;
    }

    public void Start()
    {
        DrawHearts();
    }

    public void DrawHearts()
    {
        ClearHearts();

        float maxHealthRemainder = gameState.stats.maxHealth % 2;
        int heartsToMake = (int)(gameState.stats.maxHealth / 2 + maxHealthRemainder);

        for (int i = 0; i < heartsToMake; i++)
        {
            CreateEmptyHeart();
        }

        for (int i = 0; i < hearts.Count; i++)
        {
            int heartStatusRemainder = Mathf.Clamp(gameState.stats.PlayerHealth - (i * 2), 0, 2);
            hearts[i].SetHeartImage((HeartStatus)heartStatusRemainder);
        }

    }

    public void CreateEmptyHeart()
    {
        GameObject newHeart = Instantiate(heartPrefab);
        newHeart.transform.SetParent(transform);

        // Fix for heart scaling issue
        RectTransform rectTransform = newHeart.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.localScale = Vector3.one;

            // Optional - if you need specific size, uncomment and adjust values
            // rectTransform.sizeDelta = new Vector2(50, 50);
        }

        HealthHeart heartComponent = newHeart.GetComponent<HealthHeart>();
        heartComponent.SetHeartImage(HeartStatus.Empty);
        hearts.Add(heartComponent);
    }

    public void ClearHearts()
    {
        foreach (Transform t in transform)
        {
            Destroy(t.gameObject);
        }
        hearts = new List<HealthHeart>();
    }

}
