using UnityEngine;
using System.Collections;
using UnityEditor.Animations;

namespace UnityEngine
{
    [System.Serializable]
    public class Combo
    {
        public string signature;
        public string abilityName;
        public string defaultState;
        public int layer;
        public AnimatorState state;
        public AbilityData data;
        //public Ability ability;
        //public UnityEditor.Animations.AnimatorState preState;

        //public Combo(string signature,string abilityName,string defaultState, UnityEditor.Animations.AnimatorState preState, Ability ability) : base(ability.layer, ability.state, ability.data)
        //{
        //    this.signature = signature;
        //    this.abilityName = abilityName;
        //    this.defaultState = defaultState;
        //    this.state = preState;
        //}
    }
}