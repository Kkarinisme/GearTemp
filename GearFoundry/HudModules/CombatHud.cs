
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
	
		
	
		private ACImage CurrentBar =  new ACImage(Color.DarkRed);
		private ACImage RedBar = new ACImage(Color.Red);
		private ACImage EmptyBar = new ACImage(Color.Black);
		private ACImage DebuffedBar = new ACImage(Color.Goldenrod);
		private ACImage DebuffedCurrentBar = new ACImage(Color.DarkGoldenrod);
				
		
		
		private HudView TacticianHudView = null;
		private HudTabView TacticianHudTabView = null;
		private HudFixedLayout TacticianTabLayout = null;
		private HudFixedLayout TacticianSettingsLayout = null;
		private HudList TacticianDiplayList = null;
		private HudList.HudListRowAccessor TacticianRow = null;
		
		private void RenderTacticianHud()
		{
			try
			{
				
				TacticianHudView = new HudView("GearTactician", gtSettings.CombatHudWidth, gtSettings.CombatHudHeight, new ACImage(0x6AA8));
				TacticianHudView.Visible = true;
				TacticianHudView.UserAlphaChangeable = false;
				TacticianHudView.ShowInBar = false;
				TacticianHudView.UserClickThroughable = false;
				TacticianHudView.UserMinimizable = true;
				TacticianHudView.UserResizeable = true;
				TacticianHudView.LoadUserSettings();
				
				TacticianHudTabView = new HudTabView();
				TacticianHudView.Controls.HeadControl = TacticianHudTabView;
				
				TacticianTabLayout = new HudFixedLayout();
				TacticianHudTabView.AddTab(TacticianTabLayout, "GearTactician");
				
				TacticianSettingsLayout = new HudFixedLayout();
				TacticianHudTabView.AddTab(TacticianSettingsLayout, "Settings");
				
				TacticianDiplayList = new HudList();
				TacticianTabLayout.AddControl(TacticianDiplayList, new Rectangle(0,0,TacticianHudView.Width, TacticianHudView.Height));
				TacticianDiplayList.ControlHeight = 16;
				TacticianDiplayList.AddColumn(typeof(HudPictureBox), 16, null);
				TacticianDiplayList.AddColumn(typeof(HudProgressBar), 100, null);
				TacticianDiplayList.AddColumn(typeof(HudStaticText), 1, null);
				
				TacticianDiplayList.Click += TacticianDiplayList_Click;
				TacticianHudTabView.OpenTabChange += TacticianHudTabView_OpenTabChange;
				TacticianHudView.VisibleChanged += TacticianHudView_VisibleChanged;
				TacticianHudView.Resize += TacticianHudView_Resize;
				
						
				
			}catch(Exception ex){LogError(ex);}
		}
		
		private void TacticianHudView_VisibleChanged(object sender, EventArgs e)
		{
			DisposeTacticianHud();
		}
		
		private void DisposeTacticianHud()
		{
			try
			{
				TacticianHudView.Resize -= TacticianHudView_Resize;
				TacticianHudTabView.OpenTabChange -= TacticianHudTabView_OpenTabChange;
				TacticianHudView.VisibleChanged -= TacticianHudView_VisibleChanged;				
				TacticianDiplayList.Click -= TacticianDiplayList_Click;
				TacticianHudView.Dispose();
				
				TacticianHudTabView.Dispose();
				
				TacticianTabLayout.Dispose();
				TacticianSettingsLayout.Dispose();
				
				TacticianDiplayList.Dispose();
				
			}catch(Exception ex){LogError(ex);}
		}
		
		private void TacticianHudView_Resize(object sender, EventArgs e)
		{
			try
			{
				
				gtSettings.CombatHudHeight = TacticianHudView.Height;
				gtSettings.CombatHudWidth = TacticianHudView.Width;
				
				CombatHudReadWriteSettings(false);
				
			}catch(Exception ex){LogError(ex);}
		}
		
		private void UpdateTactician()
		{
			try
			{
				TacticianDiplayList.ClearRows();
				
				for(int mobindex = 0; mobindex < CombatHudMobTrackingList.Count; mobindex++)
				{
					if(CombatHudMobTrackingList[mobindex].DebuffSpellList.Count > 0 || gtSettings.bShowAll)
					{
						TacticianRow = TacticianDiplayList.AddRow();
						((HudPictureBox)TacticianRow[0]).Image = CombatHudMobTrackingList[mobindex].Icon;
						((HudProgressBar)TacticianRow[1]).FontHeight = 10;
						((HudProgressBar)TacticianRow[1]).PreText = CombatHudMobTrackingList[mobindex].DebuffSpellList.Count.ToString() + " Debuffs";
						((HudProgressBar)TacticianRow[1]).Min = 0;
						((HudProgressBar)TacticianRow[1]).Max = 100;
						((HudProgressBar)TacticianRow[1]).ProgressEmpty = EmptyBar;
						if(CombatHudMobTrackingList[mobindex].Id == Core.Actions.CurrentSelection)
						{
							if(CombatHudMobTrackingList[mobindex].DebuffSpellList.Count > 0)
							{
								((HudProgressBar)TacticianRow[1]).ProgressFilled = DebuffedCurrentBar;
							}
							else
							{
								((HudProgressBar)TacticianRow[1]).ProgressFilled = CurrentBar;
							}
						}
						else
						{
							if(CombatHudMobTrackingList[mobindex].DebuffSpellList.Count > 0)
							{
								((HudProgressBar)TacticianRow[1]).ProgressFilled = DebuffedBar;
							}
							else
							{
								((HudProgressBar)TacticianRow[1]).ProgressFilled = RedBar;
							}	
						}
						
						((HudProgressBar)TacticianRow[1]).Position = CombatHudMobTrackingList[mobindex].HealthRemaining;
						((HudStaticText)TacticianRow[2]).Text = CombatHudMobTrackingList[mobindex].Id.ToString();
	
					}
				}	
			}catch(Exception ex){LogError(ex);}
		}
		
		private HudList.HudListRowAccessor TacticianRowClick = null;
		private void TacticianDiplayList_Click(object sender, int row, int col)
		{
			try
			{
				TacticianRowClick = TacticianDiplayList[row];
				if(col == 1)
				{
					RenderDebuffPop(CombatHudMobTrackingList.Find(x => x.Id == Convert.ToInt32(((HudStaticText)TacticianRowClick[2]).Text)));
				}
				
			}catch(Exception ex){LogError(ex);}
		}

		private HudView DebuffPopView = null;
		private HudTabView DebuffPopTabView = null;
		private HudFixedLayout DebuffPopLayout = null;
		private List<HudImageStack> DebuffPopList = null;
		private Rectangle DebuffRectangle = new Rectangle(0,0,16,16);
		private ACImage DebuffCurrent  = new ACImage(Color.Green);
		private ACImage DebuffWarning = new ACImage(Color.Yellow);
		private ACImage DebuffExpiring = new ACImage(Color.Red);
		private MonsterObject LastTarget = null;
		
		private void RenderDebuffPop(MonsterObject target)
		{
			try
			{
				if(LastTarget != target)
				{
					LastTarget = target;
				}
				
				if(DebuffPopView != null)
				{
					DisposeDebuffPopView();
				}
				
				DebuffPopView = new HudView(target.Name, target.DebuffSpellList.Count * 20, 40, new ACImage(0x6AA3));
				if(DebuffPopView.Width < 120) {DebuffPopView.Width = 120;}
				DebuffPopView.UserAlphaChangeable = false;
				DebuffPopView.ShowInBar = false;
				DebuffPopView.UserResizeable = false;
				DebuffPopView.Visible = true;
				DebuffPopView.Ghosted = gtSettings.DebuffPopGhosted;
				DebuffPopView.UserClickThroughable = false;	
				DebuffPopView.UserMinimizable = true;	
				DebuffPopView.UserGhostable = true;
				DebuffPopView.Location = gtSettings.DebuffPopLocation;
	
				DebuffPopTabView = new HudTabView();
				DebuffPopView.Controls.HeadControl = DebuffPopTabView;
				
				DebuffPopLayout = new HudFixedLayout();
				DebuffPopTabView.AddTab(DebuffPopLayout, "Debuffs");
				
				DebuffPopList = new List<HudImageStack>();
				
				foreach(var debuff in target.DebuffSpellList.OrderBy(x => x.SecondsRemaining))
				{
					HudImageStack debufficon = new HudImageStack();
					if(debuff.SecondsRemaining <= 15) {debufficon.Add(DebuffRectangle, DebuffExpiring);}
					else if(debuff.SecondsRemaining <= 30){debufficon.Add(DebuffRectangle, DebuffWarning);}
					else{debufficon.Add(DebuffRectangle,DebuffCurrent);}
					debufficon.Add(DebuffRectangle, SpellIndex[debuff.SpellId].spellicon);
					DebuffPopList.Add(debufficon);	
				}
				
				for(int i = 0; i < DebuffPopList.Count; i ++)
				{
					DebuffPopLayout.AddControl(DebuffPopList[i], new Rectangle((20*i) + 2,2,16,16));
				}

				DebuffPopView.VisibleChanged += DebuffPopView_VisibleChanged;
				DebuffPopView.GhostedChanged += DebuffPopView_GhostedChanged;
				
				
			}catch(Exception ex){LogError(ex);}
		}
		
		private void DebuffPopView_GhostedChanged(object sender, EventArgs e)
		{
			try
			{
				gtSettings.DebuffPopGhosted = DebuffPopView.Ghosted;
				gtSettings.DebuffPopLocation = DebuffPopView.Location;
				
				CombatHudReadWriteSettings(false);		
			}catch(Exception ex){LogError(ex);}
		}
		
		private void DebuffPopView_VisibleChanged(object sender, EventArgs e)
		{
			try
			{
				gtSettings.DebuffPopGhosted = DebuffPopView.Ghosted;
				gtSettings.DebuffPopLocation = DebuffPopView.Location;
				
				CombatHudReadWriteSettings(false);
				DisposeDebuffPopView();		
			}catch(Exception ex){LogError(ex);}
		}
		
		private void DisposeDebuffPopView()
		{
			try
			{
				DebuffPopView.VisibleChanged -= DebuffPopView_VisibleChanged;
				DebuffPopView.GhostedChanged -= DebuffPopView_GhostedChanged;
				if(DebuffPopList.Count != 0)
				{
					foreach(var debuff in DebuffPopList)
					{
						debuff.Dispose();
					}
				}
				DebuffPopList = null;
				DebuffPopView.Dispose();
				DebuffPopTabView.Dispose();
				DebuffPopLayout.Dispose();
				
			}catch(Exception ex){LogError(ex);}
		}
			
		private void TacticianHudTabView_OpenTabChange(object sender, EventArgs e)
		{
			try
			{
				if(TacticianHudTabView.CurrentTab == 1)
				{
					RenderCombatHudSettingsPopUp();
				}
			}catch(Exception ex){LogError(ex);}
		}
		
		private HudView CHSetPopView = null;
		private HudTabView CHSetPopTabView = null;
		private HudFixedLayout CHSetPopLayout = null;
		private HudCheckBox CHTrackCreature = null;
		private HudCheckBox CHTrackItem = null;
		private HudCheckBox CHTrackLife = null;
		private HudCheckBox CHTRackVoid = null;
		private HudCheckBox CHShowAll = null;
		
		private void RenderCombatHudSettingsPopUp()
		{
			try
			{
				if(CHSetPopView != null)
				{
					DisposeCombatHudSettingsPopUp();
				}
				
				
				CHSetPopView = new HudView("Tactician Settings", 200, 200, null);
				CHSetPopView.UserAlphaChangeable = false;
				CHSetPopView.ShowInBar = false;
				CHSetPopView.UserResizeable = true;
				CHSetPopView.Visible = true;
				CHSetPopView.Ghosted = false;
				CHSetPopView.UserClickThroughable = false;	
				CHSetPopView.UserMinimizable = true;	
				CHSetPopView.UserGhostable = false;
				CHSetPopView.Location = TacticianHudView.Location;
				
				CHSetPopTabView = new HudTabView();
				CHSetPopView.Controls.HeadControl = CHSetPopTabView;
				
				CHSetPopLayout =  new HudFixedLayout();
				CHSetPopTabView.AddTab(CHSetPopLayout, "Settings");
							
				CHTrackCreature = new HudCheckBox();
				CHTrackCreature.Text = "Creature Debuffs";
				CHTrackCreature.Checked = gtSettings.bCombatHudTrackCreatureDebuffs;
				CHSetPopLayout.AddControl(CHTrackCreature, new Rectangle(0,0,100,16));
				
				CHTrackItem = new HudCheckBox();
				CHTrackItem.Text = "Item Debuffs";
				CHTrackItem.Checked = gtSettings.bCombatHudTrackItemDebuffs;
				CHSetPopLayout.AddControl(CHTrackItem, new Rectangle(0,20,100,16));
				
				CHTrackLife = new HudCheckBox();
				CHTrackLife.Text = "Life Debuffs";
				CHTrackLife.Checked = gtSettings.bCombatHudTrackLifeDebuffs;
				CHSetPopLayout.AddControl(CHTrackLife, new Rectangle(0,40,100,16));
				
				CHTRackVoid = new HudCheckBox();
				CHTRackVoid.Text = "Void Debuffs";
				CHTRackVoid.Checked = gtSettings.bCombatHudTrackVoidDebuffs;
				CHSetPopLayout.AddControl(CHTRackVoid, new Rectangle(0,80,100,16));
				
				CHShowAll = new HudCheckBox();
				CHShowAll.Text = "Show All Mobs";
				CHShowAll.Checked = gtSettings.bShowAll;
				CHSetPopLayout.AddControl(CHShowAll, new Rectangle(0,100,100,16));
				
				CHSetPopView.VisibleChanged += CHSetPopView_VisibleChanged;		
				
			}catch(Exception ex){LogError(ex);}
		}
		
		private void CHSetPopView_VisibleChanged(object sender, EventArgs e)
		{
			try
			{
				
				gtSettings.bCombatHudTrackCreatureDebuffs = CHTrackCreature.Checked; 
				gtSettings.bCombatHudTrackItemDebuffs = CHTrackItem.Checked;
				gtSettings.bCombatHudTrackLifeDebuffs = CHTrackLife.Checked;
				gtSettings.bCombatHudTrackVoidDebuffs = CHTRackVoid.Checked;
				gtSettings.bShowAll = CHShowAll.Checked;
				
				CombatHudReadWriteSettings(false);
				
				DisposeCombatHudSettingsPopUp();
				
				TacticianHudTabView.CurrentTab = 0;
								
			}catch(Exception ex){LogError(ex);}
		}
		
		private void DisposeCombatHudSettingsPopUp()
		{
			try
			{
				CHSetPopView.VisibleChanged -= CHSetPopView_VisibleChanged;		
				
				CHSetPopView.Dispose();
				CHSetPopTabView.Dispose();
				CHSetPopLayout.Dispose();
				CHTrackCreature.Dispose();
				CHTrackItem.Dispose();
				CHTrackLife.Dispose();
				CHTRackVoid.Dispose();
				CHShowAll.Dispose();
			}catch(Exception ex){LogError(ex);}
		}		
	}
}


