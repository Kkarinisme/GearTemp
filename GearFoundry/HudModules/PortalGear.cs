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
        private static VirindiViewService.Controls.HudTabView portalGearTabView = null;
        private static VirindiViewService.Controls.HudFixedLayout portalGearTabFixedLayout = null;

        private HudStaticText txtPortalGear = null;
        private HudPictureBox mSelectCastor = null;
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
     	}	
		
		public enum RecallTypes
		{
			none,
			portal,
			lifestone,
			primaryporal,
			secondaryportal
		}
		
		private void SubscribePortalEvents()
		{
			try
			{
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
                    WriteToChat("PortalGearfilename does not exist.");
                    XDocument tempDoc = new XDocument(new XElement("Settings"));
                    tempDoc.Save(portalGearFilename);
                    tempDoc = null;
                    nOrbGuid = Core.WorldFilter.GetInventory().Where(x => x.ObjectClass == ObjectClass.WandStaffOrb).First().Id;
                    nOrbIcon = 0x6002A3C;

                }
                else
                {

                    xdocPortalGear = XDocument.Load(portalGearFilename);

                    XElement el = xdocPortalGear.Root.Element("Setting");

                    nOrbGuid = Convert.ToInt32(el.Element("OrbGuid").Value);
                    nOrbIcon = Convert.ToInt32(el.Element("OrbIcon").Value);
                    WriteToChat("nOrbIcon = " + nOrbIcon);
                }

                portalGearHud = new VirindiViewService.HudView("", 370, 40, new ACImage(Color.Transparent), false, "PortalGear");
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
                mSelectCastor = new HudPictureBox();
                int GR_SelectCastor_ICON = nOrbIcon;
                mSelectCastor.Image = (ACImage)GR_SelectCastor_ICON;
                portalGearTabFixedLayout.AddControl(mSelectCastor, new Rectangle(60, 2, 25, 39));
                VirindiViewService.TooltipSystem.AssociateTooltip(mSelectCastor, "Select Castor");
                mSelectCastor.Hit += (sender, obj) => mSelectCastor_Hit(sender, obj);

                
  
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
            mPortalGear1.Image = new ACImage(4949);
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

//            //Recall lifestone via spell
//            //Stream PortalLifestoneRecallStream = this.GetType().Assembly.GetManifestResourceStream("lsrecall.gif");
//            //Image PortalLifestoneRecallImage = new Bitmap(PortalLifestoneRecallStream);
//            string strPortalLifestoneRecallImage = GearDir + @"\lsrecall.gif";
//            Image PortalLifestoneRecallImage = new Bitmap(strPortalLifestoneRecallImage);
//            mPortalGear5 = new HudPictureBox();
//            mPortalGear5.Image = (ACImage)PortalLifestoneRecallImage;
//            portalGearTabFixedLayout.AddControl(mPortalGear5, new Rectangle(240,2, 25, 25));
//            VirindiViewService.TooltipSystem.AssociateTooltip(mPortalGear5, "Recall Spell Lifestone");
//            mPortalGear5.Hit += (sender, obj) => mPortalGear5_Hit(sender, obj);


//            //Recall Portal I
//            //Stream recallPortalIStream = this.GetType().Assembly.GetManifestResourceStream("recallP1.gif");
//            //Image PortalRecallIImage = new Bitmap(recallPortalIStream);
//            string strPortalRecallIImage = GearDir + @"\recallP1.gif";
//            Image PortalRecallIImage = new Bitmap(strPortalRecallIImage);
//            mPortalGear6 = new HudPictureBox();
//            mPortalGear6.Image = (ACImage)PortalRecallIImage;
//            portalGearTabFixedLayout.AddControl(mPortalGear6, new Rectangle(270,2, 25, 25));
//            VirindiViewService.TooltipSystem.AssociateTooltip(mPortalGear6, "Recall Portal I");
//            mPortalGear6.Hit += (sender, obj) => mPortalGear6_Hit(sender, obj);
//
//
//            //Summon Portal I
//            //Stream summonPortalIStream = this.GetType().Assembly.GetManifestResourceStream("summonP1.gif");
//            //Image PortalSummonIImage = new Bitmap(summonPortalIStream);
//            string strPortalSummonIImage = GearDir + @"\summonP1.gif";
//            Image PortalSummonIImage = new Bitmap(strPortalSummonIImage);
//            mPortalGear7 = new HudPictureBox();
//            mPortalGear7.Image = (ACImage)PortalSummonIImage;
//            portalGearTabFixedLayout.AddControl(mPortalGear7, new Rectangle(300,2, 25, 25));
//            VirindiViewService.TooltipSystem.AssociateTooltip(mPortalGear7, "Summon Portal I");
//            mPortalGear7.Hit += (sender, obj) => mPortalGear7_Hit(sender, obj);
//
//            //Recall Portal II
//            //Stream recallPortalIIStream = this.GetType().Assembly.GetManifestResourceStream("recallP2.gif");
//            //Image PortalRecallIIImage = new Bitmap(recallPortalIIStream);
//            string strPortalRecallIIImage = GearDir + @"\recallP2.gif";
//            Image PortalRecallIIImage = new Bitmap(strPortalRecallIIImage);
//            mPortalGear8 = new HudPictureBox();
//            mPortalGear8.Image = (ACImage)PortalRecallIIImage;
//            portalGearTabFixedLayout.AddControl(mPortalGear8, new Rectangle(330,2, 25, 25));
//            VirindiViewService.TooltipSystem.AssociateTooltip(mPortalGear8, "Recall Portal I");
//            mPortalGear6.Hit += (sender, obj) => mPortalGear8_Hit(sender, obj);
//
//
//            //Summon Portal II
//            //Stream summonPortalIIStream = this.GetType().Assembly.GetManifestResourceStream("summonP2.gif");
//            //Image PortalSummonIIImage = new Bitmap(summonPortalIIStream);
//            string strPortalSummonIIImage = GearDir + @"\summonP2.gif";
//            Image PortalSummonIIImage = new Bitmap(strPortalSummonIIImage);
//            mPortalGear9 = new HudPictureBox();
//            mPortalGear9.Image = (ACImage)PortalSummonIIImage;
//            portalGearTabFixedLayout.AddControl(mPortalGear9, new Rectangle(360,2, 25, 25));
//            VirindiViewService.TooltipSystem.AssociateTooltip(mPortalGear9, "Summon Portal II");
//            mPortalGear9.Hit += (sender, obj) => mPortalGear9_Hit(sender, obj);
            
           
            SubscribePortalEvents();
            
            }catch(Exception ex){LogError(ex);}
 
        }
        private void DisposePortalGearHud()
        {
        	UnsubscribePortalEvents();

            if (mSelectCastor != null) { mSelectCastor.Hit -= (sender, obj) => mSelectCastor_Hit(sender, obj); mSelectCastor.Dispose(); }
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
				if(Core.WorldFilter[Core.Actions.CurrentSelection] != null)
				{
                     int objSelectedID = e.ItemGuid;
                     
                     foreach (Decal.Adapter.Wrappers.WorldObject obj in Core.WorldFilter.GetInventory())
                    {
                        if (obj.Id == objSelectedID)
                        {
                            nOrbGuid = obj.Id;
                            nOrbIcon = obj.Icon;
                            savePortalSettings();
                            RenderPortalGearHud();
                            break;

                        }

                    }

                     Core.ItemSelected -= PortalItemSelected;

                }
            }catch(Exception ex){LogError(ex);}
        }



        private void mSelectCastor_Hit(object sender, System.EventArgs e)
        {
            try
            {
                WriteToChat("Please select castor from pack that should be used for spell recalls if not holding a wand when call requested.");
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
        
        private void PortalActionsLoad(RecallTypes recall)
        {
        	
        	try
        	{
        		if(PortalActionsPending) 
        		{
        			WriteToChat("Portal action pending.  Please wait for completion.");
        			return;
        		}
        		
        		WriteToChat("Inventory = " + Core.WorldFilter.GetInventory().Where(x => x.Values(LongValueKey.EquippedSlots) == 0x1000000).Count().ToString());
        		
        		//Not holding a caster
        		if(Core.WorldFilter.GetInventory().Where(x => x.Values(LongValueKey.EquippedSlots) == 0x1000000).Count() == 0)
        		{
        			WriteToChat("CombatState = " + Core.Actions.CombatMode.ToString());
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
        		PortalActionList[2].RecallSpell = recall;  

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
					PortalActionTimer.Interval = 100;
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
					if(PortalActionList[0].pending && (DateTime.Now - PortalActionList[0].StartAction).TotalMilliseconds < 1000)
					{
						return;
					}
					else if(Core.Actions.CombatMode != CombatState.Peace)
					{
						WriteToChat("Firing Peace State");
						PortalActionList[0].pending = true;
						PortalActionList[0].StartAction = DateTime.Now;
						Core.RenderFrame += RenderFramePortal_PeaceMode;
						return;
					}
					else
					{
						WriteToChat("Peace State Disabled");
						PortalActionList[0].pending = false;
						PortalActionList[0].StartAction = DateTime.MinValue;
						PortalActionList[0].fireaction = false;
					}
				}
				else if(PortalActionList[1].fireaction)
				{
					if(PortalActionList[1].pending && (DateTime.Now - PortalActionList[0].StartAction).TotalMilliseconds < 300)
					{
						return;
					}
					else if(Core.WorldFilter.GetInventory().Where(x => x.ObjectClass == ObjectClass.WandStaffOrb && x.LongKeys.Contains((int)LongValueKey.EquippedSlots)).Count() == 0)
					{	
						WriteToChat("Changing Gear");
						PortalActionList[1].pending = true;
						PortalActionList[1].StartAction = DateTime.Now;
						Core.RenderFrame += RenderFramePortal_Equip;
						return;
					}
					else
					{
						WriteToChat("Gear Change Complete");
						PortalActionList[1].pending = false;
						PortalActionList[1].StartAction = DateTime.MinValue;
						PortalActionList[1].pending = false;
					}
				}
				else if(PortalActionList[2].fireaction)
				{
					if(PortalActionList[2].pending && (DateTime.Now - PortalActionList[0].StartAction).TotalMilliseconds < 1000)
					{
						return;
					}
					else if(Core.Actions.CombatMode != CombatState.Magic)
					{
						WriteToChat("Entering Magic State");
						PortalActionList[2].pending = true;
						PortalActionList[2].StartAction = DateTime.Now;
						Core.RenderFrame += RenderFramePortal_CastMode;
						return;
					}
					else
					{
						WriteToChat("Magic State Complete");
						PortalActionList[2].pending = false;
						PortalActionList[2].StartAction = DateTime.MinValue;
						PortalActionList[2].pending = false;
					}
				}
				else if(PortalActionList[3].fireaction)
				{
					if(PortalActionList[3].pending && (DateTime.Now - PortalActionList[0].StartAction).TotalMilliseconds < 1000)
					{
						return;
					}
					else if(!PortalCastSuccess && PortalActionList[3].RecallSpell != RecallTypes.none)
					{
						WriteToChat("Casting");
						PortalActionList[3].pending = true;
						PortalActionList[3].StartAction = DateTime.Now;
						Core.CharacterFilter.ChangePortalMode += PortalCast_Listen;
						Core.RenderFrame += RenderFramePortal_CastSpell;
						return;
					}
					else
					{
						WriteToChat("Cast Completed");
						PortalActionList[3].pending = false;
						PortalActionList[3].StartAction = DateTime.MinValue;
						PortalActionList[3].pending = false;
						PortalCastSuccess = false;
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
        
        private void RenderFramePortal_PeaceMode(object sender, EventArgs e)
		{
			try
			{				

				if((DateTime.Now - PortalActionList[0].StartAction).TotalMilliseconds < 100){return;}
				else
				{
					Core.RenderFrame -= RenderFramePortal_PeaceMode; 
				}
				
				PortalActionList[0].StartAction = DateTime.Now;
				Core.Actions.SetCombatMode(CombatState.Peace);	
				return;
				
			}catch(Exception ex){LogError(ex);}
		}
            
        private void RenderFramePortal_Equip(object sender, EventArgs e)
        {
        	try
			{	
        		if((DateTime.Now - PortalActionList[1].StartAction).TotalMilliseconds < 100) {return;}
				else
        		{
					Core.RenderFrame -= RenderFramePortal_Equip;
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
				}
				//List in change item to flag for dequeue	
			}catch(Exception ex){LogError(ex);}
		}
        
        private void RenderFramePortal_CastMode(object sender, EventArgs e)
		{
			try
			{				
				if((DateTime.Now - PortalActionList[2].StartAction).TotalMilliseconds < 100){return;}
				else
				{
					Core.RenderFrame -= RenderFramePortal_CastMode; 
				}
					
				PortalActionList[2].StartAction = DateTime.Now;
				Core.Actions.SetCombatMode(CombatState.Magic);			
				return;
				
			}catch(Exception ex){LogError(ex);}
		}
        
        private void RenderFramePortal_CastSpell(object sender, EventArgs e)
        {
        	try
        	{
        		if((DateTime.Now - PortalActionList[3].StartAction).TotalMilliseconds < 100) {return;}
				else
        		{
					WriteToChat("Cast Spell " + PortalActionList[3].RecallSpell.ToString());
					switch(PortalActionList[3].RecallSpell)
					{
						case RecallTypes.lifestone:
							Core.Actions.CastSpell(1635, Core.CharacterFilter.Id);
							return;
						case RecallTypes.portal:
							WriteToChat("Hit recall type portal.");
							Core.Actions.CastSpell(2645, Core.CharacterFilter.Id);
							return;
						case RecallTypes.primaryporal:
							Core.Actions.CastSpell(48, Core.CharacterFilter.Id);
							return;
						case RecallTypes.secondaryportal:
							Core.Actions.CastSpell(2647, Core.CharacterFilter.Id);
							return;
						default:
							return;
														
					}
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
    }
}//end of namespace


