using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RPG.CameraUI;

namespace RPG.Characters
{
    public class Stamina : MonoBehaviour
    {
        [SerializeField] RawImage staminaBarImage;
        [SerializeField] float maxStamina = 100f;
        [SerializeField] float costPerHit = 5f;

        float currentStamina = 0f;

        CameraRaycaster cameraRaycaster;

        // Use this for initialization
        void Start()
        {
            currentStamina = maxStamina;
            SetStaminaBar();
            RegisterForRightClick();
        }

        private void RegisterForRightClick()
        {
            // Subscribe to Raycaster's on click event.
            cameraRaycaster = Camera.main.GetComponent<CameraRaycaster>();
            cameraRaycaster.onMouseOverEnemy += OnMouseOverEnemy;
        }

        void OnMouseOverEnemy (Enemy enemy)
        {
            if (Input.GetMouseButtonDown(1))
            {
                float newStamina = currentStamina - costPerHit;
                currentStamina = Mathf.Clamp(newStamina, 0, maxStamina);
                SetStaminaBar();
            }
        }

        private void SetStaminaBar()
        {
            var staminaAsPercentage = currentStamina / maxStamina;
            float xValue = -(staminaAsPercentage / 2f) - 0.5f;
            staminaBarImage.uvRect = new Rect(xValue, 0f, 0.5f, 1f);
        }
    }
}

