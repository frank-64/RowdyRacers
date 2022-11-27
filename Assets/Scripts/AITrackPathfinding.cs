using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Vehicles.Car;
using static UnityStandardAssets.Vehicles.Car.CarAIControl;

public class AITrackPathfinding : MonoBehaviour
{
    [SerializeField] private Rigidbody rigidbody;

    [SerializeField] private CarLocomotion carLocomotion;

    [SerializeField][Range(0, 1)] private float m_CautiousSpeedFactor = 0.05f;               // percentage of max speed to use when being maximally cautious
    [SerializeField][Range(0, 180)] private float m_CautiousMaxAngle = 40f;                  // angle of approaching corner to treat as warranting maximum caution
    [SerializeField] private float m_CautiousMaxDistance = 100f;                              // distance at which distance-based cautiousness begins
    [SerializeField] private float m_CautiousAngularVelocityFactor = 30f;                     // how cautious the AI should be when considering its own current angular velocity (i.e. easing off acceleration if spinning!)
    [SerializeField] private float m_SteerSensitivity = 0.05f;                                // how sensitively the AI uses steering input to turn to the desired direction
    [SerializeField] private float m_AccelSensitivity = 1f;                                // How sensitively the AI uses the accelerator to reach the current desired speed
    [SerializeField] private float m_BrakeSensitivity = 1f;                                   // How sensitively the AI uses the brake to reach the current desired speed
    [SerializeField] private float m_LateralWanderDistance = 3f;                              // how far the car will wander laterally towards its target
    [SerializeField] private float m_LateralWanderSpeed = 0.1f;                               // how fast the lateral wandering will fluctuate
    [SerializeField][Range(0, 1)] private float m_AccelWanderAmount = 0.1f;                  // how much the cars acceleration will wander
    [SerializeField] private float m_AccelWanderSpeed = 0.1f;                                 // how fast the cars acceleration wandering will fluctuate
    [SerializeField] private BrakeCondition m_BrakeCondition = BrakeCondition.TargetDistance; // what should the AI consider when accelerating/braking?                                        
    [SerializeField] private bool m_StopWhenTargetReached;                                    // should we stop driving when we reach the target?
    [SerializeField] private float m_ReachTargetThreshold = 2;

    private float m_RandomPerlin;             // A random value for the car to base its wander on (so that AI cars don't all wander in the same pattern)   // Reference to actual car controller we are controlling

    [SerializeField] private LapTime lapTime;
    [HideInInspector] public bool driving = false;
    private bool reversing = false;

    private Vector3 previousPosition;
    private float waitTime = 3.0f;
    private float timer = 0.0f;

    IEnumerator CommenceReverse()
    {
        // Drive backwards for 2 seconds then resume aiming for waypoints
        carLocomotion.Drive(0,-1);
        yield return new WaitForSeconds(1.75f);
        reversing = false;
        Debug.Log("AI car is free");
    }

    void Update()
    {
        if (driving)
        {
            timer += Time.deltaTime;

            // Check if we have reached beyond 2 seconds.
            // Subtracting two is more accurate over time than resetting to zero.
            if (timer > waitTime)
            {
                if (Math.Round(previousPosition.x, 1) == Math.Round(rigidbody.position.x, 1))
                {
                    Debug.Log("AI Car is stuck");
                    reversing = true;
                    StartCoroutine(CommenceReverse());
                }
                previousPosition = rigidbody.position;
                timer = timer - waitTime;
            }
        }
    }

