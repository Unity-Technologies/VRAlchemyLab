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

    VisualEffect vfx;
    float flow;

    float verticality;
    Vector3 upVector;

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

    // Start is called before the first frame update
    void Start()
    {
        Verif();
    }

    // Update is called once per frame
    void Update()
    {
        Verif();
        UpdateVFX();
        vfx.SetFloat("Flow", flow);
        vfx.SetFloat("BottleDiameter(cm)", bottleNeckDiameter);
        
    }
}
