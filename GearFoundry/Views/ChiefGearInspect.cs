using System;
using System.Drawing;
using System.IO;
using Decal.Adapter;
using Decal.Filters;
using Decal.Adapter.Wrappers;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using VirindiViewService;
using MyClasses.MetaViewWrappers;
using VirindiViewService.Controls;
using MyClasses.MetaViewWrappers.VirindiViewServiceHudControls;
using System.Xml.Serialization;
using System.Xml;
using System.Text.RegularExpressions;
using System.Text;

namespace GearFoundry
{
    public partial class PluginCore
    {

        private List<IDNameLoadable> cboList;
        //General controls for Inspect Page
        private bool bChiefGearInspectPageSearchRules = false;
        private bool bChiefGearInspectPageTrophies = false;
        private bool bChiefGearInspectPageMobs = false;
        private bool bChiefGearInspectPageSalvage = false;

        private HudTabView ChiefGearInspectPageTabView = null;
        private HudFixedLayout ChiefGearInspectPageSearchRules = null;
        private HudFixedLayout ChiefGearInspectPageTrophies = null;
        private HudFixedLayout ChiefGearInspectPageMobs = null;
        private HudFixedLayout ChiefGearInspectPageSalvage = null;

        private bool bChiefGearInspectPageTabViewMain = false;
        private bool bChiefGearInspectPageMenuTabViewProperties = false;
        private bool bChiefGearInspectPageMenuTabViewAppearance = false;
        private bool bChiefGearInspectPageMenuTabViewSpells = false;
        private bool bChiefGearInspectPageMenuTabViewAdvanced = false;

        private HudTabView ChiefGearInspectPageMenuTabView = null;
        private HudFixedLayout ChiefGearInspectPageMenuTabMain = null;
        private HudFixedLayout ChiefGearInspectPageMenuTabProperties = null;
        private HudFixedLayout ChiefGearInspectPageMenuTabAppearance = null;
        private HudFixedLayout ChiefGearInspectPageMenuTabReqSpells = null;
        private HudFixedLayout ChiefGearInspectPageMenuTabAdvanced = null;


        //Controls on Notify.SearchRules Page
        private HudButton btnRuleClear;
        private HudButton btnRuleNew;
        private HudButton btnRuleClone;
        private HudButton btnRuleUpdate;
        private HudList lstRules = null;
        private HudList.HudListRowAccessor LstRulesHudListRow = null;

        // Controls on Notify.SearchRules.Main Page
        private HudCheckBox chkRuleEnabled = null;
        private HudList lstRuleApplies;
        private HudList.HudListRowAccessor lstRuleAppliesListRow = null;

        private HudList lstRuleSlots;
        private HudList.HudListRowAccessor lstRuleSlotsListRow = null;

        private HudTextBox txtRuleName;
        private HudTextBox txtRulePriority;
        private HudTextBox txtRuleMaxCraft;
        private HudTextBox txtGearScore;
        private HudTextBox txtRuleArcaneLore;
        private HudTextBox txtRuleWieldLevel;

        // Controls on Notify.SearchRules.Properties Page
        private HudCombo cboWeaponAppliesTo;
        private static long objWeaponAppliesTo = 0;
        private static string objWeaponAppliesToName = null;

        private HudCombo cboMasteryType;
        private static long objMasteryType = 0;
        private static string objMasteryTypeName = null;
        private HudList lstDamageTypes;
        private HudList.HudListRowAccessor lstDamageTypesListRow;

        private HudCheckBox chkRuleWeaponsb = null;
        private HudCheckBox chkRuleWeaponsa = null;
        private HudCheckBox chkRuleWeaponsc = null;
        private HudCheckBox chkRuleWeaponsd = null;

        private HudTextBox txtRuleReqSkilla;
        private HudTextBox txtRuleReqSkillb;
        private HudTextBox txtRuleReqSkillc;
        private HudTextBox txtRuleReqSkilld;
        private HudList lstRuleArmorTypes;
        private HudList.HudListRowAccessor lstRuleArmorTypesListRow;
        private HudList lstRuleSets;
        private HudList.HudListRowAccessor lstRuleSetsListRow;

        // Controls on Notify.SearchRules.Req Spells
        private HudList lstRuleSpells;
        private HudList.HudListRowAccessor lstRuleSpellsListRow;
        private HudList lstRuleSpellsEnabled;
        private HudList.HudListRowAccessor lstRuleSpellsEnabledListRow;

        private HudCheckBox chkRuleFilterLegend = null;
        private HudCheckBox chkRuleFilterEpic = null;
        private HudCheckBox chkRuleFilterMajor = null;
        private HudCheckBox chkRuleFilterlvl8 = null;
        private HudCheckBox chkRuleFilterCloak = null;
        private HudTextBox txtRuleNumSpells;

        //Controls on Advanced Tab
        private HudCheckBox chkAdvEnabled = null;
        private HudCombo cboAdv1KeyType;
        private HudCombo cboAdv1Key;
        private HudCombo cboAdv1KeyCompare;
        private HudTextBox txtAdv1KeyValue;

        private HudCombo cboAdv1Link;

        private HudCombo cboAdv2KeyType;
        private HudCombo cboAdv2Key;
        private HudCombo cboAdv2KeyCompare;
        private HudTextBox txtAdv2KeyValue;

        private HudCombo cboAdv2Link;

        private HudCombo cboAdv3KeyType;
        private HudCombo cboAdv3Key;
        private HudCombo cboAdv3KeyCompare;
        private HudTextBox txtAdv3KeyValue;

        private HudCombo cboAdv3Link;

        private HudCombo cboAdv4KeyType;
        private HudCombo cboAdv4Key;
        private HudCombo cboAdv4KeyCompare;
        private HudTextBox txtAdv4KeyValue;

        private HudCombo cboAdv4Link;

        private HudCombo cboAdv5KeyType;
        private HudCombo cboAdv5Key;
        private HudCombo cboAdv5KeyCompare;
        private HudTextBox txtAdv5KeyValue;

        // Controls on Notify.NPC/Trophies Page

        private HudList lstmyTrophies;
        private HudList.HudListRowAccessor lstmyTrophiesListRow;
        private HudTextBox txtTrophyName;
        private HudButton btnAddTrophyItem;
        private HudButton btnUpdateTrophyItem;
        private HudCheckBox chkTrophyExact = null;
        private HudTextBox txtTrophyMax;

        // Controls on Notify.Mobs Page
        private HudList lstmyMobs;
        private HudList.HudListRowAccessor lstmyMobsListRow;
        private HudButton btnAddMobItem;
        private HudButton btnUpdateMobItem;
        private HudCheckBox chkmyMobExact = null;
        private HudTextBox txtmyMobName;



        // Controls on Notify.Salvage Page
        private HudList lstNotifySalvage;
        private HudList.HudListRowAccessor  lstNotifySalvageListRow;
        private HudButton btnUpdateSalvage;
        private HudTextBox txtSalvageString;
        private HudStaticText lblSalvageName;


        private void RenderChiefGearHudInspect()
        {
            try
            {
                if (ChiefGearHudInspect != null) { DisposeChiefGearHudInspect(); }

 
                //Set up menu for functions in the Inspect page
                ChiefGearInspectPageTabView = new HudTabView();
                ChiefGearHudInspect.AddControl(ChiefGearInspectPageTabView, new Rectangle(5, 5, 495, 500));
                ChiefGearInspectPageSearchRules = new HudFixedLayout();
                ChiefGearInspectPageTabView.AddTab(ChiefGearInspectPageSearchRules, "Search Rules");
                ChiefGearInspectPageTrophies = new HudFixedLayout();
                ChiefGearInspectPageTabView.AddTab(ChiefGearInspectPageTrophies, "Trophies/NPCs");
                ChiefGearInspectPageMobs = new HudFixedLayout();
                ChiefGearInspectPageTabView.AddTab(ChiefGearInspectPageMobs, "Mobs");
                ChiefGearInspectPageSalvage = new HudFixedLayout();
                ChiefGearInspectPageTabView.AddTab(ChiefGearInspectPageSalvage, "Salvage");
                SubscribeChiefGearInspectEvents();

                RenderChiefGearInspectPageSearchRules();
                
            }
            catch (Exception ex) { LogError(ex); }
        }

        private void SubscribeChiefGearInspectEvents()
        {
            ChiefGearInspectPageTabView.OpenTabChange += ChiefGearInspectPageTabView_OpenTabChange;

        }

        private void UnsubscribeChiefGearInspectEvents()
        {
          //  if (ChiefGearInspectPageTabView != null) { ChiefGearInspectPageTabView.OpenTabChange -= ChiefGearInspectPageTabView_OpenTabChange; }

        }


        private void ChiefGearInspectPageTabView_OpenTabChange(object sender, System.EventArgs e)
        {
            try
            {
                WriteToChat("Current tab value is " + ChiefGearInspectPageTabView.CurrentTab.ToString());
                switch (ChiefGearInspectPageTabView.CurrentTab)
                {
                    case 0:
                        WriteToChat("I am at file tab and just hit tab for rendering search rules page");
                        if (ChiefGearInspectPageTrophies != null) { DisposeChiefGearInspectPageTrophies(); }
                        if (ChiefGearInspectPageMobs != null) { DisposeChiefGearInspectPageMobs(); }
                        if (ChiefGearInspectPageSalvage != null) { DisposeChiefGearInspectPageSalvage(); }
                        RenderChiefGearInspectPageSearchRules();
                        break;
                    case 1:
                        WriteToChat("I am at file tab and just hit tab for rendering trophies page");
                        if (ChiefGearInspectPageSearchRules != null) { DisposeChiefGearInspectPageSearchRules(); }
                        if (ChiefGearInspectPageMobs != null) { DisposeChiefGearInspectPageMobs(); }
                        if (ChiefGearInspectPageSalvage != null) { DisposeChiefGearInspectPageSalvage(); }
                        RenderChiefGearInspectPageTrophies();
                        break;
                    case 2:
                        WriteToChat("I am at file tab and just hit tab for rendering mobs page");
                        if (ChiefGearInspectPageSearchRules != null) { DisposeChiefGearInspectPageSearchRules(); }
                        if (ChiefGearInspectPageTrophies != null) { DisposeChiefGearInspectPageTrophies(); }
                        if (ChiefGearInspectPageSalvage != null) { DisposeChiefGearInspectPageSalvage(); }
                        WriteToChat("I am at file tab and have finished disposes and ready to render");
                        RenderChiefGearInspectPageMobs();
                        break;
                    case 3:
                        
                        WriteToChat("I am at file tab and just hit tab for rendering salvage page");
                        if (ChiefGearInspectPageSearchRules != null) { DisposeChiefGearInspectPageSearchRules(); }
                        if (ChiefGearInspectPageTrophies != null) { DisposeChiefGearInspectPageTrophies(); }
                        if (ChiefGearInspectPageMobs != null) { DisposeChiefGearInspectPageMobs(); }

                        RenderChiefGearInspectPageSalvage();
                        break;            
                 }
            }
            catch (Exception ex) { LogError(ex); }
        }



