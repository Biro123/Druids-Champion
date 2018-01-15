using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using RPG.CameraUI;  // TODO Consider re-wiring
using RPG.Weapons;   // TODO Consider re-wiring
using RPG.Core;      // TODO Consider re-wiring

namespace RPG.Characters
{
    public class Player : MonoBehaviour, IDamageable
    {

        [SerializeField] float maxHealthPoints = 100f;
        [SerializeField] float damageCaused = 20f;
        [SerializeField] float timeBetweenHits = 0.7f;
        [SerializeField] float attackRange = 2f;
        [SerializeField] Weapon weaponInUse;
        [SerializeField] AnimatorOverrideController animatorOverrideController;

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
            SetHealthToMax();
            RegisterForMouseClick();
            PutWeaponInHand();
            OverrideAnimatorController();
        }

        private void SetHealthToMax()
        {
            currentHealthPoints = maxHealthPoints;
        }

        private void OverrideAnimatorController()
        {
            Animator animator = GetComponent<Animator>();
            animator.runtimeAnimatorController = animatorOverrideController;
            animatorOverrideController["DEFAULT ATTACK"] = weaponInUse.GetAttackAnimClip();
        }

        private void RegisterForMouseClick()
        {
            // Subscribe to Raycaster's on click event.
            cameraRaycaster = Camera.main.GetComponent<CameraRaycaster>();
            cameraRaycaster.notifyMouseClickObservers += OnMouseClick;
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

        void OnMouseClick(RaycastHit raycastHit, int layerHit)
        {
            if (layerHit == enemyLayer)
            {
                var enemy = raycastHit.collider.gameObject;
                currentTarget = enemy;

                // Check Enemy in Range
                if ((enemy.transform.position - transform.position).magnitude > attackRange)
                {
                    return;
                }

                currentTarget = enemy;

                // Find component and see if damageable (Components may be null)
                IDamageable damageableComponent = currentTarget.GetComponent<IDamageable>();

                if (damageableComponent != null)
                {
                    if (Time.time - lastHitTime >= timeBetweenHits)
                    {
                        damageableComponent.TakeDamage(damageCaused);
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
}
