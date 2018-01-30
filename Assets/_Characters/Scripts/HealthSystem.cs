using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace RPG.Characters
{
    public class HealthSystem : MonoBehaviour
    {

        [SerializeField] float maxHealthPoints = 100f;
        [SerializeField] Image healthBar;
        [SerializeField] float deathVanishSeconds = 2f;
        [SerializeField] AudioClip[] hitSounds;
        [SerializeField] AudioClip[] deathSounds;

        const string DEATH_TRIGGER = "Death";

        Animator animator;
        AudioSource audioSource;
        Character characterMovement;
        float currentHealthPoints = 0;

        public float healthAsPercentage
        {
            get { return currentHealthPoints / maxHealthPoints; }
        }

        void Start()
        {
            animator = GetComponent<Animator>();
            audioSource = GetComponent<AudioSource>();
            characterMovement = GetComponent<Character>();
            currentHealthPoints = maxHealthPoints;
        }

        void Update()
        {
            UpdateHealthBar();
        }

        private void UpdateHealthBar()
        {
            if(healthBar)
            {
                healthBar.fillAmount = healthAsPercentage;
            }
        }

        public void AdjustHealth(float amount)
        {
            bool isDieingThisHit = (currentHealthPoints > 0); // must ask before reducing health
            ReduceHealth(amount);
            if (currentHealthPoints <= 0f && isDieingThisHit)
            {
                StartCoroutine(KillCharacter());
            }
        }

        private void ReduceHealth(float damage)
        {
            currentHealthPoints = Mathf.Clamp(currentHealthPoints - damage, 0f, maxHealthPoints);
            if (damage <= 0) { return; }  // don't play sound if being healed.

            float chanceToPlaySound = (damage * 3 / maxHealthPoints);
            if (UnityEngine.Random.Range(0f, 1f) <= chanceToPlaySound)
            {
                PlayHitSound();
            }
        }

        private void PlayHitSound()
        {
            if (hitSounds.Length == 0) { return; }

            int audioIndex = UnityEngine.Random.Range(0, hitSounds.Length);
            var clip = hitSounds[audioIndex];
            audioSource.PlayOneShot(clip);

        }

        IEnumerator KillCharacter()
        {
            StopAllCoroutines();
            characterMovement.Kill();
            animator.SetTrigger(DEATH_TRIGGER);

            var playerComponent = GetComponent<Player>();
            if (playerComponent && playerComponent.isActiveAndEnabled)
            {
                PlayDeathSound();
                yield return new WaitForSeconds(audioSource.clip.length);
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
            else
            {
                Destroy(gameObject, deathVanishSeconds);
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
    }
}
