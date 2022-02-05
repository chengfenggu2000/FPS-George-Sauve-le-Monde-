using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{   
    public static PlayerController instance;
    
    public float MoveSpeed, GravityModifier, JumpPower, RunSpeed = 12f;
    public CharacterController CharCon;
    public Transform CamTrans;
    public float MouseSensitivity;
    public bool InvertX;
    public bool InvertY;
    public Transform GroundCheckPoint;
    public LayerMask WhatIsGround;
    public Animator Anim;
    public GameObject Bullet;
    public Transform FirePoint;

    private Vector3 MoveInput;
    private bool CanJump;

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //player control
        //MoveInput.x = Input.GetAxis("Horizontal") * MoveSpeed * Time.deltaTime;
        //MoveInput.z = Input.GetAxis("Vertical") * MoveSpeed * Time.deltaTime;

        float yStore = MoveInput.y;

        Vector3 VertMove = transform.forward * Input.GetAxis("Vertical");
        Vector3 HoriMove = transform.right * Input.GetAxis("Horizontal");

        MoveInput = HoriMove + VertMove; 
        MoveInput.Normalize();

        if(Input.GetKey(KeyCode.LeftShift))
        {
            MoveInput = MoveInput * RunSpeed;
        }
        else
        {
            MoveInput = MoveInput * MoveSpeed;
        }
        
        MoveInput.y = yStore;

        MoveInput.y += Physics.gravity.y * GravityModifier * Time.deltaTime;

        if(CharCon.isGrounded)
        {
            MoveInput.y = Physics.gravity.y * GravityModifier * Time.deltaTime;
        }

        //jump

        CanJump = Physics.OverlapSphere(GroundCheckPoint.position, .25f, WhatIsGround).Length > 0;
        
        if(Input.GetKeyDown(KeyCode.Space) && CanJump)
        {
            MoveInput.y = JumpPower;
        }

        CharCon.Move(MoveInput * Time.deltaTime);

        //camera control
        Vector2 MouseInput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * MouseSensitivity;

        if(InvertX)
        {
            MouseInput.x = -MouseInput.x;
        }

        if(InvertY)
        {
            MouseInput.y = -MouseInput.y;
        }

        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y + MouseInput.x, transform.rotation.eulerAngles.z);

        CamTrans.rotation = Quaternion.Euler(CamTrans.rotation.eulerAngles + new Vector3(-MouseInput.y, 0f, 0f));

        //shooting
        if(Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            if(Physics.Raycast(CamTrans.position, CamTrans.forward, out hit, 50f))
            {
                if(Vector3.Distance(CamTrans.position, hit.point) > 2f)
                {
                    FirePoint.LookAt(hit.point);
                }
                
            }else
            {
                FirePoint.LookAt(CamTrans.position + (CamTrans.forward * 30f));
            }

            Instantiate(Bullet, FirePoint.position, FirePoint.rotation);
        }
        Anim.SetFloat("MoveSpeed", MoveInput.magnitude);
        Anim.SetBool("OnGround", CanJump);
    }
}
