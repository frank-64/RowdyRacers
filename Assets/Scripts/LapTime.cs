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

public class CarInfo
{
    public int lap = 0;
    public DateTime startTime;
    public DateTime finishTime;
    private List<LapTimeObject> lapTimes;

    public CarInfo()
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
    // TODO: Laps will need to be a global variable (eventually) and will be very important in the state of the game
    [SerializeField] private int laps;
    [SerializeField] private GameObject car;
    [SerializeField] private TextMeshPro lapTimeText;
    [SerializeField] private TextMeshPro raceTimeText;
    private CarInfo carInfo;
    private string previousLapTimesText = "";
    public bool isAI;
    private Stopwatch s;
    private Stopwatch raceStopwatch;

    // Start is called before the first frame update
    void Start()
    {
        carInfo = new CarInfo();
        s = new Stopwatch();
        raceStopwatch = new Stopwatch();
        raceStopwatch.Start();
    }

    // Update is called once per frame
    void Update()
    {
        raceTimeText.text = raceStopwatch.Elapsed.StripMilliseconds().ToString();
        if (carInfo.lap != 0 && !isAI)
        {
            lapTimeText.text = previousLapTimesText;
            lapTimeText.text = lapTimeText.text + "Lap " + carInfo.lap + ": " + s.Elapsed.StripMilliseconds();
            
        }
        if (carInfo.lap == -1)
        {
            // Car finished race
            lapTimeText.text = previousLapTimesText;
        }
    }

    private void OnTriggerEnter(Collider collisionObject)
    {
        if (isAI )//&& completeLap)
        {
            lapTimeText.text = previousLapTimesText;
            lapTimeText.text = lapTimeText.text + "Lap " + carInfo.lap + ": " + s.Elapsed.StripMilliseconds();
        }
        
        // TODO: Determine car has passed all waypoints before FINISHING
        if (collisionObject.gameObject.CompareTag("Finish"))
        {
            ToggleStopWatch();
        }
    }

    public void ToggleStopWatch()
    {
        s.Start();
        if (carInfo.lap == 0)
        {
            carInfo.startTime = DateTime.Now;
            // TODO: Determine if lap should be increased based on waypoints passed
            carInfo.lap++;
            Debug.Log("The car " + car.tag + " has started lap " + carInfo.lap);
        }
        else if (carInfo.lap < laps)
        {
            s.Stop();
            TimeSpan lapTime = s.Elapsed;
            carInfo.SetLapTime(new LapTimeObject(lapTime, carInfo.lap));
            SetPreviousLapTimes();
            s.Reset();
            s.Start();
            carInfo.lap++;
            Debug.Log("The car " + car.tag + " has started lap " + carInfo.lap + ". The previous lap took " +
                      lapTime.StripMilliseconds());
        }
        else if (carInfo.lap == laps)
        {
            DateTime finishTime = DateTime.Now;
            s.Stop();
            TimeSpan lapTime = s.Elapsed;
            carInfo.SetLapTime(new LapTimeObject(lapTime, carInfo.lap));
            // Indicate race has finished
            carInfo.lap = -1;
            SetPreviousLapTimes();
            carInfo.finishTime = finishTime;
            TimeSpan t = finishTime.Subtract(carInfo.startTime);
            Debug.Log("The car " + car.tag + " crossed the finish line in time: " + t + " with the lap taking: " +
                      lapTime.StripMilliseconds());
        }
    }

    public void SetPreviousLapTimes()
    {
        string lapTimesText = "";
        foreach (LapTimeObject lapTimeObject in carInfo.GetLapTimes())
        {
            lapTimesText = lapTimesText + "Lap " + lapTimeObject.lap + ": " + lapTimeObject.time.StripMilliseconds()+"\n";
        }

        previousLapTimesText = lapTimesText;
    }
}
