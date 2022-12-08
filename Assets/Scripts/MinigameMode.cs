using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MinigameMode : MonoBehaviour
{
    [SerializeField] private GameObject menuUI;
    [SerializeField] private GameObject deathUI;
    [SerializeField] private GameObject wonUI;
    [SerializeField] private TextMeshPro gravityText;
    [SerializeField] private TextMeshPro message;
    [SerializeField] private Rigidbody ball;
    [SerializeField] private Rigidbody asteroid;
    [SerializeField] private GameObject UFOInstantiator;
    private float accelerationDueToGravityOnEarth = 9.81f; // Assuming the Dungeon is on Earth
    private float accelerationDueToGravityOnCeres = 13.0f;
    private Vector3 gravityVector;
    private bool levelStarted = false;
    private bool gameOver = false;

    public bool isSpaceMode = false;
    public bool isDungeonMode = true;


    // Start is called before the first frame update
    void Start()
    {
        levelStarted = false;
        menuUI.SetActive(true);
        deathUI.SetActive(false);
        wonUI.SetActive(false);
        string gravityValue = isDungeonMode ? accelerationDueToGravityOnEarth.ToString() : accelerationDueToGravityOnCeres.ToString();
        gravityText.text = $"g = -{gravityValue}m/s/s";
    }

    void Update()
    {
        if (gameOver)
        {
            if (Input.GetKeyDown(KeyCode.Escape)) // Level not started the user can exit to main menu
            {
                SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Escape) && !levelStarted) // Level not started the user can exit to main menu
            {
                SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
            }
            else if (Input.GetKeyDown(KeyCode.R)) // Restart level
            {
                if (isDungeonMode)
                {
                    SceneManager.LoadScene("DungeonMode", LoadSceneMode.Single);
                }
                else
                {
                    SceneManager.LoadScene("SpaceMode", LoadSceneMode.Single);
                }
            }
            else if (Input.GetKeyDown(KeyCode.Space) && (isDungeonMode || isSpaceMode))
            {
                if (!levelStarted)
                {
                    levelStarted = true;
                    StartLevel();
                }
                else
                {
                    ball.AddForce(0, 5, 0, ForceMode.Impulse);
                }
            }
            else if (!levelStarted && !isDungeonMode && !isSpaceMode && Input.GetKeyDown(KeyCode.Return))
            {
                SceneManager.LoadScene("SpaceMode", LoadSceneMode.Single);
            }
        }
    }

    private void FixedUpdate()
    {
        if (levelStarted)
        {
            // Different gravity amounts for each level
            if (isDungeonMode)
            {
                gravityVector = GravityVector(accelerationDueToGravity: accelerationDueToGravityOnEarth);
            }
            else
            {
                asteroid.useGravity = true;
                gravityVector = GravityVector(accelerationDueToGravity: accelerationDueToGravityOnCeres);
            }
            ball.AddForce(gravityVector);
        }
    }

    private void StartLevel()
    {
        Time.timeScale = 1;
        menuUI.SetActive(false);
        deathUI.SetActive(false);
        wonUI.SetActive(false);
        message.gameObject.SetActive(false);
        ball.AddForce(Vector3.right * 3f, ForceMode.Impulse);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Time.timeScale = 0;
        deathUI.SetActive(true);
        if (collision.gameObject.tag == "UFO")
        {
            message.gameObject.SetActive(true);
            message.text = "You died to a UFO!";
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (isDungeonMode && collision.gameObject.CompareTag("Finish"))
        {
            Time.timeScale = 0;
            levelStarted = false;
            isDungeonMode = false;
            wonUI.SetActive(true);

        }
        else if (isSpaceMode && collision.gameObject.CompareTag("Finish"))
        {
            gameOver = true;
            wonUI.SetActive(true);
            message.text = "You avoided the alien fleet!";
            Time.timeScale = 0;
        }
        else if (isSpaceMode && collision.gameObject.CompareTag("Ceiling"))
        {
            Time.timeScale = 0;
            deathUI.SetActive(true);
            message.gameObject.SetActive(true);
            message.text = "You flew off into space!";
        }
        else if (isSpaceMode && collision.gameObject.CompareTag("UFO Collider"))
        {
            UFOInstantiator.SetActive(true);
            StartCoroutine(AlienMessage());
        }
    }

    IEnumerator AlienMessage()
    {
        message.gameObject.SetActive(true);
        message.text = "Alien attack!";
        yield return new WaitForSeconds(1f);
        message.gameObject.SetActive(false);
    }

    public Vector3 GravityVector (float accelerationDueToGravity)
    {
        gravityVector = Vector3.down * accelerationDueToGravity;
        return gravityVector;
    }
}
