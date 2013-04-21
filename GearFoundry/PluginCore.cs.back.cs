///////////////////////////////////////////////////////////////////////////////
//This file is Copyright (c) 2011 VirindiPlugins
//
//Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
//
//The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
//
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.
///////////////////////////////////////////////////////////////////////////////

using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using Decal.Adapter;
using System.IO;
using System.Xml.Serialization;
using Microsoft.Win32;
using Decal.Filters;
using System.Runtime.InteropServices;
using Decal.Adapter.Wrappers;
using System.Drawing;
using System.Xml.Linq;
using System.ComponentModel;
using VirindiViewService;
using MyClasses.MetaViewWrappers;





namespace AlincoVVS
{	
    public partial class PluginCore : Decal.Adapter.PluginBase
    {
    	GameData iGameData = new GameData();
    	
    	// AlincoBuffs will be purged....They can always be reinstated later.
    	//external declarations, shared across all instances
    	
		static internal FileService FileService;
		static internal RenderServiceWrapper RenderServiceForHud;
		static internal Decal.Adapter.Wrappers.HooksWrapper HooksForErrorHandler;
		
		private PluginSettings mPluginConfig;
		private CharSettings mCharconfig;
	
		private WorldSettings mWorldConfig;
		private string mPluginConfigfilename;
		private string mCharConfigfilename;
		private string mWorldConfigfilename;
		private string mWorldInventoryname;
	
		private string mExportInventoryname;
		private string mSalvageBasefilename;
	
		private System.Windows.Forms.Timer MasterTimer;
	
		private bool mInportalSpace;
//		We're also done making noise, there has to be a better way to do this than current
//		private mediaplayer mplayer;
		private BackgroundWorker mBackgroundworker;
	
		private bool mFilesLoaded;
        internal static Decal.Adapter.Wrappers.PluginHost host = null;

        //View component instances
       	private static MainView mainview = null;    	

		//Various overrides
		protected override void Startup()
		{
			InitCtrlEvents();
		}
		
		protected override void Shutdown()
		{
			EndCtrlEvents();
		}
		
				
//		public partial class Plugin
//		{
//		try {
//				minstance = this;
//				//associates minstance with this particular instance of Alinco, minstance defined in Plugin.Public
//	
//				Util.docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
//				//Paths to Alinco files, references
//				Util.docPath = System.IO.Path.Combine(Util.docPath, "Decal Plugins\\Alinco3");
//				Util.wavPath = System.IO.Path.GetDirectoryName(this.GetType().Module.Assembly.Location);
//				Util.wavPath = System.IO.Path.Combine(Util.wavPath, "Wav");
//	
//				//Don't exist?  Make them
//				if (!Directory.Exists(Util.docPath)) {
//					Directory.CreateDirectory(Util.docPath);
//				}
//	
//				System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();
//				//information 
//				System.Version AppVersion = asm.GetName().Version;
//				Util.dllversion = AppVersion.ToString();
//	
//				Util.StartLog();
//				mPluginConfigfilename = (Util.docPath + "\\settings.xml");
//				Util.NumberFormatInfo = new System.Globalization.NumberFormatInfo();
//				Util.NumberFormatInfo.NumberDecimalSeparator = ".";
//				Util.NumberFormatInfo.NumberGroupSeparator = ",";
//
//
//			} catch (Exception ex) {
//				Util.ErrorLogger(ex);
//			}
//		}
		
		private string dcsxmlfile()
		{
			string path = null;
			// first try to load original file
			try {
				RegistryKey regKey = null;
				regKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Decal\\Plugins\\{5522f031-4d14-4bc1-a6f1-09a1fb36f90e}", false);
				if (regKey != null) {
					path = Convert.ToString(regKey.GetValue("Path", string.Empty));
					path = System.IO.Path.Combine(path, "dcs.xml");
					if (System.IO.File.Exists(path)) {
						return path;
					}
				}
			// 
			} catch	{
			}
	
			// use distributed file
			path = System.IO.Path.Combine(Util.docPath, "dcs.xml");
			if (System.IO.File.Exists(path)) {
				return path;
			}
	
			return string.Empty;
		}
		
		private void loadcolortable()
		{
			try {
				string xmlfilename = dcsxmlfile();
				if (System.IO.File.Exists(xmlfilename)) {
					XDocument xdoc = XDocument.Load(xmlfilename);
	
					foreach (XElement a_loopVariable in xdoc.Root.Element("colortable").Elements()) {
						var a = a_loopVariable;
						int id = 0;
	
	
						if (int.TryParse(Convert.ToString(a.Attribute("id")), out id)) {
							if (!mColorStrings.ContainsKey(id)) {
								mColorStrings.Add(id, Convert.ToString(a.Attribute("name")));
							}
	
						}
	
					}
	
				}
			} catch (Exception ex) {
				Util.ErrorLogger(ex);
			}
		}
		
