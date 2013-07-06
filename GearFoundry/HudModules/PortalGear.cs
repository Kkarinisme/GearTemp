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
        
        private Queue<PortalActions> PortalActionQueue = new Queue<PortalActions>();
        private System.Windows.Forms.Timer PortalActionTimer = new System.Windows.Forms.Timer();
        
     	private class PortalActions
     	{
     		public PAction Action = PAction.DeQueue;
			public bool pending = false;
			public DateTime StartAction = DateTime.MinValue;
			public int ItemId = 0;	
			public RecallTypes RecallSpell = RecallTypes.none;			
     	}	
			
		public enum PAction
		{
			UnEquipWeapon,
			PeaceMode,
			EquipCaster,
			CastMode,
			Recall,		
			DeQueue
		}
		
		public enum RecallTypes
		{
			none,
			portal,
			lifestone,
			primaryporal,
			secondaryportal
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
                    XDocument tempDoc = new XDocument(new XElement("Portals"));
                    tempDoc.Save(portalGearFilename);
                    tempDoc = null;
                }

                xdocPortalGear = XDocument.Load(portalGearFilename);

                portalGearHud = new VirindiViewService.HudView("", 340, 40, new ACImage(Color.Transparent), false, "PortalGear");
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
                portalGear_Head.AddControl(portalGearTabView, new Rectangle(0, 0, 340, 40));
                portalGearTabView.AddTab(portalGearTabFixedLayout, "");

 

                //Clock
                txtPortalGear = new HudStaticText();
                portalGearTabFixedLayout.AddControl(txtPortalGear, new Rectangle(0, 0, 55, 39));
                VirindiViewService.TooltipSystem.AssociateTooltip(txtPortalGear, "Bedtime yet?");                 
                
  
//            //Portal Recall
//            Stream recallPortalStream = this.GetType().Assembly.GetManifestResourceStream("recall.gif");
//            Image PortalRecallImage = new Bitmap(recallPortalStream);
//            mPortalGear0 = new HudPictureBox();
//            mPortalGear0.Image = (ACImage)PortalRecallImage;
//            portalGearTabFixedLayout.AddControl(mPortalGear0, new Rectangle(60, 2, 25, 39));
//            VirindiViewService.TooltipSystem.AssociateTooltip(mPortalGear0, "Portal Recall");
//            mPortalGear0.Hit += (sender, obj) => mPortalGear0_Hit(sender, obj);


 
            //  Lifestone Recall
            mPortalGear1 = new HudPictureBox();
            int GR_LifestoneRecall_ICON = 0x60024E1;
            mPortalGear1.Image = GR_LifestoneRecall_ICON;
            mPortalGear1.Image = new ACImage(4949);
            portalGearTabFixedLayout.AddControl(mPortalGear1, new Rectangle(90, 2, 25, 39));
            VirindiViewService.TooltipSystem.AssociateTooltip(mPortalGear1, "Lifestone Recall (/@ls)");

            mPortalGear1.Hit += (sender, obj) => mPortalGear1_Hit(sender, obj);

            //House Recall
            mPortalGear2 = new HudPictureBox();
            int GR_HouseRecall_ICON = 0x6001A2A;
            mPortalGear2.Image = GR_HouseRecall_ICON;
            portalGearTabFixedLayout.AddControl(mPortalGear2, new Rectangle(120, 2, 25, 39));
            VirindiViewService.TooltipSystem.AssociateTooltip(mPortalGear2, "House Recall (/@hr)");
            mPortalGear2.Hit += (sender, obj) => mPortalGear2_Hit(sender, obj);

 
            //Mansion Recall
            mPortalGear3 = new HudPictureBox();
            int GR_MansionRecall_ICON = 0x60022DE;
            mPortalGear3.Image = GR_MansionRecall_ICON;
            portalGearTabFixedLayout.AddControl(mPortalGear3, new Rectangle(150, 2, 25, 39));
            VirindiViewService.TooltipSystem.AssociateTooltip(mPortalGear3, "Mansion recall (/@hom)");
            mPortalGear3.Hit += (sender, obj) => mPortalGear3_Hit(sender, obj);

            //Allegiance Hometown Recall
            mPortalGear4 = new HudPictureBox();
            int GR_AHRecall_ICON = 0x60024DD;
            mPortalGear4.Image = GR_AHRecall_ICON;
            portalGearTabFixedLayout.AddControl(mPortalGear4, new Rectangle(180, 2, 25, 29));
            VirindiViewService.TooltipSystem.AssociateTooltip(mPortalGear4, "Allegiance Hometown (/@ah)");
            mPortalGear4.Hit += (sender, obj) => mPortalGear4_Hit(sender, obj);

