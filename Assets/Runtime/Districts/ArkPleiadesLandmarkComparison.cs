using System.Collections.Generic;
using UnityEngine;
namespace ArkWalk {
    internal sealed class ArkPleiadesLandmarkComparison : MonoBehaviour {
        internal const float PairedScale=.56f;
        internal readonly List<(Transform,Vector3)> Landmarks=new List<(Transform,Vector3)>();
    }
}
