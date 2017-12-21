using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.ThirdPerson;
using UnityEngine.AI;

public class Enemy : MonoBehaviour {

    [SerializeField] float maxHealthPoints = 100f;
    [SerializeField] float aggroDistance = 10f;

    [Tooltip("Can Lead ShieldWalls")]
    [SerializeField] bool isLeader = false;

    [Tooltip("Aggro distance while in formation")]
    [SerializeField] float formationAggroDistance = 3f;

    [Tooltip("Distance from enemy to reform")]
    [SerializeField] float reformDistance = 10f;

    [Tooltip("Stop Distance while reforming")]
    [SerializeField] float reformStopDistance = 0.5f;


    private float currentHealthPoints = 100f;
    private float startingStopDistance;
    private Transform startPosition = null;
    private Transform formationPosition = null;
    private AICharacterControl aICharacterControl = null;
    private NavMeshAgent navMeshAgent;
    private Player player = null;
    private bool inFormation = false;

    
    public float healthAsPercentage
    {
        get
        {
            return currentHealthPoints / maxHealthPoints;
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
        startingStopDistance = navMeshAgent.stoppingDistance;
    }

    private void Update()
    {
        if (!player) { return; }

        if (formationPosition != null )
        {
            aggroDistance = formationAggroDistance;               
        }
        else
        {
            // TODO reset aggro distance when losing formation
        }

        // TODO - needs a reform method for when out of range
        // TODO - would like to be able to target friendly NPC
        
        var distanceToPlayer = this.transform.position - player.transform.position;
        if (distanceToPlayer.magnitude <= aggroDistance)
        {
            navMeshAgent.stoppingDistance = startingStopDistance;     // stopping distance for attacking
            aICharacterControl.SetTarget(player.transform);
        }
        else
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
