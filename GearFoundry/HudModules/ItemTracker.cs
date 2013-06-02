﻿using System;
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
using VirindiHUDs;
using MyClasses.MetaViewWrappers.VirindiViewServiceHudControls;
using VirindiViewService.Themes;
using System.Xml.Serialization;
using System.Xml;
using WindowsTimer = System.Windows.Forms.Timer;



namespace GearFoundry
{
	public partial class PluginCore
	{		

		private List<ItemRule> ItemRulesList = new List<ItemRule>();
        private WindowsTimer itemHudResizeTimer;
		private OpenContainer mOpenContainer = new OpenContainer();
		
		private List<int> ItemExclusionList = new List<int>();
		private List<int> ItemIDListenList = new List<int>();
		private List<int> ModifiedIOSpells = new List<int>();
		
		private List<int> ManaTankItems = new List<int>();
		private List<IdentifiedObject> ItemTrackingList = new List<IdentifiedObject>();
		private List<IdentifiedObject> SalvageItemsList = new  List<IdentifiedObject>();
		
		private int ItemHudMoveId = 0;
		private DateTime IHRenderTime150;
		private DateTime AutoLootDelayStart;
		
		private GearInspectorSettings GISettings = new GearInspectorSettings();
			
		public class GearInspectorSettings
		{
			public bool IdentifySalvage;
			public bool AutoLoot;
			public bool AutoSalvage;
			public bool AutoAetheria;
			public bool AutoCombine;
			public bool AutoStack;
			public bool ModifiedLooting;
			public bool GearScore;
			public bool CheckForL7Scrolls;
			public bool SalvageHighValue;
            public int ItemHudWidth;
            public int ItemHudHeight;
			
    	}
		
		private void SubscribeLootEvents()
		{
			try
			{
				Core.ContainerOpened += LootContainerOpened;
             	Core.EchoFilter.ServerDispatch += ServerDispatchItem;
             	Core.ItemDestroyed += new EventHandler<ItemDestroyedEventArgs>(InspectorItemDestroyed);
             	Core.WorldFilter.ReleaseObject += new EventHandler<ReleaseObjectEventArgs>(InspectorItemReleased);
             	Core.CharacterFilter.ActionComplete += Inspector_ActionComplete;
             	Core.WorldFilter.ChangeObject += new EventHandler<ChangeObjectEventArgs>(ItemHud_ChangeObject);
       			
             	
			}
			catch(Exception ex){LogError(ex);}
		}
		
		private void UnsubscribeLootEvents()
		{
			try
			{
				Core.ContainerOpened -= LootContainerOpened;
             	Core.EchoFilter.ServerDispatch -= ServerDispatchItem;
             	Core.ItemDestroyed -= new EventHandler<ItemDestroyedEventArgs>(InspectorItemDestroyed);
             	Core.WorldFilter.ReleaseObject -= new EventHandler<ReleaseObjectEventArgs>(InspectorItemReleased);
             	Core.CharacterFilter.ActionComplete -= Inspector_ActionComplete;
             	Core.WorldFilter.ChangeObject -= new EventHandler<ChangeObjectEventArgs>(ItemHud_ChangeObject);
             	
             	
             	
			}catch(Exception ex){LogError(ex);}
		}
		
		private void InspectorItemDestroyed(object sender, ItemDestroyedEventArgs e)
		{
			try
			{
				if(e == null){return;}
				if(mOpenContainer.ContainerIOs.Any(x => x.Id == e.ItemGuid)){mOpenContainer.ContainerIOs.RemoveAll(x => x.Id == e.ItemGuid);}
				if(ItemExclusionList.Any(x => x == e.ItemGuid)){ItemExclusionList.RemoveAll(x => x == e.ItemGuid);}
				if(ItemTrackingList.Any(x => x.Id == e.ItemGuid)){ItemTrackingList.RemoveAll(x => x.Id == e.ItemGuid);}
				if(ItemIDListenList.Any(x => x == e.ItemGuid)){ItemIDListenList.RemoveAll(x => x == e.ItemGuid);}
				if(ManaTankItems.Any(x => x == e.ItemGuid)){ManaTankItems.RemoveAll(x => x == e.ItemGuid);}
				if(SalvageItemsList.Any(x => x.Id == e.ItemGuid)){SalvageItemsList.RemoveAll(x => x.Id == e.ItemGuid);}
				
			}catch(Exception ex){LogError(ex);}
		}
		
