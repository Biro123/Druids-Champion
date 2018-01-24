using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RPG.Characters
{
    public class Stamina : MonoBehaviour
    {
        [SerializeField] RawImage staminaBarImage;
        [SerializeField] float maxStamina = 100f;

        float currentStamina = 0f;

        public bool IsStaminaAvailable(float amount)
        {
            return (amount <= currentStamina);
        }

        public void UseStamina(float amount)
        {
            float newStamina = currentStamina - amount;
            currentStamina = Mathf.Clamp(newStamina, 0, maxStamina);
            SetStaminaBar(); 
        }

        // Use this for initialization
        void Start()
        {
            currentStamina = maxStamina;
            SetStaminaBar();
        }

        private void SetStaminaBar()
        {
            var staminaAsPercentage = currentStamina / maxStamina;
            float xValue = -(staminaAsPercentage / 2f) - 0.5f;
            staminaBarImage.uvRect = new Rect(xValue, 0f, 0.5f, 1f);
        }
    }
}

