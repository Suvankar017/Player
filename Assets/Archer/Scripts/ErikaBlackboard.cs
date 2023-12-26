using Cinemachine;
using ScriptableStateMachine;
using UnityEngine;

public class ErikaBlackboard : MonoBehaviour
{
    [Header("Movement")]
    public float jogSpeed = 2.585f;
    public float runSpeed = 4.416f;
    public float sprintSpeed = 5.656f;
    public float bowWalkSpeed = 1.156f;
    public float bowRunSpeed = 3.632f;
    public float bowSprintSpeed = 5.143f;
    public float acceleration = 10.0f;
    public bool rotateBasedOnAngularSpeed = true;
    public float angularSpeed = 120.0f;
    public float rotationSmoothTime = 0.2f;
    public float gravity = -9.81f;
    public Vector3 groundCheckOffset;
    public float groundCheckRadius = 0.3f;
    public LayerMask walkableLayer;
    public Transform ErikaBody;

    [Header("Camera")]
    public float topMaxAngle = 70.0f;
    public float bottomMaxAngle = -30.0f;
    public float lookSensitivity = 5.0f;
    public Transform cameraFollow;
    public Transform jumpCameraFollow;
    public CinemachineVirtualCamera virtualCamera;
    public CinemachineVirtualCamera jumpVirtualCamera;

    [Header("Components")]
    public Transform playerTransform;
    public Transform graphicsTransform;
    public Camera playerCamera;
    public CharacterController characterController;
    public Animator animator;
    public ThirdPersonInputs inputs;
    public Transform leftFootTransform;
    public Transform rightFootTransform;
    public GameObject footParticle;

    [Header("Debug Variables")]
    [SerializeField] private bool m_IsGrounded = true;
    [SerializeField] private Vector3 m_Velocity;
    [SerializeField] private bool m_AbleToMove = true;
    private float m_AngularVelocity;
    private Vector3 m_CameraRotation;