        private void RenderChiefGearInspectPageSearchRules()
        {
            try
            {
                lstRules = new HudList();
                LstRulesHudListRow = new HudList.HudListRowAccessor();
                ChiefGearInspectPageSearchRules.AddControl(lstRules, new Rectangle(5, 5, 480, 90));
                lstRules.AddColumn(typeof(HudCheckBox), 5, null);
                lstRules.AddColumn(typeof(HudStaticText), 20, null);
                lstRules.AddColumn(typeof(HudStaticText), 350, null);
                lstRules.AddColumn(typeof(HudPictureBox), 12, null);
                lstRules.AddColumn(typeof(HudStaticText), 1, null);
                _UpdateRulesTabs();
                WriteToChat("I have been at hud making list rules and now am back after trying to populate list.");
                btnRuleClear = new HudButton();
                btnRuleClear.Text = "Reset Values";
                ChiefGearInspectPageSearchRules.AddControl(btnRuleClear, new Rectangle(10, 110, 110, 20));

                btnRuleNew = new HudButton();
                btnRuleNew.Text = "Add New Rule";
                ChiefGearInspectPageSearchRules.AddControl(btnRuleNew, new Rectangle(130, 110, 110, 20));

                btnRuleClone = new HudButton();
                btnRuleClone.Text = "Clone Current Rule";
                ChiefGearInspectPageSearchRules.AddControl(btnRuleClone, new Rectangle(250, 110, 110, 20));

                btnRuleUpdate = new HudButton();
                btnRuleUpdate.Text = "Save Rule Changes";
                ChiefGearInspectPageSearchRules.AddControl(btnRuleUpdate, new Rectangle(370, 110, 110, 20));

                //Set up for adding menu and choices to ChiefGearInspectPageSearchRules
                ChiefGearInspectPageMenuTabView = new HudTabView();
                ChiefGearInspectPageSearchRules.AddControl(ChiefGearInspectPageMenuTabView, new Rectangle(0, 140, 520, 390));
                ChiefGearInspectPageMenuTabMain = new HudFixedLayout();
                ChiefGearInspectPageMenuTabView.AddTab(ChiefGearInspectPageMenuTabMain, "Main");
                ChiefGearInspectPageMenuTabProperties = new HudFixedLayout();
                ChiefGearInspectPageMenuTabView.AddTab(ChiefGearInspectPageMenuTabProperties, "Properties");
                ChiefGearInspectPageMenuTabAppearance = new HudFixedLayout();
                ChiefGearInspectPageMenuTabView.AddTab(ChiefGearInspectPageMenuTabAppearance, "Appearance");
                ChiefGearInspectPageMenuTabReqSpells = new HudFixedLayout();
                ChiefGearInspectPageMenuTabView.AddTab(ChiefGearInspectPageMenuTabReqSpells, "Req Spells");
                ChiefGearInspectPageMenuTabAdvanced = new HudFixedLayout();
                ChiefGearInspectPageMenuTabView.AddTab(ChiefGearInspectPageMenuTabAdvanced, "Advanced");

                bChiefGearInspectPageSearchRules = true;
                RenderChiefGearInspectPageMenuTabMain();

                SubscribeChiefGearInspectSearchRuleEvents();


            }
            catch (Exception ex) { LogError(ex); }

        }

        private void DisposeChiefGearInspectPageSearchRules()
        {
            unsubscribeChiefGearInspectSearchRuleEvents();
            if (lstRules != null) { lstRules.Dispose();}
            if (btnRuleClear != null) { btnRuleClear.Dispose(); }
            if (btnRuleClone != null) { btnRuleClone.Dispose(); }
            if (btnRuleUpdate != null) { btnRuleUpdate.Dispose(); }
            if (btnRuleNew != null) { btnRuleNew.Dispose(); }

        }

        private void SubscribeChiefGearInspectSearchRuleEvents()
        {
            ChiefGearInspectPageMenuTabView.OpenTabChange += ChiefGearInspectPageMenuTabView_OpenTabChange;
            lstRules.Click += (sender, row, col) => lstRules_Click(sender, row, col);
           btnRuleClear.Hit += (sender, index) => btnRuleClear_Hit(sender, index);
           btnRuleUpdate.Hit += (sender, index) => btnRuleUpdate_Hit(sender, index);
           btnRuleClone.Hit += (sender, index) => btnRuleClone_Hit(sender, index);
           btnRuleClear.Hit += (sender, index) => btnRuleClear_Hit(sender, index);
        }


       private void unsubscribeChiefGearInspectSearchRuleEvents()
        {
         //   ChiefGearInspectPageMenuTabView.OpenTabChange -= ChiefGearInspectPageMenuTabView_OpenTabChange;
            lstRules.Click += (sender, row, col) => lstRules_Click(sender, row, col);
           btnRuleClear.Hit += (sender, index) => btnRuleClear_Hit(sender, index);
           btnRuleUpdate.Hit += (sender, index) => btnRuleUpdate_Hit(sender, index);
           btnRuleClone.Hit += (sender, index) => btnRuleClone_Hit(sender, index);
           btnRuleClear.Hit += (sender, index) => btnRuleClear_Hit(sender, index);
        }




        private void ChiefGearInspectPageMenuTabView_OpenTabChange(object sender, System.EventArgs e)
        {
            try
            {
                switch (ChiefGearInspectPageMenuTabView.CurrentTab)
                {
                    case 0:
                        if (ChiefGearInspectPageMenuTabProperties != null) { DisposeChiefGearInspectPageMenuTabProperties(); }
                        if (ChiefGearInspectPageMenuTabAppearance != null) { DisposeChiefGearInspectPageMenuTabAppearance(); }
                        if (ChiefGearInspectPageMenuTabReqSpells != null) { DisposeChiefGearInspectPageMenuTabReqSpells(); }
                        if (ChiefGearInspectPageMenuTabAdvanced != null) { DisposeChiefGearInspectPageMenuTabAdvanced(); }

                        RenderChiefGearInspectPageMenuTabMain();
                        break;
                    case 1:
                        if (ChiefGearInspectPageMenuTabMain != null) { DisposeChiefGearInspectPageMenuTabMain(); }
                        if (ChiefGearInspectPageMenuTabAppearance != null) { DisposeChiefGearInspectPageMenuTabAppearance(); }
                        if (ChiefGearInspectPageMenuTabReqSpells != null) { DisposeChiefGearInspectPageMenuTabReqSpells(); }
                        if (ChiefGearInspectPageMenuTabAdvanced != null) { DisposeChiefGearInspectPageMenuTabAdvanced(); }
                        RenderChiefGearInspectPageMenuTabProperties();
                        break;
                    case 2:
                        if (ChiefGearInspectPageMenuTabMain != null) { DisposeChiefGearInspectPageMenuTabMain(); }
                        if (ChiefGearInspectPageMenuTabProperties != null) { DisposeChiefGearInspectPageMenuTabProperties(); }
                        if (ChiefGearInspectPageMenuTabReqSpells != null) { DisposeChiefGearInspectPageMenuTabReqSpells(); }
                        if (ChiefGearInspectPageMenuTabAdvanced != null) { DisposeChiefGearInspectPageMenuTabAdvanced(); }

                        RenderChiefGearInspectPageMenuTabAppearance();
                        break;
                    case 3:
                        if (ChiefGearInspectPageMenuTabMain != null) { DisposeChiefGearInspectPageMenuTabMain(); }
                        if (ChiefGearInspectPageMenuTabProperties != null) { DisposeChiefGearInspectPageMenuTabProperties(); }
                        if (ChiefGearInspectPageMenuTabAdvanced != null) { DisposeChiefGearInspectPageMenuTabAdvanced(); }
                        if (ChiefGearInspectPageMenuTabAppearance != null) { DisposeChiefGearInspectPageMenuTabAppearance(); }

                        RenderChiefGearInspectPageMenuTabReqSpells();
                        break;
                    case 4:
                        if (ChiefGearInspectPageMenuTabMain != null) { DisposeChiefGearInspectPageMenuTabMain(); }
                        if (ChiefGearInspectPageMenuTabProperties != null) { DisposeChiefGearInspectPageMenuTabProperties(); }
                        if (ChiefGearInspectPageMenuTabAppearance != null) { DisposeChiefGearInspectPageMenuTabAppearance(); }
                        if (ChiefGearInspectPageMenuTabReqSpells != null) { DisposeChiefGearInspectPageMenuTabReqSpells(); }

                        RenderChiefGearInspectPageMenuTabAdvanced();
                        break;

                }
            }
            catch (Exception ex) { LogError(ex); }
        }

