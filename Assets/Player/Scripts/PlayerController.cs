using UnityEngine;
using ScriptableStateMachine;

public class PlayerController : MonoBehaviour
{
    public PlayerBlackboard blackboard;

    private StateMachine m_StateMachine;

    private void Start()
    {
        // Create state machine
        m_StateMachine = new StateMachine();
        /*
        // Declare states
        var unarmedLocomotion = new UnarmedLocomotion(blackboard);
        var bowAndArrowLocomotion = new BowAndArrowLocomotion();

        // Define transition
        At(unarmedLocomotion, bowAndArrowLocomotion, () => Input.GetKeyDown(KeyCode.E));

        // Set current state
        m_StateMachine.SetState(unarmedLocomotion);
        */
    }

    private void Update()
    {
        // Update state machine
        m_StateMachine.Update();
    }

    private void FixedUpdate()
    {
        // FixedUpdate state machine
        m_StateMachine.FixedUpdate();
    }

    private void LateUpdate()
    {
        // LateUpdate state machine
        m_StateMachine.LateUpdate();
    }

    private void At(IState from, IState to, System.Func<bool> condition)
    {
        m_StateMachine.AddTransition(from, to, new FuncPredicate(condition));
    }

    private void Any(IState to, System.Func<bool> condition)
    {
        m_StateMachine.AddAnyTransition(to, new FuncPredicate(condition));
    }
}