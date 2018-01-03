﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour, IDamageable {

    [SerializeField] float maxHealthPoints = 100f;
    [SerializeField] float damageCaused = 20f;
    [SerializeField] float timeBetweenHits = 0.7f;
    [SerializeField] float attackRange = 2f;

    [SerializeField] int enemyLayer = 9;

    float currentHealthPoints;
    float lastHitTime = 0f;

    GameObject currentTarget;
    CameraRaycaster cameraRaycaster; 

    public float healthAsPercentage
    {
        get
        {
            return currentHealthPoints / maxHealthPoints;
        }
    }

    private void Start()
    {
        currentHealthPoints = maxHealthPoints;

        // Subscribe to Raycaster's on click event.
        cameraRaycaster = Camera.main.GetComponent<CameraRaycaster>();
        cameraRaycaster.notifyMouseClickObservers += OnMouseClick;
    }

    void OnMouseClick(RaycastHit raycastHit, int layerHit)
    {
        if (layerHit == enemyLayer)
        {
            var enemy = raycastHit.collider.gameObject;
            currentTarget = enemy;

            // Check Enemy in Range
            if ( (enemy.transform.position - transform.position).magnitude > attackRange)
            {
                return;
            }

            currentTarget = enemy;

            // Find component and see if damageable (Components may be null)
            IDamageable damageableComponent = currentTarget.GetComponent<IDamageable>();

            if (damageableComponent != null)
            {   
                if (Time.time - lastHitTime >= timeBetweenHits)
                {   damageableComponent.TakeDamage(damageCaused);
                    lastHitTime = Time.time;
                }
            }


        }
    }

    void IDamageable.TakeDamage(float damage)
    {   // TakeDamage is called by other objects via an interface
        currentHealthPoints = Mathf.Clamp(currentHealthPoints - damage, 0f, maxHealthPoints);

        if (currentHealthPoints <= 0f) { }//TODO Player is dead 

    } 

}
