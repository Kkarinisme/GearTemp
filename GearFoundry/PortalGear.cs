using System;
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
        XDocument xdocPortalGear = null;

        private static VirindiViewService.HudView portalGearHud = null;
        private static VirindiViewService.Controls.HudFixedLayout portalGear_Head = null;
        private static VirindiViewService.Controls.HudTabView portalGearTabView = null;
        private static VirindiViewService.Controls.HudFixedLayout portalGearTabFixedLayout = null;

        private HudStaticText txtPortalGear = null;
        private HudPictureBox mPortalGear0 = null;
        private HudPictureBox mPortalGear1 = null;
        private HudPictureBox mPortalGear2 = null;
        private HudPictureBox mPortalGear3 = null;
        private HudPictureBox mPortalGear4 = null;
        private HudPictureBox mPortalGear5 = null;
        private HudPictureBox mPortalGear6 = null;
        private HudPictureBox mPortalGear7 = null;
        private HudPictureBox mPortalGear8 = null;
        private HudPictureBox mPortalGear9 = null;



        private void RenderPortalGearHud()
        {
            try
            {
                if (portalGearHud != null)
                {
                    DisposePortalGearHud();
                }
                if (!File.Exists(portalGearFilename))
                {
                    XDocument tempDoc = new XDocument(new XElement("Portals"));
                    tempDoc.Save(portalGearFilename);
                    tempDoc = null;
                }

                xdocPortalGear = XDocument.Load(portalGearFilename);

                portalGearHud = new VirindiViewService.HudView("", 300, 40, new ACImage(Color.Transparent), false, "PortalGear");
                portalGearHud.ShowInBar = false;
                portalGearHud.UserAlphaChangeable = false;
                portalGearHud.Visible = true;
                portalGearHud.UserClickThroughable = false;
                portalGearHud.UserGhostable = true;
                portalGearHud.UserMinimizable = false;
                portalGearHud.UserResizeable = false;
                portalGearHud.LoadUserSettings();
                portalGear_Head = new HudFixedLayout();
                portalGearHud.Controls.HeadControl = portalGear_Head;
                portalGearTabView = new HudTabView();
                portalGearTabFixedLayout = new HudFixedLayout();
                portalGear_Head.AddControl(portalGearTabView, new Rectangle(0, 0, 300, 40));
                portalGearTabView.AddTab(portalGearTabFixedLayout, "");

                //Clock
                txtPortalGear = new HudStaticText();
                portalGearTabFixedLayout.AddControl(txtPortalGear, new Rectangle(0, 10, 40, 20));
                VirindiViewService.TooltipSystem.AssociateTooltip(txtPortalGear, "Bedtime yet?");

                //Portal Recall
                mPortalGear0 = new HudPictureBox();
                int GR_Recall_Icon = 0x60013AD;
                mPortalGear0.Image = GR_Recall_Icon;
                portalGearTabFixedLayout.AddControl(mPortalGear0, new Rectangle(50, 0, 25, 39));
                VirindiViewService.TooltipSystem.AssociateTooltip(mPortalGear0, "Portal recall");

                mPortalGear0.Hit += (sender, obj) => mPortalGear0_Hit(sender, obj);
              	MasterTimer.Tick += MasterTimer_UpdateClock;
            }
            catch (Exception ex) { LogError(ex); }




            //  Lifestone Recall
            mPortalGear1 = new HudPictureBox();
            int GR_LifestoneRecall_ICON = 0x60024E1;
            mPortalGear1.Image = GR_LifestoneRecall_ICON;
            mPortalGear1.Image = new ACImage(4949);
            portalGearTabFixedLayout.AddControl(mPortalGear1, new Rectangle(80, 2, 25, 39));
            VirindiViewService.TooltipSystem.AssociateTooltip(mPortalGear1, "Lifestone recall");

            mPortalGear1.Hit += (sender, obj) => mPortalGear1_Hit(sender, obj);

            //House Recall
            mPortalGear2 = new HudPictureBox();
            int GR_HouseRecall_ICON = 0x6001A2A;
            mPortalGear2.Image = GR_HouseRecall_ICON;
            portalGearTabFixedLayout.AddControl(mPortalGear2, new Rectangle(110, 2, 25, 39));
            VirindiViewService.TooltipSystem.AssociateTooltip(mRemoteGear2, "House recall");

            mPortalGear2.Hit += (sender, obj) => mPortalGear2_Hit(sender, obj);

 
            //Mansion Recall
            mPortalGear3 = new HudPictureBox();
            int GR_MansionRecall_ICON = 0x60022DE;
            mPortalGear3.Image = GR_MansionRecall_ICON;
            portalGearTabFixedLayout.AddControl(mPortalGear3, new Rectangle(140, 2, 25, 39));
            VirindiViewService.TooltipSystem.AssociateTooltip(mRemoteGear3, "Mansion recall");

            mPortalGear3.Hit += (sender, obj) => mPortalGear3_Hit(sender, obj);

            //Allegiance Hometown Recall
            mPortalGear4 = new HudPictureBox();
            int GR_AHRecall_ICON = 0x60024DD;
            mPortalGear4.Image = GR_AHRecall_ICON;
            portalGearTabFixedLayout.AddControl(mPortalGear4, new Rectangle(170, 2, 25, 29));
            VirindiViewService.TooltipSystem.AssociateTooltip(mRemoteGear4, "Allegiance Hometown");

            mPortalGear4.Hit += (sender, obj) => mPortalGear4_Hit(sender, obj);



            //Recall Portal I
            mPortalGear5 = new HudPictureBox();
            int GR_RecallI_ICON = 0x60021D6;
            mPortalGear5.Image = GR_RecallI_ICON;
            portalGearTabFixedLayout.AddControl(mPortalGear5, new Rectangle(200, 2, 29, 29));
            VirindiViewService.TooltipSystem.AssociateTooltip(mRemoteGear5, "Portal I recall");

            mPortalGear5.Hit += (sender, obj) => mPortalGear5_Hit(sender, obj);

            //Summon Portal I
            mPortalGear6 = new HudPictureBox();
            int GR_SummonI_ICON = 0x60021DC;
            mPortalGear6.Image = GR_SummonI_ICON;
            portalGearTabFixedLayout.AddControl(mPortalGear6, new Rectangle(230, 2, 29, 29));
            VirindiViewService.TooltipSystem.AssociateTooltip(mRemoteGear6, "Summon Portal I");
            mPortalGear6.Hit += (sender, obj) => mPortalGear6_Hit(sender, obj);
 
            //Recall Portal II
            mPortalGear7 = new HudPictureBox();
            int GR_RecallII_ICON = 0x60021DA;
            mPortalGear7.Image = GR_RecallII_ICON;
            portalGearTabFixedLayout.AddControl(mPortalGear7, new Rectangle(260, 2, 29, 29));
            VirindiViewService.TooltipSystem.AssociateTooltip(mRemoteGear7, "Recall Portal II");
            mPortalGear7.Hit += (sender, obj) => mPortalGear7_Hit(sender, obj);

            //Summon Portal I
            mPortalGear8 = new HudPictureBox();
            int GR_SummonII_ICON = 0x60021E0;
            mPortalGear8.Image = GR_SummonII_ICON;
            portalGearTabFixedLayout.AddControl(mPortalGear8, new Rectangle(290, 2, 29, 29));
            VirindiViewService.TooltipSystem.AssociateTooltip(mRemoteGear8, "Summon Portal II");
            mPortalGear8.Hit += (sender, obj) => mPortalGear8_Hit(sender, obj);

        }
        
        private void DisposePortalGearHud()
        {

            if (mPortalGear0 != null) { mPortalGear0.Hit -= (sender, obj) => mPortalGear0_Hit(sender, obj); mPortalGear0.Dispose(); }
            if (mPortalGear1 != null) { mPortalGear1.Hit -= (sender, obj) => mPortalGear1_Hit(sender, obj); mPortalGear1.Dispose(); }
            if (mPortalGear2 != null) { mPortalGear2.Hit -= (sender, obj) => mPortalGear2_Hit(sender, obj); mPortalGear2.Dispose(); }
            if (mPortalGear3 != null) { mPortalGear3.Hit -= (sender, obj) => mPortalGear3_Hit(sender, obj); mPortalGear3.Dispose(); }
            if (mPortalGear4 != null) { mPortalGear4.Hit -= (sender, obj) => mPortalGear4_Hit(sender, obj); mPortalGear4.Dispose(); }
            if (mPortalGear5 != null) { mPortalGear5.Hit -= (sender, obj) => mPortalGear5_Hit(sender, obj); mPortalGear5.Dispose(); }
            if (mPortalGear6 != null) { mPortalGear6.Hit -= (sender, obj) => mPortalGear6_Hit(sender, obj); mPortalGear6.Dispose(); }
            if (mPortalGear7 != null) { mPortalGear7.Hit -= (sender, obj) => mPortalGear7_Hit(sender, obj); mPortalGear7.Dispose(); }
            if (mPortalGear8 != null) { mPortalGear8.Hit -= (sender, obj) => mPortalGear8_Hit(sender, obj); mPortalGear8.Dispose(); }
            if (mPortalGear9 != null) { mPortalGear9.Hit -= (sender, obj) => mPortalGear9_Hit(sender, obj); mPortalGear9.Dispose(); }

            portalGear_Head.Dispose();
            portalGearHud.Dispose();

        }
        
        private void MasterTimer_UpdateClock(object sender, EventArgs e)
        {
        	try
        	{
        		txtPortalGear.Text = DateTime.Now.ToShortTimeString();
        	}catch(Exception ex){LogError(ex);}
        }


        //private void mPortalGear0_Hit(object sender, System.EventArgs e)
        //{
        //    try
        //    {
        //        if (bGearButlerEnabled == true)
        //        {
        //            bGearButlerEnabled = false;
        //            DisposeButlerHud();

        //        }
        //        else
        //        {
        //            bGearButlerEnabled = true;
        //            RenderButlerHud();

        //        }
        //        chkGearButlerEnabled.Checked = bGearButlerEnabled;
        //        SaveSettings();

        //    }
        //    catch (Exception ex) { LogError(ex); }

        //}

        private void mPortalGear0_Hit(object sender, System.EventArgs e)
        {
            try
            {
                  WriteToChat("/PR");
            }
            catch (Exception ex) { LogError(ex); }



        }

        private void mPortalGear1_Hit(object sender, System.EventArgs e)
        {
            try
            {
                Host.Actions.InvokeChatParser("/ls");
            }
            catch (Exception ex) { LogError(ex); }


        }
        private void mPortalGear2_Hit(object sender, System.EventArgs e)
        {
            try
            {
                Host.Actions.InvokeChatParser("/hr");
            }
            catch (Exception ex) { LogError(ex); }


        }

        private void mPortalGear3_Hit(object sender, System.EventArgs e)
        {
            try
            {
                Host.Actions.InvokeChatParser("/mr");
            }
            catch (Exception ex) { LogError(ex); }
        }

       private void mPortalGear4_Hit(object sender, System.EventArgs e)
        {
            try
            {
                Host.Actions.InvokeChatParser("/ah");
            }
            catch (Exception ex) { LogError(ex); }


        }

 
        
        private void mPortalGear5_Hit(object sender, System.EventArgs e)
        {
            try
            {

            }
            catch (Exception ex) { LogError(ex); }


        }
        private void mPortalGear6_Hit(object sender, System.EventArgs e)
        {
            try
            {

            }
            catch (Exception ex) { LogError(ex); }


        }
        private void mPortalGear7_Hit(object sender, System.EventArgs e)
        {
            try
            {

            }
            catch (Exception ex) { LogError(ex); }


        }
        private void mPortalGear8_Hit(object sender, System.EventArgs e)
        {
            try
            {

            }
            catch (Exception ex) { LogError(ex); }


        }
        private void mPortalGear9_Hit(object sender, System.EventArgs e)
        {
            try
            {

            }
            catch (Exception ex) { LogError(ex); }


        }



    }
}//end of namespace


