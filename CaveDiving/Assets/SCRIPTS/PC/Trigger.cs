using UnityEngine;

public class Trigger : MonoBehaviour
{
    public GameObject PC;
    void Start()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Trigger Entered");
        PC.SetActive(true);
    }

    void Update()
    {
        
    }
}
