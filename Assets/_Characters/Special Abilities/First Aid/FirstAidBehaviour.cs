using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPG.Core;
using System;

namespace RPG.Characters    
{
    public class FirstAidBehaviour : MonoBehaviour, ISpecialAbility
    {
        FirstAidConfig config;
        ParticleSystem myParticleSystem;

        public void SetConfig(FirstAidConfig configToSet)
        {
            this.config = configToSet;
        }

        public void Use(AbilityUseParams useParams)
        {
            HealPlayer(useParams);            
            PlayParticleEffec();
        }

        private void HealPlayer(AbilityUseParams useParams)
        {
            var damageable = GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.AdjustHealth(-config.GetHealAmount());
            }            
        }

        private void PlayParticleEffec()
        {
            var particlePrefab = Instantiate(config.GetParticlePrefab(), this.gameObject.transform );
            myParticleSystem = particlePrefab.GetComponent<ParticleSystem>();
            myParticleSystem.Play();
            Destroy(particlePrefab, myParticleSystem.main.duration);
        }
    }
}
