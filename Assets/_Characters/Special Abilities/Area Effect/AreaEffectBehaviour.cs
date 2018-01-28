using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPG.Core;
using System;

namespace RPG.Characters    
{
    public class AreaEffectBehaviour : AbilityBehaviour
    {
        public override void Use(AbilityUseParams useParams)
        {
            DealRadialDamage(useParams);            
            PlayParticleEffect();
        }


        private void DealRadialDamage(AbilityUseParams useParams)
        {
            float damageToDeal = useParams.baseDamage + (config as AreaEffectConfig).GetExtraDamage();
            Collider[] collidersInRange = Physics.OverlapSphere(transform.position, (config as AreaEffectConfig).GetRadius());

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
