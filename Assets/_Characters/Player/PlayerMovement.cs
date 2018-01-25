using System;
using UnityEngine;
using UnityEngine.AI;
using RPG.CameraUI;  

namespace RPG.Characters
{
    [RequireComponent(typeof(ThirdPersonCharacter))]
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(AICharacterControl))]
    public class PlayerMovement : MonoBehaviour
    {
        ThirdPersonCharacter thirdPersonCharacter = null;   
        CameraRaycaster cameraRaycaster = null;
        AICharacterControl aICharacterControl = null;
        Vector3 movement = Vector3.zero;
        GameObject walkTarget = null;

        private void Start()
        {
            cameraRaycaster = Camera.main.GetComponent<CameraRaycaster>();
            thirdPersonCharacter = GetComponent<ThirdPersonCharacter>();
            aICharacterControl = GetComponent<AICharacterControl>();

            cameraRaycaster.onMouseOverWalkable += OnMouseOverWalkable;
            cameraRaycaster.onMouseOverEnemy += OnMouseOverEnemy;

            walkTarget = new GameObject("walkTarget");
        }

        private void OnMouseOverWalkable(Vector3 targetLocation)
        {
            if (Input.GetMouseButton(0))
            {
                walkTarget.transform.position = targetLocation;
                aICharacterControl.SetTarget(walkTarget.transform);
            }
        }

        private void OnMouseOverEnemy(Enemy enemy )
        {
            if (Input.GetMouseButton(0) || Input.GetMouseButton(1) )
            {
                aICharacterControl.SetTarget(enemy.transform);
            }
        }
                
        //TODO - Direct Movement - make this get called again
        private void ProcessDirectMovement()
        {
            // read inputs
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");

            // calculate camera relative direction to move:
            Vector3 cameraForward = Vector3.Scale(Camera.main.transform.forward, new Vector3(1, 0, 1)).normalized;
            movement = v * cameraForward + h * Camera.main.transform.right;

            thirdPersonCharacter.Move(movement, false, false);
        }

    }
}
