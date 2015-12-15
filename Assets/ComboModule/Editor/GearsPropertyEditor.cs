using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditorInternal;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEditor.Events;

public abstract class GearsPropertyEditor : Editor
{
    #region Public Variables
    #endregion

    #region Private Variables
    private static Rect beginPosition;
    #endregion

    #region Utility Function
    public static void SyncComboStates(UnityEditor.Animations.AnimatorController mecanim)
    {
        int layerCount = mecanim.layers.Length;
        for (int i = 0; i < layerCount; i++)
        {
            for (int ii = 0; ii < mecanim.layers[i].stateMachine.states.Length; ii++)
            {
                if (mecanim.layers[i].stateMachine.states[ii].state.tag == "Attack")
                {
                    foreach (AnimatorStateTransition t in mecanim.layers[i].stateMachine.states[ii].state.transitions)
                    {
                        Debug.LogWarning(t.destinationState);
                    }
                }
            }
        }
    }

    public static string GetDefaultState(UnityEditor.Animations.AnimatorController mecanim)
    {
        return mecanim.layers[0].stateMachine.defaultState.name;
    }

    public static string GetAnimatorLayerDefaultStateByIndex(UnityEditor.Animations.AnimatorController animator, int index)
    {
        return animator.layers[index].stateMachine.defaultState.name;
    }

    public static int GetAnimatorLayerIndexBySignature(UnityEditor.Animations.AnimatorController animator, string signature)
    {
        char[] key = "/".ToCharArray();
        string[] split = signature.Split(key);
        string layer = split[0];
        for (int l = 0; l < animator.layers.Length; l++)
        {
            if (animator.layers[l].name == layer)
                return l;
        }
        Debug.LogWarning("Fail to get state index by signature");
        return 0;
    }

    public static AnimatorState GetAnimatorStateByIndex(UnityEditor.Animations.AnimatorController animator, int index)
    {
        AnimatorState[] _states = GetAnimatorStates(animator);
        if (_states[index] != null)
            return _states[index];
        else
            return null;
    }

    public static AnimatorState GetAnimatorState(UnityEditor.Animations.AnimatorController animator, string name)
    {
        for (int l = 0; l < animator.layers.Length; l++)
        {
            for (int s = 0; s < animator.layers[l].stateMachine.states.Length; s++)
            {
                if (animator.layers[l].stateMachine.states[s].state.name == name)
                    return animator.layers[l].stateMachine.states[s].state;
            }
        }
        return null;
    }

    public static int GetLayerIndexByName(UnityEditor.Animations.AnimatorController animator, string name)
    {
        List<string> _layer = new List<string>();
        for (int l = 0; l < animator.layers.Length; l++)
        {
            if (animator.layers[l].name == name)
                return l;
        }
        Debug.LogError("Can't find '" + name + "' layer");
        return 0;
    }

    public static AnimatorState[] GetAnimatorStates(UnityEditor.Animations.AnimatorController animator)
    {
        List<AnimatorState> _animatorState = new List<AnimatorState>();
        for (int l = 0; l < animator.layers.Length; l++)
        {
            for (int s = 0; s < animator.layers[l].stateMachine.states.Length; s++)
            {
                if (animator.layers[l].stateMachine.states[s].state)
                    _animatorState.Add(animator.layers[l].stateMachine.states[s].state);
            }
        }
        return _animatorState.ToArray();
    }

    public static AnimatorState[] GetAnimatorStates(UnityEditor.Animations.AnimatorController animator, int index)
    {
        AnimatorStateMachine _stateMachine = animator.layers[index].stateMachine;
        ChildAnimatorState[] _childAnimatorState = _stateMachine.states;
        AnimatorState[] _animatorState = new AnimatorState[_childAnimatorState.Length];
        for (int i = 0; i < _childAnimatorState.Length; i++)
        {
            _animatorState[i] = _childAnimatorState[i].state;
        }
        return _animatorState;
    }
    public static AnimatorState[] GetAnimatorStates(Animator animator, int index)
    {
        UnityEditor.Animations.AnimatorController _controller = animator.runtimeAnimatorController as UnityEditor.Animations.AnimatorController;
        return GetAnimatorStates(_controller, index);
    }

    public static string[] GetAnimatorStatesSignature(UnityEditor.Animations.AnimatorController animator)
    {
        return GetAnimatorStatesSignature(animator, "layer", "name");
    }

