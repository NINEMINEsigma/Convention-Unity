using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Convention.WindowsUI.Variant
{
    public class FocusWindowIndictaor : MonoSingleton<FocusWindowIndictaor>
    {
        [Setting, Range(0, 1), Percentage(0, 1)] public float Speed = 0.36f;
        [Resources, OnlyNotNullMode] public RectTransform RectBox;
        [Resources, OnlyNotNullMode] public RectTransform TopParent;
        [Resources] public List<RectTransform> Targets = new();
        [Content] public int TargetIndex;
        public RectTransform Target { get; private set; }

        private Stack<ConventionUtility.ActionStepCoroutineWrapper> Tasks = new();

        public void SetTargetRectTransform(RectTransform target)
        {
            if (target == null)
            {
                Target = null;
                RectTransformInfo.UpdateAnimationPlane(TopParent, RectBox, Speed, 0, true);
            }
            else
            {
                ConventionUtility.ActionStepCoroutineWrapper task = ConventionUtility.CreateSteps();
                task.Next(() =>
                    {
                        if (gameObject.activeInHierarchy == false)
                            RectTransformInfo.UpdateAnimationPlane(TopParent, RectBox, 1, 0, true);
                    })
                    .Until(() => target.gameObject.activeInHierarchy == true && Tasks.Peek() == task, () => Target = target)
                    .Next(() => Tasks.TryPop(out var _))
                    .Invoke();
                Tasks.Push(task);
            }
        }
        public void SelectNextTarget()
        {
            Target = Targets[TargetIndex = (TargetIndex + 1) % Targets.Count];
        }

        private void Start()
        {
            if (TopParent == null)
                TopParent = transform.parent as RectTransform;
        }

        private void LateUpdate()
        {
            if (Target != null)
                RectTransformInfo.UpdateAnimationPlane(Target, RectBox, Speed, 0, true);
            else
                RectTransformInfo.UpdateAnimationPlane(TopParent, RectBox, Speed, 0, true);
        }
    }
}
