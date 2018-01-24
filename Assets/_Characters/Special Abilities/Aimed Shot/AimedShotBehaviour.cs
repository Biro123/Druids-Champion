using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace RPG.Characters
{
    public class AimedShotBehaviour : MonoBehaviour, ISpecialAbility 
    {
        AimedShotConfig config;

        public void SetConfig(AimedShotConfig configToSet)
        {
            this.config = configToSet;
        }

        public void Use()
        {

        }

    }
}
