using HarmonyLib;
using RoR2;

namespace RPGMod.Extensions
{
    public static class CharacterBodyExtensions
    {
        public static BuffIndex[] ActiveBuffsList(this CharacterBody self)
        {
            return AccessTools
                .FieldRefAccess<BuffIndex[]>(typeof(CharacterBody), "activeBuffsList")
                .Invoke(self);
        }
    }
}
