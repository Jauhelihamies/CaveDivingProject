using UnityEngine;

public class SimplePlayerDetection : MonoBehaviour
{
    public Fade GameOverFade;
   
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GameOverFade.FADEout();

        }
    }
}
