using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UI;
namespace BB
{
	public static class GC
	{
		public static void SetActiveSafe(this Component comp, bool value)
		{
			if (comp)
				comp.gameObject.SetActive(value);
		}
		public static void SetActiveSafe(this GameObject obj, bool value)
		{
			if (obj)
				obj.SetActive(value);
		}
		private static T GetComponentSafe<T>(Func<T> getter, AssertHandler handler, string msg) where T : Component
		{
			var result = getter();
			handler?.NotNull(result, msg);
			return result;
		}
		private static T[] GetComponentsSafe<T>(Func<T[]> getter, AssertHandler handler = null, string msg = "") where T : Component
		{
			var result = getter();
			handler?.NotEmpty(result, msg);
			return result;
		}
		private static string GetComponentErrorMessage<T>(string article) => $"Does not have {article} {typeof(T).Name}";
		public static void Self<T>(this Component c, out T component, AssertHandler handler = null) where T : Component
		{
			component = GetComponentSafe(c.GetComponent<T>, handler, GetComponentErrorMessage<T>("a"));
		}
		public static bool InParent<T>(this Component c, out T component, string errorMsg) where T : Component
		{
			component = GetComponentSafe<T>(c.GetComponentInParent<T>, Assert.Error, errorMsg);
			return component;
		}
		public static void InParent<T>(this Component c, out T[] components, string errorMsg) where T : Component
		{
			components = GetComponentsSafe<T>(c.GetComponentsInParent<T>, Assert.Error, errorMsg);
		}
		public static void InParent<T>(this Component c, out T component, AssertHandler handler = null) where T : Component
		{
			component = GetComponentSafe<T>(c.GetComponentInParent<T>, handler, GetComponentErrorMessage<T>("a parent with"));
		}
		public static void InChildren<T>(this Component c, out T component, string errorMsg) where T : Component
		{
			component = GetComponentSafe<T>(c.GetComponentInChildren<T>, Assert.Error, errorMsg);
		}
		public static void InChildren<T>(this Component c, out T[] components, string errorMsg) where T : Component
		{
			components = GetComponentsSafe<T>(c.GetComponentsInChildren<T>, Assert.Error, errorMsg);
		}
		public static void InChildren<T>(this Component c, out T component, AssertHandler handler = null) where T : Component
		{
			component = GetComponentSafe<T>(c.GetComponentInChildren<T>, handler, GetComponentErrorMessage<T>("a child with"));
		}
	}
	public static class Vector3Utils
	{
		public static float DistanceBetweenPointAndRay(Vector3 point, Vector3 origin, Vector3 direction)
		{
			var dir = direction.normalized;
			var diff = point - origin;
			var result = Vector3.Cross(dir, diff).magnitude;
			return result;
		}
		public static bool IsZero(this Vector3 v) => v.sqrMagnitude <= Mathf.Epsilon;
		public static Vector2 Horizontal2D(this Vector3 direction) => new Vector3(direction.x, direction.z);
		public static Vector3 HorizontalNormalized(this Vector3 direction)
		{
			return direction.SetY(0).normalized;
		}
		public static Vector3 Horizontal(this Vector3 direction) => direction.SetY(0);
		public static Vector2 SetX(this Vector2 v, float value) => new(value, v.y);
		public static Vector2 SetY(this Vector2 v, float value) => new(v.x, value);
		public static Vector3 SetX(this Vector3 v, float value)
		{
			return new Vector3(value, v.y, v.z);
		}
		public static Vector3 SetZ(this Vector3 v, float value)
		{
			return new Vector3(v.x, v.y, value);
		}
		public static Vector3 SetY(this Vector3 v, float value)
		{
			return new Vector3(v.x, value, v.z);
		}
		public static Quaternion GetYRotation(this Vector3 from, Vector3 to)
		{
			var angle = Vector3.SignedAngle(from, to, Vector3.up);
			return Quaternion.Euler(0, angle, 0);
		}
		public static Quaternion GetFaceHorizontalRot(this Vector3 v)
			=> Vector3.forward.GetYRotation(v.x >= 0f ? Vector3.right : Vector3.left);
	}
	public static class UIUtils
	{
		public static void SetAlpha(this Graphic graphic, float value) => graphic.color = graphic.color.A(value);
		public static void SetAlpha(float value, params Graphic[] graphics)
		{
			foreach (var graphic in graphics)
				graphic.SetAlpha(value);
		}
		public static void SetColorAndAlpha(this Graphic graphic, Color color) => graphic.color = color;
		public static void SetColorAndAlpha(Color color, params Graphic[] graphics)
		{
			foreach (var graphic in graphics)
				graphic.SetColorAndAlpha(color);
		}
		public static void SetColorNoAlpha(this Graphic graphic, Color color) => graphic.color = color.A(graphic.color.a);
		public static void SetColorNoAlpha(Color color, params Graphic[] graphics)
		{
			foreach (var graphic in graphics)
				graphic.SetColorNoAlpha(color);
		}
		public static void SetMouseCursorVisibility(bool value)
		{
			Cursor.lockState = value ? CursorLockMode.None : CursorLockMode.Locked;
			Cursor.visible = value;
		}
	}
	public static class RectTransformUtils
	{
		public static void CopyAnchors(this RectTransform t, RectTransform other)
		{
			t.anchorMax = other.anchorMax;
			t.anchorMin = other.anchorMin;
		}
	}
	public static class TransformUtils
	{
#if UNITY_EDITOR
		public static void SetPositionWithoutMovingChildren_Undo(this Transform t, Vector3 pos, string actionName)
		{
			EditorUtils.GroupUndos(() =>
			{
				UnityEditor.Undo.RecordObject(t, "");
				var diff = pos - t.position;
				t.position += diff;
				t.ForeachDirectChild((c) =>
				{
					UnityEditor.Undo.RecordObject(c, "");
					c.position -= diff;
				});
			}, actionName);
		}
#endif
		public static Vector3 GetDirectionTo(this Transform t1, Transform t2) => t2.position - t1.position;
		public static Vector3 GetDirectionTo(this Transform t, Vector3 point) => point - t.position;
		public static string GetTargetPath(this Transform target)
		{
			if (target == null)
			{
				//Assert.Error.Log("Target is null");
				return "";
			}
			string path = "/" + target.name;
			if (target.parent)
				path = GetTargetPath(target.parent) + path;
			return path;
		}
		public static Transform FindTransform(string path)
		{
			if (path.NoE())
				return null;
			var obj = GameObject.Find(path);
			return Assert.Error.True(obj, $"Could not find object with path '{path}'")
				? obj.transform : null;
		}
		public static float GetDistance(this Transform t, Vector3 pos)
		{
			var result = (t.position - pos).magnitude;
			return result;
		}
		public static float GetDistance(this Transform t1, Transform t2)
		{
			return GetDistance(t1, t2.position);
		}
		public static bool HasParent(this Component component, out Transform parent)
		{
			parent = component ? component.transform.parent : null;
			return parent;
		}
		public static Vector3 GetFlatVectorTo(this Transform from, Transform to) => (to.position - from.position).SetY(0);
		public static Transform GetClosest(this Transform t, params Transform[] transforms) => transforms.MinElement(t2 => t.GetDistance(t2));
		public static Transform GetFarthest(this Transform t, params Transform[] transforms) => transforms.MaxElement(t2 => t.GetDistance(t2));
		public static Vector3 SafeGetPosition(this Component c) => c ? c.transform.position : Vector3.zero;
		public static GameObject SafeGameObject(this Component c) => c ? c.gameObject : default;
		public static GameObject CreateChild(this Component t, string name) => t.gameObject.CreateChild(name);
		//public static ComponentType CreateChild<ComponentType>(this Component t, string name) where ComponentType : Component => t.gameObject.CreateChild<ComponentType>(name);
		public static void FaceHorizontal(this Transform t, Vector3 position)
		{
			t.forward = (position - t.position).HorizontalNormalized();
		}
		public static TransformData ToLocalData(this Transform t)
		{
			var result = new TransformData();
			result.GetLocalData(t);
			return result;
		}
		public static TransformData ToWorldData(this Transform t)
		{
			var result = new TransformData();
			result.GetWorldData(t);
			return result;
		}
		public static Transform GetFirstChildWithTag(Transform parent, string tag)
		{
			Transform result = parent.tag == tag ? parent : null;
			if (result == null)
			{
				foreach (Transform child in parent)
				{
					result = GetFirstChildWithTag(child, tag);
					if (result != null)
					{
						break;
					}
				}
			}
			return result;
		}
		public static IEnumerable<Transform> GetDirectChildren(this Transform t)
		{
			for (int i = 0; i < t.childCount; i++)
				yield return t.GetChild(i);
		}
		private static void ProcessAllChildren(Transform transform, Predicate<Transform> condition, Action<Transform> processor)
		{
			int childCount = transform.childCount;
			for (int i = childCount - 1; i >= 0; i--)
			{
				var c = transform.GetChild(i);
				if (condition == null || condition(c))
					processor(c);
			}
		}
		public static void DestroyAllChildren(this Transform transform, Predicate<Transform> condition = null)
		{
			ProcessAllChildren(transform, condition, (c) => c.DestroyGameObject());
		}
		public static void DestroyAllChildrenImmediate(this Transform transform, Predicate<Transform> condition = null) => ProcessAllChildren(transform, condition, (c) => UnityEngine.Object.DestroyImmediate(c.gameObject));
		public static T FindClosest<T>(this Transform transform, params T[] objects) where T : Component
		{
			var result = objects.MinElement((obj) => transform.GetDistance(obj.transform));
			return result;
		}
		public static RectTransform GetRectTransform(this Component c) => c.GetComponent<RectTransform>();
		public static Transform SafeTransform(this Component c) => c ? c.transform : null;
		public static void ResetLocal(this Transform t)
		{
			t.localPosition = Vector3.zero;
			t.localRotation = Quaternion.identity;
			t.localScale = Vector3.one;
		}
		public static void ResetWorld(this Transform t)
		{
			t.position = Vector3.zero;
			t.rotation = Quaternion.identity;
		}
		private static TransformData GetWorldData(Transform t)
		{
			return new TransformData
			{
				Position = t.position,
				Rotation = t.rotation
			};
		}
		private static void SetWorldData(Transform t, TransformData data)
		{
			t.position = data.Position;
			t.rotation = data.Rotation;
		}
		public static void BakeLocalIntoChildren(this Transform t, params Transform[] children)
		{
			if (children == null || children.Length == 0)
				children = t.GetDirectChildren().ToArray();
			var datas = (from c in children select GetWorldData(c)).ToArray();
			t.ResetLocal();
			for (int i = 0; i < children.Length; i++)
			{
				SetWorldData(children[i], datas[i]);
			}
		}
		public static void MoveWithoutMovingAnyChildren(this Transform t, Vector3 position) => MoveWithoutMovingAnyChildren(t, position, t.rotation);
		public static void MoveWithoutMovingAnyChildren(this Transform t, Vector3 position, Quaternion rotation)
		{
			var children = t.GetDirectChildren();
			MoveWithoutMovingChildren(t, position, rotation, children.ToArray());
		}
		public static void MoveWithoutMovingChildren(this Transform t, Vector3 position, params Transform[] children) => MoveWithoutMovingChildren(t, position, t.rotation, children);
		public static void MoveWithoutMovingChildren(this Transform t, Vector3 position, Quaternion rotation, params Transform[] children)
		{
			var datas = (from c in children select (c, GetWorldData(c))).ToArray();
			t.position = position;
			t.rotation = rotation;
			foreach (var (c, data) in datas)
			{
				SetWorldData(c, data);
			}
		}
		public static void MoveToAlignChild(this Transform t, Transform child, Transform target)
		{
			MoveToAlignChild(t, child, target.position);
		}
		public static void MoveToAlignChild(this Transform t, Transform child, Vector3 position)
		{
			var diff = position - child.position;
			t.position += diff;
		}
		public static void MoveHorizontalToAlignChild(this Transform t, Transform child, Vector3 position)
		{
			t.position += (position - child.position).SetY(0);
		}
		public static void RotateYToAlignChild(this Transform t, Transform child, Vector3 position, Vector3 pivot)
		{
			var childDir = child.position - pivot;
			var dir = position - pivot;
			var angle = Vector3.SignedAngle(childDir, dir, Vector3.up);
			t.rotation *= Quaternion.AngleAxis(angle, Vector3.up);
		}
		public static void SnapToAxis(this Transform t, Vector3 position, Axis axis)
		{
			t.position = position.Replace(t.position, axis);
		}
		public static GameObject FindDirectChild(this GameObject obj, string name)
		{
			return obj.transform.FindDirectChild(name)?.gameObject;
		}
		public static Transform FindDirectChild(this Transform t, string name)
		{
			return t.Find(name);
		}
		public static T FindDirectChild<T>(this Transform t, string name) where T : Component
		{
			var child = t.FindDirectChild(name);
			var result = child ? child.GetComponent<T>() : null;
			return result;
		}
		public static Transform FindDeepChild(this Transform t, string name)
		{
			var result = t.FindDirectChild(name);
			if (!result)
			{
				for (int i = 0; i < t.childCount; i++)
				{
					result = t.GetChild(i).FindDeepChild(name);
					if (result)
						break;
				}
			}
			return result;
		}
		public static void ForeachDirectChild(this Transform parent, Action<Transform> childProcessor)
		{
			foreach (Transform c in parent)
				childProcessor(c);
		}
		public static void ForeachChild(this Transform parent, Action<Transform> childProcessor)
		{
			foreach (var c in GetAllChildren(parent))
				childProcessor(c);
		}
		//public static List<Transform> GetDirectChildren(this Transform parent)
		//{
		//	var result = new List<Transform>();
		//	for (int i = 0; i < parent.childCount; i++)
		//		result.Add(parent.GetChild(i));
		//	return result;
		//}
		public static List<Transform> GetAllChildren(this Transform parent)
		{
			var result = new List<Transform>();
			ForeachDirectChild(parent, (c) => AddChildrenRecursive(c));
			return result;
			void AddChildrenRecursive(Transform t)
			{
				result.Add(t);
				ForeachDirectChild(t, (c) => AddChildrenRecursive(c));
			}
		}
		public static bool IsChildOf(this GameObject child, GameObject parent)
		{
			return IsParent(child.transform);
			bool IsParent(Transform c)
			{
				return c.parent == parent.transform ? true : c.parent ? IsParent(c.parent) : false;
			}
		}
		public static bool IsInSameHierarchy(this GameObject obj, GameObject other)
		{
			return obj == other || obj.IsChildOf(other) || other.IsChildOf(obj);
		}
		public static bool HasChildren(this Transform t, params string[] names)
		{
			bool result = true;
			foreach (var name in names)
			{
				if (!t.FindDirectChild(name))
				{
					result = false;
					break;
				}
			}
			return result;
		}
		public static Transform GetChild(this Transform t, string name)
		{
			Transform result = null;
			for (int i = 0; i < t.childCount; i++)
			{
				var c = t.GetChild(i);
				if (c.name == name)
				{
					result = c;
					break;
				}
			}
			return result;
		}
		public static void SetPos(this Transform t, float x, float y)
		{
			t.position = new Vector3(x, y, t.position.z);
		}
		public static void SetPos(this Transform t, Vector2 pos)
		{
			SetPos(t, pos.x, pos.y);
		}
		public static void SetLocalPos(this Transform t, float x, float y)
		{
			t.localPosition = new Vector3(x, y, t.localPosition.z);
		}
		public static void SetLocalPos(this Transform t, Vector2 pos)
		{
			SetLocalPos(t, pos.x, pos.y);
		}
		public static void SetScale(this Transform t, float x, float y)
		{
			t.localScale = new Vector3(x, y, t.localScale.z);
		}
		public static void SetScale(this Transform t, Vector2 scale)
		{
			SetScale(t, scale.x, scale.y);
		}
		public static void SetPositionZ(this Transform transform, float z)
		{
			var pos = transform.position;
			pos.z = z;
			transform.position = pos;
		}
		public static Vector3 Center(params Transform[] transforms)
		{
			var result = Vector3.zero;
			if (transforms.Length > 0)
			{
				foreach (var t in transforms)
					result += t.position;
				result /= transforms.Length;
			}
			return result;
		}
		public static Quaternion GetYRotationDiff(this Transform t, Vector3 forward) => t.forward.GetYRotation(forward);
	}
	public static class GameObjectAndComponentUtils
	{
		public static void DestroyGameObject(this Component c)
		{
			if (c)
				Destroy(c.gameObject);
		}
		public static void Destroy(this GameObject go)
		{
			if (go)
				UnityEngine.Object.Destroy(go);
		}
		public static void ToggleActive(this bool value, Transform show, Transform hide)
		{
			value.Select(show, hide, out var shown, out var hidden);
			shown.gameObject.SetActive(true);
			hidden.gameObject.SetActive(false);
		}
		public static string GetScenePath(this GameObject obj)
		{
			if (!obj)
				return "NULL";
			var result = obj.name;
			var parent = obj.transform.parent;
			while (parent)
			{
				result = $"{parent.name}/{result}";
				parent = parent.parent;
			}
			return result;
		}
		public static string GetScenePath(this Component comp)
			=> comp ? GetScenePath(comp.gameObject) : "NULL";
		public static void Activate(this GameObject obj)
		{
			obj.SetActive(true);
		}
		public static void ToggleActive(this GameObject obj)
		{
			if (obj)
				obj.SetActive(!obj.activeSelf);
		}
		public static void ToggleGameObjectActive<T>(this T t) where T : Component
		{
			if (t)
				t.gameObject.ToggleActive();
		}
		public static bool TryGetComponentInParent<T>(this Component c, out T comp)
		{
			comp = c.GetComponentInParent<T>();
			return comp != null;
		}
		public static bool TryGetComponentInChildren<T>(this Component c, out T comp)
		{
			comp = c.GetComponentInChildren<T>();
			return comp != null;
		}
#if UNITY_EDITOR
		public static GameObject CreateAndRenameChild(this GameObject obj, string name = "Object") => CreateAndRenameObject(name, obj.transform);
		public static T CreateAndRenameChild<T>(this GameObject obj, string name = "Object") where T : Component
		{
			var result = obj.CreateAndRenameChild(name);
			return result.AddComponent<T>();
		}
		public static GameObject CreateAndRenameObject(string name, Transform parent)
		{
			//We select the HierarchyWindow in order to focus it.
			UnityEditor.EditorApplication.ExecuteMenuItem("Window/General/Hierarchy");
			//We select the new GameObject we create as the selected one.
			var obj = new GameObject(name);
			obj.transform.parent = parent;
			UnityEditor.Selection.activeGameObject = obj;

			//When hierarchy is updated (it's not inmediated) we call Rename func.
			UnityEditor.EditorApplication.hierarchyChanged += Rename;
			return obj;
			static void Rename()
			{
				//we remove itself in order to not call it twice.
				UnityEditor.EditorApplication.hierarchyChanged -= Rename;
				//we press f2 virtually
				UnityEditor.EditorWindow.focusedWindow.SendEvent(new UnityEngine.Event { keyCode = KeyCode.F2, type = EventType.KeyDown });
			}
		}
#endif
	}
	public static class UnityHelper
	{
		public static string GetNameSafe(this UnityEngine.Object obj) => obj ? obj.name : "NULL";
		public static bool HasComponentSafe<T>(this Component c, out T component)
		{
			component = c ? c.GetComponent<T>() : default;
			return component is UnityEngine.Object obj ? obj : component != null;
		}
		//public static T InstantiateIfNull<T>(ref T instance, T prefab) where T : UnityEngine.Object
		//{
		//    if (!instance)
		//        instance = UnityEngine.Object.Instantiate(prefab);
		//    return instance;
		//}
		public static Color RandomHue(float saturation = 1f, float brightness = 1f)
		{
			var hue = UnityEngine.Random.value;
			var result = Color.HSVToRGB(hue, saturation, brightness);
			return result;
		}
		public static T FirstObject<T>(this IEnumerable<T> col) where T : UnityEngine.Object => col.FirstOrDefault(obj => obj);
		public static T SafeRef<T>(this T obj) where T : UnityEngine.Object
		{
			T result;
			if (obj)
				result = obj;
			else
				result = null;
			return result;
		}
		public static string SafeName(this UnityEngine.Object c) => c ? c.name : "NONE";
		public static void PrefabSafeDestroy(GameObject root, params Transform[] children) => PrefabSafeDestroy(root, children.Convert((i, c) => c.gameObject));
		public static void PrefabSafeDestroy(GameObject root, params GameObject[] children)
		{
			if (children.Length > 0)
			{
				if (Application.isPlaying)
				{
					foreach (var obj in children)
						UnityEngine.Object.Destroy(obj);
				}
#if UNITY_EDITOR
				else
				{

					var rootPath = UnityEditor.PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(root);
					var loadedRoot = UnityEditor.PrefabUtility.LoadPrefabContents(rootPath);
					foreach (var child in children)
					{
						if (UnityEditor.PrefabUtility.IsPartOfPrefabAsset(child))
						{
							var prefabObj = loadedRoot.transform.Find(child.name);
							UnityEngine.Object.DestroyImmediate(prefabObj);
						}
						else
							child.Destroy();
					}
					UnityEditor.PrefabUtility.SaveAsPrefabAsset(loadedRoot, rootPath);
				}
#endif
			}
		}
		public static void PrefabSafeDestroy(params GameObject[] objects)
		{
			if (Application.isPlaying)
			{
				foreach (var obj in objects)
					UnityEngine.Object.Destroy(obj);
			}
#if UNITY_EDITOR
			else
			{
				GameObject currentRoot = null;
				GameObject loadedRoot = null;
				string rootPath = "";
				foreach (var obj in objects)
				{
					if (UnityEditor.PrefabUtility.IsPartOfAnyPrefab(obj))
					{
						var newRoot = UnityEditor.PrefabUtility.GetNearestPrefabInstanceRoot(obj);
						if (newRoot != currentRoot)
						{
							SavePrefab();
							currentRoot = newRoot;
							rootPath = UnityEditor.AssetDatabase.GetAssetPath(currentRoot);
							loadedRoot = UnityEditor.PrefabUtility.LoadPrefabContents(rootPath);
						}
						var prefabObj = loadedRoot.transform.Find(obj.name);
						UnityEngine.Object.DestroyImmediate(prefabObj);
					}
					else
						UnityEngine.Object.DestroyImmediate(obj);
				}
				SavePrefab();
				void SavePrefab()
				{
					if (loadedRoot)
						UnityEditor.PrefabUtility.SaveAsPrefabAsset(loadedRoot, rootPath);
				}
			}
#endif
		}
		public static RaycastHit[] RaycastAll(Vector2 point, Vector2 dir, float z, float rayWidth, float maxDistance, int mask)
		{
			var center = new Vector3(point.x, point.y, z);
			var offset = new Vector3(0, 0, 0.5f - rayWidth);
			var result = Physics.CapsuleCastAll(center - offset, center + offset, rayWidth, dir, maxDistance, mask);
			return result;
		}
		#region Timers


