using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CentreOfMass : MonoBehaviour
{
    [SerializeField] private Rigidbody rigidbody;
    // Start is called before the first frame update
    void Start()
    {

        rigidbody.centerOfMass = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
