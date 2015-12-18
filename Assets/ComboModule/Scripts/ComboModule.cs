using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEditor.Animations;

public class ComboModule : MonoBehaviour
{
    public enum HitEventType { UpdateAndFollowTransform }
    public enum HitboxCheck { }
    public enum DimensionSetup { _2D, _3D };
    public enum ComboState : int { Idle = 0, Processing = 1, Linkable = 2, Finished = 3, Staggering = 4 }

    [Header("Mode")]
    public DimensionSetup dimenstionSetup;
    [Header("Usage")]
    public Driver driver;
    [Header("Basic Setting")]
    public Vector3 attackOffset = new Vector3(0f, 0.5f, 0f);
    public int attackDirection = 1;
    public int segments = 10;
    public float size = 1f;
    public float height;
    [Header("Combo Setting")]
    public ChainOperator chainOperator = ChainOperator.Auto;
    public float chainBreakTime = 0.5f;
    public float delayTime;
    public LayerMask affectLayer;
    public HitEventType hitEventType;
    [Header("Hit Test")]
    public Transform sweepTester;
    [Header("Combo Stats")]
    [SerializeField]
    protected ComboState comboState;
    public ComboState currentComboState { get { return comboState; } }
    public bool isStaggering;

    public string defaultStateName;

    public float stateTime;

    public float chainTime;
    public int defaultCombo = 0;
    public int ctrlIndex = 0;
    public int lastLayer = 0;
    public int LastLayer { get { return lastLayer; } }
    public int chainIndex = 0;
    public int totalChance;
    public bool active;
    public bool isActive { get { return active; } }
    public bool isDelaying;
    private List<float> invokedHitEvent = new List<float>();
    public float staggerDuration;

    public enum Driver { Player, AI }
    public enum ChainOperator { Auto, HoldActivator, PressActivator }
    public enum PlayType { Default, CrossFade }
    public enum AttackType { Melee, Range }
    public AbilityDatabase abilityDatabase;
    public List<ComboChainController> slots = new List<ComboChainController>();
    private Animator animator;
    public AbilityData[] abilityData;
    public string[] signature;

    AudioClip sfx;

    void BuildLocalAbilityData()
    {
        abilityData = abilityDatabase.CreateInstance(GetUsedAbilityNames());
        for (int i = 0; i < abilityData.Length; i++)
            abilityData[i].eventTimer.Sort();

        for (int x = 0; x < slots.Count; x++)
        {
            for (int y = 0; y < slots[x].combos.Count; y++)
            {
                slots[x].combos[y].data = GetAbilityDataByName(slots[x].combos[y].abilityName);
            }
        }
    }

    private string[] GetUsedAbilityNames()
    {
        List<string> _list = new List<string>();
        for (int x = 0; x < slots.Count; x++)
        {
            for (int y = 0; y < slots[x].combos.Count; y++)
            {
                _list.Add(slots[x].combos[y].abilityName);
            }
        }
        return _list.Distinct<string>().ToArray();
    }

    AbilityData GetAbilityDataByName(string name)
    {
        for (int i = 0; i < abilityData.Length; i++)
        {
            if (abilityData[i].name == name)
                return abilityData[i];
        }
        return null;
    }

    void Awake()
    {
        BuildLocalAbilityData();
        animator = GetComponent<Animator>();

        sfx = Resources.Load("gear_click") as AudioClip;
    }

    void Start()
    {
    }

    /// <summary>
    /// If activator is true, automatically play combo chain of current control index.
    /// </summary>
    public void SetActivator(bool input)
    {
        SetActivator(input, ctrlIndex);
    }

    public void SetActivator(bool input, int index)
    {
        if (input)
        {
            if (index != ctrlIndex)
            {
                if(comboState == ComboState.Idle || comboState == ComboState.Linkable)
                {
                    ctrlIndex = index;
                    PlayCombo();
                }
            }
            else
            {
                if (comboState == ComboState.Idle)
                {
                    PlayCombo();
                }
                else if (comboState == ComboState.Linkable)
                {
                    if (AddChain())
                        PlayCombo();
                    else
                    {
                        if (SetChain(0))
                            PlayCombo();
                    }
                }
            }
        }
    }

    bool SetChain(int index)
    {
        chainIndex = index;
        return true;
    }


    bool AddChain()
    {
        //Test sound
        GetComponent<AudioSource>().PlayOneShot(sfx);

        //Before final chain
        if (chainIndex < slots[ctrlIndex].combos.Count - 1)
        {
            chainIndex++;
            ResetHitEvent();
            return true;
        }
        //Final chain
        else if (chainIndex == slots[ctrlIndex].combos.Count - 1)
        {
            ResetHitEvent();
            return false;
        }
        //Out of index or less
        else
        {
            Debug.LogWarning("Force reset chain");
            chainIndex = 0;
            ResetHitEvent();
            return true;
        }
    }

