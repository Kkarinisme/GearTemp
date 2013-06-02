///////////////////////////////////////////////////////////////////////////////
//This file is Copyright (c) 2011 VirindiPlugins
//
//Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
//
//The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
//
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.
///////////////////////////////////////////////////////////////////////////////

//This entire file is being used -- Karin 04/16/13

using System;
using System.Collections.Generic;
using System.Text;
using VirindiViewService;
using MyClasses.MetaViewWrappers;
using System.Drawing;
using Decal.Adapter;
using Decal.Adapter.Wrappers;

namespace GearFoundry
{
	public partial class PluginCore
	{
        #region Auto-generated view codee
        // Need this line to be able to initialize View below
        
        MyClasses.MetaViewWrappers.IView View;
        MyClasses.MetaViewWrappers.INotebook nbSetupsetup;
        // Controls on Setup Page
        // WormGears controls
        MyClasses.MetaViewWrappers.ICheckBox chkQuickSlotsv;
        MyClasses.MetaViewWrappers.ICheckBox chkQuickSlotsh;
        
        //GearVisection Controls
        MyClasses.MetaViewWrappers.ICheckBox chkGearVisectionEnabled;
        
        //GearSense Controls
        MyClasses.MetaViewWrappers.ICheckBox chkGearSenseEnabled;
        
        //GearButler Controls
        MyClasses.MetaViewWrappers.ICheckBox chkGearButlerEnabled;  
        
        //GearInspector Controls
        MyClasses.MetaViewWrappers.ICheckBox chkGearInspectorEnabled;

        //Gears Tactician
        MyClasses.MetaViewWrappers.ICheckBox chkCombatHudEnabled;
        
        MyClasses.MetaViewWrappers.ICheckBox chkInventoryBurden;
        MyClasses.MetaViewWrappers.ICheckBox chkInventoryComplete;
        MyClasses.MetaViewWrappers.ICheckBox chkInventory;
        MyClasses.MetaViewWrappers.ICheckBox chkToonStats;
        MyClasses.MetaViewWrappers.ICheckBox chkToonArmor;

        //Gears Misc
        MyClasses.MetaViewWrappers.ICheckBox chkMuteSounds;
        MyClasses.MetaViewWrappers.ICheckBox chkArmorHud;



        MyClasses.MetaViewWrappers.ICheckBox chkEnableTextFiltering;
        MyClasses.MetaViewWrappers.ICheckBox chkTextFilterAllStatus;
        MyClasses.MetaViewWrappers.ICheckBox chkTextFilterBusyStatus;
        MyClasses.MetaViewWrappers.ICheckBox chkTextFilterCastingStatus;
        MyClasses.MetaViewWrappers.ICheckBox chkTextFilterMyDefenseMessages;
        MyClasses.MetaViewWrappers.ICheckBox chkTextFilterMobDefenseMessages;
        MyClasses.MetaViewWrappers.ICheckBox chkTextFilterMyKillMessages;
        MyClasses.MetaViewWrappers.ICheckBox chkTextFilterPKFails;
        MyClasses.MetaViewWrappers.ICheckBox chkTextFilterDirtyFighting;
        MyClasses.MetaViewWrappers.ICheckBox chkTextFilterMySpellCasting;
        MyClasses.MetaViewWrappers.ICheckBox chkTextFilterOthersSpellCasting;
        MyClasses.MetaViewWrappers.ICheckBox chkTextFilterSpellExpirations;
        MyClasses.MetaViewWrappers.ICheckBox chkTextFilterManaStoneMessages;
        MyClasses.MetaViewWrappers.ICheckBox chkTextFilterHealingMessages;
        MyClasses.MetaViewWrappers.ICheckBox chkTextFilterSalvageMessages;
        MyClasses.MetaViewWrappers.ICheckBox chkTextFilterBotSpam;
        MyClasses.MetaViewWrappers.ICheckBox chkTextFilterIdentFailures;
        MyClasses.MetaViewWrappers.ICheckBox chkTextFilterKillTaskComplete;
        MyClasses.MetaViewWrappers.ICheckBox chkTextFilterVendorTells;
        MyClasses.MetaViewWrappers.ICheckBox chkTextFilterMonsterTells;
        MyClasses.MetaViewWrappers.ICheckBox chkTextFilterNPCChatter;


        // Controls on Notify.SearchRules Page
        MyClasses.MetaViewWrappers.IButton btnRuleClear;
        MyClasses.MetaViewWrappers.IButton btnRuleNew;
        MyClasses.MetaViewWrappers.IButton btnRuleUpdate;
        MyClasses.MetaViewWrappers.IList lstRules;

       // MyClasses.MetaViewWrappers.IStaticText lblRuleName;

        // Controls on Notify.SearchRules.Main Page
        MyClasses.MetaViewWrappers.ICheckBox chkRuleEnabled;
        MyClasses.MetaViewWrappers.IList lstRuleApplies;

        MyClasses.MetaViewWrappers.ITextBox txtRuleName;
        MyClasses.MetaViewWrappers.ITextBox txtRuleDescr;
        MyClasses.MetaViewWrappers.ITextBox txtRulePriority;
        MyClasses.MetaViewWrappers.ITextBox txtRuleMaxCraft;
        MyClasses.MetaViewWrappers.ITextBox txtRuleKeywords;
        MyClasses.MetaViewWrappers.ITextBox txtRuleArcaneLore;
        MyClasses.MetaViewWrappers.ITextBox txtRulePrice;
        MyClasses.MetaViewWrappers.ITextBox txtRuleMaxBurden;
        MyClasses.MetaViewWrappers.ITextBox txtRuleKeyWordsNot;
        MyClasses.MetaViewWrappers.ITextBox txtRuleWieldReqValue;
        MyClasses.MetaViewWrappers.ITextBox txtRuleWieldLevel;
        //MyClasses.MetaViewWrappers.ITextBox txtRuleAlertName;
        //MyClasses.MetaViewWrappers.ICheckBox chkRuleTradeBot;
        //MyClasses.MetaViewWrappers.ICheckBox chkRuleTradeBotOnly;
 
        MyClasses.MetaViewWrappers.IStaticText lblRuleInfo;
         MyClasses.MetaViewWrappers.IStaticText lblRuleWork;
        MyClasses.MetaViewWrappers.IStaticText lblRuleNameMust;
        MyClasses.MetaViewWrappers.IStaticText lblRuleBurden;
        MyClasses.MetaViewWrappers.IStaticText lblRuleNameMustNot;
        MyClasses.MetaViewWrappers.IStaticText lblRuleValue;
        MyClasses.MetaViewWrappers.IStaticText lblRuleWieldLevel;
        MyClasses.MetaViewWrappers.IStaticText lblRuleWieldReqValue;
        MyClasses.MetaViewWrappers.IStaticText lblRuleItemLevel;
       //  MyClasses.MetaViewWrappers.IStaticText lblRuleAlertName;

        // Controls on Notify.SearchRules.Weapon Page
        MyClasses.MetaViewWrappers.ICombo cboWeaponAppliesTo;
        MyClasses.MetaViewWrappers.ICombo cboMasteryType;
        MyClasses.MetaViewWrappers.IList lstDamageTypes;
        MyClasses.MetaViewWrappers.ICheckBox chkRuleMSCleavea;
        MyClasses.MetaViewWrappers.ICheckBox chkRuleMSCleaveb;
        MyClasses.MetaViewWrappers.ICheckBox chkRuleMSCleavec;
        MyClasses.MetaViewWrappers.ICheckBox chkRuleMSCleaved;
        MyClasses.MetaViewWrappers.ICheckBox chkRuleWeaponsb;
        MyClasses.MetaViewWrappers.ICheckBox chkRuleWeaponsa;
        MyClasses.MetaViewWrappers.ICheckBox chkRuleWeaponsc;
        MyClasses.MetaViewWrappers.ICheckBox chkRuleWeaponsd;

