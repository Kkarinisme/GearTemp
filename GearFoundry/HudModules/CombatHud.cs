
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
using System.Text.RegularExpressions;

namespace GearFoundry
{
	public partial class PluginCore
	{
	
		private List<IdentifiedObject> CombatHudMobTrackingList = new List<IdentifiedObject>();
		private IdentifiedObject CHTargetIO = null;
		private Queue<SpellCastInfo> SpellCastBuffer = new Queue<SpellCastInfo>();
		
		private bool bCombatHudEnabled = true;
		private bool bCombatHudMainTab = false;
		private bool bCombatHudSettingsTab = false;
		private bool bCombatHudInPortalSpace = true;
		private int CombatHudFocusTargetGUID = 0;
		
		//Enclose in class for saving.
		public bool bCombatHudTrackLifeDebuffs = true;
		public bool bCombatHudTrackCreatureDebuffs = true;
		public bool bCombatHudTrackItemDebuffs = true;
		public bool bCombatHudTrackVoidDebuffs = true;
		public int CombatHudRenderColumns = 10;
		public DateTime CombatHudLastUpdate;
		
		

		
		private List<Regex> CombatHudRegexEx;
		

		
		public class SpellCastInfo
		{
			public int SpellTargetId;
			public int SpellCastId;
		}
		

		
		
			
		private void SubscribeCombatEvents()
		{
			try
			{
				Core.CharacterFilter.SpellCast += CombatHud_SpellCast;
				Core.CharacterFilter.ActionComplete += CombatHud_ActionComplete;
				MasterTimer.Tick += CombatHud_OnTimerDo;
				Core.WorldFilter.ReleaseObject += CombatHud_ReleaseObject;
				Core.ItemDestroyed += CombatHud_ItemDestroyed;
				Core.ChatBoxMessage += new EventHandler<ChatTextInterceptEventArgs>(CombatHud_ChatBoxMessage);
				Core.EchoFilter.ServerDispatch += new EventHandler<NetworkMessageEventArgs>(ServerDispatchCombat);
				Core.WorldFilter.CreateObject += new EventHandler<CreateObjectEventArgs>(CombatHud_CreateObject);
				Core.CharacterFilter.ChangePortalMode += new EventHandler<ChangePortalModeEventArgs>(CombatHud_ChangePortalMode);
				Core.ItemSelected += new EventHandler<ItemSelectedEventArgs>(CombatHud_ItemSelected);
				FillCombatHudRegex();
			}catch(Exception ex){LogError(ex);}
		}
		
		private void FillCombatHudRegex()
		{
			try
			{
				
				CombatHudRegexEx = new List<Regex>();
				CombatHudRegexEx.Add(new Regex("^(?<targetname>.+) resists your spell!$"));
				CombatHudRegexEx.Add(new Regex("Target is out of range."));
				CombatHudRegexEx.Add(new Regex("Your spell fizzled."));
				CombatHudRegexEx.Add(new Regex("^(?<targetname>.+) has no appropriate targets equipped for this spell.$"));
				CombatHudRegexEx.Add(new Regex("You fail to affect (?<targetname>.+) because you are not a player killer!$"));			
				
			}catch(Exception ex){LogError(ex);}
		}
				
		
		private void UnsubscribeCombatEvents()
		{
			try
			{
				Core.CharacterFilter.SpellCast -= CombatHud_SpellCast;
				Core.CharacterFilter.ActionComplete -= CombatHud_ActionComplete;
				MasterTimer.Tick -= CombatHud_OnTimerDo;
				Core.WorldFilter.ReleaseObject -= CombatHud_ReleaseObject;
				Core.ItemDestroyed -= CombatHud_ItemDestroyed;
				Core.ChatBoxMessage -= new EventHandler<ChatTextInterceptEventArgs>(CombatHud_ChatBoxMessage);
				Core.EchoFilter.ServerDispatch -= new EventHandler<NetworkMessageEventArgs>(ServerDispatchCombat);
				Core.WorldFilter.CreateObject -= new EventHandler<CreateObjectEventArgs>(CombatHud_CreateObject);
				Core.CharacterFilter.ChangePortalMode -= new EventHandler<ChangePortalModeEventArgs>(CombatHud_ChangePortalMode);
				Core.ItemSelected -= new EventHandler<ItemSelectedEventArgs>(CombatHud_ItemSelected);
				
			}catch(Exception ex){LogError(ex);}
		}
		
