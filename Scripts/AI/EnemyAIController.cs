using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;
using System.Collections.Generic;
using UnityEngine.Serialization;

public class EnemyAIController : MonoBehaviour
{
   private NavMeshAgent agent;
   [SerializeField] private Transform[] waypoints;
   private Transform lastWaypoint;

   // target data
   private Transform playerT;
   private Vector3 lastKnownPos;
   private Vector3 lastPlayerDir;

   //timers
   private float minWaitTime = 2f;
   private float maxWaitTime = 10f;
   private float currentWaitTimer;
   private float waitTimer;

   private float loseSightTime = 1f;
   private float loseSightTimer;

   private int searchTargetsAmount = 4;
   private int searchTargetsReached;
   private List<Vector3> searchTargets = new List<Vector3>();

   private float idleTime = 20f;
   private float idleTimer;
   
   //states
   private enum  State
   {
      Patrol,
      Chase,
      Search,
      Idle,
   }
   private State currentState;
   
   //Detection layers
   [SerializeField] private LayerMask obstacleLayerMask;
   [SerializeField] private LayerMask playerLayerMask;
   
   //vision settings
   [SerializeField] private float visionRadius = 20f;
   private float fov = 160;
   private float closeRangeViewDistance = 3f;
   private float visionHeight = 1;

   //agent speed
   [FormerlySerializedAs("patrolSpeed")]
   [SerializeField] private float walkSpeed = 2f;
   [FormerlySerializedAs("chaseSpeed")]
   [SerializeField] private float runSpeed = 4f;

   private enum MovementType
   {
      Walk,
      Run,
   }
   private MovementType currentMovementType;

   public float CurrentMaxSpeed
   {
      get
      {
         if (!agent)
            return 0f;
         return agent.speed;
      }
   }
   public float CurrentSpeed
   {
      get
      {
         if(!agent) return 0;
         Vector3 v = agent.velocity;
         v.y = 0;
         return v.magnitude;
      }
   }
   public bool IsRunning => currentMovementType == MovementType.Run;

   private float searchRadius = 20f;
   
   bool isTargetVisible;

   private NavMeshPath path;

   [SerializeField] private GameObject disappearParticle;
   [SerializeField] private EnemyDamage enemyDamage;

   private Vector3 spawnPos;
   #region unity lifecycle
   private void Start()
   {
      spawnPos = gameObject.transform.position;
      path = new NavMeshPath();
      if (!TryGetComponent(out agent))
      {
         Debug.LogError("No NavMeshAgent found!");
         return;
      }
      
      //set initial state as patrol
      SetState(State.Patrol, true);
   }
   private void Update()
   {
      HandleVision();
      switch (currentState)
      {
         case State.Patrol: Patrol(); break;
         case State.Chase: Chase(); break;
         case State.Search: Search(); break;
         case State.Idle: Idle(); break;
      }
   }
   #endregion
   #region State handling
   private void Patrol()
   {
      // Random waypoint movement
      if (isTargetVisible)
      {
         SetState(State.Chase);
         waitTimer = 0;
         currentWaitTimer = 0;
         return;
      }
      if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
      {
         if(currentWaitTimer == 0)
            currentWaitTimer = Random.Range(minWaitTime, maxWaitTime);
         waitTimer += Time.deltaTime;
         
         if (waitTimer >= currentWaitTimer)
         {
            SetNextDestination();
            waitTimer = 0;
            currentWaitTimer = 0;
         }
      }
   }
   