        MyClasses.MetaViewWrappers.ITextBox txtRuleMcModAttack;
        MyClasses.MetaViewWrappers.ITextBox txtRuleMeleeD;
        MyClasses.MetaViewWrappers.ITextBox txtRuleMagicD;
        MyClasses.MetaViewWrappers.ITextBox txtRuleReqSkilla;
        MyClasses.MetaViewWrappers.ITextBox txtRuleMinMaxa;
        MyClasses.MetaViewWrappers.ITextBox txtRuleReqSkillb;
        MyClasses.MetaViewWrappers.ITextBox txtRuleMinMaxb;
        MyClasses.MetaViewWrappers.ITextBox txtRuleReqSkillc;
        MyClasses.MetaViewWrappers.ITextBox txtRuleMinMaxc;
        MyClasses.MetaViewWrappers.ITextBox txtRuleReqSkilld;
        MyClasses.MetaViewWrappers.ITextBox txtRuleMinMaxd;

        MyClasses.MetaViewWrappers.IStaticText lblWeapCat;
        MyClasses.MetaViewWrappers.IStaticText lblMastCat;
        MyClasses.MetaViewWrappers.IStaticText lblDamageTypes;
        MyClasses.MetaViewWrappers.IStaticText lblRuleMcModAttack;
        MyClasses.MetaViewWrappers.IStaticText lblRuleMeleeD;
        MyClasses.MetaViewWrappers.IStaticText lblRuleMagicD;
        MyClasses.MetaViewWrappers.IStaticText lblEnabled10025;
        MyClasses.MetaViewWrappers.IStaticText lblRuleReqSkill;
        MyClasses.MetaViewWrappers.IStaticText lblRuleMinMax_ElvsMons;
        MyClasses.MetaViewWrappers.IStaticText lblRuleMSCleave;

        // Controls on Notify.SearchRules.Armor Page
        MyClasses.MetaViewWrappers.IList lstRuleArmorCoverages;
        MyClasses.MetaViewWrappers.IList lstRuleArmorTypes;
        MyClasses.MetaViewWrappers.ICheckBox chkRuleMustBeUnenchantable;
        MyClasses.MetaViewWrappers.IStaticText lblRuleArmorCoverage;
        MyClasses.MetaViewWrappers.IStaticText lblRuleArmorTypes;
        MyClasses.MetaViewWrappers.ITextBox txtRuleMinArmorLevel;
        MyClasses.MetaViewWrappers.IList lstRuleSets;
        //MyClasses.MetaViewWrappers.ICheckBox chkRuleMustBeSet;
        //MyClasses.MetaViewWrappers.ICheckBox chkRuleAnySet;
        //MyClasses.MetaViewWrappers.IStaticText lblRuleMustBeSet;

        // Controls on Notify.SearchRules.Cloaks/Aetheria
        MyClasses.MetaViewWrappers.IList lstRuleCloakSets;
        MyClasses.MetaViewWrappers.IList lstRuleCloakSpells;
        MyClasses.MetaViewWrappers.ITextBox txtRuleItemLevel;
        MyClasses.MetaViewWrappers.ICheckBox chkRuleCloakMustHaveSpell;
        MyClasses.MetaViewWrappers.ICheckBox chkRuleRed;
        MyClasses.MetaViewWrappers.ICheckBox chkRuleYellow;
        MyClasses.MetaViewWrappers.ICheckBox chkRuleBlue;

        // Controls on Notify.SearchRules.Essences
        MyClasses.MetaViewWrappers.ICombo cboRuleEssMastery;
        MyClasses.MetaViewWrappers.IList lstRuleEssElements;
        MyClasses.MetaViewWrappers.ITextBox txtRuleEssLevel;
    //    MyClasses.MetaViewWrappers.ITextBox txtRuleEssSummLevel;
        MyClasses.MetaViewWrappers.ITextBox txtRuleEssDamageLevel;
        MyClasses.MetaViewWrappers.ITextBox txtRuleEssCDLevel;
        MyClasses.MetaViewWrappers.ITextBox txtRuleEssCRLevel;
        MyClasses.MetaViewWrappers.ITextBox txtRuleEssDRLevel;
        MyClasses.MetaViewWrappers.ITextBox txtRuleEssCritLevel;
        MyClasses.MetaViewWrappers.ITextBox txtRuleEssCritDamResLevel;

        // Controls on Notify.SearchRules.Req Spells

        MyClasses.MetaViewWrappers.IList lstRuleSpells;
        MyClasses.MetaViewWrappers.IList lstRuleSpellsEnabled;
        MyClasses.MetaViewWrappers.ICheckBox chkRuleFilterLegend;
        MyClasses.MetaViewWrappers.ICheckBox chkRuleFilterEpic;
        MyClasses.MetaViewWrappers.ICheckBox chkRuleFilterMajor;
        MyClasses.MetaViewWrappers.ICheckBox chkRuleFilterlvl8;
        MyClasses.MetaViewWrappers.ICheckBox chkRuleFilterlvl7;
        MyClasses.MetaViewWrappers.ICheckBox chkRuleFilterlvl6;
        MyClasses.MetaViewWrappers.ITextBox txtRuleSpellMatches;
        MyClasses.MetaViewWrappers.ITextBox txtRuleNumSpells;

        MyClasses.MetaViewWrappers.IStaticText lblRuleMustHaveSpells;
        MyClasses.MetaViewWrappers.IStaticText lblRuleMoreSpells;


        // Controls on Notify.NPC/Trophies Page

        MyClasses.MetaViewWrappers.IList lstmyTrophies;
        MyClasses.MetaViewWrappers.ITextBox txtTrophyName;
        MyClasses.MetaViewWrappers.IStaticText lblAtr11;
        MyClasses.MetaViewWrappers.IButton btnAttachTrophyItem;
        MyClasses.MetaViewWrappers.IButton btnAddTrophyItem;
        MyClasses.MetaViewWrappers.ICheckBox chkTrophyExact;
        MyClasses.MetaViewWrappers.IStaticText lblMyItemsCountMax;
        MyClasses.MetaViewWrappers.ITextBox txtTrophyMax;

        // Controls on Notify.Mobs Page
        MyClasses.MetaViewWrappers.IList lstmyMobs;
        MyClasses.MetaViewWrappers.ICombo cboMobsetupAlert;
        MyClasses.MetaViewWrappers.IButton btnAddMobItem;
        MyClasses.MetaViewWrappers.ICheckBox chkmyMobExact;
        MyClasses.MetaViewWrappers.ITextBox txtmyMobName;
        MyClasses.MetaViewWrappers.IStaticText lblatr121;

        // Controls on Notify.Salvage Page
        MyClasses.MetaViewWrappers.IList lstNotifySalvage;
        MyClasses.MetaViewWrappers.IButton btnUpdateSalvage;
        MyClasses.MetaViewWrappers.ITextBox txtSalvageName;
        MyClasses.MetaViewWrappers.ITextBox txtSalvageString;
        MyClasses.MetaViewWrappers.IStaticText lblSalvageName;
        MyClasses.MetaViewWrappers.IStaticText lblSalvageString;

