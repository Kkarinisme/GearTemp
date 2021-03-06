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
////  THE SOFTWARE.
/////////////////////////////////////////////////////////////////////////////////

////This entire file is being used -- Karin 04/16/13

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
        
        //MyClasses.MetaViewWrappers.IView View;
        //MyClasses.MetaViewWrappers.INotebook nbSetupsetup;
//        // Controls on Setup Page
//        // WormGears controls
//        MyClasses.MetaViewWrappers.ICheckBox chkQuickSlotsv;
//        MyClasses.MetaViewWrappers.ICheckBox chkQuickSlotsh;
        
//        //GearVisection Controls
//        MyClasses.MetaViewWrappers.ICheckBox chkGearVisectionEnabled;
        
//        //GearSense Controls
//        MyClasses.MetaViewWrappers.ICheckBox chkGearSenseEnabled;
        
//        //GearButler Controls
//        MyClasses.MetaViewWrappers.ICheckBox chkGearButlerEnabled;  
        
//        //GearInspector Controls
//        MyClasses.MetaViewWrappers.ICheckBox chkGearInspectorEnabled;

//        //Gears Tactician
//        MyClasses.MetaViewWrappers.ICheckBox chkCombatHudEnabled;

//        //Remote Gear
//        MyClasses.MetaViewWrappers.ICheckBox chkRemoteGearEnabled;

//        //Portal Gear
//        MyClasses.MetaViewWrappers.ICheckBox chkPortalGearEnabled;

//        //KillTask Gear
//        MyClasses.MetaViewWrappers.ICheckBox chkKillTaskGearEnabled;


//        //Gear
//        MyClasses.MetaViewWrappers.ICheckBox chkInventoryHudEnabled;
//        MyClasses.MetaViewWrappers.ICheckBox chkInventoryBurden;
//        MyClasses.MetaViewWrappers.ICheckBox chkInventoryComplete;
//        MyClasses.MetaViewWrappers.ICheckBox chkInventory;
//        MyClasses.MetaViewWrappers.ICheckBox chkToonStats;

//        //Gears Misc
//        MyClasses.MetaViewWrappers.ICheckBox chkArmorHud;
        
        
//        MyClasses.MetaViewWrappers.ICheckBox chkEnableTextFiltering;
//        MyClasses.MetaViewWrappers.ICheckBox chkTextFilterAllStatus;
//        MyClasses.MetaViewWrappers.ITextBox txtItemFontHeight;
//        MyClasses.MetaViewWrappers.ITextBox txtMenuFontHeight;
        
//        // Sounds
//        MyClasses.MetaViewWrappers.ICheckBox chkMuteSounds;
//        MyClasses.MetaViewWrappers.ICombo cboTrophyLandscape;
//        MyClasses.MetaViewWrappers.ICombo cboMobLandscape;
//        MyClasses.MetaViewWrappers.ICombo cboPlayerLandscape;
//        MyClasses.MetaViewWrappers.ICombo cboCorpseRare;
//        MyClasses.MetaViewWrappers.ICombo cboCorpseSelfKill;
//        MyClasses.MetaViewWrappers.ICombo cboCorpseFellowKill;
//        MyClasses.MetaViewWrappers.ICombo cboDeadMe;
//        MyClasses.MetaViewWrappers.ICombo cboDeadPermitted;
//        MyClasses.MetaViewWrappers.ICombo cboTrophyCorpse;
//        MyClasses.MetaViewWrappers.ICombo cboRuleCorpse;
//        MyClasses.MetaViewWrappers.ICombo cboSalvageCorpse;


//        // Controls on Notify.SearchRules Page
//        MyClasses.MetaViewWrappers.IButton btnRuleClear;
//        MyClasses.MetaViewWrappers.IButton btnRuleNew;
//        MyClasses.MetaViewWrappers.IButton btnRuleClone;
//        MyClasses.MetaViewWrappers.IButton btnRuleUpdate;
//        MyClasses.MetaViewWrappers.IList lstRules;


//        // Controls on Notify.SearchRules.Main Page
//        MyClasses.MetaViewWrappers.ICheckBox chkRuleEnabled;
//        MyClasses.MetaViewWrappers.IList lstRuleApplies;
//        MyClasses.MetaViewWrappers.IList lstRuleSlots;

//        MyClasses.MetaViewWrappers.ITextBox txtRuleName;

//        MyClasses.MetaViewWrappers.ITextBox txtRulePriority;
//        MyClasses.MetaViewWrappers.ITextBox txtRuleMaxCraft;
//        MyClasses.MetaViewWrappers.ITextBox txtGearScore;
        

//        MyClasses.MetaViewWrappers.ITextBox txtRuleArcaneLore;

//        MyClasses.MetaViewWrappers.ITextBox txtRuleWieldLevel;
 
//        // Controls on Notify.SearchRules.Weapon Page
//        MyClasses.MetaViewWrappers.ICombo cboWeaponAppliesTo;
//        MyClasses.MetaViewWrappers.ICombo cboMasteryType;
//        MyClasses.MetaViewWrappers.IList lstDamageTypes;

