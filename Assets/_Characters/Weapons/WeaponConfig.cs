﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace RPG.Characters
{
    [CreateAssetMenu(menuName = ("RPG/Weapon"))]
    public class WeaponConfig : ScriptableObject
    {

        public Transform gripTransform;

        [SerializeField] GameObject weaponPrefab;
        [SerializeField] AnimationClip attackAnimation;
        [Range(0.1f, 1.2f)] [SerializeField] float quality = 0.8f;
        [Range(0.1f, 1.0f)] [SerializeField] float condition = 0.8f;
        [SerializeField] float timeBetweenHits = 1f;
        [SerializeField] float damageDelay = 0.5f;
        [SerializeField] float attackRange = 2f;
        [Range(0f, 2.0f)] [SerializeField] float bladeDamageModifier = 0.5f;
        [Range(0f, 2.0f)] [SerializeField] float bluntDamageModifier = 0.5f;
        [Range(0f, 2.0f)] [SerializeField] float pierceDamageModifier = 0.5f;

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
            return timeBetweenHits;
        }

        public float GetDamageDelay()
        {
            return damageDelay;
        }

        public float GetAttackRange()
        {
            return attackRange;
        }

        public float GetBladeDamageModification()
        {
            return bladeDamageModifier * quality * condition;
        }

        public float GetBluntDamageModification()
        {
            return bluntDamageModifier * quality * condition;
        }

        public float GetPierceDamageModification()
        {
            return pierceDamageModifier * quality * condition;
        }

        public float GetChanceForSwing()
        {
            float mainSwingDamageMod = Mathf.Max(bladeDamageModifier, bluntDamageModifier);
            float chanceForSwing = mainSwingDamageMod / (mainSwingDamageMod + pierceDamageModifier);
            return chanceForSwing;
        }

        // So that asset packs cannot cause bugs by expecting 'hit event' methods.
        private void RemoveAnimationEvents()
        {
            attackAnimation.events = new AnimationEvent[0];
        }
    }
}
