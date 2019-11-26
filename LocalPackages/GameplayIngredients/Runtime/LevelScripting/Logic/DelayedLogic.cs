using NaughtyAttributes;
using System.Collections;
using UnityEngine;

namespace GameplayIngredients.Logic
{
    public class DelayedLogic : LogicBase
    {
        public enum DelayMode {Constant, Random};
        public DelayMode delayMode;

        public float Delay = 1.0f;

        [ShowIf("DelayIsRandom")]
        public float DelayMax = 2.0f;

        [ReorderableList]
        public Callable[] OnDelayComplete;

        [ReorderableList]
        public Callable[] OnCanceled;

        IEnumerator m_Coroutine;

        private bool DelayIsRandom()
        {
            bool random = delayMode == DelayMode.Random ? true : false;
            return random;
        }

        public void Cancel(GameObject instigator = null)
        {
            if(m_Coroutine != null)
            {
                StopCoroutine(m_Coroutine);
                Callable.Call(OnCanceled, instigator);
                m_Coroutine = null;
            }
        }

        public override void Execute(GameObject instigator = null)
        {
            float newDelay;
            if (m_Coroutine != null) Cancel();

            if (delayMode == DelayMode.Random) newDelay = Random.Range(Delay, DelayMax);

            else newDelay = Delay;

            m_Coroutine = RunDelay(newDelay, instigator);
            StartCoroutine(m_Coroutine);
        }

        IEnumerator RunDelay(float Seconds, GameObject instigator = null)
        {
            yield return new WaitForSeconds(Seconds);
            Callable.Call(OnDelayComplete, instigator);
            m_Coroutine = null;
        }
    }
}