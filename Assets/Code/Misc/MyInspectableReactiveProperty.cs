using System;

#if UNITY_EDITOR
#endif

namespace UniRx
{
#if UNITY_EDITOR


    // InspectorDisplay and for Specialized ReactiveProperty
    // If you want to customize other specialized ReactiveProperty
    [UnityEditor.CustomPropertyDrawer(typeof(Vector3ReactiveProperty))]
    public class ExtendInspectorDisplayDrawer : InspectorDisplayDrawer { }

#endif
}

namespace Cysharp.Threading.Tasks
{
    /// <summary>
    /// Inspectable ReactiveProperty.
    /// </summary>
    [Serializable]
    public class IntAsyncReactiveProperty : AsyncReactiveProperty<int>
    {
        public IntAsyncReactiveProperty(int initialValue)
            : base(initialValue) {

        }
    }
}

