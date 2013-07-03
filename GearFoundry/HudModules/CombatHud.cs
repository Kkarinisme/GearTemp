
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
	
		public DateTime CombatHudLastUpdate = DateTime.MinValue;
		
		private HudView CombatHudView = null;
		private HudFixedLayout CombatHudLayout = null;
		private HudTabView CombatHudTabView = null;
		private HudFixedLayout CombatHudMainTab = null;
		private HudFixedLayout CombatHudSettingsTab = null;
		
		private HudImageStack CombatHudTargetImage = null;
				
		private HudProgressBar CombatHudTargetHealth = null;
		private HudButton CombatHudFocusSet = null;
		private HudButton CombatHudFocusClear = null;
		
		private HudCheckBox CHTrackCreature = null;
		private HudCheckBox CHTrackItem = null;
		private HudCheckBox CHTrackLife = null;
		private HudCheckBox CHTRackVoid = null;
		private HudCheckBox CHRenderMedium = null;
		private HudCheckBox CHRenderMinimal = null;
		private HudCheckBox CHShowAll = null;
		
		
		private HudImageStack[] CombatHudMiniVulArray = null;
		
		private HudList CombatHudDebuffTrackerList = null;
		
		private HudList.HudListRowAccessor CombatHudRow = null;
		
		//private const int CombatHudGoodBackground = 0x6003355;