    void ComboStateMachine()
    {
        if (comboState == ComboState.Idle)
        {

        }
        else if (comboState == ComboState.Processing)
        {
            stateTime = CurrentStateTime();
            HitEventMachine(stateTime);
            if (stateTime > GetCurrentChain().link)
            {
                comboState = (ComboState)2;
            }
        }
        else if (comboState == ComboState.Linkable)
        {
            stateTime = CurrentStateTime();
            HitEventMachine(stateTime);
            if (chainOperator == ChainOperator.Auto)
            {
                if (AddChain())
                {
                    PlayCombo();
                }
            }
            else if (chainOperator == ChainOperator.HoldActivator)
            {
            }
            else if (chainOperator == ChainOperator.PressActivator)
            {
            }

            if (stateTime > 1f)
            {
                comboState = (ComboState)3;
            }
        }
        else if (comboState == ComboState.Finished)
        {
            if (GetCurrentChain().useDelay)
            {
                stateTime = CurrentStateTime();
                if (stateTime > (1f + GetCurrentChain().duration))
                {
                    PlayDefault();
                }
                else
                {
                    animator.speed = GetCurrentChain().slowTimeScale;
                }
            }
            else
            {
                PlayDefault();
            }
        }
    }

    public bool ResetHitEvent()
    {
        invokedHitEvent.Clear();
        return true;
    }

    public void HitEventMachine(float stateTime)
    {
        float[] _times = GetCurrentChain().eventTimer.ToArray();

        for (int i = 0; i < _times.Length; i++)
        {
            if (stateTime > _times[i] && !invokedHitEvent.Contains(_times[i]))
            {
                OnHitEvent(GetCurrentChain());
                invokedHitEvent.Add(_times[i]);
            }
        }
    }

    void OnHitEvent(AbilityData data)
    {
        Debug.LogWarning("On HitEvent");
        if (data.projectile)
            Instantiate(data.projectile, transform.position + (transform.up * (height * 0.5f)), Quaternion.identity);
    }

    void FixedUpdate()
    {
        if (comboState != ComboState.Staggering)
        {
            ComboStateMachine();
        }
        else
        {
            if (staggerDuration < 0)
            {
                PlayDefault();
            }
            else
            {
                staggerDuration -= Time.deltaTime;
            }
        }
        //Debug.Log(animator.speed);
    }

    AbilityData GetCurrentChain()
    {
        return slots[ctrlIndex].combos[chainIndex].data;
    }
    Combo GetCombo(int controllerIndex, int comboIndex)
    {
        return slots[controllerIndex].combos[comboIndex];
    }

    float CurrentStateTime()
    {
        int _index = GetCombo(ctrlIndex, chainIndex).layer;
        string _name = GetCombo(ctrlIndex, chainIndex).state.name;
        //Debug.LogWarning(_name);
        if (animator.GetCurrentAnimatorStateInfo(_index).IsName(_name))
        {
            //Debug.Log("Is same");
            return animator.GetCurrentAnimatorStateInfo(GetCombo(ctrlIndex, chainIndex).layer).normalizedTime;
        }
        else
        {
            //if (animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
            //    return animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
            //else
            return 0f;
        }
    }

    void PlayDefault()
    {
        ctrlIndex = defaultCombo;
        chainIndex = 0;
        animator.speed = 1f;
        ResetHitEvent();
        //animator.Play(defaultStateName, 0, 0);
        for (int i = 0; i < slots[ctrlIndex].combos.Count; i++)
        {
            int _index = GetCombo(ctrlIndex, i).layer;
            string _default = GetCombo(ctrlIndex, i).defaultState;
            animator.Play(_default, _index, 0f);
        }
        Debug.Log("default");
        comboState = ComboState.Idle;
    }



