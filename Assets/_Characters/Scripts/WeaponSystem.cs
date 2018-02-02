using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace RPG.Characters
{
    public class WeaponSystem : MonoBehaviour
    {
        [SerializeField] float baseDamage = 220f;
        [SerializeField] WeaponConfig currentWeaponConfig;

        GameObject target;
        HealthSystem targetHealthSystem;
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
            targetHealthSystem = target.GetComponent<HealthSystem>();
            print("Attacking: " + target);
            StartCoroutine(AttackTargetRepeatedly());
        }

        IEnumerator AttackTargetRepeatedly()
        {
            // determine both attacker and defender are alive
            bool attackerIsAlive = GetComponent<HealthSystem>().healthAsPercentage >= Mathf.Epsilon;
            bool targetIsAlive = target.GetComponent<HealthSystem>().healthAsPercentage >= Mathf.Epsilon;
            
            while(attackerIsAlive && targetIsAlive)
            {
                float weaponHitPeriod = currentWeaponConfig.GetTimeBetweenHits();
                float timeToWait = weaponHitPeriod * character.GetAnimSpeedMultiplier();
                bool isTimeToHit = Time.time - lastHitTime > timeToWait;

                if(isTimeToHit)
                {
                    AttackTargetOnce();
                    lastHitTime = Time.time;
                }
                yield return new WaitForSeconds(timeToWait);
            }            
        }

        public WeaponConfig GetCurrentWeapon()
        {
            return currentWeaponConfig;
        }
        
        private void SetAttackAnimation()
        {
            if (!character.GetAnimatorOverrideController())
            {
                Debug.Break();
                Debug.LogAssertion("Please proved " + gameObject + " with an animator Override Controller");
            }
            else
            {
                var animatorOverrideController = character.GetAnimatorOverrideController();
                animator.runtimeAnimatorController = animatorOverrideController;
                animatorOverrideController[DEFAULT_ATTACK] = currentWeaponConfig.GetAttackAnimClip();
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

        private void AttackTargetOnce()
        {
            //FaceTarget();  // TODO make work
            transform.LookAt(target.transform);

            if (targetHealthSystem != null)
            {
                SetAttackAnimation();
                animator.SetTrigger(ATTACK_TRIGGER);
                float damageDelay = 0.5f; // TODO get from weapon
                StartCoroutine(DamageAfterDelay(damageDelay));                
            }
        }

        IEnumerator DamageAfterDelay (float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            targetHealthSystem.AdjustHealth(CalculateDamage());            
        }

        private void FaceTarget()
        {
            var attackTurnSpeed = character.GetAttackTurnRate();
            var amountToRotate = Quaternion.LookRotation(target.transform.position - this.transform.position);
            var rotateSpeed = Mathf.Min(attackTurnSpeed * Time.deltaTime, 1);
            transform.rotation = Quaternion.Lerp(transform.rotation, amountToRotate, rotateSpeed);
        }

        private float CalculateDamage()
        {
            ArmourSystem.ArmourProtection targetArmour = new ArmourSystem.ArmourProtection();
            ArmourSystem targetArmourSystem = target.GetComponent<ArmourSystem>();
            if (targetArmourSystem)
            {
                targetArmour = targetArmourSystem.CalculateArmour();
            }

            if (UnityEngine.Random.Range(0f, 1f) <= currentWeaponConfig.GetChanceForSwing())
            {
                return CalculateSwingDamage(targetArmour);
            }
            else
            {
                return CalculateThrustDamage(targetArmour);
            }
        }

        private float CalculateSwingDamage(ArmourSystem.ArmourProtection targetArmour)
        {
            float bluntDamageDone = baseDamage * currentWeaponConfig.GetBluntDamageModification();
            float bluntDamageTaken = Mathf.Clamp(bluntDamageDone - targetArmour.blunt, 0f, bluntDamageDone);

            float bladeDamageDone = baseDamage * currentWeaponConfig.GetBladeDamageModification();
            float bladeDamageTaken = Mathf.Clamp(bladeDamageDone - targetArmour.blade, 0f, bladeDamageDone);

            return bluntDamageTaken + bladeDamageTaken;
        }
        
        private float CalculateThrustDamage(ArmourSystem.ArmourProtection targetArmour)
        {
            float pierceDamageDone = baseDamage * currentWeaponConfig.GetPierceDamageModification();
            float pierceDamageTaken = Mathf.Clamp(pierceDamageDone - targetArmour.pierce, 0f, pierceDamageDone);
            return pierceDamageTaken;
        }
    }
}
