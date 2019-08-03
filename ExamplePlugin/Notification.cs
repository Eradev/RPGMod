namespace RPGMod
{
    using RoR2;
    using RoR2.UI;
    using System;
    using System.Reflection;
    using UnityEngine;

    // Thank you kookeh for having this on your repo :)

    public class Notification : MonoBehaviour
    {
        public GameObject RootObject { get; set; }
        public GenericNotification QuestHolder { get; set; }
        public Func<string> GetTitle { get; set; }
        public Func<string> GetDescription { get; set; }
        public Transform Parent { get; set; }

        private void Awake()
        {
            Parent = RoR2Application.instance.mainCanvas.transform;
            RootObject = Instantiate(Resources.Load<GameObject>("prefabs/notificationpanel2"));
            QuestHolder = RootObject.GetComponent<GenericNotification>();
            QuestHolder.transform.SetParent(Parent);
            QuestHolder.iconImage.enabled = true;
        }

        private void Update()
        {
            if (QuestHolder == null)
            {
                Destroy(this);
                return;
            }

            typeof(LanguageTextMeshController).GetField("resolvedString", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(QuestHolder.titleText, GetTitle());
            typeof(LanguageTextMeshController).GetMethod("UpdateLabel", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(QuestHolder.titleText, new object[] { });
            typeof(LanguageTextMeshController).GetField("resolvedString", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(QuestHolder.descriptionText, GetDescription());
            typeof(LanguageTextMeshController).GetMethod("UpdateLabel", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(QuestHolder.descriptionText, new object[] { });
        }

        private void OnDestroy()
        {
            Destroy(QuestHolder);
            Destroy(RootObject);
        }

        public void SetIcon(Texture texture)
        {
            QuestHolder.iconImage.enabled = true;
            QuestHolder.iconImage.texture = texture;
        }

        public void SetPosition(Vector3 position)
        {
            RootObject.transform.position = position;
        }

        public void SetSize(Vector2 size)
        {
            QuestHolder.GetComponent<RectTransform>().sizeDelta = size;
        }

        public void SetSize(float x, float y)
        {
            RectTransform rectTransform = QuestHolder.GetComponent<RectTransform>();
            Vector2 size = rectTransform.sizeDelta;

            if (!float.IsNaN(x))
            {
                size.x = x;
            }

            if (!float.IsNaN(y))
            {
                size.y = y;
            }

            rectTransform.sizeDelta = size;
        }
    }
}