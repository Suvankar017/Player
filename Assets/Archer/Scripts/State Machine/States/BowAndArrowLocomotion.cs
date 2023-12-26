using UnityEngine;

namespace ScriptableStateMachine
{
    public class BowAndArrowLocomotion : State
    {

        private readonly ErikaBlackboard m_Blackboard;

        public BowAndArrowLocomotion(ErikaBlackboard blackboard)
        {
            m_Blackboard = blackboard;
        }

        public override void OnEnter()
        {
            m_Blackboard.AbleToMove = false;
            m_Blackboard.Velocity = Vector3.zero;
            m_Blackboard.animator.SetBool("BowAndArrow", true);
            m_Blackboard.animator.SetFloat("MoveSpeed", 0.0f);
        }

        public override void OnUpdate()
        {
            m_Blackboard.GroundCheck();

            if (m_Blackboard.AbleToMove)
                m_Blackboard.BowAndArrowMovement();
        }

        public override void OnLateUpdate()
        {
            m_Blackboard.UpdateCamera();
        }

        public override void OnExit()
        {
            if (m_Blackboard.IsGrounded)
                m_Blackboard.animator.SetBool("BowAndArrow", false);
        }

        public override string ToString()
        {
            return GetType().Name;
        }

    }
}