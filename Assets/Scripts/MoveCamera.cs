using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    [SerializeField] private Rigidbody BouncyBall;
    [SerializeField] private Camera camera;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(BouncyBall.position.x+2f, camera.transform.position.y,camera.transform.position.z);
    }
}
