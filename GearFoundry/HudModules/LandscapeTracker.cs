
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
using System.Xml.Linq;

namespace GearFoundry
{
	public partial class PluginCore
	{		
		private List<LandscapeObject> LandscapeTrackingList = new List<PluginCore.LandscapeObject>();
		private System.Windows.Forms.Timer LandscapeTimer = new System.Windows.Forms.Timer();
		
		public GearSenseSettings gsSettings;
		
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
			public bool bRenderMini = false;
			public int LandscapeForgetDistance = 100;
            public int LandscapeHudWidth = 300;
            public int LandscapeHudHeight = 220;
		}
				
		private void GearSenseReadWriteSettings(bool read)
		{
			try
			{
				FileInfo GearSenseSettingsFile = new FileInfo(GearDir + @"\GearSense.xml");					
				if (read)
				{
					try
					{
						if (!GearSenseSettingsFile.Exists)
		                {
	                    	gsSettings = new GearSenseSettings();
	                    	using (XmlWriter writer = XmlWriter.Create(GearSenseSettingsFile.ToString()))
							{
					   			XmlSerializer serializer2 = new XmlSerializer(typeof(GearSenseSettings));
					   			serializer2.Serialize(writer, gsSettings);
					   			writer.Close();
							}
		                }
						
						using (XmlReader reader = XmlReader.Create(GearSenseSettingsFile.ToString()))
						{	
							XmlSerializer serializer = new XmlSerializer(typeof(GearSenseSettings));
							gsSettings = (GearSenseSettings)serializer.Deserialize(reader);
							reader.Close();
						}
					}catch{gsSettings = new GearSenseSettings();}
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
				LandscapeTimer.Interval = 5000;
				LandscapeTimer.Start();
				LandscapeTimer.Tick += LandscapeTimerTick;
				Core.WorldFilter.CreateObject += OnWorldFilterCreateLandscape;
                Core.WorldFilter.ReleaseObject += OnWorldFilterDeleteLandscape;
                Core.ItemDestroyed += OnLandscapeDestroyed;
                Core.WorldFilter.ChangeObject += ChangeObjectLandscape;      
				Core.CharacterFilter.Logoff += LandscapeLogoff;                
			}
			catch(Exception ex) {LogError(ex);}
			return;
		}
		
		private void UnsubscribeLandscapeEvents()
		{
			try
			{				
				LandscapeTimer.Stop();
				LandscapeTimer.Tick -= LandscapeTimerTick;
				Core.WorldFilter.CreateObject -= OnWorldFilterCreateLandscape;
                Core.WorldFilter.ReleaseObject -= OnWorldFilterDeleteLandscape;
                Core.ItemDestroyed -= OnLandscapeDestroyed;
                Core.WorldFilter.ChangeObject -= ChangeObjectLandscape;	
				Core.CharacterFilter.Logoff -= LandscapeLogoff;                  
			}catch(Exception ex) {LogError(ex);}
			return;
		}
		
		private void LandscapeLogoff(object sender, EventArgs e)
		{
			try
			{
				LandscapeTrackingList.Clear();
				UnsubscribeLandscapeEvents();
			}catch(Exception ex){LogError(ex);}
		}
		
		private void OnWorldFilterCreateLandscape(object sender, Decal.Adapter.Wrappers.CreateObjectEventArgs e)
		{
			try 
			{   
				if(e.New.ObjectClass == ObjectClass.Unknown || e.New.Container != 0) {return;}				
				if(!LandscapeTrackingList.Any(x => x.Id == e.New.Id))
				{
					LandscapeObject lo = new LandscapeObject(e.New);
					LandscapeTrackingList.Add(lo);
					CheckLandscape(e.New.Id);
				}
			} catch (Exception ex){LogError(ex);}
			return;
		}
		
		private void ChangeObjectLandscape(object sender, ChangeObjectEventArgs e)
		{
			try
			{
				if(e.Change != WorldChangeType.IdentReceived || e.Changed.Container != 0){return;}
				if(LandscapeTrackingList.Any(x => x.Id == e.Changed.Id))
				{
					CheckLandscape(e.Changed.Id);			
				}
			}catch(Exception ex){LogError(ex);}
		}
        
        private void CheckLandscape(int loId)
		{
			try
			{
				LandscapeObject IOLandscape = LandscapeTrackingList.Find(x => x.Id == loId);
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
						if(IOLandscape.HasIdData && gsSettings.bShowFellowPlayers)
						{
							if(FellowMemberList.Any(x => x.Id == IOLandscape.Id))
							{
								IOLandscape.IOR = IOResult.fellowplayer;
								goto default;									
							}
						}
						if (IOLandscape.HasIdData && gsSettings.bShowAllegancePlayers && Core.CharacterFilter.Monarch != null && Core.CharacterFilter.Monarch.Id != 0) 
						{
							if (Core.CharacterFilter.Monarch.Id == IOLandscape.LValue(LongValueKey.Monarch)) 
							{
								IOLandscape.IOR = IOResult.allegplayers;
								goto default;
							}
						}
						if (gsSettings.bShowAllPlayers) 
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
						IOLandscape.notify = true;
						playSoundFromResource(1);
						UpdateLandscapeHud();
						break;
				}
			}
			catch (Exception ex){LogError(ex);}
		}
			
