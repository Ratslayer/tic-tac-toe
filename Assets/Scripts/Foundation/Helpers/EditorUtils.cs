#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using static BB.AssetPathUtils;
using static BB.AssetExtensions;
using static BB.FolderUtils;
namespace BB
{
	public static class AssetExtensions
	{
		public const string ControllerAsset = "controller";
		public const string Prefab = "prefab";
		public const string Fbx = "fbx";
		public const string Asset = "asset";
		public const string Blend = "blend";
		public const string Jpg = "jpg";
		public const string Jpeg = "jpeg";
		public const string Png = "png";
		public const string Material = "mat";
		public const string AnimationClip = "anim";
		public const string JSON = "json";
		public const string Scene = "unity";
		public const string Playable = "playable";
	}
	public static class AssetPathUtils
	{
		//public static string CreateScenePath(string name, params string[] folders)
		//{
		//    return UnityFolder.GetAbstractUnityAssetPath(name, Prefab, folders);
		//}
		public static string GetAbstractUnityAssetPath(string name, string extension, string path)
		   => $"{path}/{name}.{extension}";
		public static string CreateAssetPath(string name, string path)
		{
			return GetAbstractUnityAssetPath(name, Asset, path);
		}
		public static string CreatePrefabPath(string name, string folder)
		{
			return GetAbstractUnityAssetPath(name, Prefab, folder);
		}
		//public static string CreatePlayablePath(string name, params string[] folders)
		//{
		//    return UnityFolder.GetAbstractUnityAssetPath(name, Playable, folders);
		//}
		//public static string CreateTimelinePath(string name, params string[] folders)
		//{
		//    return CreatePlayablePath(name, folders);
		//}
		//public static string CreateMaterialPath(string name, string folder) => UnityFolder.GetAbstractUnityAssetPath(name, AssetExtensions.Material, folder);
		//public static string CreateControllerPath(string name, string folder) => UnityFolder.GetAbstractUnityAssetPath(name, AssetExtensions.ControllerAsset, folder);
		//public static string CreateAnimationClipPath(string name, string folder)
		//{
		//    return UnityFolder.GetAbstractUnityAssetPath(name, AssetExtensions.AnimationClip, folder);
		//}
		//private static T LoadAssetUsingPathMethod<T>(Func<string, string[], string> method, string name, params string[] folders) where T : UnityEngine.Object
		//{
		//    var path = method(name, folders);
		//    var result = AssetDatabase.LoadAssetAtPath<T>(path);
		//    return result;
		//}
		//public static Material LoadMaterial(string name, params string[] folders) => LoadAssetUsingPathMethod<Material>(CreateMaterialPath, name, folders);
		//public static AnimatorController LoadController(string name, params string[] folders) => LoadAssetUsingPathMethod<AnimatorController>(CreateControllerPath, name, folders);
		//public static T LoadPrefab<T>(string name, params string[] folders) where T : UnityEngine.Object
		//{
		//    return LoadAssetUsingPathMethod<T>(CreatePrefabPath, name, folders);
		//}
		public static string GetContainingFolder(UnityEngine.Object asset)
		{
			var path = AssetDatabase.GetAssetPath(asset);
			var result = "";
			if (!path.NoE())
			{
				SplitFilePath(path, out result, out _);
			}
			return result;
		}
	}
	public static class EditorUtils
	{
		private const string ExtensionPattern = @"\.(\w+)$";
		private static int _lastUndoGroup;
		public static void BeginUndoGroup(string groupName = "")
		{
			Undo.SetCurrentGroupName(groupName);
			_lastUndoGroup = Undo.GetCurrentGroup();
		}
		public static void EndUndoGroup()
		{
			Undo.CollapseUndoOperations(_lastUndoGroup);
		}
		public static void EndAndFlushUndoGroup()
		{
			EndUndoGroup();
			Undo.FlushUndoRecordObjects();
		}
		public static void GroupUndos(Action action, string groupName = "")
		{
			Undo.SetCurrentGroupName(groupName);
			var group = Undo.GetCurrentGroup();
			action.Invoke();
			Undo.CollapseUndoOperations(group);
		}
		public static void SelectTransform(Transform t)
		{
			Selection.activeTransform = t;
			EditorGUIUtility.PingObject(t);
		}
		public static bool Paused
		{
			get => EditorApplication.isPaused;
			set => EditorApplication.isPaused = value;
		}
		public static void ApplyPropertyModification(this PropertyModification property)
		{
			ReflectionUtils.InvokeStaticMethod<PropertyModification>("ApplyPropertyModificationToObject", property.target, property);
		}
		public static PropertyModification ToPropertyModification(this EditorCurveBinding binding, GameObject root)
		{
			var result = ReflectionUtils.InvokeStaticMethod<AnimationUtility>("EditorCurveBindingToPropertyModification", binding, root);
			return result as PropertyModification;
		}
		public static List<AssetType> LoadAllAssets<AssetType>() where AssetType : UnityEngine.Object
		{
			var ids = AssetDatabase.FindAssets("t:" + typeof(AssetType).Name);
			var result = new List<AssetType>();
			foreach (var id in ids)
				result.Add(AssetDatabase.LoadAssetAtPath<AssetType>(AssetDatabase.GUIDToAssetPath(id)));
			return result;
		}
		public static GameObject LoadAssetFromGUID(string guid)
		{
			return AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
		}
		/// <summary>
		/// Load asset associated with instance.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="asset"></param>
		/// <returns></returns>
		public static T ReloadAsset<T>(T asset) where T : UnityEngine.Object
		{
			var path = AssetDatabase.GetAssetPath(asset);
			var result = AssetDatabase.LoadAssetAtPath<T>(path);
			return result;
		}
		public static void SaveGUIChanges(UnityEngine.Object target)
		{
			if (GUIChanged)
				EditorUtility.SetDirty(target);
		}
		public static void SaveGUIChanges(SerializedObject obj)
		{
			if (GUIChanged)
				obj.ApplyModifiedProperties();
		}
		public static bool GUIChanged => GUI.changed;
		public static void SetLabels(UnityEngine.Object obj, params string[] labels)
		{
			AssetDatabase.SetLabels(obj, labels);
		}
		public static T LoadAssetFromGUID<T>(string guid) where T : UnityEngine.Component
		{
			var obj = LoadAssetFromGUID(guid);
			return obj ? obj.GetComponent<T>() : default;
		}
		public static void LoadAssetFromGUID<T>(string guid, ref T comp) where T : UnityEngine.Component
		{
			comp = LoadAssetFromGUID<T>(guid);
		}
		public static void SavePrefab(Component c)
		{
			SavePrefab(c.gameObject);
		}
		public static void SavePrefab(GameObject obj)
		{
			PrefabUtility.SavePrefabAsset(obj);
		}
		//public static T CopyAsset<T>(T asset, string name, params string[] folders) where T : UnityEngine.Object
		//{
		//    var fromPath = AssetDatabase.GetAssetPath(asset);
		//    var toPath = UnityFolder.GetAbstractUnityAssetPath(name, GetExtension(fromPath), folders);
		//    T result = null;
		//    if (AssetDatabase.CopyAsset(fromPath, toPath))
		//    {
		//        result = AssetDatabase.LoadAssetAtPath<T>(toPath);
		//    }
		//    return result;
		//}
		public static GameObject SavePrefab(GameObject obj, string name, params string[] folders)
		{
			var path = GetAbstractUnityAssetPath(name, Prefab, GetUnityAssetsPath(folders));
			return PrefabUtility.SaveAsPrefabAsset(obj, path);
		}
		public static GameObject SavePrefabAndCreateFolders(GameObject obj, string name, params string[] folders)
		{
			CreateFolders(folders);
			return SavePrefab(obj, name, folders);
		}
		public static void SaveAsset(UnityEngine.Object asset, string name, string extension, params string[] folders)
		{
			var path = GetAbstractUnityAssetPath(name, extension, GetUnityAssetsPath(folders));
			AssetDatabase.CreateAsset(asset, path);
		}
		//public static void CopyAsset(UnityEngine.Object asset, string name, params string[] folders)
		//{
		//    var originPath = AssetDatabase.GetAssetPath(asset);
		//    var extension = originPath.GetPattern(ExtensionPattern);
		//    var path = UnityFolder.GetAbstractUnityAssetPath(name, extension, folders);
		//    AssetDatabase.CopyAsset(originPath, path);
		//}
		//public static void SaveAsset(UnityEngine.Object asset)
		//{
		//    asset.Dirty();
		//    AssetDatabase.SaveAssets();
		//}
		//public static AnimatorController CreateAnimatorController(string name, params string[] folders)
		//{
		//    var path = UnityFolder.GetAbstractUnityAssetPath(name, ControllerAsset, folders);
		//    return AnimatorController.CreateAnimatorControllerAtPath(path);
		//}
		public static void SaveAnimator(AnimatorController animator, string name, params string[] folders)
		{
			SaveAsset(animator, name, ControllerAsset, folders);
		}
		public static T CopyComponent<T>(this GameObject obj, T comp) where T : Component
		{
			Type type = comp.GetType();
			Component copy = obj.AddComponent(type);
			FieldInfo[] fields = type.GetFields();
			foreach (FieldInfo field in fields)
			{
				field.SetValue(copy, field.GetValue(comp));
			}
			return copy as T;
		}
		public static string[] FindAssetGUIDs<T>(string filter, params string[] folders)
		{
			return FindAssetGUIDs($"{filter} t:{typeof(T).Name}", folders);
		}
		public static string[] FindAssetGUIDs(string filter, params string[] folders)
		{
			return AssetDatabase.FindAssets(filter, folders);
		}
		public static void RepaintGameView()
		{
			System.Reflection.Assembly assembly = typeof(UnityEditor.EditorWindow).Assembly;
			Type type = assembly.GetType("UnityEditor.GameView");
			EditorWindow gameView = EditorWindow.GetWindow(type);
			gameView.Repaint();
		}
		public static bool IsFolderSelected(out string path)
		{
			bool result;
			var obj = Selection.activeObject;
			if (obj)
			{
				path = AssetDatabase.GetAssetPath(obj.GetInstanceID());
				result = Directory.Exists(path);
			}
			else
			{
				path = "";
				result = false;
			}
			return result;
		}
		public static T CreateObject<T>(string name, Transform parent = null, Vector3 pos = new Vector3(), Quaternion rotation = new Quaternion()) where T : Component
		{
			var obj = new GameObject(name);
			obj.SetParent(parent);
			obj.transform.position = pos;
			obj.transform.rotation = rotation;
			return Undo.AddComponent<T>(obj);
		}
		public static void RecordObjectUndo(UnityEngine.Object obj, string msg = "")
		{
			Undo.RegisterCompleteObjectUndo(obj, msg);
			EditorUtility.SetDirty(obj);
		}
		public static string[] GetAllChildFolders(string path)
		{
			var result = Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly);
			return result;
		}
		public static string[] GetAllChildFolderNames(string path)
		{
			var result = GetAllChildFolders(path);
			for (int i = 0; i < result.Length; i++)
				result[i] = result[i].Split('/', '\\').Last();
			return result;
		}
		public static List<T> LoadAllChildAssets<T>(string path, bool topOnly, params string[] extensions) where T : UnityEngine.Object
		{
			var assetPaths = GetAllChildAssetPaths(path, topOnly, extensions);
			var result = new List<T>();
			foreach (var assetPath in assetPaths)
			{
				//var assetPath = UnityFolder.GetPath(path, path);
				var asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
				if (asset)
					result.Add(asset);
			}
			return result;
		}
		public static string[] GetAllChildAssetNames(string path, bool topOnly, params string[] extensions)
		{
			var files = GetAllChildFiles(path, topOnly, extensions);
			var result = from file in files select file.Name;
			return result.ToArray();
		}
		private static IEnumerable<FileInfo> GetAllChildFiles(string folderPath, bool topDirectoryOnly, params string[] extensions)
		{
			var option = topDirectoryOnly ? SearchOption.TopDirectoryOnly : SearchOption.AllDirectories;
			var info = new DirectoryInfo(folderPath);
			var files = info.GetFiles("*", option);
			IEnumerable<FileInfo> result;
			if (extensions.Length > 0)
			{
				result = from file in files where file.Name.EndsWithAny(extensions) select file;
			}
			else
			{
				result = from file in files where !file.Name.EndsWith(".meta") select file;
			}
			return result;
		}
		public static string[] GetAllChildAssetPaths(string path, bool topOnly, params string[] extensions)
		{
			var files = GetAllChildFiles(path, topOnly, extensions);
			var result = from file in files select GetAssetPath(file);
			string GetAssetPath(FileInfo file)
			{
				var assetPath = file.FullName.SimpleGroupSplit(@"\\(Assets\\.*)");
				return assetPath[0];
				//return UnityFolder.GetPath(path, file.Name);
			}
			return result.ToArray();
		}
		public static List<T> LoadAllChildAssets<T>(UnityEngine.Object folder, bool topOnly, string extension) where T : UnityEngine.Object
		{
			var folderPath = AssetDatabase.GetAssetPath(folder);
			var result = LoadAllChildAssets<T>(folderPath, topOnly, extension);
			return result;
		}
		public static List<AnimationClip> LoadAllAnimationClips(UnityEngine.Object folder, bool topOnly)
		{
			return LoadAllChildAssets<AnimationClip>(folder, topOnly, AssetExtensions.AnimationClip);
		}
		public static string GetPath(this GameObject obj)
		{
			return PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(obj);
		}
	}
}
#endif
namespace BB
{
	public static class EasyEditorUtils
	{
		public static void Dirty(this object obj)
		{
#if UNITY_EDITOR
			if (obj is UnityEngine.Object o)
				EditorUtility.SetDirty(o);
#endif
		}
	}
}