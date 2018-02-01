using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Characters
{
    public class ArmourSystem : MonoBehaviour
    {

        [SerializeField] ArmourConfig headArmourConfig;
        [SerializeField] ArmourConfig bodyArmourConfig;
        [SerializeField] ArmourConfig armArmourConfig;
        [SerializeField] ArmourConfig legArmourConfig;

        [SerializeField] ParticleSystem unarmouredHitParticleSystem;

        ArmourConfig armourConfigHit;

        public struct ArmourProtection
        {
            public float blade;
            public float blunt;
            public float pierce;
        }

        public ArmourProtection CalculateArmour()
        {
            ArmourProtection armourProtection = new ArmourProtection();

            armourConfigHit = DetermineLocationHit();

            if (armourConfigHit && IsArmourHit()) // Armour Bypassed (critical)
            {
                armourProtection.blade = armourConfigHit.GetBladeArmourAmount();
                armourProtection.blunt = armourConfigHit.GetBluntArmourAmount();
                armourProtection.pierce = armourConfigHit.GetPierceArmourAmount();
            }
            else
            {
                unarmouredHitParticleSystem.Play();
            }

            return armourProtection;
        }

        private ArmourConfig DetermineLocationHit()
        {
            var randomLocation = UnityEngine.Random.Range(0, 100);

            if (randomLocation <= 20) { return headArmourConfig; }
            else if (randomLocation <= 70) { return bodyArmourConfig; }
            else if (randomLocation <= 90) { return armArmourConfig; }
            else { return legArmourConfig; }
        }

        private bool IsArmourHit()
        {
            return UnityEngine.Random.Range(0f, 1f) <= armourConfigHit.GetArmourCoverage();
        }
    }
}
