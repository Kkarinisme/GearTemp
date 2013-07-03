﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using VirindiViewService;
using VirindiViewService.Controls;
using MyClasses.MetaViewWrappers;
using MyClasses.MetaViewWrappers.VirindiViewServiceHudControls;
using System.Drawing;
using Decal.Adapter;
using Decal.Adapter.Wrappers;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Linq;
using System.IO;
using VirindiViewService.Themes;

namespace GearFoundry
{

    public partial class PluginCore : PluginBase
    {
        XDocument xdocRemoteGear = null;

        private static VirindiViewService.HudView remoteGearHud = null;
        private static VirindiViewService.Controls.HudFixedLayout remoteGear_Head = null;
        private static VirindiViewService.Controls.HudTabView remoteGearTabView = null;
        private static VirindiViewService.Controls.HudFixedLayout remoteGearTabFixedLayout = null;

        private HudPictureBox mRemoteGear0 = null;
        private HudPictureBox mRemoteGear1 = null;
        private HudPictureBox mRemoteGear2 = null;
        private HudPictureBox mRemoteGear3 = null;
        private HudPictureBox mRemoteGear4 = null;
        private HudPictureBox mRemoteGear5 = null;
        private HudPictureBox mRemoteGear6 = null;
        private HudPictureBox mRemoteGear7 = null;
        private HudPictureBox mRemoteGear8 = null;
        private HudPictureBox mRemoteGear9 = null;
        private int RemoteGearIcon = 0x6006E0A;




//        List<Int32> remoteGearID = new List<Int32>();
//        List<HudPictureBox> remoteGearPB = new List<HudPictureBox>();

        private void RenderRemoteGearHud()
        {

            if (remoteGearHud != null)
            {
                DisposeRemoteGearHud();
            }
            if (!File.Exists(remoteGearFilename))
            {
                XDocument tempDoc = new XDocument(new XElement("Huds"));
                tempDoc.Save(remoteGearFilename);
                tempDoc = null;
            }

            xdocRemoteGear = XDocument.Load(remoteGearFilename);

            remoteGearHud = new VirindiViewService.HudView("", 30, 260, RemoteGearIcon, false, "RemoteGear");
            remoteGearHud.ShowInBar = false;
            remoteGearHud.UserAlphaChangeable = false;
            remoteGearHud.Visible = true;
            remoteGearHud.UserClickThroughable = false;
            remoteGearHud.UserGhostable = true;
            remoteGearHud.UserMinimizable = false;
            remoteGearHud.UserResizeable = false;
            remoteGearHud.LoadUserSettings();
            remoteGear_Head = new HudFixedLayout();
            remoteGearHud.Controls.HeadControl = remoteGear_Head;
            remoteGearTabView = new HudTabView();
            remoteGearTabFixedLayout = new HudFixedLayout();
            remoteGear_Head.AddControl(remoteGearTabView, new Rectangle(0, 0, 29, 259));
            remoteGearTabView.AddTab(remoteGearTabFixedLayout, "");

            //Butler
            mRemoteGear0 = new HudPictureBox();
            mRemoteGear0.Image = new ACImage(25907);
            remoteGearTabFixedLayout.AddControl(mRemoteGear0, new Rectangle(2, 5, 25, 25));
            mRemoteGear0.Hit += (sender, obj) => mRemoteGear0_Hit(sender, obj); 

            //CorpseHud
            mRemoteGear1 = new HudPictureBox();
            int GR_Corpse_ICON = 0x6001070;
            mRemoteGear1.Image = GR_Corpse_ICON;
            remoteGearTabFixedLayout.AddControl(mRemoteGear1, new Rectangle(2, 35, 25, 25));
            mRemoteGear1.Hit += (sender, obj) => mRemoteGear1_Hit(sender, obj); 

            //GearInspector
            mRemoteGear2 = new HudPictureBox();
            int GR_Inspector_ICON = 0x600218D;
            mRemoteGear2.Image = GR_Inspector_ICON;
            remoteGearTabFixedLayout.AddControl(mRemoteGear2, new Rectangle(2, 65, 25, 25));
            mRemoteGear2.Hit += (sender, obj) => mRemoteGear2_Hit(sender, obj); 

            //GearSense
            mRemoteGear3 = new HudPictureBox();
            mRemoteGear3.Image = new ACImage(4949);
            remoteGearTabFixedLayout.AddControl(mRemoteGear3, new Rectangle(2, 95, 25, 25));
            mRemoteGear3.Hit += (sender, obj) => mRemoteGear3_Hit(sender, obj); 


           //CombatHud
            mRemoteGear4 = new HudPictureBox();
            int GR_Combat_ICON = 0x6004D06;
            mRemoteGear4.Image = GR_Combat_ICON;
            remoteGearTabFixedLayout.AddControl(mRemoteGear4, new Rectangle(2, 125, 25, 25));
            mRemoteGear4.Hit += (sender, obj) => mRemoteGear4_Hit(sender, obj);
            try
            {
                //InventoryHud
                mRemoteGear5 = new HudPictureBox();
                int GR_Inventory_ICON = 0x600127E;
                mRemoteGear5.Image = GR_Inventory_ICON;
                remoteGearTabFixedLayout.AddControl(mRemoteGear5, new Rectangle(2, 155, 25, 25));
                mRemoteGear5.Hit += (sender, obj) => mRemoteGear5_Hit(sender, obj);
                WriteToChat("Inventory Hud has been added.");

            }
            catch (Exception ex) { LogError(ex); }


            try
            {
                //Vertical Switch Gear

                string vertImageFile = GearDir + @"\gearswapvert1.png";
                Image gearswapvert = new Bitmap(vertImageFile);
                mRemoteGear6 = new HudPictureBox();
                //                int GR_Inventory_ICON = 0x600127E;
                //                mRemoteGear5.Image = GR_Inventory_ICON;
                mRemoteGear6.Image = (ACImage)gearswapvert;

                remoteGearTabFixedLayout.AddControl(mRemoteGear6, new Rectangle(2, 185, 25, 25));
                WriteToChat("Vertical switch gear has been added.");
                mRemoteGear6.Hit += (sender, obj) => mRemoteGear6_Hit(sender, obj);

            }
            catch (Exception ex) { LogError(ex); }


            try
            {
                //Horizontal Switch Gear
                string horzImageFile = GearDir + @"\gearswaphorz1.png";
                Image gearswaphoriz = new Bitmap(horzImageFile);
                mRemoteGear7 = new HudPictureBox();
                //  int GR_Inventory_ICON = 0x600127E;
                //  mRemoteGear7.Image = GR_Inventory_ICON;
                mRemoteGear7.Image = (ACImage)gearswaphoriz;
                remoteGearTabFixedLayout.AddControl(mRemoteGear7, new Rectangle(2, 215, 25, 25));
                mRemoteGear7.Hit += (sender, obj) => mRemoteGear7_Hit(sender, obj);

            }
            catch (Exception ex) { LogError(ex); }

          }