   private void Chase()
   {
      // Continuously updates destination to player, switch to search after loseSightTime if player is lost
      
      if (playerT == null) return;
      if (!isTargetVisible)
      {
         //do chase period after sight is lost.
         if (loseSightTimer <= loseSightTime)
         {
            loseSightTimer += Time.deltaTime;
         }
         else
         {
            loseSightTimer = 0;
            playerT = null;
            SetState(State.Search);
            return;
         }
      } else if (isTargetVisible && loseSightTimer > 0)
         loseSightTimer = 0;

      //update lastPlayerDir and lastKnownPos
      Vector3 dir = playerT.position - lastKnownPos;
      dir.y = 0;
      if (dir.magnitude > 0.1f)
      {
         lastPlayerDir = dir.normalized;
         lastKnownPos = playerT.position;
      }

      if(Vector3.Distance(agent.destination, playerT.position) > 0.1f)
         agent.SetDestination(playerT.position);
   }
   private void Search()
   {
      // Sets random targets inside a search radius
      
      if (isTargetVisible)
      {
         SetState(State.Chase);
         searchTargetsReached = 0;
         searchTargets.Clear();
         return;
      }

      //Create targets
      if(searchTargets.Count <= 0)
         CreateSearchTargets();
      
      //arrived + set destination logic
      if (searchTargetsReached < searchTargets.Count)
      {
         if (Vector3.Distance(agent.destination, searchTargets[searchTargetsReached]) > 0.1f)
         {
            //calculate reachable path, skip if unreachable
            agent.CalculatePath(searchTargets[searchTargetsReached], path);
            if (path.status == NavMeshPathStatus.PathComplete)
            {
               agent.SetDestination(searchTargets[searchTargetsReached]);
            }
            else
            {
               searchTargetsReached++;
               return;
            }
         }

         bool arrived = !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance;
         if (arrived)
         {
            searchTargetsReached++;
         }
      }
      else //switch back to patrol
      {
         searchTargets.Clear();
         searchTargetsReached = 0;
         SetState(State.Patrol);
      }
   }
   private void Idle()
   {
      if (idleTimer >= idleTime)
      {
         idleTimer = 0;
         if(enemyDamage != null) enemyDamage.CanDamage = true;
         SetState(isTargetVisible ? State.Chase : State.Patrol);
         return;
      }
      if (enemyDamage != null && enemyDamage.CanDamage) enemyDamage.CanDamage = false;
      idleTimer += Time.deltaTime;
   }
   #endregion
   #region Helper methods
   private void CreateSearchTargets()
   {
      //create targets
      
      //check forward first, check back to last known pos after
      Vector3 forwardDir = (lastPlayerDir.magnitude > 0.01f ? lastPlayerDir : transform.forward);
      searchTargets.Add(lastKnownPos + forwardDir * (searchRadius * 0.7f));
      searchTargets.Add(lastKnownPos);
         
      for (int i = 0; i < searchTargetsAmount; i++)
      {
         //Random search targets inside radius
         Vector3 newTarget;
         bool success;
         int maxAttempts = 5;
         int attempts = 0;
         do
         {
            //target calculation, forward bias, random offset
            Vector3 forwardBias = forwardDir * (searchRadius / 2);
            float radius = searchRadius * (1 + ((i) * 0.15f));
            Vector3 randomOffset = Random.insideUnitSphere * radius;
            randomOffset.y = 0;
            newTarget = lastKnownPos + forwardBias + randomOffset;
               
            success = NavMesh.SamplePosition(newTarget, out NavMeshHit hit, 5f, NavMesh.AllAreas);
            if(success) 
               newTarget = hit.position;
               
            attempts++;
            if(attempts >= maxAttempts) break;
               
         } while (!success);
            
         if(success && !searchTargets.Exists(p => Vector3.Distance(p, newTarget) < 2f))
            searchTargets.Add(newTarget);
      }
      
   }
   private void SetNextDestination()
   {
      if (waypoints == null || waypoints.Length == 0)
      {
         Debug.LogError("No waypoints assigned!");
         return;
      }
      
      Transform nextWaypoint;
      do
      {
         nextWaypoint = GetRandomWaypoint();
      } while (nextWaypoint == lastWaypoint);
            
      lastWaypoint = nextWaypoint;
      agent.SetDestination(nextWaypoint.position);
   }
   private Transform GetRandomWaypoint()
   {
      int randomIndex = Random.Range(0, waypoints.Length);
      return waypoints[randomIndex];
   }
   #endregion
   #region Vision
   private void HandleVision()
   {
      // Scans for player inside visionRadius using OverlapSphere, checks for visibility
      
      //Player hits in a sphere radius
      Collider[] hits = new Collider[1];
      int hitCount = Physics.OverlapSphereNonAlloc(transform.position, visionRadius, hits,playerLayerMask);
      
      if (hitCount == 0)
      {
         isTargetVisible = false;
         return;
      }
      //Calculate distance
      Transform playerHit = hits[0].transform;
      float distance = Vector3.Distance(playerHit.position, transform.position);
      
      //if within closeRangeViewDistance/radius set as visible if no obstacles blocking view
      if (distance <= closeRangeViewDistance)
      {
         IsPlayerVisible(playerHit.transform);
         return;
      }
      
      //Check if inside FOV
      Vector3 dirToPlayer = (playerHit.position - transform.position).normalized;
      float angle = Vector3.Angle(transform.forward, dirToPlayer);
      
      if (Mathf.Abs(angle) <= fov / 2)
      {
         IsPlayerVisible(playerHit.transform);
      }
      else
      {
         isTargetVisible = false;
      }
   }
   private void IsPlayerVisible(Transform target)
   {
      // Multi-raycast visibility check, if any raycast doesn't hit an obstacle, player is visible
      
      Vector3 origin = transform.position + Vector3.up * visionHeight;
      Vector3[] points = 
      {
         target.position,
         target.position + target.up,
         target.position + target.right * 0.5f,
         target.position - target.right * 0.5f,
      };
      foreach (Vector3 point in points)
      {
         Vector3 direction = point - origin;
         float distance = Vector3.Distance(origin, point);
         if (!Physics.Raycast(origin, direction.normalized, distance, obstacleLayerMask))
         {
            isTargetVisible = true;
            playerT = target;
            lastKnownPos = playerT.position;
            return; //true
         }
      }
      isTargetVisible = false;
      // return false;
   }
   #endregion
   private void SetState(State newState, bool forceRun = false)
   {
      if (currentState == newState && !forceRun)
         return;
      
      currentState = newState;
      //speed setup
      switch (currentState)
      {
         case State.Patrol: SetMovementType(MovementType.Walk, forceRun); break;
         case State.Chase: SetMovementType(MovementType.Run, forceRun); break;
         case State.Search: SetMovementType(MovementType.Run, forceRun); break;
         case State.Idle: SetMovementType(MovementType.Walk, forceRun); break;
      }
   }
   private void SetMovementType(MovementType newMovementType, bool forceRun = false)
   {
      if (currentMovementType == newMovementType && !forceRun)
         return;
      currentMovementType = newMovementType;
      switch (currentMovementType)
      {
         case MovementType.Walk: agent.speed = walkSpeed; break;
         case MovementType.Run: agent.speed = runSpeed; break;
      }
   }

