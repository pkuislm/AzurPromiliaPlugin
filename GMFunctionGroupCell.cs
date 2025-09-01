using AzurPromiliaPlugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib.UI;
using UniverseLib.UI.Models;
using UniverseLib.UI.Widgets.ScrollView;

namespace AzurPromiliaPlugin
{
    public class GMFunctionGroupCell : ICell
    {
        public RectTransform Rect { get; set; }
        public GameObject UIRoot { get; set; }
        GMFunction GroupElems;
        Text GroupHeader;
        public float DefaultHeight => 50;
        public bool Enabled => UIRoot.activeInHierarchy;
        public void Enable() => UIRoot.SetActive(true);
        public void Disable() => UIRoot.SetActive(false);

        public GameObject CreateContent(GameObject parent)
        {
            UIRoot = UIFactory.CreateUIObject($"GroupRoot", parent);
            Rect = UIRoot.GetComponent<RectTransform>();
            UIFactory.SetLayoutElement(UIRoot, minHeight: 100, minWidth: 100, flexibleHeight: 9999, flexibleWidth:9999);
            UIFactory.SetLayoutGroup<VerticalLayoutGroup>(UIRoot, true, true, true, true, 5, 2, 2, 2, 2);

            GroupHeader = UIFactory.CreateLabel(UIRoot, $"GroupName", "");
            UIFactory.SetLayoutElement(GroupHeader.gameObject, minWidth: 20, flexibleWidth: 9999);

            GameObject contentObj = UIFactory.CreateUIObject($"Content", UIRoot);
            UIFactory.SetLayoutGroup<VerticalLayoutGroup>(contentObj, true, true, true, true, 5, 0, 0, 0, 0);
            UIFactory.SetLayoutElement(contentObj, minHeight: 0, minWidth: 90, flexibleHeight: 9999, flexibleWidth: 9999);

            var sp = UIFactory.CreateScrollPool<GMFunctionCell>(contentObj, "ScrollPool", out GameObject scrollObj, out GameObject scrollContent);
            UIFactory.SetLayoutGroup<VerticalLayoutGroup>(scrollContent, false, false, true, true, 2, 0, 0, 0, 0);
            UIFactory.SetLayoutElement(scrollObj, minHeight: 0, minWidth: 90, flexibleHeight: 9999, flexibleWidth: 9999);
            UIFactory.SetLayoutElement(scrollContent, minHeight: 0, minWidth: 90, flexibleHeight: 9999, flexibleWidth: 9999);
            GroupElems = new GMFunction(sp);

            return UIRoot;
        }

        public void Apply(KeyValuePair<string, List<GMFunctionInfo>> data)
        {
            GroupHeader.text = data.Key;
            GroupElems.Apply(data.Value);
        }
    }
}
