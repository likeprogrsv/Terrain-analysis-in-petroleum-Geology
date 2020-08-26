using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalizeTest : MonoBehaviour
{

    Vector3 vector1;
    // Start is called before the first frame update
    void Start()
    {
        vector1 = new Vector3(9, 10, 6);
        float mag = vector1.magnitude;
        Debug.Log(mag);
        Vector3 normalized = vector1;
        normalized.Normalize();
        Debug.Log(normalized);  
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
