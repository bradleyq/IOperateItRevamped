using AlgernonCommons;
using AlgernonCommons.UI;
using ColossalFramework.UI;
using UnityEngine;

namespace DriveIt.Utils
{
    internal static class DriveCommon
    {
        public const string MOD_NAME        = "DriveIt";
        public const string MOD_HARMONY_ID  = "bradleyq.DriveIt";

        #region Algernon Translation String Keys
        public const string TK_LANGUAGE                    = "LANGUAGE";
        public const string TK_LANGUAGE_CHOICE             = "LANGUAGE_CHOICE";
        public const string TK_LANGUAGE_GAME               = "LANGUAGE_GAME";
        public const string TK_NOTE_CLOSE                  = "NOTE_CLOSE";
        public const string TK_DONTSHOWAGAIN                = "NOTE_DONTSHOWAGAIN";
        public const string TK_NO                          = "NO";
        public const string TK_YES                         = "YES";
        public const string TK_PRESS_ANY_KEY               = "PRESS_ANY_KEY";
        public const string TK_CONFLICT_DETECTED           = "CONFLICT_DETECTED";
        public const string TK_UNABLE_TO_OPERATE           = "UNABLE_TO_OPERATE";
        public const string TK_CONFLICTING_MODS            = "CONFLICTING_MODS";
        public const string TK_HARMONY_ERROR               = "HARMONY_ERROR";
        public const string TK_HARMONY_PROBLEM_CAUSES      = "HARMONY_PROBLEM_CAUSES";
        public const string TK_HARMONY_NOT_INSTALLED       = "HARMONY_NOT_INSTALLED";
        public const string TK_HARMONY_MOD_ERROR           = "HARMONY_MOD_ERROR";
        public const string TK_HARMONY_MOD_CONFLICT        = "HARMONY_MOD_CONFLICT";
        public const string TK_DETAIL_LOGGING              = "DETAIL_LOGGING";
        #endregion

        #region DriveIt Translation String Keys
        public const string TK_WHATSNEW_L1                 = "WHATSNEW_L1";
        public const string TK_WHATSNEW_L2                 = "WHATSNEW_L2";
        public const string TK_WHATSNEW_L3                 = "WHATSNEW_L3";
        public const string TK_MOD_DESCRIPTION             = "MOD_DESCRIPTION";
        public const string TK_SETTINGS_GROUP_GENERAL      = "SETTINGS_GROUP_GENERAL";
        public const string TK_SETTINGS_GROUP_VEHICLE      = "SETTINGS_GROUP_VEHICLE";
        public const string TK_SETTINGS_GROUP_CAMERA       = "SETTINGS_GROUP_CAMERA";
        public const string TK_SETTINGS_GROUP_GAME         = "SETTINGS_GROUP_GAME";
        public const string TK_SETTINGS_GROUP_KEYS         = "SETTINGS_GROUP_KEYS";
        public const string TK_SETTINGS_MAXVELOCITY        = "SETTINGS_MAXVELOCITY";
        public const string TK_SETTINGS_ENGINEPOWER        = "SETTINGS_ENGINEPOWER";
        public const string TK_SETTINGS_BRAKINGFORCE       = "SETTINGS_BRAKINGFORCE";
        public const string TK_SETTINGS_BUILDINGCOLLISION  = "SETTINGS_BUILDINGCOLLISION";
        public const string TK_SETTINGS_VEHICLECOLLISION   = "SETTINGS_VEHICLECOLLISION";
        public const string TK_SETTINGS_OFFSET_X           = "SETTINGS_OFFSET_X";
        public const string TK_SETTINGS_OFFSET_Y           = "SETTINGS_OFFSET_Y";
        public const string TK_SETTINGS_OFFSET_Z           = "SETTINGS_OFFSET_Z";
        public const string TK_SETTINGS_KEYUUITOGGLE       = "SETTINGS_KEYUUITOGGLE";
        public const string TK_SETTINGS_KEYLIGHTTOGGLE     = "SETTINGS_KEYLIGHTTOGGLE";
        public const string TK_SETTINGS_KEYSIRENTOGGLE     = "SETTINGS_KEYSIRENTOGGLE";
        public const string TK_MAINPANELBTN_TOOLTIP        = "MAINPANELBTN_TOOLTIP";
        public const string TK_DRIVEBTN_TOOLTIP            = "DRIVEBTN_TOOLTIP";
        public const string TK_SPAWNBTN_TEXT               = "SPAWNBTN_TEXT";
        public const string TK_ROAD_SELECT                 = "ROAD_SELECT";
        #endregion

        public const string SETTINGS_PATH               = "DriveIt.xml";

        public const string TEX_BUTTON_ICON             = "Textures/DriveItIcon";
        public const string TEX_BUTTON_ICON_FULL_PATH   = "Resources/" + TEX_BUTTON_ICON + ".png";

        public const string TEX_BUTTON_BG               = "OptionBase";
        public const string TEX_BUTTON_BG_PRESSED       = "OptionBasePressed";
        public const string TEX_BUTTON_HOVER            = "OptionBaseHovered";
        public const string TEX_BUTTON_DISABLE          = "OptionBaseDisabled";

        public const int TEX_COUNT  = 5;
        public const int TEX_SIZE   = 1024;

        public static UITextureAtlas driveCommonAtlas;

        private static bool bInit = false;

        public static void Initialize()
        {
            if (!bInit)
            {
                bInit = true;

                int index = 0;
                Texture2D[] textures = new Texture2D[TEX_COUNT];
                string[] names = new string[TEX_COUNT];

                // Loaded custom textures
                names[index] = TEX_BUTTON_ICON;
                textures[index++] = DriveCommonLoadTexture(TEX_BUTTON_ICON);

                // Existing core game textures
                names[index] = TEX_BUTTON_BG;
                textures[index++] = DriveCommonRipTexture(TEX_BUTTON_BG);

                names[index] = TEX_BUTTON_BG_PRESSED;
                textures[index++] = DriveCommonRipTexture(TEX_BUTTON_BG_PRESSED);

                names[index] = TEX_BUTTON_HOVER;
                textures[index++] = DriveCommonRipTexture(TEX_BUTTON_HOVER);

                names[index] = TEX_BUTTON_DISABLE;
                textures[index++] = DriveCommonRipTexture(TEX_BUTTON_DISABLE);

                driveCommonAtlas = UITextures.CreateSpriteAtlas(MOD_HARMONY_ID + "_Atlas", TEX_SIZE, textures, names);
            }
        }

        // Hack to use Algernon load texture
        private static Texture2D DriveCommonLoadTexture(string name)
        {
            return UITextures.LoadCursor(name + ".png").m_texture;
        }

        // Rip texture from the default global atlas
        private static Texture2D DriveCommonRipTexture(string name)
        {
            foreach (UITextureAtlas.SpriteInfo si in UIView.GetAView().defaultAtlas.sprites)
            {
                if (si.name == name)
                {
                    return si.texture;
                }
            }
            return null;
        }
    }
}
