using UnityEngine;
using System.Collections.Generic;
using UnityEditor.Animations;

namespace UnityEngine
{
    [System.Serializable]
    public struct AnimatorLayer
    {
        [SerializeField]
        public AnimatorState defaultState;
        [SerializeField]
        public AnimatorStateMachine stateMachine;
        [SerializeField]
        public AnimatorState[] animatorState;
    }

    
}
