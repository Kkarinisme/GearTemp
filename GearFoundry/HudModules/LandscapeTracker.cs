
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

namespace GearFoundry
{
	public partial class PluginCore
	{
		private List<int> LandscapeExclusionList = new List<int>();
		private List<IdentifiedObject> LandscapeTrackingList = new List<IdentifiedObject>();
		private List<string> LandscapeFellowNames = new List<string>();
		private bool mLandscapeInPortalSpace = true;
		
		void SubscribeLandscapeEvents()
		{
			try
			{
				MasterTimer.Tick += LandscapeTimerTick;
				Core.WorldFilter.CreateObject += new EventHandler<CreateObjectEventArgs>(OnWorldFilterCreateLandscape);
             	Core.EchoFilter.ServerDispatch += new EventHandler<NetworkMessageEventArgs>(ServerDispatchLandscape);
                Core.WorldFilter.ReleaseObject += new EventHandler<ReleaseObjectEventArgs>(OnWorldFilterDeleteLandscape);
                Core.ItemDestroyed += new EventHandler<ItemDestroyedEventArgs>(OnLandscapeDestroyed);
                Core.CharacterFilter.ChangePortalMode += new EventHandler<ChangePortalModeEventArgs>(ChangePortalModeLandscape);
			}
			catch{}
		}
		
		void UnsubscribeLandscapeEvents()
		{
			try
			{
				MasterTimer.Tick -= LandscapeTimerTick;
				Core.WorldFilter.CreateObject -= new EventHandler<CreateObjectEventArgs>(OnWorldFilterCreateLandscape);
             	Core.EchoFilter.ServerDispatch -= new EventHandler<NetworkMessageEventArgs>(ServerDispatchLandscape);
                Core.WorldFilter.ReleaseObject -= new EventHandler<ReleaseObjectEventArgs>(OnWorldFilterDeleteLandscape);
                Core.ItemDestroyed -= new EventHandler<ItemDestroyedEventArgs>(OnLandscapeDestroyed);
                Core.CharacterFilter.ChangePortalMode -= new EventHandler<ChangePortalModeEventArgs>(ChangePortalModeLandscape);
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
                    if(iEvent == GE_CREATE_FELLOW)
                    {
                    	CreateFellowshipLandscape(e.Message);
                    }
                    if(iEvent == GE_IDENTIFY_OBJECT)
                    {
                    	 OnIdentLandscape(e.Message);
                    }
                    
            	}
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
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
        
        void CheckLandscape(IdentifiedObject IOLandscape)
		{
			try
			{
				switch(IOLandscape.ObjectClass)
				{
					case ObjectClass.Corpse:
						return;						
						
					case ObjectClass.Monster:
						if(bselectedMobsEnabled){MonsterListCheckLandscape(ref IOLandscape);}
						if(bShowAllMobs && IOLandscape.IOR != IOResult.monster){IOLandscape.IOR = IOResult.allmonster;}
						goto default;
						
					case ObjectClass.Player:
						if(IOLandscape.Id == Core.CharacterFilter.Id) {return;}
						if(bfellowEnabled)
						{
							if(LandscapeFellowNames.Contains(IOLandscape.Name))
							{
								IOLandscape.IOR = IOResult.fellowplayer;
								goto default;									
							}
						}
						if (ballegEnabled && Core.CharacterFilter.Monarch != null && Core.CharacterFilter.Monarch.Id != 0) 
						{
							if (Core.CharacterFilter.Monarch.Id == IOLandscape.IntValues(LongValueKey.Monarch)) 
							{
								IOLandscape.IOR = IOResult.allegplayers;
								goto default;
							}
						}
						if (ballPlayersEnabled && IOLandscape.Id != Core.CharacterFilter.Id) 
						{
							IOLandscape.IOR = IOResult.players;
							goto default;
						}				
						IOLandscape.IOR = IOResult.nomatch;
						goto default;
	
					case ObjectClass.Portal: 
						if (bportalsEnabled) 
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
						if(bLandscapeLifestonesEnabled)
						{
							IOLandscape.IOR = IOResult.lifestone;
							goto default;
						}
						IOLandscape.IOR = IOResult.nomatch;
						goto default;
						
					case ObjectClass.Npc:
						if(bLandscapeTrophiesEnabled)
						{
							TrophyListCheckLandscape(ref IOLandscape);
							if(IOLandscape.IOR == IOResult.npc)
							{
								goto default;
							}
						}
						if(bShowAllNPCs)
						{
							IOLandscape.IOR = IOResult.allnpcs;
							goto default;
						}
						IOLandscape.IOR = IOResult.nomatch;
						goto default; 						
					default:
						if(bLandscapeTrophiesEnabled && IOLandscape.IOR == IOResult.unknown){TrophyListCheckLandscape(ref IOLandscape);}
						if(IOLandscape.IOR ==IOResult.nomatch || IOLandscape.IOR == IOResult.unknown) {return;}
						break;
				}
				
				if(LandscapeTrackingList.FindIndex(x => x.Id == IOLandscape.Id) < 1)
				{
					LandscapeTrackingList.Add(IOLandscape);
					UpdateLandscapeHud();
				}
				LandscapeExclusionList.Add(IOLandscape.Id);
				return;
			}
			
			catch (Exception ex){LogError(ex);}
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
				} catch {}
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
				}
				catch{}
	
			}
			
