using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
namespace BB
{
    public static class InputExtensionMethods
    {
        public static bool Matches(this InputButton obj1, InputButton obj2) => obj1 && obj1 == obj2;
        public static bool MatchesAny(this InputButton obj, IEnumerable<InputButton> objects) => obj && objects.Contains(o => o.Matches(obj));
    }
    public class InputButton : BaseScriptableObject
    {
        [SerializeField]
        [FormerlySerializedAs("_inputName")]
        private string _actionName;
        public string ActionName => _actionName;
    }
}