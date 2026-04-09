using Fusion;
using UnityEngine;

public class ClickToMove : NetworkBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float stoppingDistance = 0.1f;
    public float rotationSpeed = 10f;

    [Header("Animation")]
    public Animator animator;

    [Networked] private Vector3 TargetPosition { get; set; }
    [Networked] private bool IsMoving { get; set; }

    public override void Spawned()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    public override void FixedUpdateNetwork()
    {
        if (HasStateAuthority)
        {
            if (GetInput(out NetworkInputData data))
            {
                if (data.isMoving)
                {
                    TargetPosition = data.targetPosition;
                    IsMoving = true;
                }
            }

            if (IsMoving)
            {
                Vector3 direction = TargetPosition - transform.position;
                direction.y = 0;

                if (direction.magnitude <= stoppingDistance)
                {
                    IsMoving = false;
                }
                else
                {
                    // Rotate towards target
                    if (direction != Vector3.zero)
                    {
                        Quaternion targetRotation = Quaternion.LookRotation(direction);
                        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Runner.DeltaTime);
                    }

                    // Move towards target
                    Vector3 move = direction.normalized * moveSpeed * Runner.DeltaTime;
                    transform.position += move;
                }
            }
        }

        // Update animation for all clients
        UpdateAnimation();
    }

    private void UpdateAnimation()
    {
        if (animator != null)
        {
            animator.SetBool("IsWalking", IsMoving);
        }
    }
}