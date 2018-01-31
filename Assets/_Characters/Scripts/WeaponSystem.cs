﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace RPG.Characters
{
    public class WeaponSystem : MonoBehaviour
    {
        [SerializeField] float baseDamage = 220f;
        [SerializeField] WeaponConfig currentWeaponConfig;
        [SerializeField] ParticleSystem unarmouredHitParticleSystem;

        GameObject target;
        GameObject weaponObject;
        Animator animator;
        Character character;

        float lastHitTime;

        const string ATTACK_TRIGGER = "Attack";
        const string DEFAULT_ATTACK = "DEFAULT ATTACK";

        void Start()
        {
            animator = GetComponent<Animator>();
            character = GetComponent<Character>();

            PutWeaponInHand(currentWeaponConfig);
            SetAttackAnimation();
        }

        public void PutWeaponInHand(WeaponConfig weaponToUse)
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

        public void AttackTarget(GameObject targetToAttack)
        {
            target = targetToAttack;
            print("Attacking: " + target);
            // todo use a repeat attack co-routine

        }

        public WeaponConfig GetCurrentWeapon()
        {
            return currentWeaponConfig;
        }
        
        private void SetAttackAnimation()
        {
            animator = GetComponent<Animator>();
            var animatorOverrideController = character.GetAnimatorOverrideController();
            animator.runtimeAnimatorController = animatorOverrideController;
            animatorOverrideController[DEFAULT_ATTACK] = currentWeaponConfig.GetAttackAnimClip();
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

        private void Attack()
        {
            // Find component and see if damageable (Components may be null)
            HealthSystem targetHealthSystem = target.GetComponent<HealthSystem>();

            if (targetHealthSystem != null)
            {
                if (Time.time - lastHitTime >= currentWeaponConfig.GetTimeBetweenHits())
                {
                    SetAttackAnimation();
                    animator.SetTrigger(ATTACK_TRIGGER);
                    targetHealthSystem.AdjustHealth(CalculateDamage());
                    lastHitTime = Time.time;
                }
            }
        }

        private float CalculateDamage()
        {
            bool isArmourHit = IsArmourHit();
            if (UnityEngine.Random.Range(0f, 1f) <= currentWeaponConfig.GetChanceForSwing())
            {
                // TODO fix armour penetration
                //float bluntDamage = ApplyWeaponAndArmour(
                //    currentWeaponConfig.GetBluntDamageModification(),
                //    enemy.GetBluntArmourAmount(),
                //    isArmourHit);
                //float bladeDamage = ApplyWeaponAndArmour(
                //    currentWeaponConfig.GetBladeDamageModification(),
                //    enemy.GetBladeArmourAmount(),
                //    isArmourHit);
                //Debug.Log("Swing: " + bluntDamage + " " + bladeDamage);
                //return bluntDamage + bladeDamage;
                return baseDamage;
            }
            else
            {
                // TODO fix armour penetration
                //float pierceDamage = ApplyWeaponAndArmour(
                //    currentWeaponConfig.GetPierceDamageModification(),
                //    enemy.GetPierceArmourAmount(),
                //    isArmourHit);
                //Debug.Log("Thrust: " + pierceDamage);
                //return pierceDamage;
                return baseDamage;
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
            //TODO fix armourhit
            //return UnityEngine.Random.Range(0f, 1f) <= enemy.GetArmourCoverage();
            return true;
        }

    }
}
