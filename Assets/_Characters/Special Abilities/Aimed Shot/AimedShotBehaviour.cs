using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace RPG.Characters
{
    public class AimedShotBehaviour : AbilityBehaviour 
    {
        ParticleSystem myParticleSystem;

        public override void Use(AbilityUseParams useParams)
        {
            DealDamage(useParams);
            PlayParticleEffect();
        }

        private void DealDamage(AbilityUseParams useParams)
        {
            float damageToDeal = useParams.baseDamage + (config as AimedShotConfig).GetExtraDamage();
            useParams.target.AdjustHealth(damageToDeal);
        }
        
    }
}
