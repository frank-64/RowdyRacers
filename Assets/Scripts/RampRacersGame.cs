using System;
using System.Collections;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts
{
    public class RampRacersGame : MonoBehaviour
    {
        [HideInInspector] public bool hasRaceCommenced = false;
        private bool playerFinishedRace = false;
        private bool AIFinishedRace = false;

        private Stopwatch raceStopwatch;

        // UI texts
        public TextMeshPro countdownText;
        public TextMeshPro raceStopwatchText;
        public TextMeshPro finishedRaceText;
        public TextMeshPro totalPenaltyText;
        public TextMeshPro raceDecisionText;
        public TextMeshPro difficultyText;
        public TextMeshPro lapsText;
        public TextMeshPro messageText;
        public GameObject RaceInitiatedUIObject;
        public GameObject StartRaceUIObject;
        public GameObject FinishedRaceUIObject;
        public GameObject KeysUIObject;


        // car LapTime objects involved in race
        [SerializeField] private LapTime playerLapTime;
        [SerializeField] private LapTime AILapTime;

        // car control scripts for player and AI
        [SerializeField] private PlayerCarControl playerCarControl;
        [SerializeField] private AITrackPathfinding AIPathfinding;

        void Start()
        {
            Time.timeScale = 1;
            //Initialise stopwatch
            raceStopwatch = new Stopwatch();
            difficultyText.text = $"Difficulty: {RaceSettings.RaceDifficulty}";
            lapsText.text = $"Laps: {RaceSettings.Laps}";
        }

        void Update()
        {
            // Get user to hit the space bar before they can continue with the race
            if (Input.GetKeyDown(KeyCode.Space)) { 
                if (!hasRaceCommenced)
                {
                    hasRaceCommenced = true;
                    StartGame();
                }
            }else if (Input.GetKeyDown(KeyCode.Escape) && (FinishedRaceUIObject.activeInHierarchy || StartRaceUIObject.activeInHierarchy))
            {
                SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
            }
            else if (Input.GetKeyDown(KeyCode.R)) // Restart race
            {
                SceneManager.LoadScene("RaceMode", LoadSceneMode.Single);
            }   

            if (raceStopwatch.IsRunning)
            {
                raceStopwatchText.text = raceStopwatch.Elapsed.StripMilliseconds().ToString();
            }

            
            if (!(playerFinishedRace && AIFinishedRace)) // Return if neither have finished race
            {
                return;
            }

            DisplayDecision();
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
            countdownText.gameObject.SetActive(false);
        }

        void StartGame()
        {
            // Initiate countdown - cars can't move until countdown over
            // Hide UI 
            countdownText.gameObject.SetActive(true);
            StartRaceUIObject.SetActive(false);
            KeysUIObject.SetActive(false);
            totalPenaltyText.gameObject.SetActive(false);
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

            // Testing logs
            UnityEngine.Debug.Log($"Race laps: {RaceSettings.Laps}");
            UnityEngine.Debug.Log($"Race difficulty: {RaceSettings.RaceDifficulty}");

            // Cars can now move
            playerCarControl.driving = true;
            AIPathfinding.driving = true;
        }

        public void RaceCompleted(GameObject car)
        {
            if (car.name == "PlayerCar")
            {
                playerFinishedRace = true;
                playerCarControl.driving = false;
                FinishedRaceUI();
                StartCoroutine(StopCar(playerCarControl.carRigidBody));
            }
            else
            {
                AIPathfinding.driving = false;
                AIFinishedRace = true;
                StartCoroutine(StopCar(AIPathfinding.AICarRigidbody));
            }
        }

        IEnumerator StopCar(Rigidbody rigidbody)
        {
            yield return new WaitForSeconds(1f);
            rigidbody.velocity = Vector3.zero;
        }

        public void FinishedRaceUI()
        {
            raceStopwatch.Stop();
            raceStopwatchText.color = Color.green;

            FinishedRaceUIObject.SetActive(true);
            KeysUIObject.SetActive(true);
            if (playerLapTime.totalPenaltySeconds > 0) // Display penalties obtained during race
            {
                totalPenaltyText.gameObject.SetActive(true);
                totalPenaltyText.text = $"Total penalties: +{Math.Round(playerLapTime.totalPenaltySeconds)}s";
            }
        }

        public void DisplayDecision()
        {
            // Display race decision based on each car race time span (including penalties)
            if (playerLapTime.totalRaceTimeSpan < AILapTime.totalRaceTimeSpan)
            {
                raceDecisionText.color = Color.green;
                raceDecisionText.text = "YOU WON!";
            }
            else
            {
                raceDecisionText.text = "YOU LOST!";
                raceDecisionText.color = Color.red;
            }
        }

        public void InvalidLap()
        {
            RaceInitiatedUIObject.SetActive(false);
            KeysUIObject.SetActive(true);
            messageText.text = "You missed too many waypoints!";
        }
    }
}