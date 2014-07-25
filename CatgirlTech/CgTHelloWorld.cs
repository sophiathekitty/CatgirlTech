using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CatgirlTech
{
    public class CgTHelloWorld : PartModule
    {
        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            // my test codes
            if (state == StartState.Editor)
            {
                // we're in the editor
                ScreenMessages.PostScreenMessage("Test part we're in the editor.", 6, ScreenMessageStyle.UPPER_CENTER);
            }
            else
            {
                // what state are we starting in?
                ScreenMessages.PostScreenMessage("Test part we're in the "+state+" state...?", 6, ScreenMessageStyle.UPPER_CENTER);
            }

        }
        // say hello action/event tests
        [KSPEvent(guiActive = true, guiName = "vessel parts", active = true)]
        public void vesselParts()
        {
            ScreenMessages.PostScreenMessage(this.vessel.parts.ToString(), 6, ScreenMessageStyle.UPPER_LEFT);

        }
        [KSPAction("vessel parts")]
        public void sayHello1Action(KSPActionParam param)
        {
            vesselParts();
        }
        [KSPEvent(guiActive = true, guiName = "top node", active = true)]
        public void sayHello2()
        {
            ScreenMessages.PostScreenMessage(this.part.topNode.attachedPart.ToString(), 6, ScreenMessageStyle.UPPER_LEFT);
            
        }
        [KSPAction("top node")]
        public void sayHelloAction(KSPActionParam param)
        {
            sayHello2();
        }
    }
}