        private void RenderChiefGearInspectPageMenuTabMain()
        {
            try
            {
                if (ChiefGearInspectPageMenuTabMain != null) { DisposeChiefGearInspectPageMenuTabMain(); }

 
                HudStaticText lblRuleName = new HudStaticText();
                //   lblChiefGearSwitch.FontHeight = nmenuFontHeight;
                lblRuleName.Text = "Rule Name";
                ChiefGearInspectPageMenuTabMain.AddControl(lblRuleName, new Rectangle(70, 5, 90, 16));

                txtRuleName = new HudTextBox();
                txtRuleName.Text = "";
                ChiefGearInspectPageMenuTabMain.AddControl(txtRuleName, new Rectangle(70, 25, 210, 16));

                HudStaticText lblRulePriority = new HudStaticText();
                //   lblChiefGearSwitch.FontHeight = nmenuFontHeight;
                lblRulePriority.Text = "Priority (1 - 999)";
                ChiefGearInspectPageMenuTabMain.AddControl(lblRulePriority, new Rectangle(290, 5, 90, 16));

                txtRulePriority = new HudTextBox();
                txtRulePriority.Text = "1";
                ChiefGearInspectPageMenuTabMain.AddControl(txtRulePriority, new Rectangle(290, 25, 30, 16));

                chkRuleEnabled = new HudCheckBox();
                chkRuleEnabled.Text = "Enabled";
                ChiefGearInspectPageMenuTabMain.AddControl(chkRuleEnabled, new Rectangle(5, 25, 60, 16));
              //  chkRuleEnabled.Checked = Convert.ToBoolean(mSelectedRule.Element("Enabled").Value);



                HudStaticText lblRuleApplies = new HudStaticText();
                //   lblChiefGearSwitch.FontHeight = nmenuFontHeight;
                lblRuleApplies.Text = "Item type {required:}";
                ChiefGearInspectPageMenuTabMain.AddControl(lblRuleApplies, new Rectangle(5, 55, 200, 16));


                lstRuleApplies = new HudList();
                lstRuleAppliesListRow = new HudList.HudListRowAccessor();

                ChiefGearInspectPageMenuTabMain.AddControl(lstRuleApplies, new Rectangle(5, 75, 130, 200));
                lstRules.AddColumn(typeof(HudCheckBox), 5, null);
                lstRules.AddColumn(typeof(HudStaticText), 110, null);
                lstRules.AddColumn(typeof(HudStaticText), 1, null);


  
                txtGearScore = new HudTextBox();
                txtGearScore.Text = "-1";
                ChiefGearInspectPageMenuTabMain.AddControl(txtGearScore, new Rectangle(160, 75, 50, 16));

                HudStaticText lblGearScore = new HudStaticText();
                //   lblChiefGearSwitch.FontHeight = nmenuFontHeight;
                lblGearScore.Text = "GearScore(Min)";
                ChiefGearInspectPageMenuTabMain.AddControl(lblGearScore, new Rectangle(215, 75, 100, 16));

                txtRuleArcaneLore = new HudTextBox();
                txtRuleArcaneLore.Text = "-1";
                ChiefGearInspectPageMenuTabMain.AddControl(txtRuleArcaneLore, new Rectangle(160, 95, 50, 16));

                HudStaticText lblRuleArcaneLore = new HudStaticText();
                //   lblChiefGearSwitch.FontHeight = nmenuFontHeight;
                lblRuleArcaneLore.Text = "Arcane Lore (Max)";
                ChiefGearInspectPageMenuTabMain.AddControl(lblRuleArcaneLore, new Rectangle(215, 95, 100, 16));

                txtRuleMaxCraft = new HudTextBox();
                txtRuleMaxCraft.Text = "-1";
                ChiefGearInspectPageMenuTabMain.AddControl(txtRuleMaxCraft, new Rectangle(160, 115, 50, 16));

                HudStaticText lblRuleWork = new HudStaticText();
                //   lblChiefGearSwitch.FontHeight = nmenuFontHeight;
                lblRuleWork.Text = "Work (Max)";
                ChiefGearInspectPageMenuTabMain.AddControl(lblRuleWork, new Rectangle(215, 115, 115, 16));

                txtRuleWieldLevel = new HudTextBox();
                txtRuleWieldLevel.Text = "-1";
                ChiefGearInspectPageMenuTabMain.AddControl(txtRuleWieldLevel, new Rectangle(160, 135, 50, 16));

                HudStaticText lblRuleWieldLevel = new HudStaticText();
                //   lblChiefGearSwitch.FontHeight = nmenuFontHeight;
                lblRuleWieldLevel.Text = "Char Level (Max)";
                ChiefGearInspectPageMenuTabMain.AddControl(lblRuleWieldLevel, new Rectangle(215, 135, 100, 16));

                txtRuleNumSpells = new HudTextBox();
                txtRuleNumSpells.Text = "-1";
                ChiefGearInspectPageMenuTabMain.AddControl(txtRuleNumSpells, new Rectangle(160, 155, 50, 16));

                HudStaticText lblnumSpells = new HudStaticText();
                //   lblChiefGearSwitch.FontHeight = nmenuFontHeight;
                lblnumSpells.Text = "Num Spells (Min)";
                ChiefGearInspectPageMenuTabMain.AddControl(lblnumSpells, new Rectangle(215, 155, 100, 16));

                HudStaticText lblSlots = new HudStaticText();
                //   lblChiefGearSwitch.FontHeight = nmenuFontHeight;
                lblSlots.Text = "Slots";
                ChiefGearInspectPageMenuTabMain.AddControl(lblSlots, new Rectangle(330, 55, 88, 16));

                lstRuleSlots = new HudList();
                ChiefGearInspectPageMenuTabMain.AddControl(lstRuleSlots, new Rectangle(350, 75, 135, 200));
                lstRuleSlots.AddColumn(typeof(HudCheckBox), 5, null);
                lstRuleSlots.AddColumn(typeof(HudStaticText), 110, null);
                lstRuleSlots.AddColumn(typeof(HudStaticText), 1, null);


                bChiefGearInspectPageTabViewMain = true;
                SubscribeChiefGearInspectPageMenuTabViewPageSearchRuleMainEvents();


            }
            catch (Exception ex) { LogError(ex); }

        }

        private void SubscribeChiefGearInspectPageMenuTabViewPageSearchRuleMainEvents()
        {
            try{
                lstRuleApplies.Click += (sender, row, col) => lstRuleApplies_Click(sender, row, col);
                lstRuleSlots.Click += (sender, row, col) => lstRuleSlots_Click(sender, row, col);
                chkRuleEnabled.Change += chkRuleEnabled_Change;
                txtRuleName.LostFocus += txtRuleName_LostFocus;
                txtRulePriority.LostFocus += txtRulePriority_LostFocus;
                txtGearScore.LostFocus += txtGearScore_LostFocus;
                txtRuleArcaneLore.LostFocus += txtRuleArcaneLore_LostFocus;
                txtRuleMaxCraft.LostFocus += txtRuleMaxCraft_LostFocus;
                txtRuleWieldLevel.LostFocus += txtRuleWieldLevel_LostFocus;
                txtRuleNumSpells.LostFocus += txtRuleNumSpells_LostFocus;

 

            }
            catch (Exception ex) { LogError(ex); }


        }

        private void unsubscribeChiefGearInspectPageMenuTabViewPageSearchRuleMainEvents()
        {
            try
            {
                if (lstRuleApplies != null) { lstRuleApplies.Click -= (sender, row, col) => lstRuleApplies_Click(sender, row, col); }
                if (lstRuleSlots != null) {lstRuleSlots.Click -= (sender, row, col) => lstRuleSlots_Click(sender, row, col);}
                if (chkRuleEnabled != null) { chkRuleEnabled.Change -= chkRuleEnabled_Change; }
                if (txtRuleName != null) { txtRuleName.LostFocus -= txtRuleName_LostFocus; }
                if (txtRulePriority != null) { txtRulePriority.LostFocus -= txtRulePriority_LostFocus; }
                if (txtGearScore != null) {txtGearScore.LostFocus -= txtGearScore_LostFocus;}
                if (txtRuleArcaneLore != null) { txtRuleArcaneLore.LostFocus -= txtRuleArcaneLore_LostFocus; }
                if (txtRuleMaxCraft != null) { txtRuleMaxCraft.LostFocus -= txtRuleMaxCraft_LostFocus; }
                if (txtRuleWieldLevel != null) { txtRuleWieldLevel.LostFocus -= txtRuleWieldLevel_LostFocus; }
                if (txtRuleNumSpells != null) { txtRuleNumSpells.LostFocus -= txtRuleNumSpells_LostFocus; }



            }
            catch (Exception ex) { LogError(ex); }


        }



        private void DisposeChiefGearInspectPageMenuTabMain()
        {
            try
            {
                unsubscribeChiefGearInspectPageMenuTabViewPageSearchRuleMainEvents();
                if(lstRuleApplies != null){lstRuleApplies.Dispose();} 
                if(lstRuleAppliesListRow != null){lstRuleAppliesListRow = null;}
                if(lstRuleSlots != null){lstRuleSlots.Dispose();}
                if(lstRuleSlotsListRow != null){lstRuleSlotsListRow = null;}
                if(txtRuleName != null){txtRuleName.Dispose();}
                if(txtRulePriority != null){txtRulePriority.Dispose();}
                if(txtRuleMaxCraft != null){txtRuleMaxCraft.Dispose();}
                if(txtGearScore != null){txtGearScore.Dispose();}
                if(txtRuleArcaneLore != null){txtRuleArcaneLore.Dispose();}
                if(txtRuleWieldLevel != null){txtRuleWieldLevel.Dispose();}
                if(txtRuleNumSpells != null){txtRuleNumSpells.Dispose();}
                bChiefGearInspectPageTabViewMain = false;
                

            }
            catch (Exception ex) { LogError(ex); }

        }



