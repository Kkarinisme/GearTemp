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
using VirindiViewService.Themes;
using System.Xml.Serialization;
using System.Xml;

namespace GearFoundry
{
	public partial class PluginCore
	{


		
		
		private KillTaskSettings mKTSet = new KillTaskSettings();
		private System.Windows.Forms.Timer KTSaveTimer = new System.Windows.Forms.Timer();
		
		public class KillTaskSettings
		{
			public int HudWidth = 220;
			public int HudHeight = 100;
			public bool RenderMini = false;
			public bool SquelchTaskReporting = false;
			public List<KillTask> MyKillTasks = new List<KillTask>();
			public List<CollectTask> MyCollectTasks = new List<CollectTask>();			
		}
		
		public class KillTask
		{
			public string TaskName = String.Empty;
			public List<string> MobNames = new List<string>();
			public int CurrentCount = 0;
			public int CompleteCount = 0;
			public List<string> NPCNames = new List<string>();
			public string NPCInfo = String.Empty;
			public string NPCCoords = String.Empty;
			public string NPCYellowFlagText = String.Empty;
			public string NPCYellowCompleteText = String.Empty;
			public bool track = false;
			public bool active = false;
			public bool complete = false;
		}
		
		public class CollectTask
		{
			public string TaskName = String.Empty;
			public string Item = String.Empty;
			public List<string> MobNames = new List<string>();
			public int CurrentCount = 0;
			public int CompleteCount = 0;
			public string NPCName = String.Empty;
			public string NPCInfo = String.Empty;
			public string NPCCoords = String.Empty;
			public string NPCYellowCompleteText = String.Empty;
			public bool track = false;
			public bool detect = false;
			public bool complete = false;
		}
		
		private void SubscribeKillTasks()
		{
			Core.ChatBoxMessage += KillTask_ChatBoxMessage;
			Core.CharacterFilter.Logoff += KillTask_LogOff;
			KTSaveTimer.Interval = 600000;
			KTSaveTimer.Start();
			KTSaveTimer.Tick += KTSaveUpdates;
			
			ReadWriteGearTaskSettings(true);
			RenderKillTaskPanel();
			BuildKillTaskList();
		}
		
		private void KTSaveUpdates(object sender, EventArgs e)
		{
			ReadWriteGearTaskSettings(false);
		}
		
		private void KillTask_LogOff(object sender, EventArgs e)
		{
			UnsubscribeKillTasks();
		}
		
		private void UnsubscribeKillTasks()
		{
			Core.ChatBoxMessage -= KillTask_ChatBoxMessage;
			Core.CharacterFilter.Logoff -= KillTask_LogOff;
			DisposeKillTaskPanel();
			KTSaveTimer.Stop();
			ReadWriteGearTaskSettings(false);
		}
		
		private void ReadWriteGearTaskSettings(bool read)
		{
			try
			{
				FileInfo GearTaskSettingsFile = new FileInfo(toonDir + @"\GearTask.xml");
				
				XmlWriterSettings wsettings = new XmlWriterSettings();
				wsettings.Indent = true;
				wsettings.NewLineOnAttributes = true;
				
				XmlReaderSettings rsettings = new XmlReaderSettings();
			    rsettings.IgnoreWhitespace = true;
								
				if (read)
				{

					if (!GearTaskSettingsFile.Exists)
	                {                    	
                    	FileInfo KillTasksFile = new FileInfo(GearDir + @"\KillTasks.xml");
                    	FileInfo CollectTasksFile = new FileInfo(GearDir + @"\CollectTasks.xml");
                    	
						try
						{
	                    	if(!KillTasksFile.Exists)
	                    	{
		                    	string filedefaults =  GetResourceTextFile("KillTasks.xml");		                    	
		                    	using (StreamWriter writedefaults = new StreamWriter(@KillTasksFile.ToString(), true))
								{
									writedefaults.Write(filedefaults);
									writedefaults.Close();
								}
	                    	}
						}catch(Exception ex){LogError(ex);}
						
//						try
//						{
//	                    	if(!CollectTasksFile.Exists)
//	                    	{
//		                    	string filedefaults = GetResourceTextFile("CollectTasks.xml");
//		                    	using (StreamWriter writedefaults = new StreamWriter(@CollectTasksFile.ToString(), true))
//								{
//									writedefaults.Write(filedefaults);
//									writedefaults.Close();
//								}
//	                    	}
//                    	}catch(Exception ex){LogError(ex);}
						
						try
						{
		                    	
			                mKTSet = new KillTaskSettings();
			                 
			                using (XmlReader reader = XmlReader.Create(@KillTasksFile.ToString(), rsettings))
							{	
								XmlSerializer kts = new XmlSerializer(typeof(List<KillTask>));
								mKTSet.MyKillTasks = (List<KillTask>)kts.Deserialize(reader);
								reader.Close();
			                }
			                
//			                using (XmlReader reader = XmlReader.Create(CollectTasksFile.ToString()))
//							{	
//								XmlSerializer cts = new XmlSerializer(typeof(List<CollectTask>));
//								mKTSet.MyCollectTasks = (List<CollectTask>)cts.Deserialize(reader);
//								reader.Close();
//			                }
			                    			                
	                    	using (XmlWriter writer = XmlWriter.Create(GearTaskSettingsFile.ToString(), wsettings))
							{
					   			XmlSerializer serializer2 = new XmlSerializer(typeof(KillTaskSettings));
					   			serializer2.Serialize(writer, mKTSet);
					   			writer.Close();
							}
			            						
							using (XmlReader reader = XmlReader.Create(GearTaskSettingsFile.ToString(), rsettings))
							{	
								XmlSerializer serializer = new XmlSerializer(typeof(KillTaskSettings));
								mKTSet = (KillTaskSettings)serializer.Deserialize(reader);
								reader.Close();
							}
						}catch(Exception ex){LogError(ex);}
					}
				}
				
				if(!read)
				{
					if(GearTaskSettingsFile.Exists)
					{
						GearTaskSettingsFile.Delete();
					}
					
					using (XmlWriter writer = XmlWriter.Create(@GearTaskSettingsFile.ToString(), wsettings))
					{
			   			XmlSerializer serializer2 = new XmlSerializer(typeof(KillTaskSettings));
			   			serializer2.Serialize(writer, mKTSet);
			   			writer.Close();
					}
				}
			}catch(Exception ex){LogError(ex);}
		}
		
