using UnityEngine;

namespace Convention.WindowsUI
{
    public class Tooltips : MonoSingleton<Tooltips>, IText, IWindowUIModule
    {
        [Resources] public Text MyText;

        public string text
        {
            get => ((IText)this.MyText).text;
            set
            {
                ((IText)this.MyText).text = value;
            }
        }
    }
}
