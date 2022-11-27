using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Cinemachine.CinemachineTriggerAction.ActionSettings;

public enum RaceDifficulty
{
    Easy,
    Medium,
    Hard
}

public static class RaceSettings
{
    public static int Laps { get; set; } = 1;
    public static RaceDifficulty RaceDifficulty { get; set; }
}

public class MenuSystem : MonoBehaviour
{
    [SerializeField] private GameObject MainMenuUI;
    [SerializeField] private GameObject HelpMenuUI;
    [SerializeField] private GameObject RaceMenuUI;

    [SerializeField] private TextMeshPro easyText;
    [SerializeField] private TextMeshPro mediumText;
    [SerializeField] private TextMeshPro hardText;
    [SerializeField] private TextMeshPro Laps1Text;
    [SerializeField] private TextMeshPro Laps3Text;
    [SerializeField] private TextMeshPro Laps5Text;

    enum GameMode
    {
        RaceMenu,
        RampMenu,
        MainMenu,
        HelpMenu
    }

    private GameMode gameMode = GameMode.MainMenu;

    // Start is called before the first frame update
    void Start()
    {
        MainMenuUI.SetActive(false);
        HelpMenuUI.SetActive(false);
        RaceMenuUI.SetActive(false);
        StartMainMenu();
    }

    // Update is called once per frame
    void Update()
    {
        switch (gameMode)
        {
            case GameMode.MainMenu: UpdateMainMenu(); break;
            case GameMode.HelpMenu: UpdateHelpMenu(); break;
            case GameMode.RaceMenu: UpdateRaceMenu(); break;
        }
    }

    void UpdateMainMenu()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            StartHelpMenu();
        }
        else if (Input.GetKeyDown(KeyCode.G))
        {
            StartRampMode();
        } 
        else if (Input.GetKeyDown(KeyCode.R))
        {
            StartRaceMenu();
        }
    }

    void UpdateHelpMenu()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            StartMainMenu();
        }
    }

    void UpdateRaceMenu()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartRaceMode();
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            StartMainMenu();
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            SetEasyDifficulty();
        }
        else if (Input.GetKeyDown(KeyCode.M))
        {
            SetMediumDifficulty();
        }
        else if (Input.GetKeyDown(KeyCode.H))
        {
            SetHardDifficulty();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
        {
            Lap1Selected();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
        {
            Lap3Selected();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Keypad5))
        {
            Lap5Selected();
        }
    }

    void StartMainMenu()
    {
        gameMode = GameMode.MainMenu;
        MainMenuUI.gameObject.SetActive(true);
        HelpMenuUI.gameObject.SetActive(false);
        RaceMenuUI.gameObject.SetActive(false);
    }

    void StartHelpMenu()
    {
        gameMode = GameMode.HelpMenu;
        MainMenuUI.gameObject.SetActive(false);
        HelpMenuUI.gameObject.SetActive(true);
        RaceMenuUI.gameObject.SetActive(false);
    }

    void StartRaceMenu()
    {
        gameMode = GameMode.RaceMenu;
        MainMenuUI.gameObject.SetActive(false);
        HelpMenuUI.gameObject.SetActive(false);
        RaceMenuUI.gameObject.SetActive(true);
    }

    void StartRaceMode()
    {
        SceneManager.LoadScene("RaceMode", LoadSceneMode.Single);
    }

    void StartRampMode()
    {

    }

    void SetEasyDifficulty()
    {
        RaceSettings.RaceDifficulty = RaceDifficulty.Easy;
        easyText.color = Color.red;
        mediumText.color = Color.white;
        hardText.color = Color.white;
    }

    void SetMediumDifficulty()
    {
        RaceSettings.RaceDifficulty = RaceDifficulty.Medium;
        easyText.color = Color.white;
        mediumText.color = Color.red;
        hardText.color = Color.white;
    }

    void SetHardDifficulty()
    {
        RaceSettings.RaceDifficulty = RaceDifficulty.Hard;
        easyText.color = Color.white;
        mediumText.color = Color.white;
        hardText.color = Color.red;
    }

    void Lap1Selected()
    {
        RaceSettings.Laps = 1;
        Laps1Text.color = Color.red;
        Laps3Text.color = Color.white;
        Laps5Text.color = Color.white;
    }

    void Lap3Selected()
    {
        RaceSettings.Laps = 3;
        Laps1Text.color = Color.white;
        Laps3Text.color = Color.red;
        Laps5Text.color = Color.white;
    }

    void Lap5Selected()
    {
        RaceSettings.Laps = 5;
        Laps1Text.color = Color.white;
        Laps3Text.color = Color.white;
        Laps5Text.color = Color.red;
    }
}
