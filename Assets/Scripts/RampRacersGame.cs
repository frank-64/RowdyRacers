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
        private bool hasCarFinishedRace = false;

        private Stopwatch raceStopwatch;

        // get countdown text and stopwatch text
        public TextMeshPro countdownText;
        public TextMeshPro raceStopwatchText;
        public TextMeshPro finishedRaceText;
        public TextMeshPro totalPenaltyText;
        public TextMeshPro raceDecisionText;
        public TextMeshPro difficultyText;
        public TextMeshPro lapsText;
        public TextMeshPro restartRaceText;
        public TextMeshPro mainMenuText;
        public GameObject RaceInitiatedUIObject;
        public GameObject StartRaceUIObject;
        public GameObject FinishedRaceUIObject;


        // get car LapTimes involved in race
        [SerializeField] private LapTime playerLapTime;
        [SerializeField] private LapTime AILapTime;

        [SerializeField] private PlayerCarControl playerCarControl;
        [SerializeField] private AITrackPathfinding AIPathfinding;

        // Use this for initialization
        void Start()
        {
            //Initialise objects
            raceStopwatch = new Stopwatch();
            difficultyText.text = $"Difficulty: {RaceSettings.RaceDifficulty}";
            lapsText.text = $"Laps: {RaceSettings.Laps}";
        }

        // Update is called once per frame
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
            //Initiate countdown - cars can't move until countdown over
            countdownText.gameObject.SetActive(true);
            StartRaceUIObject.SetActive(false);
            restartRaceText.gameObject.SetActive(false);
            mainMenuText.gameObject.SetActive(false);
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
                if (hasCarFinishedRace) // AI car has already finished the race, hence player lost race
                {
                    FinishedRaceUI(!hasCarFinishedRace);
                }
                else
                {
                    FinishedRaceUI(hasCarFinishedRace);
                }
            }
            else
            {
                hasCarFinishedRace = true;
            }
        }

        public void FinishedRaceUI(bool wonRace)
        {
            raceStopwatch.Stop();
            raceStopwatchText.color = Color.green;
            FinishedRaceUIObject.SetActive(true);
            restartRaceText.gameObject.SetActive(true);
            mainMenuText.gameObject.SetActive(true);
            if (playerLapTime.totalPenaltySeconds > 0) // Display penalties obtained during race
            {
                totalPenaltyText.gameObject.SetActive(true);
                totalPenaltyText.text = $"Total penalties: +{Math.Round(playerLapTime.totalPenaltySeconds)}s";
            }

            if (wonRace) // Display race decision
            {
                totalPenaltyText.color = Color.green;
                totalPenaltyText.text = "YOU WON!";
            }
            else
            {
                totalPenaltyText.text = "YOU LOST!";
                totalPenaltyText.color = Color.red;
            }
        }
    }
}