		private Dictionary<int, modeldata> mModelData = new Dictionary<int, modeldata>();
		private Dictionary<int, string> mColorStrings = new Dictionary<int, string>();
		

		
		private void LoadfilesBackground(System.Object sender, DoWorkEventArgs e)
		{
			string aerror = string.Empty;
			try {
				if (sender != null) {
					BackgroundWorker worker = (BackgroundWorker)sender;
					worker.DoWork -= LoadfilesBackground;
				}
	
	
				if (File.Exists(mPluginConfigfilename)) {
					this.mPluginConfig = (PluginSettings)Util.DeSerializeObject(mPluginConfigfilename, typeof(PluginSettings));
				}
	
				//create default file
				if (mPluginConfig == null) {
					mPluginConfig = new PluginSettings();
					mPluginConfig.Shortcuts = new SDictionary<string, string>();
					mPluginConfig.Shortcuts.Add("hr", "House Recall");
					mPluginConfig.Shortcuts.Add("mr", "House Mansion_Recall");
					mPluginConfig.Shortcuts.Add("ah", "Allegiance Hometown");
					mPluginConfig.Shortcuts.Add("ls", "Lifestone");
					mPluginConfig.Shortcuts.Add("mp", "Marketplace");
					mPluginConfig.Alerts = getbaseAlerts();
					mPluginConfig.AlertKeyMob = "Monster";
					mPluginConfig.AlertKeyPortal = "Portal";
					mPluginConfig.AlertKeySalvage = "Salvage";
					mPluginConfig.AlertKeyScroll = "Salvage";
					mPluginConfig.AlertKeyThropy = "Trophy";
					mPluginConfig.Alertwawfinished = "finished.wav";
				}
				//HACK:  Still hating the sounds	
//				mplayer = new mediaplayer();
//				mplayer.Volume = mPluginConfig.wavVolume;
				loadcolortable();
	
				//Check for Gamedata.xml
				if (!File.Exists(Util.docPath + "\\GameData.xml")) {
					//Create it if !exists
					iGameData.defaultfill();
					Util.SerializeObject(Util.docPath + "\\GameData.xml", iGameData);
				}
	
				//Now that it exists, check version and update if needed
				if (File.Exists(Util.docPath + "\\GameData.xml")) {
					iGameData = (AlincoVVS.PluginCore.GameData)Util.DeSerializeObject(Util.docPath + "\\GameData.xml", typeof(GameData));
					//Look to code this for a single line revision of Gamedata version, currently must update in 2 locations
					if (iGameData.version < 10) {
						iGameData.defaultfill();
						Util.SerializeObject(Util.docPath + "\\GameData.xml", iGameData);
					}
				}
	
				mWorldConfigfilename = Util.docPath + "\\" + Util.normalizePath(Core.CharacterFilter.Server);
				if (!Directory.Exists(mWorldConfigfilename)) {
					Directory.CreateDirectory(mWorldConfigfilename);
				}
	
				mExportInventoryname = Util.docPath + "\\Inventory";
				if (!Directory.Exists(mExportInventoryname)) {
					Directory.CreateDirectory(mExportInventoryname);
					writebasexslt(mExportInventoryname + "\\Inventory.xslt");
				}
	
				mExportInventoryname += "\\" + Core.CharacterFilter.Server + ".xml";
				mWorldInventoryname = Util.docPath + "\\" + Util.normalizePath(Core.CharacterFilter.Server) + "\\Inventory.xml";
				if (File.Exists(mWorldInventoryname)) {
					object tobj = Util.DeSerializeObject(mWorldInventoryname, typeof(SDictionary<int, InventoryItem>));
					if (tobj != null) {
						mGlobalInventory = (global::AlincoVVS.PluginCore.SDictionary<int, global::AlincoVVS.PluginCore.InventoryItem>)tobj;
						mStorageInfo = (global::AlincoVVS.PluginCore.SDictionary<int, string>)Util.DeSerializeObject(mWorldInventoryname + "storage", typeof(SDictionary<int, string>));
					} else {
						mGlobalInventory = new SDictionary<int, InventoryItem>();
					}
				} else {
					writebasexslt(Util.docPath + "\\" + Util.normalizePath(Core.CharacterFilter.Server) + "\\Inventory.xslt");
				}
	
				mCharConfigfilename = mWorldConfigfilename;
				mWorldConfigfilename += "\\Settings.xml";
				if (File.Exists(mWorldConfigfilename)) {
					mWorldConfig = (WorldSettings)Util.DeSerializeObject(mWorldConfigfilename, typeof(WorldSettings));
				}
	
				mCharConfigfilename += "\\" + Util.normalizePath(Core.CharacterFilter.Name) + ".xml";
				if (File.Exists(mCharConfigfilename)) {
					mCharconfig = (CharSettings)Util.DeSerializeObject(mCharConfigfilename, typeof(CharSettings));
				}
	
				if (mWorldConfig == null) {
					mWorldConfig = new WorldSettings();
				}
	
				if (mCharconfig == null) {
					mCharconfig = new CharSettings();
				}
	
				mProtectedCorpses.Add("Corpse of " + Core.CharacterFilter.Name);
				mFilesLoaded = true;
	
			} catch (Exception ex) {
				aerror = ex.Message + ex.StackTrace;
				Util.ErrorLogger(ex);
			}
	
			if (aerror != string.Empty) {
				Util.bcast(aerror);
			}
		}
	
