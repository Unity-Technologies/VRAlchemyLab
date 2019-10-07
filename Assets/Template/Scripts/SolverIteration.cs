using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SolverIteration : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.solverIterations = 30;
        rb.solverVelocityIterations = 30;
    }

}