//        MyClasses.MetaViewWrappers.ICheckBox chkRuleWeaponsb;
//        MyClasses.MetaViewWrappers.ICheckBox chkRuleWeaponsa;
//        MyClasses.MetaViewWrappers.ICheckBox chkRuleWeaponsc;
//        MyClasses.MetaViewWrappers.ICheckBox chkRuleWeaponsd;
//        MyClasses.MetaViewWrappers.ITextBox txtRuleReqSkilla;
//        MyClasses.MetaViewWrappers.IStaticText lblRuleReqSkilla;       
//        MyClasses.MetaViewWrappers.ITextBox txtRuleReqSkillb;       
//        MyClasses.MetaViewWrappers.ITextBox txtRuleReqSkillc;       
//        MyClasses.MetaViewWrappers.ITextBox txtRuleReqSkilld;
//        MyClasses.MetaViewWrappers.IList lstRuleArmorTypes;
//        MyClasses.MetaViewWrappers.IList lstRuleSets;


//        // Controls on Notify.SearchRules.Req Spells

//        MyClasses.MetaViewWrappers.IList lstRuleSpells;
//        MyClasses.MetaViewWrappers.IList lstRuleSpellsEnabled;
//        MyClasses.MetaViewWrappers.ICheckBox chkRuleFilterLegend;
//        MyClasses.MetaViewWrappers.ICheckBox chkRuleFilterEpic;
//        MyClasses.MetaViewWrappers.ICheckBox chkRuleFilterMajor;
//        MyClasses.MetaViewWrappers.ICheckBox chkRuleFilterlvl8;
//        MyClasses.MetaViewWrappers.ICheckBox chkRuleFilterCloak;
//        MyClasses.MetaViewWrappers.ITextBox txtRuleNumSpells;
        
        
//        //Controls on Advanced Tab
        
//        MyClasses.MetaViewWrappers.ICheckBox chkAdvEnabled;
//        MyClasses.MetaViewWrappers.ICombo cboAdv1KeyType;
//        MyClasses.MetaViewWrappers.ICombo cboAdv1Key;
//         MyClasses.MetaViewWrappers.ICombo cboAdv1KeyCompare;
//        MyClasses.MetaViewWrappers.ITextBox txtAdv1KeyValue;
        
//        MyClasses.MetaViewWrappers.ICombo cboAdv1Link;
        
        
//        MyClasses.MetaViewWrappers.ICombo cboAdv2KeyType;
//        MyClasses.MetaViewWrappers.ICombo cboAdv2Key;
//         MyClasses.MetaViewWrappers.ICombo cboAdv2KeyCompare;
//        MyClasses.MetaViewWrappers.ITextBox txtAdv2KeyValue;
        
//        MyClasses.MetaViewWrappers.ICombo cboAdv2Link;
        
        
//        MyClasses.MetaViewWrappers.ICombo cboAdv3KeyType;
//        MyClasses.MetaViewWrappers.ICombo cboAdv3Key;
//         MyClasses.MetaViewWrappers.ICombo cboAdv3KeyCompare;
//        MyClasses.MetaViewWrappers.ITextBox txtAdv3KeyValue;
        
//        MyClasses.MetaViewWrappers.ICombo cboAdv3Link;
        
        
//        MyClasses.MetaViewWrappers.ICombo cboAdv4KeyType;
//        MyClasses.MetaViewWrappers.ICombo cboAdv4Key;
//         MyClasses.MetaViewWrappers.ICombo cboAdv4KeyCompare;
//        MyClasses.MetaViewWrappers.ITextBox txtAdv4KeyValue;
        
//        MyClasses.MetaViewWrappers.ICombo cboAdv4Link;
        
//        MyClasses.MetaViewWrappers.ICombo cboAdv5KeyType;
//        MyClasses.MetaViewWrappers.ICombo cboAdv5Key;
//         MyClasses.MetaViewWrappers.ICombo cboAdv5KeyCompare;
//        MyClasses.MetaViewWrappers.ITextBox txtAdv5KeyValue;
        

        

//        // Controls on Notify.NPC/Trophies Page

//        MyClasses.MetaViewWrappers.IList lstmyTrophies;
//        MyClasses.MetaViewWrappers.ITextBox txtTrophyName;
//        MyClasses.MetaViewWrappers.IButton btnAddTrophyItem;
//        MyClasses.MetaViewWrappers.IButton btnUpdateTrophyItem;
//        MyClasses.MetaViewWrappers.ICheckBox chkTrophyExact;
//        MyClasses.MetaViewWrappers.ITextBox txtTrophyMax;

//        // Controls on Notify.Mobs Page
//        MyClasses.MetaViewWrappers.IList lstmyMobs;
//       MyClasses.MetaViewWrappers.IButton btnAddMobItem;
//       MyClasses.MetaViewWrappers.IButton btnUpdateMobItem;
//       MyClasses.MetaViewWrappers.ICheckBox chkmyMobExact;
//        MyClasses.MetaViewWrappers.ITextBox txtmyMobName;

