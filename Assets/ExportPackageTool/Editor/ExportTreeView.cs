using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEditorInternal;
using UnityEditor;
using UnityEditor.Experimental;

public class ExportTreeView : TreeView
{
    private PackageExport mPackageExport;

    internal static class Constants
    {
        public static Texture2D folderIcon = EditorGUIUtility.FindTexture(EditorResources.folderIconName);
    }

    /* Setter & Getter */

    /* Functions */

    public ExportTreeView(PackageExport exportWindow, TreeViewState state)
        : base(state)
    {
        this.mPackageExport = exportWindow;
        Reload();

        ExpandAll();
    }

    protected override TreeViewItem BuildRoot()
    {
        var root = new TreeViewItem { id = 0, depth = -1, displayName = "Root" };
        var dirs = new Dictionary<string, TreeViewItem>();
        int id = 1;

        // Sort by name, so it looks nicer
        var sorted = mPackageExport.exportList.OrderBy(f => f);

        foreach (var file in sorted)
        {
            //Debug.Log("file : " + file);
            // Get path to walk through
            string[] folders = file.Split('/');

            TreeViewItem parent = root;

            string current = "";

            foreach (var folder in folders)
            {
                //Debug.Log("folder : " + folder);
                current = Path.Combine(current, folder);

                if (folder == "Assets" || folder.Contains(".meta") || folder.Contains(".DS_Store"))
                    continue;
                
                if (dirs.ContainsKey(current))
                {
                    parent = dirs[current];
                    continue;
                }

                var newItem = new TreeViewItem { id = id, displayName = folder };

                var icon = Constants.folderIcon;
                if (File.Exists(current))
                {
                    icon = (Texture2D)AssetDatabase.GetCachedIcon(current);

                    if (icon == null)
                        icon = InternalEditorUtility.GetIconForFile(current);
                }

                newItem.icon = icon;

                dirs.Add(current, newItem);
                parent.AddChild(newItem);

                parent = newItem;
                ++id;
            }
        }

        SetupDepthsFromParentsAndChildren(root);

        return root;
    }
}