		//Drawing menus
		private void initializeView()
		{
			try {
				HooksForErrorHandler = Host.Actions;
//				cboAlertSound.Clear();
	
				// No sounds
//				if (Directory.Exists(Util.wavPath)) {
//					var wavfiles = from f in Directory.GetFiles(Util.wavPath)orderby f;
//	
//					if ((wavfiles != null)) {
//						foreach (string w in wavfiles) {
//							string wfile = System.IO.Path.GetFileName(w);
//							cboAlertFinished.Add(wfile, w);
//							cboAlertSound.Add(wfile, w);
//						}
//					}
//				} else {
//					Directory.CreateDirectory(Util.wavPath);
//				}
	
				addOption(mPluginConfig.Showpalette, eOtherOptions.Use_DCS_Color_palette_xml);
				addOption(mPluginConfig.Showhud, eOtherOptions.Show_Hud);
				addOption(mPluginConfig.Showhudvulns, eOtherOptions._Display_Vuln_Icons);
				addOption(mPluginConfig.Showhudcorpses, eOtherOptions._Display_Corpses);
	
				addOption(mCharconfig.ShowhudQuickSlots, eOtherOptions.Show_Quickslots_Hud);
	
				addOption(mPluginConfig.OutputManualIdentify, eOtherOptions.Show_Info_on_Identify);
				addOption(mPluginConfig.CopyToClipboard, eOtherOptions._Copy_To_Clipboard);
				//addOption(mPluginConfig.OutputManualIgnoreSelf, eOtherOptions._Ignore_Wielded_Items)
				//addOption(mPluginConfig.OutputManualIgnoreMobs, eOtherOptions._Ignore_Mobs)
				addOption(mPluginConfig.worldbasedsalvage, eOtherOptions.World_Based_Salvaging_Profile);
				addOption(mPluginConfig.worldbasedrules, eOtherOptions.World_Based_Rules_Profile);
				addOption(mCharconfig.usesalvageprofile, eOtherOptions.Character_Salvaging_Profile);
				addOption(mCharconfig.uselootprofile, eOtherOptions.Character_Loot_Profile);
				addOption(mCharconfig.usemobsprofile, eOtherOptions.Character_Mobs_Profile);
				addOption(mPluginConfig.SalvageHighValue, eOtherOptions.Salvage_High_Value_Items);
				addOption(mPluginConfig.FilterChatMeleeEvades, eOtherOptions.Filter_Melee_Evades);
				addOption(mPluginConfig.FilterChatResists, eOtherOptions.Filter_Resists);
				addOption(mPluginConfig.FilterSpellcasting, eOtherOptions.Filter_Spellcasting);
				addOption(mPluginConfig.FilterSpellsExpire, eOtherOptions.Filter_Spells_Expire);
				addOption(mPluginConfig.FilterTellsMerchant, eOtherOptions.Filter_Vendor_Tells);
				addOption(mPluginConfig.D3DMark0bjects, eOtherOptions.Show_3D_Arrow);
				addOption(mPluginConfig.MuteAll, eOtherOptions.Mute);
				addOption(mPluginConfig.AutoStacking, eOtherOptions.Auto_Stacking);
				addOption(mPluginConfig.AutoPickup, eOtherOptions.Auto_Pickup);
				addOption(mPluginConfig.AutoUst, eOtherOptions.Auto_Ust);
				//     addOption(mPluginConfig.AutoUstOnCloseCorpse, eOtherOptions._Salvage_When_Closing_Corpse)
	
				addOption(mPluginConfig.WindowedFullscreen, eOtherOptions.Windowed_Fullscreen);
	
				addNotifyOption(mPluginConfig.notifycorpses, eNotifyOptions.Notify_Corpses);
				addNotifyOption(mPluginConfig.showallcorpses, eNotifyOptions._Show_All_Corpses);
				addNotifyOption(mPluginConfig.NotifyPortals, eNotifyOptions.Notify_On_Portals);
				addNotifyOption(mPluginConfig.unknownscrolls, eNotifyOptions.Detect_Unknows_Scrolls_Lv7);
				addNotifyOption(mPluginConfig.trainedscrollsonly, eNotifyOptions._Trained_Schools_Only);
				addNotifyOption(mPluginConfig.unknownscrollsAll, eNotifyOptions._All_Levels);
				addNotifyOption(mCharconfig.detectscrollsontradebot, eNotifyOptions._Detect_on_Tradebot);
				addNotifyOption(mCharconfig.ShowAllPlayers, eNotifyOptions.Notify_All_Players);
				addNotifyOption(mCharconfig.ShowAllMobs, eNotifyOptions.Notify_All_Mobs);
				addNotifyOption(mCharconfig.useglobalspellbook, eNotifyOptions._Use_Global_Spellbook);
				addNotifyOption(mPluginConfig.notifyalleg, eNotifyOptions.Notify_Allegiance_Members);
				addNotifyOption(mPluginConfig.notifytells, eNotifyOptions.Notify_On_Tell_Recieved);
	
				txtmaxmana.Text = Convert.ToString(mPluginConfig.notifyItemmana);
				txtvbratio.Text = Convert.ToString(mPluginConfig.notifyValueBurden);
				txtmaxvalue.Text = Convert.ToString(mPluginConfig.notifyItemvalue);
				txtsettingscw.Text = Convert.ToString(mPluginConfig.chattargetwindow);
				chksalvageAll.Checked = mPluginConfig.SalvageHighValue;
				loadsalvage();
				LoadThropyList();
				LoadMobsList();
				loadRules();
	
				populateRulesView();
//				populateAlertView();
//				if (cboAlertEdit.Count > 0) {
//					cboAlertEdit.Selected = 0;
//				}
//	
				txtsalvageaug.Text = Convert.ToString(mCharconfig.salvageaugmentations);
				if (mPluginConfig.Shortcuts == null) {
					mPluginConfig.Shortcuts = new SDictionary<string, string>();
					mPluginConfig.Shortcuts.Add("hr", "House Recall");
					mPluginConfig.Shortcuts.Add("mr", "House Mansion_Recall");
					mPluginConfig.Shortcuts.Add("ah", "Allegiance Hometown");
					mPluginConfig.Shortcuts.Add("ls", "Lifestone");
					mPluginConfig.Shortcuts.Add("mp", "Marketplace");
				}
	
				if ((Core.HotkeySystem != null)) {
					Core.HotkeySystem.Hotkey += ACHotkeys_Hotkey;
					setupHotkey("Alinco3", "alinco3:useloadust", "Use Ust and load salvage panel ");
					setupHotkey("Alinco3", "alinco3:salvage", "Presses the salvage button");
					setupHotkey("Alinco3", "alinco3:lootitem", "Loots the first matched item or opens corpse");
					setupHotkey("Alinco3", "alinco3:healself", "Use healkit");
					setupHotkey("Alinco3", "alinco3:givenpc", "Gives one item to the selected npc");
					setupHotkey("Alinco3", "alinco3:onclickust", "load ust and salvage");
					setupHotkey("Alinco3", "alinco3:onoff", "Toggle on off");
	
					setupHotkey("Alinco3", "alinco3:targetmob", "Select nearest mob, double click for next target");
					setupHotkey("Alinco3", "alinco3:targetimp", "switch");
					setupHotkey("Alinco3", "alinco3:targetvuln", "switch");
				}
				
				//  No Alincobuffs
//				try {
//					tryloadAlincoBuffs();
//				} catch (Exception ex) {
//					//      Util.ErrorLogger(ex)
//				}
	
				if (mPluginConfig.WindowedFullscreen) {
					WinApe.WindowedFullscreen(Host.Decal.Hwnd);
				}
	
				mFreeMainPackslots = CountFreeSlots(Core.CharacterFilter.Id);
	
				Lostfocus = !WinApe.GetActiveWindow().Equals(Host.Decal.Hwnd);
				mXPH = string.Empty;
				mXPChange = string.Empty;
	
				mXPStart = Core.CharacterFilter.TotalXP;
				mXPStartTime = DateTime.Now;
				// No Sounds
				// sldVolume.SliderPostition = mPluginConfig.wavVolume;
				if (mCharconfig.quickslots == null) {
					mCharconfig.quickslots = new SDictionary<int, QuickSlotInfo>();
				}
	
				mCurrentContainer = 0;
	
				//auto xp track
				if (mCharconfig.trackobjectxpHudId != 0) {
					if (Core.WorldFilter[mCharconfig.trackobjectxpHudId] == null) {
						mCharconfig.trackobjectxpHudId = 0;
					} else {
						wtcw("xp tracking is on: " + Core.WorldFilter[mCharconfig.trackobjectxpHudId].Name);
						Host.Actions.RequestId(mCharconfig.trackobjectxpHudId);
					}
				}
	
				if (Core.CharacterFilter.Level >= 275 & mCharconfig.trackobjectxpHudId == 0) {
					foreach (WorldObject wo in Core.WorldFilter.GetInventory()) {
						var _with1 = wo;
						if (_with1.Values(LongValueKey.EquippedSlots) != 0 && _with1.Name == "Aetheria") {
							Host.Actions.RequestId(_with1.Id);
						}
					}
				}
	
	
			} catch {
			}
		}
	