        // Controls on Notify.Other Page
        MyClasses.MetaViewWrappers.IList lstNotifyOptions;
        MyClasses.MetaViewWrappers.ICheckBox chkSalvageAll;
        MyClasses.MetaViewWrappers.ITextBox txtMaxMana;
        MyClasses.MetaViewWrappers.IStaticText lblMaxMana;
        MyClasses.MetaViewWrappers.ITextBox txtMaxValue;
        MyClasses.MetaViewWrappers.IStaticText lblMaxValue;
        MyClasses.MetaViewWrappers.ITextBox txtVbratio;
        MyClasses.MetaViewWrappers.IStaticText lblVbratio;

        // Controls on Ust Page
        MyClasses.MetaViewWrappers.IList lstUstList;
        MyClasses.MetaViewWrappers.IButton btnUstItems;
        MyClasses.MetaViewWrappers.IButton btnUstClear;
        MyClasses.MetaViewWrappers.IButton btnUstSalvage;
        MyClasses.MetaViewWrappers.ITextBox txtSalvageAug;
        MyClasses.MetaViewWrappers.IStaticText lblUst002;

        // Controls on Alerts Page
        // Controls on Inventory Page
        // Controls on Settings Page
        MyClasses.MetaViewWrappers.IList lstOtherOptions;
        // Variable name txtSettingsCW was txtsettingscw
        MyClasses.MetaViewWrappers.ITextBox txtSettingsCW;
        // Variable name lblSettings001 was lblsettings001  
        MyClasses.MetaViewWrappers.IStaticText lblSettings001;

        //Controls on Inventory Page
        MyClasses.MetaViewWrappers.IButton btnGetInventory;
        MyClasses.MetaViewWrappers.IButton btnUpdateInventory;
        MyClasses.MetaViewWrappers.IButton btnGetBurden;
        MyClasses.MetaViewWrappers.IButton btnItemsWaiting;
        MyClasses.MetaViewWrappers.IButton btnLstInventory;
        MyClasses.MetaViewWrappers.IButton btnClrInventory;
        MyClasses.MetaViewWrappers.ICombo cmbSelectClass;
        MyClasses.MetaViewWrappers.ICombo cmbWieldAttrib;
        MyClasses.MetaViewWrappers.ICombo cmbDamageType;
        MyClasses.MetaViewWrappers.ICombo cmbLevel;
        MyClasses.MetaViewWrappers.ICombo cmbArmorSet;
        MyClasses.MetaViewWrappers.ICombo cmbMaterial;
        MyClasses.MetaViewWrappers.ICombo cmbCoverage;
        MyClasses.MetaViewWrappers.ICombo cmbArmorLevel;
        MyClasses.MetaViewWrappers.ICombo cmbSalvWork;
        MyClasses.MetaViewWrappers.ICombo cmbEmbue;

        MyClasses.MetaViewWrappers.IList lstInventory;
        MyClasses.MetaViewWrappers.ITextBox txbSelect;





