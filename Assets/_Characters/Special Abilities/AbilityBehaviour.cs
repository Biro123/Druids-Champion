using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Characters
{
    public abstract class AbilityBehaviour : MonoBehaviour
    {
        AbilityConfig config = null;

        // Abstract class is overridden in the child classes.
        public abstract void Use(AbilityUseParams useParams);
    }
}
