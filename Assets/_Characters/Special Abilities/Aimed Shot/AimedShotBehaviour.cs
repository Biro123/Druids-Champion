using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace RPG.Characters
{
    public class AimedShotBehaviour : AbilityBehaviour 
    {
        ParticleSystem myParticleSystem;

        public override void Use(GameObject target)
        {
            DealDamage(target);
            PlayParticleEffect();
            PlayAbilityAudio();
        }

        private void DealDamage(GameObject target)
        {
            float damageToDeal = (config as AimedShotConfig).GetExtraDamage();
            target.GetComponent<HealthSystem>().AdjustHealth(damageToDeal);
        }
        
    }
}