        private void RenderChiefGearInspectPageMenuTabProperties()
        {
            try
            {
                if (ChiefGearInspectPageMenuTabProperties != null) { DisposeChiefGearInspectPageMenuTabProperties(); }
                int i = 0;

                //Controls for Wield Skill
                HudStaticText lblWeapCat = new HudStaticText();
                //   lblChiefGearSwitch.FontHeight = nmenuFontHeight;
                lblWeapCat.Text = "Wield Skill";
                ChiefGearInspectPageMenuTabProperties.AddControl(lblWeapCat, new Rectangle(5, 5, 80, 16));

                ControlGroup WeaponAppliesToChoices = new ControlGroup();
                cboWeaponAppliesTo = new HudCombo(WeaponAppliesToChoices);
                i = 0;
                foreach (IDNameLoadable info in WeaponTypeList)
                {
                    cboWeaponAppliesTo.AddItem(info.name, i);
                    i++;
                }
                cboWeaponAppliesTo.Current = 0;
                ChiefGearInspectPageMenuTabProperties.AddControl(cboWeaponAppliesTo, new Rectangle(5, 20, 125, 20));

                //Controls for mastery
                HudStaticText lblMastCat = new HudStaticText();
                //   lblChiefGearSwitch.FontHeight = nmenuFontHeight;
                lblMastCat.Text = "Mastery";
                ChiefGearInspectPageMenuTabProperties.AddControl(lblMastCat, new Rectangle(5, 45, 80, 20));

                ControlGroup MastCatChoices = new ControlGroup();
                cboMasteryType = new HudCombo(MastCatChoices);
                i = 0;
                foreach (IDNameLoadable info in MasteryIndex)
                {
                    cboMasteryType.AddItem(info.name, i);
                    i++;
                }
                cboMasteryType.Current = 0;
                ChiefGearInspectPageMenuTabProperties.AddControl(cboMasteryType, new Rectangle(5, 65, 125, 20));

                //Controls for Damage Type
                HudStaticText lblDamageTypes = new HudStaticText();
                //   lblChiefGearSwitch.FontHeight = nmenuFontHeight;
                lblDamageTypes.Text = "Damage Type:}";
                ChiefGearInspectPageMenuTabProperties.AddControl(lblDamageTypes, new Rectangle(5, 90, 125, 16));


                lstDamageTypes = new HudList();
                lstDamageTypesListRow = new HudList.HudListRowAccessor();

                ChiefGearInspectPageMenuTabProperties.AddControl(lstDamageTypes, new Rectangle(5, 110, 125, 90));
                lstDamageTypes.AddColumn(typeof(HudCheckBox), 5, null);
                lstDamageTypes.AddColumn(typeof(HudStaticText), 110, null);
                lstDamageTypes.AddColumn(typeof(HudStaticText), 1, null);

                HudStaticText lblEnabled10025 = new HudStaticText();
                //   lblChiefGearSwitch.FontHeight = nmenuFontHeight;
                lblEnabled10025.Text = "Enabled";
                ChiefGearInspectPageMenuTabProperties.AddControl(lblEnabled10025, new Rectangle(5, 205, 40, 16));

                HudStaticText lblRuleReqSkilla = new HudStaticText();
                //   lblChiefGearSwitch.FontHeight = nmenuFontHeight;
                lblRuleReqSkilla.Text = "SkillLevel";
                ChiefGearInspectPageMenuTabProperties.AddControl(lblRuleReqSkilla, new Rectangle(55, 205, 75, 16));

                chkRuleWeaponsa = new HudCheckBox();
                chkRuleWeaponsa.Text = "";
                ChiefGearInspectPageMenuTabProperties.AddControl(chkRuleWeaponsa, new Rectangle(15, 225, 40, 16));
                chkRuleWeaponsa.Checked = true;

                txtRuleReqSkilla = new HudTextBox();
                txtRuleReqSkilla.Text = "355";
                ChiefGearInspectPageMenuTabProperties.AddControl(txtRuleReqSkilla, new Rectangle(55, 225, 75, 16));

                chkRuleWeaponsb = new HudCheckBox();
                chkRuleWeaponsb.Text = "";
                ChiefGearInspectPageMenuTabProperties.AddControl(chkRuleWeaponsb, new Rectangle(15, 245, 40, 16));
                chkRuleWeaponsb.Checked = true;

                txtRuleReqSkillb = new HudTextBox();
                txtRuleReqSkillb.Text = "375";
                ChiefGearInspectPageMenuTabProperties.AddControl(txtRuleReqSkillb, new Rectangle(55, 245, 75, 16));

                chkRuleWeaponsc = new HudCheckBox();
                chkRuleWeaponsc.Text = "";
                ChiefGearInspectPageMenuTabProperties.AddControl(chkRuleWeaponsc, new Rectangle(15, 265, 40, 16));
                chkRuleWeaponsc.Checked = true;

                txtRuleReqSkillc = new HudTextBox();
                txtRuleReqSkillc.Text = "385";
                ChiefGearInspectPageMenuTabProperties.AddControl(txtRuleReqSkillc, new Rectangle(55, 265, 75, 16));

                chkRuleWeaponsd = new HudCheckBox();
                chkRuleWeaponsd.Text = "";
                ChiefGearInspectPageMenuTabProperties.AddControl(chkRuleWeaponsd, new Rectangle(15, 285, 40, 16));
                chkRuleWeaponsd.Checked = false;

                txtRuleReqSkilld = new HudTextBox();
                txtRuleReqSkilld.Text = "";
                ChiefGearInspectPageMenuTabProperties.AddControl(txtRuleReqSkilld, new Rectangle(55, 285, 75, 16));

                //Sets
                HudStaticText lblSets = new HudStaticText();
                //   lblChiefGearSwitch.FontHeight = nmenuFontHeight;
                lblSets.Text = "Sets";
                ChiefGearInspectPageMenuTabProperties.AddControl(lblSets, new Rectangle(150, 5, 110, 16));


                lstRuleSets = new HudList();
                lstRuleSetsListRow = new HudList.HudListRowAccessor();

                ChiefGearInspectPageMenuTabProperties.AddControl(lstRuleSets, new Rectangle(150, 25, 200, 255));
                lstRuleSets.AddColumn(typeof(HudCheckBox), 5, null);
                lstRuleSets.AddColumn(typeof(HudStaticText), 195, null);
                lstRuleSets.AddColumn(typeof(HudStaticText), 1, null);

                SubscribeChiefGearInspectPageMenuTabViewPageSearchRulePropertiesEvents();
            }
            catch (Exception ex) { LogError(ex); }

        }

        private void DisposeChiefGearInspectPageMenuTabProperties()
        {
            try
            {
                if (cboWeaponAppliesTo != null) { cboWeaponAppliesTo.Dispose(); }
                if (cboMasteryType != null) { cboMasteryType.Dispose(); }
                if (lstDamageTypes != null) { lstDamageTypes.Dispose(); }
                if (chkRuleWeaponsa != null) { chkRuleWeaponsa.Dispose(); }
                if (chkRuleWeaponsb != null) { chkRuleWeaponsb.Dispose(); }
                if (chkRuleWeaponsc != null) { chkRuleWeaponsc.Dispose(); }
                if (chkRuleWeaponsd != null) { chkRuleWeaponsd.Dispose(); }
                if (txtRuleReqSkilla != null) { txtRuleReqSkilla.Dispose(); }
                if (txtRuleReqSkillb != null) { txtRuleReqSkillb.Dispose(); }
                if (txtRuleReqSkillc != null) { txtRuleReqSkillc.Dispose(); }
                if (txtRuleReqSkilld != null) { txtRuleReqSkilld.Dispose(); }
                if (lstRuleSets != null) { lstRuleSets.Dispose(); }


                 unsubscribeChiefGearInspectPageMenuTabViewPageSearchRulePropertiesEvents();
            }
            catch (Exception ex) { LogError(ex); }

        }

        private void SubscribeChiefGearInspectPageMenuTabViewPageSearchRulePropertiesEvents()
        {
            try
            {
                cboWeaponAppliesTo.Change += (sender, index) => cboWeaponAppliesTo_Change(sender, index);
                cboMasteryType.Change += (sender, index) => cboMasteryType_Change(sender, index);
                lstDamageTypes.Click += (sender, row, col) => lstDamageTypes_Click(sender, row, col);
                chkRuleWeaponsa.Change += chkRuleWeaponsa_Change;
                chkRuleWeaponsb.Change += chkRuleWeaponsb_Change;
                chkRuleWeaponsc.Change += chkRuleWeaponsc_Change;
                chkRuleWeaponsd.Change += chkRuleWeaponsd_Change;
                txtRuleReqSkilla.LostFocus += txtRuleReqSkilla_LostFocus;
                txtRuleReqSkillb.LostFocus += txtRuleReqSkillb_LostFocus;
                txtRuleReqSkillc.LostFocus += txtRuleReqSkillc_LostFocus;
                txtRuleReqSkilld.LostFocus += txtRuleReqSkilld_LostFocus;
                 lstRuleSets.Click += (sender, row, col) => lstRuleSets_Click(sender, row, col);

             }
            catch (Exception ex) { LogError(ex); }


        }

        private void unsubscribeChiefGearInspectPageMenuTabViewPageSearchRulePropertiesEvents()
        {
            try
            {
                if (cboWeaponAppliesTo != null) { cboWeaponAppliesTo.Change -= (sender, index) => cboWeaponAppliesTo_Change(sender, index); }
                if (cboMasteryType != null) { cboMasteryType.Change -= (sender, index) => cboMasteryType_Change(sender, index); }
                if (chkRuleWeaponsa != null) { chkRuleWeaponsa.Change -= chkRuleWeaponsa_Change;}
                if (chkRuleWeaponsb != null) {chkRuleWeaponsb.Change -= chkRuleWeaponsb_Change;}
                if (chkRuleWeaponsc != null) {chkRuleWeaponsc.Change -= chkRuleWeaponsc_Change;}
                if (chkRuleWeaponsd != null) {chkRuleWeaponsd.Change -= chkRuleWeaponsd_Change;}
                if (txtRuleReqSkilla != null) {txtRuleReqSkilla.LostFocus -= txtRuleReqSkilla_LostFocus;}
                if (txtRuleReqSkillb != null) {txtRuleReqSkillb.LostFocus -= txtRuleReqSkillb_LostFocus;}
                if (txtRuleReqSkillc != null) {txtRuleReqSkillc.LostFocus -= txtRuleReqSkillc_LostFocus;}
                if (txtRuleReqSkilld != null) {txtRuleReqSkilld.LostFocus -= txtRuleReqSkilld_LostFocus;}
                if (lstRuleSets != null) { lstRuleSets.Click -= (sender, row, col) => lstRuleSets_Click(sender, row, col); }
 



            }
            catch (Exception ex) { LogError(ex); }


        }


        private void RenderChiefGearInspectPageMenuTabAppearance()
        {
            try
            {
                HudStaticText lblRuleArmorTypes = new HudStaticText();
                //   lblChiefGearSwitch.FontHeight = nmenuFontHeight;
                lblRuleArmorTypes.Text = "Armor Models";
                ChiefGearInspectPageMenuTabAppearance.AddControl(lblRuleArmorTypes, new Rectangle(5, 0, 100, 16));


                lstRuleArmorTypes = new HudList();
                lstRuleArmorTypesListRow = new HudList.HudListRowAccessor();

                ChiefGearInspectPageMenuTabAppearance.AddControl(lstRuleArmorTypes, new Rectangle(5, 20, 150, 255));
                lstRuleArmorTypes.AddColumn(typeof(HudCheckBox), 5, null);
                lstRuleArmorTypes.AddColumn(typeof(HudStaticText), 195, null);
                lstRuleArmorTypes.AddColumn(typeof(HudStaticText), 1, null);

                HudStaticText lblColorPalettes = new HudStaticText();
                //   lblChiefGearSwitch.FontHeight = nmenuFontHeight;
                lblColorPalettes.Text = "Color Palettes";
                ChiefGearInspectPageMenuTabAppearance.AddControl(lblColorPalettes, new Rectangle(155, 0, 80, 16));
                SubscribeChiefGearInspectPageMenuTabViewPageSearchRuleAppearanceEvents();



            }
            catch (Exception ex) { LogError(ex); }

        }