    public bool PlayCombo()
    {
        int _index = GetCombo(ctrlIndex, chainIndex).layer;
        string _name = GetCombo(ctrlIndex, chainIndex).state.name;

        //Apply attack speed
        if (GetCurrentChain().overrideSpeed)
            animator.speed = GetCurrentChain().speed;
        else
            animator.speed = 1f;

        if (isDelaying)
            return false;

        if (!animator.GetCurrentAnimatorStateInfo(_index).IsName(_name))
        {
            animator.Play(_name, _index);
        }
        else
        {
            //Make sure this combo is on purpose
            if (chainIndex > 0 && _name ==
                GetCombo(ctrlIndex, chainIndex - 1).state.name)
            {
                Debug.Log("play combo with same name");
                animator.Play(_name, _index, 0f);
            }
            else
            {
                if (animator.GetCurrentAnimatorStateInfo(_index).normalizedTime > 1f)
                {
                    animator.Play(_name, _index, 0f);
                }
                else
                {
                    Debug.Log("combo fail");
                    return false;
                }
            }
        }
        comboState = (ComboState)1;
        return true;
    }

    public void SetStagger(float duration)
    {
        Debug.LogWarning("Set Stagger");
        staggerDuration = duration;
        comboState = (ComboState)4;
        isStaggering = true;
        animator.Play(defaultStateName, 0, 0f);
    }

    public void OnHitEvent()
    {
    }

    //bool HitEvent(int slotId)
    //{
    //    float timer = mecanim.GetCurrentAnimatorStateInfo(0).normalizedTime;
    //    //float timer = animationTime;
    //    //if (gameObject.tag == "Player")
    //    //    Debug.Log(timer);
    //    bool finisher = false;
    //    /*
    //     * State時間超過事件時間
    //     */
    //    if (lastHitEventTimer > GetCurrentCombo().data.eventTimer.Count - 1)
    //        lastHitEventTimer = 0;

    //    if (hitEventEnable &&
    //        lastHitEventTimer < GetCurrentCombo().data.eventTimer.Count &&
    //        timer > GetCurrentCombo().data.eventTimer[lastHitEventTimer] &&
    //        timer < GetCurrentCombo().data.link)
    //    {
    //        /*
    //         * 攻擊效果(無關擊中與否)
    //         */
    //        //if (slots[slotId].combo[comboChain].projectile)
    //        //Instantiate(slots[slotId].combo[comboChain].projectile, transform.position + (transform.up * (height * 0.5f)), Quaternion.identity);

    //        //Here play hit sfx, only play once in single hit
    //        if (GetCurrentCombo().data.sound)
    //        {
    //            //GameManager2dot0.PlaySFX(slots[id].combo[comboChain].sound);
    //            //onSound.Invoke(abilityDatabase.GetSound(slots[id].feedIndex[comboChain]));
    //        }

    //        if (IsFinalChain())
    //            finisher = true;

    //        //audio.PlayOneShot(slots[slotId].sounds[comboChain]);
    //        /*
    //         * 在此處放Raycast偵測
    //         */
    //        RaycastSweep(comboChain, attackOffset, attackDirection, slots[slotId].range, slots[slotId].angles, segments, finisher);

    //        //Debug.LogWarning(comboChain);
    //        /*
    //         * 最後一個HitEvent執行完後
    //         * 將在此重置
    //         */
    //        if (lastHitEventTimer < GetCurrentCombo().data.eventTimer.Count - 1)
    //            lastHitEventTimer++;
    //        else
    //        {
    //            lastHitEventTimer = 0;
    //            hitEventEnable = false;
    //        }

    //        return true;
    //    }
    //    else
    //    {
    //        //lastHitEvent = 0;
    //        return false;
    //    }
    //}

    //public List<Vector3> GetArcHitboxPoints(Vector3 attackOffset, int attackDirection, float attackRange, int attackAngle, int segments)
    //{
    //    if (segments <= 1 || segments > attackAngle)
    //        return null;
    //    int startAngle = Mathf.RoundToInt(-attackAngle * 0.5f);
    //    int finishAngle = Mathf.RoundToInt(attackAngle * 0.5f);
    //    int addOnAngle = Mathf.RoundToInt(attackAngle / segments);
    //    Vector3 startPos = transform.position;
    //    Vector3 targetDir = Vector3.zero;
    //    Vector3 fix = transform.right * attackOffset.x + transform.up * attackOffset.y + transform.forward * attackOffset.z;

    //    List<Vector3> points = new List<Vector3>();
    //    List<Vector3> l1 = new List<Vector3>();
    //    List<Vector3> l2 = new List<Vector3>();

    //    for (int i = startAngle; i <= finishAngle; i += addOnAngle)
    //    {
    //        if (dimenstionSetup == DimensionSetup._3D)
    //        {
    //            targetDir = (Quaternion.Euler(0, i, 0) * transform.forward).normalized;
    //            l1.Add((startPos + fix + ((size * .5f) * transform.up)) + (targetDir * attackRange));
    //            l2.Add((startPos + fix + ((size * .5f) * -transform.up)) + (targetDir * attackRange));
    //        }
    //        else
    //        {
    //            targetDir = (Quaternion.Euler(0, 0, i) * (transform.right * attackDirection)).normalized;
    //            l1.Add((startPos + fix) + (targetDir * attackRange));
    //        }
    //    }