//Vuln's can *sorta* be tracked. 
//
//If you look at 0xF755 Apply Visual/Sound Effect and look at the "effect" field ... 
//
//0x002B = Fire Vuln 
//0x002D = Pierce Vuln 
//0x002F = Blade Vuln 
//0x0031 = Acid Vuln 
//0x0033 = Cold Vuln 
//0x0035 = Lightning Vuln 
//0x0037 = Bludgeon Vuln or Imperil 
//
//Then look at the "playSpeed" field: 
//
//Level 1 = 0.0 
//Level 2 = 0.2 
//Level 3 = 0.4 
//Level 4 = 0.6 
//Level 5 = 0.8 
//Level 6 or 7 = 1.0 
//
//Protections are similar (although I don't have exact values.) The problems with this are that you don't get this info if you were not "aware" of the monster when it got vulned, you can't tell the difference between a bludgeon vuln and an imperil and you can't tell the difference between a level 6 and 7 spell. 
//
//So, I don't know if that helps you any, but it's the best way I have found to tell what has been vulned.





//Audio/Visual Effect ID
//0x04
//War Launch
//0x05
//War Land
//0x06
//Red Clouds Rising (Strength/Health Buff)
//0x07
//Red Clouds Falling (Strength/Health Debuff)
//0x08
//Orange Clouds Rising (Coordination Buff)
//0x09
//Orange Clouds Falling (Coordination Debuff)
//0x0A
//Yellow Clouds Rising (Endurance Buff)
//0x0B
//Yellow Clouds Falling (Endurance Debuff)
//0x0C
//Green Clouds Rising (Quickness Buff)
//0x0D
//Green Clouds Falling (Quickness Debuff)
//0x0E
//Cyan Clouds Rising (Self Buff, Lifestone Recall/Tie)
//0x0F
//Cyan Clouds Falling (Self Debuff)
//0x10
//Purple Clouds Rising (Focus Buff, Portal Recall/Summon/Tie)
//0x11
//Purple Clouds Falling (Focus Debuff)
//0x12
//Red Bubbles Rising (Weapon Skill Buff)
//0x13
//Red Bubbles Falling (Weapon Skill Debuff)
//0x14
//Orange Bubbles Rising (Allegiance/Crafting Skill Buff)
//0x15
//Orange Bubbles Falling (Allegiance/Crafting Skill Debuff)
//0x16
//Yellow Bubbles Rising (Defense Skill Buff)
//0x17
//Yellow Bubbles Falling (Defense Skill Debuff)
//0x18
//Green Bubbles Rising (Run/Jump Skill Buff)
//0x19
//Green Bubbles Falling (Run/Jump Skill Debuff)
//0x1A
//Cyan Bubbles Rising (Magic/Alchemy Skill Buff)
//0x1B
//Cyan Bubbles Falling (Magic/Alchemy Skill Debuff)
//0x1C
//Purple Bubbles Rising (Assessment/Tinkering Skill Buff, Learn Spell)
//0x1D
//Purple Bubbles Falling (Assessment/Tinkering Skill Debuff)
//0x1E
//Red Stars In (Heal, Infuse Health)
//0x1F
//Red Stars Out (Harm, Drain Health)
//0x20
//Blue Stars In (Mana Boost, Infuse Mana)
//0x21
//Blue Stars Out (Mana Drain, Drain Mana)
//0x22
//Yellow Stars In (Revitalize, Infuse Stamina)
//0x23
//Yellow Stars Out (Enfeeble, Drain Stamina)
//0x24
//Red Stars Rotating Out (Regeneration)
//0x25
//Red Stars Rotating In (Fester)
//0x26
//Blue Stars Rotating Out (Mana Renewal)
//0x27
//Blue Stars Rotating In (Mana Depletion)
//0x28
//Yellow Stars Rotating In (Rejuvenation)
//0x29
//Yellow Stars Rotating Out (Exhaustion)
//0x2A
//Red Shield Rising (Fire Protection)
//0x2B
//Red Shield Falling (Fire Vulnerability)
//0x2C
//Orange Shield Rising (Piercing Protection)
//0x2D
//Orange Shield Falling (Piercing Vulnerability)
//0x2E
//Yellow Shield Rising (Blade Protection)
//0x2F
//Yellow Shield Falling (Blade Vulnerability)
//0x30
//Green Shield Rising (Acid Protection)
//0x31
//Green Shield Falling (Acid Vulnerability)
//0x32
//Cyan Shield Rising (Cold Protection)
//0x33
//Cyan Shield Falling (Cold Vulnerability)
//0x34
//Purple Shield Rising (Lightning Protection)
//0x35
//Purple Shield Falling (Lightning Vulnerability)
//0x36
//Black Shield Rising (Bludgeon Protection, Armor)
//0x37
//Black Shield Falling (Bludgeon Vulnerability, Imperil)
//0x38
//Red/White Sparks (Flame Bane, Blood Drinker)
//0x39
//Red/Black Sparks (Flame Lure, Blood Loather)
//0x3A
//Orange/White Sparks (Piercing Bane, Heart Seeker, Strengthen Lock)
//0x3B
//Orange/Black Sparks (Piercing Lure, Turn Blade, Weaken Lock)
//0x3C
//Yellow/White Sparks (Blade Bane, Defender)
//0x3D
//Yellow/Black Sparks (Blade Lure, Lure Blade)
//0x3E
//Green/White Sparks (Acid Bane, Swift Killer)
//0x3F
//Green/Black Sparks (Acid Lure, Leaden Weapon)
//0x40
//Cyan/White Sparks (Frost Bane)
//0x41
//Cyan/Black Sparks (Frost Lure)
//0x42
//Purple/White Sparks (Bludgeon/Lightning Bane, Hermetic Link)
//0x43
//Purple/Black Sparks (Bludgeon/Lightning Lure, Hermetic Void, Dispel)
//0x48
//Red Stars Out / Yellow Stars In (Health to Stamina)
//0x49
//Red Stars Out / Blue Stars In (Health to Mana)
//0x4A
//Yellow Stars Out / Red Stars In (Stamina to Health)
//0x4B
//Yellow Stars Out / Blue Stars In (Stamina to Mana)
//0x4C
//Blue Stars Out / Red Stars In (Mana to Health)
//0x4D
//Blue Stars Out / Yellow Stars In (Mana to Stamina)
//0x50
//Fizzle
//0x57
//Idle Emote
//0x58
//Item Dissolve
//0x73
//Portal Bubbles
//0x76
//Raise Attribute or Skill
//0x77
//Equip Item
//0x78
//Unequip Item
//0x79
//Give Item
//0x7A
//Pick Up Item
//0x7B
//Drop Item
//0x7E
//Unlock Item
//0x81
//Enchantment Expiration
//0x82
//Item Out of Mana
//0x89
//Gain Level
//0x8D
//White/White Sparks (Impenetrability)
//0x8E
//White/Black Sparks (Brittlemail)
//0x91
//White/Purple Clouds (Life Dispel)
//0x92
//White/Cyan Clouds (Creature Dispel)
//
//

