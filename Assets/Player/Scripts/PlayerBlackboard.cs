using System;
using UnityEngine;

public class PlayerBlackboard : MonoBehaviour
{
    [Header("Movement")]
    public float jogSpeed = 4.469f;
    public float runSpeed = 5.668f;
    public float acceleration = 10.0f;
    public float angularSpeed = 120.0f;
    public CharacterController characterController;
    public Animator animator;
    public ThirdPersonInputs inputs;
}