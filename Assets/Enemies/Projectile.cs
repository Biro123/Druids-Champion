using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {

    public float projectileSpeed = 10f;
    private float damage;

    public float GetDamage()
    {
        return damage;
    }

    public void SetDamage(float value)
    {
        damage = value;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Find component and see if damageable (Components may be null)
        Component damageableComponent = other.GetComponent(typeof(IDamageable));

        if (damageableComponent)
        {   // Cast as IDamageble because Player.TakeDamage inherits from both mono and IDamageable
            // So we need to tell it which to behave as.
            (damageableComponent as IDamageable).TakeDamage(damage);
        }
    }
}