		private void MonsterListCheckLandscape(ref LandscapeObject IOLandscape)
		{
			try 
			{		
				string namecheck = IOLandscape.Name;
				List<XElement> matches;
				
				var exactmobs = from XMobs in mSortedMobsList
					where XMobs.Element("checked").Value == "true" && 
					XMobs.Element("isexact").Value == "true"
					select XMobs;
				
				matches = (from exMobs in exactmobs
				           where (string)@exMobs.Element("key").Value == @namecheck
				           select exMobs).ToList();
				
				if(matches.Count() == 0)
				{
					var notexactmobs = from XMobs in mSortedMobsList
								where XMobs.Element("checked").Value == "true" && 
								XMobs.Element("isexact").Value == "false"
								select XMobs;
					
					matches = (from nxMobs in notexactmobs
						where @namecheck.ToLower().Contains((string)@nxMobs.Element("key").Value.ToLower())
						select nxMobs).ToList();
				}
				
				if(matches.Count() > 0)
				{
					IOLandscape.IOR = IOResult.monster;
				}
				else
				{
					IOLandscape.IOR = IOResult.nomatch;
				}
			} catch (Exception ex) {LogError(ex);} 
		}
		
		private void TrophyListCheckLandscape(ref LandscapeObject IOLandscape)
		{	
			try
			{
				string namecheck = IOLandscape.Name;
				List<XElement> matches;
				
				var exacttrophies = from XTrophies in mSortedTrophiesList
					where XTrophies.Element("checked").Value == "true" && 
					XTrophies.Element("isexact").Value == "true"
					select XTrophies;
				
				matches = (from exTrophies in exacttrophies
					where (string)@exTrophies.Element("key").Value == @namecheck
					select exTrophies).ToList();
				
				if(matches.Count() == 0)
				{
					var notexacttrophies = from XTrophies in mSortedTrophiesList
					where XTrophies.Element("checked").Value == "true" && 
					XTrophies.Element("isexact").Value == "false"
					select XTrophies;
					
					matches = (from nxTrophies in notexacttrophies
						where @namecheck.ToLower().Contains((string)@nxTrophies.Element("key").Value.ToLower())
						select nxTrophies).ToList();
				}
				
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
				if(LandscapeTrackingList.Any(x => x.Id == e.Released.Id))
				{
					LandscapeTrackingList.RemoveAll(x => x.Id == e.Released.Id);
					UpdateLandscapeHud();
				}
			} 
			catch (Exception ex) {LogError(ex);}
		}
		
		private void OnLandscapeDestroyed(object sender, ItemDestroyedEventArgs e)
		{
			try
			{
				if(LandscapeTrackingList.Any(x => x.Id == e.ItemGuid))
				{
					LandscapeTrackingList.RemoveAll(x => x.Id == e.ItemGuid);
					UpdateLandscapeHud();
				}
			} catch(Exception ex) {LogError(ex);}
			return;
		}
			
        private void LandscapeTimerTick(object sender, EventArgs e)
        {
        	try
        	{	
	        	DistanceCheckLandscape();
        	}catch(Exception ex) {LogError(ex);}
        	return;
        }
		