	        int LandscapeTimer = 0;
	        void LandscapeTimerTick(object sender, EventArgs e)
	        {
	        	try
	        	{
		        	if(LandscapeTimer == 4)
		        	{	        		
		        		DistanceCheckLandscape();
		        		LandscapeTimer = 0;
		        	}
		        	LandscapeTimer++;
	        	}
	        	catch{}
	        }
			
	    private void DistanceCheckLandscape()
	    {
     		try
	   		{	
	     		var LTLpurge = from detectedstuff in LandscapeTrackingList
	     			where Core.WorldFilter.Distance(Core.CharacterFilter.Id, detectedstuff.Id) > 5
	     			select detectedstuff.Id;
	     		
	     		foreach(var item in LTLpurge)
	     		{
	     			LandscapeTrackingList.RemoveAll(x => x.Id == item);
	     		}
	     		UpdateLandscapeHud();
	     	}
	     	catch{}
    	}
	    
	    private void CreateFellowshipLandscape(Decal.Adapter.Message pMsg)
	    {
	    	try
	    	{
	    		
	    	
		    	LandscapeFellowNames.Clear();
		    	
		    	int FellowshipCount;
		    	
		    	FellowshipCount = Convert.ToInt32(pMsg.Value<string>("fellowCount"));
	 	
		    	Decal.Adapter.MessageStruct FellowshipData = pMsg.Struct("fellows");
		        	
		    	for(int i = 0; i < FellowshipCount; i++)
		    	{
		    		LandscapeFellowNames.Add(pMsg.Struct(i).Struct("fellow").Value<string>("name"));
		    	}
	    	} catch{}
//            Set vec = pMsg.Struct("fellows")
//            With pMsg
//               FCount = .value("fellowCount")
//               FLeaderID = .value("leader")
//               FName = .value("name")
//               FState = .value("open")
//               FXpShare = .value("shareXP")
//            End With
//            For i = 0 To FCount - 1
//               With vec.Struct(i).Struct("fellow")
//                  FMember(i).GUID = .value("fellow")
//                  FMember(i).Name = .value("name")
//                  FMember(i).Level = .value("level")
//                  FMember(i).Health = .value("maxHealth")
//                  FMember(i).Stam = .value("maxStam")
//                  FMember(i).Mana = .value("maxMana")
//               End With
//            Next
	    }
				
	    private HudView LandscapeHudView = null;
		private HudFixedLayout LandscapeHudLayout = null;
		private HudTabView LandscapeHudTabView = null;
		private HudFixedLayout LandscapeHudTabLayout = null;
		private HudList LandscapeHudList = null;
		private HudList.HudListRowAccessor LandscapeHudListRow = null;
		
		//Assembly tests
		
		private const int LandscapeRemoveCircle = 0x60011F8;
			
