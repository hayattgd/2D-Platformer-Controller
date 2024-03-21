using System;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class CharacterMovement : MonoBehaviour
{
    //All parameter or settings variables
    [Header("Positions")]
    [Tooltip("Place a gameobject in bottom of the character and assign it")] [SerializeField] Transform groundPos;

    [Header("Render (Optional)")]
    public bool enableGizmos = true;
    [Space]
    public Transform cam;
    public Vector2 cameraOffset;
    [Min(0)] public float cameraSmooth = 4f;
    [Space]
    public Animator animator;
    public string xSpeed;
    public string ySpeed;
    public string grounded;

    [Header("Movement parameter")]
    [Min(0)]  public float acceleration = 20f;
    [Min(0)]  public float deceleration = 15f;
    [Min(0)]  public float maxSpeed = 5f;
    [Min(0)]  public float airDrag = 2.5f;
    [Space]
    [Min(0)]  public float jumpPower = 8f;
    [Min(-1)] [Tooltip("Must be positive number or -1 to no limit")]  public float maxFallSpeed = 16f;

    [Header("Tags")]
    public string[] groundTags = new string[] { "Untagged" };

    [Header("Behaviour")]
    [Tooltip("Makes script move character with input without extra script")] public bool moveBySelf;
    public FlipMethod flipMethod = FlipMethod.SpriteRender;

    [Header("Events")]
    public UnityEvent OnLanded = new();
    public UnityEvent LeftGround = new();
    public UnityEvent ReachedMaxFallSpeed = new();

    //Private variables
    Rigidbody2D rb;
    BoxCollider2D col;
    SpriteRenderer render;

    float width;

    //Misc
    public bool IsGrounded { protected set; get; }
    public bool AlreadyJumped { protected set; get; } //To make sure player cant double jump in ground to get more speed
    Vector2 pending_velocity;
    float dec_smooth_vel;
    Vector2 cam_smooth_vel;

    public enum FlipMethod
    {
        None,
        Scaling,
        SpriteRender
    }

    private void Start()
    {
        //Initialize private variables
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
        width = col.size.x;

        if (flipMethod == FlipMethod.SpriteRender)
        {
            render = GetComponent<SpriteRenderer>();
            if (!render)
            {
                Debug.LogWarning("SpriteRenderer Not found. Automatically switching to Scaling method.");
                flipMethod = FlipMethod.Scaling;
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (enableGizmos)
        {
            if (groundPos)
            {
                Gizmos.color = IsGrounded ? Color.green : Color.gray;
                Gizmos.DrawCube(groundPos.position, new Vector2(width, 0.05f));
            }
        }
    }

    private void FixedUpdate()
    {
        //Apply pending velocity to actual velocity
        rb.velocity += pending_velocity;
        pending_velocity = Vector2.zero;
    }

    private void LateUpdate()
    {
        //Make camera follows player with offset and smooth
        if(cam)
        cam.position = Vector3.forward * cam.position.z + (Vector3)Vector2.SmoothDamp(cam.position, (Vector2)transform.position + cameraOffset, ref cam_smooth_vel, 1 / cameraSmooth);
    }

    private void Update()
    {
        //Ground detection
        bool ig = IsGrounded;
        IsGrounded = false;
        RaycastHit2D[] hits = Physics2D.BoxCastAll(groundPos.position, new Vector2(width, 0.05f), 0, Vector2.down, 0f);
        foreach (RaycastHit2D hit in hits)
        {
            foreach (string tag in groundTags)
            {
                if (hit.transform.CompareTag(tag))
                {
                    //Call Event
                    if (!ig) OnLanded.Invoke();

                    IsGrounded = true;
                    break;
                }
            }
            if (IsGrounded) break;
        }
        //Reset this so player can jump once again
        if (!IsGrounded) AlreadyJumped = false;

        //Call Event
        if (ig && !IsGrounded) LeftGround.Invoke();

        //Assign Animator values
        if (animator)
        {
            animator.SetFloat(xSpeed, Mathf.Abs(rb.velocity.x));
            animator.SetFloat(ySpeed, rb.velocity.y);
            animator.SetBool(grounded, IsGrounded);
        }

        //Move character with basic input system.
        //Using this will lose customizability
        if (moveBySelf)
        {
            Control(Mathf.RoundToInt(Input.GetAxisRaw("Horizontal")), Input.GetButton("Jump"));
        }
    }

    /// <summary>
    /// Method to let character move by arguments.
    /// To deny players input, just put 0 and false in argument
    /// </summary>
    /// <param name="xdirection">Direction to move</param>
    /// <param name="jump">True for jump</param>
    public void Control(int xdirection, bool jump)
    {
        int direction = Mathf.Clamp(xdirection, -1, 1);

        //Sets transform.localScale depending on direction
        if (direction == 1)
        {
            switch (flipMethod)
            {
                case FlipMethod.Scaling:
                    transform.localScale = new Vector3(1, 1, 1);
                    break;

                case FlipMethod.SpriteRender:
                    if(!render) render = GetComponent<SpriteRenderer>();
                    render.flipX = false;
                    break;
            }
        }
        else if (direction == -1)
        {
            switch (flipMethod)
            {
                case FlipMethod.Scaling:
                    transform.localScale = new Vector3(-1, 1, 1);
                    break;

                case FlipMethod.SpriteRender:
                    if (!render) render = GetComponent<SpriteRenderer>();
                    render.flipX = true;
                    break;
            }
        }

        Vector2 targetvel = Vector2.zero;
        
        //Limit x speed or Just let player move character with input
        if (rb.velocity.x > maxSpeed)
        {
            rb.velocity = new Vector2(maxSpeed, rb.velocity.y);
        }
        else if (rb.velocity.x < -maxSpeed)
        {
            rb.velocity = new Vector2(-maxSpeed, rb.velocity.y);
        }
        else if (direction == 0 && Mathf.Abs(rb.velocity.x) > 0.1f && IsGrounded)
        {
            rb.velocity = new Vector2(Mathf.SmoothDamp(rb.velocity.x, 0, ref dec_smooth_vel, 1 / deceleration), rb.velocity.y);
        }
        else
        {
            targetvel += acceleration * direction * Time.deltaTime * Vector2.right;
        }

        //Apply Air drag to velocity
        if (!IsGrounded) targetvel *= new Vector2(1 / airDrag, 1);

        //Check for jump input and character is on ground. also check for alreadyJumped for preventing double jump while in ground
        if (jump && IsGrounded && !AlreadyJumped)
        {
            targetvel += Vector2.up * jumpPower;
            AlreadyJumped = true;
        }

        //Check maxFallSpeed is greater than 0 and limit velocity.
        if (maxFallSpeed > 0 && rb.velocity.y < -maxFallSpeed)
        {
            rb.velocity = new Vector2(rb.velocity.x, -maxFallSpeed);
        }

        pending_velocity += targetvel;
    }
}