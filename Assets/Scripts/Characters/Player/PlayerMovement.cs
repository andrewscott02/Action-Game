using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PlayerMovement : CharacterMovement
{
    [HideInInspector]
    public Animator animator;
    public GameObject skeleton;

    [HideInInspector]
    public PlayerController controller;

    Transform model; public void SetModel(Transform newModel) { model = newModel; }

    public float moveSpeed = 4;
    public float sprintSpeed = 8;
    public float lerpSpeed = 0.01f;

    Vector3 movement = Vector3.zero;

    CinemachineVirtualCamera vCam;
    Cinemachine3rdPersonFollow vCamFollow;
    float defaultFOV;
    public float moveFOVMultiplier = 2;

    protected override void Start()
    {
        base.Start();
        vCam = GameObject.FindObjectOfType<CinemachineVirtualCamera>();
        vCamFollow = vCam.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
        defaultFOV = vCam.m_Lens.FieldOfView;
    }

    /// <summary>
    /// Moves the player and adjusts the animation
    /// </summary>
    /// <param name="xSpeed"> Determines the horizontal movement (Left and Right) </param>
    /// <param name="ySpeed"> Determines the vertical movement (Forward and Backward) </param>
    public void Move(Vector2 moveInput)
    {
        float xRemap = HelperFunctions.Remap(moveInput.x, -1, 1, 0, 1);

        if (moveInput != Vector2.zero)
        {
            //Rotate towards direction
            Vector3 moveInput3D = new Vector3(moveInput.x, 0, moveInput.y);
            //Quaternion newRot = Quaternion.LookRotation(moveInput3D, Vector3.up) * Quaternion.Euler(0, controller.followTarget.transform.rotation.eulerAngles.y, 0);
            Quaternion newRot = Quaternion.LookRotation(moveInput3D, Vector3.up) * Quaternion.Euler(0, Camera.main.transform.rotation.eulerAngles.y, 0);
            targetRotation = newRot;
        }

        //Animate movement
        currentSpeed = moveInput.magnitude * (sprinting ? sprintSpeed : moveSpeed);
        currentSpeed = Mathf.Lerp(animator.GetFloat("RunBlend"), currentSpeed, lerpSpeed * Time.fixedDeltaTime);
        animator.SetFloat("RunBlend", currentSpeed);

        float newFOV = currentSpeed * moveFOVMultiplier;
        SetCameraValues(xRemap, defaultFOV + newFOV);
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        targetRotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotateSpeed * Time.fixedDeltaTime);
        transform.rotation = targetRotation;
    }

    public void Sprint(bool sprinting)
    {
        //Debug.Log("Sprinting: " + sprinting);
        this.sprinting = sprinting;
    }

    void SetCameraValues(float camSide, float desiredFOV)
    {
        vCamFollow.CameraSide = Mathf.Lerp(vCamFollow.CameraSide, camSide, Time.deltaTime);
        vCam.m_Lens.FieldOfView = Mathf.Lerp(vCam.m_Lens.FieldOfView, desiredFOV, Time.deltaTime);
    }
}