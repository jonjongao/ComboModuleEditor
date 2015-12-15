using UnityEngine;
using System.Collections;
using UnityEditor;

public class AbilityDatabaseEditor
{
    private static string GetSavePath()
    {
        return EditorUtility.SaveFilePanelInProject("New Ability Database", "New Ability Database", "asset", "Create a new ability database.");
    }

    [MenuItem("Assets/Create/Ability Database", false,2)]
    public static void CreateDatabase()
    {
        string[] labels = new string[3] { "Database", "Abilities", "Ability" };
        string assetPath = GetSavePath();
        AbilityDatabase asset = ScriptableObject.CreateInstance("AbilityDatabase") as AbilityDatabase;  //scriptable object
        AssetDatabase.CreateAsset(asset, AssetDatabase.GenerateUniqueAssetPath(assetPath));
        AssetDatabase.SetLabels(asset, labels);
        AssetDatabase.Refresh();
    }
}
