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

        public class DGRControls
        {
        	public HudPictureBox ControlPictureBox = new HudPictureBox();
        	public string ControlName = String.Empty;
        	public string ToolTipName = String.Empty;
        }
        
        private List<DGRControls> DGRControlsList = new List<DGRControls>();
       
        private HudView DynamicGearRemoteView = null;
        private HudTabView DynamicGearRemoteTabView = null;
        private HudFixedLayout DynamicGearRemoteLayout = null;
        private HudStaticText DynamicGearRemoteClock = null;

        GearRemoteSettings mGearRemoteSettings = new GearRemoteSettings();

        public class GearRemoteSettings
        {
            public bool GearTacticianRendered = false;
            public bool GearSenseRendered = false;
            public bool GearVisectionRendered = false;
            public bool GearTaskerRendered = false;
            public bool GearButlerRendered = false;
            public bool GearInspectorRendered = false;
            public bool GearPortalRendered = false;
            public bool GearSwitchHRendered = false;
            public bool GearSwitchVRendered = false;
            public bool GearInventoryRendered = false;
            public bool GearArmorRendered = false;
        }

        private void LoadRemoteGearSettings()
        {
            try
            {
        
	         	if(!File.Exists(remoteGearFilename))
            	{
            		saveRemoteGearSettings();
            	}
            	
            	using (XmlReader reader = XmlReader.Create(remoteGearFilename))
				{	
						XmlSerializer serializer = new XmlSerializer(typeof(GearRemoteSettings));
						mGearRemoteSettings = (GearRemoteSettings)serializer.Deserialize(reader);
						reader.Close();
				}
            }
            catch (Exception ex) { LogError(ex); }
        }


        private void SetRenderState()
        {
            //Called from GearFoundry Start Routines to determine huds to make visible
            //Load saved Xdoc into mGearRemoteSettings values
           	try
           	{
	            if (mGearRemoteSettings.GearTacticianRendered) { RenderTacticianHud(); }
	            if (mGearRemoteSettings.GearSenseRendered) { RenderLandscapeHud(); }
	            if (mGearRemoteSettings.GearVisectionRendered) { RenderCorpseHud(); }
	            if (mGearRemoteSettings.GearTaskerRendered) { RenderKillTaskPanel(); }
	            if (mGearRemoteSettings.GearButlerRendered) { RenderButlerHud(); }
	            if (mGearRemoteSettings.GearInspectorRendered) { RenderItemHud(); }
	            if (mGearRemoteSettings.GearPortalRendered) { RenderPortalGearHud(); }
	            if (mGearRemoteSettings.GearSwitchHRendered) { RenderHorizontalQuickSlots(); }
	            if (mGearRemoteSettings.GearSwitchVRendered) { RenderVerticalQuickSlots(); }
	            if (mGearRemoteSettings.GearInventoryRendered) { RenderInventoryHud(); }
	         	if (mGearRemoteSettings.GearArmorRendered) { RenderArmorHud(); }
           }
           catch (Exception ex) { LogError(ex); }
        }
        
        private void UpdateRemoteGearSettings()
        {
        	try
        	{
        		if (TacticianHudView != null) mGearRemoteSettings.GearTacticianRendered = true;
        		else mGearRemoteSettings.GearTacticianRendered = false;
        		
                if (LandscapeHudView != null) mGearRemoteSettings.GearSenseRendered = true;
                else mGearRemoteSettings.GearSenseRendered = false;
                
                if (CorpseHudView != null) mGearRemoteSettings.GearVisectionRendered = true;
                else mGearRemoteSettings.GearVisectionRendered = false;
                
                if (TaskHudView != null) mGearRemoteSettings.GearTaskerRendered = true;
                else mGearRemoteSettings.GearTaskerRendered = false;
                
                if (ButlerHudView != null) mGearRemoteSettings.GearButlerRendered = true;
                else mGearRemoteSettings.GearButlerRendered = false;
                
                if (ItemHudView != null) mGearRemoteSettings.GearInspectorRendered = true;
                else mGearRemoteSettings.GearInspectorRendered = false;
                
                if (DynamicPortalGearView != null) mGearRemoteSettings.GearPortalRendered = true;
                else mGearRemoteSettings.GearPortalRendered = false;
                
                if (quickiesvHud != null) mGearRemoteSettings.GearSwitchVRendered = true;
                else mGearRemoteSettings.GearSwitchVRendered = false;
                
                if (quickieshHud != null) mGearRemoteSettings.GearSwitchHRendered = true;
                else mGearRemoteSettings.GearSwitchHRendered = false;
                
                if (InventoryHudView != null) mGearRemoteSettings.GearInventoryRendered = true;
                else mGearRemoteSettings.GearInventoryRendered = false;
                
                if (ArmorHudView != null) mGearRemoteSettings.GearArmorRendered = true;
               	else mGearRemoteSettings.GearArmorRendered = false;
        		
                
                saveRemoteGearSettings();   
        	}catch(Exception ex){LogError(ex);}
        }

        private void saveRemoteGearSettings()
        {
            try
            {
            	WriteToChat("Saved Settings");
            	
                if(File.Exists(remoteGearFilename))
				{
                	File.Delete(remoteGearFilename);
				}	
				using (XmlWriter writer = XmlWriter.Create(remoteGearFilename))
				{
					XmlSerializer serializer2 = new XmlSerializer(typeof(GearRemoteSettings));
		   			serializer2.Serialize(writer, mGearRemoteSettings);
		   			writer.Close();
				}
            }
            catch (Exception ex) { LogError(ex); }
        }

        private void RenderDynamicRemoteGear()
        {
        	try
        	{
        		if(DynamicGearRemoteView != null)
        		{
        			DisposeDynamicRemote();
        		}
        		
        		AssembleDGRControls();
        		
        		int DGRViewHeight = 35 + 35 * DGRControlsList.Count;
        		
        		DynamicGearRemoteView = new HudView("", 35, DGRViewHeight, GearGraphics.RemoteGearIcon, false, "RemoteGear");
	            DynamicGearRemoteView.ShowInBar = false;
	            DynamicGearRemoteView.UserAlphaChangeable = false;
	            DynamicGearRemoteView.Visible = true;
	            DynamicGearRemoteView.UserClickThroughable = false;
	            DynamicGearRemoteView.UserGhostable = true;
	            DynamicGearRemoteView.UserMinimizable = false;
	            DynamicGearRemoteView.UserResizeable = false;
	            DynamicGearRemoteView.LoadUserSettings();
	            
	            DynamicGearRemoteTabView = new HudTabView();
	            DynamicGearRemoteView.Controls.HeadControl = DynamicGearRemoteTabView;
				
	            DynamicGearRemoteLayout = new HudFixedLayout();
	            DynamicGearRemoteTabView.AddTab(DynamicGearRemoteLayout, ""); 

	            DynamicGearRemoteClock = new HudStaticText();
	            DynamicGearRemoteLayout.AddControl(DynamicGearRemoteClock, new Rectangle(0,4,30,16));
	            DynamicGearRemoteClock.FontHeight = 8;
	            
				MasterTimer.Tick += DynamicGearClock;	            

	            
	            for(int i = 0; i < DGRControlsList.Count; i++)
	            {
	            	DynamicGearRemoteLayout.AddControl(DGRControlsList[i].ControlPictureBox, new Rectangle(2, 35 + 35 * i, 25,25));
	            	if(DGRControlsList[i].ControlName == "Butler") VirindiViewService.TooltipSystem.AssociateTooltip(DGRControlsList[i].ControlPictureBox, "GearButler"); 
	            	if(DGRControlsList[i].ControlName == "Visection") VirindiViewService.TooltipSystem.AssociateTooltip(DGRControlsList[i].ControlPictureBox, "GearVisection");
	            	if(DGRControlsList[i].ControlName == "Inspector") VirindiViewService.TooltipSystem.AssociateTooltip(DGRControlsList[i].ControlPictureBox, "GearInspector");
	            	if(DGRControlsList[i].ControlName == "Sense") VirindiViewService.TooltipSystem.AssociateTooltip(DGRControlsList[i].ControlPictureBox, "GearSense");
	            	if(DGRControlsList[i].ControlName == "Tactician") VirindiViewService.TooltipSystem.AssociateTooltip(DGRControlsList[i].ControlPictureBox, "GearTactician");
	            	if(DGRControlsList[i].ControlName == "Inventory") VirindiViewService.TooltipSystem.AssociateTooltip(DGRControlsList[i].ControlPictureBox, "Gear");
	            	if(DGRControlsList[i].ControlName == "Armor") VirindiViewService.TooltipSystem.AssociateTooltip(DGRControlsList[i].ControlPictureBox, "Armor");
	            	if(DGRControlsList[i].ControlName == "VSG") VirindiViewService.TooltipSystem.AssociateTooltip(DGRControlsList[i].ControlPictureBox, "Vertical SwitchGear");
	            	if(DGRControlsList[i].ControlName == "HSG") VirindiViewService.TooltipSystem.AssociateTooltip(DGRControlsList[i].ControlPictureBox, "Horizontal SwitchGear");
	            	if(DGRControlsList[i].ControlName == "Portal") VirindiViewService.TooltipSystem.AssociateTooltip(DGRControlsList[i].ControlPictureBox, "PortalGear");
	            	if(DGRControlsList[i].ControlName == "Tasker") VirindiViewService.TooltipSystem.AssociateTooltip(DGRControlsList[i].ControlPictureBox, "GearTasker");

	            }
        	}catch(Exception ex){LogError(ex);}
        }
        
        private void DynamicGearClock(object sender, EventArgs e)
        {
        	try
        	{
        		 DynamicGearRemoteClock.Text = DateTime.Now.ToShortTimeString();
        	}catch(Exception ex){LogError(ex);}
        }
        
        private void DisposeDynamicRemote()
        {
        	try
        	{
        		MasterTimer.Tick -= DynamicGearClock;
        		DestroyDGRControls();
        		
           
	            DynamicGearRemoteClock.Dispose();
	            DynamicGearRemoteLayout.Dispose();
	            DynamicGearRemoteTabView.Dispose();
	            DynamicGearRemoteView.Dispose();
	            
	            DynamicGearRemoteView = null;
        		
        	}catch(Exception ex){LogError(ex);}
        }
        
        private void DGRButler_Hit(object sender, EventArgs e)
        {
        	try
        	{
        		if (ButlerHudView != null)
                {
                	DisposeButlerHud();
                }
                else
                {
                	RenderButlerHud();
                }    
				UpdateRemoteGearSettings();                
        	}catch(Exception ex){LogError(ex);}
        }
        
        private void DGRVisection_Hit(object sender, EventArgs e)
        {
        	try
        	{
        		if (CorpseHudView == null)
                {
                	RenderCorpseHud();
                }
                else
                {
                	DisposeCorpseHud();
                }
       			UpdateRemoteGearSettings();
        	}catch(Exception ex){LogError(ex);}
        }
        
        private void DGRInspector_Hit(object sender, EventArgs e)
        {
        	try
        	{
        		if (ItemHudView != null)
                {
                    DisposeItemHud();
                }
                else
                {
                	RenderItemHud();
                }
       			UpdateRemoteGearSettings();
        	}catch(Exception ex){LogError(ex);}
        }
        
        private void DGRSense_Hit(object sender, EventArgs e)
        {
        	try
        	{
        		if (LandscapeHudView != null)
                {
                	DisposeLandscapeHud();
                }
                else
                {
                    RenderLandscapeHud();
                }
       			UpdateRemoteGearSettings();
        	}catch(Exception ex){LogError(ex);}
        }
        
        private void DGRTactician_Hit(object sender, EventArgs e)
        {
        	try
        	{
        		if (TacticianHudView != null)
                {
                    DisposeTacticianHud();
                }
                else
                {
                    RenderTacticianHud();
                }
       			UpdateRemoteGearSettings();
        	}catch(Exception ex){LogError(ex);}
        }
        
        private void DGRInventory_Hit(object sender, EventArgs e)
        {
        	try
        	{
        		if (InventoryHudView != null)
                {
                    DisposeInventoryHud();
                }
                else
                {
                    RenderInventoryHud();		
                }
       			UpdateRemoteGearSettings();
        	}catch(Exception ex){LogError(ex);}
        }
        
        private void DGRArmor_Hit(object sender, EventArgs e)
        {
        	try
        	{
        		if(ArmorHudView == null)
        		{
        			RenderArmorHud();
        		}
        		else
        		{
        			DisposeArmorHud();
        		}
        		UpdateRemoteGearSettings();
        	}catch(Exception ex){LogError(ex);}
        }
        
        private void DGRVSG_Hit(object sender, EventArgs e)
        {
        	try
        	{
        		if (quickiesvHud == null)
        		{
                	RenderVerticalQuickSlots();
        		}
                else
                {
                	DisposeVerticalQuickSlots();
                }
       			UpdateRemoteGearSettings();
        	}catch(Exception ex){LogError(ex);}
        }
             
		private void DGRHSG_Hit(object sender, EventArgs e)
        {
        	try
        	{
        		if (quickieshHud == null)
                {
              		RenderHorizontalQuickSlots();
                }
                else
                {
                	DisposeHorizontalQuickSlots();
                }
       			UpdateRemoteGearSettings();
        	}catch(Exception ex){LogError(ex);}
        }

		private void DGRPortal_Hit(object sender, EventArgs e)
        {
        	try
        	{
        		if(DynamicPortalGearView == null)
                {
                	RenderPortalGearHud();
                }
                else
                {
                	DisposePortalGearHud();
                }
                UpdateRemoteGearSettings();
        	}catch(Exception ex){LogError(ex);}
        }

		private void DGRTasker_Hit(object sender, EventArgs e)
        {
        	try
        	{
        		if(TaskHudView != null)
            	{
            		DisposeKillTaskPanel();
            	}
            	else
            	{
            		RenderKillTaskPanel();
            	}
            	UpdateRemoteGearSettings();
        	}catch(Exception ex){LogError(ex);}
        }
        
        private void AssembleDGRControls()
        {
        	try
        	{
        		DGRControls dgrc;        		
        		
        		if(mMainSettings.bGearButlerEnabled)
        		{
        			dgrc = new DGRControls();
        			dgrc.ControlPictureBox.Image = GearGraphics.GearBulterIcon;
        			dgrc.ControlPictureBox.Hit += DGRButler_Hit;
        			dgrc.ControlName = "Butler";
        			dgrc.ToolTipName = "GearButler";
        			DGRControlsList.Add(dgrc);
        		}
        		
        		if(mMainSettings.bGearVisection)
        		{
        			dgrc = new DGRControls();
        			dgrc.ControlPictureBox.Image = GearGraphics.GearVisectionIcon;
        			dgrc.ControlPictureBox.Hit += DGRVisection_Hit;
        			dgrc.ControlName = "Visection";
        			dgrc.ToolTipName = "GearVisection";
        			DGRControlsList.Add(dgrc);
        		}
        		
        		if(mMainSettings.bGearInspectorEnabled)
        		{
        			dgrc = new DGRControls();
        			dgrc.ControlPictureBox.Image = GearGraphics.GearInspectorIcon;
        			dgrc.ControlPictureBox.Hit += DGRInspector_Hit;
        			dgrc.ControlName = "Inspector";
        			dgrc.ToolTipName = "GearInspector";
        			DGRControlsList.Add(dgrc);
        		}
        		
        		if(mMainSettings.bGearSenseHudEnabled)
        		{
        			dgrc = new DGRControls();
        			dgrc.ControlPictureBox.Image = GearGraphics.GearSenseIcon;
        			dgrc.ControlPictureBox.Hit += DGRSense_Hit;
        			dgrc.ControlName = "Sense";
        			dgrc.ToolTipName = "GearSense";
        			DGRControlsList.Add(dgrc);
        		}
        		
        		if(mMainSettings.bGearTacticianEnabled)
        		{
        			dgrc = new DGRControls();
        			dgrc.ControlPictureBox.Image = GearGraphics.GearTacticianIcon;
        			dgrc.ControlPictureBox.Hit += DGRTactician_Hit;
        			dgrc.ControlName = "Tactician";
        			dgrc.ToolTipName = "GearTactician";
        			DGRControlsList.Add(dgrc);
        		}
        		
        		if(mMainSettings.binventoryHudEnabled)
        		{
        			dgrc = new DGRControls();
        			dgrc.ControlPictureBox.Image = GearGraphics.GearInventoryIcon;
        			dgrc.ControlPictureBox.Hit += DGRInventory_Hit;
        			dgrc.ControlName = "Inventory";
        			dgrc.ToolTipName = "Gear";
        			DGRControlsList.Add(dgrc);
        		}
        		
        		if(mMainSettings.bArmorHudEnabled)
        		{
        			dgrc = new DGRControls();
                    dgrc.ControlPictureBox.Image = GearGraphics.GearArmorIcon;
        			dgrc.ControlPictureBox.Hit += DGRArmor_Hit;
        			dgrc.ControlName = "Armor";
        			dgrc.ToolTipName = "GearArmor";
        			DGRControlsList.Add(dgrc);
        		}
        		
        		if(mMainSettings.bquickSlotsvEnabled)
        		{
        			dgrc = new DGRControls();
        			dgrc.ControlPictureBox.Image = CreateIconFromResource("gearswapvert1.png");
        			dgrc.ControlPictureBox.Hit += DGRVSG_Hit;
        			dgrc.ControlName = "VSG";
        			dgrc.ToolTipName = "Vertical SwitchGear";
        			DGRControlsList.Add(dgrc);
        			
        		}
        		
        		if(mMainSettings.bquickSlotshEnabled)
        		{
        			dgrc = new DGRControls();
        			dgrc.ControlPictureBox.Image = CreateIconFromResource("gearswaphorz1.png");
        			dgrc.ControlPictureBox.Hit += DGRHSG_Hit;
        			dgrc.ControlName = "HSG";
        			dgrc.ToolTipName = "Horizontal SwitchGear";
        			DGRControlsList.Add(dgrc);
        			
        		}
        		
        		if(mMainSettings.bPortalGearEnabled)
        		{
        			dgrc = new DGRControls();
        			dgrc.ControlPictureBox.Image = GearGraphics.GearPortalIcon;
        			dgrc.ControlPictureBox.Hit += DGRPortal_Hit;
        			dgrc.ControlName = "Portal";
        			dgrc.ToolTipName = "PortalGear";
        			DGRControlsList.Add(dgrc);
        		}
        		
        		if(mMainSettings.bGearTaskerEnabled)
        		{
        			dgrc = new DGRControls();
        			dgrc.ControlPictureBox.Image = GearGraphics.GearTaskerIcon;
        			dgrc.ControlPictureBox.Hit += DGRTasker_Hit;
        			dgrc.ControlName = "Tasker";
        			dgrc.ToolTipName = "GearTasker";
        			DGRControlsList.Add(dgrc);
        		}
        		
        	}catch(Exception ex){LogError(ex);}
        			
        }
        
        private void DestroyDGRControls()
        {
        	try
        	{  
        		foreach(DGRControls control in DGRControlsList)
        		{
        			if(control.ControlName == "Butler")
        			{
        				control.ControlPictureBox.Hit -= DGRButler_Hit;
        				control.ControlPictureBox.Dispose();
        			}
        			if(control.ControlName == "Visection")
        			{
        				control.ControlPictureBox.Hit -= DGRVisection_Hit;
        				control.ControlPictureBox.Dispose();
        			}
        			if(control.ControlName == "Inspector")
        			{
        				control.ControlPictureBox.Hit -= DGRInspector_Hit;
        				control.ControlPictureBox.Dispose();
        			}
        			if(control.ControlName == "Sense")
        			{
        				control.ControlPictureBox.Hit -= DGRSense_Hit;
        				control.ControlPictureBox.Dispose();
        			}
        			if(control.ControlName == "Tactician")
        			{
        				control.ControlPictureBox.Hit -= DGRTactician_Hit;
        				control.ControlPictureBox.Dispose();
        			}
        			if(control.ControlName == "Inventory")
        			{
        				control.ControlPictureBox.Hit -= DGRInventory_Hit;
        				control.ControlPictureBox.Dispose();
        			}
        			if(control.ControlName == "Armor")
        			{
        				control.ControlPictureBox.Hit -= DGRArmor_Hit;
        				control.ControlPictureBox.Dispose();
        			}
        			if(control.ControlName == "VSG")
        			{
        				control.ControlPictureBox.Hit -= DGRVSG_Hit;
        				control.ControlPictureBox.Dispose();
        			}
        			if(control.ControlName == "HSG")
        			{
        				control.ControlPictureBox.Hit -= DGRHSG_Hit;
        				control.ControlPictureBox.Dispose();
        			}
        			if(control.ControlName == "Portal")
        			{
        				control.ControlPictureBox.Hit -= DGRPortal_Hit;
        				control.ControlPictureBox.Dispose();
        			}
        			if(control.ControlName == "Tasker")
        			{
        				control.ControlPictureBox.Hit -= DGRTasker_Hit;
        				control.ControlPictureBox.Dispose();
        			}
        		}
        		
        		DGRControlsList.Clear();
        		
        	}catch(Exception ex){LogError(ex);}
        			
        }
    }
}//end of namespace