        private void DistanceCheckLandscape()
	    {
     		try
	   		{	
     			for(int i = LandscapeTrackingList.Count - 1; i >= 0 ; i--)
		    	{
	     			LandscapeTrackingList[i].DistanceAway = Core.WorldFilter.Distance(Core.CharacterFilter.Id, LandscapeTrackingList[i].Id);
	     			if(LandscapeTrackingList[i].DistanceAway > ((double)gsSettings.LandscapeForgetDistance/(double)100)) {LandscapeTrackingList[i].notify = false;}
		    	}
	     		UpdateLandscapeHud();
	     	}catch(Exception ex){LogError(ex);}
    	}
        

				
	    private HudView LandscapeHudView = null;
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
		private HudCheckBox LandscapeRenderMini;
		private HudStaticText ForgetLabel;
		private HudTextBox LandscapeForgetDistance;
		
		
		private bool LandscapeMainTab;
		private bool LandscapeSettingsTab;

					
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

                LandscapeHudView = new HudView("GearSense", gsSettings.LandscapeHudWidth, gsSettings.LandscapeHudHeight, new ACImage(0x6AA5));
    			LandscapeHudView.UserAlphaChangeable = false;
    			LandscapeHudView.ShowInBar = false;
    			LandscapeHudView.Visible = true;
    			LandscapeHudView.Ghosted = false;
                LandscapeHudView.UserMinimizable = true;
                LandscapeHudView.UserClickThroughable = false;
                LandscapeHudView.UserResizeable = true;
                LandscapeHudView.LoadUserSettings();
    			
    			LandscapeHudTabView = new HudTabView();
    			LandscapeHudView.Controls.HeadControl = LandscapeHudTabView;
    		
    			LandscapeHudTabLayout = new HudFixedLayout();
    			LandscapeHudTabView.AddTab(LandscapeHudTabLayout, "Sense");
    			
    			LandscapeHudSettings = new HudFixedLayout();
    			LandscapeHudTabView.AddTab(LandscapeHudSettings, "Set");
    			
    			LandscapeHudTabView.OpenTabChange += LandscapeHudTabView_OpenTabChange;
                LandscapeHudView.Resize += LandscapeHudView_Resize; 
    			
