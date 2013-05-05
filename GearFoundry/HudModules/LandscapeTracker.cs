﻿
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
using System.Xml.Serialization;
using System.Xml;

namespace GearFoundry
{
	public partial class PluginCore
	{
		private List<int> LandscapeExclusionList = new List<int>();
		private List<IdentifiedObject> LandscapeTrackingList = new List<IdentifiedObject>();
		private List<string> LandscapeFellowMemberTrackingList = new List<string>();
		private bool mLandscapeInPortalSpace = true;
		
		public GearSenseSettings gsSettings = new GearSenseSettings();
		
		public class GearSenseSettings
		{			
			public bool bShowAllMobs = true;
			public bool bShowSelectedMobs = true;
			public bool bShowAllPlayers = true;
			public bool bShowAllegancePlayers = true;
			public bool bShowFellowPlayers = true;
			public bool bShowTrophies = true;
			public bool bShowLifeStones = true;
			public bool bShowAllPortals = true;
			public bool bShowAllNPCs = true;
		}
		
		
		private void GearSenseReadWriteSettings(bool read)
		{
			try
			{
				FileInfo GearSenseSettingsFile = new FileInfo(toonDir + @"\GearSense.xml");
								
				if (read)
				{
					
					try
					{
						if (!GearSenseSettingsFile.Exists)
		                {
		                    try
		                    {
		                    	string filedefaults = GetResourceTextFile("GearSense.xml");
		                    	using (StreamWriter writedefaults = new StreamWriter(GearSenseSettingsFile.ToString(), true))
								{
									writedefaults.Write(filedefaults);
									writedefaults.Close();
								}
		                    }
		                    catch (Exception ex) { LogError(ex); }
		                }
						
						using (XmlReader reader = XmlReader.Create(GearSenseSettingsFile.ToString()))
						{	
							XmlSerializer serializer = new XmlSerializer(typeof(GearSenseSettings));
							gsSettings = (GearSenseSettings)serializer.Deserialize(reader);
							reader.Close();
						}
					}
					catch
					{
						gsSettings = new GearSenseSettings();
					}
				}
				
				
				if(!read)
				{
					if(GearSenseSettingsFile.Exists)
					{
						GearSenseSettingsFile.Delete();
					}
					
					using (XmlWriter writer = XmlWriter.Create(GearSenseSettingsFile.ToString()))
					{
			   			XmlSerializer serializer2 = new XmlSerializer(typeof(GearSenseSettings));
			   			serializer2.Serialize(writer, gsSettings);
			   			writer.Close();
					}
				}
			}catch(Exception ex){WriteToChat(ex.ToString());}
		}
				
		private void SubscribeLandscapeEvents()
		{
			try
			{
				MasterTimer.Tick += LandscapeTimerTick;
				Core.WorldFilter.CreateObject += OnWorldFilterCreateLandscape;
             	Core.EchoFilter.ServerDispatch += ServerDispatchLandscape;
                Core.WorldFilter.ReleaseObject += OnWorldFilterDeleteLandscape;
                Core.ItemDestroyed +=OnLandscapeDestroyed;
                Core.CharacterFilter.ChangePortalMode += ChangePortalModeLandscape;
                Core.CharacterFilter.ChangeFellowship += ChangeFellowship_Changed;
			}
			catch(Exception ex) {LogError(ex);}
			return;
		}
		
		private void UnsubscribeLandscapeEvents()
		{
			try
			{
				MasterTimer.Tick -= LandscapeTimerTick;
				Core.WorldFilter.CreateObject -= OnWorldFilterCreateLandscape;
             	Core.EchoFilter.ServerDispatch -= ServerDispatchLandscape;
                Core.WorldFilter.ReleaseObject -= OnWorldFilterDeleteLandscape;
                Core.ItemDestroyed -= OnLandscapeDestroyed;
                Core.CharacterFilter.ChangePortalMode -= ChangePortalModeLandscape;  
 				Core.CharacterFilter.ChangeFellowship -= ChangeFellowship_Changed;                
			
			}catch(Exception ex) {LogError(ex);}
			return;
		}
		
