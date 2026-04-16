using AlgernonCommons.Translation;
using AlgernonCommons.UI;
using ColossalFramework.UI;
using DriveIt.Utils;
using System.ComponentModel;
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

        private static UIScrollablePanel CreateScrollable(UITabContainer container, UIPanel panel)
        {
            UIScrollablePanel scrollPanel = panel.AddUIComponent<UIScrollablePanel>();
            UIScrollbar settingsScroll = UIScrollbars.AddScrollbar(panel, scrollPanel);

            scrollPanel.relativePosition = new Vector2(0, 0);
            scrollPanel.autoSize = false;
            scrollPanel.autoLayout = false;
            scrollPanel.width = container.width - StandardSubpanelMargin;
            scrollPanel.height = container.height;
            scrollPanel.clipChildren = true;
            scrollPanel.builtinKeyNavigation = true;
            scrollPanel.scrollWheelDirection = UIOrientation.Vertical;
            scrollPanel.eventVisibilityChanged += (_, isShow) => { if (isShow) scrollPanel.Reset(); };

            return scrollPanel;
        }
        protected override void Setup()
        {
            var tabBar = AutoTabstrip.AddTabstrip(this, 0, 0, width, height, out UITabContainer container, StandardComponentHeight);
            UIPanel settingsObject = UITabstrips.AddTextTab(tabBar, Translations.Translate(DriveCommon.TK_SETTINGS_TAB_SETTINGS), 0, out _);
            UIPanel settingsBikeObject = UITabstrips.AddTextTab(tabBar, Translations.Translate(DriveCommon.TK_SETTINGS_TAB_BIKE), 1, out _);
            UIPanel settingsBoatObject = UITabstrips.AddTextTab(tabBar, Translations.Translate(DriveCommon.TK_SETTINGS_TAB_BOAT), 2, out _);
            UIPanel settingsCarObject = UITabstrips.AddTextTab(tabBar, Translations.Translate(DriveCommon.TK_SETTINGS_TAB_CAR), 3, out _);
            UIPanel settingsHeliObject = UITabstrips.AddTextTab(tabBar, Translations.Translate(DriveCommon.TK_SETTINGS_TAB_HELI), 4, out _);
            UIPanel settingsPlaneObject = UITabstrips.AddTextTab(tabBar, Translations.Translate(DriveCommon.TK_SETTINGS_TAB_PLANE), 5, out _);
            UIPanel settingsTrailerObject = UITabstrips.AddTextTab(tabBar, Translations.Translate(DriveCommon.TK_SETTINGS_TAB_TRAILER), 6, out _);
            UIPanel settingsTrainObject = UITabstrips.AddTextTab(tabBar, Translations.Translate(DriveCommon.TK_SETTINGS_TAB_TRAIN), 7, out _);
            UIPanel keybindsObject = UITabstrips.AddTextTab(tabBar, Translations.Translate(DriveCommon.TK_SETTINGS_TAB_KEYS), 8, out _);
            tabBar.EqualizeTabs();

            Settings.Tabs.Configuration.Populate(CreateScrollable(container, settingsObject));
            Settings.Tabs.ConfigBike.Populate(CreateScrollable(container, settingsBikeObject));
            Settings.Tabs.ConfigBoat.Populate(CreateScrollable(container, settingsBoatObject));
            Settings.Tabs.ConfigCar.Populate(CreateScrollable(container, settingsCarObject));
            Settings.Tabs.ConfigHeli.Populate(CreateScrollable(container, settingsHeliObject));
            Settings.Tabs.ConfigPlane.Populate(CreateScrollable(container, settingsPlaneObject));
            Settings.Tabs.ConfigTrailer.Populate(CreateScrollable(container, settingsTrailerObject));
            Settings.Tabs.ConfigTrain.Populate(CreateScrollable(container, settingsTrainObject));
            Settings.Tabs.Keybinds.Populate(CreateScrollable(container, keybindsObject));

            // Force update to tab 0
            tabBar.selectedIndex = -1;
            tabBar.selectedIndex = 0;
        }
    }
}