//        // Controls on Notify.Salvage Page
//        MyClasses.MetaViewWrappers.IList lstNotifySalvage;
//        MyClasses.MetaViewWrappers.IButton btnUpdateSalvage;
//        MyClasses.MetaViewWrappers.IStaticText lblSalvageName;
//        MyClasses.MetaViewWrappers.ITextBox txtSalvageString;
//        MyClasses.MetaViewWrappers.IStaticText lblSalvageString;




       //void ViewInit()
       // {
       //     try
       //     {

       //         //Create view here
            //    View = MyClasses.MetaViewWrappers.ViewSystemSelector.CreateViewResource(PluginCore.host, "MainView.xml");
            //    nbSetupsetup = (MyClasses.MetaViewWrappers.INotebook)View["nbSetupsetup"];
            //    nbSetupsetup.Change +=new EventHandler<MVIndexChangeEventArgs>(nbSetupsetup_Change);

//                //Controls on Setup Page
//                try
//                {
//                    //WormGears Controls
//                    chkQuickSlotsv = (MyClasses.MetaViewWrappers.ICheckBox)View["chkQuickSlotsv"];
//                    chkQuickSlotsh = (MyClasses.MetaViewWrappers.ICheckBox)View["chkQuickSlotsh"];
//                }catch(Exception ex){LogError(ex);}
                
//                try
//                {
//                    //GearVisection Controls
//                    chkGearVisectionEnabled = (MyClasses.MetaViewWrappers.ICheckBox)View["chkGearVisectionEnabled"];
//                 }catch(Exception ex){LogError(ex);}   
//                 try
//                 {
//                    //GearSense Controls
//                    chkGearSenseEnabled = (MyClasses.MetaViewWrappers.ICheckBox)View["chkGearSenseEnabled"];
//                }catch(Exception ex){LogError(ex);}
//                try
//                {
//                    //GearButler Controls
//                    chkGearButlerEnabled =(MyClasses.MetaViewWrappers.ICheckBox)View["chkGearButlerEnabled"];
//                    chkGearInspectorEnabled = (MyClasses.MetaViewWrappers.ICheckBox)View["chkGearInspectorEnabled"];
//                }catch(Exception ex){LogError(ex);}
//                try
//                {

//                    chkInventory = (MyClasses.MetaViewWrappers.ICheckBox)View["chkInventory"];
//                    chkInventoryHudEnabled = (MyClasses.MetaViewWrappers.ICheckBox)View["chkInventoryHudEnabled"];
//                    chkInventoryBurden = (MyClasses.MetaViewWrappers.ICheckBox)View["chkInventoryBurden"];
//                   chkInventoryComplete = (MyClasses.MetaViewWrappers.ICheckBox)View["chkInventoryComplete"];
//                   chkToonStats = (MyClasses.MetaViewWrappers.ICheckBox)View["chkToonStats"];
//                }catch(Exception ex){LogError(ex);}

//                //Gears Tactician page
//                chkCombatHudEnabled = (MyClasses.MetaViewWrappers.ICheckBox)View["chkCombatHudEnabled"];

//                //Remote Gear
//                chkRemoteGearEnabled = (MyClasses.MetaViewWrappers.ICheckBox)View["chkRemoteGearEnabled"];

//                //Portal Gear
//                chkPortalGearEnabled = (MyClasses.MetaViewWrappers.ICheckBox)View["chkPortalGearEnabled"];

//                //KillTaskGear
//                chkKillTaskGearEnabled = (MyClasses.MetaViewWrappers.ICheckBox)View["chkKillTaskGearEnabled"];


//                //Misc Gears

//                chkArmorHud = (MyClasses.MetaViewWrappers.ICheckBox)View["chkArmorHud"];
 
//                //Text Filtering Controls
//                chkEnableTextFiltering = (MyClasses.MetaViewWrappers.ICheckBox)View["chkEnableTextFiltering"];
//                chkTextFilterAllStatus = (MyClasses.MetaViewWrappers.ICheckBox)View["chkTextFilterAllStatus"];
//                txtItemFontHeight = (MyClasses.MetaViewWrappers.ITextBox)View["txtItemFontHeight"];
//                txtMenuFontHeight = (MyClasses.MetaViewWrappers.ITextBox)View["txtMenuFontHeight"];
                
//                //Sounds
//                chkMuteSounds = (MyClasses.MetaViewWrappers.ICheckBox)View["chkMuteSounds"];
//                cboTrophyLandscape = (MyClasses.MetaViewWrappers.ICombo)View["cboTrophyLandscape"];
//                cboMobLandscape = (MyClasses.MetaViewWrappers.ICombo)View["cboMobLandscape"];
//                cboPlayerLandscape = (MyClasses.MetaViewWrappers.ICombo)View["cboPlayerLandscape"];
//                cboCorpseRare = (MyClasses.MetaViewWrappers.ICombo)View["cboCorpseRare"];
//                cboCorpseSelfKill = (MyClasses.MetaViewWrappers.ICombo)View["cboCorpseSelfKill"];
//                cboCorpseFellowKill = (MyClasses.MetaViewWrappers.ICombo)View["cboCorpseFellowKill"];
//                cboDeadMe = (MyClasses.MetaViewWrappers.ICombo)View["cboDeadMe"];
//                cboDeadPermitted = (MyClasses.MetaViewWrappers.ICombo)View["cboDeadPermitted"];
//                cboTrophyCorpse = (MyClasses.MetaViewWrappers.ICombo)View["cboTrophyCorpse"];
//                cboRuleCorpse = (MyClasses.MetaViewWrappers.ICombo)View["cboRuleCorpse"];
//                cboSalvageCorpse = (MyClasses.MetaViewWrappers.ICombo)View["cboSalvageCorpse"];