        private void DisposeRemoteGearHud()
        {

            if (mRemoteGear0 != null) { mRemoteGear0.Hit -= (sender, obj) => mRemoteGear0_Hit(sender, obj); mRemoteGear0.Dispose(); }
            if (mRemoteGear1 != null) { mRemoteGear1.Hit -= (sender, obj) => mRemoteGear1_Hit(sender, obj); mRemoteGear1.Dispose(); }
            if (mRemoteGear2 != null) { mRemoteGear2.Hit -= (sender, obj) => mRemoteGear2_Hit(sender, obj); mRemoteGear2.Dispose(); }
            if (mRemoteGear3 != null) { mRemoteGear3.Hit -= (sender, obj) => mRemoteGear3_Hit(sender, obj); mRemoteGear3.Dispose(); }
            if (mRemoteGear4 != null) { mRemoteGear4.Hit -= (sender, obj) => mRemoteGear4_Hit(sender, obj); mRemoteGear4.Dispose(); }
            if (mRemoteGear5 != null) { mRemoteGear5.Hit -= (sender, obj) => mRemoteGear5_Hit(sender, obj); mRemoteGear5.Dispose(); }
            if (mRemoteGear6 != null) { mRemoteGear6.Hit -= (sender, obj) => mRemoteGear6_Hit(sender, obj); mRemoteGear6.Dispose(); }
            if (mRemoteGear7 != null) { mRemoteGear7.Hit -= (sender, obj) => mRemoteGear7_Hit(sender, obj); mRemoteGear7.Dispose(); }
            if (mRemoteGear8 != null) { mRemoteGear8.Hit -= (sender, obj) => mRemoteGear8_Hit(sender, obj); mRemoteGear8.Dispose(); }
            if (mRemoteGear9 != null) { mRemoteGear9.Hit -= (sender, obj) => mRemoteGear9_Hit(sender, obj); mRemoteGear9.Dispose(); }

            remoteGear_Head.Dispose();
            remoteGearHud.Dispose();

        }


