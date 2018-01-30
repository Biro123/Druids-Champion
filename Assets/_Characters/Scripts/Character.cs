using System;
using UnityEngine;
using UnityEngine.AI;

namespace RPG.Characters
{
    [SelectionBase]
    public class Character : MonoBehaviour
    {
        [Header("Animator")]
        [SerializeField] RuntimeAnimatorController animatorController;
        [SerializeField] AnimatorOverrideController animatorOverrideController;
        [SerializeField] Avatar characterAvatar;

        [Header("Audio")]
        [Range(0f, 1f)] [SerializeField] float audioVolume = 1;

        [Header("Capsule Collider")]
        [SerializeField] Vector3 colliderCentre = new Vector3(0, 0.9f, 0f);
        [SerializeField] float colliderRadius = 0.3f;
        [SerializeField] float colliderHeight = 1.8f;

        [Header("Movement")]
        [SerializeField] float moveSpeedMultiplier = 1f;
        [SerializeField] float animationSpeedMultiplier = 1f;
        [SerializeField] float movingTurnSpeed = 360;
        [SerializeField] float stationaryTurnSpeed = 180;

        [Header("Navigation")]
        [SerializeField] float navMeshAgentSteeringSpeed = 1.2f;
        [SerializeField] float navMeshAgentStoppingDistance = 1f;
        [SerializeField] bool navMeshAgentAutoBraking = false;

        [Header("Rigid Body")]
        [SerializeField] float rigidBodyMass = 80f;

        NavMeshAgent navMeshAgent;
        Animator animator;
        Rigidbody rigidBody;
        float turnAmount;
        float forwardAmount;

        private void Awake()
        {
            AddRequiredComponents();
        }

        private void AddRequiredComponents()
        {
            animator = gameObject.AddComponent<Animator>();
            animator.runtimeAnimatorController = animatorController;
            animator.avatar = characterAvatar;

            var capsuleCollider = gameObject.AddComponent<CapsuleCollider>();
            capsuleCollider.center = colliderCentre;
            capsuleCollider.radius = colliderRadius;
            capsuleCollider.height = colliderHeight;

            rigidBody = gameObject.AddComponent<Rigidbody>();
            rigidBody.constraints = RigidbodyConstraints.FreezeRotation;
            rigidBody.mass = rigidBodyMass;

            var audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.volume = audioVolume;

            navMeshAgent = gameObject.AddComponent<NavMeshAgent>();
            navMeshAgent.updatePosition = true;
            navMeshAgent.updateRotation = false;
            navMeshAgent.autoBraking = navMeshAgentAutoBraking;
            navMeshAgent.stoppingDistance = navMeshAgentStoppingDistance;
            navMeshAgent.speed = navMeshAgentSteeringSpeed;
            //navMeshAgent.areaMask.   // TODO need to set to walkable? 
        }

        private void Start()
        {
            CameraUI.CameraRaycaster cameraRaycaster = Camera.main.GetComponent<CameraUI.CameraRaycaster>();
            cameraRaycaster.onMouseOverWalkable += OnMouseOverWalkable;
            cameraRaycaster.onMouseOverEnemy += OnMouseOverEnemy;            
        }

        private void Update()
        {
            if (navMeshAgent.remainingDistance > navMeshAgent.stoppingDistance)
            {
                Move(navMeshAgent.desiredVelocity);
            }
            else
            {
                Move(Vector3.zero);
            }
        }

        public void Kill()
        {
            // to allow death signalling
        }

        public void Move(Vector3 movement)
        {
            SetForwardAndTurn(movement);
            ApplyExtraTurnRotation();
            UpdateAnimator();
        }

        private void SetForwardAndTurn(Vector3 movement)
        {
            // convert the world relative moveInput vector into a local-relative
            // turn amount and forward amount required to head in the desired direction.
            if (movement.magnitude > 1f)
            {
                movement.Normalize();
            }
            var localMove = transform.InverseTransformDirection(movement);
            //   move = Vector3.ProjectOnPlane(move, m_GroundNormal);  Was this removal ok?
            turnAmount = Mathf.Atan2(localMove.x, localMove.z);
            forwardAmount = localMove.z;
        }

        void ApplyExtraTurnRotation()
        {
            // help the character turn faster (this is in addition to root rotation in the animation)
            float turnSpeed = Mathf.Lerp(stationaryTurnSpeed, movingTurnSpeed, forwardAmount);
            transform.Rotate(0, turnAmount * turnSpeed * Time.deltaTime, 0);
        }

        void UpdateAnimator()
        {
            animator.SetFloat("Forward", forwardAmount, 0.1f, Time.deltaTime);
            animator.SetFloat("Turn", turnAmount, 0.1f, Time.deltaTime);
            animator.speed = animationSpeedMultiplier;
        }

        private void OnMouseOverWalkable(Vector3 targetLocation)
        {
            if (Input.GetMouseButton(0))
            {
                navMeshAgent.SetDestination(targetLocation);
            }
        }

        private void OnMouseOverEnemy(Enemy enemy )
        {
            if (Input.GetMouseButton(0) || Input.GetMouseButton(1) )
            {
                navMeshAgent.SetDestination(enemy.transform.position);
            }
        }

        public void OnAnimatorMove()
        {
            // we implement this function to override the default root motion.
            // this allows us to modify the positional speed before it's applied.
            if (Time.deltaTime > 0)
            {
                Vector3 velocity = (animator.deltaPosition * moveSpeedMultiplier) / Time.deltaTime;

                // we preserve the existing y part of the current velocity.
                velocity.y = rigidBody.velocity.y;
                rigidBody.velocity = velocity;
            }
        }        
    }
}