    public static string[] GetAnimatorStatesSignature(UnityEditor.Animations.AnimatorController animator, params string[] layout)
    {
        string[] _name = GetAnimatorStatesName(animator);
        int[] _index = GetAnimatorStatesLayerIndex(animator);
        string[] _layer = GetAnimatorStatesLayerName(animator);
        if (_index.Length != _name.Length) { Debug.LogWarning("Result List of Name and Index not match"); return null; }
        if (_layer.Length != _name.Length) { Debug.LogWarning("Result List of Name and Layer not match"); return null; }
        string[] _out = new string[_name.Length];
        for (int i = 0; i < _out.Length; i++)
        {
            foreach (string s in layout)
            {
                if (s == "name")
                    _out[i] += _name[i];
                else if (s == "index")
                    _out[i] += _index[i].ToString();
                else if (s == "layer")
                    _out[i] += _layer[i].ToString();
                else
                    _out[i] += s;
            }
        }
        return _out;
    }

    public static string[] GetAnimatorStatesLayerName(UnityEditor.Animations.AnimatorController animator)
    {
        List<string> _layer = new List<string>();
        for (int l = 0; l < animator.layers.Length; l++)
        {
            for (int s = 0; s < animator.layers[l].stateMachine.states.Length; s++)
            {
                _layer.Add(animator.layers[l].name);
            }
        }
        return _layer.ToArray();
    }

    public static int[] GetAnimatorStatesLayerIndex(UnityEditor.Animations.AnimatorController animator)
    {
        List<int> _index = new List<int>();
        for (int l = 0; l < animator.layers.Length; l++)
        {
            for (int s = 0; s < animator.layers[l].stateMachine.states.Length; s++)
            {
                _index.Add(l);
            }
        }
        return _index.ToArray();
    }

    public static string[] GetAnimatorStatesName(UnityEditor.Animations.AnimatorController animator)
    {
        List<string> _name = new List<string>();
        for (int l = 0; l < animator.layers.Length; l++)
        {
            for (int s = 0; s < animator.layers[l].stateMachine.states.Length; s++)
            {
                _name.Add(animator.layers[l].stateMachine.states[s].state.name);
            }
        }
        //_name = _name.Distinct();
        return _name.ToArray();
    }

    private static void GenericComboManagingController(SerializedProperty usage, SerializedProperty linkType, SerializedProperty chainBreakTime, List<ComboChainController> slots)
    {
        EditorGUILayout.BeginHorizontal();
        usage.enumValueIndex = EditorGUILayout.Popup("Usage", usage.enumValueIndex, usage.enumNames, EditorStyles.popup);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        linkType.enumValueIndex = EditorGUILayout.Popup("Chain Link Type", linkType.enumValueIndex, linkType.enumNames, EditorStyles.popup);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        chainBreakTime.floatValue = EditorGUILayout.FloatField("Chain Break Time", chainBreakTime.floatValue);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add New Combo"))
        {
            ComboChainController newSlot = new ComboChainController();
            newSlot.tag = "Default" + (slots.Count + 1);
            slots.Add(newSlot);
        }
        if (GUILayout.Button("Remove Last Combo"))
        {
            if (slots.Count > 0)
                slots.RemoveAt(slots.Count - 1);
        }
        EditorGUILayout.EndHorizontal();
    }

    public enum FoldoutTitleLayoutOption { Left, Mid, Right, All }
    public static bool ButtonToggleLayout(bool toggle, string label, FoldoutTitleLayoutOption option)
    {
        return ButtonToggleLayout(toggle, label, label, option);
    }

    public static bool ButtonToggleLayout(bool toggle, string enableSymbol, string disableSymbol, FoldoutTitleLayoutOption option)
    {
        GUIStyle style;
        if (option == FoldoutTitleLayoutOption.Left)
            style = EditorStyles.miniButtonLeft;
        else if (option == FoldoutTitleLayoutOption.Mid)
        {
            style = new GUIStyle(EditorStyles.miniButtonMid);
            style.alignment = TextAnchor.MiddleLeft;
        }
        else if (option == FoldoutTitleLayoutOption.Right)
        {
            style = new GUIStyle(EditorStyles.miniButtonRight);
            style.alignment = TextAnchor.MiddleLeft;
        }
        else
        {
            style = new GUIStyle(EditorStyles.miniButton);
            style.alignment = TextAnchor.MiddleLeft;
            style.padding.left = 31;
        }

        if (option == FoldoutTitleLayoutOption.Left)
        {
            if (toggle)
            {
                if (GUILayout.Button(enableSymbol, style, GUILayout.Width(25f))) toggle = false;
            }
            else
            {
                if (GUILayout.Button(disableSymbol, style, GUILayout.Width(25f))) toggle = true;
            }
        }
        else
        {
            if (toggle)
            {
                if (GUILayout.Button(enableSymbol, style)) toggle = false;
            }
            else
            {
                if (GUILayout.Button(disableSymbol, style)) toggle = true;
            }
        }
        return toggle;
    }
    #endregion

