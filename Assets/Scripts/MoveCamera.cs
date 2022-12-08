using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    [SerializeField] private Rigidbody ball;
    [SerializeField] private Camera mainCamera;
    void Update()
    {
        transform.position = new Vector3(ball.position.x+2f, mainCamera.transform.position.y, mainCamera.transform.position.z);
    }
}
