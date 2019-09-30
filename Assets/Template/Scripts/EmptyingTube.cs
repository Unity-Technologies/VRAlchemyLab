using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]

public class EmptyingTube : MonoBehaviour
{

    public float dotResult;
    public float filling;
       
    float newFilling;

    public float speedMultiplier = 1.0f;
    float level = 0.025f;

    MeshRenderer meshRenderer;
    Material material;
    
    Vector3 vectorObj;
    public int idMat = 0;
    
    // Start is called before the first frame update
    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        material = meshRenderer.materials[idMat];
        
        filling = material.GetFloat("FillingRate");
     }

    // Update is called once per frame
    void Update()
    {
        //Calcul the angle to empty the tube
        vectorObj = transform.InverseTransformVector(Vector3.up).normalized;
        dotResult = Vector3.Dot(vectorObj, Vector3.forward);
        
        float speed = speedMultiplier * Time.deltaTime;

        if (dotResult < level)
        {
            //Empty the tube
            if (filling > 0)
            {
                newFilling = filling - speed;
                                
                material.SetFloat("FillingRate", newFilling);

                filling = newFilling;
                
             }
            //To correct an artefact when empty.
            if (filling < 0.007)
            {
                material.SetFloat("AlphaModifier", 0.0f);
            }
         }
    }
}
