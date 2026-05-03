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
        private const bool              AUTOTRANS = true;
        private const bool              BRAKINGABS = true;
        private const int               TCSLEVEL = 0;
        private const float             GRIPCOEFFS = 1.15f;
        private const float             GRIPCOEFFK = 0.8f;
        private const float             GRAVITY = 10.0f;

        private const float             ENGINEPOWER = 350f;
        private const float             BRAKINGFORCE = 50f;
        private const float             DOWNFORCE = 10.0f;
        private const float             DRIVEBIAS = 0.5f;
        private const float             BRAKEBIAS = 0.5f;
        private const float             SPRINGDAMP = 2.25f;
        private const float             SPRINGOFFSET = -0.2f;
        private const float             SPRINGSWAYBAR = 77.0f;
        private const float             MASSCENTERHEIGHT = 0.1f;
        private const float             MASSCENTERBIAS = 0.55f;

        private const float             BIKEENGINEPOWER = 150f;
        private const float             BIKEBRAKINGFORCE = 8f;
        private const float             BIKEDOWNFORCE = 15.0f;
        private const float             BIKEDRIVEBIAS = 0.6f;
        private const float             BIKEBRAKEBIAS = 0.0f;
        private const float             BIKESPRINGDAMP = 4.0f;
        private const float             BIKESPRINGOFFSET = -0.05f;
        private const float             BIKEMASSCENTERHEIGHT = 0.05f;
        private const float             BIKEMASSCENTERBIAS = 0.5f;

        private const float             BOATENGINEPOWER = 25000f;
        private const float             BOATBRAKINGFORCE = 200f;
        private const float             BOATDOWNFORCE = 5.0f;
        private const float             BOATDRIVEBIAS = 0.25f;
        private const float             BOATBRAKEBIAS = 0.25f;
        private const float             BOATSPRINGOFFSET = -0.6f;
        private const float             BOATMASSCENTERHEIGHT = 0.1f;
        private const float             BOATMASSCENTERBIAS = 0.5f;

        private const float             CARENGINEPOWER = 350f;
        private const float             CARBRAKINGFORCE = 50f;
        private const float             CARDOWNFORCE = 15.0f;
        private const float             CARDRIVEBIAS = 0.65f;
        private const float             CARBRAKEBIAS = 0.65f;
        private const float             CARSPRINGDAMP = 3.3f;
        private const float             CARSPRINGOFFSET = -0.1f;
        private const float             CARSPRINGSWAYBAR = 97.0f;
        private const float             CARMASSCENTERHEIGHT = 0.05f;
        private const float             CARMASSCENTERBIAS = 0.55f;

        private const float             HELIENGINEPOWER = 350f;
        private const float             HELISPRINGDAMP = 2.0f;
        private const float             HELISPRINGOFFSET = -0.1f;
        private const float             HELIMASSCENTERHEIGHT = 0.1f;
        private const float             HELIMASSCENTERBIAS = 0.5f;

        private const float             PLANEENGINEPOWER = 35000f;
        private const float             PLANEBRAKINGFORCE = 300f;
        private const float             PLANEDOWNFORCE = -0.07f;
        private const float             PLANEBRAKEBIAS = 0.4f;
        private const float             PLANESPRINGDAMP = 4.0f;
        private const float             PLANESPRINGOFFSET = -1.5f;
        private const float             PLANEMASSCENTERHEIGHT = 0.1f;
        private const float             PLANEMASSCENTERBIAS = 0.5f;

        private const float             TRAILERBRAKINGFORCE = 75f;
        private const float             TRAILERDOWNFORCE = 0.0f;
        private const float             TRAILERSPRINGDAMP = 2.0f;
        private const float             TRAILERSPRINGOFFSET = -0.1f;
        private const float             TRAILERSPRINGSWAYBAR = 147.0f;
        private const float             TRAILERMASSCENTERHEIGHT = 0.05f;
        private const float             TRAILERMASSCENTERBIAS = 0.5f;

        private const float             TRAINENGINEPOWER = 2000f;
        private const float             TRAINBRAKINGFORCE = 100f;
        private const float             TRAINDOWNFORCE = 5.0f;
        private const float             TRAINDRIVEBIAS = 0.5f;
        private const float             TRAINBRAKEBIAS = 0.5f;
        private const float             TRAINSPRINGDAMP = 4.0f;
        private const float             TRAINSPRINGOFFSET = -0.2f;
        private const float             TRAINSPRINGSWAYBAR = 125.0f;
        private const float             TRAINMASSCENTERHEIGHT = 0.1f;
        private const float             TRAINMASSCENTERBIAS = 0.5f;

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
            #region Default Values
            BuildingCollision = BUILDINGCOLLISION;
            VehicleCollision = VEHICLECOLLISION;
            Gravity = GRAVITY;
            MaxVelocity = MAXVELOCITY;
            AutoTrans = AUTOTRANS;
            BrakingABS = BRAKINGABS;
            TCSLevel = TCSLEVEL;
            GripCoeffS = GRIPCOEFFS;
            GripCoeffK = GRIPCOEFFK;

            EnginePower = ENGINEPOWER;
            BrakingForce = BRAKINGFORCE;
            DownForce = DOWNFORCE;
            DriveBias = DRIVEBIAS;
            BrakeBias = BRAKEBIAS;
            SpringDamp = SPRINGDAMP;
            SpringOffset = SPRINGOFFSET;
            SpringSwayBar = SPRINGSWAYBAR;
            MassCenterHeight = MASSCENTERHEIGHT;
            MassCenterBias = MASSCENTERBIAS;

            BikeEnginePower = BIKEENGINEPOWER;
            BikeBrakingForce = BIKEBRAKINGFORCE;
            BikeDownForce = BIKEDOWNFORCE;
            BikeDriveBias = BIKEDRIVEBIAS;
            BikeBrakeBias = BIKEBRAKEBIAS;
            BikeSpringDamp = BIKESPRINGDAMP;
            BikeSpringOffset = BIKESPRINGOFFSET;
            BikeMassCenterHeight = BIKEMASSCENTERHEIGHT;
            BikeMassCenterBias = BIKEMASSCENTERBIAS;

            BoatEnginePower = BOATENGINEPOWER;
            BoatBrakingForce = BOATBRAKINGFORCE;
            BoatDownForce = BOATDOWNFORCE;
            BoatDriveBias = BOATDRIVEBIAS;
            BoatBrakeBias = BOATBRAKEBIAS;
            BoatSpringOffset = BOATSPRINGOFFSET;
            BoatMassCenterHeight = BOATMASSCENTERHEIGHT;
            BoatMassCenterBias = BOATMASSCENTERBIAS;

            CarEnginePower = CARENGINEPOWER;
            CarBrakingForce = CARBRAKINGFORCE;
            CarDownForce = CARDOWNFORCE;
            CarDriveBias = CARDRIVEBIAS;
            CarBrakeBias = CARBRAKEBIAS;
            CarSpringDamp = CARSPRINGDAMP;
            CarSpringOffset = CARSPRINGOFFSET;
            CarSpringSwayBar = CARSPRINGSWAYBAR;
            CarMassCenterHeight = CARMASSCENTERHEIGHT;
            CarMassCenterBias = CARMASSCENTERBIAS;

            HeliEnginePower = HELIENGINEPOWER;
            HeliSpringDamp = HELISPRINGDAMP;
            HeliSpringOffset = HELISPRINGOFFSET;
            HeliMassCenterHeight = HELIMASSCENTERHEIGHT;
            HeliMassCenterBias = HELIMASSCENTERBIAS;

            PlaneEnginePower = PLANEENGINEPOWER;
            PlaneBrakingForce = PLANEBRAKINGFORCE;
            PlaneDownForce = PLANEDOWNFORCE;
            PlaneBrakeBias = PLANEBRAKEBIAS;
            PlaneSpringDamp = PLANESPRINGDAMP;
            PlaneSpringOffset = PLANESPRINGOFFSET;
            PlaneMassCenterHeight = PLANEMASSCENTERHEIGHT;
            PlaneMassCenterBias = PLANEMASSCENTERBIAS;

            TrailerBrakingForce = TRAILERBRAKINGFORCE;
            TrailerDownForce = TRAILERDOWNFORCE;
            TrailerSpringDamp = TRAILERSPRINGDAMP;
            TrailerSpringOffset = TRAILERSPRINGOFFSET;
            TrailerSpringSwayBar = TRAILERSPRINGSWAYBAR;
            TrailerMassCenterHeight = TRAILERMASSCENTERHEIGHT;
            TrailerMassCenterBias = TRAILERMASSCENTERBIAS;

            TrainEnginePower = TRAINENGINEPOWER;
            TrainBrakingForce = TRAINBRAKINGFORCE;
            TrainDownForce = TRAINDOWNFORCE;
            TrainDriveBias = TRAINDRIVEBIAS;
            TrainBrakeBias = TRAINBRAKEBIAS;
            TrainSpringDamp = TRAINSPRINGDAMP;
            TrainSpringOffset = TRAINSPRINGOFFSET;
            TrainSpringSwayBar = TRAINSPRINGSWAYBAR;
            TrainMassCenterHeight = TRAINMASSCENTERHEIGHT;
            TrainMassCenterBias = TRAINMASSCENTERBIAS;

            Offset = OFFSET;
            CamMouseRotateSensitivity = CAMMOUSEROTATESENSITIVITY;
            CamKeyRotateSensitivity = CAMKEYROTATESENSITIVITY;
            CamFieldOfView = CAMFIELDOFVIEW;
            CamMaxPitch = CAMMAXPITCH;
            CamSmoothing = CAMSMOOTHING;
            MainButtonPos = MAINBUTTONPOS;
            #endregion
        }

        #region General Settings
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

        [XmlElement("GripCoeffS")]
        public float XMLGripCoeffS { get => GripCoeffS; set => GripCoeffS = value; }
        [XmlIgnore]
        internal static float GripCoeffS = GRIPCOEFFS;

        [XmlElement("GripCoeffK")]
        public float XMLGripCoeffK { get => GripCoeffK; set => GripCoeffK = value; }
        [XmlIgnore]
        internal static float GripCoeffK = GRIPCOEFFK;
        #endregion

        #region Generic Vehicle Settings
        [XmlElement("Gravity")]
        public float XMLGravity { get => Gravity; set => Gravity = value; }
        [XmlIgnore]
        internal static float Gravity = GRAVITY;

        [XmlElement("EnginePower")]
        public float XMLEnginePower { get => EnginePower; set => EnginePower = value; }
        [XmlIgnore]
        internal static float EnginePower = ENGINEPOWER;

        [XmlElement("BrakingForce")]
        public float XMLBrakingForce { get => BrakingForce; set => BrakingForce = value; }
        [XmlIgnore]
        internal static float BrakingForce = BRAKINGFORCE;

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

        [XmlElement("MassCenterHeight")]
        public float XMLMassCenterHeight { get => MassCenterHeight; set => MassCenterHeight = value; }
        [XmlIgnore]
        internal static float MassCenterHeight = MASSCENTERHEIGHT;

        [XmlElement("MassCenterBias")]
        public float XMLMassCenterBias { get => MassCenterBias; set => MassCenterBias = value; }
        [XmlIgnore]
        internal static float MassCenterBias = MASSCENTERBIAS;
        #endregion

        #region Bike Vehicle Settings
        [XmlElement("BikeEnginePower")]
        public float XMLBikeEnginePower { get => BikeEnginePower; set => BikeEnginePower = value; }
        [XmlIgnore]
        internal static float BikeEnginePower = BIKEENGINEPOWER;

        [XmlElement("BikeBrakingForce")]
        public float XMLBikeBrakingForce { get => BikeBrakingForce; set => BikeBrakingForce = value; }
        [XmlIgnore]
        internal static float BikeBrakingForce = BIKEBRAKINGFORCE;

        [XmlElement("BikeDownForce")]
        public float XMLBikeDownForce { get => BikeDownForce; set => BikeDownForce = value; }
        [XmlIgnore]
        internal static float BikeDownForce = BIKEDOWNFORCE;

        [XmlElement("BikeDriveBias")]
        public float XMLBikeDriveBias { get => BikeDriveBias; set => BikeDriveBias = value; }
        [XmlIgnore]
        internal static float BikeDriveBias = BIKEDRIVEBIAS;

        [XmlElement("BikeBrakeBias")]
        public float XMLBikeBrakeBias { get => BikeBrakeBias; set => BikeBrakeBias = value; }
        [XmlIgnore]
        internal static float BikeBrakeBias = BIKEBRAKEBIAS;

        [XmlElement("BikeSpringDamp")]
        public float XMLBikeSpringDamp { get => BikeSpringDamp; set => BikeSpringDamp = value; }
        [XmlIgnore]
        internal static float BikeSpringDamp = BIKESPRINGDAMP;

        [XmlElement("BikeSpringOffset")]
        public float XMLBikeSpringOffset { get => BikeSpringOffset; set => BikeSpringOffset = value; }
        [XmlIgnore]
        internal static float BikeSpringOffset = BIKESPRINGOFFSET;

        [XmlElement("BikeMassCenterHeight")]
        public float XMLBikeMassCenterHeight { get => BikeMassCenterHeight; set => BikeMassCenterHeight = value; }
        [XmlIgnore]
        internal static float BikeMassCenterHeight = BIKEMASSCENTERHEIGHT;

        [XmlElement("BikeMassCenterBias")]
        public float XMLBikeMassCenterBias { get => BikeMassCenterBias; set => BikeMassCenterBias = value; }
        [XmlIgnore]
        internal static float BikeMassCenterBias = BIKEMASSCENTERBIAS;
        #endregion

        #region Boat Vehicle Settings
        [XmlElement("BoatEnginePower")]
        public float XMLBoatEnginePower { get => BoatEnginePower; set => BoatEnginePower = value; }
        [XmlIgnore]
        internal static float BoatEnginePower = BOATENGINEPOWER;

        [XmlElement("BoatBrakingForce")]
        public float XMLBoatBrakingForce { get => BoatBrakingForce; set => BoatBrakingForce = value; }
        [XmlIgnore]
        internal static float BoatBrakingForce = BOATBRAKINGFORCE;

        [XmlElement("BoatDownForce")]
        public float XMLBoatDownForce { get => BoatDownForce; set => BoatDownForce = value; }
        [XmlIgnore]
        internal static float BoatDownForce = BOATDOWNFORCE;

        [XmlElement("BoatDriveBias")]
        public float XMLBoatDriveBias { get => BoatDriveBias; set => BoatDriveBias = value; }
        [XmlIgnore]
        internal static float BoatDriveBias = BOATDRIVEBIAS;

        [XmlElement("BoatBrakeBias")]
        public float XMLBoatBrakeBias { get => BoatBrakeBias; set => BoatBrakeBias = value; }
        [XmlIgnore]
        internal static float BoatBrakeBias = BOATBRAKEBIAS;

        [XmlElement("BoatSpringOffset")]
        public float XMLBoatSpringOffset { get => BoatSpringOffset; set => BoatSpringOffset = value; }
        [XmlIgnore]
        internal static float BoatSpringOffset = BOATSPRINGOFFSET;

        [XmlElement("BoatMassCenterHeight")]
        public float XMLBoatMassCenterHeight { get => BoatMassCenterHeight; set => BoatMassCenterHeight = value; }
        [XmlIgnore]
        internal static float BoatMassCenterHeight = BOATMASSCENTERHEIGHT;

        [XmlElement("BoatMassCenterBias")]
        public float XMLBoatMassCenterBias { get => BoatMassCenterBias; set => BoatMassCenterBias = value; }
        [XmlIgnore]
        internal static float BoatMassCenterBias = BOATMASSCENTERBIAS;
        #endregion

        #region Car Vehicle Settings
        [XmlElement("CarEnginePower")]
        public float XMLCarEnginePower { get => CarEnginePower; set => CarEnginePower = value; }
        [XmlIgnore]
        internal static float CarEnginePower = CARENGINEPOWER;

        [XmlElement("CarBrakingForce")]
        public float XMLCarBrakingForce { get => CarBrakingForce; set => CarBrakingForce = value; }
        [XmlIgnore]
        internal static float CarBrakingForce = CARBRAKINGFORCE;

        [XmlElement("CarDownForce")]
        public float XMLCarDownForce { get => CarDownForce; set => CarDownForce = value; }
        [XmlIgnore]
        internal static float CarDownForce = CARDOWNFORCE;

        [XmlElement("CarDriveBias")]
        public float XMLCarDriveBias { get => CarDriveBias; set => CarDriveBias = value; }
        [XmlIgnore]
        internal static float CarDriveBias = CARDRIVEBIAS;

        [XmlElement("CarBrakeBias")]
        public float XMLCarBrakeBias { get => CarBrakeBias; set => CarBrakeBias = value; }
        [XmlIgnore]
        internal static float CarBrakeBias = CARBRAKEBIAS;

        [XmlElement("CarSpringDamp")]
        public float XMLCarSpringDamp { get => CarSpringDamp; set => CarSpringDamp = value; }
        [XmlIgnore]
        internal static float CarSpringDamp = CARSPRINGDAMP;

        [XmlElement("CarSpringOffset")]
        public float XMLCarSpringOffset { get => CarSpringOffset; set => CarSpringOffset = value; }
        [XmlIgnore]
        internal static float CarSpringOffset = CARSPRINGOFFSET;

        [XmlElement("CarSpringSwayBar")]
        public float XMLCarSpringSwayBar { get => CarSpringSwayBar; set => CarSpringSwayBar = value; }
        [XmlIgnore]
        internal static float CarSpringSwayBar = CARSPRINGSWAYBAR;

        [XmlElement("CarMassCenterHeight")]
        public float XMLCarMassCenterHeight { get => CarMassCenterHeight; set => CarMassCenterHeight = value; }
        [XmlIgnore]
        internal static float CarMassCenterHeight = CARMASSCENTERHEIGHT;

        [XmlElement("CarMassCenterBias")]
        public float XMLCarMassCenterBias { get => CarMassCenterBias; set => CarMassCenterBias = value; }
        [XmlIgnore]
        internal static float CarMassCenterBias = CARMASSCENTERBIAS;
        #endregion

        #region Heli Vehicle Settings
        [XmlElement("HeliEnginePower")]
        public float XMLHeliEnginePower { get => HeliEnginePower; set => HeliEnginePower = value; }
        [XmlIgnore]
        internal static float HeliEnginePower = HELIENGINEPOWER;

        [XmlElement("HeliSpringDamp")]
        public float XMLHeliSpringDamp { get => HeliSpringDamp; set => HeliSpringDamp = value; }
        [XmlIgnore]
        internal static float HeliSpringDamp = HELISPRINGDAMP;

        [XmlElement("HeliSpringOffset")]
        public float XMLHeliSpringOffset { get => HeliSpringOffset; set => HeliSpringOffset = value; }
        [XmlIgnore]
        internal static float HeliSpringOffset = HELISPRINGOFFSET;

        [XmlElement("HeliMassCenterHeight")]
        public float XMLHeliMassCenterHeight { get => HeliMassCenterHeight; set => HeliMassCenterHeight = value; }
        [XmlIgnore]
        internal static float HeliMassCenterHeight = HELIMASSCENTERHEIGHT;

        [XmlElement("HeliMassCenterBias")]
        public float XMLHeliMassCenterBias { get => HeliMassCenterBias; set => HeliMassCenterBias = value; }
        [XmlIgnore]
        internal static float HeliMassCenterBias = HELIMASSCENTERBIAS;
        #endregion

        #region Plane Vehicle Settings
        [XmlElement("PlaneEnginePower")]
        public float XMLPlaneEnginePower { get => PlaneEnginePower; set => PlaneEnginePower = value; }
        [XmlIgnore]
        internal static float PlaneEnginePower = PLANEENGINEPOWER;

        [XmlElement("PlaneBrakingForce")]
        public float XMLPlaneBrakingForce { get => PlaneBrakingForce; set => PlaneBrakingForce = value; }
        [XmlIgnore]
        internal static float PlaneBrakingForce = PLANEBRAKINGFORCE;

        [XmlElement("PlaneDownForce")]
        public float XMLPlaneDownForce { get => PlaneDownForce; set => PlaneDownForce = value; }
        [XmlIgnore]
        internal static float PlaneDownForce = PLANEDOWNFORCE;

        [XmlElement("PlaneBrakeBias")]
        public float XMLPlaneBrakeBias { get => PlaneBrakeBias; set => PlaneBrakeBias = value; }
        [XmlIgnore]
        internal static float PlaneBrakeBias = PLANEBRAKEBIAS;

        [XmlElement("PlaneSpringDamp")]
        public float XMLPlaneSpringDamp { get => PlaneSpringDamp; set => PlaneSpringDamp = value; }
        [XmlIgnore]
        internal static float PlaneSpringDamp = PLANESPRINGDAMP;

        [XmlElement("PlaneSpringOffset")]
        public float XMLPlaneSpringOffset { get => PlaneSpringOffset; set => PlaneSpringOffset = value; }
        [XmlIgnore]
        internal static float PlaneSpringOffset = PLANESPRINGOFFSET;

        [XmlElement("PlaneMassCenterHeight")]
        public float XMLPlaneMassCenterHeight { get => PlaneMassCenterHeight; set => PlaneMassCenterHeight = value; }
        [XmlIgnore]
        internal static float PlaneMassCenterHeight = PLANEMASSCENTERHEIGHT;

        [XmlElement("PlaneMassCenterBias")]
        public float XMLPlaneMassCenterBias { get => PlaneMassCenterBias; set => PlaneMassCenterBias = value; }
        [XmlIgnore]
        internal static float PlaneMassCenterBias = PLANEMASSCENTERBIAS;
        #endregion

        #region Trailer Vehicle Settings
        [XmlElement("TrailerBrakingForce")]
        public float XMLTrailerBrakingForce { get => TrailerBrakingForce; set => TrailerBrakingForce = value; }
        [XmlIgnore]
        internal static float TrailerBrakingForce = TRAILERBRAKINGFORCE;

        [XmlElement("TrailerDownForce")]
        public float XMLTrailerDownForce { get => TrailerDownForce; set => TrailerDownForce = value; }
        [XmlIgnore]
        internal static float TrailerDownForce = TRAILERDOWNFORCE;

        [XmlElement("TrailerSpringDamp")]
        public float XMLTrailerSpringDamp { get => TrailerSpringDamp; set => TrailerSpringDamp = value; }
        [XmlIgnore]
        internal static float TrailerSpringDamp = TRAILERSPRINGDAMP;

        [XmlElement("TrailerSpringOffset")]
        public float XMLTrailerSpringOffset { get => TrailerSpringOffset; set => TrailerSpringOffset = value; }
        [XmlIgnore]
        internal static float TrailerSpringOffset = TRAILERSPRINGOFFSET;

        [XmlElement("TrailerSpringSwayBar")]
        public float XMLTrailerSpringSwayBar { get => TrailerSpringSwayBar; set => TrailerSpringSwayBar = value; }
        [XmlIgnore]
        internal static float TrailerSpringSwayBar = TRAILERSPRINGSWAYBAR;

        [XmlElement("TrailerMassCenterHeight")]
        public float XMLTrailerMassCenterHeight { get => TrailerMassCenterHeight; set => TrailerMassCenterHeight = value; }
        [XmlIgnore]
        internal static float TrailerMassCenterHeight = TRAILERMASSCENTERHEIGHT;

        [XmlElement("TrailerMassCenterBias")]
        public float XMLTrailerMassCenterBias { get => TrailerMassCenterBias; set => TrailerMassCenterBias = value; }
        [XmlIgnore]
        internal static float TrailerMassCenterBias = TRAILERMASSCENTERBIAS;
        #endregion

        #region Train Vehicle Settings
        [XmlElement("TrainEnginePower")]
        public float XMLTrainEnginePower { get => TrainEnginePower; set => TrainEnginePower = value; }
        [XmlIgnore]
        internal static float TrainEnginePower = TRAINENGINEPOWER;

        [XmlElement("TrainBrakingForce")]
        public float XMLTrainBrakingForce { get => TrainBrakingForce; set => TrainBrakingForce = value; }
        [XmlIgnore]
        internal static float TrainBrakingForce = TRAINBRAKINGFORCE;

        [XmlElement("TrainDownForce")]
        public float XMLTrainDownForce { get => TrainDownForce; set => TrainDownForce = value; }
        [XmlIgnore]
        internal static float TrainDownForce = TRAINDOWNFORCE;

        [XmlElement("TrainDriveBias")]
        public float XMLTrainDriveBias { get => TrainDriveBias; set => TrainDriveBias = value; }
        [XmlIgnore]
        internal static float TrainDriveBias = TRAINDRIVEBIAS;

        [XmlElement("TrainBrakeBias")]
        public float XMLTrainBrakeBias { get => TrainBrakeBias; set => TrainBrakeBias = value; }
        [XmlIgnore]
        internal static float TrainBrakeBias = TRAINBRAKEBIAS;

        [XmlElement("TrainSpringDamp")]
        public float XMLTrainSpringDamp { get => TrainSpringDamp; set => TrainSpringDamp = value; }
        [XmlIgnore]
        internal static float TrainSpringDamp = TRAINSPRINGDAMP;

        [XmlElement("TrainSpringOffset")]
        public float XMLTrainSpringOffset { get => TrainSpringOffset; set => TrainSpringOffset = value; }
        [XmlIgnore]
        internal static float TrainSpringOffset = TRAINSPRINGOFFSET;

        [XmlElement("TrainSpringSwayBar")]
        public float XMLTrainSpringSwayBar { get => TrainSpringSwayBar; set => TrainSpringSwayBar = value; }
        [XmlIgnore]
        internal static float TrainSpringSwayBar = TRAINSPRINGSWAYBAR;

        [XmlElement("TrainMassCenterHeight")]
        public float XMLTrainMassCenterHeight { get => TrainMassCenterHeight; set => TrainMassCenterHeight = value; }
        [XmlIgnore]
        internal static float TrainMassCenterHeight = TRAINMASSCENTERHEIGHT;

        [XmlElement("TrainMassCenterBias")]
        public float XMLTrainMassCenterBias { get => TrainMassCenterBias; set => TrainMassCenterBias = value; }
        [XmlIgnore]
        internal static float TrainMassCenterBias = TRAINMASSCENTERBIAS;
        #endregion

        #region Camera Settings
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
        #endregion

        #region Keybind Settings
        [XmlElement("KeyUUIToggle")]
        public Keybinding XMLKeyUUIToggle { get => Utils.UUISupport.UUIKey.Keybinding; set => Utils.UUISupport.UUIKey.Keybinding = value; }

        [XmlElement("KeyLightToggle")]
        public KeyOnlyBinding XMLKeyLightToggle { get => KeyLightToggle; set => KeyLightToggle = value; }
        [XmlIgnore]
        internal static KeyOnlyBinding KeyLightToggle = new KeyOnlyBinding(KeyCode.H);

        [XmlElement("KeyExtrasToggle")]
        public KeyOnlyBinding XMLKeyExtrasToggle { get => KeyExtrasToggle; set => KeyExtrasToggle = value; }
        [XmlIgnore]
        internal static KeyOnlyBinding KeyExtrasToggle = new KeyOnlyBinding(KeyCode.G);

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

        [XmlElement("KeyPitchUp")]
        public KeyOnlyBinding XMLKeyPitchUp { get => KeyPitchUp; set => KeyPitchUp = value; }
        [XmlIgnore]
        internal static KeyOnlyBinding KeyPitchUp = new KeyOnlyBinding(KeyCode.PageDown);

        [XmlElement("KeyPitchDown")]
        public KeyOnlyBinding XMLKeyPitchDown { get => KeyPitchDown; set => KeyPitchDown = value; }
        [XmlIgnore]
        internal static KeyOnlyBinding KeyPitchDown = new KeyOnlyBinding(KeyCode.PageUp);

        [XmlElement("KeyHandbrake")]
        public KeyOnlyBinding XMLKeyHandbrake { get => KeyHandbrake; set => KeyHandbrake = value; }
        [XmlIgnore]
        internal static KeyOnlyBinding KeyHandbrake = new KeyOnlyBinding(KeyCode.Space);

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
        #endregion
    }
}
