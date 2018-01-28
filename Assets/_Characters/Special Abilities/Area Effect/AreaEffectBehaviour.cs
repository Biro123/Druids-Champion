using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPG.Core;
using System;

namespace RPG.Characters    
{
    public class AreaEffectBehaviour : MonoBehaviour, ISpecialAbility
    {
        AreaEffectConfig config;
        ParticleSystem myParticleSystem;

        public void SetConfig(AreaEffectConfig configToSet)
        {
            this.config = configToSet;
        }

        public void Use(AbilityUseParams useParams)
        {
            DealRadialDamage(useParams);            
            PlayParticleEffec();
        }

        private void PlayParticleEffec()
        {
            var particlePrefab = Instantiate(config.GetParticlePrefab(), this.gameObject.transform );
            myParticleSystem = particlePrefab.GetComponent<ParticleSystem>();
            myParticleSystem.Play();
            Destroy(particlePrefab, myParticleSystem.main.duration);
        }

        private void DealRadialDamage(AbilityUseParams useParams)
        {
            float damageToDeal = useParams.baseDamage + config.GetExtraDamage();
            Collider[] collidersInRange = Physics.OverlapSphere(transform.position, config.GetRadius());

            foreach (Collider colliderInRange in collidersInRange)
            {
                if (colliderInRange.gameObject.GetInstanceID() != this.gameObject.GetInstanceID())
                {  //Don't hit self
                    IDamageable damageableEnemy = colliderInRange.GetComponent<IDamageable>();
                    if (damageableEnemy != null)
                    {
                        damageableEnemy.AdjustHealth(damageToDeal);
                    }
                }
            }
        }
    }
}
