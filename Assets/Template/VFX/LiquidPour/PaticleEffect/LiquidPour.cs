using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

[ExecuteAlways]
[RequireComponent(typeof(VisualEffect))]
public class LiquidPour : MonoBehaviour
{
    [Range(0.01f, 7)]
    public float bottleNeckDiameter = 2;
    [Range(0f, 0.3f)]
    public float StripCutBasedOnFlow = 0.01f;
    public Color color = Color.white;

    VisualEffect vfx;
    float flow;

    float verticality;
    Vector3 upVector;
    bool playVFX;

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

        if ((!playVFX) && (flow <= StripCutBasedOnFlow))
        {
            playVFX = true;
        }

        if ((playVFX) && (flow > StripCutBasedOnFlow))
        {
            vfx.Play();
            playVFX = false;
        }
        

        vfx.SetFloat("Flow", flow);
        vfx.SetFloat("BottleDiameter(cm)", bottleNeckDiameter);
        vfx.SetVector4("Color", color);

    }
}
