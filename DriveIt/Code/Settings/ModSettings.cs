using AlgernonCommons.Keybinding;
using AlgernonCommons.XML;
using ColossalFramework.IO;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;
using DriveIt.Utils;

namespace DriveIt.Settings
{
    [XmlRoot(DriveCommon.MOD_NAME)]
    public sealed class ModSettings : SettingsXMLBase
    {
        private const bool              BUILDINGCOLLISION = true;
        private const bool              VEHICLECOLLISION = false;
        private const float             MAXVELOCITY = 250f;
        private const float             ENGINEPOWER = 350f;
        private const float             BRAKINGFORCE = 50f;
        private const bool              AUTOTRANS = true;
        private const bool              BRAKINGABS = true;
        private const int               TCSLEVEL = 0;
        private const float             DOWNFORCE = 10.0f;
        private const float             DRIVEBIAS = 0.6f;
        private const float             BRAKEBIAS = 0.4f;
        private const float             GRIPCOEFFS = 1.1f;
        private const float             GRIPCOEFFK = 0.85f;
        private const float             SPRINGDAMP = 2.0f;
        private const float             SPRINGOFFSET = -0.1f;
        private const float             SPRINGSWAYBAR = 77.0f;
        private const float             MASSFACTOR = 85.0f;
        private const float             MASSCENTERHEIGHT = 0.05f;
        private const float             MASSCENTERBIAS = 0.5f;
        private static readonly Vector3 OFFSET = new Vector3(0f, 2f, 0f);
        private const float             CAMMOUSEROTATESENSITIVITY = 1f;
        private const float             CAMKEYROTATESENSITIVITY = 1f;
        private const float             CAMFIELDOFVIEW = 80f;
        private const float             CAMMAXPITCH = 70f;
        private const float             CAMSMOOTHING = 1f;
        private static readonly Vector2 MAINBUTTONPOS = new Vector2(0f, 0f);


        [XmlIgnore]
        private static readonly string SettingsFileName = Path.Combine(DataLocation.localApplicationData, DriveCommon.SETTINGS_PATH);

        internal static void Load() => XMLFileUtils.Load<ModSettings>(SettingsFileName);

        internal static void Save() => XMLFileUtils.Save<ModSettings>(SettingsFileName);

        internal static void RestoreDefaults()
        {
            BuildingCollision = BUILDINGCOLLISION;
            VehicleCollision = VEHICLECOLLISION;
            MaxVelocity = MAXVELOCITY;
            EnginePower = ENGINEPOWER;
            BrakingForce = BRAKINGFORCE;
            AutoTrans = AUTOTRANS;
            BrakingABS = BRAKINGABS;
            TCSLevel = TCSLEVEL;
            DownForce = DOWNFORCE;
            DriveBias = DRIVEBIAS;
            BrakeBias = BRAKEBIAS;
            GripCoeffS = GRIPCOEFFS;
            GripCoeffK = GRIPCOEFFK;
            SpringDamp = SPRINGDAMP;
            SpringOffset = SPRINGOFFSET;
            SpringSwayBar = SPRINGSWAYBAR;
            MassFactor = MASSFACTOR;
            MassCenterHeight = MASSCENTERHEIGHT;
            MassCenterBias = MASSCENTERBIAS;
            Offset = OFFSET;
            CamMouseRotateSensitivity = CAMMOUSEROTATESENSITIVITY;
            CamKeyRotateSensitivity = CAMKEYROTATESENSITIVITY;
            CamFieldOfView = CAMFIELDOFVIEW;
            CamMaxPitch = CAMMAXPITCH;
            CamSmoothing = CAMSMOOTHING;
            MainButtonPos = MAINBUTTONPOS;
        }

        [XmlElement("BuildingCollision")]
        public bool XMLBuildingCollision { get => BuildingCollision; set => BuildingCollision = value; }
        [XmlIgnore]
        internal static bool BuildingCollision = BUILDINGCOLLISION;

