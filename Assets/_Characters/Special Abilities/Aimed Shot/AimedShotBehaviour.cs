using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace RPG.Characters
{
    public class AimedShotBehaviour : MonoBehaviour, ISpecialAbility 
    {
        AimedShotConfig config;
        ParticleSystem myParticleSystem;

        public void SetConfig(AimedShotConfig configToSet)
        {
            this.config = configToSet;
        }

        public void Use(AbilityUseParams useParams)
        {
            DealDamage(useParams);
            PlayParticleEffect();
        }

        private void DealDamage(AbilityUseParams useParams)
        {
            float damageToDeal = useParams.baseDamage + config.GetExtraDamage();
            useParams.target.TakeDamage(damageToDeal);
        }

        private void PlayParticleEffect()
        {
            var particlePrefab = Instantiate(config.GetParticlePrefab(), this.gameObject.transform);
            myParticleSystem = particlePrefab.GetComponent<ParticleSystem>();
            myParticleSystem.Play();
            Destroy(particlePrefab, myParticleSystem.main.duration);
        }

    }
}