   public void OnOfferingHit()
   {
      GameObject p = Instantiate(disappearParticle, transform.position, transform.rotation);
      float duration = p.TryGetComponent(out ParticleSystem ps) ? ps.main.duration : 2f;
      Destroy(p, duration);
      
      transform.position = spawnPos;
      SetState(State.Idle, true);
      agent.ResetPath();
      
      /*GameObject p2 =  Instantiate(disappearParticle, transform.position, transform.rotation);
      Destroy(p2, duration);*/
   }
   
   private void OnDrawGizmosSelected()
   {
      // Vision radius
      Gizmos.color = Color.yellow;
      Gizmos.DrawWireSphere(transform.position, visionRadius);
      
      // Close range vision radius
      Gizmos.color = Color.red;
      Gizmos.DrawWireSphere(transform.position, closeRangeViewDistance);

      // FOV lines
      Vector3 leftDir = Quaternion.Euler(0, -fov / 2, 0) * transform.forward;
      Vector3 rightDir = Quaternion.Euler(0, fov / 2, 0) * transform.forward;

      Gizmos.color = Color.blue;
      Gizmos.DrawLine(transform.position, transform.position + leftDir * visionRadius);
      Gizmos.DrawLine(transform.position, transform.position + rightDir * visionRadius);

      // Current state color
      Gizmos.color = currentState == State.Chase ? Color.red : Color.green;
      Gizmos.DrawSphere(transform.position + Vector3.up * 2, 0.2f);
   }
}