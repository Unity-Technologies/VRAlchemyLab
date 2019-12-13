using NaughtyAttributes;
using UnityEngine;

namespace GameplayIngredients.Events
{
    [RequireComponent(typeof(Rigidbody))]
    public class OnAudioCollisionEvent : EventBase
    {
        [NonNullCheck]
        public AudioImpactLogic audioImpactLogic;
        [ReorderableList]
        public Callable[] onCollisionEnter;
        private void OnCollisionEnter(Collision collision)
        {
            audioImpactLogic.SetCollision(collision);
            Callable.Call(onCollisionEnter, collision.collider.gameObject);
        }
    }
}