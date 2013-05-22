﻿/////////////////////////////////////////////////////////////////////////////////
////This file is Copyright (c) 2011 VirindiPlugins
////
////Permission is hereby granted, free of charge, to any person obtaining a copy
////  of this software and associated documentation files (the "Software"), to deal
////  in the Software without restriction, including without limitation the rights
////  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
////  copies of the Software, and to permit persons to whom the Software is
////  furnished to do so, subject to the following conditions:
////
////The above copyright notice and this permission notice shall be included in
////  all copies or substantial portions of the Software.
////
////THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
////  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
////  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
////  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
////  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
////  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
////  THE SOFTWARE.//
/////////////////////////////////////////////////////////////////////////////////

// This entire file is being used -- Karin 4/16/13

using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using Decal.Adapter;
using System.IO;
using System.Xml.Serialization;
using Microsoft.Win32;
using Decal.Filters;
using Decal.Interop;
using System.Runtime.InteropServices;
using Decal.Adapter.Wrappers;
using System.Drawing;
using System.Xml.Linq;
using System.ComponentModel;
using VirindiViewService;
using MyClasses.MetaViewWrappers;

namespace GearFoundry
{
   [WireUpBaseEvents]
   
    [FriendlyName("GearFoundry")]
    public partial class PluginCore : PluginBase
    {

        protected override void Startup()
        {
            try
            {
                Globals.Init("GearFoundry", Host, Core);
                ViewInit();
                Instance = this;
                host = Host;
                InitEvents();
            }
            catch (Exception ex) { LogError(ex); }
        }

        protected override void Shutdown()
        {
            try
            {
            	
            	DisposeOnShutdown();
                //Destroy the view.
                MVWireupHelper.WireupEnd(this);
                View.Dispose();
                EndEvents();
                Core.CharacterFilter.LoginComplete -= new EventHandler(OnCharacterFilterLoginCompleted);
            }
            catch (Exception ex) { LogError(ex); }
        }
        
        private void DisposeOnShutdown()
        {
            if (quickieshHud != null) { DisposeHorizontalQuickSlots(); }
            if (quickiesvHud != null) { DisposeVerticalQuickSlots(); }
       		if(CorpseHudView != null) {DisposeCorpseHud();}
            if(LandscapeHudView != null) {DisposeLandscapeHud();}
            if(ItemHudView != null) {DisposeItemHud();}
            if(ButlerHudView != null){UnsubscribeButlerEvents();}
        }
        
        
        public void InitEvents()
		{

			try 
			{
				FileService = Core.Filter<FileService>();				
				Core.CharacterFilter.LoginComplete += new EventHandler(OnCharacterFilterLoginCompleted);			
				MasterTimer = new System.Windows.Forms.Timer();	
          

				
			} catch (Exception ex) {
				LogError(ex);
			}
		}


		public void EndEvents()
		{
			try {

                chkInventory.Change -= new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkInventory_Change);
                chkInventoryBurden.Change -= new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkInventoryBurden_Change);
                chkInventoryComplete.Change -= new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkInventoryComplete_Change);
                chkToonStats.Change -= new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkToonStats_Change);
                chkCombatHudEnabled.Change -= new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkCombatHudEnabled_Change);
                chkMuteSounds.Change -= new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkMuteSounds_Change);
                chkEnableTextFiltering.Change -= new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkEnableTextFiltering_Change);
                chkTextFilterAllStatus.Change -= new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkTextFilterAllStatus_Change);
                chkTextFilterBusyStatus.Change -= new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkTextFilterBusyStatus_Change);
                chkTextFilterCastingStatus.Change -= new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkTextFilterCastingStatus_Change);
                chkTextFilterMyDefenseMessages.Change -= new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkTextFilterMyDefenseMessages_Change);
                chkTextFilterMobDefenseMessages.Change -= new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkTextFilterMobDefenseMessages_Change);
                chkTextFilterMyKillMessages.Change -= new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkTextFilterMyKillMessages_Change);
                chkTextFilterPKFails.Change -= new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkTextFilterPKFails_Change);
                chkTextFilterDirtyFighting.Change -= new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkTextFilterDirtyFighting_Change);
                chkTextFilterMySpellCasting.Change -= new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkTextFilterMySpellCasting_Change);
                chkTextFilterOthersSpellCasting.Change -= new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkTextFilterOthersSpellCasting_Change);
                chkTextFilterSpellExpirations.Change -= new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkTextFilterSpellExpirations_Change);
                chkTextFilterManaStoneMessages.Change -= new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkTextFilterManaStoneMessages_Change);
                chkTextFilterHealingMessages.Change -= new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkTextFilterHealingMessages_Change);
                chkTextFilterSalvageMessages.Change -= new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkTextFilterSalvageMessages_Change);
                chkTextFilterBotSpam.Change -= new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkTextFilterBotSpam_Change);
                chkTextFilterIdentFailures.Change -= new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkTextFilterIdentFailures_Change);
                chkTextFilterKillTaskComplete.Change -= new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkTextFilterKillTaskComplete_Change);
                chkTextFilterVendorTells.Change -= new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkTextFilterVendorTells_Change);
                chkTextFilterMonsterTells.Change -= new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkTextFilterMonsterTells_Change);
                chkTextFilterNPCChatter.Change -= new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkTextFilterNPCChatter_Change);
 

				if (MasterTimer != null) {
					MasterTimer.Stop();
					MasterTimer.Tick -= LandscapeTimerTick;
					MasterTimer.Tick -= CorpseCheckerTick;
				}
				
				
				DisposeItemHud();

				GC.Collect();
				GC.WaitForPendingFinalizers();
	
				host = null;

			} catch (Exception ex) {
				LogError(ex);
			}

		}


        private void OnCharacterFilterLoginCompleted(object sender, System.EventArgs e)
        {
            WriteToChat("Plugin now online. Server population: " + Core.CharacterFilter.ServerPopulation);
            try
            {
            	InitPaths();
            	LoadSettingsFiles();
            	
				InitListBuilder();
               
                InitFilenames();
                loadFiles();
                loadLists();
                populateRulesListBox();
                populateRuleSpellEnabledListBox();
                startRoutines();
                
                

                Decal.Adapter.CoreManager.Current.ItemSelected += new EventHandler<ItemSelectedEventArgs>(Current_ItemSelected);

                WriteToChat("Plugin now online. Server population: " + Core.CharacterFilter.ServerPopulation);
                if(EssElementsList != null)
                    WriteToChat("EssElementsList count: " + EssElementsList.Count);
                else
                    WriteToChat("EssElementsList does not exist." );



                MasterTimer.Interval = 1000;
                MasterTimer.Start();
                
               

                mCharacterLoginComplete = true;
                
                RenderCombatHud();
                
  

            }
            catch (Exception ex) { LogError(ex); }
        }

        private void LoadSettingsFiles()
        {
        	try
        	{
        		
        	}
        	catch(Exception ex){LogError(ex);}
        }
        
        

        

    }
}

