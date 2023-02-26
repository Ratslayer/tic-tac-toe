#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
namespace BB
{
    public static class GUIUtils
    {
        public static readonly Color ButtonColor = new Color(0.7f, 0.7f, 0.7f);
        public static readonly Color ToggledButtonColor = new Color(0.5f, 0.5f, 0.5f);
        private delegate T1 GUIField<T1>(string name, T1 value, params GUILayoutOption[] options);
        private delegate T1 GUISlider<T1>(string name, T1 value, T1 min, T1 max, params GUILayoutOption[] options);
        public delegate T1 FieldProcessor<T1>(T1 value);
        public delegate void FieldRenderer<T>(string name, ref T value);
        public static void Float(string name, ref float value, FieldProcessor<float> processor, params GUILayoutOption[] options)
        {
            value = processor != null ? processor(EditorGUILayout.FloatField(name, processor(value), options)) : EditorGUILayout.FloatField(name, value, options);
            //return value;
            //I don't know why this is not working for FloatField, works for everything else
            //return GenericField(name, ref value, EditorGUILayout.FloatField, processor);
        }
        public static float Float(string name, ref float value, Action<float> onChange = null, params GUILayoutOption[] options)
        {
            var prev = value;
            Float(name, ref value, null, options);
            if (prev != value)
                onChange?.Invoke(value);
            return value;
        }
        public static float Float(string name, float value, Action<float> onChange = null, params GUILayoutOption[] options)
        {
            return GenericField(name, value, EditorGUILayout.FloatField, null, onChange, options);
        }
        public static float Float(string name, Func<float> getter, Action<float> setter, params GUILayoutOption[] options)
        {
            var value = getter();
            return Float(name, value, setter, options);
        }
        public static void FloatSlider(string name, ref float value, float min, float max, Action<float> onChange = null)
            => GenericSlider(name, ref value, min, max, EditorGUILayout.Slider, onChange);
        public static float FloatSlider(string name, float value, float min, float max)
            => GenericSlider(name, value, min, max, EditorGUILayout.Slider);
        public static void MinMax(string name, ref float min, ref float max, float minValue, float maxValue)
        {
            BeginVerticalBox(name);
            FloatSlider("Min", ref min, minValue, maxValue);
            FloatSlider("Max", ref max, minValue, maxValue);
            //clamp dat shiet
            var minBound = Mathf.Max(min, minValue);
            var maxBound = Mathf.Min(max, maxValue);
            min = Mathf.Clamp(min, minBound, maxBound);
            max = Mathf.Clamp(max, minBound, maxBound);
            if (Button("Clear"))
            {
                min = minValue;
                max = maxValue;
            }
            EndVertical();
        }
        public static void Int(string name, ref int value, Action<int> onChange = null)
        {
            GenericField(name, ref value, EditorGUILayout.IntField, null, onChange);
        }
        public static void IntSlider(string name, ref int value, int min, int max, Action<int> onChange = null)
            => GenericSlider(name, ref value, min, max, EditorGUILayout.IntSlider, onChange);
        public static int IntSlider(string name, int value, int min, int max, Action<int> onChange = null)
            => GenericSlider(name, value, min, max, EditorGUILayout.IntSlider);
        public static bool Bool(string name, ref bool value, Action<bool> onChange = null, params GUILayoutOption[] options)
        {
            value = Bool(name, value, onChange, options);
            return value;
        }
        public static bool Bool(string name, bool value, Action<bool> onChange = null, params GUILayoutOption[] options)
        {
            return GenericField(name, value, EditorGUILayout.Toggle, null, onChange, options);
        }
        public static bool BoolNoStateChanged(string label, bool initialValue, params GUILayoutOption[] options)
        {
            var newValue = Bool(label, initialValue, null, options);
            return newValue != initialValue;
        }
        public static void String(string name, ref string value, Action<string> onChange = null, params GUILayoutOption[] options)
        {
            value = String(name, value, onChange, options);
        }
        public static string String(string name, string value, Action<string> onChange = null, params GUILayoutOption[] options)
        {
            return GenericField(name, value, EditorGUILayout.TextField, null, onChange, options);
        }
        public static void Curve(string name, ref AnimationCurve value, Action<AnimationCurve> onChange = null)
        {
            GenericField(name, ref value, EditorGUILayout.CurveField, null, onChange);
        }
        public static void V2(string name, ref Vector2 value, FieldProcessor<Vector2> processor, Action<Vector2> onChange = null)
        {
            GenericField(name, ref value, EditorGUILayout.Vector2Field, processor, onChange);
            //value = processor != null ? processor(EditorGUILayout.Vector2Field(name, processor(value))) : EditorGUILayout.Vector2Field(name, value);
        }
        public static void V2(string name, ref Vector2 value)
        {
            V2(name, ref value, null);
        }
        public static void V3(string name, ref Vector3 value, FieldProcessor<Vector3> processor, Action<Vector3> onChange = null)
        {
            GenericField(name, ref value, EditorGUILayout.Vector3Field, processor, onChange);
        }
        public static void Q(string name, ref Quaternion value, Action<Quaternion> onChange = null)
        {
            var v3 = value.eulerAngles;
            V3(name, ref v3);
            value.eulerAngles = v3;
        }
        public static Color Col(string name, Color value, Action<Color> onChange = null)
        {
            var result = GenericField(name, value, EditorGUILayout.ColorField, null, onChange);
            return result;
        }
        public static void Col(string name, ref Color value, Action<Color> onChange = null)
        {
            value = Col(name, value, onChange);
        }
        public static void Mask(string name, ref int value, params string[] options)
        {
            value = EditorGUILayout.MaskField(name, value, options);
        }
        public static IEnumerable<T> MultiSelectList<T>(string name, IEnumerable<T> list, IEnumerable<T> options, Func<T, string> toString)
        {
            var result = new List<T>();
            if (options.Count() <= 32)
            {
                var mask = 0;
                options.ForEach(WriteToMask);
                void WriteToMask(T t, int i)
                {
                    if (list.Contains(t))
                    {
                        mask |= 1 << i;
                    }
                }
                var optionNames = options.ToArray().Convert((i, t) => toString(t));
                Mask(name, ref mask, optionNames);
                options.ForEach(ReadFromMask);
                void ReadFromMask(T t, int i)
                {
                    if ((mask & (1 << i)) != 0)
                    {
                        result.Add(t);
                    }
                }
            }
            else
                Error($"{name} list has {options.Count()} options while the max is 32");
            return result;
        }
        public static IEnumerable<T> MultiSelectList<T>(string name, IEnumerable<T> list, IEnumerable<T> options) where T : UnityEngine.Object
        {
            return MultiSelectList<T>(name, list, options, (obj) => obj.name);
        }
        public static void MultiSelectList<T>(string name, ref T[] array, T[] options) where T : UnityEngine.Object
        {
            if (array == null)
                array = new T[0];
            array = MultiSelectList(name, array, options).ToArray();
        }
        //public static void Layers(string name, ref int value)
        //{
        //    Mask(name, ref value, UnityLayers.GetAllLayerNames());
        //}
        public static void V3(string name, ref Vector3 value)
        {
            V3(name, ref value, null);
        }
        public static Vector3 V3(string name, Vector3 value)
        {
            return EditorGUILayout.Vector3Field(name, value);
        }
        public static Vector3 Position(string name, Transform t)
        {
            t.position = V3(name, t.position);
            return t.position;
        }
        public static Vector3 LocalPosition(string name, Transform t)
        {
            t.localPosition = V3(name, t.localPosition);
            return t.localPosition;
        }
        //public static Octant Octant(string name, ref Octant o)
        //{
        //    BeginHorizontal();
        //    if (name != null)
        //        Label(name);
        //    o.x = Value("Left", o.x, -1);
        //    o.y = Value("Up", o.y, 1);
        //    o.y = Value("Down", o.y, -1);
        //    o.x = Value("Right", o.x, 1);
        //    EndHorizontal();
        //    return o;
        //    int Value(string n, int v, int d)
        //    {
        //        int r = v;
        //        var toggled = v == d;
        //        var newToggled = ToggleButton(n, toggled);
        //        if (newToggled != toggled)
        //            r = newToggled ? d : 0;
        //        return r;
        //    }
        //}
        public static Quaternion Rotation(string name, Transform t)
        {
            t.rotation = Quaternion.Euler(V3(name, t.rotation.eulerAngles));
            return t.rotation;
        }
        public static Quaternion LocalRotation(string name, Transform t)
        {
            t.localRotation = Quaternion.Euler(V3(name, t.localRotation.eulerAngles));
            return t.localRotation;
        }
        public static Vector3 LocalScale(string name, Transform t)
        {
            t.localScale = V3(name, t.localScale);
            return t.localScale;
        }
        public static void Label(string value, params GUILayoutOption[] options)
        {
            EditorGUILayout.LabelField(value, options);
        }
        public static void Label(string value, GUIStyle style, params GUILayoutOption[] options)
        {
            EditorGUILayout.LabelField(value, style, options);
        }
        public static void Label(string leftValue, string rightValue, params GUILayoutOption[] options)
        {
            EditorGUILayout.LabelField(leftValue, rightValue, options);
        }
        public static void Repaint()
        {
            SceneView.RepaintAll();
        }
        public static void SetInspectorLock(bool value)
        {
            ActiveEditorTracker.sharedTracker.isLocked = value;
        }
        public static void SetSelection(UnityEngine.Object obj)
        {
            Selection.SetActiveObjectWithContext(obj, null);
        }
        public static void SetScene2D(bool value)
        {
            SceneView.lastActiveSceneView.in2DMode = value;
        }
        #region Buttons
        public static bool ToggleButton(string name, bool enabled, ref bool toggled, Action onPress = null, Action onDepress = null, params GUILayoutOption[] options)
        {
            var result = false;
            if (enabled)
            {
                result = ToggleButton(name, ref toggled, onPress, onDepress, options);
            }
            else
                Button(name, false, options);
            return result;
        }
        public static bool ToggleButton(string name, ref bool toggled, Action onPress = null, Action onDepress = null, params GUILayoutOption[] options)
        {
            var prev = toggled;
            toggled = GUILayout.Toggle(toggled, name, "Button", options);
            if (toggled != prev)
                if (toggled)
                    onPress.SafeInvoke();
                else
                    onDepress.SafeInvoke();
            return toggled;
        }
        public static bool ToggleButton(string name, bool toggled, Action onPress = null, Action onDepress = null, params GUILayoutOption[] options)
        {
            return ToggleButton(name, ref toggled, onPress, onDepress, options);
        }
        public static bool ToggleButton(string name, bool enabled, bool toggled, Action onPress = null, Action onDepress = null, params GUILayoutOption[] options)
        {
            return ToggleButton(name, enabled, ref toggled, onPress, onDepress, options);
        }
        public static bool ToggleButton<T>(string name, bool enabled, ref T currentValue, T desiredValue, T defaultValue, Action onPress = null, Action onDepress = null, params GUILayoutOption[] options)// where T : struct, IConvertible
        {
            bool before = EqualityComparer<T>.Default.Equals(currentValue, desiredValue);
            bool after = before;
            ToggleButton(name, enabled, ref after, onPress, onDepress, options);
            if (after)
            {
                currentValue = desiredValue;
            }
            else if (before)
            {
                currentValue = defaultValue;
            }
            return after;
        }
        public static void Toolbar<T>(ref T selected, Action<T> onChange, params T[] options)
        {
            var oldId = Array.IndexOf(options, selected);
            var newId = GUILayout.Toolbar(oldId, options.Convert((i, t) => t.ToString()));
            var newSelection = options.SafeGet(newId);
            if (!EqualityComparer<T>.Default.Equals(newSelection, default(T)) && !newSelection.Equals(selected))
            {
                onChange?.Invoke(newSelection);
            }
            selected = newSelection;
        }
        public static bool Button(string name, bool enabled = true, params GUILayoutOption[] options)
        {
            EditorGUI.BeginDisabledGroup(!enabled);
            var result = GUILayout.Button(name, options) && enabled;
            EditorGUI.EndDisabledGroup();
            return result;
        }
        public static void Button(string name, Action action, bool enabled = true, params GUILayoutOption[] options)
        {
            if (Button(name, enabled, options))
            {
                action.Invoke();
            }
        }
        public static void UndoButton(string name, Action action, bool enabled = true, params GUILayoutOption[] options)
        {
            Button(name, () => EditorUtils.GroupUndos(action, name), enabled, options);
        }
        public static bool MouseOverLastControl()
        {
            var e = UnityEngine.Event.current;
            var result = e.type == EventType.Repaint && GUILayoutUtility.GetLastRect().Contains(e.mousePosition);
            return result;
        }
        #endregion
        public static void List<T>(List<T> list, Func<T, string> elementNameGetter, Action<T, int> elementGUI, Action<T> onRemove)
        {
            Collection(list, elementNameGetter, elementGUI, (t) =>
               {
                   list.Remove(t);
                   onRemove?.Invoke(t);
               });
        }
        public static void Collection<T>(IEnumerable<T> collection, Func<T, string> elementNameGetter, Action<T, int> elementGUI, Action<T> remove)
        {
            T removedT = default;
            var index = 0;
            foreach (var t in collection)
            {
                var name = elementNameGetter != null ? elementNameGetter(t) : t.ToString();
                AutoFoldoutGUI(name, t, t, () => Remove(t), () => elementGUI(t, index));
                index++;
            }
            void Remove(T t)
            {
                if (Button("Remove"))
                    removedT = t;
            }
            if (!EqualityComparer<T>.Default.Equals(removedT, default(T)))
            {
                remove(removedT);
            }
        }
        public static void Collection<T>(string name, IEnumerable<T> collection, Func<T, string> elementNameGetter, Action<T, int> elementGUI, Func<T> addGUI, Action<T> add, Action<T> remove)
        {
            T addedT = default;
            AutoFoldoutGUI(name, collection, null, Add, InsideGUI);
            void Add()
            {
                addedT = addGUI();
            }
            void InsideGUI()
            {
                Indent();
                Collection(collection, elementNameGetter, elementGUI, remove);
                Undent();
            }
            if (!EqualityComparer<T>.Default.Equals(addedT, default))
            {
                add?.Invoke(addedT);
            }
        }
        public static void List<T>(string name, List<T> list, Func<T, string> elementNameGetter, Action<T, int> elementGUI, Action<T> onRemove, Func<T> addGUI)
        {
            Collection(name, list, elementNameGetter, elementGUI, addGUI, (t) => list.Add(t), Remove);
            void Remove(T t)
            {
                list.Remove(t);
                onRemove?.Invoke(t);
            }
        }
        public static void AutoFoldoutWithButton(string label, object obj, object context, string buttonLabel, Action buttonAction, Action insideGUI)
        {
            AutoFoldoutGUI(label, obj, context, () => Button(buttonLabel, buttonAction), insideGUI);
        }
        public static void AutoFoldoutGUI(string label, object obj, object context, Action headerGUI, Action insideGUI)
        {
            BeginHorizontal();
            if (AutoFoldout(label, obj, context))
            {
                headerGUI?.Invoke();
                EndHorizontal();
                insideGUI?.Invoke();
            }
            else
                EndHorizontal();
        }
        public static void AutoFoldoutList<T>(string name, List<T> list, Func<T, string> elementNameGetter, Action beginListGUI, Action<T, int> elementGUI, Action endListGUI, Action<T> onRemove = null)
        {
            AutoFoldoutWithButton(name, list, list, "Clear", ClearList, DisplayList);
            void DisplayList()
            {
                Indent();
                beginListGUI?.Invoke();
                List(list, elementNameGetter, elementGUI, onRemove);
                endListGUI?.Invoke();
                Undent();
            }
            void ClearList()
            {
                if (onRemove != null)
                    foreach (var e in list)
                        onRemove(e);
                list.Clear();
            }
        }
        public static void Indent()
        {
            EditorGUI.indentLevel++;
        }
        public static void Undent()
        {
            EditorGUI.indentLevel--;
        }
        public static void Space(float width = 10)
        {
            EditorGUILayout.Space(width);
        }
        public static void BeginHorizontal()
        {
            EditorGUILayout.BeginHorizontal();
        }
        public static void BeginHorizontalBox(string title = "")
        {
            if (!title.NoE())
                Label(title, EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
        }
        public static void BeginVerticalBox(string title = "")
        {
            if (!title.NoE())
                Label(title, EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        }
        public static void BeginHorizontalBox()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
        }
        public static void EndHorizontal()
        {
            EditorGUILayout.EndHorizontal();
        }
        public static void BeginVertical()
        {
            EditorGUILayout.BeginVertical();
        }
        public static void EndVertical()
        {
            EditorGUILayout.EndVertical();
        }
        public static void HorizontalLine()
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }
        public static void Enum<T>(string name, ref T value, Action<T, T> onSelect = null) where T : struct, IConvertible
        {
            var newValue = (T)(object)EditorGUILayout.EnumPopup(name, value as Enum);
            bool changed = !value.Equals(newValue);
            value = newValue;
            if (changed)
                onSelect?.Invoke(value, newValue);
        }
        public static void DisableIf(bool condition, Action guiSection)
        {
            EditorGUI.BeginDisabledGroup(condition);
            guiSection();
            EditorGUI.EndDisabledGroup();
        }
        public static bool Foldout(string label, ref bool value, Action onOpen = null, Action onClose = null)
        {
            var oldValue = value;
            value = EditorGUILayout.Foldout(value, label);
            if (oldValue != value)
            {
                if (value)
                    onOpen?.Invoke();
                else
                    onClose?.Invoke();
            }
            return value;
        }
        private static readonly Hashtable _foldoutHashes = new Hashtable();
        private static readonly Hashtable _toggleHashes = new Hashtable();
        /// <summary>
        /// Foldout that automatically keeps track of all folded out objects. This is pretty hacky and could overlap in various windows
        /// </summary>
        /// <param name="label"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool AutoFoldout(string label, object obj, object context = null)
        {
            if (context == null)
                context = label;
            return AutoFoldout(label, obj, context, null, null);
        }
        public static bool AutoFoldout(string label, object obj, object context, Action onOpen, Action onClose)
        {
            //var hash = GetGUIHash(obj, context);
            //var prevValue = AutoFoldoutHashExists(hash);
            //var curValue = EditorGUILayout.Foldout(prevValue, label);
            //if (curValue != prevValue)
            //{
            //    if (curValue)
            //    {
            //        _foldoutHashes.Add(hash, null);
            //        onOpen?.Invoke();
            //    }
            //    else
            //    {
            //        _foldoutHashes.Remove(hash);
            //        onClose?.Invoke();
            //    }
            //}
            //return curValue;
            var result = AutoBool((value) => EditorGUILayout.Foldout(value, label), obj, context, _foldoutHashes, onOpen, onClose);
            return result;
        }
        public static bool AutoToggleButton(string label, object obj, object context, Action onPress = null, Action onDepress = null)
        {
            var result = AutoBool((value) => ToggleButton(label, value), obj, context, _toggleHashes, onPress, onDepress);
            return result;
        }
        public static bool HasAutoFoldout(object obj, object context = null)
        {
            var hash = GetGUIHash(obj, context);
            return AutoFoldoutHashExists(hash);
        }
        private static bool AutoBool(Func<bool, bool> boolMethod, object obj, object context, Hashtable table, Action onEnable, Action onDisable)
        {
            var hash = GetGUIHash(obj, context);
            var prevValue = AutoFoldoutHashExists(hash);
            var curValue = boolMethod(prevValue);//EditorGUILayout.Foldout(prevValue, label);
            if (curValue != prevValue)
            {
                if (curValue)
                {
                    _foldoutHashes.Add(hash, null);
                    onEnable?.Invoke();
                }
                else
                {
                    _foldoutHashes.Remove(hash);
                    onDisable?.Invoke();
                }
            }
            return curValue;
        }
        private static bool AutoFoldoutHashExists(int hash)
        {
            return _foldoutHashes.ContainsKey(hash);
        }
        private static bool AutoToggleHashExists(int hash)
        {
            return _toggleHashes.ContainsKey(hash);
        }
        private static int GetGUIHash(object obj, object context)
        {
            var result = obj.GetHashCode();
            if (context != null)
            {
                result *= context.GetHashCode();
            }
            return result;
        }
        public delegate void OnObjectChange<T>(T oldValue, T newValue);
        // public static T Object<T>(string name, ref T value, OnObjectChange<T> onChangeAction, bool allowSceneObjects = true) where T : UnityEngine.Object
        // {
        //     value = Object(name, value, onChangeAction, allowSceneObjects);
        //     return value;
        // }
        public static T Object<T>(string name, T value, OnObjectChange<T> onChangeAction = null, bool allowSceneObjects = true) where T : UnityEngine.Object
        {
            var result = Object(name, ref value, onChangeAction, allowSceneObjects);
            return result;
        }
        public static T Object<T>(string name, ref T value, OnObjectChange<T> onChangeAction = null, bool allowSceneObjects = true) where T : UnityEngine.Object
        {
            var oldValue = value;
            value = (T)EditorGUILayout.ObjectField(name, value, typeof(T), allowSceneObjects);
            if (oldValue != value)
                onChangeAction?.Invoke(oldValue, value);
            return value;
        }
        // public static T Object<T>(string name, ref T value, OnObjectChange<T> onChangeAction, bool allowSceneObjects = true) where T : UnityEngine.Object
        // {
        //     return Object(name, ref value, onChangeAction, allowSceneObjects);
        // }
        // public static T Object<T>(string name, ref T value, bool allowSceneObjects = true) where T : UnityEngine.Object
        // {
        //     return Object(name, ref value, default(OnObjectChange<T>), allowSceneObjects);
        // }
        // public static T Object<T>(string name, T value, bool allowSceneObjects = true) where T : UnityEngine.Object
        // {
        //     return Object(name, value, default(OnObjectChange<T>), allowSceneObjects);
        // }
        public static void Object(string name, Type type, ref UnityEngine.Object value, bool allowSceneObjects = true)
        {
            value = EditorGUILayout.ObjectField(name, value, type, allowSceneObjects);
        }
        public static void List<T>(string label, ref T value, Action<T> onChange, params T[] options)
        {
            List(label, ref value, onChange, false, options);
        }
        public static void List<T>(string label, ref T value, Action<T> onChange, bool allowNull, params T[] options)
        {
            Select(label, ref value, onChange, GetName, allowNull, options);
            string GetName(T o)
            {
                string name;
                switch (o)
                {
                    //case IGUIListable io:
                    //    name = io.GetDisplayName();
                    //    break;
                    case UnityEngine.Object uo:
                        name = uo ? uo.name : "Destroyed Object";
                        break;
                    default:
                        name = o.ToString();
                        break;
                }
                return name;
            }
        }
        public static void ListProperty<T>(UnityEngine.Object target, string propertyName, List<T> list, Func<T> addItemUI) where T : class, new()
        {
            T addedItem = null;
            var removeIndex = -1;
            using (var serializedObj = new SerializedObject(target))
            {
                var listProperty = serializedObj.FindProperty(propertyName);
                BeginHorizontal();
                var foldout = AutoFoldout(propertyName, target, propertyName);
                if (foldout)//&& Button("Add"))
                {
                    addedItem = addItemUI();
                }
                EndHorizontal();
                if (foldout)
                {
                    Indent();
                    for (int i = 0; i < listProperty.arraySize; i++)
                    {
                        SerializedProperty elementProperty = listProperty.GetArrayElementAtIndex(i);
                        EditorGUILayout.PropertyField(elementProperty, new GUIContent(list[i]?.ToString()), true);
                        if (Button("Remove"))
                            removeIndex = i;
                    }
                    Undent();
                }
                serializedObj.ApplyModifiedProperties();
            }
            if (addedItem != null)
                list.Add(addedItem);
            if (removeIndex != -1)
                list.RemoveAt(removeIndex);
        }
        public static void ListProperty<T>(UnityEngine.Object target, string propertyName, List<T> list) where T : class, new()
        {
            ListProperty(target, propertyName, list, () => Button("Add") ? new T() : null);
        }
        public static void Help(string msg, MessageType type)
        {
            EditorGUILayout.HelpBox(msg, type);
        }
        public static void Error(string msg)
        {
            Help(msg, MessageType.Error);
        }
        public static void Warning(string msg)
        {
            Help(msg, MessageType.Warning);
        }
        public static void Info(string msg)
        {
            Help(msg, MessageType.Info);
        }
        public static void Select<T>(string label, ref T value, IEnumerable<T> values, Action<T> onChange = null, Func<T, string> nameGetter = null, bool prependNull = false)
        {
            T[] options;
            if (values != null)
            {
                if (prependNull)
                    options = values.Prepend(default).ToArray();
                else
                    options = values.ToArray();
            }
            else
                options = new T[0];
            var currentIndex = Array.IndexOf(options, value);//, (e1, e2) => GetName(e1) == GetName(e2));
            var stringOptions = from o in options select GetName(o);
            var newIndex = EditorGUILayout.Popup(label, currentIndex, stringOptions.ToArray());
            if (newIndex != -1)
            {
                if (newIndex != currentIndex)
                {
                    value = options[newIndex];
                    onChange?.Invoke(options[newIndex]);
                }
            }
            string GetName(T t)
            {
                return t != null ? nameGetter != null ? nameGetter(t) : t.ToString() : "None";
            }
        }
        public static T Select<T>(string label, T value, IEnumerable<T> values, Action<T> onChange = null, Func<T, string> nameGetter = null, bool prependNull = false)
        {
            Select(label, ref value, values, onChange, nameGetter, prependNull);
            return value;
        }
        public static T Select<T>(string label, T value, Action<T> onChange, Func<T, string> nameGetter, bool allowNull, params T[] options)
        {
            Select(label, ref value, options, onChange, nameGetter, allowNull);
            return value;
        }
        public static void Select<T>(string label, ref T value, Action<T> onChange, Func<T, string> nameGetter, bool allowNull, params T[] options)
        {
            Select(label, ref value, options, onChange, nameGetter, allowNull);
        }
        private static T FieldPostRender<T>(T field, T value, FieldProcessor<T> processor, Action<T> onChange)
        {
            var prevField = field;
            field = processor != null ? processor(value) : value;
            if (!CSHelper.DefaultEquals(field, prevField))
                onChange.SafeInvoke(field);
            return field;
        }
        private static T GenericField<T>(string name, ref T field, GUIField<T> renderer, FieldProcessor<T> processor, Action<T> onChange, params GUILayoutOption[] options)
        {
            field = GenericField<T>(name, field, renderer, processor, onChange, options);
            return field;
        }
        private static T GenericField<T>(string name, T field, GUIField<T> renderer, FieldProcessor<T> processor, Action<T> onChange, params GUILayoutOption[] options)
        {
            T value = renderer(name, field, options);
            var result = FieldPostRender(field, value, processor, onChange);
            return result;
        }
        private static T GenericSlider<T>(string name, T field, T min, T max, GUISlider<T> renderer, params GUILayoutOption[] options) => renderer(name, field, min, max, options);
        private static void GenericSlider<T>(string name, ref T field, T min, T max, GUISlider<T> renderer, Action<T> onChange, params GUILayoutOption[] options)
        {
            EditorGUI.BeginChangeCheck();
            field = GenericSlider(name, field, min, max, renderer, options);
            if (EditorGUI.EndChangeCheck())
                onChange?.Invoke(field);
        }
        public static GameObject GetSelectedGameObject()
        {
            return Selection.activeGameObject;
        }
        public static void Universal(string name, Type type, ref object obj)
        {
            switch (obj)
            {
                case float f:
                    Float(name, ref f);
                    obj = f;
                    break;
                case int i:
                    Int(name, ref i);
                    obj = i;
                    break;
                case bool b:
                    Bool(name, ref b);
                    obj = b;
                    break;
                case string s:
                    String(name, ref s);
                    obj = s;
                    break;
                case Vector2 v:
                    V2(name, ref v);
                    obj = v;
                    break;
                case Vector3 v:
                    V3(name, ref v);
                    obj = v;
                    break;
                case UnityEngine.Object o:
                    Object(name, type, ref o);
                    obj = o;
                    break;
                case null:
                    if (type == typeof(string))
                    {
                        string s = null;
                        String(name, ref s);
                        obj = s;
                    }
                    else
                    {

                        var n = (UnityEngine.Object)obj;
                        Object(name, type, ref n);
                        obj = n;
                    }
                    break;
                default:
                    Assert.Error.Log($"Could not find suitable GUI display function for param '{name}' of type '{type}'");
                    break;
            }
        }
        public static void Universal(Field[] allObjects)
        {
            for (int i = 0; i < allObjects.Length; i++)
            {
                var element = allObjects[i];
                Universal(element.Name, element.Type, ref element.Value);
            }
        }
        public static void Property(UnityEngine.Object obj, params string[] names)
        {
            using (var serializedObj = new SerializedObject(obj))
            {
                var propertyName = names.Join(".");
                var property = serializedObj.FindProperty(propertyName);
                serializedObj.Update();
                Property(property);
                serializedObj.ApplyModifiedProperties();
            }
        }
        private static SerializedProperty GetProperty(SerializedObject obj, params string[] names)
        {
            var propertyName = names.Join(".");
            var property = obj.FindProperty(propertyName);
            return property;
        }
        public static SerializedProperty GetProperty(this SerializedProperty property, params string[] names)
        {
            var propertyName = $"{property.propertyPath}.{names.Join(".")}";
            var result = GetProperty(property.serializedObject, propertyName);
            return result;
        }
        public static bool PropertyList(string name, SerializedProperty property)
        {
            var result = false;
            var propertyObject = GetTargetObjectOfProperty(property);
            if (propertyObject is IList list)
            {
                result = AutoFoldout(name, property.serializedObject.targetObject);
                if (result)
                {
                    Indent();
                    object removedObj = null;
                    var length = property.arraySize;
                    for (int i = 0; i < length; i++)
                    {
                        var elementProperty = property.GetArrayElementAtIndex(i);
                        var elementObject = GetTargetObjectOfProperty(elementProperty);
                        Property(elementObject.ToString(), elementProperty);
                        if (elementProperty.isExpanded && Button("Remove"))
                            removedObj = elementObject;
                    }
                    if (removedObj != null)
                        list.Remove(removedObj);
                    Undent();
                }
            }
            else
                Debug.Log($"Trying to display {name} property as a list, while it's not a list.");
            return result;
        }
        //taken from https://github.com/lordofduct/spacepuppy-unity-framework/blob/master/SpacepuppyBaseEditor/EditorHelper.cs
        public static object GetTargetObjectOfProperty(SerializedProperty prop)
        {
            if (prop == null)
                return null;