    			RenderLandscapeTabLayout();    			
    			SubscribeLandscapeEvents();
						
    		}catch(Exception ex) {LogError(ex);}
    	}

        private void LandscapeHudView_Resize(object sender, System.EventArgs e)
        {
            try
            {
            	gsSettings.LandscapeHudWidth = LandscapeHudView.Width;
            	gsSettings.LandscapeHudHeight = LandscapeHudView.Height;
            	GearSenseReadWriteSettings(false);
            }
            catch (Exception ex) { LogError(ex); }
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
    					UpdateLandscapeHud();
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
    			LandscapeHudTabLayout.AddControl(LandscapeHudList, new Rectangle(0,0, gsSettings.LandscapeHudWidth,gsSettings.LandscapeHudHeight));
				LandscapeHudList.ControlHeight = 16;	
				LandscapeHudList.AddColumn(typeof(HudPictureBox), 14, null);
				LandscapeHudList.AddColumn(typeof(HudStaticText), gsSettings.LandscapeHudWidth - 60, null);
				LandscapeHudList.AddColumn(typeof(HudPictureBox), 14, null);	
				LandscapeHudList.AddColumn(typeof(HudStaticText), 1, null);
				
				LandscapeHudList.Click += LandscapeHudList_Click; 

				UpdateLandscapeHud();
				
				LandscapeMainTab = true;
    			
    		}catch{}
    	}
    	
    	private void DisposeLandscapeTabLayout()
    	{
    		try
    		{	
    			if(!LandscapeMainTab) {return;}
    			
    			LandscapeHudList.Click -= LandscapeHudList_Click;   
    			LandscapeHudList.Dispose();
    			
    			LandscapeMainTab = false;
    						
    		}catch{}
    	}
    	
    	private void RenderLandscapeSettingsTabLayout()
    	{
    		try
    		{
    			ShowAllMobs = new HudCheckBox();
    			ShowAllMobs.Text = "Trk All Mobs";
    			LandscapeHudSettings.AddControl(ShowAllMobs, new Rectangle(0,0,150,16));
    			ShowAllMobs.Checked = gsSettings.bShowAllMobs;
    			
    			ShowSelectedMobs = new HudCheckBox();
    			ShowSelectedMobs.Text = "Trk Mob List";
    			LandscapeHudSettings.AddControl(ShowSelectedMobs, new Rectangle(0,18,150,16));
    			ShowSelectedMobs.Checked = gsSettings.bShowSelectedMobs;
    			
    			ShowAllPlayers = new HudCheckBox();
    			ShowAllPlayers.Text = "Trk All Players";
    			LandscapeHudSettings.AddControl(ShowAllPlayers, new Rectangle(0,36,150,16));
    			ShowAllPlayers.Checked = gsSettings.bShowAllPlayers;
    			
    			ShowAllegancePlayers = new HudCheckBox();
    			ShowAllegancePlayers.Text = "Trk Allegiance";
    			LandscapeHudSettings.AddControl(ShowAllegancePlayers, new Rectangle(0,54,150,16));
    			ShowAllegancePlayers.Checked = gsSettings.bShowAllegancePlayers;
    			
    			ShowFellowPlayers = new HudCheckBox();
    			ShowFellowPlayers.Text = "Trk Fellows";
    			LandscapeHudSettings.AddControl(ShowFellowPlayers, new Rectangle(0,72,150,16));
    			ShowFellowPlayers.Checked = gsSettings.bShowFellowPlayers;
    			    			
    			ShowAllNPCs= new HudCheckBox();
    			ShowAllNPCs.Text = "Trk All NPCs";
    			LandscapeHudSettings.AddControl(ShowAllNPCs, new Rectangle(0,90,150,16));
    			ShowAllNPCs.Checked = gsSettings.bShowAllNPCs;
    			
    			ShowTrophies = new HudCheckBox();
    			ShowTrophies.Text = "Trk Trophy/NPC";
    			LandscapeHudSettings.AddControl(ShowTrophies, new Rectangle(0,108,150,16));
    			ShowTrophies.Checked = gsSettings.bShowTrophies;
    				
    			ShowLifeStones = new HudCheckBox();
    			ShowLifeStones.Text = "Trk LifeStones";
    			LandscapeHudSettings.AddControl(ShowLifeStones, new Rectangle(0,126,150,16));
    			ShowLifeStones.Checked = gsSettings.bShowLifeStones;
    			
    			ShowAllPortals= new HudCheckBox();
    			ShowAllPortals.Text = "Trk Portals";
    			LandscapeHudSettings.AddControl(ShowAllPortals, new Rectangle(0,144,150,16));
    			ShowAllPortals.Checked = gsSettings.bShowAllPortals;
    			
    			LandscapeForgetDistance = new HudTextBox();
    			ForgetLabel = new HudStaticText();
    			ForgetLabel.Text = "Forget Dist.";
    			LandscapeForgetDistance.Text = gsSettings.LandscapeForgetDistance.ToString();
    			LandscapeHudSettings.AddControl(LandscapeForgetDistance, new Rectangle(0,162,45,16));
    			LandscapeHudSettings.AddControl(ForgetLabel, new Rectangle(50,162,150,16));
    		
				LandscapeRenderMini = new HudCheckBox();
    			LandscapeRenderMini.Text = "R. Mini.";
    			LandscapeHudSettings.AddControl(LandscapeRenderMini, new Rectangle(0,180,150,16));
    			LandscapeRenderMini.Checked = gsSettings.bRenderMini;
    			
    			ShowAllMobs.Change += ShowAllMobs_Change;
    			ShowSelectedMobs.Change += ShowSelectedMobs_Change;
    			ShowAllPlayers.Change += ShowAllPlayers_Change;
    			ShowFellowPlayers.Change += ShowFellowPlayers_Change;
    			ShowAllegancePlayers.Change += ShowAllegancePlayers_Change;
    			ShowAllNPCs.Change += ShowAllNPCs_Change;
    			ShowTrophies.Change += ShowTrophies_Change;
    			ShowLifeStones.Change += ShowLifeStones_Change;
    			ShowAllPortals.Change += ShowAllPortals_Change;
    			LandscapeForgetDistance.LostFocus += LandscapeForgetDistance_LostFocus;
    			LandscapeRenderMini.Change += LandscapeRenderMini_Change;
    			
    			LandscapeSettingsTab = true;
    		}catch{}
    	}
    	
    	
    	private void DisposeLandscapeSettingsLayout()
    	{
    		try
    		{
    			if(!LandscapeSettingsTab) {return;}
    			ShowAllMobs.Change -= ShowAllMobs_Change;
    			ShowSelectedMobs.Change -= ShowSelectedMobs_Change;
    			ShowAllPlayers.Change -= ShowAllPlayers_Change;
    			ShowFellowPlayers.Change -= ShowFellowPlayers_Change;
    			ShowAllegancePlayers.Change -= ShowAllegancePlayers_Change;
    			ShowAllNPCs.Change -= ShowAllNPCs_Change;
    			ShowTrophies.Change -= ShowTrophies_Change;
    			ShowLifeStones.Change -= ShowLifeStones_Change;
    			ShowAllPortals.Change -= ShowAllPortals_Change;
    			LandscapeForgetDistance.LostFocus -= LandscapeForgetDistance_LostFocus;
    			LandscapeRenderMini.Change -= LandscapeRenderMini_Change;
    			
    			LandscapeRenderMini.Dispose();
    			ForgetLabel.Dispose();
    			LandscapeForgetDistance.Dispose();
    			ShowAllPortals.Dispose();
    			ShowLifeStones.Dispose();
    			ShowTrophies.Dispose();
    			ShowAllNPCs.Dispose();
    			ShowFellowPlayers.Dispose();
    			ShowAllegancePlayers.Dispose();
    			ShowAllPlayers.Dispose();
    			ShowSelectedMobs.Dispose();
    			ShowAllMobs.Dispose();			
    			
    			LandscapeSettingsTab = false;
    		}catch{}
    	}
    	
    	private void LandscapeRenderMini_Change(object sender, System.EventArgs e)
    	{
    		try
    		{
    			gsSettings.bRenderMini = LandscapeRenderMini.Checked;
    			if(gsSettings.bRenderMini)
    			{
    				gsSettings.LandscapeHudHeight = 220;
    				gsSettings.LandscapeHudWidth = 100;
    			}
    			else
    			{
    				gsSettings.LandscapeHudHeight = 220;
    				gsSettings.LandscapeHudWidth = 300;
    			}
    			GearSenseReadWriteSettings(false);
    			RenderLandscapeHud();
    		}catch(Exception ex){LogError(ex);}
    	}
    
    	private void ShowAllMobs_Change(object sender, System.EventArgs e)
    	{
    		try
    		{
    			gsSettings.bShowAllMobs = ShowAllMobs.Checked;
    			GearSenseReadWriteSettings(false);
    		}catch{}
    	}

    	private void ShowSelectedMobs_Change(object sender, System.EventArgs e)
    	{
    		try
    		{
    			gsSettings.bShowSelectedMobs = ShowSelectedMobs.Checked;
    			GearSenseReadWriteSettings(false);
    		}catch{}
    	}
    	
    	private void ShowAllPlayers_Change(object sender, System.EventArgs e)
    	{
    		try
    		{
    			gsSettings.bShowAllPlayers = ShowAllPlayers.Checked;
    			GearSenseReadWriteSettings(false);
    		}catch{}
    	}
    	
    	private void ShowFellowPlayers_Change(object sender, System.EventArgs e)
    	{
    		try
    		{
    			gsSettings.bShowFellowPlayers = ShowFellowPlayers.Checked;
    			GearSenseReadWriteSettings(false);
    		}catch{}
    	}
    	private void ShowAllNPCs_Change(object sender, System.EventArgs e)
    	{
    		try
    		{
    			gsSettings.bShowAllNPCs = ShowAllNPCs.Checked;	
    			GearSenseReadWriteSettings(false);
    		}catch{}
    	}
    	
    	private void ShowAllegancePlayers_Change(object sender, System.EventArgs e)
    	{
    		try
    		{
    			gsSettings.bShowAllegancePlayers = ShowAllegancePlayers.Checked;
				GearSenseReadWriteSettings(false);    			
    		}catch{}
    	}
    	
    	private void ShowTrophies_Change(object sender, System.EventArgs e)
    	{
    		try
    		{
    			gsSettings.bShowTrophies = ShowTrophies.Checked;
    			GearSenseReadWriteSettings(false);
    		}catch{}
    	}
    	    	
    	private void ShowLifeStones_Change(object sender, System.EventArgs e)
    	{
    		try
    		{
    			gsSettings.bShowLifeStones = ShowLifeStones.Checked;
    			GearSenseReadWriteSettings(false);
    		}catch{}
    	}
    	    	    	
    	private void ShowAllPortals_Change(object sender, System.EventArgs e)
    	{
    		try
    		{
    			gsSettings.bShowAllPortals = ShowAllPortals.Checked;
    			GearSenseReadWriteSettings(false);
    		}catch{}
    	}
    	
    	private void LandscapeForgetDistance_LostFocus(object sender, EventArgs e)
    	{
    		try
    		{
    			gsSettings.LandscapeForgetDistance = Convert.ToInt32(LandscapeForgetDistance.Text);
    			GearSenseReadWriteSettings(false);
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
    			LandscapeHudTabLayout.Dispose();
    			LandscapeHudTabView.Dispose();
    			LandscapeHudView.Dispose();		
    		}catch(Exception ex) {LogError(ex);}
    		return;
    	}
    		
    	private HudList.HudListRowAccessor LandscapeRow = new HudList.HudListRowAccessor();
    	private void LandscapeHudList_Click(object sender, int row, int col)
    	{
    		try
			{
    			LandscapeRow = LandscapeHudList[row];
    			int woId = Convert.ToInt32(((HudStaticText)LandscapeRow[3]).Text);
    			LandscapeObject lo = LandscapeTrackingList.Find(x => x.Id == woId);
    			
    			if(col == 0)
    			{
    				Host.Actions.UseItem(woId, 0);
    			}
    			if(col == 1)
    			{
    				Host.Actions.SelectItem(woId);
   		    		int textcolor;
    				switch(lo.IOR)
    				{
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
					HudToChat(lo.LinkString(), textcolor);
                    nusearrowid = woId;
                    ArrowInitiator();
    			}
    			if(col == 2)
    			{   	
					lo.notify = false;
    			}
				UpdateLandscapeHud();
			}
			catch (Exception ex) { LogError(ex); }		
    	}
    		
	    private void UpdateLandscapeHud()
	    {  	
	    	try
	    	{       			    			    		
	    		if(!LandscapeMainTab) {return;}
	    		
	    		var HudUpdateList = from allitems in LandscapeTrackingList
	    					where allitems.notify
	    					orderby allitems.DistanceAway
	    					select allitems;
	    			    		
	    		LandscapeHudList.ClearRows();
	    		
	    	    foreach(LandscapeObject item in HudUpdateList)
	    	    {
	    	    	LandscapeHudListRow = LandscapeHudList.AddRow();
	    	    	
	    	    	((HudPictureBox)LandscapeHudListRow[0]).Image = item.Icon + 0x6000000;
	    	    	if(gsSettings.bRenderMini) {((HudStaticText)LandscapeHudListRow[1]).Text = item.MiniHudString();}
	    	    	else{((HudStaticText)LandscapeHudListRow[1]).Text = item.HudString();}
                    ((HudStaticText)LandscapeHudListRow[1]).FontHeight = 10;
	    	    	if(item.IOR == IOResult.trophy) {((HudStaticText)LandscapeHudListRow[1]).TextColor = Color.Gold;}
	    	    	if(item.IOR == IOResult.lifestone) {((HudStaticText)LandscapeHudListRow[1]).TextColor = Color.SkyBlue;}
	    	    	if(item.IOR == IOResult.monster) {((HudStaticText)LandscapeHudListRow[1]).TextColor = Color.Orange;}
	    	    	if(item.IOR == IOResult.npc) {((HudStaticText)LandscapeHudListRow[1]).TextColor = Color.Yellow;}
	    	    	if(item.IOR == IOResult.portal)  {((HudStaticText)LandscapeHudListRow[1]).TextColor = Color.MediumPurple;}
	    	    	if(item.IOR == IOResult.players)  {((HudStaticText)LandscapeHudListRow[1]).TextColor = Color.AntiqueWhite;}
	    	    	if(item.IOR == IOResult.fellowplayer)  {((HudStaticText)LandscapeHudListRow[1]).TextColor = Color.LightGreen;}
	    	    	if(item.IOR == IOResult.allegplayers)  {((HudStaticText)LandscapeHudListRow[1]).TextColor = Color.Tan;}
					((HudPictureBox)LandscapeHudListRow[2]).Image = LandscapeRemoveCircle;
					((HudStaticText)LandscapeHudListRow[3]).Text = item.Id.ToString();
	    	    }
	    	}catch(Exception ex){LogError(ex);}  	
	    }
	}
}

        

