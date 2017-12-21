using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.ThirdPerson;

public class Formation : MonoBehaviour {

    [SerializeField] GameObject row1Enemy;
    [SerializeField] GameObject leaderPrefab;
    
    private Transform[] troopPositions;
    private GameObject leader;

	// Use this for initialization
	void Start () {

        troopPositions = GetComponentsInChildren<Transform>();
        foreach (Transform troopPosition in troopPositions)
        {
            if (troopPosition.position == this.transform.position)
            {
                leader = Instantiate(leaderPrefab, troopPosition.position, this.transform.rotation, transform.parent);
                leader.GetComponent<Enemy>().SetFormationPos(troopPosition);
            }
            else
            {
                GameObject enemy = Instantiate(row1Enemy, troopPosition.position, this.transform.rotation, transform.parent);
                enemy.GetComponent<Enemy>().SetFormationPos(troopPosition);
                
                enemy.GetComponent<AICharacterControl>().SetTarget(troopPosition);
            }
        }
	}
	
	// Update is called once per frame
	void Update () {
        transform.position = leader.transform.position;
        transform.rotation = leader.transform.rotation;
        //transform.localScale = warlord.localScale;
	}
}