//		private const int CombatHudWarningBackground = 0x600335B;
//		private const int CombatHudExpiringBackground = 0x6003359;	
//		private const int CombatHudFocusTargetBackground =  0x600335C;
//		private const int CombatHudCurrentTargetBackground =  0x60011F4;
//		private const int CombatHudNeutralBackground = 0x600109A;
		private ACImage CombatHudGoodBackground = new ACImage(Color.LightSeaGreen);
		private ACImage CombatHudWarningBackground = new ACImage(Color.LightYellow);
		private ACImage CombatHudExpiringBackground = new ACImage(Color.OrangeRed);
		private ACImage CombatHudFocusTargetBackground =  new ACImage(Color.DarkRed);
		private ACImage CombatHudCurrentTargetBackground =  new ACImage(Color.MediumVioletRed);
		private ACImage CombatHudNeutralBackground = new ACImage(Color.Black);
		private ACImage FocusBar = new ACImage(Color.DarkRed);
		private ACImage CurrentBar =  new ACImage(Color.MediumVioletRed);
		private ACImage RedBar = new ACImage(Color.Red);
		private  ACImage EmptyBar = new ACImage(Color.Black);
				
		private Rectangle CombatHudTargetRectangle = new Rectangle(0,0,50,50);
		private Rectangle CombatHudMiniVulsRectangle = new Rectangle(0,0,16,16);
		private Rectangle CombatHudListVulsRectangle = new Rectangle(0,0,16,16);

		
		private void RenderCombatHud()
		{
			try
			{
				CombatHudReadWriteSettings(true);	
			}catch(Exception ex){LogError(ex);}
			
			try
			{
			
				if(CombatHudView != null)
				{
					DisposeCombatHud();
				}

				CombatHudView = new HudView("GearTactician", gtSettings.CombatHudWidth, gtSettings.CombatHudHeight, new ACImage(0x6AA8));
				CombatHudView.Visible = true;
				CombatHudView.UserAlphaChangeable = false;
				CombatHudView.ShowInBar = false;
				CombatHudView.UserClickThroughable = false;
				CombatHudView.UserMinimizable = false;
				if(gtSettings.bCombatHudMinimal){CombatHudView.UserResizeable = false;}
				else{CombatHudView.UserResizeable = true;}
				
				CombatHudView.LoadUserSettings();
				
				CombatHudLayout = new HudFixedLayout();
				CombatHudView.Controls.HeadControl = CombatHudLayout;
				
				CombatHudTabView = new HudTabView();
				CombatHudLayout.AddControl(CombatHudTabView, new Rectangle(0,0, CombatHudView.Width, CombatHudView.Height));
				
				CombatHudMainTab = new HudFixedLayout();
				CombatHudTabView.AddTab(CombatHudMainTab, "GearTactician");
				
				CombatHudSettingsTab = new HudFixedLayout();
				CombatHudTabView.AddTab(CombatHudSettingsTab, "Settings");
				
				CombatHudTabView.OpenTabChange += CombatHudTabView_OpenTabChange;
				if(CombatHudView.UserResizeable){CombatHudView.Resize += CombatHudView_Resize;}
				
                if(bCombatHudSettingsTab) {RenderCombatHudSettingsTab();}
                else{RenderCombatHudMainTab();}
                
				
				SubscribeCombatEvents();
						
			}catch(Exception ex){LogError(ex);}
			return;
		}
		
		private void AlterCombatHud()
		{		
			try
			{
				//Clean up old combathud tabs
				DisposeCombatHudMainTab();
				DisposeCombatHudSettingsTab();
				CombatHudTabView.OpenTabChange -= CombatHudTabView_OpenTabChange;
				CombatHudLayout.Dispose();
				CombatHudTabView.Dispose();
				CombatHudMainTab.Dispose();
				CombatHudSettingsTab.Dispose();
			
				CombatHudView.Height = gtSettings.CombatHudHeight;
				CombatHudView.Width = gtSettings.CombatHudWidth;

				if(gtSettings.bCombatHudMinimal){CombatHudView.UserResizeable = false;}
				else{CombatHudView.UserResizeable = true;}			
				
				CombatHudLayout = new HudFixedLayout();
				CombatHudView.Controls.HeadControl = CombatHudLayout;
				
				CombatHudTabView = new HudTabView();
				CombatHudLayout.AddControl(CombatHudTabView, new Rectangle(0,0, CombatHudView.Width, CombatHudView.Height));
				
				CombatHudMainTab = new HudFixedLayout();
				CombatHudTabView.AddTab(CombatHudMainTab, "GearTactician");
				
				CombatHudSettingsTab = new HudFixedLayout();
				CombatHudTabView.AddTab(CombatHudSettingsTab, "Settings");
				
				CombatHudTabView.OpenTabChange += CombatHudTabView_OpenTabChange;
				if(CombatHudView.UserResizeable){CombatHudView.Resize += CombatHudView_Resize;}
				
                if(bCombatHudSettingsTab) {RenderCombatHudSettingsTab();}
                else{RenderCombatHudMainTab();}
						
			}catch(Exception ex){LogError(ex);}
			return;
		}
		

        private void CombatHudView_Resize(object sender, System.EventArgs e)
        {
            try
            {
            	if(bCombatHudSettingsTab) { return;}
                bool bw = Math.Abs(CombatHudView.Width - gtSettings.CombatHudWidth) > 20;
                bool bh = Math.Abs(CombatHudView.Height - gtSettings.CombatHudHeight) > 20;
                if (bh || bw)
                {
                    gtSettings.CombatHudWidth = CombatHudView.Width;
                    gtSettings.CombatHudHeight = CombatHudView.Height;
                    CombatHudResizeTime = DateTime.Now;
                    Core.RenderFrame += new EventHandler<EventArgs>(CombatHudResizeWait);
                }
            }
            catch (Exception ex) { LogError(ex); }
            return;



        }
		
        DateTime CombatHudResizeTime;
        private void CombatHudResizeWait(object sender, EventArgs e)
        {
        	if((DateTime.Now - CombatHudResizeTime).TotalMilliseconds > 500)
        	{
	        	Core.RenderFrame -= CombatHudResizeWait;       	

           		CombatHudReadWriteSettings(false);
            	AlterCombatHud();
        	}
        }


		private void CombatHudTabView_OpenTabChange(object sender, System.EventArgs e)
		{
			try
			{
				 switch(CombatHudTabView.CurrentTab)
    			{
    				case 0:
				 		DisposeCombatHudSettingsTab();
    					RenderCombatHudMainTab();
    					return;
    				case 1:
    					DisposeCombatHudMainTab();
    					RenderCombatHudSettingsTab();
    					return;
    			}
				
			}catch(Exception ex){LogError(ex);}
		}
		
		private void DisposeCombatHud()
		{
			try
			{
				UnsubscribeCombatEvents();
				DisposeCombatHudMainTab();
				DisposeCombatHudSettingsTab();
				
				if(CombatHudView.UserResizeable) {CombatHudView.Resize -= CombatHudView_Resize;}
				
				CombatHudTabView.OpenTabChange -= CombatHudTabView_OpenTabChange;
				
				CombatHudSettingsTab.Dispose();
				CombatHudMainTab.Dispose();
				CombatHudTabView.Dispose();
				CombatHudLayout.Dispose();
				CombatHudView.Dispose();
				
			}catch(Exception ex){LogError(ex);}
		}
		
		
		private void RenderCombatHudMainTab()
		{
			try
			{				
				if(gtSettings.bCombatHudMedium)
				{
					RenderMedium();
				}
				else if(gtSettings.bCombatHudMinimal)
				{
					RenderMinimal();
				}
				else
				{
					RenderFull();
				}
				
				if(CombatHudDebuffTrackerList != null)
				{
					CombatHudDebuffTrackerList.Click +=  (sender, row, col) => CombatHudDebuffTrackerList_Click(sender, row, col);
				}	

				CombatHudFocusSet.Hit += CombatHudFocusSet_Hit;
				CombatHudFocusClear.Hit += CombatHudFocusClear_Hit;

				UpdateCombatHudMainTab();

			}catch(Exception ex){LogError(ex);}
		}
		
		private void RenderMinimal()
		{
			try
			{
				CombatHudFocusSet = new HudButton();
				CombatHudFocusSet.Text = "F";
				CombatHudMainTab.AddControl(CombatHudFocusSet, new Rectangle(0,0,20,16));

				CombatHudFocusClear = new HudButton();
				CombatHudFocusClear.Text = "R";
				CombatHudMainTab.AddControl(CombatHudFocusClear, new Rectangle(22,0,20,16));	

				CombatHudTargetHealth = new HudProgressBar();
                CombatHudTargetHealth.ProgressEmpty = EmptyBar;
				CombatHudTargetHealth.ProgressFilled = RedBar;
                CombatHudTargetHealth.FontHeight = 10;
				CombatHudTargetHealth.Min = 0;
				CombatHudTargetHealth.Max = 100;
				CombatHudMainTab.AddControl(CombatHudTargetHealth, new Rectangle(45,0,180,16));

				CombatHudMiniVulArray = new HudImageStack[10];
				for(int i = 0; i < 10; i++)
				{
					CombatHudMiniVulArray[i] = new HudImageStack();
					CombatHudMainTab.AddControl(CombatHudMiniVulArray[i], new Rectangle((i*18) + 230,0,16,16));
				}		
				CombatHudDebuffTrackerList = null;	

				bCombatHudMainTab = true;				
				
			}catch(Exception ex){LogError(ex);}
		}
		
		private void RenderMedium()
		{
			try
			{
				CombatHudFocusSet = new HudButton();
				CombatHudFocusSet.Text = "Focus";
				CombatHudMainTab.AddControl(CombatHudFocusSet, new Rectangle(0,0,35,16));

				CombatHudFocusClear = new HudButton();
				CombatHudFocusClear.Text = "Reset";
				CombatHudMainTab.AddControl(CombatHudFocusClear, new Rectangle(0,20,35,16));

				CombatHudDebuffTrackerList = new HudList();
				CombatHudMainTab.AddControl(CombatHudDebuffTrackerList, new Rectangle(40,0, CombatHudView.Width -40, CombatHudView.Height -10));
				CombatHudDebuffTrackerList.ControlHeight = 12; 
				CombatHudDebuffTrackerList.AddColumn(typeof(HudProgressBar), 150, null);
                for (int i = 0; i < 10; i++)
				{
					CombatHudDebuffTrackerList.AddColumn(typeof(HudImageStack), 12, null);
				}	
				CombatHudDebuffTrackerList.AddColumn(typeof(HudStaticText), 1, null);	
			
				bCombatHudMainTab = true;	
				
				
			}catch(Exception ex){LogError(ex);}
		}
		
		private void RenderFull()
		{
			try
			{
				CombatHudTargetImage = new HudImageStack();

                CombatHudMainTab.AddControl(CombatHudTargetImage, new Rectangle(25, 20, 50, 50));

                CombatHudFocusSet = new HudButton();
                CombatHudFocusSet.Text = "Focus";
                CombatHudMainTab.AddControl(CombatHudFocusSet, new Rectangle(5, 75, 35, 16));

                CombatHudFocusClear = new HudButton();
                CombatHudFocusClear.Text = "Reset";
                CombatHudMainTab.AddControl(CombatHudFocusClear, new Rectangle(45, 75, 35, 16));

				CombatHudTargetHealth = new HudProgressBar();
                CombatHudTargetHealth.ProgressEmpty = EmptyBar;
				CombatHudTargetHealth.ProgressFilled = RedBar;

                CombatHudTargetHealth.FontHeight = 10;
				CombatHudTargetHealth.Min = 0;
				CombatHudTargetHealth.Max = 100;
				CombatHudMainTab.AddControl(CombatHudTargetHealth, new Rectangle(5,115,95,16));

				CombatHudMiniVulArray = new HudImageStack[10];
				for(int i = 0; i < 10; i++)
				{
					CombatHudMiniVulArray[i] = new HudImageStack();
				}

				for(int i = 0; i < 10; i++)
				{
					if(i < 5)
					{
						CombatHudMainTab.AddControl(CombatHudMiniVulArray[i], new Rectangle((i*20),140,16,16));
					}
					else
					{
						CombatHudMainTab.AddControl(CombatHudMiniVulArray[i], new Rectangle(((i-5)*20),160,16,16));
					}
				}

				CombatHudDebuffTrackerList = new HudList();
				CombatHudMainTab.AddControl(CombatHudDebuffTrackerList, new Rectangle(110,0,CombatHudView.Width - 110,CombatHudView.Height));
				CombatHudDebuffTrackerList.ControlHeight = 12;	
				CombatHudDebuffTrackerList.AddColumn(typeof(HudProgressBar), 180, null);
				for(int i = 0; i < 10; i++)
				{
					CombatHudDebuffTrackerList.AddColumn(typeof(HudImageStack), 12, null);
				}
                CombatHudDebuffTrackerList.AddColumn((typeof(HudStaticText)), 1, null);
                

				bCombatHudMainTab = true;
	
			}catch(Exception ex){LogError(ex);}
		}
		
		
		private void CombatHudDebuffTrackerList_Click(object sender, int row, int col)
		{
			try
			{
				if(col == 0)
				{
					CombatHudRow = CombatHudDebuffTrackerList[row];
					Core.Actions.SelectItem(Convert.ToInt32(((HudStaticText)CombatHudRow[11]).Text));
				}
				
			}catch(Exception ex){LogError(ex);}
		}
		
		private void DisposeCombatHudMainTab()
		{
			try
			{
				if(!bCombatHudMainTab) {return;}
				
				if(CombatHudFocusSet != null){CombatHudFocusSet.Hit -= CombatHudFocusSet_Hit;}
				if(CombatHudFocusClear != null){CombatHudFocusClear.Hit -= CombatHudFocusClear_Hit;}
				
				if(CombatHudDebuffTrackerList != null) {CombatHudDebuffTrackerList.Dispose();}
				
				if(CombatHudMiniVulArray != null)
				{
					for(int i = 0; i < 10; i++)
					{
						CombatHudMiniVulArray[i].Dispose();
					}
				}
											
				if(CombatHudFocusClear != null){CombatHudFocusClear.Dispose();}
				if(CombatHudFocusSet != null){CombatHudFocusSet.Dispose();}
				if(CombatHudTargetHealth != null){CombatHudTargetHealth.Dispose();}
				
				if(CombatHudTargetImage != null) {CombatHudTargetImage.Dispose();}
				
				bCombatHudMainTab = false;

			}catch(Exception ex){LogError(ex);}
		}
		
		private void CombatHudFocusSet_Hit(object sender, System.EventArgs e)
		{
			try
			{
				if(Core.Actions.CurrentSelection == 0) {return;}
				if(Core.WorldFilter[Core.Actions.CurrentSelection].ObjectClass == ObjectClass.Monster)
				{
					CombatHudFocusTargetGUID = Core.Actions.CurrentSelection;
					IdqueueAdd(CombatHudFocusTargetGUID);
					UpdateCombatHudMainTab();
				}
				else
				{
					WriteToChat("No monster selected.  Hud will not focus on PKs");
				}
			}catch(Exception ex){LogError(ex);}
		}
		
		private void CombatHudFocusClear_Hit(object sender, System.EventArgs e)
		{
			try
			{
				CombatHudFocusTargetGUID = 0;	
				UpdateCombatHudMainTab();
			}catch(Exception ex){LogError(ex);}
			
		}
		
		
		private void RenderCombatHudSettingsTab()
		{
			try
			{
				CHTrackCreature = new HudCheckBox();
				CHTrackCreature.Text = "C";
				CHTrackCreature.Checked = gtSettings.bCombatHudTrackCreatureDebuffs;
				CombatHudSettingsTab.AddControl(CHTrackCreature, new Rectangle(0,0,30,16));
				
				CHTrackItem = new HudCheckBox();
				CHTrackItem.Text = "I";
				CHTrackItem.Checked = gtSettings.bCombatHudTrackItemDebuffs;
				CombatHudSettingsTab.AddControl(CHTrackItem, new Rectangle(40,0,30,16));
				
				CHTrackLife = new HudCheckBox();
				CHTrackLife.Text = "L";
				CHTrackLife.Checked = gtSettings.bCombatHudTrackLifeDebuffs;
				CombatHudSettingsTab.AddControl(CHTrackLife, new Rectangle(80,0,40,16));
				
				CHTRackVoid = new HudCheckBox();
				CHTRackVoid.Text = "V";
				CHTRackVoid.Checked = gtSettings.bCombatHudTrackVoidDebuffs;
				CombatHudSettingsTab.AddControl(CHTRackVoid, new Rectangle(120,0,40,16));
				
				CHRenderMedium = new HudCheckBox();
				CHRenderMedium.Text = "Med";
				CHRenderMedium.Checked = gtSettings.bCombatHudMedium;
				CombatHudSettingsTab.AddControl(CHRenderMedium, new Rectangle(160,0,40,16));
				
				CHRenderMinimal = new HudCheckBox();
				CHRenderMinimal.Text = "Mini";
				CHRenderMinimal.Checked = gtSettings.bCombatHudMinimal;
				CombatHudSettingsTab.AddControl(CHRenderMinimal, new Rectangle(205,0,40,16));
				
				CHShowAll = new HudCheckBox();
				CHShowAll.Text = "All Mobs";
				CHShowAll.Checked = gtSettings.bShowAll;
				CombatHudSettingsTab.AddControl(CHShowAll, new Rectangle(250,0,70,16));
				
				CHTrackCreature.Change += CHTrackCreature_Change;
				CHTrackItem.Change += CHTrackItem_Change;
				CHTrackLife.Change += CHTrackLife_Change;
				CHTRackVoid.Change += CHTRackVoid_Change;
				CHRenderMedium.Change += CHRenderMedium_Change;
				CHRenderMinimal.Change += CHRenderMinimal_Change;
				CHShowAll.Change += CHShowAll_Change;
				
				
			}catch(Exception ex){LogError(ex);}
		}
		
		private void DisposeCombatHudSettingsTab()
		{
			try
			{
				if(!bCombatHudSettingsTab) {return;}
				
				CHTrackCreature.Change -= CHTrackCreature_Change;
				CHTrackItem.Change -= CHTrackItem_Change;
				CHTrackLife.Change -= CHTrackLife_Change;
				CHTRackVoid.Change -= CHTRackVoid_Change;
				CHRenderMedium.Change -= CHRenderMedium_Change;
				CHRenderMinimal.Change -= CHRenderMinimal_Change;
				CHShowAll.Change -= CHShowAll_Change;
				
				CHTrackCreature.Dispose();				
				CHTrackItem.Dispose();
				CHTrackLife.Dispose();				
				CHTRackVoid.Dispose();				
				CHRenderMedium.Dispose();			
				CHRenderMinimal.Dispose();
				
				bCombatHudSettingsTab = false;				
				
			}catch(Exception ex){LogError(ex);}
		}
		
		private void CHShowAll_Change(object sender, EventArgs e)
		{
			try
			{
				gtSettings.bShowAll = CHShowAll.Checked;
				CombatHudReadWriteSettings(false);
			}catch(Exception ex){LogError(ex);}
		}
		
		private void CHTrackCreature_Change(object sender, EventArgs e)
		{
			try
			{
				gtSettings.bCombatHudTrackCreatureDebuffs = CHTrackCreature.Checked;
				CombatHudReadWriteSettings(false);
			}catch(Exception ex){LogError(ex);}
		}
			
		private void CHTrackItem_Change(object sender, EventArgs e)
		{
			try
			{
				gtSettings.bCombatHudTrackItemDebuffs = CHTrackItem.Checked;
				CombatHudReadWriteSettings(false);
			}catch(Exception ex){LogError(ex);}
		}
		
		private void CHTrackLife_Change(object sender, EventArgs e)
		{
			try
			{
				gtSettings.bCombatHudTrackLifeDebuffs = CHTrackLife.Checked;
				CombatHudReadWriteSettings(false);
			}catch(Exception ex){LogError(ex);}
		}
		
		private void CHTRackVoid_Change(object sender, EventArgs e)
		{
			try
			{
				gtSettings.bCombatHudTrackVoidDebuffs = CHTRackVoid.Checked;
				CombatHudReadWriteSettings(false);
				
			}catch(Exception ex){LogError(ex);}
		}
		
		private void CHRenderMedium_Change(object sender, EventArgs e)
		{
			try
			{
				gtSettings.bCombatHudMedium = CHRenderMedium.Checked;
				if(gtSettings.bCombatHudMinimal && gtSettings.bCombatHudMedium)
				{
					gtSettings.bCombatHudMinimal = false;
					CHRenderMinimal.Checked = gtSettings.bCombatHudMinimal;
				}
				if(gtSettings.bCombatHudMedium)
				{
					gtSettings.CombatHudWidth = 420;
					gtSettings.CombatHudHeight = 200;
				}
				else
				{
					gtSettings.CombatHudWidth = 520;
					gtSettings.CombatHudHeight = 220;
				}
				CombatHudReadWriteSettings(false);
				Core.RenderFrame += new EventHandler<EventArgs>(CombatHudResizeWait);
				
			
			}catch(Exception ex){LogError(ex);}
		}
		
		private void CHRenderMinimal_Change(object sender, EventArgs e)
		{
			try
			{
				gtSettings.bCombatHudMinimal = CHRenderMinimal.Checked;
				if(gtSettings.bCombatHudMinimal && gtSettings.bCombatHudMedium)
				{
					gtSettings.bCombatHudMedium = false;		
					CHRenderMedium.Checked = gtSettings.bCombatHudMedium;
				}
				if(gtSettings.bCombatHudMinimal)
				{
					gtSettings.CombatHudWidth = 420;
					gtSettings.CombatHudHeight = 30;
				}
				else
				{
					gtSettings.CombatHudWidth = 520;
					gtSettings.CombatHudHeight = 220;
				}
				CombatHudReadWriteSettings(false);
				Core.RenderFrame += new EventHandler<EventArgs>(CombatHudResizeWait);
			}catch(Exception ex){LogError(ex);}
		}
		
		private void UpdateCombatHudMainTab()
		{
			try
			{				
				if(!bCombatHudMainTab) {return;}
				if(((TimeSpan)(DateTime.Now - CombatHudLastUpdate)).TotalSeconds < 1){return;}
				
				if(CombatHudFocusTargetGUID != 0)
				{
					if(!CombatHudMobTrackingList.Any(x => x.Id == CombatHudFocusTargetGUID))
					{
						CombatHudMobTrackingList.Add(new MonsterObject(Core.WorldFilter[CombatHudFocusTargetGUID]));
					}
					CHTargetIO = CombatHudMobTrackingList.Find(x => x.Id == CombatHudFocusTargetGUID);
				}
				else if(Core.Actions.CurrentSelection != 0 && Core.WorldFilter[Core.Actions.CurrentSelection].ObjectClass == ObjectClass.Monster)
				{
					if(!CombatHudMobTrackingList.Any(x => x.Id == Core.Actions.CurrentSelection))
					{
						CombatHudMobTrackingList.Add(new MonsterObject(Core.WorldFilter[Core.Actions.CurrentSelection]));
					}
					CHTargetIO = CombatHudMobTrackingList.Find(x => x.Id == Core.Actions.CurrentSelection);
				}
				else
				{
					CHTargetIO = null;
				}
			}catch(Exception ex){LogError(ex);}
			try
			{
				//Updates to Target Panel Values
				if(CHTargetIO == null || !CHTargetIO.isvalid)
				{
					if(CombatHudTargetImage != null)
					{
						CombatHudTargetImage.Clear();
						CombatHudTargetImage.Add(CombatHudTargetRectangle, CombatHudNeutralBackground);
					}
					if(CombatHudTargetHealth != null)
					{
						CombatHudTargetHealth.PreText = "No Target";
						CombatHudTargetHealth.Position = 100;
					}
					if(CombatHudMiniVulArray != null)
					{
						for(int i = 0; i < 10; i++)
						{
							CombatHudMiniVulArray[i].Add(CombatHudTargetRectangle, CombatHudNeutralBackground);	
						}
					}
				}
				else
				{
					if(CombatHudTargetImage != null)
					{	
						CombatHudTargetImage.Clear();
						if(CombatHudFocusTargetGUID != 0){CombatHudTargetImage.Add(CombatHudTargetRectangle, CombatHudFocusTargetBackground);}
						else{CombatHudTargetImage.Add(CombatHudTargetRectangle, CombatHudNeutralBackground);}
						CombatHudTargetImage.Add(CombatHudTargetRectangle,  CHTargetIO.Icon);													
					}
					
					if(CombatHudTargetHealth != null)
					{
						CombatHudTargetHealth.PreText = CHTargetIO.Name;
						CombatHudTargetHealth.Position = CHTargetIO.HealthRemaining;
					}
					if(CombatHudMiniVulArray != null)
					{
						for(int i = 0; i < 10; i++)
						{
							if(i < CHTargetIO.DebuffSpellList.Count)
							{
								if(CHTargetIO.DebuffSpellList[i].SecondsRemaining <= 15) {CombatHudMiniVulArray[i].Add(CombatHudMiniVulsRectangle, CombatHudExpiringBackground);}
								else if(CHTargetIO.DebuffSpellList[i].SecondsRemaining > 30) {CombatHudMiniVulArray[i].Add(CombatHudMiniVulsRectangle, CombatHudGoodBackground);}
								else{CombatHudMiniVulArray[i].Add(CombatHudMiniVulsRectangle, CombatHudWarningBackground);}
								CombatHudMiniVulArray[i].Add(CombatHudMiniVulsRectangle, SpellIndex[CHTargetIO.DebuffSpellList[i].SpellId].spellicon);			
							}
							else
							{
								CombatHudMiniVulArray[i].Add(CombatHudTargetRectangle, CombatHudNeutralBackground);	
							}
						}
					}	
				}
				}catch(Exception ex){LogError(ex);}
				try
				{
				
				//Updates to Target Lists
				if(CombatHudDebuffTrackerList != null)
				{
					CombatHudDebuffTrackerList.ClearRows();
					try
					{
					//Pull the Focus Target to the top
					if(CombatHudFocusTargetGUID != 0)
					{
						MonsterObject FocusIO = CombatHudMobTrackingList.Find(x => x.Id == CombatHudFocusTargetGUID);
						CombatHudRow = CombatHudDebuffTrackerList.AddRow();
						((HudProgressBar)CombatHudRow[0]).FontHeight = 10;
						((HudProgressBar)CombatHudRow[0]).PreText = FocusIO.Name;	
						((HudProgressBar)CombatHudRow[0]).Min = 0;
						((HudProgressBar)CombatHudRow[0]).Max = 100;
						((HudProgressBar)CombatHudRow[0]).ProgressFilled = FocusBar;
						((HudProgressBar)CombatHudRow[0]).ProgressEmpty = EmptyBar;
						((HudProgressBar)CombatHudRow[0]).Position = FocusIO.HealthRemaining;
						
						for(int i= 0; i < 10; i++)
						{
							if(i < FocusIO.DebuffSpellList.Count)
							{
								if(FocusIO.DebuffSpellList[i].SecondsRemaining < 15){((HudImageStack)CombatHudRow[i+1]).Add(CombatHudListVulsRectangle, CombatHudExpiringBackground);}
								else if (FocusIO.DebuffSpellList[i].SecondsRemaining > 30){((HudImageStack)CombatHudRow[i+1]).Add(CombatHudListVulsRectangle, CombatHudGoodBackground);}
								else {((HudImageStack)CombatHudRow[i+1]).Add(CombatHudListVulsRectangle, CombatHudWarningBackground);}
								((HudImageStack)CombatHudRow[i+1]).Add(CombatHudListVulsRectangle, SpellIndex[FocusIO.DebuffSpellList[i].SpellId].spellicon);
							}
							else
							{
								((HudImageStack)CombatHudRow[i+1]).Add(CombatHudListVulsRectangle, CombatHudNeutralBackground);
							}
						}
						((HudStaticText)CombatHudRow[11]).Text = FocusIO.Id.ToString();
					}
					}catch(Exception ex){LogError(ex);}
					try
					{

					for(int mobindex = 0; mobindex < CombatHudMobTrackingList.Count; mobindex++)
					{
						if((CombatHudMobTrackingList[mobindex].DebuffSpellList.Count > 0 || gtSettings.bShowAll) && CombatHudMobTrackingList[mobindex].Id != CombatHudFocusTargetGUID)
						{
							CombatHudRow = CombatHudDebuffTrackerList.AddRow();
							((HudProgressBar)CombatHudRow[0]).FontHeight = 10;
							((HudProgressBar)CombatHudRow[0]).PreText = CombatHudMobTrackingList[mobindex].Name;	
							((HudProgressBar)CombatHudRow[0]).Min = 0;
							((HudProgressBar)CombatHudRow[0]).Max = 100;
							((HudProgressBar)CombatHudRow[0]).ProgressEmpty = EmptyBar;
							if(CombatHudMobTrackingList[mobindex].Id == Core.Actions.CurrentSelection) {((HudProgressBar)CombatHudRow[0]).ProgressFilled = CurrentBar;}
							else{((HudProgressBar)CombatHudRow[0]).ProgressFilled = RedBar;}
							((HudProgressBar)CombatHudRow[0]).Position = CombatHudMobTrackingList[mobindex].HealthRemaining;
	
							
							for(int i = 0; i < 10; i++)
							{
								if(i < CombatHudMobTrackingList[mobindex].DebuffSpellList.Count)
								{
									if(CombatHudMobTrackingList[mobindex].DebuffSpellList[i].SecondsRemaining < 15 ) 
									{
										((HudImageStack)CombatHudRow[i+1]).Add(CombatHudListVulsRectangle, CombatHudExpiringBackground);
									}
									else if(CombatHudMobTrackingList[mobindex].DebuffSpellList[i].SecondsRemaining > 60)
									{
										((HudImageStack)CombatHudRow[i+1]).Add(CombatHudListVulsRectangle, CombatHudGoodBackground);
									}
									else
									{
										((HudImageStack)CombatHudRow[i+1]).Add(CombatHudListVulsRectangle, CombatHudWarningBackground);
									}
									((HudImageStack)CombatHudRow[i+1]).Add(CombatHudListVulsRectangle, SpellIndex[CombatHudMobTrackingList[mobindex].DebuffSpellList[i].SpellId].spellicon);
								}
								else
								{
									((HudImageStack)CombatHudRow[i+1]).Add(CombatHudListVulsRectangle, CombatHudNeutralBackground);
								}
								((HudStaticText)CombatHudRow[11]).Text = CombatHudMobTrackingList[mobindex].Id.ToString();
							}
						}	
					}	
					}catch(Exception ex){LogError(ex);}					
				}
					
				CombatHudLastUpdate = DateTime.Now;
				return;

				
			}catch(Exception ex){LogError(ex);}
			
			
		}
		
		
		
	}
}


