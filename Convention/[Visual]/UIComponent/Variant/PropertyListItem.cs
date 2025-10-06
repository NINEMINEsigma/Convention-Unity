using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static Convention.WindowsUI.Variant.PropertiesWindow;

namespace Convention.WindowsUI.Variant
{
    public class PropertyListItem : WindowUIModule, ITitle, IText, IItemEntry, IActionInvoke
    {
        [Resources, SerializeField] private Button m_rawButton;
        [Resources, SerializeField] private ModernUIButton m_modernButton;
        public GameObject ButtonGameObject => m_rawButton == null ? m_modernButton.gameObject : m_rawButton.gameObject;
        [Resources, SerializeField] private float layerTab = 7.5f;
        [Resources, SerializeField] private float layerHeight = 15f;
        [Resources, SerializeField, OnlyNotNullMode] private RectTransform dropdownImage;
        [Resources, SerializeField, OnlyNotNullMode] private Text m_buttonText;
        [Resources, SerializeField, OnlyNotNullMode, Header("Self Layer")] private RectTransform m_Layer;

        public RectTransform TextRectTransform;

        [Content, SerializeField] private ItemEntry m_entry;
        [Content, SerializeField] private bool m_folderStats = true;

        public ItemEntry Entry
        {
            get => m_entry;
            set
            {
                if (this.gameObject.activeInHierarchy &&
                    //因为unity会生成可序列化的成员, 所以需要再检查类内必定存在的成员是否生成
                    (m_entry != null && m_entry.rootWindow != null))
                {
                    throw new InvalidOperationException();
                }
                m_entry = value;
                m_entry.ref_value = this;
            }
        }

        private void Relayer()
        {
            m_Layer.sizeDelta = new(m_entry != null ? layerTab * m_entry.layer : 0, layerHeight);
        }

        protected virtual void Start()
        {
            ButtonGameObject.AddComponent<RectTransformExtension.AdjustSizeIgnore>();
            dropdownImage.gameObject.AddComponent<RectTransformExtension.AdjustSizeIgnore>();
            m_buttonText.gameObject.AddComponent<RectTransformExtension.AdjustSizeIgnore>();
            if (m_rawButton != null)
                m_rawButton.onClick.AddListener(Switch);
            else
                m_modernButton.AddListener(Switch);
            TextRectTransform = m_buttonText.GetComponent<RectTransform>();
            dropdownImage.eulerAngles = new(0, 0, IsFold ? 90 : 0);
        }

        protected virtual void OnEnable()
        {
            Relayer();
        }

        public bool IsFold
        {
            get => m_folderStats;
            set
            {
                if (value != m_folderStats)
                {
                    m_folderStats = value;
                    if (value)
                    {
                        FoldChilds();
                    }
                    else
                    {
                        UnfoldChilds();
                    }
                }
            }
        }

        public virtual string title { get => m_buttonText.title; set => m_buttonText.title = value; }
        public virtual string text { get => m_buttonText.text; set => m_buttonText.text = value; }
        public virtual bool interactable
        {
            get
            {
                if (m_rawButton != null)
                    return m_rawButton.interactable;
                else
                    return m_modernButton.interactable;
            }
            set
            {
                if (m_rawButton != null)
                    m_rawButton.interactable = value;
                else
                    m_modernButton.interactable = value;
            }
        }

        public void Switch()
        {
            IsFold = !IsFold;
        }

        protected virtual void FoldChilds()
        {
            dropdownImage.eulerAngles = new(0, 0, 90);
            m_entry.DisableChilds(true);
        }
        protected virtual void UnfoldChilds()
        {
            m_entry.EnableChilds(true);
            dropdownImage.eulerAngles = new(0, 0, 0);
        }

        public virtual void RefreshChilds()
        {
            ConventionUtility.CreateSteps()
                .Next(() => Switch())
                .Next(() => Switch())
                .Invoke();
        }

        public List<ItemEntry> CreateSubPropertyItem([In] PropertiesWindow propertyWindow, int count)
        {
            List<ItemEntry> result = new();
            while (count-- > 0)
            {
                var item = ItemEntry.MakeItemWithInstantiate(propertyWindow.ItemPrefab, this.Entry);
                (item.ref_value as PropertyListItem).Entry = item;
                result.Add(item);
            }
            return result;
        }
        public List<ItemEntry> CreateSubPropertyItem(int count)
        {
            return CreateSubPropertyItem(Entry.rootWindow, count);
        }
        public List<ItemEntry> CreateSubPropertyItem([In] WindowUIModule prefab, int count)
        {
            List<ItemEntry> result = new();
            while (count-- > 0)
            {
                var item = ItemEntry.MakeItemWithInstantiate(prefab, this.Entry);
                (item.ref_value as PropertyListItem).Entry = item;
                result.Add(item);
            }
            return result;
        }

        [Content]
        public void AdjustSizeToContainsChilds()
        {
            RectTransformExtension.AdjustSizeToContainsChilds(transform as RectTransform);
        }

        public IActionInvoke AddListener(params UnityAction[] action)
        {
            foreach (var item in action)
            {
                if (m_rawButton != null)
                    m_rawButton.onClick.AddListener(item);
                else
                    m_modernButton.AddListener(item);
            }
            return this;
        }

        public IActionInvoke RemoveListener(params UnityAction[] action)
        {
            foreach (var item in action)
            {
                if (m_rawButton != null)
                    m_rawButton.onClick.RemoveListener(item);
                else
                    m_modernButton.RemoveListener(item);
            }
            return this;
        }

        public IActionInvoke RemoveAllListeners()
        {
            if (m_rawButton != null)
                m_rawButton.onClick.RemoveAllListeners();
            else
                m_modernButton.RemoveAllListeners();
            return this;
        }
    }
}

