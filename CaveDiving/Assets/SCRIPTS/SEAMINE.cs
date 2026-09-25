using UnityEngine;
using System.Collections;

public class SEAMINE : MonoBehaviour
{
    public GameObject Mine;
    public AudioSource MineSoundEffects;
    public AudioClip Clank;
    public AudioClip Explosion;
    public float Timer = 1f;
    private Explosion Effect;
    public Fade Fade;
    private int A1 = 0;



    private void Start()
    {
        Effect = GetComponent<Explosion>();


    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (A1 == 0)
            {

                MineSoundEffects.PlayOneShot(Clank);
                StartCoroutine(StartTheBomb());
                A1++;
            }
        }
    }
    IEnumerator StartTheBomb()
    {
        yield return new WaitForSeconds(Timer);

        MineSoundEffects.PlayOneShot(Explosion);
        Effect.ExplosionEffect();
        Mine.SetActive(false);
        Fade.FADEout();


        yield return null;
    }
}