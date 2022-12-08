using UnityEngine;

namespace Assets.Scripts
{
    public enum CornerSeverity
    {
        Warning,
        Severe,
        Low
    }
    public class Waypoint : MonoBehaviour
    {
        public bool breakingRequired;
        public CornerSeverity cornerSeverity;
    }
}