//            //Recall lifestone via spell
//            //Stream PortalLifestoneRecallStream = this.GetType().Assembly.GetManifestResourceStream("lsrecall.gif");
//            //Image PortalLifestoneRecallImage = new Bitmap(PortalLifestoneRecallStream);
//            string strPortalLifestoneRecallImage = GearDir + @"\lsrecall.gif";
//            Image PortalLifestoneRecallImage = new Bitmap(strPortalLifestoneRecallImage);
//            mPortalGear5 = new HudPictureBox();
//            mPortalGear5.Image = (ACImage)PortalLifestoneRecallImage;
//            portalGearTabFixedLayout.AddControl(mPortalGear5, new Rectangle(180,2, 25, 25));
//            VirindiViewService.TooltipSystem.AssociateTooltip(mPortalGear5, "Recall Spell Lifestone");
//            mPortalGear5.Hit += (sender, obj) => mPortalGear5_Hit(sender, obj);


//            //Recall Portal I
//            //Stream recallPortalIStream = this.GetType().Assembly.GetManifestResourceStream("recallP1.gif");
//            //Image PortalRecallIImage = new Bitmap(recallPortalIStream);
//            string strPortalRecallIImage = GearDir + @"\recallP1.gif";
//            Image PortalRecallIImage = new Bitmap(strPortalRecallIImage);
//            mPortalGear6 = new HudPictureBox();
//            mPortalGear6.Image = (ACImage)PortalRecallIImage;
//            portalGearTabFixedLayout.AddControl(mPortalGear6, new Rectangle(210,2, 25, 25));
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
//            portalGearTabFixedLayout.AddControl(mPortalGear7, new Rectangle(240,2, 25, 25));
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
//            portalGearTabFixedLayout.AddControl(mPortalGear8, new Rectangle(270,2, 25, 25));
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
//            portalGearTabFixedLayout.AddControl(mPortalGear9, new Rectangle(300,2, 25, 25));
//            VirindiViewService.TooltipSystem.AssociateTooltip(mPortalGear9, "Summon Portal II");
//            mPortalGear9.Hit += (sender, obj) => mPortalGear9_Hit(sender, obj);
            
            MasterTimer.Tick += MasterTimer_UpdateClock;
            }catch(Exception ex){LogError(ex);}
 
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


        private void mPortalGear0_Hit(object sender, System.EventArgs e)
        {
            try
            {
                 
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
        
        private void PortalActionsLoadQueue(RecallTypes recall)
        {
        	
        	try
        	{
        		if(PortalActionsPending) 
        		{
        			WriteToChat("Portal action pending.  Please wait for completion.");
        			return;
        		}
        		
        		
        		//Not holding a caster
        		if(Core.WorldFilter.GetInventory().Where(x => x.Values(LongValueKey.EquippedSlots) > 0 && x.ObjectClass == ObjectClass.WandStaffOrb).Count() == 0)
        		{
        			if(Core.Actions.CombatMode != CombatState.Peace)
        			{
        				PortalActions peace = new PortalActions();
        				peace.Action = PAction.PeaceMode;
        				PortalActionQueue.Enqueue(peace);
        			}
        			
        			if(Core.WorldFilter.GetInventory().Where(x => x.Values(LongValueKey.EquippedSlots) > 0 && 
        			                                         (x.ObjectClass == ObjectClass.MeleeWeapon || x.ObjectClass == ObjectClass.MissileWeapon)).Count() > 0)
        			{
	        			PortalActions unequip = new PortalActions();
    	    			unequip.Action = PAction.UnEquipWeapon;
    	    			unequip.ItemId = Core.WorldFilter.GetInventory().Where(x => x.Values(LongValueKey.EquippedSlots) > 0 && 
    	    			                 (x.ObjectClass == ObjectClass.MeleeWeapon || x.ObjectClass == ObjectClass.MissileWeapon)).First().Id;
    	    			PortalActionQueue.Enqueue(unequip);
        			}
        			
        			PortalActions equip = new PortalActions();
        			equip.Action = PAction.EquipCaster;
        			PortalActionQueue.Enqueue(equip);
        			
        			PortalActions castmode = new PortalActions();
        			castmode.Action = PAction.CastMode;
        			equip.ItemId = Core.WorldFilter.GetInventory().Where(x => x.ObjectClass == ObjectClass.WandStaffOrb).First().Id;
        			PortalActionQueue.Enqueue(castmode);
        		}
        		else if(Core.Actions.CombatMode != CombatState.Magic)
        		{
        			PortalActions castmode = new PortalActions();
        			castmode.Action = PAction.CastMode;
        			PortalActionQueue.Enqueue(castmode);
          		}
        		
        		PortalActions castwhat = new PortalActions();
        		castwhat.Action = PAction.Recall;
        		castwhat.RecallSpell = recall;
        		PortalActionQueue.Enqueue(castwhat);    

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
					Core.WorldFilter.ChangeObject += ChangeObject_Portal;
					Core.CharacterFilter.ActionComplete += PortalActionComplete;
					return;
        		}
        		else
        		{
        			if(PortalActionQueue.Count > 0){return;}
					else
					{
						PortalActionsPending = false;
						PortalActionTimer.Tick -= PortalActionInitiator;
						Core.WorldFilter.ChangeObject -= ChangeObject_Portal;
						Core.CharacterFilter.ActionComplete -= PortalActionComplete;
						
						PortalActionTimer.Stop();
						return;
					}
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

				if(PortalActionsPending && PortalActionQueue.Count == 0)
				{
					PortalActionsPending = false;
					PortalActionTimer.Tick -= PortalActionInitiator;
					Core.WorldFilter.ChangeObject -= ChangeObject_Portal;
					Core.CharacterFilter.ActionComplete -= PortalActionComplete;
					PortalActionTimer.Stop();
					return;
				}
					
				if(PortalActionQueue.First().pending && PortalActionQueue.First().Action != PAction.DeQueue)
				{
					if((DateTime.Now - PortalActionQueue.First().StartAction).TotalSeconds < 2)	{return;}
				}

				PortalActionQueue.First().StartAction = DateTime.Now;
				PortalActionQueue.First().pending = true;
				
				switch(PortalActionQueue.First().Action)
				{
					case PAction.DeQueue:
						PortalActionQueue.Dequeue();
						return;
					case PAction.PeaceMode:
						Core.RenderFrame += RenderFramePortal_CombatMode;
						return;
					case PAction.UnEquipWeapon:
						Core.RenderFrame += RenderFramePortal_UnEquip;
						return;
					case PAction.EquipCaster:
						Core.RenderFrame += RenderFramePortal_Equip;
						return;
					case PAction.CastMode:
						Core.RenderFrame += RenderFramePortal_CombatMode;
						return;
					default:
						Core.RenderFrame += RenderFramePortal_CastSpell;
						return;
				}
			}catch(Exception ex){LogError(ex);}
		}
        
        private void RenderFramePortal_CombatMode(object sender, EventArgs e)
		{
			try
			{	
				if((DateTime.Now - PortalActionQueue.First().StartAction).TotalMilliseconds < 100){return;}
				else
				{
					Core.RenderFrame -= RenderFramePortal_CombatMode; 
				}
				
				if(PortalActionQueue.First().Action == PAction.PeaceMode)
				{
					Core.Actions.SetCombatMode(CombatState.Peace);	
				}
				else if(PortalActionQueue.First().Action == PAction.CastMode)
				{
					Core.Actions.SetCombatMode(CombatState.Magic);	
				}
				
				PortalActionQueue.First().StartAction = DateTime.Now;
				Core.RenderFrame += RenderFramePortal_SwitchCombatWait;				
				
			}catch(Exception ex){LogError(ex);}
		}
        
        private void RenderFramePortal_SwitchCombatWait(object sender, EventArgs e)
		{
			try
			{	
				if(PortalActionQueue.First().Action == PAction.PeaceMode)
				{
					if(Core.Actions.CombatMode == CombatState.Peace && (DateTime.Now - PortalActionQueue.First().StartAction).TotalMilliseconds > 1000)
					{
						Core.RenderFrame -= RenderFramePortal_SwitchCombatWait;
						PortalActionQueue.First().Action = PAction.DeQueue;
						return;
					}
					else
					{
						return;
					}
				}
				else if(PortalActionQueue.First().Action == PAction.CastMode)
				{
					if(Core.Actions.CombatMode == CombatState.Magic && (DateTime.Now - PortalActionQueue.First().StartAction).TotalMilliseconds > 1000)
					{
						Core.RenderFrame -= RenderFramePortal_SwitchCombatWait;
						PortalActionQueue.First().Action = PAction.DeQueue;
						return;
					}
					else
					{
						return;
					}
				}
			}catch(Exception ex){LogError(ex);}
		}
        
        private void ChangeObject_Portal(object sender, ChangeObjectEventArgs e)
        {
        	try
        	{
        		if(e.Change == WorldChangeType.StorageChange)
        		{
	        		if(PortalActionQueue.First().ItemId == e.Changed.Id)
	        		{
						PortalActionQueue.First().Action = PAction.DeQueue;
	        		}
        		}	
        	}catch(Exception ex){LogError(ex);}
        }
		
        private void RenderFramePortal_UnEquip(object sender, EventArgs e)
        {
        	try
			{	
        		if((DateTime.Now - PortalActionQueue.First().StartAction).TotalMilliseconds < 100) {return;}
				else
        		{
					Core.RenderFrame -= RenderFramePortal_UnEquip;
					Core.Actions.UseItem(PortalActionQueue.First().ItemId, 0);
					return;
				}
				//List in change item to flag for dequeue
				
			}catch(Exception ex){LogError(ex);}
		}
        
        private void RenderFramePortal_Equip(object sender, EventArgs e)
        {
        	try
			{	
        		if((DateTime.Now - PortalActionQueue.First().StartAction).TotalMilliseconds < 100) {return;}
				else
        		{
					Core.RenderFrame -= RenderFramePortal_Equip;
					Core.Actions.UseItem(PortalActionQueue.First().ItemId, 0);
					return;
				}
				//List in change item to flag for dequeue	
			}catch(Exception ex){LogError(ex);}
		}
        
        private void RenderFramePortal_CastSpell(object sender, EventArgs e)
        {
        	try
        	{
        		if((DateTime.Now - PortalActionQueue.First().StartAction).TotalMilliseconds < 100) {return;}
				else
        		{
					switch(PortalActionQueue.First().RecallSpell)
					{
						case RecallTypes.lifestone:
							Core.Actions.CastSpell(1635, Core.CharacterFilter.Id);
							return;
						case RecallTypes.portal:
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
        
        private void PortalActionComplete(object sender, EventArgs e)
        {
        	try
        	{
        		if(PortalActionQueue.First().Action == PAction.Recall)
        		{
        			PortalActionQueue.First().Action = PAction.DeQueue;
        		}
        		
        	}catch(Exception ex){LogError(ex);}
        }
        



    }
}//end of namespace


