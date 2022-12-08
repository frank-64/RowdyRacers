using UnityEngine;

public class CentreOfMass : MonoBehaviour
{
    public Rigidbody body;
    void Start()
    {
        body.centerOfMass = transform.localPosition;
    }
}
