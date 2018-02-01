using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using RPG.Core;    

namespace RPG.Characters
{
    [RequireComponent(typeof(Character))]
    [RequireComponent(typeof(WeaponSystem))]
    public class EnemyAI : MonoBehaviour
    {
        [Tooltip("Enemies within this range will move to attack range")]
        [SerializeField] float aggroDistance = 10f;
        
        //TODO try to do this without layers
        [SerializeField] int[] layersToTarget = { 10, 11 };

        float currentWeaponRange;

        Character character;
        Transform target = null;
        int opponentLayerMask = 0;
        float distanceToTarget = 0f;


        enum State { attacking, chasing, idle, patrolling, returning };
        State state = State.idle;

        private void Start()
        {
            character = GetComponent<Character>();
            WeaponSystem weaponSystem = GetComponent<WeaponSystem>();
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
            if (target)
            {
                distanceToTarget = (transform.position - target.transform.position).magnitude;
            }

            if (target == null && state != State.patrolling)
            {
                StopAllCoroutines();
                state = State.patrolling;
            }
            
            if (target && state != State.chasing)
            {
                StopAllCoroutines();
                StartCoroutine(ChaseTarget());
            }

            if(distanceToTarget <= currentWeaponRange && state != State.attacking)
            {
                StopAllCoroutines();
                state = State.attacking;
                //  TODO    AttackIfInRange(target);
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
