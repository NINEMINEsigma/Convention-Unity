using System;
using UnityEngine;

namespace Convention.WindowsUI.Variant
{
    public class InspectorWindow : WindowsComponent
    {
        public static InspectorWindow instance { get; private set; }
        private void Awake()
        {
            if (instance != null)
            {
                throw new InvalidProgramException("mutil-InspectorWindow is awake");
            }
            instance = this;
        }

        [Content] private object target;

        public object GetTarget()
        {
            return target;
        }

        [Content]
        public void ClearWindow()
        {
            if (target != null)
            {
                target = null;

            }
        }

        public void SetTarget(object target)
        {
            if (this.target != target)
            {
                this.target = target;
            }
        }

        [Header("Inspector Items")]
        [Resources, SerializeField] private InspectorBaseItem TextFieldPrefab;
    }
}
