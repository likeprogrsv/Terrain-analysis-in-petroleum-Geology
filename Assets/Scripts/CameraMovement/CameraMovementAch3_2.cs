using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovementAch3_2 : MonoBehaviour
{

    public float zSpeed = 0.5f;
    public float yRotSpeed = -0.1f;





    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        transform.position = transform.position + new Vector3(0, 0, zSpeed * Time.deltaTime);
        transform.Rotate(0, yRotSpeed * Time.deltaTime, 0, Space.World);

    }
}
