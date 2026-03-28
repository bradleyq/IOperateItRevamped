using AlgernonCommons;
using AlgernonCommons.UI;
using ColossalFramework;
using ColossalFramework.UI;
using System;
using System.IO;
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

        #region DriveIt General Constants
        public const float ROAD_WALL_HEIGHT = 0.75f;    // per tick height delta before the slope is considered a wall
        public const float ROAD_THICKNESS = 1.5f;       // height delta below road surface to be considered still part of the road

        #endregion


        public const string SETTINGS_PATH               = "DriveIt.xml";

        public const string TEX_BUTTON_ICON             = "Textures/DriveItIcon";
        public const string TEX_BUTTON_ICON_FULL_PATH   = "Resources/" + TEX_BUTTON_ICON + ".png";

        public const string TEX_BUTTON_BG               = "OptionBase";
        public const string TEX_BUTTON_BG_PRESSED       = "OptionBasePressed";
        public const string TEX_BUTTON_HOVER            = "OptionBaseHovered";
        public const string TEX_BUTTON_DISABLE          = "OptionBaseDisabled";

        public const int    TEX_ATLAS_COUNT  = 5;
        public const int    TEX_ATLAS_SIZE   = 1024;

        public const string TEX_GAUGE_CLUSTER           = "Textures/GaugeCluster";

        public const string SND_TIRE_SQUEAL             = "Sounds/TireSqueal";
        public const string SND_TIRE_SQUEAL_NAME        = "Tire Squeal";

        public const string SND_TIRE_GRAVEL             = "Sounds/TireGravel";
        public const string SND_TIRE_GRAVEL_NAME        = "Tire Gravel";

        public const float  SND_RANGE                   = 200.0f;

        private static EffectsWrapper s_effectsWrapper;

        public static UITextureAtlas s_driveCommonAtlas;
        public static Texture2D s_driveTextureGaugeCluster;
        public static SoundEffect s_driveSoundTireSqueal;
        public static SoundEffect s_driveSoundTireGravel;

        private static bool bInit = false;

        public static void Initialize()
        {
            if (!bInit)
            {
                bInit = true;

                s_effectsWrapper = Singleton<EffectManager>.instance.m_EffectsWrapper;

                int index = 0;
                Texture2D[] textures = new Texture2D[TEX_ATLAS_COUNT];
                string[] names = new string[TEX_ATLAS_COUNT];

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

                s_driveCommonAtlas = UITextures.CreateSpriteAtlas(MOD_HARMONY_ID + "_Atlas", TEX_ATLAS_SIZE, textures, names);

                s_driveTextureGaugeCluster = DriveCommonLoadTexture(TEX_GAUGE_CLUSTER);

                AudioClip clip;
                
                clip = DriveCommonLoadAudioClip(SND_TIRE_SQUEAL);
                s_driveSoundTireSqueal = DriveCommonSoundEffect(clip, 2.0f, true, true);

                clip = DriveCommonLoadAudioClip(SND_TIRE_GRAVEL);
                s_driveSoundTireGravel = DriveCommonSoundEffect(clip, 2.0f, true, true);
            }
        }

        public static void DrawRingSegment(Vector2 center, float outerR, float innerR, float startAngle, float endAngle, float fill, Color color, Material mat)
        {
            int segments = 40;
            fill = Mathf.Clamp01(fill);
            center.y = Screen.height - center.y;

            float totalAngle = endAngle - startAngle;


            GL.PushMatrix();
            GL.LoadPixelMatrix();

            mat.SetPass(0);

            GL.Begin(GL.TRIANGLES);
            GL.Color(color);

            int segDraw = (int)Mathf.Ceil(segments * fill);

            for (int i = 0; i < segDraw; i++)
            {
                float t0 = i / (float)segments;
                float t1 = (i + 1) / (float)segments;

                float a0 = startAngle + totalAngle * t0;
                float a1 = startAngle + totalAngle * Mathf.Min(t1, fill);

                Vector2 o0 = center + new Vector2(Mathf.Cos(a0), Mathf.Sin(a0)) * outerR;
                Vector2 o1 = center + new Vector2(Mathf.Cos(a1), Mathf.Sin(a1)) * outerR;

                Vector2 i0 = center + new Vector2(Mathf.Cos(a0), Mathf.Sin(a0)) * innerR;
                Vector2 i1 = center + new Vector2(Mathf.Cos(a1), Mathf.Sin(a1)) * innerR;

                // Two triangles per segment
                GL.Vertex3(i0.x, i0.y, 0);
                GL.Vertex3(o0.x, o0.y, 0);
                GL.Vertex3(o1.x, o1.y, 0);

                GL.Vertex3(i0.x, i0.y, 0);
                GL.Vertex3(o1.x, o1.y, 0);
                GL.Vertex3(i1.x, i1.y, 0);
            }

            GL.End();
            GL.PopMatrix();
        }

        public static void FormatDriveButton(UIButton button)
        {
            button.atlas = s_driveCommonAtlas;
            button.size = new Vector2(40f, 40f);
            button.scaleFactor = .8f;
            button.normalBgSprite = TEX_BUTTON_BG;
            button.pressedBgSprite = TEX_BUTTON_BG_PRESSED;
            button.hoveredBgSprite = TEX_BUTTON_HOVER;
            button.disabledBgSprite = TEX_BUTTON_DISABLE;
            button.normalFgSprite = TEX_BUTTON_ICON;
            button.textColor = new Color32(255, 255, 255, 255);
            button.disabledTextColor = new Color32(7, 7, 7, 255);
            button.hoveredTextColor = new Color32(255, 255, 255, 255);
            button.focusedTextColor = new Color32(255, 255, 255, 255);
            button.pressedTextColor = new Color32(30, 30, 44, 255);
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
        private static AudioClip DriveCommonLoadAudioClip(string name)
        {
            string path = Path.Combine(AssemblyUtils.AssemblyPath, "Resources");
            path = Path.Combine(path, name + ".ogg");
            WWW www = new WWW(new Uri(path).AbsoluteUri);
            return www.GetAudioClip(true, false);
        }

        private static SoundEffect DriveCommonSoundEffect(AudioClip clip, float volume, bool loop, bool random)
        {

            ICities.UserAudioSettings settings = new ICities.UserAudioSettings();
            settings.volume = volume;
            settings.pitch = 1.0f;
            settings.loop = loop;
            settings.is3D = false;
            settings.range = SND_RANGE;
            settings.fadeLength = 0.1f;
            settings.randomTime = random;

            SoundEffect clipEffect = s_effectsWrapper.CreateSoundEffect(SND_TIRE_SQUEAL_NAME, clip, ref settings) as SoundEffect;

            return clipEffect;
        }
    }
}