            var path = prop.propertyPath.Replace(".Array.data[", "[");
            object obj = prop.serializedObject.targetObject;
            var elements = path.Split('.');
            foreach (var element in elements)
            {
                if (element.Contains("["))
                {
                    var elementName = element.Substring(0, element.IndexOf("["));
                    var index = System.Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                    obj = GetValue_Imp(obj, elementName, index);
                }
                else
                {
                    obj = GetValue_Imp(obj, element);
                }
            }
            return obj;
        }
        private static object GetValue_Imp(object source, string name)
        {
            if (source == null)
                return null;
            var type = source.GetType();

            while (type != null)
            {
                var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (f != null)
                    return f.GetValue(source);

                var p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (p != null)
                    return p.GetValue(source, null);

                type = type.BaseType;
            }
            return null;
        }

        private static object GetValue_Imp(object source, string name, int index)
        {
            var enumerable = GetValue_Imp(source, name) as System.Collections.IEnumerable;
            if (enumerable == null)
                return null;
            var enm = enumerable.GetEnumerator();
            //while (index-- >= 0)
            //    enm.MoveNext();
            //return enm.Current;

            for (int i = 0; i <= index; i++)
            {
                if (!enm.MoveNext())
                    return null;
            }
            return enm.Current;
        }
        public static void Property(SerializedProperty property)
        {
            EditorGUILayout.PropertyField(property, true);
        }
        public static void Property(string name, SerializedProperty property)
        {
            EditorGUILayout.PropertyField(property, new GUIContent(name), true);
        }
        public static void PropertyArrayElement(UnityEngine.Object obj, string propertyName, int index)
        {
            using (var serializedObj = new SerializedObject(obj))
            {
                var arrayProperty = serializedObj.FindProperty(propertyName);
                var property = arrayProperty.GetArrayElementAtIndex(index);
                serializedObj.Update();
                EditorGUILayout.PropertyField(property, true);
                serializedObj.ApplyModifiedProperties();
            }
        }
        public static void AllProperties(UnityEngine.Object obj)
        {
            DisplayProperties(obj, (prop) => prop.name != "m_Script" && !prop.propertyPath.Contains("."));
        }
        public static void Properties(UnityEngine.Object obj, params string[] names)
        {
            ProcessProperties(obj, Display);
            void Display(SerializedObject sobj)
            {
                foreach (var name in names)
                {
                    var prop = sobj.FindProperty(name);
                    EditorGUILayout.PropertyField(prop, true);
                }
            }
        }
        private static void ProcessProperties(UnityEngine.Object obj, Action<SerializedObject> action)
        {
            using (var serializedObj = new SerializedObject(obj))
            {
                serializedObj.Update();
                action(serializedObj);
                serializedObj.ApplyModifiedProperties();
            }
        }
        private static void DisplayProperties(UnityEngine.Object obj, Predicate<SerializedProperty> predicate)
        {
            ProcessProperties(obj, Display);
            void Display(SerializedObject serializedObj)
            {
                var prop = serializedObj.GetIterator();
                while (prop.NextVisible(true))
                {
                    if (predicate(prop))
                        EditorGUILayout.PropertyField(prop, true);
                }
            }
        }

