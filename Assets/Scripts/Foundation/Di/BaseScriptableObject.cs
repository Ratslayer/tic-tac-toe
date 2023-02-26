using Sirenix.OdinInspector;
using UnityEngine;
namespace BB
{
	[HideMonoScript]
    public abstract class BaseScriptableObject : SerializedScriptableObject
    {
        private readonly AssertHandler _error = new LogErrorHandler();
        protected AssertHandler AssertError => _error.SetContext(this);
        public override string ToString() => this ? name : "---DESTROYED---";
        protected bool IsApplicationPlaying => Application.isPlaying;
    }
}