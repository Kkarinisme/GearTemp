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
using System.Reflection;

namespace GearFoundry
{

    public partial class PluginCore : PluginBase
    {
        XDocument xdocPortalGear = null;

        private static VirindiViewService.HudView portalGearHud = null;
        private static VirindiViewService.Controls.HudTabView portalGearTabView = null;
        private static VirindiViewService.Controls.HudFixedLayout portalGearTabFixedLayout = null;
        private static VirindiViewService.HudView portalRecallGearHud = null;
        private static VirindiViewService.Controls.HudTabView portalRecallGearTabView = null;
        private static VirindiViewService.Controls.HudFixedLayout portalRecallGearTabFixedLayout = null;

        private HudStaticText txtPortalGear = null;
        private HudPictureBox mSelectCaster = null;
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

        private HudPictureBox mPortalRecallGear0 = null;
        private HudPictureBox mPortalRecallGear1 = null;
        private HudPictureBox mPortalRecallGear2 = null;
        private HudPictureBox mPortalRecallGear3 = null;
        private HudPictureBox mPortalRecallGear4 = null;
        private HudPictureBox mPortalRecallGear5 = null;
        private HudPictureBox mPortalRecallGear6 = null;
        private HudPictureBox mPortalRecallGear7 = null;
        private HudPictureBox mPortalRecallGear8 = null;
        private HudPictureBox mPortalRecallGear9 = null;
        private HudPictureBox mPortalRecallGear10 = null;

        private int nOrbGuid = 0;
        private int nOrbIcon = 0;
        
        private List<PortalActions> PortalActionList = new List<PortalActions>();
        private System.Windows.Forms.Timer PortalActionTimer = new System.Windows.Forms.Timer();
        
     	private class PortalActions
     	{
     		public bool fireaction = false;
			public bool pending = false;
			public DateTime StartAction = DateTime.MinValue;
			public int ItemId = 0;	
			public RecallTypes RecallSpell = RecallTypes.none;	
			public int Retries = 0;			
     	}	
		
		public enum RecallTypes
		{
			none,
			portal,
			lifestone,
			primaryporal,
			summonprimary,
			secondaryportal,
			summonsecondary,
            sanctuary,
            bananaland,
            col,
            aerlinthe,
            mhoire,
            neftet,
            gearknight,
            caul,
            bur,
            olthoi_north,
            facilityhub,
		}
		
		private void SubscribePortalEvents()
		{
			try
			{
				for(int i = 0; i < 4; i++)
				{
					PortalActionList.Add(new PortalActions());
				}
				 MasterTimer.Tick += MasterTimer_UpdateClock;	
				
			}catch(Exception ex){LogError(ex);}
		}
		
		private void UnsubscribePortalEvents()
		{
			try
			{
				 MasterTimer.Tick -= MasterTimer_UpdateClock;
			}catch(Exception ex){LogError(ex);}
		}

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
                    savePortalSettings();
                }
                else
 				{
                    try
					{
	                    xdocPortalGear = XDocument.Load(portalGearFilename);
                        XElement elem = xdocPortalGear.Root;
                        if(elem.Element("Setting") == null){ savePortalSettings();}
                    }
                    catch(Exception ex){LogError(ex); nOrbGuid = 0; nOrbIcon = 0;}
                }
                try{
	            XElement el = xdocPortalGear.Root.Element("Setting");
                if(el.Element("OrbGuid") != null){nOrbGuid = Convert.ToInt32(el.Element("OrbGuid").Value);}
                if (el.Element("OrbIcon") == null && el.Element("OrbGuid") != null)
                {
                    nOrbIcon = Convert.ToInt32(el.Element("OrbIcond").Value); savePortalSettings();
                }
                else { nOrbIcon = Convert.ToInt32(el.Element("OrbIcon").Value); }
                }
                catch(Exception ex){LogError(ex); nOrbGuid = 0; nOrbIcon = 0;}
 				
               

                portalGearHud = new VirindiViewService.HudView("", 380, 40, new ACImage(0x6AA2), false, "PortalGear");
                portalGearHud.ShowInBar = false;
                portalGearHud.UserAlphaChangeable = false;
                portalGearHud.Visible = true;
                portalGearHud.UserClickThroughable = false;
                portalGearHud.UserGhostable = true;
                portalGearHud.UserMinimizable = true;
                portalGearHud.UserResizeable = false;
                portalGearHud.LoadUserSettings();
                portalGearTabView = new HudTabView();
                portalGearHud.Controls.HeadControl = portalGearTabView;
                portalGearTabFixedLayout = new HudFixedLayout();
                portalGearTabView.AddTab(portalGearTabFixedLayout, "");

 

                //Clock
                txtPortalGear = new HudStaticText();
                portalGearTabFixedLayout.AddControl(txtPortalGear, new Rectangle(0, 0, 55, 39));
                VirindiViewService.TooltipSystem.AssociateTooltip(txtPortalGear, "Bedtime yet?"); 
                
                //Select Wand
                mSelectCaster = new HudPictureBox();
                if(nOrbIcon != 0) {mSelectCaster.Image = nOrbIcon;}
                else{mSelectCaster.Image = 0x2A38;}
                portalGearTabFixedLayout.AddControl(mSelectCaster, new Rectangle(60, 2, 25, 39));
                VirindiViewService.TooltipSystem.AssociateTooltip(mSelectCaster, "Select Caster");
                mSelectCaster.Hit += (sender, obj) => mSelectCaster_Hit(sender, obj);

                
  