//void
//AnimationList.Add("Zojak Bor",100691559,4,0);  //Corrosion
//AnimationList.Add("Jevak Bor",100691561,4,0);  //Corruption
//AnimationList.Add("Drosta Ves",100691552,168,0);  //Festering Curse 
//AnimationList.Add("Traku Ves",100691553,169,0);  //Weakening Curse
//AnimationList.Add("Slavu Bor",100691551,167,0);  //Destructive Curse
//Zojak Ves = nether arc
//Tugak Ves = Blast
//Zojak Ves = nether bolt
//Jevak Ves = nether streak

//Creature
//AnimationList.Add("Equin Eatak",100670578,27,1);  //Bottle Breaker
//AnimationList.Add("Equin Eaja",100668285,27,1);  //Hands of Chorizite
//AnimationList.Add("Equin Oloi",100668352,29,1);  //Jibril's Vitae
//AnimationList.Add("Equin Caril",100668277,17,1);  //Synaptic Misfire
//AnimationList.Add("Equin Cavik",100668268,9,1);  //Ataxia
//AnimationList.Add("Equin Guafeth",100670579,21,1);  //Challenger's Legacy
//AnimationList.Add("Equin Ealoi",100668358,27,1);  //Wrath of Adja
//AnimationList.Add("Equin Gualoi",100668296,29,1);  //Hearts on Sleeves
//AnimationList.Add("Equin Ofeth",100669126,23,1);  //Broadside of a Barn
//AnimationList.Add("Equin Luvik",100692227,21,1);  //Dirty Fighting Ineptitude Other VII
//AnimationList.Add("Equin Lureth",100692228,21,1);  //Dual Wield Ineptitude Other VII
//AnimationList.Add("Equin Guavik",100668286,21,1);  //Sashi Mu's Kiss
//AnimationList.Add("Equin Casith",100668296,15,1);  //Self Loathing
//AnimationList.Add("Equin Luzael",100692225,19,1);  //Finesse Weapon Ineptitude Other VII
//AnimationList.Add("Equin Guareth",100670580,21,1);  //Twisted Digits
//AnimationList.Add("Equin Careth",100668273,11,1);  //Brittle Bones
//AnimationList.Add("Equin Guaguz",100668279,21,1);  //Unsteady Hands
//AnimationList.Add("Equin Luril",100692226,19,1);  //Heavy Weapon Ineptitude Other VII
//AnimationList.Add("Equin Eapaj",100668272,27,1);  //Wrath of Celcynd
//AnimationList.Add("Equin Guaja",100668264,29,1);  //Unfortunate Appraisal
//AnimationList.Add("Equin Guasith",100668282,25,1);  //Feat of Radaz
//AnimationList.Add("Equin Guati",100668295,25,1);  //Gears Unwound
//AnimationList.Add("Equin Guatak",100668283,21,1);  //Kwipetian Vision
//AnimationList.Add("Equin Eavik",100668337,27,1);  //Wrath of Harlune
//AnimationList.Add("Equin Lutak",100692224,19,1);  //Light Weapon Ineptitude Other VII
//AnimationList.Add("Equin Guaril",100668284,163,1);  //Fat Fingers
//AnimationList.Add("Equin Eareth",100668354,29,1);  //Eyes Clouded
//AnimationList.Add("Puish Zharil",100668351,76,1);  //Meditative Trance
//AnimationList.Add("Equin Opaj",100668330,23,1);  //Futility
//AnimationList.Add("Equin Easith",100668288,27,1);  //Inefficient Investment
//AnimationList.Add("Equin Hatak",100668266,19,1);  //Missile Weapon Ineptitude Other VII
//AnimationList.Add("Equin Guapaj",100668353,29,1);  //Ignorance's Bliss
//AnimationList.Add("Equin Guazael",100668355,29,1);  //Introversion
//AnimationList.Add("Equin Hati",100668265,21,1);  //Recklessness Ineptitude Other VII
//AnimationList.Add("Equin Hafeth",100692229,21,1);  //Shield Ineptitude Other VII
//AnimationList.Add("Equin Caja",100668294,13,1);  //Belly of Lead
//AnimationList.Add("Equin Luguz",100692230,21,1);  //Sneak Attack Ineptitude Other VII
//AnimationList.Add("Equin Eaves",100691575,27,1);  //Void Magic Ineptitude Other VII
//AnimationList.Add("Equin Ozael",100668331,23,1);  //Gravity Well
//AnimationList.Add("Equin Eati",100668272,27,1);  //Wrath of the Hieromancer
//AnimationList.Add("Equin Cazael",100668300,7,1);  //Senescence
//AnimationList.Add("Equin Luja",100668357,29,1);  //Eye of the Grunt

