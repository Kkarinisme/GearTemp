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
using System.Text.RegularExpressions;
using System.Text;

namespace GearFoundry
{
	public partial class PluginCore
	{
	
		private List<MonsterObject> CombatHudMobTrackingList = new List<MonsterObject>();
		private MonsterObject CHTargetIO = null;
		private Queue<SpellCastInfo> SpellCastBuffer = new Queue<SpellCastInfo>();
		private List<OtherDebuffCastInfo> OtherCastBuffer = new List<OtherDebuffCastInfo>();

		private List<Regex> CastFailRegexEx = new List<Regex>();
		private List<Regex> OtherCastRegexList = new List<Regex>();
		private List<string> OtherCastQuickKeepString = new List<string>();
		
		private List<SpellMapLoadable> AnimationList;
		
		public List<BuildSpellInfoHolder> bsiList  = new List<BuildSpellInfoHolder>();
		public BuildSpellInfoHolder bsi = new BuildSpellInfoHolder();
		
		
		private bool bCombatHudMainTab = false;
		private bool bCombatHudSettingsTab = false;
		private bool bCombatHudInPortalSpace = true;
		private int CombatHudFocusTargetGUID = 0;
		

		private GearTacticianSettings gtSettings;
				
		public class GearTacticianSettings
		{
			public bool bCombatHudTrackLifeDebuffs = true;
			public bool bCombatHudTrackCreatureDebuffs = true;
			public bool bCombatHudTrackItemDebuffs = true;
			public bool bCombatHudTrackVoidDebuffs = true;
			public bool bCombatHudMedium = false;
			public bool bCombatHudMinimal = false;
			public bool bShowAll = false;
            public int CombatHudWidth = 600;
            public int CombatHudHeight = 220;
		}
				
		public class BuildSpellInfoHolder
		{
			public string SpellWords;
			public int SpellID;
			public string SpellAnimationString;
		}
		
		public class SpellCastInfo
		{
			public int SpellTargetId;
			public int SpellCastId;
			public bool AutoDequeue = false;
			public DateTime CastTime = DateTime.MinValue;
			public DateTime CompleteTime = DateTime.MinValue;
		}
		

		public class OtherDebuffCastInfo
		{
			public string SpellWords = String.Empty;
			public DateTime HeardTime = DateTime.MinValue;
			public int SpellId = 0;
			public int Animation = 0;
			public string SpellSchool = String.Empty;
		}
		
		private class SpellMapLoadable
		{
			public string SpellCastWords;
			public int SpellAnimation;
			public int SpellIcon;
			public int SpellId;
			
			public SpellMapLoadable(string name, int icon, int animation, int spellid)
			{
				SpellCastWords = name;
				SpellIcon = icon;
				SpellAnimation = animation;	
				SpellId = spellid;
			}
		}	
		
		private void CombatHudReadWriteSettings(bool read)
		{
			try
			{
                FileInfo GearTacticianSettingsFile = new FileInfo(GearDir + @"\GearTactician.xml");
								
				if (read)
				{
					
					try
					{
						if (!GearTacticianSettingsFile.Exists)
		                {
		                    try
		                    {
		                    	string filedefaults = GetResourceTextFile("GearTactician.xml");
		                    	using (StreamWriter writedefaults = new StreamWriter(GearTacticianSettingsFile.ToString(), true))
								{
									writedefaults.Write(filedefaults);
									writedefaults.Close();
								}
		                    }
		                    catch (Exception ex) { LogError(ex); }
		                }
						
						using (XmlReader reader = XmlReader.Create(GearTacticianSettingsFile.ToString()))
						{	
							XmlSerializer serializer = new XmlSerializer(typeof(GearTacticianSettings));
							gtSettings = (GearTacticianSettings)serializer.Deserialize(reader);
							reader.Close();
						}
					}
					catch
					{
						gtSettings = new GearTacticianSettings();
					}
				}
				
				
				if(!read)
				{
					if(GearTacticianSettingsFile.Exists)
					{
						GearTacticianSettingsFile.Delete();
					}
					
					using (XmlWriter writer = XmlWriter.Create(GearTacticianSettingsFile.ToString()))
					{
			   			XmlSerializer serializer2 = new XmlSerializer(typeof(GearTacticianSettings));
			   			serializer2.Serialize(writer, gtSettings);
			   			writer.Close();
					}
				}
			}catch(Exception ex){LogError(ex);}
		}
			
