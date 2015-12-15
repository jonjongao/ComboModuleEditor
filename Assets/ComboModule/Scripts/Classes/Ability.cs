using UnityEngine;
using System.Collections.Generic;
using UnityEditor.Animations;

namespace UnityEngine
{
    [System.Serializable]
    public class Ability
    {
        public int layer { get; set; }
        public AnimatorState state { get; set; }
        public AbilityData data { get; set; }

        public Ability(int layerIndex, AnimatorState animatorState, AbilityData abilityData)
        {
            layer = layerIndex;
            state = animatorState;
            data = abilityData;
        }
    }
}