		//public static UnityTimer DelayedInvoke(this MonoBehaviour behaviour, float time, Action action)
		//{
		//    return new UnityTimer(behaviour, GetCoroutine(time));
		//    IEnumerator GetCoroutine(float waitTime)
		//    {
		//        yield return new WaitForSeconds(waitTime);
		//        action();
		//    }
		//}
		public static void GetGlobalTR(this Transform t, out Vector3 position, out Quaternion rotation)
		{
			position = t.position;
			rotation = t.rotation;
		}
		public static void SetGlobalTR(this Transform t, Vector3 position, Quaternion rotation)
		{
			t.position = position;
			t.rotation = rotation;
		}
		public static void CopyGlobalTR(this Transform t, Transform t2)
		{
			t.SetGlobalTR(t2.position, t2.rotation);
		}
		public static void GetGlobalTR_LocalS(this Transform t, out Vector3 position, out Quaternion rotation, out Vector3 scale)
		{
			GetGlobalTR(t, out position, out rotation);
			scale = t.localScale;
		}
		public static void SetGlobalTR_LocalS(this Transform t, Vector3 position, Quaternion rotation, Vector3 scale)
		{
			SetGlobalTR(t, position, rotation);
			t.localScale = scale;
		}
		public static void GetLocalTRS(this Transform t, out Vector3 position, out Quaternion rotation, out Vector3 scale)
		{
			position = t.localPosition;
			rotation = t.localRotation;
			scale = t.localScale;
		}
		public static void SetLocalTRS(this Transform t, Vector3 position, Quaternion rotation, Vector3 scale)
		{
			t.localPosition = position;
			t.localRotation = rotation;
			t.localScale = scale;
		}
		public static void SetLocalTR(this Transform t, Vector3 position, Quaternion rotation)
		{
			t.localPosition = position;
			t.localRotation = rotation;
		}
		#endregion
		#region GameObject
		public static void SetParent(this GameObject obj, GameObject parent)
		{
			obj.transform.SetParent(parent ? parent.transform : null);
		}
		public static void SetParent(this GameObject obj, Component parent)
		{
			obj.SetParent(parent?.gameObject);
		}
		public static void SetParent(this Component component, Component parent)
		{
			component.gameObject.SetParent(parent);
		}
		public static void SetParent(this Component component, GameObject parent)
		{
			component.gameObject.SetParent(parent);
		}

