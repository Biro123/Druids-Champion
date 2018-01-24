using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using RPG.CameraUI;  
using RPG.Weapons;   
using RPG.Core;      

namespace RPG.Characters
{
    public class Player : MonoBehaviour, IDamageable
    {
        [SerializeField] float maxHealthPoints = 100f;
        [SerializeField] float damageCaused = 20f;
        [SerializeField] Weapon weaponInUse;
        [SerializeField] AnimatorOverrideController animatorOverrideController;
        
        float currentHealthPoints;
        float lastHitTime = 0f;

        CameraRaycaster cameraRaycaster;
        Animator animator;

        public float healthAsPercentage
        {
            get
            {
                return currentHealthPoints / maxHealthPoints;
            }
        }

        void IDamageable.TakeDamage(float damage)
        {   // TakeDamage is called by other objects via an interface
            currentHealthPoints = Mathf.Clamp(currentHealthPoints - damage, 0f, maxHealthPoints);

            if (currentHealthPoints <= 0f) { } // Player is dead 
        }

        private void Start()
        {
            SetHealthToMax();
            RegisterForMouseClick();
            PutWeaponInHand();
            SetupRuntimeAnimator();
        }

        private void SetHealthToMax()
        {
            currentHealthPoints = maxHealthPoints;
        }

        private void SetupRuntimeAnimator()
        {
            animator = GetComponent<Animator>();
            animator.runtimeAnimatorController = animatorOverrideController;
            animatorOverrideController["DEFAULT ATTACK"] = weaponInUse.GetAttackAnimClip();
        }

        private void RegisterForMouseClick()
        {
            // Subscribe to Raycaster's on click event.
            cameraRaycaster = Camera.main.GetComponent<CameraRaycaster>();
            cameraRaycaster.onMouseOverEnemy += OnMouseOverEnemy;
        }

        private void OnMouseOverEnemy(Enemy enemy)
        {
            if(Input.GetMouseButtonDown(0) && IsInRange(enemy.gameObject) )
            {
                Attack(enemy.gameObject);
            }
        }

        private void PutWeaponInHand()
        {
            GameObject dominantHand = RequestDominantHand();
            var weapon = Instantiate(weaponInUse.GetWeaponPrefab(), dominantHand.transform);
            weapon.transform.localPosition = weaponInUse.gripTransform.localPosition;
            weapon.transform.localRotation = weaponInUse.gripTransform.localRotation;
        }

        private GameObject RequestDominantHand()
        {
            var dominantHands = GetComponentsInChildren<DominantHand>();
            int numberOfDominantHands = dominantHands.Length;

            // Ensure either 1 dominant hand - or an error is returned. 
            Assert.AreNotEqual(numberOfDominantHands, 0, "No Dominant Hand on Player");
            Assert.IsFalse(numberOfDominantHands > 1, "Multiple Dominant Hands on Player");
            return dominantHands[0].gameObject;
        }

        private bool IsInRange(GameObject target)
        {
            float distanceToTarget = (target.transform.position - this.transform.position).magnitude;
            return distanceToTarget <= weaponInUse.GetAttackRange();
        }

        private void Attack(GameObject enemy)
        {
            // Find component and see if damageable (Components may be null)
            IDamageable damageableComponent = enemy.GetComponent<IDamageable>();

            if (damageableComponent != null)
            {
                if (Time.time - lastHitTime >= weaponInUse.GetTimeBetweenHits())
                {
                    animator.SetTrigger("Attack");  // TODO make const
                    damageableComponent.TakeDamage(damageCaused);
                    lastHitTime = Time.time;
                }
            }
        }
    }
}
