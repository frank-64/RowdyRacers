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

public class CarLocomotion : MonoBehaviour
{
    public List<AxleInfo> axleInfos;
    [SerializeField] private float maxMotorTorque;
    [SerializeField] private float maxSteeringAngle;

    public float currentSpeed { get { return carRigidbody.velocity.magnitude; } }
    public float topSpeed = 12f;

    private Rigidbody carRigidbody;

    public void Start()
    {
        carRigidbody = GetComponent<Rigidbody>();
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
        float speed = carRigidbody.velocity.magnitude;

        if (speed > topSpeed)
        {
            carRigidbody.velocity = topSpeed * carRigidbody.velocity.normalized;
        }
    }
}

