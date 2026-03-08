using AlgernonCommons.Patching;
using ICities;
using DriveIt.UI;
using DriveIt.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace DriveIt
{
    public class DriveItLoading : PatcherLoadingBase<SettingsPanel, PatcherBase>
    {
        protected override List<AppMode> PermittedModes => new List<AppMode> { AppMode.Game, AppMode.MapEditor };
        protected override bool CreatedChecksPassed() { return true; }
        public override void OnLevelUnloading()
        {
            if (gameObject != null)
            {
                Object.Destroy(gameObject);
                gameObject = null;
            }
            base.OnLevelUnloading();
        }

        protected override void LoadedActions(LoadMode mode)
        {
            base.LoadedActions(mode);
            gameObject = new GameObject("DriveIt");
            gameObject.AddComponent<MainPanel>();
            gameObject.AddComponent<DriveButtons>();
            gameObject.AddComponent<DriveController>();
            gameObject.AddComponent<DriveCam>();
        }

        public override void OnCreated(ILoading loading)
        {
            base.OnCreated(loading);
            ModSupport.Initialize();
        }

        private GameObject gameObject = null;
    }
}
