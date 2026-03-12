using AlgernonCommons;
using AlgernonCommons.Notifications;
using AlgernonCommons.Patching;
using AlgernonCommons.Translation;
using ICities;
using DriveIt.Utils;
using DriveIt.Settings;

namespace DriveIt
{
    public sealed class DriveItMod : PatcherMod<SettingsPanel, PatcherBase>, IUserMod
    {
        public override string BaseName => DriveCommon.MOD_NAME;
        public override string HarmonyID => DriveCommon.MOD_HARMONY_ID;
        public string Description => Translations.Translate(DriveCommon.TK_MOD_DESCRIPTION);
        public override void LoadSettings() => ModSettings.Load();
        public override void SaveSettings() => ModSettings.Save();
        public override WhatsNewMessage[] WhatsNewMessages => new WhatsNewMessage[]
        {
            new WhatsNewMessage
            {
                Version = AssemblyUtils.CurrentVersion,
                MessagesAreKeys = true,
                Messages = new string[]
                {
                    DriveCommon.TK_WHATSNEW_L1,
                    DriveCommon.TK_WHATSNEW_L2,
                    DriveCommon.TK_WHATSNEW_L3,
                }
            }
        };
        public override void OnEnabled()
        {
            base.OnEnabled();
        }
    }
}
