using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class Enemy : MonoBehaviour
{
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float visionAngle = 90f;
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private float waitTime;

    private int currentPatrolIndex = 0;
    private NavMeshAgent agent;
    private Transform player;
    private bool isChasing = false;
    private bool playerInTrigger = false;
    private bool canMove = true;

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
        if (other.CompareTag("Player"))
        {
            playerInTrigger = true;
            StartCoroutine(PlayerInTrigger());
        }
        
    }

    IEnumerator PlayerInTrigger()
    {
        while (playerInTrigger)
        {
            Vector3 dirToPlayer = (player.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, dirToPlayer);

            if (angle < visionAngle / 2f)
            {
                float distanceToPlayer = Vector3.Distance(transform.position, player.position);
                
                if (!Physics.Raycast(transform.position + Vector3.up, dirToPlayer, distanceToPlayer, obstacleMask))
                {
                    isChasing = true;
                }
            }
            yield return null;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTrigger = false;
            isChasing = false;
            GoToNextPatrolPoint();
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}