using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIController : MonoBehaviour
{
    public Transform Target;
    public Animator Anim;
    public float SphereSize = 0.1f;
    public float Distance = 100.0f;
    public float TargetSpeed = 5.0f;
    public float Acceleration = 10.0f;
    public float DistanceCover = 0.0f;

    [SerializeField] private float m_CurrentSpeed = 0.0f;
    [SerializeField] private float m_AccelerationTime = 0.0f;
    [SerializeField] private float m_TimeTakenToAccelerate = 0.0f;
    [SerializeField] private float m_CurrentPosition = 0.0f;

    private Vector3 m_LastPlayerPos;
    private List<Vector3> m_Points = new List<Vector3>();
    private NavMeshAgent m_Agent;
    [SerializeField] private bool m_StartSimulation = false;

    private void Awake()
    {
        m_Agent = GetComponent<NavMeshAgent>();
        m_Points.Add(new Vector3(0.0f, 0.0f));
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            m_StartSimulation = true;
            m_AccelerationTime = (TargetSpeed - m_CurrentSpeed) / Acceleration;
            DistanceCover = (TargetSpeed * TargetSpeed - m_CurrentSpeed * m_CurrentSpeed) / (2.0f * Acceleration);
        }

        if (!m_StartSimulation)
            return;

        float dt = Time.deltaTime;
        //m_CurrentSpeed = Mathf.Lerp(m_CurrentSpeed, TargetSpeed, Acceleration * dt);
        m_CurrentSpeed = m_CurrentSpeed + Acceleration * dt;
        m_CurrentPosition = m_CurrentPosition + m_CurrentSpeed * dt;
        m_TimeTakenToAccelerate += dt;

        //m_Points.Add(new Vector3(m_TimeTakenToAccelerate, m_CurrentPosition));

        if (Mathf.Approximately(m_CurrentSpeed, TargetSpeed) || m_CurrentSpeed > TargetSpeed)
            m_StartSimulation = false;
    }

    private void OnDrawGizmos()
    {
        if (m_Points.Count == 0)
            return;

        for (int i = 0; i < m_Points.Count - 1; i++)
        {
            Gizmos.color = Color.yellow * 0.5f;
            Gizmos.DrawSphere(m_Points[i], SphereSize);
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(m_Points[i], m_Points[i + 1]);
        }

        Gizmos.color = Color.yellow * 0.5f;
        Gizmos.DrawSphere(m_Points[m_Points.Count - 1], SphereSize);
    }

    private void UpdateAI()
    {
        if (m_LastPlayerPos != Target.position)
        {
            m_LastPlayerPos = Target.position;
            m_Agent.SetDestination(m_LastPlayerPos);

            m_Points.Clear();
            m_Points.AddRange(m_Agent.path.corners);
        }

        Anim.SetFloat("MoveSpeed", m_Agent.velocity.magnitude);
    }
}
