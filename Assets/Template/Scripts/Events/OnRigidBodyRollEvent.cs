using NaughtyAttributes;
using UnityEngine;

namespace GameplayIngredients.Events
{
    [RequireComponent(typeof(Collider))]
    public class OnRidigBodyRollEvent : EventBase
    {
        [ReorderableList]
        public Callable[] onCollisionEnter;

        public bool OnlyInteractWithPhysicMaterial = true;
        [EnableIf("OnlyInteractWithPhysicMaterial")]
        public string PhysicMaterialName = "Wood";
        public float angularVelocityThreshold = 1.0f;

        private void OnCollisionStay(Collision collision)
        {
            var rb = gameObject.GetComponent<Rigidbody>();
            if (rb.angularVelocity.x > angularVelocityThreshold)
            {
                Callable.Call(onCollisionEnter, collision.collider.gameObject);
            }
        }
    }
}