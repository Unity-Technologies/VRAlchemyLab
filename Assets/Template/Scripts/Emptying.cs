using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SkinnedMeshRenderer))]

public class Emptying : MonoBehaviour
{

    float dotResult;
    float invertMaxValue;
    float maxValue;
    float horizontalValue;
    
    float newMaxValue;
    float newInvertMaxValue;
    float newHorizontalValue;

    public float speedMultiplier = 1.0f;
    int i = 0;

    SkinnedMeshRenderer skinnedMeshRenderer;
    Material material;
    
    public Vector3 vectorObj;

    
    // Start is called before the first frame update
    void Start()
    {
        skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
        material = skinnedMeshRenderer.materials[0];
        invertMaxValue = material.GetFloat("InvertMaxLevel");
        maxValue = material.GetFloat("MaxLevel");
        horizontalValue = material.GetFloat("HorizontalMaxLevel");
     }

    // Update is called once per frame
    void Update()
    {
        vectorObj = transform.InverseTransformVector(Vector3.up).normalized;
        dotResult = Vector3.Dot(vectorObj, Vector3.forward);
        float speed = speedMultiplier * Time.deltaTime;

        if (dotResult < 0)
        {
            if (i < 500)
            {
                newInvertMaxValue = invertMaxValue - speed;
                newMaxValue = maxValue - speed;
                newHorizontalValue = horizontalValue - speed;
                
                material.SetFloat("InvertMaxLevel", newInvertMaxValue);
                material.SetFloat("MaxLevel", newMaxValue);
                material.SetFloat("HorizontalMaxLevel", newHorizontalValue);

                invertMaxValue = newInvertMaxValue;
                maxValue = newMaxValue;
                horizontalValue = newHorizontalValue;

                 i++;
             }
         }
    }
}