    //    if (dimenstionSetup == DimensionSetup._3D)
    //    {
    //        for (int i = 0; i < l1.Count; i++)
    //        {
    //            points.Add(startPos + fix + ((size * .5f) * transform.up));
    //            points.Add(l1[i]);
    //            points.Add(l2[i]);
    //            points.Add(startPos + fix + ((size * .5f) * -transform.up));
    //            points.Add(startPos + fix + ((size * .5f) * transform.up));
    //            if (i + 1 < l1.Count)
    //            {
    //                points.Add(l1[i]);
    //                points.Add(l2[i]);
    //                points.Add(l2[i + 1]);
    //                points.Add(l1[i + 1]);
    //                points.Add(l1[i]);
    //                if (i + 1 == l1.Count - 1)
    //                {
    //                    points.Add(startPos + fix + ((size * .5f) * transform.up));
    //                    points.Add(l1[i + 1]);
    //                    points.Add(l2[i + 1]);
    //                    points.Add(startPos + fix + ((size * .5f) * -transform.up));
    //                    points.Add(startPos + fix + ((size * .5f) * transform.up));
    //                }
    //            }
    //        }
    //        return points;
    //    }
    //    else
    //    {
    //        for (int i = 0; i < l1.Count; i++)
    //        {
    //            points.Add(startPos + fix);
    //            points.Add(l1[i]);
    //            if (i + 1 < l1.Count)
    //            {
    //                points.Add(l1[i + 1]);
    //                if (i + 1 == l1.Count - 1)
    //                {
    //                    points.Add(startPos + fix);
    //                }
    //            }
    //        }
    //        return points;
    //    }
    //}

    //public void RaycastSweep(int combo, Vector3 attackOffset, int attackDirection, float attackRange, int attackAngle, int segments, bool isFinisher)
    //{
    //    //attackHits.Clear();
    //    List<GameObject> attackHits = new List<GameObject>();
    //    List<GameObject> list = new List<GameObject>();
    //    Vector3 startPos = transform.position;
    //    Vector3 targetPos = Vector3.zero;
    //    int startAngle = Mathf.RoundToInt(-attackAngle * 0.5f);
    //    int finishAngle = Mathf.RoundToInt(attackAngle * 0.5f);

    //    int addOnAngle = Mathf.RoundToInt(attackAngle / segments);

    //    RaycastHit[] hits;

    //    for (int i = startAngle; i <= finishAngle; i += addOnAngle)
    //    {
    //        if (dimenstionSetup == DimensionSetup._3D)
    //            targetPos = (Quaternion.Euler(0, i, 0) * transform.forward).normalized;
    //        else
    //            targetPos = (Quaternion.Euler(0, 0, i) * (transform.right * attackDirection)).normalized;
    //        Vector3 fix = transform.right * attackOffset.x + transform.up * attackOffset.y + transform.forward * attackOffset.z;
    //        hits = Physics.RaycastAll(startPos + fix, targetPos, attackRange, affectLayer);
    //        Debug.DrawRay(startPos + fix, targetPos * attackRange, Color.green, 2f);

    //        foreach (RaycastHit h in hits)
    //        {
    //            //if (h.collider.gameObject.tag == enemyTag)
    //            //{
    //            list.Add(h.collider.gameObject);
    //            //}
    //        }
    //    }

    //    if (dimenstionSetup == DimensionSetup._3D)
    //    {
    //        for (int i = startAngle; i <= finishAngle; i += addOnAngle * 2)
    //        {
    //            targetPos = (Quaternion.Euler(0, i, 45) * transform.forward).normalized;

    //            hits = Physics.RaycastAll(startPos + attackOffset, targetPos, attackRange, affectLayer);
    //            Debug.DrawRay(startPos + attackOffset, targetPos * attackRange, Color.green, 2f);

    //            foreach (RaycastHit h in hits)
    //            {
    //                list.Add(h.collider.gameObject);
    //            }
    //        }

    //        for (int i = startAngle; i <= finishAngle; i += addOnAngle * 2)
    //        {
    //            targetPos = (Quaternion.Euler(0, i, -45) * transform.forward).normalized;

    //            hits = Physics.RaycastAll(startPos + attackOffset, targetPos, attackRange, affectLayer);
    //            Debug.DrawRay(startPos + attackOffset, targetPos * attackRange, Color.green, 2f);

