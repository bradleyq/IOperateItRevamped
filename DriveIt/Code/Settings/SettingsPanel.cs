using AlgernonCommons.Translation;
using AlgernonCommons.UI;
using ColossalFramework.UI;
using DriveIt.Utils;
using UnityEngine;

namespace DriveIt
{
    public class SettingsPanel : OptionsPanelBase
    {
        public const float Margin = 5f;
        public const float MediumMargin = 24f;
        public const float LargeMargin = 50f;
        public const float StandardComponentHeight = 30f;
        public const float StandardSubpanelMargin = 15f;

        protected override void Setup()
        {
            var tabBar = AutoTabstrip.AddTabstrip(this, 0, 0, width, height, out UITabContainer container, StandardComponentHeight);
            UIPanel settingsObject = UITabstrips.AddTextTab(tabBar, Translations.Translate(DriveCommon.TK_SETTINGS_TAB_SETTINGS), 0, out _);
            UIPanel keybindsObject = UITabstrips.AddTextTab(tabBar, Translations.Translate(DriveCommon.TK_SETTINGS_TAB_KEYBINDS), 1, out _);
            tabBar.EqualizeTabs();

            var settingsScrollPanel = settingsObject.AddUIComponent<UIScrollablePanel>();
            settingsScrollPanel.relativePosition = new Vector2(0, 0);
            settingsScrollPanel.autoSize = false;
            settingsScrollPanel.autoLayout = false;
            settingsScrollPanel.width = container.width - StandardSubpanelMargin;
            settingsScrollPanel.height = container.height;
            settingsScrollPanel.clipChildren = true;
            settingsScrollPanel.builtinKeyNavigation = true;
            settingsScrollPanel.scrollWheelDirection = UIOrientation.Vertical;
            settingsScrollPanel.eventVisibilityChanged += (_, isShow) => { if (isShow) settingsScrollPanel.Reset(); };
            var settingsScroll = UIScrollbars.AddScrollbar(settingsObject, settingsScrollPanel);
            Settings.Tabs.Configuration.Populate(settingsScrollPanel);

            var keybindsScrollPanel = keybindsObject.AddUIComponent<UIScrollablePanel>();
            keybindsScrollPanel.relativePosition = new Vector2(0, 0);
            keybindsScrollPanel.autoSize = false;
            keybindsScrollPanel.autoLayout = false;
            keybindsScrollPanel.width = container.width - StandardSubpanelMargin;
            keybindsScrollPanel.height = container.height;
            keybindsScrollPanel.clipChildren = true;
            keybindsScrollPanel.builtinKeyNavigation = true;
            keybindsScrollPanel.scrollWheelDirection = UIOrientation.Vertical;
            keybindsScrollPanel.eventVisibilityChanged += (_, isShow) => { if (isShow) keybindsScrollPanel.Reset(); };
            var keybindsScroll = UIScrollbars.AddScrollbar(keybindsObject, keybindsScrollPanel);
            Settings.Tabs.Keybinds.Populate(keybindsScrollPanel);

            // Force update to tab 0
            tabBar.selectedIndex = -1;
            tabBar.selectedIndex = 0;
        }
    }
}
