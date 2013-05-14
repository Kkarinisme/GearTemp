
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
	
		private List<DebuffSpell> DebuffSpellList = new List<DebuffSpell>();
		
		//For now, I am only going to track primary debuffs which are used in PvE combat.
		
		public class DebuffSpell
		{
			public int SpellId;
			public double SpellDuration;
			public DateTime CastTime;
			public string SecondsRemaining;
			public HudImageStack SpellIconStack;
		}
		
		private void SubscribeCombatEvents()
		{
			try
			{
				Core.CharacterFilter.SpellCast += CombatHud_SpellCast;
//				Core.WorldFilter.ReleaseObject += CombatHud_ReleaseObject;
//				Core.ItemDestroyed += CombatHud_ItemDestroyed;
				Core.ChatBoxMessage += CombatHud_ChatBoxMessage;
			}catch(Exception ex){LogError(ex);}
		}
		
		private void UnsubscribeCombatEvents()
		{
			try
			{
				Core.CharacterFilter.SpellCast -= CombatHud_SpellCast;
//				Core.WorldFilter.ReleaseObject -= CombatHud_ReleaseObject;
//				Core.ItemDestroyed -= CombatHud_ItemDestroyed;
				Core.ChatBoxMessage -= CombatHud_ChatBoxMessage;
			}catch(Exception ex){LogError(ex);}
		}
		
		
//		0x07 Magic Spell Results
		
		private void CombatHud_ChatBoxMessage(object sender, ChatTextInterceptEventArgs e)
		{
			try
			{
				if(e.Color == 0x07)
				{
					
				}
			}catch(Exception ex){LogError(ex);}
		}
		                                
		
		private void CombatHud_SpellCast(object sender, SpellCastEventArgs e)
		{
			try
			{
				if(SpellIndex[e.SpellId].isdebuff)
				{
					
				}
			}catch(Exception ex){LogError(ex);}
		}
		

//					
//    	private void RenderLandscapeHud()
//    	{
//    		try
//    		{
//    			GearSenseReadWriteSettings(true);
//    	
//    		}catch{}
//    		
//    		try
//    		{
//    			    			
//    			if(LandscapeHudView != null)
//    			{
//    				DisposeLandscapeHud();
//    			}			
//    			
//    			LandscapeHudView = new HudView("GearSense", 300, 220, new ACImage(0x6AA5));
//    			LandscapeHudView.Theme = VirindiViewService.HudViewDrawStyle.GetThemeByName("Minimalist Transparent");
//    			LandscapeHudView.UserAlphaChangeable = false;
//    			LandscapeHudView.ShowInBar = false;
//    			LandscapeHudView.UserResizeable = false;
//    			LandscapeHudView.Visible = true;
//    			LandscapeHudView.Ghosted = false;
//                LandscapeHudView.UserMinimizable = false;
//                LandscapeHudView.UserClickThroughable = false;
//                LandscapeHudView.LoadUserSettings();
//             
//    			
//    			LandscapeHudLayout = new HudFixedLayout();
//    			LandscapeHudView.Controls.HeadControl = LandscapeHudLayout;
//    			
//    			LandscapeHudTabView = new HudTabView();
//    			LandscapeHudLayout.AddControl(LandscapeHudTabView, new Rectangle(0,0,300,220));
//    		
//    			LandscapeHudTabLayout = new HudFixedLayout();
//    			LandscapeHudTabView.AddTab(LandscapeHudTabLayout, "GearSense");
//    			
//    			LandscapeHudSettings = new HudFixedLayout();
//    			LandscapeHudTabView.AddTab(LandscapeHudSettings, "Settings");
//    			
//    			LandscapeHudTabView.OpenTabChange += LandscapeHudTabView_OpenTabChange;
//    			
//    			RenderLandscapeTabLayout();
//    			
//    			SubscribeLandscapeEvents();
//  							
//    		}catch(Exception ex) {LogError(ex);}
//    		return;
//    	}

		private HudView CombatHudView = null;
		private HudFixedLayout CombatHudLayout = null;
		private HudTabView CombatHudTabView = null;
		private HudFixedLayout CombatHudMainTab = null;
		private HudImageStack CombatHudFocusTargetImage = null;
		private HudStaticText CombatHudFocusTargetName = null;
		private HudProgressBar CombatHudFocusTargetHealth = null;
		private HudProgressBar CombatHudFocusTargetStamina = null;
		private HudProgressBar CombatHudFocusTargetMana = null;
		private HudButton CombatHudFocusTargetSet = null;
		private HudButton CombatHudFocusTargetClear = null;
		
		private HudList CombatHudDebuffTrackerList = null;
		private HudList.HudListRowAccessor CombatHudRow = null;
		

//		
//		private HudFixedLayout LandscapeHudSettings;
//		private HudCheckBox ShowAllMobs;
//		private HudCheckBox ShowSelectedMobs;
//		private HudCheckBox ShowAllPlayers;
//		private HudCheckBox ShowAllegancePlayers;
//		private HudCheckBox ShowFellowPlayers;
//		private HudCheckBox ShowTrophies;
//		private HudCheckBox ShowLifeStones;
//		private HudCheckBox ShowAllPortals;
//		private HudCheckBox ShowAllNPCs;
//		
//		private HudStaticText txtLSS1;
//		private HudStaticText txtLSS2;
//		
//		private bool LandscapeMainTab;
//		private bool LandscapeSettingsTab;
		
		private void RenderCombatHud()
		{
			try
			{
				
			}catch(Exception ex){LogError(ex);}
		}
		
		private void DisposeCombatHud()
		{
			try
			{
				
			}catch(Exception ex){LogError(ex);}
		}
		
		private void RenderCombatHudMainTab()
		{
			try
			{
				
			}catch(Exception ex){LogError(ex);}
		}
		
		private void DisposeCombatHudMainTab()
		{
			try
			{
				
			}catch(Exception ex){LogError(ex);}
		}
		
		private void RenderCombatHudSettingsTab()
		{
			try
			{
				
			}catch(Exception ex){LogError(ex);}
		}
		
		private void DisposeCombatHudSettingsTab()
		{
			try
			{
				
			}catch(Exception ex){LogError(ex);}
		}
		
		
		
		
	}
}
