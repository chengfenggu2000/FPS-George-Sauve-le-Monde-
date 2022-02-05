using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static scr_Models;

public class scr_WeaponController : MonoBehaviour
{
    private scr_CharacterController characterController;

    [Header("References")]
    public Animator weaponAnimator;


    [Header("Settings")]
    public WeaponSettingsModel settings;

    bool isInitialised;

    Vector3 newWeaponRotation;
    Vector3 newWeaponRotationVelocity;

    Vector3 targetWeaponRotation;
    Vector3 targetWeaponRotationVelocity;

    Vector3 newWeaponMovementRotation;
    Vector3 newWeaponMovementRotationVelocity;

    Vector3 targetWeaponMovementRotation;
    Vector3 targetWeaponMovementRotationVelocity;

    [Header("Weapon Breathing")]
    public Transform weaponSwayObject;

    public float swayAmountA = 1;
    public float swayAmountB = 2;
    public float swayScale = 100;
    public float swayLerpSpeed = 14;

    public float swayTime;
    public Vector3 swayPosition;

    [Header("AimingIn")]
    public Transform sightTarget;
    public float sightOffset;
    public float aimingInTime;
    public Vector3 weaponSwayPosition;
    public Vector3 weaponSwayPositionVelocity;
    //[HideInInspector]
    public bool isAimingIn;

    [Header("Weapon Shooting")]
    public GameObject Bullet;
    public Transform FirePoint;
    public float t;
    //[HideInInspector]
    public bool isShooting;

    [Header("RayShooting")]
    public int gunDamage = 1;

    public float fireRate = .25f;

    public float weaponRange = 50f;

    public float hitForce = 100f;

    public Transform gunEnd;

    public Camera fpsCam;

    private WaitForSeconds shotDuration = new WaitForSeconds(.07f);

    private AudioSource gunAudio;

    private LineRenderer laserLine;

    private float nextFire;


    public void Start()
    {
        newWeaponRotation = transform.localRotation.eulerAngles;

        laserLine = GetComponent<LineRenderer>();

        gunAudio = GetComponent<AudioSource>();

        //fpsCam = GetComponentInParent<Camera>();
    }

    public void Initialise(scr_CharacterController CharacterController)
    {
        characterController = CharacterController;
        isInitialised = true;
    }

    private void Update()
    {
        if (!isInitialised)
        {
            return;
        }


        CalculateWeaponRotation();
        SetWeaponAnimations();
        CalculateWeaponSway();
        CalculateAimingIn();
        //CalculateShooting();
        RayShooting();

    }

    private void CalculateShooting()
    {
        if (isShooting)
        {
            t += Time.deltaTime;
            if (t >= 0.1)
            {
                Debug.Log("t > 0.2");
                Instantiate(Bullet, FirePoint.position, FirePoint.rotation);
                //Debug.Log("isShooting");
                t = 0;
            }

        }

    }

    private void CalculateAimingIn()
    {
        var targetPosition = transform.position;

        if (isAimingIn)
        {
            targetPosition = characterController.cameraHolder.transform.position + (weaponSwayObject.transform.position - sightTarget.position) + (characterController.cameraHolder.transform.forward * sightOffset);
            //Debug.Log("isAiming");
        }

        weaponSwayPosition = weaponSwayObject.transform.position;
        weaponSwayPosition = Vector3.SmoothDamp(weaponSwayPosition, targetPosition, ref weaponSwayPositionVelocity, aimingInTime);
        weaponSwayObject.transform.position = weaponSwayPosition;
    }

    private void CalculateWeaponRotation()
    {
        targetWeaponRotation.y += settings.SwayAmount * (settings.SwayXInverted ? -characterController.input_View.x : characterController.input_View.x) * Time.deltaTime;
        targetWeaponRotation.x += settings.SwayAmount * (settings.SwayYInverted ? characterController.input_View.y : -characterController.input_View.y) * Time.deltaTime;

        targetWeaponRotation.x = Mathf.Clamp(targetWeaponRotation.x, -settings.SwayClampX, settings.SwayClampX);
        targetWeaponRotation.y = Mathf.Clamp(targetWeaponRotation.y, -settings.SwayClampY, settings.SwayClampY);
        targetWeaponRotation.z = targetWeaponRotation.y;

        targetWeaponRotation = Vector3.SmoothDamp(targetWeaponRotation, Vector3.zero, ref targetWeaponRotationVelocity, settings.SwayResetSmoothing);
        newWeaponRotation = Vector3.SmoothDamp(newWeaponRotation, targetWeaponRotation, ref newWeaponRotationVelocity, settings.SwaySmoothing);

        targetWeaponMovementRotation.z = settings.MovementSwayX * (settings.MovementSwayXInverted ? -characterController.input_Movement.x : characterController.input_Movement.x);
        targetWeaponMovementRotation.x = settings.MovementSwayY * (settings.MovementSwayYInverted ? -characterController.input_Movement.y : characterController.input_Movement.y);

        targetWeaponMovementRotation = Vector3.SmoothDamp(targetWeaponMovementRotation, Vector3.zero, ref targetWeaponMovementRotationVelocity, settings.MovementSwaySmoothing);
        newWeaponMovementRotation = Vector3.SmoothDamp(newWeaponMovementRotation, targetWeaponMovementRotation, ref newWeaponMovementRotationVelocity, settings.MovementSwaySmoothing);

        transform.localRotation = Quaternion.Euler(newWeaponRotation + newWeaponMovementRotation);
    }

    private void SetWeaponAnimations()
    {
        weaponAnimator.speed = characterController.weaponAnimationSpeed;
    }

    private void CalculateWeaponSway()
    {
        var targetPosition = LissajousCurve(swayTime, swayAmountA, swayAmountB) / swayScale;

        swayPosition = Vector3.Lerp(swayPosition, targetPosition, Time.smoothDeltaTime * swayLerpSpeed);
        swayTime += Time.deltaTime;

        if(swayTime > 6.3f)
        {
            swayTime = 0;
        }

        if (!isAimingIn && !isShooting)
        {
            weaponSwayObject.localPosition = swayPosition;
        }
        

    }

    private Vector3 LissajousCurve(float Time, float A, float B)
    {
        return new Vector3(Mathf.Sin(Time), A * Mathf.Sin(B * Time + Mathf.PI));
    }

    private void RayShooting()
    {
        if (isShooting && Time.time > nextFire)
        {
            nextFire = Time.time + fireRate;

            StartCoroutine(ShotEffect());

            Vector3 rayOrigin = fpsCam.ViewportToWorldPoint(new Vector3(.5f, .5f, .0f));

            RaycastHit hit;

            laserLine.SetPosition(0, gunEnd.position);

            if (Physics.Raycast(rayOrigin, fpsCam.transform.forward, out hit, weaponRange))
            {
                laserLine.SetPosition(1, hit.point);
                ShootableBox health = hit.collider.GetComponent<ShootableBox>();

                if (health != null)
                {
                    health.Damage(gunDamage);
                }

                if (hit.rigidbody != null)
                {
                    hit.rigidbody.AddForce(-hit.normal * hitForce);
                }
            }
            else
            {
                laserLine.SetPosition(1, fpsCam.transform.forward * weaponRange);
            }
        }
    }
    private IEnumerator ShotEffect()
    {
        gunAudio.Play();
        laserLine.enabled = true;

        yield return shotDuration;

        laserLine.enabled = false;
    }



}