		private void startworker()
		{
			mBackgroundworker = new BackgroundWorker();
			mBackgroundworker.WorkerSupportsCancellation = true;
			mBackgroundworker.DoWork += LoadfilesBackground;
			mBackgroundworker.RunWorkerAsync();
		}
	
		
	
		private void resetprofiles()
		{
			try {
				mPluginConfig.Alerts = getbaseAlerts();
				if (mCharconfig.uselootprofile) {
					mCharconfig.ThropyList = getbaseThropyList();
					mActiveThropyProfile = mCharconfig.ThropyList;
				} else {
					mPluginConfig.ThropyList = getbaseThropyList();
					mActiveThropyProfile = mPluginConfig.ThropyList;
				}
	
				if (mCharconfig.usemobsprofile) {
					mCharconfig.MobsList = getbaseMobsList();
					mActiveMobProfile = mCharconfig.MobsList;
				} else {
					mPluginConfig.MobsList = getbaseMobsList();
					mActiveMobProfile = mPluginConfig.MobsList;
				}
	
				if (mCharconfig.usesalvageprofile) {
					mCharconfig.SalvageProfile = getbaseSalvageProfile();
					mActiveSalvageProfile = mCharconfig.SalvageProfile;
				} else if (mPluginConfig.worldbasedsalvage) {
					mWorldConfig.SalvageProfile = getbaseSalvageProfile();
					mActiveSalvageProfile = mWorldConfig.SalvageProfile;
				} else {
					mPluginConfig.SalvageProfile = getbaseSalvageProfile();
	
					mActiveSalvageProfile = mPluginConfig.SalvageProfile;
				}
	
				if (mPluginConfig.worldbasedrules) {
					mWorldConfig.Rules = getbaseRules();
					mActiveRulesProfile = mWorldConfig.Rules;
				} else {
					mPluginConfig.Rules = getbaseRules();
					mActiveRulesProfile = mPluginConfig.Rules;
				}
	
	
			} catch {
			}
		}
	
		private Dictionary<int, int> mCollTradeItemRecieved = new Dictionary<int, int>();
		private bool mTradeWindowOpen;
		private void OnWorldfilterTradeReset(object sender, Decal.Adapter.Wrappers.ResetTradeEventArgs e)
		{
			try {
				mCollTradeItemRecieved.Clear();
	
			} catch (Exception ex) {
				Util.ErrorLogger(ex);
			}
		}
	
		private void OnWorldfilterAddTradeItem(object sender, Decal.Adapter.Wrappers.AddTradeItemEventArgs e)
		{
			try {
				if (e.SideId == 2) {
					if (!mCollTradeItemRecieved.ContainsKey(e.ItemId)) {
						mCollTradeItemRecieved.Add(e.ItemId, 0);
					}
	
					if (Paused)
						return;
	
					WorldObject wo = Core.WorldFilter[e.ItemId];
					if (wo != null) {
						switch (wo.ObjectClass) {
							case ObjectClass.Armor:
							case ObjectClass.Clothing:
							case ObjectClass.Jewelry:
							case ObjectClass.MeleeWeapon:
							case ObjectClass.WandStaffOrb:
							case ObjectClass.MissileWeapon:
							case ObjectClass.Gem:
								IdqueueAdd(wo.Id);
								break;
						}
					}
				}
			} catch (Exception ex) {
				Util.ErrorLogger(ex);
			}
		}
	
		private void OnWorldfilterTradeEnd(object sender, Decal.Adapter.Wrappers.EndTradeEventArgs e)
		{
			try {
				mCollTradeItemRecieved.Clear();
				mTradeWindowOpen = false;
			} catch (Exception ex) {
				Util.ErrorLogger(ex);
			}
		}
	
		private void OnWorldfilterTradeEnter(object sender, Decal.Adapter.Wrappers.EnterTradeEventArgs e)
		{
			try {
				mCollTradeItemRecieved.Clear();
				mTradeWindowOpen = true;
			} catch (Exception ex) {
				Util.ErrorLogger(ex);
			}
		}
		//HACK:  Alinco Buffs Disabled
	
//		private void tryloadAlincoBuffs()
//		{
//			try {
//				malincobuffs = Alinco3Buffs.Plugin.Instance;
//				malincobuffsAvailable = true;
//				return;
//			} catch (Exception ex) {
//				//  Util.ErrorLogger(ex)
//			}
//			malincobuffsAvailable = false;
//		}
	
		[BaseEvent("ChatNameClicked")]
		private void Plugin_ChatNameClicked(object sender, Decal.Adapter.ChatClickInterceptEventArgs e)
		{
	
			try {
	
				switch (e.Id) {
					case NOTIFYLINK_ID:
	
						int Itemid = Convert.ToInt32(e.Text);
						if (mHudlistboxItems.ContainsKey(Itemid)) {
							huditemclick(true, (global::AlincoVVS.PluginCore.notify)mHudlistboxItems[Itemid], true);
						}
						e.Eat = true;
	
						break;
					case Util.ERRORLINK_ID:
	
						string url = null;
						url = Util.docPath + "\\Errors.txt";
	
	
						if (File.Exists(url)) {
							System.Diagnostics.Process myProcess = new System.Diagnostics.Process();
							myProcess.StartInfo.FileName = "notepad.exe";
							myProcess.StartInfo.Arguments = url;
							myProcess.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
							myProcess.Start();
	
						}
						e.Eat = true;
						break;
				}
	
			} catch (Exception ex) {
				Util.ErrorLogger(ex);
			}
		}
	
