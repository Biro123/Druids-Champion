using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPG.Core;
using System;

namespace RPG.Characters    
{
    public class FirstAidBehaviour : AbilityBehaviour
    {
        Player player;

        private void Start()
        {
            player = GetComponent<Player>();
        }

        public override void Use(GameObject target)
        {
            HealPlayer();            
            PlayParticleEffect();
            PlayAbilityAudio();
        }

        private void HealPlayer()
        {
            var playerHealth = player.GetComponent<HealthSystem>();
            if (playerHealth != null)
            {
                playerHealth.AdjustHealth( -(config as FirstAidConfig).GetHealAmount() );
            }            
        }
        
    }
}
