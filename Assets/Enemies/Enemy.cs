using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.ThirdPerson;

public class Enemy : MonoBehaviour {

    [SerializeField] float maxHealthPoints = 100f;
    [SerializeField] float aggroDistance = 10f;

    private float currentHealthPoints = 100f;
    private ThirdPersonCharacter thirdPersonCharacter = null;
    private Transform startPosition = null;
    private AICharacterControl aICharacterControl = null;
    private Player player = null;

    
    public float healthAsPercentage
    {
        get
        {
            return currentHealthPoints / maxHealthPoints;
        }
    }

    private void Start()
    {
        startPosition = this.transform;
        player = FindObjectOfType<Player>();
        aICharacterControl = GetComponent<AICharacterControl>();
    }

    private void Update()
    {
        if (!player) { return; }

        var distanceToPlayer = this.transform.position - player.transform.position;
        if (distanceToPlayer.magnitude <= aggroDistance)
        {
            aICharacterControl.SetTarget(player.transform);
        }
        else
        {  // TODO - not working as transform moves with enemy
            aICharacterControl.SetTarget(startPosition);  
        }
    }

}
