using UnityEngine;
using UnityEngine.UI; 

using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;
public class SubManager : MonoBehaviour
{

    public static SubManager Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI endGameText;
    [SerializeField] private float restartDelay = 3f;
    public CloseButton StopGame;
    public ObjectRespawner Minigame;
 
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);


        Time.timeScale = 1f;
    }

    public void TriggerWin()
    {
        endGameText.text = "YOU WIN!";
        endGameText.gameObject.SetActive(true);
        StartCoroutine(FreezeAndRestartRoutine());
    }

    public void TriggerLoss()
    {
        endGameText.text = "YOU LOSE!";
        endGameText.gameObject.SetActive(true);
        StartCoroutine(FreezeAndRestartRoutine());
    }

    private IEnumerator FreezeAndRestartRoutine()
    {

        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(restartDelay);
        StopGame.CLOSE();
        yield return null;
    }
}