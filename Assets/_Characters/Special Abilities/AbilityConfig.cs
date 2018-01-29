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

    public abstract class AbilityConfig : ScriptableObject
    {
        [Header("Special Ability General")]
        [SerializeField] float staminaCost = 20f;
        [SerializeField] GameObject particlePrefab = null;
        [SerializeField] AudioClip[] audioClips = null;

        protected AbilityBehaviour behaviour;  // protected allows only children to access it

        public abstract AbilityBehaviour GetBehaviour(GameObject objectToAttachTo);

        public void AttachAbilityTo(GameObject objectToAttachTo)
        {
            var behaviourComponent = GetBehaviour(objectToAttachTo);
            behaviourComponent.SetConfig(this);
            behaviour = behaviourComponent;
        }

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

        public AudioClip GetRandomAbilitySound()
        {
            return audioClips[Random.Range(0, audioClips.Length) ];
        }

    }
}
