using UnityEngine;
using UnityEngine.AI;
using Fusion;

[RequireComponent(typeof(NavMeshAgent))]
public class ClickTooMove : NetworkBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float interpolationSpeed = 15f;
    public float rotationSpeed = 720f;

    [Header("Animation")]
    public Animator animator;

    private NavMeshAgent agent;

    private Vector3 targetPosition;
    [Networked] private bool IsMoving { get; set; }
    [Networked] private Vector3 NetworkedPosition { get; set; }
    [Networked] private Quaternion NetworkedRotation { get; set; }

    public override void Spawned()
    {
        agent = GetComponent<NavMeshAgent>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (HasStateAuthority)
        {
            // Authority controls the NavMeshAgent
            agent.updatePosition = false;
            agent.updateRotation = false;
            agent.speed = moveSpeed;

            NetworkedPosition = transform.position;
            NetworkedRotation = transform.rotation;
        }
        else
        {
            // Disable NavMeshAgent on remote clients - they just follow networked position
            agent.enabled = false;
        }
    }

    public override void FixedUpdateNetwork()
    {
        // Only State Authority moves the character
        if (HasStateAuthority)
        {
            if (GetInput(out NetworkInputData data))
            {
                if (data.isMoving)
                {
                    targetPosition = data.targetPosition;
                    IsMoving = true;

                    agent.SetDestination(targetPosition);
                }
            }

            MoveWithNavMesh();

            // Sync position and rotation to network
            NetworkedPosition = transform.position;
            NetworkedRotation = transform.rotation;
        }

        // Update animation for all clients
        UpdateAnimation();
    }

    public override void Render()
    {
        // Interpolate position and rotation for remote players (smooth visual update)
        if (!HasStateAuthority)
        {
            transform.position = Vector3.Lerp(
                transform.position,
                NetworkedPosition,
                interpolationSpeed * Time.deltaTime
            );

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                NetworkedRotation,
                rotationSpeed * Time.deltaTime
            );
        }
    }

    private void MoveWithNavMesh()
    {
        if (!IsMoving) return;

        if (agent.pathPending) return;

        // Calculate actual distance to target
        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);

        // Stop when close enough to target
        if (distanceToTarget <= agent.stoppingDistance + 0.1f)
        {
            IsMoving = false;
            agent.ResetPath();
            return;
        }

        // Sync agent position with transform (required when updatePosition is false)
        agent.nextPosition = transform.position;

        // Move using NavMeshAgent desired velocity
        if (agent.desiredVelocity.sqrMagnitude > 0.01f)
        {
            Vector3 move = agent.desiredVelocity.normalized * moveSpeed * Runner.DeltaTime;
            transform.position += move;

            Quaternion lookRotation = Quaternion.LookRotation(agent.desiredVelocity);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                lookRotation,
                10f * Runner.DeltaTime
            );
        }
        else
        {
            // No velocity means we've stopped or path is invalid
            IsMoving = false;
            agent.ResetPath();
        }
    }

    private void UpdateAnimation()
    {
        if (animator != null)
        {
            animator.SetBool("IsWalking", IsMoving);
        }
    }
}