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
		//UNDONE:  Need to complete logging and verify functionality of DEADME
		private List<string> FellowMemberTrackingList = new List<string>();
		private List<int> CorpseExclusionList = new List<int>();
		private List<IdentifiedObject> CorpseTrackingList = new List<IdentifiedObject>();
		private bool mCorpseTrackerInPoralSpace = true;
		private List<string> PermittedCorpsesList = new List<string>(); 
		public GearVisectionSettings ghSettings;	
		public DateTime LastCorpseHudUpdate;			
		
		public class MyCorpses  
		{
			public int GUID;
			public string Name;
			public string Coordinates;
			public int IconID;
		}

		public class GearVisectionSettings
    	{
			public bool bAllCorpses = true;
			public bool bKillsBySelf = true;
			public bool bKillsByFellows = true;
			public bool bDeadMes = true;
			public bool Permitteds = true;
			public List<MyCorpses> DeadMeList = new List<PluginCore.MyCorpses>();
            public int CorpseHudWidth;
            public int CorpseHudHeight;

    	}
		
		private void GearVisectionReadWriteSettings(bool read)
		{
			try
			{
				FileInfo GearVisectionSettingsFile = new FileInfo(GearDir + @"\GearVisection.xml");
								
				if (read)
				{
					
					try
					{
						if (!GearVisectionSettingsFile.Exists)
		                {
		                    try
		                    {
		                    	string filedefaults = GetResourceTextFile("GearVisection.xml");
		                    	using (StreamWriter writedefaults = new StreamWriter(GearVisectionSettingsFile.ToString(), true))
								{
									writedefaults.Write(filedefaults);
									writedefaults.Close();
								}
		                    }
		                    catch (Exception ex) { LogError(ex); }
		                }
						
						using (XmlReader reader = XmlReader.Create(GearVisectionSettingsFile.ToString()))
						{	
							XmlSerializer serializer = new XmlSerializer(typeof(GearVisectionSettings));
							ghSettings = (GearVisectionSettings)serializer.Deserialize(reader);
							reader.Close();
						}
					}
					catch
					{
						ghSettings = new GearVisectionSettings();
					}
				}
				
				if(!read)
				{
					if(GearVisectionSettingsFile.Exists)
					{
						GearVisectionSettingsFile.Delete();
					}
					
					using (XmlWriter writer = XmlWriter.Create(GearVisectionSettingsFile.ToString()))
					{
			   			XmlSerializer serializer2 = new XmlSerializer(typeof(GearVisectionSettings));
			   			serializer2.Serialize(writer, ghSettings);
			   			writer.Close();
					}
				}
			}catch(Exception ex){LogError(ex);}
		}
		
		
		private void SubscribeCorpseEvents()
		{
			try
			{
				MasterTimer.Tick += CorpseCheckerTick;
				Core.WorldFilter.CreateObject += new EventHandler<CreateObjectEventArgs>(OnWorldFilterCreateCorpse);
             	Core.EchoFilter.ServerDispatch += new EventHandler<NetworkMessageEventArgs>(ServerDispatchCorpse);
                Core.WorldFilter.ReleaseObject += new EventHandler<ReleaseObjectEventArgs>(OnWorldFilterDeleteCorpse);
                Core.ItemDestroyed += new EventHandler<ItemDestroyedEventArgs>(OnCorpseDestroyed);
                Core.CharacterFilter.ChangePortalMode += new EventHandler<ChangePortalModeEventArgs>(ChangePortalModeCorpses);
                Core.ChatBoxMessage += new EventHandler<ChatTextInterceptEventArgs>(ChatBoxCorpse);
                Core.ContainerOpened += new EventHandler<ContainerOpenedEventArgs>(OnCorpseOpened);
                CorpseHudView.Resize += CorpseHudView_Resize; 

			}
			catch(Exception ex){LogError(ex);}
		}
		
		private void UnsubscribeCorpseEvents()
		{
			try
			{
				MasterTimer.Tick -= CorpseCheckerTick;
				Core.WorldFilter.CreateObject -= new EventHandler<CreateObjectEventArgs>(OnWorldFilterCreateCorpse);
             	Core.EchoFilter.ServerDispatch -= new EventHandler<NetworkMessageEventArgs>(ServerDispatchCorpse);
                Core.WorldFilter.ReleaseObject -= new EventHandler<ReleaseObjectEventArgs>(OnWorldFilterDeleteCorpse);
                Core.ItemDestroyed -= new EventHandler<ItemDestroyedEventArgs>(OnCorpseDestroyed);
                Core.CharacterFilter.ChangePortalMode -= new EventHandler<ChangePortalModeEventArgs>(ChangePortalModeCorpses);
                Core.ChatBoxMessage -= new EventHandler<ChatTextInterceptEventArgs>(ChatBoxCorpse);
                Core.ContainerOpened -= new EventHandler<ContainerOpenedEventArgs>(OnCorpseOpened);
                CorpseHudView.Resize -= CorpseHudView_Resize; 

			}
			catch(Exception ex){LogError(ex);}
		}
		
		private void OnWorldFilterCreateCorpse(object sender, Decal.Adapter.Wrappers.CreateObjectEventArgs e)
		{
			try 
			{  	
				if(!mCharacterLoginComplete || !bCorpseHudEnabled) {return;}
				if(e.New.ObjectClass == ObjectClass.Corpse) 
				{
					if(CorpseExclusionList.Contains(e.New.Id)){return;}
					else{CheckCorpse(new IdentifiedObject(e.New));}
				}
		
			} catch (Exception ex){LogError(ex);}
		}
		
	

        private void ServerDispatchCorpse(object sender, Decal.Adapter.NetworkMessageEventArgs e)
        {
        	int iEvent = 0;
            try
            {
            	if(e.Message.Type == AC_PLAYER_KILLED)
            	{
            		//AddDeadMe();
            	}
            	if(e.Message.Type == AC_GAME_EVENT)
            	{
            		try
                    {
                    	iEvent = Convert.ToInt32(e.Message["event"]);
                    }
                    catch{}
                    if(iEvent == GE_IDENTIFY_OBJECT)
                    {
                    	 OnIdentCorpse(e.Message);
                    }    
            	}
            }
            catch (Exception ex){LogError(ex);}
        }  
        
        private void OnIdentCorpse(Decal.Adapter.Message pMsg)
		{
			try
			{
				if(!mCharacterLoginComplete || !bCorpseHudEnabled) {return;}
				
        		int PossibleCorpseID = Convert.ToInt32(pMsg["object"]);	
        		if(Core.WorldFilter[PossibleCorpseID].ObjectClass == ObjectClass.Corpse)
				{
        			if(CorpseExclusionList.Contains(PossibleCorpseID)){return;}
        			else{CheckCorpse(new IdentifiedObject(Core.WorldFilter[PossibleCorpseID]));}
				}
			} 
			catch (Exception ex) {LogError(ex);}
		}
        
        private void CheckCorpse(IdentifiedObject IOCorpse)
		{
			try
			{	
				if(!IOCorpse.isvalid){return;}
				if(IOCorpse.Name.Contains(Core.CharacterFilter.Name) && ghSettings.bDeadMes)
				{
					IOCorpse.IOR = IOResult.corpseofself;
					if(ghSettings.DeadMeList.FindIndex(x => x.GUID == IOCorpse.Id) < 0)
					{
						MyCorpses DeadMe = new MyCorpses();
						DeadMe.Name = IOCorpse.Name;
						DeadMe.IconID = IOCorpse.Icon;
//						DeadMe.Coordinates = IOCorpse.Coordinates.ToString();
						DeadMe.GUID = IOCorpse.Id;
						
						ghSettings.DeadMeList.Add(DeadMe);
					}
					if(CorpseTrackingList.FindIndex(x => x.Id == IOCorpse.Id) < 0) 
					{
						CorpseTrackingList.Add(IOCorpse); 
						CorpseExclusionList.Add(IOCorpse.Id);
						UpdateCorpseHud();
					}
					return;
				}
				
				//Flags corpes for recovery by that player as permitted
				if(ghSettings.Permitteds && PermittedCorpsesList.Contains(IOCorpse.Name))
				{
					IOCorpse.IOR = IOResult.corpsepermitted;
					if(CorpseTrackingList.FindIndex(x => x.Id == IOCorpse.Id) < 0) 
					{
						CorpseTrackingList.Add(IOCorpse); 
						CorpseExclusionList.Add(IOCorpse.Id);
						UpdateCorpseHud();
					}
					return;
				}
							
				//Corpses with loot on them.
				//Enables tracking of kills made by the character
				if(IOCorpse.LValue(LongValueKey.Burden) > 6000 && ghSettings.bKillsBySelf)
				{
					if(!IOCorpse.HasIdData)	{IdqueueAdd(IOCorpse.Id); return;}
					else if(string.IsNullOrEmpty(IOCorpse.SValue(StringValueKey.FullDescription))){return;}
					else
					{
						if (IOCorpse.SValue(StringValueKey.FullDescription).Contains(Core.CharacterFilter.Name)) 
						{
							if (IOCorpse.SValue(StringValueKey.FullDescription).Contains("generated a rare item")) 
							{
								IOCorpse.IOR = IOResult.corpsewithrare;
								if(CorpseTrackingList.FindIndex(x => x.Id == IOCorpse.Id) < 0) 
								{
									CorpseTrackingList.Add(IOCorpse); 
									CorpseExclusionList.Add(IOCorpse.Id);
									UpdateCorpseHud();
								}
								return;
							} 
							else 
							{
								IOCorpse.IOR = IOResult.corpseselfkill;
								if(CorpseTrackingList.FindIndex(x => x.Id == IOCorpse.Id) < 0) 
								{
									CorpseTrackingList.Add(IOCorpse); 
									CorpseExclusionList.Add(IOCorpse.Id);
									UpdateCorpseHud();
								}
								return;
							}
						}
						else if(FellowMemberTrackingList.Count() > 0 && ghSettings.bKillsByFellows)
						{
							foreach(string fellow in FellowMemberTrackingList)
							{
								if(IOCorpse.SValue(StringValueKey.FullDescription).Contains(fellow)) 
								{
									IOCorpse.IOR = IOResult.corpsefellowkill;
									if(CorpseTrackingList.FindIndex(x => x.Id == IOCorpse.Id) < 0) 
									{
										CorpseTrackingList.Add(IOCorpse); 
										CorpseExclusionList.Add(IOCorpse.Id);
										UpdateCorpseHud();
									}
									return;
								}
							}					
						}
						
					}
				}	
				
				if(ghSettings.bAllCorpses)
				{
					if(CorpseTrackingList.FindIndex(x => x.Id == IOCorpse.Id) < 0) 
					{
						IOCorpse.IOR = IOResult.allcorpses;
						CorpseTrackingList.Add(IOCorpse); 
						CorpseExclusionList.Add(IOCorpse.Id);
						UpdateCorpseHud();
						return;
					}
				}
			}
			catch (Exception ex){LogError(ex);}
		}
        
        
        
		private void OnWorldFilterDeleteCorpse(object sender, Decal.Adapter.Wrappers.ReleaseObjectEventArgs e)
		{
			try 
			{
				CorpseTrackingList.RemoveAll(x => !x.isvalid);
				if (CorpseTrackingList.Any(x => x.Id == e.Released.Id))
				{
					CorpseTrackingList.RemoveAll(x => x.Id == e.Released.Id);
					CorpseExclusionList.RemoveAll(x => x == e.Released.Id);
					UpdateCorpseHud();
				}
			} 
			catch (Exception ex) {LogError(ex);}
		}
		
		private void OnCorpseDestroyed(object sender, ItemDestroyedEventArgs e)
		{
			try
			{
				CorpseTrackingList.RemoveAll(x => !x.isvalid);
				if(CorpseTrackingList.Any(x => x.Id == e.ItemGuid))
				{
					CorpseTrackingList.RemoveAll(x => x.Id == e.ItemGuid);
					CorpseExclusionList.RemoveAll(x => x == e.ItemGuid);
					UpdateCorpseHud();
				}
			}catch(Exception ex){LogError(ex);}
		}
		
		private void ChangePortalModeCorpses(object sender, Decal.Adapter.Wrappers.ChangePortalModeEventArgs e)
		{
			try
			{
				if(!mCharacterLoginComplete){return;}
				else
				{
					if(mCorpseTrackerInPoralSpace) {mCorpseTrackerInPoralSpace = false;}
					else{mCorpseTrackerInPoralSpace = true;}
					
					if(mCorpseTrackerInPoralSpace)
					{
						var CTLpurge = from corpses in CorpseTrackingList
						where Core.WorldFilter.Distance(Core.CharacterFilter.Id, corpses.Id) > 5
		     			select corpses.Id;
		     		
			     		foreach(var corpseID in CTLpurge)
			     		{
			     			if(ghSettings.DeadMeList.FindIndex(x => x.GUID == corpseID) < 0)
			     			{
			     				CorpseTrackingList.RemoveAll(x => x.Id == corpseID);
			     			}
			     		}
			     		UpdateCorpseHud();
					}
				}			
			}catch(Exception ex){LogError(ex);}

		}
		
        private void ChatBoxCorpse(object sender, Decal.Adapter.ChatTextInterceptEventArgs e)
        {
            try 
            {
            	//Line Feed Strip
        		string CBMessage = e.Text.Substring(0, e.Text.Length - 1);
                
                if(CBMessage.Contains("has given you permission to loot his or her kills."))
                {
               		string FellowMemberName = CBMessage.Replace(" has given you permission to loot his or her kills.", "");
                   	FellowMemberTrackingList.Add(FellowMemberName);
                }
                
                if(CBMessage == "You no longer have permission to loot anyone else's kills.")
                {
                	FellowMemberTrackingList.Clear();
                }
                
                if(CBMessage.StartsWith("You have lost permission to loot the kills of"))
                {
                	string FellowMemberName = CBMessage.Replace("You have lost permission to loot the kills of ", "");
                	FellowMemberName.Replace(".", "");
                	FellowMemberTrackingList.RemoveAll(x => x == FellowMemberName);
                }
                
                if(CBMessage.EndsWith("This permission will last one hour."))
                {
                	string LootMyCorpseName = CBMessage.Substring(0, CBMessage.IndexOf(" has given you permission to loot"));
                	PermittedCorpsesList.Add("Corpse of " + LootMyCorpseName);
                	if(CorpseTrackingList.FindIndex(x => x.Name == "Corpse of " + LootMyCorpseName) >= 0)
                	{
                		CorpseTrackingList.Find(x => x.Name == "Corpse of " + LootMyCorpseName).IOR = IOResult.corpsepermitted;
                	}
                	UpdateCorpseHud();
                }               
	
            } catch (Exception ex) {LogError(ex);}
        }
        
        private void OnCorpseOpened(object sender, Decal.Adapter.ContainerOpenedEventArgs e)
        {
        	try
        	{
        		CorpseTrackingList.RemoveAll(x => x.Id == e.ItemGuid);
		        UpdateCorpseHud();   
        	}catch(Exception ex){LogError(ex);}
        }
		
        private int CorpseTimer = 0;
        private void CorpseCheckerTick(object sender, EventArgs e)
        {
        	try
        	{
	        	if(CorpseTimer == 4)
	        	{	        		
	        		DistanceCheckCorpses();
	        		CorpseTimer = 0;
	        	}
	        	CorpseTimer++;
        	}catch(Exception ex){LogError(ex);}
        }
		
	    private void DistanceCheckCorpses()
	    {
     		try
	   		{	
     			for(int i = CorpseTrackingList.Count - 1; i >= 0 ; i--)
		    	{
     				if(CorpseTrackingList[i].isvalid)
     				{
     					CorpseTrackingList[i].DistanceAway = Core.WorldFilter.Distance(Core.CharacterFilter.Id, CorpseTrackingList[i].Id);
	     				if(CorpseTrackingList[i].DistanceAway > 5) {CorpseTrackingList.RemoveAt(i);}
     				}
     				else
     				{
	     				CorpseTrackingList.RemoveAt(i);
     				}
		    	}
     			CorpseTrackingList = CorpseTrackingList.OrderBy(x => x.DistanceAway).ToList();
	     		UpdateCorpseHud();
	     	}catch(Exception ex){LogError(ex);}
    	}
				
	    private HudView CorpseHudView = null;
		private HudFixedLayout CorpseHudLayout = null;
		private HudTabView CorpseHudTabView = null;
		private HudFixedLayout CorpseHudTabLayout = null;
		private HudFixedLayout CorpseHudSettingsTab = null;
		private HudList CorpseHudList = null;
		private HudList.HudListRowAccessor CorpseHudListRow = null;
		
		private bool CorpseMainTab;
		private bool CorpseSettingsTab;
		
		private const int CorpseRemoveCircle = 0x60011F8;
        private int CorpseHudWidth;
        private int CorpseHudHeight;
        private int CorpseHudFirstWidth = 300;
        private int CorpseHudFirstHeight = 220;
        private int CorpseHudWidthNew;
        private int CorpseHudHeightNew;

			
    	private void RenderCorpseHud()
    	{
    		try
    		{
    			GearVisectionReadWriteSettings(true);
    	
    		}catch(Exception ex){LogError(ex);}
    		
    		
    		try
    		{
    			    			
    			if(CorpseHudView != null)
    			{
    				DisposeCorpseHud();
    			}
                if (ghSettings.CorpseHudWidth == 0) { CorpseHudWidth = CorpseHudFirstWidth; }
                else { CorpseHudWidth = ghSettings.CorpseHudWidth; }
                if (ghSettings.CorpseHudHeight == 0) { CorpseHudHeight = CorpseHudFirstHeight; }
                else { CorpseHudHeight = ghSettings.CorpseHudHeight; }

    			
    		//	CorpseHudView = new HudView("GearVisection", 300, 220, new ACImage(0x6AA4));
                CorpseHudView = new HudView("GearVisection", CorpseHudWidth, CorpseHudHeight, new ACImage(0x6AA4));
              //  CorpseHudView.Theme = VirindiViewService.HudViewDrawStyle.GetThemeByName("Minimalist Transparent");
    			CorpseHudView.UserAlphaChangeable = false;
    			CorpseHudView.ShowInBar = false;
    			CorpseHudView.UserResizeable = false;
    			CorpseHudView.Visible = true;
    			CorpseHudView.Ghosted = false;
                CorpseHudView.UserMinimizable = false;
                CorpseHudView.LoadUserSettings();
    			
    			
    			CorpseHudLayout = new HudFixedLayout();
    			CorpseHudView.Controls.HeadControl = CorpseHudLayout;
    			
    			CorpseHudTabView = new HudTabView();
    			CorpseHudLayout.AddControl(CorpseHudTabView, new Rectangle(0,0,CorpseHudWidth,CorpseHudHeight));
    		
    			CorpseHudTabLayout = new HudFixedLayout();
    			CorpseHudTabView.AddTab(CorpseHudTabLayout, "GearVisection");
    			
    			CorpseHudSettingsTab = new HudFixedLayout();
    			CorpseHudTabView.AddTab(CorpseHudSettingsTab, "Settings");
    			
    			CorpseHudTabView.OpenTabChange += CorpseHudTabView_OpenTabChange;
                CorpseHudView.Resize += CorpseHudView_Resize; 

    			RenderCorpseHudTab();
    			

				SubscribeCorpseEvents();
                CorpseHudView.UserResizeable = true;


			  							
    		}catch(Exception ex){LogError(ex);}
    		
    	}

        private void CorpseHudView_Resize(object sender, System.EventArgs e)
        {
            try
            {
                bool bw = Math.Abs(CorpseHudView.Width - CorpseHudWidth) > 20;
                bool bh = Math.Abs(CorpseHudView.Height - CorpseHudHeight) > 20;
                if (bh || bw)
                {
                    CorpseHudWidthNew = CorpseHudView.Width;
                    CorpseHudHeightNew = CorpseHudView.Height;
                    MasterTimer.Tick += CorpseHudResizeTimerTick;
                }
            }
            catch (Exception ex) { LogError(ex); }
            return;



        }

        private void CorpseHudResizeTimerTick(object sender, EventArgs e)
        {
            CorpseHudWidth = CorpseHudWidthNew;
            CorpseHudHeight = CorpseHudHeightNew;
            ghSettings.CorpseHudWidth = CorpseHudWidth;
            ghSettings.CorpseHudHeight = CorpseHudHeight;
            GearVisectionReadWriteSettings(false);            
            MasterTimer.Tick -= CorpseHudResizeTimerTick;
            RenderCorpseHud();

        }

    	
    	private void CorpseHudTabView_OpenTabChange(object sender, System.EventArgs e)
    	{
    		try
    		{
    			switch(CorpseHudTabView.CurrentTab)
    			{
    				case 0:
    					DisposeCorpseHudSettingsTab();
    					RenderCorpseHudTab();
    					return;
    				case 1:
    					DisposeCorpseHudTab();
    					RenderCorpseHudSettingsTab();
    					return;
    			}
    			
    		}catch(Exception ex){LogError(ex);}
    		
    	}
    	
    	
    	private void RenderCorpseHudTab()
    	{
    		try
    		{
    			CorpseHudList = new HudList();
    			CorpseHudTabLayout.AddControl(CorpseHudList, new Rectangle(0,0,CorpseHudWidth,CorpseHudHeight));
				CorpseHudList.ControlHeight = 16;	
				CorpseHudList.AddColumn(typeof(HudPictureBox), 16, null);
				CorpseHudList.AddColumn(typeof(HudStaticText), CorpseHudWidth-40, null);
				CorpseHudList.AddColumn(typeof(HudPictureBox), 16, null);
				
				CorpseHudList.Click += (sender, row, col) => CorpseHudList_Click(sender, row, col);	
				
				CorpseMainTab = true;
    			
    		}catch(Exception ex){LogError(ex);}
    	}
    	
    	private void DisposeCorpseHudTab()
    	{
    		try
    		{
    			if(!CorpseMainTab) { return;}
    			CorpseHudList.Click -= (sender, row, col) => CorpseHudList_Click(sender, row, col);	
    			CorpseHudList.Dispose();	
				CorpseMainTab = false;    			
    			
    		}catch(Exception ex){LogError(ex);}
    	}
    	
    	private void DisposeCorpseHud()
    	{
    			
    		try
    		{
    			UnsubscribeCorpseEvents();
    			DisposeCorpseHudTab();
    			DisposeCorpseHudSettingsTab();

    			CorpseHudSettingsTab.Dispose();
    			CorpseHudTabLayout.Dispose();
                CorpseHudTabView.Dispose();
                CorpseHudLayout.Dispose();
                CorpseHudView.Dispose();
				
    		}	
    		catch(Exception ex){LogError(ex);}
    	}
    	   	
    	HudCheckBox AllCorpses;
    	HudCheckBox KillsBySelf;
    	HudCheckBox KillsByFellows;
    	HudCheckBox DeadMes;
    	HudCheckBox Permitteds;
    	
    	private void RenderCorpseHudSettingsTab()
    	{
    		try
    		{
    			AllCorpses = new HudCheckBox();
    			AllCorpses.Text = "Track All Corpses";
    			CorpseHudSettingsTab.AddControl(AllCorpses, new Rectangle(0,0,150,16));
    			AllCorpses.Checked = ghSettings.bAllCorpses;
    			
    			KillsBySelf = new HudCheckBox();
    			KillsBySelf.Text = "Track Kills by Self";
    			CorpseHudSettingsTab.AddControl(KillsBySelf, new Rectangle(0,18,150,16));
    			KillsBySelf.Checked = ghSettings.bKillsBySelf;
    			
    			KillsByFellows = new HudCheckBox();
    			KillsByFellows.Text = "Track Kills by Fellows";
    			CorpseHudSettingsTab.AddControl(KillsByFellows, new Rectangle(0,36,150,16));
    			KillsByFellows.Checked = ghSettings.bKillsByFellows;
    			
    			DeadMes = new HudCheckBox();
    			DeadMes.Text = "Track Dead Me(s)";
    			CorpseHudSettingsTab.AddControl(DeadMes, new Rectangle(0,54,150,16));
    			DeadMes.Checked = ghSettings.bDeadMes;
    			
    			Permitteds = new HudCheckBox();
    			Permitteds.Text = "Track Permitted Corpses";
    			CorpseHudSettingsTab.AddControl(Permitteds, new Rectangle(0,72,150,16));
    			Permitteds.Checked = ghSettings.Permitteds;
    			    			
    			AllCorpses.Change += AllCorpses_Change;
    			KillsBySelf.Change += KillsBySelf_Change;
    			KillsByFellows.Change += KillsByFellows_Change;
    			DeadMes.Change += DeadMes_Change;
    			Permitteds.Change += Permitteds_Change;
    			
    			CorpseSettingsTab = true;
    			
    		}catch(Exception ex){LogError(ex);}
    	}
    		
    	private void DisposeCorpseHudSettingsTab()
    	{
    		try
    		{
    			if(!CorpseSettingsTab) { return;}
    			AllCorpses.Change -= AllCorpses_Change;
    			KillsBySelf.Change -= KillsBySelf_Change;
    			KillsByFellows.Change -= KillsByFellows_Change;
    			DeadMes.Change -= DeadMes_Change;
    			Permitteds.Change -= Permitteds_Change;
    			
    			
    			Permitteds.Dispose();
    			DeadMes.Dispose();
    			KillsByFellows.Dispose();
    			KillsBySelf.Dispose();
    			AllCorpses.Dispose();
    			
    			CorpseSettingsTab = false;
    			
    		}catch(Exception ex){LogError(ex);}
    	}
    	
    	void CorpseHudList_Click(object sender, int row, int col)
    	{
    		try
			{
    			if(col == 0)
    			{
    				Host.Actions.UseItem(CorpseTrackingList[row].Id, 0);		
    			}
    			if(col == 1)
    			{
    				Host.Actions.SelectItem(CorpseTrackingList[row].Id);
    				HudToChat(LandscapeTrackingList[row].LinkString(), 2);
    			}
    			if(col == 2)
    			{    				
    				CorpseExclusionList.Add(CorpseTrackingList[row].Id);
    				CorpseTrackingList.RemoveAt(row);
    			}
				UpdateCorpseHud();
			}
			catch (Exception ex) { LogError(ex); }	
    	}
    	
    	private void AllCorpses_Change(object sender, System.EventArgs e)
    	{
    		try
    		{
    			ghSettings.bAllCorpses = AllCorpses.Checked;
    			GearVisectionReadWriteSettings(false);
    		}catch(Exception ex){LogError(ex);}
    	}
    	
    	private void KillsBySelf_Change(object sender, System.EventArgs e)
    	{
    		try
    		{
    			ghSettings.bKillsBySelf = KillsBySelf.Checked;
    			GearVisectionReadWriteSettings(false);
    		}catch(Exception ex){LogError(ex);}
    	}
    	
    	private void KillsByFellows_Change(object sender, System.EventArgs e)
    	{
    		try
    		{
    			ghSettings.bKillsByFellows = KillsByFellows.Checked;
    			GearVisectionReadWriteSettings(false);
    		}catch(Exception ex){LogError(ex);}
    	}
    	
    	    	private void DeadMes_Change(object sender, System.EventArgs e)
    	{
    		try
    		{
    			ghSettings.bDeadMes = DeadMes.Checked;
    			GearVisectionReadWriteSettings(false);
    		}catch(Exception ex){LogError(ex);}
    	}
    	    	
    	    	private void Permitteds_Change(object sender, System.EventArgs e)
    	{
    		try
    		{
    			ghSettings.Permitteds = Permitteds.Checked;
    			GearVisectionReadWriteSettings(false);
    		}catch(Exception ex){LogError(ex);}
    	}
    		
	    private void UpdateCorpseHud()
	    {  	
	    	try
	    	{    		
	    		if((DateTime.Now - LastCorpseHudUpdate).TotalMilliseconds < 1000){return;}
	    		else{LastCorpseHudUpdate = DateTime.Now;}
	    		
	    		if(!CorpseMainTab) {return;}
	    		
	    		CorpseHudList.ClearRows();
  	    		
	    	    foreach(IdentifiedObject corpse in CorpseTrackingList)
	    	    {
	    	    	CorpseHudListRow = CorpseHudList.AddRow();
	    	    	
	    	    	((HudPictureBox)CorpseHudListRow[0]).Image = corpse.Icon + 0x6000000;
	    	    	((HudStaticText)CorpseHudListRow[1]).Text = corpse.Name + corpse.DistanceString();
	    	    	if(corpse.IOR == IOResult.corpseselfkill) {((HudStaticText)CorpseHudListRow[1]).TextColor = Color.AntiqueWhite;}
	    	    	if(corpse.IOR == IOResult.corpsepermitted) {((HudStaticText)CorpseHudListRow[1]).TextColor = Color.Cyan;}
	    	    	if(corpse.IOR == IOResult.corpseofself) {((HudStaticText)CorpseHudListRow[1]).TextColor = Color.Yellow;}
	    	    	if(corpse.IOR == IOResult.corpsewithrare) {((HudStaticText)CorpseHudListRow[1]).TextColor = Color.Magenta;}
	    	    	if(corpse.IOR == IOResult.corpsefellowkill) {((HudStaticText)CorpseHudListRow[1]).TextColor = Color.Green;}
	    	    	if(corpse.IOR == IOResult.allcorpses) {((HudStaticText)CorpseHudListRow[1]).TextColor = Color.SlateGray;}
					((HudPictureBox)CorpseHudListRow[2]).Image = CorpseRemoveCircle;
	    	    }
	    	}catch(Exception ex){LogError(ex);}
	    	
	    }	    

	}
}
