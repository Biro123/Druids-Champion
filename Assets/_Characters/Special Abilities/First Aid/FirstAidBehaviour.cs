using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPG.Core;
using System;

namespace RPG.Characters    
{
    public class FirstAidBehaviour : AbilityBehaviour
    {
        FirstAidConfig config = null;
        ParticleSystem myParticleSystem = null;
        AudioSource audioSource = null;
        AudioClip audioclip = null;

        public void SetConfig(FirstAidConfig configToSet)
        {
            this.config = configToSet;
        }

        public override void Use(AbilityUseParams useParams)
        {
            HealPlayer(useParams);            
            PlayParticleEffect();
            PlayAbilityAudio();
        }

        private void PlayAbilityAudio()
        {
            audioclip = config.GetAudioClip();
            if (audioclip != null)
            {
                audioSource = GetComponent<AudioSource>();
                audioSource.clip = audioclip;
                audioSource.Play();
            }
        }

        private void HealPlayer(AbilityUseParams useParams)
        {
            var damageable = GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.AdjustHealth(-config.GetHealAmount());
            }            
        }

        private void PlayParticleEffect()
        {
            var particlePrefab = Instantiate(config.GetParticlePrefab(), this.gameObject.transform );
            myParticleSystem = particlePrefab.GetComponent<ParticleSystem>();
            myParticleSystem.Play();
            Destroy(particlePrefab, myParticleSystem.main.duration);
        }
    }
}
