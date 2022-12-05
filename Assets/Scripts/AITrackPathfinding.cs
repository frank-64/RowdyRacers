using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AITrackPathfinding : MonoBehaviour
{
    [SerializeField] private Rigidbody AICarRigidbody;

    [SerializeField] private CarLocomotion carLocomotion;

    [SerializeField][Range(0, 1)] private float m_CautiousSpeedFactor = 0.05f;               // percentage of max speed to use when being maximally cautious
    [SerializeField][Range(0, 180)] private float m_CautiousMaxAngle = 40f;                  // angle of approaching corner to treat as warranting maximum caution
    [SerializeField] private float m_CautiousMaxDistance = 100f;                              // distance at which distance-based cautiousness begins
    [SerializeField] private float m_CautiousAngularVelocityFactor = 30f;                     // how cautious the AI should be when considering its own current angular velocity (i.e. easing off acceleration if spinning!)
    [SerializeField] private float m_SteerSensitivity = 0.05f;                                // how sensitively the AI uses steering input to turn to the desired direction
    [SerializeField] private float m_AccelSensitivity = 1f;                                // How sensitively the AI uses the accelerator to reach the current desired speed
    [SerializeField] private float m_BrakeSensitivity = 1f;                                   // How sensitively the AI uses the brake to reach the current desired speed

    private List<Transform> waypoints;
    private Transform targetWaypoint;
    private int waypointIndex = 0;
    private int waypointCount;

    private bool reversing = false;
    private Vector3 previousPosition;
    private float waitTime = 3.0f;
    private float timer = 0.0f;

    [HideInInspector] public bool driving = false;

    private void Start()
    {
        waypoints = GameObject.FindGameObjectsWithTag("Waypoint").Select(waypoint => waypoint.transform).ToList();
        targetWaypoint = waypoints[waypointIndex];
        waypointCount = waypoints.Count();
    }
    IEnumerator CommenceReverse()
    {
        // Drive backwards for 2 seconds then resume aiming for waypoints
        carLocomotion.Drive(0, -1);
        yield return new WaitForSeconds(1.75f);
        reversing = false;
        Debug.Log("AI car is free");
    }

    void Update()
    {
        if (driving)
        {
            timer += Time.deltaTime;

            // Check if we have reached beyond 3 seconds without moving in x axis
            if (timer > waitTime)
            {
                if (Math.Round(previousPosition.x, 1) == Math.Round(AICarRigidbody.position.x, 1))
                {
                    Debug.Log("AI Car is stuck");
                    reversing = true;
                    StartCoroutine(CommenceReverse());
                }
                previousPosition = AICarRigidbody.position;
                timer = timer - waitTime;
            }
        }
        if (Vector3.Distance(AICarRigidbody.position, targetWaypoint.position) < 5f)
        {
            if (waypointIndex != waypointCount - 1)
            {
                waypointIndex += 1;
                targetWaypoint = waypoints[waypointIndex];
            }
        }
    }

    private void FixedUpdate()
    {
        if (targetWaypoint == null || !driving)
        {
            // Car should not be moving,
            // use handbrake to stop
            carLocomotion.Drive(0, 0);
        }
        else if (!reversing)
        {
            Vector3 fwd = transform.forward;
            if (AICarRigidbody.velocity.magnitude > carLocomotion.topSpeed * 0.1f)
            {
                fwd = AICarRigidbody.velocity;
            }

            float desiredSpeed = carLocomotion.topSpeed;

            // the car will brake according to the upcoming change in direction of the target. Useful for route-based AI, slowing for corners.

            // check out the angle of our target compared to the current direction of the car
            float approachingCornerAngle = Vector3.Angle(targetWaypoint.forward, fwd);

            // also consider the current amount we're turning, multiplied up and then compared in the same way as an upcoming corner angle
            float spinningAngle = AICarRigidbody.angularVelocity.magnitude * m_CautiousAngularVelocityFactor;

            // if it's different to our current angle, we need to be cautious (i.e. slow down) a certain amount
            float cautiousnessRequired = Mathf.InverseLerp(0, m_CautiousMaxAngle,
                                                            Mathf.Max(spinningAngle,
                                                                        approachingCornerAngle));
            desiredSpeed = Mathf.Lerp(carLocomotion.topSpeed, carLocomotion.topSpeed * m_CautiousSpeedFactor,
                                        cautiousnessRequired);



            // Overtaking?
            //Vector3 offsetTargetPos = targetWaypoint.position;
            //offsetTargetPos += targetWaypoint.right * m_AvoidPathOffset;

            float accelBrakeSensitivity = (desiredSpeed < carLocomotion.currentSpeed)
                                              ? m_BrakeSensitivity
                                              : m_AccelSensitivity;

            // decide the actual amount of accel/brake input to achieve desired speed.
            float accel = Mathf.Clamp(desiredSpeed * accelBrakeSensitivity, -1, 1);

            Waypoint target = targetWaypoint.GetComponent<Waypoint>(); // Get current target waypoint properties from script

            // If breaking is required for corner and the car has significant speed and the car is within a reasonable distance of the corner start breaking.
            if (target != null && target.breakingRequired && AICarRigidbody.velocity.magnitude > 5 && Vector3.Distance(AICarRigidbody.position, target.transform.position) < 5f)
            {
                // Break
                switch (target.cornerSeverity)
                {
                    case CornerSeverity.Warning:
                        accel = -1f;
                        break;
                    case CornerSeverity.Severe:
                        accel = -0.6f;
                        break;
                    case CornerSeverity.Low:
                        accel = -0.3f;
                        break;
                    default:
                        break;
                }
            }

            // calculate the local-relative position of the target, to steer towards
            Vector3 localTarget = transform.InverseTransformPoint(targetWaypoint.position);

            // work out the local angle towards the target
            float targetAngle = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;

            // get the amount of steering needed to aim the car towards the target
            float steer = Mathf.Clamp(targetAngle * m_SteerSensitivity, -1, 1) * Mathf.Sign(carLocomotion.currentSpeed);

            // feed input to the car controller.
            carLocomotion.Drive(steer, accel);
        }
    }
}