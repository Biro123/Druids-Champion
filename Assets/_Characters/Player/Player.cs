using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using RPG.CameraUI;  
using RPG.Core;      

namespace RPG.Characters
{
    public class Player : MonoBehaviour
    {
        [SerializeField] float baseDamage = 20f;
        [SerializeField] Weapon currentWeaponConfig;
        [SerializeField] AnimatorOverrideController animatorOverrideController;
        [SerializeField] ParticleSystem unarmouredHitParticleSystem = null;
        
        const string ATTACK_TRIGGER = "Attack";
        const string DEFAULT_ATTACK = "DEFAULT ATTACK";

        float lastHitTime = 0f;

        Enemy enemy = null;
        CameraRaycaster cameraRaycaster = null;
        Animator animator;
        SpecialAbilities specialAbilities;
        GameObject weaponObject;

        
        private void Start()
        {
            specialAbilities = GetComponent<SpecialAbilities>();
            RegisterForMouseClick();
            PutWeaponInHand(currentWeaponConfig);
            SetAttackAnimation();
        }


        public void PutWeaponInHand(Weapon weaponToUse)
        {
            print("Putting " + weaponToUse + " in hand");
            currentWeaponConfig = weaponToUse;
            var weaponPrefab = weaponToUse.GetWeaponPrefab();
            GameObject dominantHand = RequestDominantHand();
            Destroy(weaponObject);
            weaponObject = Instantiate(weaponPrefab, dominantHand.transform);
            weaponObject.transform.localPosition = currentWeaponConfig.gripTransform.localPosition;
            weaponObject.transform.localRotation = currentWeaponConfig.gripTransform.localRotation;
        }

        private void Update()
        {
            var healthAsPercentage = GetComponent<HealthSystem>().healthAsPercentage;
            if (healthAsPercentage > Mathf.Epsilon)  // If we are alive
            {
                ScanForAbilityKeyDown();
            }
        }

        private void ScanForAbilityKeyDown()
        {
            for (int keyIndex = 1; keyIndex <= specialAbilities.GetNumberOfAbilities(); keyIndex++)
            {
                if(Input.GetKeyDown(keyIndex.ToString()))
                {
                    specialAbilities.AttemptSpecialAbility(keyIndex-1);
                }
            }
        }
        
        private void SetAttackAnimation()
        {
            animator = GetComponent<Animator>();
            animator.runtimeAnimatorController = animatorOverrideController;
            animatorOverrideController[DEFAULT_ATTACK] = currentWeaponConfig.GetAttackAnimClip();
        }

        private void RegisterForMouseClick()
        {
            // Subscribe to Raycaster's on click event.
            cameraRaycaster = Camera.main.GetComponent<CameraRaycaster>();
            cameraRaycaster.onMouseOverEnemy += OnMouseOverEnemy;
        }

        private void OnMouseOverEnemy(Enemy enemyToSet)
        {
            enemy = enemyToSet;
            if(Input.GetMouseButton(0) && IsInRange(enemy.gameObject) )
            {
                Attack();
            }

            if (Input.GetMouseButtonDown(1) )
            {
                specialAbilities.AttemptSpecialAbility(0);
            }
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
            return distanceToTarget <= currentWeaponConfig.GetAttackRange();
        }

        private void Attack()
        {
            // Find component and see if damageable (Components may be null)
            IDamageable damageableComponent = enemy.GetComponent<IDamageable>();

            if (damageableComponent != null)
            {
                if (Time.time - lastHitTime >= currentWeaponConfig.GetTimeBetweenHits())
                {
                    SetAttackAnimation();
                    animator.SetTrigger(ATTACK_TRIGGER);
                    damageableComponent.AdjustHealth(CalculateDamage());
                    lastHitTime = Time.time;
                }
            }
        }

        private float CalculateDamage()
        {
            bool isArmourHit = IsArmourHit();
            if (UnityEngine.Random.Range(0f, 1f) <= currentWeaponConfig.GetChanceForSwing())
            {
                float bluntDamage = ApplyWeaponAndArmour(
                    currentWeaponConfig.GetBluntDamageModification(), 
                    enemy.GetBluntArmourAmount(),
                    isArmourHit);
                float bladeDamage = ApplyWeaponAndArmour(
                    currentWeaponConfig.GetBladeDamageModification(), 
                    enemy.GetBladeArmourAmount(),
                    isArmourHit);
                Debug.Log("Swing: " + bluntDamage + " " + bladeDamage);
                return bluntDamage + bladeDamage;
            }
            else
            {
                float pierceDamage = ApplyWeaponAndArmour(
                    currentWeaponConfig.GetPierceDamageModification(), 
                    enemy.GetPierceArmourAmount(),
                    isArmourHit);
                Debug.Log("Thrust: " + pierceDamage);
                return pierceDamage;
            }
        }

        private float ApplyWeaponAndArmour(float weaponDamageMod, float armourProtection, bool armourHit)
        {
            float weaponDamage = baseDamage * weaponDamageMod;

            if (armourHit)
            {
                float damageWithArmour = Mathf.Clamp(weaponDamage - armourProtection, 0f, weaponDamage);
                return damageWithArmour;
            }
            else
            {
                unarmouredHitParticleSystem.Play();
                return weaponDamage;
            }
        }

        private bool IsArmourHit()
        {
            return UnityEngine.Random.Range(0f, 1f) <= enemy.GetArmourCoverage();
        }
    }
}
