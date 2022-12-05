using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts
{
    public class PlayerCarControl : MonoBehaviour
    {
        [SerializeField] private CarLocomotion carLocomotion;
        [SerializeField] private Rigidbody carRigidBody;
        [SerializeField] private List<WheelScript> wheels;
        [SerializeField] private LapTime lapTime;
        [SerializeField] private bool isRampMode;
        public bool driving = false;

        private Stopwatch allWheelsOffTrackStopwatch;

        private void Start()
        {
            allWheelsOffTrackStopwatch = new Stopwatch();
        }

        private void Update()
        {
            if (wheels.Any())
            {
                TimeSpan timeSpanOffTrack = allWheelsOffTrackStopwatch.Elapsed;
                // Check if all four wheels have left the track
                if (wheels[0].WheelColliderLeftTrack && wheels[1].WheelColliderLeftTrack && wheels[2].WheelColliderLeftTrack && wheels[3].WheelColliderLeftTrack)
                {
                    if (!allWheelsOffTrackStopwatch.IsRunning)
                    {
                        allWheelsOffTrackStopwatch.Start();
                    }
                    else if (allWheelsOffTrackStopwatch.IsRunning && timeSpanOffTrack.TotalSeconds > 1)
                    {
                        lapTime.penaltyOverlay.SetActive(true); // Red screen negative feedback loop for leaving track
                    }
                }
                else
                {
                    if (allWheelsOffTrackStopwatch.IsRunning)
                    {
                        lapTime.penaltyOverlay.SetActive(false);
                        allWheelsOffTrackStopwatch.Stop();

                        allWheelsOffTrackStopwatch.Reset();
                        if (timeSpanOffTrack.TotalSeconds > 1)
                        {
                            lapTime.CalculatePenalty(timeSpanOffTrack);
                        }
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
                if (isRampMode)
                {
                    carLocomotion.Drive(0, verticalInput);
                }
                else
                {
                    carLocomotion.Drive(horizontalInput, verticalInput);
                }
            }
        }
    }
}
