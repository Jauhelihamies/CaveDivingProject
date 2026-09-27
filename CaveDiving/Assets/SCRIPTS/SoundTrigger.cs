using UnityEngine;

public class PlayRandomSoundOnTrigger2D : MonoBehaviour
{
    [Header("Ääniasetukset")]
    [SerializeField] private AudioClip[] audioClips; 
    [SerializeField] private AudioSource audioSource; 

    private bool hasPlayed = false; 

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !hasPlayed)
        {
            if (audioClips.Length > 0 && audioSource != null)
            {
                int randomIndex = Random.Range(0, audioClips.Length);
                AudioClip selectedClip = audioClips[randomIndex];
                audioSource.PlayOneShot(selectedClip);
                hasPlayed = true;
            }
            else
            {
                Debug.LogWarning("AudioClips-lista on tyhjä tai AudioSource puuttuu!");
            }
        }
    }
}