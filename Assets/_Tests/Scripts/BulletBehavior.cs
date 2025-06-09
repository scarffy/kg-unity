using UnityEngine;

public class BulletBehavior : MonoBehaviour
{
    public float lifeTime = 10f;
    
    void Start()
    {
        Destroy(gameObject, lifeTime);
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Target"))
        {
            Debug.Log("Hit target!");
            Destroy(gameObject);
        }
    }
}