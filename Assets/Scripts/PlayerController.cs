using UnityEngine;

[RequireComponent(typeof(CharacterController), typeof(AudioSource))]
public class SimpleFirstPersonController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 5.0f;
    public float runSpeed = 9.0f;
    public float gravity = 20f;

    [Header("Mouse Look")]
    public float mouseSensitivityX = 150f;
    public float mouseSensitivityY = 150f;
    public float maxLookUp = 85f;
    public float maxLookDown = 85f;
    public bool invertY = false;
    public bool lockAndHideCursor = true;

    [Header("References")]
    public Camera playerCamera;

    [Header("Footstep Sounds")]
    [SerializeField] private AudioClip footstepClip;
    [SerializeField] private float walkStepInterval = 0.5f;
    [SerializeField] private float runStepInterval = 0.35f;

    private CharacterController controller;
    private AudioSource audioSource;

    private float pitch;
    private float verticalVelocity;

    private float footstepTimer;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();

        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>();

        if (lockAndHideCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void Update()
    {
        HandleMouseLook();
        HandleMovement();
        HandleFootsteps();
    }

    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivityX * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivityY * Time.deltaTime;

        transform.Rotate(Vector3.up * mouseX);

        float invert = invertY ? 1f : -1f;
        pitch += mouseY * invert;
        pitch = Mathf.Clamp(pitch, -maxLookDown, maxLookUp);

        if (playerCamera != null)
            playerCamera.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    private void HandleMovement()
    {
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputZ = Input.GetAxisRaw("Vertical");

        Vector3 input = new Vector3(inputX, 0f, inputZ);
        input = Vector3.ClampMagnitude(input, 1f);

        Vector3 move = transform.TransformDirection(input);

        bool isRunning = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        float speed = isRunning ? runSpeed : walkSpeed;

        Vector3 velocity = move * speed;

        if (controller.isGrounded)
        {
            verticalVelocity = -2f;
        }
        else
        {
            verticalVelocity -= gravity * Time.deltaTime;
        }
        velocity.y = verticalVelocity;

        controller.Move(velocity * Time.deltaTime);
    }

    private void HandleFootsteps()
    {
        // Only play footsteps if moving and grounded
        if (controller.isGrounded && (controller.velocity.magnitude > 0.1f))
        {
            bool isRunning = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            float interval = isRunning ? runStepInterval : walkStepInterval;

            footstepTimer -= Time.deltaTime;

            if (footstepTimer <= 0f)
            {
                if (footstepClip != null)
                {
                    audioSource.PlayOneShot(footstepClip);
                }
                footstepTimer = interval;
            }
        }
        else
        {
            // Reset timer when not moving
            footstepTimer = 0f;
        }
    }
}
