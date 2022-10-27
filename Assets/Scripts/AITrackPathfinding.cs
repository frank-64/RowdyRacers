using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AITrackPathfinding : MonoBehaviour
{
    public List<AxleInfo> axleInfos;
    
    [SerializeField] private Rigidbody car;
    private float maxMotorTorque = 400f;
    private float maxSteerAngle = 40f;
    
    //The difference between the center of the car and the position where we steer
    public float centerSteerDifference;
    //The position where the car is steering
    private Vector3 steerPosition;
    //Average the steering angles to simulate the time it takes to turn the wheel
    float averageSteeringAngle = 0f;
    
    //To get a more realistic behavior
    public Vector3 centerOfMassChange;
    
    private List<Transform> waypoints;
    private Vector3 targetWaypoint;
    private Vector3 previousWaypoint;
    private int waypointIndex = 0;


    // Start is called before the first frame update
    void Start()
    {
        car.centerOfMass = car.centerOfMass + centerOfMassChange;
        waypoints = GameObject.FindGameObjectsWithTag("Waypoint").Select(waypoint => waypoint.transform).ToList();
        targetWaypoint = waypoints[waypointIndex].position;
        previousWaypoint = DeterminePreviousWaypoint();
    }

    public void Update()
    {
        //So we can experiment with the position where the car is checking if it should steer left/right
        //doesn't have to be where the wheels are - especially if we are reversing
        steerPosition = transform.position + transform.forward * centerSteerDifference;
        //Debug.Log("AI is heading towards waypoint:"+waypointIndex);

        //Check if we should change waypoint
        if (Vector3.Distance(car.position, targetWaypoint) < 2f)
        {
            waypointIndex += 1;

            if (waypointIndex == waypoints.Count)
            {
                waypointIndex = 0;
            }

            targetWaypoint = waypoints[waypointIndex].position;

            previousWaypoint = DeterminePreviousWaypoint();
        }
    }

    // Update is called once per frame
    public void FixedUpdate()
    {
        float motor = maxMotorTorque;
        
        float steeringAngle = maxSteerAngle * Math.SteerDirection(transform, steerPosition, targetWaypoint);

        //Average the steering angles to simulate the time it takes to turn the steering wheel
        float averageAmount = 30f;

        averageSteeringAngle = averageSteeringAngle + ((steeringAngle - averageSteeringAngle) / averageAmount);


        foreach (AxleInfo axleInfo in axleInfos)
        {
            if (axleInfo.steering)
            {
                axleInfo.leftWheel.steerAngle = averageSteeringAngle;
                axleInfo.rightWheel.steerAngle = averageSteeringAngle;
            }
            if (axleInfo.motor)
            {
                axleInfo.leftWheel.motorTorque = motor;
                axleInfo.rightWheel.motorTorque = motor;
            }
        }
    }
    

    Vector3 DeterminePreviousWaypoint()
    {
        previousWaypoint = Vector3.zero;

        if (waypointIndex - 1 < 0)
        {
            previousWaypoint = waypoints[waypoints.Count - 1].position;
        }
        else
        {
            previousWaypoint = waypoints[waypointIndex - 1].position;
        }

        return previousWaypoint;
    }
}
