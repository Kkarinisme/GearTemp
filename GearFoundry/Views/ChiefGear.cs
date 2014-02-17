
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
        private HudView ChiefGearHudView = null;
        private HudTabView ChiefGearHudTabView = null;
        private HudFixedLayout ChiefGearHudSettings = null;
        private HudFixedLayout ChiefGearHudInspect = null;
        private HudFixedLayout ChiefGearHudSounds = null;
        private HudFixedLayout ChiefGearHudAbout = null;

        //Controls on Settings Page
        private HudCheckBox chkQuickSlotsv = null;
        private HudCheckBox chkQuickSlotsh = null;
        private HudCheckBox chkGearVisectionEnabled = null;
        private HudCheckBox chkGearSenseEnabled = null;
        private HudCheckBox chkGearButlerEnabled = null;
        private HudCheckBox chkGearInspectorEnabled = null;
        private HudCheckBox chkCombatHudEnabled = null;
        private HudCheckBox chkRemoteGearEnabled = null;
        private HudCheckBox chkPortalGearEnabled = null;
        private HudCheckBox chkKillTaskGearEnabled = null;
        private HudCheckBox chkInventoryHudEnabled = null;
        private HudCheckBox chkInventoryBurden = null;
        private HudCheckBox chkInventoryComplete = null;
        private HudCheckBox chkInventory = null;
        private HudCheckBox chkToonStats = null;
        private HudCheckBox chkArmorHud = null;
        private HudCheckBox chkEnableTextFiltering = null;
        private HudCheckBox chkTextFilterAllStatus = null;
        private HudTextBox txtItemFontHeight;
        private HudTextBox txtMenuFontHeight;

        //Controls on Sounds Page
        private HudCheckBox chkMuteSounds = null;
        private HudCombo cboTrophyLandscape;
        private HudCombo cboMobLandscape;
        private HudCombo cboPlayerLandscape;
        private HudCombo cboCorpseRare;
        private HudCombo cboCorpseSelfKill;
        private HudCombo cboCorpseFellowKill;
        private HudCombo cboDeadMe;
        private HudCombo cboDeadPermitted;
        private HudCombo cboTrophyCorpse;
        private HudCombo cboRuleCorpse;
        private HudCombo cboSalvageCorpse;



 
  
        private void RenderChiefGearHud()
        {


            try
            {
                WriteToChat("I am in RenderChiefGearHud");
                if (ChiefGearHudView != null)
                {
                    DisposeChiefGearHud();
                }

                ChiefGearHudTabView = new HudTabView();
                ChiefGearHudView = new HudView("Gear Foundry", 500, 500, new ACImage(28170));
                ChiefGearHudView.UserAlphaChangeable = false;
                ChiefGearHudView.ShowInBar = true;
                ChiefGearHudView.UserResizeable = false;
                ChiefGearHudView.Visible = true;
                ChiefGearHudView.Ghosted = false;
                ChiefGearHudView.UserMinimizable = true;
                ChiefGearHudView.UserClickThroughable = false;
                ChiefGearHudView.Controls.HeadControl = ChiefGearHudTabView;
                ChiefGearHudSettings = new HudFixedLayout();
                ChiefGearHudTabView.AddTab(ChiefGearHudSettings, "Settings");
                ChiefGearHudInspect = new HudFixedLayout();
                ChiefGearHudTabView.AddTab(ChiefGearHudInspect, "Inspect");
                ChiefGearHudSounds = new HudFixedLayout();
                ChiefGearHudTabView.AddTab(ChiefGearHudSounds, "Sounds");
                ChiefGearHudAbout = new HudFixedLayout();
                ChiefGearHudTabView.AddTab(ChiefGearHudAbout, "About");
                ChiefGearHudView.LoadUserSettings();

                SubscribeChiefGearEvents();

                RenderChiefGearHudSettings();

            }
            catch (Exception ex) { LogError(ex); }
            return;
        }

        private void ChiefGearHudTabView_OpenTabChange(object sender, System.EventArgs e)
        {
            try
            {
                switch (ChiefGearHudTabView.CurrentTab)
                {
                    case 0:
                        if (ChiefGearHudInspect != null) { DisposeChiefGearHudInspect(); }
                        if (ChiefGearHudSounds != null) { DisposeChiefGearHudSounds(); }
                        if (ChiefGearHudAbout != null) { DisposeChiefGearHudAbout(); }

                        RenderChiefGearHudSettings();
                        break;
                    case 1:
                        if (ChiefGearHudSettings != null) { DisposeChiefGearHudSettings(); }
                        if (ChiefGearHudSounds != null) { DisposeChiefGearHudSounds(); }
                        if (ChiefGearHudAbout != null) { DisposeChiefGearHudAbout(); }

                        RenderChiefGearHudInspect();
                        break;
                    case 2:
                        if (ChiefGearHudSettings != null) { DisposeChiefGearHudSettings(); }
                        if (ChiefGearHudInspect != null) { DisposeChiefGearHudInspect(); }
                        if (ChiefGearHudAbout!= null) { DisposeChiefGearHudAbout(); }

                        RenderChiefGearHudSounds();
                        break;
                    case 3:
                        if (ChiefGearHudSettings != null) { DisposeChiefGearHudSettings(); }
                        if (ChiefGearHudInspect != null) { DisposeChiefGearHudInspect(); }
                        if (ChiefGearHudSounds != null) { DisposeChiefGearHudSounds(); }

                        RenderChiefGearHudAbout();
                        break;

                }

            }
            catch (Exception ex) { LogError(ex); }
        }

        private void RenderChiefGearHudSettings()
        {
            try
            {
                //RemoteGear
                HudStaticText lblRemoteGearEnabled = new HudStaticText();
                lblRemoteGearEnabled.FontHeight = nmenuFontHeight;
                lblRemoteGearEnabled.Text = "Remote Gear:";
                ChiefGearHudSettings.AddControl(lblRemoteGearEnabled, new Rectangle(5, 5, 150, 20));


                chkRemoteGearEnabled = new HudCheckBox();
                chkRemoteGearEnabled.Text = "Enable Remote Gear";
                ChiefGearHudSettings.AddControl(chkRemoteGearEnabled, new Rectangle(10, 25, 150, 20));
                chkRemoteGearEnabled.Checked = mMainSettings.bRemoteGearEnabled;


               //SwitchGear 
                HudStaticText lblChiefGearSwitch = new HudStaticText();
                lblChiefGearSwitch.FontHeight = nmenuFontHeight;
                lblChiefGearSwitch.Text = "SwitchGears";
                ChiefGearHudSettings.AddControl(lblChiefGearSwitch, new Rectangle(5, 55, 150, 20));


                chkQuickSlotsv = new HudCheckBox();
                chkQuickSlotsv.Text = "Enable Vertical";
                ChiefGearHudSettings.AddControl(chkQuickSlotsv, new Rectangle(10, 75, 150, 20));
                chkQuickSlotsv.Checked = mMainSettings.bquickSlotsvEnabled;

                chkQuickSlotsh = new HudCheckBox();
                chkQuickSlotsh.Text = "Enable Horizontal";
                ChiefGearHudSettings.AddControl(chkQuickSlotsh, new Rectangle(10, 95, 150, 20));
                chkQuickSlotsh.Checked = mMainSettings.bquickSlotsvEnabled;

                //Gear 
                HudStaticText lblInventorySetup = new HudStaticText();
                lblInventorySetup.FontHeight = nmenuFontHeight;
                lblInventorySetup.Text = "Gear";
                ChiefGearHudSettings.AddControl(lblInventorySetup, new Rectangle(5, 125, 150, 20));


                chkInventoryHudEnabled = new HudCheckBox();
                chkInventoryHudEnabled.Text = "Enable Inventory";
                ChiefGearHudSettings.AddControl(chkInventoryHudEnabled, new Rectangle(10, 145, 180, 20));
                chkInventoryHudEnabled.Checked = mMainSettings.binventoryHudEnabled;

                chkInventory = new HudCheckBox();
                chkInventory.Text = "Inventory on Startup";
                ChiefGearHudSettings.AddControl(chkInventory, new Rectangle(10, 165, 180, 20));
                chkInventory.Checked = mMainSettings.binventoryEnabled;

                chkInventoryBurden = new HudCheckBox();
                chkInventoryBurden.Text = "Update Stacks on Startup";
                ChiefGearHudSettings.AddControl(chkInventoryBurden, new Rectangle(10, 185, 180, 20));
                chkInventoryBurden.Checked = mMainSettings.binventoryBurdenEnabled;

                chkInventoryComplete = new HudCheckBox();
                chkInventoryComplete.Text = "Complete Inventory on Startup";
                ChiefGearHudSettings.AddControl(chkInventoryComplete, new Rectangle(10, 205, 180, 20));
                chkInventoryComplete.Checked = mMainSettings.binventoryCompleteEnabled;

                HudStaticText lblGearVisection = new HudStaticText();

                lblGearVisection.FontHeight = nmenuFontHeight;
                lblGearVisection.Text = "Gear Visection:";
                ChiefGearHudSettings.AddControl(lblGearVisection, new Rectangle(200, 5, 150, 20));


                chkGearVisectionEnabled = new HudCheckBox();
                chkGearVisectionEnabled.Text = "Enable Visection Gear";
                ChiefGearHudSettings.AddControl(chkGearVisectionEnabled, new Rectangle(205, 30, 150, 20));
                chkGearVisectionEnabled.Checked = mMainSettings.bRemoteGearEnabled;

                HudStaticText lblGearSense = new HudStaticText();
                lblGearSense.FontHeight = nmenuFontHeight;
                lblGearSense.Text = "GearSense:";
                ChiefGearHudSettings.AddControl(lblGearSense, new Rectangle(200, 60, 150, 20));


                chkGearSenseEnabled = new HudCheckBox();
                chkGearSenseEnabled.Text = "Enable Gear Sense";
                ChiefGearHudSettings.AddControl(chkGearSenseEnabled, new Rectangle(205, 80, 150, 20));
                chkGearSenseEnabled.Checked = mMainSettings.bGearSenseHudEnabled;

                HudStaticText lblGearButler = new HudStaticText();
                lblGearButler.FontHeight = nmenuFontHeight;
                lblGearButler.Text = "GearButler:";
                ChiefGearHudSettings.AddControl(lblGearButler, new Rectangle(200, 110, 150, 20));


                chkGearButlerEnabled = new HudCheckBox();
                chkGearButlerEnabled.Text = "Enable Gear Butler";
                ChiefGearHudSettings.AddControl(chkGearButlerEnabled, new Rectangle(205, 130, 150, 20));
                chkGearButlerEnabled.Checked = mMainSettings.bGearButlerEnabled;

                HudStaticText lblPortalGear = new HudStaticText();
                lblPortalGear.FontHeight = nmenuFontHeight;
                lblPortalGear.Text = "Portal Gear:";
                ChiefGearHudSettings.AddControl(lblPortalGear, new Rectangle(200, 160, 150, 20));


                chkPortalGearEnabled = new HudCheckBox();
                chkPortalGearEnabled.Text = "Use Portal Gear";
                ChiefGearHudSettings.AddControl(chkPortalGearEnabled, new Rectangle(205, 180, 150, 20));
                chkPortalGearEnabled.Checked = mMainSettings.bPortalGearEnabled;


                HudStaticText lblGearTactician = new HudStaticText();
                lblGearTactician.FontHeight = nmenuFontHeight;
                lblGearTactician.Text = "Gear Tactician:";
                ChiefGearHudSettings.AddControl(lblGearTactician, new Rectangle(350, 5, 150, 20));


                chkCombatHudEnabled = new HudCheckBox();
                chkCombatHudEnabled.Text = "Enable Gear Tactician";
                ChiefGearHudSettings.AddControl(chkCombatHudEnabled, new Rectangle(355, 25, 150, 20));
                chkCombatHudEnabled.Checked = mMainSettings.bGearTacticianEnabled;

                HudStaticText lblGearInspector = new HudStaticText();
                lblGearInspector.FontHeight = nmenuFontHeight;
                lblGearInspector.Text = "Gear Inspector:";
                ChiefGearHudSettings.AddControl(lblGearInspector, new Rectangle(350, 55, 150, 20));


                chkGearInspectorEnabled = new HudCheckBox();
                chkGearInspectorEnabled.Text = "Enable Gear Inspector";
                ChiefGearHudSettings.AddControl(chkGearInspectorEnabled, new Rectangle(355, 75, 150, 20));
                chkGearInspectorEnabled.Checked = mMainSettings.bGearInspectorEnabled;

                HudStaticText lblKillTaskGear = new HudStaticText();
                lblKillTaskGear.FontHeight = nmenuFontHeight;
                lblKillTaskGear.Text = "KillTask Gear:";
                ChiefGearHudSettings.AddControl(lblKillTaskGear, new Rectangle(350, 105, 150, 20));


                chkKillTaskGearEnabled = new HudCheckBox();
                chkKillTaskGearEnabled.Text = "Use KillTask Gear";
                ChiefGearHudSettings.AddControl(chkKillTaskGearEnabled, new Rectangle(355, 125, 150, 20));
                chkKillTaskGearEnabled.Checked = mMainSettings.bGearTaskerEnabled;

 
                HudStaticText lblGearFilters = new HudStaticText();
                lblGearFilters.FontHeight = nmenuFontHeight;
                lblGearFilters.Text = "Gear Filters:";
                ChiefGearHudSettings.AddControl(lblGearFilters, new Rectangle(350, 155, 145, 20));


                chkEnableTextFiltering = new HudCheckBox();
                chkEnableTextFiltering.Text = "Filter All Non-Chat Text";
                ChiefGearHudSettings.AddControl(chkEnableTextFiltering, new Rectangle(355, 175, 150, 20));
                chkEnableTextFiltering.Checked = bEnableTextFiltering;

                chkTextFilterAllStatus = new HudCheckBox();
                chkTextFilterAllStatus.Text = "Filter All Non-Chat Text";
                ChiefGearHudSettings.AddControl(chkTextFilterAllStatus, new Rectangle(355, 195, 150, 20));
                chkTextFilterAllStatus.Checked = bTextFilterAllStatus;


                HudStaticText lblMisc = new HudStaticText();
                lblMisc.FontHeight = nmenuFontHeight;
                lblMisc.Text = "Misc Gears:";
                ChiefGearHudSettings.AddControl(lblMisc, new Rectangle(5, 235, 150, 20));


                chkToonStats = new HudCheckBox();
                chkToonStats.Text = "Gather Toon Stats on Startup";
                ChiefGearHudSettings.AddControl(chkToonStats, new Rectangle(10, 255, 150, 20));
                chkToonStats.Checked = mMainSettings.btoonStatsEnabled;

                chkArmorHud = new HudCheckBox();
                chkArmorHud.Text = "Inventory Armor";
                ChiefGearHudSettings.AddControl(chkArmorHud, new Rectangle(10, 275, 150, 20));
                chkArmorHud.Checked = mMainSettings.bArmorHudEnabled;

                //HudStaticText lblItemFontHeight = new HudStaticText();
                // lblItemFontHeight.Text = "Item Font Height:";
                //ChiefGearHudSettings.AddControl(lblItemFontHeight, new Rectangle(225, 200, 100, 20));

                //txtItemFontHeight = new HudTextBox();
                //ChiefGearHudSettings.AddControl(txtItemFontHeight, new Rectangle(335, 200, 40, 20));

                //HudStaticText lblMenuFontHeight = new HudStaticText();
                //lblMenuFontHeight.Text = "Menu Font Height:";
                //ChiefGearHudSettings.AddControl(lblMenuFontHeight, new Rectangle(225, 215, 100, 20));

                //txtMenuFontHeight = new HudTextBox();
                //ChiefGearHudSettings.AddControl(txtMenuFontHeight, new Rectangle(335, 215, 40, 20));


                SubscribeChiefGearSettingsEvents();


            }
            catch (Exception ex) { LogError(ex); }

        }

        private void DisposeChiefGearHudSettings()
        {
            if (ChiefGearHudSettings == null) { return; }
            UnsubscribeChiefGearSettingsEvents();
            if (chkRemoteGearEnabled != null) { chkRemoteGearEnabled.Dispose(); }
            if (chkQuickSlotsv != null) { chkQuickSlotsv.Dispose(); }
            if (chkQuickSlotsh != null) { chkQuickSlotsh.Dispose(); }
            if (chkGearVisectionEnabled != null) { chkGearVisectionEnabled.Dispose(); }
            if (chkGearSenseEnabled != null) { chkGearSenseEnabled.Dispose(); }
            if (chkGearButlerEnabled != null) { chkGearButlerEnabled.Dispose(); }
            if (chkCombatHudEnabled != null) { chkCombatHudEnabled.Dispose(); }
            if (chkGearInspectorEnabled != null) { chkGearInspectorEnabled.Dispose(); }
            if (chkKillTaskGearEnabled != null) { chkKillTaskGearEnabled.Dispose(); }
            if (chkPortalGearEnabled != null) { chkPortalGearEnabled.Dispose(); }
            if (chkInventoryHudEnabled != null) { chkInventoryHudEnabled.Dispose(); }
            if (chkInventory != null) { chkInventory.Dispose(); }
            if (chkInventoryBurden != null) { chkInventoryBurden.Dispose(); }
            if (chkInventoryComplete != null) { chkInventoryComplete.Dispose(); }
            if (chkEnableTextFiltering != null) { chkEnableTextFiltering.Dispose(); }
            if (chkTextFilterAllStatus != null) { chkTextFilterAllStatus.Dispose(); }
            if (chkToonStats != null) { chkToonStats.Dispose(); }
            if (chkArmorHud != null) { chkArmorHud.Dispose(); }
            if (chkEnableTextFiltering != null) { chkEnableTextFiltering.Dispose(); }
            if (chkTextFilterAllStatus != null) { chkTextFilterAllStatus.Dispose(); }
            //    if (txtItemFontHeight != null) {txtItemFontHeight.Dispose();}
            //   if (txtMenuFontHeight != null) { txtMenuFontHeight.Dispose(); }

           //   ChiefGearHudSettings = null;


        }


        private void SubscribeChiefGearSettingsEvents()
        {
            chkRemoteGearEnabled.Change += chkRemoteGearEnabled_Change;
            chkQuickSlotsv.Change += chkQuickSlotsv_Change;
            chkQuickSlotsh.Change += chkQuickSlotsh_Change;
            chkGearVisectionEnabled.Change += chkGearVisectionEnabled_Change;
            chkGearSenseEnabled.Change += chkGearSenseEnabled_Change;
            chkGearButlerEnabled.Change += chkGearButlerEnabled_Change;
            chkCombatHudEnabled.Change += chkCombatHudEnabled_Change;
            chkGearInspectorEnabled.Change += chkGearInspectorEnabled_Change;
            chkKillTaskGearEnabled.Change += chkKillTaskGearEnabled_Change;
            chkPortalGearEnabled.Change += chkPortalGearEnabled_Change;
            chkInventoryHudEnabled.Change += chkInventoryHudEnabled_Change;
            chkInventory.Change += chkInventory_Change;
            chkInventoryBurden.Change += chkInventoryBurden_Change;
            chkInventoryComplete.Change += chkInventoryComplete_Change;
            chkToonStats.Change += chkToonStats_Change;
            chkArmorHud.Change += chkArmorHud_Change;
            chkEnableTextFiltering.Change += chkEnableTextFiltering_Change;
            chkTextFilterAllStatus.Change += chkTextFilterAllStatus_Change;
        //    txtItemFontHeight.LostFocus += txtItemFontHeight_LostFocus;
         //   txtMenuFontHeight.LostFocus += txtMenuFontHeight_LostFocus;
        }

        private void UnsubscribeChiefGearSettingsEvents()
        {
            chkRemoteGearEnabled.Change -= chkRemoteGearEnabled_Change;
            chkQuickSlotsv.Change -= chkQuickSlotsv_Change;
            chkQuickSlotsh.Change -= chkQuickSlotsh_Change;
            chkGearVisectionEnabled.Change -= chkGearVisectionEnabled_Change;
            chkGearButlerEnabled.Change -= chkGearButlerEnabled_Change;
            chkCombatHudEnabled.Change -= chkCombatHudEnabled_Change;
            chkGearInspectorEnabled.Change -= chkGearInspectorEnabled_Change;
            chkKillTaskGearEnabled.Change -= chkKillTaskGearEnabled_Change;
            chkPortalGearEnabled.Change -= chkPortalGearEnabled_Change;
            chkGearSenseEnabled.Change -= chkGearSenseEnabled_Change;
            chkInventoryHudEnabled.Change -= chkInventoryHudEnabled_Change;
            chkInventory.Change -= chkInventory_Change;
            chkInventoryBurden.Change -= chkInventoryBurden_Change;
            chkInventoryComplete.Change -= chkInventoryComplete_Change;
            chkToonStats.Change -= chkToonStats_Change;
            chkArmorHud.Change -= chkArmorHud_Change;
            chkEnableTextFiltering.Change -= chkEnableTextFiltering_Change;
            chkTextFilterAllStatus.Change -= chkTextFilterAllStatus_Change;
       //     txtItemFontHeight.LostFocus -= txtItemFontHeight_LostFocus;
       //     txtMenuFontHeight.LostFocus -= txtMenuFontHeight_LostFocus;


        }

		    	

        private void RenderChiefGearHudSounds()
        {
            try
            {
                int i = 0;

                chkMuteSounds = new HudCheckBox();
                chkMuteSounds.Text = "Mute Sound Effects";
                ChiefGearHudSounds.AddControl(chkMuteSounds, new Rectangle(8, 5, 115, 20));
                chkMuteSounds.Checked = mMainSettings.bGearTacticianEnabled;

                HudStaticText lblLandscapeHud = new HudStaticText();
                lblLandscapeHud.FontHeight = nmenuFontHeight;
                lblLandscapeHud.Text = "Gear Sense Sounds:";
                ChiefGearHudSounds.AddControl(lblLandscapeHud, new Rectangle(8, 30, 200, 16));

                ControlGroup cboTrophyLandscapeChoices = new ControlGroup();
                cboTrophyLandscape = new    HudCombo(cboTrophyLandscapeChoices);
                i = 0;
                foreach (Sounds s in SoundList)
                {
                    cboTrophyLandscape.AddItem(s.name, i);
                    i++;
                }
                cboTrophyLandscape.Current = 0;
                ChiefGearHudSounds.AddControl(cboTrophyLandscape, new Rectangle(5, 55, 125, 20));


                HudStaticText lblSound1 = new HudStaticText();
                lblSound1.FontHeight = nmenuFontHeight;
                lblSound1.Text = "Trophies";
                ChiefGearHudSounds.AddControl(lblSound1, new Rectangle(135, 55, 250, 16));

                ControlGroup cboMobLandscapeChoices = new ControlGroup();
                cboMobLandscape = new    HudCombo(cboMobLandscapeChoices);
                i = 0;
                foreach (Sounds s in SoundList)
                {
                    cboMobLandscape.AddItem(s.name, i);
                    i++;
                }
                cboMobLandscape.Current = 0;
                ChiefGearHudSounds.AddControl(cboMobLandscape, new Rectangle(5, 80, 125, 20));


                HudStaticText lblSound2 = new HudStaticText();
                lblSound2.FontHeight = nmenuFontHeight;
                lblSound2.Text = "Mobs";
                ChiefGearHudSounds.AddControl(lblSound2, new Rectangle(135, 80, 250, 16));

                ControlGroup cboPlayerLandscapeChoices = new ControlGroup();
                cboPlayerLandscape = new    HudCombo(cboPlayerLandscapeChoices);
                i = 0;
                foreach (Sounds s in SoundList)
                {
                    cboPlayerLandscape.AddItem(s.name, i);
                    i++;
                }
                cboPlayerLandscape.Current = 0;
                ChiefGearHudSounds.AddControl(cboPlayerLandscape, new Rectangle(5, 105, 125, 20));


                HudStaticText lblSound3 = new HudStaticText();
                lblSound3.FontHeight = nmenuFontHeight;
                lblSound3.Text = "Players";
                ChiefGearHudSounds.AddControl(lblSound3, new Rectangle(135, 105, 250, 16));


                HudStaticText lblCorpseHud = new HudStaticText();
                lblCorpseHud.FontHeight = nmenuFontHeight;
                lblCorpseHud.Text = "GearVisection Sounds:";
                ChiefGearHudSounds.AddControl(lblCorpseHud, new Rectangle(8, 140, 200, 16));

                ControlGroup cboCorpseRareChoices = new ControlGroup();
                cboCorpseRare = new HudCombo(cboCorpseRareChoices);
                i = 0;
                foreach (Sounds s in SoundList)
                {
                    cboCorpseRare.AddItem(s.name, i);
                    i++;
                }
                cboCorpseRare.Current = 0;
                ChiefGearHudSounds.AddControl(cboCorpseRare, new Rectangle(5, 165, 125, 20));


                HudStaticText lblSound4 = new HudStaticText();
                lblSound4.FontHeight = nmenuFontHeight;
                lblSound4.Text = "Corpse with Rare";
                ChiefGearHudSounds.AddControl(lblSound4, new Rectangle(135, 165, 250, 16));

                ControlGroup cboCorpseSelfKillChoices = new ControlGroup();
                cboCorpseSelfKill = new HudCombo(cboCorpseSelfKillChoices);
                i = 0;
                foreach (Sounds s in SoundList)
                {
                    cboCorpseSelfKill.AddItem(s.name, i);
                    i++;
                }
                cboCorpseSelfKill.Current = 0;
                ChiefGearHudSounds.AddControl(cboCorpseSelfKill, new Rectangle(5, 190, 125, 20));


                HudStaticText lblSound5 = new HudStaticText();
                lblSound5.FontHeight = nmenuFontHeight;
                lblSound5.Text = "Lootable Corpse";
                ChiefGearHudSounds.AddControl(lblSound5, new Rectangle(130, 190, 250, 16));

                ControlGroup cboCorpseFellowKillChoices = new ControlGroup();
                cboCorpseFellowKill = new HudCombo(cboCorpseFellowKillChoices);
                i = 0;
                foreach (Sounds s in SoundList)
                {
                    cboCorpseFellowKill.AddItem(s.name, i);
                    i++;
                }
                cboCorpseFellowKill.Current = 0;
                ChiefGearHudSounds.AddControl(cboCorpseFellowKill, new Rectangle(5, 215, 125, 20));


                HudStaticText lblSound6 = new HudStaticText();
                lblSound6.FontHeight = nmenuFontHeight;
                lblSound6.Text = "Lootable Corpse by Fellow";
                ChiefGearHudSounds.AddControl(lblSound6, new Rectangle(130, 215, 250, 16));

               ControlGroup cboDeadMeChoices = new ControlGroup();
                cboDeadMe = new    HudCombo(cboDeadMeChoices);
                i = 0;
                foreach (Sounds s in SoundList)
                {
                    cboDeadMe.AddItem(s.name, i);
                    i++;
                }
                cboDeadMe.Current = 0;
                ChiefGearHudSounds.AddControl(cboDeadMe, new Rectangle(5, 240, 125, 20));


                HudStaticText lblSound7 = new HudStaticText();
                lblSound7.FontHeight = nmenuFontHeight;
                lblSound7.Text = "Dead Me";
                ChiefGearHudSounds.AddControl(lblSound7, new Rectangle(130, 240, 250, 16));

               ControlGroup cboDeadPermittedChoices = new ControlGroup();
                cboDeadPermitted = new    HudCombo(cboDeadPermittedChoices);
                i = 0;
                foreach (Sounds s in SoundList)
                {
                    cboDeadPermitted.AddItem(s.name, i);
                    i++;
                }
                cboDeadPermitted.Current = 0;
                ChiefGearHudSounds.AddControl(cboDeadPermitted, new Rectangle(5, 265, 125, 20));


                HudStaticText lblSound8 = new HudStaticText();
                lblSound8.FontHeight = nmenuFontHeight;
                lblSound8.Text = "Recovery Corpse";
                ChiefGearHudSounds.AddControl(lblSound8, new Rectangle(130, 265, 250, 16));

                HudStaticText lblInspectorHud = new HudStaticText();
                lblInspectorHud.FontHeight = nmenuFontHeight;
                lblInspectorHud.Text = "GearInspector Sounds:";
                ChiefGearHudSounds.AddControl(lblInspectorHud, new Rectangle(8, 295, 200, 16));

                ControlGroup cboTrophyCorpseChoices = new ControlGroup();
                cboTrophyCorpse = new HudCombo(cboTrophyCorpseChoices);
                i = 0;
                foreach (Sounds s in SoundList)
                {
                    cboTrophyCorpse.AddItem(s.name, i);
                    i++;
                }
                cboTrophyCorpse.Current = 0;
                ChiefGearHudSounds.AddControl(cboTrophyCorpse, new Rectangle(5, 320, 125, 20));


                HudStaticText lblSound9 = new HudStaticText();
                lblSound9.FontHeight = nmenuFontHeight;
                lblSound9.Text = "Trophies";
                ChiefGearHudSounds.AddControl(lblSound9, new Rectangle(130, 320, 250, 16));

                ControlGroup cboRuleCorpseChoices = new ControlGroup();
                cboRuleCorpse = new    HudCombo(cboRuleCorpseChoices);
                i = 0;
                foreach (Sounds s in SoundList)
                {
                    cboRuleCorpse.AddItem(s.name, i);
                    i++;
                }
                cboRuleCorpse.Current = 0;
                ChiefGearHudSounds.AddControl(cboRuleCorpse, new Rectangle(5, 345, 125, 20));


                HudStaticText lblSound10 = new HudStaticText();
                lblSound10.FontHeight = nmenuFontHeight;
                lblSound10.Text = "Rule";
                ChiefGearHudSounds.AddControl(lblSound10, new Rectangle(130, 345, 250, 16));

                ControlGroup cboSalvageCorpseChoices = new ControlGroup();
                cboSalvageCorpse = new    HudCombo(cboSalvageCorpseChoices);
                i = 0;
                foreach (Sounds s in SoundList)
                {
                    cboSalvageCorpse.AddItem(s.name, i);
                    i++;
                }
                cboSalvageCorpse.Current = 0;
                ChiefGearHudSounds.AddControl(cboSalvageCorpse, new Rectangle(5, 370, 125, 20));


                HudStaticText lblSound11 = new HudStaticText();
                lblSound11.FontHeight = nmenuFontHeight;
                lblSound11.Text = "Salvage";
                ChiefGearHudSounds.AddControl(lblSound11, new Rectangle(130, 370, 250, 16));

                SubscribeChiefGearHudSounds();

 
            }
            catch (Exception ex) { LogError(ex); }

        }

        private void DisposeChiefGearHudSounds()
        {
            if (ChiefGearHudSounds == null) { return; }
            unsubscribeChiefGearHudSounds();

            if (chkMuteSounds != null) { chkMuteSounds.Dispose(); }
            if (cboTrophyLandscape != null) { cboTrophyLandscape.Dispose(); }
            if (cboMobLandscape != null) { cboMobLandscape.Dispose(); }
            if (cboPlayerLandscape != null) { cboPlayerLandscape.Dispose(); }
            if (cboCorpseRare != null) { cboCorpseRare.Dispose(); }
            if (cboCorpseSelfKill != null) { cboCorpseSelfKill.Dispose(); }
            if (cboCorpseFellowKill != null) { cboCorpseFellowKill.Dispose(); }
            if (cboDeadMe != null) { cboDeadMe.Dispose(); }
            if (cboDeadPermitted != null) { cboDeadPermitted.Dispose(); }
            if (cboTrophyCorpse != null) { cboTrophyCorpse.Dispose(); }
            if (cboRuleCorpse != null) { cboRuleCorpse.Dispose(); }
            if (cboSalvageCorpse != null) { cboSalvageCorpse.Dispose(); }
         //   ChiefGearHudSounds = null;
        }

        private void SubscribeChiefGearHudSounds()
        {
            chkMuteSounds.Change += (sender, index) => chkMuteSounds_Change();
            cboTrophyLandscape.Change += (sender, index) => cboTrophyLandscape_Change(sender, index);
            cboMobLandscape.Change += (sender, index) => cboMobLandscape_Change(sender, index);
            cboPlayerLandscape.Change += (sender, index) => cboPlayerLandscape_Change(sender, index);
            cboCorpseRare.Change += (sender, index) => cboCorpseRare_Change(sender, index);
            cboCorpseSelfKill.Change += (sender, index) => cboCorpseSelfKill_Change(sender, index);
            cboCorpseFellowKill.Change += (sender, index) => cboCorpseFellowKill_Change(sender, index);
            cboDeadMe.Change += (sender, index) => cboDeadMe_Change(sender, index);
            cboDeadPermitted.Change += (sender, index) => cboDeadPermitted_Change(sender, index);
            cboTrophyCorpse.Change += (sender, index) => cboTrophyCorpse_Change(sender, index);
            cboRuleCorpse.Change += (sender, index) => cboRuleCorpse_Change(sender, index);
            cboSalvageCorpse.Change += (sender, index) => cboSalvageCorpse_Change(sender, index);

        }

        private void unsubscribeChiefGearHudSounds()
        {
            if (chkMuteSounds != null) { chkMuteSounds.Change -= (sender, index) => chkMuteSounds_Change(); }

            if (cboTrophyLandscape != null) {cboTrophyLandscape.Change -= (sender, index) => cboTrophyLandscape_Change(sender, index);}
            if (cboTrophyLandscape != null) {cboMobLandscape.Change -= (sender, index) => cboMobLandscape_Change(sender, index);}
            if (cboTrophyLandscape != null) {cboPlayerLandscape.Change -= (sender, index) => cboPlayerLandscape_Change(sender, index);}
            if (cboTrophyLandscape != null) {cboCorpseRare.Change -= (sender, index) => cboCorpseRare_Change(sender, index);}
            if (cboTrophyLandscape != null) {cboCorpseSelfKill.Change -= (sender, index) => cboCorpseSelfKill_Change(sender, index);}
            if (cboTrophyLandscape != null) {cboCorpseFellowKill.Change -= (sender, index) => cboCorpseFellowKill_Change(sender, index);}
            if (cboTrophyLandscape != null) {cboDeadMe.Change -= (sender, index) => cboDeadMe_Change(sender, index);}
            if (cboTrophyLandscape != null) {cboDeadPermitted.Change -= (sender, index) => cboDeadPermitted_Change(sender, index);}
            if (cboTrophyLandscape != null) {cboTrophyCorpse.Change -= (sender, index) => cboTrophyCorpse_Change(sender, index);}
            if (cboTrophyLandscape != null) {cboRuleCorpse.Change -= (sender, index) => cboRuleCorpse_Change(sender, index);}
            if (cboTrophyLandscape != null) { cboSalvageCorpse.Change -= (sender, index) => cboSalvageCorpse_Change(sender, index); }

        }


        private void RenderChiefGearHudAbout()
        {
            try
            {
                HudStaticText lblAboutText1 = new HudStaticText();
                lblAboutText1.FontHeight = nmenuFontHeight;
                lblAboutText1.Text = "GearFoundry is a community development ";
                ChiefGearHudAbout.AddControl(lblAboutText1, new Rectangle(20, 20, 450, 20));

                HudStaticText lblAboutText1a = new HudStaticText();
                lblAboutText1a.FontHeight = nmenuFontHeight;
                lblAboutText1a.Text = "platform for multiple plugins.";
                ChiefGearHudAbout.AddControl(lblAboutText1a, new Rectangle(25, 40, 450, 20));

                HudStaticText lblAboutText2 = new HudStaticText();
                lblAboutText2.FontHeight = nmenuFontHeight;
                lblAboutText2.Text = "It consists of notifying,looting,inventorying ";
                ChiefGearHudAbout.AddControl(lblAboutText2, new Rectangle(20, 70, 450, 20));

                HudStaticText lblAboutText2a = new HudStaticText();
                lblAboutText2a.FontHeight = nmenuFontHeight;
                lblAboutText2a.Text = "and other options.";
                ChiefGearHudAbout.AddControl(lblAboutText2a, new Rectangle(25, 90, 450, 20));

                HudStaticText lblAboutText3 = new HudStaticText();
                lblAboutText3.FontHeight = nmenuFontHeight;
                lblAboutText3.Text = "The inspiration for GearFoundry was drawn from ";
                ChiefGearHudAbout.AddControl(lblAboutText3, new Rectangle(20, 120, 450, 20));

                HudStaticText lblAboutText3a = new HudStaticText();
                lblAboutText3a.FontHeight = nmenuFontHeight;
                lblAboutText3a.Text = "the original Alinco, many thanks Losado.";
                ChiefGearHudAbout.AddControl(lblAboutText3a, new Rectangle(25, 140, 450, 20));

                HudStaticText lblAboutText4 = new HudStaticText();
                lblAboutText4.FontHeight = nmenuFontHeight;
                lblAboutText4.Text = "Special Thanks to Virini Inquisitor, Hazridi, ";
                ChiefGearHudAbout.AddControl(lblAboutText4, new Rectangle(20, 170, 450, 20));

                HudStaticText lblAboutText4a = new HudStaticText();
                lblAboutText4a.FontHeight = nmenuFontHeight;
                lblAboutText4a.Text = "Mag-nus and all who have helped.";
                ChiefGearHudAbout.AddControl(lblAboutText4a, new Rectangle(25, 190, 450, 20));

                HudStaticText lblEditionNumber = new HudStaticText();
                lblEditionNumber.FontHeight = nmenuFontHeight;
                lblEditionNumber.Text = "Edition: ";
                ChiefGearHudAbout.AddControl(lblEditionNumber, new Rectangle(20, 220, 450, 20));
 
                HudStaticText lblOwnRisk = new HudStaticText();
                lblOwnRisk.FontHeight = nmenuFontHeight;
                lblOwnRisk.Text = "USE AT YOUR OWN RISK!";
                ChiefGearHudAbout.AddControl(lblOwnRisk, new Rectangle(20, 250, 450, 20));


            }
            catch (Exception ex) { LogError(ex); }

        }

        private void DisposeChiefGearHudAbout()
        {
            if (ChiefGearHudAbout == null) { return; }
         //   ChiefGearHudAbout = null;

        }



        private void SubscribeChiefGearEvents()
        {
            Decal.Adapter.CoreManager.Current.ItemSelected += new EventHandler<ItemSelectedEventArgs>(Current_ItemSelected);
           ChiefGearHudTabView.OpenTabChange += ChiefGearHudTabView_OpenTabChange;
           Core.CharacterFilter.Logoff += ChiefGearLogoff;
        //   SubscribeChiefGearSettingsEvents();

        }

        private void UnsubscribeChiefGearEvents()
        {
           // ChiefGearHudTabView.OpenTabChange -= ChiefGearHudTabView_OpenTabChange;
            Core.CharacterFilter.Logoff -= ChiefGearLogoff;


         //   UnsubscribeChiefGearSettingsEvents();

        }

        private void ChiefGearLogoff(object sender, EventArgs e)
        {
			try
			{
				UnsubscribeChiefGearEvents();
				if(ChiefGearHudSettings!= null) {DisposeChiefGearHudSettings();}
				if(ChiefGearHudInspect!= null) {DisposeChiefGearHudInspect();}
				if(ChiefGearHudSounds != null) {DisposeChiefGearHudSounds();}
                if (ChiefGearHudAbout != null) {DisposeChiefGearHudAbout();}
				LandscapeTrackingList.Clear();
			}catch(Exception ex){LogError(ex);}
		}
	
        





 
        private void DisposeChiefGearHudInspect()
        {
            if (lstRules != null) { lstRules.Dispose(); }

            if (btnRuleClear != null) { btnRuleClear.Dispose();}
            if (btnRuleNew != null) {btnRuleNew.Dispose();}
            if (btnRuleClone != null) {btnRuleClone.Dispose();}
            if (btnRuleUpdate != null) { btnRuleUpdate.Dispose(); }


        }

 

        
        private void DisposeChiefGearHud()
        {

            try
            {
                if (ChiefGearHudView == null) { return; }
                //SaveChiefGearSettings();
                UnsubscribeChiefGearEvents();

                if (ChiefGearHudSettings != null) { DisposeChiefGearHudSettings(); }
                if (ChiefGearHudInspect != null) { DisposeChiefGearHudInspect(); }
                if (ChiefGearHudSounds != null) { DisposeChiefGearHudSounds(); }
                if (ChiefGearHudAbout != null) { DisposeChiefGearHudAbout(); }

                ChiefGearHudView = null;
            }
            catch (Exception ex) { LogError(ex); }
            return;

        
        
        }






    }
}



 