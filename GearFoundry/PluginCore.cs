/////////////////////////////////////////////////////////////////////////////////
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
        
        public void InitEvents()
		{

			try 
			{	
				fileservice = (FileService)Core.FileService;				
				Core.CharacterFilter.LoginComplete += OnCharacterFilterLoginCompleted;	
		        
			} catch (Exception ex) {LogError(ex);}
		}

        private void OnCharacterFilterLoginCompleted(object sender, System.EventArgs e)
        {
            try
            {

            	InitPaths();           	
				InitListBuilder();               
                InitFilenames();
                loadFiles();
                startRoutines();
                _UpdateRulesTabs();
                _UpdateSalvagePanel();
                
                SubscribeFellowshipEvents();

                //TODO:  This could be moved to be subscribed situationally.
                Decal.Adapter.CoreManager.Current.ItemSelected += new EventHandler<ItemSelectedEventArgs>(Current_ItemSelected);

                WriteToChat("GearFoundry now online. Server population: " + Core.CharacterFilter.ServerPopulation);

                MasterTimer.Interval = 1000;
                MasterTimer.Start();
                           
            }
            catch (Exception ex) { LogError(ex); }
        }  

        protected override void Shutdown()
        {
            try
            {
            	DisposeOnShutdown();
                MVWireupHelper.WireupEnd(this);
                View.Dispose();
                EndEvents();
                Core.CharacterFilter.LoginComplete -= new EventHandler(OnCharacterFilterLoginCompleted);
                ClearRuleRelatedComponents();

            }
            catch (Exception ex) { LogError(ex); }
        }
        
        private void DisposeOnShutdown()
        {
            if (quickieshHud != null) { DisposeHorizontalQuickSlots(); }
            if (quickiesvHud != null) { DisposeVerticalQuickSlots(); }
            if (ArmorHudView != null) { DisposeArmorHud(); }
       		if(CorpseHudView != null) {DisposeCorpseHud();}
            if(LandscapeHudView != null) {DisposeLandscapeHud();}
            if(ItemHudView != null) {DisposeItemHud();}
            if(ButlerHudView != null){UnsubscribeButlerEvents();}
            if(InventoryHudView != null){DisposeInventoryHud();}
            if (ArmorHudView != null) { DisposeArmorHud(); }

        }
        
        



		public void EndEvents()
		{
			try {

                chkInventory.Change -= new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkInventory_Change);
                chkInventoryBurden.Change -= new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkInventoryBurden_Change);
                chkInventoryComplete.Change -= new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkInventoryComplete_Change);
                chkToonStats.Change -= new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkToonStats_Change);
                if (mWaitingForIDTimer != null) { mWaitingForIDTimer.Tick -= new EventHandler(TimerEventProcessor); mWaitingForIDTimer = null; }
                if (mWaitingForID != null) { mWaitingForID = null; }
                chkToonStats.Change -= new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkToonStats_Change);
                chkCombatHudEnabled.Change -= new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkCombatHudEnabled_Change);
                chkMuteSounds.Change -= new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkMuteSounds_Change);
                chkEnableTextFiltering.Change -= new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkEnableTextFiltering_Change);
                chkTextFilterAllStatus.Change -= new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkTextFilterAllStatus_Change);

                lstRules.Selected -= new EventHandler<MVListSelectEventArgs>(lstRules_Selected);
                lstRuleApplies.Selected -= new EventHandler<MVListSelectEventArgs>(lstRuleApplies_Selected);
                lstRuleSlots.Selected -= new EventHandler<MVListSelectEventArgs>(lstRuleSlots_Selected);
                lstRuleSets.Selected -= new EventHandler<MVListSelectEventArgs>(lstRuleSets_Selected);
                lstRuleArmorTypes.Selected -= new EventHandler<MVListSelectEventArgs>(lstRuleArmorTypes_Selected);
                btnRuleClear.Click -= new EventHandler<MyClasses.MetaViewWrappers.MVControlEventArgs>(btnRuleClear_Click);
                btnRuleNew.Click -= new EventHandler<MyClasses.MetaViewWrappers.MVControlEventArgs>(btnRuleNew_Click);
                btnRuleUpdate.Click -= new EventHandler<MyClasses.MetaViewWrappers.MVControlEventArgs>(btnRuleUpdate_Click);
                chkRuleEnabled.Change -= new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkRuleEnabled_Change);
                txtRulePriority.End -= new EventHandler<MVTextBoxEndEventArgs>(txtRulePriority_End);
                txtGearScore.End -= txtGearScore_End;
              
                txtRuleName.End -= new EventHandler<MVTextBoxEndEventArgs>(txtRuleName_End);
                txtRuleMaxCraft.End -= new EventHandler<MVTextBoxEndEventArgs>(txtRuleMaxCraft_End);
                txtRuleArcaneLore.End -= new EventHandler<MVTextBoxEndEventArgs>(txtRuleArcaneLore_End);
                txtRuleWieldLevel.End -= new EventHandler<MVTextBoxEndEventArgs>(txtRuleWieldLevel_End);
              

                cboWeaponAppliesTo.Change -= new EventHandler<MyClasses.MetaViewWrappers.MVIndexChangeEventArgs>(cboWeaponAppliesTo_Change);
                cboMasteryType.Change -= new EventHandler<MyClasses.MetaViewWrappers.MVIndexChangeEventArgs>(cboMasteryType_Change);

                chkRuleWeaponsa.Change -= new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkRuleWeaponsa_Change);
                chkRuleWeaponsb.Change -= new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkRuleWeaponsb_Change);
                chkRuleWeaponsc.Change -= new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkRuleWeaponsc_Change);
                chkRuleWeaponsd.Change -= new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkRuleWeaponsd_Change);
                lstDamageTypes.Selected -= new EventHandler<MVListSelectEventArgs>(lstDamageTypes_Selected);
    
             
                txtRuleReqSkilla.End -= new EventHandler<MVTextBoxEndEventArgs>(txtRuleReqSkilla_End);
              
                txtRuleReqSkillb.End -= new EventHandler<MVTextBoxEndEventArgs>(txtRuleReqSkillb_End);
                txtRuleReqSkillc.End -= new EventHandler<MVTextBoxEndEventArgs>(txtRuleReqSkillc_End);
                txtRuleReqSkilld.End -= new EventHandler<MVTextBoxEndEventArgs>(txtRuleReqSkilld_End);
              
              
                lstRuleSpells.Selected -= new EventHandler<MVListSelectEventArgs>(lstRuleSpells_Selected);
                lstRuleSpellsEnabled.Selected -= new EventHandler<MVListSelectEventArgs>(lstRuleSpellsEnabled_Selected);

                chkRuleFilterLegend.Change -= new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkRuleFilterLegend_Change);
                chkRuleFilterEpic.Change -= new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkRuleFilterEpic_Change);
                chkRuleFilterMajor.Change -= new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkRuleFilterMajor_Change);
                chkRuleFilterlvl8.Change -= new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkRuleFilterlvl8_Change);
                chkRuleFilterCloak.Change -= new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkRuleFilterCloak_Change);
                	
                txtRuleNumSpells.End -= new EventHandler<MVTextBoxEndEventArgs>(txtRuleNumSpells_End);
                chkTrophyExact.Change -= new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkTrophyExact_Change);
                txtTrophyName.End -= new EventHandler<MVTextBoxEndEventArgs>(txtTrophyName_End);
                txtTrophyMax.End -= new EventHandler<MVTextBoxEndEventArgs>(txtTrophyMax_End);
                btnAddTrophyItem.Click -= new EventHandler<MyClasses.MetaViewWrappers.MVControlEventArgs>(btnAddTrophyItem_Click);
                lstmyMobs.Selected -= new EventHandler<MVListSelectEventArgs>(lstmyMobs_Selected);
                chkmyMobExact.Change -= new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkmyMobExact_Change);

                txtmyMobName.End -= new EventHandler<MVTextBoxEndEventArgs>(txtmyMobName_End);
                btnAddMobItem.Click -= new EventHandler<MyClasses.MetaViewWrappers.MVControlEventArgs>(btnAddMobItem_Click);
                lstNotifySalvage.Selected -= new EventHandler<MVListSelectEventArgs>(lstNotifySalvage_Selected);
                btnUpdateSalvage.Click -= new EventHandler<MyClasses.MetaViewWrappers.MVControlEventArgs>(btnUpdateSalvage_Click);
         
                txtSalvageString.End -= new EventHandler<MVTextBoxEndEventArgs>(txtSalvageString_End);
                    
                chkAdvEnabled.Change -= chkAdvEnabled_Change;
                
                cboAdv1KeyType.Change -= cboAdv1KeyType_Change;
                cboAdv1Key.Change -= cboAdv1Key_Change;
                cboAdv1KeyCompare.Change -= cboAdv1KeyCompare_Change;
                txtAdv1KeyValue.End -= txtAdv1KeyValue_Change; 	 
                
                cboAdv1Link.Change -= cboAdv1Link_Change;
                
                cboAdv2KeyType.Change -= cboAdv2KeyType_Change;
                cboAdv2Key.Change -= cboAdv2Key_Change;
                cboAdv2KeyCompare.Change -= cboAdv2KeyCompare_Change;
                txtAdv2KeyValue.End -= txtAdv2KeyValue_Change; 	 
                
                cboAdv2Link.Change -= cboAdv2Link_Change;
                
                cboAdv3KeyType.Change -= cboAdv3KeyType_Change;
                cboAdv3Key.Change -= cboAdv3Key_Change;
                cboAdv3KeyCompare.Change -= cboAdv3KeyCompare_Change;
                txtAdv3KeyValue.End -= txtAdv3KeyValue_Change; 	 
                
                cboAdv3Link.Change -= cboAdv3Link_Change;
                
                cboAdv4KeyType.Change -= cboAdv4KeyType_Change;
                cboAdv4Key.Change -= cboAdv4Key_Change;
                cboAdv4KeyCompare.Change -= cboAdv4KeyCompare_Change;
                txtAdv4KeyValue.End -= txtAdv4KeyValue_Change; 	 
                
                cboAdv4Link.Change -= cboAdv4Link_Change;
                
                cboAdv5KeyType.Change -= cboAdv5KeyType_Change;
                cboAdv5Key.Change -= cboAdv5Key_Change;
                cboAdv5KeyCompare.Change -= cboAdv5KeyCompare_Change;
                txtAdv5KeyValue.End -= txtAdv5KeyValue_Change; 	 
                

                
                
                
                
                

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


  

    }
}

