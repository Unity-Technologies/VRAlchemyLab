using NaughtyAttributes;
using UnityEngine;

namespace GameplayIngredients.Events
{
    [RequireComponent(typeof(Collider))]
    public class OnAudioCollisionEvent : EventBase
    {
        [ReorderableList]
        public Callable[] onCollisionEnter;
        [NonNullCheck]
        public AudioImpactLogic audioImpactLogic;
        private void OnCollisionEnter(Collision collision)
        {
            audioImpactLogic.SetCollision(collision);
            Callable.Call(onCollisionEnter, collision.collider.gameObject);
        }
    }
}