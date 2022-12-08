using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class TimeExtensions
{
    public static TimeSpan StripMilliseconds(this TimeSpan time)
    {
        return new TimeSpan(time.Days, time.Hours, time.Minutes, time.Seconds);
    }
}

public class LapTimeObject
{
    public TimeSpan time;
    public int lap;

    public LapTimeObject(TimeSpan _time, int _lap)
    {
        time = _time;
        lap = _lap;
    }
}

public class LapInfo
{
    public int lap = 1;
    public DateTime startTime;
    public DateTime finishTime;
    public TimeSpan raceTimeSpan;
    private List<LapTimeObject> lapTimes;

    public LapInfo()
    {
        lapTimes = new List<LapTimeObject>();
    }
    public void SetLapTime(LapTimeObject lapTime)
    {
        lapTimes.Add(lapTime);
    }

    public List<LapTimeObject> GetLapTimes()
    {
        return lapTimes;
    }
}

public class LapTime : MonoBehaviour
{
    public RampRacersGame rampRacersGame;

    [SerializeField] private Rigidbody rigidBody;
    [SerializeField] private TextMeshPro lapTimeText;
    [SerializeField] private TextMeshPro penaltyText;
    [SerializeField] private TextMeshPro raceTimeSpanText;
    [SerializeField] private TextMeshPro waypointText;
    public GameObject penaltyOverlay;
    private string previousLapTimesText = "";

    private bool crossedStartLine = false;
    private LapInfo lapInfo;
    private Stopwatch carStopwatch;
    private int totalLaps = 1;
    private bool isPlayer = false;

    [HideInInspector] public List<GameObject> waypoints;
    private int waypointCount;
    private int waypointsCollected;
    private double waypointThreshold;
    private bool waypointThresholdMet = false;

    [HideInInspector] public bool hasCompletedRace = false;
    [HideInInspector] public double totalPenaltySeconds = 0;
    public TimeSpan totalRaceTimeSpan => lapInfo.raceTimeSpan;

    // Start is called before the first frame update
    public void Start()
    {
        lapInfo = new LapInfo();
        carStopwatch = new Stopwatch();
        totalLaps = RaceSettings.Laps;
        waypoints = GameObject.FindGameObjectsWithTag("Waypoint").ToList();
        waypointCount = waypoints.Count();
        if (rigidBody.CompareTag("PlayerCar"))
        {
            isPlayer = true;
        }

        switch (RaceSettings.RaceDifficulty)
        {
            case RaceDifficulty.Easy:
                waypointThreshold = Math.Round(waypointCount * 0.75f);
                break;
            case RaceDifficulty.Medium:
                waypointThreshold = Math.Round(waypointCount * 0.85f);
                break;
            case RaceDifficulty.Hard:
                waypointThreshold = Math.Round(waypointCount * 1f);
                break;
            default:
                break;
        }
    }

    public void StartStopwatch()
    {
        carStopwatch.Start();
        lapInfo.startTime = DateTime.Now;
    }

    public void CalculatePenalty(TimeSpan timeSpanOffTrack)
    {
        if (hasCompletedRace)
        {
            return;
        }
        var seconds = timeSpanOffTrack.TotalSeconds;
        switch (RaceSettings.RaceDifficulty)
        {
            case RaceDifficulty.Easy: AddPenalty(Math.Clamp(Math.Round(seconds), 1, 10)); break; // Min 1 second penalty
            case RaceDifficulty.Medium: AddPenalty(Math.Clamp(Math.Round(seconds) * 2, 2, 10)); break; // Min 2 second penalty and penalty * 2 time off track
            case RaceDifficulty.Hard: AddPenalty(Math.Clamp(Math.Round(seconds) * 2.5, 3, 10)); break; // Min 3 second penalty and penalty * 2.5 time off track
            default:
                break;
        }
    }

    public void AddPenalty(double penalty)
    {
        totalPenaltySeconds += penalty;
        if (isPlayer)
        {
            StartCoroutine(DisplayPenalty(penalty.ToString()));
        }
    }