            //Portal Recall
            Stream recallPortalStream = this.GetType().Assembly.GetManifestResourceStream("recall.gif");
            Image PortalRecallImage = new Bitmap(recallPortalStream);
            mPortalGear0 = new HudPictureBox();
            mPortalGear0.Image = (ACImage)PortalRecallImage;
            portalGearTabFixedLayout.AddControl(mPortalGear0, new Rectangle(90, 2, 25, 39));
            VirindiViewService.TooltipSystem.AssociateTooltip(mPortalGear0, "Portal Recall");
            mPortalGear0.Hit += (sender, obj) => mPortalGear0_Hit(sender, obj);

            //  Lifestone Recall
            mPortalGear1 = new HudPictureBox();
            int GR_LifestoneRecall_ICON = 0x60024E1;
            mPortalGear1.Image = GR_LifestoneRecall_ICON;
            portalGearTabFixedLayout.AddControl(mPortalGear1, new Rectangle(120, 2, 25, 39));
            VirindiViewService.TooltipSystem.AssociateTooltip(mPortalGear1, "Lifestone Recall (/@ls)");
            mPortalGear1.Hit += (sender, obj) => mPortalGear1_Hit(sender, obj);

            //House Recall
            mPortalGear2 = new HudPictureBox();
            int GR_HouseRecall_ICON = 0x6001A2A;
            mPortalGear2.Image = GR_HouseRecall_ICON;
            portalGearTabFixedLayout.AddControl(mPortalGear2, new Rectangle(150, 2, 25, 39));
            VirindiViewService.TooltipSystem.AssociateTooltip(mPortalGear2, "House Recall (/@hr)");
            mPortalGear2.Hit += (sender, obj) => mPortalGear2_Hit(sender, obj);

 
            //Mansion Recall
            mPortalGear3 = new HudPictureBox();
            int GR_MansionRecall_ICON = 0x60022DE;
            mPortalGear3.Image = GR_MansionRecall_ICON;
            portalGearTabFixedLayout.AddControl(mPortalGear3, new Rectangle(180, 2, 25, 39));
            VirindiViewService.TooltipSystem.AssociateTooltip(mPortalGear3, "Mansion recall (/@hom)");
            mPortalGear3.Hit += (sender, obj) => mPortalGear3_Hit(sender, obj);

            //Allegiance Hometown Recall
            mPortalGear4 = new HudPictureBox();
            int GR_AHRecall_ICON = 0x60024DD;
            mPortalGear4.Image = GR_AHRecall_ICON;
            portalGearTabFixedLayout.AddControl(mPortalGear4, new Rectangle(210, 2, 25, 29));
            VirindiViewService.TooltipSystem.AssociateTooltip(mPortalGear4, "Allegiance Hometown (/@ah)");
            mPortalGear4.Hit += (sender, obj) => mPortalGear4_Hit(sender, obj);

            //Recall lifestone via spell
            Stream PortalLifestoneRecallStream = this.GetType().Assembly.GetManifestResourceStream("lsrecall.gif");
            Image PortalLifestoneRecallImage = new Bitmap(PortalLifestoneRecallStream);
            mPortalGear5 = new HudPictureBox();
            mPortalGear5.Image = (ACImage)PortalLifestoneRecallImage;
            portalGearTabFixedLayout.AddControl(mPortalGear5, new Rectangle(240,2, 25, 25));
            VirindiViewService.TooltipSystem.AssociateTooltip(mPortalGear5, "Recall Spell Lifestone");
            mPortalGear5.Hit += (sender, obj) => mPortalGear5_Hit(sender, obj);


