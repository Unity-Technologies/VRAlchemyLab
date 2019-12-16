using NaughtyAttributes;
using UnityEngine;

namespace GameplayIngredients.Events
{
    public class OnCollisionStayEvent : EventBase
    {
        [ReorderableList]
        public Callable[] onCollisionStay;

        private void OnCollisionStay(Collision collision)
        {
            Callable.Call(onCollisionStay, gameObject);
        }
    }
}