        public class Field
        {
            public string Name;
            public Type Type;
            public object Value;
        }
        public class SceneObjectSelector<T> : AbstractSelector<T> where T : UnityEngine.Object
        {
            public T Selection
            {
                get
                {
                    return objects.SafeGet(GetSelectionIndex());
                }
            }
            private T[] objects;
            private Predicate<T> filter;
            public SceneObjectSelector(string listName, Predicate<T> filter = null) : base(listName)
            {
                this.filter = filter;
            }

            public override void OnChangeSelection(string newSelection)
            {
            }

            protected override string[] GetOptionsInternal()
            {
                objects = UnityEngine.Object.FindObjectsOfType<T>();
                if (filter != null)
                    objects = Array.FindAll(objects, filter);
                return objects.Convert((i, o) => o.name);
            }
        }
        public abstract class AbstractSelector<T>
        {
            protected const string NoneString = "None";
            protected string[] options;
            public string SelectedName;
            private string listName;
            protected abstract string[] GetOptionsInternal();
            public virtual void OnChangeSelection(string newSelection)
            {
            }
            protected virtual string NicifyOptionName(string option)
            {
                return option;
            }
            protected virtual bool AllowNone()
            {
                return false;
            }
            public virtual void Display()
            {
                List();
            }
            protected void List()
            {
                GUIUtils.Select(listName, ref SelectedName, OnChangeSelection, NicifyOptionName, false, options);
            }
            public void Select(string newSelection)
            {
                SelectedName = newSelection;
                OnChangeSelection(newSelection);
            }
            public void GetOptions()
            {
                options = GetOptionsInternal();
                if (AllowNone())
                    options = options.Prepend(NoneString).ToArray();
            }
            protected int GetSelectionIndex()
            {
                return Array.IndexOf(options, SelectedName);
            }
            public bool HasSelection()
            {
                return GetSelectionIndex() >= 0;
            }
            public AbstractSelector(string listName)
            {
                this.listName = listName;
                GetOptions();
                SelectedName = AllowNone() ? NoneString : options.FirstOrDefault();
            }
        }
    }
}
#endif