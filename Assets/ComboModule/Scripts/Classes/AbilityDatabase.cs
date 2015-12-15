using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

[System.Serializable]
public class AbilityDatabase : ScriptableObject
{
    public List<AbilityData> combos;
    public string[] category;

    //-------------------------------------------------------------------
    public bool CheckName(string name)
    {
        if (combos.Count > 0)
        {
            for (int i = 0; i < combos.Count; i++)
                if (combos[i].name == name) return false;
        }
        return true;
    }
    //-------------------------------------------------------------------

    //-------------------------------------------------------------------

    //-------------------------------------------------------------------
    public void DuplicateAbilityData(int target)
    {
        AbilityData _target = new AbilityData(GetByIndex(target));
        _target.name = SetName(_target.name);
        combos.Add(_target);
    }
    public bool Swap(int indexA, int indexB)
    {
        if (indexA > -1 && indexA < combos.Count && indexB > -1 && indexB < combos.Count)
        {
            AbilityData temp = combos[indexA];
            combos[indexA] = combos[indexB];
            combos[indexB] = temp;
            return true;
        }
        else
            return false;
    }
    public int GetIndex(string abilityName)
    {
        for (int i = 0; i < this.combos.Count; i++)
        {
            if (this.combos[i].name == abilityName)
                return i;
        }
        Debug.LogWarning("Can't find index of " + abilityName);
        return 0;
    }

    private string SetName()
    {
        return SetName("New Ability");
    }
    private string SetName(string text)
    {
        int _nameIndex = 1;
        while (true)
        {
            if (!CheckName(text + " " + _nameIndex))
                _nameIndex++;
            else
                break;
        }
        return text + " " + _nameIndex;
    }

    public bool Create()
    {
        combos.Add(new AbilityData(SetName()));
        return true;
    }

    public bool Remove(AbilityData data)
    {
        return combos.Remove(data);
    }
    public bool RemoveAt(int index)
    {
        if (index < combos.Count)
        {
            combos.RemoveAt(index);
            return true;
        }
        else
            return false;
    }
    public AbilityData GetByIndex(int index)
    {
        return this.combos[index];
    }

    public AbilityData[] CreateInstance(string[] targetNames)
    {
        List<AbilityData> _array = new List<AbilityData>();
        for (int x = 0; x < targetNames.Length; x++)
        {
            for (int y = 0; y < this.combos.Count; y++)
            {
                if (targetNames[x] == this.combos[y].name)
                {
                    _array.Add(new AbilityData(this.combos[y]));
                    break;
                }
            }
        }
        return _array.ToArray();
    }
    public AbilityData[] CreateInstance()
    {
        AbilityData[] _array = new AbilityData[this.combos.Count];
        for (int i = 0; i < _array.Length; i++)
        {
            _array[i] = new AbilityData(this.combos[i]);
        }
        return _array;
    }

    public List<AbilityData> GetAll()
    {
        List<AbilityData> c = new List<AbilityData>();
        if (combos.Count > 0)
            foreach (AbilityData d in combos)
                c.Add(d);
        return c;
    }
    public List<string> GetNameAll()
    {
        List<string> n = new List<string>();
        if (combos.Count > 0)
            foreach (AbilityData d in combos)
                n.Add(d.name);
        return n;
    }
    public string GetName(int index)
    {
        return this.combos[index].name;
    }
    public string GetCategory(int index)
    {
        return this.combos[index].category;
    }
    public GameObject GetProjectile(int index)
    {
        return this.combos[index].projectile;
    }
    public float GetLink(int index)
    {
        return this.combos[index].link;
    }
    public float[] GetEventTimer(int index)
    {
        return this.combos[index].eventTimer.ToArray();
    }
    public float GetDamage(int index)
    {
        return this.combos[index].damage;
    }
    public bool SetDamage(int index, float value)
    {
        this.combos[index].damage = value;
        return true;
    }
    public float Getforce(int index)
    {
        return this.combos[index].force;
    }
    public ForceType GetForceTarget(int index)
    {
        return this.combos[index].forceType;
    }
    public float GetSpeed(int index)
    {
        return this.combos[index].speed;
    }
    public AudioClip GetSound(int index)
    {
        return this.combos[index].sound;
    }
}
