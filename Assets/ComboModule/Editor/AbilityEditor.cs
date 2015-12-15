using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections;
using System.Collections.Generic;

public class AbilityEditor : EditorWindow
{
    AbilityDatabase database;
    string[] guid;
    string[] assetNames;
    int editingIndex = 0;
    Vector2 scroll;
    public static AbilityEditor window;

    private Vector2 listScroll = Vector2.zero;
    private Vector2 editorScroll = Vector2.zero;
    float currentScrollViewHeight;
    float currentScrollViewWidth;
    float minScrollWidth = 140;
    float maxScrollWidth;
    bool resize = false;
    Rect cursorChangeRect;

    int lastSelectIndex = 0;

    [MenuItem("Window/Combo Data Editor")]
    static void Init()
    {
        window = (AbilityEditor)EditorWindow.GetWindow(typeof(AbilityEditor));
        window.titleContent.text = "Ability";
        window.Show();
    }

    void OnEnable()
    {
        //this.position = new Rect((Screen.currentResolution.width * 0.5f) - 200, (Screen.currentResolution.height * 0.5f) - 200, 400, 400);
        lastSelectIndex = 0;
        currentScrollViewHeight = this.position.height;
        currentScrollViewWidth = minScrollWidth;
        cursorChangeRect = new Rect(currentScrollViewWidth, 0f, 5f, currentScrollViewHeight);
    }

    private bool ResizeScrollView()
    {
        GUI.color = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).window.normal.textColor;
        GUI.DrawTexture(new Rect(cursorChangeRect.x, cursorChangeRect.y + EditorGUIUtility.singleLineHeight, 1f, position.height - EditorGUIUtility.singleLineHeight), EditorGUIUtility.whiteTexture);
        GUI.color = Color.white;
        EditorGUIUtility.AddCursorRect(new Rect(cursorChangeRect.x, cursorChangeRect.y, cursorChangeRect.width, cursorChangeRect.height), MouseCursor.ResizeHorizontal);

