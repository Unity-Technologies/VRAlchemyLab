using NaughtyAttributes;
using UnityEngine;

namespace GameplayIngredients.StateMachines
{
    [AdvancedHierarchyIcon("Packages/net.peeweek.gameplay-ingredients/Icons/Misc/ic-State.png")]
    public class State : MonoBehaviour
    {
        public string StateName { get { return gameObject.name; } }

        [ReorderableList]
        public Callable[] OnStateEnter;
        [ReorderableList]
        public Callable[] OnStateExit;
        [ReorderableList, ShowIf("AllowUpdateCalls")]
        public Callable[] OnStateUpdate;

        private bool AllowUpdateCalls()
        {
            return GameplayIngredientsSettings.currentSettings.allowUpdateCalls;
        }
    }
}
