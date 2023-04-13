using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour {

    public float speed = 6.0f;
    public float jumpSpeed = 8.0f;
    public float gravity = 20.0f;
    public float mouseSensitivity = 100.0f;

    private Vector3 moveDirection = Vector3.zero;
    private CharacterController controller;
    private Rigidbody rb;
    private Animator animator;

	private AudioSource footstepSource;
    public AudioClip footstepSound;
    public float soundDelay = 0.5f;

	private bool jumping = false;
    public bool firstPerson = false;

    public Camera playerCamera;
    public Transform firstPersonCameraLocation;
    public Transform thirdPersonCameraLocation;

    [SerializeField] private LayerMask layerMaskFirstPerson;
    [SerializeField] private LayerMask layerMaskThirdPerson;
    
    [SerializeField] private AvatarMask upperBodyMoving;
    [SerializeField] private AvatarMask upperBodyStill;

    public float minAngle = -45.0f;
    public float maxAngle = 45.0f;

    //Rotation Value
    float yRotate = 0.0f;
    bool newCamera = true;

    void Start() {
        controller = GetComponent<CharacterController>();
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        animator = GetComponent<Animator>();

        footstepSource = GetComponent<AudioSource>();
        footstepSource.clip = footstepSound;

        // Lock cursor to the center of the screen
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Awake()
    {
        // first person init settings
        playerCamera.cullingMask = layerMaskThirdPerson;
    }

    private void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        HandleRotation();

        if (Input.GetKeyDown(KeyCode.V))
        {
            UpdateCamera();
        }

        if (controller.isGrounded)
        {
            // Get input for movement
            bool moving = Mathf.Abs(horizontal) > 0.0f || Mathf.Abs(vertical) > 0.0f;
            animator.SetBool("isMoving", moving);

            int combatLayers = 2;
            int combatLayerStart = 1;

            bool animating = false;
            for(int i = combatLayerStart; i <= combatLayers; i++)
            {
                AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(i);
                if(animating = info.IsName("Attack") || info.IsName("Attack 2"))
                {
                    animating = true;
                    break;
                }
            }
            animator.SetBool("Animating", animating);
                  
            Vector2 input = Vector2.ClampMagnitude(new Vector2(horizontal, vertical), 1f);
            if (jumping)
            {
                // just finished jumping make sure
                jumping = false;
                horizontal = 0;
                vertical = 0;
            }

            // Calculate movement direction based on input
            moveDirection = new Vector3(input.x, 0, input.y);
            moveDirection = transform.TransformDirection(moveDirection);
            moveDirection *= speed;

            animator.SetFloat("Horizontal", horizontal);
            animator.SetFloat("Vertical", vertical);

            // Jump if the jump button is pressed
            if (Input.GetButton("Jump"))
            {
                animator.SetBool("Strafing", false);
                animator.SetTrigger("Jump");
                moveDirection.y = jumpSpeed;
                jumping = true;
            }
        }

        // Apply gravity
        moveDirection.y -= gravity * Time.deltaTime;

        // Move the character controller
        controller.Move(moveDirection * Time.deltaTime);
    }

    void HandleRotation()
    {
        // Rotate X view
        float horizontalRotation = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        transform.Rotate(0, horizontalRotation, 0);

        //Rotate Y view if 1st Person
        if (firstPerson)
        {
            Transform cam = playerCamera.transform;

            yRotate += Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime * -1;
            yRotate = Mathf.Clamp(yRotate, minAngle, maxAngle);

            playerCamera.transform.eulerAngles = new Vector3(yRotate, cam.eulerAngles.y, cam.eulerAngles.z);
        }
    }

    void UpdateCamera()
    {
        if (firstPerson)
        {
            // make third person mode
            firstPerson = false;
            playerCamera.cullingMask = layerMaskThirdPerson;

            playerCamera.transform.position = thirdPersonCameraLocation.position;
            playerCamera.transform.rotation = thirdPersonCameraLocation.rotation;
            print("Going into Third Person mode");
        }
        else
        {
            // make first person mode
            firstPerson = true;
            playerCamera.cullingMask = layerMaskFirstPerson;

            playerCamera.transform.position = firstPersonCameraLocation.position;
            print("Going into First Person mode");
        }
    }
}