		private string actornamefromtell(string tell)
		{
			string actor = string.Empty;
	
			if (!string.IsNullOrEmpty(tell)) {
				int pos1 = 0;
				int pos2 = 0;
				string cmd = null;
				pos1 = tell.IndexOf(" tells you, ");
	
				if (pos1 > 1) {
					cmd = tell.Substring(pos1 + 12);
					cmd = cmd.Replace("\"", "");
	
					pos2 = tell.IndexOf(">");
					if (pos2 > 0) {
						string strTemp = tell.Substring(pos2 + 1);
	
						pos2 = strTemp.IndexOf("<");
						if (pos2 > 0) {
							strTemp = strTemp.Substring(0, pos2);
							Util.Log(":" + strTemp);
							actor = strTemp;
						}
					//vendor / npc chat
					} else {
						string strTemp = tell.Substring(0, pos1);
	
						actor = strTemp;
					}
				}
			}
			return actor;
		}
	
		[BaseEvent("ChatBoxMessage")]
		private void Plugin_ChatBoxMessage(object sender, Decal.Adapter.ChatTextInterceptEventArgs e)
		{
			try {
	
				if (mCharconfig != null) {
					string msg = e.ToString().Substring(1,e.ToString().Length -1);
					//strip linefeed
					int pos = 0;
	
					switch (e.Color) {
						case 0:
						case 24:
	
							if (msg.EndsWith("This permission will last one hour.")) {
								pos = msg.IndexOf(" has given you permission to loot");
								if (pos >= 0) {
									string playercorpse = "Corpse of " + msg.Substring(0, pos);
	
									if (!mProtectedCorpses.Contains(playercorpse)) {
										wtcw("Alinco added " + playercorpse + " to protected corpses.", 0);
										mProtectedCorpses.Add(playercorpse);
									}
	
								}
							} else if (msg.EndsWith("DTLN")) {
								msg += "= Air (West)";
								wtcw(msg, e.Color);
								e.Eat = true;
							} else if (msg.EndsWith("DBTNK")) {
								msg += " = Water (South)";
								wtcw(msg, e.Color);
								e.Eat = true;
							} else if (msg.EndsWith("NTLN")) {
								msg += " = Fire (North)";
								wtcw(msg, e.Color);
								e.Eat = true;
							} else if (msg.EndsWith("ZTNK")) {
								msg += " = Earth (East)";
								wtcw(msg, e.Color);
								e.Eat = true;
							}
							break;
						case 7:
							//spellcasting
							if (mPluginConfig.FilterSpellcasting && msg.StartsWith("The spell")) {
								e.Eat = true;
							} else if (mPluginConfig.FilterSpellsExpire && msg.EndsWith("has expired.")) {
								e.Eat = true;
								// resists your spell
							} else if (mPluginConfig.FilterChatResists && msg.StartsWith("You resist the spell")) {
								e.Eat = true;
	
							}
							break;
						case 17:
							if (msg.IndexOf("Cruath Quareth") >= 0) {
								mSpellwords = "Cruath Quareth";
							} else if (msg.IndexOf("Cruath Quasith") >= 0) {
								mSpellwords = "Cruath Quasith";
							} else if (msg.IndexOf("Equin Opaj") >= 0) {
								mSpellwords = "Equin Opaj";
							} else if (msg.IndexOf("Equin Ozael") >= 0) {
								mSpellwords = "Equin Ozael";
							} else if (msg.IndexOf("Equin Ofeth") >= 0) {
								mSpellwords = "Equin Ofeth";
							} else if (mPluginConfig.FilterSpellcasting) {
								if (msg.StartsWith("The spell")) {
									e.Eat = true;
								} else if (msg.StartsWith("You say, ")) {
									e.Eat = true;
								} else if (msg.IndexOf("says,\"") > 0) {
									e.Eat = true;
								}
							}
	
							break;
						case 21:
							//melee evades
							if (mPluginConfig.FilterChatMeleeEvades) {
								if (msg.IndexOf("You evaded") >= 0) {
									e.Eat = true;
								}
							}
							break;
						case 3:
							if (mPluginConfig.FilterTellsMerchant | mPluginConfig.notifytells) {
								string actorname = actornamefromtell(msg);
	
								WorldObject cursel = Core.WorldFilter[Host.Actions.CurrentSelection];
								WorldObjectCollection actors = Core.WorldFilter.GetByName(actorname);
	
								bool vendorTell = false;
								bool npcTell = false;
	
								//multipe actors possible?
								foreach (WorldObject x in actors) {
									if (x.ObjectClass == ObjectClass.Npc) {
										npcTell = true;
									} else if (x.ObjectClass == ObjectClass.Vendor) {
										vendorTell = true;
									}
								}
	
	
								if (mPluginConfig.FilterTellsMerchant && vendorTell) {
									e.Eat = true;
									return;
								}
	
	
								if (mPluginConfig.notifytells & !vendorTell & !npcTell) {
	
									if (cursel == null || ((!(msg.IndexOf(cursel.Name) >= 0)))) {
										PlaySoundFile("rcvtell.wav", mPluginConfig.wavVolume);
	
									}
	
								}
	
							}
							break;
					}
				}
	
			} catch (Exception ex) {
				Util.ErrorLogger(ex);
			}
		}
	
		private const int AC_SET_OBJECT_LINK = 0x2da;
		private const int AC_GAME_EVENT = 0xf7b0;
		private const int GE_SETPACK_CONTENTS = 0x196;
		private const int GE_IDENTIFY_OBJECT = 0xc9;
		private const int AC_ADJUST_STACK = 0x197;
		private const int AC_APPLY_VISUALSOUND = 0xf755;
		private const int AC_CREATE_OBJECT = 0xf745;
	
