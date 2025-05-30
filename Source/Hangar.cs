﻿//   Hangar.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2015 Allis Tauri

using System.Collections.Generic;
using UnityEngine;
using AT_Utils;

namespace AtHangar
{
    public class Hangar : HangarMachinery
    {
        protected override List<HangarPassage> get_connected_passages() => 
        Storage?.ConnectedPassages();

        protected override void early_setup(StartState state)
        {
            base.early_setup(state);
            Storage = part.Modules.GetModule<HangarStorage>();
            if(Storage == null)
            {
                this.ConfigurationInvalid("\"{0}\" part has no HangarStorage module", part.Title());
                return;
            }
        }

#if DEBUG
        [KSPEvent(guiActive = true, guiName = "Check Airlock", active = true)]
        public void CheckAirlock()
        {
            if(part.airlock == null) return;
            RaycastHit raycastHit;
            if(Physics.Raycast(part.airlock.transform.position, (part.airlock.transform.position - part.transform.position).normalized, out raycastHit, 1, 32769))
            {
                this.Log("Airlock should be blocked:\n" +
                         "collider 'in front': {}\n" +
                         "distance to it: {}\n",
                         raycastHit.collider.name,
                         raycastHit.distance
                        );
            }
        }
#endif
    }
}

