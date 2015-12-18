using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
//using System.Collections;
using System.Collections.Generic;
using System.Linq;
//using System.Reflection;
//using UnityEditor.AnimatedValues;
//using UnityEditor.Animations;

[CustomEditor(typeof(ComboModule)), CanEditMultipleObjects]
[RequireComponent(typeof(Animator))]
public class ComboModuleEditor : GearsPropertyEditor
{
    private ComboModule me;
    private UnityEditor.Animations.AnimatorController animator;
    private SerializedProperty tagCount;
    private SerializedProperty chainOperator;
    private SerializedProperty chainBreakTime;
    private SerializedProperty driver;
    private SerializedProperty defaultStateName;
    private SerializedProperty dimenstionSetup;
    private SerializedProperty attackOffset;
    private SerializedProperty attackDirection;
    private SerializedProperty segments;
    private SerializedProperty size;
    private SerializedProperty affectLayer;
    private SerializedProperty height;
    private SerializedProperty onActive;

    private List<ReorderableList> abilitiesList = new List<ReorderableList>();

    protected override void Initialize()
    {
        me = (ComboModule)target;
        if (me.gameObject.GetComponent<Animator>())
            animator = me.gameObject.GetComponent<Animator>().runtimeAnimatorController as UnityEditor.Animations.AnimatorController;
        chainOperator = serializedObject.FindProperty("chainOperator");
        chainBreakTime = serializedObject.FindProperty("chainBreakTime");
        driver = serializedObject.FindProperty("driver");
        defaultStateName = serializedObject.FindProperty("defaultStateName");
        dimenstionSetup = serializedObject.FindProperty("dimenstionSetup");
        attackOffset = serializedObject.FindProperty("attackOffset");
        attackDirection = serializedObject.FindProperty("attackDirection");
        segments = serializedObject.FindProperty("segments");
        size = serializedObject.FindProperty("size");
        affectLayer = serializedObject.FindProperty("affectLayer");
        height = serializedObject.FindProperty("height");
        onActive = serializedObject.FindProperty("onActive");

        abilityDatabase = serializedObject.FindProperty("abilityDatabase");
        abilityDataStore = serializedObject.FindProperty("abilityDataStore");
        comboChainController = serializedObject.FindProperty("slots");
        signature = serializedObject.FindProperty("signature");
    }

    private SerializedProperty abilityDatabase;
    private SerializedProperty abilityDataStore;
    private SerializedProperty comboChainController;
    private SerializedProperty signature;
    private string lastDatabaseName;

    int selected = 0;
    int dragIndex = 0;

    private void FillReorderableComboChain(int targetIndex)
    {
        for (int i = abilitiesList.Count; i <= targetIndex; i++)
        {
            abilitiesList.Add(new ReorderableList(serializedObject, serializedObject.FindProperty("slots"), true, true, true, true));
        }
    }

    void StoreAbilityData()
    {

    }