//Vuln's can *sorta* be tracked. 
//
//If you look at 0xF755 Apply Visual/Sound Effect and look at the "effect" field ... 
//
//0x002B = Fire Vuln 
//0x002D = Pierce Vuln 
//0x002F = Blade Vuln 
//0x0031 = Acid Vuln 
//0x0033 = Cold Vuln 
//0x0035 = Lightning Vuln 
//0x0037 = Bludgeon Vuln or Imperil 
//
//Then look at the "playSpeed" field: 
//
//Level 1 = 0.0 
//Level 2 = 0.2 
//Level 3 = 0.4 
//Level 4 = 0.6 
//Level 5 = 0.8 
//Level 6 or 7 = 1.0 
//
//Protections are similar (although I don't have exact values.) The problems with this are that you don't get this info if you were not "aware" of the monster when it got vulned, you can't tell the difference between a bludgeon vuln and an imperil and you can't tell the difference between a level 6 and 7 spell. 
//
//So, I don't know if that helps you any, but it's the best way I have found to tell what has been vulned.





//Audio/Visual Effect ID
//0x04
//War Launch
//0x05
//War Land
//0x06
//Red Clouds Rising (Strength/Health Buff)
//0x07
//Red Clouds Falling (Strength/Health Debuff)
//0x08
//Orange Clouds Rising (Coordination Buff)
//0x09
//Orange Clouds Falling (Coordination Debuff)
//0x0A
//Yellow Clouds Rising (Endurance Buff)
//0x0B
//Yellow Clouds Falling (Endurance Debuff)
//0x0C
//Green Clouds Rising (Quickness Buff)
//0x0D
//Green Clouds Falling (Quickness Debuff)
//0x0E
//Cyan Clouds Rising (Self Buff, Lifestone Recall/Tie)
//0x0F
//Cyan Clouds Falling (Self Debuff)
//0x10
//Purple Clouds Rising (Focus Buff, Portal Recall/Summon/Tie)
//0x11
//Purple Clouds Falling (Focus Debuff)
//0x12
//Red Bubbles Rising (Weapon Skill Buff)
//0x13
//Red Bubbles Falling (Weapon Skill Debuff)
//0x14
//Orange Bubbles Rising (Allegiance/Crafting Skill Buff)
//0x15
//Orange Bubbles Falling (Allegiance/Crafting Skill Debuff)
//0x16
//Yellow Bubbles Rising (Defense Skill Buff)
//0x17
//Yellow Bubbles Falling (Defense Skill Debuff)
//0x18
//Green Bubbles Rising (Run/Jump Skill Buff)
//0x19
//Green Bubbles Falling (Run/Jump Skill Debuff)
//0x1A
//Cyan Bubbles Rising (Magic/Alchemy Skill Buff)
//0x1B
//Cyan Bubbles Falling (Magic/Alchemy Skill Debuff)
//0x1C
//Purple Bubbles Rising (Assessment/Tinkering Skill Buff, Learn Spell)
//0x1D
//Purple Bubbles Falling (Assessment/Tinkering Skill Debuff)
//0x1E
//Red Stars In (Heal, Infuse Health)
//0x1F
//Red Stars Out (Harm, Drain Health)
//0x20
//Blue Stars In (Mana Boost, Infuse Mana)
//0x21
//Blue Stars Out (Mana Drain, Drain Mana)
//0x22
//Yellow Stars In (Revitalize, Infuse Stamina)
//0x23
//Yellow Stars Out (Enfeeble, Drain Stamina)
//0x24
//Red Stars Rotating Out (Regeneration)
//0x25
//Red Stars Rotating In (Fester)
//0x26
//Blue Stars Rotating Out (Mana Renewal)
//0x27
//Blue Stars Rotating In (Mana Depletion)
//0x28
//Yellow Stars Rotating In (Rejuvenation)
//0x29
//Yellow Stars Rotating Out (Exhaustion)
//0x2A
//Red Shield Rising (Fire Protection)
//0x2B
//Red Shield Falling (Fire Vulnerability)
//0x2C
//Orange Shield Rising (Piercing Protection)
//0x2D
//Orange Shield Falling (Piercing Vulnerability)
//0x2E
//Yellow Shield Rising (Blade Protection)
//0x2F
//Yellow Shield Falling (Blade Vulnerability)
//0x30
//Green Shield Rising (Acid Protection)
//0x31
//Green Shield Falling (Acid Vulnerability)
//0x32
//Cyan Shield Rising (Cold Protection)
//0x33
//Cyan Shield Falling (Cold Vulnerability)
//0x34
//Purple Shield Rising (Lightning Protection)
//0x35
//Purple Shield Falling (Lightning Vulnerability)
//0x36
//Black Shield Rising (Bludgeon Protection, Armor)
//0x37
//Black Shield Falling (Bludgeon Vulnerability, Imperil)
//0x38
//Red/White Sparks (Flame Bane, Blood Drinker)
//0x39
//Red/Black Sparks (Flame Lure, Blood Loather)
//0x3A
//Orange/White Sparks (Piercing Bane, Heart Seeker, Strengthen Lock)
//0x3B
//Orange/Black Sparks (Piercing Lure, Turn Blade, Weaken Lock)
//0x3C
//Yellow/White Sparks (Blade Bane, Defender)
//0x3D
//Yellow/Black Sparks (Blade Lure, Lure Blade)
//0x3E
//Green/White Sparks (Acid Bane, Swift Killer)
//0x3F
//Green/Black Sparks (Acid Lure, Leaden Weapon)
//0x40
//Cyan/White Sparks (Frost Bane)
//0x41
//Cyan/Black Sparks (Frost Lure)
//0x42
//Purple/White Sparks (Bludgeon/Lightning Bane, Hermetic Link)
//0x43
//Purple/Black Sparks (Bludgeon/Lightning Lure, Hermetic Void, Dispel)
//0x48
//Red Stars Out / Yellow Stars In (Health to Stamina)
//0x49
//Red Stars Out / Blue Stars In (Health to Mana)
//0x4A
//Yellow Stars Out / Red Stars In (Stamina to Health)
//0x4B
//Yellow Stars Out / Blue Stars In (Stamina to Mana)
//0x4C
//Blue Stars Out / Red Stars In (Mana to Health)
//0x4D
//Blue Stars Out / Yellow Stars In (Mana to Stamina)
//0x50
//Fizzle
//0x57
//Idle Emote
//0x58
//Item Dissolve
//0x73
//Portal Bubbles
//0x76
//Raise Attribute or Skill
//0x77
//Equip Item
//0x78
//Unequip Item
//0x79
//Give Item
//0x7A
//Pick Up Item
//0x7B
//Drop Item
//0x7E
//Unlock Item
//0x81
//Enchantment Expiration
//0x82
//Item Out of Mana
//0x89
//Gain Level
//0x8D
//White/White Sparks (Impenetrability)
//0x8E
//White/Black Sparks (Brittlemail)
//0x91
//White/Purple Clouds (Life Dispel)
//0x92
//White/Cyan Clouds (Creature Dispel)
//
//

