using RoR2;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace RPGMod.UI
{
    internal enum UIState
    {
        Creating,
        Updating
    }

    public static class Utils
    {
        public static AssetBundle AssetBundle;
        public static Vector2 ScreenSize;
        public static bool IsReady;

        public static float HudScale { get; private set; }

        public static IEnumerator Setup()
        {
            IsReady = false;
            var hudConVar = Console.instance.FindConVar("hud_scale");

            if (Config.UI.UseHUDScale)
            {
                if (hudConVar != null && TextSerialization.TryParseInvariant(hudConVar.GetString(), out float num))
                {
                    HudScale = num / 100f;
                }
            }
            else
            {
                HudScale = Config.UI.OverrideHUDScale;
            }

            while (!IsReady)
            {
                yield return new WaitForSeconds(0.1f);

                if (LocalUserManager.GetFirstLocalUser() == null || LocalUserManager.GetFirstLocalUser()?.cameraRigController?.hud == null)
                {
                    continue;
                }

                ScreenSize = LocalUserManager.GetFirstLocalUser()?.cameraRigController?.hud.GetComponent<RectTransform>().sizeDelta ?? new Vector2(0, 0);
                if (!(ScreenSize.x <= 1 || ScreenSize.y <= 1))
                {
                    IsReady = true;
                }
            }
        }

        public static void AddBorder(GameObject element)
        {
            var border = new GameObject
            {
                name = "border"
            };

            border.AddComponent<RectTransform>();
            border.AddComponent<Image>();

            border.transform.SetParent(element.transform);

            border.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
            border.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
            border.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
            border.GetComponent<RectTransform>().sizeDelta = new Vector2(element.GetComponent<RectTransform>().sizeDelta.x + 2, element.GetComponent<RectTransform>().sizeDelta.y + 2);
            border.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);

            border.GetComponent<Image>().sprite = Object.Instantiate(AssetBundle.LoadAsset<Sprite>("Assets/UIBorder.png"));
            border.GetComponent<Image>().type = Image.Type.Sliced;
        }
    }
}