    private void ReorderableComboChain()
    {
        if (!abilityDatabase.objectReferenceValue)
            return;
        //================================================================
        //Get states data like uniqueName
        //Set signature value to ComboModule
        string[] _statesSignature = GetAnimatorStatesSignature(animator, "layer", "/", "name");
        signature.arraySize = _statesSignature.Length;
        for (int i = 0; i < signature.arraySize; i++)
        {
            signature.GetArrayElementAtIndex(i).stringValue = _statesSignature[i];
        }
        //================================================================

        //================================================================
        string[] _abilityNames = new string[0];
        _abilityNames = ((AbilityDatabase)abilityDatabase.objectReferenceValue).GetNameAll().ToArray();
        //================================================================

        //================================================================
        //================================================================
        int _controllerCount = comboChainController.arraySize;

        if (abilitiesList.Count > _controllerCount)
        {
            Debug.LogWarning("Abilities List oversize");
            abilitiesList.Clear();
        }
        if (abilitiesList.Count < _controllerCount)
        {
            for (int i = 0; i < _controllerCount; i++)
            {
                abilitiesList.Add(new ReorderableList(serializedObject, serializedObject.FindProperty("slots").GetArrayElementAtIndex(i).FindPropertyRelative("combos"), true, true, true, true));
            }
        }

        //Draw ability slot
        if (abilitiesList.Count > 0)
        {
            for (int i = 0; i < abilitiesList.Count; i++)
            {
                var _comboChain = comboChainController.GetArrayElementAtIndex(i);

                abilitiesList[i].drawHeaderCallback =
                (Rect rect) =>
                {
                    EditorGUI.LabelField(rect, "Combo Chain " + i.ToString());
                };

                abilitiesList[i].drawElementCallback =
                (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    var _combo = abilitiesList[i].serializedProperty.GetArrayElementAtIndex(index);

                    int _selectedState = 0;
                    _selectedState = EditorGUI.Popup(new Rect(rect.x, rect.y, rect.width * 0.5f, EditorGUIUtility.singleLineHeight),
                         _statesSignature.ToList().IndexOf(_combo.FindPropertyRelative("signature").stringValue), _statesSignature);
                    if (_selectedState < 0)
                        _selectedState = 0;
                    else if (_selectedState > _statesSignature.Length)
                        _selectedState = _statesSignature.Length - 1;

                    _combo.FindPropertyRelative("signature").stringValue = _statesSignature[_selectedState];
                    _combo.FindPropertyRelative("state").objectReferenceValue = GetAnimatorStateByIndex(animator, _selectedState);
                    _combo.FindPropertyRelative("layer").intValue = GetAnimatorLayerIndexBySignature(animator, _statesSignature[_selectedState]);
                    _combo.FindPropertyRelative("defaultState").stringValue = GetAnimatorLayerDefaultStateByIndex(animator, _combo.FindPropertyRelative("layer").intValue);

                    rect.x += rect.width * 0.5f;

                    int _selectedAbility = 0;
                    _selectedAbility = EditorGUI.Popup(new Rect(rect.x, rect.y, rect.width * 0.5f, EditorGUIUtility.singleLineHeight),
                    _abilityNames.ToList().IndexOf(me.slots[i].combos[index].abilityName), _abilityNames);
                    if (_selectedAbility < 0)
                        _selectedAbility = 0;
                    else if (_selectedAbility > _abilityNames.Length)
                        _selectedAbility = _abilityNames.Length - 1;

                    _combo.FindPropertyRelative("abilityName").stringValue = _abilityNames[_selectedAbility];
                };

                abilitiesList[i].onAddDropdownCallback = (Rect rect, ReorderableList list) =>
                {
                    GenericMenu menu = new GenericMenu();
                    menu.AddItem(new GUIContent("New Chain"), false, CreateNewComboChain, list);
                    menu.AddSeparator("");
                    menu.AddItem(new GUIContent("Conditional Chain"), false, CreateConditionalComboChain, list);
                    menu.ShowAsContext();
                };

                abilitiesList[i].DoLayoutList();
            }
            EditorUtility.SetDirty(me);
        }
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        //return;
        GUILayout.Box(string.Empty, new GUIStyle("sv_iconselector_sep"), GUILayout.ExpandWidth(true));
        BeginEdit();

        EditorGUILayout.BeginHorizontal();

        GUIStyle style = new GUIStyle(EditorStyles.miniButton);
        GUILayout.Button("Dimension", style, GUILayout.MaxWidth(80f));
        GUILayout.Button("Setting", style, GUILayout.MaxWidth(80f));

        EditorGUILayout.EndHorizontal();

        if (animator)
        {
            defaultStateName.stringValue = GetDefaultState(animator);
            PropertyField(dimenstionSetup, "Dimension Setup");
            PropertyField(affectLayer, "Affect Layer");
            PropertyField(attackOffset, "Offset");
            PropertyField(attackDirection, "Direction");
            segments.intValue = Mathf.Clamp(segments.intValue, 2, 360);
            PropertyField(segments, "Segments");
            PropertyField(height, "Character Height");
            PropertyField(abilityDatabase, "Feed");
            if (abilityDatabase.objectReferenceValue)
                lastDatabaseName = abilityDatabase.objectReferenceValue.name;
            ReorderableComboChain();
        }
        else
        {
            EditorGUILayout.HelpBox("This Module Need Animator Component", MessageType.Error);
            if (GUILayout.Button("Reload"))
            {
                if (me.gameObject.GetComponent<Animator>())
                    animator = me.gameObject.GetComponent<Animator>().runtimeAnimatorController as UnityEditor.Animations.AnimatorController;
            }
        }
        //EditorGUILayout.PropertyField(onActive, new GUIContent("On Active"), true);
        EditorUtility.SetDirty(me);
        EndEdit();
        //DrawDefaultInspector();
    }

    void CreateNewComboChain(object obj)
    {
        ReorderableList l = (ReorderableList)obj;
        l.serializedProperty.InsertArrayElementAtIndex(l.serializedProperty.arraySize);
        serializedObject.ApplyModifiedProperties();
    }

    void CreateConditionalComboChain(object obj)
    {

    }

    void OnSceneGUI()
    {
        if (serializedObject != null)
        {
            //for (int i = 0; i < me.slots.Count; i++)
            //{
            //    if (me.slots[i].detail)
            //    {
            //        me.slots[i].angles = Mathf.Clamp(me.slots[i].angles, segments.intValue, 360);
            //        Vector3[] arc = new Vector3[0];
            //        if (me.GetArcHitboxPoints(attackOffset.vector3Value, attackDirection.intValue, me.slots[i].range, me.slots[i].angles, segments.intValue) != null)
            //            arc = me.GetArcHitboxPoints(attackOffset.vector3Value, attackDirection.intValue, me.slots[i].range, me.slots[i].angles, segments.intValue).ToArray();
            //        Handles.color = new Color(1, 0, 0, .8f);
            //        GUIStyle style = new GUIStyle(EditorStyles.whiteLargeLabel);
            //        style.normal.textColor = Color.red;
            //        Vector3 fix = me.transform.right * attackOffset.vector3Value.x + me.transform.up * attackOffset.vector3Value.y + me.transform.forward * attackOffset.vector3Value.z;
            //        if (me.dimenstionSetup == ComboModule.DimensionSetup._3D)
            //        {
            //            if (arc.Length > 0)
            //                Handles.DrawPolyLine(arc);
            //        }
            //        else
            //        {
            //            if (arc.Length > 0)
            //                Handles.DrawPolyLine(arc);
            //        }
            //    }
            //}
        }
    }
}
