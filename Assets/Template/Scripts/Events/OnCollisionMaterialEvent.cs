using NaughtyAttributes;
using UnityEngine;

namespace GameplayIngredients.Events
{
    [RequireComponent(typeof(Collider))]
    public class OnCollisionMaterialEvent : EventBase
    {
        [ReorderableList]
        public Callable[] onCollisionEnter;

        public bool OnlyInteractWithPhysicMaterial = true;
        [EnableIf("OnlyInteractWithPhysicMaterial")]
        public string PhysicMaterialName = "Wood";

        private void OnCollisionEnter(Collision other)
        {
            //Debug.Log(other.collider.sharedMaterial.name.ToString());
            if (OnlyInteractWithPhysicMaterial && other.collider.sharedMaterial.name == PhysicMaterialName)
            {
                Callable.Call(onCollisionEnter, other.collider.gameObject);
            }
            if (!OnlyInteractWithPhysicMaterial)
            {
                Callable.Call(onCollisionEnter, other.collider.gameObject);
            }
        }
    }
}