        private void SubscribeChiefGearInspectPageMenuTabViewPageSearchRuleAppearanceEvents()
        {
            try
            {
                lstRuleArmorTypes.Click += (sender, row, col) => lstRuleArmorTypes_Click(sender, row, col);

             }
            catch (Exception ex) { LogError(ex); }
        }

        private void unsubscribeChiefGearInspectPageMenuTabViewPageSearchRuleAppearanceEvents()
        {
            try
            {
                lstRuleArmorTypes.Click -= (sender, row, col) => lstRuleArmorTypes_Click(sender, row, col);

            }
            catch (Exception ex) { LogError(ex); }
        }


        private void DisposeChiefGearInspectPageMenuTabAppearance()
        {
            try
            {
               if(lstRuleArmorTypes != null) {lstRuleArmorTypes.Dispose();}
            }
            catch (Exception ex) { LogError(ex); }

        }

        private void RenderChiefGearInspectPageMenuTabReqSpells()
        {
            try
            {
                HudStaticText lblCurrentSpells = new HudStaticText();
                //   lblChiefGearSwitch.FontHeight = nmenuFontHeight;
                lblCurrentSpells.Text = "Current Spells";
                ChiefGearInspectPageMenuTabReqSpells.AddControl(lblCurrentSpells, new Rectangle(5, 0, 130, 16));


                lstRuleSpellsEnabled = new HudList();
                lstRuleSpellsEnabledListRow = new HudList.HudListRowAccessor();

                ChiefGearInspectPageMenuTabReqSpells.AddControl(lstRuleSpellsEnabled, new Rectangle(5, 20, 220, 200));
                lstRuleSpellsEnabled.AddColumn(typeof(HudCheckBox), 5, null);
                lstRuleSpellsEnabled.AddColumn(typeof(HudStaticText), 195, null);
                lstRuleSpellsEnabled.AddColumn(typeof(HudStaticText), 1, null);

                HudStaticText lblRuleMoreSpells = new HudStaticText();
                //   lblChiefGearSwitch.FontHeight = nmenuFontHeight;
                lblRuleMoreSpells.Text = "Available Spells";
                ChiefGearInspectPageMenuTabReqSpells.AddControl(lblRuleMoreSpells, new Rectangle(250, 0, 180, 16));


                lstRuleSpells = new HudList();
                lstRuleSpellsListRow = new HudList.HudListRowAccessor();

                ChiefGearInspectPageMenuTabReqSpells.AddControl(lstRuleSpells, new Rectangle(250, 20, 250, 200));
                lstRuleSpells.AddColumn(typeof(HudCheckBox), 5, null);
                lstRuleSpells.AddColumn(typeof(HudStaticText), 195, null);
                lstRuleSpells.AddColumn(typeof(HudStaticText), 1, null);

                HudStaticText lblRuleFilterSpells = new HudStaticText();
                //   lblChiefGearSwitch.FontHeight = nmenuFontHeight;
                lblRuleFilterSpells.Text = "Filter Spells by:";
                ChiefGearInspectPageMenuTabReqSpells.AddControl(lblRuleFilterSpells, new Rectangle(5, 230, 130, 16));
 
                chkRuleFilterlvl8 = new HudCheckBox();
                chkRuleFilterlvl8.Text = "lvl 8";
                ChiefGearInspectPageMenuTabReqSpells.AddControl(chkRuleFilterlvl8, new Rectangle(5, 250, 70, 16));
                chkRuleFilterlvl8.Checked = false;

                chkRuleFilterLegend = new HudCheckBox();
                chkRuleFilterLegend.Text = "Legendary";
                ChiefGearInspectPageMenuTabReqSpells.AddControl(chkRuleFilterLegend, new Rectangle(80, 250, 70, 16));
                chkRuleFilterLegend.Checked = false;

                chkRuleFilterEpic = new HudCheckBox();
                chkRuleFilterEpic.Text = "Epic";
                ChiefGearInspectPageMenuTabReqSpells.AddControl(chkRuleFilterEpic, new Rectangle(160, 250, 70, 16));
                chkRuleFilterEpic.Checked = false;

                chkRuleFilterMajor = new HudCheckBox();
                chkRuleFilterMajor.Text = "Major";
                ChiefGearInspectPageMenuTabReqSpells.AddControl(chkRuleFilterMajor, new Rectangle(240, 250, 70, 16));
                chkRuleFilterMajor.Checked = false;

                chkRuleFilterCloak = new HudCheckBox();
                chkRuleFilterCloak.Text = "Cloak";
                ChiefGearInspectPageMenuTabReqSpells.AddControl(chkRuleFilterCloak, new Rectangle(320, 250, 70, 16));
                chkRuleFilterCloak.Checked = false;

                SubscribeChiefGearInspectPageMenuTabViewPageSearchRuleReqSpellsEvents();

            }
            catch (Exception ex) { LogError(ex); }

        }

        private void DisposeChiefGearInspectPageMenuTabReqSpells()
        {
            try
            {
                if (lstRuleSpellsEnabled != null) { lstRuleSpellsEnabled.Dispose(); }
                if (lstRuleSpells != null) { lstRuleSpells.Dispose(); }
                if (chkRuleFilterlvl8 != null) { chkRuleFilterlvl8.Dispose(); }
                if (chkRuleFilterLegend != null) { chkRuleFilterLegend.Dispose(); }
                if (chkRuleFilterEpic != null) { chkRuleFilterEpic.Dispose(); }
                if (chkRuleFilterMajor != null) { chkRuleFilterMajor.Dispose(); }
                if (chkRuleFilterCloak != null) { chkRuleFilterCloak.Dispose(); }


            }
            catch (Exception ex) { LogError(ex); }

        }

        private void SubscribeChiefGearInspectPageMenuTabViewPageSearchRuleReqSpellsEvents()
        {
            try
            {
                lstRuleSpellsEnabled.Click += (sender, row, col) => lstRuleSpellsEnabled_Click(sender, row, col);
                lstRuleSpells.Click += (sender, row, col) => lstRuleSpells_Click(sender, row, col);
                chkRuleFilterlvl8.Change += chkRuleFilterlvl8_Change;
                chkRuleFilterLegend.Change += chkRuleFilterLegend_Change;
                chkRuleFilterEpic.Change += chkRuleFilterEpic_Change;
                chkRuleFilterMajor.Change += chkRuleFilterMajor_Change;
                chkRuleFilterCloak.Change += chkRuleFilterCloak_Change;
 

            }
            catch (Exception ex) { LogError(ex); }
        }

        private void unsubscribeChiefGearInspectPageMenuTabViewPageSearchRuleReqSpellsEvents()
        {
            try
            {
                lstRuleSpellsEnabled.Click -= (sender, row, col) => lstRuleSpellsEnabled_Click(sender, row, col);
                lstRuleSpells.Click -= (sender, row, col) => lstRuleSpells_Click(sender, row, col);
                chkRuleFilterlvl8.Change -= chkRuleFilterlvl8_Change;
                chkRuleFilterLegend.Change -= chkRuleFilterLegend_Change;
                chkRuleFilterEpic.Change -= chkRuleFilterEpic_Change;
                chkRuleFilterMajor.Change -= chkRuleFilterMajor_Change;
                chkRuleFilterCloak.Change -= chkRuleFilterCloak_Change;


            }
            catch (Exception ex) { LogError(ex); }
        }



