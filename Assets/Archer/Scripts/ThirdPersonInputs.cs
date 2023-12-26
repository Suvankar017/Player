using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonInputs : MonoBehaviour
{
    public Vector2 Move;
    public Vector2 Look;
    public bool Jog;
    public bool Attack;
    public bool Aim;

    public void MoveInput(Vector2 move)
    {
        Move = move;
    }

    public void LookInput(Vector2 look)
    {
        Look = look;
    }

    public void JogInput(bool jog)
    {
        Jog = jog;
    }

    public void AttackInput(bool attack)
    {
        Attack = attack;
    }

    public void AimInput(bool aim)
    {
        Aim = aim;
    }

    private void OnMove(InputValue value)
    {
        MoveInput(value.Get<Vector2>());
    }

    private void OnLook(InputValue value)
    {
        LookInput(value.Get<Vector2>());
    }

    private void OnJog(InputValue value)
    {
        JogInput(value.isPressed);
    }

    private void OnAttack(InputValue value)
    {
        AttackInput(true);
    }

    private void OnAim(InputValue value)
    {
        AimInput(value.isPressed);
    }
}