        [XmlElement("VehicleCollision")]
        public bool XMLVehicleCollision { get => VehicleCollision; set => VehicleCollision = value; }
        [XmlIgnore]
        internal static bool VehicleCollision = VEHICLECOLLISION;

        [XmlElement("MaxVelocity")]
        public float XMLMaxVelocity { get => MaxVelocity; set => MaxVelocity = value; }
        [XmlIgnore]
        internal static float MaxVelocity = MAXVELOCITY;

        [XmlElement("EnginePower")]
        public float XMLEnginePower { get => EnginePower; set => EnginePower = value; }
        [XmlIgnore]
        internal static float EnginePower = ENGINEPOWER;

        [XmlElement("BrakingForce")]
        public float XMLBrakingForce { get => BrakingForce; set => BrakingForce = value; }
        [XmlIgnore]
        internal static float BrakingForce = BRAKINGFORCE;

        [XmlElement("AutoTrans")]
        public bool XMLAutoTrans { get => AutoTrans; set => AutoTrans = value; }
        [XmlIgnore]
        internal static bool AutoTrans = AUTOTRANS;

        [XmlElement("BrakingABS")]
        public bool XMLBrakingABS { get => BrakingABS; set => BrakingABS = value; }
        [XmlIgnore]
        internal static bool BrakingABS = BRAKINGABS;

        [XmlElement("TCSLevel")]
        public int XMLTCSLevel { get => TCSLevel; set => TCSLevel = value; }
        [XmlIgnore]
        internal static int TCSLevel = TCSLEVEL;

        [XmlElement("DownForce")]
        public float XMLDownForce { get => DownForce; set => DownForce = value; }
        [XmlIgnore]
        internal static float DownForce = DOWNFORCE;

        [XmlElement("DriveBias")]
        public float XMLDriveBias { get => DriveBias; set => DriveBias = value; }
        [XmlIgnore]
        internal static float DriveBias = DRIVEBIAS;

        [XmlElement("BrakeBias")]
        public float XMLBrakeBias { get => BrakeBias; set => BrakeBias = value; }
        [XmlIgnore]
        internal static float BrakeBias = BRAKEBIAS;

        [XmlElement("GripCoeffS")]
        public float XMLGripCoeffS { get => GripCoeffS; set => GripCoeffS = value; }
        [XmlIgnore]
        internal static float GripCoeffS = GRIPCOEFFS;

        [XmlElement("GripCoeffK")]
        public float XMLGripCoeffK { get => GripCoeffK; set => GripCoeffK = value; }
        [XmlIgnore]
        internal static float GripCoeffK = GRIPCOEFFK;

        [XmlElement("SpringDamp")]
        public float XMLSpringDamp { get => SpringDamp; set => SpringDamp = value; }
        [XmlIgnore]
        internal static float SpringDamp = SPRINGDAMP;

        [XmlElement("SpringOffset")]
        public float XMLSpringOffset { get => SpringOffset; set => SpringOffset = value; }
        [XmlIgnore]
        internal static float SpringOffset = SPRINGOFFSET;

        [XmlElement("SpringSwayBar")]
        public float XMLSpringSwayBar { get => SpringSwayBar; set => SpringSwayBar = value; }
        [XmlIgnore]
        internal static float SpringSwayBar = SPRINGSWAYBAR;

        [XmlElement("MassFactor")]
        public float XMLMassFactor { get => MassFactor; set => MassFactor = value; }
        [XmlIgnore]
        internal static float MassFactor = MASSFACTOR;

        [XmlElement("MassCenterHeight")]
        public float XMLMassCenterHeight { get => MassCenterHeight; set => MassCenterHeight = value; }
        [XmlIgnore]
        internal static float MassCenterHeight = MASSCENTERHEIGHT;