		private void SubscribeCombatEvents()
		{
			try
			{
								
				Core.CharacterFilter.SpellCast += CombatHud_SpellCast;
				Core.CharacterFilter.ActionComplete += CombatHud_ActionComplete;
				MasterTimer.Tick += CombatHud_OnTimerDo;
				Core.WorldFilter.ReleaseObject += CombatHud_ReleaseObject;
				Core.ChatBoxMessage += CombatHud_ChatBoxMessage;
				Core.EchoFilter.ServerDispatch += ServerDispatchCombat;
				Core.WorldFilter.CreateObject += CombatHud_CreateObject;
				Core.CharacterFilter.ChangePortalMode += CombatHud_ChangePortalMode;
				Core.ItemDestroyed += CombatHud_ItemDestroyed;
				Core.ItemSelected += CombatHud_ItemSelected;
				
				//Host.Actions.InvokeChatParser("@unfilter -spellcasting");

				FillCombatHudLists();
			}catch(Exception ex){LogError(ex);}
		}
		
		private void UnsubscribeCombatEvents()
		{
			try
			{
				SpellCastBuffer.Clear();
				OtherCastBuffer.Clear();
				OtherCastRegexList.Clear();
				bsiList.Clear();
								
				Core.CharacterFilter.SpellCast -= CombatHud_SpellCast;
				Core.CharacterFilter.ActionComplete -= CombatHud_ActionComplete;
				MasterTimer.Tick -= CombatHud_OnTimerDo;
				Core.WorldFilter.ReleaseObject -= CombatHud_ReleaseObject;
				Core.ChatBoxMessage -= CombatHud_ChatBoxMessage;
				Core.EchoFilter.ServerDispatch -= ServerDispatchCombat;
				Core.WorldFilter.CreateObject -= CombatHud_CreateObject;
				Core.CharacterFilter.ChangePortalMode -= CombatHud_ChangePortalMode;
				Core.ItemSelected -= CombatHud_ItemSelected;
				Core.ItemDestroyed -= CombatHud_ItemDestroyed;

			}catch(Exception ex){LogError(ex);}
		}
		