		private void ChangeFellowship_Changed(object sender, System.EventArgs e)
		{
			try
			{
				foreach(string name in LandscapeFellowMemberTrackingList)
				{
					if(LandscapeTrackingList.FindIndex(x => x.Name == name) < 0)
					{
						LandscapeTrackingList.Add(new IdentifiedObject(Core.WorldFilter.GetByName(name).First));
					}
				}
			}catch{}
		}
		
		
		private void OnWorldFilterCreateLandscape(object sender, Decal.Adapter.Wrappers.CreateObjectEventArgs e)
		{
			try 
			{   
				if(!bLandscapeHudEnabled) {return;}
				if(e.New.Container != 0 || LandscapeExclusionList.Contains(e.New.Id)) {return;}
				else
				{
					CheckLandscape(new IdentifiedObject(e.New));
				}
			} catch (Exception ex){LogError(ex);}
			return;
		}

        private void ServerDispatchLandscape(object sender, Decal.Adapter.NetworkMessageEventArgs e)
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
                    if(iEvent == GE_ADD_FELLOWMEMBER)
                    {
                    	AddFellowLandscape(e);
                    }
                    if(iEvent == GE_FELLOWSHIP_MEMBER_QUIT || iEvent == GE_FELLOWSHIP_MEMBER_DISMISSED)
                    {
                    	RemoveFellowLandscape(e);
                    }
                    if(iEvent == GE_DISBAND_FELLOWSHIP)
                    {
                    	ClearFellowLandscape(e);
                    }
                    if(iEvent == GE_CREATE_FELLOWSHIP)
                    {
                    	CreateorJoinFellowLandscape(e);
                    }
                    if(iEvent == GE_IDENTIFY_OBJECT)
                    {
                    	 OnIdentLandscape(e.Message);
                    }
                    
            	}
            }
            catch (Exception ex){LogError(ex);}
        }  
        
        private void OnIdentLandscape(Decal.Adapter.Message pMsg)
		{
			try
			{
				if(!bLandscapeHudEnabled) {return;}
				
        		int PossibleLandscapeID = Convert.ToInt32(pMsg["object"]);	
        		if(Core.WorldFilter[PossibleLandscapeID].Container != 0 || LandscapeExclusionList.Contains(PossibleLandscapeID)) {return;}
        		else{CheckLandscape(new IdentifiedObject(Core.WorldFilter[PossibleLandscapeID]));}
				
			} 
			catch (Exception ex) {LogError(ex);}
		}
        
        private void CheckLandscape(IdentifiedObject IOLandscape)
		{
			try
			{
				switch(IOLandscape.ObjectClass)
				{
					case ObjectClass.Corpse:
						return;						
						
					case ObjectClass.Monster:
						if(gsSettings.bShowSelectedMobs){MonsterListCheckLandscape(ref IOLandscape);}
						if(gsSettings.bShowAllMobs && IOLandscape.IOR != IOResult.monster){IOLandscape.IOR = IOResult.allmonster;}
						goto default;
						
					case ObjectClass.Player:
						if(IOLandscape.Id == Core.CharacterFilter.Id) {return;}
						if(gsSettings.bShowFellowPlayers)
						{
							if(LandscapeFellowMemberTrackingList.Contains(IOLandscape.Name))
							{
								IOLandscape.IOR = IOResult.fellowplayer;
								goto default;									
							}
						}
						if (gsSettings.bShowAllegancePlayers && Core.CharacterFilter.Monarch != null && Core.CharacterFilter.Monarch.Id != 0) 
						{
							if (Core.CharacterFilter.Monarch.Id == IOLandscape.IntValues(LongValueKey.Monarch)) 
							{
								IOLandscape.IOR = IOResult.allegplayers;
								goto default;
							}
						}
						if (gsSettings.bShowAllPlayers && IOLandscape.Id != Core.CharacterFilter.Id) 
						{
							IOLandscape.IOR = IOResult.players;
							goto default;
						}				
						IOLandscape.IOR = IOResult.nomatch;
						goto default;
	
					case ObjectClass.Portal: 
						if (gsSettings.bShowAllPortals) 
						{						
							if(IOLandscape.Name.Contains("House Portal"))
							{
								IOLandscape.IOR = IOResult.nomatch;
								goto default;
							}
							else
							{
								IOLandscape.IOR = IOResult.portal;
								goto default;
							}
						}									
						IOLandscape.IOR = IOResult.nomatch;							
						goto default;
						
					case ObjectClass.Lifestone:
						if(gsSettings.bShowLifeStones)
						{
							IOLandscape.IOR = IOResult.lifestone;
							goto default;
						}
						IOLandscape.IOR = IOResult.nomatch;
						goto default;
						
					case ObjectClass.Npc:
						if(gsSettings.bShowTrophies)
						{
							TrophyListCheckLandscape(ref IOLandscape);
							if(IOLandscape.IOR == IOResult.npc)
							{
								goto default;
							}
						}
						if(gsSettings.bShowAllNPCs)
						{
							IOLandscape.IOR = IOResult.allnpcs;
							goto default;
						}
						IOLandscape.IOR = IOResult.nomatch;
						goto default; 						
					default:
						if(gsSettings.bShowTrophies && IOLandscape.IOR == IOResult.unknown){TrophyListCheckLandscape(ref IOLandscape);}
						if(IOLandscape.IOR ==IOResult.nomatch || IOLandscape.IOR == IOResult.unknown) {return;}
						break;
				}
				
				if(LandscapeTrackingList.FindIndex(x => x.Id == IOLandscape.Id) < 1)
				{
					LandscapeTrackingList.Add(IOLandscape);
					UpdateLandscapeHud();
				}
				if(!LandscapeExclusionList.Contains(IOLandscape.Id)){LandscapeExclusionList.Add(IOLandscape.Id);}
				return;
			}
			catch (Exception ex){LogError(ex);}
			return;
		}
			
		private void MonsterListCheckLandscape(ref IdentifiedObject IOLandscape)
		{
			try 
			{		
				string namecheck = IOLandscape.Name;
				var matches = from XMobs in mSortedMobsListChecked
					where (namecheck.ToLower().Contains((string)XMobs.Element("key").Value.ToLower()) && !Convert.ToBoolean(XMobs.Element("isexact").Value)) ||
					(namecheck == (string)XMobs.Element("key").Value && Convert.ToBoolean(XMobs.Element("isexact").Value))
							  select XMobs;
				
				if(matches.Count() > 0)
				{
					IOLandscape.IOR = IOResult.monster;
				}
				else
				{
					IOLandscape.IOR = IOResult.nomatch;
				}
			} catch (Exception ex) {LogError(ex);} 
			return;
		}
		
		private void TrophyListCheckLandscape(ref IdentifiedObject IOLandscape)
		{	
			try
			{
				string namecheck = IOLandscape.Name;
				var matches = from XTrophies in mSortedTrophiesListChecked
					where (namecheck.ToLower().Contains((string)XTrophies.Element("key").Value.ToLower()) && !Convert.ToBoolean(XTrophies.Element("isexact").Value)) ||
					(namecheck == (string)XTrophies.Element("key").Value && Convert.ToBoolean(XTrophies.Element("isexact").Value))
							  select XTrophies;
				
				if(matches.Count() > 0)
				{
					if(IOLandscape.ObjectClass == ObjectClass.Npc)
					{
						IOLandscape.IOR = IOResult.npc;
					}
					else
					{
						IOLandscape.IOR = IOResult.trophy;
					}
				}
				else
				{
					IOLandscape.IOR = IOResult.nomatch;
				}
				
			} catch (Exception ex) {LogError(ex);}	
			return;
		}
        
		private void OnWorldFilterDeleteLandscape(object sender, Decal.Adapter.Wrappers.ReleaseObjectEventArgs e)
		{
			try 
			{
				LandscapeTrackingList.RemoveAll(x => x.Id == e.Released.Id);
				LandscapeExclusionList.RemoveAll(x => x == e.Released.Id);
				UpdateLandscapeHud();
			} 
			catch (Exception ex) {LogError(ex);}
		}
		
		private void OnLandscapeDestroyed(object sender, ItemDestroyedEventArgs e)
		{
			try
			{
				LandscapeTrackingList.RemoveAll(x => x.Id == e.ItemGuid);
				LandscapeExclusionList.RemoveAll(x => x == e.ItemGuid);
				UpdateLandscapeHud();
			} catch(Exception ex) {LogError(ex);}
			return;
		}
		
		private void ChangePortalModeLandscape(object sender, Decal.Adapter.Wrappers.ChangePortalModeEventArgs e)
		{
			try
			{
				if(!mCharacterLoginComplete) {return;}
				else
				{
					if(mLandscapeInPortalSpace) {mLandscapeInPortalSpace = false;}
					else{mLandscapeInPortalSpace = true;}
					
					if(mLandscapeInPortalSpace)
					{
						LandscapeTrackingList.Clear();					
						LandscapeExclusionList.Clear();
						UpdateLandscapeHud();
					}
				}
			}catch(Exception ex) {LogError(ex);}
			return;
		}
			
        private int LandscapeTimer = 0;
        private void LandscapeTimerTick(object sender, EventArgs e)
        {
        	try
        	{
	        	if(LandscapeTimer == 4)
	        	{	        		
	        		DistanceCheckLandscape();
	        		LandscapeTimer = 0;
	        	}
	        	LandscapeTimer++;
        	}catch(Exception ex) {LogError(ex);}
        	return;
        }
	        			
	    private void DistanceCheckLandscape()
	    {
     		try
	   		{	
     			foreach(IdentifiedObject spawn in LandscapeTrackingList)
		    	{
		    		spawn.DistanceAway = Core.WorldFilter.Distance(Core.CharacterFilter.Id, spawn.Id);
		    	}
     			
     			LandscapeTrackingList = LandscapeTrackingList.OrderBy(x => x.DistanceAway).ToList();
     			
     			
	     		var LTLpurge = from detectedstuff in LandscapeTrackingList
	     			where Core.WorldFilter.Distance(Core.CharacterFilter.Id, detectedstuff.Id) > 5 && !LandscapeFellowMemberTrackingList.Contains(detectedstuff.Name)
	     			select detectedstuff.Id;
	     		
	     		foreach(var item in LTLpurge)
	     		{
	     			LandscapeTrackingList.RemoveAll(x => x.Id == item);
	     		} 		
	     		UpdateLandscapeHud();
	     		
	     	}catch(Exception ex) {LogError(ex);}
     		return;
    	}
	    
	    private void AddFellowLandscape(NetworkMessageEventArgs e)
	    {
	    	try
	    	{
	    		int fellow = (int)e.Message.Struct("fellow")["fellow"];
	    		if(fellow != Core.CharacterFilter.Id)
	    		{
	    			if(!LandscapeFellowMemberTrackingList.Contains((string)e.Message.Struct("fellow")["name"]))
	    			{
	    				LandscapeFellowMemberTrackingList.Add((string)e.Message.Struct("fellow")["name"]);
	    			}
	    		}    
	    	}catch(Exception ex) {LogError(ex);}
	    	return;
	    }
	    
	   	private void RemoveFellowLandscape(NetworkMessageEventArgs e)
	    {
	    	try
	    	{
	    		int fellow = (int)e.Message.Value<int>("fellow");
	    		
	    		if(fellow == Core.CharacterFilter.Id)
	    		{
	    			LandscapeFellowMemberTrackingList.Clear();
	    		}
	    		else
	    		{
	    			LandscapeFellowMemberTrackingList.RemoveAll(x => x == Core.WorldFilter[fellow].Name);
	    		}	    
	    	} catch(Exception ex){LogError(ex);}
	    	return;

	    }
	   	
	   	private void ClearFellowLandscape(NetworkMessageEventArgs e)
	    {
	    	try
	    	{
	   			LandscapeFellowMemberTrackingList.Clear();	    
	    	} catch(Exception ex) {LogError(ex);}
	    	return;
	    }
	   	
	   	private void CreateorJoinFellowLandscape(NetworkMessageEventArgs e)
	   	{
	   		try
	   		{
	   			LandscapeFellowMemberTrackingList.Clear();
	   			var fellowmembers = e.Message.Struct("fellows");
	   			int fellowcount = (int)e.Message.Value<int>("fellowCount");
	   			for(int i = 0; i < fellowcount; i++)
	   			{
	   				var fellow = fellowmembers.Struct(i).Struct("fellow");
	   				LandscapeFellowMemberTrackingList.Add((string)fellow.Value<string>("name"));
	   			}
	   			
	   		}catch(Exception ex) {LogError(ex);}
	   		return;
	   	}
