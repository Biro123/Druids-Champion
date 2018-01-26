using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using RPG.CameraUI;  
using RPG.Weapons;   
using RPG.Core;      

namespace RPG.Characters
{
    public class Player : MonoBehaviour, IDamageable
    {
        [SerializeField] float maxHealthPoints = 100f;
        [SerializeField] float baseDamage = 20f;
        [SerializeField] Weapon weaponInUse;
        [SerializeField] AnimatorOverrideController animatorOverrideController;
        [SerializeField] AudioClip[] hitSounds;
        [SerializeField] AudioClip[] deathSounds;

        // Temporarily serializing for build/testing
        [SerializeField] SpecialAbility[] abilities;

        const string DEATH_TRIGGER = "Death";
        const string ATTACK_TRIGGER = "Attack";

        float currentHealthPoints;
        float lastHitTime = 0f;

        CameraRaycaster cameraRaycaster;
        Animator animator;
        Stamina stamina;
        AudioSource audioSource;

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
            PutWeaponInHand();
            SetupRuntimeAnimator();
            abilities[0].AttachComponentTo(this.gameObject);   // Adds the ability behaviour script to the player.
        }

        void IDamageable.TakeDamage(float damage)
        {   
            bool isDieingThisHit = (currentHealthPoints > 0); // must ask before reducing health
            ReduceHealth(damage);
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
            PlayHitSound();
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
            if(Input.GetMouseButton(0) && IsInRange(enemy.gameObject) )
            {
                Attack(enemy.gameObject);
            }

            if (Input.GetMouseButtonDown(1) )
            {
                AttemptSpecialAbility(0, enemy);
            }
        }

        private void AttemptSpecialAbility(int abilityIndex, Enemy enemy)
        {
            float staminaCost = abilities[abilityIndex].GetStaminaCost();
            if (IsInRange(enemy.gameObject) && stamina.IsStaminaAvailable(staminaCost))  // TODO - get cost from SO
            {
                stamina.UseStamina(staminaCost);
                var abilityParams = new AbilityUseParams(enemy, baseDamage);
                abilities[abilityIndex].Use(abilityParams);
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
                    animator.SetTrigger(ATTACK_TRIGGER);  
                    damageableComponent.TakeDamage(baseDamage);
                    lastHitTime = Time.time;
                }
            }
        }
    }
}
