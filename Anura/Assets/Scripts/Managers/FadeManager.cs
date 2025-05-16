using UnityEditor.SearchService;
using UnityEngine;

public class FadeManager : MonoBehaviour
{

    [SerializeField] SceneFader sceneFader;

    void Start()
    {
        sceneFader.FadeIn(SceneFader.FadeType.Goop);
    }

    void Update()
    {
        
    }
}
