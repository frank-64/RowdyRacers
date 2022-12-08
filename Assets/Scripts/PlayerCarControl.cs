using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts
{
    public class PlayerCarControl : MonoBehaviour
    {
        [SerializeField] private CarLocomotion carLocomotion;
        [SerializeField] private List<WheelScript> wheels;
        [SerializeField] private LapTime lapTime;

        [HideInInspector] public bool driving = false;
        public Rigidbody carRigidBody;

        private Stopwatch allPlayerWheelsOffTrackStopwatch;

        private void Start()
        {
            allPlayerWheelsOffTrackStopwatch = new Stopwatch();
        }

        private void Update()
        {
            if (!wheels.Any()) // No wheels so return ASAP
            {
                return;
            }

            if (!driving) // Car not driving so return ASAP
            {
                return;
            }

            TimeSpan timeSpanOffTrack = allPlayerWheelsOffTrackStopwatch.Elapsed;
            // Check if all four wheels have left the track
            if (wheels[0].WheelColliderLeftTrack && wheels[1].WheelColliderLeftTrack && wheels[2].WheelColliderLeftTrack && wheels[3].WheelColliderLeftTrack)
            {
                if (!allPlayerWheelsOffTrackStopwatch.IsRunning)
                {
                    allPlayerWheelsOffTrackStopwatch.Start();
                }
                else if (allPlayerWheelsOffTrackStopwatch.IsRunning && timeSpanOffTrack.TotalSeconds > 1 && driving)
                {
                    lapTime.penaltyOverlay.SetActive(true); // Screen overlay negative feedback for leaving track
                }
            }
            else
            {
                if (allPlayerWheelsOffTrackStopwatch.IsRunning)
                {
                    lapTime.penaltyOverlay.SetActive(false);
                    allPlayerWheelsOffTrackStopwatch.Stop();

                    allPlayerWheelsOffTrackStopwatch.Reset();
                    if (timeSpanOffTrack.TotalSeconds > 1)
                    {
                        lapTime.CalculatePenalty(timeSpanOffTrack);
                    }
                }
            }
        }

        private void FixedUpdate()
        {
            if (driving)
            {
                float verticalInput = Input.GetAxis("Vertical");
                float horizontalInput = Input.GetAxis("Horizontal");
                carLocomotion.Drive(horizontalInput, verticalInput);
            }
        }
    }
}
