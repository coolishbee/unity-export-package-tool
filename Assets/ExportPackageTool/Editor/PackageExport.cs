using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using System.IO;
using System.Linq;

public class PackageExport : EditorWindow
{
    private ExportTreeView mTreeView;
    private TreeViewState mTreeViewState;

    public List<string> exportList = new List<string>();
    public string packageName = "";


    public static readonly string PROJECT_PATH =
            Application.dataPath.Remove(Application.dataPath.Length - 6, 6);


    internal static class Styles
    {
        public static GUIStyle title = "LargeBoldLabel";
        public static GUIStyle bottomBarBg = "ProjectBrowserBottomBarBg";
        public static GUIStyle topBarBg = "OT TopBar";
        public static GUIStyle loadingTextStyle = "CenteredLabel";
        public static GUIContent allText = EditorGUIUtility.TrTextContent("All");
        public static GUIContent noneText = EditorGUIUtility.TrTextContent("None");
        public static GUIContent includeDependenciesText = EditorGUIUtility.TrTextContent("Include dependencies");
    }

    /* Setter & Getter */

    /* Functions */

    public PackageExport()
    {
        // Initial pos and minsize
        position = new Rect(100, 100, 400, 300);
        minSize = new Vector2(350, 350);
        titleContent = new GUIContent("Export package");
    }

    public static void ShowExportWindow()
    {
        var window = CreateInstance<PackageExport>();
        window.ShowUtility();

        var assetPaths = new List<string>();
        foreach (var assetFolder in PackageExportSettings.GetOrCreateSettings().IncludeList)
        {
            //if (PackageExportSettings.GetOrCreateSettings().ExcludeList.Any(x =>
            //    Path.GetFullPath(Path.Combine(PROJECT_PATH, assetFolder))
            //        .Contains(Path.GetFullPath(Path.Combine(PROJECT_PATH, x)))))
            //{
            //    Debug.Log(Path.GetFullPath(Path.Combine(PROJECT_PATH, assetFolder)));
            //    continue;
            //}            
            assetPaths.AddRange(GetAssetPathList(assetFolder));
        }

        window.packageName = "ExportPackageTool";
        window.exportList = assetPaths;
    }

    private void OnGUI()
    {
        TopArea();

        TreeViewArea();

        BottomArea();
    }

    private void OnDestroy()
    {        
    }

    private void TopArea()
    {
        float totalTopHeight = 53f;
        Rect r = GUILayoutUtility.GetRect(position.width, totalTopHeight);

        // Background
        GUI.Label(r, GUIContent.none, Styles.topBarBg);

        // Header
        Rect titleRect = new Rect(r.x + 5f, r.yMin, r.width, r.height);
        GUI.Label(titleRect, EditorGUIUtility.TrTextContent("Items to Export (" + packageName + ")"), Styles.title);
    }

    private void BottomArea()
    {
        // Background
        GUILayout.BeginVertical(Styles.bottomBarBg);
        GUILayout.Space(8);
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        if (GUILayout.Button(EditorGUIUtility.TrTextContent("Export...")))
        {
            Export();

            Close();
            GUIUtility.ExitGUI();
        }
        GUILayout.Space(10);
        GUILayout.EndHorizontal();
        GUILayout.Space(5);
        GUILayout.EndVertical();
    }

    private void TreeViewArea()
    {
        Rect treeAreaRect = GUILayoutUtility.GetRect(1, 9999, 1, 99999);

        if (exportList.Count == 0)
        {
            GUI.Label(treeAreaRect, "Nothing to export!", Styles.loadingTextStyle);
            return;
        }

        if (mTreeViewState == null)
            mTreeViewState = new TreeViewState();

        if (mTreeView == null)
            mTreeView = new ExportTreeView(this, mTreeViewState);

        mTreeView.OnGUI(treeAreaRect);
    }

    private void Export()
    {
        string ext = "unitypackage";
        string savePath = EditorUtility.SaveFilePanel("Export package ...", "", packageName, ext);

        if (savePath == "")
            return;

        PackageExportSettings.ExportPackage(exportList.ToArray(), savePath);
    }

    static private List<string> GetAssetPathList(string assetFolder)
    {
        var assetPaths = new List<string>
        {
            assetFolder,
            string.Format("{0}.meta", assetFolder)
        };

        var fullProjectPath = Path.GetFullPath(PROJECT_PATH);

        var allFilesFullPaths = GetAllFiles(assetFolder);
        foreach (var fileFullPath in allFilesFullPaths)
        {
            //Debug.Log("fileFullPath : " + fileFullPath);
            // If any of the paths we're looking at match the ignore paths from the user, skip them
            if (PackageExportSettings.GetOrCreateSettings().ExcludeList.Any(x =>
                fileFullPath.Contains(Path.GetFullPath(Path.Combine(fullProjectPath, x)))))
            {
                continue;
            }

            var relativeAssetPath = fileFullPath.Replace(fullProjectPath, string.Empty);
            //Debug.Log("relativeAssetPath : " + relativeAssetPath);
            assetPaths.Add(relativeAssetPath);
        }

        return assetPaths;
    }

    static private IEnumerable<string> GetAllFiles(string folderPath)
    {
        var filePaths = new List<string>();
        var fullPath = Path.GetFullPath(folderPath);
        filePaths.AddRange(Directory.GetFiles(
            fullPath,
            "*",
            SearchOption.AllDirectories));

        filePaths.AddRange(Directory.GetDirectories(
            fullPath,
            "*",
            SearchOption.AllDirectories));

        return filePaths.Distinct().OrderBy(x => x);
    }   
}