        private void RenderChiefGearInspectPageMenuTabAdvanced()
        {
            try
            {
                int i = 0;

                chkAdvEnabled = new HudCheckBox();
                chkAdvEnabled.Text = "Enabled";
                ChiefGearInspectPageMenuTabAdvanced.AddControl(chkAdvEnabled, new Rectangle(5, 5, 50, 16));
                chkAdvEnabled.Checked = false;


                HudStaticText lblAdvKey = new HudStaticText();
                //   lblChiefGearSwitch.FontHeight = nmenuFontHeight;
                lblAdvKey.Text = "Key Type";
                ChiefGearInspectPageMenuTabAdvanced.AddControl(lblAdvKey, new Rectangle(5, 25, 100, 16));


                HudStaticText lblAdvKeyName = new HudStaticText();
                //   lblChiefGearSwitch.FontHeight = nmenuFontHeight;
                lblAdvKeyName.Text = "Key Name";
                ChiefGearInspectPageMenuTabAdvanced.AddControl(lblAdvKeyName, new Rectangle(75, 25, 100, 16));


                HudStaticText lblAdvKeyComparison = new HudStaticText();
                //   lblChiefGearSwitch.FontHeight = nmenuFontHeight;
                lblAdvKeyComparison.Text = "Comparison";
                ChiefGearInspectPageMenuTabAdvanced.AddControl(lblAdvKeyComparison, new Rectangle(275, 25, 100, 16));


                HudStaticText lblAdvKeyValue = new HudStaticText();
                //   lblChiefGearSwitch.FontHeight = nmenuFontHeight;
                lblAdvKeyValue.Text = "Key Value";
                ChiefGearInspectPageMenuTabAdvanced.AddControl(lblAdvKeyValue, new Rectangle(380, 25, 100, 16));

                ControlGroup cboAdv1KeyTypeChoices = new ControlGroup();
                cboAdv1KeyType = new HudCombo(cboAdv1KeyTypeChoices);
                i = 0;
                cboList = new List<IDNameLoadable>();
                foreach (IDNameLoadable info in cboList)
                {
                    cboAdv1KeyType.AddItem(info.name, i);
                    i++;
                }
                cboAdv1KeyType.Current = 0;
                ChiefGearInspectPageMenuTabAdvanced.AddControl(cboAdv1KeyType, new Rectangle(5, 45, 60, 20));


                ControlGroup cboAdv1KeyChoices = new ControlGroup();
                cboAdv1Key = new HudCombo(cboAdv1KeyChoices);
                i = 0;
                cboList = new List<IDNameLoadable>();
                foreach (IDNameLoadable info in cboList)
                {
                    cboAdv1Key.AddItem(info.name, i);
                    i++;
                }
                cboAdv1Key.Current = 0;
                ChiefGearInspectPageMenuTabAdvanced.AddControl(cboAdv1Key, new Rectangle(75, 45, 175, 20));

                ControlGroup cboAdv1KeyCompareChoices = new ControlGroup();
                cboAdv1KeyCompare = new HudCombo(cboAdv1KeyCompareChoices);
                i = 0;
                cboList = new List<IDNameLoadable>();
                foreach (IDNameLoadable info in cboList)
                {
                    cboAdv1KeyCompare.AddItem(info.name, i);
                    i++;
                }
                cboAdv1Key.Current = 0;
                ChiefGearInspectPageMenuTabAdvanced.AddControl(cboAdv1KeyCompare, new Rectangle(275, 45, 75, 20));

                txtAdv1KeyValue = new HudTextBox();
                txtAdv1KeyValue.Text = "";
                ChiefGearInspectPageMenuTabAdvanced.AddControl(txtAdv1KeyValue, new Rectangle(380, 45, 75, 16));


                ControlGroup cboAdv1LinkChoices = new ControlGroup();
                cboAdv1Link = new HudCombo(cboAdv1LinkChoices);
                i = 0;
                cboList = new List<IDNameLoadable>();
                foreach (IDNameLoadable info in cboList)
                {
                    cboAdv1Link.AddItem(info.name, i);
                    i++;
                }
                cboAdv1Link.Current = 0;
                ChiefGearInspectPageMenuTabAdvanced.AddControl(cboAdv1Link, new Rectangle(5, 65, 60, 20));


                //Second group of new advanced rule controls
                ControlGroup cboAdv2KeyTypeChoices = new ControlGroup();
                cboAdv2KeyType = new HudCombo(cboAdv2KeyTypeChoices);
                i = 0;
                cboList = new List<IDNameLoadable>();
                foreach (IDNameLoadable info in cboList)
                {
                    cboAdv2KeyType.AddItem(info.name, i);
                    i++;
                }
                cboAdv2KeyType.Current = 0;
                ChiefGearInspectPageMenuTabAdvanced.AddControl(cboAdv2KeyType, new Rectangle(5, 85, 60, 20));


                ControlGroup cboAdv2KeyChoices = new ControlGroup();
                cboAdv2Key = new HudCombo(cboAdv2KeyChoices);
                i = 0;
                cboList = new List<IDNameLoadable>();
                foreach (IDNameLoadable info in cboList)
                {
                    cboAdv2Key.AddItem(info.name, i);
                    i++;
                }
                cboAdv2Key.Current = 0;
                ChiefGearInspectPageMenuTabAdvanced.AddControl(cboAdv2Key, new Rectangle(75, 85, 175, 20));

                ControlGroup cboAdv2KeyCompareChoices = new ControlGroup();
                cboAdv2KeyCompare = new HudCombo(cboAdv2KeyCompareChoices);
                i = 0;
                cboList = new List<IDNameLoadable>();
                foreach (IDNameLoadable info in cboList)
                {
                    cboAdv2KeyCompare.AddItem(info.name, i);
                    i++;
                }
                cboAdv2Key.Current = 0;
                ChiefGearInspectPageMenuTabAdvanced.AddControl(cboAdv2KeyCompare, new Rectangle(275, 85, 75, 20));

                txtAdv2KeyValue = new HudTextBox();
                txtAdv2KeyValue.Text = "";
                ChiefGearInspectPageMenuTabAdvanced.AddControl(txtAdv2KeyValue, new Rectangle(380, 85, 75, 16));


                ControlGroup cboAdv2LinkChoices = new ControlGroup();
                cboAdv2Link = new HudCombo(cboAdv2LinkChoices);
                i = 0;
                cboList = new List<IDNameLoadable>();
                foreach (IDNameLoadable info in cboList)
                {
                    cboAdv1Link.AddItem(info.name, i);
                    i++;
                }
                cboAdv2Link.Current = 0;
                ChiefGearInspectPageMenuTabAdvanced.AddControl(cboAdv2Link, new Rectangle(5, 105, 60, 20));

                //Third group of new advanced rule controls
                ControlGroup cboAdv3KeyTypeChoices = new ControlGroup();
                cboAdv3KeyType = new HudCombo(cboAdv3KeyTypeChoices);
                i = 0;
                cboList = new List<IDNameLoadable>();
                foreach (IDNameLoadable info in cboList)
                {
                    cboAdv3KeyType.AddItem(info.name, i);
                    i++;
                }
                cboAdv3KeyType.Current = 0;
                ChiefGearInspectPageMenuTabAdvanced.AddControl(cboAdv3KeyType, new Rectangle(5, 125, 60, 20));


                ControlGroup cboAdv3KeyChoices = new ControlGroup();
                cboAdv3Key = new HudCombo(cboAdv3KeyChoices);
                i = 0;
                cboList = new List<IDNameLoadable>();
                foreach (IDNameLoadable info in cboList)
                {
                    cboAdv3Key.AddItem(info.name, i);
                    i++;
                }
                cboAdv3Key.Current = 0;
                ChiefGearInspectPageMenuTabAdvanced.AddControl(cboAdv3Key, new Rectangle(75, 125, 175, 20));

                ControlGroup cboAdv3KeyCompareChoices = new ControlGroup();
                cboAdv3KeyCompare = new HudCombo(cboAdv3KeyCompareChoices);
                i = 0;
                cboList = new List<IDNameLoadable>();
                foreach (IDNameLoadable info in cboList)
                {
                    cboAdv3KeyCompare.AddItem(info.name, i);
                    i++;
                }
                cboAdv3Key.Current = 0;
                ChiefGearInspectPageMenuTabAdvanced.AddControl(cboAdv3KeyCompare, new Rectangle(275, 125, 75, 20));

                txtAdv3KeyValue = new HudTextBox();
                txtAdv3KeyValue.Text = "";
                ChiefGearInspectPageMenuTabAdvanced.AddControl(txtAdv3KeyValue, new Rectangle(380, 125, 75, 16));


                ControlGroup cboAdv3LinkChoices = new ControlGroup();
                cboAdv3Link = new HudCombo(cboAdv3LinkChoices);
                i = 0;
                cboList = new List<IDNameLoadable>();
                foreach (IDNameLoadable info in cboList)
                {
                    cboAdv1Link.AddItem(info.name, i);
                    i++;
                }
                cboAdv3Link.Current = 0;
                ChiefGearInspectPageMenuTabAdvanced.AddControl(cboAdv3Link, new Rectangle(5, 145, 60, 20));

                //Fourth group of new advanced rule controls
                ControlGroup cboAdv4KeyTypeChoices = new ControlGroup();
                cboAdv4KeyType = new HudCombo(cboAdv4KeyTypeChoices);
                i = 0;
                cboList = new List<IDNameLoadable>();
                foreach (IDNameLoadable info in cboList)
                {
                    cboAdv4KeyType.AddItem(info.name, i);
                    i++;
                }
                cboAdv4KeyType.Current = 0;
                ChiefGearInspectPageMenuTabAdvanced.AddControl(cboAdv4KeyType, new Rectangle(5, 165, 60, 20));


                ControlGroup cboAdv4KeyChoices = new ControlGroup();
                cboAdv4Key = new HudCombo(cboAdv4KeyChoices);
                i = 0;
                cboList = new List<IDNameLoadable>();
                foreach (IDNameLoadable info in cboList)
                {
                    cboAdv4Key.AddItem(info.name, i);
                    i++;
                }
                cboAdv4Key.Current = 0;
                ChiefGearInspectPageMenuTabAdvanced.AddControl(cboAdv4Key, new Rectangle(75, 165, 175, 20));

                ControlGroup cboAdv4KeyCompareChoices = new ControlGroup();
                cboAdv4KeyCompare = new HudCombo(cboAdv4KeyCompareChoices);
                i = 0;
                cboList = new List<IDNameLoadable>();
                foreach (IDNameLoadable info in cboList)
                {
                    cboAdv4KeyCompare.AddItem(info.name, i);
                    i++;
                }
                cboAdv4Key.Current = 0;
                ChiefGearInspectPageMenuTabAdvanced.AddControl(cboAdv4KeyCompare, new Rectangle(275, 165, 75, 20));

                txtAdv4KeyValue = new HudTextBox();
                txtAdv4KeyValue.Text = "";
                ChiefGearInspectPageMenuTabAdvanced.AddControl(txtAdv4KeyValue, new Rectangle(380, 165, 75, 16));


                ControlGroup cboAdv4LinkChoices = new ControlGroup();
                cboAdv4Link = new HudCombo(cboAdv4LinkChoices);
                i = 0;
                cboList = new List<IDNameLoadable>();
                foreach (IDNameLoadable info in cboList)
                {
                    cboAdv1Link.AddItem(info.name, i);
                    i++;
                }
                cboAdv4Link.Current = 0;
                ChiefGearInspectPageMenuTabAdvanced.AddControl(cboAdv4Link, new Rectangle(5, 185, 60, 20));

                //Fifth group of new advanced rule controls
                ControlGroup cboAdv5KeyTypeChoices = new ControlGroup();
                cboAdv5KeyType = new HudCombo(cboAdv5KeyTypeChoices);
                i = 0;
                cboList = new List<IDNameLoadable>();
                foreach (IDNameLoadable info in cboList)
                {
                    cboAdv5KeyType.AddItem(info.name, i);
                    i++;
                }
                cboAdv5KeyType.Current = 0;
                ChiefGearInspectPageMenuTabAdvanced.AddControl(cboAdv5KeyType, new Rectangle(5, 205, 60, 20));


                ControlGroup cboAdv5KeyChoices = new ControlGroup();
                cboAdv5Key = new HudCombo(cboAdv5KeyChoices);
                i = 0;
                cboList = new List<IDNameLoadable>();
                foreach (IDNameLoadable info in cboList)
                {
                    cboAdv5Key.AddItem(info.name, i);
                    i++;
                }
                cboAdv5Key.Current = 0;
                ChiefGearInspectPageMenuTabAdvanced.AddControl(cboAdv5Key, new Rectangle(75, 205, 175, 20));

                ControlGroup cboAdv5KeyCompareChoices = new ControlGroup();
                cboAdv5KeyCompare = new HudCombo(cboAdv5KeyCompareChoices);
                i = 0;
                cboList = new List<IDNameLoadable>();
                foreach (IDNameLoadable info in cboList)
                {
                    cboAdv5KeyCompare.AddItem(info.name, i);
                    i++;
                }
                cboAdv5Key.Current = 0;
                ChiefGearInspectPageMenuTabAdvanced.AddControl(cboAdv5KeyCompare, new Rectangle(275, 205, 75, 20));

                txtAdv5KeyValue = new HudTextBox();
                txtAdv5KeyValue.Text = "";
                ChiefGearInspectPageMenuTabAdvanced.AddControl(txtAdv5KeyValue, new Rectangle(380, 165, 75, 16));




             }
            catch (Exception ex) { LogError(ex); }

        }

