#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using BB;
public static class AssetDatabaseUtils
{
    public static string GetFolder(UnityEngine.Object asset)
    {
        var path = AssetDatabase.GetAssetPath(asset);
        var result = path.NoE() ? "" : FolderUtils.GetFolderFromPath(path);
        return result;
    }
    public static List<T> FindAllAssetsInSameFolder<T>(UnityEngine.Object asset) where T : UnityEngine.Object
    {
        var folder = GetFolder(asset);
        var result = folder.NoE() ? new() : FindAllAssetsOfType<T>(folder);
        return result;
    }
    public static List<T> FindAllAssetsOfChildType<T>() where T : UnityEngine.Object
    {
        var result = new List<T>();
        foreach (var type in typeof(T).GetAllChildTypes())
            foreach (var obj in FindAllAssetsOfType(type))
                if (obj is T t)
                    result.Add(t);
        return result;
    }
    public static List<UnityEngine.Object> FindAllAssetsOfType(Type type, params string[] folders)
    {
        var filter = "t: " + type.Name;
        var guids = folders.NoE() ? AssetDatabase.FindAssets(filter) : AssetDatabase.FindAssets(filter, folders);
        var result = new List<UnityEngine.Object>();
        foreach (var guid in guids)
            result.Add(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(AssetDatabase.GUIDToAssetPath(guid)));
        return result;
    }
    public static List<T> FindAllAssetsOfType<T>(params string[] folders) where T : UnityEngine.Object
    {
        var result = new List<T>();
        foreach (var asset in FindAllAssetsOfType(typeof(T), folders))
            if (asset is T t)
                result.Add(t);
        return result;
    }
    //public static GameObject SaveGameObjectToPath(UnityEngine.Object assetObject, GameObject gameObject, string name)
    //{
    //    var path = AssetDatabase.GetAssetPath(assetObject);
    //    var folder = Path.GetDirectoryName(path);
    //    var instance = gameObject.CreateCopy(null);
    //    var asset = PrefabUtility.SaveAsPrefabAsset(instance, AssetPathUtils.CreatePrefabPath(name, folder));
    //    UnityEngine.Object.DestroyImmediate(instance);
    //    return asset;
    //}
    //public static StageType EnterPreviewStage<StageType>(Action<StageType> beforeEnter, Action<StageType> afterEnter, Action onExit) 
    //    where StageType : BasePreviewSceneStage
    //{
    //    var stage = ScriptableObject.CreateInstance<StageType>();
    //    stage._onExit = onExit;
    //    beforeEnter?.Invoke(stage);
    //    StageUtility.GoToStage(stage, true);
    //    afterEnter?.Invoke(stage);
    //    SceneView.FrameLastActiveSceneView();
    //    return stage;
    //}
}
#endif