using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]

public class CalculOrientation : MonoBehaviour
{

    public float dotResult;

    MeshRenderer meshRenderer;
    Material material;

    Vector3 vectorObj;
    
    // Start is called before the first frame update
    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        material = meshRenderer.materials[0];
    }

    // Update is called once per frame
    void Update()
    {
        vectorObj = transform.InverseTransformVector(Vector3.up);
        dotResult = Vector3.Dot(vectorObj, Vector3.up);
        material.SetFloat("Orientation", dotResult);
     }
}
