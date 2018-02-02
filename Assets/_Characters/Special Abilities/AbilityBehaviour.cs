﻿using System.Collections;
using UnityEngine;

namespace RPG.Characters
{
    public abstract class AbilityBehaviour : MonoBehaviour
    {
        protected AbilityConfig config = null;   // Protected can be accessed by children

        AudioSource audioSource = null;
        AudioClip audioclip = null;

        const float PARTICLE_CLEAN_UP_DELAY = 5f;
        const string ATTACK_TRIGGER = "Attack";
        const string DEFAULT_ATTACK = "DEFAULT ATTACK";

        // Abstract class is overridden in the child classes.
        public abstract void Use(GameObject target = null);

        public void SetConfig(AbilityConfig configToSet)
        {
            config = configToSet;
        }

        protected void PlayAbilityAnimation()
        {
            var animatorOverrideController = GetComponent<Character>().GetAnimatorOverrideController();
            if (!animatorOverrideController)
            {
                Debug.Break();
                Debug.LogAssertion("Please provide " + gameObject + " with an animator Override Controller");
            }
            else
            {
                Animator animator = GetComponent<Animator>();                
                animator.runtimeAnimatorController = animatorOverrideController;
                animatorOverrideController[DEFAULT_ATTACK] = config.GetAbilityAnimation();
                animator.SetTrigger(ATTACK_TRIGGER);
            }
        }


        protected void PlayAbilityAudio()
        {
            audioclip = config.GetRandomAbilitySound();
            if (audioclip != null)
            {
                audioSource = GetComponent<AudioSource>();
                audioSource.PlayOneShot(audioclip);
            }
        }

        protected void PlayParticleEffect()
        {
            var particlePrefab = Instantiate(config.GetParticlePrefab(), this.gameObject.transform);
            particlePrefab.GetComponent<ParticleSystem>().Play();
            StartCoroutine(DestroyParticleWhenFinished(particlePrefab));
        }

        IEnumerator DestroyParticleWhenFinished(GameObject particlePrefab)
        {
            while (particlePrefab.GetComponent<ParticleSystem>().isPlaying)
            {
                yield return new WaitForSeconds(PARTICLE_CLEAN_UP_DELAY);
            }
            Destroy(particlePrefab);
            yield return new WaitForEndOfFrame();
        }


    }
}