        private void DisposeChiefGearInspectPageMenuTabAdvanced()
        {
            try
            {
                if (chkAdvEnabled != null) { chkAdvEnabled.Dispose(); }
                if (cboAdv1KeyType != null) { cboAdv1KeyType.Dispose(); }
                if (cboAdv1Key != null) { cboAdv1Key.Dispose(); }
                if (cboAdv1KeyCompare != null) { cboAdv1KeyCompare.Dispose(); }
                if (txtAdv1KeyValue != null) { txtAdv1KeyValue.Dispose(); }
                if (cboAdv1Link != null) { cboAdv1Link.Dispose(); }
                if (cboAdv2KeyType != null) { cboAdv2KeyType.Dispose(); }
                if (cboAdv2Key != null) { cboAdv2Key.Dispose(); }
                if (cboAdv2KeyCompare != null) { cboAdv2KeyCompare.Dispose(); }
                if (txtAdv2KeyValue != null) { txtAdv2KeyValue.Dispose(); }
                if (cboAdv2Link != null) { cboAdv2Link.Dispose(); }
                if (cboAdv3KeyType != null) { cboAdv3KeyType.Dispose(); }
                if (cboAdv3Key != null) { cboAdv3Key.Dispose(); }
                if (cboAdv3KeyCompare != null) { cboAdv3KeyCompare.Dispose(); }
                if (txtAdv3KeyValue != null) { txtAdv3KeyValue.Dispose(); }
                if (cboAdv3Link != null) { cboAdv3Link.Dispose(); }
                if (cboAdv4KeyType != null) { cboAdv4KeyType.Dispose(); }
                if (cboAdv4Key != null) { cboAdv4Key.Dispose(); }
                if (cboAdv4KeyCompare != null) { cboAdv4KeyCompare.Dispose(); }
                if (txtAdv4KeyValue != null) { txtAdv4KeyValue.Dispose(); }
                if (cboAdv4Link != null) { cboAdv4Link.Dispose(); }
                if (cboAdv5KeyType != null) { cboAdv5KeyType.Dispose(); }
                if (cboAdv5Key != null) { cboAdv5Key.Dispose(); }
                if (cboAdv5KeyCompare != null) { cboAdv5KeyCompare.Dispose(); }
                if (txtAdv5KeyValue != null) { txtAdv5KeyValue.Dispose(); }

            }
            catch (Exception ex) { LogError(ex); }

        }

        private void SubscribeChiefGearInspectPageMenuTabAdvanced()
        {
            try
            {
                chkAdvEnabled.Change += chkAdvEnabled_Change;
                cboAdv1KeyType.Change += (sender, index) => cboAdv1KeyType_Change(sender, index);
                cboAdv1Key.Change += (sender, index) => cboAdv1Key_Change(sender, index);
                cboAdv1KeyCompare.Change += (sender, index) => cboAdv1KeyCompare_Change(sender, index);
                txtAdv1KeyValue.LostFocus += txtAdv1KeyValue_LostFocus;
                cboAdv1Link.Change += (sender, index) => cboAdv1Link_Change(sender, index);

                cboAdv2KeyType.Change += (sender, index) => cboAdv2KeyType_Change(sender, index);
                cboAdv2Key.Change += (sender, index) => cboAdv2Key_Change(sender, index);
                cboAdv2KeyCompare.Change += (sender, index) => cboAdv2KeyCompare_Change(sender, index);
                txtAdv2KeyValue.LostFocus += txtAdv2KeyValue_LostFocus;
                cboAdv2Link.Change += (sender, index) => cboAdv2Link_Change(sender, index);

                cboAdv3KeyType.Change += (sender, index) => cboAdv3KeyType_Change(sender, index);
                cboAdv3Key.Change += (sender, index) => cboAdv3Key_Change(sender, index);
                cboAdv3KeyCompare.Change += (sender, index) => cboAdv3KeyCompare_Change(sender, index);
                txtAdv3KeyValue.LostFocus += txtAdv3KeyValue_LostFocus;
                cboAdv3Link.Change += (sender, index) => cboAdv3Link_Change(sender, index);

                cboAdv4KeyType.Change += (sender, index) => cboAdv4KeyType_Change(sender, index);
                cboAdv4Key.Change += (sender, index) => cboAdv4Key_Change(sender, index);
                cboAdv4KeyCompare.Change += (sender, index) => cboAdv4KeyCompare_Change(sender, index);
                txtAdv4KeyValue.LostFocus += txtAdv4KeyValue_LostFocus;
                cboAdv4Link.Change += (sender, index) => cboAdv4Link_Change(sender, index);

                cboAdv5KeyType.Change += (sender, index) => cboAdv5KeyType_Change(sender, index);
                cboAdv5Key.Change += (sender, index) => cboAdv5Key_Change(sender, index);
                cboAdv5KeyCompare.Change += (sender, index) => cboAdv5KeyCompare_Change(sender, index);
                txtAdv5KeyValue.LostFocus += txtAdv5KeyValue_LostFocus;


            }
            catch (Exception ex) { LogError(ex); }
        }

        private void unsubscribeChiefGearInspectPageMenuTabAdvanced()
        {
            try
            {
                if (chkAdvEnabled != null) { chkAdvEnabled.Change += chkAdvEnabled_Change; }
                if (chkAdvEnabled != null) {cboAdv1KeyType.Change += (sender, index) => cboAdv1KeyType_Change(sender, index);}
                if (chkAdvEnabled != null) {cboAdv1Key.Change += (sender, index) => cboAdv1Key_Change(sender, index);}
                if (chkAdvEnabled != null) {cboAdv1KeyCompare.Change += (sender, index) => cboAdv1KeyCompare_Change(sender, index);}
                if (chkAdvEnabled != null) {txtAdv1KeyValue.LostFocus += txtAdv1KeyValue_LostFocus;}
                if (chkAdvEnabled != null) {cboAdv1Link.Change += (sender, index) => cboAdv1Link_Change(sender, index);}

                if (cboAdv2KeyType != null) {cboAdv2KeyType.Change += (sender, index) => cboAdv2KeyType_Change(sender, index);}
                if (cboAdv2Key != null) {cboAdv2Key.Change += (sender, index) => cboAdv2Key_Change(sender, index);}
                if (cboAdv2KeyCompare != null) {cboAdv2KeyCompare.Change += (sender, index) => cboAdv2KeyCompare_Change(sender, index);}
                if (txtAdv2KeyValue != null) {txtAdv2KeyValue.LostFocus += txtAdv2KeyValue_LostFocus;}
                if (cboAdv2Link != null) {cboAdv2Link.Change += (sender, index) => cboAdv2Link_Change(sender, index);}

                if (cboAdv3KeyType != null) {cboAdv3KeyType.Change += (sender, index) => cboAdv3KeyType_Change(sender, index);}
                if (cboAdv3Key != null) { cboAdv3Key.Change += (sender, index) => cboAdv3Key_Change(sender, index); }
                if (cboAdv3KeyCompare != null) {cboAdv3KeyCompare.Change += (sender, index) => cboAdv3KeyCompare_Change(sender, index);}
                if (txtAdv3KeyValue != null) {txtAdv3KeyValue.LostFocus += txtAdv3KeyValue_LostFocus;}
                if (cboAdv3Link != null) {cboAdv3Link.Change += (sender, index) => cboAdv3Link_Change(sender, index);}

                if (cboAdv4KeyType != null) {cboAdv4KeyType.Change += (sender, index) => cboAdv4KeyType_Change(sender, index);}
                if (cboAdv4Key != null) {cboAdv4Key.Change += (sender, index) => cboAdv4Key_Change(sender, index);}
                if (cboAdv4KeyCompare != null) {cboAdv4KeyCompare.Change += (sender, index) => cboAdv4KeyCompare_Change(sender, index);}
                if (txtAdv4KeyValue != null) {txtAdv4KeyValue.LostFocus += txtAdv4KeyValue_LostFocus;}
                if (cboAdv4Link != null) {cboAdv4Link.Change += (sender, index) => cboAdv4Link_Change(sender, index);}

                if (cboAdv5KeyType != null) {cboAdv5KeyType.Change += (sender, index) => cboAdv5KeyType_Change(sender, index);}
                if (cboAdv5Key != null) {cboAdv5Key.Change += (sender, index) => cboAdv5Key_Change(sender, index);}
                if (cboAdv5KeyCompare != null) { cboAdv5KeyCompare.Change += (sender, index) => cboAdv5KeyCompare_Change(sender, index); }
                if (txtAdv5KeyValue != null) {txtAdv5KeyValue.LostFocus += txtAdv5KeyValue_LostFocus;}


            }
            catch (Exception ex) { LogError(ex); }
        }


  
        private void RenderChiefGearInspectPageTrophies()
        {
            try
            {
                WriteToChat("I am in function to render trophy hud.");

                lstmyTrophies = new HudList();
                lstmyTrophiesListRow = new HudList.HudListRowAccessor();
                ChiefGearInspectPageTrophies.AddControl(lstmyTrophies, new Rectangle(5, 5, 260, 300));
                lstmyTrophies.AddColumn(typeof(HudCheckBox), 5, null);
                lstmyTrophies.AddColumn(typeof(HudStaticText), 230, null);
                lstmyTrophies.AddColumn(typeof(HudPictureBox), 10, null);

                txtTrophyName = new HudTextBox();
                txtTrophyName.Text = "";
                ChiefGearInspectPageTrophies.AddControl(txtTrophyName, new Rectangle(8, 320, 150, 20));

                btnUpdateTrophyItem = new HudButton();
                btnUpdateTrophyItem.Text = "Update";
                ChiefGearInspectPageTrophies.AddControl(btnUpdateTrophyItem, new Rectangle(190, 320, 90, 18));

                btnAddTrophyItem = new HudButton();
                btnAddTrophyItem.Text = "Add New Item";
                ChiefGearInspectPageTrophies.AddControl(btnAddTrophyItem, new Rectangle(190, 360, 90, 18));

                chkTrophyExact = new HudCheckBox();
                chkTrophyExact.Text = "Exact Match";
                ChiefGearInspectPageTrophies.AddControl(chkTrophyExact, new Rectangle(8, 370, 80, 16));
                chkTrophyExact.Checked = false;

                txtTrophyMax = new HudTextBox();
                txtTrophyMax.Text = "";
                ChiefGearInspectPageTrophies.AddControl(txtTrophyMax, new Rectangle(5, 390, 100, 20));


                HudStaticText lblMyItemsCountMax = new HudStaticText();
                //   lblChiefGearSwitch.FontHeight = nmenuFontHeight;
                lblMyItemsCountMax.Text = "Max # to Loot:";
                ChiefGearInspectPageTrophies.AddControl(lblMyItemsCountMax, new Rectangle(110, 390, 100, 16));

                SubscribeChiefGearInspectPageTrophies();



            }
            catch (Exception ex) { LogError(ex); }

        }

