/*
MIT License

Copyright (c) 2023 Multitask - Unified - Suite -for-Expts

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files(the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and / or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/


using System.Collections.Generic;
using UnityEngine;
using USE_States;
using USE_StimulusManagement;
using StimHandling_Namespace;
using UnityEngine.UI;
using USE_ExperimentTemplate_Trial;

public class StimHandling_TrialLevel : ControlLevel_Trial_Template
{
    public StimHandling_TrialDef CurrentTrialDef => GetCurrentTrialDef<StimHandling_TrialDef>();

    private StimGroup externalStimsA, externalStimsB, externalStimsC;
    public override void DefineControlLevel()
    {
        State startScreen = new State("StartScreen");
        State SetGroupAActive = new State("SetGroupAActive");
        State SetGroupBActiveAndInactive = new State("SetGroupBActiveAndInactive");
        State SetGroupC_Stim0Active = new State("SetGroupC_Stim0Active");
        State SetGroupC_Stim1ActiveAndInactive = new State("SetGroupC_Stim1ActiveAndInactive");
        State SetGroupsAandCInactive = new State("SetGroupsAandCInactive");

        Text stateNameText = null, commandText = null;
        int stateCount = 0;

        string[] stateNames = new string[] {"StartScreen", "SetGroupAActive", "SetGroupBActiveAndInactive", "SetGroupC_Stim0Active", "SetGroupC_Stim1ActiveAndInactive", "SetGroupsAandCInactive"};
        

        AddActiveStates(new List<State> {startScreen, SetGroupAActive, SetGroupBActiveAndInactive, SetGroupC_Stim0Active, SetGroupC_Stim1ActiveAndInactive, SetGroupsAandCInactive});

        SetupTrial.SpecifyTermination(() => true, startScreen);

        startScreen.AddDefaultInitializationMethod(() =>
        {
            stateNameText = GameObject.Find("StateNameText").GetComponent<Text>();
            commandText = GameObject.Find("CommandText").GetComponent<Text>();
            stateCount = 0;
            stateNameText.text = StateNameString();
            commandText.text = "Press the mouse button to set Group A stimuli active in initialization of next state.";
        });
        startScreen.SpecifyTermination(() => InputBroker.GetMouseButtonUp(0), SetGroupAActive, ()=>stateCount++);

        SetGroupAActive.AddDefaultInitializationMethod(() =>
        {
            stateNameText.text = StateNameString();
            commandText.text = "Press the mouse button to set Group B stimuli active in initialization of next state.";
        });
        SetGroupAActive.SpecifyTermination(() => InputBroker.GetMouseButtonUp(0), SetGroupBActiveAndInactive, ()=>stateCount++);
        
        SetGroupBActiveAndInactive.AddDefaultInitializationMethod(() =>
        {
            stateNameText.text = StateNameString();
            commandText.text = "Press the mouse button to set Group B stimuli inactive at the termination of the current state, and set Group C Stim 0 active in initialization of next state.";
        });
        SetGroupBActiveAndInactive.SpecifyTermination(() => InputBroker.GetMouseButtonUp(0), SetGroupC_Stim0Active, ()=>stateCount++);
        
        SetGroupC_Stim0Active.AddDefaultInitializationMethod(() =>
        {
            stateNameText.text = StateNameString();
            commandText.text = "Press the mouse button to set Group C Stim 1 active in initialization of next state.";
        });
        SetGroupC_Stim0Active.SpecifyTermination(() => InputBroker.GetMouseButtonUp(0), SetGroupC_Stim1ActiveAndInactive, ()=>stateCount++);
        
        SetGroupC_Stim1ActiveAndInactive.AddDefaultInitializationMethod(() =>
        {
            stateNameText.text = StateNameString();
            commandText.text = "Press the mouse button to set Group C Stim 1 inactive at the termination of the current state.";
        });
        SetGroupC_Stim1ActiveAndInactive.SpecifyTermination(() => InputBroker.GetMouseButtonUp(0), SetGroupsAandCInactive, ()=>stateCount++);
        
        SetGroupsAandCInactive.AddDefaultInitializationMethod(() =>
        {
            stateNameText.text = StateNameString();
            commandText.text = "Press the mouse button to end the trial (all stimuli will be deleted).";
        });
        SetGroupsAandCInactive.SpecifyTermination(() => InputBroker.GetMouseButtonUp(0), FinishTrial);

        string StateNameString()
        {
            if (stateCount == 0)
                return "Prior Trial State is SetupTrial (new trial), current State: " + stateNames[stateCount] +
                       " [" + stateCount + "], next State: " + stateNames[stateCount + 1] + " [" +
                       (stateCount + 1) + "].";
            else if (stateCount == stateNames.Length - 1)
                return "Prior Trial State: " + stateNames[stateCount - 1] + " [" + (stateCount - 1) +
                       "], current State: " + stateNames[stateCount] +
                       " [" + (stateCount) + "], next State is FinishTrial (end of trial).";
            else
                return "Prior Trial State: " + stateNames[stateCount - 1] + " [" + (stateCount - 1) +
                       "], current State: " + stateNames[stateCount] + " [" + stateCount +
                       "], next State: " + stateNames[stateCount + 1] + " [" + (stateCount + 1) + "].";

        }
    }

    protected override void DefineTrialStims()
    {
        //Define StimGroups consisting of StimDefs whose gameobjects will be loaded at TrialLevel_SetupTrial and 
        //destroyed at TrialLevel_Finish
        
        //StimGroup constructor which creates a subset of an already-existing StimGroup
        //string Name, .
        externalStimsA = new StimGroup("StimGroupA", ExternalStims, CurrentTrialDef.GroupAIndices);
        externalStimsA.SetVisibilityOnOffStates(GetStateFromName("SetGroupAActive"), null);
        externalStimsA.SetLocations(CurrentTrialDef.GroupALocations);
        externalStimsB = new StimGroup("StimGroupB", ExternalStims, CurrentTrialDef.GroupBIndices);
        externalStimsB.SetLocations(CurrentTrialDef.GroupBLocations);
        externalStimsB.SetVisibilityOnOffStates(GetStateFromName("SetGroupBActiveAndInactive"),GetStateFromName("SetGroupBActiveAndInactive"));
        externalStimsC = new StimGroup("StimGroupC", ExternalStims, CurrentTrialDef.GroupCIndices);
        externalStimsC.SetVisibilityOnOffStates(null, GetStateFromName("SetGroupsAandCInactive"));
        externalStimsC.stimDefs[0]
            .SetVisibilityOnOffStates(GetStateFromName("SetGroupC_Stim0Active"), null);
        externalStimsC.stimDefs[1].SetVisibilityOnOffStates(GetStateFromName("SetGroupC_Stim1ActiveAndInactive"), GetStateFromName("SetGroupC_Stim1ActiveAndInactive"));
        externalStimsC.SetLocations(CurrentTrialDef.GroupCLocations);
        TrialStims.Add(externalStimsA);
        TrialStims.Add(externalStimsB);
        TrialStims.Add(externalStimsC);
    }
    
}