    public bool IsGrounded
    {
        get { return m_IsGrounded; }
    }
    public Vector3 Velocity
    {
        get { return m_Velocity; }
        set { m_Velocity = value; }
    }
    public bool AbleToMove
    {
        get => m_AbleToMove;
        set => m_AbleToMove = value;
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {

    }

    private void Update()
    {
        if (!m_JumpCam)
        {
            if (Physics.Raycast(jumpCameraFollow.position, Vector3.down, out RaycastHit hit))
            {
                if (CompareLayer(hit.collider.gameObject.layer, walkableLayer))
                {
                    print(hit.point);
                    ErikaBody.SetParent(null);
                    transform.position = hit.point;
                    //ErikaBody.SetParent(transform);
                    //ErikaBody.localPosition = Vector3.zero;


                    //virtualCamera.Priority = 20;
                    //jumpVirtualCamera.Priority = 10;
                }
            }

            m_JumpCam = true;
        }

        if (normalCam)
        {
            normalCam = false;

            ErikaBody.SetParent(transform);
            ErikaBody.localPosition = Vector3.zero;


            virtualCamera.Priority = 20;
            jumpVirtualCamera.Priority = 10;
        }
    }

    public float m_MoveKeyPressTime = 0.0f;
    public float m_MoveKeyPressThresholdTime = 1.0f;

    public void Movement()
    {
        float targetSpeed = GetTargetSpeed();
        float currentSpeed = m_Velocity.magnitude;
        const float speedOffset = 0.1f;
        Vector2 moveInput = inputs.Move;

        if (moveInput.sqrMagnitude > 0.0f)
        {
            float tempTargetSpeed = targetSpeed;
            if (m_MoveKeyPressTime > m_MoveKeyPressThresholdTime)
            {
                targetSpeed = tempTargetSpeed;
                moveInput = inputs.Move;
            }
            else
            {
                targetSpeed = 0.0f;
                moveInput = Vector2.zero;
                m_MoveKeyPressTime += Time.deltaTime;
            }
        }
        else
        {
            m_MoveKeyPressTime = 0.0f;
        }

        // accelerating or decelerating
        if (currentSpeed < targetSpeed - speedOffset || currentSpeed > targetSpeed + speedOffset)
        {
            currentSpeed += Mathf.Sign(targetSpeed - currentSpeed) * acceleration * Time.deltaTime;
            currentSpeed = Mathf.Round(currentSpeed * 1000.0f) * 0.001f;
        }
        else
        {
            currentSpeed = targetSpeed;
        }


        if (moveInput != Vector2.zero)
        {
            Transform playerCameraTransform = playerCamera.transform;
            Vector2 moveInputDir = moveInput.normalized;
            float targetYRotation = Mathf.Atan2(moveInputDir.x, moveInputDir.y) * Mathf.Rad2Deg + playerCameraTransform.eulerAngles.y;

            if (rotateBasedOnAngularSpeed)
            {
                Quaternion targetRotation = Quaternion.Euler(0.0f, targetYRotation, 0.0f);
                playerTransform.rotation = Quaternion.RotateTowards(playerTransform.rotation, targetRotation, angularSpeed * Time.deltaTime);
            }
            else
            {
                float currentRotation = Mathf.SmoothDampAngle(playerTransform.eulerAngles.y, targetYRotation, ref m_AngularVelocity, rotationSmoothTime);
                playerTransform.rotation = Quaternion.Euler(0.0f, currentRotation, 0.0f);
            }
        }

        Vector3 moveDir = Quaternion.Euler(0.0f, playerTransform.eulerAngles.y, 0.0f) * Vector3.forward;
        Ray rayFromOrigin = new Ray(playerTransform.position/* + PlayerTransform.forward * Controller.radius*/, Vector3.down);
        Color rayColor = Color.red;

        if (Physics.Raycast(rayFromOrigin, out RaycastHit hitOrigin))
        {
            if (CompareLayer(hitOrigin.collider.gameObject.layer, walkableLayer))
            {
                if (hitOrigin.distance <= 0.5f)
                {
                    moveDir = Vector3.ProjectOnPlane(moveDir, hitOrigin.normal).normalized;
                    rayColor = Color.green;
                }
            }
        }

        Debug.DrawRay(rayFromOrigin.origin, rayFromOrigin.direction * 0.5f, rayColor);

        m_Velocity = (moveDir * 0.5f).normalized * currentSpeed;
        m_Velocity.y += gravity * Time.deltaTime;

        characterController.Move(m_Velocity * Time.deltaTime);

        animator.SetFloat("MoveSpeed", Mathf.Clamp(currentSpeed, 0.0f, sprintSpeed));
    }

    public void BowAndArrowMovement()
    {
        float targetSpeed = GetBowAndArrowTargetSpeed();
        float currentSpeed = m_Velocity.magnitude;
        const float speedOffset = 0.1f;
        Vector2 moveInput = inputs.Move;

        if (moveInput.sqrMagnitude > 0.0f)
        {
            float tempTargetSpeed = targetSpeed;
            if (m_MoveKeyPressTime > m_MoveKeyPressThresholdTime)
            {
                targetSpeed = tempTargetSpeed;
                moveInput = inputs.Move;
            }
            else
            {
                targetSpeed = 0.0f;
                moveInput = Vector2.zero;
                m_MoveKeyPressTime += Time.deltaTime;
            }
        }
        else
        {
            m_MoveKeyPressTime = 0.0f;
        }

        // accelerating or decelerating
        if (currentSpeed < targetSpeed - speedOffset || currentSpeed > targetSpeed + speedOffset)
        {
            currentSpeed += Mathf.Sign(targetSpeed - currentSpeed) * acceleration * Time.deltaTime;
            currentSpeed = Mathf.Round(currentSpeed * 1000.0f) * 0.001f;
        }
        else
        {
            currentSpeed = targetSpeed;
        }


        if (moveInput != Vector2.zero)
        {
            Transform playerCameraTransform = playerCamera.transform;
            Vector2 moveInputDir = moveInput.normalized;
            float targetYRotation = Mathf.Atan2(moveInputDir.x, moveInputDir.y) * Mathf.Rad2Deg + playerCameraTransform.eulerAngles.y;

            if (rotateBasedOnAngularSpeed)
            {
                Quaternion targetRotation = Quaternion.Euler(0.0f, targetYRotation, 0.0f);
                playerTransform.rotation = Quaternion.RotateTowards(playerTransform.rotation, targetRotation, angularSpeed * Time.deltaTime);
            }
            else
            {
                float currentRotation = Mathf.SmoothDampAngle(playerTransform.eulerAngles.y, targetYRotation, ref m_AngularVelocity, rotationSmoothTime);
                playerTransform.rotation = Quaternion.Euler(0.0f, currentRotation, 0.0f);
            }
        }

        Vector3 moveDir = Quaternion.Euler(0.0f, playerTransform.eulerAngles.y, 0.0f) * Vector3.forward;
        Ray rayFromOrigin = new Ray(playerTransform.position/* + PlayerTransform.forward * Controller.radius*/, Vector3.down);
        Color rayColor = Color.red;

        if (Physics.Raycast(rayFromOrigin, out RaycastHit hitOrigin))
        {
            if (CompareLayer(hitOrigin.collider.gameObject.layer, walkableLayer))
            {
                if (hitOrigin.distance <= 0.5f)
                {
                    moveDir = Vector3.ProjectOnPlane(moveDir, hitOrigin.normal).normalized;
                    rayColor = Color.green;
                }
            }
        }

        Debug.DrawRay(rayFromOrigin.origin, rayFromOrigin.direction * 0.5f, rayColor);

        m_Velocity = (moveDir * 0.5f).normalized * currentSpeed;
        m_Velocity.y += gravity * Time.deltaTime;

        characterController.Move(m_Velocity * Time.deltaTime);

        animator.SetFloat("MoveSpeed", Mathf.Clamp(currentSpeed, 0.0f, bowSprintSpeed));
    }

    public void BowAndArrowAimingMovement()
    {
        float targetSpeed = GetBowAndArrowAimingTargetSpeed();
        float currentSpeed = m_Velocity.magnitude;
        const float speedOffset = 0.1f;
        Vector2 moveInput = inputs.Move;

        if (moveInput.sqrMagnitude > 0.0f)
        {
            float tempTargetSpeed = targetSpeed;
            if (m_MoveKeyPressTime > m_MoveKeyPressThresholdTime)
            {
                targetSpeed = tempTargetSpeed;
            }
            else
            {
                targetSpeed = 0.0f;
                m_MoveKeyPressTime += Time.deltaTime;
            }
        }
        else
        {
            m_MoveKeyPressTime = 0.0f;
        }

        // accelerating or decelerating
        if (currentSpeed < targetSpeed - speedOffset || currentSpeed > targetSpeed + speedOffset)
        {
            currentSpeed += Mathf.Sign(targetSpeed - currentSpeed) * acceleration * Time.deltaTime;
            currentSpeed = Mathf.Round(currentSpeed * 1000.0f) * 0.001f;
        }
        else
        {
            currentSpeed = targetSpeed;
        }


        playerTransform.rotation = Quaternion.Euler(0.0f, playerCamera.transform.eulerAngles.y, 0.0f);
        

        Vector3 moveDir = Quaternion.Euler(0.0f, playerTransform.eulerAngles.y, 0.0f) * new Vector3(moveInput.x, 0.0f, moveInput.y);
        Ray rayFromOrigin = new Ray(playerTransform.position, Vector3.down);
        Color rayColor = Color.red;

        if (Physics.Raycast(rayFromOrigin, out RaycastHit hitOrigin))
        {
            if (CompareLayer(hitOrigin.collider.gameObject.layer, walkableLayer))
            {
                if (hitOrigin.distance <= 0.5f)
                {
                    moveDir = Vector3.ProjectOnPlane(moveDir, hitOrigin.normal).normalized;
                    rayColor = Color.green;
                }
            }
        }

        Debug.DrawRay(rayFromOrigin.origin, rayFromOrigin.direction * 0.5f, rayColor);

        m_Velocity = (moveDir * 0.5f).normalized * currentSpeed;
        m_Velocity.y += gravity * Time.deltaTime;

        characterController.Move(m_Velocity * Time.deltaTime);

        animator.SetFloat("MoveSpeed", Mathf.Clamp(currentSpeed, 0.0f, bowSprintSpeed) * moveInput.y);
        float strafeSpeed = Remap(currentSpeed, 0.0f, 1.156f, 0.0f, 1.0f);
        animator.SetFloat("StrafeSpeed", Mathf.Clamp(strafeSpeed, 0.0f, 1.0f) * moveInput.x);
    }

    public float Remap(float current, float fromA, float fromB, float toA, float toB)
    {
        return Mathf.Lerp(toA, toB, Mathf.InverseLerp(fromA, fromB, current));
    }

    public void Falling()
    {
        m_Velocity.y += gravity * Time.deltaTime;
        characterController.Move(m_Velocity * Time.deltaTime);
    }

    public void GroundCheck()
    {
        m_IsGrounded = Physics.CheckSphere(playerTransform.position + groundCheckOffset, groundCheckRadius, walkableLayer);

        animator.SetBool("IsGrounded", m_IsGrounded);
    }

    private bool m_JumpCam = true;
    public bool normalCam = false;

    public void JumpCamera(bool active)
    {
        if (active)
        {
            jumpVirtualCamera.Priority = 20;
            virtualCamera.Priority = 10;
        }
        else
        {
            m_JumpCam = false;
        }
    }

    public void UpdateCamera()
    {
        Vector2 look = inputs.Look;

        if (look.magnitude >= 0.01f)
        {
            m_CameraRotation.x += look.y * lookSensitivity;
            m_CameraRotation.y += look.x * lookSensitivity;
        }

        m_CameraRotation.x = ClampAngle(m_CameraRotation.x, bottomMaxAngle, topMaxAngle);
        m_CameraRotation.y = ClampAngle(m_CameraRotation.y, float.MinValue, float.MaxValue);

        cameraFollow.rotation = Quaternion.Euler(m_CameraRotation.x, m_CameraRotation.y, 0.0f);
    }
    
    public void BowAndArrowCameraMovement()
    {
        Vector2 look = inputs.Look;

        if (look.magnitude >= 0.01f)
        {
            m_CameraRotation.x += look.y * lookSensitivity;
            m_CameraRotation.y += look.x * lookSensitivity;
        }

        m_CameraRotation.x = ClampAngle(m_CameraRotation.x, bottomMaxAngle, topMaxAngle);
        m_CameraRotation.y = ClampAngle(m_CameraRotation.y, float.MinValue, float.MaxValue);

        cameraFollow.rotation = Quaternion.Euler(m_CameraRotation.x, m_CameraRotation.y, 0.0f);
    }

    public void OnFootTouchGround(int foot)
    {
        if (foot == -1)
        {
            var particle = Instantiate(footParticle, leftFootTransform);
            Destroy(particle, 1.0f);
        }
        else
        {
            var particle = Instantiate(footParticle, rightFootTransform);
            Destroy(particle, 1.0f);
        }
    }

    private float ClampAngle(float angle, float minAngle, float maxAngle)
    {
        if (angle < -360.0f)
            angle += 360.0f;

        if (angle > 360.0f)
            angle -= 360.0f;

        return Mathf.Clamp(angle, minAngle, maxAngle);
    }

    private bool CompareLayer(int layerIndex, LayerMask layerMask)
    {
        return ((1 << layerIndex) & layerMask.value) != 0;
    }

    public float m_MaxDoubleTapGap = 0.25f;
    private float m_PrevTapTime = 0.0f;
    private bool m_IsHoldingMoveKeys = false;
    private bool m_IsDoubleTappedMoveKeys = false;

    private float GetTargetSpeed()
    {
        float targetSpeed = inputs.Jog ? jogSpeed : runSpeed;

        if (inputs.Move.sqrMagnitude > 0.0f)
        {
            if (!m_IsHoldingMoveKeys)
            {
                float timePassedSinceLastTap = Time.time - m_PrevTapTime;

                if (timePassedSinceLastTap <= m_MaxDoubleTapGap)
                {
                    m_IsDoubleTappedMoveKeys = true;
                }

                m_PrevTapTime = Time.time;
                m_IsHoldingMoveKeys = true;
            }
        }
        else
        {
            m_IsHoldingMoveKeys = false;
            m_IsDoubleTappedMoveKeys = false;
        }

        if (m_IsDoubleTappedMoveKeys)
            targetSpeed = sprintSpeed;

        if (inputs.Move == Vector2.zero)
            targetSpeed = 0.0f;

        return targetSpeed;
    }

    private float GetBowAndArrowTargetSpeed()
    {
        float targetSpeed = inputs.Jog ? bowWalkSpeed : bowRunSpeed;

        if (inputs.Move.sqrMagnitude > 0.0f)
        {
            if (!m_IsHoldingMoveKeys)
            {
                float timePassedSinceLastTap = Time.time - m_PrevTapTime;

                if (timePassedSinceLastTap <= m_MaxDoubleTapGap)
                {
                    m_IsDoubleTappedMoveKeys = true;
                }

                m_PrevTapTime = Time.time;
                m_IsHoldingMoveKeys = true;
            }
        }
        else
        {
            m_IsHoldingMoveKeys = false;
            m_IsDoubleTappedMoveKeys = false;
        }

        if (m_IsDoubleTappedMoveKeys)
            targetSpeed = bowSprintSpeed;

        if (inputs.Move == Vector2.zero)
            targetSpeed = 0.0f;

        return targetSpeed;
    }

    private float GetBowAndArrowAimingTargetSpeed()
    {
        float targetSpeed = 1.156f;
        Vector2 moveInput = inputs.Move;

        if (moveInput.x > 0.0f && moveInput.y > 0.0f)
            targetSpeed = (1.156f + 1.474f) * 0.5f;
        else if (moveInput.x < 0.0f && moveInput.y > 0.0f)
            targetSpeed = (1.156f + 1.49f) * 0.5f;
        else if (moveInput.x > 0.0f && moveInput.y < 0.0f)
            targetSpeed = (0.946f + 1.474f) * 0.5f;
        else if (moveInput.x < 0.0f && moveInput.y < 0.0f)
            targetSpeed = (0.946f + 1.49f) * 0.5f;
        else if (moveInput.x < 0.0f)
            targetSpeed = 1.49f;
        else if (moveInput.x > 0.0f)
            targetSpeed = 1.474f;
        else if (moveInput.y < 0.0f)
            targetSpeed = 0.946f;
        else if (moveInput.y > 0.0f)
            targetSpeed = 1.156f;

        if (inputs.Move == Vector2.zero)
            targetSpeed = 0.0f;

        return targetSpeed;
    }
}