    private void FixedUpdate()
    {
        if (lapTime.targetWaypoint == null || !driving)
        {
            // Car should not be moving,
            // use handbrake to stop
            carLocomotion.Drive(0, 0);
        }
        else if (!reversing)
        {
            //Debug.Log("AI car is heading towards waypoint: "+(waypointIndex+1));

            Vector3 fwd = transform.forward;
            if (rigidbody.velocity.magnitude > carLocomotion.topSpeed * 0.1f)
            {
                fwd = rigidbody.velocity;
            }

            float desiredSpeed = carLocomotion.topSpeed;
      
            // the car will brake according to the upcoming change in direction of the target. Useful for route-based AI, slowing for corners.

            // check out the angle of our target compared to the current direction of the car
            float approachingCornerAngle = Vector3.Angle(lapTime.targetWaypoint.forward, fwd);

            // also consider the current amount we're turning, multiplied up and then compared in the same way as an upcoming corner angle
            float spinningAngle = rigidbody.angularVelocity.magnitude * m_CautiousAngularVelocityFactor;

            // if it's different to our current angle, we need to be cautious (i.e. slow down) a certain amount
            float cautiousnessRequired = Mathf.InverseLerp(0, m_CautiousMaxAngle,
                                                            Mathf.Max(spinningAngle,
                                                                        approachingCornerAngle));
            desiredSpeed = Mathf.Lerp(carLocomotion.topSpeed, carLocomotion.topSpeed * m_CautiousSpeedFactor,
                                        cautiousnessRequired);



            //// Evasive action due to collision with other cars:

            //// our target position starts off as the 'real' target position
            //Vector3 offsetTargetPos = targetWaypoint.position;

            //// if are we currently taking evasive action to prevent being stuck against another car:
            //if (Time.time < m_AvoidOtherCarTime)
            //{
            //    // slow down if necessary (if we were behind the other car when collision occured)
            //    desiredSpeed *= m_AvoidOtherCarSlowdown;

            //    // and veer towards the side of our path-to-target that is away from the other car
            //    offsetTargetPos += targetWaypoint.right * m_AvoidPathOffset;
            //}
            //else
            //{
            //    // no need for evasive action, we can just wander across the path-to-target in a random way,
            //    // which can help prevent AI from seeming too uniform and robotic in their driving
            //    offsetTargetPos += targetWaypoint.right *
            //                       (Mathf.PerlinNoise(Time.time * m_LateralWanderSpeed, m_RandomPerlin) * 2 - 1) *
            //                       m_LateralWanderDistance;
            //}

            // use different sensitivity depending on whether accelerating or braking:
            float accelBrakeSensitivity = (desiredSpeed < carLocomotion.currentVelocity)
                                              ? m_BrakeSensitivity
                                              : m_AccelSensitivity;

            // decide the actual amount of accel/brake input to achieve desired speed.
            float accel = Mathf.Clamp(desiredSpeed * accelBrakeSensitivity, -1, 1);

            // calculate the local-relative position of the target, to steer towards
            Vector3 localTarget = transform.InverseTransformPoint(lapTime.targetWaypoint.position);

            // work out the local angle towards the target
            float targetAngle = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;

            // get the amount of steering needed to aim the car towards the target
            float steer = Mathf.Clamp(targetAngle * m_SteerSensitivity, -1, 1) * Mathf.Sign(carLocomotion.currentVelocity);

            // feed input to the car controller.
            carLocomotion.Drive(steer, accel);
        }
    }

    //public void Update()
    //{
    //    //So we can experiment with the position where the car is checking if it should steer left/right
    //    //doesn't have to be where the wheels are - especially if we are reversing
    //    steerPosition = transform.position + transform.forward * centerSteerDifference;
    //    //Debug.Log("AI is heading towards waypoint:"+waypointIndex);

    //    //Check if we should change waypoint
    //    if (Vector3.Distance(car.position, targetWaypoint) < 2f)
    //    {
    //        waypointIndex += 1;

    //        if (waypointIndex == waypoints.Count)
    //        {
    //            waypointIndex = 0;
    //        }

    //        targetWaypoint = waypoints[waypointIndex].position;

    //        previousWaypoint = DeterminePreviousWaypoint();
    //    }
    //}

    //// Update is called once per frame
    ////public void FixedUpdate()
    ////{
    ////    float motor = maxMotorTorque;

    ////    float steeringAngle = maxSteerAngle * MathHelpers.SteerDirection(transform, steerPosition, targetWaypoint);

    ////    //Average the steering angles to simulate the time it takes to turn the steering wheel
    ////    float averageAmount = 30f;

    ////    averageSteeringAngle = averageSteeringAngle + ((steeringAngle - averageSteeringAngle) / averageAmount);


    ////    foreach (AxleInfo axleInfo in axleInfos)
    ////    {
    ////        if (axleInfo.steering)
    ////        {
    ////            axleInfo.leftWheel.steerAngle = averageSteeringAngle;
    ////            axleInfo.rightWheel.steerAngle = averageSteeringAngle;
    ////        }
    ////        if (axleInfo.motor)
    ////        {
    ////            axleInfo.leftWheel.motorTorque = motor;
    ////            axleInfo.rightWheel.motorTorque = motor;
    ////        }
    ////    }
    ////}
}
