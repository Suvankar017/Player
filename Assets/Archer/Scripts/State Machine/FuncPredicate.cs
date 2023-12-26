using System;

namespace ScriptableStateMachine
{
    public class FuncPredicate : IPredicate
    {
        private readonly Func<bool> m_Func = null;

        public FuncPredicate(Func<bool> func)
        {
            m_Func = func;
        }

        public bool Evaluate() => m_Func.Invoke();
    }
}