    #region Main Function
    protected SerializedObject serializedObject;
    private static GUIStyle commentStyle = null;
    private static bool cameraRendered = false;

    protected abstract void Initialize();

    public void BeginEdit()
    {
        if (serializedObject != null && serializedObject.targetObject == target)
        {
            serializedObject.Update();
            return;
        }

        serializedObject = new SerializedObject(target);

        Initialize();
    }

    public void EndEdit()
    {
        serializedObject.ApplyModifiedProperties();
    }

    public static GUIStyle CommentStyle
    {
        get
        {
            if (commentStyle == null)
            {
                commentStyle = new GUIStyle(GUI.skin.GetStyle("Box"));
                commentStyle.font = EditorStyles.miniFont;
                commentStyle.alignment = TextAnchor.UpperLeft;
            }

            return commentStyle;
        }
    }

    protected void ArrayProperty(string label, string name, SerializedObject target)
    {
        int size = target.FindProperty(name + ".Array.size").intValue;
        BeginSection(label + " x " + size);
        if (size > 0)
        {
            for (int i = 0; i < size; i++)
            {
                var prop = target.FindProperty(string.Format("{0}.Array.data[{1}]", name, i));
                EditorGUILayout.PropertyField(prop);
            }
        }
    }

    #region Property Field
    protected void PropertyField(SerializedProperty property, string label, bool includeChildren, params GUILayoutOption[] options)
    {
        if (string.IsNullOrEmpty(label))
        {
            EditorGUILayout.PropertyField(property, includeChildren, options);
        }
        else
        {
            EditorGUILayout.PropertyField(property, new GUIContent(label), includeChildren, options);
        }
    }

    protected void PropertyField(SerializedProperty property, string label, params GUILayoutOption[] options)
    {
        PropertyField(property, label, false, options);
    }

    protected void PropertyField(SerializedProperty property, bool includeChildren, params GUILayoutOption[] options)
    {
        PropertyField(property, null, includeChildren, options);
    }

    protected void PropertyField(SerializedProperty property, params GUILayoutOption[] options)
    {
        PropertyField(property, null, false, options);
    }
    #endregion

    protected void FloatPropertyField(SerializedProperty property, params GUILayoutOption[] options)
    {
        float newValue = EditorGUILayout.FloatField(property.floatValue, options);
        if (newValue != property.floatValue)
        {
            property.floatValue = newValue;
        }
    }

    protected void StringPropertyField(SerializedProperty property, params GUILayoutOption[] options)
    {
        string newValue = EditorGUILayout.TextField(property.stringValue, options);
        if (newValue != property.stringValue)
        {
            property.stringValue = newValue;
        }
    }

    protected void TexturePropertyField(SerializedProperty property, params GUILayoutOption[] options)
    {
        Object newValue = EditorGUILayout.ObjectField(property.objectReferenceValue, typeof(Texture2D), false, options);
        if (newValue != property.objectReferenceValue)
        {
            property.objectReferenceValue = newValue;
        }
    }

    protected void MinMaxPropertySlider(SerializedProperty minProperty, SerializedProperty maxProperty, float minCap, float maxCap, params GUILayoutOption[] options)
    {
        float newMin = minProperty.floatValue, newMax = maxProperty.floatValue;
        EditorGUILayout.MinMaxSlider(ref newMin, ref newMax, minCap, maxCap, options);

        if (newMin != minProperty.floatValue || newMax != maxProperty.floatValue)
        {
            minProperty.floatValue = newMin;
            maxProperty.floatValue = newMax;
        }
    }