//                 try
//                 {
//                    //WormGears Control Events
//                    chkQuickSlotsv.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkQuickSlotsv_Change);
//                    chkQuickSlotsh.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkQuickSlotsh_Change);
//                 }catch(Exception ex){LogError(ex);}
//                  try
//                  {
//                    //GearVisection Control Events
//                    chkGearVisectionEnabled.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkGearVisectionEnabled_Change);
//                 }catch(Exception ex){LogError(ex);}
//                  try
//                  {
//                    //GearSense Control Events
//                    chkGearSenseEnabled.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkGearSenseEnabled_Change);
//                    }catch(Exception ex){LogError(ex);}
//                  try
//                  {
//                    //GearButler Controls
//                    chkGearButlerEnabled.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkGearButlerEnabled_Change);
//                   }catch(Exception ex){LogError(ex);}
//                  try
//                  { 
//                    //GearInspector Controls
//                    chkGearInspectorEnabled.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkGearInspectorEnabled_Change);
//                   }catch(Exception ex){LogError(ex);}

//                  //Gear Tactician Controls
//                  try
//                  {
//                      chkCombatHudEnabled.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkCombatHudEnabled_Change);
//                  }
//                  catch (Exception ex) { LogError(ex); }


//                  try
//                  {
//                      chkRemoteGearEnabled.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkRemoteGearEnabled_Change);
//                  }
//                  catch (Exception ex) { LogError(ex); }

//                  try
//                  {
//                      chkPortalGearEnabled.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkPortalGearEnabled_Change);
//                  }
//                  catch (Exception ex) { LogError(ex); }

//                  try
//                  {
//                      chkKillTaskGearEnabled.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkKillTaskGearEnabled_Change);
//                  }
//                  catch (Exception ex) { LogError(ex); }
//                  try
//                  { 
//                    //Inventory Control Section

//                      chkInventoryHudEnabled.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkInventoryHudEnabled_Change);
//                      chkInventory.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkInventory_Change);
//                      chkInventoryBurden.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkInventoryBurden_Change);
//                    chkInventoryComplete.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkInventoryComplete_Change);
//                    chkToonStats.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkToonStats_Change);
 
//                }
//                catch (Exception ex) { LogError(ex); }

//                  //GearsMisc Controls
//                  try
//                  {

//                      chkArmorHud.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkArmorHud_Change);
                      
//                      //Sounds
//                        chkMuteSounds.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkMuteSounds_Change);
//                        cboTrophyLandscape.Change += cboTrophyLandscape_Change;
//                        cboMobLandscape.Change += cboMobLandscape_Change;
//                        cboPlayerLandscape.Change += cboPlayerLandscape_Change;
//                        cboCorpseRare.Change += cboCorpseRare_Change;
//                        cboCorpseSelfKill.Change += cboCorpseSelfKill_Change;
//                        cboCorpseFellowKill.Change += cboCorpseFellowKill_Change;
//                        cboDeadMe.Change += cboDeadMe_Change;
//                        cboDeadPermitted.Change += cboDeadPermitted_Change;
//                        cboTrophyCorpse.Change += cboTrophyCorpse_Change;
//                        cboRuleCorpse.Change += cboRuleCorpse_Change;
//                        cboSalvageCorpse.Change += cboSalvageCorpse_Change;
                      
//                  }
//                  catch (Exception ex) { LogError(ex); }

//                  try
//                  {
//                      //Text Control Section

//                      chkEnableTextFiltering.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkEnableTextFiltering_Change);
//                      chkTextFilterAllStatus.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkTextFilterAllStatus_Change);
//                      txtItemFontHeight.End += new EventHandler<MVTextBoxEndEventArgs>(txtItemFontHeight_End);
//                      txtMenuFontHeight.End += new EventHandler<MVTextBoxEndEventArgs>(txtMenuFontHeight_End);

//                  }
//                  catch (Exception ex) { LogError(ex); }



//                  try
//                  {
//                //Controls on Notify.SearchRules page

//                lstRules = (MyClasses.MetaViewWrappers.IList)View["lstRules"];

//                btnRuleClear = (MyClasses.MetaViewWrappers.IButton)View["btnRuleClear"];
//                btnRuleNew = (MyClasses.MetaViewWrappers.IButton)View["btnRuleNew"];
//                btnRuleClone = (MyClasses.MetaViewWrappers.IButton)View["btnRuleClone"];
//                btnRuleUpdate = (MyClasses.MetaViewWrappers.IButton)View["btnRuleUpdate"];
//                chkRuleEnabled = (MyClasses.MetaViewWrappers.ICheckBox)View["chkRuleEnabled"];