		private const int AC_SET_OBJECT_DWORD = 0x2ce;
		[BaseEvent("ServerDispatch")]
		private void Plugin_ServerDispatch(object sender, Decal.Adapter.NetworkMessageEventArgs e)
		{
			try {
				switch (e.Message.Type) {
					case AC_ADJUST_STACK:
						OnSetStack(e.Message);
						break;
					case AC_APPLY_VISUALSOUND:
						OnAppyVisualSound(e.Message);
						break;
					case AC_CREATE_OBJECT:
						OnCreateObject(e.Message);
						break;
					case AC_SET_OBJECT_LINK:
						OnSetObjectLink(e.Message);
						break;
					case AC_GAME_EVENT:
						int iEvent = 0;
						iEvent = 0;
	
						try {
							iEvent = Convert.ToInt32(e.Message["event"]);
							//used to crash sometimes
	
						} catch {
						}
	
						switch (iEvent) {
							case GE_SETPACK_CONTENTS:
								OnSetPackContents(e.Message);
								break;
							case GE_IDENTIFY_OBJECT:
								OnIdentObject(e.Message);
								break;
						}
						break;
					//Case AC_SET_OBJECT_DWORD
					//    OnSetObjectWord(e.Message)
				}
			} catch (Exception ex) {
				Util.ErrorLogger(ex);
			}
		}
	
	
	
		private void OnSetObjectWord(Decal.Adapter.Message pMsg)
		{
	
			if (mFilesLoaded) {
				int Id = Convert.ToInt32(pMsg["object"]);
				WorldObject objItem = null;
				objItem = Core.WorldFilter[Id];
				if (objItem == null) {
					return;
				}
	
				int key = Convert.ToInt32(pMsg["key"]);
				int value = Convert.ToInt32(pMsg["value"]);
			}
		}
	
		private string mSpellwords = string.Empty;
		private Dictionary<int, Mobdata> mMobs = new Dictionary<int, Mobdata>();
		private void OnAppyVisualSound(Decal.Adapter.Message pMsg)
		{
			int guid = 0;
			guid = Convert.ToInt32(pMsg["object"]);
			if (guid != Core.CharacterFilter.Id) {
				Decal.Adapter.Wrappers.WorldObject oWo = null;
				oWo = Core.WorldFilter[guid];
				if (oWo != null && oWo.Category == 16) {
					int effect = Convert.ToInt32(pMsg["effect"]);
	
					if (effect == 23 || effect == 38 || effect == 44 || effect == 46 || effect == 48 || effect == 50 || effect == 52 || effect == 54 || effect == 56) {
						if (!mMobs.ContainsKey(oWo.Id)) {
							mMobs.Add(oWo.Id, new Mobdata());
						}
	
						Mobdata bo = null;
						bo = mMobs[oWo.Id];
						if (bo != null) {
							bo.UpdateEffect(effect, mSpellwords);
						}
	
					}
	
				}
			}
		}
	
		private string mmanualstackname = string.Empty;
		private void OnSetStack(Decal.Adapter.Message pMsg)
		{
			int id = 0;
			int stack = 0;
			int value = 0;
			id = Convert.ToInt32(pMsg["item"]);
			stack = Convert.ToInt32(pMsg["count"]);
			value = Convert.ToInt32(pMsg["value"]);
			WorldObject obj = Core.WorldFilter[id];
			if (obj != null) {
				Util.Log("OnSetStack " + id.ToString() + " " + obj.Name);
				if (obj.Container == Core.CharacterFilter.Id && obj.Values(LongValueKey.Slot) != -1) {
					//  wtcw2("Manualstack " & obj.Name & " stack " & stack & " wostat " & obj.Values(LongValueKey.StackCount))
					mmanualstackname = obj.Name;
				}
			}
		}
		private void wtcw(string msg, int color = 14)
		{
			try {
				Host.Actions.AddChatText(msg, color, mPluginConfig.chattargetwindow);
			} catch (Exception ex) {
				Util.ErrorLogger(ex);
			}
		}
	
	
		[Conditional("DEBUGTEST")]
		private void wtcwd(string msg, int color = 13)
		{
			try {
				Host.Actions.AddChatText(msg, color, 2);
			} catch (Exception ex) {
				Util.ErrorLogger(ex);
			}
		}
	
		private Hashtable mColStacker = new Hashtable();
		//move object to container or equipper
		private void OnSetObjectLink(Decal.Adapter.Message pMsg)
		{
			try {
				//see if configuration rules exist
				if (mPluginConfig == null || mCharconfig == null) {
					return;
					// exit if not
				}
				int Id = 0;
				int key = 0;
				//id as int
				Id = Convert.ToInt32(pMsg["object"]);
				//Gets the objectID and converts to int in ID
	
				WorldObject obj = Core.WorldFilter[Id];
				// obj as WorldObject structure and map Item(ID) into it
	
				//This section is for items flagged as salvage to be crushed by vtlooter
				//is it a valid object
				if (obj != null) {
					key = Convert.ToInt32(pMsg["key"]);
					//Gets the dword, etc. keys from Item
					// move to container 
					if (key == 2) {
						//Notified items have an item ID?
						if (mNotifiedItems.ContainsKey(Id)) {
							notify dn = (global::AlincoVVS.PluginCore.notify)mNotifiedItems[Id];
							//salvage loots w/wo value
							if (dn.scantype == eScanresult.salvage || (dn.scantype == eScanresult.value && mPluginConfig.SalvageHighValue)) {
								WorldObject wo = Core.WorldFilter[Id];
								if (wo != null) {
									var _with2 = wo;
									//add to ustlist
									if (!mUstItems.ContainsKey(_with2.Id)) {
										if (Convert.ToInt32(_with2.Values(DoubleValueKey.SalvageWorkmanship)) > 0) {
											salvageustinfo newinfo = getsalvageinfo(_with2.Id, _with2.Name, Convert.ToInt32(_with2.Values(DoubleValueKey.SalvageWorkmanship)), _with2.Values(LongValueKey.Material), _with2.Values(LongValueKey.UsesRemaining), false);
											mUstItems.Add(_with2.Id, newinfo);
											addToUstList(_with2.Name, _with2.Id, dn.description);
										}
									}
								}
							}
							mNotifiedItems.Remove(Id);
							//removes the item from the ID queue
						}
						//stacks looted items
						if (mPluginConfig.AutoStacking) {
							if (!string.IsNullOrEmpty(mmanualstackname) & mmanualstackname == obj.Name) {
							//wtcw2("block stacking ")
							} else {
								if (obj.Values(LongValueKey.StackMax) > obj.Values(LongValueKey.StackCount)) {
									if (!mColStacker.ContainsKey(obj.Id)) {
										//wtcw2(" add to stacking queue")
										mColStacker.Add(obj.Id, obj.Name);
									}
								}
							}
						}
					}
				}
				if (mNotifiedItems.ContainsKey(Id)) {
					mNotifiedItems.Remove(Id);
				}
			} catch (Exception ex) {
				Util.ErrorLogger(ex);
			}
		}
	
