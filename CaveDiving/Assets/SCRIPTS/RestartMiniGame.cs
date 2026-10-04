using UnityEngine;

public class ObjectRespawner : MonoBehaviour
{

    public GameObject Minigame;
    private GameObject currentInstance;


    public void RestartObject()
    {

        if (currentInstance != null)
        {
            Destroy(currentInstance);
        }


        currentInstance = Instantiate(Minigame, transform.position, transform.rotation);
        Minigame.SetActive(true);
    }
}