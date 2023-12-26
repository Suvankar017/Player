using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonController : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private float m_JogSpeed = 1.0f;
    [SerializeField] private float m_RunSpeed = 3.0f;
    [SerializeField] private float m_Accleration = 10.0f;
    [SerializeField] private float m_RotationSmoothTime = 0.2f;
    [SerializeField] private Animator m_Animator = null;

    [Header("Camera")]
    [SerializeField] private float m_CameraSensitivity = 5.0f;
    [SerializeField] private float m_TopMaxAngle = 70.0f;
    [SerializeField] private float m_BottomMaxAngle = -30.0f;
    [SerializeField] private Transform m_CameraFollowTarget = null;
    [SerializeField] private Cinemachine.CinemachineVirtualCamera m_IdleCamera = null;
    [SerializeField] private Cinemachine.CinemachineVirtualCamera m_JogCamera = null;
    [SerializeField] private Cinemachine.CinemachineVirtualCamera m_JogCloseCamera = null;

    private float m_CurrMoveSpeed = 0.0f;
    private float m_CameraXRot = 0.0f;
    private float m_CameraYRot = 0.0f;
    private float m_TargetRotation = 0.0f;
    private float m_RotationVelocity = 0.0f;
    private Transform m_Transform = null;
    private Transform m_CameraTransform = null;
    private ThirdPersonInputs m_Input = null;
    private CharacterController m_Controller = null;
    

    private void Awake()
    {
        m_Input = GetComponent<ThirdPersonInputs>();
        m_Controller = GetComponent<CharacterController>();
    }

    private void Start()
    {
        m_Transform = transform;
        m_CameraTransform = Camera.main.transform;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            m_IdleCamera.Priority = 20;
            m_JogCamera.Priority = 10;
            m_JogCloseCamera.Priority = 10;
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            m_IdleCamera.Priority = 10;
            m_JogCamera.Priority = 20;
            m_JogCloseCamera.Priority = 10;
        }

        UpdateMovement();
    }

    private void LateUpdate()
    {
        UpdateCameraRotation();
    }

    private void UpdateCameraRotation()
    {
        Vector2 look = m_Input.Look;

        if (look.sqrMagnitude >= 0.0001f)
        {
            m_CameraYRot += look.x * m_CameraSensitivity;
            m_CameraXRot += look.y * m_CameraSensitivity;
        }

        m_CameraYRot = ClampAngle(m_CameraYRot, float.MinValue, float.MaxValue);
        m_CameraXRot = ClampAngle(m_CameraXRot, m_BottomMaxAngle, m_TopMaxAngle);

        if (m_CameraXRot < m_BottomMaxAngle + 10.0f)
        {
            m_JogCloseCamera.Priority = 20;
            m_IdleCamera.Priority = 10;
            m_JogCamera.Priority = 10;
        }
        else
        {
            m_IdleCamera.Priority = 10;
            m_JogCamera.Priority = 20;
            m_JogCloseCamera.Priority = 10;
        }

        m_CameraFollowTarget.rotation = Quaternion.Euler(m_CameraXRot, m_CameraYRot, 0.0f);
    }

    private void UpdateMovement()
    {
        bool sprint = m_Input.Jog;
        Vector2 move = m_Input.Move;

        float targetSpeed = sprint ? m_RunSpeed : m_JogSpeed;
        if (move == Vector2.zero) targetSpeed = 0.0f;

        float speedOffset = 0.1f;
        float currHorizontalSpeed = new Vector3(m_Controller.velocity.x, 0.0f, m_Controller.velocity.z).magnitude;

        if (currHorizontalSpeed < targetSpeed - speedOffset || currHorizontalSpeed > targetSpeed + speedOffset)
        {
            m_CurrMoveSpeed = Mathf.Lerp(currHorizontalSpeed, targetSpeed, m_Accleration * Time.deltaTime);
            m_CurrMoveSpeed = Mathf.Round(m_CurrMoveSpeed * 1000.0f) * 0.001f;
        }
        else
        {
            m_CurrMoveSpeed = targetSpeed;
        }

        if (move != Vector2.zero)
        {
            Vector2 moveDir = move.normalized;
            
            m_TargetRotation = Mathf.Atan2(moveDir.x, moveDir.y) * Mathf.Rad2Deg + m_CameraTransform.eulerAngles.y;
            float yRot = Mathf.SmoothDampAngle(m_Transform.eulerAngles.y, m_TargetRotation, ref m_RotationVelocity, m_RotationSmoothTime);

            m_Transform.rotation = Quaternion.Euler(0.0f, yRot, 0.0f);
        }

        Vector3 targetDir = (Quaternion.Euler(0.0f, m_Transform.eulerAngles.y, 0.0f) * Vector3.forward).normalized;

        m_Controller.Move(targetDir * m_CurrMoveSpeed * Time.deltaTime);

        m_Animator.SetFloat("MoveSpeed", m_CurrMoveSpeed);
    }

    private float ClampAngle(float currAngle, float minAngle, float maxAngle)
    {
        if (currAngle < -360.0f)
            currAngle += 360.0f;

        if (currAngle > 360.0f)
            currAngle -= 360.0f;

        return Mathf.Clamp(currAngle, minAngle, maxAngle);
    }
}