//	   	            case 0x02BE: // Create Or Join Fellowship
//               mFellowship.Clear();
//               fellowMembers = pMsg.get_Struct("fellows");
//               int fellowCount = (int)pMsg.get_Value("fellowCount");
//               for (int i = 0; i < fellowCount; i++) {
//                  fellow = fellowMembers.get_Struct(i).get_Struct("fellow");
//                  mFellowship[(int)fellow.get_Value("fellow")] = (string)fellow.get_Value("name");
//               }
//               break;
	    
				
	    private HudView LandscapeHudView = null;
		private HudFixedLayout LandscapeHudLayout = null;
		private HudTabView LandscapeHudTabView = null;
		private HudFixedLayout LandscapeHudTabLayout = null;
		private HudList LandscapeHudList = null;
		private HudList.HudListRowAccessor LandscapeHudListRow = null;
		private const int LandscapeRemoveCircle = 0x60011F8;
		
		private HudFixedLayout LandscapeHudSettings;
		private HudCheckBox ShowAllMobs;
		private HudCheckBox ShowSelectedMobs;
		private HudCheckBox ShowAllPlayers;
		private HudCheckBox ShowAllegancePlayers;
		private HudCheckBox ShowFellowPlayers;
		private HudCheckBox ShowTrophies;
		private HudCheckBox ShowLifeStones;
		private HudCheckBox ShowAllPortals;
		private HudCheckBox ShowAllNPCs;
		
		private HudStaticText txtLSS1;
		private HudStaticText txtLSS2;
					
    	private void RenderLandscapeHud()
    	{
    		try
    		{
    			GearSenseReadWriteSettings(true);
    	
    		}catch{}
    		
    		try
    		{
    			    			
    			if(LandscapeHudView != null)
    			{
    				DisposeLandscapeHud();
    			}			
    			
    			LandscapeHudView = new HudView("GearSense", 300, 220, new ACImage(0x6AA5));
    			LandscapeHudView.Theme = VirindiViewService.HudViewDrawStyle.GetThemeByName("Minimalist Transparent");
    			LandscapeHudView.UserAlphaChangeable = false;
    			LandscapeHudView.ShowInBar = false;
    			LandscapeHudView.UserResizeable = false;
    			LandscapeHudView.Visible = true;
    			LandscapeHudView.Ghosted = false;
                LandscapeHudView.UserMinimizable = false;
                LandscapeHudView.UserClickThroughable = false;
                LandscapeHudView.LoadUserSettings();
             
    			
    			LandscapeHudLayout = new HudFixedLayout();
    			LandscapeHudView.Controls.HeadControl = LandscapeHudLayout;
    			
    			LandscapeHudTabView = new HudTabView();
    			LandscapeHudLayout.AddControl(LandscapeHudTabView, new Rectangle(0,0,300,220));
    		
    			LandscapeHudTabLayout = new HudFixedLayout();
    			LandscapeHudTabView.AddTab(LandscapeHudTabLayout, "GearSense");
    			
    			LandscapeHudSettings = new HudFixedLayout();
    			LandscapeHudTabView.AddTab(LandscapeHudSettings, "Settings");
    			
    			LandscapeHudTabView.OpenTabChange += LandscapeHudTabView_OpenTabChange;
    			
    			RenderLandscapeTabLayout();
    			
    			SubscribeLandscapeEvents();
  							
    		}catch(Exception ex) {LogError(ex);}
    		return;
    	}
    	
    	
    	private void LandscapeHudTabView_OpenTabChange(object sender, System.EventArgs e)
    	{
    		try
    		{
    			switch(LandscapeHudTabView.CurrentTab)
    			{
    				case 0:
    					DisposeLandscapeSettingsLayout();
    					RenderLandscapeTabLayout();
    					GearSenseReadWriteSettings(false);
    					return;
    				case 1:
    					DisposeLandscapeTabLayout();
    					RenderLandscapeSettingsTabLayout();
    					break;
    			}
    			    			
    		}catch{}
    		
    	}
    	
    	   	
    	private void RenderLandscapeTabLayout()
    	{
    		try
    		{
    			LandscapeHudList = new HudList();
    			LandscapeHudTabLayout.AddControl(LandscapeHudList, new Rectangle(0,0,300,220));

				LandscapeHudList.ControlHeight = 16;	
				LandscapeHudList.AddColumn(typeof(HudPictureBox), 16, null);
				LandscapeHudList.AddColumn(typeof(HudStaticText), 230, null);
				LandscapeHudList.AddColumn(typeof(HudPictureBox), 16, null);
				
				LandscapeHudList.Click += (sender, row, col) => LandscapeHudList_Click(sender, row, col); 

				UpdateLandscapeHud();
    			
    		}catch{}
    	}
    	
    	private void DisposeLandscapeTabLayout()
    	{
    		try
    		{	
    			LandscapeHudList.Click -= (sender, row, col) => LandscapeHudList_Click(sender, row, col);   
    			LandscapeHudList.Dispose();
    						
    		}catch{}
    	}
    	
    	private void RenderLandscapeSettingsTabLayout()
    	{
    		try
    		{
    			ShowAllMobs = new HudCheckBox();
    			ShowAllMobs.Text = "Track All Mobs";
    			LandscapeHudSettings.AddControl(ShowAllMobs, new Rectangle(0,0,150,16));
    			ShowAllMobs.Checked = gsSettings.bShowAllMobs;
    			
    			ShowSelectedMobs = new HudCheckBox();
    			ShowSelectedMobs.Text = "Track Selected Mobs";
    			LandscapeHudSettings.AddControl(ShowSelectedMobs, new Rectangle(0,18,150,16));
    			ShowSelectedMobs.Checked = gsSettings.bShowSelectedMobs;
    			
    			ShowAllPlayers = new HudCheckBox();
    			ShowAllPlayers.Text = "Track All Players";
    			LandscapeHudSettings.AddControl(ShowAllPlayers, new Rectangle(0,36,150,16));
    			ShowAllPlayers.Checked = gsSettings.bShowAllPlayers;
    			
    			ShowAllegancePlayers = new HudCheckBox();
    			ShowAllegancePlayers.Text = "Track Allegiance Players";
    			LandscapeHudSettings.AddControl(ShowAllegancePlayers, new Rectangle(0,54,150,16));
    			ShowAllegancePlayers.Checked = gsSettings.bShowAllegancePlayers;
    			
    			ShowFellowPlayers = new HudCheckBox();
    			ShowFellowPlayers.Text = "Track Fellowship Players";
    			LandscapeHudSettings.AddControl(ShowFellowPlayers, new Rectangle(0,72,150,16));
    			ShowFellowPlayers.Checked = gsSettings.bShowFellowPlayers;
    			    			
    			ShowAllNPCs= new HudCheckBox();
    			ShowAllNPCs.Text = "Track All NPCs";
    			LandscapeHudSettings.AddControl(ShowAllNPCs, new Rectangle(0,90,150,16));
    			ShowAllNPCs.Checked = gsSettings.bShowAllNPCs;
    			
    			ShowTrophies = new HudCheckBox();
    			ShowTrophies.Text = "Track Selected NPCs and Trophies";
    			LandscapeHudSettings.AddControl(ShowTrophies, new Rectangle(0,108,150,16));
    			ShowTrophies.Checked = gsSettings.bShowTrophies;
    				
    			ShowLifeStones = new HudCheckBox();
    			ShowLifeStones.Text = "Track Lifestones";
    			LandscapeHudSettings.AddControl(ShowLifeStones, new Rectangle(0,126,150,16));
    			ShowLifeStones.Checked = gsSettings.bShowLifeStones;
    			
    			ShowAllPortals= new HudCheckBox();
    			ShowAllPortals.Text = "Track Portals";
    			LandscapeHudSettings.AddControl(ShowAllPortals, new Rectangle(0,144,150,16));
    			ShowAllPortals.Checked = gsSettings.bShowAllPortals;
    			
    			txtLSS1 = new HudStaticText();
    			txtLSS1.Text = "Player tracking funtions do not request player IDs.";
    			txtLSS2 = new HudStaticText();
    			txtLSS2.Text = "Players will not track until ID'd another way.";		
    			LandscapeHudSettings.AddControl(txtLSS1, new Rectangle(0,162,300,16));
    			LandscapeHudSettings.AddControl(txtLSS2, new Rectangle(0,180,300,16));
    			
    			ShowAllMobs.Change += ShowAllMobs_Change;
    			ShowSelectedMobs.Change += ShowSelectedMobs_Change;
    			ShowAllPlayers.Change += ShowAllPlayers_Change;
    			ShowFellowPlayers.Change += ShowFellowPlayers_Change;
    			ShowAllegancePlayers.Change += ShowAllegancePlayers_Change;
    			ShowAllNPCs.Change += ShowAllNPCs_Change;
    			ShowTrophies.Change += ShowTrophies_Change;
    			ShowLifeStones.Change += ShowLifeStones_Change;
    			ShowAllPortals.Change += ShowAllPortals_Change;
    		}catch{}
    	}
    	
    	
    	private void DisposeLandscapeSettingsLayout()
    	{
    		try
    		{
    			
    			ShowAllMobs.Change -= ShowAllMobs_Change;
    			ShowSelectedMobs.Change -= ShowSelectedMobs_Change;
    			ShowAllPlayers.Change -= ShowAllPlayers_Change;
    			ShowFellowPlayers.Change -= ShowFellowPlayers_Change;
    			ShowAllegancePlayers.Change -= ShowAllegancePlayers_Change;
    			ShowAllNPCs.Change -= ShowAllNPCs_Change;
    			ShowTrophies.Change -= ShowTrophies_Change;
    			ShowLifeStones.Change -= ShowLifeStones_Change;
    			ShowAllPortals.Change -= ShowAllPortals_Change;
    			
    			txtLSS2.Dispose();
    			txtLSS1.Dispose();
    			ShowAllPortals.Dispose();
    			ShowLifeStones.Dispose();
    			ShowTrophies.Dispose();
    			ShowAllNPCs.Dispose();
    			ShowFellowPlayers.Dispose();
    			ShowAllegancePlayers.Dispose();
    			ShowAllPlayers.Dispose();
    			ShowSelectedMobs.Dispose();
    			ShowAllMobs.Dispose();			
    		}catch{}
    	}
    
    	private void ShowAllMobs_Change(object sender, System.EventArgs e)
    	{
    		try
    		{
    			gsSettings.bShowAllMobs = ShowAllMobs.Checked;
    		}catch{}
    	}

    	private void ShowSelectedMobs_Change(object sender, System.EventArgs e)
    	{
    		try
    		{
    			gsSettings.bShowSelectedMobs = ShowSelectedMobs.Checked;
    		}catch{}
    	}
    	
    	private void ShowAllPlayers_Change(object sender, System.EventArgs e)
    	{
    		try
    		{
    			gsSettings.bShowAllPlayers = ShowAllPlayers.Checked;
    		}catch{}
    	}
    	
    	private void ShowFellowPlayers_Change(object sender, System.EventArgs e)
    	{
    		try
    		{
    			gsSettings.bShowFellowPlayers = ShowFellowPlayers.Checked;
    		}catch{}
    	}
    	private void ShowAllNPCs_Change(object sender, System.EventArgs e)
    	{
    		try
    		{
    			gsSettings.bShowAllNPCs = ShowAllNPCs.Checked;	
    		}catch{}
    	}
    	
    	private void ShowAllegancePlayers_Change(object sender, System.EventArgs e)
    	{
    		try
    		{
    			gsSettings.bShowAllegancePlayers = ShowAllegancePlayers.Checked;		
    		}catch{}
    	}
    	
    	private void ShowTrophies_Change(object sender, System.EventArgs e)
    	{
    		try
    		{
    			gsSettings.bShowTrophies = ShowTrophies.Checked;
    		}catch{}
    	}
    	    	
    	private void ShowLifeStones_Change(object sender, System.EventArgs e)
    	{
    		try
    		{
    			gsSettings.bShowLifeStones = ShowLifeStones.Checked;
    		}catch{}
    	}
    	    	    	
    	private void ShowAllPortals_Change(object sender, System.EventArgs e)
    	{
    		try
    		{
    			gsSettings.bShowAllPortals = ShowAllPortals.Checked;
    		}catch{}
    	}
   
    	private void DisposeLandscapeHud()
    	{
    			
    		try
    		{
    			UnsubscribeLandscapeEvents();
    			try{DisposeLandscapeTabLayout();}catch{}
    			try{DisposeLandscapeSettingsLayout();}catch{}
    			
    			LandscapeHudSettings.Dispose();
    			LandscapeHudLayout.Dispose();
    			LandscapeHudTabLayout.Dispose();
    			LandscapeHudTabView.Dispose();
    			LandscapeHudView.Dispose();		
    		}catch(Exception ex) {LogError(ex);}
    		return;
    	}
    		
    	private void LandscapeHudList_Click(object sender, int row, int col)
    	{
    		try
			{
    			if(col == 0)
    			{
					Host.Actions.UseItem(LandscapeTrackingList[row].Id, 0);
    			}
    			if(col == 1)
    			{
    				Host.Actions.SelectItem(LandscapeTrackingList[row].Id);
    				int textcolor;
    				switch(LandscapeTrackingList[row].IOR)
    				{
    					case IOResult.lifestone:
    						textcolor = 13;
    						break;
    					case IOResult.monster:
    						textcolor = 6;
    						break;
    					case IOResult.allegplayers:
    						textcolor = 13;
    						break;
    					case IOResult.npc:
    						textcolor = 3;
    						break;
    					default:
    						textcolor = 2;
    						break;
    				}
					HudToChat(LandscapeTrackingList[row].LinkString(), textcolor);	
    			}
    			if(col == 2)
    			{    				
    				LandscapeExclusionList.Add(LandscapeTrackingList[row].Id);
    				LandscapeTrackingList.RemoveAt(row);
    			}
				UpdateLandscapeHud();
			}
			catch (Exception ex) { LogError(ex); }
			return;			
    	}
    		
	    private void UpdateLandscapeHud()
	    {  	
	    	try
	    	{   
	    		if(LandscapeHudTabView.CurrentTab != 0) {return;}
	    		
	    		LandscapeHudList.ClearRows();	    		   	    		
	    	    foreach(IdentifiedObject spawn in LandscapeTrackingList)
	    	    {
	    	    	LandscapeHudListRow = LandscapeHudList.AddRow();
	    	    	
	    	    	((HudPictureBox)LandscapeHudListRow[0]).Image = spawn.Icon + 0x6000000;
	    	    	((HudStaticText)LandscapeHudListRow[1]).Text = spawn.IORString() + spawn.Name + spawn.DistanceString();
	    	    	if(spawn.IOR == IOResult.trophy) {((HudStaticText)LandscapeHudListRow[1]).TextColor = Color.Gold;}
	    	    	if(spawn.IOR == IOResult.lifestone) {((HudStaticText)LandscapeHudListRow[1]).TextColor = Color.MediumBlue;}
	    	    	if(spawn.IOR == IOResult.monster) {((HudStaticText)LandscapeHudListRow[1]).TextColor = Color.Orange;}
	    	    	if(spawn.IOR == IOResult.npc) {((HudStaticText)LandscapeHudListRow[1]).TextColor = Color.Yellow;}
	    	    	if(spawn.IOR == IOResult.portal)  {((HudStaticText)LandscapeHudListRow[1]).TextColor = Color.MediumPurple;}
	    	    	if(spawn.IOR == IOResult.players)  {((HudStaticText)LandscapeHudListRow[1]).TextColor = Color.AntiqueWhite;}
	    	    	if(spawn.IOR == IOResult.fellowplayer)  {((HudStaticText)LandscapeHudListRow[1]).TextColor = Color.Green;}
	    	    	if(spawn.IOR == IOResult.allegplayers)  {((HudStaticText)LandscapeHudListRow[1]).TextColor = Color.Tan;}
					((HudPictureBox)LandscapeHudListRow[2]).Image = LandscapeRemoveCircle;
	    	    }
	    	}catch(Exception ex){LogError(ex);}
	    	return;	    	
	    }
	}
}

        
