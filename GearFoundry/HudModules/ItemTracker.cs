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
using VirindiHUDs;
using MyClasses.MetaViewWrappers.VirindiViewServiceHudControls;
using VirindiViewService.Themes;
using System.Xml.Serialization;
using System.Xml;



namespace GearFoundry
{
	public partial class PluginCore
	{		

		private List<ItemRule> ItemRulesList = new List<ItemRule>();
		private OpenContainer mOpenContainer = new OpenContainer();
		
		private List<int> ItemExclusionList = new List<int>();
		private List<int> ItemIDListenList = new List<int>();
		private List<int> ModifiedIOSpells = new List<int>();
		
		private List<int> ManaTankItems = new List<int>();
		private List<LootObject> ItemTrackingList = new List<LootObject>();
		private List<LootObject> SalvageItemsList = new  List<LootObject>();
		
		private int ItemHudMoveId = 0;
 		
		private GearInspectorSettings GISettings = new GearInspectorSettings();
			
		public class GearInspectorSettings
		{
			public bool IdentifySalvage;
			public bool AutoLoot;
			public bool AutoSalvage;
			public bool AutoAetheria;
			public bool AutoCombine;
			public bool AutoStack;
			public bool ModifiedLooting = true;
			public bool GearScore;
			public bool CheckForL7Scrolls;
			public bool SalvageHighValue = false;
            public int ItemHudWidth;
            public int ItemHudHeight;
			public int LootByValue = 0;
			public int LootByMana = 0;
			
    	}
		
		private void SubscribeItemEvents()
		{
			try
			{
				
             	Core.EchoFilter.ServerDispatch += ServerDispatchItem;
             	Core.ItemDestroyed += new EventHandler<ItemDestroyedEventArgs>(InspectorItemDestroyed);
             	Core.WorldFilter.ReleaseObject += new EventHandler<ReleaseObjectEventArgs>(InspectorItemReleased);
             	Core.CharacterFilter.ActionComplete += Inspector_ActionComplete;
             	
             	
             	SubscribeItemTrackerLooterEvents();
       			
             	
			}
			catch(Exception ex){LogError(ex);}
		}
		
		private void UnsubscribeItemEvents()
		{
			try
			{
				
             	Core.EchoFilter.ServerDispatch -= ServerDispatchItem;
             	Core.ItemDestroyed -= new EventHandler<ItemDestroyedEventArgs>(InspectorItemDestroyed);
             	Core.WorldFilter.ReleaseObject -= new EventHandler<ReleaseObjectEventArgs>(InspectorItemReleased);
             	Core.CharacterFilter.ActionComplete -= Inspector_ActionComplete;
                ItemHudView.Resize -= ItemHudView_Resize;
             	
             	
             	UnSubscribeItemTrackerLooterEvents();
             	
             	
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
		private HudStaticText InspectorHudValueLabel = null;
		private HudTextBox InspectorLootByValue = null;
		private HudStaticText InspectorHudManaLabel = null;
		private HudTextBox InspectorLootByMana = null;

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
                if (GISettings.ItemHudWidth == 0) { ItemHudWidth = ItemHudFirstWidth; }
                else { ItemHudWidth = GISettings.ItemHudWidth; }
                if (GISettings.ItemHudHeight == 0) { ItemHudHeight = ItemHudFirstHeight; }
                else { ItemHudHeight = GISettings.ItemHudHeight; }
                ItemHudView = new HudView("Inspector", ItemHudWidth, ItemHudHeight, new ACImage(0x6AA8));
    		//	ItemHudView.Theme = VirindiViewService.HudViewDrawStyle.GetThemeByName("Minimalist Transparent");
    			ItemHudView.UserAlphaChangeable = false;
    			ItemHudView.UserMinimizable = false;
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
                ItemHudView.Resize += ItemHudView_Resize; 
  				
    			RenderItemHudInspectorTab();
    			
				SubscribeItemEvents();
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
                    MasterTimer.Tick += ItemHudResizeTimerTick;
                    return;
                }
            }
            catch (Exception ex) { LogError(ex); }
            return;
        }