		private string nibble = String.Empty;
		private void KillTask_ChatBoxMessage(object sender, ChatTextInterceptEventArgs e)
		{
			try
			{
				
				if(e.Color != 0 && e.Color != 3) {return;}
				if(e.Color == 0)
				{
					
					//	You have killed 20 Frozen Wights! Your task is complete!
					//	You have killed 18 Gurog Soldiers! You must kill 20 to complete your task.					
					
					if(!e.Text.StartsWith("You have killed")){return;}
					
					int mobskilled = 0;
					int totalmobs = 0;
					string mobname = String.Empty;
					bool taskcomplete = false;
					
					nibble = e.Text.Remove(0, 16);
					Int32.TryParse(nibble.Substring(0, nibble.IndexOf(' ')), out mobskilled);
					
					nibble = nibble.Remove(0, nibble.IndexOf(' '));
					mobname = (nibble.Substring(0, nibble.IndexOf('!'))).Trim();
					
					if(mobname.EndsWith("ies"))
					{
						mobname = mobname.Replace("ies","y").Trim();
					}
					else if(mobname.EndsWith("xes"))
					{
						mobname = mobname.Remove(mobname.Length - 2, 2).Trim();
					}
					else if(mobname.EndsWith("s"))
					{
						mobname = mobname.Remove(mobname.Length -1, 1).Trim();
					}
					else if(mobname.EndsWith("men"))
					{
							mobname = mobname.Replace("men","man");
					}
					
					nibble = nibble.Remove(0, nibble.IndexOf('!') + 2).Trim();
										
					if(nibble.IndexOf("Your task is complete") == -1)
					{
						nibble = nibble.Remove(0, 14).Trim();
						Int32.TryParse(nibble.Substring(0, nibble.IndexOf(' ')), out totalmobs);																		
					}
					else
					{
						totalmobs = mobskilled;
						taskcomplete = true;
					}
										
					int TaskIndex = mKTSet.MyKillTasks.FindIndex(x => x.CompleteCount == totalmobs && x.MobNames.Any(y => y == mobname));

					WriteToChat("Task Index = " + TaskIndex);
					
					mKTSet.MyKillTasks[TaskIndex].CurrentCount = mobskilled;
					mKTSet.MyKillTasks[TaskIndex].complete = taskcomplete;
					mKTSet.MyKillTasks[TaskIndex].active = true;
				}
				if(e.Color == 3)
				{
					int TaskIndex = mKTSet.MyKillTasks.FindIndex(x => x.NPCNames.Any(y => e.Text.StartsWith(y)));
					
					WriteToChat("mKT.KillTaskList.Count = " + mKTSet.MyKillTasks.Count());
					
					WriteToChat("Task Index = " + TaskIndex.ToString());
					
					                                             
					
					if(TaskIndex != -1)
					{
						if(e.Text.IndexOf(mKTSet.MyKillTasks[TaskIndex].NPCYellowCompleteText) != -1)
						{
							mKTSet.MyKillTasks[TaskIndex].active = false;
							mKTSet.MyKillTasks[TaskIndex].CurrentCount = 0;
						}
						if(e.Text.IndexOf(mKTSet.MyKillTasks[TaskIndex].NPCYellowFlagText) != -1)
						{						
							mKTSet.MyKillTasks[TaskIndex].active = true;
						}					
					}
				}
				
			}catch(Exception ex){LogError(ex);}
		}
		
		
		private HudView TaskHudView = null;
		private HudTabView TaskTabView = null;
		private HudFixedLayout TaskIncompleteLayout = null;
		private HudFixedLayout TaskCompleteLayout = null;
		private HudFixedLayout KillTaskLayout = null;
		private HudFixedLayout CollectTaskLayout = null;
		private HudList TaskIncompleteList = null;
		private HudList TaskCompleteList = null;
		private HudList KillTaskList = null;
		private HudList CollectTaskLis = null;
		private HudList.HudListRowAccessor TaskListRow = null;
		
		
		private void RenderKillTaskPanel()
		{
			try
			{
				if(TaskHudView != null)
				{
					DisposeKillTaskPanel();
				}
			
				TaskHudView = new HudView("GearTasker", mKTSet.HudWidth, mKTSet.HudHeight, new ACImage(0x6AA4));
				TaskHudView.UserAlphaChangeable = false;
				TaskHudView.ShowInBar = false;
				if(mKTSet.RenderMini){TaskHudView.UserResizeable = false;}
				else{TaskHudView.UserResizeable = true;}
				TaskHudView.Visible = true;
				TaskHudView.Ghosted = false;
				TaskHudView.UserClickThroughable = false;
	            TaskHudView.UserMinimizable = true;
	            TaskHudView.LoadUserSettings();
	            
	            TaskTabView = new HudTabView();
	            TaskHudView.Controls.HeadControl = TaskTabView;
	            
	            TaskIncompleteLayout = new HudFixedLayout();
	            TaskTabView.AddTab(TaskIncompleteLayout, "Incomplete");
	            	            
	            TaskIncompleteList = new HudList();
	            TaskIncompleteLayout.AddControl(TaskIncompleteList, new Rectangle(0,0,mKTSet.HudWidth,mKTSet.HudHeight));
	            TaskIncompleteList.ControlHeight = 16;
	            TaskIncompleteList.AddColumn(typeof(HudStaticText), Convert.ToInt32(mKTSet.HudWidth/3), null);  //MobName
	            TaskIncompleteList.AddColumn(typeof(HudStaticText), Convert.ToInt32(mKTSet.HudWidth/5), null);  //CurrentCount
	            TaskIncompleteList.AddColumn(typeof(HudStaticText), Convert.ToInt32(mKTSet.HudWidth/5), null);  //CompleteCount
	            
	            TaskCompleteLayout = new HudFixedLayout();
	            TaskTabView.AddTab(TaskCompleteLayout, "Complete");
	            TaskCompleteList = new HudList();
	            
	            TaskCompleteLayout.AddControl(TaskCompleteList, new Rectangle(0,0,mKTSet.HudWidth,mKTSet.HudHeight));
	            TaskCompleteList.ControlHeight = 16;
	            TaskCompleteList.AddColumn(typeof(HudStaticText), Convert.ToInt32(mKTSet.HudWidth/3), null);  //MobName
	            TaskCompleteList.AddColumn(typeof(HudStaticText), Convert.ToInt32(mKTSet.HudWidth/5), null);  //CurrentCount
	            TaskCompleteList.AddColumn(typeof(HudStaticText), Convert.ToInt32(mKTSet.HudWidth/5), null);  //CompleteCount
	            
	            KillTaskLayout = new HudFixedLayout();
	            TaskTabView.AddTab(KillTaskLayout, "Kill");
	            
	            KillTaskList = new HudList();
	            KillTaskLayout.AddControl(KillTaskList, new Rectangle(0,0,mKTSet.HudWidth,mKTSet.HudHeight));
	            KillTaskList.ControlHeight = 16;
	            KillTaskList.AddColumn(typeof(HudCheckBox), 16, null);  //Track
	            KillTaskList.AddColumn(typeof(HudStaticText), Convert.ToInt32(mKTSet.HudWidth/3), null);  //TaskName
	            KillTaskList.AddColumn(typeof(HudStaticText), Convert.ToInt32(mKTSet.HudWidth/5), null);  //MobName
	            
	            KillTaskList.Click += KTTL_Click;
	            
	            CollectTaskLayout = new HudFixedLayout();
	            TaskTabView.AddTab(CollectTaskLayout, "Collect");
	            
	            

	            
	            
	            
	            
	            
	            //(Detect) (TaskName) (NumberKilled)/(CompleteCount) (Active) (Complete)
	            
				
			}catch(Exception ex){LogError(ex);}
        
			
		}
		
		private void UpdateKillTaskPanel()
		{
			try
			{
				
			}catch(Exception ex){LogError(ex);}
		}
		
		
		
		private void KTTL_Click(object sender, int row, int col)
		{
			try
			{
				
			}catch(Exception ex){LogError(ex);}
		}
		
		private void DisposeKillTaskPanel()
		{
			
		}
		
		
		
