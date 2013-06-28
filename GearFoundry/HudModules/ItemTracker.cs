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
		
		private Queue<LootObject> ManaTankQueue = new Queue<LootObject>();
		private List<LootObject> ItemTrackingList = new List<LootObject>();
		
		private Queue<LootObject> ItemHudMoveQueue = new Queue<LootObject>();
		private List<LootObject> ProcessItemsList = new List<LootObject>();
		private Queue<LootObject> KeyItemsQueue = new Queue<LootObject>();
		private Queue<LootObject> SalvageItemsQueue = new Queue<LootObject>();
		private Queue<LootObject> DesiccateItemsQueue = new Queue<LootObject>();
		private Queue<LootObject> RingKeyQueue = new Queue<LootObject>();
 		
		private GearInspectorSettings GISettings = new GearInspectorSettings();
			
		public class GearInspectorSettings
		{
			public bool IdentifySalvage;
			public bool AutoSalvage;
			public bool AutoDessicate;
			public bool ModifiedLooting = true;
			public bool CheckForL7Scrolls;
			public bool SalvageHighValue = false;
			public bool AutoRingKeys;
            public int ItemHudWidth;
            public int ItemHudHeight;
			public int LootByValue = 0;
			public int LootByMana = 0;
			
    	}
		
		private void SubscribeItemEvents()
		{
			try
			{			
             	Core.ItemDestroyed += new EventHandler<ItemDestroyedEventArgs>(InspectorItemDestroyed);
             	Core.WorldFilter.ReleaseObject += new EventHandler<ReleaseObjectEventArgs>(InspectorItemReleased);             		           	
             	SubscribeItemTrackerLooterEvents();           	
			}
			catch(Exception ex){LogError(ex);}
		}
		
		private void UnsubscribeItemEvents()
		{
			try
			{				
             	Core.ItemDestroyed -= new EventHandler<ItemDestroyedEventArgs>(InspectorItemDestroyed);
             	Core.WorldFilter.ReleaseObject -= new EventHandler<ReleaseObjectEventArgs>(InspectorItemReleased);
             	UnSubscribeItemTrackerLooterEvents();         	         	
			}catch(Exception ex){LogError(ex);}
		}
		
		
		private void GearInspectorReadWriteSettings(bool read)
		{
			try
			{
                FileInfo GearInspectorSettingsFile = new FileInfo(GearDir + @"\GearInspector.xml");
								
				if (read)
				{
					
					try
					{
						if (!GearInspectorSettingsFile.Exists)
		                {
		                    try
		                    {
		                    	string filedefaults = GetResourceTextFile("GearInspector.xml");
		                    	using (StreamWriter writedefaults = new StreamWriter(GearInspectorSettingsFile.ToString(), true))
								{
									writedefaults.Write(filedefaults);
									writedefaults.Close();
								}
		                    }
		                    catch (Exception ex) { LogError(ex); }
		                }
						
						using (XmlReader reader = XmlReader.Create(GearInspectorSettingsFile.ToString()))
						{	
							XmlSerializer serializer = new XmlSerializer(typeof(GearInspectorSettings));
							GISettings = (GearInspectorSettings)serializer.Deserialize(reader);
							reader.Close();
						}
					}
					catch
					{
						GISettings = new GearInspectorSettings();
					}
				}
				
				
				if(!read)
				{
					if(GearInspectorSettingsFile.Exists)
					{
						GearInspectorSettingsFile.Delete();
					}
					
					using (XmlWriter writer = XmlWriter.Create(GearInspectorSettingsFile.ToString()))
					{
			   			XmlSerializer serializer2 = new XmlSerializer(typeof(GearInspectorSettings));
			   			serializer2.Serialize(writer, GISettings);
			   			writer.Close();
					}
				}
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
		private const int ItemManaStoneIcon = 0x60032D4;
		private const int ItemDesiccantIcon = 0x6006C0D;
		
		private HudCheckBox InspectorIdentifySalvage = null;
		private HudCheckBox InspectorAutoAetheria = null;
		private HudCheckBox InspectorAutoSalvage = null;
		private HudCheckBox InspectorModifiedLooting = null;
		private HudCheckBox InspectorSalvageHighValue = null;
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
    			ItemHudTabView.AddTab(ItemHudUstLayout, "Process");
    			
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
    					UpdateItemHud();
    					return;
    				case 1:
    					DisposeItemHudInspectorTab();
    					DisposeItemHudSettingsTab();
    					RenderItemHudUstTab();
    					UpdateItemHud();
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
    			ItemHudUstButton.Text = "Process List";
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
    			WriteToChat("Prepare for Warp Speed!  And CoreDumping!");
    			WriteToChat("If the looter is 'hung' pressing this a lot will clear the queue for now");
    			
//    			foreach(LootObject lo in ProcessItemsList)
//    			{
//    				if(lo.IOR == IOResult.salvage)
//    				{
//    					SalvageItemsQueue.Enqueue(lo);
//    				}
//    				else if(lo.IOR == IOResult.dessicate)
//    				{
//    					DesiccateItemsQueue.Enqueue(lo);
//    				}
//    				else if(lo.IOR == IOResult.manatank)
//    				{
//    					ManaTankQueue.Enqueue(lo);
//    				}
//    				else if(lo.ObjectClass == ObjectClass.Key)
//    				{
//    					KeyItemsQueue.Enqueue(lo);
//    				}
//    			}

    			FireInspectorActions();

    		}catch(Exception ex){LogError(ex);}
    	}
    	

    	
    	private void ItemHudUstList_Click(object sender, int row, int col)
    	{
    		try
    		{
    			//Salvage
    			if(col == 0)
    			{
    				if(ProcessItemsList.ElementAt(row).IOR == IOResult.salvage)
    				{
    					SalvageItemsQueue.Enqueue(ProcessItemsList.ElementAt(row));
    				}
    				if(ProcessItemsList.ElementAt(row).IOR == IOResult.dessicate)
    				{
    					DesiccateItemsQueue.Enqueue(ProcessItemsList.ElementAt(row));
    				}
    				if(ProcessItemsList.ElementAt(row).IOR == IOResult.manatank)
    				{
    					ManaTankQueue.Enqueue(ProcessItemsList.ElementAt(row));
    				}
    				if(ProcessItemsList.ElementAt(row).ObjectClass == ObjectClass.Key)
    				{
    					KeyItemsQueue.Enqueue(ProcessItemsList.ElementAt(row));
    				}
    				FireInspectorActions();
    			}
    			//Report
    			if(col == 1)
    			{
    				if(GISettings.ModifiedLooting) {HudToChat(ProcessItemsList.ElementAt(row).GSReportString(), 1);}
    				else{HudToChat(ProcessItemsList.ElementAt(row).LinkString(), 1);}
    			}
    			//Remove
    			if(col == 2)
    			{
    				ProcessItemsList.RemoveAt(row);
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
    			
    			InspectorAutoSalvage = new HudCheckBox();
    			InspectorAutoSalvage.Text = "Auto Salvage";
                ItemHudSettingsLayout.AddControl(InspectorAutoSalvage, new Rectangle(0, 18, 200, 16));
    			InspectorAutoSalvage.Checked = GISettings.AutoSalvage;
    						
    			InspectorAutoAetheria = new HudCheckBox();
    			InspectorAutoAetheria.Text = "Loot and Dessicate Junk Aetheria";
                ItemHudSettingsLayout.AddControl(InspectorAutoAetheria, new Rectangle(0, 36, 200, 16));
    			InspectorAutoAetheria.Checked = GISettings.AutoDessicate;
    			
    			InspectorLootByValue = new HudTextBox();
    			ItemHudSettingsLayout.AddControl(InspectorLootByValue, new Rectangle(0,54,45,16));
    			InspectorLootByValue.Text = GISettings.LootByValue.ToString();
    			
    			InspectorHudValueLabel = new HudStaticText();
                InspectorHudValueLabel.FontHeight = nmenuFontHeight;
    			InspectorHudValueLabel.Text = "High Value Loot.";
    			ItemHudSettingsLayout.AddControl(InspectorHudValueLabel, new Rectangle(50,54,200,16));
    			
    			InspectorSalvageHighValue = new HudCheckBox();
    			InspectorSalvageHighValue.Text = "Salvage High Value Loot";
    			ItemHudSettingsLayout.AddControl(InspectorSalvageHighValue, new Rectangle(0,72,200,16));
    			InspectorSalvageHighValue.Checked = GISettings.SalvageHighValue;
    					
    			InspectorModifiedLooting = new HudCheckBox();
    			InspectorModifiedLooting.Text = "Enabled Modified Looting";
                ItemHudSettingsLayout.AddControl(InspectorModifiedLooting, new Rectangle(0, 90, 200, 16));
    			InspectorModifiedLooting.Checked = GISettings.ModifiedLooting;
    			
    			InspectorCheckForL7Scrolls = new HudCheckBox();
    			InspectorCheckForL7Scrolls.Text = "Loot Unknown L7 Spells";
                ItemHudSettingsLayout.AddControl(InspectorCheckForL7Scrolls, new Rectangle(0, 126, 200, 16));
    			InspectorCheckForL7Scrolls.Checked = GISettings.CheckForL7Scrolls;
    			

    			

    			
    			InspectorHudManaLabel = new HudStaticText();
                InspectorHudManaLabel.FontHeight = nmenuFontHeight;
    			InspectorHudManaLabel.Text = "Mana Value Loot.";
    			ItemHudSettingsLayout.AddControl(InspectorHudManaLabel, new Rectangle(50,158,200,16));		
    			
    			InspectorLootByMana = new HudTextBox();
    			ItemHudSettingsLayout.AddControl(InspectorLootByMana, new Rectangle(0,158,45,16));
    			InspectorLootByMana.Text = GISettings.LootByMana.ToString();
    			
    			InspectorIdentifySalvage.Change += InspectorIdentifySalvage_Change;
    			InspectorAutoAetheria.Change += InspectorAutoAetheria_Change;
    			InspectorAutoSalvage.Change += InspectorAutoSalvage_Change;
    			InspectorModifiedLooting.Change += InspectorModifiedLooting_Change;
    			InspectorCheckForL7Scrolls.Change += InspectorCheckForL7Scrolls_Change;
    			InspectorLootByValue.LostFocus += InspectorLootByValue_LostFocus;
    			InspectorSalvageHighValue.Change += InspectorSalvageHighValue_Change;
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
    	
    	private void InspectorSalvageHighValue_Change(object sender, EventArgs e)
    	{
    		try
    		{
    			GISettings.SalvageHighValue = InspectorSalvageHighValue.Checked;
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
    	
    	private void InspectorAutoAetheria_Change(object sender, System.EventArgs e)
    	{
    		try
    		{
    			GISettings.AutoDessicate = InspectorAutoAetheria.Checked;
    			GearInspectorReadWriteSettings(false);
    		}catch(Exception ex){LogError(ex);}
    	}
    	
    	private void InspectorAutoSalvage_Change(object sender, System.EventArgs e)
    	{
    		try
    		{
    			GISettings.AutoSalvage = InspectorAutoSalvage.Checked;
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
    			InspectorAutoAetheria.Change -= InspectorAutoAetheria_Change;
    			InspectorAutoSalvage.Change -= InspectorAutoSalvage_Change;
    			InspectorModifiedLooting.Change -= InspectorModifiedLooting_Change;
    			InspectorCheckForL7Scrolls.Change -= InspectorCheckForL7Scrolls_Change;
    			InspectorLootByValue.LostFocus -= InspectorLootByValue_LostFocus;
    			InspectorLootByMana.LostFocus -= InspectorLootByMana_LostFocus;
    			InspectorSalvageHighValue.Change -= InspectorSalvageHighValue_Change;
    			
    			
    			InspectorIdentifySalvage.Dispose();
    			InspectorAutoAetheria.Dispose();
    			InspectorAutoSalvage.Dispose();
    			InspectorModifiedLooting.Dispose();
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
    			
    			ItemHudView.Resize -= ItemHudView_Resize;
    			
    			ItemHudSettingsLayout.Dispose();
    			ItemHudUstLayout.Dispose();
    			ItemHudInspectorLayout.Dispose();   			
    			ItemHudTabView.Dispose();
    			ItemHudLayout.Dispose();
    			ItemHudView.Dispose();
    		}	
    		catch(Exception ex){LogError(ex);}
    	}
    		
    	private void ItemHudInspectorList_Click(object sender, int row, int col)
    	{
    		try
			{
    			if(col == 0)
    			{  
    				if(ItemHudMoveQueue.Count == 0)
    				{
    					ItemHudMoveQueue.Enqueue(ItemTrackingList[row]);
    					FireInspectorActions();
    				}
    				else
    				{
    					ItemHudMoveQueue.Enqueue(ItemTrackingList[row]);
    				}
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
                        ((HudStaticText)ItemHudListRow[1]).FontHeight = nitemFontHeight;
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
	    			ItemHudUstList.ClearRows();
	    			foreach(LootObject ustitem in ProcessItemsList)
	    			{
	    				ItemHudListRow = ItemHudUstList.AddRow();
	    				if(ustitem.IOR == IOResult.salvage) {((HudPictureBox)ItemHudListRow[0]).Image = ItemUstIcon;}
	    				if(ustitem.IOR == IOResult.dessicate) {((HudPictureBox)ItemHudListRow[0]).Image = ItemDesiccantIcon;}
	    				if(ustitem.IOR == IOResult.manatank) {((HudPictureBox)ItemHudListRow[0]).Image = ItemManaStoneIcon;}
                        ((HudStaticText)ItemHudListRow[1]).Text = ustitem.IORString() + ustitem.Name;
                        ((HudStaticText)ItemHudListRow[1]).FontHeight = nmenuFontHeight;
                        ((HudPictureBox)ItemHudListRow[2]).Image = ItemRemoveCircle;	
	    			}
	    		}	
	    	}catch(Exception ex){LogError(ex);}
	    	
	    }
	}
}
