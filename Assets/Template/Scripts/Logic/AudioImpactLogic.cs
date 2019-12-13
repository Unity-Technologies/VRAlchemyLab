using GameplayIngredients.Logic;
using System.Collections;
using UnityEngine;
using GameplayIngredients;
public class AudioImpactLogic : LogicBase
{
    [HideInInspector]
    public Collision collision;
    public float cooldownDuration = 0.2f;
    public float velocityThreshold = 0.2f;
    bool coolingDown = false;
    [NonNullCheck]
    public AudioSource audioSource;
    public float magnitudeMultiplier = 2;
    [Range(0,1)]
    public float maximumVolume = 1;
    public PhysicMaterial[] physicMaterials;
    public Callable[] onImpactSound;

    public override void Execute(GameObject instigator = null)
    {
        if(!coolingDown && collision.collider.sharedMaterial != null)
        {
            Debug.Log(collision.relativeVelocity.magnitude.ToString());
            audioSource.volume = Mathf.Clamp(collision.relativeVelocity.magnitude * magnitudeMultiplier, 0, maximumVolume);
            if (physicMaterials.Length == onImpactSound.Length)
            {
                for (int i = 0; i < physicMaterials.Length; i++)
                {
                    if (collision.collider.sharedMaterial == physicMaterials[i])
                    {
                        Callable.Call(onImpactSound[i]);
                        if (cooldownDuration > 0)
                        {
                            coolingDown = true;
                            StartCoroutine(CoolDown());
                        }
                    }
                }
            }
            else Debug.Log(gameObject.name + "has wrong impact volume logic setup. Requires as many physic materials as callable.");
        }
    }

    // Start is called before the first frame update
    public void SetCollision(Collision value)
    {
        if ( value.relativeVelocity.magnitude > velocityThreshold && !coolingDown)
        {
            collision = value;
        }        
    }

    IEnumerator CoolDown()
    {
        yield return new WaitForSeconds(cooldownDuration);
        coolingDown = false;
    }
}