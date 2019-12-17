using GameplayIngredients.Actions;
using System.Collections;
using UnityEngine;
using GameplayIngredients;
public class AudioRollAction : ActionBase
{
    public enum RollAxis { X,Y,Z};
    public RollAxis rollAxis;
    [HideInInspector]
    public Collision collision;
    public float cooldownDuration = 0f;
    public float angularVelocityThreshold = 0.02f;
    bool coolingDown = false;
    [NonNullCheck]
    public AudioSource rollAudioSource;
    public float angularVelocityMultiplier = 2;
    [Range(0,1)]
    public float maximumVolume = 1;
    float currentVolume;
    public float lerpSpeed = 1;

    private void Start()
    {
        rollAudioSource.volume = 0;
    }

    public override void Execute(GameObject instigator = null)
    {
        if(!coolingDown)
        {
            Rigidbody rb = instigator.GetComponent<Rigidbody>();

            //Debug.Log(rb.angularVelocity.ToString());
            if (rb == null) return;

            float angularVelocity = new float();
            switch (rollAxis)
            {
                case RollAxis.X:
                        angularVelocity = Mathf.Abs(rb.angularVelocity.x);
                        break;
                case RollAxis.Y:
                        angularVelocity = Mathf.Abs(rb.angularVelocity.y);
                    break;
                case RollAxis.Z:
                        angularVelocity = Mathf.Abs(rb.angularVelocity.z);
                    break;
            }

            if (angularVelocity < angularVelocityThreshold && rollAudioSource.isPlaying) rollAudioSource.Stop();
            else
            {
                currentVolume = rollAudioSource.volume;
                rollAudioSource.volume = Mathf.Lerp( currentVolume, Mathf.Clamp(angularVelocity * angularVelocityMultiplier, 0, maximumVolume), Time.deltaTime * lerpSpeed);
            }
        }
    }

    IEnumerator CoolDown()
    {
        yield return new WaitForSeconds(cooldownDuration);
        coolingDown = false;
    }
}