using Unity.Netcode;
using UnityEngine;
using Cinemachine;

/// <summary>
/// Networked player controller — handles movement, camera, and animation.
/// Only the owner controls movement; all clients see synced transforms via NetworkTransform.
/// Attach to the Player prefab alongside NetworkObject, NetworkTransform, and NetworkRigidbody.
/// </summary>
[RequireComponent(typeof(CapsuleCollider))]
public class NetworkPlayerController : NetworkBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float rotationSpeed = 10f;

    [Header("Jump")]
    public float jumpForce = 5f;
    public float groundCheckDistance = 0.3f;
    public LayerMask groundMask;

    [Header("Camera")]
    [Tooltip("Cinemachine FreeLook camera child of the player prefab. Auto-found if null.")]
    public CinemachineFreeLook freeLookCamera;

    [Header("Animator")]
    public Animator animator;

    private Rigidbody rb;
    private Vector2 moveInput;
    private bool isGrounded;
    private bool jumpRequested;

    // Synced animation speed so other clients can see walk/run anims
    private NetworkVariable<float> networkAnimSpeed = new NetworkVariable<float>(
        0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner
    );

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Start kinematic by default so the avatar doesn't fall before network spawn
            rb.isKinematic = true;
            rb.freezeRotation = true;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Auto-find FreeLookCamera if not assigned in Inspector
        if (freeLookCamera == null)
            freeLookCamera = GetComponentInChildren<CinemachineFreeLook>(true);

        if (IsOwner)
        {
            // Enable camera only for the local player
            if (freeLookCamera != null)
            {
                freeLookCamera.gameObject.SetActive(true);

                // Set camera to follow and look at this player
                freeLookCamera.Follow = transform;
                freeLookCamera.LookAt = transform;

                // Disable Cinemachine's built-in input (we handle it manually)
                freeLookCamera.m_XAxis.m_InputAxisName = "";
                freeLookCamera.m_YAxis.m_InputAxisName = "";

                Debug.Log("[NetworkPlayerController] FreeLookCamera enabled for local player");
            }
            else
            {
                Debug.LogWarning("[NetworkPlayerController] No FreeLookCamera found on player prefab!");
            }

            // Enable physics for owner only
            if (rb != null)
                rb.isKinematic = false;
        }
        else
        {
            // Disable camera for remote players
            if (freeLookCamera != null)
                freeLookCamera.gameObject.SetActive(false);

            // Remote players driven by NetworkTransform — keep kinematic
            if (rb != null)
                rb.isKinematic = true;
        }

        // Listen for animation state changes on remote players
        networkAnimSpeed.OnValueChanged += OnAnimSpeedChanged;
    }

    public override void OnNetworkDespawn()
    {
        networkAnimSpeed.OnValueChanged -= OnAnimSpeedChanged;
    }

    void Update()
    {
        if (!IsOwner) return;

        // Movement input
        moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        // Jump input
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            jumpRequested = true;

        CheckGround();
        UpdateAnimator();
        HandleCameraInput();
    }

    void FixedUpdate()
    {
        if (!IsOwner) return;

        Move();
        if (jumpRequested)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            jumpRequested = false;
        }
    }

    void Move()
    {
        if (rb == null) return;

        bool isSprinting = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        float currentSpeed = (moveInput.magnitude > 0.1f && isSprinting) ? runSpeed : walkSpeed;

        if (moveInput.magnitude < 0.1f)
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
            return;
        }

        // Get camera-relative movement direction
        Transform cam = freeLookCamera != null ? freeLookCamera.transform : Camera.main?.transform;
        if (cam == null) return;

        Vector3 camForward = cam.forward;
        Vector3 camRight = cam.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDirection = camForward * moveInput.y + camRight * moveInput.x;

        rb.linearVelocity = new Vector3(
            moveDirection.x * currentSpeed,
            rb.linearVelocity.y,
            moveDirection.z * currentSpeed
        );

        Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.fixedDeltaTime
        );
    }

    void UpdateAnimator()
    {
        if (animator == null) return;

        bool isSprinting = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        float speed = 0f;
        if (moveInput.magnitude > 0.1f)
            speed = isSprinting ? 2f : 1f; // 0=idle, 1=walk, 2=run

        float currentSpeed = animator.GetFloat("Speed");
        float smoothed = Mathf.Lerp(currentSpeed, speed, Time.deltaTime * 8f);
        animator.SetFloat("Speed", smoothed);

        // Sync animation state to other clients
        networkAnimSpeed.Value = smoothed;
    }

    // Apply animation speed from network on remote players
    void OnAnimSpeedChanged(float oldVal, float newVal)
    {
        if (IsOwner) return;
        if (animator != null)
            animator.SetFloat("Speed", newVal);
    }

    void HandleCameraInput()
    {
        if (freeLookCamera == null) return;

        // Right-click drag to orbit camera around player
        if (Input.GetMouseButton(1))
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            freeLookCamera.m_XAxis.Value += mouseX * 300f * Time.deltaTime;
            freeLookCamera.m_YAxis.Value -= mouseY * 2f * Time.deltaTime;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void CheckGround()
    {
        isGrounded = Physics.Raycast(
            transform.position + Vector3.up * 0.1f,
            Vector3.down,
            groundCheckDistance + 0.1f,
            groundMask
        );
    }
}
