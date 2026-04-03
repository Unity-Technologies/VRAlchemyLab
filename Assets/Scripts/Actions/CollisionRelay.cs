using UnityEngine;

public class CollisionRelay : MonoBehaviour
{
    public PlaySoundOnCollision target;

    void OnCollisionEnter(Collision collision)
    {
        target?.OnCollisionReceived(collision.collider);
    }

    void OnTriggerEnter(Collider other)
    {
        target?.OnCollisionReceived(other);
    }
}
