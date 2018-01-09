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
        IDamageable damageableComponent = other.GetComponent<IDamageable>();

        if (damageableComponent != null)
        {   
            (damageableComponent as IDamageable).TakeDamage(damage);
        }
    }
}
