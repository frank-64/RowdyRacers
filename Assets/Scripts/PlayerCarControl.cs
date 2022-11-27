using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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
                // Check if all four wheels have left the track
                if (wheels[0].WheelColliderLeftTrack && wheels[1].WheelColliderLeftTrack && wheels[2].WheelColliderLeftTrack && wheels[3].WheelColliderLeftTrack)
                {
                    allWheelsOffTrackStopwatch.Start();
                    // TODO: Do some more stuff here to indicate negative feedback loop
                    // Red screen, bumpy camera etc
                }
                else
                {
                    if (allWheelsOffTrackStopwatch.IsRunning)
                    {
                        allWheelsOffTrackStopwatch.Stop();
                        TimeSpan timeSpanOffTrack = allWheelsOffTrackStopwatch.Elapsed;
                        allWheelsOffTrackStopwatch.Reset();
                        //TODO: this penalty will depend on the difficulty of the race
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
                    //carRigidBody.velocity = new Vector3(initalVelocity.x, carRigidBody.velocity.y, carRigidBody.velocity.z);
                }
                else
                {
                    carLocomotion.Drive(horizontalInput, verticalInput);
                }
            }
        }
    }
}
