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
            PlayAbilityAnimation();
        }

        private void DealDamage(GameObject target)
        {
            GetComponent<WeaponSystem>().SpecialAttack(
                target, 
                (config as AimedShotConfig).GetAttackAdj(),
                (config as AimedShotConfig).GetDamageAdj(),
                (config as AimedShotConfig).GetArmourAvoidAdj() 
                );
            //float damageToDeal = (config as AimedShotConfig).GetExtraDamage();
            //target.GetComponent<HealthSystem>().AdjustHealth(damageToDeal);
        }
        
    }
}