       void ViewInit()
        {
            try
            {

                //Create view here
                View = MyClasses.MetaViewWrappers.ViewSystemSelector.CreateViewResource(PluginCore.host, "MainView.xml");
                nbSetupsetup = (MyClasses.MetaViewWrappers.INotebook)View["nbSetupsetup"];
                nbSetupsetup.Change +=new EventHandler<MVIndexChangeEventArgs>(nbSetupsetup_Change);

                //Controls on Setup Page
                try
                {
                    //WormGears Controls
                	chkQuickSlotsv = (MyClasses.MetaViewWrappers.ICheckBox)View["chkQuickSlotsv"];
                    chkQuickSlotsh = (MyClasses.MetaViewWrappers.ICheckBox)View["chkQuickSlotsh"];
                }catch(Exception ex){LogError(ex);}
                
                try
                {
                    //GearVisection Controls
                    chkGearVisectionEnabled = (MyClasses.MetaViewWrappers.ICheckBox)View["chkGearVisectionEnabled"];
                 }catch(Exception ex){LogError(ex);}   
                 try
                 {
                    //GearSense Controls
                    chkGearSenseEnabled = (MyClasses.MetaViewWrappers.ICheckBox)View["chkGearSenseEnabled"];
        		}catch(Exception ex){LogError(ex);}
				try
				{
        			//GearButler Controls
        			chkGearButlerEnabled =(MyClasses.MetaViewWrappers.ICheckBox)View["chkGearButlerEnabled"];
        			chkGearInspectorEnabled = (MyClasses.MetaViewWrappers.ICheckBox)View["chkGearInspectorEnabled"];
        		//	chkAutoRingKeys = (MyClasses.MetaViewWrappers.ICheckBox)View["chkAutoRingKeys"];
				}catch(Exception ex){LogError(ex);}
     			try
     			{

                   chkInventory = (MyClasses.MetaViewWrappers.ICheckBox)View["chkInventory"];
                   chkInventoryBurden = (MyClasses.MetaViewWrappers.ICheckBox)View["chkInventoryBurden"];
                   chkInventoryComplete = (MyClasses.MetaViewWrappers.ICheckBox)View["chkInventoryComplete"];
                   chkToonStats = (MyClasses.MetaViewWrappers.ICheckBox)View["chkToonStats"];
                   chkToonArmor = (MyClasses.MetaViewWrappers.ICheckBox)View["chkToonArmor"];
     			}catch(Exception ex){LogError(ex);}

                //Gears Tactician page
                chkCombatHudEnabled = (MyClasses.MetaViewWrappers.ICheckBox)View["chkCombatHudEnabled"];

                //Misc Gears
                chkMuteSounds = (MyClasses.MetaViewWrappers.ICheckBox)View["chkMuteSounds"];
                chkArmorHud = (MyClasses.MetaViewWrappers.ICheckBox)View["chkArmorHud"];
 
                //Text Filtering Controls
                chkEnableTextFiltering = (MyClasses.MetaViewWrappers.ICheckBox)View["chkEnableTextFiltering"];
                chkTextFilterAllStatus = (MyClasses.MetaViewWrappers.ICheckBox)View["chkTextFilterAllStatus"];
                chkTextFilterBusyStatus = (MyClasses.MetaViewWrappers.ICheckBox)View["chkTextFilterBusyStatus"];
                chkTextFilterCastingStatus = (MyClasses.MetaViewWrappers.ICheckBox)View["chkTextFilterCastingStatus"];
                chkTextFilterMyDefenseMessages = (MyClasses.MetaViewWrappers.ICheckBox)View["chkTextFilterMyDefenseMessages"];
                chkTextFilterMobDefenseMessages = (MyClasses.MetaViewWrappers.ICheckBox)View["chkTextFilterMobDefenseMessages"];
                chkTextFilterMyKillMessages = (MyClasses.MetaViewWrappers.ICheckBox)View["chkTextFilterMyKillMessages"];
                chkTextFilterPKFails = (MyClasses.MetaViewWrappers.ICheckBox)View["chkTextFilterPKFails"];
                chkTextFilterDirtyFighting = (MyClasses.MetaViewWrappers.ICheckBox)View["chkTextFilterDirtyFighting"];
                chkTextFilterMySpellCasting = (MyClasses.MetaViewWrappers.ICheckBox)View["chkTextFilterMySpellCasting"];
                chkTextFilterOthersSpellCasting = (MyClasses.MetaViewWrappers.ICheckBox)View["chkTextFilterOthersSpellCasting"];
                chkTextFilterSpellExpirations = (MyClasses.MetaViewWrappers.ICheckBox)View["chkTextFilterSpellExpirations"];
                chkTextFilterManaStoneMessages = (MyClasses.MetaViewWrappers.ICheckBox)View["chkTextFilterManaStoneMessages"];
                chkTextFilterHealingMessages = (MyClasses.MetaViewWrappers.ICheckBox)View["chkTextFilterHealingMessages"];
                chkTextFilterSalvageMessages = (MyClasses.MetaViewWrappers.ICheckBox)View["chkTextFilterSalvageMessages"];
                chkTextFilterBotSpam = (MyClasses.MetaViewWrappers.ICheckBox)View["chkTextFilterBotSpam"];
                chkTextFilterIdentFailures = (MyClasses.MetaViewWrappers.ICheckBox)View["chkTextFilterIdentFailures"];
                chkTextFilterKillTaskComplete = (MyClasses.MetaViewWrappers.ICheckBox)View["chkTextFilterKillTaskComplete"];
                chkTextFilterVendorTells = (MyClasses.MetaViewWrappers.ICheckBox)View["chkTextFilterVendorTells"];
                chkTextFilterMonsterTells = (MyClasses.MetaViewWrappers.ICheckBox)View["chkTextFilterMonsterTells"];
                chkTextFilterNPCChatter = (MyClasses.MetaViewWrappers.ICheckBox)View["chkTextFilterNPCChatter"];


				 try
				 {
                    //WormGears Control Events
                    chkQuickSlotsv.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkQuickSlotsv_Change);
                    chkQuickSlotsh.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkQuickSlotsh_Change);
				 }catch(Exception ex){LogError(ex);}
                  try
                  {
                    //GearVisection Control Events
					chkGearVisectionEnabled.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkGearVisectionEnabled_Change);
                 }catch(Exception ex){LogError(ex);}
                  try
                  {
                    //GearSense Control Events
                    chkGearSenseEnabled.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkGearSenseEnabled_Change);
			        }catch(Exception ex){LogError(ex);}
                  try
                  {
			        //GearButler Controls
			        chkGearButlerEnabled.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkGearButlerEnabled_Change);
			      //  chkAutoRingKeys.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkAutoRingKeys_Change);
                   }catch(Exception ex){LogError(ex);}
                  try
                  { 
			        //GearInspector Controls
			        chkGearInspectorEnabled.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkGearInspectorEnabled_Change);
                   }catch(Exception ex){LogError(ex);}

                  //Gear Tactician Controls
                  try
                  {
                      chkCombatHudEnabled.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkCombatHudEnabled_Change);
                  }
                  catch (Exception ex) { LogError(ex); }

                  try
                  { 
                    //Inventory Control Section
                    
                    chkInventory.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkInventory_Change);
                    chkInventoryBurden.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkInventoryBurden_Change);
                    chkInventoryComplete.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkInventoryComplete_Change);
                    chkToonStats.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkToonStats_Change);
                   chkToonArmor.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkToonArmor_Change);

                }
                catch (Exception ex) { LogError(ex); }

                  //GearsMisc Controls
                  try
                  {
                      chkMuteSounds.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkMuteSounds_Change);
                      chkArmorHud.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkArmorHud_Change);
                  }
                  catch (Exception ex) { LogError(ex); }

                  try
                  {
                      //Text Control Section

                      chkEnableTextFiltering.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkEnableTextFiltering_Change);
                      chkTextFilterAllStatus.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkTextFilterAllStatus_Change);
                      chkTextFilterBusyStatus.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkTextFilterBusyStatus_Change);
                      chkTextFilterCastingStatus.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkTextFilterCastingStatus_Change);
                      chkTextFilterMyDefenseMessages.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkTextFilterMyDefenseMessages_Change);
                      chkTextFilterMobDefenseMessages.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkTextFilterMobDefenseMessages_Change);
                      chkTextFilterMyKillMessages.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkTextFilterMyKillMessages_Change);
                      chkTextFilterPKFails.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkTextFilterPKFails_Change);
                      chkTextFilterDirtyFighting.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkTextFilterDirtyFighting_Change);
                      chkTextFilterMySpellCasting.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkTextFilterMySpellCasting_Change);
                      chkTextFilterOthersSpellCasting.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkTextFilterOthersSpellCasting_Change);
                      chkTextFilterSpellExpirations.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkTextFilterSpellExpirations_Change);
                      chkTextFilterManaStoneMessages.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkTextFilterManaStoneMessages_Change);
                      chkTextFilterHealingMessages.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkTextFilterHealingMessages_Change);
                      chkTextFilterSalvageMessages.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkTextFilterSalvageMessages_Change);
                      chkTextFilterBotSpam.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkTextFilterBotSpam_Change);
                      chkTextFilterIdentFailures.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkTextFilterIdentFailures_Change);
                      chkTextFilterKillTaskComplete.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkTextFilterKillTaskComplete_Change);
                      chkTextFilterVendorTells.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkTextFilterVendorTells_Change);
                      chkTextFilterMonsterTells.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkTextFilterMonsterTells_Change);
                      chkTextFilterNPCChatter.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkTextFilterNPCChatter_Change);

                  }
                  catch (Exception ex) { LogError(ex); }



                  try
                  {
                //Controls on Notify.SearchRules page

                lstRules = (MyClasses.MetaViewWrappers.IList)View["lstRules"];

                btnRuleClear = (MyClasses.MetaViewWrappers.IButton)View["btnRuleClear"];
                btnRuleNew = (MyClasses.MetaViewWrappers.IButton)View["btnRuleNew"];
                btnRuleUpdate = (MyClasses.MetaViewWrappers.IButton)View["btnRuleUpdate"];
                chkRuleEnabled = (MyClasses.MetaViewWrappers.ICheckBox)View["chkRuleEnabled"];
 				}catch(Exception ex){LogError(ex);}
                  try
                  {
                lstRuleApplies = (MyClasses.MetaViewWrappers.IList)View["lstRuleApplies"];
                txtRuleName = (MyClasses.MetaViewWrappers.ITextBox)View["txtRuleName"];
                txtRuleDescr = (MyClasses.MetaViewWrappers.ITextBox)View["txtRuleDescr"];
                txtRulePriority = (MyClasses.MetaViewWrappers.ITextBox)View["txtRulePriority"];
                txtRuleArcaneLore = (MyClasses.MetaViewWrappers.ITextBox)View["txtRuleArcaneLore"];
                txtRuleMaxCraft = (MyClasses.MetaViewWrappers.ITextBox)View["txtRuleMaxCraft"];
                txtRuleKeywords = (MyClasses.MetaViewWrappers.ITextBox)View["txtRuleKeywords"];
                txtRulePrice = (MyClasses.MetaViewWrappers.ITextBox)View["txtRulePrice"];
                txtRuleMaxBurden = (MyClasses.MetaViewWrappers.ITextBox)View["txtRuleMaxBurden"];
                txtRuleKeyWordsNot = (MyClasses.MetaViewWrappers.ITextBox)View["txtRuleKeyWordsNot"];
                txtRuleWieldReqValue = (MyClasses.MetaViewWrappers.ITextBox)View["txtRuleWieldReqValue"];
                txtRuleWieldLevel = (MyClasses.MetaViewWrappers.ITextBox)View["txtRuleWieldLevel"];
                txtRuleItemLevel = (MyClasses.MetaViewWrappers.ITextBox)View["txtRuleItemLevel"];
				}catch(Exception ex){LogError(ex);}
                  try
                  {
                
                lstRules.Selected += new EventHandler<MVListSelectEventArgs>(lstRules_Selected);
                btnRuleClear.Click += new EventHandler<MyClasses.MetaViewWrappers.MVControlEventArgs>(btnRuleClear_Click);
                btnRuleNew.Click += new EventHandler<MyClasses.MetaViewWrappers.MVControlEventArgs>(btnRuleNew_Click);
                btnRuleUpdate.Click += new EventHandler<MyClasses.MetaViewWrappers.MVControlEventArgs>(btnRuleUpdate_Click);
 				}catch(Exception ex){LogError(ex);}
                  try
                  {
                chkRuleEnabled.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkRuleEnabled_Change);
				}catch(Exception ex){LogError(ex);}
                  try
                  {
                txtRulePriority.End += new EventHandler<MVTextBoxEndEventArgs>(txtRulePriority_End);
                txtRuleDescr.End += new EventHandler<MVTextBoxEndEventArgs>(txtRuleDescr_End);
                txtRuleName.End += new EventHandler<MVTextBoxEndEventArgs>(txtRuleName_End);
                txtRuleMaxCraft.End += new EventHandler<MVTextBoxEndEventArgs>(txtRuleMaxCraft_End);
                txtRuleArcaneLore.End += new EventHandler<MVTextBoxEndEventArgs>(txtRuleArcaneLore_End);
                txtRuleMaxBurden.End += new EventHandler<MVTextBoxEndEventArgs>(txtRuleMaxBurden_End);
                txtRuleWieldReqValue.End += new EventHandler<MVTextBoxEndEventArgs>(txtRuleWieldReqValue_End);
                txtRuleWieldLevel.End += new EventHandler<MVTextBoxEndEventArgs>(txtRuleWieldLevel_End);
                txtRuleItemLevel.End += new EventHandler<MVTextBoxEndEventArgs>(txtRuleItemLevel_End);
                txtRulePrice.End += new EventHandler<MVTextBoxEndEventArgs>(txtRulePrice_End);
                txtRuleKeyWordsNot.End += new EventHandler<MVTextBoxEndEventArgs>(txtRuleKeyWordsNot_End);
                txtRuleKeywords.End += new EventHandler<MVTextBoxEndEventArgs>(txtRuleKeywords_End);
				}catch(Exception ex){LogError(ex);}
                  try
                  {
                // Controls on Notify.SearchRules.Weapon Page
                cboWeaponAppliesTo = (MyClasses.MetaViewWrappers.ICombo)View["cboWeaponAppliesTo"];
                cboWeaponAppliesTo.Selected = 0;
                cboMasteryType = (MyClasses.MetaViewWrappers.ICombo)View["cboMasteryType"];
                cboMasteryType.Selected = 0;
				}catch(Exception ex){LogError(ex);}
                  try
                  {
                lstDamageTypes = (MyClasses.MetaViewWrappers.IList)View["lstDamageTypes"];

                chkRuleMSCleavea = (MyClasses.MetaViewWrappers.ICheckBox)View["chkRuleMSCleavea"];
                chkRuleMSCleaveb = (MyClasses.MetaViewWrappers.ICheckBox)View["chkRuleMSCleaveb"];
                chkRuleMSCleavec = (MyClasses.MetaViewWrappers.ICheckBox)View["chkRuleMSCleavec"];
                chkRuleMSCleaved = (MyClasses.MetaViewWrappers.ICheckBox)View["chkRuleMSCleaved"];
                chkRuleWeaponsa = (MyClasses.MetaViewWrappers.ICheckBox)View["chkRuleWeaponsa"];
                chkRuleWeaponsb = (MyClasses.MetaViewWrappers.ICheckBox)View["chkRuleWeaponsb"];
                chkRuleWeaponsc = (MyClasses.MetaViewWrappers.ICheckBox)View["chkRuleWeaponsc"];
                chkRuleWeaponsd = (MyClasses.MetaViewWrappers.ICheckBox)View["chkRuleWeaponsd"];
				}catch(Exception ex){LogError(ex);}
                  try
                  {
                txtRuleMcModAttack = (MyClasses.MetaViewWrappers.ITextBox)View["txtRuleMcModAttack"];
                txtRuleMeleeD = (MyClasses.MetaViewWrappers.ITextBox)View["txtRuleMeleeD"];
                txtRuleMagicD = (MyClasses.MetaViewWrappers.ITextBox)View["txtRuleMagicD"];
                txtRuleReqSkilla = (MyClasses.MetaViewWrappers.ITextBox)View["txtRuleReqSkilla"];
                txtRuleReqSkillb = (MyClasses.MetaViewWrappers.ITextBox)View["txtRuleReqSkillb"];
                txtRuleReqSkillc = (MyClasses.MetaViewWrappers.ITextBox)View["txtRuleReqSkillc"];
                txtRuleReqSkilld = (MyClasses.MetaViewWrappers.ITextBox)View["txtRuleReqSkilld"];
                txtRuleMinMaxa = (MyClasses.MetaViewWrappers.ITextBox)View["txtRuleMinMaxa"];
                txtRuleMinMaxb = (MyClasses.MetaViewWrappers.ITextBox)View["txtRuleMinMaxb"];
                txtRuleMinMaxc = (MyClasses.MetaViewWrappers.ITextBox)View["txtRuleMinMaxc"];
                txtRuleMinMaxd = (MyClasses.MetaViewWrappers.ITextBox)View["txtRuleMinMaxd"];
				}catch(Exception ex){LogError(ex);}
                  try
                  {
                lblRuleMinMax_ElvsMons = (MyClasses.MetaViewWrappers.IStaticText)View["lblRuleMinMax_ElvsMons"];
                lblRuleMcModAttack = (MyClasses.MetaViewWrappers.IStaticText)View["lblRuleMcModAttack"];

                cboWeaponAppliesTo.Change += new EventHandler<MyClasses.MetaViewWrappers.MVIndexChangeEventArgs>(cboWeaponAppliesTo_Change);
                cboMasteryType.Change += new EventHandler<MyClasses.MetaViewWrappers.MVIndexChangeEventArgs>(cboMasteryType_Change);
                chkRuleMSCleavea.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkRuleMSCleavea_Change);
                chkRuleMSCleaveb.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkRuleMSCleaveb_Change);
                chkRuleMSCleavec.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkRuleMSCleavec_Change);
                chkRuleMSCleaved.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkRuleMSCleaved_Change);
                chkRuleWeaponsa.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkRuleWeaponsa_Change);
                chkRuleWeaponsb.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkRuleWeaponsb_Change);
                chkRuleWeaponsc.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkRuleWeaponsc_Change);
                chkRuleWeaponsd.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkRuleWeaponsd_Change);
                lstDamageTypes.Selected += new EventHandler<MVListSelectEventArgs>(lstDamageTypes_Selected);
				}catch(Exception ex){LogError(ex);}
                  try
                  {
                txtRuleMcModAttack.End += new EventHandler<MVTextBoxEndEventArgs>(txtRuleMcModAttack_End);
                txtRuleMeleeD.End += new EventHandler<MVTextBoxEndEventArgs>(txtRuleMeleeD_End);
                txtRuleMagicD.End += new EventHandler<MVTextBoxEndEventArgs>(txtRuleMagicD_End);
                txtRuleReqSkilla.End += new EventHandler<MVTextBoxEndEventArgs>(txtRuleReqSkilla_End);
                txtRuleMinMaxa.End += new EventHandler<MVTextBoxEndEventArgs>(txtRuleMinMaxa_End);
                txtRuleReqSkillb.End += new EventHandler<MVTextBoxEndEventArgs>(txtRuleReqSkillb_End);
                txtRuleReqSkillc.End += new EventHandler<MVTextBoxEndEventArgs>(txtRuleReqSkillc_End);
                txtRuleReqSkilld.End += new EventHandler<MVTextBoxEndEventArgs>(txtRuleReqSkilld_End);
                txtRuleMinMaxb.End += new EventHandler<MVTextBoxEndEventArgs>(txtRuleMinMaxb_End);
                txtRuleMinMaxc.End += new EventHandler<MVTextBoxEndEventArgs>(txtRuleMinMaxc_End);
                txtRuleMinMaxd.End += new EventHandler<MVTextBoxEndEventArgs>(txtRuleMinMaxd_End);
  				}catch(Exception ex){LogError(ex);}
                  try
                  {

                // Controls on Notify.SearchRules.Armor Page
                lstRuleArmorCoverages = (MyClasses.MetaViewWrappers.IList)View["lstRuleArmorCoverages"];
                lstRuleArmorTypes = (MyClasses.MetaViewWrappers.IList)View["lstRuleArmorTypes"];
                txtRuleMinArmorLevel = (MyClasses.MetaViewWrappers.ITextBox)View["txtRuleMinArmorLevel"];
                chkRuleMustBeUnenchantable = (MyClasses.MetaViewWrappers.ICheckBox)View["chkRuleMustBeUnenchantable"];
                lstRuleSets = (MyClasses.MetaViewWrappers.IList)View["lstRuleSets"];
				}catch(Exception ex){LogError(ex);}
                  try
                  {
                chkRuleMustBeUnenchantable.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkRuleMustBeUnenchantable_Change);
                txtRuleMinArmorLevel.End += new EventHandler<MVTextBoxEndEventArgs>(txtRuleMinArmorLevel_End);
				}catch(Exception ex){LogError(ex);}
                  try
                  {
                // Controls on Notify.SearchRules.Cloaks/Aetheria
                lstRuleCloakSets = (MyClasses.MetaViewWrappers.IList)View["lstRuleCloakSets"];
                lstRuleCloakSpells = (MyClasses.MetaViewWrappers.IList)View["lstRuleCloakSpells"];
                txtRuleItemLevel = (MyClasses.MetaViewWrappers.ITextBox)View["txtRuleItemLevel"];
                chkRuleCloakMustHaveSpell = (MyClasses.MetaViewWrappers.ICheckBox)View["chkRuleCloakMustHaveSpell"];
                chkRuleRed = (MyClasses.MetaViewWrappers.ICheckBox)View["chkRuleRed"];
                chkRuleYellow = (MyClasses.MetaViewWrappers.ICheckBox)View["chkRuleYellow"];
                chkRuleBlue = (MyClasses.MetaViewWrappers.ICheckBox)View["chkRuleBlue"];
				}catch(Exception ex){LogError(ex);}
                  try
                  {
                chkRuleCloakMustHaveSpell.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkRuleCloakMustHaveSpell_Change);
                chkRuleRed.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkRuleRed_Change);
                chkRuleYellow.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkRuleYellow_Change);
                chkRuleBlue.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkRuleBlue_Change);
                }catch(Exception ex){LogError(ex);}

                  try
                  {

                      // Controls on Notify.SearchRules.Essences.
                      cboRuleEssMastery = (MyClasses.MetaViewWrappers.ICombo)View["cboRuleEssMastery"];
                      cboRuleEssMastery.Selected = 0;
                      lstRuleEssElements = (MyClasses.MetaViewWrappers.IList)View["lstRuleEssElements"];
                      txtRuleEssLevel = (MyClasses.MetaViewWrappers.ITextBox)View["txtRuleEssLevel"];
                  //    txtRuleEssSummLevel = (MyClasses.MetaViewWrappers.ITextBox)View["txtRuleEssSummLevel"];
                      txtRuleEssDamageLevel = (MyClasses.MetaViewWrappers.ITextBox)View["txtRuleEssDamageLevel"];
                      txtRuleEssCDLevel = (MyClasses.MetaViewWrappers.ITextBox)View["txtRuleEssCDLevel"];
                      txtRuleEssCRLevel = (MyClasses.MetaViewWrappers.ITextBox)View["txtRuleEssCRLevel"];
                      txtRuleEssDRLevel = (MyClasses.MetaViewWrappers.ITextBox)View["txtRuleEssDRLevel"];
                      txtRuleEssCritLevel = (MyClasses.MetaViewWrappers.ITextBox)View["txtRuleEssCritLevel"];
                      txtRuleEssCritDamResLevel = (MyClasses.MetaViewWrappers.ITextBox)View["txtRuleEssCritDamResLevel"];

                      cboRuleEssMastery.Change += new EventHandler<MyClasses.MetaViewWrappers.MVIndexChangeEventArgs>(cboRuleEssMastery_Change);
                      txtRuleEssLevel.End += new EventHandler<MVTextBoxEndEventArgs>(txtRuleEssLevel_End);
                  //    txtRuleEssSummLevel.End += new EventHandler<MVTextBoxEndEventArgs>(txtRuleEssSummLevel_End);
                      txtRuleEssDamageLevel.End += new EventHandler<MVTextBoxEndEventArgs>(txtRuleEssDamageLevel_End);
                      txtRuleEssCDLevel.End += new EventHandler<MVTextBoxEndEventArgs>(txtRuleEssCDLevel_End);
                      txtRuleEssCRLevel.End += new EventHandler<MVTextBoxEndEventArgs>(txtRuleEssCRLevel_End);
                      txtRuleEssDRLevel.End += new EventHandler<MVTextBoxEndEventArgs>(txtRuleEssDRLevel_End);
                      txtRuleEssCritLevel.End += new EventHandler<MVTextBoxEndEventArgs>(txtRuleEssCritLevel_End);
                      txtRuleEssCritDamResLevel.End += new EventHandler<MVTextBoxEndEventArgs>(txtRuleEssCritDamResLevel_End);

                  }
                  catch (Exception ex) { LogError(ex); }

                  try
                  {
                // Controls on Notify.SearchRules.Req Spells
                lstRuleSpells = (MyClasses.MetaViewWrappers.IList)View["lstRuleSpells"];
                lstRuleSpellsEnabled = (MyClasses.MetaViewWrappers.IList)View["lstRuleSpellsEnabled"];
              //  lstRuleCloakSpells = (MyClasses.MetaViewWrappers.IList)View["lstRuleCloakSpells"];
                chkRuleFilterLegend = (MyClasses.MetaViewWrappers.ICheckBox)View["chkRuleFilterLegend"];
                chkRuleFilterLegend.Checked = bRuleFilterLegend;
                chkRuleFilterEpic = (MyClasses.MetaViewWrappers.ICheckBox)View["chkRuleFilterEpic"];
                chkRuleFilterEpic.Checked = bRuleFilterEpic;
                chkRuleFilterMajor = (MyClasses.MetaViewWrappers.ICheckBox)View["chkRuleFilterMajor"];
                chkRuleFilterMajor.Checked = bRuleFilterMajor;
                chkRuleFilterlvl8 = (MyClasses.MetaViewWrappers.ICheckBox)View["chkRuleFilterlvl8"];
                  chkRuleFilterlvl8.Checked = bRuleFilterlvl8;
                  chkRuleFilterlvl7 = (MyClasses.MetaViewWrappers.ICheckBox)View["chkRuleFilterlvl7"];
                chkRuleFilterlvl7.Checked = bRuleFilterlvl7;
                chkRuleFilterlvl6 = (MyClasses.MetaViewWrappers.ICheckBox)View["chkRuleFilterlvl6"];
                chkRuleFilterlvl6.Checked = bRuleFilterlvl6;
                txtRuleSpellMatches = (MyClasses.MetaViewWrappers.ITextBox)View["txtRuleSpellMatches"];
                txtRuleNumSpells = (MyClasses.MetaViewWrappers.ITextBox)View["txtRuleNumSpells"];
       			}catch(Exception ex){LogError(ex);}
                  try
                  {
                lstRuleSpells.Selected += new EventHandler<MVListSelectEventArgs>(lstRuleSpells_Selected); 
                lstRuleSpellsEnabled.Selected += new EventHandler<MVListSelectEventArgs>(lstRuleSpellsEnabled_Selected); 
                //lstRuleCloakSpells.Selected += new EventHandler<MVListSelectEventArgs>(lstRuleCloakSpells_Selected);
                chkRuleFilterLegend.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkRuleFilterLegend_Change);
                chkRuleFilterEpic.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkRuleFilterEpic_Change);
                chkRuleFilterMajor.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkRuleFilterMajor_Change);
                chkRuleFilterlvl8.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkRuleFilterlvl8_Change);
                chkRuleFilterlvl7.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkRuleFilterlvl7_Change);
                chkRuleFilterlvl6.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkRuleFilterlvl6_Change);
                txtRuleSpellMatches.End += new EventHandler<MVTextBoxEndEventArgs>(txtRuleSpellMatches_End);
                txtRuleNumSpells.End += new EventHandler<MVTextBoxEndEventArgs>(txtRuleNumSpells_End);
				}catch(Exception ex){LogError(ex);}
                  try
                  {
   
                // Controls on Notify.NPC/Trophies Page

                btnAttachTrophyItem = (MyClasses.MetaViewWrappers.IButton)View["btnAttachTrophyItem"];
                btnAddTrophyItem = (MyClasses.MetaViewWrappers.IButton)View["btnAddTrophyItem"];
                lstmyTrophies = (MyClasses.MetaViewWrappers.IList)View["lstmyTrophies"];
                txtTrophyName = (MyClasses.MetaViewWrappers.ITextBox)View["txtTrophyName"];
                //lblAtr11 = (MyClasses.MetaViewWrappers.IStaticText)View["lblAtr11"];
                //cboTrophysetupAlert = (MyClasses.MetaViewWrappers.ICombo)View["cboTrophysetupAlert"];
                //cboTrophysetupAlert.Selected = 0;
                chkTrophyExact = (MyClasses.MetaViewWrappers.ICheckBox)View["chkTrophyExact"];
                //lblMyItemsCountMax = (MyClasses.MetaViewWrappers.IStaticText)View["lblMyItemsCountMax"];
                txtTrophyMax = (MyClasses.MetaViewWrappers.ITextBox)View["txtTrophyMax"];
                lstmyTrophies.Selected += new EventHandler<MVListSelectEventArgs>(lstmyTrophies_Selected);
                chkTrophyExact.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkTrophyExact_Change);
                //  // cboTrophysetupAlert.Change += new EventHandler<MyClasses.MetaViewWrappers.MVIndexChangeEventArgs>(cboTrophysetupAlert_Change);
                txtTrophyName.End += new EventHandler<MVTextBoxEndEventArgs>(txtTrophyName_End);
                txtTrophyMax.End += new EventHandler<MVTextBoxEndEventArgs>(txtTrophyMax_End);
                btnAttachTrophyItem.Click += new EventHandler<MyClasses.MetaViewWrappers.MVControlEventArgs>(btnAttachTrophyItem_Click);
                btnAddTrophyItem.Click += new EventHandler<MyClasses.MetaViewWrappers.MVControlEventArgs>(btnAddTrophyItem_Click);
                }catch(Exception ex){LogError(ex);}
                  try
                  {
                
                
                // Controls on Notify.Mobs Page

                lstmyMobs = (MyClasses.MetaViewWrappers.IList)View["lstmyMobs"];
                //  cboMobsetupAlert = (MyClasses.MetaViewWrappers.ICombo)View["cboMobsetupAlert"];
                //  cboMobsetupAlert.Selected = 0;
                btnAddMobItem = (MyClasses.MetaViewWrappers.IButton)View["btnAddMobItem"];
                chkmyMobExact = (MyClasses.MetaViewWrappers.ICheckBox)View["chkmyMobExact"];
                txtmyMobName = (MyClasses.MetaViewWrappers.ITextBox)View["txtmyMobName"];
                // lblatr121 = (MyClasses.MetaViewWrappers.IStaticText)View["lblAtr121"];
				}catch(Exception ex){LogError(ex);}
                  try
                  {
                lstmyMobs.Selected += new EventHandler<MVListSelectEventArgs>(lstmyMobs_Selected);
                chkmyMobExact.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkmyMobExact_Change);
                //// cboMobsetupAlert.Change += new EventHandler<MyClasses.MetaViewWrappers.MVIndexChangeEventArgs>(cboMobsetupAlert_Change);
                txtmyMobName.End += new EventHandler<MVTextBoxEndEventArgs>(txtmyMobName_End);
                btnAddMobItem.Click += new EventHandler<MyClasses.MetaViewWrappers.MVControlEventArgs>(btnAddMobItem_Click);
				}catch(Exception ex){LogError(ex);}
                  try
                  {
                // Controls on Notify.Salvage Page
                lstNotifySalvage = (MyClasses.MetaViewWrappers.IList)View["lstNotifySalvage"];
                txtSalvageName = (MyClasses.MetaViewWrappers.ITextBox)View["txtSalvageName"];
                txtSalvageString = (MyClasses.MetaViewWrappers.ITextBox)View["txtSalvageString"];
                btnUpdateSalvage = (MyClasses.MetaViewWrappers.IButton)View["btnUpdateSalvage"];
                lblSalvageName = (MyClasses.MetaViewWrappers.IStaticText)View["lblSalvageName"];
                lblSalvageString = (MyClasses.MetaViewWrappers.IStaticText)View["lblSalvageString"];
				}catch(Exception ex){LogError(ex);}
                  try
                  {
                lstNotifySalvage.Selected += new EventHandler<MVListSelectEventArgs>(lstNotifySalvage_Selected);
                btnUpdateSalvage.Click += new EventHandler<MyClasses.MetaViewWrappers.MVControlEventArgs>(btnUpdateSalvage_Click);
                txtSalvageName.End += new EventHandler<MVTextBoxEndEventArgs>(txtSalvageName_End);
                txtSalvageString.End += new EventHandler<MVTextBoxEndEventArgs>(txtSalvageString_End);
                }catch(Exception ex){LogError(ex);}
                  try
                  {
                 //Controls on Notify.Other Page
               lstNotifyOptions = (MyClasses.MetaViewWrappers.IList)View["lstNotifyOptions"];
               chkSalvageAll = (MyClasses.MetaViewWrappers.ICheckBox)View["chkmyMobExact"];
               txtMaxMana = (MyClasses.MetaViewWrappers.ITextBox)View["txtMaxMana"];
               txtMaxValue = (MyClasses.MetaViewWrappers.ITextBox)View["txtMaxValue"];
               txtVbratio = (MyClasses.MetaViewWrappers.ITextBox)View["txtVbratio"];
               //lblMaxMana = (MyClasses.MetaViewWrappers.IStaticText)View["lblMaxMana"];
               lblMaxValue = (MyClasses.MetaViewWrappers.IStaticText)View["lblMaxValue"];
               lblVbratio = (MyClasses.MetaViewWrappers.IStaticText)View["lblVbratio"];
				}catch(Exception ex){LogError(ex);}
                  try
                  {
             //  TODO: Add the functions in when obvious where would go and what should be done
              // lstNotifyOptions.Selected += new EventHandler<MVListSelectEventArgs>(lstNotifyOptions_Selected);
               txtMaxMana.End += new EventHandler<MVTextBoxEndEventArgs>(txtMaxMana_End);
               txtMaxValue.End += new EventHandler<MVTextBoxEndEventArgs>(txtMaxValue_End);
				}catch(Exception ex){LogError(ex);}
                  try
                  {
               // Controls on Ust Page
               lstUstList = (MyClasses.MetaViewWrappers.IList)View["lstUstList"];
               btnUstItems = (MyClasses.MetaViewWrappers.IButton)View["btnUstItems"];
               btnUstClear = (MyClasses.MetaViewWrappers.IButton)View["btnUstClear"];
               btnUstSalvage = (MyClasses.MetaViewWrappers.IButton)View["btnUstSalvage"];
               txtSalvageAug = (MyClasses.MetaViewWrappers.ITextBox)View["txtSalvageAug"];
               //lblUst002 = (MyClasses.MetaViewWrappers.IStaticText)View["lblUst002"];
				}catch(Exception ex){LogError(ex);}
                  try
                  {
                ////lstUstList.Selected += new EventHandler<MVListSelectEventArgs>(lstUstList_Selected);
                //btnUstItems.Click += new EventHandler<MyClasses.MetaViewWrappers.MVControlEventArgs>(btnUstItems_Click);
                //btnUstClear.Click += new EventHandler<MyClasses.MetaViewWrappers.MVControlEventArgs>(btnUstClear_Click);
                //btnUstSalvage.Click += new EventHandler<MyClasses.MetaViewWrappers.MVControlEventArgs>(btnUstSalvage_Click);

                // Controls on Alerts Page
                // Controls on Inventory Page
 
                // Controls on Settings Page
                //lstOtherOptions = (MyClasses.MetaViewWrappers.IList)View["lstOtherOptions"];
                //txtSettingsCW = (MyClasses.MetaViewWrappers.ITextBox)View["txtSettingsCW"];
                ////lblSettings001 = (MyClasses.MetaViewWrappers.IStaticText)View["lblSettings001"];

                //lstOtherOptions.Selected += new EventHandler<MVListSelectEventArgs>(lstOtherOptions_Selected);
  

            

                //Controls on Inventory Page
                btnGetInventory = (MyClasses.MetaViewWrappers.IButton)View["btnGetInventory"];
                btnUpdateInventory = (MyClasses.MetaViewWrappers.IButton)View["btnUpdateInventory"];
                btnGetBurden = (MyClasses.MetaViewWrappers.IButton)View["btnGetBurden"];
                btnItemsWaiting = (MyClasses.MetaViewWrappers.IButton)View["btnItemsWaiting"];
                //      btnGetToonArmor = (MyClasses.MetaViewWrappers.IButton)View["btnGetToonArmor"];
                btnLstInventory = (MyClasses.MetaViewWrappers.IButton)View["btnLstInventory"];
                btnClrInventory = (MyClasses.MetaViewWrappers.IButton)View["btnClrInventory"];
                }catch(Exception ex){LogError(ex);}
                  try
                  {
                cmbSelectClass = (MyClasses.MetaViewWrappers.ICombo)View["cmbSelectClass"];
                cmbSelectClass.Selected = 0;
                cmbWieldAttrib = (MyClasses.MetaViewWrappers.ICombo)View["cmbWieldAttrib"];
                cmbWieldAttrib.Selected = 0;
                cmbDamageType = (MyClasses.MetaViewWrappers.ICombo)View["cmbDamageType"];
                cmbDamageType.Selected = 0;
                cmbLevel = (MyClasses.MetaViewWrappers.ICombo)View["cmbLevel"];
                cmbLevel.Selected = 0;
                cmbArmorSet = (MyClasses.MetaViewWrappers.ICombo)View["cmbArmorSet"];
                cmbArmorSet.Selected = 0;
                cmbMaterial = (MyClasses.MetaViewWrappers.ICombo)View["cmbMaterial"];
                cmbMaterial.Selected = 0;
                cmbCoverage = (MyClasses.MetaViewWrappers.ICombo)View["cmbCoverage"];
                cmbCoverage.Selected = 0;
                cmbArmorLevel = (MyClasses.MetaViewWrappers.ICombo)View["cmbArmorLevel"];
                cmbArmorLevel.Selected = 0;
                cmbSalvWork = (MyClasses.MetaViewWrappers.ICombo)View["cmbSalvWork"];
                cmbSalvWork.Selected = 0;
                cmbEmbue = (MyClasses.MetaViewWrappers.ICombo)View["cmbEmbue"];
                cmbEmbue.Selected = 0;
				}catch(Exception ex){LogError(ex);}
                  try
                  {
                
                lstInventory = (MyClasses.MetaViewWrappers.IList)View["lstInventory"];

                txbSelect = (MyClasses.MetaViewWrappers.ITextBox)View["txbSelect"];

                lstInventory.Selected += new EventHandler<MVListSelectEventArgs>(lstInventory_Selected);
                btnGetInventory.Click += new EventHandler<MyClasses.MetaViewWrappers.MVControlEventArgs>(btnGetInventory_Click);
                btnUpdateInventory.Click += new EventHandler<MyClasses.MetaViewWrappers.MVControlEventArgs>(btnUpdateInventory_Click);
                btnGetBurden.Click += new EventHandler<MyClasses.MetaViewWrappers.MVControlEventArgs>(btnGetBurden_Click);
                btnItemsWaiting.Click += new EventHandler<MyClasses.MetaViewWrappers.MVControlEventArgs>(btnItemsWaiting_Click);
                ////      btnGetToonArmor.Click += new EventHandler<MyClasses.MetaViewWrappers.MVControlEventArgs>(btnGetToonArmor_Click);
                btnLstInventory.Click += new EventHandler<MyClasses.MetaViewWrappers.MVControlEventArgs>(btnLstInventory_Click);
                btnClrInventory.Click += new EventHandler<MyClasses.MetaViewWrappers.MVControlEventArgs>(btnClrInventory_Click);
                cmbSelectClass.Change += new EventHandler<MyClasses.MetaViewWrappers.MVIndexChangeEventArgs>(cmbSelectClass_Change);
                cmbSelectClass.Selected = 0;
                cmbWieldAttrib.Change += new EventHandler<MyClasses.MetaViewWrappers.MVIndexChangeEventArgs>(cmbWieldAttrib_Change);
                cmbDamageType.Change += new EventHandler<MyClasses.MetaViewWrappers.MVIndexChangeEventArgs>(cmbDamageType_Change);
                cmbLevel.Change += new EventHandler<MyClasses.MetaViewWrappers.MVIndexChangeEventArgs>(cmbLevel_Change);
                cmbMaterial.Change += new EventHandler<MyClasses.MetaViewWrappers.MVIndexChangeEventArgs>(cmbMaterial_Change);
                cmbArmorSet.Change += new EventHandler<MyClasses.MetaViewWrappers.MVIndexChangeEventArgs>(cmbArmorSet_Change);
                cmbArmorLevel.Change += new EventHandler<MyClasses.MetaViewWrappers.MVIndexChangeEventArgs>(cmbArmorLevel_Change);
                cmbCoverage.Change += new EventHandler<MyClasses.MetaViewWrappers.MVIndexChangeEventArgs>(cmbCoverage_Change);
                cmbSalvWork.Change += new EventHandler<MyClasses.MetaViewWrappers.MVIndexChangeEventArgs>(cmbSalvWork_Change);
                cmbEmbue.Change += new EventHandler<MyClasses.MetaViewWrappers.MVIndexChangeEventArgs>(cmbEmbue_Change);
       }

           catch (Exception ex) {LogError(ex);	}
       }catch(Exception ex){LogError(ex);}

      
					 


            }
	
}
	}

        #endregion