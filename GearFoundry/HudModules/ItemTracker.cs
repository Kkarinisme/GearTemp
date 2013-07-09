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
		private List<int> ModifiedIOSpells  = new List<int>();		
		
		private List<LootObject> ItemTrackingList = new List<LootObject>();	
		
		private List<LootObject> ProcessItemsList = new List<LootObject>();
 		
		private GearInspectorSettings GISettings;
		
			
		public class GearInspectorSettings
		{
			public bool IdentifySalvage = true;
			public bool AutoSalvage = false;
			public bool AutoDessicate = false;
			public bool ModifiedLooting = true;
			public bool CheckForL7Scrolls = false;
			public bool SalvageHighValue = false;
			public bool AutoRingKeys = false;
			public bool RenderMini = false;
            public int ItemHudWidth = 300;
            public int ItemHudHeight = 220;
			public int LootByValue = 0;
			public int LootByMana = 0;
			
    	}
		
		private void SubscribeItemEvents()
		{
			try
			{		           	
             	SubscribeItemTrackerLooterEvents();           	
			}
			catch(Exception ex){LogError(ex);}
		}
		
		private void UnsubscribeItemEvents()
		{
			try
			{		
				mOpenContainer = null;
				
				ItemExclusionList = null;
				ItemIDListenList = null;
				ModifiedIOSpells = null;
				
				ItemTrackingList = null;
				
				ProcessItemsList = null;

				
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
		                    	GISettings = new GearInspectorSettings();
		                    	
		                    	using (XmlWriter writer = XmlWriter.Create(GearInspectorSettingsFile.ToString()))
								{
						   			XmlSerializer serializer2 = new XmlSerializer(typeof(GearInspectorSettings));
						   			serializer2.Serialize(writer, GISettings);
						   			writer.Close();
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
		private HudCheckBox InspectorRenderMini = null;
	
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

                ItemHudView = new HudView("Inspector", GISettings.ItemHudWidth, GISettings.ItemHudHeight, new ACImage(0x6AA8));
    			ItemHudView.UserAlphaChangeable = false;
    			ItemHudView.UserMinimizable = false;
    			ItemHudView.ShowInBar = false;
    			ItemHudView.Visible = true;
    			ItemHudView.UserResizeable = false;
    			ItemHudView.LoadUserSettings();
    			
    			ItemHudLayout = new HudFixedLayout();
    			ItemHudView.Controls.HeadControl = ItemHudLayout;
    			
    			ItemHudTabView = new HudTabView();
    			ItemHudLayout.AddControl(ItemHudTabView, new Rectangle(0,0, GISettings.ItemHudWidth, GISettings.ItemHudHeight));
    		
    			ItemHudInspectorLayout = new HudFixedLayout();
    			ItemHudTabView.AddTab(ItemHudInspectorLayout, "Inspect");
    			
    			ItemHudUstLayout = new HudFixedLayout();
    			ItemHudTabView.AddTab(ItemHudUstLayout, "Process");
    			
    			ItemHudSettingsLayout = new HudFixedLayout();
    			ItemHudTabView.AddTab(ItemHudSettingsLayout, "Set");
    			
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
               bool bw = Math.Abs(ItemHudView.Width - GISettings.ItemHudWidth) > 20;
                bool bh = Math.Abs(ItemHudView.Height - GISettings.ItemHudHeight) > 20;
                if (bh || bw)
                {
                    MasterTimer.Tick += ItemHudResizeTimerTick;
                    return;
                }
            }
            catch (Exception ex) { LogError(ex); }
            return;
        }

        private void ItemHudResizeTimerTick(object sender, EventArgs e)
        {
        	MasterTimer.Tick -= ItemHudResizeTimerTick;
            GISettings.ItemHudWidth = ItemHudView.Width;
            GISettings.ItemHudHeight = ItemHudView.Height;
            GearInspectorReadWriteSettings(false);
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
    			ItemHudUstButton.Text = "Proc. List";
    			ItemHudUstLayout.AddControl(ItemHudUstButton, new Rectangle(Convert.ToInt32(((double)GISettings.ItemHudWidth - (double)100) /(double)2),0,100,20));
    			
    			ItemHudUstList = new HudList();
    			ItemHudUstList.AddColumn(typeof(HudPictureBox), 16, null);
    			ItemHudUstList.AddColumn(typeof(HudStaticText), GISettings.ItemHudWidth - 60, null);
    			ItemHudUstList.AddColumn(typeof(HudPictureBox), 16, null);
    			ItemHudUstLayout.AddControl(ItemHudUstList, new Rectangle(0,30,GISettings.ItemHudWidth,GISettings.ItemHudHeight - 30));
    			
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
    			if(ActionsPending)
    			{
    				WriteToChat("Wait for it!");
    				return;
    			}
    			
    			WriteToChat("Prosessing Queue.");
    			
    			if(Core.Actions.CombatMode != CombatState.Peace)
				{
    				PendingActions peaceaction = new PendingActions();
		    		peaceaction.Action = IAction.PeaceMode;
    				InspectorActionQueue.Enqueue(peaceaction);
    			}
    			
    			foreach(LootObject proc in  ProcessItemsList)
    			{
     				PendingActions nextaction = new PendingActions();
    				nextaction.LootItem = proc;
	    				
	    			if(proc.IOR == IOResult.salvage)
	    			{
	    				nextaction.Action = IAction.SalvageItem;
    					InspectorActionQueue.Enqueue(nextaction);
	    			}
	    			else if(proc.IOR == IOResult.dessicate)
	    			{
	    				nextaction.Action = IAction.Desiccate;
    					InspectorActionQueue.Enqueue(nextaction);
	    			}
	    			else if(proc.IOR == IOResult.manatank)
	    			{
	    				nextaction.Action = IAction.ManaStone;
    					InspectorActionQueue.Enqueue(nextaction);
	    			}
	    			else if(proc.ObjectClass == ObjectClass.Key)
	    			{
	    				nextaction.Action = IAction.RingKey;
    					InspectorActionQueue.Enqueue(nextaction);
	    			}		
    			}
    			    			
    			InitiateInspectorActionSequence();
    			
    		}catch(Exception ex){LogError(ex);}
    	}    
    	
    	private void ItemHudUstList_Click(object sender, int row, int col)
    	{
    		try
    		{
    			//Salvage
    			if(col == 0)
    			{	
    				if(InspectorActionQueue.Any(x => x.LootItem.Id == ProcessItemsList.ElementAt(row).Id))
    				{
    					return;
    				}
    				else
    				{
	    				if(ProcessItemsList.ElementAt(row).IOR == IOResult.salvage)
	    				{
	    					PendingActions nextaction = new PendingActions();
	    					nextaction.Action = IAction.SalvageItem;
	    					nextaction.LootItem = ProcessItemsList.ElementAt(row);
	    					InspectorActionQueue.Enqueue(nextaction);
	    				}
	    				if(ProcessItemsList.ElementAt(row).IOR == IOResult.dessicate)
	    				{
	    					PendingActions nextaction = new PendingActions();
	    					nextaction.Action = IAction.Desiccate;
	    					nextaction.LootItem = ProcessItemsList.ElementAt(row);
	    					InspectorActionQueue.Enqueue(nextaction);
	    				}
	    				if(ProcessItemsList.ElementAt(row).IOR == IOResult.manatank)
	    				{
	    					PendingActions nextaction = new PendingActions();
	    					nextaction.Action = IAction.ManaStone;
	    					nextaction.LootItem = ProcessItemsList.ElementAt(row);
	    					InspectorActionQueue.Enqueue(nextaction);
	    				}
	    				if(ProcessItemsList.ElementAt(row).ObjectClass == ObjectClass.Key)
	    				{
	    					PendingActions nextaction = new PendingActions();
	    					nextaction.Action = IAction.RingKey;
	    					nextaction.LootItem = ProcessItemsList.ElementAt(row);
	    					InspectorActionQueue.Enqueue(nextaction);
	    				}
	    				if(!ActionsPending) {InitiateInspectorActionSequence();}
    				}
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
    			InspectorModifiedLooting = new HudCheckBox();
    			InspectorModifiedLooting.Text = "Enable GS Loot";
                ItemHudSettingsLayout.AddControl(InspectorModifiedLooting, new Rectangle(0, 0, 100, 16));
    			InspectorModifiedLooting.Checked = GISettings.ModifiedLooting;
    			InspectorIdentifySalvage = new HudCheckBox();
    			
    			InspectorIdentifySalvage.Text = "Ident. Salv.";
                ItemHudSettingsLayout.AddControl(InspectorIdentifySalvage, new Rectangle(0, 18, 100, 16));
    			InspectorIdentifySalvage.Checked = GISettings.IdentifySalvage;
    			
    			InspectorAutoSalvage = new HudCheckBox();
    			InspectorAutoSalvage.Text = "Auto Salv.";
                ItemHudSettingsLayout.AddControl(InspectorAutoSalvage, new Rectangle(0, 36, 100, 16));
    			InspectorAutoSalvage.Checked = GISettings.AutoSalvage;
    						
    			InspectorAutoAetheria = new HudCheckBox();
    			InspectorAutoAetheria.Text = "Des. J. Aeth.";
                ItemHudSettingsLayout.AddControl(InspectorAutoAetheria, new Rectangle(0, 54, 100, 16));
    			InspectorAutoAetheria.Checked = GISettings.AutoDessicate;
    			
    			InspectorCheckForL7Scrolls = new HudCheckBox();
    			InspectorCheckForL7Scrolls.Text = "Unk. L7 Spl.";
                ItemHudSettingsLayout.AddControl(InspectorCheckForL7Scrolls, new Rectangle(0, 72, 100, 16));
    			InspectorCheckForL7Scrolls.Checked = GISettings.CheckForL7Scrolls;  			
    			
    			InspectorLootByValue = new HudTextBox();
    			ItemHudSettingsLayout.AddControl(InspectorLootByValue, new Rectangle(0,90,45,16));
    			InspectorLootByValue.Text = GISettings.LootByValue.ToString();
    			
    			InspectorHudValueLabel = new HudStaticText();
                InspectorHudValueLabel.FontHeight = nmenuFontHeight;
    			InspectorHudValueLabel.Text = "High Value Loot.";
    			ItemHudSettingsLayout.AddControl(InspectorHudValueLabel, new Rectangle(50,90,100,16));
    			
    			InspectorSalvageHighValue = new HudCheckBox();
    			InspectorSalvageHighValue.Text = "Salv. HV";
    			ItemHudSettingsLayout.AddControl(InspectorSalvageHighValue, new Rectangle(0,108,100,16));
    			InspectorSalvageHighValue.Checked = GISettings.SalvageHighValue;
    					
    			InspectorHudManaLabel = new HudStaticText();
                InspectorHudManaLabel.FontHeight = nmenuFontHeight;
    			InspectorHudManaLabel.Text = "ManaTanks";
    			ItemHudSettingsLayout.AddControl(InspectorHudManaLabel, new Rectangle(50,126,100,16));		
    			
    			InspectorLootByMana = new HudTextBox();
    			ItemHudSettingsLayout.AddControl(InspectorLootByMana, new Rectangle(0,126,45,16));
    			InspectorLootByMana.Text = GISettings.LootByMana.ToString();
    			
    			InspectorRenderMini = new HudCheckBox();
    			InspectorRenderMini.Text = "R. Mini.";
    			ItemHudSettingsLayout.AddControl(InspectorRenderMini, new Rectangle(0,144,60,16));
    			InspectorRenderMini.Checked = GISettings.RenderMini;
    			
    			
    			InspectorIdentifySalvage.Change += InspectorIdentifySalvage_Change;
    			InspectorAutoAetheria.Change += InspectorAutoAetheria_Change;
    			InspectorAutoSalvage.Change += InspectorAutoSalvage_Change;
    			InspectorModifiedLooting.Change += InspectorModifiedLooting_Change;
    			InspectorCheckForL7Scrolls.Change += InspectorCheckForL7Scrolls_Change;
    			InspectorLootByValue.LostFocus += InspectorLootByValue_LostFocus;
    			InspectorSalvageHighValue.Change += InspectorSalvageHighValue_Change;
    			InspectorLootByMana.LostFocus += InspectorLootByMana_LostFocus;	
    			InspectorRenderMini.Change += InspectorRenderMini_Change;
    			  			
    			InspectorSettingsTab = true;
    			
   
    		}catch(Exception ex){LogError(ex);}
    	}
    	
    	private void InspectorRenderMini_Change(object sender, System.EventArgs e)
    	{
    		try
    		{
    			GISettings.RenderMini = InspectorRenderMini.Checked;
    			if(GISettings.RenderMini)
    			{
    				GISettings.ItemHudHeight = 220;
    				GISettings.ItemHudWidth = 150;
    			}
    			else
    			{
    				GISettings.ItemHudHeight = 220;
    				GISettings.ItemHudWidth = 300;
    			}
    			GearInspectorReadWriteSettings(false);
    			RenderItemHud();
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
    			ItemHudInspectorLayout.AddControl(ItemHudInspectorList, new Rectangle(0,0,GISettings.ItemHudWidth,GISettings.ItemHudHeight));
				ItemHudInspectorList.ControlHeight = 16;	
				ItemHudInspectorList.AddColumn(typeof(HudPictureBox), 16, null);
				ItemHudInspectorList.AddColumn(typeof(HudStaticText), GISettings.ItemHudWidth - 60, null);
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
    		
    	//TODO:  Need to add a 4th column to lists with GUIDs in them to prevent the hud list from desynching with the tracking list
    	//(minor irritation)  Currently resolved on any update to the hud.
    	private void ItemHudInspectorList_Click(object sender, int row, int col)
    	{
    		try
			{
    			if(col == 0)
    			{  	
    				try
    				{
	    				if(!InspectorActionQueue.Any(x => x.LootItem.Id == ItemTrackingList.ElementAt(row).Id))
	    				{
		    				if(Core.Actions.CombatMode != CombatState.Peace)
							{
		    					PendingActions peaceaction = new PendingActions();
				    			peaceaction.Action = IAction.PeaceMode;
		    					InspectorActionQueue.Enqueue(peaceaction);
							}
		    				
		    				PendingActions nextaction = new PendingActions();
				    		nextaction.Action = IAction.MoveItem;
				    		nextaction.LootItem = ItemTrackingList.ElementAt(row);
		    				InspectorActionQueue.Enqueue(nextaction);
		    				
		    				if(!ActionsPending) {InitiateInspectorActionSequence();}
	    				
    					}
    					else
    					{
	    					return;
    					}
    				//Empty catch will eliminate the slow list update causing exceptions when clicked.  TODO:  Better solution with the item ID stored in the hud list.	
    				}catch{}
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
		    	    	if(GISettings.RenderMini){((HudStaticText)ItemHudListRow[1]).Text = item.MiniIORString() + item.TruncateName();}
		    	    	else{((HudStaticText)ItemHudListRow[1]).Text = item.IORString() + item.Name;}
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
	    				else if(ustitem.IOR == IOResult.dessicate) {((HudPictureBox)ItemHudListRow[0]).Image = ItemDesiccantIcon;}
	    				else if(ustitem.IOR == IOResult.manatank) {((HudPictureBox)ItemHudListRow[0]).Image = ItemManaStoneIcon;}
	    				else {((HudPictureBox)ItemHudListRow[0]).Image = ustitem.Icon;}
	    				if(GISettings.RenderMini) {((HudStaticText)ItemHudListRow[1]).Text = ustitem.MiniIORString() + ustitem.TruncateName();}
	    				else{((HudStaticText)ItemHudListRow[1]).Text = ustitem.IORString() + ustitem.Name;}
                        ((HudStaticText)ItemHudListRow[1]).FontHeight = nmenuFontHeight;
                        ((HudPictureBox)ItemHudListRow[2]).Image = ItemRemoveCircle;	
	    			}
	    		}	
	    	}catch(Exception ex){LogError(ex);}
	    	
	    }
	}
}