//void
//AnimationList.Add("Zojak Bor",100691559,4,0);  //Corrosion
//AnimationList.Add("Jevak Bor",100691561,4,0);  //Corruption
//AnimationList.Add("Drosta Ves",100691552,168,0);  //Festering Curse 
//AnimationList.Add("Traku Ves",100691553,169,0);  //Weakening Curse
//AnimationList.Add("Slavu Bor",100691551,167,0);  //Destructive Curse

//Creature
//AnimationList.Add("Equin Eatak",100670578,27,1);  //Bottle Breaker
//AnimationList.Add("Equin Eaja",100668285,27,1);  //Hands of Chorizite
//AnimationList.Add("Equin Oloi",100668352,29,1);  //Jibril's Vitae
//AnimationList.Add("Equin Caril",100668277,17,1);  //Synaptic Misfire
//AnimationList.Add("Equin Cavik",100668268,9,1);  //Ataxia
//AnimationList.Add("Equin Guafeth",100670579,21,1);  //Challenger's Legacy
//AnimationList.Add("Equin Ealoi",100668358,27,1);  //Wrath of Adja
//AnimationList.Add("Equin Gualoi",100668296,29,1);  //Hearts on Sleeves
//AnimationList.Add("Equin Ofeth",100669126,23,1);  //Broadside of a Barn
//AnimationList.Add("Equin Luvik",100692227,21,1);  //Dirty Fighting Ineptitude Other VII
//AnimationList.Add("Equin Lureth",100692228,21,1);  //Dual Wield Ineptitude Other VII
//AnimationList.Add("Equin Guavik",100668286,21,1);  //Sashi Mu's Kiss
//AnimationList.Add("Equin Casith",100668296,15,1);  //Self Loathing
//AnimationList.Add("Equin Luzael",100692225,19,1);  //Finesse Weapon Ineptitude Other VII
//AnimationList.Add("Equin Guareth",100670580,21,1);  //Twisted Digits
//AnimationList.Add("Equin Careth",100668273,11,1);  //Brittle Bones
//AnimationList.Add("Equin Guaguz",100668279,21,1);  //Unsteady Hands
//AnimationList.Add("Equin Luril",100692226,19,1);  //Heavy Weapon Ineptitude Other VII
//AnimationList.Add("Equin Eapaj",100668272,27,1);  //Wrath of Celcynd
//AnimationList.Add("Equin Guaja",100668264,29,1);  //Unfortunate Appraisal
//AnimationList.Add("Equin Guasith",100668282,25,1);  //Feat of Radaz
//AnimationList.Add("Equin Guati",100668295,25,1);  //Gears Unwound
//AnimationList.Add("Equin Guatak",100668283,21,1);  //Kwipetian Vision
//AnimationList.Add("Equin Eavik",100668337,27,1);  //Wrath of Harlune
//AnimationList.Add("Equin Lutak",100692224,19,1);  //Light Weapon Ineptitude Other VII
//AnimationList.Add("Equin Guaril",100668284,163,1);  //Fat Fingers
//AnimationList.Add("Equin Eareth",100668354,29,1);  //Eyes Clouded
//AnimationList.Add("Puish Zharil",100668351,76,1);  //Meditative Trance
//AnimationList.Add("Equin Opaj",100668330,23,1);  //Futility
//AnimationList.Add("Equin Easith",100668288,27,1);  //Inefficient Investment
//AnimationList.Add("Equin Hatak",100668266,19,1);  //Missile Weapon Ineptitude Other VII
//AnimationList.Add("Equin Guapaj",100668353,29,1);  //Ignorance's Bliss
//AnimationList.Add("Equin Guazael",100668355,29,1);  //Introversion
//AnimationList.Add("Equin Hati",100668265,21,1);  //Recklessness Ineptitude Other VII
//AnimationList.Add("Equin Hafeth",100692229,21,1);  //Shield Ineptitude Other VII
//AnimationList.Add("Equin Caja",100668294,13,1);  //Belly of Lead
//AnimationList.Add("Equin Luguz",100692230,21,1);  //Sneak Attack Ineptitude Other VII
//AnimationList.Add("Equin Eaves",100691575,27,1);  //Void Magic Ineptitude Other VII
//AnimationList.Add("Equin Ozael",100668331,23,1);  //Gravity Well
//AnimationList.Add("Equin Eati",100668272,27,1);  //Wrath of the Hieromancer
//AnimationList.Add("Equin Cazael",100668300,7,1);  //Senescence
//AnimationList.Add("Equin Luja",100668357,29,1);  //Eye of the Grunt

