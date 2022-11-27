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

    public float currentVelocity { get { return rigidbody.velocity.magnitude; } }
    public float topSpeed = 12f;

    private Rigidbody rigidbody;

    public void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    public void Drive(float steer, float acceleration)
    {
        steer = Mathf.Clamp(steer, -1, 1);

        float steerAngle = steer * maxSteeringAngle;
        axleInfos[0].leftWheel.steerAngle = steerAngle;
        axleInfos[0].rightWheel.steerAngle = steerAngle;

        ApplyTorque(acceleration);
        LimitSpeed();
    }

    public void ApplyTorque(float acceleration)
    {
        axleInfos[0].leftWheel.motorTorque = maxMotorTorque * acceleration;
        axleInfos[0].rightWheel.motorTorque = maxMotorTorque * acceleration;
    }

    private void LimitSpeed()
    {
        float speed = rigidbody.velocity.magnitude;

        if (speed > topSpeed)
        {
            rigidbody.velocity = topSpeed * rigidbody.velocity.normalized;
        }
    }
}

