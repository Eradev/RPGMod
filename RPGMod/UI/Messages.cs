using RPGMod.Extensions;
using System.Collections.Generic;

namespace RPGMod.UI
{
    internal static class Messages
    {
        private static readonly List<string> _newQuest = new List<string>
        {
            "It's time to work again, <b><color=orange>{0}</color></b>! Can you complete these on time?",
            "Here are your new orders, <b><color=orange>{0}</color></b>. Get on it!",
            "You wanted more work? I deliver!",
            "New orders are in. Time is of the essence, <b><color=orange>{0}</color></b>.",
            "What is this? A shiny new toy? Complete these missions and you'll be gifted with it!",
            "How about you work on these tasks while you're at it? You'll be well rewarded.",
            "Hello <b><color=orange>{0}</color></b>. New orders. Be quick. Bye."
        };

        public static string NewQuestAnnouncement => _newQuest.Random();

        private static readonly List<string> _questFailed = new List<string>
        {
            "I'm disappointed in you, <b><color=orange>{0}</color></b>. I hope you'll do better next time.",
            "*sigh* I'm surprised you survived so long with your skills, or lack thereof.",
            "I was looking forward to good results. Maybe I should ask another survivor...",
            "Time's up! Did you enjoy the scenery for too long, <b><color=orange>{0}</color></b>?",
            "Was it bad luck, or are you simply bad at your job?",
            "You won't stand a chance against <b>Mithrix</b> if you can't even complete these simple tasks!",
            "It would take a month to explain to you my disappointment.",
            "Was it too hard, <b><color=orange>{0}</color></b>? I'll keep it in mind for next time.",
            "It's ok <b><color=orange>{0}</color></b>. I wasn't expecting much from you."
        };

        public static string QuestFailedAnnouncement => _questFailed.Random();

        private static readonly List<string> _questComplete = new List<string>
        {
            "Mission accomplished! Take your reward. You earned it.",
            "I knew I could count on you, <b><color=orange>{0}</color></b>!",
            "Here, take this. Good work <b><color=orange>{0}</color></b>!",
            "It was too easy for you! Maybe I'll ramp up the difficulty next time.",
            "You've raised the bar again, <b><color=orange>{0}</color></b>. Amazing!",
            "Done already?! Color me surprised! Here's your reward.",
            "Here's your reward for a job well done!",
            "Nothing tastes better than success... except the reward you just got!",
            "Hell yeah, <b><color=orange>{0}</color></b>! You really came through!",
            "<b>UES : Safe Travels</b> is lucky to have you, <b><color=orange>{0}</color></b>!"
        };

        public static string QuestCompleteAnnouncement => _questComplete.Random();
    }
}
