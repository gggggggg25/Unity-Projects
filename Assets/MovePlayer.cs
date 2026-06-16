using System.Numerics;
using Unity.Netcode;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class MovePlayer : NetworkBehaviour
{
    [Header("Movement")]
    private float movespeed;
    public float walkspeed;
    public float sprintspeed;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask ground;
    bool grounded;
    public float groundDrag;
    public float wallrunspeed;

    [Header("SlopeHandler")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;


    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintkey = KeyCode.LeftShift;
    public KeyCode crouchkey = KeyCode.LeftControl;

    public Transform Orientation;

    float horizontalInput;
    float verticalInput;

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;



    [Header("Jumping")]
    public float jumpforce;
    public float jumpcooldown;
    public float airmultiplier;
    bool ready = true;

    public MovementState state;
    public enum MovementState
    {
        walking,
        sprinting,
        wallrunning,
        crouching,
        air
    }

    UnityEngine.Vector3 movementdirection;

    public bool wallrunning;

    Rigidbody rb;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        startYScale = transform.localScale.y;

    }
    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            enabled = false;
            return;
        }
    }

    // Update is called once per frame
    void Update()
    {
        myInput();
        speedControl();
        stateHandler();

        //drag control
        grounded = Physics.Raycast(transform.position, UnityEngine.Vector3.down, playerHeight * 0.5f + 0.2f, ground);
        if (grounded)
        {
            rb.linearDamping = groundDrag;
        }
        else
        {
            rb.linearDamping = 0;
        }
    }

    private void FixedUpdate()
    {
        movePlayer();
    }

    private void myInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        if(Input.GetKey(jumpKey) && ready && grounded)
        {
            ready = false;
            jump();
            Invoke(nameof(resetjump), jumpcooldown);
        }

        //startcrouch
        if (Input.GetKeyDown(crouchkey))
        {
            transform.localScale = new UnityEngine.Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(UnityEngine.Vector3.down * 5f, ForceMode.Impulse);
        }

        if (Input.GetKeyUp(crouchkey))
        {
            transform.localScale = new UnityEngine.Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        }
    }

    private void stateHandler()
    {
        if (wallrunning)
        {
            state = MovementState.wallrunning;
            movespeed = wallrunspeed;
        }
        //crouching 
        if (Input.GetKey(crouchkey))
        {
            state = MovementState.crouching;
            movespeed = crouchSpeed;
        }
        //sprinting
        if(grounded && Input.GetKey(sprintkey))
        {
            state = MovementState.sprinting;
            movespeed = sprintspeed;
        }
        //walking
        else if(grounded)
        {
            state = MovementState.walking;
            movespeed = walkspeed;
        }
        //air
        else
        {
            state = MovementState.air;

        }
        
    }
    private void movePlayer()
    {
        movementdirection = Orientation.forward * verticalInput + Orientation.right * horizontalInput;

        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection() * movespeed * 20f, ForceMode.Force);

            if (rb.linearVelocity.y > 0)
                rb.AddForce(UnityEngine.Vector3.down * 80f, ForceMode.Force);
        }
        else if (grounded)
        {
            rb.AddForce(movementdirection * movespeed * 10f, ForceMode.Force);
        }
        else
        {
           rb.AddForce(movementdirection * movespeed * 10f * airmultiplier, ForceMode.Force); 
        }

        rb.useGravity = !OnSlope();
    }     

    private void speedControl()
    {
        if (OnSlope() && !exitingSlope)
        {
            if (rb.linearVelocity.magnitude > movespeed)
                rb.linearVelocity = rb.linearVelocity.normalized * movespeed;
        }
        else
        {
            UnityEngine.Vector3 flatVelocity = new UnityEngine.Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            if(flatVelocity.magnitude > movespeed)
            {
                UnityEngine.Vector3 limitedVelocity = flatVelocity.normalized * movespeed;
                rb.linearVelocity = new UnityEngine.Vector3(limitedVelocity.x, rb.linearVelocity.y, limitedVelocity.z);
            }    
        }
        
    }

    private void jump()
    {
        exitingSlope = true;
        rb.linearVelocity = new UnityEngine.Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(transform.up * jumpforce, ForceMode.Impulse);
    }

    private void resetjump()
    {
        ready = true;
        exitingSlope = false;
    }

    private bool OnSlope()
    {
        if(Physics.Raycast(transform.position, UnityEngine.Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = UnityEngine.Vector3.Angle(UnityEngine.Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    private UnityEngine.Vector3 GetSlopeMoveDirection()
    {
        return UnityEngine.Vector3.ProjectOnPlane(movementdirection, slopeHit.normal).normalized;
    }
}
