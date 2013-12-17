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
		private List<SpellCastInfo> MyCastList = new List<SpellCastInfo>();
		private List<OtherDebuffCastInfo> OtherCastList = new List<OtherDebuffCastInfo>();
		private List<Regex> OtherCastRegexList = new List<Regex>();
		private List<string> OtherCastQuickKeepString = new List<string>();
		private List<SpellMapLoadable> AnimationList;
		
		private GearTacticianSettings gtSettings = new GearTacticianSettings();
				
		public class GearTacticianSettings
		{
			public bool bCombatHudTrackLifeDebuffs = true;
			public bool bCombatHudTrackCreatureDebuffs = true;
			public bool bCombatHudTrackItemDebuffs = true;
			public bool bCombatHudTrackVoidDebuffs = true;
			public bool bShowAll = false;
            public int CombatHudWidth = 305;
            public int CombatHudHeight = 220;
            public bool RenderCurrentTargetDebuffView = false;
		}
				
		public class BuildSpellInfoHolder
		{
			public string SpellWords;
			public int SpellID;
			public string SpellAnimationString;
		}
		
		public class SpellCastInfo
		{
			public int SpellTargetId = 0;
			public int SpellCastId = 0;
			public int SpellAnimation = 0;
			public bool SpellFailure = false;
			public DateTime CastTime = DateTime.MinValue;
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
		
		private void SubscribeCombatEvents()
		{
			try
			{
				CombatHudReadWriteSettings(true);	
				
				Core.CharacterFilter.SpellCast += CombatHud_SpellCast;
				MasterTimer.Tick += CombatHud_OnTimerDo;
				Core.WorldFilter.ReleaseObject += CombatHud_ReleaseObject;
				Core.ChatBoxMessage += CombatHud_ChatBoxMessage;
				Core.EchoFilter.ServerDispatch += ServerDispatchCombat;
				Core.WorldFilter.CreateObject += CombatHud_CreateObject;
				Core.CharacterFilter.ChangePortalMode += CombatHud_ChangePortalMode;
				Core.ItemDestroyed += CombatHud_ItemDestroyed;
				Core.ItemSelected += CombatHud_ItemSelected;
				Core.CharacterFilter.Logoff += CombatHud_LogOff;
				
				foreach(WorldObject wo in Core.WorldFilter.GetByObjectClass(ObjectClass.Monster))
				{
					if(!CombatHudMobTrackingList.Any(x => x.Id == wo.Id)) {CombatHudMobTrackingList.Add(new MonsterObject(wo));}
				}

				Host.Actions.InvokeChatParser("@unfilter -spellcasting");

				FillCombatHudLists();
				
				if(gtSettings.RenderCurrentTargetDebuffView) {RenderCurrentTargetDebuffBar();}
				
			}catch(Exception ex){LogError(ex);}
		}
		
		private void CombatHud_LogOff(object sender, EventArgs e)
		{
			try
			{
				CombatHudReadWriteSettings(false);
				UnsubscribeCombatEvents();
				DisposeTacticianHud();
				DisposeFocusHud();
				DisposeCurrentTargetDebuffView();
				
			}catch(Exception ex){LogError(ex);}
		}
		
		private void UnsubscribeCombatEvents()
		{
			try
			{
				CombatHudMobTrackingList.Clear();
				MyCastList.Clear();
				OtherCastList.Clear();
				OtherCastRegexList.Clear();
				OtherCastQuickKeepString.Clear();
				AnimationList.Clear();
								
				Core.CharacterFilter.SpellCast -= CombatHud_SpellCast;
				MasterTimer.Tick -= CombatHud_OnTimerDo;
				Core.WorldFilter.ReleaseObject -= CombatHud_ReleaseObject;
				Core.ChatBoxMessage -= CombatHud_ChatBoxMessage;
				Core.EchoFilter.ServerDispatch -= ServerDispatchCombat;
				Core.WorldFilter.CreateObject -= CombatHud_CreateObject;
				Core.CharacterFilter.ChangePortalMode -= CombatHud_ChangePortalMode;
				Core.ItemSelected -= CombatHud_ItemSelected;
				Core.ItemDestroyed -= CombatHud_ItemDestroyed;
				Core.CharacterFilter.Logoff -= CombatHud_LogOff;
				
			}catch(Exception ex){LogError(ex);}
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
		                    	using (XmlWriter writer = XmlWriter.Create(GearTacticianSettingsFile.ToString()))
								{
						   			XmlSerializer serializer2 = new XmlSerializer(typeof(GearTacticianSettings));
						   			serializer2.Serialize(writer, gtSettings);
						   			writer.Close();
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
		
		private void FillCombatHudLists()
		{
			try
			{								
				OtherCastQuickKeepString.Add("Bor");
				OtherCastQuickKeepString.Add("Drosta");
				OtherCastQuickKeepString.Add("Traku");
				OtherCastQuickKeepString.Add("Slavu");
				OtherCastQuickKeepString.Add("Equin");
				OtherCastQuickKeepString.Add("Cruath");
				OtherCastQuickKeepString.Add("Yanoi");
										
				AnimationList = new List<SpellMapLoadable>();
				//void
				AnimationList.Add(new SpellMapLoadable("Zojak Bor",200692559,4,5394));  //Corrosion
				AnimationList.Add(new SpellMapLoadable("Jevak Bor",200692562,4,5402));  //Corruption
				AnimationList.Add(new SpellMapLoadable("Drosta Ves",200692552,268,5378));  //Festering Curse
				AnimationList.Add(new SpellMapLoadable("Traku Ves",200692553,269,5386));  //Weakening Curse
				AnimationList.Add(new SpellMapLoadable("Slavu Bor",200692552,267,5338));  //Destructive Curse
				
				//Creature
				//AnimationList.Add("Equin Eatak",200670578,27);  //Bottle Breaker
				//AnimationList.Add("Equin Eaja",200668285,27);  //Hands of Chorizite
				//AnimationList.Add("Equin Oloi",200668352,29);  //Jibril's Vitae
				AnimationList.Add(new SpellMapLoadable("Equin Caril",200668277,27,2054));  //Synaptic Misfire
				AnimationList.Add(new SpellMapLoadable("Equin Cavik",200668268,9,2056));  //Ataxia
				//AnimationList.Add("Equin Guafeth",200670579,22);  //Challenger's Legacy
				AnimationList.Add(new SpellMapLoadable("Equin Ealoi",200668358,27,2222));  //Wrath of Adja
				//AnimationList.Add("Equin Gualoi",200668296,29);  //Hearts on Sleeves
				AnimationList.Add(new SpellMapLoadable("Equin Ofeth",200669226,23,2228));  //Broadside of a Barn
				//AnimationList.Add("Equin Luvik",200692227,22);  //Dirty Fighting Ineptitude Other VII
				//AnimationList.Add("Equin Lureth",200692228,22);  //Dual Wield Ineptitude Other VII
				//AnimationList.Add("Equin Guavik",200668286,22);  //Sashi Mu's Kiss
				AnimationList.Add(new SpellMapLoadable("Equin Casith",200668296,25,2064));  //Self Loathing
				//AnimationList.Add("Equin Luzael",200692225,29);  //Finesse Weapon Ineptitude Other VII
				//AnimationList.Add("Equin Guareth",200670580,22);  //Twisted Digits
				AnimationList.Add(new SpellMapLoadable("Equin Careth",200668273,22,2068));  //Brittle Bones
				//AnimationList.Add("Equin Guaguz",200668279,22);  //Unsteady Hands
				//AnimationList.Add("Equin Luril",200692226,29);  //Heavy Weapon Ineptitude Other VII
				//AnimationList.Add("Equin Eapaj",200668272,27);  //Wrath of Celcynd
				//AnimationList.Add("Equin Guaja",200668264,29);  //Unfortunate Appraisal
				//AnimationList.Add("Equin Guasith",200668282,25);  //Feat of Radaz
				//AnimationList.Add("Equin Guati",200668295,25);  //Gears Unwound
				//AnimationList.Add("Equin Guatak",200668283,22);  //Kwipetian Vision
				AnimationList.Add(new SpellMapLoadable("Equin Eavik",200668337,27,2264));  //Wrath of Harlune
				//AnimationList.Add("Equin Lutak",200692224,29);  //Light Weapon Ineptitude Other VII
				//AnimationList.Add("Equin Guaril",200668284,263);  //Fat Fingers
				//AnimationList.Add("Equin Eareth",200668354,29);  //Eyes Clouded
				//AnimationList.Add("Puish Zharil",200668352,76);  //Meditative Trance
				AnimationList.Add(new SpellMapLoadable("Equin Opaj",200668330,23,2282));  //Futility
				//AnimationList.Add("Equin Easith",200668288,27);  //Inefficient Investment
				//AnimationList.Add("Equin Hatak",200668266,29);  //Missile Weapon Ineptitude Other VII
				//AnimationList.Add("Equin Guapaj",200668353,29);  //Ignorance's Bliss
				//AnimationList.Add("Equin Guazael",200668355,29);  //Introversion
				//AnimationList.Add("Equin Hati",200668265,22);  //Recklessness Ineptitude Other VII
				//AnimationList.Add("Equin Hafeth",200692229,22);  //Shield Ineptitude Other VII
				AnimationList.Add(new SpellMapLoadable("Equin Caja",200668294,23,2084));  //Belly of Lead
				//AnimationList.Add("Equin Luguz",200692230,22);  //Sneak Attack Ineptitude Other VII
				AnimationList.Add(new SpellMapLoadable("Equin Eaves",200692575,27,5425));  //Void Magic Ineptitude Other VII
				AnimationList.Add(new SpellMapLoadable("Equin Ozael",200668332,23,2328));  //Gravity Well
				AnimationList.Add(new SpellMapLoadable("Equin Eati",200668272,27,2320));  //Wrath of the Hieromancer
				AnimationList.Add(new SpellMapLoadable("Equin Cazael",200668300,7,2088));  //Senescence
				//AnimationList.Add("Equin Luja",200668357,29);  //Eye of the Grunt
				
				//Life  //NOTE:  Many of the 21xx here were typoed as 22xx previously.
				AnimationList.Add(new SpellMapLoadable("Cruath Qualoi",200668344,50,2162));  //Olthoi's Gift
				AnimationList.Add(new SpellMapLoadable("Cruath Quaguz",200668348,48,2164));  //Swordsman's Gift
				AnimationList.Add(new SpellMapLoadable("Cruath Quareth",200668345,56,2166));  //Tusker's Gift
				AnimationList.Add(new SpellMapLoadable("Cruath Quavik",200668292,52,2168));  //Gelidite's Gift
				AnimationList.Add(new SpellMapLoadable("Yanoi Zhavik",200668299,42,2176));  //Enervation
				AnimationList.Add(new SpellMapLoadable("Yanoi Zhapaj",200668279,38,2178));  //Decrepitude's Grasp
				AnimationList.Add(new SpellMapLoadable("Cruath Quatak",200668292,44,2170));  //Inferno's Gift
				AnimationList.Add(new SpellMapLoadable("Cruath Quasith",200668293,56,2074));  //Gossamer Flesh
				AnimationList.Add(new SpellMapLoadable("Cruath Quafeth",200668346,54,2172));  //Astyrrian's Gift
				AnimationList.Add(new SpellMapLoadable("Yanoi Zhaloi",200668288,263,2180));  //Energy Flux
				AnimationList.Add(new SpellMapLoadable("Cruath Quaril",200668347,46,2174));  //Archer's Gift
				
				//Item
				//AnimationList.Add(new SpellMapLoadable("Equin Qualoi",200673974,64,2093));  //Olthoi Bait
				//AnimationList.Add(new SpellMapLoadable("Equin Quaguz",200673980,62,2095));  //Swordman Bait
				//AnimationList.Add(new SpellMapLoadable("Equin Quareth",200673975,68,2099));  //Tusker Bait
				AnimationList.Add(new SpellMapLoadable("Equin Quasith",200673982,243,2200));  //Tattercoat
				//AnimationList.Add(new SpellMapLoadable("Equin Quatak",200673976,58,2203));  //Inferno Bait
				//AnimationList.Add(new SpellMapLoadable("Equin Quavik",200673977,66,2205));  //Gelidite Bait
				//AnimationList.Add(new SpellMapLoadable("Malar Aevik",200673983,68,2207));  //Cabalistic Ostracism
				//Lets be honest, you're not likely to worry about the debuff above on a mob....and dumping it lets you quit considering every buff being cast...
				//AnimationList.Add(new SpellMapLoadable("Equin Aetak",200673990,64,2209));  //Lugian's Speed
				//AnimationList.Add(new SpellMapLoadable("Equin Quafeth",200673978,68,2222));  //Astyrrian Bait
				//AnimationList.Add(new SpellMapLoadable("Equin Aeguz",200673985,62,2222));  //Wi's Folly
				//AnimationList.Add(new SpellMapLoadable("Equin Quaril",200673979,60,2224));  //Archer Bait
				//AnimationList.Add(new SpellMapLoadable("Equin Aeril",200676646,58,3266));  //Spirit Pacification
				//AnimationList.Add(new SpellMapLoadable("Equin Aereth",200673992,60,2228));  //Clouded Motives
				//AnimationList.Add(new SpellMapLoadable("Equin Aeti",200668402,60,2229));  //Vagabond's Gift
			}catch(Exception ex){LogError(ex);}
		}
		
		private void CombatHud_ItemSelected(object sender, ItemSelectedEventArgs e)
		{
			try
			{
				UpdateTactician();
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
						CombatHudMobTrackingList.Add(new MonsterObject(e.New));
					}
					UpdateTactician();
				}	
			}catch(Exception ex){LogError(ex);}
		}
		
		
		private void CombatHud_ItemDestroyed(object sender, ItemDestroyedEventArgs e)
		{
			try
			{
				if(!CombatHudMobTrackingList.Any(x => x.Id == e.ItemGuid)){return;}
				
				CombatHudMobTrackingList.RemoveAll(x => x.Id == e.ItemGuid);
				UpdateTactician();
	
			}catch(Exception ex){LogError(ex);}
		}
		
		private void CombatHud_ReleaseObject(object sender, ReleaseObjectEventArgs e)
		{
			try
			{
				if(!CombatHudMobTrackingList.Any(x => x.Id == e.Released.Id)){return;}
	
				CombatHudMobTrackingList.RemoveAll(x => x.Id == e.Released.Id);	
				UpdateTactician();

			}catch(Exception ex){LogError(ex);}
		}	
		
		private void CombatHud_ChangePortalMode(object sender, ChangePortalModeEventArgs e)
		{
			try
			{							
				CombatHudMobTrackingList.Clear();
				foreach(WorldObject wo in Core.WorldFilter.GetByObjectClass(ObjectClass.Monster))
				{
					CombatHudMobTrackingList.Add(new MonsterObject(wo));
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
				if(MyCastList.Count == 0 && OtherCastList.Count == 0) {return;}
				
				int AnimationTarget = 0;
				try{AnimationTarget = pMsg.Value<int>(0);}catch{AnimationTarget = 0;}
				if(AnimationTarget == 0) { return;}
				
				if(Core.WorldFilter[AnimationTarget].ObjectClass != ObjectClass.Monster) {return;}
				if(!CombatHudMobTrackingList.Any(x => x.Id == AnimationTarget))
				{
					CombatHudMobTrackingList.Add(new MonsterObject(Core.WorldFilter[AnimationTarget]));
				}
				
				MonsterObject MobTarget = CombatHudMobTrackingList.Find(x => x.Id == AnimationTarget);	
				
				int SpellAnimation = pMsg.Value<int>(1);
				double AnimationDuration = pMsg.Value<double>(2);		
				
				if(MyCastList.Count > 0 && MyCastList.Any(x => x.SpellAnimation == SpellAnimation && x.SpellTargetId == AnimationTarget))
				{
					SpellCastInfo MyCastSpell = MyCastList.Find(x => x.SpellAnimation == SpellAnimation && x.SpellTargetId == AnimationTarget);
					int index = MobTarget.DebuffSpellList.FindIndex(x => x.SpellId == MyCastSpell.SpellCastId);
					
					if(index >= 0)
				   	{
						MobTarget.DebuffSpellList[index].SpellCastTime = DateTime.Now;
						MobTarget.DebuffSpellList[index].SecondsRemaining = SpellIndex[MyCastSpell.SpellCastId].duration;
				   	}
				   	else
				   	{
				   		MonsterObject.DebuffSpell dbspellnew = new MonsterObject.DebuffSpell();
				   		dbspellnew.SpellId = MyCastSpell.SpellCastId;
				   		dbspellnew.SpellCastTime = DateTime.Now;
				   		dbspellnew.SecondsRemaining = SpellIndex[MyCastSpell.SpellCastId].duration;
				   		MobTarget.DebuffSpellList.Add(dbspellnew);	
				   	}
				   	MyCastList.Remove(MyCastSpell);
				   	UpdateTactician();
				}
				
				if(OtherCastList.Count > 0 && OtherCastList.Any(x => x.Animation == SpellAnimation))
				{
					int index_o = OtherCastList.FindIndex(x => x.Animation == SpellAnimation);
					
					if(index_o >= 0)
					{
						OtherDebuffCastInfo OtherCastSpell = OtherCastList[index_o];
						int index = MobTarget.DebuffSpellList.FindIndex(x => x.SpellId == OtherCastSpell.SpellId);
						
						if(index >= 0)
					   	{
							MobTarget.DebuffSpellList[index].SpellCastTime = DateTime.Now;
							MobTarget.DebuffSpellList[index].SecondsRemaining = SpellIndex[OtherCastSpell.SpellId].duration;
					   	}
						else
					   	{
					   		MonsterObject.DebuffSpell dbspellnew = new MonsterObject.DebuffSpell();
					   		dbspellnew.SpellId = OtherCastSpell.SpellId;
					   		dbspellnew.SpellCastTime = DateTime.Now;
					   		dbspellnew.SecondsRemaining = SpellIndex[OtherCastSpell.SpellId].duration;
					   		MobTarget.DebuffSpellList.Add(dbspellnew);	
					   	}
					   	OtherCastList.Remove(OtherCastSpell);
					   	UpdateTactician();
					}
				}
				
				MyCastList.RemoveAll(x => (DateTime.Now - x.CastTime).TotalSeconds > 8);				
				OtherCastList.RemoveAll(x => (DateTime.Now - x.HeardTime).TotalSeconds > 8);
	
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
	        		
        			if((pMsg.Value<int>("flags") & 0x200) == 0x200)
        			{       	
						//Empty try/catch to deal with cast not valid error.  (infrequent)        				
        				try
        				{
        					if(pMsg.Value<int>(22) > 0) {UpdateMonster.HealthMax = pMsg.Value<int>(22);}
	      					if(pMsg.Value<int>(20) > 0) {UpdateMonster.HealthCurrent = pMsg.Value<int>(20);}
        				}catch{}
//        				if(pMsg.Value<int>(20) > 0){CombatHudMobTrackingList.First(x => x.Id == PossibleMobID).StaminaMax = pMsg.Value<int>(20);}
//        				if(pMsg.Value<int>(28) > 0) {CombatHudMobTrackingList.First(x => x.Id == PossibleMobID).StaminaCurrent = pMsg.Value<int>(28);}
//        				if(pMsg.Value<int>(22) > 0) {CombatHudMobTrackingList.First(x => x.Id == PossibleMobID).ManaMax = pMsg.Value<int>(22);}
//        				if(pMsg.Value<int>(29) > 0) {CombatHudMobTrackingList.First(x => x.Id == PossibleMobID).ManaCurrent = pMsg.Value<int>(29);}
						if(UpdateMonster.HealthMax > 0) 
						{
							UpdateMonster.HealthRemaining = Convert.ToInt32((double)UpdateMonster.HealthCurrent/(double)UpdateMonster.HealthMax*100);
						}
						
        			}
				}
        		UpdateTactician();
        		UpdateFocusHud();
			} 
			catch (Exception ex) {LogError(ex);}
		}
		
		private void OnUpdateHealth(Decal.Adapter.Message pMsg)
		{
			try
			{
				int PossibleMobID = 0;
				try {PossibleMobID = Convert.ToInt32(pMsg["object"]);}catch{PossibleMobID = 0;}
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
        		UpdateTactician();
        		UpdateFocusHud();
			} 
			catch (Exception ex) {LogError(ex);}
		}		                                         
		
		private void CombatHud_OnTimerDo(object sender, System.EventArgs e)
		{
			try
			{

					if(CombatHudMobTrackingList.Count == 0) {return;}
					
					CombatHudMobTrackingList.RemoveAll(x => x.ObjectClass != ObjectClass.Monster);
					
					for(int i = CombatHudMobTrackingList.Count -2; i >= 0; i--)
					{
						CombatHudMobTrackingList[i].DistanceAway = Core.WorldFilter.Distance(Core.CharacterFilter.Id, CombatHudMobTrackingList[i].Id);
						for(int j = CombatHudMobTrackingList[i].DebuffSpellList.Count -2; j >= 0; j--)
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

					UpdateTactician();
		
			}catch(Exception ex){LogError(ex);}
		}
		
		private void CombatHud_ChatBoxMessage(object sender, ChatTextInterceptEventArgs e)
		{
			try
			{	
				//WriteToChat("Echo (" + e.Color + ") " + e.Text);
				
				if(e.Color == 7) {e.Eat = true; return;}
				if(e.Color != 17){return;}
				if(e.Text.Trim().StartsWith("You say,")) {e.Eat = true; return;}			
				
				if(!OtherCastQuickKeepString.Any(x => @e.Text.Contains(x))) {return;}				
				if(AnimationList.Any(x => @e.Text.Contains(x.SpellCastWords)))
				{	
					OtherDebuffCastInfo odci = new OtherDebuffCastInfo();
					
					var tanimation = AnimationList.Find(x => @e.Text.Contains(x.SpellCastWords));	
					odci.HeardTime = DateTime.Now;
					odci.SpellWords = tanimation.SpellCastWords;
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
					OtherCastList.Add(odci);
				}
				e.Eat = true;
			}catch(Exception ex){LogError(ex);}
		}
		                                
		private void CombatHud_SpellCast(object sender, SpellCastEventArgs e)
		{
			try
			{
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
								scinfo.SpellAnimation = SpellIndex[e.SpellId].animation;
								MyCastList.Add(scinfo);
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
								scinfo.SpellAnimation = SpellIndex[e.SpellId].animation;
								MyCastList.Add(scinfo);
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
								scinfo.SpellAnimation = SpellIndex[e.SpellId].animation;
								MyCastList.Add(scinfo);
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
								scinfo.SpellAnimation = SpellIndex[e.SpellId].animation;
								MyCastList.Add(scinfo);
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
