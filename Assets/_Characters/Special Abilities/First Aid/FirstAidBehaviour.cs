using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPG.Core;
using System;

namespace RPG.Characters    
{
    public class FirstAidBehaviour : AbilityBehaviour
    {
        
        public override void Use(AbilityUseParams useParams)
        {
            HealPlayer(useParams);            
            PlayParticleEffect();
            PlayAbilityAudio();
        }

        private void HealPlayer(AbilityUseParams useParams)
        {
            var damageable = GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.AdjustHealth( -(config as FirstAidConfig).GetHealAmount() );
            }            
        }
        
    }
}
