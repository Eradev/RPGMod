using UnityEngine;
using UnityEngine.UI;
using RoR2;
using System.Collections;

namespace RPGMod.UI
{
    internal enum UIState
    {
        creating,
        updating
    }

    public static class Utils
    {
        public static AssetBundle assetBundle;
        public static Vector2 screenSize;
        public static bool ready;

        public static float HudScale { get; private set; }

        public static IEnumerator Setup()
        {
            ready = false;
            var hudConVar = Console.instance.FindConVar("hud_scale");

            if (Config.UI.useHUDScale)
            {
                if (hudConVar != null && TextSerialization.TryParseInvariant(hudConVar.GetString(), out float num))
                {
                    HudScale = num / 100f;
                }
            }
            else
            {
                HudScale = Config.UI.overrideHUDScale;
            }

            while (!ready)
            {
                yield return new WaitForSeconds(0.1f);

                if (LocalUserManager.GetFirstLocalUser() == null || LocalUserManager.GetFirstLocalUser()?.cameraRigController?.hud == null)
                {
                    continue;
                }

                screenSize = LocalUserManager.GetFirstLocalUser()?.cameraRigController?.hud.GetComponent<RectTransform>().sizeDelta ?? new Vector2(0, 0);
                if (!(screenSize.x <= 1 || screenSize.y <= 1))
                {
                    ready = true;
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

            border.GetComponent<Image>().sprite = Object.Instantiate(assetBundle.LoadAsset<Sprite>("Assets/UIBorder.png"));
            border.GetComponent<Image>().type = Image.Type.Sliced;
        }
    }
}