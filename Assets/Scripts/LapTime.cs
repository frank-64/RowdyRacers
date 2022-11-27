using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using Debug = UnityEngine.Debug;

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
    private string previousLapTimesText = "";

    private bool crossedStartLine = false;
    private LapInfo lapInfo;
    private Stopwatch carStopwatch;
    private int totalLaps = 1;

    [HideInInspector] public List<Transform> waypoints;
    [HideInInspector] public Transform targetWaypoint;
    [HideInInspector] public int waypointIndex = 0;
    private int waypointCount;
    private bool waypointThresholdMet = false;

    [HideInInspector] public bool hasCompletedRace = false;
    [HideInInspector] public double totalPenaltySeconds = 0;

    // Start is called before the first frame update
    public void Start()
    {
        lapInfo = new LapInfo();
        carStopwatch = new Stopwatch();
        totalLaps = RaceSettings.Laps;
        waypoints = GameObject.FindGameObjectsWithTag("Waypoint").Select(waypoint => waypoint.transform).ToList();
        targetWaypoint = waypoints[waypointIndex];
        waypointCount = waypoints.Count();

    }

    public void Update()
    {
        if (Vector3.Distance(rigidBody.position, targetWaypoint.position) < 5f)
        {
            if (waypointIndex != waypointCount-1)
            {
                waypointIndex += 1;
                targetWaypoint = waypoints[waypointIndex];
            }
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
        StartCoroutine(DisplayPenalty(penalty.ToString()));
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

        if(collisionObject.gameObject.CompareTag("Finish")) 
        {
            if (!crossedStartLine) // Don't do anything before car crosses start line
            {
                crossedStartLine = true;
                return;
            }

            carStopwatch.Stop();
            TimeSpan lapTime = carStopwatch.Elapsed;

            // Decide what percentage of waypoints must've been crossed to allow lap to count
            switch (RaceSettings.RaceDifficulty)
            {
                case RaceDifficulty.Easy: DetermineValidLap(Math.Round(waypointCount * 0.75f));
                    break;
                case RaceDifficulty.Medium: DetermineValidLap(Math.Round(waypointCount * 0.85f)); 
                    break;
                case RaceDifficulty.Hard: DetermineValidLap(Math.Round(waypointCount * 0.9f));
                    break;
                default:
                    break;
            }
            waypointIndex = 0;
            if (lapInfo.lap == totalLaps && waypointThresholdMet) //  -- RACE COMPLETE -- TODO: Car has passed all waypoints before crossing the finish line
            {
                waypointThresholdMet = false;
                Debug.Log("Race completed!");
                hasCompletedRace = true;
                rampRacersGame.RaceCompleted(rigidBody.gameObject);

                DateTime finishTime = DateTime.Now;
                lapInfo.finishTime = finishTime;
                lapInfo.raceTimeSpan = finishTime.Subtract(lapInfo.startTime);
                AddPenaltiesToTimeSpan();

                if (rigidBody.gameObject.tag == "PlayerCar")
                {
                    SetRaceTimeSpanText();
                }
                Debug.Log("The car " + rigidBody.gameObject.tag + " crossed the finish line in time: " + lapInfo.raceTimeSpan + " with the lap taking: " +
                          lapTime.StripMilliseconds());

                RecordLapTime(lapTime);
                SetLapTimesList();
                lapTimeText.text = previousLapTimesText;
            }
            else if (waypointThresholdMet)
            {
                Debug.Log("Valid lap!");
                // Only reset and start stopwatch if race is not complete
                carStopwatch.Reset();
                carStopwatch.Start();

                RecordLapTime(lapTime);
                SetLapTimesList();
                lapTimeText.text = previousLapTimesText;
            }
            else
            {
                Debug.Log("You did not pass enough checkpoints!");
                return;
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
        Debug.Log(totalTime);
        raceTimeSpanText.text = totalTime;
    }

    public void DetermineValidLap(double waypointThreshold)
    {
        if (waypointIndex >= waypointThreshold)
        {
            waypointThresholdMet = true;
        }
        else
        {
            waypointThresholdMet = false;
        }
    }
}
