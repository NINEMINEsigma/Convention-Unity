using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Convention.WindowsUI.Variant
{
    public class SharedModule : MonoSingleton<SharedModule>, IWindowUIModule
    {
        [Resources, OnlyNotNullMode, SerializeField] private ModernUIInputField SingleInputField;
        [Resources, OnlyNotNullMode, SerializeField, WhenAttribute.Not(nameof(SingleInputField), null)] private RectTransform SingleInputFieldRelease;
        [Resources, OnlyNotNullMode, SerializeField, IsInstantiated(false)] private CustomMenu CustomMenuPrefab;
        [Resources, OnlyNotNullMode, SerializeField, WhenAttribute.Not(nameof(CustomMenuPrefab), null)] private RectTransform CustomMenuPlane;
        [Resources, OnlyNotNullMode, SerializeField, WhenAttribute.Not(nameof(CustomMenuPrefab), null)] private Button CustomMenuRelease;
        [Content,SerializeField,OnlyPlayMode]private List<CustomMenu> customMenus = new List<CustomMenu>();
        private Action<string> RenameCallback;

        private void Start()
        {
            SingleInputField.AddListener(x =>
            {
                SingleInputFieldRelease.gameObject.SetActive(false);
                RenameCallback(x);
                SingleInputField.gameObject.SetActive(false);
            });
            this.CustomMenuRelease.onClick.AddListener(() =>
            {
                ReleaseAllCustomMenu();
            });
        }

        private void ReleaseAllCustomMenu()
        {
            foreach (var menu in customMenus)
            {
                menu.ReleaseMenu();
            }
            CustomMenuRelease.gameObject.SetActive(false);
            customMenus.Clear();
        }

        public void SingleEditString([In]string title, [In]string initText, [In]Action<string> callback)
        {
            SingleInputFieldRelease.gameObject.SetActive(true);
            SingleInputField.gameObject.SetActive(true);
            SingleInputField.title = title;
            SingleInputField.text = initText;
            RenameCallback = callback;
        }

        public void Rename([In] string initText, [In] Action<string> callback)
        {
            SingleEditString("Rename", initText, callback);
        }

        [ArgPackage]
        public class CallbackData : AnyClass
        {
            public string name;
            public Action<Vector3> callback;
            public CallbackData(string name, Action<Vector3> callback)
            {
                this.name = name;
                this.callback = callback;
            }
        }
        /// <summary>
        /// �ص������Ĳ�����root
        /// </summary>
        [return: ReturnNotNull, IsInstantiated(true)]
        public CustomMenu OpenCustomMenu([In] RectTransform root, params CallbackData[] actions)
        {
            var target = GameObject.Instantiate(CustomMenuPrefab.gameObject, CustomMenuPlane).GetComponent<CustomMenu>();
            target.gameObject.SetActive(true);
            customMenus.Add(target);
            Vector3[] points = new Vector3[4];
            root.GetWorldCorners(points);
            var rightTop = points[2];
            Vector3[] points2 = new Vector3[4];
            target.rectTransform.GetWorldCorners(points2);
            var leftTop = points2[1];
            target.rectTransform.Translate(rightTop - leftTop, Space.World);
            foreach (var action in actions)
            {
                target.CreateItem(() =>
                {
                    action.callback(rightTop);
                    ReleaseAllCustomMenu();
                }, action.name);
            }
            CustomMenuRelease.gameObject.SetActive(true);
            return target;
        }
    }
}
