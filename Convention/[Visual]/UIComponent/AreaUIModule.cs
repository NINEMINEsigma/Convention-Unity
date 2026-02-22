using UnityEngine;
using UnityEngine.EventSystems;

namespace Convention.WindowsUI
{
    public class AreaUIModule : WindowUIModule, IText
    {
        [Header(nameof(AreaUIModule))]
        [Setting] public string AreaInfo = "";

        public string text { get => AreaInfo; set => AreaInfo = value; }

        private void OnPointerEnter(PointerEventData eventData)
        {
            if (Tooltips.instance == null)
                return;
            Tooltips.instance.text = AreaInfo;
        }

        private void OnPointerExit(PointerEventData eventData)
        {
            if (Tooltips.instance == null)
                return;
            Tooltips.instance.text = "";
        }

        protected virtual void Reset()
        {
            AreaInfo = "";
        }

        protected virtual void Start()
        {
            var context = this.GetOrAddComponent<BehaviourContextManager>();
            context.OnPointerEnterEvent = BehaviourContextManager.InitializeContextSingleEvent(context.OnPointerEnterEvent, OnPointerEnter);
            context.OnPointerExitEvent = BehaviourContextManager.InitializeContextSingleEvent(context.OnPointerExitEvent, OnPointerExit);
        }
    }
}
