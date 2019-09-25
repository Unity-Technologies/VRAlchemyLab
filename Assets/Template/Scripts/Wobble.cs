using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[ExecuteInEditMode]
[RequireComponent(typeof(SkinnedMeshRenderer))]
public class Wobble : MonoBehaviour
{
    public float Decay = 1.0f;
    public float ShakeForce = 0.01f;

    SkinnedMeshRenderer skinnedMeshRenderer;
    Material material;

    float Value;

    Vector3 previousPosition;

    // Start is called before the first frame update
    void Start()
    {
        // We get the mesh renderer
        skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
        // We store a copy of the 1st material
        material = skinnedMeshRenderer.materials[3];

        // Init Wobbling to zero
        Value = 0.0f;
        // Initing previous positino
        previousPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        // Wobbling decay
        Value -= Decay * Time.deltaTime;

        // Wobbling force when moving
        Value += ShakeForce * (Vector3.Distance(transform.position, previousPosition) / Time.deltaTime);

        // Ensure we stay in 0..1 range
        Value = Mathf.Clamp01(Value);
        // Set the material value
        material.SetFloat("WobbleIntensity", Value);

        // Backup position
        previousPosition = transform.position;
    }
}
