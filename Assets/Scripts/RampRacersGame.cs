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
        private bool hasRaceCompleted = false;
        public int laps = 3;

        private Stopwatch raceStopwatch;

        // get countdown text and stopwatch text
        public TextMeshPro countdownText;
        public TextMeshPro raceStopwatchText;
        public TextMeshPro finishedRaceText;
        public GameObject RaceInitiatedUIObject;
        public GameObject StartRaceUIObject;


        // get car LapTimes involved in race
        [SerializeField] private LapTime playerLapTime;
        [SerializeField] private LapTime AILapTime;

        // get car locomotion
        [SerializeField] private PlayerCarControl playerCarControl;
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
            // Get user to hit the space bar before it can continue with the race starting to determine that the user is active
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
            for (int i = 4; i > 0; i--)
            {
                yield return new WaitForSeconds(1);
                countdownText.text = i.ToString();
            }
            yield return new WaitForSeconds(1);
            countdownText.text = "GO!";
            CountdownFinished();
            yield return new WaitForSeconds(1);
            countdownText.text = "";
        }

        void StartGame()
        {
            //Initiate countdown - cars can't move until countdown over
            countdownText.gameObject.SetActive(true);
            StartRaceUIObject.SetActive(false);
            StartCoroutine(Countdown());
        }

        void CountdownFinished()
        {
            // Starting all the stopwatches
            raceStopwatch.Start();
            playerLapTime.StartStopwatch();
            AILapTime.StartStopwatch();

            // Display lap times and stop watches
            RaceInitiatedUIObject.SetActive(true);

            // Cars can now move
            playerCarControl.driving = true;
            AIPathfinding.driving = true;
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