using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera_System : MonoBehaviour
{
    [Header("Important-References")]
    public GameObject PlayerPosition;
    public Vector3 offset;
    private float mouseX;
    private float mouseY;

    [Header("Camera-Settings")]
    [SerializeField] private float CameraSpeed = 0.125f;
    private Vector3 targetPosition;
    private bool allowFollowPlayer = true;
    private float timeSinceMovement = 0f;
    [SerializeField] private float movementCooldown = 0.5f;

    [Header("Camera-Border")]
    public float minX = -5f;
    public float maxX = 5f;
    public float minY = -5f;
    public float maxY = 5f;

    void Start()
    {
    }

    void Update()
    {
        FollowPlayer();
        NavigateCamera();
        GetData();
    }

    private void FollowPlayer()
    {
        targetPosition = PlayerPosition.transform.position + offset;
        if (allowFollowPlayer)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, CameraSpeed);
        }
    }

    private void NavigateCamera()
    {
        if (Input.GetMouseButton(1) && PlayerMovement.MovementDirection == 0 && PlayerMovement.IsGrounded == true)
        {
            timeSinceMovement += Time.deltaTime;

            if (timeSinceMovement >= movementCooldown)
            {
                allowFollowPlayer = false;
                Vector3 newPosition = new Vector3(transform.position.x + mouseX, transform.position.y + mouseY, transform.position.z);

                newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
                newPosition.y = Mathf.Clamp(newPosition.y, minY, maxY);

                transform.position = newPosition;
            }
        }
        else if (Input.GetMouseButtonUp(1))
        {
            allowFollowPlayer = true;
            timeSinceMovement = 0f;
        }
        else
        {
            timeSinceMovement = 0f;
        }
    }

    private void GetData()
    {
        mouseX = Input.GetAxis("Mouse X");
        mouseY = Input.GetAxis("Mouse Y");
    }
}