        private void mRemoteGear0_Hit(object sender, System.EventArgs e)
        {
            try
            {
                if (bGearButlerEnabled == true)
                {
                    bGearButlerEnabled = false;
                    DisposeButlerHud();

                }
                else
                {
                    bGearButlerEnabled = true;
                    RenderButlerHud();

                }
                chkGearButlerEnabled.Checked = bGearButlerEnabled;
                SaveSettings();

            }
            catch (Exception ex) { LogError(ex); }

        }

        private void mRemoteGear1_Hit(object sender, System.EventArgs e)
        {
            try
            {
                if (bCorpseHudEnabled == true)
                {
                    bCorpseHudEnabled = false;
                    DisposeCorpseHud();

                }
                else
                {
                    bCorpseHudEnabled = true;
                    RenderCorpseHud();

                }
                    chkGearVisectionEnabled.Checked = bCorpseHudEnabled;

            }
            catch (Exception ex) { LogError(ex); }
            SaveSettings();



        }
        private void mRemoteGear2_Hit(object sender, System.EventArgs e)
        {
            try
            {
                if (bGearInspectorEnabled == true)
                {
                    bGearInspectorEnabled = false;
                    DisposeItemHud();

                }
                else
                {
                    bGearInspectorEnabled = true;
                    RenderItemHud();

                }
                chkGearInspectorEnabled.Checked = bGearInspectorEnabled;
                SaveSettings();

            }
            catch (Exception ex) { LogError(ex); }


        }

        private void mRemoteGear3_Hit(object sender, System.EventArgs e)
        {
            try{
                if (bLandscapeHudEnabled == true)
                {
                    bLandscapeHudEnabled = false;
                    DisposeLandscapeHud();

                }
                else
                {
                    bLandscapeHudEnabled = true;
                    RenderLandscapeHud();

                }
                   chkGearSenseEnabled.Checked = bLandscapeHudEnabled;
                   SaveSettings();

            }
            catch (Exception ex) { LogError(ex); }


        }
        private void mRemoteGear4_Hit(object sender, System.EventArgs e)
        {
            try
            {
                if (bCombatHudEnabled == true)
                {
                    bCombatHudEnabled = false;
                    DisposeCombatHud();

                }
                else
                {
                    bCombatHudEnabled = true;
                    RenderCombatHud();

                }
                chkCombatHudEnabled.Checked = bCombatHudEnabled;
                SaveSettings();

            }
            catch (Exception ex) { LogError(ex); }



        }
        private void mRemoteGear5_Hit(object sender, System.EventArgs e)
        {
            try
            {
                if (binventoryHudEnabled == true)
                {
                    binventoryHudEnabled = false;
                    DisposeInventoryHud();

                }
                else
                {

                    binventoryHudEnabled = true;
                    RenderInventoryHud();

                }
                chkInventoryHudEnabled.Checked = binventoryHudEnabled;
                SaveSettings();

            }
            catch (Exception ex) { LogError(ex); }


        }
        private void mRemoteGear6_Hit(object sender, System.EventArgs e)
        {
            try
            {
                if (bquickSlotsvEnabled == true)
                {
                    bquickSlotsvEnabled = false;
                    DisposeVerticalQuickSlots();
                }
                else
                {

                    bquickSlotsvEnabled = true;
                    RenderVerticalQuickSlots();

                }
                chkQuickSlotsv.Checked = bquickSlotsvEnabled;
                SaveSettings();

            }
            catch (Exception ex) { LogError(ex); }

        }
        private void mRemoteGear7_Hit(object sender, System.EventArgs e)
        {
            try{
                if (bquickSlotshEnabled == true)
                {
                    bquickSlotshEnabled = false;
                    DisposeHorizontalQuickSlots();
                }
                else
                {

                    bquickSlotshEnabled = true;
                    RenderHorizontalQuickSlots();

                }
                chkQuickSlotsh.Checked = bquickSlotshEnabled;
                SaveSettings();

            
            }
            catch (Exception ex) { LogError(ex); }

        }
        private void mRemoteGear8_Hit(object sender, System.EventArgs e)
        {
            try
            {
            }
            catch (Exception ex) { LogError(ex); }

        }
        private void mRemoteGear9_Hit(object sender, System.EventArgs e)
        {
            try
            {
            }
            catch (Exception ex) { LogError(ex); }

        }





    }
}//end of namespace


