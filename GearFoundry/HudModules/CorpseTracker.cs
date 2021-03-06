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
using MyClasses.MetaViewWrappers.VirindiViewServiceHudControls;
using VirindiViewService.Themes;
using System.Xml.Serialization;
using System.Xml;

namespace GearFoundry
{

	public partial class PluginCore
	{
		//IRQUK:  06/24/2013.  I'm declaring this done unless bugs need to be squashed later.
		//Need to complete logging and verify functionality of DEADME
		//DeadMe Status:  Not really sure if this functionality matters.  Enabling it is under review.
		
		private List<LandscapeObject> CorpseTrackingList = new List<LandscapeObject>();
		private List<string> PermittedCorpsesList =  new List<string>(); 	
		public GearVisectionSettings ghSettings = new GearVisectionSettings();
		public DateTime LastCorpseHudUpdate = DateTime.Now;			
		
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
			public bool RenderMini = false;
			public List<MyCorpses> DeadMeList = new List<PluginCore.MyCorpses>();
            public int CorpseHudWidth = 300;
            public int CorpseHudHeight = 220;

    	}
		
		
		private void SubscribeCorpseEvents()
		{
			try
			{

				GearVisectionReadWriteSettings(true);
				
				MasterTimer.Tick += CorpseCheckerTick;
				Core.WorldFilter.CreateObject += OnWorldFilterCreateCorpse;
				Core.WorldFilter.ChangeObject += Corpse_ChangeObject;
                Core.WorldFilter.ReleaseObject += OnWorldFilterDeleteCorpse;
                Core.ItemDestroyed += OnCorpseDestroyed;
                Core.CharacterFilter.ChangePortalMode += ChangePortalModeCorpses;
                Core.CharacterFilter.Logoff += Corpse_Logoff;
                Core.ContainerOpened += CorpseTrackerContainerOpened;
                
                foreach(WorldObject wo in Core.WorldFilter.GetByObjectClass(ObjectClass.Corpse))
                {
					if(!CorpseTrackingList.Any(x => x.Id == wo.Id)){CheckCorpse(new LandscapeObject(wo));}
                }

			}
			catch(Exception ex){LogError(ex);}
		}
		
		private void Corpse_Logoff(object sender, EventArgs e)
		{
			try
			{
				UnsubscribeCorpseEvents();
				DisposeCorpseHud();
			}catch(Exception ex){LogError(ex);}
		}
		
		private void UnsubscribeCorpseEvents()
		{
			try
			{
				MasterTimer.Tick -= CorpseCheckerTick;
				Core.WorldFilter.CreateObject -= OnWorldFilterCreateCorpse;
             	Core.WorldFilter.ChangeObject -= Corpse_ChangeObject;
                Core.WorldFilter.ReleaseObject -= OnWorldFilterDeleteCorpse;
                Core.ItemDestroyed -= OnCorpseDestroyed;
                Core.CharacterFilter.ChangePortalMode -= ChangePortalModeCorpses;
				Core.CharacterFilter.Logoff -= Corpse_Logoff; 
 				Core.ContainerOpened -= CorpseTrackerContainerOpened;				

				CorpseTrackingList.Clear();
				PermittedCorpsesList.Clear();
                
                GearVisectionReadWriteSettings(false);
			}
			catch(Exception ex){LogError(ex);}
		}
		
