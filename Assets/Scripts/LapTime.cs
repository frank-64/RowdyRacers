using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
    public bool hasCompletedRace = false;
    private bool crossedStartLine = false;

    [SerializeField] private GameObject car;
    [SerializeField] private TextMeshPro lapTimeText;

    private LapInfo lapInfo;
    private string previousLapTimesText = "";
    private Stopwatch carStopwatch;

    // Start is called before the first frame update
    public void Start()
    {
        lapInfo = new LapInfo();
        carStopwatch = new Stopwatch();
    }

    // Update is called once per frame
    void Update()
    {
        //if (car.CompareTag("PlayerCar"))
        //{
        //    Debug.Log(carStopwatch.Elapsed.ToString());
        //}
    }

    public void StartStopwatch()
    {
        carStopwatch.Start();
        lapInfo.startTime = DateTime.Now;
        Debug.Log("The car " + car.tag + " has started lap " + lapInfo.lap);
    }

    private void OnTriggerEnter(Collider collisionObject)
    {
        if (hasCompletedRace) // Return as fast as possible and do nothing if race completed
        {
            return;
        }

        if (!crossedStartLine) // Don't do anything before car crosses start line
        {
            crossedStartLine = true;
            return;
        }

        else if(collisionObject.gameObject.CompareTag("Finish") && true) // TODO: Determine car has passed all waypoints before crossing the finish line
        {
            carStopwatch.Stop();
            TimeSpan lapTime = carStopwatch.Elapsed;

            if (lapInfo.lap == rampRacersGame.laps) //  -- RACE COMPLETE -- 
            {
                Debug.Log("Race completed!");
                hasCompletedRace = true;
                rampRacersGame.RaceCompleted(car);

                DateTime finishTime = DateTime.Now;
                lapInfo.finishTime = finishTime;
                lapInfo.raceTimeSpan = finishTime.Subtract(lapInfo.startTime);

                Debug.Log("The car " + car.tag + " crossed the finish line in time: " + lapInfo.raceTimeSpan + " with the lap taking: " +
                          lapTime.StripMilliseconds());
            }
            else
            {
                // Only reset and start stopwatch if race is not complete
                carStopwatch.Reset();
                carStopwatch.Start();
            }

            // Update laptime and finialise finish
            RecordLapTime(lapTime);
            SetLapTimesList();
            lapTimeText.text = previousLapTimesText;
        }
    }
    

    public void RecordLapTime(TimeSpan lapTime)
    {
        lapInfo.SetLapTime(new LapTimeObject(lapTime, lapInfo.lap));
        if (lapInfo.lap < rampRacersGame.laps)
        {
            Debug.Log("The car " + car.tag + " has started lap " + lapInfo.lap + ". The previous lap took " +
            lapTime.StripMilliseconds());

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
}
