using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
public class LoadLevel : MonoBehaviour
{
    public float StartDelay = 1.0f;
    public string LevelName;
    void Start()
    {
        StartCoroutine(Restart());
    }
    IEnumerator Restart()
    {
        yield return new WaitForSeconds(StartDelay);
        SceneManager.LoadScene(LevelName);
        yield return null;
    }
}
