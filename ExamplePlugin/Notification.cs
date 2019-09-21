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
        public GenericNotification UINotification { get; set; }
        public Func<string> GetTitle { get; set; }
        public Func<string> GetDescription { get; set; }
        public Transform parent { get; set; }
        public RectTransform rectTransform;
        public float startTime;
        public int index;

        private void Awake()
        {
            parent = RoR2Application.instance.mainCanvas.transform;
            RootObject = Instantiate(Resources.Load<GameObject>("prefabs/notificationpanel2"));
            UINotification = RootObject.GetComponent<GenericNotification>();
            UINotification.transform.SetParent(parent);
            startTime = Time.time;
        }

        private void Update()
        {
            if (UINotification == null)
            {
                Destroy(this);
                return;
            }

            typeof(LanguageTextMeshController).GetField("resolvedString", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(UINotification.titleText, GetTitle());
            typeof(LanguageTextMeshController).GetMethod("UpdateLabel", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(UINotification.titleText, new object[] { });
            typeof(LanguageTextMeshController).GetField("resolvedString", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(UINotification.descriptionText, GetDescription());
            typeof(LanguageTextMeshController).GetMethod("UpdateLabel", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(UINotification.descriptionText, new object[] { });
            float num = (Time.time - startTime) / 0.8f;
            if (num < 1)
            {
                SetPosition(new Vector3(Mathf.SmoothStep(Screen.width * 1.3f, Screen.width * ModConfig.screenPosX / 100f, num), (Screen.height * ModConfig.screenPosY / 100f) - ( rectTransform.sizeDelta.y * index ), 0));
            }
        }

        private void OnDestroy()
        {
            Destroy(UINotification);
            Destroy(RootObject);
        }

        public void SetIcon(Texture texture)
        {
            UINotification.iconImage.enabled = true;
            UINotification.iconImage.texture = texture;
        }

        public void SetPosition(Vector3 position)
        {
            RootObject.transform.position = position;
        }

        public void SetSize(Vector2 size)
        {
            UINotification.GetComponent<RectTransform>().sizeDelta = size;
        }

        public void SetSize(float x, float y)
        {
            rectTransform = UINotification.GetComponent<RectTransform>();
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