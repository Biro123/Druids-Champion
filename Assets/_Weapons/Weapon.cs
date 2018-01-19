using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace RPG.Weapons
{
    [CreateAssetMenu(menuName = ("RPG/Weapon"))]
    public class Weapon : ScriptableObject
    {

        public Transform gripTransform;

        [SerializeField] GameObject weaponPrefab;
        [SerializeField] AnimationClip attackAnimation;
        [SerializeField] float timeBetweenHits = 0.7f;
        [SerializeField] float attackRange = 2f;
        [SerializeField] float bladeDamageModifier  = 0.5f;
        [SerializeField] float bluntDamageModifier = 0.5f;
        [SerializeField] float pierceDamageModifier = 0.5f;

        public GameObject GetWeaponPrefab()
        {
            return weaponPrefab;
        }

        public AnimationClip GetAttackAnimClip()
        {
            RemoveAnimationEvents();
            return attackAnimation;
        }

        public float GetTimeBetweenHits()
        {
            // TODO consider whether to take animation time into account
            return timeBetweenHits;
        }

        public float GetAttackRange()
        {
            return attackRange;
        }

        // So that asset packs cannot cause bugs by expecting 'hit event' methods.
        private void RemoveAnimationEvents()
        {
            attackAnimation.events = new AnimationEvent[0];
        }
    }
}