//Life
//AnimationList.Add("Cruath Qualoi",100668344,50,1);  //Olthoi's Gift
//AnimationList.Add("Cruath Quaguz",100668348,48,1);  //Swordsman's Gift
//AnimationList.Add("Cruath Quareth",100668345,56,1);  //Tusker's Gift
//AnimationList.Add("Cruath Quavik",100668292,164,1);  //Gelidite's Gift
//AnimationList.Add("Yanoi Zhavik",100668299,42,1);  //Enervation
//AnimationList.Add("Yanoi Zhapaj",100668279,38,1);  //Decrepitude's Grasp
//AnimationList.Add("Cruath Quatak",100668291,44,1);  //Inferno's Gift
//AnimationList.Add("Cruath Quasith",100668293,56,1);  //Gossamer Flesh
//AnimationList.Add("Cruath Quafeth",100668346,54,1);  //Astyrrian's Gift
//AnimationList.Add("Yanoi Zhaloi",100668288,163,1);  //Energy Flux
//AnimationList.Add("Cruath Quaril",100668347,46,1);  //Archer's Gift

//Item
//AnimationList.Add("Equin Qualoi",100673974,64,1);  //Acid Lure VI
//AnimationList.Add("Equin Quaguz",100673980,62,1);  //Blade Lure VI
//AnimationList.Add("Equin Quareth",100673975,68,1);  //Bludgeon Lure VI
//AnimationList.Add("Equin Quasith",100673981,143,1);  //Brittlemail VI
//AnimationList.Add("Equin Quatak",100673976,58,1);  //Flame Lure VI
//AnimationList.Add("Equin Quavik",100673977,66,1);  //Frost Lure VI
//AnimationList.Add("Malar Aevik",100673983,68,1);  //Hermetic Void VI
//AnimationList.Add("Equin Aetak",100673990,64,1);  //Leaden Weapon VI
//AnimationList.Add("Equin Quafeth",100673978,68,1);  //Lightning Lure VI
//AnimationList.Add("Equin Aeguz",100673985,62,1);  //Lure Blade VI
//AnimationList.Add("Equin Quaril",100673979,60,1);  //Piercing Lure VI
//AnimationList.Add("Equin Aeril",100676646,58,1);  //Spirit Loather VI
//AnimationList.Add("Equin Aereth",100673992,60,1);  //Turn Blade VI
//AnimationList.Add("Equin Aeti",100668401,60,1);  //Weaken Lock VI
//AnimationList.Add("Equin Quasith",100673981,143,1);  //Tattercoat

