using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using RPG.Core;    

namespace RPG.Characters
{

    public class Enemy : MonoBehaviour, IDamageable
    {

        [SerializeField] float maxHealthPoints = 100f;

        [Tooltip("Enemies within this range will move to attack range")]
        [SerializeField]
        float aggroDistance = 10f;

        [Tooltip("Range which can attack from")]
        [SerializeField]
        float attackRange = 1.5f;

        [Tooltip("Aggro distance while in formation")]
        [SerializeField]
        float formationAggroDistance = 3f;

        [Tooltip("Stop Distance while going back to formation")]
        [SerializeField]
        float formationStopDistance = 0.5f;

        [SerializeField] GameObject projectileToUse;
        [SerializeField] GameObject projectileSocket;
        [SerializeField] float damagePerShot = 9f;
        [SerializeField] float secondsBetweenShots = 1.0f;
        [SerializeField] float shotRateVariation = 0.1f;
        [SerializeField] Vector3 aimOffset = new Vector3(0f, 1f, 0f);
        [Range(0.0f, 1.0f)] [SerializeField] float armourCoverage = 0.4f;
        [SerializeField] float bladeArmourAmount = 20f;
        [SerializeField] float bluntArmourAmount = 20f;
        [SerializeField] float pierceArmourAmount = 20f;


        [SerializeField] int[] layersToTarget = { 10, 11 };

        private float currentHealthPoints = 100f;
        private float originalStopDistance;
        private Transform startPosition = null;
        private Transform formationPosition = null;
        private NavMeshAgent navMeshAgent;
        private UnitOrder currentOrder = UnitOrder.Solo;
        private Transform target = null;
        private bool returnToStart = false;
        private bool isAttacking = false;
        private int opponentLayerMask = 0;


        public float healthAsPercentage
        {
            get
            {
                return currentHealthPoints / maxHealthPoints;
            }
        }

        public float GetArmourCoverage() { return armourCoverage; }

        public float GetBladeArmourAmount() { return bladeArmourAmount; }
        public float GetBluntArmourAmount() { return bluntArmourAmount; }
        public float GetPierceArmourAmount() { return pierceArmourAmount; }

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

        void IDamageable.AdjustHealth(float damage)
        {
            currentHealthPoints = Mathf.Clamp(currentHealthPoints - damage, 0f, maxHealthPoints);
            if (currentHealthPoints <= 0)
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            currentHealthPoints = maxHealthPoints;
            startPosition = this.transform;
            navMeshAgent = GetComponent<NavMeshAgent>();
            originalStopDistance = navMeshAgent.stoppingDistance;

            // Set up the layermask of opponents to look for.
            foreach (var layer in layersToTarget)
            {
                opponentLayerMask = opponentLayerMask | (1 << layer);
            }
        }

        private void Update()
        {
            // Handle Solo

            if (currentOrder == UnitOrder.Solo || currentOrder == UnitOrder.Skirmish)
            {
                target = FindTargetInRange(aggroDistance);

                if (target != null)
                {
                    // TODO Following code needs refactoring and including in other formations
                    Player player = target.GetComponent<Player>();
                    if (player)
                    {
                        if (player.healthAsPercentage <= Mathf.Epsilon)
                        {
                            StopAllCoroutines();
                            Destroy(this);  // To stop enemy attacking dead player
                        }
                    }

                    // 2. if have target - move to attack range
                    navMeshAgent.stoppingDistance = attackRange;
                    //REM aICharacterControl.SetTarget(target);
                    AttackIfInRange(target);
                }
                else
                {   // Else back to orig pos - TODO - need an original transform.
                    // TODO - need to cancel Invoke from AttackIfInReange when target becomes null
                    // Also refactor
                    navMeshAgent.stoppingDistance = originalStopDistance;
                    //REM aICharacterControl.SetTarget(startPosition);
                }
            }

            if (currentOrder == UnitOrder.ShieldWall)
            {
                target = FindTargetInRange(formationAggroDistance);

                // Check distance from formation position and decide whether to return
                var distanceFromFormation = this.transform.position - formationPosition.position;
                if (distanceFromFormation.magnitude >= formationAggroDistance * 2f)
                {
                    returnToStart = true;
                }
                else if (distanceFromFormation.magnitude <= formationAggroDistance)
                {
                    returnToStart = false;
                }

                // No target or too far from formation - return to formation
                if (target == null || returnToStart)
                {   // Else back to formation pos 
                    navMeshAgent.stoppingDistance = formationStopDistance;
                    //REM aICharacterControl.SetTarget(formationPosition);
                }
                else
                {   // Have target - move to attack range
                    navMeshAgent.stoppingDistance = attackRange;
                    //REM aICharacterControl.SetTarget(target);
                    AttackIfInRange(target);
                }
            }

            if (currentOrder == UnitOrder.Skirmish)
            {
                target = FindTargetInRange(aggroDistance);
                if (target != null)
                {   // Have target - move to attack range
                    navMeshAgent.stoppingDistance = attackRange;
                    //REM aICharacterControl.SetTarget(target);
                    AttackIfInRange(target);
                }
                else
                {   // Else back to formation pos 
                    navMeshAgent.stoppingDistance = originalStopDistance;
                    //REM aICharacterControl.SetTarget(formationPosition);
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
                    //REM aICharacterControl.SetTarget(formationPosition);
                }
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

        private void AttackIfInRange(Transform target)
        {

            if (projectileToUse == null || projectileSocket == null) { return; }

            float distanceToTarget = Vector3.Distance(target.position, this.transform.position);
            if (target != null && distanceToTarget <= attackRange)
            {
                if (!isAttacking)
                {
                    isAttacking = true;
                    float thisShotDelay = secondsBetweenShots 
                        + Random.Range(-shotRateVariation, +shotRateVariation);
                    // add start delay to avoid quick attacks when dipping in and out of range
                    InvokeRepeating("FireProjectile", thisShotDelay/2, thisShotDelay);  //TODO switch to coroutines
                }
            }
            else
            {
                isAttacking = false;
                CancelInvoke();
            }
        }

        // seperate out Character Firing Logic
        private void FireProjectile() // Change to supply target?
        {
            // Spawn projectile
            GameObject newProjectile = Instantiate(projectileToUse, projectileSocket.transform.position, Quaternion.identity);

            // Apply damage to it from the attacker
            Projectile projectileComponent = newProjectile.GetComponent<Projectile>();
            projectileComponent.SetDamage(damagePerShot);
            projectileComponent.SetShooter(gameObject);

            // Determine and apply its direction and velocity
            Vector3 unitVectorToTarget = Vector3.Normalize(target.position + aimOffset - projectileSocket.transform.position);
            float projectileSpeed = projectileComponent.GetDefaultLaunchSpeed();
            newProjectile.GetComponent<Rigidbody>().velocity = unitVectorToTarget * projectileSpeed;
        }

        private void OnDrawGizmos()
        {
            // Draw Attack Sphere
            Gizmos.color = new Color(255f, 0f, 0f);
            Gizmos.DrawWireSphere(transform.position, attackRange);

            // Draw Move Sphere
            Gizmos.color = new Color(0f, 255f, 0f);
            if (currentOrder == UnitOrder.Reform || currentOrder == UnitOrder.ShieldWall)
            {
                Gizmos.DrawWireSphere(transform.position, formationAggroDistance);
            }
            else
            {
                Gizmos.DrawWireSphere(transform.position, aggroDistance);
            }

        }
    }

}
