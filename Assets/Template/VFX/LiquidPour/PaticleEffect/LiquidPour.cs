using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

[ExecuteAlways]
[RequireComponent(typeof(VisualEffect))]
public class LiquidPour : MonoBehaviour
{
    [Range(0.01f, 7)]
    public float bottleNeckDiameter = 4;
    [Range(0f, 0.3f)]
    public float StripCutBasedOnFlow = 0.1f;
    public Color color = Color.white;
    public GameObject objectRef = null;
    public bool colorFromObjMat = false;
    public int matId = 2;
    public bool flowFromObjScript = false;

    VisualEffect vfx;
    float flow;

    float verticality;
    Vector3 upVector;
    bool playVFX;

    float currentFilling = 1;
    float oldFilling = 1;

    private void Init()
    {
        vfx = GetComponent<VisualEffect>();
    }

    private void Verif()
    {
        if(vfx == null)
            vfx = GetComponent<VisualEffect>();
    }

    private void UpdateVFX()
    {
        upVector = transform.up;
        verticality = Vector3.Dot(upVector, new Vector3(0, -1, 0));
        flow = Mathf.Clamp01(verticality);
    }

    void Awake()
    {
        Init();
    }

    void Start()
    {
        Verif();
    }

    void Update()
    {
        Verif();
        UpdateVFX();

        //restarting the effect makes strip index change

        if ((!playVFX) && (flow <= StripCutBasedOnFlow))
        {
            playVFX = true;
        }

        if ((playVFX) && (flow > StripCutBasedOnFlow))
        {
            vfx.Play();
            playVFX = false;
        }


        if (colorFromObjMat)
        {
            if (objectRef == null)
            {
                Debug.LogError("The object reference is empty, reseting the color mode");
                colorFromObjMat = false;
            }
            else
            {
                var smr = objectRef.GetComponent<SkinnedMeshRenderer>();
                var m = smr.sharedMaterials[matId];

                var colorFromMat =  m.GetColor("Color_2B85FF3B");
                colorFromMat.a = 1;
                color = colorFromMat;
            }
        }

        if (flowFromObjScript)
        {
            if (objectRef == null)
            {
                Debug.LogError("The object reference is empty, reseting the color mode");
                colorFromObjMat = false;
            }
            else
            {
                //var emptyingSkinned = objectRef.GetComponent<EmptyingSkinned>();
                //currentFilling = emptyingSkinned.filling;
                //float fillingSpeed = Mathf.Abs(oldFilling - currentFilling) / Mathf.Max(Time.deltaTime, 0.0001f);
                //oldFilling = currentFilling;
                //flow = Mathf.Clamp01(fillingSpeed);

                var smr = objectRef.GetComponent<SkinnedMeshRenderer>();
                var m = smr.sharedMaterials[matId];

                currentFilling = Mathf.Clamp01(m.GetFloat("FillingRate"));
                float fillingSpeed = Mathf.Abs(oldFilling - currentFilling) / Mathf.Max(Time.deltaTime, 0.0001f);
                oldFilling = currentFilling;
                flow = Mathf.Clamp01(fillingSpeed);
            }
        }

        vfx.SetFloat("Flow", flow);
        vfx.SetFloat("BottleDiameter(cm)", bottleNeckDiameter);
        vfx.SetVector4("Color", color);

    }
}