		private void FillCombatHudLists()
		{
			try
			{				
				CastFailRegexEx.Add(new Regex("^(?<targetname>.+) resists your spell$"));
				CastFailRegexEx.Add(new Regex("Target is out of range!"));
				CastFailRegexEx.Add(new Regex("Your spell fizzled."));
				CastFailRegexEx.Add(new Regex("^(?<targetname>.+) has no appropriate targets equipped for this spell.$"));
				CastFailRegexEx.Add(new Regex("You fail to affect (?<targetname>.+) because you are not a player killer!$"));	
				CastFailRegexEx.Add(new Regex("Your spell fizzled."));
				
				OtherCastQuickKeepString.Add("Bor");
				OtherCastQuickKeepString.Add("Drosta");
				OtherCastQuickKeepString.Add("Traku");
				OtherCastQuickKeepString.Add("Slavu");
				OtherCastQuickKeepString.Add("Equin");
				OtherCastQuickKeepString.Add("Cruath");
				OtherCastQuickKeepString.Add("Yanoi");
										
				AnimationList = new List<SpellMapLoadable>();
				//void
				AnimationList.Add(new SpellMapLoadable("Zojak Bor",100691559,4,5393));  //Corrosion
				AnimationList.Add(new SpellMapLoadable("Jevak Bor",100691561,4,5401));  //Corruption
				AnimationList.Add(new SpellMapLoadable("Drosta Ves",100691552,168,5377));  //Festering Curse
				AnimationList.Add(new SpellMapLoadable("Traku Ves",100691553,169,5385));  //Weakening Curse
				AnimationList.Add(new SpellMapLoadable("Slavu Bor",100691551,167,5337));  //Destructive Curse
				
				//Creature
				//AnimationList.Add("Equin Eatak",100670578,27);  //Bottle Breaker
				//AnimationList.Add("Equin Eaja",100668285,27);  //Hands of Chorizite
				//AnimationList.Add("Equin Oloi",100668352,29);  //Jibril's Vitae
				AnimationList.Add(new SpellMapLoadable("Equin Caril",100668277,17,2054));  //Synaptic Misfire
				AnimationList.Add(new SpellMapLoadable("Equin Cavik",100668268,9,2056));  //Ataxia
				//AnimationList.Add("Equin Guafeth",100670579,21);  //Challenger's Legacy
				AnimationList.Add(new SpellMapLoadable("Equin Ealoi",100668358,27,2212));  //Wrath of Adja
				//AnimationList.Add("Equin Gualoi",100668296,29);  //Hearts on Sleeves
				AnimationList.Add(new SpellMapLoadable("Equin Ofeth",100669126,23,2228));  //Broadside of a Barn
				//AnimationList.Add("Equin Luvik",100692227,21);  //Dirty Fighting Ineptitude Other VII
				//AnimationList.Add("Equin Lureth",100692228,21);  //Dual Wield Ineptitude Other VII
				//AnimationList.Add("Equin Guavik",100668286,21);  //Sashi Mu's Kiss
				AnimationList.Add(new SpellMapLoadable("Equin Casith",100668296,15,2064));  //Self Loathing
				//AnimationList.Add("Equin Luzael",100692225,19);  //Finesse Weapon Ineptitude Other VII
				//AnimationList.Add("Equin Guareth",100670580,21);  //Twisted Digits
				AnimationList.Add(new SpellMapLoadable("Equin Careth",100668273,11,2068));  //Brittle Bones
				//AnimationList.Add("Equin Guaguz",100668279,21);  //Unsteady Hands
				//AnimationList.Add("Equin Luril",100692226,19);  //Heavy Weapon Ineptitude Other VII
				//AnimationList.Add("Equin Eapaj",100668272,27);  //Wrath of Celcynd
				//AnimationList.Add("Equin Guaja",100668264,29);  //Unfortunate Appraisal
				//AnimationList.Add("Equin Guasith",100668282,25);  //Feat of Radaz
				//AnimationList.Add("Equin Guati",100668295,25);  //Gears Unwound
				//AnimationList.Add("Equin Guatak",100668283,21);  //Kwipetian Vision
				AnimationList.Add(new SpellMapLoadable("Equin Eavik",100668337,27,2264));  //Wrath of Harlune
				//AnimationList.Add("Equin Lutak",100692224,19);  //Light Weapon Ineptitude Other VII
				//AnimationList.Add("Equin Guaril",100668284,163);  //Fat Fingers
				//AnimationList.Add("Equin Eareth",100668354,29);  //Eyes Clouded
				//AnimationList.Add("Puish Zharil",100668351,76);  //Meditative Trance
				AnimationList.Add(new SpellMapLoadable("Equin Opaj",100668330,23,2282));  //Futility
				//AnimationList.Add("Equin Easith",100668288,27);  //Inefficient Investment
				//AnimationList.Add("Equin Hatak",100668266,19);  //Missile Weapon Ineptitude Other VII
				//AnimationList.Add("Equin Guapaj",100668353,29);  //Ignorance's Bliss
				//AnimationList.Add("Equin Guazael",100668355,29);  //Introversion
				//AnimationList.Add("Equin Hati",100668265,21);  //Recklessness Ineptitude Other VII
				//AnimationList.Add("Equin Hafeth",100692229,21);  //Shield Ineptitude Other VII
				AnimationList.Add(new SpellMapLoadable("Equin Caja",100668294,13,2084));  //Belly of Lead
				//AnimationList.Add("Equin Luguz",100692230,21);  //Sneak Attack Ineptitude Other VII
				AnimationList.Add(new SpellMapLoadable("Equin Eaves",100691575,27,5425));  //Void Magic Ineptitude Other VII
				AnimationList.Add(new SpellMapLoadable("Equin Ozael",100668331,23,2318));  //Gravity Well
				AnimationList.Add(new SpellMapLoadable("Equin Eati",100668272,27,2320));  //Wrath of the Hieromancer
				AnimationList.Add(new SpellMapLoadable("Equin Cazael",100668300,7,2088));  //Senescence
				//AnimationList.Add("Equin Luja",100668357,29);  //Eye of the Grunt
				
				//Life
				AnimationList.Add(new SpellMapLoadable("Cruath Qualoi",100668344,50,2162));  //Olthoi's Gift
				AnimationList.Add(new SpellMapLoadable("Cruath Quaguz",100668348,48,2164));  //Swordsman's Gift
				AnimationList.Add(new SpellMapLoadable("Cruath Quareth",100668345,56,2166));  //Tusker's Gift
				AnimationList.Add(new SpellMapLoadable("Cruath Quavik",100668292,52,2168));  //Gelidite's Gift
				AnimationList.Add(new SpellMapLoadable("Yanoi Zhavik",100668299,42,2176));  //Enervation
				AnimationList.Add(new SpellMapLoadable("Yanoi Zhapaj",100668279,38,2178));  //Decrepitude's Grasp
				AnimationList.Add(new SpellMapLoadable("Cruath Quatak",100668291,44,2170));  //Inferno's Gift
				AnimationList.Add(new SpellMapLoadable("Cruath Quasith",100668293,56,2074));  //Gossamer Flesh
				AnimationList.Add(new SpellMapLoadable("Cruath Quafeth",100668346,54,2172));  //Astyrrian's Gift
				AnimationList.Add(new SpellMapLoadable("Yanoi Zhaloi",100668288,163,2180));  //Energy Flux
				AnimationList.Add(new SpellMapLoadable("Cruath Quaril",100668347,46,2174));  //Archer's Gift
				
				//Item
				//AnimationList.Add(new SpellMapLoadable("Equin Qualoi",100673974,64,2093));  //Olthoi Bait
				//AnimationList.Add(new SpellMapLoadable("Equin Quaguz",100673980,62,2095));  //Swordman Bait
				//AnimationList.Add(new SpellMapLoadable("Equin Quareth",100673975,68,2099));  //Tusker Bait
				AnimationList.Add(new SpellMapLoadable("Equin Quasith",100673981,143,2100));  //Tattercoat
				//AnimationList.Add(new SpellMapLoadable("Equin Quatak",100673976,58,2103));  //Inferno Bait
				//AnimationList.Add(new SpellMapLoadable("Equin Quavik",100673977,66,2105));  //Gelidite Bait
				//AnimationList.Add(new SpellMapLoadable("Malar Aevik",100673983,68,2107));  //Cabalistic Ostracism
				//Lets be honest, you're not likely to worry about the debuff above on a mob....and dumping it lets you quit considering every buff being cast...
				//AnimationList.Add(new SpellMapLoadable("Equin Aetak",100673990,64,2109));  //Lugian's Speed
				//AnimationList.Add(new SpellMapLoadable("Equin Quafeth",100673978,68,2111));  //Astyrrian Bait
				//AnimationList.Add(new SpellMapLoadable("Equin Aeguz",100673985,62,2112));  //Wi's Folly
				//AnimationList.Add(new SpellMapLoadable("Equin Quaril",100673979,60,2114));  //Archer Bait
				//AnimationList.Add(new SpellMapLoadable("Equin Aeril",100676646,58,3266));  //Spirit Pacification
				//AnimationList.Add(new SpellMapLoadable("Equin Aereth",100673992,60,2118));  //Clouded Motives
				//AnimationList.Add(new SpellMapLoadable("Equin Aeti",100668401,60,2119));  //Vagabond's Gift
			}catch(Exception ex){LogError(ex);}
		}
	
	
		
