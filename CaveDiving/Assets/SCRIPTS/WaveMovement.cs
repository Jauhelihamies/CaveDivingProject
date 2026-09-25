using UnityEngine;

public class WaveMovement : MonoBehaviour
{

    public float Height = 0.5f;


    public float speed = 3f;

    private Vector3 aloitusSijainti;

    void Start()
    {

        aloitusSijainti = transform.position;
    }

    void Update()
    {

        float uP = aloitusSijainti.y + Mathf.Sin(Time.time * speed) * Height;
        transform.position = new Vector3(aloitusSijainti.x,uP, aloitusSijainti.z);
    }
}