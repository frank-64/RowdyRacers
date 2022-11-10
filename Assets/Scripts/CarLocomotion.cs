using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[System.Serializable]
public class AxleInfo {
    public WheelCollider leftWheel;
    public WheelCollider rightWheel;
}

//public class CarLocomotion : MonoBehaviour
//{
//    public List<AxleInfo> axleInfos; 
//    public float maxMotorTorque;
//    public float maxSteeringAngle;

//    public void FixedUpdate()
//    {
//        float motor = maxMotorTorque * Input.GetAxis("Vertical");
//        float steering = maxSteeringAngle * Input.GetAxis("Horizontal");
     
//        foreach (AxleInfo axleInfo in axleInfos) {
//            if (axleInfo.steering) {
//                axleInfo.leftWheel.steerAngle = steering;
//                axleInfo.rightWheel.steerAngle = steering;
//            }
//            if (axleInfo.motor) {
//                axleInfo.leftWheel.motorTorque = motor;
//                axleInfo.rightWheel.motorTorque = motor;
//            }
//            // ApplyLocalPositionToVisuals(axleInfo.leftWheel);
//            // ApplyLocalPositionToVisuals(axleInfo.rightWheel);
//        }
//    }
//}

public class CarLocomotion : MonoBehaviour
{
    public List<AxleInfo> axleInfos;
    [SerializeField] private float maxMotorTorque;
    [SerializeField] private float maxSteeringAngle;
    [SerializeField] private float maxBreakTorque = 10000f;

    public float currentVelocity { get { return rigidbody.velocity.magnitude; } }
    public float maxSpeed = 200f;

    private Rigidbody rigidbody;

    public void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    public void Drive(float steer, float breaking, float acceleration)
    {
        steer = Mathf.Clamp(steer, -1, 1);
        float accelerationVal = acceleration = Mathf.Clamp(acceleration, 0, 1);
        breaking = breaking = -1 * Mathf.Clamp(breaking, -1, 0);

        float steerAngle = steer * maxSteeringAngle;
        axleInfos[0].leftWheel.steerAngle = steerAngle;
        axleInfos[0].rightWheel.steerAngle = steerAngle;

        ApplyTorque(acceleration, breaking);
    }

    public void ApplyTorque(float acceleration, float breaking)
    {
        axleInfos[0].leftWheel.motorTorque = maxMotorTorque * acceleration;
        axleInfos[0].rightWheel.motorTorque = maxMotorTorque * acceleration;

        // sort breaking out
        for (int i = 0; i < 1; i++)
        {
            if (currentVelocity > 5 && Vector3.Angle(transform.forward, rigidbody.velocity) < 50f)
            {
                axleInfos[i].leftWheel.brakeTorque = maxBreakTorque * breaking;
                axleInfos[i].rightWheel.brakeTorque = maxBreakTorque * breaking;
            }
            else if (breaking > 0)
            {
                axleInfos[i].rightWheel.brakeTorque = 0f;
                axleInfos[i].leftWheel.brakeTorque = 0f;
            }
        }

    }

    // Add max speed
}