        [XmlElement("MassCenterBias")]
        public float XMLMassCenterBias { get => MassCenterBias; set => MassCenterBias = value; }
        [XmlIgnore]
        internal static float MassCenterBias = MASSCENTERBIAS;

        [XmlElement("Offset")]
        public Vector3 XMLOffset { get => Offset; set => Offset = value; }
        [XmlIgnore]
        internal static Vector3 Offset = OFFSET;

        [XmlElement("CamMouseRotateSensitivity")]
        public float XMLCamMouseRotateSensitivity { get => CamMouseRotateSensitivity; set => CamMouseRotateSensitivity = value; }
        [XmlIgnore]
        internal static float CamMouseRotateSensitivity = CAMMOUSEROTATESENSITIVITY;

        [XmlElement("CamKeyRotateSensitivity")]
        public float XMLCamKeyRotateSensitivity { get => CamKeyRotateSensitivity; set => CamKeyRotateSensitivity = value; }
        [XmlIgnore]
        internal static float CamKeyRotateSensitivity = CAMKEYROTATESENSITIVITY;

        [XmlElement("CamFieldOfView")]
        public float XMLCamFieldOfView { get => CamFieldOfView; set => CamFieldOfView = value; }
        [XmlIgnore]
        internal static float CamFieldOfView = CAMFIELDOFVIEW;

        [XmlElement("CamMaxPitch")]
        public float XMLCamMaxPitchDeg { get => CamMaxPitch; set => CamMaxPitch = value; }
        [XmlIgnore]
        internal static float CamMaxPitch = CAMMAXPITCH;

        [XmlElement("CamSmoothing")]
        public float XMLCamSmoothing { get => CamSmoothing; set => CamSmoothing = value; }
        [XmlIgnore]
        internal static float CamSmoothing = CAMSMOOTHING;

        [XmlElement("KeyUUIToggle")]
        public Keybinding XMLKeyUUIToggle { get => Utils.UUISupport.UUIKey.Keybinding; set => Utils.UUISupport.UUIKey.Keybinding = value; }

        [XmlElement("KeyLightToggle")]
        public KeyOnlyBinding XMLKeyLightToggle { get => KeyLightToggle; set => KeyLightToggle = value; }
        [XmlIgnore]
        internal static KeyOnlyBinding KeyLightToggle = new KeyOnlyBinding(KeyCode.H);

        [XmlElement("KeySirenToggle")]
        public KeyOnlyBinding XMLKeySirenToggle { get => KeySirenToggle; set => KeySirenToggle = value; }
        [XmlIgnore]
        internal static KeyOnlyBinding KeySirenToggle = new KeyOnlyBinding(KeyCode.G);

        [XmlElement("KeyMoveForward")]
        public KeyOnlyBinding XMLKeyMoveForward { get => KeyMoveForward; set => KeyMoveForward = value; }
        [XmlIgnore]
        internal static KeyOnlyBinding KeyMoveForward = new KeyOnlyBinding(KeyCode.W);

        [XmlElement("KeyMoveBackward")]
        public KeyOnlyBinding XMLKeyMoveBackward { get => KeyMoveBackward; set => KeyMoveBackward = value; }
        [XmlIgnore]
        internal static KeyOnlyBinding KeyMoveBackward = new KeyOnlyBinding(KeyCode.S);

        [XmlElement("KeyMoveLeft")]
        public KeyOnlyBinding XMLKeyMoveLeft { get => KeyMoveLeft; set => KeyMoveLeft = value; }
        [XmlIgnore]
        internal static KeyOnlyBinding KeyMoveLeft = new KeyOnlyBinding(KeyCode.A);

        [XmlElement("KeyMoveRight")]
        public KeyOnlyBinding XMLKeyMoveRight { get => KeyMoveRight; set => KeyMoveRight = value; }
        [XmlIgnore]
        internal static KeyOnlyBinding KeyMoveRight = new KeyOnlyBinding(KeyCode.D);