		private void CombatHud_ItemSelected(object sender, ItemSelectedEventArgs e)
		{
			try
			{
				if(e == null) {return;}
				if(Core.WorldFilter[e.ItemGuid].ObjectClass == ObjectClass.Monster)
				{
					IdqueueAdd(e.ItemGuid);
					UpdateCombatHudMainTab();
				}
			}catch(Exception ex){LogError(ex);}
		}
		
		
		private void CombatHud_ItemDestroyed(object sender, ItemDestroyedEventArgs e)
		{
			try
			{
				if(e == null) {return;}
				CombatHudMobTrackingList.RemoveAll(x => !x.isvalid);
				if(CombatHudMobTrackingList.Any(x => x.Id == e.ItemGuid))
				{
				   	CombatHudMobTrackingList.RemoveAll(x => x.Id == e.ItemGuid);
				}
				
			}catch(Exception ex){LogError(ex);}
		}
		
		private void CombatHud_ChangePortalMode(object sender, ChangePortalModeEventArgs e)
		{
			try
			{
				if(bCombatHudInPortalSpace){bCombatHudInPortalSpace = false;}
				else{bCombatHudInPortalSpace = true;}
								
				CombatHudMobTrackingList.Clear();
				foreach(WorldObject wo in Core.WorldFilter.GetByObjectClass(ObjectClass.Monster))
				{
					CombatHudMobTrackingList.Add(new IdentifiedObject(wo));
				}
				
			}catch(Exception ex){LogError(ex);}
		}
		
		private void CombatHud_CreateObject(object sender, CreateObjectEventArgs e)
		{
			try
			{
				if(e == null) {return;}
				if(Core.WorldFilter[e.New.Id].ObjectClass == ObjectClass.Monster)
				{
					if(!CombatHudMobTrackingList.Any(x => x.Id == e.New.Id))
					{
						CombatHudMobTrackingList.Add(new IdentifiedObject(Core.WorldFilter[e.New.Id]));
					}
					IdqueueAdd(e.New.Id);
				}
				
			}catch(Exception ex){LogError(ex);}
		}
		
