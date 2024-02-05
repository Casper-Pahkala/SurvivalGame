using QFSW.QC;
using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public bool isGrounded = true;
    Rigidbody rb;
    public float speed = 4f;
    public float jumpHeight = 4f;
    public CharacterController controller;
    public float gravity = -9.81f;
    Vector3 velocity;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;
    public Transform groundCheck;
    bool jump = false;
    bool running = false;
    Vector3 move;
    bool isJumping = false;
    Animator currentAnimator;
    public float camerayOffset = 1f;
    public bool isCrouching = false;
    public bool cIsGrounded = false;
    public float _velocity = 10f;
    public float jumpSpeedFactor = 0.75f;
    public float jumpCrouchSpeedFactor = 0.3f;
    public bool hitRoof = false;
    public float velocityOnRoofHit = 0f;
    public bool fromground = true;
    // Start is called before the first frame update
    void Start()
    {
        if (!GetComponent<NetworkObject>().IsOwner)
        {
            this.enabled = false;
            return;
        }
        rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;


    }


    private void Update()
    {
        cIsGrounded = GetComponent<CharacterController>().isGrounded;
        if (cIsGrounded)
        {
            if (!fromground)
            {
                fromground = true;
            }
        }
        if (!GetComponent<NetworkObject>().IsOwner)
        {
            GetComponent<PlayerMovement>().enabled = false;
        }


        if (!isCrouching)
        {
            isGrounded = cIsGrounded;
        }
        else
        {
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        }
        if (isGrounded)
        {
            isJumping = false;

        }
        CheckKey();


        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");


        if (!QuantumConsole.gamePaused)
        {

            move = transform.right * x + transform.forward * z;

        }
        else
        {
            move = Vector3.zero;
        }

        if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.LeftShift))
        {
            if (isGrounded)
            {
                if (!isCrouching)
                {
                    _state = CharacterState.sprinting;
                    running = true;
                    move = move * 1.5f;
                    move = Vector3.ClampMagnitude(move, _velocity * 1.5f);
                    controller.Move(move * speed * Time.deltaTime);
                }
                else
                {
                    move = Vector3.ClampMagnitude(move, _velocity * 0.5f);
                    controller.Move(move * speed * Time.deltaTime);
                }
            }
            else
            {
                if (!isCrouching)
                {
                    _state = CharacterState.jumping;
                    running = false;
                    move = Vector3.ClampMagnitude(move, _velocity * jumpSpeedFactor);
                    controller.Move(move * speed * Time.deltaTime);
                }
                else
                {
                    _state = CharacterState.jumping;
                    running = false;
                    move = Vector3.ClampMagnitude(move, _velocity * jumpCrouchSpeedFactor);
                    controller.Move(move * speed * Time.deltaTime);
                }

            }





        }
        else
        {
            if (isGrounded)
            {
                if (!isCrouching)
                {
                    running = false;
                    move = Vector3.ClampMagnitude(move, _velocity);
                    controller.Move(move * speed * Time.deltaTime);
                }
                else
                {
                    running = false;
                    move = Vector3.ClampMagnitude(move, _velocity * 0.5f);
                    controller.Move(move * speed * Time.deltaTime);
                }
            }
            else
            {

                if (!isCrouching)
                {
                    _state = CharacterState.jumping;
                    running = false;
                    move = Vector3.ClampMagnitude(move, _velocity * 0.5f);
                    controller.Move(move * speed * Time.deltaTime);
                }
                else
                {
                    _state = CharacterState.jumping;
                    running = false;
                    move = Vector3.ClampMagnitude(move, _velocity * 0.3f);
                    controller.Move(move * speed * Time.deltaTime);
                }

            }


        }




        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && !QuantumConsole.gamePaused)
        {

            isJumping = true;
            jump = true;
        }

        if (Input.GetKey(KeyCode.LeftControl) && !QuantumConsole.gamePaused && isGrounded)
        {
            if (!isCrouching)
            {
                //crouching
                isCrouching = true;
                GetComponent<CharacterController>().height = 2f;
            }
        }
        else
        {
            //standing
            if (isCrouching)
            {
                isCrouching = false;
                GetComponent<CharacterController>().height = 3.3f;
            }
        }





    }

    private void FixedUpdate()
    {
        if (jump)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jump = false;
        }
        if (hitRoof)
        {
            hitRoof = false;
            velocity.y = velocityOnRoofHit;
            Debug.Log("hit roof");
        }


        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity);

        PublicVariables.myX = transform.position.x;
        PublicVariables.myY = transform.position.y;
        PublicVariables.myZ = transform.position.z;
    }

    [Command]
    void jumpheight(float height)
    {
        jumpHeight = height;
    }

    [Command]
    void movementspeed(float _speed)
    {
        speed = _speed;
    }

    public enum CharacterState
    {
        idle,
        walking,
        sprinting,
        jumping,
        none,
        hit
    }

    public CharacterState _state;
    public string idleAnimName;
    public string walkAnimName;


    void CheckKey()
    {
        if (transform.GetChild(0).childCount > 0)
        {
            currentAnimator = transform.GetChild(0).GetChild(0).GetComponent<Animator>(); ;
        }
        else
        {
            currentAnimator = null;
        }


        if (currentAnimator == null) return;

        if (move == Vector3.zero)
        {
            _state = CharacterState.idle;

        }
        if ((Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.S)) && !running)
        {

            _state = CharacterState.walking;

        }
        if (running)
        {
            _state = CharacterState.sprinting;
        }
        if (isJumping && !PublicVariables.isHitting)
        {
            _state = CharacterState.jumping;
        }
        if (PublicVariables.isHitting)
        {
            _state = CharacterState.hit;
        }


        PlayAnimation();
    }

    void PlayAnimation()
    {
        switch (_state)
        {
            case CharacterState.idle:
                currentAnimator.SetTrigger("Idle");
                break;

            case CharacterState.walking:
                currentAnimator.SetTrigger("Walk");
                break;
            case CharacterState.sprinting:
                currentAnimator.SetTrigger("Sprint");
                break;
            case CharacterState.jumping:
                currentAnimator.SetTrigger("Jump");
                break;
            case CharacterState.hit:
                _state = CharacterState.none;
                PublicVariables.isHitting = false;
                isJumping = false;
                running = false;
                break;
        }
    }




}
