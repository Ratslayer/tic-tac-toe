using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
namespace BB
{
    public static class FolderUtils
    {
        public static string GetPath(params string[] folders) => StringUtils.GetPath(folders);
        public static string GetUnityAssetsPath(params string[] folders) => "Assets/" + StringUtils.GetPath(folders);
        public static string GetExtension(string name)
        {
            var result = name.Split('.').Last();
            return result;
        }
        public static string RemoveLastPathElement(string path)
        {
            var elements = path.Split('/');
            var result = elements.Length > 1 ? elements[0] : "";
            for (int i = 1; i < elements.Length - 1; i++)
            {
                result += "/" + elements[i];
            }
            return result;
        }
        public static string GetFolderFromPath(string path)
        {
            SplitFilePath(path, out var result, out _, out _);
            return result;
        }
        public static void SplitFilePath(string path, out string folder, out string fullName, out string name, out string extension)
        {
            var parts = path.Split('/', '\\');
            fullName = parts.LastOrDefault();
            var nameParts = fullName.Split('.');
            folder = RemoveLastPathElement(path);
            name = nameParts.FirstOrDefault();
            extension = nameParts.LastOrDefault();
        }
        public static void SplitFilePath(string path, out string folder, out string name, out string extension)
        {
            SplitFilePath(path, out folder, out _, out name, out extension);
        }
        public static void SplitFilePath(string path, out string folder, out string fileName)
        {
            SplitFilePath(path, out folder, out fileName, out _, out _);
        }
        public static string[] GetNamesFromPaths(params string[] paths)
        {
            var result = paths.Convert((i, str) => GetNameFromPath(str));
            return result;
        }
        public static string GetNameFromPath(string path)
        {
            SplitFilePath(path, out _, out var result, out _);
            return result;
        }
        public static string[] SplitPathIntoFolders(string path)
        {
            var result = path.Split('/');
            if (result.Last().Contains("."))
            {
                Array.Resize(ref result, result.Length - 1);
            }
            return result;
        }
#if UNITY_EDITOR
        public static void CreateFoldersForPath(string path)
        {
            var folders = SplitPathIntoFolders(path);
            CreateFolders(folders);
        }
        private static string CreateFolder(string parent, string name)
        {
            var result = $"{parent}/{name}";
            if (!UnityEditor.AssetDatabase.IsValidFolder(result))
            {
                var p = UnityEditor.AssetDatabase.CreateFolder(parent, name);
                Debug.Log($"Created: {result}");// | Created: {p}");
            }
            return result;
        }
        public static void CreateFolders(params string[] folders)
        {
            var result = "Assets";
            foreach (var folder in folders)
            {
                CreateFolder(result, folder);
                result = $"{result}/{folder}";
            }
            UnityEditor.AssetDatabase.Refresh();
        }
        public static bool HasCurrentSelectionPath(out string path)
        {
            var selection = UnityEditor.Selection.activeObject;
            path = "";
            if (selection)
            {
                var selectionPath = UnityEditor.AssetDatabase.GetAssetPath(selection);
                //if selection is not an asset then gtfo
                if (selectionPath.Length > 0)
                {
                    //if selection is a folder just use its path
                    if (Directory.Exists(selectionPath))
                        path = selectionPath + "/";
                    //selection is an asset so use its folder
                    else
                        path = Regex.Match(selectionPath, ".+/").Value;
                }
            }
            return selection;
        }
#endif
    }
}