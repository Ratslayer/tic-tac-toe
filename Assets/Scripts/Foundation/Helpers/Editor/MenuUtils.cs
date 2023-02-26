using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using BB;
public static class MenuUtils
{
    public const string AssetsGS = "Assets/GS/";
    public const string WindowsGS = "Window/GS/";
    public const string GameObjectGS = "GameObject/GS/";
    #region Validation
    public static bool IsSelectedObjectModel(bool acceptFolders = false)
    {
        return SelectedObjectEndsWith(acceptFolders, "blend", ".obj", ".fbx");
    }
    public static bool IsFolder(UnityEngine.Object obj)
    {
        return obj.GetType() == typeof(DefaultAsset);
    }
    public static bool IsSelectedObjectPSD()
    {
        return SelectedObjectEndsWith(false, "psd");
    }
    public static bool IsSelectedObjectImage(bool useFolders)
    {
        return SelectedObjectEndsWith(useFolders, "png", "jpg", "jpeg");
    }
    public static bool IsSelectedObjectXML()
    {
        return SelectedObjectEndsWith(false, "xml");
    }
    public static bool IsSelectedObjectCS(bool useFolders = false)
    {
        return SelectedObjectEndsWith(useFolders, "cs");
    }
    public static bool SelectedObjectEndsWith(bool acceptFolders, params string[] endings)
    {
        bool result = Selection.objects.Length > 0;
        foreach (var select in Selection.objects)
        {
            if (!(EndsWith(select, endings) || acceptFolders && IsFolder(select)))
            {
                result = false;
                break;
            }
        }
        return result;
    }
    private static bool EndsWith(UnityEngine.Object obj, params string[] endings)
    {
        var result = false;
        var path = GetPath(obj).ToLower();
        foreach (var ending in endings)
        {
            if (path.EndsWith("." + ending))
            {
                result = true;
                break;
            }
        }
        return result;
    }
    #endregion
    private static void RunForAllSelected<T>(Action<T> action) where T : UnityEngine.Object
    {
        foreach (var select in Selection.objects)
        {
            if (select is T t)
                action(t);
        }
    }
    public static void RunForAllPathsInSelectedFolders<T>(Action<string> action) where T : UnityEngine.Object
    {
        RunForAllAssetsInSelectedFoldersInternal<T>(null, action);
    }
    public static void RunForAllAssetsInSelectedFolders<T>(Action<T> action) where T : UnityEngine.Object
    {
        RunForAllAssetsInSelectedFoldersInternal<T>(action, null);
    }
    private static void RunForAllAssetsInSelectedFoldersInternal<T>(Action<T> action, Action<string> pathAction) where T : UnityEngine.Object
    {
        var folderList = new List<string>();
        foreach (var select in Selection.objects)
        {
            if (IsFolder(select))
            {
                folderList.Add(GetPath(select));
            }
            else if (action != null && select is T t)
                action(t);
            else
                pathAction?.Invoke(GetPath(select));
        }
        if (folderList.Count > 0)
        {
            var query = $"t:{typeof(T).Name}";
            var guids = AssetDatabase.FindAssets(query, folderList.ToArray());
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (action != null)
                {
                    var asset = AssetDatabase.LoadAssetAtPath<T>(path);
                    if (asset)
                    {
                        action(asset);
                    }
                    else
                        Assert.Error.Log($"Asset at path {path} matched but not found or not of type {typeof(T).Name}");
                }
                else
                {
                    pathAction?.Invoke(path);
                }
            }
        }
    }
    #region Methods
    private static void Open<T>() where T : EditorWindow
    {
        var window = EditorWindow.GetWindow<T>();
        window.Show();
    }
    public static string GetPath(UnityEngine.Object obj)
    {
        return AssetDatabase.GetAssetPath(obj);
    }
    public static string GetSelectedPath()
    {
        return GetPath(Selection.activeObject);
    }
    #endregion
}
