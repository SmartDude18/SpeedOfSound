using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Basic Movement Settings")]
    [SerializeField]
    private float groundAcceleration;
    [SerializeField]
    private float groundSpeedDecayUpperBound, groundSpeedDecayRate, groundSpeedDecayDelay;
    [SerializeField]
    private float groundCheckDistance, jumpPower, airControlSensitivity;

    [Space(15)]
    [Header("Sliding Settings")]
    [SerializeField]
    private float maxSlideDuration;
    [SerializeField]
    private float slideHeightModifier;

    [Space(15)]
    [Header("WallRunning Settings")]
    [SerializeField]
    private float maxWallrunTime;

    [Space(15)]
    [Header("Grappeling Settings")]
    [SerializeField]
    private float grappelMaxLength;
    [SerializeField]
    private float swingAirControlSensitivity, rappelReelInStrength;
    [SerializeField]
    private Material grappelLineMat;

    [Space(15)]
    [Header("object Configurations")]
    [SerializeField]
    private Rigidbody playerRB;
    [SerializeField]
    private GameObject playerCamera, groundCheckStart;


    [Space(15)]
    [Header("tag Configurations")]
    [SerializeField]
    private LayerMask groundLayer;
    [SerializeField]
    private string wallRunTag, rappelTag, swingTag;


    [Space(15)]
    [Header("Debug")]
    [SerializeField]
    private grappelState currentGrappelState = grappelState.NONE;
    [SerializeField]
    private bool hasDoubleJumped;

    private bool isGrounded;
    private float slideTimer = 0, groundDecayDelayTimer = 0;

    //grappeling
    private Vector3? grappelPoint;
    private GameObject grappelObject;
    private LineRenderer lineRenderer;
    private SpringJoint activeSpringJoint;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        // Set the width
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        //color
        lineRenderer.material = grappelLineMat;
    }

    // Update is called once per frame
    void Update()
    {
        playerMove();
        GroundCheck();
        if (Input.GetButtonDown("Jump"))
        {
            Jump();
        }
        //startiing a fresh line
        if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
        {
            if (currentGrappelState == grappelState.NONE)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    GrappelBase(true);
                }
                else if (Input.GetMouseButtonDown(1))
                {
                    GrappelBase(false);
                }
            }
            if (currentGrappelState == grappelState.SWING && !Input.GetMouseButton(0) && Input.GetMouseButton(1))
            {
                currentGrappelState = grappelState.RAPPEL;
                StartRappel();
            }
            else if (currentGrappelState == grappelState.RAPPEL && Input.GetMouseButton(0) && !Input.GetMouseButton(1))
            {
                currentGrappelState = grappelState.SWING;
                StartSwing();
            }
            if (grappelPoint != null)
            {
                lineRenderer.SetPosition(0, transform.position);
                lineRenderer.SetPosition(1, grappelPoint.Value);
            }
        }
        else
        {
            currentGrappelState = grappelState.NONE;
            grappelPoint = null;
        }



    }

    void Jump()
    {
        if (isGrounded)
        {
            //in case we go to change this later
            Vector3 jumpDir = Vector3.up;
            playerRB.AddForce(jumpDir * jumpPower, ForceMode.Impulse);
        }
        else
        {
            if (!hasDoubleJumped)
            {
                //in case we go to change this later 
                //yes duplicate, might be different between double jump and ground
                Vector3 jumpDir = Vector3.up;
                playerRB.AddForce(jumpDir * jumpPower, ForceMode.Impulse);
                hasDoubleJumped = true;
            }
        }
    }

    void playerMove()
    {
        //take input with acceleration and add to rigidbody
        playerRB.AddForce((transform.forward * Input.GetAxis("Vertical") * groundAcceleration) + (transform.right * Input.GetAxis("Horizontal") * groundAcceleration), ForceMode.Force);
        //if on the ground and speed is more than the groundSpeedDecayUpperBound, start the timer
        if (isGrounded && playerRB.linearVelocity.magnitude >= groundSpeedDecayUpperBound)
        {
            groundDecayDelayTimer += Time.deltaTime;
            if (groundDecayDelayTimer >= groundSpeedDecayDelay)
            {
                //reduce the speed
                playerRB.linearVelocity *= groundSpeedDecayRate;
            }

        }
        else //reset if not on ground or slowed down
        {
            groundDecayDelayTimer = 0;
        }
    }

    void GroundCheck()
    {
        isGrounded = Physics.CheckSphere(groundCheckStart.transform.position, groundCheckDistance, groundLayer);
        //if it is, config doublejump to default
        if (isGrounded) { hasDoubleJumped = false; }

    }

    //add in the connection point to the method params
    void StartSwing()
    {
        //connect the player and the connection point with a joint
        //probably a spring joint, configured to match current length to point, perhaps a tad less
    }

    //add in the connection point to the method params
    void StartRappel()
    {
        activeSpringJoint = transform.AddComponent<SpringJoint>();
        Rigidbody grappelRb;
        if (grappelObject.TryGetComponent<Rigidbody>(out grappelRb))
        {
            activeSpringJoint.connectedBody = grappelRb;
            activeSpringJoint.spring = rappelReelInStrength;
            activeSpringJoint.maxDistance = 0;
        }

        //connect the player and the connection point with a joint
        //probably a spring joint, shorter to pull the player to the point
    }

    void GrappelBase(bool isLeftClick)
    {
        //figure out the connection point, and allocate it to the respective function based on what button is held
        //may need to be reconfigured due to input system
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        Physics.Raycast(ray, out hit);
        grappelPoint = hit.point;
        grappelObject = hit.transform.gameObject;
        //also draw the line
        //line renderer
        if (isLeftClick)
        {
            currentGrappelState = grappelState.SWING;
            StartSwing();
        }
        else
        {
            currentGrappelState = grappelState.RAPPEL;
            StartRappel();
        }
    }

    void Slide()
    {
        //shorten player to fit under gaps
        //run timer for how long this can go for in a single go
        //timer starts at 0 and goes up until it goes out of time
        //can be canceled with jumps
    }

    void WallRun()
    {
        //i dont think this is how it will be formatted, but I can use this to hold notes
        //raycast to either side of player to look for walls
        //if hit on wallrun surface :
        //(not if swing or rappel)
        //find wall normal (this is a bit weird If i remember correctly)
        //move player along it and against the wall
        //refresh double jump
    }


    private enum grappelState
    {
        NONE,
        SWING,
        RAPPEL
    }
}