		public static void SetParentNoShift(this GameObject obj, GameObject parent)
		{
			if (parent)
			{
				obj.transform.parent = parent.transform;
				obj.transform.localPosition = Vector3.zero;
				obj.transform.localScale = Vector3.one;
				obj.transform.localRotation = Quaternion.identity;
			}

		}
		public static void CopyLocalTransform(this Transform to, Transform from)
		{
			to.localPosition = from.localPosition;
			to.localRotation = from.localRotation;
			to.localScale = from.localScale;
		}
		public static void CopyTransform(this Transform to, Transform from)
		{
			to.SetPositionAndRotation(from.position, from.rotation);
			to.localScale = from.localScale;
		}
		public static void SetParentNoShift(this GameObject obj, Component parent)
		{
			obj.SetParentNoShift(parent?.gameObject);
		}
		public static void SetParentNoShift(this Component component, Component parent)
		{
			component.gameObject.SetParentNoShift(parent);
		}
		public static void SetParentNoShift(this Component component, GameObject parent)
		{
			component.gameObject.SetParentNoShift(parent);
		}


		public static GameObject GetParent(this GameObject obj)
		{
			return obj.transform.parent?.gameObject;
		}
		public static GameObject GetParent(this Component obj)
		{
			return obj.transform.parent?.gameObject;
		}
		public static Transform GetParentOrSelf(this Component obj)
		{
			var parent = obj.transform.parent;
			return parent ? parent : obj.transform;
		}
		public static GameObject GetRoot(this GameObject obj)
		{
			return obj.GetParent() ? GetRoot(obj.GetParent()) : obj;
		}
		public static GameObject GetRoot(this Component obj)
		{
			return GetRoot(obj.gameObject);
		}
		public static Component GetComponentAnywhere(this GameObject obj, Type type)
		{
			var result = obj.GetComponentInChildren(type);
			if (!result)
				result = obj.GetComponentInParent(type);
			return result;
			//return obj.GetRoot().GetComponentInChildren(type);
		}
		public static Component[] GetComponentsAnywhere(this GameObject obj, Type type)
		{
			return obj.GetRoot().GetComponentsInChildren(type);
		}
		public static Component GetComponentAnywhere(this Component comp, Type type)
		{
			return GetComponentAnywhere(comp.gameObject, type);
		}
		public static Component[] GetComponentsAnywhere(this Component comp, Type type)
		{
			return GetComponentsAnywhere(comp.gameObject, type);
		}
		public static T GetComponentAnywhere<T>(this GameObject obj) where T : Component
		{
			return GetComponentAnywhere(obj, typeof(T)) as T;//obj.GetRoot().GetComponentInChildren<T>();
		}
		public static T GetComponentAnywhere<T>(this Component obj) where T : Component
		{
			return GetComponentAnywhere<T>(obj.gameObject);
		}
		public static T[] GetComponentsAnywhere<T>(this GameObject obj)
		{
			return obj.GetRoot().GetComponentsInChildren<T>();
		}
		public static T[] GetComponentsAnywhere<T>(this Component obj)
		{
			return GetComponentsAnywhere<T>(obj.gameObject);
		}
		public static void GetComponentAnywhere<T>(this Component obj, ref T result) where T : Component
		{
			result = GetComponentAnywhere<T>(obj);
		}
		#endregion
		public static bool TryParseV2(string s, out Vector2 result)
		{
			var parsed = CSHelper.TryParseArray<float>(s, ',', float.TryParse, out var array, 2);
			result = parsed ? new Vector2(array[0], array[1]) : default;
			return parsed;
		}
		public static Vector2 CreateV2FromArray(object[] array)
		{
			Assert.Throw.LengthIs(array, 2, $"Cannot create vector2 from an array of length {array.Length}");
			var x = Convert.ToSingle(array[0]);
			var y = Convert.ToSingle(array[1]);
			return new Vector2(x, y);
		}
		public static bool TryParseV3(string s, out Vector3 result)
		{
			var parsed = CSHelper.TryParseArray<float>(s, ',', float.TryParse, out var array, 3);
			result = parsed ? new Vector3(array[0], array[1], array[2]) : default;
			return parsed;
		}
		public static Vector2 Clamp(this Vector2 v, Vector2 min, Vector2 max)
		{
			var result = new Vector2()
			{
				x = Mathf.Clamp(v.x, min.x, max.x),
				y = Mathf.Clamp(v.y, min.y, max.y)
			};
			return result;
		}
		public static Vector2 Clamp1Neg1(this Vector2 v) => v.Clamp(new Vector2(-1, -1), new Vector2(1, 1));
		public static Vector2 GetSlicedSize(this Sprite sprite)
		{
			var width = sprite.rect.width - sprite.border.x - sprite.border.z;
			var height = sprite.rect.height - sprite.border.y - sprite.border.w;
			return new Vector2(width, height) / sprite.pixelsPerUnit;
		}
		public static Vector3 Z(this Vector2 v, float z)
		{
			return new Vector3(v.x, v.y, z);
		}
		public static Vector3 Z(this Vector2 v, Vector3 vz)
		{
			return v.Z(vz.z);
		}
		public static Color A(this Color c, float alpha)
		{
			c.a = alpha;
			return c;
		}
		public static Bounds EncapsulateBounds<T>(IEnumerable<T> es, Func<T, Bounds> getter)
		{
			Bounds result = new Bounds();
			var hasInited = false;
			foreach (var e in es)
			{
				if (hasInited)
				{
					result.Encapsulate(getter(e));
				}
				else
				{
					result = getter(e);
					hasInited = true;
				}

			}
			return result;
		}
		//public static void SafeDestroyGameObject(this Component comp)
		//{
		//    if (comp)
		//        Destroy(comp.gameObject);
		//}
		//public static void SafeDestroyParentGameObject(this Component comp)
		//{
		//    if (comp)
		//    {
		//        var parent = comp.transform.parent;
		//        if (parent)
		//            Destroy(parent.gameObject);
		//        else
		//            Destroy(comp.gameObject);
		//    }
		//}
		public static void SafeSetEnabled(this Behaviour comp, bool value)
		{
			if (comp)
				comp.enabled = value;
		}
		public static void SafeSetEnabled(this Collider collider, bool value)
		{
			if (collider)
				collider.enabled = value;
		}
		#region Component
		public static T GetComponentSafe<T>(Func<T> getter, AssertHandler handler, string messageFormat, params string[] args) where T : Component
		{
			var result = getter();
			handler?.NotNull(result, messageFormat, args);
			return result;
		}
		public static string GetComponentErrorMessage<T>(string article)
		{
			string result = string.Format("Does not have {0} {1}", article, typeof(T).Name);
			return result;
		}
		public static void GetComponent<T>(this Component c, out T component, AssertHandler handler = null) where T : Component
		{
			component = GetComponentSafe(c.GetComponent<T>, handler, GetComponentErrorMessage<T>("a"));
		}
		public static void GetComponentInParent<T>(this Component c, out T component, string errorMsg) where T : Component
		{
			component = GetComponentSafe<T>(c.GetComponentInParent<T>, Assert.Error, errorMsg);
		}
		public static void GetComponentInParent<T>(this Component c, out T component, AssertHandler handler = null) where T : Component
		{
			component = GetComponentSafe<T>(c.GetComponentInParent<T>, handler, GetComponentErrorMessage<T>("a parent with"));
		}
		public static void GetComponentInChildren<T>(this Component c, out T component, AssertHandler handler = null) where T : Component
		{
			component = GetComponentSafe<T>(c.GetComponentInChildren<T>, handler, GetComponentErrorMessage<T>("a child with"));
		}
		public static void GetComponentAnywhere<T>(this Component c, out T component, AssertHandler handler = null) where T : Component
		{
			component = c.gameObject.GetRoot().GetComponentInChildren<T>();
		}
		public static T FindComponent<T>(this Component c, Func<T, bool> comparator)
		{
			return FindComponentInternal(c.GetComponents<T>(), comparator);
		}
		public static T FindComponentInChildren<T>(this Component c, Func<T, bool> comparator)
		{
			return FindComponentInternal(c.GetComponentsInChildren<T>(), comparator);
		}
		public static T FindComponentInParent<T>(this Component c, Func<T, bool> comparator)
		{
			return FindComponentInternal(c.GetComponentsInParent<T>(), comparator);
		}

		private static T FindComponentInternal<T>(T[] components, Func<T, bool> comparator)
		{
			T result = default;
			foreach (var comp in components)
			{
				if (comparator(comp))
				{
					result = comp;
					break;
				}
			}
			return result;
		}
		#endregion
		#region Creation

		public static T InstantiatePrefab<T>(T prefab, string name = "", Transform parent = null) where T : UnityEngine.Object
		{
			T result = UnityEngine.Object.Instantiate<T>(prefab, parent);
			result.name = name.NoE() ? prefab.name : name;
			return result;
		}
		public static GameObject InstantiatePrefab<T>(GameObject prefab, string prefabName, Vector3 position, Quaternion rotation, Transform parent = null)
		{
			GameObject result = GameObject.Instantiate(prefab, position, rotation, parent);
			result.name = prefabName;
			return result;
		}
		public static GameObject InstantiatePrefab(GameObject prefab, string prefabName, Transform parent = null)
		{
			return InstantiatePrefab<GameObject>(prefab, prefabName, Vector3.zero, Quaternion.identity, parent);
		}
		#endregion
	}
}