        if (cursorChangeRect.Contains(Event.current.mousePosition) && Event.current.type == EventType.mouseDown && Event.current.button == 0)
        {
            resize = true;
        }
        if (resize)
        {
            maxScrollWidth = position.width * 0.5f;
            float mouseX = Event.current.mousePosition.x;
            if (mouseX < minScrollWidth)
                currentScrollViewWidth = minScrollWidth;
            else if (mouseX > maxScrollWidth)
                currentScrollViewWidth = maxScrollWidth;
            else
                currentScrollViewWidth = mouseX;
            cursorChangeRect.Set(currentScrollViewWidth, cursorChangeRect.y, cursorChangeRect.width, cursorChangeRect.height);
        }
        if (Event.current.type == EventType.MouseUp)
            resize = false;
        return cursorChangeRect.Contains(Event.current.mousePosition);
    }

    void OnInspectorUpdate()
    {
        Repaint();
    }

    void OnDatabaseChange()
    {
        Debug.Log("Database has update");
        reorderableHitEventTimes = null;
        reorderableAbilities = null;
    }

    void OnGUI()
    {
        guid = AssetDatabase.FindAssets("l:Combo l:Database t:ScriptableObject");
        List<string> n = new List<string>();
        List<string> p = new List<string>();
        if (guid.Length > 0)
        {
            foreach (string id in guid)
            {
                string path = AssetDatabase.GUIDToAssetPath(id);
                char[] key = "/.".ToCharArray();
                string[] split = path.Split(key);
                string name = split[split.Length - 2];
                n.Add(name);
                p.Add(path);
            }
            assetNames = n.ToArray();
        }

        bool isResizing = ResizeScrollView();

        GUILayout.BeginArea(new Rect(0, 0, currentScrollViewWidth, position.height));
        GUILayout.BeginHorizontal();
        GUILayout.Box(string.Empty, EditorStyles.toolbarButton, GUILayout.Width(5f));
        if (guid.Length > 0)
        {
            editingIndex = EditorGUILayout.Popup(editingIndex, assetNames, EditorStyles.toolbarDropDown, GUILayout.Width(minScrollWidth - 10f));
            if (AssetDatabase.LoadAssetAtPath<AbilityDatabase>(p[editingIndex]) != database)
            {
                lastSelectIndex = 0;
                OnDatabaseChange();
                database = AssetDatabase.LoadAssetAtPath<AbilityDatabase>(p[editingIndex]);
            }
        }
        else
        {
            GUILayout.Label("Cant Find Asset", EditorStyles.toolbarDropDown);
            database = null;
            editingIndex = 0;
            lastSelectIndex = 0;
        }
        GUILayout.Box(string.Empty, EditorStyles.toolbarButton, GUILayout.Width(currentScrollViewWidth - (minScrollWidth - 15f)));
        GUILayout.EndHorizontal();

        listScroll = GUILayout.BeginScrollView(listScroll, GUILayout.Width(currentScrollViewWidth), GUILayout.Height(position.height - EditorGUIUtility.singleLineHeight));
        if (database != null)
            DrawAbilitySlot(database);
        GUILayout.EndScrollView();
        GUILayout.EndArea();
        GUILayout.BeginArea(new Rect(currentScrollViewWidth + 1, 0, position.width - currentScrollViewWidth - 1, position.height));

        GUILayout.BeginHorizontal();
        GUILayout.Box(string.Empty, EditorStyles.toolbarButton);
        GUILayout.EndHorizontal();

        editorScroll = GUILayout.BeginScrollView(editorScroll, GUILayout.Width(position.width - currentScrollViewWidth - 1), GUILayout.Height(position.height - EditorGUIUtility.singleLineHeight));
        if (database != null && database.combos != null && lastSelectIndex < database.combos.Count)
            DrawAbilityData(database.combos[lastSelectIndex]);
        GUILayout.EndScrollView();
        if (!isResizing && Event.current.type == EventType.mouseDown)
        {
            CleanSelect();
        }
        GUILayout.EndArea();

        Repaint();
    }

    private void CleanSelect()
    {
        EditorGUIUtility.editingTextField = false;
        HandleUtility.Repaint();
        Event.current.Use();
    }

    ReorderableList reorderableHitEventTimes;
    ReorderableList reorderableAbilities;

    void DrawReorderablelist(AbilityData data)
    {
        if (reorderableHitEventTimes == null)
        {
            reorderableHitEventTimes = new ReorderableList(database.combos[lastSelectIndex].eventTimer, typeof(float), true, false, true, true);
            reorderableHitEventTimes.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, "HitEvent Time");
            };
            reorderableHitEventTimes.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                Rect field = new Rect(rect.x, rect.y + 2f, rect.width, EditorGUIUtility.singleLineHeight);
                //database.combos[lastSelectIndex].eventTimer[index] = EditorGUI.FloatField(field, database.combos[lastSelectIndex].eventTimer[index]);
                database.combos[lastSelectIndex].eventTimer[index] = EditorGUI.Slider(field, database.combos[lastSelectIndex].eventTimer[index], 0f, 1f);
            };
        }
        reorderableHitEventTimes.DoLayoutList();
    }

    void OnDisable()
    {
        OnDatabaseChange();
    }

    void DrawAbilitySlot(AbilityDatabase database)
    {
        if (database.combos == null || database.combos.Count == 0)
        {
            if (Event.current.type == EventType.mouseDown && Event.current.button == 1)
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Create"), false, CreateAbilityData, null);
                menu.ShowAsContext();
                CleanSelect();
            }
            return;
        }

        if (reorderableAbilities == null)
        {
            reorderableAbilities = new ReorderableList(database.combos, typeof(AbilityData), true, false, true, true);
            reorderableAbilities.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                Rect field = new Rect(rect.x, rect.y + 2f, rect.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.LabelField(field, database.combos[index].name);
            };
            reorderableAbilities.onSelectCallback = (ReorderableList list) =>
              {

                  lastSelectIndex = list.index;
                  reorderableHitEventTimes = null;
              };
            reorderableAbilities.onAddDropdownCallback = (Rect rect, ReorderableList list) =>
              {
                  GenericMenu menu = new GenericMenu();
                  menu.AddItem(new GUIContent("Create"), false, CreateAbilityData, null);

                  if (list.index >= 0)
                  {
                      menu.AddSeparator("");
                      menu.AddItem(new GUIContent("Duplicate"), false, DuplicateAbilityData, list.index);
                  }
                  menu.ShowAsContext();
              };
        }
        reorderableAbilities.DoLayoutList();

        return;

        ////////////////////////////////////////////////////////////////
        //GUI Button Mode
        ////////////////////////////////////////////////////////////////
        //GUIStyle skin = new GUIStyle("ShurikenModuleTitle");

        //for (int i = 0; i < database.combos.Count; i++)
        //{
        //    GUILayout.BeginHorizontal();
        //    if (GUILayout.Button(database.combos[i].name, skin))
        //    {
        //        if (Event.current.button == 0)
        //        {
        //            lastSelectIndex = i;
        //            list = null;
        //        }
        //        else if (Event.current.button == 1)
        //        {
        //            GenericMenu menu = new GenericMenu();
        //            menu.AddItem(new GUIContent("Create"), false, CreateAbilityData, i);
        //            menu.AddItem(new GUIContent("Delete"), false, DeleteAbilityData, i);
        //            if (database.combos.Count > 1)
        //                menu.AddSeparator("");
        //            if (i > 0)
        //                menu.AddItem(new GUIContent("Move Up"), false, AbilityMoveUp, i);
        //            if (i < database.combos.Count - 1)
        //                menu.AddItem(new GUIContent("Move Down"), false, AbilityMoveDown, i);
        //            menu.ShowAsContext();
        //        }
        //        CleanSelect();
        //    }
        //    GUILayout.EndHorizontal();
        //}
    }

    #region GenericMenu Function
    void AbilityMoveUp(object obj)
    {
        if (!database) return;
        database.Swap((int)obj, (int)obj - 1);
        EditorUtility.SetDirty(database);
    }

    void AbilityMoveDown(object obj)
    {
        if (!database) return;
        database.Swap((int)obj, (int)obj + 1);
        EditorUtility.SetDirty(database);
    }

    void DuplicateAbilityData(object obj)
    {
        if (!database) return;
        reorderableHitEventTimes = null;
        database.DuplicateAbilityData((int)obj);
        EditorUtility.SetDirty(database);
    }

    void CreateAbilityData(object obj)
    {
        if (!database) return;
        reorderableHitEventTimes = null;
        database.Create();
        EditorUtility.SetDirty(database);
    }

    void DeleteAbilityData(object obj)
    {
        if (!database) return;
        database.RemoveAt((int)obj);
        EditorUtility.SetDirty(database);
    }
    #endregion

    void ShowHitEventPerview(AbilityData data)
    {
        GUIStyle progress = new GUIStyle("ProgressBarBack");
        GUIStyle action = new GUIStyle("ProgressBarBar");
        GUIStyle container = new GUIStyle("ProgressBarBack");
        GUIStyle background = new GUIStyle("AnimationEventBackground");
        GUIStyle info = new GUIStyle(EditorStyles.centeredGreyMiniLabel);
        float lineHeight = EditorGUIUtility.singleLineHeight;

        GUILayout.BeginArea(new Rect(0, lineHeight, position.width - currentScrollViewWidth, lineHeight * 3));
        Vector2 startPosition = new Vector2(10, 0);
        //startPosition.y += 2;
        GUI.Box(new Rect(0, startPosition.y, position.width - currentScrollViewWidth, lineHeight * 3), string.Empty, background);

        float width = position.width - currentScrollViewWidth - 20;
        float progressWidth = width * data.link;
        float actionWidth = width - progressWidth;

        startPosition.y += lineHeight;
        GUI.Box(new Rect(startPosition.x, startPosition.y, width, lineHeight), "", container);
        GUI.Box(new Rect(startPosition.x, startPosition.y, progressWidth, lineHeight), string.Empty, progress);
        GUI.Label(new Rect(startPosition.x, startPosition.y, progressWidth, lineHeight), "Progressing", info);
        GUI.Box(new Rect(startPosition.x + progressWidth, startPosition.y, actionWidth, lineHeight), "", action);
        GUI.Label(new Rect(startPosition.x + progressWidth, startPosition.y, actionWidth, lineHeight), "Linking", info);

        for (int e = 0; e < data.eventTimer.Count; e++)
        {
            float n = data.eventTimer[e];
            Texture image = EditorGUIUtility.Load("icons/Animation.EventMarker.png") as Texture2D;
            GUI.DrawTexture(new Rect(startPosition.x + (width * data.eventTimer[e]) - (1 * data.eventTimer[e]), startPosition.y + lineHeight, 5, 14), image);
        }
        GUILayout.EndArea();
        GUILayout.Space(lineHeight * 4);
    }

    void DrawAbilityData(AbilityData data)
    {
        bool error = false;
        //ShowHitEventPerview(data);
        GUIStyle line = new GUIStyle("sv_iconselector_sep");

        string _typeInName = EditorGUILayout.TextField("Name", data.name);
        if (database.CheckName(_typeInName))
            data.name = _typeInName;

        GUILayout.Box(string.Empty, line, GUILayout.ExpandWidth(true));

        data.projectile = EditorGUILayout.ObjectField("Effect", data.projectile, typeof(GameObject), false) as GameObject;
        data.sound = EditorGUILayout.ObjectField("Sound", data.sound, typeof(AudioClip), false) as AudioClip;
        data.damage = EditorGUILayout.FloatField("Damage", data.damage);
        GUILayout.Box(string.Empty, line, GUILayout.ExpandWidth(true));

        data.useKnockback = EditorGUILayout.BeginToggleGroup("Knockback", data.useKnockback);
        data.forceType = (ForceType)EditorGUILayout.EnumPopup("Force Type", data.forceType);
        data.force = EditorGUILayout.Slider("Knockback Force", data.force, 0f, 1f);
        EditorGUILayout.EndToggleGroup();

        data.overrideSpeed = EditorGUILayout.BeginToggleGroup("Attack Speed", data.overrideSpeed);
        data.speedType = (BlendingType)EditorGUILayout.EnumPopup("Blending Type", data.speedType);
        data.speed = EditorGUILayout.FloatField("Speed", data.speed);
        EditorGUILayout.EndToggleGroup();

        data.useDelay = EditorGUILayout.BeginToggleGroup("Delay", data.useDelay);
        data.duration = EditorGUILayout.FloatField("Duration", data.duration);
        data.slowType = (BlendingType)EditorGUILayout.EnumPopup("Blending Type", data.slowType);
        data.slowTimeScale = EditorGUILayout.FloatField("Time Scale", data.slowTimeScale);
        EditorGUILayout.EndToggleGroup();

        GUILayout.Box(string.Empty, line, GUILayout.ExpandWidth(true));

        data.link = EditorGUILayout.Slider("Link Time", data.link, 0f, 1f);
        DrawReorderablelist(data);
        GUILayout.Box(string.Empty, line, GUILayout.ExpandWidth(true));

        EditorUtility.SetDirty(database);
    }
}