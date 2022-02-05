using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static scr_Models;

public class scr_CharacterController : MonoBehaviour
{
    private CharacterController characterController;
    private DefaultInput defaultInput;
    [HideInInspector]
    public Vector2 input_Movement;
    [HideInInspector]
    public Vector2 input_View;

    private Vector3 newCameraRotation;
    private Vector3 newCharacterRotation;

    [Header("References")]
    public Transform cameraHolder;

    [Header("Settings")]
    public PlayerSettingsModel playerSettings;
    public float viewClampYMin = -70;
    public float viewClampYMax = 80;

    [Header("Gravity")]
    public float gravityAmount;
    public float gravityMin;
    public float playerGravity;

    public Vector3 jumpingForce;
    private Vector3 jumpingForceVelocity;

    [Header("Weapon")]
    public scr_WeaponController currentWeapon;

    public float weaponAnimationSpeed;

    [Header("Aiming In")]
    public bool isAimingIn;
    public bool isShooting;

    #region - Awake -
    private void Awake()
    {
        defaultInput = new DefaultInput();

        defaultInput.Character.Movement.performed += e => input_Movement = e.ReadValue<Vector2>();
        defaultInput.Character.View.performed += e => input_View = e.ReadValue<Vector2>();
        defaultInput.Character.Jump.performed += e => Jump();

        defaultInput.Weapon.Fire2Pressed.performed += e => AimingInPressed();
        defaultInput.Weapon.Fire2Released.performed += e => AimingInReleased();

        defaultInput.Weapon.Fire1Pressed.performed += e => ShootingPressed();
        defaultInput.Weapon.Fire1Released.performed += e => ShootingReleased();

        defaultInput.Enable();

        newCameraRotation = cameraHolder.localRotation.eulerAngles;
        newCharacterRotation = transform.localRotation.eulerAngles;

        characterController = GetComponent<CharacterController>();

        if (currentWeapon)
        {
            currentWeapon.Initialise(this);
        }
    }
    #endregion

    #region - Update - 
    private void Update()
    {
        CalculateView();
        CalculateMovement();
        CalculateJump();
        CalculateAimingIn();
        CalculateShooting();
    }
    #endregion


    #region - Shooting -
    private void ShootingPressed()
    {
        isShooting = true;
        //Debug.Log("ShootingPressed");
    }

    private void ShootingReleased()
    {
        isShooting = false;
        //Debug.Log("ShootingReleased");
    }

    private void CalculateShooting()
    {
        if (!currentWeapon)
        {
            return;
        }

        currentWeapon.isShooting = isShooting;
    }
    #endregion


    #region - Aiming In -

    private void AimingInPressed()
    {
        isAimingIn = true;
        //Debug.Log("AimingInPressed");
    } 

    private void AimingInReleased()
    {
        isAimingIn = false;
        //Debug.Log("AimingInReleased");
    }


    private void CalculateAimingIn()
    {
        if (!currentWeapon)
        {
            return;
        }

        currentWeapon.isAimingIn = isAimingIn;
    }

    #endregion

    private void CalculateView()
    {
        //(playerSettings.ViewXInverted ? -input_View.y : input_View.y): if ViewXInverted true, we use -input_View.x; if it false, use input_View.x
        newCharacterRotation.y += playerSettings.ViewXSensitivity * (playerSettings.ViewXInverted ? -input_View.x : input_View.x) * Time.deltaTime;
        transform.localRotation = Quaternion.Euler(newCharacterRotation);

        //if ViewYInverted true, we use input_View.y; if it false, use -input_View.y
        newCameraRotation.x += playerSettings.ViewYSensitivity * (playerSettings.ViewYInverted ? input_View.y : -input_View.y) * Time.deltaTime;
        newCameraRotation.x = Mathf.Clamp(newCameraRotation.x, viewClampYMin, viewClampYMax);

        cameraHolder.localRotation = Quaternion.Euler(newCameraRotation);
    }

    private void CalculateMovement()
    {
        var verticalSpeed = playerSettings.WalkingForwardSpeed * input_Movement.y * Time.deltaTime;
        var horizontalSpeed = playerSettings.WalkingStrafeSpeed * input_Movement.x * Time.deltaTime;

        var newMovementSpeed = new Vector3(horizontalSpeed, 0, verticalSpeed);

        newMovementSpeed = transform.TransformDirection(newMovementSpeed);

        //weaponAnimationSpeed = characterController.velocity.magnitude/(playerSettings.WalkingForwardSpeed * playerSettings.SpeedEffector);
        weaponAnimationSpeed = characterController.velocity.magnitude / (playerSettings.WalkingForwardSpeed);

        if (weaponAnimationSpeed > 1)
        {
            weaponAnimationSpeed = 1;
        }


        if(playerGravity > gravityMin)//Here the gravityAmount == acceleration, and the playerGravity == speed
        {
            playerGravity -= gravityAmount * Time.deltaTime;
        }
    
        if(playerGravity < -0.1f && characterController.isGrounded)
        {
            playerGravity = -0.1f;
        }


        newMovementSpeed.y += playerGravity;

        newMovementSpeed += jumpingForce * Time.deltaTime;

        characterController.Move(newMovementSpeed);
    }

    private void CalculateJump()
    {
        jumpingForce = Vector3.SmoothDamp(jumpingForce, Vector3.zero, ref jumpingForceVelocity, playerSettings.JumpingFalloff);
    }

    private void Jump()
    {
        if (!characterController.isGrounded)
        {
            return;
        }

        //Jump
        jumpingForce = Vector3.up * playerSettings.JumpingHeight;
        playerGravity = 0;
    }



}