        private void ItemHudResizeTimerTick(object sender, EventArgs e)
        {
            ItemHudWidth = ItemHudWidthNew;
            ItemHudHeight = ItemHudHeightNew;
            GISettings.ItemHudWidth = ItemHudWidth;
            GISettings.ItemHudHeight = ItemHudHeight;
            GearInspectorReadWriteSettings(false);
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
    			
    			ItemHudUstList.Click += ItemHudUstList_Click;
    			ItemHudUstButton.Hit += ItemHudUstButton_Hit;
				
    			InspectorUstTab = true;
    			
    			UpdateItemHud();
    			
    			
    		}catch(Exception ex){LogError(ex);}
    	}
    	
    	private void ItemHudUstButton_Hit(object sender, EventArgs e)
    	{
    		try
    		{
    			foreach(LootObject item in SalvageItemsList)
    			{
    				SalvageObjectQueue.Enqueue(item);
    			}
    			SalvageItems();
    		}catch(Exception ex){LogError(ex);}
    	}
    	

    	
    	private void ItemHudUstList_Click(object sender, int row, int col)
    	{
    		try
    		{
    			//Salvage
    			if(col == 0)
    			{
    				Core.Actions.SalvagePanelAdd(SalvageItemsList[row].Id);
    				Core.Actions.SalvagePanelSalvage();
    			}
    			//Report
    			if(col == 1)
    			{
    				HudToChat(SalvageItemsList[row].GSReportString(), 1);
    			}
    			//Remove
    			if(col == 2)
    			{
    				SalvageItemsList.RemoveAt(row);
    			}
    			
    			UpdateItemHud();
    			
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
                ItemHudSettingsLayout.AddControl(InspectorIdentifySalvage, new Rectangle(0, 0, 200, 16));
    			InspectorIdentifySalvage.Checked = GISettings.IdentifySalvage;
    			
    			InspectorAutoLoot = new HudCheckBox();
    			InspectorAutoLoot.Text = "Automatically Loot Items";
    			ItemHudSettingsLayout.AddControl(InspectorAutoLoot, new Rectangle(0,18,200,16));
    			InspectorAutoLoot.Checked = GISettings.AutoLoot;
    			
    			InspectorAutoAetheria = new HudCheckBox();
    			InspectorAutoAetheria.Text = "Loot and Dessicate Junk Aetheria";
                ItemHudSettingsLayout.AddControl(InspectorAutoAetheria, new Rectangle(0, 36, 200, 16));
    			InspectorAutoAetheria.Checked = GISettings.AutoAetheria;
    			
    			InspectorAutoCombine = new HudCheckBox();
    			InspectorAutoCombine.Text = "AutoCombine Looted Salvage";
                ItemHudSettingsLayout.AddControl(InspectorAutoCombine, new Rectangle(0, 54, 200, 16));
    			InspectorAutoCombine.Checked = GISettings.AutoCombine;
    			
    			InspectorAutoStack = new HudCheckBox();
    			InspectorAutoStack.Text = "AutoStack Looted Items";
                ItemHudSettingsLayout.AddControl(InspectorAutoStack, new Rectangle(0, 72, 200, 16));
    			InspectorAutoStack.Checked = GISettings.AutoStack;
    			
    			InspectorModifiedLooting = new HudCheckBox();
    			InspectorModifiedLooting.Text = "Enabled Modified Looting";
                ItemHudSettingsLayout.AddControl(InspectorModifiedLooting, new Rectangle(0, 90, 200, 16));
    			InspectorModifiedLooting.Checked = GISettings.ModifiedLooting;
    			
    			InspectorGearScore = new HudCheckBox();
    			InspectorGearScore.Text = "Use GearScore Report Strings";
                ItemHudSettingsLayout.AddControl(InspectorGearScore, new Rectangle(0, 108, 200, 16));
    			InspectorGearScore.Checked = GISettings.GearScore;
    			
    			InspectorCheckForL7Scrolls = new HudCheckBox();
    			InspectorCheckForL7Scrolls.Text = "Loot Unknown L7 Spells";
                ItemHudSettingsLayout.AddControl(InspectorCheckForL7Scrolls, new Rectangle(0, 126, 200, 16));
    			InspectorCheckForL7Scrolls.Checked = GISettings.CheckForL7Scrolls;
    			
    			InspectorHudValueLabel = new HudStaticText();
    			InspectorHudValueLabel.Text = "High Value Loot.";
    			ItemHudSettingsLayout.AddControl(InspectorHudValueLabel, new Rectangle(50,142,200,16));
    			
    			InspectorLootByValue = new HudTextBox();
    			ItemHudSettingsLayout.AddControl(InspectorLootByValue, new Rectangle(0,142,45,16));
    			InspectorLootByValue.Text = GISettings.LootByValue.ToString();
    			
    			InspectorHudManaLabel = new HudStaticText();
    			InspectorHudManaLabel.Text = "Mana Value Loot.";
    			ItemHudSettingsLayout.AddControl(InspectorHudManaLabel, new Rectangle(50,158,200,16));		
    			
    			InspectorLootByMana = new HudTextBox();
    			ItemHudSettingsLayout.AddControl(InspectorLootByMana, new Rectangle(0,158,45,16));
    			InspectorLootByMana.Text = GISettings.LootByMana.ToString();
    			
    			InspectorIdentifySalvage.Change += InspectorIdentifySalvage_Change;
    			InspectorAutoLoot.Change += InspectorAutoLoot_Change;
    			InspectorAutoAetheria.Change += InspectorAutoAetheria_Change;
    			InspectorAutoCombine.Change += InspectorAutoCombine_Change;
    			InspectorAutoStack.Change += InspectorAutoStack_Change;
    			InspectorModifiedLooting.Change += InspectorModifiedLooting_Change;
    			InspectorGearScore.Change += InspectorGearScore_Change;
    			InspectorCheckForL7Scrolls.Change += InspectorCheckForL7Scrolls_Change;
    			InspectorLootByValue.LostFocus += InspectorLootByValue_LostFocus;
    			InspectorLootByMana.LostFocus += InspectorLootByMana_LostFocus;	
    			  			
    			InspectorSettingsTab = true;
    			
   
    		}catch(Exception ex){LogError(ex);}
    	}
    	
    	private void InspectorLootByValue_LostFocus(object sender, System.EventArgs e)
    	{
    		try
    		{
    			Int32.TryParse(InspectorLootByValue.Text, out GISettings.LootByValue);
    			GearInspectorReadWriteSettings(false);
    			
    		}catch(Exception ex){LogError(ex);}
    	}
    	
    	private void InspectorLootByMana_LostFocus(object sender, System.EventArgs e)
    	{
    		try
    		{
    			Int32.TryParse(InspectorLootByMana.Text, out GISettings.LootByMana);
    			GearInspectorReadWriteSettings(false);
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
    			InspectorLootByValue.LostFocus -= InspectorLootByValue_LostFocus;
    			InspectorLootByMana.LostFocus -= InspectorLootByMana_LostFocus;
    			
    			
    			InspectorIdentifySalvage.Dispose();
    			InspectorAutoLoot.Dispose();
    			InspectorAutoAetheria.Dispose();
    			InspectorAutoCombine.Dispose();
    			InspectorAutoStack.Dispose();
    			InspectorModifiedLooting.Dispose();
    			InspectorGearScore.Dispose();
    			InspectorCheckForL7Scrolls.Dispose();
    			InspectorLootByMana.Dispose();
    			InspectorLootByValue.Dispose();
    			InspectorHudManaLabel.Dispose();
    			InspectorHudValueLabel.Dispose();
    			  			
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
    			UnsubscribeItemEvents();
    			
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
    				if(GISettings.ModifiedLooting) {HudToChat(ItemTrackingList[row].GSReportString(), 1);}
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
		    		   	    		
		    	    foreach(LootObject item in ItemTrackingList)
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
	    			foreach(LootObject ustitem in SalvageItemsList)
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
