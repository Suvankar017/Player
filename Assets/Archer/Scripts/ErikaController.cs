using UnityEngine;
using ScriptableStateMachine;

public class ErikaController : MonoBehaviour
{

    [SerializeField] private ErikaBlackboard m_Blackboard;

    [Header("Debugs")]
    [SerializeField] private string m_ActiveStateName;

    private StateMachine m_Machine;
    private IState m_PrevState;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Create state machine
        m_Machine = new StateMachine();

        // Declare states
        var unarmedLocomotion = new UnarmedLocomotion(m_Blackboard);
        var freeFalling = new FreeFalling(m_Blackboard);
        var bowAndArrowLocomotion = new BowAndArrowLocomotion(m_Blackboard);
        var bowAndArrowAiming = new BowAndArrowAiming(m_Blackboard);

        // Define transition
        At(unarmedLocomotion, bowAndArrowLocomotion, () => Input.GetKeyDown(KeyCode.E));

        At(bowAndArrowLocomotion, unarmedLocomotion, () => Input.GetKeyDown(KeyCode.Q));
        At(bowAndArrowLocomotion, bowAndArrowAiming, () => m_Blackboard.inputs.Aim);

        At(bowAndArrowAiming, bowAndArrowLocomotion, () => !m_Blackboard.inputs.Aim);

        Any(freeFalling, () => !m_Blackboard.IsGrounded);
        At(freeFalling, unarmedLocomotion, () => m_Blackboard.IsGrounded && m_PrevState == unarmedLocomotion);
        At(freeFalling, bowAndArrowLocomotion, () => m_Blackboard.IsGrounded && m_PrevState == bowAndArrowLocomotion);

        // Set current state
        m_Machine.SetState(unarmedLocomotion);
        m_PrevState = unarmedLocomotion;
    }

    private void Update()
    {
        // Update state machine
        m_Machine.Update();
        m_ActiveStateName = m_Machine.GetActiveState().ToString();
    }

    private void FixedUpdate()
    {
        // FixedUpdate state machine
        m_Machine.FixedUpdate();
    }

    private void LateUpdate()
    {
        // LateUpdate state machine
        m_Machine.LateUpdate();
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
            return;

        m_Machine.DrawGizmos();
    }

    private void At(IState from, IState to, System.Func<bool> condition)
    {
        m_Machine.AddTransition(from, to, new FuncPredicate(condition));
    }

    private void Any(IState to, System.Func<bool> condition)
    {
        m_Machine.AddAnyTransition(to, new FuncPredicate(condition));
    }

    private void OnStateChanged(IState current, IState next)
    {
        m_PrevState = current;
    }
}