    //            foreach (RaycastHit h in hits)
    //            {
    //                list.Add(h.collider.gameObject);
    //            }
    //        }
    //    }

    //    if (list.Count > 0)
    //    {
    //        attackHits = list.Distinct().ToList();
    //        foreach (GameObject g in attackHits)
    //        {
    //            switch (slots[id].attackType)
    //            {
    //                //  如果是近身攻擊, 在此處產生粒子效果
    //                case AttackType.Melee:
    //                    Damage(g, combo, isFinisher);
    //                    //if (abilityDatabase.GetProjectile(slots[id].feedIndex[comboChain]))
    //                    //{
    //                    //    Instantiate(abilityDatabase.GetProjectile(slots[id].feedIndex[comboChain]), g.transform.position + (transform.up * (height * 0.5f)), Quaternion.identity);
    //                    //}
    //                    break;
    //                case AttackType.Range:
    //                    //if (abilityDatabase.GetProjectile(slots[id].feedIndex[comboChain]))
    //                    //{
    //                    //    GameObject r = Instantiate(abilityDatabase.GetProjectile(slots[id].feedIndex[comboChain]), transform.position + attackOffset + (transform.forward), Quaternion.identity) as GameObject;
    //                    //    //Projectile p = r.GetComponent<Projectile>();
    //                    //    //p.targetTag = g.tag;
    //                    //    //p.offset = attackOffset;
    //                    //    //p.id = combo;
    //                    //    //p.manager = this;
    //                    //    //p.target = g;
    //                    //}
    //                    break;
    //            }
    //        }
    //    }
    //}

    //public void Damage(GameObject target, int combo)
    //{
    //    Damage(target, combo, false);
    //}

    //public void Damage(GameObject target, int combo, bool isFinisher)
    //{
    //    AbilityData c = new AbilityData();
    //    c = abilityDatabase.GetByIndex(combo);
    //    if (isFinisher)
    //        c.isFinisher = true;

    //    Debug.Log(target);
    //    target.SendMessage("GetHit", c, SendMessageOptions.DontRequireReceiver);

    //    c.isFinisher = false;
    //    /*
    //     * Knockback Effect
    //     */
    //    float force = abilityDatabase.Getforce(combo);
    //    if (force > 0f)
    //    {
    //        if (target.GetComponent<NavMeshAgent>())
    //        {
    //            target.GetComponent<NavMeshAgent>().Move((target.transform.position - gameObject.transform.position).normalized * force);
    //        }
    //        else if (target.GetComponent<Rigidbody>())
    //        {
    //            target.GetComponent<Rigidbody>().AddForce((target.transform.position - gameObject.transform.position).normalized * (force * 30));
    //        }
    //        else if (target.GetComponent<CharacterController>())
    //        {
    //            target.GetComponent<CharacterController>().Move((target.transform.position - gameObject.transform.position).normalized * force);
    //        }
    //    }
    //    Debug.DrawRay(transform.position, (target.transform.position - gameObject.transform.position).normalized * 0.5f, Color.magenta, 5f);
    //}

    //public int CheckComboWeight(float range)
    //{
    //    int weight = 0;
    //    foreach (ComboChainController s in slots)
    //        if (range < s.range)
    //            weight++;
    //    return weight;
    //}

    //private int[] GetComboWithinRange(float range)
    //{
    //    List<int> pick = new List<int>();
    //    for (int i = 0; i < slots.Count; i++)
    //        if (slots[i].range > range)
    //            pick.Add(i);
    //    return pick.ToArray();
    //}

    //public int GetChooseComboWithinRange(float range)
    //{
    //    int[] pick = GetComboWithinRange(range);

    //    if (pick.Length > 0)
    //    {
    //        float pool = 0f;

    //        foreach (int p in pick)
    //            pool += slots[p].chance;

    //        for (int i = 0; i < pick.Length; i++)
    //        {
    //            float chance = Random.value * pool;
    //            if (chance < slots[pick[i]].chance)
    //            {
    //                return pick[i];
    //            }
    //            else
    //                pool -= slots[pick[i]].chance;
    //        }
    //    }
    //    return -1;
    //}

    //public int GetRandomComboWithinRange(float range)
    //{
    //    int[] pick = GetComboWithinRange(range);

    //    if (pick.Length > 0)
    //    {
    //        int choose = -1;
    //        while (choose == -1)
    //        {
    //            foreach (int c in pick)
    //                if (Random.value > 0.5f)
    //                    choose = c;
    //        }
    //        return choose;
    //    }
    //    else
    //        return -1;
    //}
}