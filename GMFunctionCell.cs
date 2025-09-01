using AzurPromiliaPlugin;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib.UI;
using UniverseLib.UI.Models;
using UniverseLib.UI.Widgets.ScrollView;

namespace AzurPromiliaPlugin
{
    public class GMFunctionCell : ICell
    {
        public RectTransform Rect { get; set; }
        public GameObject UIRoot { get; set; }
        public ButtonRef Button { get; set; }
        public Text Title { get; set; }
        public InputFieldRef InputField { get; set; }
        public float DefaultHeight => 25;
        public bool Enabled => UIRoot.activeInHierarchy;
        public void Enable() => UIRoot.SetActive(true);
        public void Disable() => UIRoot.SetActive(false);

        public GameObject CreateContent(GameObject parent)
        {
            UIRoot = UIFactory.CreateUIObject("FunctionRoot", parent);
            Rect = UIRoot.GetComponent<RectTransform>();
            UIFactory.SetLayoutGroup<HorizontalLayoutGroup>(UIRoot, true, true, true, true, 5, 0, 0, 0, 0);
            UIFactory.SetLayoutElement(UIRoot, minWidth: 90, flexibleWidth: 9999, minHeight:25);

            Title = UIFactory.CreateLabel(UIRoot, "FunctionName", "PlaceHolder");
            InputField = UIFactory.CreateInputField(UIRoot, "FunctionArg", "Arguments...");
            UIFactory.SetLayoutElement(InputField.GameObject, minWidth: 60);
            Button = UIFactory.CreateButton(UIRoot, $"SubmitButton", "Execute", new Color(0.2f, 0.2f, 0.2f));
            UIFactory.SetLayoutElement(Button.GameObject, minWidth: 30);

            return UIRoot;
        }

        public void Apply(GMFunctionInfo data)
        {
            Title.text = $"{data.Name}  <color=#a0a0a0ff>{data.Description}</color>";
            InputField.SetActive(data.Type == 1);
            Button.OnClick = () =>
            {
                data.Callback(InputField.Text);
            };
        }
    }
}
