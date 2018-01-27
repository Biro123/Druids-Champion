using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPG.Core;

namespace RPG.Characters
{
    public struct AbilityUseParams
    {
        public IDamageable target;
        public float baseDamage;

        public AbilityUseParams(IDamageable target, float baseDamage)
        { // Constructor
            this.target = target;
            this.baseDamage = baseDamage;
        }
    }

    public abstract class SpecialAbility : ScriptableObject
    {
        [Header("Special Ability General")]
        [SerializeField] float staminaCost = 20f;
        [SerializeField] GameObject particlePrefab = null;

        protected ISpecialAbility behaviour;  // protected allows only children to access it

        abstract public void AttachComponentTo(GameObject gameObjectToAttachTo);

        public void Use(AbilityUseParams useParams)
        {
            behaviour.Use(useParams);
        }

        public float GetStaminaCost()
        {
            return staminaCost;
        }

        public GameObject GetParticlePrefab()
        {
            return particlePrefab;
        }

    }

    public interface ISpecialAbility
    {
        void Use(AbilityUseParams useParams);
    }
}
