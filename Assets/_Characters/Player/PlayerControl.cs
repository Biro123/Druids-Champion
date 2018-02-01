using UnityEngine;
using RPG.CameraUI;    // For mouse events  

namespace RPG.Characters
{
    public class PlayerControl : MonoBehaviour
    {
        EnemyAI enemy;
        CameraRaycaster cameraRaycaster;
        SpecialAbilities specialAbilities;
        Character character;
        WeaponSystem weaponSystem;
        
        private void Start()
        {
            character = GetComponent<Character>();
            specialAbilities = GetComponent<SpecialAbilities>();
            weaponSystem = GetComponent<WeaponSystem>();

            RegisterForMouseEvents();
        }

        private void RegisterForMouseEvents()
        {
            // Subscribe to Raycaster's on click event.
            cameraRaycaster = Camera.main.GetComponent<CameraRaycaster>();
            cameraRaycaster.onMouseOverEnemy += OnMouseOverEnemy;
            cameraRaycaster.onMouseOverWalkable += OnMouseOverWalkable;
        }

        private void Update()
        {
            ScanForAbilityKeyDown();
        }

        private void ScanForAbilityKeyDown()
        {
            for (int keyIndex = 1; keyIndex <= specialAbilities.GetNumberOfAbilities(); keyIndex++)
            {
                if (Input.GetKeyDown(keyIndex.ToString()))
                {
                    specialAbilities.AttemptSpecialAbility(keyIndex - 1);
                }
            }
        }

        private void OnMouseOverWalkable(Vector3 targetLocation)
        {
            if (Input.GetMouseButton(0))
            {
                character.SetDestination(targetLocation);
            }
        }

        private void OnMouseOverEnemy(EnemyAI enemyToSet)
        {
            enemy = enemyToSet;
            if (Input.GetMouseButton(0) && IsInRange(enemy.gameObject))
            {
                weaponSystem.AttackTarget(enemy.gameObject);
            }

            if (Input.GetMouseButtonDown(1))
            {
                specialAbilities.AttemptSpecialAbility(0, enemy.gameObject);
            }
        }
              
        private bool IsInRange(GameObject target)
        {
            float distanceToTarget = (target.transform.position - this.transform.position).magnitude;
            return distanceToTarget <= weaponSystem.GetCurrentWeapon().GetAttackRange();
        }        
    }
}
