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
        XDocument xdocRemoteGear = null;
        
        public class DGRControls
        {
        	public ACImage ControlIcon = new ACImage();
        	public string ControlName = String.Empty;
        	public string ToolTipName = String.Empty;
        }
        
        private List<DGRControls> DGRControlsList = new List<DGRControls>();
       
        private HudView DynamicGearRemoteView = null;
        private HudTabView DynamicGearRemoteTabView = null;
        private HudFixedLayout DynamicGearRemoteLayout = null;
        private HudButton DynamicGearRemoteClock = null;
        private HudList DynamicGearRemoteHudList = null;
        private HudList.HudListRowAccessor DRGHudAcessor = null;

        private void RenderDynamicRemoteGear()
        {
        	try
        	{
        		if(DynamicRemoteView != null)
        		{
        			DisposeDynamicRemote();
        		}
        		
        		AssembleDGRControls();
        		int DGRViewHeight = 30 + 30 * DGRControlsList.Count;
        		
        		DynamicGearRemoteHudView = new HudView("", 30, DGRViewHeight, GearGraphics.RemoteGearIcon, false, "RemoteGear");
	            DynamicGearRemoteHudView.ShowInBar = false;
	            DynamicGearRemoteHudView.UserAlphaChangeable = false;
	            DynamicGearRemoteHudView.Visible = true;
	            DynamicGearRemoteHudView.UserClickThroughable = false;
	            DynamicGearRemoteHudView.UserGhostable = true;
	            DynamicGearRemoteHudView.UserMinimizable = false;
	            DynamicGearRemoteHudView.UserResizeable = false;
	            DynamicGearRemoteHudView.LoadUserSettings();
	            
	            DynamicGearRemoteTabView = new HudTabView();
	            DynamicGearRemoteHudView.Controls.HeadControl = DynamicGearRemoteTabView;
				
	            DynamicGearRemoteLayout = new HudFixedLayout();
	            DynamicGearRemoteTabView.AddTab(DynamicGearRemoteLayout, ""); 
	            
	            DynamicGearRemoteClock = new HudButton();
	            DynamicGearRemoteLayout.AddControl(DynamicGearRemoteClock, new Rectangle(0,0,25,25));
	            DynamicGearRemoteClock.Text = DateTime.Now;
	            //TODO:  Make the clock tick.

	            DynamicGearRemoteHudList = new HudList();
	            DynamicGearRemoteLayout.AddControl(0,25,25,DGRControlsList.Count * 25);
	            DynamicGearRemoteHudList.ControlHeight = 25;
	            DynamicGearRemoteHudList.ScrollBarWidth = 0;
	            DynamicGearRemoteHudList.AddColumn(typeof(HudPictureBox), 25, null);
	            
	            HudList.HudListRowAccessor acessor = new HudList.HudListRowAccessor();
	            foreach(DGRControls dr in DGRControlsList)
	            {
	            	DRGHudAcessor = DynamicGearRemoteHudList.AddRow();
	            	((HudPictureBox)DRGHudAcessor[0]).Image = dr.ControlIcon;
	            	VirindiViewService.TooltipSystem.AssociateTooltip(DRGHudAcessor[0], dr.ToolTipName);
	            }
	            
	            DynamicGearRemoteHudList.Hit += DynamicGearRemoteHudList_Hit;
        		
        		
        		
        	}catch(Exception ex){LogError(ex);}
        }
        
        private void DisposeDynamicRemote()
        {
        	try
        	{
        		
        	}catch(Exception ex){LogError(ex);}
        }
        
        private void DynamicGearRemoteHudList_Hit(object sender, int row, int col)
        {
        	try
        	{
        		DRGHudAcessor = DynamicGearRemoteHudList[row];
        		
        		switch(((HudPictureBox)DRGHudAcessor[0]).Image)
        		{
        			case GearGraphics.GearBulterIcon:
						if (ButlerHudView != null)
		                {
		                	DisposeButlerHud();
		                }
		                else
		                {
		                	RenderButlerHud();
		                }  
						return;		                
					    
					case GearGraphics.GearVisectionIcon:
						if (CorpseHudView == null)
		                {
		                	RenderCorpseHud();
		                }
		                else
		                {
		                	DisposeCorpseHud();
		                }
		                return;
		                
		           case GearGraphics.GearInspectorIcon:
		                if (ItemHudView != null)
		                {
		                    DisposeItemHud();
		                }
		                else
		                {
		                	RenderItemHud();
		                }
		                return;
		             
		          case GearGraphics.GearSenseIcon:
		               if (LandscapeHudView != null)
		                {
		                	DisposeLandscapeHud();
		                }
		                else
		                {
		                    RenderLandscapeHud();
		                }
		                return;
		            
		           case GearGraphics.GearTacticianIcon:
			            if (TacticianHudView != null)
		                {
		                    DisposeTacticianHud();
		                }
		                else
		                {
		                    RenderTacticianHud();
		                }
		                return;
		                
		          case GearGraphics.GearInventoryIcon:
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
		                return;
		                
		                case GearGraphics.GearArmorIcon;
		           case GearGraphics.VertSwitchGearIcon;
		                if (quickiesvHud != null)
                		{
		                	RenderVerticalQuickSlots();
                		}
		                else
		                {
		                	DisposeVerticalQuickSlots();
		                }
		                return;
		                

		           case GearGraphics.HoriSwitchGearIcon;
	                  	if (quickieshHud != null)
		                {
	                  		RenderHorizontalQuickSlots();
		                }
		                else
		                {
		                	DisposeHorizontalQuickSlots();
		                }
		                return;
	                
	               case GearGraphics.GearPortalIcon:
		                if(portalGearHud != null)
		                {
		                	RenderPortalGearHud();
		                }
		                else
		                {
		                	DisposePortalGearHud();
		                }
		                if(portalRecallGearHud != null)
		                {
		                	RenderPortal2GearHud();
		                }
		                else
		                {
		                	DisposePortalRecallGearHud();
		                }
						return;
			                
		           	case GearGraphics.GearTaskerIcon:
		                if(TaskHudView != null)
		            	{
		            		DisposeKillTaskPanel();
		            	}
		            	else
		            	{
		            		RenderKillTaskPanel();
		            	}
		            	return;
		            	
		            case default:
		            	WriteToChat("Remote Gear Error");
		            	return;
		
        		}
        		
        		
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
        			dgrc.ControlIcon = GearGraphics.GearBulterIcon;
        			dgrc.ControlName = "Butler";
        			dgrc.ToolTipName = "GearButler"
        			DGRControlsList.Add(dgrc);
        		}
        		
        		if(mMainSettings.bGearVisection)
        		{
        			dgrc = new DGRControls();
        			dgrc.ControlIcon = GearGraphics.GearVisectionIcon;
        			dgrc.ControlName = "Visection";
        			dgrc.ToolTipName = "GearVisection"
        			DGRControlsList.Add(dgrc);
        		}
        		
        		if(mMainSettings.bGearInspectorEnabled)
        		{
        			dgrc = new DGRControls();
        			dgrc.ControlIcon = GearGraphics.GearInspectorIcon;
        			dgrc.ControlName = "Inspector";
        			dgrc.ToolTipName = "GearInspector"
        			DGRControlsList.Add(dgrc);
        		}
        		
        		if(mMainSettings.bGearSenseHudEnabled)
        		{
        			dgrc = new DGRControls();
        			dgrc.ControlIcon = GearGraphics.GearSenseIcon;
        			dgrc.ControlName = "Sense";
        			dgrc.ToolTipName = "GearSense"
        			DGRControlsList.Add(dgrc);
        		}
        		
        		if(mMainSettings.bGearTacticianEnabled)
        		{
        			dgrc = new DGRControls();
        			dgrc.ControlIcon = GearGraphics.GearTacticianIcon;
        			dgrc.ControlName = "Tactician";
        			dgrc.ToolTipName = "GearTactician"
        			DGRControlsList.Add(dgrc);
        		}
        		
        		if(mMainSettings.binventoryHudEnabled)
        		{
        			dgrc = new DGRControls();
        			dgrc.ControlIcon = GearGraphics.GearInventoryIcon;
        			dgrc.ControlName = "Inventory";
        			dgrc.ToolTipName = "Gear"
        			DGRControlsList.Add(dgrc);
        		}
        		
        		if(mMainSettings.bArmorHudEnabled)
        		{
        			dgrc = new DGRControls();
        			dgrc.ControlIcon = new ACImage(Color.Black);
        			dgrc.ControlName = "Armor";
        			dgrc.ToolTipName = "GearArmor"
        			DGRControlsList.Add(dgrc);
        		}
        		
        		if(mMainSettings.bquickSlotsvEnabled)
        		{
        			dgrc = new DGRControls();
        			dgrc.ControlIcon = CreateIconFromResource("gearswapvert1.png");
        			GearGraphics.VertSwitchGearIcon = Convert.ToInt32(dgrc.ControlIcon);
        			dgrc.ControlName = "VSG";
        			dgrc.ToolTipName = "Vertical SwitchGear"
        			DGRControlsList.Add(dgrc);
        			
        		}
        		
        		if(mMainSettings.bquickSlotshEnabled)
        		{
        			dgrc = new DGRControls();
        			dgrc.ControlIcon = CreateIconFromResource("gearswaphorz1.png");
        			GearGraphics.HoriSwitchGearIcon = Convert.ToInt32(dgrc.ControlIcon);
        			dgrc.ControlName = "HSG";
        			dgrc.ToolTipName = "Horizontal SwitchGear";
        			DGRControlsList.Add(dgrc);
        			
        		}
        		
        		if(mMainSettings.bPortalGearEnabled)
        		{
        			dgrc = new DGRControls();
        			dgrc.ControlIcon = GearGraphics.GearPortalIcon;
        			dgrc.ControlName = "Portal";
        			dgrc.ToolTipName = "PortalGear"
        			DGRControlsList.Add(dgrc);
        		}
        		
        		if(mMainSettings.bGearTaskerEnabled)
        		{
        			dgrc = new DGRControls();
        			dgrc.ControlIcon = GearGraphics.GearTaskerIcon;
        			dgrc.ControlName = "Tasker";
        			dgrc.ToolTipName = "GearTasker"
        			DGRControlsList.Add(dgrc);
        		}
        		
        	}catch(Exception ex){LogError(ex);}
        			
        }
    }
}//end of namespace


