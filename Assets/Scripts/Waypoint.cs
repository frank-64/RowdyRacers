using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
