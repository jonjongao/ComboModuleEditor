using UnityEngine;
using System.Collections.Generic;

namespace UnityEngine
{
    [System.Serializable]
    public class ComboChainController
    {
        public string name = "New Combo Chain";
        public string tag = string.Empty;
        public bool slotActive = false;
        public bool detail = true;
        public int chain = 0;
        public int chance = 100;
        public float transitionDuration = 0f;
        public float range = 2f;
        public float delay = 0.5f;
        [Range(0,360)]
        public int angles = 90;
        public ComboModule.PlayType playMode = ComboModule.PlayType.Default;
        public ComboModule.AttackType attackType = ComboModule.AttackType.Melee;
        public List<Combo> combos = new List<Combo>();
    }
}