		private int FindStackableItemGuidbyName(string ItemToStackName, int ItemToStackId)
		{
			int packid = Core.CharacterFilter.Id;
			WorldObjectCollection wocol = Core.WorldFilter.GetByName(ItemToStackName);
			foreach (WorldObject obj in wocol) {
				if (IsItemInInventory(obj)) {
					// self
					if (obj.Id != ItemToStackId) {
						if (obj.Name == ItemToStackName) {
							// stackable
							if (obj.Values(LongValueKey.StackMax) > 1) {
								if (obj.Values(LongValueKey.StackCount) < obj.Values(LongValueKey.StackMax)) {
									return obj.Id;
								} else if (obj.Container != packid) {
									packid = obj.Container;
								}
							}
						}
					}
				}
			}
	
			if (packid != Core.CharacterFilter.Id) {
				return packid;
			}
	
			return 0;
		}
	
		private bool Stacking()
		{
	
			if (mColStacker.Count > 0) {
				// wtcw2("Trystacking 1")
				int removeid = 0;
				string objectname = string.Empty;
	
				foreach (DictionaryEntry xs in mColStacker) {
					removeid = Convert.ToInt32(xs.Key);
					objectname = Convert.ToString(xs.Value);
					break; // TODO: might not be correct. Was : Exit For
				}
	
				mColStacker.Remove(removeid);
				// wtcw2("Trystacking " & Hex(removeid) & " " & objectname)
				int x = FindStackableItemGuidbyName(objectname, removeid);
				if (x != 0) {
					Host.Actions.MoveItem(removeid, x);
					return true;
				}
			} else {
				mmanualstackname = string.Empty;
			}
			//HACK:  Not all code paths return a value
			return false;
	
		}
	
		//Subroutine fires when a new container opens
		private void OnOpenContainer(object sender, Decal.Adapter.ContainerOpenedEventArgs e)
		{
			try {
				if (Paused)
					return;
	
				if (e.ItemGuid == 0) {
					wtcwd("OnCloseContainer ");
					mCorpsScanning = false;
					//todo check if valid to just clear them
					//it is verry possible to close the container while the scanprocess is bussy
					if (mNotifiedCorpses.ContainsKey(mCurrentContainer)) {
						mNotifiedCorpses.Remove(mCurrentContainer);
						wtcwd("OnCloseContainer  removed , wierd missed open event");
						Renderhud();
					}
					mCurrentContainer = 0;
					if (mCurrentContainerContent != null) {
						mCurrentContainerContent.Clear();
					}
	
					if (mPluginConfig.AutoPickup | mPluginConfig.AutoUst) {
						mFreeMainPackslots = CountFreeSlots(Core.CharacterFilter.Id);
					}
				} else {
					mwaitonopen = false;
					if (mNotifiedCorpses.ContainsKey(e.ItemGuid)) {
						mCorpsScanning = finishedscanning();
						mNotifiedCorpses.Remove(e.ItemGuid);
						wtcwd("OnCloseContainer removed");
	
						Renderhud();
					}
					Util.Log("OnOpenContainer " + e.ItemGuid.ToString());
				}
	
			} catch (Exception ex) {
				Util.ErrorLogger(ex);
			}
		}
	
		private void setupHotkey(string sPlugin, string sTitle, string sDescription)
		{
			try {
	
				if (!Core.HotkeySystem.Exists(sTitle)) {
					Core.HotkeySystem.AddHotkey(sPlugin, sTitle, sDescription);
	
				}
	
			} catch (Exception ex) {
				Util.ErrorLogger(ex);
			}
		}
	
