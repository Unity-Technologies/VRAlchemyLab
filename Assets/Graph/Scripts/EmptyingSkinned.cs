using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SkinnedMeshRenderer))]

public class EmptyingSkinned : MonoBehaviour
{

    public float dotResult;
    public float filling;

    float newFilling;

    public float speedMultiplier = 1.0f;
    float level = 0.0f;
    public float speedModulate = 0.01f;

    SkinnedMeshRenderer skinnedMeshRenderer;
    Material material;

    Vector3 vectorObj;
    public int idMat = 2;

    [HideInInspector]
    public float speed = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
        material = skinnedMeshRenderer.materials[idMat];

        filling = material.GetFloat("FillingRate");
    }

    // Update is called once per frame
    void Update()
    {
        //Calcul the angle to empty the tube
        vectorObj = transform.InverseTransformVector(Vector3.up).normalized;
        dotResult = Vector3.Dot(vectorObj, Vector3.forward);


        speedModulate = -1 * (dotResult - level - 0.01f);
        speed = speedMultiplier * speedModulate;

        

        if (dotResult < level)
        {
            //Empty the flask
            if (filling > 0)
            {
                newFilling = filling - speed * Time.deltaTime;

                material.SetFloat("FillingRate", newFilling);

                filling = Mathf.Clamp01(newFilling);

            }
            //To correct an artefact when empty.
            if (filling < 0.007)
            {
                material.SetFloat("AlphaModifier", 0.0f);
            }
        }
    }
}