		private void CombatHud_ItemSelected(object sender, ItemSelectedEventArgs e)
		{
			try
			{
				if(e.ItemGuid != 0 && Core.WorldFilter[e.ItemGuid].ObjectClass == ObjectClass.Monster)
				{
					UpdateCombatHudMainTab();
				}
			}catch(Exception ex){LogError(ex);}
		}
		
		
		private void CombatHud_ItemDestroyed(object sender, ItemDestroyedEventArgs e)
		{
			try
			{
				if(CombatHudFocusTargetGUID == e.ItemGuid) {CombatHudFocusTargetGUID = 0;}
				if(CombatHudMobTrackingList.Count == 0) {return;}
				else
				{
					CombatHudMobTrackingList.RemoveAll(x => x.Id == e.ItemGuid);
					UpdateCombatHudMainTab();
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
					CombatHudMobTrackingList.Add(new MonsterObject(wo));
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
						CombatHudMobTrackingList.Add(new MonsterObject(Core.WorldFilter[e.New.Id]));
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
            	if(e.Message.Type == AC_APPLY_VISUALSOUND)
                {
                    	OnVisualSound(e.Message);
                }
            	if(e.Message.Type == AC_GAME_EVENT)
            	{
            		try
                    {
                    	iEvent = Convert.ToInt32(e.Message["event"]);
                    }
            		catch{}
            		if(iEvent == GE_UPDATE_HEALTH)
            		{
            			OnUpdateHealth(e.Message);
            		}
            		if(iEvent == GE_IDENTIFY_OBJECT)
                    {
                    	 OnIdentCombat(e.Message);
                    } 
                    
            	}
            }
            catch (Exception ex){LogError(ex);}
        }  
				
		private void OnVisualSound(Decal.Adapter.Message pMsg)
		{
			try
			{	

				if(OtherCastBuffer.Count == 0) {return;}
				
				OtherCastBuffer.RemoveAll(x => (DateTime.Now - x.HeardTime).TotalSeconds > 5);
				if(OtherCastBuffer.Count == 0) {return;}
				
				if(Core.WorldFilter[pMsg.Value<int>(0)].ObjectClass != ObjectClass.Monster) {return;}
				else
				{
					if(!CombatHudMobTrackingList.Any(x => x.Id == pMsg.Value<int>(0)))
					{
						CombatHudMobTrackingList.Add(new MonsterObject(Core.WorldFilter[pMsg.Value<int>(0)]));
					}
				}
	
				//Will ignore debuffs under L6 (or there abouts).
				if(pMsg.Value<double>(2) < 1) 
				{
					return;
				}
								
				int probablespellid = (from spels in OtherCastBuffer
										where spels.Animation == pMsg.Value<int>(1)
										select spels).FirstOrDefault().SpellId;
				
				if(probablespellid != 0)
				{	
					if(CombatHudMobTrackingList.Any(x => x.Id == pMsg.Value<int>(0)))
					{
						
						MonsterObject CastTarget = CombatHudMobTrackingList.First(x => x.Id == pMsg.Value<int>(0));
						
						if(CastTarget.DebuffSpellList.Any(x => x.SpellId == probablespellid))
					   	{
							CastTarget.DebuffSpellList.Find(x => x.SpellId == probablespellid).SpellCastTime = DateTime.Now;
							CastTarget.DebuffSpellList.Find(x => x.SpellId == probablespellid).SecondsRemaining = SpellIndex[probablespellid].duration;
					   	}
					   	else
					   	{
					   		MonsterObject.DebuffSpell dbspellnew = new MonsterObject.DebuffSpell();
					   		dbspellnew.SpellId = probablespellid;
					   		dbspellnew.SpellCastTime = DateTime.Now;
					   		dbspellnew.SecondsRemaining = SpellIndex[probablespellid].duration;
					   		CastTarget.DebuffSpellList.Add(dbspellnew);	
					   	}
					}
					UpdateCombatHudMainTab();
				}
			}catch(Exception ex){LogError(ex);}
		}
				
		private void OnIdentCombat(Decal.Adapter.Message pMsg)
		{
			try
			{
				int PossibleMobID = Convert.ToInt32(pMsg["object"]);
        		if(Core.WorldFilter[PossibleMobID].ObjectClass == ObjectClass.Monster)
				{
        			if(CombatHudMobTrackingList.Count == 0)
        			{
        				CombatHudMobTrackingList.Add(new MonsterObject(Core.WorldFilter[PossibleMobID]));
        			}
	        		else if(!CombatHudMobTrackingList.Any(x => x.Id == PossibleMobID))
					{
						CombatHudMobTrackingList.Add(new MonsterObject(Core.WorldFilter[PossibleMobID]));
					}
	        		
	        		MonsterObject UpdateMonster = CombatHudMobTrackingList.First(x => x.Id == PossibleMobID);
	        		
        			if((pMsg.Value<int>("flags") & 0x100) == 0x100)
        			{       	
						//Empty try/catch to deal with cast not valid error.  (infrequent)        				
        				try
        				{
        					if(pMsg.Value<int>(11) > 0) {UpdateMonster.HealthMax = pMsg.Value<int>(11);}
	      					if(pMsg.Value<int>(10) > 0) {UpdateMonster.HealthCurrent = pMsg.Value<int>(10);}
        				}catch{}
//        				if(pMsg.Value<int>(20) > 0){CombatHudMobTrackingList.First(x => x.Id == PossibleMobID).StaminaMax = pMsg.Value<int>(20);}
//        				if(pMsg.Value<int>(18) > 0) {CombatHudMobTrackingList.First(x => x.Id == PossibleMobID).StaminaCurrent = pMsg.Value<int>(18);}
//        				if(pMsg.Value<int>(21) > 0) {CombatHudMobTrackingList.First(x => x.Id == PossibleMobID).ManaMax = pMsg.Value<int>(21);}
//        				if(pMsg.Value<int>(19) > 0) {CombatHudMobTrackingList.First(x => x.Id == PossibleMobID).ManaCurrent = pMsg.Value<int>(19);}
						if(UpdateMonster.HealthMax > 0) 
						{
							UpdateMonster.HealthRemaining = Convert.ToInt32((double)UpdateMonster.HealthCurrent/(double)UpdateMonster.HealthMax*100);
						}
						
        			}
				}
        		UpdateCombatHudMainTab();
			} 
			catch (Exception ex) {LogError(ex);}
		}
		
		private void OnUpdateHealth(Decal.Adapter.Message pMsg)
		{
			try
			{
				int PossibleMobID = 0;
				try {PossibleMobID = Convert.ToInt32(pMsg["object"]);}catch{}
				if(PossibleMobID == 0){return;}
        		if(Core.WorldFilter[PossibleMobID].ObjectClass == ObjectClass.Monster)
				{
        			if(CombatHudMobTrackingList.Count == 0)
        			{
        				CombatHudMobTrackingList.Add(new MonsterObject(Core.WorldFilter[PossibleMobID]));
        			}
	        		else if(!CombatHudMobTrackingList.Any(x => x.Id == PossibleMobID))
					{
						CombatHudMobTrackingList.Add(new MonsterObject(Core.WorldFilter[PossibleMobID]));
					}
	        		
	        		MonsterObject UpdateMonster = CombatHudMobTrackingList.First(x => x.Id == PossibleMobID);
	        		
	        		UpdateMonster.HealthRemaining = Convert.ToInt32(Convert.ToDouble(pMsg["health"])*100);
				}
        		UpdateCombatHudMainTab();
			} 
			catch (Exception ex) {LogError(ex);}
		}
		
		private void CombatHud_ReleaseObject(object sender, ReleaseObjectEventArgs e)
		{
			try
			{
				if(CombatHudFocusTargetGUID == e.Released.Id) {CombatHudFocusTargetGUID = 0;}
				if(CombatHudMobTrackingList.Count == 0){return;}
				else
				{	
					CombatHudMobTrackingList.RemoveAll(x => x.Id == e.Released.Id);	
					UpdateCombatHudMainTab();
				}
			}catch(Exception ex){LogError(ex);}
		}

		private bool CombatActionCompleteDelayRunning = false;
		private void CombatHud_ActionComplete(object sender, System.EventArgs e)
		{
			try
			{
				if(SpellCastBuffer.Count > 0)
				{
					SpellCastBuffer.First().CompleteTime = DateTime.Now;
					if((SpellCastBuffer.First().CompleteTime - SpellCastBuffer.First().CastTime).TotalMilliseconds < 200)
					{
						SpellCastBuffer.First().AutoDequeue = true;
					}
					
					if(CombatActionCompleteDelayRunning) {return;}
					else
					{
						CombatActionCompleteDelayRunning  = true;
						Core.RenderFrame += RenderFrame_CombatActionCompleteDelay;
					}
				}
				else{return;}
			}catch(Exception ex){LogError(ex);}
		}
		private void RenderFrame_CombatActionCompleteDelay(object sender, EventArgs e)
		{
			try
			{
				if((DateTime.Now - SpellCastBuffer.First().CompleteTime).TotalMilliseconds < 1500) {return;}
				else
				{
					Core.RenderFrame -= RenderFrame_CombatActionCompleteDelay;	
					CombatActionCompleteDelayRunning  = false;
				}
				
				if(SpellCastBuffer.First().AutoDequeue)
				{
					SpellCastBuffer.Dequeue();
					return;
				}
				SpellCastInfo spellcast = SpellCastBuffer.Dequeue();
				
				if(CombatHudMobTrackingList.Any(x => x.Id == spellcast.SpellTargetId))
				{
					MonsterObject CastTarget = CombatHudMobTrackingList.First(x => x.Id == spellcast.SpellTargetId);
						
				   	if(CastTarget.DebuffSpellList.Any(x => x.SpellId == spellcast.SpellCastId))
				   	{
				   		CastTarget.DebuffSpellList.Find(x => x.SpellId == spellcast.SpellCastId).SpellCastTime = DateTime.Now;
				   		CastTarget.DebuffSpellList.Find(x => x.SpellId == spellcast.SpellCastId).SecondsRemaining = 
				   		Convert.ToInt32(SpellIndex[spellcast.SpellCastId].duration);
				   	}
				   	else
				   	{
				   		MonsterObject.DebuffSpell dbspellnew = new MonsterObject.DebuffSpell();
				   		dbspellnew.SpellId = spellcast.SpellCastId;
				   		dbspellnew.SpellCastTime = DateTime.Now;
				   		dbspellnew.SecondsRemaining = Convert.ToInt32(SpellIndex[spellcast.SpellCastId].duration);
				   		CastTarget.DebuffSpellList.Add(dbspellnew);	
				   	}
				}
				UpdateCombatHudMainTab();

				
			}catch(Exception ex){Core.RenderFrame -= RenderFrame_CombatActionCompleteDelay; LogError(ex);}
		}
		                                         
		
		private void CombatHud_OnTimerDo(object sender, System.EventArgs e)
		{
			try
			{
				if((DateTime.Now - CombatHudLastUpdate).TotalSeconds < 2) {return;}
				else
				{
					if(CombatHudMobTrackingList.Count == 0) {return;}
					
					CombatHudMobTrackingList.RemoveAll(x => x.ObjectClass != ObjectClass.Monster);
					
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
					CombatHudLastUpdate = DateTime.Now;
					UpdateCombatHudMainTab();

				}		
			}catch(Exception ex){LogError(ex);}
		}
		
		private void CombatHud_ChatBoxMessage(object sender, ChatTextInterceptEventArgs e)
		{
			try
			{	
				if(e.Color != 17 && e.Color != 7){return;}
				if(e.Text.StartsWith("You cast")) {return;}
				if(e.Text.StartsWith("You say,")) {return;}
				
				if(e.Color == 7)
				{	
					if(SpellCastBuffer.Count != 0 && CastFailRegexEx.Any(x => x.IsMatch(e.Text)))
					{
						//WriteToChat("Caught spell failure");
						SpellCastBuffer.First().AutoDequeue = true;
					}
					return;
				}
				
				if(!OtherCastQuickKeepString.Any(x => e.Text.Contains(x))) {return;}			
				if(AnimationList.Any(x => e.Text.Contains(x.SpellCastWords)))
				{	
					OtherDebuffCastInfo odci = new OtherDebuffCastInfo();
					odci.HeardTime = DateTime.Now;
					var tanimation = AnimationList.Find(x => e.Text.Contains(x.SpellCastWords));	
					odci.SpellWords =tanimation.SpellCastWords;
					odci.SpellId = tanimation.SpellId;
					odci.Animation = tanimation.SpellAnimation;
					odci.SpellSchool = SpellIndex[odci.SpellId].spellschool;
					
					switch(SpellIndex[odci.SpellId].spellschool.ToLower())
					{
						case "item enchantment":
							if(!gtSettings.bCombatHudTrackItemDebuffs) {return;}
							break;
						case "creature enchantment":
							if(!gtSettings.bCombatHudTrackCreatureDebuffs) {return;}
							break;
						case "life magic":
							if(!gtSettings.bCombatHudTrackLifeDebuffs) {return;}
							break;
						case "void magic":
							if(!gtSettings.bCombatHudTrackVoidDebuffs){return;}
							break;
						default:
							return;	
					}	
					OtherCastBuffer.Add(odci);
				}
			}catch(Exception ex){LogError(ex);}
		}
		                                
		
		private void CombatHud_SpellCast(object sender, SpellCastEventArgs e)
		{
			try
			{
				bsi.SpellID = e.SpellId;
				switch(SpellIndex[e.SpellId].spellschool.ToLower())
				{
					case "item enchantment":
						if(gtSettings.bCombatHudTrackItemDebuffs)
						{
							if(SpellIndex[e.SpellId].isdebuff)
							{	
								SpellCastInfo scinfo = new SpellCastInfo();
								scinfo.SpellTargetId = e.TargetId;
								scinfo.SpellCastId = e.SpellId;
								scinfo.CastTime = DateTime.Now;
								SpellCastBuffer.Enqueue(scinfo);
							}
						}
						return;
					case "creature enchantment":
						if(gtSettings.bCombatHudTrackCreatureDebuffs)
						{
							if(SpellIndex[e.SpellId].isdebuff)
							{
								SpellCastInfo scinfo = new SpellCastInfo();
								scinfo.SpellTargetId = e.TargetId;
								scinfo.SpellCastId = e.SpellId;
								scinfo.CastTime = DateTime.Now;
								SpellCastBuffer.Enqueue(scinfo);
							}
						}
						return;
					case "life magic":
						if(gtSettings.bCombatHudTrackLifeDebuffs)
						{
							if(SpellIndex[e.SpellId].isdebuff)
							{
								SpellCastInfo scinfo = new SpellCastInfo();
								scinfo.SpellTargetId = e.TargetId;
								scinfo.SpellCastId = e.SpellId;
								scinfo.CastTime = DateTime.Now;
								SpellCastBuffer.Enqueue(scinfo);
							}
						}
						return;
					case "void magic":
						if(gtSettings.bCombatHudTrackVoidDebuffs)
						{
							if(SpellIndex[e.SpellId].duration > 0)
							{
								SpellCastInfo scinfo = new SpellCastInfo();
								scinfo.SpellTargetId = e.TargetId;
								scinfo.SpellCastId = e.SpellId;
								scinfo.CastTime = DateTime.Now;
								SpellCastBuffer.Enqueue(scinfo);
							}
						}
						return;
					default:
						return;	
				}
			}catch(Exception ex){LogError(ex);}
		}
	}
}