//Life
//AnimationList.Add("Cruath Qualoi",100668344,50,1);  //Olthoi's Gift
//AnimationList.Add("Cruath Quaguz",100668348,48,1);  //Swordsman's Gift
//AnimationList.Add("Cruath Quareth",100668345,56,1);  //Tusker's Gift
//AnimationList.Add("Cruath Quavik",100668292,164,1);  //Gelidite's Gift
//AnimationList.Add("Yanoi Zhavik",100668299,42,1);  //Enervation
//AnimationList.Add("Yanoi Zhapaj",100668279,38,1);  //Decrepitude's Grasp
//AnimationList.Add("Cruath Quatak",100668291,44,1);  //Inferno's Gift
//AnimationList.Add("Cruath Quasith",100668293,56,1);  //Gossamer Flesh
//AnimationList.Add("Cruath Quafeth",100668346,54,1);  //Astyrrian's Gift
//AnimationList.Add("Yanoi Zhaloi",100668288,163,1);  //Energy Flux
//AnimationList.Add("Cruath Quaril",100668347,46,1);  //Archer's Gift

//Item
//AnimationList.Add("Equin Qualoi",100673974,64,1);  //Acid Lure VI
//AnimationList.Add("Equin Quaguz",100673980,62,1);  //Blade Lure VI
//AnimationList.Add("Equin Quareth",100673975,68,1);  //Bludgeon Lure VI
//AnimationList.Add("Equin Quasith",100673981,143,1);  //Brittlemail VI
//AnimationList.Add("Equin Quatak",100673976,58,1);  //Flame Lure VI
//AnimationList.Add("Equin Quavik",100673977,66,1);  //Frost Lure VI
//AnimationList.Add("Malar Aevik",100673983,68,1);  //Hermetic Void VI
//AnimationList.Add("Equin Aetak",100673990,64,1);  //Leaden Weapon VI
//AnimationList.Add("Equin Quafeth",100673978,68,1);  //Lightning Lure VI
//AnimationList.Add("Equin Aeguz",100673985,62,1);  //Lure Blade VI
//AnimationList.Add("Equin Quaril",100673979,60,1);  //Piercing Lure VI
//AnimationList.Add("Equin Aeril",100676646,58,1);  //Spirit Loather VI
//AnimationList.Add("Equin Aereth",100673992,60,1);  //Turn Blade VI
//AnimationList.Add("Equin Aeti",100668401,60,1);  //Weaken Lock VI
//AnimationList.Add("Equin Quasith",100673981,143,1);  //Tattercoat