		private void BuildKillTaskList()
		{
			
			WriteToChat("KillTaskList Building");
			KillTask t;  
			
			//Jarvis Hammerstone Quests
			t = new KillTask();
			t.TaskName = "Drudge Lurker Kill Task";
			t.MobNames.Add("Drudge Lurker");
			t.CompleteCount = 100;
			t.NPCNames.Add("Jarvis Hammerstone");
			t.NPCInfo = "Cragstone";
			t.NPCCoords = "25.6N, 49.4E";
			t.NPCYellowFlagText = "In the meantime I need to rid the surrounding area of some of these Drudges.";
			t.NPCYellowCompleteText = "Drudge Doom";
			mKTSet.MyKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Drudge Stalker Kill Task";
			t.MobNames.Add("Drudge Stalker");
			t.CompleteCount = 100;
			t.NPCNames.Add("Jarvis Hammerstone");
			t.NPCInfo = "Cragstone";
			t.NPCCoords = "25.6N, 49.4E";
			t.NPCYellowFlagText = "In the meantime I need to rid the surrounding area of some of these Drudges.";
			t.NPCYellowCompleteText = "Stalker Stalker";
			mKTSet.MyKillTasks.Add(t);

			t = new KillTask();
			t.TaskName = "Drudge Ravener Kill Task";
			t.MobNames.Add("Drudge Ravener");
			t.CompleteCount = 100;
			t.NPCNames.Add("Jarvis Hammerstone");
			t.NPCInfo = "Cragstone";
			t.NPCCoords = "25.6N, 49.4E";
			t.NPCYellowFlagText = "In the meantime I need to rid the surrounding area of some of these Drudges.";
			t.NPCYellowCompleteText = "Ravenous";
			mKTSet.MyKillTasks.Add(t);
						
			t.TaskName = "Altered Drudge Kill Task";
			t.MobNames.Add("Altered Drudge");
			t.CompleteCount = 40;
			t.NPCNames.Add("Jarvis Hammerstone");
			t.NPCInfo = "Cragstone";
			t.NPCCoords = "25.6N, 49.4E";
			t.NPCYellowFlagText = "In the meantime I need to rid the surrounding area of some of these Drudges.";
			t.NPCYellowCompleteText = "Altered Hunter";
			mKTSet.MyKillTasks.Add(t);	

			t = new KillTask();
			t.TaskName = "Augmented Drudge Kill Task";
			t.MobNames.Add("Augmented Drudge");
			t.CompleteCount = 40;
			t.NPCNames.Add("Jarvis Hammerstone");
			t.NPCInfo = "Cragstone";
			t.NPCCoords = "25.6N, 49.4E";
			t.NPCYellowFlagText = "In the meantime I need to rid the surrounding area of some of these Drudges.";
			t.NPCYellowCompleteText = "Augmented Hunter";
			mKTSet.MyKillTasks.Add(t);	

			
			//Neftet Quests		
			t = new KillTask();
			t.TaskName = "Armoredillo Hunting: Lost City of Neftet";
			t.MobNames.Add("Guardian Armoredillo");
			t.MobNames.Add("Tamed Armoredillo");
			t.MobNames.Add("War Armoredillo");			
			t.CompleteCount = 30;
			t.NPCNames.Add("Dame Tularin");
			t.NPCInfo = "Neftet";
			t.NPCCoords = "22.1S, 6.3E";
			t.NPCYellowFlagText = "If you will kill 30 of the armoredillos within the canyon walls or up on the plateaus, I will reward you for your help.";
			t.NPCYellowCompleteText = "Well done.";
			mKTSet.MyKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Golem Hunting: Lost City of Neftet";
			t.MobNames.Add("Burning Sands Golem");
			t.MobNames.Add("Dust Golem");
			t.CompleteCount = 15;
			t.NPCNames.Add("Sir Ibreh bin Kassim");
			t.NPCInfo = "Encampment near Neftet";
			t.NPCCoords = "22.2S, 6.2E";
			t.NPCYellowFlagText = "If you will kill 15 of the golems within the canyon walls or up on the plateaus, I will reward you for your assistance.";
			t.NPCYellowCompleteText = "Well done.";
			mKTSet.MyKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Mumiyah Hunting: Lost City of Neftet";
			t.MobNames.Add("Mu-miyah Channeller");
			t.MobNames.Add("Mu-miyah Champion");
			t.MobNames.Add("Mu-miyah Guardian");
			t.MobNames.Add("Mu-miyah Lord");
			t.MobNames.Add("Mu-miyah Sentinel");
			t.MobNames.Add("Mu-miyah Soldier");
			t.MobNames.Add("Mu-miyah Soothsayer");
			t.MobNames.Add("Mu-miyah Vizier");
			t.MobNames.Add("Mu-miyah Grand Vizier");
			t.MobNames.Add("Mu-miyah Slave Master");			
			t.CompleteCount = 30;
			t.NPCNames.Add("Sir Adarl");
			t.NPCInfo = " Encampment near Neftet";
			t.NPCCoords = "22.2S, 6.3E";
			t.NPCYellowFlagText = "If you will kill 30 of the Mumiyah within the canyon walls or up on the plateaus, I will reward you for your assistance.";
			t.NPCYellowCompleteText = "Well done.";
			mKTSet.MyKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Reedshark Hunting: Lost City of Neftet";
			t.MobNames.Add("Reedshark Hunter");
			t.MobNames.Add("Reedshark Seeker");
			t.MobNames.Add("Tamed Reaper");
			t.MobNames.Add("War Reaper");			
			t.CompleteCount = 30;
			t.NPCNames.Add("Sir Hassim bin Tamarek");
			t.NPCInfo = " Encampment near Neftet";
			t.NPCCoords = "22.3S, 6.3E";
			t.NPCYellowFlagText = "If you will kill 30 of the reedsharks within the canyon walls or up on the plateaus, I will reward you for your aid to the crown.";
			t.NPCYellowCompleteText = "Well done.";
			mKTSet.MyKillTasks.Add(t);
			
			//Frozen Valley Tasks			
			t = new KillTask();
			t.TaskName = "Gurog Kill Task (Soldiers)";
			t.MobNames.Add("Gurog Soldier");
			t.CompleteCount = 20;
			t.NPCNames.Add("Gregoria, Gurog Destroyer");
			t.NPCInfo = "Frozen Valley";
			t.NPCCoords = "83.8N 4.3W";
			t.NPCYellowFlagText = "Kill 20 Gurog Soldiers and I will reward you for your efforts.";
			t.NPCYellowCompleteText = "Well done! Always a pleasure to meet someone who shares my hatred of these beasts.";
			mKTSet.MyKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Gurog Kill Task (Minions)";
			t.MobNames.Add("Gurog Minion");
			t.CompleteCount = 20;
			t.NPCNames.Add("Gregoria, Gurog Destroyer");
			t.NPCInfo = "Frozen Valley";
			t.NPCCoords = "83.8N 4.3W";
			t.NPCYellowFlagText = "Kill 20 Gurog Minions and I will reward you for your efforts.";
			t.NPCYellowCompleteText = "Well done! Ugly creatures aren't they? Glad to be rid of them.";
			mKTSet.MyKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Gurog Kill Task (Henchman)";
			t.MobNames.Add("Gurog Henchmen");
			t.CompleteCount = 20;
			t.NPCNames.Add("Gregoria, Gurog Destroyer");
			t.NPCInfo = "Frozen Valley";
			t.NPCCoords = "83.8N 4.3W";
			t.NPCYellowFlagText = "Kill 20 Gurog Henchmen and I will reward you for your efforts.";
			t.NPCYellowCompleteText = "Well done! Hardly a scratch on you as well, you truly are a great warrior.";
			mKTSet.MyKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Snow Tusker Kill Task (Snow Tusker)";
			t.MobNames.Add("Snow Tusker");
			t.CompleteCount = 20;
			t.NPCNames.Add("Hunter");
			t.NPCInfo = "Frozen Valley";
			t.NPCCoords = "83.8N 4.3W";
			t.NPCYellowFlagText = "Kill 20 Snow Tuskers and I will reward you for your efforts.";
			t.NPCYellowCompleteText = "Fantastic job! Those mutated beasts need to be put down, every kill helps.";	
			mKTSet.MyKillTasks.Add(t);

			t = new KillTask();
			t.TaskName = "Snow Tusker Kill Task (Snow Tusker Warrior)";
			t.MobNames.Add("Snow Tusker Warrior");
			t.CompleteCount = 20;
			t.NPCNames.Add("Hunter");
			t.NPCInfo = "Frozen Valley";
			t.NPCCoords = "83.8N 4.3W";
			t.NPCYellowFlagText = "Kill 20 Snow Tusker Warriors and I will reward you for your efforts.";
			t.NPCYellowCompleteText = "Well done! Their coats remind me of the creature know as a wolf back on Ispar.";
			mKTSet.MyKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Snow Tusker Kill Task (Snow Tusker Leader)";
			t.MobNames.Add("Snow Tusker Leader");
			t.CompleteCount = 10;
			t.NPCNames.Add("Hunter");
			t.NPCInfo = "Frozen Valley";
			t.NPCCoords = "83.8N 4.3W";
			t.NPCYellowFlagText = "Kill 10 Snow Tusker Leaders and I will reward you for your efforts.";
			t.NPCYellowCompleteText = "Amazing that you survived, those tusks can spear a man all the way through.";
			mKTSet.MyKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Frozen Dread Kill Task";
			t.MobNames.Add("Frozen Dread");
			t.CompleteCount = 20;
			t.NPCNames.Add("Kumiko");
			t.NPCInfo = "Frozen Valley";
			t.NPCCoords = "83.8N 4.3W";
			t.NPCYellowFlagText = "Kill 10 of these Frozen Dreads and I will reward you for your efforts.";
			t.NPCYellowCompleteText = "Those creatures haunt my dreams.";
			mKTSet.MyKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Frozen Wight Kill Task";
			t.MobNames.Add("Frozen Wight");
			t.MobNames.Add("Frozen Wight Captain");
			t.MobNames.Add("Frozen Wight Sorcerer");
			t.CompleteCount = 20;
			t.NPCNames.Add("Leilah");
			t.NPCInfo = "Frozen Valley";
			t.NPCCoords = "83.8N 4.3W";
			t.NPCYellowFlagText = "Kill 20 Frozen Wights and I will reward you for your efforts.";
			t.NPCYellowCompleteText = "Those creatures send chills down my spine.";
			mKTSet.MyKillTasks.Add(t);
			
			//Tou-Tou Kill Tasks
			t = new KillTask();
			t.TaskName = "Shadow Flyer Kill Task";
			t.MobNames.Add("Shadow Flyer");
			t.CompleteCount = 15;
			t.NPCNames.Add(" Umbral Guard");
			t.NPCInfo = "Tou-Tou";
			t.NPCCoords = "30.3S, 94.8E";
			t.NPCYellowFlagText = "Kill 15 Shadow Flyers and I will reward you for your efforts.";
			t.NPCYellowCompleteText = "Well done! That should at least keep the corruption from spreading any further.";
			mKTSet.MyKillTasks.Add(t);
			
			//TODO:  Names of proper shadows for this task
			t = new KillTask();
			t.TaskName = "Shadow Kill Task";
			t.MobNames.Add("Shadow");
			t.CompleteCount = 25;
			t.NPCNames.Add("Umbral Guard");
			t.NPCInfo = "Tou-Tou";
			t.NPCCoords = "30.3S, 94.8E";
			t.NPCYellowFlagText = "Kill 25 of the shadows to fight back this corruption.";
			t.NPCYellowCompleteText = "Well done! If you keep this up, Tou-Tou may be ours once again.";
			mKTSet.MyKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Devourer Margul Kill Task";
			t.MobNames.Add("Devourer Margul");
			t.CompleteCount = 15;
			t.NPCNames.Add("Umbral Guard");
			t.NPCInfo = "Tou-Tou";
			t.NPCCoords = "30.3S, 94.8E";
			t.NPCYellowFlagText = "Kill 15 Devourer Marguls and I will reward you for your efforts.";
			t.NPCYellowCompleteText = "Well done! The flapping of those leathery wings is quieter already.";
			mKTSet.MyKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Grievver Shredder Kill Task";
			t.MobNames.Add("Grievver Shredder");
			t.CompleteCount = 15;
			t.NPCNames.Add("Royal Guard");
			t.NPCInfo = "Tou-Tou";
			t.NPCCoords = "30.3S, 94.8E";
			t.NPCYellowFlagText = "Kill 15 Grievver Shredders and I will reward you for your efforts.";
			t.NPCYellowCompleteText = "Well done! The clicking of those ";
			mKTSet.MyKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Void Lord Kill Task";
			t.MobNames.Add("Void Lord");
			t.CompleteCount = 15;
			t.NPCNames.Add("Royal Guard");
			t.NPCInfo = "Tou-Tou";
			t.NPCCoords = "30.3S, 94.8E";
			t.NPCYellowFlagText = "Kill 15 Void Lords and I will reward you for your efforts.";
			t.NPCYellowCompleteText = "Well done! That should help me sleep at night.";
			mKTSet.MyKillTasks.Add(t);
			
			//TODO:  Completion Text
			t = new KillTask();
			t.TaskName = "Aun Golem Hunters:  Mud Golem Sludge Lord";
			t.MobNames.Add("Mud Golem Sludge Lord");
			t.CompleteCount = 5;
			t.NPCNames.Add("Aun Akuarea");
			t.NPCInfo = "(near Samsur)";
			t.NPCCoords = "2.6S, 20.0E";
			t.NPCYellowFlagText = "Kill five of these golems, return to me and I will see that your battles are rewarded!";
			t.NPCYellowCompleteText = "";
			mKTSet.MyKillTasks.Add(t);
			
			//TODO:  Completion Text
			t = new KillTask();
			t.TaskName = "Aun Golem Hunters:  Copper Golem Kingpin";
			t.MobNames.Add("Copper Golem Kingpin");
			t.CompleteCount = 5;
			t.NPCNames.Add("Aun Tiulerea");
			t.NPCInfo = "Eastham";
			t.NPCCoords = "16.5N, 63.6E";
			t.NPCYellowFlagText = "Kill five of these golems, return to me and I will see that your battles are rewarded!";
			t.NPCYellowCompleteText = "";
			mKTSet.MyKillTasks.Add(t);
			
			//TODO:  Completion Text
			t = new KillTask();
			t.TaskName = "Aun Golem Hunters:  Glacial Golem Margrave";
			t.MobNames.Add("Glacial Golem Margrave");
			t.CompleteCount = 5;
			t.NPCNames.Add("Aun Maerirea");
			t.NPCInfo = "Qalaba'r";
			t.NPCCoords = "74.5S, 19.3E";
			t.NPCYellowFlagText = "Kill five of these golems, return to me and I will see that your battles are rewarded!";
			t.NPCYellowCompleteText = "";
			mKTSet.MyKillTasks.Add(t);
			
			//TODO:  Completion Text
			t = new KillTask();
			t.TaskName = "Aun Golem Hunters:  Magma Golem Exarch";
			t.MobNames.Add("Magma Golem Exarch");
			t.CompleteCount = 5;
			t.NPCNames.Add("Aun Autuorea");
			t.NPCInfo = "Candeth Keep";
			t.NPCCoords = "87.5S, 67.0W";
			t.NPCYellowFlagText = "Kill five of these golems, return to me and I will see that your battles are rewarded!";
			t.NPCYellowCompleteText = "";
			mKTSet.MyKillTasks.Add(t);
			
			//TODO:  Completion Text
			t = new KillTask();
			t.TaskName = "Aun Golem Hunters:  Coral Golem Viceroy";
			t.MobNames.Add("Coral Golem Viceroy");
			t.CompleteCount = 5;
			t.NPCNames.Add("Aun Aukherea");
			t.NPCInfo = "Ayan Baqur";
			t.NPCCoords = "60.8S, 88.0W";
			t.NPCYellowFlagText = "Kill five of these golems, return to me and I will see that your battles are rewarded!";
			t.NPCYellowCompleteText = "";
			mKTSet.MyKillTasks.Add(t);
			
			//TODO:  Completion Text
			t = new KillTask();
			t.TaskName = "Aun Golem Hunters:  Platinum Golem Mountain King";
			t.MobNames.Add("Platinum Golem Mountain King");
			t.CompleteCount = 5;
			t.NPCNames.Add("Aun Khekierea");
			t.NPCInfo = "Ayan Baqur";
			t.NPCCoords = "60.8S, 88.0W";
			t.NPCYellowFlagText = "Kill five of these golems, return to me and I will see that your battles are rewarded!";
			t.NPCYellowCompleteText = "";
			mKTSet.MyKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Aun Golem Hunters:  Crystal Lord";
			t.MobNames.Add("Crystal Lord");
			t.CompleteCount = 1;
			t.NPCNames.Add("Aun Tahuirea");
			t.NPCInfo = "Camp";
			t.NPCCoords = "24.0N, 72.0W";
			t.NPCYellowFlagText = "Also, if you and your fellows succeed in defeating one, I will be pleased to share with you the bounties I have recovered from my previous victories.";
			t.NPCYellowCompleteText = "Your tale was truly one of triumph!";
			mKTSet.MyKillTasks.Add(t);
			
			
			
			//Society C&H Tasks
			
			t = new KillTask();
			t.TaskName = "Moarsmen Jailbreak (Prisoners)";
			t.MobNames.Add("Moarsman Prisoner");
			t.CompleteCount = 20;  
			t.NPCNames.Add("Avarin");
			t.NPCInfo = "Freebooter Isle";
			t.NPCCoords = "56.4S 96.9E";
			t.NPCYellowFlagText = "I'm authorized to pay a bounty for culling the population of escaped moarsmen prisoners by twenty.";
			t.NPCYellowCompleteText = "For culling the moarsman prisoner population here's the bounty you're owed.";
			mKTSet.MyKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Moarsmen Jailbreak (Blessed Leader)";
			t.MobNames.Add("Large Blessed Moarsman");
			t.CompleteCount = 1;
			t.NPCNames.Add("Avarin");
			t.NPCInfo = "Freebooter Isle";
			t.NPCCoords = "56.4S 96.9E";
			t.NPCYellowFlagText = "I'm authorized to pay a bounty for the death of the Blessed moarsman leader.";
			t.NPCYellowCompleteText = "For putting down the Blessed leader here's the bounty you're owed.";
			mKTSet.MyKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Moarsmen Jailbreak (Ardent Leader)";
			t.MobNames.Add("Large Ardent Moarsman");
			t.CompleteCount = 1;  
			t.NPCNames.Add("Avarin");
			t.NPCInfo = "Freebooter Isle";
			t.NPCCoords = "56.4S 96.9E";
			t.NPCYellowFlagText = "I'm authorized to pay a bounty for the death of the Ardent moarsman leader.";
			t.NPCYellowCompleteText = "For putting down the Ardent leader here's the bounty you're owed.";
			mKTSet.MyKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Moarsmen Jailbreak (Verdant Leader)";
			t.MobNames.Add("Large Verdant Moarsman");
			t.CompleteCount = 1; 
			t.NPCNames.Add("Avarin");
			t.NPCInfo = "Freebooter Isle";
			t.NPCCoords = "56.4S 96.9E";
			t.NPCYellowFlagText = "I'm authorized to pay a bounty for the death of the Verdant moarsman leader.";
			t.NPCYellowCompleteText = "For putting down the Verdant leader here's the bounty you're owed.";
			mKTSet.MyKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Black Coral Golem Kill Task";
			t.MobNames.Add("Black Coral Golem");
			t.CompleteCount = 100;
			t.NPCNames.Add("Chiriko");
			t.NPCInfo = "Northwatch Castle Black Market";
			t.NPCCoords = "81.5N 25.0E";
			t.NPCYellowFlagText = "If you would be willing to go there and prove your prowess by destroying 100 of these strange golems, I will reward you handsomely for your actions.";
			t.NPCYellowCompleteText = "Well done, well done indeed. You have proven your skill and honored my task. I thank you. Here is the reward promised.";
			mKTSet.MyKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Weeding of the Deru Tree";
			t.MobNames.Add("Eyestalk of T'thuun");
			t.MobNames.Add("Tendril of T'thuun");
			t.MobNames.Add("Tentacle of T'thuun");
			t.CompleteCount = 50;
			t.NPCNames.Add("Valerian McGreggor");
			t.NPCInfo = "Freebooter Keep Black Market";
			t.NPCCoords = "64.0S 97.5E";
			t.NPCYellowFlagText = "I'll tell ye what. If ye go out there and kill 50 of those tentacles for me, just the ones on Freebooter, mind ye, I'll make it worth ye while.";
			t.NPCYellowCompleteText = "That should do it! The Mana flows around the Ruins of Degar'Alesh are moving much better now, thank ye. Here's a little something for yer efforts."; 
			mKTSet.MyKillTasks.Add(t);

			t = new KillTask();
			t.TaskName = "Blighted Coral Golem Kill Task";
			t.MobNames.Add("Blighted Coral Golem");
			t.CompleteCount = 100;
			t.NPCNames.Add("Hanzo");
			t.NPCInfo = "Northwatch Castle Black Market";
			t.NPCCoords = "81.5N 25.0E";
			t.NPCYellowFlagText = "I wish you to destroy 100 of the foul, Blighted Coral Golems upon the isle.";
			t.NPCYellowCompleteText = "Impressively done, Honored Master. You have accomplished all I have wished from you. Now, for your promised reward.";
			mKTSet.MyKillTasks.Add(t);		
					
			t = new KillTask();
			t.TaskName = "Blessed Moarsman Kill Task";
			t.MobNames.Add("Blessed Moarsman");
			t.CompleteCount = 50;
			t.NPCNames.Add("Kieran Stronghammer");
			t.NPCNames.Add("Alexander Bowspeaker");
			t.NPCNames.Add("Ian Foefinder");
			t.NPCInfo = "Society Stronghold";
			t.NPCCoords = "Unknown";
			t.NPCYellowFlagText = "Survive, kill 50 Blessed Moarsmen, and I'll make sure you are recognized for your valorous service to our Society.";
			t.NPCYellowCompleteText = "Congratulations, you survived and succeeded. Here, allow me to reward you for your assistance to our Society.";
			mKTSet.MyKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Gear Knight Phalanx Kill Task";
			t.MobNames.Add("Invading Bronze Gauntlet Phalanx");
			t.MobNames.Add("Invading Copper Cog Phalanx");
			t.MobNames.Add("Invading Iron Blade Phalanx");
			t.MobNames.Add("Invading Silver Scope Phalanx");
			t.CompleteCount = 10;
			t.NPCNames.Add("Ladice");
			t.NPCNames.Add("Tressar");
			t.NPCNames.Add("Aun Quanah");
			t.NPCInfo = "Society Stronghold";
			t.NPCCoords = "Unknown";
			t.NPCYellowFlagText = "Go there and defeat 10 Phalanx.";
			t.NPCYellowCompleteText = "Satisfactory. Take these rewards as compensation for your efforts.";
			mKTSet.MyKillTasks.Add(t);
			
			//TODO:  This is actually a collect task, move to collection tashs
			//Item:  "Holy Symbol"
//			t = new KillTask();
//			t.TaskName = "High Priest of T'thuun Kill Task";
//			t.MobNames.Add("High Priest of T'thuun");
//			t.CompleteCount = 1;
//			t.NPCNames.Add("Kaymor ibn Dumandi");
//			t.NPCNames.Add("Milos ibn Ashud");
//			t.NPCNames.Add("Lunbal Dolicci");
//			t.NPCInfo = "Society Stronghold";
//			t.NPCCoords = "Unknown";
//			t.NPCYellowFlagText = "Kill this Moarsman High Priest and bring back the Holy Symbol he wields as proof of your kill.";
//			t.NPCYellowCompleteText = "Excellent, you were able to defeat the High Priest!";
//			mKTSet.MyKillTasks.Add(t);			
			
			t = new KillTask();
			t.TaskName = "Killer Phyntos Wasp Kill Task";
			t.MobNames.Add("Killer Phyntos Drone");
			t.MobNames.Add("Killer Phyntos Soldier");
			t.MobNames.Add("Killer Phyntos Swarm");
			t.CompleteCount = 1;
			t.NPCNames.Add("Khanldun");
			t.NPCNames.Add("Jonathan");
			t.NPCNames.Add("Mik");
			t.NPCInfo = "Society Stronghold";
			t.NPCCoords = "Unknown";
			t.NPCYellowFlagText = "The society will appreciate any efforts you make towards their extermination.";
			t.NPCYellowCompleteText = "You've done well in exterminating this aggressive breed of Phyntos.";
			mKTSet.MyKillTasks.Add(t);
				
			//TODO:  Flag Text
			t = new KillTask();
			t.TaskName = "Moguth Moarsman Kill Task";
			t.MobNames.Add("Moguth Moarsman");
			t.CompleteCount = 60;
			t.NPCNames.Add("Marconi di Bellenesse");
			t.NPCNames.Add("Ricaldo di Alduressa");
			t.NPCNames.Add("Berrando Piatelli");
			t.NPCInfo = "Society Stronghold";
			t.NPCCoords = "Unknown";
			t.NPCYellowFlagText = "";
			t.NPCYellowCompleteText = "Well done! Here, allow me to reward you for your assistance to our Society.";
			mKTSet.MyKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Phyntos Larva Kill Task";
			t.MobNames.Add("Phyntos Larva");
			t.CompleteCount = 20;
			t.NPCNames.Add("Alderic");
			t.NPCNames.Add("Haruki");
			t.NPCNames.Add("Ghali al-Fariq");
			t.NPCInfo = "Society Stronghold";
			t.NPCCoords = "Unknown";
			t.NPCYellowFlagText = "In order to control the population of hyper aggressive Phyntos I'm contracting adventurers to kill their larvae.";
			t.NPCYellowCompleteText = "Excellent work infiltrating the Phyntos hive and killing their larvae.";
			mKTSet.MyKillTasks.Add(t);			
			
			//TODO:  Flag Text
			t = new KillTask();
			t.TaskName = "Shoguth Moarsman Kill Task";
			t.MobNames.Add("Shoguth Moarsman");
			t.CompleteCount = 40;
			t.NPCNames.Add("Gavin Hammerstone");
			t.NPCNames.Add("Dorn Bowspeaker");
			t.NPCNames.Add("Kylos Hunterson");
			t.NPCInfo = "Society Stronghold";
			t.NPCCoords = "Unknown";
			t.NPCYellowFlagText = "";
			t.NPCYellowCompleteText = "Well done! Here, allow me to reward you for your assistance to our Society.";
			mKTSet.MyKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Wight Blade Sorcerer Kill Task";
			t.MobNames.Add("Wight Blade Sorcerer");
			t.CompleteCount = 12;
			t.NPCNames.Add("Mashira bint Tamur");
			t.NPCNames.Add("Zumaq al-Jaluzi");
			t.NPCNames.Add("Idaris bint Qumal");
			t.NPCInfo = "Society Stronghold";
			t.NPCCoords = "Unknown";
			t.NPCYellowFlagText = "Just concern yourself with killing 12 Wight Blade Sorcerers, and report back to me when you're done.";
			t.NPCYellowCompleteText = "Congratulations! Twelve dead Wight Blade Sorcerers. Our field researchers will be quite pleased. I can reward you now.";
			mKTSet.MyKillTasks.Add(t);
			
			//Graveyard Kill Tasks
			
			//This is a one time only kill task for society flagging, omitted
//			t = new KillTask();
//			t.TaskName = "Blight Spirit Kill Task";
//			t.MobNames.Add("Blight Spirit");
//			t.CompleteCount = 8;
//			t.NPCNames.Add("Candrus Steady-Hand");
//			t.NPCInfo = "Graveyard";
//			t.NPCCoords = "65.8S, 45.3W";
//			t.NPCYellowFlagText = "";
//			t.NPCYellowCompleteText = "";
//			mKTSet.MyKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Guardian Statue Kill Task";
			t.MobNames.Add("Guardian Statue");
			t.CompleteCount = 10;
			t.NPCNames.Add("Shade of Fordroth");
			t.NPCInfo = "Mhoire Castle Northeast Tower";
			t.NPCCoords = "64.7S 45.2W";
			t.NPCYellowFlagText = "Destroy 10 of these corrupted gargoyles to ease the pain of those that wander these halls and I will reward you.";
			t.NPCYellowCompleteText = "Well done, champion. You must be skilled indeed. Allow me to reward you.";
			mKTSet.MyKillTasks.Add(t);	
			
			//TODO:  This is actually a collection task,  move to collection list.
//			t = new KillTask();
//			t.TaskName = "Noble Remains Kill Task";
//			t.MobNames.Add("Noble Remain");
//			t.CompleteCount = 10;
//			t.NPCNames.Add("Shade of Ormend");
//			t.NPCInfo = "Mhoire Castle Northeast Tower";
//			t.NPCCoords = "64.7S 45.2W";
//			t.NPCYellowFlagText = "Destroy these corrupted remains and gather the signet rings from the bones. Return them to me and I will reward you.";
//			t.NPCYellowCompleteText = "I see that you have recovered 10 signet rings of House Mhoire.";
//			mKTSet.MyKillTasks.Add(t);
			
			//TODO:  Completion Text
			t = new KillTask();
			t.TaskName = "Grave Rat Kill Task";
			t.MobNames.Add("Grave Rat");
			t.CompleteCount = 100;
			t.NPCNames.Add("Lo Shoen");
			t.NPCInfo = "Graveyard";
			t.NPCCoords = "65.3S, 43.4W";
			t.NPCYellowFlagText = "If you want to help me, kill 100 of these Grave Rats. Maybe then I will have more work for you.";
			t.NPCYellowCompleteText = "";
			mKTSet.MyKillTasks.Add(t);

			//Gear Knight Kill Tasks
			//UNDONE:  complete messages may be reversed.
			t = new KillTask();
			t.TaskName = "Gear Knight Kill Task (Squires)";
			t.MobNames.Add("Invading Iron Blade Squire");
			t.MobNames.Add("Invading Silver Scope Squire");
			t.MobNames.Add("Invading Copper Cog Squire");
			t.MobNames.Add("Invading Bronze Gauntlet Squire");
			t.CompleteCount = 10;
			t.NPCNames.Add("Sir Yanov");
			t.NPCInfo = "Direlands Gear Knight Resistance Camp";
			t.NPCCoords = "12.3S, 74.9W";
			t.NPCYellowFlagText = "Those Squires are becoming a serious problem. They constantly interupt the supply lines to our camp. Destroy 10 of them and I'll reward you.";
			t.NPCYellowCompleteText = "Given the time it takes for these things to regroup, I'll have more work for you by this time tomorrow.";
			mKTSet.MyKillTasks.Add(t);			
			
			t = new KillTask();
			t.TaskName = "Gear Knight Kill Task (Knights)";
			t.MobNames.Add("Invading Iron Blade Knight");
			t.MobNames.Add("Invading Silver Scope Knight");
			t.MobNames.Add("Invading Copper Cog Knight");
			t.MobNames.Add("Invading Bronze Gauntlet Knight");
			t.CompleteCount = 10;
			t.NPCNames.Add("Sir Yanov");
			t.NPCInfo = "Direlands Gear Knight Resistance Camp";
			t.NPCCoords = "12.3S, 74.9W";
			t.NPCYellowFlagText = "If you wish to help me, just head over to the area these 'Gear Knights' have occupied and kill 10 Knights.";
			t.NPCYellowCompleteText = "Congratulations, you survived and succeeded.  Here, allow me to reward you for your assistance to our Queen.";
			mKTSet.MyKillTasks.Add(t);
			
			//TODO:  Insert texts below for flag and complete
			
			t = new KillTask();
			t.TaskName = "Iron Blade Commander";
			t.MobNames.Add("Invading Iron Blade Commander");
			t.CompleteCount = 1;
			t.NPCNames.Add("Dame Trielle");
			t.NPCInfo = "Direlands Gear Knight Resistance Camp";
			t.NPCCoords = "12.3S 75.0W";
			t.NPCYellowFlagText = "";
			t.NPCYellowCompleteText = "";
			mKTSet.MyKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Bronze Gauntlet Trooper Kill Task";
			t.MobNames.Add("Bronze Gauntlet Trooper");
			t.CompleteCount = 25;
			t.NPCNames.Add("Lieutenant Grenlin");
			t.NPCInfo = "Yaraq to Ijaniya (22.4S, 0.2E)";
			t.NPCCoords = "33.4S, 6.3E";
			t.NPCYellowFlagText = "";
			t.NPCYellowCompleteText = "";
			mKTSet.MyKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Copper Cog Trooper Kill Task";
			t.MobNames.Add("Copper Cog Trooper");
			t.CompleteCount = 25;
			t.NPCNames.Add("Lieutenant Zin");
			t.NPCInfo = "Yaraq to Ijaniya (22.4S, 0.2E)";
			t.NPCCoords = "33.4S, 6.3E";
			t.NPCYellowFlagText = "";
			t.NPCYellowCompleteText = "";
			mKTSet.MyKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Iron Blade Trooper Kill Task";
			t.MobNames.Add("Iron Blade Trooper");
			t.CompleteCount = 25;
			t.NPCNames.Add("Lieutenant Micham");
			t.NPCInfo = "Yaraq to Ijaniya (22.4S, 0.2E)";
			t.NPCCoords = "33.4S, 6.3E";
			t.NPCYellowFlagText = "";
			t.NPCYellowCompleteText = "";
			mKTSet.MyKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Silver Scope Trooper Kill Task";
			t.MobNames.Add("Silver Scope Trooper");
			t.CompleteCount = 25;
			t.NPCNames.Add("Lieutenant Faen");
			t.NPCInfo = "Yaraq to Ijaniya (22.4S, 0.2E)";
			t.NPCCoords = "33.4S, 6.3E";
			t.NPCYellowFlagText = "";
			t.NPCYellowCompleteText = "";
			mKTSet.MyKillTasks.Add(t);
			

			
//			//TODO:  Fillout mob list
//			t = new KillTask();
//			t.TaskName = "Glenden Wood Invaders	Invaders";
//			t.MobNames.Add("Invader");
//			t.CompleteCount = 20;
//			t.NPCNames.Add("Londigul Ellic the Armorer";
//			t.NPCInfo = "Glenden Wood";
//			t.NPCCoords = "29.9N, 27.1E";
//			mKTSet.MyKillTasks.Add(t);	

			//Royal Tent Kill Tasks			

			t = new KillTask();
			t.TaskName = "Golem Samurai Kill Task";
			t.MobNames.Add("Bronze Golem Samurai");
			t.MobNames.Add("Iron Golem Samurai");
			t.MobNames.Add("Clay Golem Samurai");
			t.CompleteCount = 10;
			t.NPCNames.Add("Lieutenant Aurin");
			t.NPCInfo = "Royal Tent";
			t.NPCCoords = "80.7N 43.0W";
			t.NPCYellowFlagText = "";
			t.NPCYellowCompleteText = "";
			mKTSet.MyKillTasks.Add(t);	
			
			t = new KillTask();
			t.TaskName = "Spectral Archer Kill Task";
			t.MobNames.Add("Spectral Archer");
			t.CompleteCount = 10;
			t.NPCNames.Add("Sergeant Trebuus");
			t.NPCInfo = "Royal Tent";
			t.NPCCoords = "80.7N 43.0W";
			t.NPCYellowFlagText = "";
			t.NPCYellowCompleteText = "";
			mKTSet.MyKillTasks.Add(t);

			t = new KillTask();
			t.TaskName = "Spectral Bushi Kill Task";
			t.MobNames.Add("Spectral Bushi");
			t.CompleteCount = 10;
			t.NPCNames.Add("Corporal Irashi");
			t.NPCInfo = "Royal Tent";
			t.NPCCoords = "80.7N 43.0W";
			t.NPCYellowFlagText = "";
			t.NPCYellowCompleteText = "";
			mKTSet.MyKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Spectral Blade and Claw Kill Task";
			t.MobNames.Add("Spectral Blade Adept");
			t.MobNames.Add("Spectral Blade Master");
			t.MobNames.Add("Spectral Claw Adept");
			t.MobNames.Add("Spectral Claw Master");
			t.CompleteCount = 10;
			t.NPCNames.Add("Griffon");
			t.NPCInfo = "Royal Tent";
			t.NPCCoords = "80.7N 43.0W";
			t.NPCYellowFlagText = "";
			t.NPCYellowCompleteText = "";
			mKTSet.MyKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Spectral Mage Kill Task";
			t.MobNames.Add("Spectral Bloodmage");
			t.MobNames.Add("Spectral Voidmage");
			t.CompleteCount = 10;
			t.NPCNames.Add("Lord Eorlinde");
			t.NPCInfo = "Royal Tent";
			t.NPCCoords = "80.7N 43.0W";
			t.NPCYellowFlagText = "";
			t.NPCYellowCompleteText = "";
			mKTSet.MyKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Spectral Minion Kill Task";
			t.MobNames.Add("Spectral Minion");
			t.CompleteCount = 15;
			t.NPCNames.Add("Aun Kirtal");
			t.NPCInfo = "Royal Tent";
			t.NPCCoords = "80.7N 43.0W";
			t.NPCYellowFlagText = "";
			t.NPCYellowCompleteText = "";
			mKTSet.MyKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Spectral Nanjou Shou-jen Kill Task";
			t.MobNames.Add("Spectral Nanjou Shou-jen");
			t.CompleteCount = 5;
			t.NPCNames.Add("Hanamoto Aki'ko");
			t.NPCInfo = "Royal Tent";
			t.NPCCoords = "80.7N 43.0W";
			t.NPCYellowFlagText = "";
			t.NPCYellowCompleteText = "";
			mKTSet.MyKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Spectral Samurai Kill Task";
			t.MobNames.Add("Spectral Samurai");
			t.CompleteCount = 10;
			t.NPCNames.Add("Lieutenant Takamaki");
			t.NPCInfo = "Royal Tent";
			t.NPCCoords = "80.7N 43.0W";
			t.NPCYellowFlagText = "";
			t.NPCYellowCompleteText = "";
			mKTSet.MyKillTasks.Add(t);
						
			
			t = new KillTask();
			t.TaskName = "Eye of T'thuun Quest";
			t.MobNames.Add("Tentacle of T'thuun");
			t.CompleteCount = 50;
			t.NPCNames.Add("Tamara du Cinghalle");
			t.NPCInfo = "Greenspire";
			t.NPCCoords = "43.2N 67.1W";
			t.NPCYellowFlagText = "";
			t.NPCYellowCompleteText = "";
			mKTSet.MyKillTasks.Add(t);

			
			
			
			//TODO:  Viamontian knight types
			t = new KillTask();
			t.TaskName = "Torgash's Tasks";
			t.MobNames.Add("Viamontian Knight");
			t.CompleteCount = 30;
			t.NPCNames.Add("Torgash");
			t.NPCInfo = "Shattered Outlands";
			t.NPCCoords = "94.0N 45.9W";
			mKTSet.MyKillTasks.Add(t);
			
			
			
			

			t = new KillTask();
			t.TaskName = "Arctic Mattekar Kill Task";
			t.MobNames.Add("Arctic Mattekar");
			t.CompleteCount = 25;
			t.NPCNames.Add("Enzo Ilario");
			t.NPCInfo = "Silyun";
			t.NPCCoords = "87.4N 70.5W";
			mKTSet.MyKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Banished Creature Kill Task";
			t.MobNames.Add("Banished Banderling");
			t.MobNames.Add("Banished Drudge");
			t.MobNames.Add("Banished Grievver");
			t.MobNames.Add("Banished Lugian");
			t.MobNames.Add("Banished Monouga");
			t.MobNames.Add("Banished Mu-miyah");
			t.MobNames.Add("Banished Olthoi");
			t.MobNames.Add("Banished Phyntos Wasp");
			t.MobNames.Add("Banished Shadow");
			t.MobNames.Add("Banished Tumerok");
			t.MobNames.Add("Banished Tusker");
			t.CompleteCount = 10;
			t.NPCNames.Add("Belinda du Loc");
			t.NPCInfo = "Stonehold";
			t.NPCCoords = "68.9N 21.6W";
			mKTSet.MyKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Fallen Creature Kill Task";
			t.MobNames.Add("Fallen Crystal Shard");
			t.MobNames.Add("Fallen Doll");
			t.MobNames.Add("Fallen Drudge");
			t.MobNames.Add("Fallen Grievver");
			t.MobNames.Add("Fallen Lugian");
			t.MobNames.Add("Fallen Margul");
			t.MobNames.Add("Fallen Marionette");
			t.MobNames.Add("Fallen Mite");
			t.MobNames.Add("Fallen Rift");
			t.MobNames.Add("Fallen Shadow");
			t.MobNames.Add("Fallen Tumerok");
			t.CompleteCount = 10;
			t.NPCNames.Add("Belinda du Loc");
			t.NPCInfo = "Stonehold";
			t.NPCCoords = "68.9N 21.6W";
			mKTSet.MyKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Benek Niffis Kill Task";
			t.MobNames.Add("Benek Niffis");
			t.CompleteCount = 50;
			t.NPCNames.Add("Colista Fluress");
			t.NPCInfo = "The Deep (Vissidal)";
			t.NPCCoords = "77.8N 67.1E";
			mKTSet.MyKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Blood Shreth Kill Task";
			t.MobNames.Add("Blood Shreth");
			t.CompleteCount = 10;
			t.NPCNames.Add("San Ming");
			t.NPCInfo = "Shoushi";
			t.NPCCoords = "33.5S, 72.8E";
			mKTSet.MyKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Coral Golem Kill Task";
			t.MobNames.Add("Coral Golem");
			t.CompleteCount = 50;
			t.NPCNames.Add("Malrin");
			t.NPCInfo = "Sanamar";
			t.NPCCoords = "71.8N, 60.8W";
			mKTSet.MyKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Deathcap Thrungus Kill Task";
			t.MobNames.Add("Deathcap Thrungus");
			t.CompleteCount = 25;
			t.NPCNames.Add("Jiang Li");
			t.NPCInfo = "Westwatch";
			t.NPCCoords = "72.7N 73.3W";
			mKTSet.MyKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Dire Mattekar Kill Task";
			t.MobNames.Add("Dire Mattekar");
			t.CompleteCount = 10;
			t.NPCNames.Add("Fergal the Dire");
			t.NPCInfo = "Baishi";
			t.NPCCoords = "49.4S, 62.4E";
			mKTSet.MyKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Ebon Gromnie Kill Task";
			t.MobNames.Add("Ebon Gromnie");
			t.CompleteCount = 25;
			t.NPCNames.Add("Afra bint Abbas");
			t.NPCInfo = "Redspire";
			t.NPCCoords = "40.8N, 83.0W";
			mKTSet.MyKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Elemental Kill Task";
			t.MobNames.Add("Caustic");
			t.MobNames.Add("Synnast");
			t.MobNames.Add("Inferno");
			t.MobNames.Add("Hyems");
			t.CompleteCount = 25;
			t.NPCNames.Add("Zahir");
			t.NPCInfo = "Stonehold";
			t.NPCCoords = "68.7N, 21.5W";
			mKTSet.MyKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Floeshark Kill Task";
			t.MobNames.Add("Floeshark");
			t.CompleteCount = 50;
			t.NPCNames.Add("Ryuichi Tai");
			t.NPCInfo = "Eastwatch";
			t.NPCCoords = "90.3N, 43.0W";
			mKTSet.MyKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Gold Gear Trooper Kill Task";
			t.MobNames.Add("Gold Gear Trooper");
			t.CompleteCount = 25;
			t.NPCNames.Add("Sir Stavitor");
			t.NPCInfo = "Yaraq to Ijaniya (22.4S, 0.2E)";
			t.NPCCoords = "33.4S, 6.3E";
			mKTSet.MyKillTasks.Add(t);
			
			
			
			
			
			t = new KillTask();
			t.TaskName = "Grievver Violator Kill Task";
			t.MobNames.Add("Grievver Violator");
			t.CompleteCount = 100;
			t.NPCNames.Add("Moina");
			t.NPCInfo = "Eastwatch";
			t.NPCCoords = "90.4N 43.1W";
			mKTSet.MyKillTasks.Add(t);
			
			
			
			t = new KillTask();
			t.TaskName = "Guruk Basher Kill Task";
			t.MobNames.Add("Guruk Basher");
			t.CompleteCount = 40;
			t.NPCNames.Add("Shiruuk");
			t.NPCInfo = "Kor-Gursha";
			t.NPCCoords = "Bur";
			mKTSet.MyKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Guruk Colossus Kill Task";
			t.MobNames.Add("Guruk Colossi");
			t.MobNames.Add("Guruk Colossus");
			t.CompleteCount = 30;
			t.NPCNames.Add("Brogosh");
			t.NPCInfo = "Kor-Gursha";
			t.NPCCoords = "Bur";
			mKTSet.MyKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Guruk Fiend Kill Task";
			t.MobNames.Add("Guruk Fiend");
			t.CompleteCount = 30;
			t.NPCNames.Add("Mohor");
			t.NPCInfo = "Kor-Gursha";
			t.NPCCoords = "Bur";
			mKTSet.MyKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Guruk Marauder Kill Task";
			t.MobNames.Add("Guruk Marauder");
			t.CompleteCount = 40;
			t.NPCNames.Add("Kurket");
			t.NPCInfo = "Kor-Gursha";
			t.NPCCoords = "Bur";
			mKTSet.MyKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Guruk Monstrosity Kill Task";
			t.MobNames.Add("Guruk Monstrosity");
			t.CompleteCount = 10;
			t.NPCNames.Add("Borsh");
			t.NPCInfo = "Kor-Gursha";
			t.NPCCoords = "Bur";
			mKTSet.MyKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Guruk Smasher Kill Task";
			t.MobNames.Add("Guruk Smasher");
			t.CompleteCount = 40;
			t.NPCNames.Add("Kushuk");
			t.NPCInfo = "Kor-Gursha";
			t.NPCCoords = "Bur";
			mKTSet.MyKillTasks.Add(t);
			
			//Harvest Reaper Kill Task Omitted due to complexity and seasonal
			
			t = new KillTask();
			t.TaskName = "Hea Windreave Kill Task";
			t.MobNames.Add("Hea Windreave");
			t.CompleteCount = 25;
			t.NPCNames.Add("Susana du Loc");
			t.NPCInfo = "Redspire";
			t.NPCCoords = "40.7N, 83.2W";
			mKTSet.MyKillTasks.Add(t);
			
			
			
			t = new KillTask();
			t.TaskName = "Iron Spined Chittick Kill Task";
			t.MobNames.Add("Iron Spined Chittick");
			t.CompleteCount = 50;
			t.NPCNames.Add("Aidene");
			t.NPCInfo = "Oolatanga's Refuge";
			t.NPCCoords = "2.0N 95.6E";
			mKTSet.MyKillTasks.Add(t);

			t = new KillTask();
			t.TaskName = "Kilif Zefir Kill Task";
			t.MobNames.Add("Kilif Zefir");
			t.CompleteCount = 35;
			t.NPCNames.Add("Hadiya bint Anan");
			t.NPCInfo = "Shoushi";
			t.NPCCoords = "33.7S 73.1E";
			mKTSet.MyKillTasks.Add(t);	
			
			t = new KillTask();
			t.TaskName = "K'nath An'dras Kill Task";
			t.MobNames.Add("K'nath An'dra");
			t.CompleteCount = 25;
			t.NPCNames.Add("Nona");
			t.NPCInfo = "Wai Jhou";
			t.NPCCoords = "61.8S 51.3W";
			mKTSet.MyKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Littoral Siraluun Kill Task";
			t.MobNames.Add("Littoral Siraluun");
			t.CompleteCount = 25;
			t.NPCNames.Add(" Rico Cellini");
			t.NPCInfo = "Greenspire";
			t.NPCCoords = "43.2N 67.1W";
			mKTSet.MyKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Putrid Moar Kill Task";
			t.MobNames.Add("Putrid Moar");
			t.CompleteCount = 25;
			t.NPCNames.Add("Tibik");
			t.NPCInfo = "Kor-Gursha";
			t.NPCCoords = "Bur";
			mKTSet.MyKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Mosswart Worshipper Kill Task";
			t.MobNames.Add("Mosswart Worshipper");
			t.CompleteCount = 175;
			t.NPCNames.Add("Orfeo Orlando");
			t.NPCInfo = "Eastwatch";
			t.NPCCoords = "90.2N, 43.1W";
			mKTSet.MyKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Mottled Carenzi Kill Task";
			t.MobNames.Add("Mottled Carenzi");
			t.CompleteCount = 50;
			t.NPCNames.Add("Grania the Bold");
			t.NPCInfo = "Candeth Keep";
			t.NPCCoords = "87.6S 67.4W";
			mKTSet.MyKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Mosswart Townsfolk Kill Task";
			t.MobNames.Add("Mosswart Townsfolk");
			t.CompleteCount = 40;
			t.NPCNames.Add("Corporal Massein");
			t.NPCInfo = "Kryst";
			t.NPCCoords = "74.4S 84.6E";
			mKTSet.MyKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Mucky Moarsman Kill Task";
			t.MobNames.Add("Mucky Moarsman");
			t.CompleteCount = 50;
			t.NPCNames.Add("Algar Oreksun");
			t.NPCInfo = "The Deep (Vissidal)";
			t.NPCCoords = "77.8N 67.1E";
			mKTSet.MyKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Naughty Skeleton Kill Task";
			t.MobNames.Add("Naughty Skeleton");
			t.CompleteCount = 100;
			t.NPCNames.Add("Taku Yukio");
			t.NPCInfo = "Eastwatch";
			t.NPCCoords = "90.3N 43.1W";
			mKTSet.MyKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Olthoi Drone Kill Task";
			t.MobNames.Add("Olthoi Drone");
			t.CompleteCount = 20;
			t.NPCNames.Add("Olthoi Hunter");
			t.NPCInfo = "Arwic (South)";
			t.NPCCoords = "30.9N 56.3E";
			mKTSet.MyKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Olthoi Nettler Kill Task";
			t.MobNames.Add("Olthoi Nettler");
			t.CompleteCount = 10;
			t.NPCNames.Add("Olthoi Hunter");
			t.NPCInfo = "Arwic (South)";
			t.NPCCoords = "30.9N 56.3E";
			mKTSet.MyKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Olthoi Nymph Kill Task";
			t.MobNames.Add("Olthoi Nymph");
			t.CompleteCount = 20;
			t.NPCNames.Add("Olthoi Hunter");
			t.NPCInfo = "Arwic (South)";
			t.NPCCoords = "30.9N 56.3E";
			mKTSet.MyKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Olthoi Ripper Kill Task";
			t.MobNames.Add("Olthoi Ripper");
			t.CompleteCount = 250;
			t.NPCNames.Add("Marcello");
			t.NPCInfo = "Eastwatch";
			t.NPCCoords = "90.2N 43.1W";
			mKTSet.MyKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Paradox-touched Grub Kill Task";
			t.MobNames.Add("Paradox-touched Olthoi Noble Grub");
			t.CompleteCount = 50;
			t.NPCNames.Add("Alicia Swiftblade");
			t.NPCInfo = "Olthoi North";
			t.NPCCoords = "43.8N 54.9E";
			mKTSet.MyKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Paradox-touched Nymph Kill Task";
			t.MobNames.Add("Paradox-touched Olthoi Warrior Nymph");
			t.CompleteCount = 50;
			t.NPCNames.Add("Tomihino");
			t.NPCInfo = "Olthoi North";
			t.NPCCoords = "43.8N 54.9E";
			mKTSet.MyKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Plate Armoredillo Kill Task";
			t.MobNames.Add("Plate Armoredillo");
			t.CompleteCount = 25;
			t.NPCNames.Add("Saqr");
			t.NPCInfo = "Fort Tethana";
			t.NPCCoords = "1.5N 71.8W";
			mKTSet.MyKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Polardillo Kill Task";
			t.MobNames.Add("Polardillo");
			t.CompleteCount = 10;
			t.NPCNames.Add("Alessandro Mardor");
			t.NPCInfo = "Sanamar";
			t.NPCCoords = "72.0N 61.2W";
			mKTSet.MyKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Polar Ursuin Kill Task";
			t.MobNames.Add("Polar Ursuin");
			t.CompleteCount = 25;
			t.NPCNames.Add("Mariabella Varanese");
			t.NPCInfo = "Fiun Outpost";
			t.NPCCoords = "95.6N, 56.3W";
			mKTSet.MyKillTasks.Add(t);
			
			//Pumpkin Lord Kill Task
			
			t = new KillTask();
			t.TaskName = "Rare Game Kill Task";
			t.MobNames.Add("Basalt Golem");
			t.MobNames.Add("Cold One");
			t.MobNames.Add("Dark Myrmidon");
			t.MobNames.Add("Dark Sorcerer");
			t.MobNames.Add("Lord of Decay");
			t.MobNames.Add("Lugian Warlord");
			t.MobNames.Add("Master of the Pack");
			t.MobNames.Add("Pure One");
			t.MobNames.Add("Sentient Fragment");
			t.MobNames.Add("Swamp King");
			t.MobNames.Add("Tundra Mattekar");
			t.CompleteCount = 50;
			t.NPCNames.Add(" Belinda du Loc");
			t.NPCInfo = "Stonehold";
			t.NPCCoords = "68.9N 21.6W";
			mKTSet.MyKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Remoran Sea Raptor Kill Task";
			t.MobNames.Add("Remoran Sea Raptor");
			t.CompleteCount = 50;
			t.NPCNames.Add("Peng-Ya");
			t.NPCInfo = "The Deep (Vissidal)";
			t.NPCCoords = "77.8N 67.1E";
			mKTSet.MyKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Repugnant Eater Kill Task";
			t.MobNames.Add("Repugnant Eater");
			t.CompleteCount = 50;
			t.NPCNames.Add("Xun Yu");
			t.NPCInfo = "Eastwatch";
			t.NPCCoords = "90.2N 43.1W";
			mKTSet.MyKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Ruschk Kill Task";
			t.MobNames.Add("Ruschk Draktehn");
			t.MobNames.Add("Ruschk Laktar");
			t.CompleteCount = 30;
			t.NPCNames.Add("Commander Rylane di Cinghalle");
			t.NPCInfo = "Shattered Outlands";
			t.NPCCoords = "93.2N 48.2W";
			mKTSet.MyKillTasks.Add(t);
			
			
			t = new KillTask();
			t.TaskName = "Shadow-touched Virindi Paradox Kill Task";
			t.MobNames.Add("Shadow-touched Virindi Paradox");
			t.CompleteCount = 75;
			t.NPCNames.Add("Guard Li");
			t.NPCInfo = "Wai Jhou";
			t.NPCCoords = "61.8S, 51.3W";
			mKTSet.MyKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Shadow-touched Virindi Quidiox Kill Task";
			t.MobNames.Add("Shadow-touched Virindi Quidiox");
			t.CompleteCount = 75;
			t.NPCNames.Add("Guard Alfric");
			t.NPCInfo = "Candeth Keep";
			t.NPCCoords = "87.9S, 67.4W";
			mKTSet.MyKillTasks.Add(t);

			t = new KillTask();
			t.TaskName = "Shallows Gorger Kill Task";
			t.MobNames.Add("Shallows Gorger");
			t.CompleteCount = 50;
			t.NPCNames.Add("Dayla Bint Kazm");
			t.NPCInfo = "The Deep (Vissidal)";
			t.NPCCoords = "77.8N 67.1E";
			mKTSet.MyKillTasks.Add(t);
			
			
			
			t = new KillTask();
			t.TaskName = "Sishalti Slithis Kill Task";
			t.MobNames.Add("Sishalti Tentacle");
			t.MobNames.Add("Sishalti Tendril");
			t.MobNames.Add("Sishalti Eye Stalk");
			t.CompleteCount = 150;
			t.NPCNames.Add("Zava bint Laurma");
			t.NPCInfo = "Zaikhal";
			t.NPCCoords = "13.9N, 0.6E";
			mKTSet.MyKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Small Fledgling Mukkir Kill Task";
			t.MobNames.Add("Small Fledgling Mukkir");
			t.CompleteCount = 15;
			t.NPCNames.Add("Royal Guard");
			t.NPCInfo = "Holtburg, Shoushi, or Yaraq";
			t.NPCCoords = "Unknown";
			mKTSet.MyKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Tenebrous Rift Kill Task";
			t.MobNames.Add("Tenebrous Rift");
			t.CompleteCount = 350;
			t.NPCNames.Add("Solange");
			t.NPCInfo = "Singularity Caul";
			t.NPCCoords = "97.4S 94.6W";
			mKTSet.MyKillTasks.Add(t);

			t = new KillTask();
			t.TaskName = "Three Eyed Snowman Kill Task";
			t.MobNames.Add("Three Eyed Snowman");
			t.CompleteCount = 5;
			t.NPCNames.Add("Blind Snowman");
			t.NPCInfo = "Mountains (north of Holtburg)";
			t.NPCCoords = "46.7N, 48.9E";
			mKTSet.MyKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Tukora Lieutenant Kill Task";
			t.MobNames.Add("Tukora Lieutenant");
			t.CompleteCount = 250;
			t.NPCNames.Add("Claire Artmad");
			t.NPCInfo = "Eastwatch";
			t.NPCCoords = "90.2N, 43.1W";
			mKTSet.MyKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Tumerok Gladiator Kill Task";
			t.MobNames.Add("Tumerok Gladiator");
			t.CompleteCount = 25;
			t.NPCNames.Add("Lieutenant Rothe");
			t.NPCInfo = "Dryreach";
			t.NPCCoords = "8.2S 73.1E";
			mKTSet.MyKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Tusker Guard Kill Task";
			t.MobNames.Add("Tusker Guard");
			t.CompleteCount = 500;
			t.NPCNames.Add("Shoichi");
			t.NPCInfo = "Lin";
			t.NPCCoords = "54.4S 72.9E";
			mKTSet.MyKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Umbral Rift Kill Task	Umbral Rifts";
			t.MobNames.Add("Umbral Rift");
			t.CompleteCount = 30;
			t.NPCNames.Add("Solange");
			t.NPCInfo = "Singularity Caul";
			t.NPCCoords = "97.4S 94.6W";
			mKTSet.MyKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Viamontian Man-at-Arms Kill Task";
			t.MobNames.Add("Viamontian Man-at-Arm");
			t.CompleteCount = 50;
			t.NPCNames.Add("Robert Gutsmasher");
			t.NPCInfo = "Rebel Hideout";
			t.NPCCoords = "43.9N 73.9W";
			mKTSet.MyKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Virindi Paradox Kill Task";
			t.MobNames.Add("Virindi Paradox");
			t.CompleteCount = 75;
			t.NPCNames.Add("Guard Taziq");
			t.NPCInfo = "Qalaba'r";
			t.NPCCoords = "74.3S, 19.1E";
			mKTSet.MyKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Virindi Quidiox Kill Task";
			t.MobNames.Add("Virindi Quidiox");
			t.CompleteCount = 75;
			t.NPCNames.Add("Guard Q'alia");
			t.NPCInfo = "Ayan Baqur";
			t.NPCCoords = "60.0S, 88.0W";
			mKTSet.MyKillTasks.Add(t);			
			
			t = new KillTask();
			t.TaskName = "Voracious Eater Kill Task";
			t.MobNames.Add("Voracious Eater");
			t.CompleteCount = 50;
			t.NPCNames.Add("Aun Ruperea");
			t.NPCInfo = "Timaru";
			t.NPCCoords = "44.3N, 77.9W";
			mKTSet.MyKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Wicked Skeleton Kill Task";
			t.MobNames.Add("Wicked Skeleton");
			t.CompleteCount = 100;
			t.NPCNames.Add("Ruqaya al Mubarak");
			t.NPCInfo = "Eastwatch";
			t.NPCCoords = "90.3N 43.1W";
			mKTSet.MyKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Crystalline Killer";
			t.MobNames.Add("Crystalline Wisps");
			t.MobNames.Add("Aggregate Crystalline Wisp");
			t.MobNames.Add("Intense Aggregate Crystalline Wisp");
			t.MobNames.Add("Corroding Pillar");
			t.MobNames.Add("Incalescent Pillar");
			t.MobNames.Add("Shivering Pillar");
			t.MobNames.Add("Voltaic Pillar");
			t.MobNames.Add("Corroding Crystalline Wisp");
			t.MobNames.Add("Incalescent Crystalline Wisp");
			t.MobNames.Add("Shivering Crystalline Wisp");
			t.MobNames.Add("Voltaic Crystalline Wisp");
			t.MobNames.Add("Intense Shivering Pillar");
			t.MobNames.Add("Intense Shivering Crystalline Wisp");
			t.MobNames.Add("Intense Incalescent Pillar");
			t.MobNames.Add("Intense Incalescent Crystalline Wisp");
			t.MobNames.Add("Intense Corroding Crystalline Wisp");	
			t.MobNames.Add("Intense Voltaic Crystalline Wisp");
			t.MobNames.Add("Progenitor of Acid");
			t.MobNames.Add("Progenitor of Fire");
			t.MobNames.Add("Progenitor of Frost");
			t.MobNames.Add("Progenitor of Lightning");
			t.MobNames.Add("Progenitor of Shadow");
			t.MobNames.Add("Spectral Progenitor");
			t.CompleteCount = 100;
			t.NPCNames.Add("Oorjit");
			t.NPCInfo = "Crystalline Crag";
			t.NPCCoords = "90.3N 43.1W";
			mKTSet.MyKillTasks.Add(t);
			
			FileInfo TaskFile = new FileInfo(GearDir + @"\Tasks.xml");
			if(TaskFile.Exists)
			{
				TaskFile.Delete();
			}
					
			XmlWriterSettings settings = new XmlWriterSettings();
			settings.Indent = true;
			settings.NewLineOnAttributes = true;
			
			XmlWriter writer = XmlWriter.Create(TaskFile.ToString(), settings);
			
   			XmlSerializer serializer2 = new XmlSerializer(typeof(List<KillTask>));
   			serializer2.Serialize(writer, mKTSet.MyKillTasks);
   			writer.Close();
			
			
			
		}
		
		
	}
}
