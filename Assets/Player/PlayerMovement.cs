using System;
using UnityEngine;
using UnityStandardAssets.Characters.ThirdPerson;
using UnityEngine.AI;

[RequireComponent(typeof (ThirdPersonCharacter))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(AICharacterControl))]
public class PlayerMovement : MonoBehaviour
{
    ThirdPersonCharacter thirdPersonCharacter = null;   // A reference to the ThirdPersonCharacter on the object
    CameraRaycaster cameraRaycaster = null;
    AICharacterControl aICharacterControl = null;
    Vector3 movement = Vector3.zero;
    GameObject walkTarget = null;

    // TODO - resolve conflict with const and SerialiseField
    [SerializeField] const int walkableLayerNumber = 8;
    [SerializeField] const int enemyLayerNumber = 9;


    private void Start()
    {
        cameraRaycaster = Camera.main.GetComponent<CameraRaycaster>();
        thirdPersonCharacter = GetComponent<ThirdPersonCharacter>();
        aICharacterControl = GetComponent<AICharacterControl>();

        cameraRaycaster.notifyMouseClickObservers += ProcessMouseClick; // Registering as Subscriber

        walkTarget = new GameObject("walkTarget");
    }

    private void ProcessMouseClick (RaycastHit raycastHit, int layerHit)
    {
        switch (layerHit)   
        {
            case walkableLayerNumber:
                walkTarget.transform.position = raycastHit.point;
                aICharacterControl.SetTarget(walkTarget.transform);
                break;
            case enemyLayerNumber:
                aICharacterControl.SetTarget(raycastHit.transform);
                break;
            default:
                Debug.LogWarning("Don't know how to handle mouseclick for player movement");
                break;
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

