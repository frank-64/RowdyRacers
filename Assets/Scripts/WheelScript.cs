using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelScript : MonoBehaviour
{
    private WheelCollider wheelCollider;
    public bool WheelColliderLeftTrack { get; set; } = false;
    // Start is called before the first frame update
    void Start()
    {
        wheelCollider = GetComponent<WheelCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        if (wheelCollider.isGrounded)
        {
            WheelHit wheelhit;
            wheelCollider.GetGroundHit(out wheelhit);
            // Wheel collider is on terrain
            if (wheelhit.collider.name != "road_0001")
            {
                WheelColliderLeftTrack = true;
            }
            else
            {
                WheelColliderLeftTrack = false;
            }
        }
    }
}