		private void ServerDispatchCombat(object sender, Decal.Adapter.NetworkMessageEventArgs e)
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
                    if(iEvent == GE_IDENTIFY_OBJECT)
                    {
                    	 OnIdentCombat(e.Message);
                    }    
            	}
            }
            catch (Exception ex){LogError(ex);}
        }  
		
		private void OnIdentCombat(Decal.Adapter.Message pMsg)
		{
			try
			{

				if(!bCombatHudEnabled) {return;}
				
        		int PossibleMobID = Convert.ToInt32(pMsg["object"]);	
        		if(Core.WorldFilter[PossibleMobID].ObjectClass == ObjectClass.Monster)
				{
	        		if(!CombatHudMobTrackingList.Any(x => x.Id == PossibleMobID))
					{
						CombatHudMobTrackingList.Add(new IdentifiedObject(Core.WorldFilter[PossibleMobID]));
					}
        			if((pMsg.Value<int>("flags") & 0x100) == 0x100)
        			{
 
        				if(pMsg.Value<int>(11) > 0) {CombatHudMobTrackingList.First(x => x.Id == PossibleMobID).HealthMax = pMsg.Value<int>(11);}
        				//else{CombatHudMobTrackingList.First(x => x.Id == PossibleMobID).HealthMax = 0;}
        				if(pMsg.Value<int>(10) > 0) {CombatHudMobTrackingList.First(x => x.Id == PossibleMobID).HealthCurrent = pMsg.Value<int>(10);}
        				//else{CombatHudMobTrackingList.First(x => x.Id == PossibleMobID).HealthCurrent = 0;}
//        				if(pMsg.Value<int>(20) > 0){CombatHudMobTrackingList.First(x => x.Id == PossibleMobID).StaminaMax = pMsg.Value<int>(20);}
//        				//else{CombatHudMobTrackingList.First(x => x.Id == PossibleMobID).StaminaMax = 0;}
//        				if(pMsg.Value<int>(18) > 0) {CombatHudMobTrackingList.First(x => x.Id == PossibleMobID).StaminaCurrent = pMsg.Value<int>(18);}
//        				//else{CombatHudMobTrackingList.First(x => x.Id == PossibleMobID).StaminaCurrent = 0;}
//        				if(pMsg.Value<int>(21) > 0) {CombatHudMobTrackingList.First(x => x.Id == PossibleMobID).ManaMax = pMsg.Value<int>(21);}
//        				//else{CombatHudMobTrackingList.First(x => x.Id == PossibleMobID).ManaMax = 0;}
//        				if(pMsg.Value<int>(19) > 0) {CombatHudMobTrackingList.First(x => x.Id == PossibleMobID).ManaCurrent = pMsg.Value<int>(19);}
//        				//else{CombatHudMobTrackingList.First(x => x.Id == PossibleMobID).ManaCurrent = 0;}
        				
//        				int	health = pMsg.Value<int>(10);
//        				int maxHealth = pMsg.Value<int>(11);
//        				int stamina = pMsg.Value<int>(18);
//        				int maxStamina = pMsg.Value<int>(20);
//        				int mana = pMsg.Value<int>(19);
//        				int maxMana = pMsg.Value<int>(21);
        				
	
        			}
				}
			} 
			catch (Exception ex) {LogError(ex);}
		}
		
		private void CombatHud_ReleaseObject(object sender, ReleaseObjectEventArgs e)
		{
			try
			{
				if(e == null) {return;}
				if(CombatHudMobTrackingList.Any(x => x.Id == e.Released.Id))
				{
				   	CombatHudMobTrackingList.RemoveAll(x => x.Id == e.Released.Id);
				}
			}catch(Exception ex){LogError(ex);}
		}
		
		private void CombatHud_ActionComplete(object sender, System.EventArgs e)
		{
			try
			{
				if(SpellCastBuffer.Count > 0 )
				{
					SpellCastInfo spellcast = SpellCastBuffer.Dequeue();
					
					if(CombatHudMobTrackingList.Any(x => x.Id == spellcast.SpellTargetId))
					{
						IdentifiedObject CastTarget = CombatHudMobTrackingList.First(x => x.Id == spellcast.SpellTargetId);
							
					   	if(CastTarget.DebuffSpellList.Any(x => x.SpellId == spellcast.SpellCastId))
					   	{
					   		CastTarget.DebuffSpellList.Find(x => x.SpellId == spellcast.SpellCastId).SpellCastTime = DateTime.Now;
					   		CastTarget.DebuffSpellList.Find(x => x.SpellId == spellcast.SpellCastId).SecondsRemaining = 
					   		Convert.ToInt32(SpellIndex[spellcast.SpellCastId].duration);
					   	}
					   	else
					   	{
					   		IdentifiedObject.DebuffSpell dbspellnew = new IdentifiedObject.DebuffSpell();
					   		dbspellnew.SpellId = spellcast.SpellCastId;
					   		dbspellnew.SpellCastTime = DateTime.Now;
					   		dbspellnew.SecondsRemaining = Convert.ToInt32(SpellIndex[spellcast.SpellCastId].duration);
					   		CastTarget.DebuffSpellList.Add(dbspellnew);	
					   	}
					}
					UpdateCombatHudMainTab();
				}
			}catch(Exception ex){LogError(ex);}
		}
		
		private int CombatTimerSeconds = 0;
		private void CombatHud_OnTimerDo(object sender, System.EventArgs e)
		{
			try
			{
				if(CombatTimerSeconds < 4)
				{
					CombatTimerSeconds++; 
					return;
				}
				else
				{
					CombatTimerSeconds = 0;	
					for(int i = CombatHudMobTrackingList.Count -1; i >= 0; i--)
					{
						CombatHudMobTrackingList[i].DistanceAway = Core.WorldFilter.Distance(Core.CharacterFilter.Id, CombatHudMobTrackingList[i].Id);
						for(int j = CombatHudMobTrackingList[i].DebuffSpellList.Count -1; j >= 0; j--)
						{
							double elapsedtime = ((TimeSpan)(DateTime.Now - CombatHudMobTrackingList[i].DebuffSpellList[j].SpellCastTime)).TotalSeconds;
							CombatHudMobTrackingList[i].DebuffSpellList[j].SecondsRemaining = SpellIndex[CombatHudMobTrackingList[i].DebuffSpellList[j].SpellId].duration - elapsedtime;
							if(CombatHudMobTrackingList[i].DebuffSpellList[j].SecondsRemaining <= 0)
							{
								CombatHudMobTrackingList[i].DebuffSpellList.RemoveAt(j);
							}
						}
						if(CombatHudMobTrackingList[i].DebuffSpellList.Count > 0)
						{
							CombatHudMobTrackingList[i].DebuffSpellList = CombatHudMobTrackingList[i].DebuffSpellList.OrderBy(x => x.SecondsRemaining).ToList();
						}
					}
					CombatHudMobTrackingList = CombatHudMobTrackingList.OrderBy(x => x.DistanceAway).ToList();	
					if(((TimeSpan)(DateTime.Now - CombatHudLastUpdate)).TotalSeconds > 5)
					{
						UpdateCombatHudMainTab();
					}
				}		
			}catch(Exception ex){LogError(ex);}
		}
		
		private void CombatHud_ChatBoxMessage(object sender, ChatTextInterceptEventArgs e)
		{
			try
			{
				if(SpellCastBuffer.Count == 0) {return;}
				if(CombatHudRegexEx.Any(x => x.IsMatch(e.Text)))
			   	{
					WriteToChat("spell removed");
					SpellCastBuffer.Dequeue();
				}		
			}catch(Exception ex){LogError(ex);}
		}
		                                
		
		private void CombatHud_SpellCast(object sender, SpellCastEventArgs e)
		{
			try
			{
				switch(SpellIndex[e.SpellId].spellschool.ToLower())
				{
					case "item enchantment":
						if(bCombatHudTrackItemDebuffs)
						{
							if(SpellIndex[e.SpellId].isdebuff)
							{	
								SpellCastInfo scinfo = new SpellCastInfo();
								scinfo.SpellTargetId = e.TargetId;
								scinfo.SpellCastId = e.SpellId;
								SpellCastBuffer.Enqueue(scinfo);
							}
						}
						return;
					case "creature enchantment":
						if(bCombatHudTrackCreatureDebuffs)
						{
							if(SpellIndex[e.SpellId].isdebuff)
							{
								SpellCastInfo scinfo = new SpellCastInfo();
								scinfo.SpellTargetId = e.TargetId;
								scinfo.SpellCastId = e.SpellId;
								SpellCastBuffer.Enqueue(scinfo);
							}
						}
						return;
					case "life magic":
						if(bCombatHudTrackLifeDebuffs)
						{
							if(SpellIndex[e.SpellId].isdebuff)
							{
								SpellCastInfo scinfo = new SpellCastInfo();
								scinfo.SpellTargetId = e.TargetId;
								scinfo.SpellCastId = e.SpellId;
								SpellCastBuffer.Enqueue(scinfo);
							}
						}
						return;
					case "void magic":
						if(bCombatHudTrackVoidDebuffs)
						{
							if(SpellIndex[e.SpellId].duration > 0)
							{
								SpellCastInfo scinfo = new SpellCastInfo();
								scinfo.SpellTargetId = e.TargetId;
								scinfo.SpellCastId = e.SpellId;
								SpellCastBuffer.Enqueue(scinfo);
							}
						}
						return;
					default:
						return;	
				}
			}catch(Exception ex){LogError(ex);}
		}

		private HudView CombatHudView = null;
		private HudFixedLayout CombatHudLayout = null;
		private HudTabView CombatHudTabView = null;
		private HudFixedLayout CombatHudMainTab = null;
		private HudFixedLayout CombatHudSettingsTab = null;
		
		private HudImageStack CombatHudTargetImage = null;
		private HudStaticText CombatHudTargetName = null;
				
		private HudProgressBar CombatHudTargetHealth = null;
		private HudButton CombatHudFocusSet = null;
		private HudButton CombatHudFocusClear = null;
		
		private HudImageStack[] CombatHudMiniVulArray = null;
		
		private HudList CombatHudDebuffTrackerList = null;
		
		private HudList.HudListRowAccessor CombatHudRow = null;
		
		private const int CombatHudGoodBackground = 0x6003355;
		private const int CombatHudWarningBackground = 0x600335B;
		private const int CombatHudExpiringBackground = 0x6003359;	
		private const int CombatHudFocusTargetBackground =  0x600335C;
		private const int CombatHudCurrentTargetBackground =  0x60011F4;
		private const int CombatHudNeutralBackground = 0x600109A;
		
		private Rectangle CombatHudTargetRectangle = new Rectangle(0,0,50,50);
		private Rectangle CombatHudMiniVulsRectangle = new Rectangle(0,0,16,16);
		private Rectangle CombatHudListVulsRectangle = new Rectangle(0,0,16,16);

		
		private void RenderCombatHud()
		{
			try
			{
				//ReadWrite here
				
				
			}catch(Exception ex){LogError(ex);}
			
			try
			{
			
				if(CombatHudView != null)
				{
					DisposeCombatHud();
				}
				
				CombatHudView = new HudView("GearTactician", 600, 220, new ACImage(0x6AA8));
				CombatHudView.Theme = VirindiViewService.HudViewDrawStyle.GetThemeByName("Minimalist Transparent");
				CombatHudView.Visible = true;
				CombatHudView.UserAlphaChangeable = false;
				CombatHudView.ShowInBar = false;
				CombatHudView.UserResizeable = false;
				CombatHudView.UserClickThroughable = false;
				CombatHudView.UserMinimizable = false;
				CombatHudView.LoadUserSettings();
				
				CombatHudLayout = new HudFixedLayout();
				CombatHudView.Controls.HeadControl = CombatHudLayout;
				
				CombatHudTabView = new HudTabView();
				CombatHudLayout.AddControl(CombatHudTabView, new Rectangle(0,0,600,220));
				
				CombatHudMainTab = new HudFixedLayout();
				CombatHudTabView.AddTab(CombatHudMainTab, "GearTactician");
				
				CombatHudSettingsTab = new HudFixedLayout();
				CombatHudTabView.AddTab(CombatHudSettingsTab, "Settings");
				
				CombatHudTabView.OpenTabChange += CombatHudTabView_OpenTabChange;
				
				RenderCombatHudMainTab();
				
				SubscribeCombatEvents();
						
			}catch(Exception ex){LogError(ex);}
			return;
		}	
		
		private void CombatHudTabView_OpenTabChange(object sender, System.EventArgs e)
		{
			try
			{
				
				
			}catch(Exception ex){LogError(ex);}
		}
		
		private void DisposeCombatHud()
		{
			try
			{
				UnsubscribeCombatEvents();
				DisposeCombatHudMainTab();
				DisposeCombatHudSettingsTab();
				
				CombatHudTabView.OpenTabChange -= CombatHudTabView_OpenTabChange;
				
				CombatHudSettingsTab.Dispose();
				CombatHudMainTab.Dispose();
				CombatHudTabView.Dispose();
				CombatHudLayout.Dispose();
				CombatHudView.Dispose();
				
			}catch(Exception ex){LogError(ex);}
		}
		
		private void RenderCombatHudMainTab()
		{
			try
			{
				CombatHudTargetName = new HudStaticText();
				CombatHudTargetName.TextAlignment = VirindiViewService.WriteTextFormats.Center;
				CombatHudMainTab.AddControl(CombatHudTargetName, new Rectangle(0,0,100,16));
				
				CombatHudTargetImage = new HudImageStack();
				CombatHudMainTab.AddControl(CombatHudTargetImage, new Rectangle(20,20,50,50));
				
				CombatHudTargetHealth = new HudProgressBar();
				CombatHudTargetHealth.ProgressEmpty = new ACImage(Color.Black);
				CombatHudTargetHealth.ProgressFilled = new ACImage(Color.Red);
				CombatHudTargetHealth.Min = 0;
				CombatHudTargetHealth.Max = 100;
				CombatHudMainTab.AddControl(CombatHudTargetHealth, new Rectangle(5,75,95,16));
				
				CombatHudMiniVulArray = new HudImageStack[20];
				for(int i = 0; i < 20; i++)
				{
					CombatHudMiniVulArray[i] = new HudImageStack();
				}
				
				for(int i = 0; i < 20; i++)
				{
					if(i < 5)
					{
						CombatHudMainTab.AddControl(CombatHudMiniVulArray[i], new Rectangle((i*20),105,16,16));
					}
					if(5 <= i && i < 10)
					{
						CombatHudMainTab.AddControl(CombatHudMiniVulArray[i], new Rectangle(((i-5)*20),125,16,16));
					}
					if(10 <= i && i < 15)
					{
						CombatHudMainTab.AddControl(CombatHudMiniVulArray[i], new Rectangle(((i-10)*20),145,16,16));
					}
					if(15 <= i)
					{
						CombatHudMainTab.AddControl(CombatHudMiniVulArray[i], new Rectangle(((i-15)*20),165,16,16));
					}
				}
				
				CombatHudFocusSet = new HudButton();
				CombatHudFocusSet.Text = "Focus";
				CombatHudMainTab.AddControl(CombatHudFocusSet, new Rectangle(5,190,35,16));
				
				CombatHudFocusClear = new HudButton();
				CombatHudFocusClear.Text = "Reset";
				CombatHudMainTab.AddControl(CombatHudFocusClear, new Rectangle(45,190,35,16));
				
				CombatHudDebuffTrackerList = new HudList();
				CombatHudMainTab.AddControl(CombatHudDebuffTrackerList, new Rectangle(110,0,500,200));
				CombatHudDebuffTrackerList.ControlHeight = 12;	
				CombatHudDebuffTrackerList.AddColumn(typeof(HudProgressBar), 100, null);
				for(int i = 0; i < 20; i++)
				{
					CombatHudDebuffTrackerList.AddColumn(typeof(HudImageStack), 16, null);
				}
				
				bCombatHudMainTab = true;
			
				CombatHudFocusSet.Hit += CombatHudFocusSet_Hit;
				CombatHudFocusClear.Hit += CombatHudFocusClear_Hit;

				UpdateCombatHudMainTab();
				
			}catch(Exception ex){LogError(ex);}
		}
		
		private void DisposeCombatHudMainTab()
		{
			try
			{
				if(!bCombatHudMainTab) {return;}
				
				CombatHudFocusSet.Hit -= CombatHudFocusSet_Hit;
				CombatHudFocusClear.Hit -= CombatHudFocusClear_Hit;
				
				CombatHudDebuffTrackerList.Dispose();
				
				for(int i = 0; i < 20; i++)
				{
					CombatHudMiniVulArray[i].Dispose();
				}
											
				CombatHudFocusClear.Dispose();
				CombatHudFocusSet.Dispose();
				CombatHudTargetHealth.Dispose();
				
				CombatHudTargetImage.Dispose();
				CombatHudTargetName.Dispose();
				
				bCombatHudMainTab = false;
								

				

			}catch(Exception ex){LogError(ex);}
		}
		
		private void CombatHudFocusSet_Hit(object sender, System.EventArgs e)
		{
			try
			{
				if(Core.WorldFilter[Core.Actions.CurrentSelection].ObjectClass == ObjectClass.Monster)
				{
					CombatHudFocusTargetGUID = Core.Actions.CurrentSelection;
					IdqueueAdd(CombatHudFocusTargetGUID);
					UpdateCombatHudMainTab();
				}
				else
				{
					WriteToChat("No monster selected.  Hud will not focus on PKs");
				}
			}catch(Exception ex){LogError(ex);}
		}
		private void CombatHudFocusClear_Hit(object sender, System.EventArgs e)
		{
			try
			{
				CombatHudFocusTargetGUID = 0;	
				UpdateCombatHudMainTab();
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
		
		private void UpdateCombatHudMainTab()
		{
			try
			{				
				if(!bCombatHudMainTab) {return;}
				if(CombatHudMobTrackingList.Count == 0) {return;}
				if(((TimeSpan)(DateTime.Now - CombatHudLastUpdate)).TotalSeconds < 1){return;}
								
				if(CombatHudFocusTargetGUID != 0)
				{
					if(!CombatHudMobTrackingList.Any(x => x.Id == CombatHudFocusTargetGUID))
					{
						CombatHudMobTrackingList.Add(new IdentifiedObject(Core.WorldFilter[CombatHudFocusTargetGUID]));
					}
					CHTargetIO = CombatHudMobTrackingList.Find(x => x.Id == CombatHudFocusTargetGUID);
				}
				else if(Core.WorldFilter[Core.Actions.CurrentSelection].ObjectClass == ObjectClass.Monster)
				{
					if(!CombatHudMobTrackingList.Any(x => x.Id == Core.Actions.CurrentSelection))
					{
						CombatHudMobTrackingList.Add(new IdentifiedObject(Core.WorldFilter[Core.Actions.CurrentSelection]));
					}
					CHTargetIO = CombatHudMobTrackingList.Find(x => x.Id == Core.Actions.CurrentSelection);
				}
				else
				{
					CHTargetIO = null;
				}
					
				if(CHTargetIO.isvalid)
				{	
					CombatHudTargetName.Text = CHTargetIO.Name;
					
					CombatHudTargetImage.Clear();
					if(CombatHudFocusTargetGUID != 0){CombatHudTargetImage.Add(CombatHudTargetRectangle, CombatHudFocusTargetBackground);}
					else{CombatHudTargetImage.Add(CombatHudTargetRectangle, CombatHudCurrentTargetBackground);}
					CombatHudTargetImage.Add(CombatHudTargetRectangle,  CHTargetIO.Icon);
					
					CombatHudTargetHealth.Min = 0;
					CombatHudTargetHealth.Max = 100;
					CombatHudTargetHealth.PreText = CHTargetIO.HealthCurrent + "/" + CHTargetIO.HealthMax;
					if(CHTargetIO.HealthCurrent < CHTargetIO.HealthMax)
					{
						CombatHudTargetHealth.Position = Convert.ToInt32(((double)CHTargetIO.HealthCurrent / (double)CHTargetIO.HealthMax)*100);
					}
					else
					{
						CombatHudTargetHealth.Position = 100;
					}
					
					if(CHTargetIO.DebuffSpellList.Count > 0)
					{
						for(int i = 0; i < 20; i++)
						{
							CombatHudMiniVulArray[i].Clear();
							if(i < CHTargetIO.DebuffSpellList.Count)
							{
								if(CHTargetIO.DebuffSpellList[i].SecondsRemaining > 60)
								{
									CombatHudMiniVulArray[i].Add(CombatHudTargetRectangle, CombatHudGoodBackground);
									CombatHudMiniVulArray[i].Add(CombatHudTargetRectangle, SpellIndex[CHTargetIO.DebuffSpellList[i].SpellId].spellicon);
									
								}
								if(CHTargetIO.DebuffSpellList[i].SecondsRemaining <= 60 && CHTargetIO.DebuffSpellList[i].SecondsRemaining > 30)
								{
									CombatHudMiniVulArray[i].Add(CombatHudTargetRectangle, CombatHudWarningBackground);
									CombatHudMiniVulArray[i].Add(CombatHudTargetRectangle, SpellIndex[CHTargetIO.DebuffSpellList[i].SpellId].spellicon);
								}
								if(CHTargetIO.DebuffSpellList[i].SecondsRemaining <= 30)
								{
									CombatHudMiniVulArray[i].Add(CombatHudTargetRectangle, CombatHudExpiringBackground);
									CombatHudMiniVulArray[i].Add(CombatHudTargetRectangle, SpellIndex[CHTargetIO.DebuffSpellList[i].SpellId].spellicon);
								}
							}
							else
							{
								CombatHudMiniVulArray[i].Add(CombatHudTargetRectangle, CombatHudNeutralBackground);
							}
						}
					}
				}
				else
				{
					CombatHudTargetName.Text = String.Empty;
					
					CombatHudTargetImage.Clear();
					
					CombatHudTargetHealth.Min = 0;
					CombatHudTargetHealth.Max = 100;
					CombatHudTargetHealth.PreText = String.Empty;
					CombatHudTargetHealth.Position = 0;
					
					for(int i = 0; i < 20; i++)
					{
						CombatHudMiniVulArray[i].Clear();
						CombatHudMiniVulArray[i].Add(CombatHudTargetRectangle, CombatHudNeutralBackground);
					}
				}
				
				CombatHudDebuffTrackerList.ClearRows();
				
				for(int i = 0; i < CombatHudMobTrackingList.Count; i++)
				{
					if(CombatHudMobTrackingList[i].DebuffSpellList.Count > 0)
					{
						CombatHudRow = CombatHudDebuffTrackerList.AddRow();
						((HudProgressBar)CombatHudRow[0]).FontHeight = 6;
						((HudProgressBar)CombatHudRow[0]).PreText = CombatHudMobTrackingList[i].Name;	
						((HudProgressBar)CombatHudRow[0]).Min = 0;
						((HudProgressBar)CombatHudRow[0]).Max = 100;
						((HudProgressBar)CombatHudRow[0]).ProgressFilled = new ACImage(Color.Red);
						((HudProgressBar)CombatHudRow[0]).ProgressEmpty = new ACImage(Color.Black);
						
						if(CombatHudMobTrackingList[i].HealthCurrent < CombatHudMobTrackingList[i].HealthMax)
						{					
							((HudProgressBar)CombatHudRow[0]).Position = Convert.ToInt32(((double)CombatHudMobTrackingList[i].HealthCurrent / (double)CombatHudMobTrackingList[i].HealthMax)*100);
						}
						else
						{
							((HudProgressBar)CombatHudRow[0]).Position = 100;
						}
						
						int loopindex;
						if(CombatHudMobTrackingList[i].DebuffSpellList.Count < 20){loopindex = CombatHudMobTrackingList[i].DebuffSpellList.Count;}
						else{loopindex = 20;}
						
						for(int j = 0; j < loopindex; j++)
						{
							if(CombatHudMobTrackingList[i].DebuffSpellList[j].SecondsRemaining > 60) 
							{
								((HudImageStack)CombatHudRow[j+1]).Add(CombatHudListVulsRectangle, CombatHudGoodBackground);
							}
							else if(CombatHudMobTrackingList[i].DebuffSpellList[j].SecondsRemaining <= 60  && CombatHudMobTrackingList[i].DebuffSpellList[j].SecondsRemaining > 30)
							{
								((HudImageStack)CombatHudRow[j+1]).Add(CombatHudListVulsRectangle, CombatHudWarningBackground);
							}
							else if(CombatHudMobTrackingList[i].DebuffSpellList[j].SecondsRemaining <= 30 && CombatHudMobTrackingList[i].DebuffSpellList[j].SecondsRemaining > 0)
							{
								((HudImageStack)CombatHudRow[j+1]).Add(CombatHudListVulsRectangle, CombatHudExpiringBackground);
							}
							else
							{
								((HudImageStack)CombatHudRow[j+1]).Add(CombatHudListVulsRectangle, CombatHudNeutralBackground);
							}
							((HudImageStack)CombatHudRow[j+1]).Add(CombatHudListVulsRectangle, SpellIndex[CombatHudMobTrackingList[i].DebuffSpellList[j].SpellId].spellicon);
						}
						
					}
				}
				
				CombatHudLastUpdate = DateTime.Now;

				
			}catch(Exception ex){LogError(ex);}
			
			
		}
		
		
		
	}
}
