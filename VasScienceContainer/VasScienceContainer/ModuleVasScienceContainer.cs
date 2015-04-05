//VAS Science Container plugin
//Writting by Diazo by request of Vas on #kspmodders
//Licensed into the public domain, do what you will as you will with this code
//
//This plugin requires a ModuleAnimateGeneric and ModuleSienceContainer on the same part.
//It exposes a "Load Science" command to a kerbal on eva which when pressed transfers science reports from the kerbal to the ModuleScienceContainer
//At the same time, it plays the ModuleAnimateGeneric animation from 0 -> 1
//After 5 seconds, it plays the ModuleAnimateGeneric from 1 -> 0
//The delay can be set in the part.cfg file under the AnimationDelayTime KSPField

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Timers;

namespace VasScienceContainer
{
    public class ModuleVasScienceContainer : PartModule
    {
        private Timer AnimationDelay;
        private ModuleAnimateGeneric animationModule; //find our associated ModuleAnimateGeneric on this part
        private ModuleScienceContainer scienceBoxModule; //find our associated ModuleScienceContainer on this part
        private BaseEvent loadSci;

        public override void OnStart(StartState state) //should run every time a part with this module on it loads
        {
            animationModule = this.part.Modules.OfType<ModuleAnimateGeneric>().First(); //can only be one ModuleAnimateGeneric on part
            scienceBoxModule = this.part.Modules.OfType<ModuleScienceContainer>().First(); //can only be one ModuleScienceContainer on part

            foreach (BaseAction ba in animationModule.Actions)
            {
                ba.active = false; //disable all actions
            }
            foreach (BaseAction ba2 in scienceBoxModule.Actions)
            {
                ba2.active = false; //disable all actions
            }
            foreach (BaseEvent be in animationModule.Events)
            {
                be.guiActive = false; //hide from right click menu in flight
                be.guiActiveEditor = false; //hide from menu in editor
                be.guiActiveUnfocused = false; //hide from menu when a kerbal on eva
            }
            foreach (BaseEvent be2 in scienceBoxModule.Events)
            {

                be2.guiActive = false; //hide from right click menu in flight
                be2.guiActiveEditor = false; //hide from menu in editor
                be2.guiActiveUnfocused = false; //hide from menu when a kerbal on eva

            }

            AnimationDelay = new Timer(); //setup our delay timer
            AnimationDelay.Interval = 5000; //5 second default delay
            AnimationDelay.Stop(); //stop the timer if it is running for some reason
            AnimationDelay.AutoReset = true; //make timer auto-reset
            AnimationDelay.Elapsed += new ElapsedEventHandler(AnimationDelayTimerElapsed); //hook in our method to execute when time up
            loadSci = this.Events.Find(e => e.name == "LoadScience");
            CheckButtonVisibility();
        }

        public void AnimationDelayTimerElapsed(object source, ElapsedEventArgs e) //runs when timer elapses
        {
            AnimationDelay.Stop(); //stop the timer so it resets
            if (animationModule.Progress == 1) //check that animation has finished playing
            {
                animationModule.Toggle(); //trigger animation in reverse 1 -> 0
            }
            CheckButtonVisibility();
        }
        public void CheckButtonVisibility()
        {

            if (scienceBoxModule.GetStoredDataCount() < scienceBoxModule.capacity)
            {
                loadSci.guiName = "Load Science";
            }
            else
            {
                loadSci.guiName = "Science Full";
            }

            BaseEvent revSci = this.Events.Find(e => e.name == "ReviewData");
            if (scienceBoxModule.GetStoredDataCount() > 0)
            {
                revSci.active = true;
            }
            else
            {
                revSci.active = false;
            }
        }

        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = false, guiName = "AnimationDelayTime")] //not player exposed, sets how long the delay is before the reverse animation starts playing, from the part.cfg file
        public float AnimationDelayTime = 10;

        [KSPEvent(active = true, guiActive = false, guiActiveEditor = false, guiName = "Load Science", guiActiveUnfocused = true, externalToEVAOnly = true)] //our button to expose on the right click menu, only show when on eva
        public void LoadScience()
        {
            if (loadSci.guiName == "Load Science")
            {
                scienceBoxModule.StoreDataExternalEvent(); //run sience load routine from the ModuleScienceContainer
                if (animationModule.Progress == 0) //check that animation has finished playing and is reset to start
                {
                    animationModule.Toggle(); //trigger animation 0 -> 1
                }
                AnimationDelay.Interval = AnimationDelayTime * 1000; //set our animation delay
                AnimationDelay.Start(); //start our timer for the return animation
            }

            CheckButtonVisibility();
        }

        [KSPEvent(active = true, guiActive = true, guiActiveEditor = false, guiName = "Review Data", guiActiveUnfocused = true, externalToEVAOnly = false)] //our button to expose on the right click menu once science is loaded
        public void ReviewData()
        {
            scienceBoxModule.ReviewDataEvent();
        }

    }


}
