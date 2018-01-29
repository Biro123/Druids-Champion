using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Characters
{
    [CreateAssetMenu(menuName = ("RPG/Special Ability/Aimed Shot"))]
    public class AimedShotConfig : AbilityConfig
    {
        [Header("Aimed Shot Specific")]
        [SerializeField] float extraDamage = 100f;

        public override AbilityBehaviour GetBehaviour(GameObject objectToAttachTo)
        {
            return objectToAttachTo.AddComponent<AimedShotBehaviour>();
        }

        public float GetExtraDamage()
        {
            return extraDamage;
        }

    }
}
