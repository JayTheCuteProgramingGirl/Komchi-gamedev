using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera_System : MonoBehaviour
{
    [Header("Important-References")]
    public GameObject PlayerPosition;
    public Camera Camera;
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

    [Header("Change-POV")]
    [SerializeField] private float minPOV = 60f; 
    [SerializeField] private float MaxPOV = 70f; 
    [SerializeField] private float LerpStrenght; 

    void Start()
    {
    }

    void Update()
    {
        FollowPlayer();
        NavigateCamera();
        GetData();
        ChangePOV();
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
                Vector3 newPosition = new Vector3(Camera.transform.localPosition.x + mouseX, Camera.transform.localPosition.y + mouseY, Camera.transform.localPosition.z);

                newPosition.x = Mathf.Clamp(newPosition.x, Camera.transform.position.x + minX, Camera.transform.position.x + maxX);
                newPosition.y = Mathf.Clamp(newPosition.y, Camera.transform.position.y + minY, Camera.transform.position.y + maxY);

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

    private void ChangePOV()
    {
        if(PlayerMovement.iscurrentlyDashing == true)
        {
            Camera.main.fieldOfView = Mathf.Lerp(minPOV, MaxPOV, LerpStrenght); //Langsam die Kamera sicht vergösßern 
        }
        else 
        {
            Camera.main.fieldOfView = Mathf.Lerp(MaxPOV, minPOV, LerpStrenght); //Kamerea sicht verleiner 
        }
    }
}