    	private void RenderLandscapeHud()
    	{
    		try{
    			    			
    			if(LandscapeHudView != null)
    			{
    				DisposeLandscapeHud();
    			}			
    			
    			LandscapeHudView = new HudView("GearSense", 300, 220, new ACImage(0x1F88));
    			LandscapeHudView.Theme = VirindiViewService.HudViewDrawStyle.GetThemeByName("Minimalist Transparent");
    			LandscapeHudView.UserAlphaChangeable = false;
    			LandscapeHudView.ShowInBar = false;
    			LandscapeHudView.UserResizeable = false;
    			LandscapeHudView.Visible = true;
    			LandscapeHudView.Ghosted = false;
                LandscapeHudView.UserMinimizable = false;
                LandscapeHudView.UserClickThroughable = true;
    			
    			LandscapeHudLayout = new HudFixedLayout();
    			LandscapeHudView.Controls.HeadControl = LandscapeHudLayout;
    			
    			LandscapeHudTabView = new HudTabView();
    			LandscapeHudLayout.AddControl(LandscapeHudTabView, new Rectangle(0,0,300,220));
    		
    			LandscapeHudTabLayout = new HudFixedLayout();
    			LandscapeHudTabView.AddTab(LandscapeHudTabLayout, "Landscape");
    			
    			LandscapeHudList = new HudList();
    			LandscapeHudTabLayout.AddControl(LandscapeHudList, new Rectangle(0,0,300,220));
				LandscapeHudList.ControlHeight = 16;	
				LandscapeHudList.AddColumn(typeof(HudPictureBox), 16, null);
				LandscapeHudList.AddColumn(typeof(HudStaticText), 230, null);
				LandscapeHudList.AddColumn(typeof(HudPictureBox), 16, null);
				
				LandscapeHudList.Click += (sender, row, col) => LandscapeHudList_Click(sender, row, col);				
			  							
    		}catch(Exception ex) {WriteToChat(ex.ToString());}
    		
    	}
    	
    	void DisposeLandscapeHud()
    	{
    			
    		try
    		{
    			LandscapeHudList.Click -= (sender, row, col) => LandscapeHudList_Click(sender, row, col);		
    			LandscapeHudList.Dispose();
    			LandscapeHudLayout.Dispose();
    			LandscapeHudView.Dispose();		
    		}	
    		catch{}
    	}
    		
    	void LandscapeHudList_Click(object sender, int row, int col)
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
    	}
    		
	    private void UpdateLandscapeHud()
	    {  	
	    	try
	    	{    		
	    		LandscapeHudList.ClearRows();
	    		   	    		
	    	    foreach(IdentifiedObject spawn in LandscapeTrackingList)
	    	    {
	    	    	LandscapeHudListRow = LandscapeHudList.AddRow();
	    	    	
	    	    	((HudPictureBox)LandscapeHudListRow[0]).Image = spawn.Icon + 0x6000000;
	    	    	((HudStaticText)LandscapeHudListRow[1]).Text = spawn.IORString() + spawn.Name;
	    	    	if(spawn.IOR == IOResult.trophy) {((HudStaticText)LandscapeHudListRow[1]).TextColor = Color.SlateGray;}
	    	    	if(spawn.IOR == IOResult.lifestone) {((HudStaticText)LandscapeHudListRow[1]).TextColor = Color.Blue;}
	    	    	if(spawn.IOR == IOResult.monster) {((HudStaticText)LandscapeHudListRow[1]).TextColor = Color.Orange;}
	    	    	if(spawn.IOR == IOResult.npc) {((HudStaticText)LandscapeHudListRow[1]).TextColor = Color.Yellow;}
	    	    	if(spawn.IOR == IOResult.portal)  {((HudStaticText)LandscapeHudListRow[1]).TextColor = Color.Purple;}
	    	    	if(spawn.IOR == IOResult.players)  {((HudStaticText)LandscapeHudListRow[1]).TextColor = Color.AntiqueWhite;}
	    	    	if(spawn.IOR == IOResult.fellowplayer)  {((HudStaticText)LandscapeHudListRow[1]).TextColor = Color.Green;}
	    	    	if(spawn.IOR == IOResult.allegplayers)  {((HudStaticText)LandscapeHudListRow[1]).TextColor = Color.DarkSlateBlue;}
					((HudPictureBox)LandscapeHudListRow[2]).Image = LandscapeRemoveCircle;
	    	    }
	       	}catch(Exception ex){WriteToChat(ex.ToString());}
	    	
	    }
	}
}

        
//            Case Fellow_Create '&H2BE&
//            Set vec = pMsg.Struct("fellows")
//            With pMsg
//               FCount = .value("fellowCount")
//               FLeaderID = .value("leader")
//               FName = .value("name")
//               FState = .value("open")
//               FXpShare = .value("shareXP")
//            End With
//            For i = 0 To FCount - 1
//               With vec.Struct(i).Struct("fellow")
//                  FMember(i).GUID = .value("fellow")
//                  FMember(i).Name = .value("name")
//                  FMember(i).Level = .value("level")
//                  FMember(i).Health = .value("maxHealth")
//                  FMember(i).Stam = .value("maxStam")
//                  FMember(i).Mana = .value("maxMana")
//               End With
//            Next

