using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
  public float damageToPlayer = 20f;
  public float enemyHealth = 100f;
  public float attackRadius = 1f;
  public float attackCooldown = 1f;
  public float detectionRadius = 10f;
  public float fieldOfView = 60f;
  public Transform[] waypoints;
  public AudioClip detectSound;
  public Rigidbody gfxRigidBody;

  public Rigidbody miniEnemy;
  public AudioSource backgroundMusic;

  private NavMeshAgent agent;
  private Transform player;
  private Vector3 lastKnownPlayerPosition;
  private bool playerInSight = false;

  private bool waiting = false;
  private float waitTime = 5f;
  private float waitTimer = 0f;

  private int currentWaypointIndex = 0;
  private AudioSource audioSource;
  private bool isRagdoll = false;

  private bool playerIsAttacked = false;

  private void Start()
  {
    agent = GetComponent<NavMeshAgent>();
    player = GameObject.FindGameObjectWithTag("Player").transform;
    audioSource = GetComponent<AudioSource>();
  }

  private void Update()
  {
    if (enemyHealth <= 0)
    {
      Destroy(gameObject);
    }

    if (waiting)
    {
      waitTimer += Time.deltaTime;

      // Girar o inimigo enquanto espera
      transform.Rotate(transform.up, 90f * Time.deltaTime);

      if (waitTimer >= waitTime)
      {
        waiting = false;
        waitTimer = 0f;
        SetNextWaypoint();
      }
    }

    if (playerInSight && !isRagdoll)
    {
      waiting = false;
      lastKnownPlayerPosition = player.position;
      agent.SetDestination(lastKnownPlayerPosition);
    }
    else
    {
      if (agent.remainingDistance < 0.5f)
      {
        if (lastKnownPlayerPosition != Vector3.zero)
        {
          waiting = true;
        }
        else
        {
          SetNextWaypoint();
        }
      }
    }

    if (CanSeePlayer())
    {
      playerInSight = true;
      backgroundMusic.volume = 1;

      // Reproduzir som de detecção
      if (detectSound != null && !audioSource.isPlaying)
      {
        audioSource.clip = detectSound;
        audioSource.Play();
      }
    }
    else
    {
        playerInSight = false;
        backgroundMusic.volume = 0.3f;
    }
  }

  private void FixedUpdate()
  {
    VerifyPlayerInRange();
  }

  private void SetNextWaypoint()
  {
    currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
    Vector3 nextWaypoint = waypoints[currentWaypointIndex].position;
    if (!isRagdoll) agent.SetDestination(nextWaypoint);
    lastKnownPlayerPosition = Vector3.zero;
  }

  private bool CanSeePlayer()
  {
    Vector3 directionToPlayer = player.position - transform.position;

    // Alteração para permitir detecção do jogador no ar
    float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
    float heightDiff = Mathf.Abs(player.position.y - transform.position.y);
    if (heightDiff > 1f)
    {
      angleToPlayer *= Mathf.Lerp(1f, 0.5f, heightDiff / detectionRadius);
    }

    if (directionToPlayer.magnitude < detectionRadius && angleToPlayer < fieldOfView / 2)
    {
      RaycastHit hit;
      if (Physics.Raycast(transform.position, directionToPlayer, out hit))
      {
        if (hit.collider.gameObject.tag == "Player")
        {
          return true;
        }
      }
    }
    return false;
  }

  public void TakeDamage(float damage)
  {
    enemyHealth -= damage;
    agent.enabled = false;
    isRagdoll = true;

    if (enemyHealth <= 0)
    {
      Die();
    }

    Invoke("EnableAgent", 5f);
  }

  public void Die()
  {
    for (int i = 0; i < 50; i++)
    {
      Rigidbody miniEnemyRb = Instantiate(miniEnemy, transform.position, Quaternion.LookRotation(transform.forward, Vector3.up));
      miniEnemyRb.gameObject.SetActive(true);
    }
    Destroy(gameObject);
  }

  private void VerifyPlayerInRange()
  {
    Collider[] colliders = Physics.OverlapSphere(transform.position, attackRadius);
    foreach (Collider collider in colliders)
    {
      if (collider.gameObject.CompareTag("Player") && !playerIsAttacked)
      {
        AttackPlayer();
      }
    }
  }

  public void AttackPlayer()
  {
    GlobalState.Instance.playerHealth -= damageToPlayer;
    playerIsAttacked = true;

    if (GlobalState.Instance.playerHealth <= 0)
    {
      GlobalState.Instance.DestroyPlayer();
    } 

    Invoke("ResetCoolDown", attackCooldown);
  }

  private void ResetCoolDown()
  {
    playerIsAttacked = false;
  }

  private void EnableAgent()
  {
    agent.enabled = true;
    isRagdoll = false;
  }

  private void OnDrawGizmosSelected()
  {
    Gizmos.color = Color.yellow;
    Gizmos.DrawWireSphere(transform.position, detectionRadius);

    Gizmos.color = Color.red;
    Gizmos.DrawWireSphere(transform.position, attackRadius);

    Vector3 fovLine1 = Quaternion.AngleAxis(fieldOfView / 2, transform.up) * transform.forward * detectionRadius;
    Vector3 fovLine2 = Quaternion.AngleAxis(-fieldOfView / 2, transform.up) * transform.forward * detectionRadius;

    Gizmos.color = Color.blue;
    Gizmos.DrawRay(transform.position, fovLine1);
    Gizmos.DrawRay(transform.position, fovLine2);
  }
}