//                lstRuleApplies = (MyClasses.MetaViewWrappers.IList)View["lstRuleApplies"];
//                lstRuleSlots = (MyClasses.MetaViewWrappers.IList)View["lstRuleSlots"];
//                lstRuleSets = (MyClasses.MetaViewWrappers.IList)View["lstRuleSets"];
//                lstRuleArmorTypes = (MyClasses.MetaViewWrappers.IList)View["lstRuleArmorTypes"];
//                txtRuleName = (MyClasses.MetaViewWrappers.ITextBox)View["txtRuleName"];
               
//                txtRulePriority = (MyClasses.MetaViewWrappers.ITextBox)View["txtRulePriority"];
//                txtGearScore = (MyClasses.MetaViewWrappers.ITextBox)View["txtGearScore"];
//                txtRuleArcaneLore = (MyClasses.MetaViewWrappers.ITextBox)View["txtRuleArcaneLore"];
//                txtRuleMaxCraft = (MyClasses.MetaViewWrappers.ITextBox)View["txtRuleMaxCraft"];


//                txtRuleWieldLevel = (MyClasses.MetaViewWrappers.ITextBox)View["txtRuleWieldLevel"];
          
//                }catch(Exception ex){LogError(ex);}
//                  try
//                  {
                
//                lstRules.Selected += new EventHandler<MVListSelectEventArgs>(lstRules_Selected);
//                lstRuleApplies.Selected += new EventHandler<MVListSelectEventArgs>(lstRuleApplies_Selected);
//                lstRuleSlots.Selected += new EventHandler<MVListSelectEventArgs>(lstRuleSlots_Selected);
//                lstRuleSets.Selected += new EventHandler<MVListSelectEventArgs>(lstRuleSets_Selected);
//                lstRuleArmorTypes.Selected += new EventHandler<MVListSelectEventArgs>(lstRuleArmorTypes_Selected);
//                btnRuleClear.Click += new EventHandler<MyClasses.MetaViewWrappers.MVControlEventArgs>(btnRuleClear_Click);
//                btnRuleNew.Click += new EventHandler<MyClasses.MetaViewWrappers.MVControlEventArgs>(btnRuleNew_Click);
//                btnRuleClone.Click += btnRuleClone_Click;
//                btnRuleUpdate.Click += new EventHandler<MyClasses.MetaViewWrappers.MVControlEventArgs>(btnRuleUpdate_Click);
//                }catch(Exception ex){LogError(ex);}
//                  try
//                  {
//                chkRuleEnabled.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkRuleEnabled_Change);
//                }catch(Exception ex){LogError(ex);}
//                  try
//                  {
//                txtRulePriority.End += new EventHandler<MVTextBoxEndEventArgs>(txtRulePriority_End);
//                txtGearScore.End += new EventHandler<MVTextBoxEndEventArgs>(txtGearScore_End);
             
//                txtRuleName.End += new EventHandler<MVTextBoxEndEventArgs>(txtRuleName_End);
//                txtRuleMaxCraft.End += new EventHandler<MVTextBoxEndEventArgs>(txtRuleMaxCraft_End);
//                txtRuleArcaneLore.End += new EventHandler<MVTextBoxEndEventArgs>(txtRuleArcaneLore_End);
//                txtRuleWieldLevel.End += new EventHandler<MVTextBoxEndEventArgs>(txtRuleWieldLevel_End);
             


//                }catch(Exception ex){LogError(ex);}
//                  try
//                  {
//                // Controls on Notify.SearchRules.Weapon Page
//                cboWeaponAppliesTo = (MyClasses.MetaViewWrappers.ICombo)View["cboWeaponAppliesTo"];
//                cboWeaponAppliesTo.Selected = 0;
//                cboMasteryType = (MyClasses.MetaViewWrappers.ICombo)View["cboMasteryType"];
//                cboMasteryType.Selected = 0;
//                }catch(Exception ex){LogError(ex);}
//                  try
//                  {
//                lstDamageTypes = (MyClasses.MetaViewWrappers.IList)View["lstDamageTypes"];


//                chkRuleWeaponsa = (MyClasses.MetaViewWrappers.ICheckBox)View["chkRuleWeaponsa"];
//                chkRuleWeaponsb = (MyClasses.MetaViewWrappers.ICheckBox)View["chkRuleWeaponsb"];
//                chkRuleWeaponsc = (MyClasses.MetaViewWrappers.ICheckBox)View["chkRuleWeaponsc"];
//                chkRuleWeaponsd = (MyClasses.MetaViewWrappers.ICheckBox)View["chkRuleWeaponsd"];
//                }catch(Exception ex){LogError(ex);}
//                  try
//                  {
   
                
//                lblRuleReqSkilla = (MyClasses.MetaViewWrappers.IStaticText)View["lblRuleReqSkilla"];

//                txtRuleReqSkilla = (MyClasses.MetaViewWrappers.ITextBox)View["txtRuleReqSkilla"];
//                txtRuleReqSkillb = (MyClasses.MetaViewWrappers.ITextBox)View["txtRuleReqSkillb"];
//                txtRuleReqSkillc = (MyClasses.MetaViewWrappers.ITextBox)View["txtRuleReqSkillc"];
//                txtRuleReqSkilld = (MyClasses.MetaViewWrappers.ITextBox)View["txtRuleReqSkilld"];
              
