using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ErikaAnimatorEventHandler : MonoBehaviour
{
    public ErikaBlackboard blackboard;

    public void CanMove(int ableToMove)
    {
        blackboard.AbleToMove = ableToMove > 0;
        if (ableToMove > 0) blackboard.normalCam = true;
    }

    public void JumpCamera(int activate)
    {
        blackboard.JumpCamera(activate > 0);
    }

    public void OnFootTouchGround(int foot)
    {
        blackboard.OnFootTouchGround(foot);
    }
}
