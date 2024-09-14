using RoR2;

namespace RPGMod.Extensions
{
    public static class BuffDefExtensions
    {
        public static string GetDisplayName(this BuffDef self)
        {
            return !self.isElite
                ? self.name
                : Language.GetString(self.eliteDef.modifierToken).Replace("{0}", string.Empty).Trim();
        }
    }
}
