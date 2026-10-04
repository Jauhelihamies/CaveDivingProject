using UnityEngine;

public class CloseButton : MonoBehaviour
{
    public GameObject CLOSE_THIS_PROGRAM;

    public void CLOSE()
    {
        CLOSE_THIS_PROGRAM.SetActive(false);
    }

}
