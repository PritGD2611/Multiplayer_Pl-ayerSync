using UnityEngine;
using Fusion;
public class Script : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Animator animator;
    public AudioSource walkAudio;   // Drag AudioSource here

    private Vector3 targetPosition;
    private bool isMoving = false;

    void FixedUpdate()
    {
        HandleMouseClick();
        MovePlayer();
        HandleWalkingSound();
    }

    void HandleMouseClick()
    {
        if (Input.GetMouseButtonDown(0)) // Left click
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.CompareTag("Ground") || hit.collider.gameObject.name == "Ground")
                {
                    targetPosition = hit.point;
                    targetPosition.y = transform.position.y; // keep same height
                    isMoving = true;
                    animator.SetBool("isWalking", true);
                }
            }
        }
    }

    void MovePlayer()
    {
        if (!isMoving)
            return;

        float step = moveSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);

        // Rotate toward movement direction
        Vector3 direction = targetPosition - transform.position;
        if (direction != Vector3.zero)
        {
            Quaternion rot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 10f);
        }

        // Check if reached destination
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            isMoving = false;
            animator.SetBool("isWalking", false);
        }
    }

    void HandleWalkingSound()
    {
        if (isMoving)
        {
            if (!walkAudio.isPlaying)
                walkAudio.Play();
        }
        else
        {
            if (walkAudio.isPlaying)
                walkAudio.Stop();
        }
    }
}
