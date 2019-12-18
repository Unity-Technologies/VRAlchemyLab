using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImpactObj : MonoBehaviour
{
    public GameObject spawnerObj;

    private void OnCollisionEnter(Collision collision)
    {
        spawnerObj.GetComponent<LiquidPour>().impactCount += 1;
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
