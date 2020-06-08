using RoR2;

namespace RPGMod.Utils
{
    internal static class RPGModChat
    {
        public static void SendMessage(string message)
        {
            var simpleChatMessage = new Chat.SimpleChatMessage
            {
                
                baseToken = $"<color=#F8BA00>[RPGMOD]</color> {message}"
            };

            Chat.SendBroadcastChat(simpleChatMessage);
        }
    }
}