    IEnumerator DisplayPenalty(string penalty)
    {
        penaltyText.text = $"You received a {penalty}s penalty\nStay on the track!";
        penaltyText.gameObject.SetActive(true);
        yield return new WaitForSeconds(3);
        penaltyText.gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider collisionObject)
    {
        if (hasCompletedRace) // Return as fast as possible and do nothing if race completed
        {
            return;
        }
        if (collisionObject.gameObject.CompareTag("Dungeon") && isPlayer) //  Start minigame
        {
            SceneManager.LoadScene("DungeonMode", LoadSceneMode.Single);
        }
        else if (collisionObject.gameObject.CompareTag("Waypoint"))
        {
            waypointsCollected++;
            if (!isPlayer) 
            {
                return;
            }

            // Display waypoint collected for player
            collisionObject.gameObject.GetComponent<MeshRenderer>().enabled = false;
            UpdateWaypointText(waypointsCollected, waypointThreshold);
        }
        else if (collisionObject.gameObject.CompareTag("Finish")) 
        {
            if (!crossedStartLine) // Don't do anything before car crosses start line
            {
                crossedStartLine = true;
                return;
            }

            carStopwatch.Stop();
            TimeSpan lapTime = carStopwatch.Elapsed;

            DetermineValidLap(waypointThreshold);

            if (lapInfo.lap == totalLaps && waypointThresholdMet) //  -- RACE COMPLETE --
            {
                UnityEngine.Debug.Log("Race won!");
                waypointThresholdMet = false;
                hasCompletedRace = true;
                rampRacersGame.RaceCompleted(rigidBody.gameObject);

                DateTime finishTime = DateTime.Now;
                lapInfo.finishTime = finishTime;
                lapInfo.raceTimeSpan = finishTime.Subtract(lapInfo.startTime);
                AddPenaltiesToTimeSpan();

                if (isPlayer)
                {
                    SetRaceTimeSpanText();
                }

                RecordLapTime(lapTime);
                SetLapTimesList();
                lapTimeText.text = previousLapTimesText;
            }
            else if (waypointThresholdMet)
            {
                // Only reset and start stopwatch if race is not complete
                carStopwatch.Reset();
                carStopwatch.Start();

                RecordLapTime(lapTime);
                SetLapTimesList();
                lapTimeText.text = previousLapTimesText;
                if (isPlayer)
                {
                    waypoints.ForEach(f => f.gameObject.GetComponent<MeshRenderer>().enabled = true);
                }
                waypointsCollected = 0;
            }
            else if (isPlayer && !waypointThresholdMet)
            {
                rampRacersGame.InvalidLap();
            }
        }
    }
    

    public void RecordLapTime(TimeSpan lapTime)
    {
        lapInfo.SetLapTime(new LapTimeObject(lapTime, lapInfo.lap));
        if (lapInfo.lap < totalLaps)
        {
            lapTime.StripMilliseconds();
            lapInfo.lap++;
        }
    }

    public void SetLapTimesList()
    {
        string lapTimesText = "";
        foreach (LapTimeObject lapTimeObject in lapInfo.GetLapTimes())
        {
            lapTimesText = lapTimesText + "Lap " + lapTimeObject.lap + ": " + lapTimeObject.time.StripMilliseconds() + "\n";
        }

        previousLapTimesText = lapTimesText;
        return;
    }

    public void AddPenaltiesToTimeSpan() { 
        lapInfo.raceTimeSpan += TimeSpan.FromSeconds(totalPenaltySeconds);
    }

    public void SetRaceTimeSpanText()
    {
        raceTimeSpanText.gameObject.SetActive(true);
        string totalTime = $"Total race time: {lapInfo.raceTimeSpan.StripMilliseconds()}";
        UnityEngine.Debug.Log(totalTime);
        raceTimeSpanText.text = totalTime;
    }

    public void DetermineValidLap(double waypointThreshold)
    {
        if (waypointsCollected >= waypointThreshold)
        {
            waypointThresholdMet = true;
        }
        else
        {
            waypointThresholdMet = false;
        }
    }

    public void UpdateWaypointText(int waypointsCollected, double waypointThreshold)
    {
        waypointText.text = $"{waypointsCollected}/{waypointThreshold} waypoints";
    }
}
