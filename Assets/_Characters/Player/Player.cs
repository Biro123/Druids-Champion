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
    public class Player : MonoBehaviour, IDamageable
    {
        [SerializeField] float maxHealthPoints = 100f;
        [SerializeField] float baseDamage = 20f;
        [SerializeField] Weapon currentWeaponConfig;
        [SerializeField] AnimatorOverrideController animatorOverrideController;
        [SerializeField] ParticleSystem unarmouredHitParticleSystem = null;
        [SerializeField] AudioClip[] hitSounds;
        [SerializeField] AudioClip[] deathSounds;


        // Temporarily serializing for build/testing
        [SerializeField] AbilityConfig[] abilities;

        const string DEATH_TRIGGER = "Death";
        const string ATTACK_TRIGGER = "Attack";
        const string DEFAULT_ATTACK = "DEFAULT ATTACK";

        float currentHealthPoints = 0;
        float lastHitTime = 0f;

        Enemy enemy = null;
        CameraRaycaster cameraRaycaster = null;
        Animator animator = null;
        Stamina stamina = null;
        AudioSource audioSource = null;
        GameObject weaponObject;

        public float healthAsPercentage
        {
            get
            { return currentHealthPoints / maxHealthPoints; }
        }
        
        private void Start()
        {
            stamina = GetComponent<Stamina>();
            audioSource = GetComponent<AudioSource>();
            SetHealthToMax();
            RegisterForMouseClick();
            PutWeaponInHand(currentWeaponConfig);
            SetAttackAnimation();
            AttachInitialAbilities();
        }

        private void AttachInitialAbilities()
        {
            for (int abilityIndex = 0; abilityIndex < abilities.Length; abilityIndex++)
            {
                // Add the behaviour script to the player.
                abilities[abilityIndex].AttachAbilityTo(this.gameObject);   
            }
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
            if(healthAsPercentage > Mathf.Epsilon)
            {
                ScanForAbilityKeyDown();
            }
        }

        private void ScanForAbilityKeyDown()
        {
            for (int keyIndex = 1; keyIndex <= abilities.Length; keyIndex++)
            {
                if(Input.GetKeyDown(keyIndex.ToString()))
                {
                    AttemptSpecialAbility(keyIndex-1);
                }
            }
        }

        void IDamageable.AdjustHealth(float amount)
        {   
            bool isDieingThisHit = (currentHealthPoints > 0); // must ask before reducing health
            ReduceHealth(amount);
            if (currentHealthPoints <= 0f && isDieingThisHit)
            {                
                StartCoroutine(KillPlayer());
            }
        }

        IEnumerator KillPlayer()
        {
            PlayDeathSound();
            animator.SetTrigger(DEATH_TRIGGER);
            yield return new WaitForSeconds(3f);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void ReduceHealth(float damage)
        {
            currentHealthPoints = Mathf.Clamp(currentHealthPoints - damage, 0f, maxHealthPoints);
            if(damage <= 0) { return; }  // don't play sound if being healed.

            float chanceToPlaySound = (damage*3 / maxHealthPoints);
            if (UnityEngine.Random.Range(0f, 1f) <= chanceToPlaySound)
            {
                PlayHitSound();
            }
        }

        private void PlayHitSound()
        {
            if (hitSounds.Length == 0) { return; }

            int audioIndex = UnityEngine.Random.Range(0, hitSounds.Length);
            audioSource.clip = hitSounds[audioIndex];
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }

        private void PlayDeathSound()
        {
            if (deathSounds.Length == 0) { return; }

            int audioIndex = UnityEngine.Random.Range(0, deathSounds.Length);
            audioSource.clip = deathSounds[audioIndex];
            audioSource.Stop();
            audioSource.Play();
        }

        private void SetHealthToMax()
        {
            currentHealthPoints = maxHealthPoints;
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
                AttemptSpecialAbility(0);
            }
        }

        private void AttemptSpecialAbility(int abilityIndex)
        {
            float staminaCost = abilities[abilityIndex].GetStaminaCost();
            // TODO if (IsInRange(enemy.gameObject) && stamina.IsStaminaAvailable(staminaCost))
            if (stamina.IsStaminaAvailable(staminaCost))
            {
                stamina.UseStamina(staminaCost);
                var abilityParams = new AbilityUseParams(enemy, baseDamage);
                abilities[abilityIndex].Use(abilityParams);
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
