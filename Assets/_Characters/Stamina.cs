using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RPG.Characters
{
    public class Stamina : MonoBehaviour
    {
        [SerializeField] Image staminaBarImage;
        [SerializeField] float maxStamina = 100f;
        [SerializeField] float recovPerSecond = 5f;

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

        private void Update()
        {
            if (currentStamina < maxStamina)
            {
                RecoverStamina();
                SetStaminaBar();
            }
        }

        private void RecoverStamina()
        {
            float staminaToAdd = recovPerSecond * Time.deltaTime;
            currentStamina = Mathf.Clamp(currentStamina + staminaToAdd, 0, maxStamina);
        }

        private void SetStaminaBar()
        {
            var staminaAsPercentage = currentStamina / maxStamina;
            staminaBarImage.fillAmount = staminaAsPercentage;
        }
    }
}