		private void InspectorItemReleased(object sender, ReleaseObjectEventArgs e)
		{
			try
			{
				if(e == null) {return;}
				
				if(mOpenContainer.ContainerIOs.Any(x => x.Id == e.Released.Id)){mOpenContainer.ContainerIOs.RemoveAll(x => x.Id == e.Released.Id);}
				if(ItemExclusionList.Any(x => x == e.Released.Id)){ItemExclusionList.RemoveAll(x => x == e.Released.Id);}
				if(ItemTrackingList.Any(x => x.Id == e.Released.Id)){ItemTrackingList.RemoveAll(x => x.Id == e.Released.Id);}
				if(ItemIDListenList.Any(x => x == e.Released.Id)){ItemIDListenList.RemoveAll(x => x == e.Released.Id);}
				if(ManaTankItems.Any(x => x == e.Released.Id)){ManaTankItems.RemoveAll(x => x == e.Released.Id);}
				if(SalvageItemsList.Any(x => x.Id == e.Released.Id)){SalvageItemsList.RemoveAll(x => x.Id == e.Released.Id);}
				
			}catch(Exception ex){LogError(ex);}
		}
		

		
		private void ServerDispatchItem(object sender, Decal.Adapter.NetworkMessageEventArgs e)
        {
        	int iEvent = 0;
            try
            {                   	
            	if(e.Message.Type == AC_GAME_EVENT)
            	{	
            		try
                    {
                    	iEvent = Convert.ToInt32(e.Message["event"]);
                    }
                    catch{}
                    if(iEvent == GE_IDENTIFY_OBJECT)
                    {
                    	 OnIdentItem(e.Message);
                    }
                    
            	}
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
        }  
		
		
		private void OnIdentItem(Decal.Adapter.Message pMsg)
		{
			try
			{
				if(!bItemHudEnabled) {return;}
    	   		int PossibleItemID = Convert.ToInt32(pMsg["object"]);		
        		//Read and report manual IDs, but keep them out of the item list
        		if(PossibleItemID == Host.Actions.CurrentSelection && bReportItemStrings)
        		{
        			ManualCheckItemForMatches(new IdentifiedObject(Core.WorldFilter[PossibleItemID]));
        		}  		
        		if(!ItemIDListenList.Contains(PossibleItemID)) {return;}
        		CheckItemForMatches(new IdentifiedObject(Core.WorldFilter[PossibleItemID]));
			} 
			catch (Exception ex) {LogError(ex);}
		}
		
		private void ManualCheckItemForMatches(IdentifiedObject IOItem)
		{
			try
			{
				if(IOItem.HasIdData){CheckRulesItem(ref IOItem);}
				if(IOItem.ObjectClass == ObjectClass.Scroll){CheckUnknownScrolls(ref IOItem);}
				if(IOItem.IOR == IOResult.unknown) {TrophyListCheckItem(ref IOItem);}
				if(IOItem.IOR == IOResult.unknown) {CheckSalvageItem(ref IOItem);}
				if(IOItem.IOR == IOResult.unknown) {CheckManaItem(ref IOItem);}
				if(IOItem.IOR == IOResult.unknown) {CheckValueItem(ref IOItem);}
				if(IOItem.IOR == IOResult.unknown) {IOItem.IOR = IOResult.nomatch;}
				
				if(GISettings.ModifiedLooting) {ReportStringToChat(IOItem.ModString());}
				else {ReportStringToChat(IOItem.LinkString());}
				
			}catch(Exception ex){LogError(ex);}
		}
		

		
		private void SeparateItemsToID(IdentifiedObject IOItem)
		{
			try
			{
				//Get rid of non-existant items...
				if(!IOItem.isvalid)
				{
					IOItem.IOR = IOResult.nomatch;
					mOpenContainer.ContainerIOs.RemoveAll(x => !x.isvalid);
					return;
				}
				
				//Flag items that need additional info to ID...
				if(!IOItem.HasIdData)
				{
					switch(IOItem.ObjectClass)
					{
						case ObjectClass.Armor:
							IdqueueAdd(IOItem.Id);
							mOpenContainer.ContainerIOs[mOpenContainer.ContainerIOs.FindIndex(x => x.Id == IOItem.Id)].IOR = IOResult.unknown;
							ItemIDListenList.Add(IOItem.Id);
							return;
							
						case ObjectClass.Clothing:
							IdqueueAdd(IOItem.Id);
							mOpenContainer.ContainerIOs[mOpenContainer.ContainerIOs.FindIndex(x => x.Id == IOItem.Id)].IOR = IOResult.unknown;
							ItemIDListenList.Add(IOItem.Id);
							return;
							
						case ObjectClass.Gem:
							if(IOItem.Aetheriacheck)
							{
								IdqueueAdd(IOItem.Id);
								mOpenContainer.ContainerIOs[mOpenContainer.ContainerIOs.FindIndex(x => x.Id == IOItem.Id)].IOR = IOResult.unknown;
								ItemIDListenList.Add(IOItem.Id);
								return;
							}
							else goto default;							
						case ObjectClass.Jewelry:
							IdqueueAdd(IOItem.Id);
							mOpenContainer.ContainerIOs[mOpenContainer.ContainerIOs.FindIndex(x => x.Id == IOItem.Id)].IOR = IOResult.unknown;
							ItemIDListenList.Add(IOItem.Id);
							return;		
						case ObjectClass.MeleeWeapon:
							IdqueueAdd(IOItem.Id);
							mOpenContainer.ContainerIOs[mOpenContainer.ContainerIOs.FindIndex(x => x.Id == IOItem.Id)].IOR = IOResult.unknown;
							ItemIDListenList.Add(IOItem.Id);
							return;
							
						case ObjectClass.MissileWeapon:	//bow = mastery 8, crossbow = mastery 9, atlan = mastery 10, don't ID rocks....
							if (IOItem.WeaponMasteryCategory == 8 | IOItem.WeaponMasteryCategory == 9 | IOItem.WeaponMasteryCategory == 10) 
							{
								IdqueueAdd(IOItem.Id);
								mOpenContainer.ContainerIOs[mOpenContainer.ContainerIOs.FindIndex(x => x.Id == IOItem.Id)].IOR = IOResult.unknown;
								ItemIDListenList.Add(IOItem.Id);
								return;
							}
							else goto default;						
						case ObjectClass.Misc:
							if(IOItem.Name.Contains("Essence"))
							{
								IdqueueAdd(IOItem.Id);
								mOpenContainer.ContainerIOs[mOpenContainer.ContainerIOs.FindIndex(x => x.Id == IOItem.Id)].IOR = IOResult.unknown;
								ItemIDListenList.Add(IOItem.Id);
								return;
							}
							else goto default;
							
						case ObjectClass.WandStaffOrb:
							IdqueueAdd(IOItem.Id);
							mOpenContainer.ContainerIOs[mOpenContainer.ContainerIOs.FindIndex(x => x.Id == IOItem.Id)].IOR = IOResult.unknown;
							ItemIDListenList.Add(IOItem.Id);
							return;	
						default:
							CheckItemForMatches(IOItem);
							return;				
					}		
				}
			} catch (Exception ex) {LogError(ex);} 
			return;
		}
		
		
		
		private void CheckItemForMatches(IdentifiedObject IOItem)
		{
			try
			{
				if(IOItem.HasIdData){CheckRulesItem(ref IOItem);}
				if(IOItem.ObjectClass == ObjectClass.Scroll){CheckUnknownScrolls(ref IOItem);}
				if(IOItem.IOR == IOResult.unknown) {TrophyListCheckItem(ref IOItem);}
				if(IOItem.IOR == IOResult.unknown) {CheckSalvageItem(ref IOItem);}
				if(IOItem.IOR == IOResult.unknown) {CheckManaItem(ref IOItem);}
				if(IOItem.IOR == IOResult.unknown) {CheckValueItem(ref IOItem);}
				if(IOItem.IOR == IOResult.unknown) {IOItem.IOR = IOResult.nomatch;}
				
				if(IOItem.IOR != IOResult.nomatch)
				{
					if(GISettings.ModifiedLooting) {ReportStringToChat(IOItem.ModString());}
					else {ReportStringToChat(IOItem.LinkString());}
				}
				
				if(IOItem.IOR != IOResult.unknown) {EvaluateItemMatches(IOItem);}
				
			}catch(Exception ex){LogError(ex);}
		}	
		
		private void EvaluateItemMatches(IdentifiedObject IOItem)
		{
			try
			{
				//Keep those duplicates out
				if(ItemTrackingList.Any(x => x.Id == IOItem.Id) || ItemExclusionList.Contains(IOItem.Id)) {return;}
				
				switch(IOItem.IOR)
				{
					case IOResult.rule:
						ItemTrackingList.Add(IOItem);
						ItemExclusionList.Add(IOItem.Id);
						UpdateItemHud();
						//PlaySound?
						return;
					case IOResult.manatank:
						ItemTrackingList.Add(IOItem);
						ManaTankItems.Add(IOItem.Id);
						ItemExclusionList.Add(IOItem.Id);
						UpdateItemHud();
						//PlaySound?
						return;
					case IOResult.rare:
						ItemTrackingList.Add(IOItem);
						ItemExclusionList.Add(IOItem.Id);
						UpdateItemHud();
						//PlaySound?
						return;
					case IOResult.salvage:
						ItemTrackingList.Add(IOItem);
						SalvageItemsList.Add(IOItem);
						ItemExclusionList.Add(IOItem.Id);
						UpdateItemHud();
						//PlaySound?
						return;
					case IOResult.spell:
						ItemTrackingList.Add(IOItem);
						ItemExclusionList.Add(IOItem.Id);
						UpdateItemHud();
						//PlaySound?
						return;
					case IOResult.trophy:
						ItemTrackingList.Add(IOItem);
						ItemExclusionList.Add(IOItem.Id);
						UpdateItemHud();
						//PlaySound?
						return;
					case IOResult.val:
						ItemTrackingList.Add(IOItem);
						ItemExclusionList.Add(IOItem.Id);
						if(GISettings.SalvageHighValue) {SalvageItemsList.Add(IOItem);}
						UpdateItemHud();
						//PlaySound?
						return;
					default:
						ItemExclusionList.Add(IOItem.Id);
						mOpenContainer.ContainerIOs.RemoveAll(x => x.Id == IOItem.Id);
						return;
				}
			}catch(Exception ex){LogError(ex);}
		}

			
		private bool InspectorTab = false;
		private bool InspectorUstTab = false;
		private bool InspectorSettingsTab = false;
		
		private HudView ItemHudView = null;
		private HudFixedLayout ItemHudLayout = null;
		private HudTabView ItemHudTabView = null;
		private HudFixedLayout ItemHudInspectorLayout = null;
		private HudFixedLayout ItemHudUstLayout = null;
		private HudFixedLayout ItemHudSettingsLayout = null;
		
		
		
		private HudList ItemHudInspectorList = null;
		private HudList.HudListRowAccessor ItemHudListRow = null;
		
		
		private HudList ItemHudUstList = null;
		private HudButton ItemHudUstButton = null;
		
		private const int ItemRemoveCircle = 0x60011F8;
		private const int ItemUstIcon = 0x60026BA;
		
		private HudCheckBox InspectorIdentifySalvage = null;
		private HudCheckBox InspectorAutoLoot = null;
		private HudCheckBox InspectorAutoAetheria = null;
		private HudCheckBox InspectorAutoCombine = null;
		private HudCheckBox InspectorAutoStack = null;
		private HudCheckBox InspectorModifiedLooting = null;
		private HudCheckBox InspectorGearScore = null;
		private HudCheckBox InspectorCheckForL7Scrolls = null;

        private int ItemHudWidth;
        private int ItemHudHeight;
        private int ItemHudFirstWidth = 300;
        private int ItemHudFirstHeight = 220;
        private int ItemHudWidthNew;
        private int ItemHudHeightNew;
	
    	private void RenderItemHud()
    	{
    		try
    		{
    			GearInspectorReadWriteSettings(true);
    	
    		}catch{}
    		
    		
    		try{
    			    			
    			if(ItemHudView != null)
    			{
    				DisposeItemHud();
    			}

                if (ItemHudWidth == 0) { ItemHudWidth = ItemHudFirstWidth; }
                if (ItemHudHeight == 0) { ItemHudHeight = ItemHudFirstHeight; }



                ItemHudView = new HudView("Inspector", ItemHudWidth, ItemHudHeight, new ACImage(0x6AA8));
    			ItemHudView.Theme = VirindiViewService.HudViewDrawStyle.GetThemeByName("Minimalist Transparent");
    			ItemHudView.UserAlphaChangeable = false;
    			ItemHudView.ShowInBar = false;
    			ItemHudView.Visible = true;
    			ItemHudView.UserResizeable = false;
    			ItemHudView.LoadUserSettings();
    			
    			ItemHudLayout = new HudFixedLayout();
    			ItemHudView.Controls.HeadControl = ItemHudLayout;
    			
    			ItemHudTabView = new HudTabView();
    			ItemHudLayout.AddControl(ItemHudTabView, new Rectangle(0,0,ItemHudWidth,ItemHudHeight));
    		
    			ItemHudInspectorLayout = new HudFixedLayout();
    			ItemHudTabView.AddTab(ItemHudInspectorLayout, "Inspector");
    			
    			ItemHudUstLayout = new HudFixedLayout();
    			ItemHudTabView.AddTab(ItemHudUstLayout, "Ust");
    			
    			ItemHudSettingsLayout = new HudFixedLayout();
    			ItemHudTabView.AddTab(ItemHudSettingsLayout, "Settings");
    			
    			ItemHudTabView.OpenTabChange += ItemHudTabView_OpenTabChange;
  				
    			RenderItemHudInspectorTab();
    			
				SubscribeLootEvents();
                ItemHudView.UserResizeable = true;

			  							
    		}catch(Exception ex) {LogError(ex);}
    		
    	}

        private void ItemHudView_Resize(object sender, System.EventArgs e)
        {
            try
            {
                bool bw = Math.Abs(ItemHudView.Width - ItemHudWidth) > 20;
                bool bh = Math.Abs(ItemHudView.Height - ItemHudHeight) > 20;
                if (bh || bw)
                {
                    ItemHudWidthNew = ItemHudView.Width;
                    ItemHudHeightNew = ItemHudView.Height;
                    itemHudResizeTimer = new WindowsTimer();
                    itemHudResizeTimer.Interval = 1000;
                    itemHudResizeTimer.Enabled = true;
                    itemHudResizeTimer.Start();
                    itemHudResizeTimer.Tick += ItemHudResizeTimerTick;
                }
            }
            catch (Exception ex) { LogError(ex); }
            return;
        }

        private void ItemHudResizeTimerTick(object sender, EventArgs e)
        {
            ItemHudWidth = ItemHudWidthNew;
            ItemHudHeight = ItemHudHeightNew;
             MasterTimer.Tick -= ItemHudResizeTimerTick;
            RenderItemHud();
        }

    	
    	private void ItemHudTabView_OpenTabChange(object sender, System.EventArgs e)
    	{
    		try
    		{
    			switch(ItemHudTabView.CurrentTab)
    			{
    				case 0:
    					DisposeItemHudUstTab();
    					DisposeItemHudSettingsTab();
    					RenderItemHudInspectorTab();
    					return;
    				case 1:
    					DisposeItemHudInspectorTab();
    					DisposeItemHudSettingsTab();
    					RenderItemHudUstTab();
    					return;
    				case 2:
    					DisposeItemHudInspectorTab();
    					DisposeItemHudUstTab();
    					RenderItemHudSettingsTab();
    					return;
    			}
    		}catch(Exception ex){LogError(ex);}
    	}
    	
    	private void RenderItemHudUstTab()
    	{
    		try
    		{
    			ItemHudUstButton = new HudButton();
    			ItemHudUstButton.Text = "Salvage List";
    			ItemHudUstLayout.AddControl(ItemHudUstButton, new Rectangle(75,0,150,20));
    			
    			ItemHudUstList = new HudList();
    			ItemHudUstList.AddColumn(typeof(HudPictureBox), 16, null);
    			ItemHudUstList.AddColumn(typeof(HudStaticText), 200, null);
    			ItemHudUstList.AddColumn(typeof(HudPictureBox), 16, null);
    			ItemHudUstLayout.AddControl(ItemHudUstList, new Rectangle(0,30,ItemHudWidth,ItemHudHeight));
		
    			
    			InspectorUstTab = true;
    		}catch(Exception ex){LogError(ex);}
    	}
    	
    	private void DisposeItemHudUstTab()
    	{
    		try
    		{
    			if(!InspectorUstTab){return;}
    			
    			ItemHudUstList.Dispose();
    			ItemHudUstButton.Dispose();
    			
    			InspectorUstTab = false;
    			
    		}catch(Exception ex){LogError(ex);}
    	}
    	
    	private void RenderItemHudSettingsTab()
    	{
    		try
    		{

    			InspectorIdentifySalvage = new HudCheckBox();
    			InspectorIdentifySalvage.Text = "Identify Salvage";
    			ItemHudSettingsLayout.AddControl(InspectorIdentifySalvage, new Rectangle(0,0,150,16));
    			InspectorIdentifySalvage.Checked = GISettings.IdentifySalvage;
    			
    			InspectorAutoLoot = new HudCheckBox();
    			InspectorAutoLoot.Text = "Automatically Loot Items";
    			ItemHudSettingsLayout.AddControl(InspectorAutoLoot, new Rectangle(0,18,150,16));
    			InspectorAutoLoot.Checked = GISettings.AutoLoot;
    			
    			InspectorAutoAetheria = new HudCheckBox();
    			InspectorAutoAetheria.Text = "Loot and Dessicate Junk Aetheria";
    			ItemHudSettingsLayout.AddControl(InspectorAutoAetheria, new Rectangle(0,36,150,16));
    			InspectorAutoAetheria.Checked = GISettings.AutoAetheria;
    			
    			InspectorAutoCombine = new HudCheckBox();
    			InspectorAutoCombine.Text = "AutoCombine Looted Salvage";
    			ItemHudSettingsLayout.AddControl(InspectorAutoCombine, new Rectangle(0,54,150,16));
    			InspectorAutoCombine.Checked = GISettings.AutoCombine;
    			
    			InspectorAutoStack = new HudCheckBox();
    			InspectorAutoStack.Text = "AutoStack Looted Items";
    			ItemHudSettingsLayout.AddControl(InspectorAutoStack, new Rectangle(0,72,150,16));
    			InspectorAutoStack.Checked = GISettings.AutoStack;
    			
    			InspectorModifiedLooting = new HudCheckBox();
    			InspectorModifiedLooting.Text = "Enabled Modified Looting";
    			ItemHudSettingsLayout.AddControl(InspectorModifiedLooting, new Rectangle(0,90,150,16));
    			InspectorModifiedLooting.Checked = GISettings.ModifiedLooting;
    			
    			InspectorGearScore = new HudCheckBox();
    			InspectorGearScore.Text = "Use GearScore Report Strings";
    			ItemHudSettingsLayout.AddControl(InspectorGearScore, new Rectangle(0,108,150,16));
    			InspectorGearScore.Checked = GISettings.GearScore;
    			
    			InspectorCheckForL7Scrolls = new HudCheckBox();
    			InspectorCheckForL7Scrolls.Text = "Loot Unknown L7 Spells";
    			ItemHudSettingsLayout.AddControl(InspectorCheckForL7Scrolls, new Rectangle(0,126,150,16));
    			InspectorCheckForL7Scrolls.Checked = GISettings.CheckForL7Scrolls;
    			
    			InspectorIdentifySalvage.Change += InspectorIdentifySalvage_Change;
    			InspectorAutoLoot.Change += InspectorAutoLoot_Change;
    			InspectorAutoAetheria.Change += InspectorAutoAetheria_Change;
    			InspectorAutoCombine.Change += InspectorAutoCombine_Change;
    			InspectorAutoStack.Change += InspectorAutoStack_Change;
    			InspectorModifiedLooting.Change += InspectorModifiedLooting_Change;
    			InspectorGearScore.Change += InspectorGearScore_Change;
    			InspectorCheckForL7Scrolls.Change += InspectorCheckForL7Scrolls_Change;
    			
    			
    			  			
    			InspectorSettingsTab = true;
    			
   
    		}catch(Exception ex){LogError(ex);}
    	}
    	
    	private void InspectorIdentifySalvage_Change(object sender, System.EventArgs e)
    	{
    		try
    		{
    			GISettings.IdentifySalvage = InspectorIdentifySalvage.Checked;
    			GearInspectorReadWriteSettings(false);
    		}catch(Exception ex){LogError(ex);}
    	}
    	

    	private void InspectorAutoLoot_Change(object sender, System.EventArgs e)
    	{
    		try
    		{
    			GISettings.AutoLoot = InspectorAutoLoot.Checked;
    			GearInspectorReadWriteSettings(false);
    		}catch(Exception ex){LogError(ex);}
    	}
    	
    	private void InspectorAutoAetheria_Change(object sender, System.EventArgs e)
    	{
    		try
    		{
    			GISettings.AutoAetheria = InspectorAutoAetheria.Checked;
    			GearInspectorReadWriteSettings(false);
    		}catch(Exception ex){LogError(ex);}
    	}
    	
    	private void InspectorAutoCombine_Change(object sender, System.EventArgs e)
    	{
    		try
    		{
    			GISettings.AutoCombine = InspectorAutoCombine.Checked;
				GearInspectorReadWriteSettings(false);    			
    		}catch(Exception ex){LogError(ex);}
    	}
    	
    	private void InspectorAutoStack_Change(object sender, System.EventArgs e)
    	{
    		try
    		{
    			GISettings.AutoStack = InspectorAutoStack.Checked;
    			GearInspectorReadWriteSettings(false);
    		}catch(Exception ex){LogError(ex);}
    	}
    	
    	private void InspectorModifiedLooting_Change(object sender, System.EventArgs e)
    	{
    		try
    		{
    			GISettings.ModifiedLooting = InspectorModifiedLooting.Checked;
    			GearInspectorReadWriteSettings(false);
    		}catch(Exception ex){LogError(ex);}
    	}
    	
    	private void InspectorGearScore_Change(object sender, System.EventArgs e)
    	{
    		try
    		{
    			GISettings.GearScore = InspectorGearScore.Checked;
    			GearInspectorReadWriteSettings(false);
    		}catch(Exception ex){LogError(ex);}
    	}
    	
    	private void InspectorCheckForL7Scrolls_Change(object sender, System.EventArgs e)
    	{
    		try
    		{
    			GISettings.CheckForL7Scrolls = InspectorCheckForL7Scrolls.Checked;
    			GearInspectorReadWriteSettings(false);
    		}catch(Exception ex){LogError(ex);}
    	}
    		
    	private void DisposeItemHudSettingsTab()
    	{
    		try
    		{
    			if(!InspectorSettingsTab){return;}
    			
    			InspectorIdentifySalvage.Change -= InspectorIdentifySalvage_Change;
    			InspectorAutoLoot.Change -= InspectorAutoLoot_Change;
    			InspectorAutoAetheria.Change -= InspectorAutoAetheria_Change;
    			InspectorAutoCombine.Change -= InspectorAutoCombine_Change;
    			InspectorAutoStack.Change -= InspectorAutoStack_Change;
    			InspectorModifiedLooting.Change -= InspectorModifiedLooting_Change;
    			InspectorGearScore.Change -= InspectorGearScore_Change;
    			InspectorCheckForL7Scrolls.Change -= InspectorCheckForL7Scrolls_Change;
    			
    			
    			InspectorIdentifySalvage.Dispose();
    			InspectorAutoLoot.Dispose();
    			InspectorAutoAetheria.Dispose();
    			InspectorAutoCombine.Dispose();
    			InspectorAutoStack.Dispose();
    			InspectorModifiedLooting.Dispose();
    			InspectorGearScore.Dispose();
    			InspectorCheckForL7Scrolls.Dispose();
    			  			
    			InspectorSettingsTab = false;
    			
    		}catch(Exception ex){LogError(ex);}
    	}
    	
    	
    	
    	private void RenderItemHudInspectorTab()
    	{
    		try
    		{
    			ItemHudInspectorList = new HudList();
    			ItemHudInspectorLayout.AddControl(ItemHudInspectorList, new Rectangle(0,0,300,220));
				ItemHudInspectorList.ControlHeight = 16;	
				ItemHudInspectorList.AddColumn(typeof(HudPictureBox), 16, null);
				ItemHudInspectorList.AddColumn(typeof(HudStaticText), 230, null);
				ItemHudInspectorList.AddColumn(typeof(HudPictureBox), 16, null);
				
				ItemHudInspectorList.Click += (sender, row, col) => ItemHudInspectorList_Click(sender, row, col);	

				InspectorTab = true;				

    		}catch(Exception ex){LogError(ex);}
    	}
    	
    	private void DisposeItemHudInspectorTab()
    	{
    		try
    		{
    			if(!InspectorTab){return;}
    			
    			ItemHudInspectorList.Click -= (sender, row, col) => ItemHudInspectorList_Click(sender, row, col);	 			
    			ItemHudInspectorList.Dispose(); 
    			
    			InspectorTab = false;
    			
    		}catch(Exception ex){LogError(ex);}
    	}
    		
    	private void DisposeItemHud()
    	{    			
    		try
    		{
    			UnsubscribeLootEvents();
    			
    			ItemHudSettingsLayout.Dispose();
    			ItemHudUstLayout.Dispose();
    			ItemHudInspectorLayout.Dispose();   			
    			ItemHudTabView.Dispose();
    			ItemHudLayout.Dispose();
    			ItemHudView.Dispose();
    		}	
    		catch{}
    	}
    		
    	private void ItemHudInspectorList_Click(object sender, int row, int col)
    	{
    		try
			{
    			if(col == 0)
    			{
    				Core.Actions.MoveItem(ItemTrackingList[row].Id,Core.CharacterFilter.Id,0,true);  
    				ItemHudMoveId = ItemTrackingList[row].Id;
    				ItemTrackingList.RemoveAll(x => x.Id == ItemTrackingList[row].Id);
    				
    				
    			}
    			if(col == 1)
    			{
    				if(GISettings.ModifiedLooting) {HudToChat(ItemTrackingList[row].ModString(), 1);}
    				else{HudToChat(ItemTrackingList[row].LinkString(), 1);}
    			}
    			if(col == 2)
    			{    				
    				ItemExclusionList.Add(ItemTrackingList[row].Id);
    				ItemTrackingList.RemoveAt(row);
    			}
				UpdateItemHud();
			}
			catch (Exception ex) { LogError(ex); }	
    	}
    		
	    private void UpdateItemHud()
	    {  	
	    	try
	    	{    

	    		
	    		if(InspectorTab)
	    		{
		    		ItemHudInspectorList.ClearRows();
		    		   	    		
		    	    foreach(IdentifiedObject item in ItemTrackingList)
		    	    {
		    	    	ItemHudListRow = ItemHudInspectorList.AddRow();
		    	    	
		    	    	((HudPictureBox)ItemHudListRow[0]).Image = item.Icon + 0x6000000;
		    	    	((HudStaticText)ItemHudListRow[1]).Text = item.IORString() + item.Name;
		    	    	if(item.IOR == IOResult.trophy) {((HudStaticText)ItemHudListRow[1]).TextColor = Color.Wheat;}
		    	    	if(item.IOR == IOResult.salvage) {((HudStaticText)ItemHudListRow[1]).TextColor = Color.PaleVioletRed;}
		    	    	if(item.IOR == IOResult.val) {((HudStaticText)ItemHudListRow[1]).TextColor = Color.PaleGoldenrod;}
		    	    	if(item.IOR == IOResult.spell) {((HudStaticText)ItemHudListRow[1]).TextColor = Color.Lavender;}
		    	    	if(item.IOR == IOResult.rule)  {((HudStaticText)ItemHudListRow[1]).TextColor = Color.LightGreen;}
		    	    	if(item.IOR == IOResult.rare)  {((HudStaticText)ItemHudListRow[1]).TextColor = Color.HotPink;}
		    	    	if(item.IOR == IOResult.manatank)  {((HudStaticText)ItemHudListRow[1]).TextColor = Color.CornflowerBlue;}
						((HudPictureBox)ItemHudListRow[2]).Image = ItemRemoveCircle;
		    	    }
	    		}
	    		if(InspectorUstTab)
	    		{
	    			foreach(IdentifiedObject ustitem in SalvageItemsList)
	    			{
	    				ItemHudListRow = ItemHudUstList.AddRow();
	    				((HudPictureBox)ItemHudListRow[0]).Image = ItemUstIcon;
	    				((HudStaticText)ItemHudListRow[1]).Text = ustitem.IORString() + ustitem.Name;
	    				((HudPictureBox)ItemHudListRow[2]).Image = ItemRemoveCircle;
	    				
	    			}
	    		}
	    		
	    	}catch(Exception ex){LogError(ex);}
	    	
	    }
		
	
	}
}