        private void SubscribeChiefGearInspectPageTrophies()
        {
            lstmyTrophies.Click += (sender, row, col) => lstmyTrophies_Click(sender, row, col);
            txtTrophyName.LostFocus += txtTrophyName_LostFocus;
            btnAddTrophyItem.Hit += (sender, index) => btnAddTrophyItem_Hit(sender, index);
            //btnUpdateTrophyItem.Hit += (sender, index) => btnUpdateTrophyItem_Hit(sender, index);
            //chkTrophyExact.Change += chkTrophyExact_Change;
            //txtTrophyMax.LostFocus -= txtTrophyMax_LostFocus;
        }

        private void UnsubscribeChiefGearInspectPageTrophies()
        {
            lstmyTrophies.Click -= (sender, row, col) => lstmyTrophies_Click(sender, row, col);
            txtTrophyName.LostFocus -= txtTrophyName_LostFocus;
            btnAddTrophyItem.Hit -= (sender, index) => btnAddTrophyItem_Hit(sender, index);
            btnUpdateTrophyItem.Hit -= (sender, index) => btnUpdateTrophyItem_Hit(sender, index);
            chkTrophyExact.Change -= chkTrophyExact_Change;
            txtTrophyMax.LostFocus -= txtTrophyMax_LostFocus;


        }

        private void DisposeChiefGearInspectPageTrophies()
        {
            if (lstmyTrophies != null) { lstmyTrophies.Dispose(); }
            if (txtTrophyName != null) { txtTrophyName.Dispose(); }
            if (btnUpdateTrophyItem != null) { btnUpdateTrophyItem.Dispose(); }
            if (btnAddTrophyItem != null) { btnAddTrophyItem.Dispose(); }
            if (chkTrophyExact != null) { chkTrophyExact.Dispose(); }
            if (txtTrophyMax != null) { txtTrophyMax.Dispose(); }


        }

           private void RenderChiefGearInspectPageMobs()
        {
            try
            {
                WriteToChat("I am in hud to render mobs");

                lstmyMobs = new HudList();
                lstmyMobsListRow = new HudList.HudListRowAccessor();
                ChiefGearInspectPageMobs.AddControl(lstmyMobs, new Rectangle(5, 5, 260, 300));
                lstmyMobs.AddColumn(typeof(HudCheckBox), 5, null);
                lstmyMobs.AddColumn(typeof(HudStaticText), 230, null);
                lstmyMobs.AddColumn(typeof(HudPictureBox), 10, null);

                txtmyMobName = new HudTextBox();
                txtmyMobName.Text = "";
                ChiefGearInspectPageMobs.AddControl(txtmyMobName, new Rectangle(8, 320, 150, 20));

                btnUpdateMobItem = new HudButton();
                btnUpdateMobItem.Text = "Update";
                ChiefGearInspectPageMobs.AddControl(btnUpdateMobItem, new Rectangle(190, 320, 90, 18));

                btnAddMobItem = new HudButton();
                btnAddMobItem.Text = "Add New Mob";
                ChiefGearInspectPageMobs.AddControl(btnAddMobItem, new Rectangle(190, 360, 90, 18));

                chkmyMobExact = new HudCheckBox();
                chkmyMobExact.Text = "Exact Match";
                ChiefGearInspectPageMobs.AddControl(chkmyMobExact, new Rectangle(8, 370, 80, 16));
                chkmyMobExact.Checked = false;


                SubscribeChiefGearInspectPageMobs();

            }
            catch (Exception ex) { LogError(ex); }

        }

        private void SubscribeChiefGearInspectPageMobs()
        {
            lstmyMobs.Click += (sender, row, col) => lstmyMobs_Click(sender, row, col);
            txtmyMobName.LostFocus -= txtmyMobName_LostFocus;
            btnAddMobItem.Hit += (sender, index) => btnAddMobItem_Hit(sender, index);
            btnUpdateMobItem.Hit += (sender, index) => btnUpdateMobItem_Hit(sender, index);
            chkmyMobExact.Change += chkmyMobExact_Change;
        }

        private void UnsubscribeChiefGearInspectPageMobs()
        {
            if (lstmyMobs != null) { lstmyMobs.Click -= (sender, row, col) => lstmyMobs_Click(sender, row, col); }
            if (txtmyMobName != null) { txtTrophyName.LostFocus -= txtTrophyName_LostFocus; }
            if (btnUpdateMobItem != null) { btnAddTrophyItem.Hit -= (sender, index) => btnAddTrophyItem_Hit(sender, index); }
            if (btnAddMobItem != null) { btnUpdateTrophyItem.Hit -= (sender, index) => btnUpdateTrophyItem_Hit(sender, index); }
            if (chkmyMobExact != null) { chkmyMobExact.Change -= chkmyMobExact_Change; }


        }

        private void DisposeChiefGearInspectPageMobs()
        {
            try{
                WriteToChat("I am in function to dispose chiefgearinspectpagemobs");
            if (lstmyMobs != null) { lstmyMobs.Dispose(); }
            if (txtmyMobName != null) { txtmyMobName.Dispose(); }
            if (btnUpdateMobItem != null) { btnUpdateMobItem.Dispose(); }
            if (btnAddMobItem != null) { btnAddMobItem.Dispose(); }
            if (chkmyMobExact != null) { chkmyMobExact.Dispose(); }
            }
            catch (Exception ex) { LogError(ex); }


        }

        private void RenderChiefGearInspectPageSalvage()
        {
            try
            {
                lstNotifySalvage = new HudList();
                lstNotifySalvageListRow = new HudList.HudListRowAccessor();
                ChiefGearInspectPageSalvage.AddControl(lstNotifySalvage, new Rectangle(5, 5, 260, 300));
                lstNotifySalvage.AddColumn(typeof(HudCheckBox), 5, null);
                lstNotifySalvage.AddColumn(typeof(HudStaticText), 130, null);
                 lstNotifySalvage.AddColumn(typeof(HudStaticText), 60, null);
                 lstNotifySalvage.AddColumn(typeof(HudStaticText), 1, null);

                HudStaticText lblSalvagelblName = new HudStaticText();
                //   lblChiefGearSwitch.FontHeight = nmenuFontHeight;
                lblSalvagelblName.Text = "Salvage Material: ";
                ChiefGearInspectPageSalvage.AddControl(lblSalvagelblName, new Rectangle(5,310, 250, 16));

                 lblSalvageName = new HudStaticText();
                //   lblChiefGearSwitch.FontHeight = nmenuFontHeight;
                lblSalvageName.Text = "";
                ChiefGearInspectPageSalvage.AddControl(lblSalvageName, new Rectangle(5, 330, 250, 16));

                 HudStaticText lblSalvageString = new HudStaticText();
                //   lblChiefGearSwitch.FontHeight = nmenuFontHeight;
                lblSalvageString.Text= "Salvage Combine String: ";
                ChiefGearInspectPageSalvage.AddControl(lblSalvageString, new Rectangle(5, 360, 150, 16));

                txtSalvageString = new HudTextBox();
                txtSalvageString.Text = "";
                ChiefGearInspectPageSalvage.AddControl(txtSalvageString, new Rectangle(5, 380, 150, 20));

                btnUpdateSalvage = new HudButton();
                btnUpdateSalvage.Text = "Update";
                ChiefGearInspectPageSalvage.AddControl(btnUpdateSalvage, new Rectangle(5, 410, 90, 18));




                SubscribeChiefGearInspectPageSalvage();

            }
            catch (Exception ex) { LogError(ex); }

        }

        private void SubscribeChiefGearInspectPageSalvage()
        {

            lstNotifySalvage.Click += (sender, row, col) => lstNotifySalvage_Click(sender, row, col);
            lblSalvageName.LostFocus += lblSalvageName_LostFocus;
            txtSalvageString.LostFocus += txtSalvageString_LostFocus;
            btnUpdateSalvage.Hit += (sender, index) => btnUpdateSalvage_Hit(sender, index);

        }

        private void UnsubscribeChiefGearInspectPageSalvage()
        {
            if (lstNotifySalvage != null) { lstNotifySalvage.Click -= (sender, row, col) => lstNotifySalvage_Click(sender, row, col); }
            if (lblSalvageName != null) {lblSalvageName.LostFocus -= lblSalvageName_LostFocus;}
            if (txtSalvageString != null) {txtSalvageString.LostFocus -= txtSalvageString_LostFocus;}
            if (btnUpdateSalvage != null) { btnUpdateSalvage.Hit -= (sender, index) => btnUpdateSalvage_Hit(sender, index); }

        }

        private void DisposeChiefGearInspectPageSalvage()
        {
            try
            {

                if (lstNotifySalvage != null) { lstNotifySalvage.Dispose(); }
                if (lblSalvageName != null) { lblSalvageName.Dispose(); }
                if (txtSalvageString != null) { txtSalvageString.Dispose(); }
                if (btnUpdateSalvage != null) { btnUpdateSalvage.Dispose(); }


            }
            catch (Exception ex) { LogError(ex); }
        }





    }
}