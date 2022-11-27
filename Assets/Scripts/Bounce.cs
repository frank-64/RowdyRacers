using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Bounce : MonoBehaviour
{
    [SerializeField] private GameObject menuUI;
    [SerializeField] private GameObject deathUI;
    [SerializeField] private GameObject wonUI;
    [SerializeField] private TextMeshPro levelName;
    [SerializeField] private Rigidbody bouncyBall;
    [SerializeField] private float accelerationDueToGravityOnEarth = 9.81f; // Assuming the Dungeon is on Earth
    [SerializeField] private float accelerationDueToGravityOnParctuis = 13.0f; // Fictional planet
    private Vector3 gravityVector;
    public bool isSpaceMode = false;
    public bool isDungeonMode = true;
    private bool levelStarted = false;


    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1;
        levelStarted = false;
        menuUI.SetActive(true);
        deathUI.SetActive(false);
        wonUI.SetActive(false);
        if (isDungeonMode)
        {
            levelName.text = "Level 1: Dungeon";
        }
        else
        {
            levelName.text = "Level 2: Space";
        }
    }

    void Update()
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
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!levelStarted)
            {
                levelStarted = true;
                StartLevel();
            }
            else
            {
                bouncyBall.AddForce(0, 5, 0, ForceMode.Impulse);
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
                gravityVector = GravityVector(accelerationDueToGravity: accelerationDueToGravityOnParctuis);
            }
            bouncyBall.AddForce(gravityVector);
        }
    }

    private void StartLevel()
    {
        Time.timeScale = 1;
        menuUI.SetActive(false);
        deathUI.SetActive(false);
        wonUI.SetActive(false);
        bouncyBall.AddForce(Vector3.right * 3f, ForceMode.Impulse);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Time.timeScale = 0;
        deathUI.SetActive(true);
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (isDungeonMode && collision.gameObject.CompareTag("Finish"))
        {
            levelStarted = false;
            SceneManager.LoadScene("SpaceMode", LoadSceneMode.Single);
        }
        else if (isSpaceMode && collision.gameObject.CompareTag("Finish"))
        {
            levelStarted = false;
            Time.timeScale = 0;
        }
    }

    public Vector3 GravityVector (float accelerationDueToGravity)
    {
        gravityVector = Vector3.down * accelerationDueToGravity;
        Debug.Log(gravityVector);
        return gravityVector;
    }

    

    //Vector3 OppositeVector(Vector3 collsionVelocity, Vector3 preCollisionVelocity)
    //{
    //    if (collsionVelocity.x == 0)
    //    {
    //        return new Vector3(-preCollisionVelocity.x, preCollisionVelocity.y, preCollisionVelocity.z);
    //    }else if (collsionVelocity.y == 0)
    //    {
    //        return new Vector3(preCollisionVelocity.x, -preCollisionVelocity.y, preCollisionVelocity.z);
    //    }else if (collsionVelocity.z == 0)
    //    {
    //        return new Vector3(preCollisionVelocity.x, preCollisionVelocity.y, -preCollisionVelocity.z);
    //    }
    //    return preCollisionVelocity;
    //}

    //void OnCollisionEnter()
    //{
    //    Vector3 oppositeVector = OppositeVector(BouncyBall.velocity, currentVelocity);
    //    Debug.Log(oppositeVector);
    //    BouncyBall.AddForce(oppositeVector * 0.8f, ForceMode.Impulse);
    //}
}
