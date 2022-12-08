using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AxleInfo {
    public WheelCollider leftWheel;
    public WheelCollider rightWheel;
}

public class CarLocomotion : MonoBehaviour
{
    public List<AxleInfo> axleInfos;
    public float maxMotorTorque = 500f;
    private float maxSteeringAngle = 50f;

    public float currentSpeed { get { return carRigidbody.velocity.magnitude; } }
    public float topSpeed = 15f;

    private Rigidbody carRigidbody;

    public void Start()
    {
        carRigidbody = GetComponent<Rigidbody>();

        // Give AI car more torque than the player for higher difficulties
        if (carRigidbody.name == "AI Car")
        {
            switch (RaceSettings.RaceDifficulty)
            {
                case RaceDifficulty.Easy: break;
                case RaceDifficulty.Medium: SetDifficulty(15f, 525f); break;
                case RaceDifficulty.Hard: SetDifficulty(15f, 550f); break;
                default:
                    break;
            }
        }
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

    private void SetDifficulty(float speed, float torque)
    {
        topSpeed = speed;
        maxMotorTorque = torque;
    }
}