		private void CorpseTrackerContainerOpened(object sender, ContainerOpenedEventArgs e)
		{
			try
			{
				int ctIndex = CorpseTrackingList.FindIndex(x => x.Id == e.ItemGuid);
				if(ctIndex >= 0)
				{
					CorpseTrackingList[ctIndex].notify = false;
					UpdateCorpseHud();
				}				
			}catch(Exception ex){LogError(ex);}
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
		                    	ghSettings = new GearVisectionSettings();
		                    	
		                    	using (XmlWriter writer = XmlWriter.Create(GearVisectionSettingsFile.ToString()))
								{
						   			XmlSerializer serializer2 = new XmlSerializer(typeof(GearVisectionSettings));
						   			serializer2.Serialize(writer, ghSettings);
						   			writer.Close();
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
		
		private void OnWorldFilterCreateCorpse(object sender, Decal.Adapter.Wrappers.CreateObjectEventArgs e)
		{
			try 
			{  	
				if(e.New.ObjectClass == ObjectClass.Corpse) 
				{
					if(!CorpseTrackingList.Any(x => x.Id == e.New.Id))
					{
						CheckCorpse(new LandscapeObject(e.New));
					}
				}
			} catch (Exception ex){LogError(ex);}
		}
		
        private void Corpse_ChangeObject(object sender, ChangeObjectEventArgs e)
        {
        	try
        	{
	        	if(e.Change != WorldChangeType.IdentReceived) {return;}
	        	if(e.Changed.ObjectClass == ObjectClass.Corpse)
	        	{
	        		if(!CorpseTrackingList.Any(x => x.Id == e.Changed.Id))
	        		{
	        			CheckCorpse(new LandscapeObject(e.Changed));
	        		}
	        	}
        	}
            catch (Exception ex){LogError(ex);}
        }  
        
        private void CheckCorpse(LandscapeObject IOCorpse)
		{
			try
			{	
				if(!IOCorpse.isvalid){return;}
				if(IOCorpse.Name.Contains(Core.CharacterFilter.Name) && ghSettings.bDeadMes)
				{
					IOCorpse.IOR = IOResult.corpseofself;
					if(!ghSettings.DeadMeList.Any(x => x.GUID == IOCorpse.Id))
					{
						MyCorpses DeadMe = new MyCorpses();
						DeadMe.Name = IOCorpse.Name;
						DeadMe.IconID = IOCorpse.Icon;
//						DeadMe.Coordinates = IOCorpse.Coordinates.ToString();
						DeadMe.GUID = IOCorpse.Id;
						
						ghSettings.DeadMeList.Add(DeadMe);
					}
					if(!CorpseTrackingList.Any(x => x.Id == IOCorpse.Id)) 
					{
						IOCorpse.notify = true;
						CorpseTrackingList.Add(IOCorpse);
						UpdateCorpseHud();
					}
					playSoundFromResource(mSoundsSettings.DeadMe);
					return;
				}
				
				//Flags corpes for recovery by that player as permitted
				if(ghSettings.Permitteds && PermittedCorpsesList.Contains(IOCorpse.Name))
				{
					IOCorpse.IOR = IOResult.corpsepermitted;
					if(!CorpseTrackingList.Any(x => x.Id == IOCorpse.Id)) 
					{
						IOCorpse.notify = true;
						CorpseTrackingList.Add(IOCorpse); 
						UpdateCorpseHud();
					}
					playSoundFromResource(mSoundsSettings.DeadPermitted);
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
								if(!CorpseTrackingList.Any(x => x.Id == IOCorpse.Id)) 
								{
									IOCorpse.notify = true;
									CorpseTrackingList.Add(IOCorpse); 
									UpdateCorpseHud();
								}
								playSoundFromResource(mSoundsSettings.CorpseRare);
								return;
							} 
							else 
							{
								IOCorpse.IOR = IOResult.corpseselfkill;
								if(!CorpseTrackingList.Any(x => x.Id == IOCorpse.Id)) 
								{
									IOCorpse.notify = true;
									CorpseTrackingList.Add(IOCorpse);
									UpdateCorpseHud();
								}
								playSoundFromResource(mSoundsSettings.CorpseSelfKill);
								return;
							}
						}
						else if(FellowMemberList.Count > 0 && ghSettings.bKillsByFellows)
						{
							if(FellowMemberList.Any(x => x.Looting && IOCorpse.SValue(StringValueKey.FullDescription).Contains(x.Name)))
							{
								IOCorpse.IOR = IOResult.corpsefellowkill;
								if(!CorpseTrackingList.Any(x => x.Id == IOCorpse.Id)) 
								{
									IOCorpse.notify = true;
									CorpseTrackingList.Add(IOCorpse);
									UpdateCorpseHud();
								}
								playSoundFromResource(mSoundsSettings.CorpseFellowKill);
								return;
							}
						}
					}
				}	
				
				if(ghSettings.bAllCorpses)
				{
					if(!CorpseTrackingList.Any(x => x.Id == IOCorpse.Id)) 
					{
						IOCorpse.IOR = IOResult.allcorpses;
						IOCorpse.notify = true;
						CorpseTrackingList.Add(IOCorpse);
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
				CorpseTrackingList.RemoveAll(x => x.Id == e.Released.Id);
				UpdateCorpseHud();
			} 
			catch (Exception ex) {LogError(ex);}
		}
		
		private void OnCorpseDestroyed(object sender, ItemDestroyedEventArgs e)
		{
			try
			{
				CorpseTrackingList.RemoveAll(x => x.Id == e.ItemGuid);
				UpdateCorpseHud();
			}catch(Exception ex){LogError(ex);}
		}
		
		private void ChangePortalModeCorpses(object sender, Decal.Adapter.Wrappers.ChangePortalModeEventArgs e)
		{
			try
			{
				CorpseTrackingList.Clear();
				foreach(WorldObject wo in Core.WorldFilter.GetByObjectClass(ObjectClass.Corpse))
                {
					if(!CorpseTrackingList.Any(x => x.Id == wo.Id)){CheckCorpse(new LandscapeObject(wo));}
                }
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
		private HudTabView CorpseHudTabView = null;
		private HudFixedLayout CorpseHudTabLayout = null;
		private HudFixedLayout CorpseHudSettingsTab = null;
		private HudList CorpseHudList = null;
		private HudCheckBox AllCorpses;
    	private HudCheckBox KillsBySelf;
    	private HudCheckBox KillsByFellows;
    	private HudCheckBox DeadMes;
    	private HudCheckBox Permitteds;
    	private HudCheckBox GVisRenderMini;
		private HudList.HudListRowAccessor CorpseHudListRow = null;
		
			
    	private void RenderCorpseHud()
    	{		
    		try
    		{
    			    			
    			if(CorpseHudView != null)
    			{
    				DisposeCorpseHud();
    			}
    			
                CorpseHudView = new HudView("GearVisection", ghSettings.CorpseHudWidth, ghSettings.CorpseHudHeight, new ACImage(0x6AA4));
    			CorpseHudView.UserAlphaChangeable = false;
    			CorpseHudView.ShowInBar = false;
    			if(ghSettings.RenderMini){CorpseHudView.UserResizeable = false;}
    			else{CorpseHudView.UserResizeable = true;}
    			CorpseHudView.Visible = true;
    			CorpseHudView.Ghosted = false;
    			CorpseHudView.UserClickThroughable = false;
                CorpseHudView.UserMinimizable = true;
                CorpseHudView.LoadUserSettings();
    			
    			CorpseHudTabView = new HudTabView();
    			CorpseHudView.Controls.HeadControl = CorpseHudTabView;
    		
    			CorpseHudTabLayout = new HudFixedLayout();
    			CorpseHudTabView.AddTab(CorpseHudTabLayout, "Corpses");
    			
    			CorpseHudList = new HudList();
    			CorpseHudTabLayout.AddControl(CorpseHudList, new Rectangle(0,0, ghSettings.CorpseHudWidth,ghSettings.CorpseHudHeight));
				CorpseHudList.ControlHeight = 16;	
				CorpseHudList.AddColumn(typeof(HudPictureBox), 16, null);
				CorpseHudList.AddColumn(typeof(HudStaticText), ghSettings.CorpseHudWidth - 60, null);
				CorpseHudList.AddColumn(typeof(HudPictureBox), 16, null);
				CorpseHudList.AddColumn(typeof(HudStaticText), 1, null);
				
				
    				
    			CorpseHudSettingsTab = new HudFixedLayout();
    			CorpseHudTabView.AddTab(CorpseHudSettingsTab, "Set");
    			
    			AllCorpses = new HudCheckBox();
    			AllCorpses.Text = "Trk All";
    			CorpseHudSettingsTab.AddControl(AllCorpses, new Rectangle(0,0,150,16));
    			AllCorpses.Checked = ghSettings.bAllCorpses;
    			
    			KillsBySelf = new HudCheckBox();
    			KillsBySelf.Text = "Trk Self";
    			CorpseHudSettingsTab.AddControl(KillsBySelf, new Rectangle(0,18,150,16));
    			KillsBySelf.Checked = ghSettings.bKillsBySelf;
    			
    			KillsByFellows = new HudCheckBox();
    			KillsByFellows.Text = "Trk Fellows";
    			CorpseHudSettingsTab.AddControl(KillsByFellows, new Rectangle(0,36,150,16));
    			KillsByFellows.Checked = ghSettings.bKillsByFellows;
    			
    			DeadMes = new HudCheckBox();
    			DeadMes.Text = "Trk DeadMe";
    			CorpseHudSettingsTab.AddControl(DeadMes, new Rectangle(0,54,150,16));
    			DeadMes.Checked = ghSettings.bDeadMes;
    			
    			Permitteds = new HudCheckBox();
    			Permitteds.Text = "Trk Permit";
    			CorpseHudSettingsTab.AddControl(Permitteds, new Rectangle(0,72,150,16));
    			Permitteds.Checked = ghSettings.Permitteds;
    			
    			GVisRenderMini = new HudCheckBox();
    			GVisRenderMini.Text = "R. Mini";
    			CorpseHudSettingsTab.AddControl(GVisRenderMini, new Rectangle(0,90,150,16));
    			GVisRenderMini.Checked = ghSettings.RenderMini;
    			
    			CorpseHudList.Click += CorpseHudList_Click;
    			GVisRenderMini.Change += GVisRenderMini_Change;    			
    			AllCorpses.Change += AllCorpses_Change;
    			KillsBySelf.Change += KillsBySelf_Change;
    			KillsByFellows.Change += KillsByFellows_Change;
    			DeadMes.Change += DeadMes_Change;
    			Permitteds.Change += Permitteds_Change;
    				
                CorpseHudView.Resize += CorpseHudView_Resize; 
               	CorpseHudView.VisibleChanged += CorpseHudView_VisibleChanged;
    			
	  							
    		}catch(Exception ex){LogError(ex);}
    		
    	}

        private void CorpseHudView_Resize(object sender, System.EventArgs e)
        {
            try
            {
	            ghSettings.CorpseHudWidth = CorpseHudView.Width;
	            ghSettings.CorpseHudHeight = CorpseHudView.Height;
	            
	            AlterCorpseTracker();
	            
	            GearVisectionReadWriteSettings(false);            
            }
            catch (Exception ex) { LogError(ex); }
        }
        
        private void CorpseHudView_VisibleChanged(object sender, EventArgs e)
        {
        	try
        	{
        		DisposeCorpseHud();
        	}catch(Exception ex){LogError(ex);}
        }
    	    	
    	
    	private void DisposeCorpseHud()
    	{
    			
    		try
    		{
    			if(CorpseHudView == null){return;}
    		
    			CorpseHudView.Resize -= CorpseHudView_Resize; 
               	CorpseHudView.VisibleChanged -= CorpseHudView_VisibleChanged;

    			CorpseHudList.Click -= CorpseHudList_Click;
    			AllCorpses.Change -= AllCorpses_Change;
    			KillsBySelf.Change -= KillsBySelf_Change;
    			KillsByFellows.Change -= KillsByFellows_Change;
    			DeadMes.Change -= DeadMes_Change;
    			Permitteds.Change -= Permitteds_Change;
    			GVisRenderMini.Change -= GVisRenderMini_Change;
				CorpseHudView.Resize -= CorpseHudView_Resize;     			
    			
    			GVisRenderMini.Dispose();
    			Permitteds.Dispose();
    			DeadMes.Dispose();
    			KillsByFellows.Dispose();
    			KillsBySelf.Dispose();
    			AllCorpses.Dispose();
    			
    			CorpseHudList.Dispose();

    			CorpseHudSettingsTab.Dispose();
    			CorpseHudTabLayout.Dispose();
                CorpseHudTabView.Dispose();
                CorpseHudView.Dispose();
                CorpseHudView = null;

				
    		}	
    		catch(Exception ex){LogError(ex);}
    	}
    	   	    	
    	
    	private void GVisRenderMini_Change(object sender, System.EventArgs e)
    	{
    		try
    		{
    			ghSettings.RenderMini = GVisRenderMini.Checked;
    			
    			if(ghSettings.RenderMini)
    			{
    				CorpseHudView.UserResizeable = false;
    				ghSettings.CorpseHudHeight = 220;
    				ghSettings.CorpseHudWidth = 80;
    			}
    			else
    			{
    				CorpseHudView.UserResizeable = true;
    				ghSettings.CorpseHudHeight = 220;
    				ghSettings.CorpseHudWidth = 300;
    			}
    			
    			CorpseHudView.Height = ghSettings.CorpseHudHeight;
    			CorpseHudView.Width = ghSettings.CorpseHudWidth;
    			
    			AlterCorpseTracker();
    			GearVisectionReadWriteSettings(false);
    			
    		}catch(Exception ex){LogError(ex);}
    	}
    	
    	private void AlterCorpseTracker()
    	{
    		try
    		{
    			CorpseHudList.Click -= CorpseHudList_Click;
    			
    			CorpseHudList.Dispose();
	            CorpseHudList = new HudList();
    			CorpseHudTabLayout.AddControl(CorpseHudList, new Rectangle(0,0, ghSettings.CorpseHudWidth,ghSettings.CorpseHudHeight));
				CorpseHudList.ControlHeight = 16;	
				CorpseHudList.AddColumn(typeof(HudPictureBox), 16, null);
				CorpseHudList.AddColumn(typeof(HudStaticText), ghSettings.CorpseHudWidth - 60, null);
				CorpseHudList.AddColumn(typeof(HudPictureBox), 16, null);
				CorpseHudList.AddColumn(typeof(HudStaticText), 1, null);
				
				CorpseHudList.Click += CorpseHudList_Click;
    			
    			UpdateCorpseHud();
    		}catch(Exception ex){LogError(ex);}
    	}
    	
    	private HudList.HudListRowAccessor CorpseRow = new HudList.HudListRowAccessor();
    	private void CorpseHudList_Click(object sender, int row, int col)
    	{
    		try
			{
    			
    			CorpseRow = CorpseHudList[row];
    			int woId = Convert.ToInt32(((HudStaticText)CorpseRow[3]).Text);
    			LandscapeObject co = CorpseTrackingList.Find(x => x.Id == woId);
    			
    			if(col == 0)
    			{
    				WriteToChat("loaded open container");
    				FoundryLoadAction(FoundryActionTypes.OpenContainer, co.Id);
    				InitiateFoundryActions();
    			}
    			if(col == 1)
    			{
    				Host.Actions.SelectItem(co.Id);
    				HudToChat(co.LinkString(), 2);
    				nusearrowid = co.Id;
                    ArrowInitiator();
    			}
    			if(col == 2)
    			{    				
  					co.notify = false;
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
	    		if(CorpseHudView == null) {return;}
	    		
	    		int scroll = CorpseHudList.ScrollPosition;
	    		
	    		CorpseHudList.ClearRows();
  	    		
	    	    foreach(LandscapeObject corpse in CorpseTrackingList)
	    	    {
	    	    	if(corpse.notify)
	    	    	{
		    	    	CorpseHudListRow = CorpseHudList.AddRow();
		    	    	((HudPictureBox)CorpseHudListRow[0]).Image = corpse.Icon + 0x6000000;
		    	    	if(ghSettings.RenderMini){((HudStaticText)CorpseHudListRow[1]).Text = corpse.MiniHudString();}
		    	    	else{((HudStaticText)CorpseHudListRow[1]).Text = corpse.HudString();}
	                    ((HudStaticText)CorpseHudListRow[1]).FontHeight = 10;
		    	    	if(corpse.IOR == IOResult.corpseselfkill) {((HudStaticText)CorpseHudListRow[1]).TextColor = Color.AntiqueWhite;}
		    	    	if(corpse.IOR == IOResult.corpsepermitted) {((HudStaticText)CorpseHudListRow[1]).TextColor = Color.Cyan;}
		    	    	if(corpse.IOR == IOResult.corpseofself) {((HudStaticText)CorpseHudListRow[1]).TextColor = Color.Yellow;}
		    	    	if(corpse.IOR == IOResult.corpsewithrare) {((HudStaticText)CorpseHudListRow[1]).TextColor = Color.Magenta;}
		    	    	if(corpse.IOR == IOResult.corpsefellowkill) {((HudStaticText)CorpseHudListRow[1]).TextColor = Color.Green;}
		    	    	if(corpse.IOR == IOResult.allcorpses) {((HudStaticText)CorpseHudListRow[1]).TextColor = Color.SlateGray;}
						((HudPictureBox)CorpseHudListRow[2]).Image = GearGraphics.RemoveCircle;
						((HudStaticText)CorpseHudListRow[3]).Text = corpse.Id.ToString();
	    	    	}
	    	    }
	    	    
	    	    CorpseHudList.ScrollPosition = scroll;
	    	}catch(Exception ex){LogError(ex);}
	    	
	    }	    

	}
}


