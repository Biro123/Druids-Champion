using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.ThirdPerson;
using UnityEngine.AI;

public class Enemy : MonoBehaviour {

    [SerializeField] float maxHealthPoints = 100f;

    [Tooltip("Enemies within this range will move to attack range")]
    [SerializeField] float aggroDistance = 10f;

    //[Tooltip("Can Lead ShieldWalls")]
    //[SerializeField] bool isLeader = false;

    [Tooltip("Aggro distance while in formation")]
    [SerializeField] float formationAggroDistance = 3f;

    [Tooltip("Range which can attack from")]
    [SerializeField] float attackRange = 1.5f;

    [Tooltip("Stop Distance while reforming")]
    [SerializeField] float reformStopDistance = 0.5f;

    [SerializeField] int[] layersToTarget = { 10, 11 };


    private float currentHealthPoints = 100f;
    private float originalStopDistance;
    private Transform startPosition = null;
    private Transform formationPosition = null;
    private AICharacterControl aICharacterControl = null;
    private NavMeshAgent navMeshAgent;
    private Player player = null;
    private bool inFormation = false;
    private UnitOrder currentOrder = UnitOrder.Solo;
    private Transform target = null;

    
    public float healthAsPercentage
    {
        get
        {
            return currentHealthPoints / maxHealthPoints;
        }
    }

    public void SetOrder(UnitOrder order, Transform position)
    {
        currentOrder = order;
        switch (order)
        {
            case UnitOrder.Solo:
                formationPosition = null;
                break;
            case UnitOrder.ShieldWall:
                formationPosition = position;
                break;
            case UnitOrder.Skirmish:
                formationPosition = position;
                break;
            case UnitOrder.Reform:
                formationPosition = position;
                break;
            case UnitOrder.Refill:
                break;
            default:
                break;
        }
    }

    public void SetFormationPos(Transform position)
    {
        formationPosition = position;
    }

    private void Start()
    {
        startPosition = this.transform;
        player = FindObjectOfType<Player>();
        aICharacterControl = GetComponent<AICharacterControl>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        originalStopDistance = navMeshAgent.stoppingDistance;
    }

    private void Update()
    {
        // Handle Solo

        if (currentOrder == UnitOrder.Solo || currentOrder == UnitOrder.Skirmish)
        {
            target = FindTargetInRange(aggroDistance);

            if (target != null)
            {   // 2. if have target - move to attack range
                navMeshAgent.stoppingDistance = originalStopDistance + attackRange;
                aICharacterControl.SetTarget(target);
            }
            else
            {   // Else back to orig pos - TODO - need an original transform.
                navMeshAgent.stoppingDistance = originalStopDistance;
                aICharacterControl.SetTarget(startPosition);
            }
        }

        if (currentOrder == UnitOrder.ShieldWall)
        {
            target = FindTargetInRange(formationAggroDistance);
            if (target != null)
            {   // Have target - move to attack range
                navMeshAgent.stoppingDistance = originalStopDistance + attackRange;
                aICharacterControl.SetTarget(target);
            }
            else
            {   // Else back to formation pos 
                navMeshAgent.stoppingDistance = reformStopDistance;
                aICharacterControl.SetTarget(formationPosition);
            }
        }

        if (currentOrder == UnitOrder.Skirmish)
        {
            target = FindTargetInRange(aggroDistance);
            if (target != null)
            {   // Have target - move to attack range
                navMeshAgent.stoppingDistance = originalStopDistance + attackRange;
                aICharacterControl.SetTarget(target);
            }
            else
            {   // Else back to formation pos 
                navMeshAgent.stoppingDistance = originalStopDistance;
                aICharacterControl.SetTarget(formationPosition);
            }
        }

        if (currentOrder == UnitOrder.Reform)
        {
            target = FindTargetInRange(formationAggroDistance);
            if (target != null)
            {   // Have target - stop reforming
                currentOrder = UnitOrder.ShieldWall;
            }
            else
            {   // Else back to formation pos 
                navMeshAgent.stoppingDistance = 0.5f;
                aICharacterControl.SetTarget(formationPosition);
            }
        }

    }

    Transform FindTargetInRange(float aggroRange)
    {
        // Set up the layermask to check - ie. look for player or his allies.
        int opponentLayerMask = 0;
        foreach (var layer in layersToTarget)
        {
            opponentLayerMask = opponentLayerMask | (1 << layer);
        }
                

        // See what are in range
        Collider[] opponentsInRange = Physics.OverlapSphere(this.transform.position, aggroRange, opponentLayerMask);

        if (opponentsInRange.Length == 0) { return null; }

        // Find closest in range
        float closestRange = 0;
        Collider closestTarget = null;
        foreach (var opponentInRange in opponentsInRange)
        {
            if (target != null && opponentInRange.gameObject == target.gameObject)  
            {  // keep current target if still in range
                return opponentInRange.transform;
            }
            float currentRange = (transform.position - opponentInRange.transform.position).magnitude;
            if (closestTarget == null || currentRange < closestRange)
            {
                closestTarget = opponentInRange;
                closestRange = currentRange;
            }
        }
        return closestTarget.transform;
    }



    private void MoveToTarget(Transform target, float maxDistance)
    {
        var distanceToTarget = this.transform.position - target.position;
        if (distanceToTarget.magnitude <= maxDistance)
        {
            navMeshAgent.stoppingDistance = originalStopDistance;     // stopping distance for attacking
            aICharacterControl.SetTarget(target);
        }
        else   // Not in range of target
        {
            if (formationPosition != null)
            {
                navMeshAgent.stoppingDistance = reformStopDistance;   // small stopping distance to keep formation
                aICharacterControl.SetTarget(formationPosition);
            }
            else
            {
                aICharacterControl.SetTarget(startPosition);
            }
        }
    }
}
