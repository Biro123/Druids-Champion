using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Characters
{
    [CreateAssetMenu(menuName = ("RPG/Special Ability/Area Effect"))]
    public class AreaEffectConfig : SpecialAbility
    {
        [Header("Area Effect Specific")]
        [SerializeField] float extraDamage = 10f;
        [SerializeField] float radius = 15f;

        public override void AttachComponentTo(GameObject gameObjectToAttachTo)
        {
            // Adds the ability Behaviour script to the player gameobject
            var behaviourComponent = gameObjectToAttachTo.AddComponent<AreaEffectBehaviour>();
            behaviourComponent.SetConfig(this);
            behaviour = behaviourComponent;
        }

        public float GetExtraDamage()
        {
            return extraDamage;
        }
        public float GetRadius()
        {
            return radius;
        }

    }
}