//                }catch(Exception ex){LogError(ex);}
//                  try
//                  {
            

//                cboWeaponAppliesTo.Change += new EventHandler<MyClasses.MetaViewWrappers.MVIndexChangeEventArgs>(cboWeaponAppliesTo_Change);
//                cboMasteryType.Change += new EventHandler<MyClasses.MetaViewWrappers.MVIndexChangeEventArgs>(cboMasteryType_Change);

//                chkRuleWeaponsa.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkRuleWeaponsa_Change);
//                chkRuleWeaponsb.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkRuleWeaponsb_Change);
//                chkRuleWeaponsc.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkRuleWeaponsc_Change);
//                chkRuleWeaponsd.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkRuleWeaponsd_Change);
//                lstDamageTypes.Selected += new EventHandler<MVListSelectEventArgs>(lstDamageTypes_Selected);
//                }catch(Exception ex){LogError(ex);}
//                  try
//                  {
          

//                txtRuleReqSkilla.End += new EventHandler<MVTextBoxEndEventArgs>(txtRuleReqSkilla_End);
              
//                txtRuleReqSkillb.End += new EventHandler<MVTextBoxEndEventArgs>(txtRuleReqSkillb_End);
//                txtRuleReqSkillc.End += new EventHandler<MVTextBoxEndEventArgs>(txtRuleReqSkillc_End);
//                txtRuleReqSkilld.End += new EventHandler<MVTextBoxEndEventArgs>(txtRuleReqSkilld_End);
             
//                }catch(Exception ex){LogError(ex);}
//                  try
//                  {

//                // Controls on Notify.SearchRules.Armor Page
//                lstRuleSlots = (MyClasses.MetaViewWrappers.IList)View["lstRuleSlots"];
//                lstRuleArmorTypes = (MyClasses.MetaViewWrappers.IList)View["lstRuleArmorTypes"];
              

//                lstRuleSets = (MyClasses.MetaViewWrappers.IList)View["lstRuleSets"];

//                // Controls on Notify.SearchRules.Req Spells
//                lstRuleSpells = (MyClasses.MetaViewWrappers.IList)View["lstRuleSpells"];
//                lstRuleSpellsEnabled = (MyClasses.MetaViewWrappers.IList)View["lstRuleSpellsEnabled"];

//                txtRuleNumSpells = (MyClasses.MetaViewWrappers.ITextBox)View["txtRuleNumSpells"];
                
//                chkRuleFilterLegend  = (MyClasses.MetaViewWrappers.ICheckBox)View["chkRuleFilterLegend"];
//                chkRuleFilterEpic  = (MyClasses.MetaViewWrappers.ICheckBox)View["chkRuleFilterEpic"];
//                chkRuleFilterMajor = (MyClasses.MetaViewWrappers.ICheckBox)View["chkRuleFilterMajor"];
//                chkRuleFilterlvl8 = (MyClasses.MetaViewWrappers.ICheckBox)View["chkRuleFilterlvl8"];
//                chkRuleFilterCloak = (MyClasses.MetaViewWrappers.ICheckBox)View["chkRuleFilterCloak"];
                
                

//                lstRuleSpells.Selected += new EventHandler<MVListSelectEventArgs>(lstRuleSpells_Selected); 
//                lstRuleSpellsEnabled.Selected += new EventHandler<MVListSelectEventArgs>(lstRuleSpellsEnabled_Selected); 
//                chkRuleFilterLegend.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkRuleFilterLegend_Change);
//                chkRuleFilterEpic.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkRuleFilterEpic_Change);
//                chkRuleFilterCloak.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkRuleFilterCloak_Change);
//                chkRuleFilterMajor.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkRuleFilterMajor_Change);
//                chkRuleFilterlvl8.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkRuleFilterlvl8_Change);
//                txtRuleNumSpells.End += new EventHandler<MVTextBoxEndEventArgs>(txtRuleNumSpells_End);
                
//                //Controls on Advanced Tab
                
                
//                chkAdvEnabled = (MyClasses.MetaViewWrappers.ICheckBox)View["chkAdvEnabled"];
//                cboAdv1KeyType = (MyClasses.MetaViewWrappers.ICombo)View["cboAdv1KeyType"];
//                cboAdv1KeyType.Selected = 0;
//                cboAdv1Key = (MyClasses.MetaViewWrappers.ICombo)View["cboAdv1Key"];
//                cboAdv1Key.Selected = 0;  		    
//                cboAdv1KeyCompare = (MyClasses.MetaViewWrappers.ICombo)View["cboAdv1KeyCompare"];
//                cboAdv1KeyCompare.Selected = 0;   		    
//                txtAdv1KeyValue = (MyClasses.MetaViewWrappers.ITextBox)View["txtAdv1KeyValue"];
    		    
//                cboAdv1Link = (MyClasses.MetaViewWrappers.ICombo)View["cboAdv1Link"];
//                cboAdv1Link.Selected = 0;
    		    
