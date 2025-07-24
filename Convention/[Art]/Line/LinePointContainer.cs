using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Convention.VFX
{
    [RequireComponent(typeof(LineRenderer))]
    public class LinePointContainer : MonoBehaviour
    {
        [Resources, HopeNotNull, SerializeField] private LineRenderer lineRenderer;
        private void Reset()
        {
            lineRenderer = GetComponent<LineRenderer>();
        }

        private void Start()
        {
            if (lineRenderer == null)
                lineRenderer = GetComponent<LineRenderer>();
        }

        public void Rebuild()
        {
            
        }
    }
}
