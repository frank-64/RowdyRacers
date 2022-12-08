using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

public class AITrackPathfinding : MonoBehaviour
{
    [SerializeField] private CarLocomotion carLocomotion;

    // Variables used for Unity standard assets pathfinding code
    private float m_CautiousSpeedFactor = 0.05f;
    private float m_CautiousMaxAngle = 40f;
    private float m_CautiousAngularVelocityFactor = 30f;
    private float m_SteerSensitivity = 0.05f;
    private float m_AccelSensitivity = 1f;
    private float m_BrakeSensitivity = 1f;

    [SerializeField] private List<WheelScript> wheels;
    [SerializeField] private LapTime lapTime;

    private List<Transform> waypoints;
    private Transform targetWaypoint;
    private int waypointIndex = 0;
    private int waypointCount;

    private bool reversing = false;
    private Vector3 previousPosition;
    private float waitTime = 3.0f;
    private float timer = 0.0f;

    private bool overtaking;
    private double overtakeProbability = 1;
    private bool playerInFront;
    private float overtakingTorque = 650f;
    private float standardTorque;

    public bool driving = false;
    public Rigidbody AICarRigidbody;
    public Rigidbody PlayerCarRigidbody;
    private Stopwatch allAIWheelsOffTrackStopwatch;

    private void Start()
    {
        waypoints = GameObject.FindGameObjectsWithTag("Waypoint").Select(waypoint => waypoint.transform).ToList();
        targetWaypoint = waypoints[waypointIndex];
        waypointCount = waypoints.Count();
        allAIWheelsOffTrackStopwatch = new Stopwatch();
        standardTorque = carLocomotion.maxMotorTorque;

        switch (RaceSettings.RaceDifficulty)
        {
            case RaceDifficulty.Easy: overtakeProbability = 0.2; break; // 20% chance of overtake for Easy
            case RaceDifficulty.Medium: overtakeProbability = 0.5; break; // 50% chance of overtake for Medium
            case RaceDifficulty.Hard: overtakeProbability = 0.8; break; // 80% chance of overtake for Hard
            default:
                break;
        }
    }
    IEnumerator CommenceReverse()
    {
        // Drive backwards for 2 seconds then resume aiming for waypoints
        carLocomotion.Drive(0, -1);
        yield return new WaitForSeconds(1.75f);
        reversing = false;
        UnityEngine.Debug.Log("AI is free!");
    }

    IEnumerator CommenceOvertake()
    {
        overtaking = true;
        yield return new WaitForSeconds(3f);
        overtaking = false;
    }

    void Update()
    {
        if (!driving) // Car not driving so return ASAP
        {
            return;
        }

        if (!overtaking) 
        {
            Vector3 playerCarInversePoint = transform.InverseTransformPoint(PlayerCarRigidbody.position);

            float distanceToPlayer = Vector3.Distance(PlayerCarRigidbody.transform.position, AICarRigidbody.transform.position);
            float distanceToWaypoint = Vector3.Distance(AICarRigidbody.transform.position, targetWaypoint.position);

            if (playerCarInversePoint.z > 0)
            {
                playerInFront = true;
            }
            else
            {
                playerInFront = false;
            }

            // Determine if overtake should take place depending on distance to the player, if the player is in front and distance remaining to target waypoint
            if (distanceToPlayer < 8f && playerInFront && distanceToWaypoint > 15f)
            {
                System.Random rand = new System.Random();
                int number = rand.Next(0, 100);
                if (number <= overtakeProbability * 100) // Determine if random number falls under probability for overtake
                {
                    UnityEngine.Debug.Log($"AI attempting overtake!");
                    StartCoroutine(CommenceOvertake());
                }
            }
        }

        timer += Time.deltaTime;

        // Check if we have reached beyond 3 seconds without moving in x axis
        if (timer > waitTime)
        {
            if (Math.Round(previousPosition.x, 1) == Math.Round(AICarRigidbody.position.x, 1))
            {
                reversing = true;
                UnityEngine.Debug.Log("AI is stuck!");
                StartCoroutine(CommenceReverse());
            }
            previousPosition = AICarRigidbody.position;
            timer = timer - waitTime;
        }

        // Update to next waypoint
        if (Vector3.Distance(AICarRigidbody.position, targetWaypoint.position) < 5f)
        {
            if (waypointIndex != waypointCount - 1)
            {
                waypointIndex += 1;
                targetWaypoint = waypoints[waypointIndex];
            }
            else
            {
                waypointIndex = 0;
                targetWaypoint = waypoints[waypointIndex];
            }
        }

        if (!wheels.Any()) // No wheels assigned so return
        {
            return;
        }

        TimeSpan timeSpanOffTrack = allAIWheelsOffTrackStopwatch.Elapsed;
        // Check if all four wheels have left the track
        if (wheels[0].WheelColliderLeftTrack && wheels[1].WheelColliderLeftTrack && wheels[2].WheelColliderLeftTrack && wheels[3].WheelColliderLeftTrack)
        {
            if (!allAIWheelsOffTrackStopwatch.IsRunning)
            {
                allAIWheelsOffTrackStopwatch.Start();
            }
        }
        else
        {
            if (allAIWheelsOffTrackStopwatch.IsRunning)
            {
                allAIWheelsOffTrackStopwatch.Stop();

                allAIWheelsOffTrackStopwatch.Reset();
                if (timeSpanOffTrack.TotalSeconds > 1)
                {
                    lapTime.CalculatePenalty(timeSpanOffTrack);
                }
            }
        }
    }

    private void FixedUpdate()
    {
        if (!driving)
        {
            return;
        }

        Vector3 offsetTargetPos = targetWaypoint.position;
        if (overtaking)
        {
            offsetTargetPos += targetWaypoint.right * 2;
            carLocomotion.maxMotorTorque = overtakingTorque;
        }
        else
        {
            carLocomotion.maxMotorTorque = standardTorque;
        }

        if (!reversing)
        {
            //NOTE: The following waypoint pathfinding code is adapted from the Unity standard assets

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

            float accelBrakeSensitivity = (desiredSpeed < carLocomotion.currentSpeed)
                                              ? m_BrakeSensitivity
                                              : m_AccelSensitivity;

            // decide the actual amount of accel/brake input to achieve desired speed.
            float accel = Mathf.Clamp(desiredSpeed * accelBrakeSensitivity, -1, 1);

            // calculate the local-relative position of the target, to steer towards
            Vector3 localTarget = transform.InverseTransformPoint(offsetTargetPos);

            // work out the local angle towards the target
            float targetAngle = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;

            // get the amount of steering needed to aim the car towards the target
            float steer = Mathf.Clamp(targetAngle * m_SteerSensitivity, -1, 1) * Mathf.Sign(carLocomotion.currentSpeed);

            //END of pathfinding code from Unity standard assets

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

            carLocomotion.Drive(steer, accel);
        }
    }
}