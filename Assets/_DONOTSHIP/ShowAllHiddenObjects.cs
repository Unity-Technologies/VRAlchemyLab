using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteAlways]
public class ShowAllHiddenObjects : MonoBehaviour
{
    public bool showEverything;
    // Start is called before the first frame update
    void Start()
    {
    }
    // Update is called once per frame
    void Update()
    {
        if (showEverything)
        {
            foreach (GameObject obj in Object.FindObjectsOfType(typeof(GameObject)))
            {
                obj.hideFlags = HideFlags.None;
            }
            showEverything = false;
        }
    }
}