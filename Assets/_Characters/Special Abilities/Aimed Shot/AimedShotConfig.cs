using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Characters
{
    [CreateAssetMenu(menuName = ("RPG/Special Ability/Aimed Shot"))]
    public class AimedShotConfig : SpecialAbility
    {
        [Header("Aimed Shot Specific")]
        [SerializeField] float extraDamage = 10f;

        public override void AttachComponentTo(GameObject gameObjectToAttachTo)
        {
            // Adds the ability Behaviour script to the player gameobject
            var behaviourComponent = gameObjectToAttachTo.AddComponent<AimedShotBehaviour>();
            behaviourComponent.SetConfig(this);
            behaviour = behaviourComponent;
        }

        public float GetExtraDamage()
        {
            return extraDamage;
        }

    }
}
