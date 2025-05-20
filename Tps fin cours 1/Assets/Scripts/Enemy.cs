using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class Enemy : MonoBehaviour
{
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float visionAngle = 90f;
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private float waitTime = 1f;
    [SerializeField] private float lostPlayerDelay = 2f;

    private int currentPatrolIndex = 0;
    private NavMeshAgent agent;
    private Transform player;
    private bool isChasing = false;
    private bool playerInTrigger = false;
    private bool canMove = true;
    private Coroutine visionCheckCoroutine;
    private Coroutine lostPlayerCoroutine;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        GoToNextPatrolPoint();
    }

    void Update()
    {
        if (!isChasing && !agent.pathPending && agent.remainingDistance < 0.5f && canMove)
        {
            GoToNextPatrolPoint();
        }

        if (isChasing && player != null)
        {
            agent.SetDestination(player.position);
        }
    }

    void GoToNextPatrolPoint()
    {
        if (patrolPoints.Length == 0)
            return;

        canMove = false;
        StartCoroutine(CanMoveCoroutine());
    }

    IEnumerator CanMoveCoroutine()
    {
        yield return new WaitForSeconds(waitTime);
        agent.destination = patrolPoints[currentPatrolIndex].position;
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        canMove = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && visionCheckCoroutine == null)
        {
            playerInTrigger = true;
            visionCheckCoroutine = StartCoroutine(PlayerInTrigger());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTrigger = false;
            if (visionCheckCoroutine != null)
            {
                StopCoroutine(visionCheckCoroutine);
                visionCheckCoroutine = null;
            }

            if (lostPlayerCoroutine != null)
            {
                StopCoroutine(lostPlayerCoroutine);
                lostPlayerCoroutine = null;
            }

            isChasing = false;
            GoToNextPatrolPoint();
        }
    }

    IEnumerator PlayerInTrigger()
    {
        while (playerInTrigger && player != null)
        {
            Vector3 dirToPlayer = (player.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, dirToPlayer);

            if (angle < visionAngle / 2f)
            {
                float distanceToPlayer = Vector3.Distance(transform.position, player.position);
                Vector3 origin = transform.position + Vector3.up * 1.5f;

                if (!Physics.Raycast(origin, dirToPlayer, distanceToPlayer, obstacleMask))
                {
                    if (!isChasing)
                        isChasing = true;

                    if (lostPlayerCoroutine != null)
                    {
                        StopCoroutine(lostPlayerCoroutine);
                        lostPlayerCoroutine = null;
                    }
                }
                else
                {
                    TryLosePlayer();
                }
            }
            else
            {
                TryLosePlayer();
            }

            yield return null;
        }
    }

    void TryLosePlayer()
    {
        if (isChasing && lostPlayerCoroutine == null)
        {
            lostPlayerCoroutine = StartCoroutine(LosePlayerAfterDelay());
        }
    }

    IEnumerator LosePlayerAfterDelay()
    {
        yield return new WaitForSeconds(lostPlayerDelay);
        isChasing = false;
        GoToNextPatrolPoint();
        lostPlayerCoroutine = null;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
