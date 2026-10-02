using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TheEnd : MonoBehaviour
{
    [SerializeField] private float delaySeconds = 5f;
    [SerializeField] private string sceneName = "MainMenu";

    private void Start()
    {
        StartCoroutine(WaitAndLoad());
    }

    private IEnumerator WaitAndLoad()
    {
        yield return new WaitForSeconds(delaySeconds);
        SceneManager.LoadScene(sceneName);
    }
}