		//opening a pack,container,chest,.. also inventory packs at startup
		//or moving packs in inventory
		private void OnSetPackContents(Decal.Adapter.Message pMsg)
		{
			try {
				if (Paused || mCharconfig == null)
					return;
				if (Host.Actions.VendorId != 0) {
					return;
				}
	
				Decal.Adapter.MessageStruct pItems = null;
				Decal.Adapter.MessageStruct pItem = null;
				int ItemId = 0;
				int itemcount = 0;
				mCurrentContainer = Convert.ToInt32(pMsg["container"]);
				itemcount = Convert.ToInt32(pMsg["itemCount"]);
	
				WorldObject objContainer = Core.WorldFilter[mCurrentContainer];
	
	
				if (objContainer != null && mPluginConfig.PackOrCorpseOrChestExclude != null) {
					foreach (string s in mPluginConfig.PackOrCorpseOrChestExclude) {
						if (objContainer.Name == s) {
							mCurrentContainer = 0;
							return;
						}
					}
				}
				mCurrentContainerContent = new Dictionary<int, int>();
	
				string msg = string.Empty;
				bool skipContainer = false;
				if (objContainer != null) {
					msg = objContainer.Name;
	
					if (objContainer.Id == Core.CharacterFilter.Id || objContainer.Container == Core.CharacterFilter.Id) {
						msg += " skip player pack";
						//moving/adding packs in inventory
						skipContainer = true;
					} else if (objContainer.Container != 0) {
						WorldObject checkcontainerincontainer = Core.WorldFilter[objContainer.Container];
						if (checkcontainerincontainer != null) {
							if (checkcontainerincontainer.Container == Core.CharacterFilter.Id) {
								msg += " skip container in container storage";
								skipContainer = true;
							}
						}
					}
	
					if (skipContainer) {
						Util.Log("OnSetPackContents " + mCurrentContainer.ToString() + " " + msg);
						mCurrentContainer = 0;
						return;
					}
				}
	
				Util.Log("OnSetPackContents " + mCurrentContainer.ToString() + " " + msg);
	
				//fill mCurrentContainerContent
				if (itemcount > 0) {
					pItems = pMsg.Struct("items");
					for (int i = 0; i <= pItems.Count - 1; i++) {
						pItem = pItems.Struct(i);
						ItemId = Convert.ToInt32(pItem["item"]);
	
						if (!mCurrentContainerContent.ContainsKey(ItemId)) {
							mCurrentContainerContent.Add(ItemId, 0);
							WorldObject objItem = Core.WorldFilter[ItemId];
							if (objItem != null && mCharconfig != null) {
								eScanresult result = CheckObjectForMatch(new IdentifiedObject(objItem), false);
								//check scan finished
	
								if (result != eScanresult.needsident) {
									mCurrentContainerContent[ItemId] = 1;
								} else {
									mCorpsScanning = true;
								}
							}
	
						}
	
					}
				}
	
			} catch (Exception ex) {
				Util.ErrorLogger(ex);
			}
		}
	
		
// NO ALINCO BUFFS I SAID!	
//		private void StartbuffsPending()
//		{
//			try {
//				if (malincobuffsAvailable) {
//					if (malincobuffs.Buffing) {
//						malincobuffs.CancelBuffs(Alinco3Buffs.eMagicSchool.Creature | Alinco3Buffs.eMagicSchool.Life | Alinco3Buffs.eMagicSchool.Item);
//					} else {
//						bool pendingsbuffsonly = (malincobuffs.BuffsPending > 0);
//						malincobuffs.StartBuffs(Alinco3Buffs.eMagicSchool.Creature | Alinco3Buffs.eMagicSchool.Life | Alinco3Buffs.eMagicSchool.Item, pendingsbuffsonly);
//	
//					}
//	
//				}
//			} catch (Exception ex) {
//				Util.ErrorLogger(ex);
//			}
//		}
//		private int buffpending()
//		{
//			try {
//	
//				if (malincobuffsAvailable) {
//					return malincobuffs.BuffsPending;
//				}
//	
//			} catch (Exception ex) {
//			}
//		}
//	
//		private int togglebuffing()
//		{
//			try {
//				if (malincobuffsAvailable) {
//					if (malincobuffs.Buffing) {
//						malincobuffs.Pause = !malincobuffs.Pause;
//					}
//				}
//	
//			} catch (Exception ex) {
//			}
//		}
//	
//		private string buffingstring()
//		{
//			try {
//				if (malincobuffsAvailable) {
//					string result = string.Empty;
//					int secs1 = malincobuffs.BuffTimeRemaining(Alinco3Buffs.eMagicSchool.Creature | Alinco3Buffs.eMagicSchool.Life);
//					int secs2 = malincobuffs.BuffTimeRemaining(Alinco3Buffs.eMagicSchool.Item);
//	
//					result = secondstoTimeString(secs1, false);
//	
//					if (Math.Abs(secs1 - secs2) > 180) {
//						result += " / " + secondstoTimeString(secs2, false);
//					}
//					return result;
//				}
//	
//			} catch (Exception ex) {
//			}
//			return string.Empty;
//		}
//		private string buffingstring2()
//		{
//			try {
//				if (malincobuffsAvailable) {
//					string result = string.Empty;
//					int secs1 = malincobuffs.BuffTimeRemaining(Alinco3Buffs.eMagicSchool.Creature | Alinco3Buffs.eMagicSchool.Life);
//	
//	
//					result = secondstoTimeString(secs1, false);
//	
//	
//					return result;
//				}
//	
//			} catch (Exception ex) {
//			}
//			return string.Empty;
//		}
	
		
	

	
		internal class lookupstats
		{
			public int totalq;
			public TimeSpan slowest;
			public TimeSpan avg;
			public TimeSpan fastest;
			public TimeSpan totalspend;
			public void reset()
			{
				totalq = 0;
				slowest = TimeSpan.Zero;
				avg = TimeSpan.Zero;
				fastest = TimeSpan.Zero;
				totalspend = TimeSpan.Zero;
			}
			public string toostring()
			{
	
				string slow = slowest.TotalMilliseconds.ToString("0.000");
				string fast = fastest.TotalMilliseconds.ToString("0.000");
				string totals = totalspend.TotalSeconds.ToString("0.00");
	
				string ts = totalq.ToString();
				return string.Format("Total Lookup {0} slowest {1}ms,  fastest {2}ms total {3}s ", ts, slow, fast, totals);
			}
		}
	
		private lookupstats mThropylookup;
		private lookupstats mRulelookup;
		private lookupstats mSalvagelookup;
	
		private lookupstats mMoblookup;
		private int m10seconds;
		private int m4seconds;
		private int m2seconds;
	
		private bool viewinitialized;
		private void MainTimer_Tick(object sender, System.EventArgs e)
		{
			try {
				if ((mBackgroundworker != null && mBackgroundworker.IsBusy) || mCharconfig == null) {
					return;
				}
	
				if (mFilesLoaded & !viewinitialized) {
					viewinitialized = true;
					initializeView();
				}
	
				m4seconds += 1;
				if (m4seconds >= 8) {
					m4seconds = 0;
					setlistboxranges();
	
					xphour();
					if (mMarkObject != null && (DateTime.Now - mMarkObjectDate).Seconds > 3) {
						mMarkObject.visible = false;
						mMarkObject = null;
					}
	
				}
				NotifyObjectsFromQueue();
	
				m2seconds += 1;
				if (m2seconds >= 4) {
					m2seconds = 0;
					if (RenderServiceForHud != null) {
						Renderhud();
						RenderQuickslotsHud();
					}
				}
	
				m10seconds += 1;
				if (m10seconds >= 12) {
					m10seconds = 0;
					if (RenderServiceForHud != null) {
						//hudflags1 =0 dont care about xp
						if (mCharconfig.trackobjectxpHudId != 0 & mPluginConfig.hudflags1 == 1) {
	
							if (Host.Actions.IsValidObject(mCharconfig.trackobjectxpHudId)) {
								Host.Actions.RequestId(mCharconfig.trackobjectxpHudId);
							}
	
						}
					}
				}
	
				if (!mPaused) {
					if (Host.Actions.BusyState == 0) {
						if (AutoPickup()) {
							return;
						}
	
						if (Stacking()) {
							return;
						}
	
	
						if (Host.Actions.CombatMode != CombatState.Peace || (mPluginConfig.AutoUst & mCurrentContainer != 0)) {
						} else if (mPluginConfig.AutoUst) {
							if (AutoUst())
								return;
						}
	
					}
				}
	
			} catch (Exception ex) {
				Util.ErrorLogger(ex);
			}	
		}
    }
}
