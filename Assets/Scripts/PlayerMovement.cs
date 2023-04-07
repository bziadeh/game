using UnityEngine;

public class PlayerMovement : MonoBehaviour {

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
    private float lastPlay;
    public bool firstPerson;

    public Camera playerCamera;
    public Transform firstPersonCameraLocation;
    public Transform thirdPersonCameraLocation;

    [SerializeField] private LayerMask layerMaskFirstPerson;
    [SerializeField] private LayerMask layerMaskThirdPerson;

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
        lastPlay = Time.time;

        // first person init settings
        firstPerson = true;
        playerCamera.cullingMask = layerMaskFirstPerson;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            UpdateCamera();
        }

        if (controller.isGrounded)
        {
            // Get input for movement
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            if (jumping)
            {
                // just finished jumping make sure
                jumping = false;
                horizontal = 0;
                vertical = 0;
            }

            // Calculate movement direction based on input
            moveDirection = new Vector3(horizontal, 0, vertical);
            moveDirection = transform.TransformDirection(moveDirection);
            moveDirection *= speed;

            animator.SetFloat("Horizontal", horizontal);
            animator.SetFloat("Vertical", vertical);

            // Jump if the jump button is pressed
            if (Input.GetButton("Jump"))
            {
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