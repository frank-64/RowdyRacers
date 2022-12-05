using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CentreOfMass : MonoBehaviour
{
    public Rigidbody body;
    // Start is called before the first frame update
    void Start()
    {
        body.centerOfMass = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
