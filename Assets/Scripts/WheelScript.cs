using UnityEngine;

public class WheelScript : MonoBehaviour
{
    private WheelCollider wheelCollider;
    public bool WheelColliderLeftTrack { get; set; } = false;

    void Start()
    {
        wheelCollider = GetComponent<WheelCollider>();
    }

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