//                chkAdvEnabled = (MyClasses.MetaViewWrappers.ICheckBox)View["chkAdvEnabled"];
//                cboAdv2KeyType = (MyClasses.MetaViewWrappers.ICombo)View["cboAdv2KeyType"];
//                cboAdv2KeyType.Selected = 0;
//                cboAdv2Key = (MyClasses.MetaViewWrappers.ICombo)View["cboAdv2Key"];
//                cboAdv2Key.Selected = 0;  		    
//                cboAdv2KeyCompare = (MyClasses.MetaViewWrappers.ICombo)View["cboAdv2KeyCompare"];
//                cboAdv2KeyCompare.Selected = 0;   		    
//                txtAdv2KeyValue = (MyClasses.MetaViewWrappers.ITextBox)View["txtAdv2KeyValue"];
    		    
//                cboAdv2Link = (MyClasses.MetaViewWrappers.ICombo)View["cboAdv2Link"];
//                cboAdv2Link.Selected = 0;
    		    
//                chkAdvEnabled = (MyClasses.MetaViewWrappers.ICheckBox)View["chkAdvEnabled"];
//                cboAdv3KeyType = (MyClasses.MetaViewWrappers.ICombo)View["cboAdv3KeyType"];
//                cboAdv3KeyType.Selected = 0;
//                cboAdv3Key = (MyClasses.MetaViewWrappers.ICombo)View["cboAdv3Key"];
//                cboAdv3Key.Selected = 0;  		    
//                cboAdv3KeyCompare = (MyClasses.MetaViewWrappers.ICombo)View["cboAdv3KeyCompare"];
//                cboAdv3KeyCompare.Selected = 0;   		    
//                txtAdv3KeyValue = (MyClasses.MetaViewWrappers.ITextBox)View["txtAdv3KeyValue"];
    		    
//                cboAdv3Link = (MyClasses.MetaViewWrappers.ICombo)View["cboAdv3Link"];
//                cboAdv3Link.Selected = 0;
    		    
//                chkAdvEnabled = (MyClasses.MetaViewWrappers.ICheckBox)View["chkAdvEnabled"];
//                cboAdv4KeyType = (MyClasses.MetaViewWrappers.ICombo)View["cboAdv4KeyType"];
//                cboAdv4KeyType.Selected = 0;
//                cboAdv4Key = (MyClasses.MetaViewWrappers.ICombo)View["cboAdv4Key"];
//                cboAdv4Key.Selected = 0;  		    
//                cboAdv4KeyCompare = (MyClasses.MetaViewWrappers.ICombo)View["cboAdv4KeyCompare"];
//                cboAdv4KeyCompare.Selected = 0;   		    
//                txtAdv4KeyValue = (MyClasses.MetaViewWrappers.ITextBox)View["txtAdv4KeyValue"];
    		    
//                cboAdv4Link = (MyClasses.MetaViewWrappers.ICombo)View["cboAdv4Link"];
//                cboAdv4Link.Selected = 0;
    		    
//                chkAdvEnabled = (MyClasses.MetaViewWrappers.ICheckBox)View["chkAdvEnabled"];
//                cboAdv5KeyType = (MyClasses.MetaViewWrappers.ICombo)View["cboAdv5KeyType"];
//                cboAdv5KeyType.Selected = 0;
//                cboAdv5Key = (MyClasses.MetaViewWrappers.ICombo)View["cboAdv5Key"];
//                cboAdv5Key.Selected = 0;  		    
//                cboAdv5KeyCompare = (MyClasses.MetaViewWrappers.ICombo)View["cboAdv5KeyCompare"];
//                cboAdv5KeyCompare.Selected = 0;   		    
//                txtAdv5KeyValue = (MyClasses.MetaViewWrappers.ITextBox)View["txtAdv5KeyValue"];
    		         
                
//                chkAdvEnabled.Change += chkAdvEnabled_Change;
//                cboAdv1KeyType.Change += cboAdv1KeyType_Change;
//                cboAdv1Key.Change += cboAdv1Key_Change;
//                cboAdv1KeyCompare.Change += cboAdv1KeyCompare_Change;
//                txtAdv1KeyValue.End += txtAdv1KeyValue_Change;
                
//                cboAdv1Link.Change += cboAdv1Link_Change;
                
//                chkAdvEnabled.Change += chkAdvEnabled_Change;
//                cboAdv2KeyType.Change += cboAdv2KeyType_Change;
//                cboAdv2Key.Change += cboAdv2Key_Change;
//                cboAdv2KeyCompare.Change += cboAdv2KeyCompare_Change;
//                txtAdv2KeyValue.End += txtAdv2KeyValue_Change;
                
//                cboAdv2Link.Change += cboAdv2Link_Change;
                
//                chkAdvEnabled.Change += chkAdvEnabled_Change;
//                cboAdv3KeyType.Change += cboAdv3KeyType_Change;
//                cboAdv3Key.Change += cboAdv3Key_Change;
//                cboAdv3KeyCompare.Change += cboAdv3KeyCompare_Change;
//                txtAdv3KeyValue.End += txtAdv3KeyValue_Change;
                
