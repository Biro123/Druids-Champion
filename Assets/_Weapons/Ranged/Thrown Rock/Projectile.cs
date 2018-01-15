using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPG.Core;      // TODO Consider re-wiring

namespace RPG.Weapons
{
    public class Projectile : MonoBehaviour
    {

        [SerializeField] float projectileSpeed = 10f;

        [SerializeField] GameObject shooter;  // Serialisable to inspect when paused
        private float damage;

        public void SetShooter(GameObject shooter)
        {
            this.shooter = shooter;
        }

        public float GetDamage()
        {
            return damage;
        }

        public void SetDamage(float value)
        {
            damage = value;
        }

        public float GetDefaultLaunchSpeed()
        {
            return projectileSpeed;
        }

        private void OnCollisionEnter(Collision collision)
        {
            // Find component and see if damageable (Components may be null)
            IDamageable damageableComponent = collision.collider.GetComponent<IDamageable>();

            if (shooter && shooter.layer != collision.gameObject.layer)  // Do not damage shooter (or its allies)
            {
                DamageIfDamageable(damageableComponent);
            }

        }

        private void DamageIfDamageable(IDamageable damageableComponent)
        {
            if (damageableComponent != null)
            {
                (damageableComponent as IDamageable).TakeDamage(damage);
            }
            Destroy(gameObject);
        }
    }
}
