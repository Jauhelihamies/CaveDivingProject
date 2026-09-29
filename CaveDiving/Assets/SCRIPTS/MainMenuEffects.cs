using System.Collections;
using UnityEngine;

public class MainMenuEffects : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip startSound;
    [SerializeField] private ParticleSystem[] smokeParticlesArray;

    [Header("Tuhoutumisasetukset")]
    [Tooltip("Kuinka monta sekuntia odotetaan partikkelien k‰ynnistymisen j‰lkeen ennen kuin t‰m‰ objekti tuhotaan.")]
    [SerializeField] private float destroyDelay = 3f;

    public void StartSmokes()
    {
        if (audioSource != null && startSound != null)
        {
            audioSource.PlayOneShot(startSound);
        }

        if (smokeParticlesArray != null && smokeParticlesArray.Length > 0)
        {
            // K‰ynnistet‰‰n Coroutine, joka hoitaa k‰ynnistyksen ja tuhoamisen viiveell‰
            StartCoroutine(CleanStartRoutine());
        }
        else
        {
            Debug.LogWarning("Smoke Particles -listaan ei ole lis‰tty yht‰‰n efekti‰!");
        }
    }

    private IEnumerator CleanStartRoutine()
    {
        // Alustetaan partikkelit
        foreach (ParticleSystem particle in smokeParticlesArray)
        {
            if (particle != null)
            {
                particle.gameObject.SetActive(true);
                particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        yield return null;

        // K‰ynnistet‰‰n partikkelit
        foreach (ParticleSystem particle in smokeParticlesArray)
        {
            if (particle != null)
            {
                particle.Play();
            }
        }

        yield return new WaitForSeconds(destroyDelay);
        Destroy(gameObject);
    }
}