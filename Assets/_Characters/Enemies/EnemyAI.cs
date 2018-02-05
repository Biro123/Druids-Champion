using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Characters
{
    [RequireComponent(typeof(Character))]
    [RequireComponent(typeof(WeaponSystem))]
    [RequireComponent(typeof(HealthSystem))]
    public class EnemyAI : MonoBehaviour
    {
        [Tooltip("Enemies within this range will move to attack range")]
        [SerializeField] float aggroDistance = 10f;
        [SerializeField] WaypointContainer patrolPath;
        [SerializeField] float waypointTolerance = 3f;
        [SerializeField] float waypointDwellTime = 0.5f;

        //TODO try to do this without layers
        [SerializeField] int[] layersToTarget = { 10, 11 };

        float currentWeaponRange;

        Character character;
        Transform target = null;
        WeaponSystem weaponSystem;
        int opponentLayerMask = 0;
        float distanceToTarget = 0f;
        int waypointIndex;

        enum State { attacking, chasing, idle, patrolling, returning };
        State state = State.idle;

        private void Start()
        {
            character = GetComponent<Character>();
            weaponSystem = GetComponent<WeaponSystem>();
            currentWeaponRange = weaponSystem.GetCurrentWeapon().GetAttackRange();
  
            // Set up the layermask of opponents to look for.
            foreach (var layer in layersToTarget)
            {
                opponentLayerMask = opponentLayerMask | (1 << layer);
            }
        }

        private void Update()
        {
            target = FindTargetInRange(aggroDistance);
            distanceToTarget = 0f;
            bool inAttackRange = false;
            bool inAggroRange = false;

            if (target)
            {
                distanceToTarget = (transform.position - target.transform.position).magnitude;
                inAttackRange = (distanceToTarget <= currentWeaponRange);
                inAggroRange = (distanceToTarget <= aggroDistance && !inAttackRange);
            }
            else
            {
                inAttackRange = false;
                inAggroRange = false;
            }

            if (! inAttackRange && ! inAggroRange )
            {
                StopAllCoroutines();
                weaponSystem.StopAttacking();
                StartCoroutine(Patrol());
            }
            
            if (inAggroRange)
            {
                StopAllCoroutines();
                weaponSystem.StopAttacking();
                StartCoroutine(ChaseTarget());
            }

            if(inAttackRange)
            {
                StopAllCoroutines();
                state = State.attacking;
                weaponSystem.AttackTarget(target.gameObject);
            }
        }

        IEnumerator Patrol()
        {
            state = State.patrolling;

            while(patrolPath != null)
            {
                // work out where to go next
                Vector3 nextWaypointPos = patrolPath.transform.GetChild(waypointIndex).position;

                // tell character to go there
                character.SetDestination(nextWaypointPos);
                CycleWaypointWhenClose(nextWaypointPos);
                // wait at waypoint
                yield return new WaitForSeconds(waypointDwellTime);  
            }

        }

        private void CycleWaypointWhenClose(Vector3 nextWaypointPos)
        {
            if (Vector3.Distance(transform.position, nextWaypointPos) <= waypointTolerance)
            {
                // % = remainder from division.. i don't understand this
                waypointIndex = (waypointIndex + 1) % patrolPath.transform.childCount;
            }
        }

        IEnumerator ChaseTarget()
        {
            state = State.chasing;
            while(distanceToTarget >= currentWeaponRange )
            {
                character.SetDestination(target.position);
                yield return new WaitForEndOfFrame();
            }
        }

        private Transform FindTargetInRange(float aggroRange)
        {
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

        private void OnDrawGizmos()
        {
            // Draw Attack Sphere
            Gizmos.color = new Color(255f, 0f, 0f);
            Gizmos.DrawWireSphere(transform.position, currentWeaponRange);

            // Draw Move Sphere
            Gizmos.color = new Color(0f, 255f, 0f);
            Gizmos.DrawWireSphere(transform.position, aggroDistance);
        }
    }
}
