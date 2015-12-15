using UnityEngine;
using System.Collections.Generic;
using UnityEditorInternal;

namespace UnityEngine
{
    [System.Serializable]
    public enum ForceType { Rigidbody, Transform, NavMeshAgent }
    [System.Serializable]
    public enum BlendingType { Override, Addition, Subtraction, Multiplication }

    [System.Serializable]
    public class AbilityData
    {
        public string name = "New Ability";
        //ArtAssets
        public AudioClip sound = null;
        public GameObject projectile = null;
        //ValueAssets
        [Range(0, 1)]
        public float link = 0.9f;
        [Range(0, 1)]
        public List<float> eventTimer = new List<float>() { 0.5f };
        [Multiline]
        public string category;
        public float damage = 0f;
        //Knockback
        public bool useKnockback = false;
        public ForceType forceType;
        public float force = 0f;
        //StateSpeed
        public bool overrideSpeed = false;
        public BlendingType speedType;
        public float speed = 1f;
        //Slowmotion
        public bool useDelay = false;
        public float duration = 1f;
        public BlendingType slowType;
        public float slowTimeScale = 0.3f;

        public AbilityData()
        {

        }

        public AbilityData(string name)
        {
            this.name = name;
        }

        public AbilityData(string name,
            AudioClip sound, GameObject projectile,
            float link, List<float> eventTimer, string category, float damage,
            bool useKnockback, ForceType forceType, float force,
            bool overrideSpeed, BlendingType speedType, float speed,
            bool useDelay, float time, BlendingType slowType, float slow)
        {
            this.name = name;
            this.sound = sound;
            this.projectile = projectile;
            this.link = link;
            this.eventTimer = eventTimer;
            this.category = category;
            this.damage = damage;
            this.useKnockback = useKnockback;
            this.forceType = forceType;
            this.force = force;
            this.overrideSpeed = overrideSpeed;
            this.speedType = speedType;
            this.speed = speed;
            this.useDelay = useDelay;
            this.duration = time;
            this.slowType = slowType;
            this.slowTimeScale = slow;
        }

        public AbilityData(AbilityData reference)
        {
            this.name = reference.name;
            this.sound = reference.sound;
            this.projectile = reference.projectile;
            this.link = reference.link;
            this.eventTimer = new List<float>();
            foreach (float i in reference.eventTimer)
                this.eventTimer.Add(i);
            this.category = reference.category;
            this.damage = reference.damage;
            this.useKnockback = reference.useKnockback;
            this.forceType = reference.forceType;
            this.force = reference.force;
            this.overrideSpeed = reference.overrideSpeed;
            this.speedType = reference.speedType;
            this.speed = reference.speed;
            this.useDelay = reference.useDelay;
            this.duration = reference.duration;
            this.slowType = reference.slowType;
            this.slowTimeScale = reference.slowTimeScale;
        }
    }
}