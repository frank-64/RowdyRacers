using System;
using System.Collections;
using System.Diagnostics;
using TMPro;
using UnityEngine;

namespace Assets.Scripts
{
    public class RampRacersGame : MonoBehaviour
    {
        public bool hasRaceCommenced = false;
        private bool hasRaceCompleted = true;
        public int laps = 3;

        private Stopwatch raceStopwatch;

        // get countdown text and stopwatch text
        public TextMeshPro countdownText;
        public TextMeshPro raceStopwatchText;
        public TextMeshPro finishedRaceText;

        // get car LapTimes involved in race
        [SerializeField] private LapTime playerLapTime;
        [SerializeField] private LapTime AILapTime;

        // get car locomotion
        [SerializeField] private CarLocomotion playerCarLocomotion;
        [SerializeField] private AITrackPathfinding AIPathfinding;


        // Use this for initialization
        void Start()
        {
            //Initialise objects
            raceStopwatch = new Stopwatch();
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space)) { 
                if (!hasRaceCommenced)
                {
                    hasRaceCommenced = true;
                    StartGame();
                }
            }   

            if (raceStopwatch.IsRunning)
            {
                raceStopwatchText.text = raceStopwatch.Elapsed.StripMilliseconds().ToString();
            }
        }

        IEnumerator Countdown()
        {
            for (int i = 4; i > 1; i--)
            {
                yield return new WaitForSeconds(1);
                countdownText.text = i.ToString();
            }
            yield return new WaitForSeconds(1);
            countdownText.text = "GO!";
        }

        void StartGame()
        {
            // Get user to hit the space bar before it can continue with the race starting to determine that the user is active

            //Initiate countdown - cars can't move until countdown over

            // Do countdown
            //
            //
            countdownText.gameObject.SetActive(true);
            StartCoroutine(Countdown());

            // Starting all the stopwatches
            raceStopwatch.Start();
            playerLapTime.StartStopwatch();
            AILapTime.StartStopwatch();

            // Cars can now move
            playerCarLocomotion.axleInfos[0].motor = true;
            AIPathfinding.axleInfos[0].motor = true;
        }

        public void RaceCompleted(GameObject car)
        {
            hasRaceCompleted = true;
            raceStopwatch.Stop();
            raceStopwatchText.color = Color.green;
            finishedRaceText.gameObject.SetActive(true);

            // Wait 5 seconds but capture boolean values of completed cars
            // determine if object 'car' is winner
            // set text color green and change text to 'Winner!'

        }
    }
}