           //Recall Portal I
            Stream recallPortalIStream = this.GetType().Assembly.GetManifestResourceStream("recallP1.gif");
            Image PortalRecallIImage = new Bitmap(recallPortalIStream);
            mPortalGear6 = new HudPictureBox();
            mPortalGear6.Image = (ACImage)PortalRecallIImage;
            portalGearTabFixedLayout.AddControl(mPortalGear6, new Rectangle(270,2, 25, 25));
            VirindiViewService.TooltipSystem.AssociateTooltip(mPortalGear6, "Recall Portal I");
            mPortalGear6.Hit += (sender, obj) => mPortalGear6_Hit(sender, obj);
//
//
//            //Summon Portal I
            Stream summonPortalIStream = this.GetType().Assembly.GetManifestResourceStream("summonP1.gif");
            Image PortalSummonIImage = new Bitmap(summonPortalIStream);
            mPortalGear7 = new HudPictureBox();
            mPortalGear7.Image = (ACImage)PortalSummonIImage;
            portalGearTabFixedLayout.AddControl(mPortalGear7, new Rectangle(300,2, 25, 25));
            VirindiViewService.TooltipSystem.AssociateTooltip(mPortalGear7, "Summon Portal I");
            mPortalGear7.Hit += (sender, obj) => mPortalGear7_Hit(sender, obj);
//
//            //Recall Portal II
            Stream recallPortalIIStream = this.GetType().Assembly.GetManifestResourceStream("recallP2.gif");
            Image PortalRecallIIImage = new Bitmap(recallPortalIIStream);
            mPortalGear8 = new HudPictureBox();
            mPortalGear8.Image = (ACImage)PortalRecallIIImage;
            portalGearTabFixedLayout.AddControl(mPortalGear8, new Rectangle(330,2, 25, 25));
            VirindiViewService.TooltipSystem.AssociateTooltip(mPortalGear8, "Recall Portal II");
            mPortalGear8.Hit += (sender, obj) => mPortalGear8_Hit(sender, obj);
//
//
//            //Summon Portal II
            Stream summonPortalIIStream = this.GetType().Assembly.GetManifestResourceStream("summonP2.gif");
            Image PortalSummonIIImage = new Bitmap(summonPortalIIStream);
            mPortalGear9 = new HudPictureBox();
            mPortalGear9.Image = (ACImage)PortalSummonIIImage;
            portalGearTabFixedLayout.AddControl(mPortalGear9, new Rectangle(360,2, 25, 25));
            VirindiViewService.TooltipSystem.AssociateTooltip(mPortalGear9, "Summon Portal II");
            mPortalGear9.Hit += (sender, obj) => mPortalGear9_Hit(sender, obj);
            
           
            SubscribePortalEvents();
            }catch(Exception ex){LogError(ex);}
 
        }
        private void DisposePortalGearHud()
        {
        	UnsubscribePortalEvents();

            if (mSelectCaster != null) { mSelectCaster.Hit -= (sender, obj) => mSelectCaster_Hit(sender, obj); mSelectCaster.Dispose(); }
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

            portalGearHud.Dispose();

        }

        private void RenderPortal2GearHud()
        {
            try
            {
                if (portalRecallGearHud != null)
                {
                    DisposePortalRecallGearHud();
                }

                portalRecallGearHud = new VirindiViewService.HudView("", 320, 40, new ACImage(0x6AA2), false, "PortalRecallGear");
                portalRecallGearHud.ShowInBar = false;
                portalRecallGearHud.UserAlphaChangeable = false;
                portalRecallGearHud.Visible = true;
                portalRecallGearHud.UserClickThroughable = false;
                portalRecallGearHud.UserGhostable = true;
                portalRecallGearHud.UserMinimizable = true;
                portalRecallGearHud.UserResizeable = false;
                portalRecallGearHud.LoadUserSettings();
                portalRecallGearTabView = new HudTabView();
                portalRecallGearHud.Controls.HeadControl = portalRecallGearTabView;
                portalRecallGearTabFixedLayout = new HudFixedLayout();
                portalRecallGearTabView.AddTab(portalRecallGearTabFixedLayout, "");


               //Sanctuary Recall
                Stream recallSanctuaryStream = this.GetType().Assembly.GetManifestResourceStream("sanctuary.gif");
                Image SanctuaryRecallImage = new Bitmap(recallSanctuaryStream);
                mPortalRecallGear0 = new HudPictureBox();
                mPortalRecallGear0.Image = (ACImage)SanctuaryRecallImage;
                portalRecallGearTabFixedLayout.AddControl(mPortalRecallGear0, new Rectangle(2, 2, 25, 39));
                VirindiViewService.TooltipSystem.AssociateTooltip(mPortalRecallGear0, "Sanctuary Recall");
                mPortalRecallGear0.Hit += (sender, obj) => mPortalRecallGear0_Hit(sender, obj);

                //  BananaLand Recall
                Stream recallBananaLandStream = this.GetType().Assembly.GetManifestResourceStream("bananaland.gif");
                Image BananaLandRecallImage = new Bitmap(recallBananaLandStream);
                //string strBananaLandRecallImage = GearDir + @"\bananaland.gif";
                //Image BananaLandRecallImage = new Bitmap(strBananaLandRecallImage);
                mPortalRecallGear1 = new HudPictureBox();
                mPortalRecallGear1.Image = (ACImage)BananaLandRecallImage;
                portalRecallGearTabFixedLayout.AddControl(mPortalRecallGear1, new Rectangle(30, 2, 25, 39));
                VirindiViewService.TooltipSystem.AssociateTooltip(mPortalRecallGear1, "BananaLand Recall");
                mPortalRecallGear1.Hit += (sender, obj) => mPortalRecallGear1_Hit(sender, obj);

                //  Facility Hub Recall
                Stream recallFacilityStream = this.GetType().Assembly.GetManifestResourceStream("facility.gif");
                Image FacilityRecallImage = new Bitmap(recallFacilityStream);
                mPortalRecallGear2 = new HudPictureBox();
                mPortalRecallGear2.Image = (ACImage)FacilityRecallImage;
                portalRecallGearTabFixedLayout.AddControl(mPortalRecallGear2, new Rectangle(60, 2, 25, 39));
                VirindiViewService.TooltipSystem.AssociateTooltip(mPortalRecallGear2, "Facility Hub Recall");
                mPortalRecallGear2.Hit += (sender, obj) => mPortalRecallGear2_Hit(sender, obj);
 
                //  Colloseum Recall
                Stream recallColloseumStream = this.GetType().Assembly.GetManifestResourceStream("col.gif");
                Image ColloseumRecallImage = new Bitmap(recallColloseumStream);
                mPortalRecallGear3 = new HudPictureBox();
                mPortalRecallGear3.Image = (ACImage)ColloseumRecallImage;
                portalRecallGearTabFixedLayout.AddControl(mPortalRecallGear3, new Rectangle(90, 2, 25, 39));
                VirindiViewService.TooltipSystem.AssociateTooltip(mPortalRecallGear3, "Colloseum Recall");
                mPortalRecallGear3.Hit += (sender, obj) => mPortalRecallGear3_Hit(sender, obj);

                //  Aerlinthe Recall
                Stream recallAerlintheStream = this.GetType().Assembly.GetManifestResourceStream("aerlinthe.gif");
                Image AerlintheRecallImage = new Bitmap(recallAerlintheStream);
                mPortalRecallGear4 = new HudPictureBox();
                mPortalRecallGear4.Image = (ACImage)AerlintheRecallImage;
                portalRecallGearTabFixedLayout.AddControl(mPortalRecallGear4, new Rectangle(120, 2, 25, 39));
                VirindiViewService.TooltipSystem.AssociateTooltip(mPortalRecallGear4, "Aerlinthe Recall");
                mPortalRecallGear4.Hit += (sender, obj) => mPortalRecallGear4_Hit(sender, obj);

                //  Singularity Caul Recall
                Stream recallCaulStream = this.GetType().Assembly.GetManifestResourceStream("caul.gif");
                Image CaulRecallImage = new Bitmap(recallCaulStream);
                mPortalRecallGear5 = new HudPictureBox();
                mPortalRecallGear5.Image = (ACImage)CaulRecallImage;
                portalRecallGearTabFixedLayout.AddControl(mPortalRecallGear5, new Rectangle(150, 2, 25, 39));
                VirindiViewService.TooltipSystem.AssociateTooltip(mPortalRecallGear5, "Singularity Caul Recall");
                mPortalRecallGear5.Hit += (sender, obj) => mPortalRecallGear5_Hit(sender, obj);

                //  Bur Recall
                Stream recallBurStream = this.GetType().Assembly.GetManifestResourceStream("bur.gif");
                Image BurRecallImage = new Bitmap(recallBurStream);
                mPortalRecallGear6 = new HudPictureBox();
                mPortalRecallGear6.Image = (ACImage)BurRecallImage;
                portalRecallGearTabFixedLayout.AddControl(mPortalRecallGear6, new Rectangle(180, 2, 25, 39));
                VirindiViewService.TooltipSystem.AssociateTooltip(mPortalRecallGear6, "Bur Recall");
                mPortalRecallGear6.Hit += (sender, obj) => mPortalRecallGear6_Hit(sender, obj);

                //  Olthoi_North Recall
                Stream recallOlthoi_NorthStream = this.GetType().Assembly.GetManifestResourceStream("olthoi_north.gif");
                Image OlthoiRecallImage = new Bitmap(recallOlthoi_NorthStream);
                mPortalRecallGear7 = new HudPictureBox();
                mPortalRecallGear7.Image = (ACImage)OlthoiRecallImage;
                portalRecallGearTabFixedLayout.AddControl(mPortalRecallGear7, new Rectangle(210, 2, 25, 39));
                VirindiViewService.TooltipSystem.AssociateTooltip(mPortalRecallGear7, "Olthoi_north Recall");
                mPortalRecallGear7.Hit += (sender, obj) => mPortalRecallGear7_Hit(sender, obj);

                //  Mhoire Recall
                Stream recallMhoireStream = this.GetType().Assembly.GetManifestResourceStream("mhoire.gif");
                Image MhoireRecallImage = new Bitmap(recallMhoireStream);
                mPortalRecallGear8 = new HudPictureBox();
                mPortalRecallGear8.Image = (ACImage)MhoireRecallImage;
                portalRecallGearTabFixedLayout.AddControl(mPortalRecallGear8, new Rectangle(240, 2, 25, 39));
                VirindiViewService.TooltipSystem.AssociateTooltip(mPortalRecallGear8, "Mhoire Recall");
                mPortalRecallGear8.Hit += (sender, obj) => mPortalRecallGear8_Hit(sender, obj);

                //  GearKnight Recall
                Stream recallGearKnightStream = this.GetType().Assembly.GetManifestResourceStream("gearknight.gif");
                Image GearKnightRecallImage = new Bitmap(recallGearKnightStream);
                mPortalRecallGear9 = new HudPictureBox();
                mPortalRecallGear9.Image = (ACImage)GearKnightRecallImage;
                portalRecallGearTabFixedLayout.AddControl(mPortalRecallGear9, new Rectangle(270, 2, 25, 39));
                VirindiViewService.TooltipSystem.AssociateTooltip(mPortalRecallGear9, "GearKnight Recall");
                mPortalRecallGear9.Hit += (sender, obj) => mPortalRecallGear9_Hit(sender, obj);

                //  Neftet Recall
                Stream recallNeftetStream = this.GetType().Assembly.GetManifestResourceStream("neftet.gif");
                Image NeftetRecallImage = new Bitmap(recallNeftetStream);
                mPortalRecallGear10 = new HudPictureBox();
                mPortalRecallGear10.Image = (ACImage)NeftetRecallImage;
                portalRecallGearTabFixedLayout.AddControl(mPortalRecallGear10, new Rectangle(300, 2, 25, 39));
                VirindiViewService.TooltipSystem.AssociateTooltip(mPortalRecallGear10, "Neftet Recall");
                mPortalRecallGear10.Hit += (sender, obj) => mPortalRecallGear10_Hit(sender, obj);



                SubscribePortalEvents();

            }
            catch (Exception ex) { LogError(ex); }

        }
        private void DisposePortalRecallGearHud()
        {
            UnsubscribePortalEvents();

            if (mPortalRecallGear0 != null) { mPortalRecallGear0.Hit -= (sender, obj) => mPortalRecallGear0_Hit(sender, obj); mPortalRecallGear0.Dispose(); }
            if (mPortalRecallGear1 != null) { mPortalRecallGear1.Hit -= (sender, obj) => mPortalRecallGear1_Hit(sender, obj); mPortalRecallGear1.Dispose(); }
            if (mPortalRecallGear2 != null) { mPortalRecallGear2.Hit -= (sender, obj) => mPortalRecallGear2_Hit(sender, obj); mPortalRecallGear2.Dispose(); }
            if (mPortalRecallGear3 != null) { mPortalRecallGear3.Hit -= (sender, obj) => mPortalRecallGear3_Hit(sender, obj); mPortalRecallGear3.Dispose(); }
            if (mPortalRecallGear4 != null) { mPortalRecallGear4.Hit -= (sender, obj) => mPortalRecallGear4_Hit(sender, obj); mPortalRecallGear4.Dispose(); }
            if (mPortalRecallGear5 != null) { mPortalRecallGear5.Hit -= (sender, obj) => mPortalRecallGear5_Hit(sender, obj); mPortalRecallGear5.Dispose(); }
            if (mPortalRecallGear6 != null) { mPortalRecallGear6.Hit -= (sender, obj) => mPortalRecallGear6_Hit(sender, obj); mPortalRecallGear6.Dispose(); }
            if (mPortalRecallGear7 != null) { mPortalRecallGear7.Hit -= (sender, obj) => mPortalRecallGear7_Hit(sender, obj); mPortalRecallGear7.Dispose(); }
            if (mPortalRecallGear8 != null) { mPortalRecallGear8.Hit -= (sender, obj) => mPortalRecallGear8_Hit(sender, obj); mPortalRecallGear8.Dispose(); }
            if (mPortalRecallGear9 != null) { mPortalRecallGear9.Hit -= (sender, obj) => mPortalRecallGear9_Hit(sender, obj); mPortalRecallGear9.Dispose(); }
            if (mPortalRecallGear10 != null) { mPortalRecallGear10.Hit -= (sender, obj) => mPortalRecallGear10_Hit(sender, obj); mPortalRecallGear10.Dispose(); }
 
            portalRecallGearHud.Dispose();

        }


      
        private void MasterTimer_UpdateClock(object sender, EventArgs e)
        {
	    	try
	        {
          		txtPortalGear.Text = DateTime.Now.ToShortTimeString();
         	}catch(Exception ex){LogError(ex);}
       }

        private void PortalItemSelected(object sender, ItemSelectedEventArgs e)
		{
			try
    		{	
				Core.ItemSelected -= PortalItemSelected;
				if(Core.WorldFilter[Core.Actions.CurrentSelection].ObjectClass == ObjectClass.WandStaffOrb && 
				   Core.WorldFilter.GetInventory().Where(x => x.Id == Core.Actions.CurrentSelection).Count() != 0)
				{		
					nOrbGuid = Core.Actions.CurrentSelection;
					nOrbIcon = Core.WorldFilter[nOrbGuid].Icon;
					savePortalSettings();
                    RenderPortalGearHud();
                }
            }catch(Exception ex){LogError(ex);}
        }



        private void mSelectCaster_Hit(object sender, System.EventArgs e)
        {
            try
            {
                WriteToChat("Please select caster from pack that should be used for spell recalls if not holding a wand when call requested.");
                 Core.ItemSelected += PortalItemSelected;


            }
            catch (Exception ex) { LogError(ex); }
        }

        private void savePortalSettings()
        {
           try
           {
                xdoc = new XDocument(new XElement("Settings"));
                xdoc.Element("Settings").Add(new XElement("Setting",
                        new XElement("OrbGuid", nOrbGuid),
                         new XElement("OrbIcon", nOrbIcon)));
                xdoc.Save(portalGearFilename);

            }
            catch (Exception ex) { LogError(ex); }

        }


        

        private void mPortalGear0_Hit(object sender, System.EventArgs e)
        {
            try
            {
            	PortalActionsLoad(RecallTypes.portal);
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
                Host.Actions.InvokeChatParser("/hom");
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
				PortalActionsLoad(RecallTypes.lifestone);
            }
            catch (Exception ex) { LogError(ex); }
        }
        private void mPortalGear6_Hit(object sender, System.EventArgs e)
        {
            try
            {
				PortalActionsLoad(RecallTypes.primaryporal);
            }
            catch (Exception ex) { LogError(ex); }
        }
        private void mPortalGear7_Hit(object sender, System.EventArgs e)
        {
            try
            {
				PortalActionsLoad(RecallTypes.summonprimary);
            }
            catch (Exception ex) { LogError(ex); }
        }
        private void mPortalGear8_Hit(object sender, System.EventArgs e)
        {
            try
            {
				PortalActionsLoad(RecallTypes.secondaryportal);
            }
            catch (Exception ex) { LogError(ex); }
        }
        private void mPortalGear9_Hit(object sender, System.EventArgs e)
        {
            try
            {
				PortalActionsLoad(RecallTypes.summonsecondary);
            }
            catch (Exception ex) { LogError(ex); }
        }

        private void mPortalRecallGear0_Hit(object sender, System.EventArgs e)
        {
            try
            {
                PortalActionsLoad(RecallTypes.sanctuary);
            }
            catch (Exception ex) { LogError(ex); }
        }
        private void mPortalRecallGear1_Hit(object sender, System.EventArgs e)
        {
            try
            {
                PortalActionsLoad(RecallTypes.bananaland);
            }
            catch (Exception ex) { LogError(ex); }
        }
        private void mPortalRecallGear2_Hit(object sender, System.EventArgs e)
        {
            try
            {
                PortalActionsLoad(RecallTypes.facilityhub);
            }
            catch (Exception ex) { LogError(ex); }
        }
        private void mPortalRecallGear3_Hit(object sender, System.EventArgs e)
        {
            try
            {
                PortalActionsLoad(RecallTypes.col);
            }
            catch (Exception ex) { LogError(ex); }
        }
        private void mPortalRecallGear4_Hit(object sender, System.EventArgs e)
        {
            try
            {
                PortalActionsLoad(RecallTypes.aerlinthe);
            }
            catch (Exception ex) { LogError(ex); }
        }
        private void mPortalRecallGear5_Hit(object sender, System.EventArgs e)
        {
            try
            {
                PortalActionsLoad(RecallTypes.caul);
            }
            catch (Exception ex) { LogError(ex); }
        }
        private void mPortalRecallGear6_Hit(object sender, System.EventArgs e)
        {
            try
            {
                PortalActionsLoad(RecallTypes.bur);
            }
            catch (Exception ex) { LogError(ex); }
        }
        private void mPortalRecallGear7_Hit(object sender, System.EventArgs e)
        {
            try
            {
                PortalActionsLoad(RecallTypes.olthoi_north);
            }
            catch (Exception ex) { LogError(ex); }
        }
        private void mPortalRecallGear8_Hit(object sender, System.EventArgs e)
        {
            try
            {
                PortalActionsLoad(RecallTypes.mhoire);
            }
            catch (Exception ex) { LogError(ex); }
        }
        private void mPortalRecallGear9_Hit(object sender, System.EventArgs e)
        {
            try
            {
                PortalActionsLoad(RecallTypes.gearknight);
            }
            catch (Exception ex) { LogError(ex); }
        }
        private void mPortalRecallGear10_Hit(object sender, System.EventArgs e)
        {
            try
            {
                PortalActionsLoad(RecallTypes.neftet);
            }
            catch (Exception ex) { LogError(ex); }
        }

        
        private void PortalActionsLoad(RecallTypes recall)
        {
        	
        	try
        	{
        		if(PortalActionsPending) 
        		{
        			WriteToChat("Portal action pending.  Please wait for completion.");
        			return;
        		}
        		
        		if(nOrbGuid == 0)
        		{
        			nOrbGuid = Core.WorldFilter.GetInventory().Where(x => x.ObjectClass == ObjectClass.WandStaffOrb && 
        			           (x.Values(LongValueKey.WieldReqValue) == 0 || x.Values(LongValueKey.WieldReqValue) == 150 ||
        			           x.Values(LongValueKey.WieldReqValue) == 180)).ToList().OrderByDescending(x => x.Values(DoubleValueKey.MeleeDefenseBonus)).First().Id;
        			
        			nOrbIcon = Core.WorldFilter[nOrbGuid].Icon;
       
        			mSelectCaster.Image = nOrbIcon;
        			
        			xdoc = new XDocument(new XElement("Settings"));
                xdoc.Element("Settings").Add(new XElement("Setting",
                        new XElement("OrbGuid", nOrbGuid),
                         new XElement("OrbIcon", nOrbIcon)));
                		xdoc.Save(portalGearFilename);
        		}
        		
        		//Not holding a caster
        		if(Core.WorldFilter.GetInventory().Where(x => x.Values(LongValueKey.EquippedSlots) == 0x1000000).Count() == 0)
        		{
        			if(Core.Actions.CombatMode != CombatState.Peace)
        			{
        				PortalActionList[0].fireaction = true;
        			}
        			PortalActionList[1].fireaction = true;
        			PortalActionList[2].fireaction = true;
        		}
        		else if(Core.Actions.CombatMode != CombatState.Magic)
        		{
        			PortalActionList[2].fireaction = true;
          		}
        		PortalActionList[3].fireaction = true;
        		PortalActionList[3].RecallSpell = recall;  

        		InitiatePortalActions();
		
        	}catch(Exception ex){LogError(ex);}
        }
        
        private bool PortalActionsPending = false;
        private void InitiatePortalActions()
        {
        	try
        	{
        		if(!PortalActionsPending) 
        		{
        			PortalActionsPending = true;
					PortalActionTimer.Interval = 250;
					PortalActionTimer.Start();
					
					PortalActionTimer.Tick += PortalActionInitiator;
					return;
        		}
        	}catch(Exception ex){LogError(ex);}
        }

        private void PortalActionInitiator(object sender, EventArgs e)
		{
			try
			{
				FirePortalActions();
			}catch(Exception ex){LogError(ex);}
		}
        
        
        private void FirePortalActions()
		{
			try
			{				
				if(PortalActionList[0].fireaction)
				{
					if(PortalActionList[0].pending && (DateTime.Now - PortalActionList[0].StartAction).TotalMilliseconds < 600)
					{
						return;
					}
					else if(Core.Actions.CombatMode != CombatState.Peace)
					{
						PortalActionList[0].pending = true;
						PortalActionList[0].StartAction = DateTime.Now;
						Core.Actions.SetCombatMode(CombatState.Peace);
						return;
					}
					else
					{
						PortalActionList[0].pending = false;
						PortalActionList[0].StartAction = DateTime.MinValue;
						PortalActionList[0].fireaction = false;
					}
				}
				else if(PortalActionList[1].fireaction)
				{
					if(PortalActionList[1].pending && (DateTime.Now - PortalActionList[1].StartAction).TotalMilliseconds < 300)
					{
						return;
					}
					else if(Core.WorldFilter.GetInventory().Where(x => x.Values(LongValueKey.EquippedSlots) == 0x1000000).Count() == 0)
					{	
						PortalActionList[1].pending = true;
						PortalActionList[1].StartAction = DateTime.Now;
						PortalActionEquip();
						return;
					}
					else
					{
						PortalActionList[1].pending = false;
						PortalActionList[1].StartAction = DateTime.MinValue;
						PortalActionList[1].fireaction = false;
					}
				}
				else if(PortalActionList[2].fireaction)
				{
					if(PortalActionList[2].pending && (DateTime.Now - PortalActionList[2].StartAction).TotalMilliseconds < 600)
					{
						return;
					}
					else if(Core.Actions.CombatMode != CombatState.Magic)
					{
						PortalActionList[2].pending = true;
						PortalActionList[2].StartAction = DateTime.Now;
						Core.Actions.SetCombatMode(CombatState.Magic);
						return;
					}
					else
					{
						PortalActionList[2].pending = false;
						PortalActionList[2].StartAction = DateTime.MinValue;
						PortalActionList[2].fireaction = false;
					}
				}
				else if(PortalActionList[3].fireaction)
				{
					if(PortalActionList[3].pending && (DateTime.Now - PortalActionList[3].StartAction).TotalSeconds < 4)
					{
						return;
					}
					else if(!PortalCastSuccess && PortalActionList[3].Retries < 3)
					{
						PortalActionList[3].pending = true;
						PortalActionList[3].StartAction = DateTime.Now;
						PortalActionList[3].Retries++;
						PortalActionsCastSpell();
						return;
					}	
					else
					{
						if(PortalActionList[3].Retries > 2) {WriteToChat("Recall/Summon Failed. Check ties and other recall requirements.");}
						PortalActionList[3].pending = false;
						PortalActionList[3].StartAction = DateTime.MinValue;
						PortalActionList[3].fireaction = false;
						PortalActionList[3].Retries = 0;	
						PortalCastSuccess = false;
						Core.CharacterFilter.ChangePortalMode -= PortalCast_Listen;
    					Core.CharacterFilter.ActionComplete -= PortalCast_ListenComplete;
					}

				}
				else
				{
					PortalActionsPending = false;
					PortalActionTimer.Tick -= PortalActionInitiator;	
					PortalActionTimer.Stop();
					return;
				}
			}catch(Exception ex){LogError(ex);}
		}     
     
            
        private void PortalActionEquip()
        {
        	try
			{	

				if(Core.WorldFilter.GetInventory().Where(x => x.ObjectClass == ObjectClass.Armor && x.Values(LongValueKey.EquippedSlots) == 0x200000).Count() > 0 &&
				   Core.WorldFilter.GetInventory().Where(x => x.ObjectClass == ObjectClass.MeleeWeapon && x.LongKeys.Contains((int)LongValueKey.EquippedSlots)).Count() == 0)
				{
					Core.Actions.UseItem(Core.WorldFilter.GetInventory().Where(x => x.ObjectClass == ObjectClass.Armor && x.Values(LongValueKey.EquippedSlots) == 0x200000).First().Id,0);
					return;
				}
				else
				{
					Core.Actions.UseItem(nOrbGuid, 0);
					return;
				}
					
			}catch(Exception ex){LogError(ex);}
		}
        
        private void PortalActionsCastSpell()
        {
        	try
        	{
        		//Clean up listens in cast
				Core.CharacterFilter.ChangePortalMode -= PortalCast_Listen;
    			Core.CharacterFilter.ActionComplete -= PortalCast_ListenComplete;
					
				WriteToChat("Cast Spell " + PortalActionList[3].RecallSpell.ToString());
					
				switch(PortalActionList[3].RecallSpell)
				{
					case RecallTypes.lifestone:
						if(!Core.CharacterFilter.IsSpellKnown(1635))
						{
							PortalActionList[3].fireaction = false;
							WriteToChat("You do not know Lifestone Recall.  Action disabled.");
							return;
						}
						Core.CharacterFilter.ChangePortalMode += PortalCast_Listen;
						Core.Actions.CastSpell(1635, Core.CharacterFilter.Id);
						return;
					case RecallTypes.portal:
						if(!Core.CharacterFilter.IsSpellKnown(2645))
						{
							PortalActionList[3].fireaction = false;
							WriteToChat("You do not know Portal Recall.  Action disabled.");
							return;
						}
						Core.CharacterFilter.ChangePortalMode += PortalCast_Listen;
						Core.Actions.CastSpell(2645, Core.CharacterFilter.Id);
						return;
					case RecallTypes.primaryporal:
						if(!Core.CharacterFilter.IsSpellKnown(48))
						{
							PortalActionList[3].fireaction = false;
							WriteToChat("You do not know Primary Portal Recall.  Action disabled.");
							return;
						}
						Core.CharacterFilter.ChangePortalMode += PortalCast_Listen;
						Core.Actions.CastSpell(48, Core.CharacterFilter.Id);
						return;
					case RecallTypes.summonprimary:
						if(!Core.CharacterFilter.IsSpellKnown(157))
						{
							PortalActionList[3].fireaction = false;
							WriteToChat("You do not know Summon Primary Portal I.  Action disabled.");
							return;
						}
						Core.CharacterFilter.ActionComplete += PortalCast_ListenComplete;
						Core.Actions.CastSpell(157, Core.CharacterFilter.Id);
						return;
					case RecallTypes.secondaryportal:
						if(!Core.CharacterFilter.IsSpellKnown(2647))
						{
							PortalActionList[3].fireaction = false;
							WriteToChat("You do not know Secondary Portal Recall.  Action disabled.");
							return;
						}
						WriteToChat("Recalled secondary");
						Core.CharacterFilter.ChangePortalMode += PortalCast_Listen;
						Core.Actions.CastSpell(2647, Core.CharacterFilter.Id);
						return;
					case RecallTypes.summonsecondary:
						if(!Core.CharacterFilter.IsSpellKnown(2648))
						{
							PortalActionList[3].fireaction = false;
							WriteToChat("You do not know Summon Secondary Portal I.  Action disabled.");
							return;
						}
						WriteToChat("Summoned secondary");
						Core.CharacterFilter.ActionComplete += PortalCast_ListenComplete;
						Core.Actions.CastSpell(2648, Core.CharacterFilter.Id);
						return;
                    case RecallTypes.sanctuary:
                        if (!Core.CharacterFilter.IsSpellKnown(2023))
                        {
                            PortalActionList[3].fireaction = false;
                            WriteToChat("You do not know Sanctuary Recall.  Action disabled.");
                            return;
                        }
                        WriteToChat("Sanctuary Recalled");
                        Core.CharacterFilter.ActionComplete += PortalCast_ListenComplete;
                        Core.Actions.CastSpell(2023, Core.CharacterFilter.Id);
                        return;
                    case RecallTypes.bananaland:
                        if (!Core.CharacterFilter.IsSpellKnown(2931))
                        {
                            PortalActionList[3].fireaction = false;
                            WriteToChat("You do not know BananaLand Recall.  Action disabled.");
                            return;
                        }
                        WriteToChat("Recalled to Bananaland");
                        Core.CharacterFilter.ActionComplete += PortalCast_ListenComplete;
                        Core.Actions.CastSpell(2931, Core.CharacterFilter.Id);
                        return;
                    case RecallTypes.col:
                        if (!Core.CharacterFilter.IsSpellKnown(4213))
                        {
                            PortalActionList[3].fireaction = false;
                            WriteToChat("You do not know Colloseum Recall.  Action disabled.");
                            return;
                        }
                        WriteToChat("Recalled to Colloseum");
                        Core.CharacterFilter.ActionComplete += PortalCast_ListenComplete;
                        Core.Actions.CastSpell(4213, Core.CharacterFilter.Id);
                        return;
                    case RecallTypes.aerlinthe:
                        if (!Core.CharacterFilter.IsSpellKnown(2041))
                        {
                            PortalActionList[3].fireaction = false;
                            WriteToChat("You do not know Aerlinthe Recall.  Action disabled.");
                            return;
                        }
                        WriteToChat("Recalled to Aerlinthe");
                        Core.CharacterFilter.ActionComplete += PortalCast_ListenComplete;
                        Core.Actions.CastSpell(2041, Core.CharacterFilter.Id);
                        return;
                    case RecallTypes.caul:
                        if (!Core.CharacterFilter.IsSpellKnown(2943))
                        {
                            PortalActionList[3].fireaction = false;
                            WriteToChat("You do not know Singularity Caul Recall.  Action disabled.");
                            return;
                        }
                        WriteToChat("Recalled to Singularity Caul");
                        Core.CharacterFilter.ActionComplete += PortalCast_ListenComplete;
                        Core.Actions.CastSpell(2943, Core.CharacterFilter.Id);
                        return;
                    case RecallTypes.bur:
                        if (!Core.CharacterFilter.IsSpellKnown(4084))
                        {
                            PortalActionList[3].fireaction = false;
                            WriteToChat("You do not know Bur Recall.  Action disabled.");
                            return;
                        }
                        WriteToChat("Recalled to Bur");
                        Core.CharacterFilter.ActionComplete += PortalCast_ListenComplete;
                        Core.Actions.CastSpell(4084, Core.CharacterFilter.Id);
                        return;
                    case RecallTypes.olthoi_north:
                        if (!Core.CharacterFilter.IsSpellKnown(4198))
                        {
                            PortalActionList[3].fireaction = false;
                            WriteToChat("You do not know Paradox-touched Olthoi Infested Area Recall.  Action disabled.");
                            return;
                        }
                        WriteToChat("Recalled to Paradox-touched Olthoi Infested Area");
                        Core.CharacterFilter.ActionComplete += PortalCast_ListenComplete;
                        Core.Actions.CastSpell(4198, Core.CharacterFilter.Id);
                        return;
                    case RecallTypes.facilityhub:
                        if (!Core.CharacterFilter.IsSpellKnown(5175))
                        {
                            PortalActionList[3].fireaction = false;
                            WriteToChat("You do not know the Facility Hub Recall.  Action disabled.");
                            return;
                        }
                        WriteToChat("Recalled to Facility Hub");
                        Core.CharacterFilter.ActionComplete += PortalCast_ListenComplete;
                        Core.Actions.CastSpell(5175, Core.CharacterFilter.Id);
                        return;
                    case RecallTypes.gearknight:
                        if (!Core.CharacterFilter.IsSpellKnown(5330))
                        {
                            PortalActionList[3].fireaction = false;
                            WriteToChat("You do not know the Gear Knight Invasion Area Camp Recall.  Action disabled.");
                            return;
                        }
                        WriteToChat("Recalled to Gear Knight Invasion Area Camp");
                        Core.CharacterFilter.ActionComplete += PortalCast_ListenComplete;
                        Core.Actions.CastSpell(5330, Core.CharacterFilter.Id);
                        return;
                    case RecallTypes.neftet:
                        if (!Core.CharacterFilter.IsSpellKnown(5541))
                        {
                            PortalActionList[3].fireaction = false;
                            WriteToChat("You do not know the Lost City of Neftet Recall.  Action disabled.");
                            return;
                        }
                        WriteToChat("Recalled to Lost City of Neftet");
                        Core.CharacterFilter.ActionComplete += PortalCast_ListenComplete;
                        Core.Actions.CastSpell(5541, Core.CharacterFilter.Id);
                        return;
                    case RecallTypes.mhoire:
                        if (!Core.CharacterFilter.IsSpellKnown(4128))
                        {
                            PortalActionList[3].fireaction = false;
                            WriteToChat("You do not know the Mhoire Forge Recall.  Action disabled.");
                            return;
                        }
                        WriteToChat("Recalled to Mhoire Forge");
                        Core.CharacterFilter.ActionComplete += PortalCast_ListenComplete;
                        Core.Actions.CastSpell(4128, Core.CharacterFilter.Id);
                        return;
                    default:
						return;
													
				}	
        	}catch(Exception ex){LogError(ex);}
        }
        
        private bool PortalCastSuccess = false;
        private void PortalCast_Listen(object sender, EventArgs e)
        {
        	try
        	{
        		PortalCastSuccess = true;
        		Core.CharacterFilter.ChangePortalMode -= PortalCast_Listen;
        	}catch(Exception ex){LogError(ex);}
        }
        
        private void PortalCast_ListenComplete(object sender, EventArgs e)
        {
        	try
        	{
        		PortalCastSuccess = true;
        		Core.CharacterFilter.ActionComplete -= PortalCast_ListenComplete;
        		
        	}catch(Exception ex){LogError(ex);}
        }
    }
}//end of namespace


