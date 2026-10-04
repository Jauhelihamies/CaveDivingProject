using UnityEngine;

public class LaunchProgram : MonoBehaviour
{
    public GameObject OPEN_THIS_PROGRAM;

    public void OPEN()
    {
        OPEN_THIS_PROGRAM.SetActive(true);
    }
}
