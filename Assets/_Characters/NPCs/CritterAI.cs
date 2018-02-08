using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Characters
{
    [RequireComponent(typeof(Character))]

    public class CritterAI : MonoBehaviour
    {
        [SerializeField] float distanceBeforeRun = 10f;
        [SerializeField] WaypointContainer patrolPath;
        [SerializeField] float waypointTolerance = 3f;
        [SerializeField] float waypointDwellTime = 0.5f;

        //TODO try to do this without layers
        [SerializeField] int[] layersToRunFrom = { 10 };

        Character character;
        Transform target = null;
        int opponentLayerMask = 0;
        float distanceToTarget = 0f;
        int waypointIndex;

        private void Start()
        {
            character = GetComponent<Character>();
  
            // Set up the layermask of opponents to look for.
            foreach (var layer in layersToRunFrom)
            {
                opponentLayerMask = opponentLayerMask | (1 << layer);
            }
        }

        private void Update()
        {
            target = FindTargetInRange(Mathf.Max(distanceBeforeRun ));
            distanceToTarget = 0f;
   
            if (target)
            {
                distanceToTarget = (transform.position - target.transform.position).magnitude;
                if (distanceToTarget <= distanceBeforeRun)
                {
                    StopAllCoroutines();
                    StartCoroutine(Patrol());  // Uses patrol route to flee
                }
                else
                {
                    StopAllCoroutines();
                }
            }
            else
            {
                StopAllCoroutines();
            }
        }

        IEnumerator Patrol()
        {
            while(patrolPath != null)
            {
                Vector3 nextWaypointPos = patrolPath.transform.GetChild(waypointIndex).position;

                character.SetDestination(nextWaypointPos);
                CycleWaypointWhenClose(nextWaypointPos);
                yield return new WaitForSeconds(waypointDwellTime);  // TODO wait not working
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
            // Draw Move Sphere
            Gizmos.color = new Color(0f, 255f, 0f);
            Gizmos.DrawWireSphere(transform.position, distanceBeforeRun);
        }
    }
}
