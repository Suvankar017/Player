using System;
using System.Collections.Generic;

namespace ScriptableStateMachine
{
    public class StateMachine
    {
        private class StateNode
        {
            public IState State { get; }
            public HashSet<ITransition> Transitions { get; }

            public StateNode(IState state)
            {
                State = state;
                Transitions = new HashSet<ITransition>();
            }

            public void AddTransition(IState to, IPredicate condition)
            {
                Transitions.Add(new Transition(to, condition));
            }

            public void AddTransition(ITransition transition)
            {
                Transitions.Add(transition);
            }
        }

        public event Action<IState, IState> OnStateChanged;

        private StateNode m_Current;
        private Dictionary<Type, StateNode> m_Nodes;
        private HashSet<ITransition> m_AnyTransitions;

        public StateMachine()
        {
            m_Current = null;
            m_Nodes = new Dictionary<Type, StateNode>();
            m_AnyTransitions = new HashSet<ITransition>();
        }

        public IState GetActiveState()
        {
            return m_Current.State;
        }

        public void Update()
        {
            if (TryGetTransition(out ITransition transition))
            {
                OnStateChanged?.Invoke(m_Current.State, transition.To);
                ChangeState(transition.To);
            }

            m_Current.State?.OnUpdate();
        }

        public void FixedUpdate()
        {
            m_Current.State?.OnFixedUpdate();
        }

        public void LateUpdate()
        {
            m_Current.State?.OnLateUpdate();
        }

        public void DrawGizmos()
        {
            m_Current.State?.OnDrawGizmos();
        }

        public void SetState(IState state)
        {
            //m_Current = m_Nodes[state.GetType()];
            m_Current = GetOrAddNode(state);
            m_Current.State.OnEnter();
        }

        public void ChangeState(IState state)
        {
            if (m_Current.State == state)
                return;

            m_Current.State?.OnExit();
            m_Current = m_Nodes[state.GetType()];
            m_Current.State?.OnEnter();
        }

        public void AddTransition(IState from, IState to, IPredicate condition)
        {
            GetOrAddNode(from).AddTransition(GetOrAddNode(to).State, condition);
        }

        public void AddAnyTransition(IState to, IPredicate condition)
        {
            m_AnyTransitions.Add(new Transition(GetOrAddNode(to).State, condition));
        }

        private StateNode GetOrAddNode(IState state)
        {
            if (m_Nodes.TryGetValue(state.GetType(), out StateNode node))
                return node;

            StateNode n = new StateNode(state);
            m_Nodes.Add(state.GetType(), n);
            return n;
        }

        private bool TryGetTransition(out ITransition transition)
        {
            foreach (ITransition t in m_AnyTransitions)
            {
                if (t.Condition.Evaluate())
                {
                    transition = t;
                    return true;
                }
            }

            foreach (ITransition t in m_Current.Transitions)
            {
                if (t.Condition.Evaluate())
                {
                    transition = t;
                    return true;
                }
            }

            transition = null;
            return false;
        }
    }
}