    protected void MinMaxPropertySliderFields(string label, SerializedProperty minProperty, SerializedProperty maxProperty, float minCap, float maxCap, params GUILayoutOption[] options)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Space(5.0f);
        Rect labelRect = GUILayoutUtility.GetRect(new GUIContent(label), EditorStyles.boldLabel);
        GUI.Label(labelRect, label, minProperty.prefabOverride || maxProperty.prefabOverride ? EditorStyles.boldLabel : EditorStyles.label);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Space(15.0f);
        FloatPropertyField(minProperty, GUILayout.Width(40.0f));
        MinMaxPropertySlider(minProperty, maxProperty, minCap, maxCap, options);
        FloatPropertyField(maxProperty, GUILayout.Width(40.0f));
        GUILayout.EndHorizontal();
    }

    public static void WideComment(string comment)
    {
        GUILayout.Box(comment, CommentStyle, GUILayout.ExpandWidth(true));
    }

    public static void Comment(string comment)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Space(105.0f);
        WideComment(comment);
        GUILayout.EndHorizontal();
    }

    public static void Header(string label)
    {
        GUILayout.Label(label, EditorStyles.boldLabel);
    }

    public static void BeginSection(string label)
    {
        Header(label);
    }

    public static void EndSection()
    {
        EditorGUILayout.Space();
        EditorGUILayout.Space();
    }

    public virtual bool RenderSceneHandles
    {
        get
        {
            return true;
        }
    }

    public virtual Color SceneHandlesColor
    {
        get
        {
            return Color.green;
        }
    }

    public Transform TargetTransform
    {
        get
        {
            return ((Component)target).transform;
        }
    }

    //public void OnPreSceneGUI()
    //{
    //    cameraRendered = false;
    //}

    public static float AngularSlider(Vector3 position, Vector3 forward, Vector3 right, Vector3 up, float angle, float radius, Handles.DrawCapFunction capFunction, float offset = 0.0f, float handleSize = 1.0f)
    {
        Vector3 angleVector = PlanarAngleVector(forward, right, angle) * radius;
        Vector3 directionVector = Vector3.Cross(angleVector, up) * -1;
        Vector3 sliderPosition = position + angleVector + angleVector.normalized * offset;
        Vector3 changeVector = Handles.Slider(sliderPosition, directionVector, handleSize, capFunction, 1.0f) - sliderPosition;
        return angle + (Vector3.Angle(directionVector, changeVector) > 90.0f ? changeVector.magnitude * -1.0f : changeVector.magnitude);
    }

    public static float AngularSlider(Vector3 position, Vector3 forward, Vector3 right, Vector3 up, float angle, float radius, float offset = 0.0f)
    {
        return AngularSlider(position, forward, right, up, angle, radius, Handles.ArrowCap, offset, HandleUtility.GetHandleSize(position));
    }

    public static float AngularSlider(Transform transform, float angle, float radius, float offset = 0.0f)
    {
        return AngularSlider(transform.position, transform.forward, transform.right, transform.up, angle, radius, offset);
    }

    public static void DrawThickWireArc(Vector3 position, Vector3 forward, Vector3 up, float angle, float radius, int thickness, float resolution)
    {
        for (int i = 0; i < thickness; i++)
        {
            Handles.DrawWireArc(
                position,
                up,
                forward,
                angle,
                radius + resolution * (float)i * HandleUtility.GetHandleSize(position)
            );
        }
    }

    public static void DrawThickWireArc(Transform transform, float angle, float radius, int thickness, float resolution)
    {
        DrawThickWireArc(transform.position, transform.forward, transform.up, angle, radius, thickness, resolution);
    }

    public static Vector3 PlanarAngleVector(Vector3 forward, Vector3 right, float angle)
    {
        if (angle < 90.0f)
        {
            return Vector3.Slerp(
                forward,
                right,
                angle / 90.0f
            );
        }
        else if (angle < 180.0f)
        {
            return Vector3.Slerp(
                right,
                forward * -1.0f,
                (angle - 90.0f) / 90.0f
            );
        }
        else if (angle < 270.0f)
        {
            return Vector3.Slerp(
                forward * -1.0f,
                right * -1.0f,
                (angle - 180.0f) / 90.0f
            );
        }
        else
        {
            return Vector3.Slerp(
                right * -1.0f,
                forward,
                (angle - 270.0f) / 90.0f
            );
        }
    }


    public static void MinMaxRadiusHandle(Transform transform, ref float min, ref float max, float minClamp, float maxClamp)
    {
        min = Mathf.Clamp(Handles.RadiusHandle(transform.rotation, transform.position, min), minClamp, max);
        max = Mathf.Clamp(Handles.RadiusHandle(transform.rotation * Quaternion.AngleAxis(45.0f, transform.up), transform.position, max), min, maxClamp);
    }
    #endregion
}