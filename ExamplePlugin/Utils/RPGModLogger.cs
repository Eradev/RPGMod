namespace RPGMod.Utils
{
    internal static class RPGModLogger
    {
        public static void Debug(string message, bool force = false)
        {
            if (force || Core.DebugMode)
            {
                UnityEngine.Debug.Log($"[RPGMOD-DEBUG] {message}");
            }
        }

        public static void Debug(object obj, bool force = false)
        {
            Debug(obj.ToString(), force);
        }

        public static void Error(string message)
        {
            UnityEngine.Debug.LogError($"[RPGMOD] {message}");
        }

        public static void Error(object obj)
        {
            Error(obj.ToString());
        }
    }
}
