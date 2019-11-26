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
    public string colorNameInMat = "Color_2B85FF3B";
    public int matId = 0;
    public bool flowFromObjScript = false;
    //public string fillStateNameInMat = "FillingRate";
    public float flowMultiplier = 3;

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

                if (objectRef.GetComponent<SkinnedMeshRenderer>() != null)
                {
                    var smr = objectRef.GetComponent<SkinnedMeshRenderer>();
                    matId = Mathf.Min(matId, smr.sharedMaterials.Length - 1);
                    var colorFromMat = smr.sharedMaterials[matId].GetColor(colorNameInMat);
                    colorFromMat.a = 1;
                    color = colorFromMat;
                }
                else
                {
                    var mr = objectRef.GetComponent<MeshRenderer>();
                    matId = Mathf.Min(matId, mr.sharedMaterials.Length - 1);
                    var colorFromMat = mr.sharedMaterials[matId].GetColor(colorNameInMat);
                    colorFromMat.a = 1;
                    color = colorFromMat;
                }
                
            }
        }

        if (flowFromObjScript)
        {
            if (objectRef == null)
            {
                Debug.LogError("LiquidPourScript: The object reference is empty, reseting the flow mode");
                flowFromObjScript = false;
            }
            else
            {
                Material m = null;

                if (objectRef.GetComponent<EmptyingSkinned>() != null)
                {
                    var es = objectRef.GetComponent<EmptyingSkinned>();

                    float fillingSpeed = Mathf.Clamp01(es.speed);
                    flow *= Mathf.Clamp01(es.filling * 10); // lower the flow on the latest 10%
                }
                else if (objectRef.GetComponent<EmptyingTube>() != null)
                {
                    var es = objectRef.GetComponent<EmptyingTube>();

                    float fillingSpeed = Mathf.Clamp01(es.speed);
                    flow *= Mathf.Clamp01(es.filling * 10); // lower the flow on the latest 10%
                }
                else
                {
                    Debug.LogError("LiquidPourScript: The object reference require an EmptyingSkinned or EmptyingTube Script");
                    flowFromObjScript = false;
                }
            }
        }

        vfx.SetFloat("Flow", flow);
        vfx.SetFloat("BottleDiameter(cm)", bottleNeckDiameter);
        vfx.SetVector4("Color", color);

    }
}
