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

namespace GearFoundry
{

	public partial class PluginCore
	{
		//UNDONE:  Need to complete logging and verify functionality of DEADME and Permitted corpses.
		private List<string> FellowMemberTrackingList = new List<string>();
		private List<int> CorpseExclusionList = new List<int>();
		private List<IdentifiedObject> CorpseTrackingList = new List<IdentifiedObject>();
		private bool mCorpseTrackerInPoralSpace = true;
		
		void SubscribeCorpseEvents()
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
			}
			catch(Exception ex){LogError(ex);}
		}
		
		void UnsubscribeCorpseEvents()
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
            		AddDeadMe();
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
            catch (Exception ex)
            {
                LogError(ex);
            }
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
        
        void CheckCorpse(IdentifiedObject IOCorpse)
		{
			try
			{
				if(IOCorpse.Name.Contains(Core.CharacterFilter.Name) && btoonCorpsesEnabled)
				{
					IOCorpse.IOR = IOResult.corpseofself;
					if(DeadMeCoordinatesList.FindIndex(x => x.GUID == IOCorpse.Id) < 0)
					{
						MyCorpses DeadMe = new MyCorpses();
						DeadMe.Name = IOCorpse.Name;
						DeadMe.IconID = IOCorpse.Icon;
						DeadMe.Coordinates = IOCorpse.Coordinates.ToString();
						DeadMe.GUID = IOCorpse.Id;
						
						DeadMeCoordinatesList.Add(DeadMe);
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
				if(PermittedCorpsesList.Contains(IOCorpse.Name))
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
				if(IOCorpse.IntValues(LongValueKey.Burden) > 6000 && btoonKillsEnabled)
				{
					if(!IOCorpse.HasIdData)	{IdqueueAdd(IOCorpse.Id); return;}
					else if(string.IsNullOrEmpty(IOCorpse.StringValues(StringValueKey.FullDescription))){return;}
					else
					{
						if (IOCorpse.StringValues(StringValueKey.FullDescription).Contains(Core.CharacterFilter.Name)) 
						{
							if (IOCorpse.StringValues(StringValueKey.FullDescription).Contains("generated a rare item")) 
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
						else if(FellowMemberTrackingList.Count() > 0 && bFellowKillsEnabled)
						{
							foreach(string fellow in FellowMemberTrackingList)
							{
								if(IOCorpse.StringValues(StringValueKey.FullDescription).Contains(fellow)) 
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
			}
			catch (Exception ex)
			{
				LogError(ex);
			}
		}
        
        
        
		private void OnWorldFilterDeleteCorpse(object sender, Decal.Adapter.Wrappers.ReleaseObjectEventArgs e)
		{
			try 
			{
				CorpseTrackingList.RemoveAll(x => x.Id == e.Released.Id);
				CorpseExclusionList.RemoveAll(x => x == e.Released.Id);
				UpdateCorpseHud();

			} 
			catch (Exception ex) {LogError(ex);}
		}
		
		private void OnCorpseDestroyed(object sender, ItemDestroyedEventArgs e)
		{
			try
			{
				CorpseTrackingList.RemoveAll(x => x.Id == e.ItemGuid);
				CorpseExclusionList.RemoveAll(x => x == e.ItemGuid);
				UpdateCorpseHud();
			} catch {}
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
			     			if(DeadMeCoordinatesList.FindIndex(x => x.GUID == corpseID) < 0)
			     			{
			     				CorpseTrackingList.RemoveAll(x => x.Id == corpseID);
			     			}
			     		}
			     		UpdateCorpseHud();
					}
				}
				
			}
			catch{}

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
                }               
	
            } catch (Exception ex) {
                LogError(ex);
            }
        }
        
        void OnCorpseOpened(object sender, Decal.Adapter.ContainerOpenedEventArgs e)
        {
        	try
        	{
        		CorpseTrackingList.RemoveAll(x => x.Id == e.ItemGuid);
		        UpdateCorpseHud();
	        
        	}catch{}
        }
		
        int CorpseTimer = 0;
        void CorpseCheckerTick(object sender, EventArgs e)
        {
        	try
        	{
	        	if(CorpseTimer == 4)
	        	{	        		
	        		DistanceCheckCorpses();
	        		CorpseTimer = 0;
	        	}
	        	CorpseTimer++;
        	}
        	catch{}
        }
		
	    private void DistanceCheckCorpses()
	    {
     		try
	   		{	
	     		var CTLpurge = from corpses in CorpseTrackingList
	     			where Core.WorldFilter.Distance(Core.CharacterFilter.Id, corpses.Id) > 5
	     			select corpses.Id;
	     		
	     		foreach(var corpse in CTLpurge)
	     		{
	     			if(DeadMeCoordinatesList.FindIndex(x => x.GUID == corpse) < 0)
	     			{
	     				CorpseTrackingList.RemoveAll(x => x.Id == corpse);
	     			}
	     		}
	     		UpdateCorpseHud();
	     	}
	     	catch{}
    	}
	    
	    private void AddDeadMe()
	    {
	    	
	    }
				
	    private HudView CorpseHudView = null;
		private HudFixedLayout CorpseHudLayout = null;
		private HudTabView CorpseHudTabView = null;
		private HudFixedLayout CorpseHudTabLayout = null;
		private HudList CorpseHudList = null;
		private HudList.HudListRowAccessor CorpseHudListRow = null;
		
		//Assembly tests
		
		private const int CorpseRemoveCircle = 0x60011F8;
			
    	private void RenderCorpseHud()
    	{
    		try{
    			    			
    			if(CorpseHudView != null)
    			{
    				DisposeCorpseHud();
    			}			
    			
    			CorpseHudView = new HudView("GearHound", 300, 220, new ACImage(4208));
    			CorpseHudView.Theme = VirindiViewService.HudViewDrawStyle.GetThemeByName("Minimalist Transparent");
    			CorpseHudView.UserAlphaChangeable = false;
    			CorpseHudView.ShowInBar = false;
    			CorpseHudView.UserResizeable = false;
    			CorpseHudView.Visible = true;
    			CorpseHudView.Ghosted = false;
                CorpseHudView.UserMinimizable = false;
                CorpseHudView.UserClickThroughable = true;
    			
    			
    			CorpseHudLayout = new HudFixedLayout();
    			CorpseHudView.Controls.HeadControl = CorpseHudLayout;
    			
    			CorpseHudTabView = new HudTabView();
    			CorpseHudLayout.AddControl(CorpseHudTabView, new Rectangle(0,0,300,220));
    		
    			CorpseHudTabLayout = new HudFixedLayout();
    			CorpseHudTabView.AddTab(CorpseHudTabLayout, "GearHound");
    			
    			CorpseHudList = new HudList();
    			CorpseHudTabLayout.AddControl(CorpseHudList, new Rectangle(0,0,300,220));
				CorpseHudList.ControlHeight = 16;	
				CorpseHudList.AddColumn(typeof(HudPictureBox), 16, null);
				CorpseHudList.AddColumn(typeof(HudStaticText), 230, null);
				CorpseHudList.AddColumn(typeof(HudPictureBox), 16, null);
				
				CorpseHudList.Click += (sender, row, col) => CorpseHudList_Click(sender, row, col);				
			  							
    		}
    		catch(Exception ex){LogError(ex);}
    		
    	}
    	
    	void DisposeCorpseHud()
    	{
    			
    		try
    		{
    			CorpseHudList.Click -= (sender, row, col) => CorpseHudList_Click(sender, row, col);		
    			CorpseHudList.Dispose();
    			CorpseHudTabLayout.Dispose();
    			CorpseHudTabView.Dispose();
    			CorpseHudLayout.Dispose();
    			CorpseHudView.Dispose();		
    		}	
    		catch(Exception ex){LogError(ex);}
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
    				ItemExclusionList.Add(CorpseTrackingList[row].Id);
    				CorpseTrackingList.RemoveAt(row);
    			}
				UpdateCorpseHud();
			}
			catch (Exception ex) { LogError(ex); }	
    	}
    		
	    private void UpdateCorpseHud()
	    {  	
	    	try
	    	{    		
	    		CorpseHudList.ClearRows();
	    		   	    		
	    	    foreach(IdentifiedObject corpse in CorpseTrackingList)
	    	    {
	    	    	CorpseHudListRow = CorpseHudList.AddRow();
	    	    	
	    	    	((HudPictureBox)CorpseHudListRow[0]).Image = corpse.Icon + 0x6000000;
	    	    	((HudStaticText)CorpseHudListRow[1]).Text = corpse.Name;
	    	    	if(corpse.IOR == IOResult.corpseselfkill) {((HudStaticText)CorpseHudListRow[1]).TextColor = Color.AntiqueWhite;}
	    	    	if(corpse.IOR == IOResult.corpsepermitted) {((HudStaticText)CorpseHudListRow[1]).TextColor = Color.Cyan;}
	    	    	if(corpse.IOR == IOResult.corpseofself) {((HudStaticText)CorpseHudListRow[1]).TextColor = Color.Yellow;}
	    	    	if(corpse.IOR == IOResult.corpsewithrare) {((HudStaticText)CorpseHudListRow[1]).TextColor = Color.Magenta;}
	    	    	if(corpse.IOR == IOResult.corpsefellowkill) {((HudStaticText)CorpseHudListRow[1]).TextColor = Color.Green;}
					((HudPictureBox)CorpseHudListRow[2]).Image = CorpseRemoveCircle;
	    	    }
	       	}catch(Exception ex){WriteToChat(ex.ToString());}
	    	
	    }	    

	}
}
