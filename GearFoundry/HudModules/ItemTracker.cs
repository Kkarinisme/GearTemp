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
		private GearInspectorSettings GISettings = new GearInspectorSettings();
		
		internal List<LootObject> LOList = new List<LootObject>();
			
		public class GearInspectorSettings
		{
			public bool IdentifySalvage = true;
			public bool AutoLoot = false;
			public bool AutoProcess = false;
			public bool AutoDessicate = false;
			public bool CheckForL7Scrolls = false;
			public bool SalvageHighValue = false;
			public bool AutoRingKeys = false;
			public bool RenderMini = false;
			public bool GSStrings = true;
			public bool AlincoStrings = true;
            public int ItemHudWidth = 300;
            public int ItemHudHeight = 220;
			public int LootByValue = 0;
			public int LootByMana = 0;	
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
		
		private HudView ItemHudView = null;
		private HudTabView ItemHudTabView = null;
		private HudFixedLayout ItemHudInspectorLayout = null;
		private HudFixedLayout ItemHudUstLayout = null;
		private HudFixedLayout ItemHudSettingsLayout = null;		
		
		private HudList ItemHudInspectorList = null;
		private HudList.HudListRowAccessor ItemHudListRow = null;
				
		private HudList ItemHudUstList = null;
		private HudButton ItemHudUstButton = null;
		
		
		
		private HudCheckBox InspectorIdentifySalvage = null;
		private HudCheckBox InspectorAutoAetheria = null;
		private HudCheckBox InspectorAutoProcess = null;
		private HudCheckBox InspectorAutoLoot = null;
		private HudCheckBox InspectorSalvageHighValue = null;
		private HudCheckBox InspectorCheckForL7Scrolls = null;
		private HudStaticText InspectorHudValueLabel = null;
		private HudTextBox InspectorLootByValue = null;
		private HudStaticText InspectorHudManaLabel = null;
		private HudTextBox InspectorLootByMana = null;
		private HudCheckBox InspectorRenderMini = null;
		private HudCheckBox InspectorGSStrings = null;
		private HudCheckBox InspectorAlincoStrings = null;
	
    	private void RenderItemHud()
    	{  		
    		
    		try{
    			    			
    			if(ItemHudView != null)
    			{
    				DisposeItemHud();
    			}

                ItemHudView = new HudView("Inspector", GISettings.ItemHudWidth, GISettings.ItemHudHeight, new ACImage(0x6AA8));
    			ItemHudView.UserAlphaChangeable = false;
    			ItemHudView.UserMinimizable = true;
    			ItemHudView.ShowInBar = false;
    			ItemHudView.Visible = true;
    			if(GISettings.RenderMini){ItemHudView.UserResizeable = false;}
    			else{ItemHudView.UserResizeable = true;}
    			ItemHudView.LoadUserSettings();
    			
    			ItemHudTabView = new HudTabView();
    			ItemHudView.Controls.HeadControl = ItemHudTabView;
    		
    			ItemHudInspectorLayout = new HudFixedLayout();
    			ItemHudTabView.AddTab(ItemHudInspectorLayout, "Inspect");
    			
    			ItemHudUstLayout = new HudFixedLayout();
    			ItemHudTabView.AddTab(ItemHudUstLayout, "Process");
    			
    			ItemHudSettingsLayout = new HudFixedLayout();
    			ItemHudTabView.AddTab(ItemHudSettingsLayout, "Set");
  				
    			ItemHudInspectorList = new HudList();
    			ItemHudInspectorLayout.AddControl(ItemHudInspectorList, new Rectangle(0,0,GISettings.ItemHudWidth,GISettings.ItemHudHeight));
				ItemHudInspectorList.ControlHeight = 16;	
				ItemHudInspectorList.AddColumn(typeof(HudPictureBox), 16, null);
				ItemHudInspectorList.AddColumn(typeof(HudStaticText), GISettings.ItemHudWidth - 60, null);
				ItemHudInspectorList.AddColumn(typeof(HudPictureBox), 16, null);
				ItemHudInspectorList.AddColumn(typeof(HudStaticText), 1, null);
				
				ItemHudUstButton = new HudButton();
    			ItemHudUstButton.Text = "Proc. List";
    			ItemHudUstLayout.AddControl(ItemHudUstButton, new Rectangle(Convert.ToInt32((GISettings.ItemHudWidth - 100) /2),0,100,20));
    			
    			ItemHudUstList = new HudList();
    			ItemHudUstList.AddColumn(typeof(HudPictureBox), 16, null);
    			ItemHudUstList.AddColumn(typeof(HudStaticText), GISettings.ItemHudWidth - 60, null);
    			ItemHudUstList.AddColumn(typeof(HudPictureBox), 16, null);
    			ItemHudUstList.AddColumn(typeof(HudStaticText), 1, null);
    			ItemHudUstLayout.AddControl(ItemHudUstList, new Rectangle(0,30,GISettings.ItemHudWidth,GISettings.ItemHudHeight - 30));
    			
				InspectorIdentifySalvage = new HudCheckBox();
    			InspectorIdentifySalvage.Text = "Ident. Salv.";
                ItemHudSettingsLayout.AddControl(InspectorIdentifySalvage, new Rectangle(0, 17, 100, 16));
    			InspectorIdentifySalvage.Checked = GISettings.IdentifySalvage;
    			
    			InspectorAutoLoot = new HudCheckBox();
    			InspectorAutoLoot.Text = "Auto Pickup";
                ItemHudSettingsLayout.AddControl(InspectorAutoLoot, new Rectangle(0, 34, 100, 16));
    			InspectorAutoLoot.Checked = GISettings.AutoLoot;
    			
    			InspectorAutoProcess = new HudCheckBox();
    			InspectorAutoProcess.Text = "Auto Proc.";
                ItemHudSettingsLayout.AddControl(InspectorAutoProcess, new Rectangle(0, 51, 100, 16));
    			InspectorAutoProcess.Checked = GISettings.AutoProcess;
    						
    			InspectorAutoAetheria = new HudCheckBox();
    			InspectorAutoAetheria.Text = "Des. J. Aeth.";
                ItemHudSettingsLayout.AddControl(InspectorAutoAetheria, new Rectangle(0, 68, 100, 16));
    			InspectorAutoAetheria.Checked = GISettings.AutoDessicate;
    			
    			InspectorCheckForL7Scrolls = new HudCheckBox();
    			InspectorCheckForL7Scrolls.Text = "Unk. L7 Spl.";
                ItemHudSettingsLayout.AddControl(InspectorCheckForL7Scrolls, new Rectangle(0, 85, 100, 16));
    			InspectorCheckForL7Scrolls.Checked = GISettings.CheckForL7Scrolls;  			
    			
    			InspectorLootByValue = new HudTextBox();
    			ItemHudSettingsLayout.AddControl(InspectorLootByValue, new Rectangle(0,102,45,16));
    			InspectorLootByValue.Text = GISettings.LootByValue.ToString();
    			
    			InspectorHudValueLabel = new HudStaticText();
                InspectorHudValueLabel.FontHeight = nmenuFontHeight;
    			InspectorHudValueLabel.Text = "Value";
    			ItemHudSettingsLayout.AddControl(InspectorHudValueLabel, new Rectangle(50,102,100,16));
    			
    			InspectorSalvageHighValue = new HudCheckBox();
    			InspectorSalvageHighValue.Text = "Salv. Value";
    			ItemHudSettingsLayout.AddControl(InspectorSalvageHighValue, new Rectangle(0,119,100,16));
    			InspectorSalvageHighValue.Checked = GISettings.SalvageHighValue;
    					
    			InspectorHudManaLabel = new HudStaticText();
                InspectorHudManaLabel.FontHeight = nmenuFontHeight;
    			InspectorHudManaLabel.Text = "ManaTanks";
    			ItemHudSettingsLayout.AddControl(InspectorHudManaLabel, new Rectangle(50,136,100,16));		
    			
    			InspectorLootByMana = new HudTextBox();
    			ItemHudSettingsLayout.AddControl(InspectorLootByMana, new Rectangle(0,136,45,16));
    			InspectorLootByMana.Text = GISettings.LootByMana.ToString();
    			
    			InspectorRenderMini = new HudCheckBox();
    			InspectorRenderMini.Text = "R. Mini.";
    			ItemHudSettingsLayout.AddControl(InspectorRenderMini, new Rectangle(0,153,100,16));
    			InspectorRenderMini.Checked = GISettings.RenderMini;
    			
    			InspectorGSStrings = new HudCheckBox();
    			InspectorGSStrings.Text = "GS Str.";
    			ItemHudSettingsLayout.AddControl(InspectorGSStrings, new Rectangle(0,170,100,16));
    			InspectorGSStrings.Checked = GISettings.GSStrings;
    			
    			InspectorAlincoStrings = new HudCheckBox();
    			InspectorAlincoStrings.Text = "Alinco Str.";
    			ItemHudSettingsLayout.AddControl(InspectorAlincoStrings, new Rectangle(0,187,100,16));
    			InspectorAlincoStrings.Checked = GISettings.AlincoStrings;
    			
    			ItemHudView.Resize += ItemHudView_Resize;
    			ItemHudView.VisibleChanged += ItemHudView_VisisbleChanged;
    			
    			ItemHudInspectorList.Click += ItemHudInspectorList_Click;
    			
    			ItemHudUstList.Click += ItemHudUstList_Click;
    			ItemHudUstButton.Hit += ItemHudUstButton_Hit;
    				
    			InspectorIdentifySalvage.Change += InspectorIdentifySalvage_Change;
    			InspectorAutoAetheria.Change += InspectorAutoAetheria_Change;
    			InspectorAutoProcess.Change += InspectorAutoProcess_Change;
    			InspectorAutoLoot.Change += InspectorAutoLoot_Change;
    			InspectorCheckForL7Scrolls.Change += InspectorCheckForL7Scrolls_Change;
    			InspectorLootByValue.LostFocus += InspectorLootByValue_LostFocus;
    			InspectorSalvageHighValue.Change += InspectorSalvageHighValue_Change;
    			InspectorLootByMana.LostFocus += InspectorLootByMana_LostFocus;	
    			InspectorRenderMini.Change += InspectorRenderMini_Change;
    			InspectorGSStrings.Change += InspectorGSStrings_Change;
    			InspectorAlincoStrings.Change += InspectorAlincoStrings_Change;
    			
    			UpdateItemHud();
				
				
			  							
    		}catch(Exception ex) {LogError(ex);}
    		
    	}
    	
    	private void ItemHudView_VisisbleChanged(object sender, EventArgs e)
    	{
    		try
    		{
    			DisposeItemHud();
    		}catch(Exception ex){LogError(ex);}
    	}
    	
    	
    	private void DisposeItemHud()
    	{    			
    		try
    		{

    			if(ItemHudView == null) {return;}
    			
    			ItemHudUstLayout.Dispose();
    			ItemHudInspectorLayout.Dispose();   			
    			ItemHudTabView.Dispose();
    			ItemHudView.Dispose();
    			
    			ItemHudView.Resize -= ItemHudView_Resize;
    			ItemHudView.VisibleChanged += ItemHudView_VisisbleChanged;
    			
    			ItemHudInspectorList.Click -= ItemHudInspectorList_Click;	 			
    			ItemHudInspectorList.Dispose(); 
    			ItemHudUstList.Dispose();
    			ItemHudUstButton.Dispose();
    			
    			InspectorIdentifySalvage.Change -= InspectorIdentifySalvage_Change;
    			InspectorAutoAetheria.Change -= InspectorAutoAetheria_Change;
    			InspectorAutoProcess.Change -= InspectorAutoProcess_Change;
    			InspectorCheckForL7Scrolls.Change -= InspectorCheckForL7Scrolls_Change;
    			InspectorLootByValue.LostFocus -= InspectorLootByValue_LostFocus;
    			InspectorLootByMana.LostFocus -= InspectorLootByMana_LostFocus;
    			InspectorSalvageHighValue.Change -= InspectorSalvageHighValue_Change;
    			InspectorGSStrings.Change -= InspectorGSStrings_Change;
    			InspectorAlincoStrings.Change -= InspectorAlincoStrings_Change;
    			InspectorAutoLoot.Change -= InspectorAutoLoot_Change;
    			
    			   			
    			InspectorAutoLoot.Dispose();
    			InspectorIdentifySalvage.Dispose();
    			InspectorAutoAetheria.Dispose();
    			InspectorAutoProcess.Dispose();
    			InspectorCheckForL7Scrolls.Dispose();
    			InspectorLootByMana.Dispose();
    			InspectorLootByValue.Dispose();
    			InspectorHudManaLabel.Dispose();
    			InspectorHudValueLabel.Dispose();
    			InspectorGSStrings.Dispose();
    			InspectorAlincoStrings.Dispose();
    			
    			ItemHudView = null;
    		}	
    		catch(Exception ex){LogError(ex);}
    	}
    	
    	
       
        private void ItemHudView_Resize(object sender, System.EventArgs e)
        {
            try
            {
            	GISettings.ItemHudWidth = ItemHudView.Width;
            	GISettings.ItemHudHeight = ItemHudView.Height;
            	
            	AlterItemHud();
            	GearInspectorReadWriteSettings(false);         
            }
            catch (Exception ex) { LogError(ex); }
        }
        
        private void AlterItemHud()
        {
        	try
        	{
        		ItemHudInspectorList.Click -= ItemHudInspectorList_Click;
    			ItemHudUstList.Click -= ItemHudUstList_Click;
    			ItemHudUstButton.Hit -= ItemHudUstButton_Hit;
    			
    			ItemHudInspectorList.Dispose();
    			ItemHudUstButton.Dispose();
    			ItemHudUstList.Dispose();
    			
    			ItemHudInspectorList = new HudList();
    			ItemHudInspectorLayout.AddControl(ItemHudInspectorList, new Rectangle(0,0,GISettings.ItemHudWidth,GISettings.ItemHudHeight));
				ItemHudInspectorList.ControlHeight = 16;	
				ItemHudInspectorList.AddColumn(typeof(HudPictureBox), 16, null);
				ItemHudInspectorList.AddColumn(typeof(HudStaticText), GISettings.ItemHudWidth - 60, null);
				ItemHudInspectorList.AddColumn(typeof(HudPictureBox), 16, null);
				ItemHudInspectorList.AddColumn(typeof(HudStaticText), 1, null);
				
				ItemHudUstButton = new HudButton();
    			ItemHudUstButton.Text = "Proc. List";
    			ItemHudUstLayout.AddControl(ItemHudUstButton, new Rectangle(Convert.ToInt32((GISettings.ItemHudWidth - 100) /2),0,100,20));
    			
    			ItemHudUstList = new HudList();
    			ItemHudUstList.AddColumn(typeof(HudPictureBox), 16, null);
    			ItemHudUstList.AddColumn(typeof(HudStaticText), GISettings.ItemHudWidth - 60, null);
    			ItemHudUstList.AddColumn(typeof(HudPictureBox), 16, null);
    			ItemHudUstList.AddColumn(typeof(HudStaticText), 1, null);
    			ItemHudUstLayout.AddControl(ItemHudUstList, new Rectangle(0,30,GISettings.ItemHudWidth,GISettings.ItemHudHeight - 30));
    			
    			ItemHudInspectorList.Click += ItemHudInspectorList_Click;
    			ItemHudUstList.Click += ItemHudUstList_Click;
    			ItemHudUstButton.Hit += ItemHudUstButton_Hit;
    			
    			UpdateItemHud();
        		
        	}catch(Exception ex){LogError(ex);}
        }

    	private void ItemHudUstButton_Hit(object sender, EventArgs e)
    	{
    		try
    		{    			
    			foreach(LootObject lo in  LOList.FindAll(x => x.ProcessList).ToList())
    			{
    				FoundryLoadAction(lo.FoundryProcess, lo.Id);
    			}

    			InitiateFoundryActions();
    			WriteToChat("Prosessing List.");
    		}catch(Exception ex){LogError(ex);}
    	}    
    	
    	HudList.HudListRowAccessor UstListAcessor = null;
    	private void ItemHudUstList_Click(object sender, int row, int col)
    	{
    		try
    		{
    			UstListAcessor = ItemHudUstList[row];
    			LootObject lo = LOList.Find(x => x.Id == Convert.ToInt32(((HudStaticText)UstListAcessor[3]).Text));
    			if(col == 0)
    			{  
    				FoundryLoadAction(lo.FoundryProcess, lo.Id);
    				InitiateFoundryActions();
    			}
    			if(col == 1)
    			{
    				if(GISettings.GSStrings) {HudToChat(lo.GSReportString(), 1);}
    				if(GISettings.AlincoStrings){HudToChat(lo.LinkString(), 1);}
    			}
    			if(col == 2)
    			{    				
    				lo.ProcessList = false;
    			}
    			UpdateItemHud();
    			
    		}catch(Exception ex){LogError(ex);}
    		
    	}
    			
    	private void InspectorGSStrings_Change(object sender, System.EventArgs e)
    	{
    		try
    		{
    			GISettings.GSStrings = InspectorGSStrings.Checked;
    			GearInspectorReadWriteSettings(false);
    		}catch(Exception ex){LogError(ex);}
    	}
    			
    	private void InspectorAlincoStrings_Change(object sender, System.EventArgs e)
    	{
    		try
    		{
    			GISettings.AlincoStrings = InspectorAlincoStrings.Checked;
    			GearInspectorReadWriteSettings(false);
    		}catch(Exception ex){LogError(ex);}
    	}
    	
    	private void InspectorRenderMini_Change(object sender, System.EventArgs e)
    	{
    		try
    		{
    			GISettings.RenderMini = InspectorRenderMini.Checked;
    			
    			if(GISettings.RenderMini)
    			{
    				ItemHudView.UserResizeable = false;
    				GISettings.ItemHudHeight = 220;
    				GISettings.ItemHudWidth = 120;
    			}
    			else
    			{
    				ItemHudView.UserResizeable = true;
    				GISettings.ItemHudHeight = 220;
    				GISettings.ItemHudWidth = 300;
    			}
    					
    			ItemHudView.Width = GISettings.ItemHudWidth;
    			ItemHudView.Height = GISettings.ItemHudHeight;
    			
    			AlterItemHud();
    			GearInspectorReadWriteSettings(false);
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
    	
    	private void InspectorAutoProcess_Change(object sender, System.EventArgs e)
    	{
    		try
    		{
    			GISettings.AutoProcess = InspectorAutoProcess.Checked;
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
    	    	
    	
    	private void InspectorCheckForL7Scrolls_Change(object sender, System.EventArgs e)
    	{
    		try
    		{
    			GISettings.CheckForL7Scrolls = InspectorCheckForL7Scrolls.Checked;
    			GearInspectorReadWriteSettings(false);
    		}catch(Exception ex){LogError(ex);}
    	}
   	   	      		

    		    	
    	HudList.HudListRowAccessor InspectorListRow = null;
    	private void ItemHudInspectorList_Click(object sender, int row, int col)
    	{
    		try
			{
    			InspectorListRow = ItemHudInspectorList[row];
    			LootObject lo = LOList.Find(x => x.Id == Convert.ToInt32(((HudStaticText)InspectorListRow[3]).Text));  //(HudStaticText)CombatHudRow[11]).Text
    			if(col == 0)
    			{  
					FoundryLoadAction(FoundryActionTypes.MoveToPack, lo.Id);
					InitiateFoundryActions();
    			}
    			if(col == 1)
    			{
    				if(GISettings.GSStrings) {HudToChat(lo.GSReportString(), 1);}
    				if(GISettings.AlincoStrings){HudToChat(lo.LinkString(), 1);}
    			}
    			if(col == 2)
    			{    				
    				lo.InspectList = false;
    			}
				UpdateItemHud();
			}
			catch (Exception ex) { LogError(ex); }	
    	}
    		
    	LootObject LOListAcessor = null;
	    private void UpdateItemHud()
	    {  	
	    	try
	    	{    
	    		
	    		if(ItemHudView == null) {return;}
	    		
	    		ItemHudInspectorList.ClearRows();
	    		ItemHudUstList.ClearRows();
	    		
	    		for(int i = LOList.Count - 1; i >= 0; i--)
	    		{
	    			LOListAcessor = LOList[i];
	    			if(LOListAcessor.InspectList)
	    			{
		    	    	ItemHudListRow = ItemHudInspectorList.AddRow();	
		    	    	((HudPictureBox)ItemHudListRow[0]).Image = LOListAcessor.Icon + 0x6000000;
		    	    	if(GISettings.RenderMini){((HudStaticText)ItemHudListRow[1]).Text = LOListAcessor.MiniIORString();}
		    	    	else{((HudStaticText)ItemHudListRow[1]).Text = LOListAcessor.IORString() + LOListAcessor.Name;}
                        ((HudStaticText)ItemHudListRow[1]).FontHeight = nitemFontHeight;
		    	    	if(LOListAcessor.IOR == IOResult.trophy) {((HudStaticText)ItemHudListRow[1]).TextColor = Color.Wheat;}
		    	    	if(LOListAcessor.IOR == IOResult.salvage) {((HudStaticText)ItemHudListRow[1]).TextColor = Color.PaleVioletRed;}
		    	    	if(LOListAcessor.IOR == IOResult.val) {((HudStaticText)ItemHudListRow[1]).TextColor = Color.PaleGoldenrod;}
		    	    	if(LOListAcessor.IOR == IOResult.spell) {((HudStaticText)ItemHudListRow[1]).TextColor = Color.Lavender;}
		    	    	if(LOListAcessor.IOR == IOResult.rule)  {((HudStaticText)ItemHudListRow[1]).TextColor = Color.LightGreen;}
		    	    	if(LOListAcessor.IOR == IOResult.rare)  {((HudStaticText)ItemHudListRow[1]).TextColor = Color.HotPink;}
		    	    	if(LOListAcessor.IOR == IOResult.manatank)  {((HudStaticText)ItemHudListRow[1]).TextColor = Color.CornflowerBlue;}
						((HudPictureBox)ItemHudListRow[2]).Image = GearGraphics.RemoveCircle;
						((HudStaticText)ItemHudListRow[3]).Text = LOListAcessor.Id.ToString();
	    			}
	    			
	    			if(LOListAcessor.ProcessList)
		    		{
	    				ItemHudListRow = ItemHudUstList.AddRow();
	    				if(LOListAcessor.FoundryProcess == FoundryActionTypes.Salvage) {((HudPictureBox)ItemHudListRow[0]).Image = GearGraphics.ItemUstIcon;}
	    				else if(LOListAcessor.FoundryProcess == FoundryActionTypes.Desiccate) {((HudPictureBox)ItemHudListRow[0]).Image = GearGraphics.ItemDesiccantIcon;}
	    				else if(LOListAcessor.FoundryProcess == FoundryActionTypes.ManaStone) {((HudPictureBox)ItemHudListRow[0]).Image = GearGraphics.ItemManaStoneIcon;}
	    				else {((HudPictureBox)ItemHudListRow[0]).Image = LOListAcessor.Icon;}
	    				if(GISettings.RenderMini) {((HudStaticText)ItemHudListRow[1]).Text = LOListAcessor.MiniIORString();}
	    				else{((HudStaticText)ItemHudListRow[1]).Text = LOListAcessor.IORString() + LOListAcessor.Name;}
                        ((HudStaticText)ItemHudListRow[1]).FontHeight = nmenuFontHeight;
                        ((HudPictureBox)ItemHudListRow[2]).Image = GearGraphics.RemoveCircle;	
                        ((HudStaticText)ItemHudListRow[3]).Text = LOListAcessor.Id.ToString();
		    		}
	    	    }
	   
	
	    	}catch(Exception ex){LogError(ex);}
	    	
	    }
	    
	    private void SynchWithLOList(FoundryActionTypes action, int ItemId)
	    {
	    	try
	    	{
	    		int loIndex = LOList.FindIndex(x => x.Id == ItemId);
	    		if(loIndex < 0) {return;}
	    		if(LOList[loIndex].InspectList)
	    		{
	    			if(Core.WorldFilter.GetInventory().Where(x => x.Id == ItemId).Count() > 0)
	    			{
	    				LOList[loIndex].InspectList = false;
	    			}
	    		}
	    		if(LOList[loIndex].FoundryProcess != FoundryActionTypes.None)
	    		{
	    			LOList[loIndex].ProcessList = true;
	    			if(GISettings.AutoProcess){FoundryLoadAction(LOList[loIndex].FoundryProcess, LOList[loIndex].Id);}
	    		}
	    		UpdateItemHud();
	    	}catch(Exception ex){LogError(ex);}
	    }

	}
}
