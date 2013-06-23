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
                lstRules.Selected -= new EventHandler<MVListSelectEventArgs>(lstRules_Selected);
                btnRuleClear.Click -= new EventHandler<MyClasses.MetaViewWrappers.MVControlEventArgs>(btnRuleClear_Click);
                btnRuleNew.Click -= new EventHandler<MyClasses.MetaViewWrappers.MVControlEventArgs>(btnRuleNew_Click);
                btnRuleUpdate.Click -= new EventHandler<MyClasses.MetaViewWrappers.MVControlEventArgs>(btnRuleUpdate_Click);
                chkRuleEnabled.Change -= new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkRuleEnabled_Change);
                txtRulePriority.End -= new EventHandler<MVTextBoxEndEventArgs>(txtRulePriority_End);
                txtRuleDescr.End -= new EventHandler<MVTextBoxEndEventArgs>(txtRuleDescr_End);
                txtRuleName.End -= new EventHandler<MVTextBoxEndEventArgs>(txtRuleName_End);
                txtRuleMaxCraft.End -= new EventHandler<MVTextBoxEndEventArgs>(txtRuleMaxCraft_End);
                txtRuleArcaneLore.End -= new EventHandler<MVTextBoxEndEventArgs>(txtRuleArcaneLore_End);
                txtRuleMaxBurden.End -= new EventHandler<MVTextBoxEndEventArgs>(txtRuleMaxBurden_End);
                txtRuleWieldReqValue.End -= new EventHandler<MVTextBoxEndEventArgs>(txtRuleWieldReqValue_End);
                txtRuleWieldLevel.End -= new EventHandler<MVTextBoxEndEventArgs>(txtRuleWieldLevel_End);
                txtRuleItemLevel.End -= new EventHandler<MVTextBoxEndEventArgs>(txtRuleItemLevel_End);
                txtRulePrice.End -= new EventHandler<MVTextBoxEndEventArgs>(txtRulePrice_End);
                txtRuleKeyWordsNot.End -= new EventHandler<MVTextBoxEndEventArgs>(txtRuleKeyWordsNot_End);
                txtRuleKeywords.End -= new EventHandler<MVTextBoxEndEventArgs>(txtRuleKeywords_End);
                cboWeaponAppliesTo.Change -= new EventHandler<MyClasses.MetaViewWrappers.MVIndexChangeEventArgs>(cboWeaponAppliesTo_Change);
                cboMasteryType.Change -= new EventHandler<MyClasses.MetaViewWrappers.MVIndexChangeEventArgs>(cboMasteryType_Change);
                chkRuleMSCleavea.Change -= new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkRuleMSCleavea_Change);
                chkRuleMSCleaveb.Change -= new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkRuleMSCleaveb_Change);
                chkRuleMSCleavec.Change -= new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkRuleMSCleavec_Change);
                chkRuleMSCleaved.Change -= new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkRuleMSCleaved_Change);
                chkRuleWeaponsa.Change -= new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkRuleWeaponsa_Change);
                chkRuleWeaponsb.Change -= new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkRuleWeaponsb_Change);
                chkRuleWeaponsc.Change -= new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkRuleWeaponsc_Change);
                chkRuleWeaponsd.Change -= new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkRuleWeaponsd_Change);
                lstDamageTypes.Selected -= new EventHandler<MVListSelectEventArgs>(lstDamageTypes_Selected);
                txtRuleMcModAttack.End -= new EventHandler<MVTextBoxEndEventArgs>(txtRuleMcModAttack_End);
                txtRuleMeleeD.End -= new EventHandler<MVTextBoxEndEventArgs>(txtRuleMeleeD_End);
                txtRuleMagicD.End -= new EventHandler<MVTextBoxEndEventArgs>(txtRuleMagicD_End);
                txtRuleReqSkilla.End -= new EventHandler<MVTextBoxEndEventArgs>(txtRuleReqSkilla_End);
                txtRuleMinMaxa.End -= new EventHandler<MVTextBoxEndEventArgs>(txtRuleMinMaxa_End);
                txtRuleReqSkillb.End -= new EventHandler<MVTextBoxEndEventArgs>(txtRuleReqSkillb_End);
                txtRuleReqSkillc.End -= new EventHandler<MVTextBoxEndEventArgs>(txtRuleReqSkillc_End);
                txtRuleReqSkilld.End -= new EventHandler<MVTextBoxEndEventArgs>(txtRuleReqSkilld_End);
                txtRuleMinMaxb.End -= new EventHandler<MVTextBoxEndEventArgs>(txtRuleMinMaxb_End);
                txtRuleMinMaxc.End -= new EventHandler<MVTextBoxEndEventArgs>(txtRuleMinMaxc_End);
                txtRuleMinMaxd.End -= new EventHandler<MVTextBoxEndEventArgs>(txtRuleMinMaxd_End);
                chkRuleMustBeUnenchantable.Change -= new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkRuleMustBeUnenchantable_Change);
                txtRuleMinArmorLevel.End -= new EventHandler<MVTextBoxEndEventArgs>(txtRuleMinArmorLevel_End);
                chkRuleCloakMustHaveSpell.Change -= new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkRuleCloakMustHaveSpell_Change);
                chkRuleRed.Change -= new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkRuleRed_Change);
                chkRuleYellow.Change -= new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkRuleYellow_Change);
                chkRuleBlue.Change -= new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkRuleBlue_Change);
                lstRuleSpells.Selected -= new EventHandler<MVListSelectEventArgs>(lstRuleSpells_Selected);
                lstRuleSpellsEnabled.Selected -= new EventHandler<MVListSelectEventArgs>(lstRuleSpellsEnabled_Selected);
                //lstRuleCloakSpells.Selected -= new EventHandler<MVListSelectEventArgs>(lstRuleCloakSpells_Selected);
                chkRuleFilterLegend.Change -= new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkRuleFilterLegend_Change);
                chkRuleFilterEpic.Change -= new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkRuleFilterEpic_Change);
                chkRuleFilterMajor.Change -= new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkRuleFilterMajor_Change);
                chkRuleFilterlvl8.Change -= new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkRuleFilterlvl8_Change);
                chkRuleFilterlvl7.Change -= new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkRuleFilterlvl7_Change);
                chkRuleFilterlvl6.Change -= new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkRuleFilterlvl6_Change);
             //   txtRuleSpellMatches.End -= new EventHandler<MVTextBoxEndEventArgs>(txtRuleSpellMatches_End);
                txtRuleNumSpells.End -= new EventHandler<MVTextBoxEndEventArgs>(txtRuleNumSpells_End);
                chkTrophyExact.Change -= new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkTrophyExact_Change);
                //  // cboTrophysetupAlert.Change -= new EventHandler<MyClasses.MetaViewWrappers.MVIndexChangeEventArgs>(cboTrophysetupAlert_Change);
                txtTrophyName.End -= new EventHandler<MVTextBoxEndEventArgs>(txtTrophyName_End);
                txtTrophyMax.End -= new EventHandler<MVTextBoxEndEventArgs>(txtTrophyMax_End);
                btnAttachTrophyItem.Click -= new EventHandler<MyClasses.MetaViewWrappers.MVControlEventArgs>(btnAttachTrophyItem_Click);
                btnAddTrophyItem.Click -= new EventHandler<MyClasses.MetaViewWrappers.MVControlEventArgs>(btnAddTrophyItem_Click);
                lstmyMobs.Selected -= new EventHandler<MVListSelectEventArgs>(lstmyMobs_Selected);
                chkmyMobExact.Change -= new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkmyMobExact_Change);
                //// cboMobsetupAlert.Change -= new EventHandler<MyClasses.MetaViewWrappers.MVIndexChangeEventArgs>(cboMobsetupAlert_Change);
                txtmyMobName.End -= new EventHandler<MVTextBoxEndEventArgs>(txtmyMobName_End);
                btnAddMobItem.Click -= new EventHandler<MyClasses.MetaViewWrappers.MVControlEventArgs>(btnAddMobItem_Click);
                lstNotifySalvage.Selected -= new EventHandler<MVListSelectEventArgs>(lstNotifySalvage_Selected);
                btnUpdateSalvage.Click -= new EventHandler<MyClasses.MetaViewWrappers.MVControlEventArgs>(btnUpdateSalvage_Click);
                txtSalvageName.End -= new EventHandler<MVTextBoxEndEventArgs>(txtSalvageName_End);
                txtSalvageString.End -= new EventHandler<MVTextBoxEndEventArgs>(txtSalvageString_End);
 	 

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

