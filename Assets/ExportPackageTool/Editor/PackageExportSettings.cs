using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;

public class PackageExportSettings : ScriptableObject
{
    const string assetPath = "Assets/Editor/PackageExport/PackageExportSettings.asset";    

    [SerializeField]
    private string[] includeList = { };

    [SerializeField]
    private string[] excludeList = { };

    public string[] IncludeList { get { return includeList; } }
    public string[] ExcludeList { get { return excludeList; } }

    internal static PackageExportSettings GetOrCreateSettings()
    {
        var settings = AssetDatabase.LoadAssetAtPath<PackageExportSettings>(assetPath);
        if (settings == null)
        {
            settings = ScriptableObject.CreateInstance<PackageExportSettings>();            

            Directory.CreateDirectory("Assets/Editor/PackageExport/");

            AssetDatabase.CreateAsset(settings, assetPath);
            AssetDatabase.SaveAssets();
        }
        return settings;
    }

    internal static SerializedObject GetSerializedSettings()
    {
        return new SerializedObject(GetOrCreateSettings());
    }

    public static void ExportPackage(string[] exportList, string savePath)
    {
        AssetDatabase.ExportPackage(exportList, savePath, ExportPackageOptions.Default);

        // show it in file explorer. (GUI)
        EditorUtility.RevealInFinder(savePath);
    }
}

static class PackageExportSettingsProvider
{
    static SerializedObject settings;

    private class Provider : SettingsProvider
    {
        public Provider(string path, SettingsScope scope = SettingsScope.User) : base(path, scope) { }
        public override void OnGUI(string searchContext)
        {
            DrawPref();
        }
    }
    [SettingsProvider]
    static SettingsProvider MyNewPrefCode()
    {
        return new Provider("Preferences/PackageExport");
    }

    static void DrawPref()
    {
        if (settings == null)
        {
            settings = PackageExportSettings.GetSerializedSettings();
        }
        settings.Update();
        EditorGUI.BeginChangeCheck();
        
        var propertyIncludeList = settings.FindProperty("includeList");
        var propertyExcludeList = settings.FindProperty("excludeList");

        //GUILayout.Space(20);
        //GUI.skin.label.fontSize = 17;
        //GUILayout.Label("Facebook", GUILayout.Width(200), GUILayout.Height(30));       

        EditorGUILayout.PropertyField(propertyIncludeList, true);
        EditorGUILayout.PropertyField(propertyExcludeList, true);

        if (GUILayout.Button("Export Package..."))
            PackageExport.ShowExportWindow();        

        if (EditorGUI.EndChangeCheck())
        {
            settings.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
        }
    }    
}
