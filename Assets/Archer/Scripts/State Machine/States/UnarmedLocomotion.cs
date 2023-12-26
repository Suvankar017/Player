using UnityEngine;

namespace ScriptableStateMachine
{
    public class UnarmedLocomotion : State
    {
        private readonly ErikaBlackboard m_Blackboard;

        public UnarmedLocomotion(ErikaBlackboard blackboard)
        {
            m_Blackboard = blackboard;
        }

        public override void OnEnter()
        {
            m_Blackboard.AbleToMove = false;
            m_Blackboard.animator.SetFloat("MoveSpeed", 0.0f);
        }

        public override void OnUpdate()
        {
            m_Blackboard.GroundCheck();

            if (m_Blackboard.AbleToMove)
                m_Blackboard.Movement();
        }

        public override void OnLateUpdate()
        {
            m_Blackboard.UpdateCamera();
        }

        public override void OnExit()
        {

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
}