        [XmlElement("KeyResetVehicle")]
        public KeyOnlyBinding XMLKeyResetVehicle { get => KeyResetVehicle; set => KeyResetVehicle = value; }
        [XmlIgnore]
        internal static KeyOnlyBinding KeyResetVehicle = new KeyOnlyBinding(KeyCode.R);

        [XmlElement("KeyGearUp")]
        public KeyOnlyBinding XMLKeyGearUp { get => KeyGearUp; set => KeyGearUp = value; }
        [XmlIgnore]
        internal static KeyOnlyBinding KeyGearUp = new KeyOnlyBinding(KeyCode.Quote);

        [XmlElement("KeyGearDown")]
        public KeyOnlyBinding XMLKeyGearDown { get => KeyGearDown; set => KeyGearDown = value; }
        [XmlIgnore]
        internal static KeyOnlyBinding KeyGearDown = new KeyOnlyBinding(KeyCode.Slash);

        [XmlElement("KeyCamCursorToggle")]
        public KeyOnlyBinding XMLKeyCamCursorToggle { get => KeyCamCursorToggle; set => KeyCamCursorToggle = value; }
        [XmlIgnore]
        internal static KeyOnlyBinding KeyCamCursorToggle = new KeyOnlyBinding(KeyCode.Tab);

        [XmlElement("KeyCamReset")]
        public KeyOnlyBinding XMLKeyCamReset { get => KeyCamReset; set => KeyCamReset = value; }
        [XmlIgnore]
        internal static KeyOnlyBinding KeyCamReset = new KeyOnlyBinding(KeyCode.Backslash);

        [XmlElement("KeyCamZoomIn")]
        public KeyOnlyBinding XMLKeyCamZoomIn { get => KeyCamZoomIn; set => KeyCamZoomIn = value; }
        [XmlIgnore]
        internal static KeyOnlyBinding KeyCamZoomIn = new KeyOnlyBinding(KeyCode.Equals);

        [XmlElement("KeyCamZoomOut")]
        public KeyOnlyBinding XMLKeyCamZoomOut { get => KeyCamZoomOut; set => KeyCamZoomOut = value; }
        [XmlIgnore]
        internal static KeyOnlyBinding KeyCamZoomOut = new KeyOnlyBinding(KeyCode.Minus);

        [XmlElement("KeyCamRotateLeft")]
        public KeyOnlyBinding XMLKeyCamRotateLeft { get => KeyCamRotateLeft; set => KeyCamRotateLeft = value; }
        [XmlIgnore]
        internal static KeyOnlyBinding KeyCamRotateLeft = new KeyOnlyBinding(KeyCode.LeftArrow);

        [XmlElement("KeyCamRotateRight")]
        public KeyOnlyBinding XMLKeyCamRotateRight { get => KeyCamRotateRight; set => KeyCamRotateRight = value; }
        [XmlIgnore]
        internal static KeyOnlyBinding KeyCamRotateRight = new KeyOnlyBinding(KeyCode.RightArrow);

        [XmlElement("KeyCamRotateUp")]
        public KeyOnlyBinding XMLKeyCamRotateUp { get => KeyCamRotateUp; set => KeyCamRotateUp = value; }
        [XmlIgnore]
        internal static KeyOnlyBinding KeyCamRotateUp = new KeyOnlyBinding(KeyCode.UpArrow);

        [XmlElement("KeyCamRotateDown")]
        public KeyOnlyBinding XMLKeyCamRotateDown { get => KeyCamRotateDown; set => KeyCamRotateDown = value; }
        [XmlIgnore]
        internal static KeyOnlyBinding KeyCamRotateDown = new KeyOnlyBinding(KeyCode.DownArrow);

        [XmlElement("MainButtonPos")]
        public Vector2 XMLMainButtonPos { get => MainButtonPos; set => MainButtonPos = value; }
        [XmlIgnore]
        internal static Vector2 MainButtonPos = MAINBUTTONPOS;
    }
}
