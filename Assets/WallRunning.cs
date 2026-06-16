using Unity.VisualScripting;
using UnityEngine;

public class WallRunning : MonoBehaviour
{

    [Header("Wallrunning")]
    public LayerMask ground;
    public LayerMask wall;
    public float wallrunforce;
    public float maxwallruntime;
    private float wallruntimer;
    public float walljumpupforce;
    public float walljumpsideforce;

    [Header("Exiting")]
    private bool exitingWall;
    public float exitwalltime;
    private float exitwalltimer;

    [Header("Input")]
    private float horizontalinput;
    private float verticalinput;
    public KeyCode jumpkey = KeyCode.Space;

    [Header("Detection")]
    public float wallCheckDistance;
    public float minjumpheight;
    private RaycastHit rightwallhit;
    private RaycastHit leftwallhit;
    private bool wallLeft;
    private bool wallRight;

    [Header("Refrences")]
    public Transform orientation;
    private MovePlayer pm;
    private Rigidbody rb;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pm = GetComponent<MovePlayer>();
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        WallCheck();
        statemachine();
    }

    private void FixedUpdate()
    {
        if (pm.wallrunning)
        {
            WallRunningMovement();
        }
    }

    private void walljump()
    {
        exitingWall = true;
        exitwalltimer = exitwalltime;
        Vector3 wallNormal = wallRight ? rightwallhit.normal : leftwallhit.normal;

        Vector3 forcetoapply = transform.up * walljumpupforce + wallNormal * walljumpsideforce;

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(forcetoapply, ForceMode.Impulse);
    }

    private void WallCheck()
    {
        wallRight = Physics.Raycast(transform.position, orientation.right, out rightwallhit, wallCheckDistance, wall);
        wallLeft = Physics.Raycast(transform.position, -orientation.right, out leftwallhit, wallCheckDistance, wall);
    }

    private bool AboveGround()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minjumpheight, ground);
    }

    private void statemachine()
    {
        //get inputs
        horizontalinput = Input.GetAxisRaw("Horizontal");
        verticalinput = Input.GetAxisRaw("Vertical");

        //state 1: wallrunning
        if((wallLeft || wallRight) && verticalinput > 0 && AboveGround() && !exitingWall)
        {
            if (!pm.wallrunning)
            {
                startwallrun();
            }

            if (Input.GetKeyDown(jumpkey))
            {
                walljump();
            }

        }
        //state 2: exit wall
        else if(exitingWall)
        {
            if (pm.wallrunning)
            {
                stopwallrun();
            }

            if(exitwalltimer > 0)
            {
                exitwalltimer -= Time.deltaTime;
            }

            if (exitwalltimer <= 0)
            {
                exitingWall = false;
            }
            else
            {
                if (pm.wallrunning)
                {
                    stopwallrun();
                }
            }
        }

    }

    private void startwallrun()
    {
        pm.wallrunning = true;
    }

    private void stopwallrun()
    {
        pm.wallrunning = false;
    }

    private void WallRunningMovement()
    {
        rb.useGravity = false;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        Vector3 wallNormal = wallRight ? rightwallhit.normal : leftwallhit.normal;
        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

        if((orientation.forward - wallForward).magnitude > (orientation.forward - -wallForward).magnitude)
        {
            wallForward = -wallForward;
        }

        rb.AddForce(wallForward * wallrunforce, ForceMode.Force);

        if(!(wallLeft && horizontalinput > 0) && !(wallRight && horizontalinput < 0))
        {
            rb.AddForce( -wallNormal * 100, ForceMode.Force);
        }
    }
}