//                cboAdv3Link.Change += cboAdv3Link_Change;
                
//                chkAdvEnabled.Change += chkAdvEnabled_Change;
//                cboAdv4KeyType.Change += cboAdv4KeyType_Change;
//                cboAdv4Key.Change += cboAdv4Key_Change;
//                cboAdv4KeyCompare.Change += cboAdv4KeyCompare_Change;
//                txtAdv4KeyValue.End += txtAdv4KeyValue_Change;
                
//                cboAdv4Link.Change += cboAdv4Link_Change;
                
//                chkAdvEnabled.Change += chkAdvEnabled_Change;
//                cboAdv5KeyType.Change += cboAdv5KeyType_Change;
//                cboAdv5Key.Change += cboAdv5Key_Change;
//                cboAdv5KeyCompare.Change += cboAdv5KeyCompare_Change;
//                txtAdv5KeyValue.End += txtAdv5KeyValue_Change;
                
                     
                                
//                }catch(Exception ex){LogError(ex);}
//                  try
//                  {
   
//                // Controls on Notify.NPC/Trophies Page

//                btnAddTrophyItem = (MyClasses.MetaViewWrappers.IButton)View["btnAddTrophyItem"];
//                btnUpdateTrophyItem = (MyClasses.MetaViewWrappers.IButton)View["btnUpdateTrophyItem"];
//                lstmyTrophies = (MyClasses.MetaViewWrappers.IList)View["lstmyTrophies"];
//                txtTrophyName = (MyClasses.MetaViewWrappers.ITextBox)View["txtTrophyName"];
//                chkTrophyExact = (MyClasses.MetaViewWrappers.ICheckBox)View["chkTrophyExact"];
//                txtTrophyMax = (MyClasses.MetaViewWrappers.ITextBox)View["txtTrophyMax"];
//                lstmyTrophies.Selected += new EventHandler<MVListSelectEventArgs>(lstmyTrophies_Selected);
//                chkTrophyExact.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkTrophyExact_Change);
//                txtTrophyName.End += new EventHandler<MVTextBoxEndEventArgs>(txtTrophyName_End);
//                txtTrophyMax.End += new EventHandler<MVTextBoxEndEventArgs>(txtTrophyMax_End);
//                btnAddTrophyItem.Click += new EventHandler<MyClasses.MetaViewWrappers.MVControlEventArgs>(btnAddTrophyItem_Click);
//                btnUpdateTrophyItem.Click += new EventHandler<MyClasses.MetaViewWrappers.MVControlEventArgs>(btnUpdateTrophyItem_Click);
//                  }
//                  catch (Exception ex) { LogError(ex); }
//                  try
//                  {
                
                
//                // Controls on Notify.Mobs Page

//                lstmyMobs = (MyClasses.MetaViewWrappers.IList)View["lstmyMobs"];
//                btnAddMobItem = (MyClasses.MetaViewWrappers.IButton)View["btnAddMobItem"];
//                btnUpdateMobItem = (MyClasses.MetaViewWrappers.IButton)View["btnUpdateMobItem"];
//                chkmyMobExact = (MyClasses.MetaViewWrappers.ICheckBox)View["chkmyMobExact"];
//                txtmyMobName = (MyClasses.MetaViewWrappers.ITextBox)View["txtmyMobName"];
//                lstmyMobs.Selected += new EventHandler<MVListSelectEventArgs>(lstmyMobs_Selected);
//                chkmyMobExact.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkmyMobExact_Change);
//                txtmyMobName.End += new EventHandler<MVTextBoxEndEventArgs>(txtmyMobName_End);
//                btnAddMobItem.Click += new EventHandler<MyClasses.MetaViewWrappers.MVControlEventArgs>(btnAddMobItem_Click);
//                btnUpdateMobItem.Click += new EventHandler<MyClasses.MetaViewWrappers.MVControlEventArgs>(btnUpdateMobItem_Click);
//                  }
//                  catch (Exception ex) { LogError(ex); }
//                  try
//                  {
//                // Controls on Notify.Salvage Page
//                lstNotifySalvage = (MyClasses.MetaViewWrappers.IList)View["lstNotifySalvage"];
//                lblSalvageName = (MyClasses.MetaViewWrappers.IStaticText)View["lblSalvageName"];
//                txtSalvageString = (MyClasses.MetaViewWrappers.ITextBox)View["txtSalvageString"];
//                btnUpdateSalvage = (MyClasses.MetaViewWrappers.IButton)View["btnUpdateSalvage"];
//                lblSalvageString = (MyClasses.MetaViewWrappers.IStaticText)View["lblSalvageString"];
//                lstNotifySalvage.Selected += new EventHandler<MVListSelectEventArgs>(lstNotifySalvage_Selected);
//                btnUpdateSalvage.Click += new EventHandler<MyClasses.MetaViewWrappers.MVControlEventArgs>(btnUpdateSalvage_Click);
//                txtSalvageString.End += new EventHandler<MVTextBoxEndEventArgs>(txtSalvageString_End);
//                }catch(Exception ex){LogError(ex);}

        //    }

        //    catch (Exception ex) { LogError(ex); }


        //}

    }
}

        #endregion