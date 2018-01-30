using System;
using UnityEngine;
using UnityEngine.AI;

namespace RPG.Characters
{
    [RequireComponent(typeof(ThirdPersonCharacter))]
    [RequireComponent(typeof(NavMeshAgent))]
    public class CharacterMovement : MonoBehaviour
    {
        [SerializeField] float stoppingDistance = 1f;
        [SerializeField] float moveSpeedMultiplier = 1f;

        ThirdPersonCharacter character;   
        NavMeshAgent agent;
        Animator animator;
        Rigidbody rigidBody;

        private void Start()
        {
            CameraUI.CameraRaycaster cameraRaycaster = Camera.main.GetComponent<CameraUI.CameraRaycaster>();
            character = GetComponent<ThirdPersonCharacter>();
            animator = GetComponent<Animator>();
            rigidBody = GetComponent<Rigidbody>();

            agent = GetComponent<NavMeshAgent>();
            agent.updatePosition = true;
            agent.updateRotation = false;
            agent.stoppingDistance = stoppingDistance;

            cameraRaycaster.onMouseOverWalkable += OnMouseOverWalkable;
            cameraRaycaster.onMouseOverEnemy += OnMouseOverEnemy;

            // walkTarget = new GameObject("walkTarget");
        }

        private void Update()
        {
            if (agent.remainingDistance > agent.stoppingDistance)
            {
                character.Move(agent.desiredVelocity);
            }
            else
            {
                character.Move(Vector3.zero);
            }
        }

        private void OnMouseOverWalkable(Vector3 targetLocation)
        {
            if (Input.GetMouseButton(0))
            {
                //REM walkTarget.transform.position = targetLocation;
                //REM aICharacterControl.SetTarget(walkTarget.transform);
                agent.SetDestination(targetLocation);
            }
        }

        private void OnMouseOverEnemy(Enemy enemy )
        {
            if (Input.GetMouseButton(0) || Input.GetMouseButton(1) )
            {
                //REM aICharacterControl.SetTarget(enemy.transform);
                agent.SetDestination(enemy.transform.position);
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
