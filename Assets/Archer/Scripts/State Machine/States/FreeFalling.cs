using UnityEngine;
using ScriptableStateMachine;

public class FreeFalling : State
{
    private readonly ErikaBlackboard m_Blackboard;

    public FreeFalling(ErikaBlackboard blackboard)
    {
        m_Blackboard = blackboard;
    }

    public override void OnEnter()
    {
        m_Blackboard.AbleToMove = false;
        m_Blackboard.animator.SetTrigger("FreeFall");
    }

    public override void OnUpdate()
    {
        m_Blackboard.GroundCheck();
        m_Blackboard.Falling();
    }

    public override void OnExit()
    {
        m_Blackboard.Velocity = new Vector3(0.0f, 0.0f, 0.0f);
        m_Blackboard.animator.SetFloat("MoveSpeed", 0.0f);
    }

    public override void OnDrawGizmos()
    {
        Gizmos.color = (m_Blackboard.IsGrounded ? Color.green : Color.red) * 0.5f;
        Gizmos.DrawSphere(m_Blackboard.playerTransform.position + m_Blackboard.groundCheckOffset, m_Blackboard.groundCheckRadius);
    }

    public override string ToString()
    {
        return GetType().Name;
    }
}