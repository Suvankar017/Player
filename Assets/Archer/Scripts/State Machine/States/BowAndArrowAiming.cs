using UnityEngine;
using ScriptableStateMachine;

public class BowAndArrowAiming : State
{
    private readonly ErikaBlackboard m_Blackboard;

    public BowAndArrowAiming(ErikaBlackboard blackboard)
    {
        m_Blackboard = blackboard;
    }

    public override void OnEnter()
    {
        m_Blackboard.animator.SetBool("BowAndArrow", true);
        m_Blackboard.animator.SetBool("Aiming", true);
    }

    public override void OnUpdate()
    {
        m_Blackboard.GroundCheck();
        m_Blackboard.BowAndArrowAimingMovement();
    }

    public override void OnLateUpdate()
    {
        m_Blackboard.BowAndArrowCameraMovement();
    }

    public override void OnExit()
    {
        m_Blackboard.animator.SetBool("Aiming", false);
    }

    public override string ToString()
    {
        return GetType().Name;
    }
}