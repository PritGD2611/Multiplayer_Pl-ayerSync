using Fusion;
using UnityEngine;

public class PlayerSync1 : NetworkBehaviour
{
    [Networked] private Vector3 SyncPos { get; set; }
    [Networked] private Quaternion SyncRot { get; set; }

    public float moveSpeed = 10f;
    public float rotSpeed = 360f;

   public override void Spawned()
    {
        if (!HasStateAuthority)
        {
            transform.position = SyncPos;
            transform.rotation = SyncRot;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (HasStateAuthority)
        {
            SyncPos = transform.position;
            SyncRot = transform.rotation;
        }
        else
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                SyncPos,
                moveSpeed * Runner.DeltaTime
            );

            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                SyncRot,
                rotSpeed * Runner.DeltaTime
            );
        }
    }
}


