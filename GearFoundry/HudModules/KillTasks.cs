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
			public int HudWidth = 300;
			public int HudHeight = 125;
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
			public bool detect = false;
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
			public List<string> NPCNames = new List<string>();
			public string NPCInfo = String.Empty;
			public string NPCCoords = String.Empty;
			public string NPCYellowFlagText = String.Empty;
			public string NPCYellowCompleteText = String.Empty;
			public bool track = false;
			public bool detect = false;
			public bool active = false;
			public bool complete = false;
		}
		
		private void SubscribeKillTasks()
		{
			ReadWriteGearTaskSettings(true);
			
			List<KillTask> KTL = ReadMasterKTList();
			List<KillTask> KTAdds = KTL.Where(x => !mKTSet.MyKillTasks.Any(y => y.TaskName == x.TaskName)).ToList();
			List<KillTask> KTRemoves = mKTSet.MyKillTasks.Where(x => !KTL.Any(y => y.TaskName == x.TaskName)).ToList();
			mKTSet.MyKillTasks.RemoveAll(x => KTRemoves.Any(y => y.TaskName == x.TaskName));
			mKTSet.MyKillTasks = mKTSet.MyKillTasks.Union(KTAdds).ToList();
			KTL = null;
			KTAdds = null;
			KTRemoves = null;
			
			List<CollectTask> CTL = ReadMasterCTList();
			List<CollectTask> CTAdds = CTL.Where(x => !mKTSet.MyCollectTasks.Any(y => y.TaskName == x.TaskName)).ToList();
			List<CollectTask> CTRemoves = mKTSet.MyCollectTasks.Where(x => !CTL.Any(y => y.TaskName == x.TaskName)).ToList();
			mKTSet.MyCollectTasks.RemoveAll(x => CTRemoves.Any(y => y.TaskName == x.TaskName));
			mKTSet.MyCollectTasks = mKTSet.MyCollectTasks.Union(CTAdds).ToList();
			CTL = null;
			CTAdds = null;
			CTRemoves = null;
				
			Globals.Core.ChatBoxMessage += KillTask_ChatBoxMessage;
			Globals.Core.CharacterFilter.Logoff += KillTask_LogOff;
			Globals.Core.WorldFilter.ChangeObject += CollectTask_ChangeObject;
			KTSaveTimer.Interval = 600000;
			KTSaveTimer.Start();
			KTSaveTimer.Tick += KTSaveUpdates;
			
			try
			{	
				foreach(CollectTask coltsk in mKTSet.MyCollectTasks)
				{
					if(GearFoundry.Globals.Core.WorldFilter.GetInventory().Any(x => @x.Name == @coltsk.Item))
					{
						List<WorldObject> inventory = Core.WorldFilter.GetInventory().Where(x => @x.Name == @coltsk.Item).ToList();
						int colcount = 0;
						foreach(WorldObject item in inventory)
						{
							colcount += item.Values(LongValueKey.StackCount);
						}
						coltsk.CurrentCount = colcount;
						if(coltsk.CurrentCount >= coltsk.CompleteCount)
						{
							coltsk.complete = true;
						}
					}
					
					
				}
			}catch(Exception ex){LogError(ex);}
			
			ReadWriteGearTaskSettings(false);
			RenderKillTaskPanel();
			
			//BuildCollectionTaskList();
		}
		
		private void KTSaveUpdates(object sender, EventArgs e)
		{
			ReadWriteGearTaskSettings(false);
		}
		
		private void KillTask_LogOff(object sender, EventArgs e)
		{
			UnsubscribeKillTasks();
		}
		
		private List<KillTask> ReadMasterKTList()
		{
			try
			{
				FileInfo KillTasksFile = new FileInfo(GearDir + @"\KillTasks.xml");
				XmlReaderSettings rsettings = new XmlReaderSettings();
			    rsettings.IgnoreWhitespace = true;
			    List<KillTask> MasterKTList = new List<KillTask>();
			    
				using (XmlReader reader = XmlReader.Create(@KillTasksFile.ToString(), rsettings))
				{	
					XmlSerializer kts = new XmlSerializer(typeof(List<KillTask>));
					MasterKTList = (List<KillTask>)kts.Deserialize(reader);
					reader.Close();
			    }
				
				return MasterKTList;
				
			}catch(Exception ex){LogError(ex); return new List<KillTask>();}
		}
		
		private void WriteMasterKTList(List<KillTask> KTL)
		{
			try
			{
				FileInfo KillTasksFile = new FileInfo(GearDir + @"\KillTasks.xml");
			
				XmlWriterSettings wsettings = new XmlWriterSettings();
				wsettings.Indent = true;
				wsettings.NewLineOnAttributes = true;
				
				using(XmlWriter writer = XmlWriter.Create(@KillTasksFile.ToString(), wsettings))
				{
					XmlSerializer serializer2 = new XmlSerializer(typeof(List<KillTask>));
					serializer2.Serialize(writer, KTL);
					writer.Close();
					      	
				}	
							
			}catch(Exception ex){LogError(ex);}
		}
				
		private List<CollectTask> ReadMasterCTList()
		{
			try
			{
				FileInfo CollectTasksFile = new FileInfo(GearDir + @"\CollectTasks.xml");
				XmlReaderSettings rsettings = new XmlReaderSettings();
			    rsettings.IgnoreWhitespace = true;
			    List<CollectTask> MasterCTList = new List<CollectTask>();

				using (XmlReader reader = XmlReader.Create(@CollectTasksFile.ToString(), rsettings))
				{	
					XmlSerializer CTs = new XmlSerializer(typeof(List<CollectTask>));
					MasterCTList = (List<CollectTask>)CTs.Deserialize(reader);
					reader.Close();
			    }
				
				return MasterCTList;		
				
			}catch(Exception ex){LogError(ex); return new List<CollectTask>();}
		}
		
		private void WriteMasterCTList(List<CollectTask> CTL)
		{
			try
			{
				FileInfo CollectTasksFile = new FileInfo(GearDir + @"\CollectTasks.xml");
				XmlWriterSettings wsettings = new XmlWriterSettings();
				wsettings.Indent = true;
				wsettings.NewLineOnAttributes = true;
				
				using(XmlWriter writer = XmlWriter.Create(@CollectTasksFile.ToString(), wsettings))
				{
					XmlSerializer serializer2 = new XmlSerializer(typeof(List<CollectTask>));
					serializer2.Serialize(writer, CTL);
					writer.Close();
					      	
				}
				
			}catch(Exception ex){LogError(ex);}
		}
			
		private void CollectTask_ChangeObject(object sender, ChangeObjectEventArgs e)
		{
			try
			{
				if(e.Change != WorldChangeType.StorageChange && e.Change != WorldChangeType.SizeChange) {return;}
				int ChangeIndex = mKTSet.MyCollectTasks.FindIndex(x => x.Item == e.Changed.Name);
				if(ChangeIndex > -1)
				{
					List<WorldObject> inventory = Core.WorldFilter.GetInventory().Where(x => @x.Name == @mKTSet.MyCollectTasks[ChangeIndex].Item).ToList();
					int colcount = 0;
					foreach(WorldObject item in inventory)
					{
						colcount += item.Values(LongValueKey.StackCount);
					}
					mKTSet.MyCollectTasks[ChangeIndex].CurrentCount = colcount;	
					
					if(mKTSet.MyCollectTasks[ChangeIndex].CurrentCount >= mKTSet.MyCollectTasks[ChangeIndex].CompleteCount)
					{
						mKTSet.MyCollectTasks[ChangeIndex].complete = true;
					}
					
				}
				
				UpdateTaskPanel();
				
				
			}catch(Exception ex){LogError(ex);}
		}
		
		private void UnsubscribeKillTasks()
		{
			Core.ChatBoxMessage -= KillTask_ChatBoxMessage;
			Core.CharacterFilter.Logoff -= KillTask_LogOff;
			Core.WorldFilter.ChangeObject -= CollectTask_ChangeObject;
						
			KTSaveTimer.Tick -= KTSaveUpdates;
			KTSaveTimer.Stop();
			
			DisposeKillTaskPanel();
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
                  	
                    FileInfo KillTasksFile = new FileInfo(GearDir + @"\KillTasks.xml");
                    FileInfo CollectTasksFile = new FileInfo(GearDir + @"\CollectTasks.xml");
                    	
                	//Makes the master KillTask file
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
						
					//Makes the master Collect Task file
					try
					{
                    	if(!CollectTasksFile.Exists)
                    	{
	                    	string filedefaults = GetResourceTextFile("CollectTasks.xml");
	                    	using (StreamWriter writedefaults = new StreamWriter(@CollectTasksFile.ToString(), true))
							{
								writedefaults.Write(filedefaults);
								writedefaults.Close();
							}
                    	}
                	}catch(Exception ex){LogError(ex);}
						
					try
					{
						
						if(GearTaskSettingsFile.Exists)
						{
							using (XmlReader reader = XmlReader.Create(GearTaskSettingsFile.ToString(), rsettings))
							{	
								XmlSerializer serializer = new XmlSerializer(typeof(KillTaskSettings));
								mKTSet = (KillTaskSettings)serializer.Deserialize(reader);
								reader.Close();
							}
						}
						else
						{
	                        using (XmlReader reader = XmlReader.Create(@KillTasksFile.ToString(), rsettings))
							{	
								XmlSerializer kts = new XmlSerializer(typeof(List<KillTask>));
								mKTSet.MyKillTasks = (List<KillTask>)kts.Deserialize(reader);
								reader.Close();
			                }
			                
			                using (XmlReader reader = XmlReader.Create(CollectTasksFile.ToString()))
							{	
								XmlSerializer cts = new XmlSerializer(typeof(List<CollectTask>));
								mKTSet.MyCollectTasks = (List<CollectTask>)cts.Deserialize(reader);
								reader.Close();
			                }
			                    			                
	                    	using (XmlWriter writer = XmlWriter.Create(GearTaskSettingsFile.ToString(), wsettings))
							{
					   			XmlSerializer serializer2 = new XmlSerializer(typeof(KillTaskSettings));
					   			serializer2.Serialize(writer, mKTSet);
					   			writer.Close();
							}
						}
					}catch(Exception ex){LogError(ex);}
					
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
					
					if(!@e.Text.StartsWith("You have killed")){return;}
					
					int mobskilled = 0;
					int totalmobs = 0;
					string mobname = String.Empty;
					bool taskcomplete = false;
					
					nibble = e.Text.Remove(0, 16);
					Int32.TryParse(nibble.Substring(0, nibble.IndexOf(' ')), out mobskilled);
					
					nibble = nibble.Remove(0, nibble.IndexOf(' '));
					mobname = (@nibble.Substring(0, nibble.IndexOf('!'))).Trim();
					
					if(@mobname.EndsWith("ies"))
					{
						@mobname = @mobname.Replace("ies","y").Trim();
					}
					else if(@mobname.EndsWith("xes"))
					{
						@mobname = @mobname.Remove(mobname.Length - 2, 2).Trim();
					}
					else if(@mobname.EndsWith("s"))
					{
						@mobname = @mobname.Remove(mobname.Length -1, 1).Trim();
					}
					else if(mobname.EndsWith("men"))
					{
						@mobname = @mobname.Replace("men","man");
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
					
					if(TaskIndex == -1)
					{
						WriteKillTaskFailureToFile(mobname, mobskilled, totalmobs);
						WriteToChat("Caught an untrackable killtask.");
						WriteToChat("You Killed " + mobname + " and need to kill " + totalmobs);
						WriteToChat("Results saved to file for future inclusion in kill task tracker.");
						return;
					}
					
					mKTSet.MyKillTasks[TaskIndex].CurrentCount = mobskilled;
					mKTSet.MyKillTasks[TaskIndex].complete = taskcomplete;
					mKTSet.MyKillTasks[TaskIndex].active = true;
					
					UpdateTaskPanel();
					
					e.Eat = true;
				}
				if(e.Color == 3)
				{   
					int CollectTasksCount = mKTSet.MyCollectTasks.Count(x => x.NPCNames.Any(y => @e.Text.StartsWith(@y)));
					int KillTasksCount = mKTSet.MyKillTasks.Count(x => x.NPCNames.Any(y => @e.Text.StartsWith(@y)));

					if (CollectTasksCount > 0)
					{
						int CollectTaskIndex = -1;
						
						if(CollectTasksCount == 1)
						{
							CollectTaskIndex = 	mKTSet.MyCollectTasks.FindIndex(x => x.NPCNames.Any(y => @e.Text.StartsWith(@y)));
						}
						else
						{
							CollectTaskIndex = mKTSet.MyCollectTasks.FindIndex(x => x.NPCNames.Any(y => @e.Text.StartsWith(@y)) &&
							                     	(@e.Text.Contains(@x.NPCYellowFlagText) || @e.Text.Contains(@x.NPCYellowCompleteText)));
						}
						
						if(CollectTaskIndex == -1) {return;}
						
						bool flag = @e.Text.Contains(@mKTSet.MyCollectTasks[CollectTaskIndex].NPCYellowFlagText);
						bool complete = @e.Text.Contains(@mKTSet.MyCollectTasks[CollectTaskIndex].NPCYellowCompleteText);
								
						if(flag)
						{
							mKTSet.MyCollectTasks[CollectTaskIndex].active = true;
							mKTSet.MyCollectTasks[CollectTaskIndex].complete = false;
							return;
						}
						if(complete)
						{
							mKTSet.MyCollectTasks[CollectTaskIndex].active = false;
							mKTSet.MyCollectTasks[CollectTaskIndex].complete = false;
							return;
						}
					}
					
					if (KillTasksCount > 0)
					{
						int KillTaskIndex = -1;
						
						if(CollectTasksCount == 1)
						{
							KillTaskIndex = mKTSet.MyKillTasks.FindIndex(x => x.NPCNames.Any(y => @e.Text.StartsWith(@y)));
						}
						else
						{
							KillTaskIndex = mKTSet.MyKillTasks.FindIndex(x => x.NPCNames.Any(y => @e.Text.StartsWith(@y)) &&
							                     	(@e.Text.Contains(@x.NPCYellowFlagText) || @e.Text.Contains(@x.NPCYellowCompleteText)));
						}
						
						if(KillTaskIndex == -1) {return;}
						
						bool flag = @e.Text.Contains(@mKTSet.MyKillTasks[KillTaskIndex].NPCYellowFlagText);
						bool complete = @e.Text.Contains(@mKTSet.MyKillTasks[KillTaskIndex].NPCYellowCompleteText);
								
						if(flag)
						{
							mKTSet.MyKillTasks[KillTaskIndex].active = true;
							mKTSet.MyKillTasks[KillTaskIndex].complete = false;
							return;
						}
						if(complete)
						{
							mKTSet.MyKillTasks[KillTaskIndex].active = false;
							mKTSet.MyKillTasks[KillTaskIndex].complete = false;
							return;
						}
					}
					UpdateTaskPanel();
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
		private HudStaticText KillTaskSelected = null;
		private HudButton KillTaskNew = null;
		private HudButton KillTaskDelete = null;
		private HudButton KillTaskEdit = null;
		private HudList CollectTaskList = null;
		private HudList.HudListRowAccessor TaskListRow = null;
		private HudStaticText KTPanelLabel1 = null;
		private HudStaticText KTPanelLabel2 = null;
		private HudStaticText IncTaskLabel1 = null;
		private HudStaticText IncTaskLabel2 = null;
		private HudStaticText CollectTaskSelected = null;
		private HudStaticText CompTaskLabel1 = null;
		private HudStaticText CompTaskLabel2 = null;
		private HudButton CollectTaskNew = null;
		private HudButton CollectTaskEdit = null;
		private HudButton CollectTaskDelete = null;
		private HudStaticText CTPanelLabel1 = null;
		private HudStaticText CTPanelLabel2 = null;
		
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
	            
	            IncTaskLabel1 = new HudStaticText();
	            TaskIncompleteLayout.AddControl(IncTaskLabel1, new Rectangle(0,0,60,16));
	            IncTaskLabel1.Text = "Task Name";
	            
	            IncTaskLabel2 = new HudStaticText();
	            TaskIncompleteLayout.AddControl(IncTaskLabel2, new Rectangle(Convert.ToInt32(mKTSet.HudWidth - mKTSet.HudWidth/3), 0,Convert.ToInt32(mKTSet.HudWidth/3),16));
	            IncTaskLabel2.Text = "Status";
				
	            TaskIncompleteList = new HudList();
	            TaskIncompleteLayout.AddControl(TaskIncompleteList, new Rectangle(0,20,mKTSet.HudWidth,mKTSet.HudHeight -20));
	            TaskIncompleteList.ControlHeight = 16;
	            TaskIncompleteList.AddColumn(typeof(HudStaticText), Convert.ToInt32(mKTSet.HudWidth*2/3), null);  //Mob/Item Name
	            TaskIncompleteList.AddColumn(typeof(HudStaticText), Convert.ToInt32(mKTSet.HudWidth/3 + 5), null);  //Completion
	            
	            VirindiViewService.TooltipSystem.AssociateTooltip(TaskIncompleteList, "Click for task completion info."); 
	            
	            TaskIncompleteList.Click += TaskIncompleteList_Click;
	            
	            TaskCompleteLayout = new HudFixedLayout();
	            TaskTabView.AddTab(TaskCompleteLayout, "Complete");
	            TaskCompleteList = new HudList();
	            
	            CompTaskLabel1 = new HudStaticText();
	            TaskCompleteLayout.AddControl(CompTaskLabel1, new Rectangle(0,0,60,16));
	            CompTaskLabel1.Text = "Task Name";
	            
	            CompTaskLabel2 = new HudStaticText();
	            TaskCompleteLayout.AddControl(CompTaskLabel2, new Rectangle(Convert.ToInt32(mKTSet.HudWidth*2/3), 0,Convert.ToInt32(mKTSet.HudWidth/3),16));
	            CompTaskLabel2.Text = "Return";
	            
	            TaskCompleteLayout.AddControl(TaskCompleteList, new Rectangle(0,20,mKTSet.HudWidth,mKTSet.HudHeight -20));
	            TaskCompleteList.ControlHeight = 16;
	            TaskCompleteList.AddColumn(typeof(HudStaticText), Convert.ToInt32(mKTSet.HudWidth*2/3), null);  //Mob/Item Name
	            TaskCompleteList.AddColumn(typeof(HudStaticText), Convert.ToInt32(mKTSet.HudWidth/3 + 5), null);  //Completion
	            
	            VirindiViewService.TooltipSystem.AssociateTooltip(TaskCompleteList, "Click for turn in info."); 
	            
	            TaskCompleteList.Click += TaskCompleteList_Click;
	            
	            KillTaskLayout = new HudFixedLayout();
	            TaskTabView.AddTab(KillTaskLayout, "Kill");
	            
	            KillTaskSelected = new HudStaticText();
	            KillTaskLayout.AddControl(KillTaskSelected, new Rectangle(0,0, TaskHudView.Width - 110, 16));
	            KillTaskSelected.Text = String.Empty;
	            
	            KillTaskNew = new HudButton();
	            KillTaskLayout.AddControl(KillTaskNew, new Rectangle(TaskHudView.Width - 105, 0, 30, 16));
	            KillTaskNew.Text = "New";
	            KillTaskNew.Hit += KillTaskNew_Hit;
	            
	            KillTaskDelete = new HudButton();
	            KillTaskLayout.AddControl(KillTaskDelete, new Rectangle(TaskHudView.Width - 70, 0, 30, 16));
	            KillTaskDelete.Text = "Del";
	            KillTaskDelete.Hit += KillTaskDelete_Hit; 
	            
	            KillTaskEdit = new HudButton();
	            KillTaskLayout.AddControl(KillTaskEdit, new Rectangle(TaskHudView.Width - 35, 0, 30, 16));
	            KillTaskEdit.Text = "Edit";
	            KillTaskEdit.Hit += KillTaskEdit_Hit;            
	            
	            KTPanelLabel1 = new HudStaticText();
	            KillTaskLayout.AddControl(KTPanelLabel1, new Rectangle(0,20,50,16));
	            KTPanelLabel1.Text = "Track";
	            
	            KTPanelLabel2 = new HudStaticText();
	            KillTaskLayout.AddControl(KTPanelLabel2, new Rectangle(40,20,100,16));
	            KTPanelLabel2.Text = "Task Name";
	            
	            KillTaskList = new HudList();
	            KillTaskLayout.AddControl(KillTaskList, new Rectangle(0,40,mKTSet.HudWidth,mKTSet.HudHeight-20));
	            KillTaskList.ControlHeight = 16;
	            KillTaskList.AddColumn(typeof(HudCheckBox), 16, null);  //Track
	            KillTaskList.AddColumn(typeof(HudStaticText), Convert.ToInt32(mKTSet.HudWidth - 16), null);  //TaskName
	            
	            VirindiViewService.TooltipSystem.AssociateTooltip(KillTaskList, "Enable Tracking or Click for info."); 
	            
	            KillTaskList.Click += KillTaskList_Click;
	            
	            CollectTaskLayout = new HudFixedLayout();
	            TaskTabView.AddTab(CollectTaskLayout, "Collect");
	            
	            CollectTaskSelected = new HudStaticText();
	            CollectTaskLayout.AddControl(CollectTaskSelected, new Rectangle(0,0, TaskHudView.Width - 110, 16));
	            CollectTaskSelected.Text = String.Empty;
	            
	            CollectTaskNew = new HudButton();
	            CollectTaskLayout.AddControl(CollectTaskNew, new Rectangle(TaskHudView.Width - 105, 0, 30, 16));
	            CollectTaskNew.Text = "New";
	            CollectTaskNew.Hit += CollectTaskNew_Hit;
	            
	            CollectTaskDelete = new HudButton();
	            CollectTaskLayout.AddControl(CollectTaskDelete, new Rectangle(TaskHudView.Width - 70, 0, 30, 16));
	            CollectTaskDelete.Text = "Del";
	            CollectTaskDelete.Hit += CollectTaskDelete_Hit; 
	            
	            CollectTaskEdit = new HudButton();
	            CollectTaskLayout.AddControl(CollectTaskEdit, new Rectangle(TaskHudView.Width - 35, 0, 30, 16));
	            CollectTaskEdit.Text = "Edit";
	            CollectTaskEdit.Hit += CollectTaskEdit_Hit;            
	            
	            CTPanelLabel1 = new HudStaticText();
	            CollectTaskLayout.AddControl(CTPanelLabel1, new Rectangle(0,20,50,16));
	            CTPanelLabel1.Text = "Track";
	            
	            CTPanelLabel2 = new HudStaticText();
	            CollectTaskLayout.AddControl(CTPanelLabel2, new Rectangle(40,20,100,16));
	            CTPanelLabel2.Text = "Task Name";
	            
	            CollectTaskList = new HudList();
	            CollectTaskLayout.AddControl(CollectTaskList, new Rectangle(0,40,mKTSet.HudWidth,mKTSet.HudHeight));
	            CollectTaskList.ControlHeight = 16;
	            CollectTaskList.AddColumn(typeof(HudCheckBox), 16, null);  //Track
	            CollectTaskList.AddColumn(typeof(HudStaticText), Convert.ToInt32(mKTSet.HudWidth - 16), null);  //TaskName
	            
	            VirindiViewService.TooltipSystem.AssociateTooltip(CollectTaskList, "Enable Tracking or Click for info."); 
	            
	            CollectTaskList.Click += CollectTaskList_Click;	            
	            TaskHudView.Resize += TaskHudView_Resize;
	    		
	            UpdateTaskPanel();
	            			
			}catch(Exception ex){LogError(ex);}
		}
		
		private void DisposeKillTaskPanel()
		{
			try
			{
				TaskIncompleteList.Click -= TaskIncompleteList_Click;
				TaskCompleteList.Click -= TaskCompleteList_Click;
				KillTaskList.Click -= KillTaskList_Click;
				KillTaskNew.Hit -= KillTaskNew_Hit;
				KillTaskDelete.Hit -= KillTaskDelete_Hit; 
				KillTaskEdit.Hit -= KillTaskEdit_Hit;
				CollectTaskNew.Hit -= CollectTaskNew_Hit;
				CollectTaskDelete.Hit -= CollectTaskDelete_Hit;
				CollectTaskEdit.Hit -= CollectTaskEdit_Hit;  	
 				CollectTaskList.Click -= CollectTaskList_Click;	            
	            TaskHudView.Resize -= TaskHudView_Resize;
				
				TaskHudView.Dispose();
				TaskTabView.Dispose();
				TaskIncompleteLayout.Dispose();	            
				IncTaskLabel1.Dispose();           
				IncTaskLabel2.Dispose(); 				
				TaskIncompleteList.Dispose(); 	              	                        
	            TaskCompleteLayout.Dispose();             
	            CompTaskLabel1.Dispose();	            
	            CompTaskLabel2.Dispose(); 	 	            
	            KillTaskLayout.Dispose();	            
	            KillTaskSelected.Dispose();	            
	            KillTaskNew.Dispose(); 	            
	            KillTaskDelete.Dispose();   
	            KillTaskEdit.Dispose(); 
	            KTPanelLabel1.Dispose();  
	            KTPanelLabel2.Dispose(); 
	            KillTaskList.Dispose(); 
	            CollectTaskLayout.Dispose();  
	            CollectTaskSelected.Dispose();
	            CollectTaskNew.Dispose();
	            CollectTaskDelete.Dispose();
	            CollectTaskEdit.Dispose();
	            CTPanelLabel1.Dispose(); 
	            CTPanelLabel2.Dispose();
	            CollectTaskList.Dispose();
	            
	            TaskHudView = null;
	            
			}catch(Exception ex){LogError(ex);}
			
		}
		
		private void TaskHudView_Resize(object sender, EventArgs e)
		{
			try
			{
				mKTSet.HudHeight = TaskHudView.Height;
				mKTSet.HudWidth = TaskHudView.Width;
				
				ReadWriteGearTaskSettings(false);
				
			}catch(Exception ex){LogError(ex);}
		}
		
		
		private void UpdateTaskPanel()
		{
			try
			{
				TaskIncompleteList.ClearRows();
				TaskCompleteList.ClearRows();
				KillTaskList.ClearRows();
				CollectTaskList.ClearRows();
				
				foreach(var ict in mKTSet.MyKillTasks.Where(x => x.track && x.active && x.complete == false).OrderBy(x => x.TaskName))
				{
					TaskListRow = TaskIncompleteList.AddRow();
					
					((HudStaticText)TaskListRow[0]).Text = ict.TaskName;
					((HudStaticText)TaskListRow[0]).TextColor = Color.Orange;
					((HudStaticText)TaskListRow[1]).Text = "(" + ict.CurrentCount.ToString() + "/" + ict.CompleteCount.ToString() + ")";
					((HudStaticText)TaskListRow[1]).TextColor = Color.Orange;					
				}
				
				foreach(var ict in mKTSet.MyCollectTasks.Where(x => x.track && x.complete == false).OrderBy(x => x.TaskName))
				{
					TaskListRow = TaskIncompleteList.AddRow();
					((HudStaticText)TaskListRow[0]).Text = ict.TaskName;
					((HudStaticText)TaskListRow[0]).TextColor = Color.Tan;
					((HudStaticText)TaskListRow[1]).Text = "(" + ict.CurrentCount.ToString() + "/" + ict.CompleteCount.ToString() + ")";	
					((HudStaticText)TaskListRow[1]).TextColor = Color.Tan;
				}
				
				foreach(var ct in mKTSet.MyKillTasks.Where(x => x.track && x.complete == true).OrderBy(x => x.TaskName))
				{
					TaskListRow = TaskCompleteList.AddRow();
					((HudStaticText)TaskListRow[0]).Text = ct.TaskName;
					((HudStaticText)TaskListRow[1]).Text = ct.NPCInfo;
				}
				
				foreach(var ct in mKTSet.MyCollectTasks.Where(x => x.track && x.complete == true).OrderBy(x => x.TaskName))
				{
					TaskListRow = TaskCompleteList.AddRow();
					((HudStaticText)TaskListRow[0]).Text = ct.TaskName;
					((HudStaticText)TaskListRow[1]).Text = ct.NPCInfo;	
				}			
				
				foreach(var kt in mKTSet.MyKillTasks.OrderBy(x => x.TaskName))
				{
					TaskListRow = KillTaskList.AddRow();
					((HudCheckBox)TaskListRow[0]).Checked = kt.track;
					((HudStaticText)TaskListRow[1]).Text = kt.TaskName;	
					((HudStaticText)TaskListRow[1]).TextColor = Color.AntiqueWhite;				
					if(kt.complete) {((HudStaticText)TaskListRow[1]).TextColor = Color.Gold;}
					else if(kt.active) {((HudStaticText)TaskListRow[1]).TextColor = Color.LightSeaGreen;}
				}
	
				foreach(var ct in mKTSet.MyCollectTasks.OrderBy(x => x.TaskName))
				{
					TaskListRow = CollectTaskList.AddRow();
					((HudCheckBox)TaskListRow[0]).Checked = ct.track;
					((HudStaticText)TaskListRow[1]).Text = ct.TaskName;	
					((HudStaticText)TaskListRow[1]).TextColor = Color.AntiqueWhite;				
					if(ct.complete) {((HudStaticText)TaskListRow[1]).TextColor = Color.Gold;}
					else if(ct.active) {((HudStaticText)TaskListRow[1]).TextColor = Color.LightSeaGreen;}
				}
				
			}catch(Exception ex){LogError(ex);}
		}
		
		
		HudList.HudListRowAccessor ClickRow = new HudList.HudListRowAccessor();
		private void KillTaskList_Click(object sender, int row, int col)
		{
			try
			{
				int scroll = KillTaskList.ScrollPosition;
				
				ClickRow = KillTaskList[row];
				KillTaskSelected.Text = ((HudStaticText)ClickRow[1]).Text;
				KillTaskSelected.TextColor = Color.Red;
				
				if(col == 0)
				{
					mKTSet.MyKillTasks.Find(x => x.TaskName == ((HudStaticText)ClickRow[1]).Text).track = ((HudCheckBox)ClickRow[0]).Checked;
				}
				if(col == 1)
				{
					KillTask kt = mKTSet.MyKillTasks.Find(x => x.TaskName == ((HudStaticText)ClickRow[1]).Text);
					string NPCs = String.Empty;
					foreach(string npc in kt.NPCNames)
					{
						NPCs += npc + ", ";
					}
					WriteToChat(kt.TaskName + ":  " + NPCs + kt.NPCInfo + CoordsStringLink(kt.NPCCoords));
				}
				
				UpdateTaskPanel();
				
				KillTaskList.ScrollPosition = scroll;
			}catch(Exception ex){LogError(ex);}
		}
		
		private void CollectTaskList_Click(object sender, int row, int col)
		{
			try
			{
				
				int scroll = CollectTaskList.ScrollPosition;
				
				ClickRow = CollectTaskList[row];
				CollectTaskSelected.Text = ((HudStaticText)ClickRow[1]).Text;
				CollectTaskSelected.TextColor = Color.Red;
				
				if(col == 0)
				{
					mKTSet.MyCollectTasks.Find(x => x.TaskName == ((HudStaticText)ClickRow[1]).Text).track = ((HudCheckBox)ClickRow[0]).Checked;
				}
				if(col == 1)
				{
					CollectTask ct = mKTSet.MyCollectTasks.Find(x => x.TaskName == ((HudStaticText)ClickRow[1]).Text);
					string NPCs = String.Empty;
					foreach(string npc in ct.NPCNames)
					{
						NPCs += npc + ", ";
					}
					
					WriteToChat(ct.TaskName + ":  " + NPCs + ct.NPCInfo + CoordsStringLink(ct.NPCCoords));	
				}
				UpdateTaskPanel();
				
				CollectTaskList.ScrollPosition = scroll;
				
			}catch(Exception ex){LogError(ex);}
		}
		
		private void TaskIncompleteList_Click(object sender, int row, int col)
		{
			try
			{
				int scroll = TaskIncompleteList.ScrollPosition;
				
				ClickRow = TaskIncompleteList[row];
				
				int ctindex = mKTSet.MyCollectTasks.FindIndex(x => x.TaskName == ((HudStaticText)ClickRow[0]).Text);			
				int ktindex = mKTSet.MyKillTasks.FindIndex(x => x.TaskName == ((HudStaticText)ClickRow[0]).Text);
				
				if(ctindex > -1)
				{
					WriteToChat(mKTSet.MyCollectTasks[ctindex].TaskName + " (" + mKTSet.MyCollectTasks[ctindex].CurrentCount.ToString() + "/" + mKTSet.MyCollectTasks[ctindex].CompleteCount.ToString() +")");
					WriteToChat("Drops from the following creature types:");
					foreach(string creature in mKTSet.MyCollectTasks[ctindex].MobNames)
					{
						WriteToChat(creature);
					}
				}
				
				if(ktindex > -1)
				{
					WriteToChat(mKTSet.MyKillTasks[ktindex].TaskName + " (" + mKTSet.MyKillTasks[ktindex].CurrentCount.ToString() + "/" + mKTSet.MyKillTasks[ktindex].CompleteCount.ToString() +")");
					WriteToChat("Kill any of the following creature types:");
					foreach(string creature in mKTSet.MyKillTasks[ktindex].MobNames)
					{
						WriteToChat(creature);
					}
				}
				
				UpdateTaskPanel();
				
				TaskIncompleteList.ScrollPosition = scroll;
			}catch(Exception ex){LogError(ex);}
		}
		
		private void TaskCompleteList_Click(object sender, int row, int col)
		{
			try
			{
				int scroll = TaskCompleteList.ScrollPosition;
				
				ClickRow = TaskCompleteList[row];
				
				int ctindex = mKTSet.MyCollectTasks.FindIndex(x => x.TaskName == ((HudStaticText)ClickRow[0]).Text);			
				int ktindex = mKTSet.MyKillTasks.FindIndex(x => x.TaskName == ((HudStaticText)ClickRow[0]).Text);
				
				if(ctindex > -1)
				{
					string NPCs = String.Empty;
					foreach(string name in mKTSet.MyCollectTasks[ctindex].NPCNames)
					{
						NPCs += ", " + name;
					}
					WriteToChat(mKTSet.MyCollectTasks[ctindex].TaskName + NPCs + ", " + mKTSet.MyCollectTasks[ctindex].NPCInfo + CoordsStringLink(mKTSet.MyCollectTasks[ctindex].NPCCoords));
				}
				
				if(ktindex > -1)
				{
					string NPCs = String.Empty;
					foreach(string name in mKTSet.MyKillTasks[ktindex].NPCNames)
					{
						NPCs += ", " + name;
					}
					WriteToChat(mKTSet.MyKillTasks[ktindex].TaskName + NPCs + ", " + mKTSet.MyKillTasks[ktindex].NPCInfo + CoordsStringLink(mKTSet.MyKillTasks[ktindex].NPCCoords));
					
				}
				
				UpdateTaskPanel();
				
				TaskCompleteList.ScrollPosition = scroll;
				
			}catch(Exception ex){LogError(ex);}
		}
		
		private void KillTaskNew_Hit(object sender, EventArgs e)
		{
			try
			{				
				KillTask NewKillTask = new KillTask();
				NewKillTask.TaskName = "NewTask " + DateTime.Now;
				KTHolder = NewKillTask;
				OldKTName = NewKillTask.TaskName;				
				RenderKillTaskPopUp();			
			}catch(Exception ex){LogError(ex);}
		}
		
		private void KillTaskEdit_Hit(object sender, EventArgs e)
		{
			try
			{
				KTHolder = mKTSet.MyKillTasks.Find(x => x.TaskName == KillTaskSelected.Text);
				OldKTName = KTHolder.TaskName;				
				RenderKillTaskPopUp();
			}catch(Exception ex){LogError(ex);}
		}
		
		private void KillTaskDelete_Hit(object sender, EventArgs e)
		{
			try
			{
				int scroll = KillTaskList.ScrollPosition;
				
			    List<KillTask> MasterKTList = ReadMasterKTList();
				if(MasterKTList.Any(x => x.TaskName == KillTaskSelected.Text))
				{
					MasterKTList.RemoveAll(x => x.TaskName == KillTaskSelected.Text);
				}
				
				WriteMasterKTList(MasterKTList);
					
				mKTSet.MyKillTasks.RemoveAll(x => x.TaskName == KillTaskSelected.Text);
				ReadWriteGearTaskSettings(false);
				
				KillTaskSelected.Text = null;
				OldKTName = String.Empty;
				KTHolder = null;
					
				UpdateTaskPanel();
				
				KillTaskList.ScrollPosition = scroll;
						
			}catch(Exception ex){LogError(ex);}
		}

		private void CollectTaskNew_Hit(object sender, EventArgs e)
		{
			try
			{	
				CollectTask NewCollectTask = new CollectTask();

				NewCollectTask.TaskName = "NewTask " + DateTime.Now;
				CTHolder = NewCollectTask;
				OldCTName = NewCollectTask.TaskName;				
				RenderCollectTaskPopUp();			
			}catch(Exception ex){LogError(ex);}
		}
		
		private void CollectTaskEdit_Hit(object sender, EventArgs e)
		{
			try
			{
				CTHolder = mKTSet.MyCollectTasks.Find(x => x.TaskName == CollectTaskSelected.Text);
				OldCTName = CTHolder.TaskName;				
				RenderCollectTaskPopUp();
			}catch(Exception ex){LogError(ex);}
		}
		
		private void CollectTaskDelete_Hit(object sender, EventArgs e)
		{
			try
			{
				int scroll = CollectTaskList.ScrollPosition;
				
				List<CollectTask> MasterCTList = ReadMasterCTList();
				if(MasterCTList.Any(x => x.TaskName == CollectTaskSelected.Text))
				{
					MasterCTList.RemoveAll(x => x.TaskName == CollectTaskSelected.Text);
				}
				
				WriteMasterCTList(MasterCTList);
			
				mKTSet.MyCollectTasks.RemoveAll(x => x.TaskName == CollectTaskSelected.Text);				
				ReadWriteGearTaskSettings(false);
								
				CollectTaskSelected.Text = null;
				OldCTName = String.Empty;
				CTHolder = null;
				
				UpdateTaskPanel();
				
				CollectTaskList.ScrollPosition = scroll;
				
			}catch(Exception ex){LogError(ex);}
		}
		
		private HudView KTPopView = null;
		private HudTabView KTPopTabView = null;
		private HudFixedLayout KTPopLayout = null;
		private HudStaticText KTLabel1 = null;
		private HudTextBox KTPopTaskName = null;
		private HudStaticText KTLabel2 = null;
		private HudTextBox KTPopCompleteCount = null;
		private HudStaticText KTLabel3 = null;
		private HudList KTPopMobsList = null;
		private HudTextBox KTPopMobTxt = null;
		private HudButton KTPopMobAddButton = null;
		private HudStaticText KTLabel4 = null;
		private HudList KTPopNPCList = null;
		private HudTextBox KTPopNPCTxt = null;
		private HudButton KTPopNPCAddButton = null;
		private HudStaticText KTLabel5 = null;
		private HudStaticText KTLabel6 = null;
		private HudStaticText KTLabel7 = null;
		private HudStaticText KTLabel8 = null;	
		private HudTextBox KTPopNPCInfo = null;
		private HudTextBox KTPopNPCCoords = null;
		private HudTextBox KTPopNPCFlagTxt = null;
		private HudTextBox KTPopNPCCompleteTxt = null;		
		private HudList.HudListRowAccessor KTPopRow = null;
		private KillTask KTHolder = null;
		private string OldKTName = String.Empty;
		
		private void RenderKillTaskPopUp()
		{		
			try
			{			
				KTPopView = new HudView(KTHolder.TaskName, 320, 500, null);
				KTPopView.UserAlphaChangeable = false;
				KTPopView.ShowInBar = false;
				KTPopView.UserResizeable = true;
				KTPopView.Visible = true;
				KTPopView.Ghosted = false;
				KTPopView.UserClickThroughable = false;	
				KTPopView.UserMinimizable = true;	
				KTPopView.UserGhostable = false;
	
				KTPopTabView = new HudTabView();
				KTPopView.Controls.HeadControl = KTPopTabView;
				
				KTPopLayout = new HudFixedLayout();
				KTPopTabView.AddTab(KTPopLayout, "Edit");
				
				KTLabel1 = new HudStaticText();
				KTPopLayout.AddControl(KTLabel1, new Rectangle(0,0,100,16));
				KTLabel1.Text = "Kill Task Name:";
				
				KTPopTaskName = new HudTextBox();
				KTPopLayout.AddControl(KTPopTaskName, new Rectangle(0,20,mKTSet.HudWidth, 16));
				KTPopTaskName.Text = KTHolder.TaskName;
				
				KTLabel2 = new HudStaticText();
				KTPopLayout.AddControl(KTLabel2, new Rectangle(0,40,75,16));
				KTLabel2.Text = "Number:";
							
				KTPopCompleteCount = new HudTextBox();
				KTPopLayout.AddControl(KTPopCompleteCount, new Rectangle(80,40,220,16));
				KTPopCompleteCount.Text = KTHolder.CompleteCount.ToString();
				
				KTLabel3 = new HudStaticText();
				KTPopLayout.AddControl(KTLabel3, new Rectangle(0,60,100,16));
				KTLabel3.Text = "Creature List:";
				
				KTPopMobsList = new HudList();
				KTPopLayout.AddControl(KTPopMobsList, new Rectangle(0,80,300,90));
				KTPopMobsList.AddColumn(typeof(HudStaticText),250,null);
				KTPopMobsList.AddColumn(typeof(HudPictureBox),16,null);
				
				foreach(string mob in KTHolder.MobNames)
				{
					KTPopRow = KTPopMobsList.AddRow();
					((HudStaticText)KTPopRow[0]).Text = mob;
					((HudPictureBox)KTPopRow[1]).Image = CorpseRemoveCircle;
				}
				
				KTPopMobsList.Click += KTPopMobsList_Click;
				
				KTPopMobTxt = new HudTextBox();
				KTPopLayout.AddControl(KTPopMobTxt, new Rectangle(0,180,250,16));
				
				KTPopMobAddButton = new HudButton();
				KTPopLayout.AddControl(KTPopMobAddButton, new Rectangle(260,180,40,16));
				KTPopMobAddButton.Text = "Add";
				
				KTPopMobAddButton.Hit += KTPopMobAddButton_Hit;
				
				KTLabel4 = new HudStaticText();
				KTPopLayout.AddControl(KTLabel4, new Rectangle(0,200,100,16));
				KTLabel4.Text = "NPC List:";
				
				KTPopNPCList = new HudList();
				KTPopLayout.AddControl(KTPopNPCList, new Rectangle(0,220,300,90));
				KTPopNPCList.AddColumn(typeof(HudStaticText),250,null);
				KTPopNPCList.AddColumn(typeof(HudPictureBox),16,null);
				
				foreach(string mob in KTHolder.NPCNames)
				{
					KTPopRow = KTPopNPCList.AddRow();
					((HudStaticText)KTPopRow[0]).Text = mob;
					((HudPictureBox)KTPopRow[1]).Image = CorpseRemoveCircle;
				}
				
				KTPopNPCList.Click += KTPopNPCList_Click;
						
				KTPopNPCTxt = new HudTextBox();
				KTPopLayout.AddControl(KTPopNPCTxt, new Rectangle(0,320,250,16));
				
				KTPopNPCAddButton = new HudButton();
				KTPopLayout.AddControl(KTPopNPCAddButton, new Rectangle(260,320,40,16));
				KTPopNPCAddButton.Text = "Add";
				
				KTPopNPCAddButton.Hit += KTPopNPCAddButton_Hit;
				
				KTLabel5 = new HudStaticText();
				KTPopLayout.AddControl(KTLabel5, new Rectangle(0,340,75,16));
				KTLabel5.Text = "NPC Info:";
				
				KTPopNPCInfo = new HudTextBox();
				KTPopLayout.AddControl(KTPopNPCInfo, new Rectangle(80,340,220,16));
				KTPopNPCInfo.Text = KTHolder.NPCInfo;
				
				KTLabel6 = new HudStaticText();
				KTPopLayout.AddControl(KTLabel6, new Rectangle(0,360,75,16));
				KTLabel6.Text = "NPC Coords:";
				
				KTPopNPCCoords = new HudTextBox();
				KTPopLayout.AddControl(KTPopNPCCoords, new Rectangle(80,360,220,16));
				KTPopNPCCoords.Text = KTHolder.NPCCoords;
				
				KTLabel7 = new HudStaticText();
				KTPopLayout.AddControl(KTLabel7, new Rectangle(0,380,75,16));
				KTLabel7.Text = "Flag Text:";
				
				KTPopNPCFlagTxt = new HudTextBox();
				KTPopLayout.AddControl(KTPopNPCFlagTxt, new Rectangle(80,380,220,16));
				KTPopNPCFlagTxt.Text = KTHolder.NPCYellowFlagText;
				
				KTLabel8 = new HudStaticText();
				KTPopLayout.AddControl(KTLabel8, new Rectangle(0, 400, 75,16));
				KTLabel8.Text = "Comp. Text:";
				
				KTPopNPCCompleteTxt = new HudTextBox();
				KTPopLayout.AddControl(KTPopNPCCompleteTxt, new Rectangle(80,400,220,16));
				KTPopNPCCompleteTxt.Text = KTHolder.NPCYellowCompleteText;
				
				KTPopView.VisibleChanged += KTPopView_VisibleChanged;
				
			}catch(Exception ex){LogError(ex);}
		}			
			
		private void KTPopDispose()
		{
			try
			{
			
				KTLabel8.Dispose();
				KTLabel7.Dispose();
				KTLabel6.Dispose();
				KTLabel5.Dispose();
				KTLabel4.Dispose();
				KTPopNPCCompleteTxt.Dispose();
				KTPopNPCFlagTxt.Dispose();
				KTPopNPCCoords.Dispose();
				KTPopNPCInfo.Dispose();
				KTPopNPCAddButton.Dispose();
				KTPopNPCTxt.Dispose();
				KTPopNPCList.Dispose(); 
				KTPopMobAddButton.Dispose();
				KTPopMobTxt.Dispose();
				KTPopMobsList.Dispose();
				KTLabel3.Dispose();
				KTPopCompleteCount.Dispose();
				KTLabel2.Dispose();
				KTPopTaskName.Dispose();
				KTLabel1.Dispose();
				KTPopLayout.Dispose();
				KTPopTabView.Dispose();
				KTPopView.Dispose();
				
				KTPopView.VisibleChanged -= KTPopView_VisibleChanged;
				
			}catch(Exception ex){LogError(ex);}
		}
		
		private void KTPopMobsList_Click(object sender, int row, int col)
		{
			try
			{
				int scroll = KTPopMobsList.ScrollPosition;
				
				KTPopRow = KTPopMobsList[row];
				if(col == 1)
				{
					KTHolder.MobNames.RemoveAll(x => x == ((HudStaticText)KTPopRow[0]).Text);
				}
				
				KTPopMobTxt.Text = String.Empty;
				KTPopMobsList.ClearRows();
				
				foreach(string mob in KTHolder.MobNames)
				{
					KTPopRow = KTPopMobsList.AddRow();
					((HudStaticText)KTPopRow[0]).Text = mob;
					((HudPictureBox)KTPopRow[1]).Image = CorpseRemoveCircle;
				}
				
				KTPopMobsList.ScrollPosition = scroll;
				
			}catch(Exception ex){LogError(ex);}
		}
		
		private void KTPopMobAddButton_Hit(object sender, EventArgs e)
		{
			try
			{
				int scroll = KTPopMobsList.ScrollPosition;
				
				KTHolder.MobNames.Add(KTPopMobTxt.Text);
				KTPopMobTxt.Text = String.Empty;
				KTPopMobsList.ClearRows();
				
				foreach(string mob in KTHolder.MobNames)
				{
					KTPopRow = KTPopMobsList.AddRow();
					((HudStaticText)KTPopRow[0]).Text = mob;
					((HudPictureBox)KTPopRow[1]).Image = CorpseRemoveCircle;
				}
				
				KTPopMobsList.ScrollPosition = scroll;
			}catch(Exception ex){LogError(ex);}
		}
		
		private void KTPopNPCList_Click(object sender, int row, int col)
		{
			try
			{
				int scroll = KTPopNPCList.ScrollPosition;
				
				KTPopRow = KTPopNPCList[row];
				if(col == 1)
				{
					KTHolder.NPCNames.RemoveAll(x => x == ((HudStaticText)KTPopRow[0]).Text);
				}
				
				KTPopNPCTxt.Text = String.Empty;
				KTPopNPCList.ClearRows();
				
				foreach(string npc in KTHolder.NPCNames)
				{
					KTPopRow = KTPopNPCList.AddRow();
					((HudStaticText)KTPopRow[0]).Text = npc;
					((HudPictureBox)KTPopRow[1]).Image = CorpseRemoveCircle;
				}
				
				KTPopNPCList.ScrollPosition = scroll;
			}catch(Exception ex){LogError(ex);}
		}
		
		private void KTPopNPCAddButton_Hit(object sender, EventArgs e)
		{
			try
			{
				int scroll = KTPopNPCList.ScrollPosition;
				
				KTHolder.NPCNames.Add(KTPopNPCTxt.Text);
				
				KTPopNPCTxt.Text = String.Empty;
				KTPopNPCList.ClearRows();
				
				foreach(string npc in KTHolder.NPCNames)
				{
					KTPopRow = KTPopNPCList.AddRow();
					((HudStaticText)KTPopRow[0]).Text = npc;
					((HudPictureBox)KTPopRow[1]).Image = CorpseRemoveCircle;
				}
				
				KTPopNPCList.ScrollPosition = scroll;
				
			}catch(Exception ex){LogError(ex);}
		}
		
			
		private void KTPopView_VisibleChanged(object sender, EventArgs e)
		{
			try
			{				
				KTHolder.TaskName = KTPopTaskName.Text;
				KTHolder.CompleteCount = Convert.ToInt32(KTPopCompleteCount.Text);
				KTHolder.NPCInfo = KTPopNPCInfo.Text;
				KTHolder.NPCCoords = KTPopNPCCoords.Text;
				KTHolder.NPCYellowFlagText = KTPopNPCFlagTxt.Text;
				KTHolder.NPCYellowCompleteText = KTPopNPCCompleteTxt.Text;			
				
				FileInfo KillTasksFile = new FileInfo(GearDir + @"\KillTasks.xml");
				XmlReaderSettings rsettings = new XmlReaderSettings();
			    rsettings.IgnoreWhitespace = true;
			    List<KillTask> MasterKTList = ReadMasterKTList();
			    
			    XmlWriterSettings wsettings = new XmlWriterSettings();
				wsettings.Indent = true;
				wsettings.NewLineOnAttributes = true;
				
				if(MasterKTList.Any(x => x.TaskName == OldKTName))
				{
					MasterKTList.RemoveAll(x => x.TaskName == OldKTName);
				}
				
				MasterKTList.Add(KTHolder);	
				
				WriteMasterKTList(MasterKTList);
				
				if(!mKTSet.MyKillTasks.Any(x => x.TaskName == KTHolder.TaskName))
				{
					mKTSet.MyKillTasks.Add(KTHolder);
				}

				if(OldKTName != KTHolder.TaskName)
				{
					mKTSet.MyKillTasks.RemoveAll(x => x.TaskName == OldKTName);
				}	
				
				ReadWriteGearTaskSettings(false);

				KTHolder = null;
				OldKTName = String.Empty;
						
				KTPopDispose();
				
				UpdateTaskPanel();
			}catch(Exception ex){LogError(ex);}
		}
		
		private HudView CTPopView = null;
		private HudTabView CTPopTabView = null;
		private HudFixedLayout CTPopLayout = null;
		private HudStaticText CTLabel1 = null;
		private HudTextBox CTPopTaskName = null;
		private HudStaticText CTLabel2 = null;
		private HudTextBox CTPopCompleteCount = null;
		private HudStaticText CTLabel3 = null;
		private HudList CTPopMobsList = null;
		private HudTextBox CTPopMobTxt = null;
		private HudButton CTPopMobAddButton = null;
		private HudStaticText CTLabel4 = null;
		private HudList CTPopNPCList = null;
		private HudTextBox CTPopNPCTxt = null;
		private HudButton CTPopNPCAddButton = null;
		private HudStaticText CTLabel5 = null;
		private HudStaticText CTLabel6 = null;
		private HudStaticText CTLabel7 = null;
		private HudStaticText CTLabel8 = null;	
		private HudTextBox CTPopNPCInfo = null;
		private HudTextBox CTPopNPCCoords = null;
		private HudTextBox CTPopNPCFlagTxt = null;
		private HudTextBox CTPopNPCCompleteTxt = null;		
		private HudList.HudListRowAccessor CTPopRow = null;
		private HudTextBox CTPopItemName = null;
		private HudStaticText CTLabel9 = null;
		private CollectTask CTHolder = null;
		private string OldCTName = String.Empty;
		
		private void RenderCollectTaskPopUp()
		{		
			try
			{			
				CTPopView = new HudView(CTHolder.TaskName, 320, 500, null);
				CTPopView.UserAlphaChangeable = false;
				CTPopView.ShowInBar = false;
				CTPopView.UserResizeable = true;
				CTPopView.Visible = true;
				CTPopView.Ghosted = false;
				CTPopView.UserClickThroughable = false;	
				CTPopView.UserMinimizable = true;	
				CTPopView.UserGhostable = false;
	
				CTPopTabView = new HudTabView();
				CTPopView.Controls.HeadControl = CTPopTabView;
				
				CTPopLayout = new HudFixedLayout();
				CTPopTabView.AddTab(CTPopLayout, "Edit");
				
				CTLabel1 = new HudStaticText();
				CTPopLayout.AddControl(CTLabel1, new Rectangle(0,0,100,16));
				CTLabel1.Text = "Collect Task Name:";
				
				CTPopTaskName = new HudTextBox();
				CTPopLayout.AddControl(CTPopTaskName, new Rectangle(0,20,mKTSet.HudWidth, 16));
				CTPopTaskName.Text = CTHolder.TaskName;
				
				CTLabel9 = new HudStaticText();
				CTPopLayout.AddControl(CTLabel9, new Rectangle(0,40,75,16));
				CTLabel9.Text = "Item Name:";
				
				CTPopItemName = new HudTextBox();
				CTPopLayout.AddControl(CTPopItemName, new Rectangle(80,40,220,16));
				CTPopItemName.Text = CTHolder.Item;
				
				CTLabel2 = new HudStaticText();
				CTPopLayout.AddControl(CTLabel2, new Rectangle(0,60,75,16));
				CTLabel2.Text = "Number:";
							
				CTPopCompleteCount = new HudTextBox();
				CTPopLayout.AddControl(CTPopCompleteCount, new Rectangle(80,60,220,16));
				CTPopCompleteCount.Text = CTHolder.CompleteCount.ToString();
				
				CTLabel3 = new HudStaticText();
				CTPopLayout.AddControl(CTLabel3, new Rectangle(0,80,100,16));
				CTLabel3.Text = "Creature List";
				
				CTPopMobsList = new HudList();
				CTPopLayout.AddControl(CTPopMobsList, new Rectangle(0,100,300,90));
				CTPopMobsList.AddColumn(typeof(HudStaticText),250,null);
				CTPopMobsList.AddColumn(typeof(HudPictureBox),16,null);
				
				foreach(string mob in CTHolder.MobNames)
				{
					CTPopRow = CTPopMobsList.AddRow();
					((HudStaticText)CTPopRow[0]).Text = mob;
					((HudPictureBox)CTPopRow[1]).Image = CorpseRemoveCircle;
				}
				
				CTPopMobsList.Click += CTPopMobsList_Click;
				
				CTPopMobTxt = new HudTextBox();
				CTPopLayout.AddControl(CTPopMobTxt, new Rectangle(0,200,250,16));
				
				CTPopMobAddButton = new HudButton();
				CTPopLayout.AddControl(CTPopMobAddButton, new Rectangle(260,200,40,16));
				CTPopMobAddButton.Text = "Add";
				
				CTPopMobAddButton.Hit += CTPopMobAddButton_Hit;
				
				CTLabel4 = new HudStaticText();
				CTPopLayout.AddControl(CTLabel4, new Rectangle(0,220,100,16));
				CTLabel4.Text = "NPC List";
				
				CTPopNPCList = new HudList();
				CTPopLayout.AddControl(CTPopNPCList, new Rectangle(0,240,300,90));
				CTPopNPCList.AddColumn(typeof(HudStaticText),250,null);
				CTPopNPCList.AddColumn(typeof(HudPictureBox),16,null);
				
				foreach(string mob in CTHolder.NPCNames)
				{
					CTPopRow = CTPopNPCList.AddRow();
					((HudStaticText)CTPopRow[0]).Text = mob;
					((HudPictureBox)CTPopRow[1]).Image = CorpseRemoveCircle;
				}
				
				CTPopNPCList.Click += CTPopNPCList_Click;
						
				CTPopNPCTxt = new HudTextBox();
				CTPopLayout.AddControl(CTPopNPCTxt, new Rectangle(0,340,250,16));
				
				CTPopNPCAddButton = new HudButton();
				CTPopLayout.AddControl(CTPopNPCAddButton, new Rectangle(260,340,40,16));
				CTPopNPCAddButton.Text = "Add";
				
				CTPopNPCAddButton.Hit += CTPopNPCAddButton_Hit;
				
				CTLabel5 = new HudStaticText();
				CTPopLayout.AddControl(CTLabel5, new Rectangle(0,360,75,16));
				CTLabel5.Text = "NPC Info:";
				
				CTPopNPCInfo = new HudTextBox();
				CTPopLayout.AddControl(CTPopNPCInfo, new Rectangle(80,360,220,16));
				CTPopNPCInfo.Text = CTHolder.NPCInfo;
				
				CTLabel6 = new HudStaticText();
				CTPopLayout.AddControl(CTLabel6, new Rectangle(0,380,75,16));
				CTLabel6.Text = "NPC Coords:";
				
				CTPopNPCCoords = new HudTextBox();
				CTPopLayout.AddControl(CTPopNPCCoords, new Rectangle(80,380,220,16));
				CTPopNPCCoords.Text = CTHolder.NPCCoords;
				
				CTLabel7 = new HudStaticText();
				CTPopLayout.AddControl(CTLabel7, new Rectangle(0,400,75,16));
				CTLabel7.Text = "Flag Text:";
				
				CTPopNPCFlagTxt = new HudTextBox();
				CTPopLayout.AddControl(CTPopNPCFlagTxt, new Rectangle(80,400,220,16));
				CTPopNPCFlagTxt.Text = CTHolder.NPCYellowFlagText;
				
				CTLabel8 = new HudStaticText();
				CTPopLayout.AddControl(CTLabel8, new Rectangle(0, 420, 75,16));
				CTLabel8.Text = "Comp. Text:";
				
				CTPopNPCCompleteTxt = new HudTextBox();
				CTPopLayout.AddControl(CTPopNPCCompleteTxt, new Rectangle(80,420,220,16));
				CTPopNPCCompleteTxt.Text = CTHolder.NPCYellowCompleteText;
				
				CTPopView.VisibleChanged += CTPopView_VisibleChanged;
				
			}catch(Exception ex){LogError(ex);}
		}			
			
		private void CTPopDispose()
		{
			try
			{
				
				CTPopView.VisibleChanged -= CTPopView_VisibleChanged;
				CTPopItemName.Dispose();
				CTLabel9.Dispose();
				CTLabel8.Dispose();
				CTLabel7.Dispose();
				CTLabel6.Dispose();
				CTLabel5.Dispose();
				CTLabel4.Dispose();
				CTPopNPCCompleteTxt.Dispose();
				CTPopNPCFlagTxt.Dispose();
				CTPopNPCCoords.Dispose();
				CTPopNPCInfo.Dispose();
				CTPopNPCAddButton.Dispose();
				CTPopNPCTxt.Dispose();
				CTPopNPCList.Dispose(); 
				CTPopMobAddButton.Dispose();
				CTPopMobTxt.Dispose();
				CTPopMobsList.Dispose();
				CTLabel3.Dispose();
				CTPopCompleteCount.Dispose();
				CTLabel2.Dispose();
				CTPopTaskName.Dispose();
				CTLabel1.Dispose();
				CTPopLayout.Dispose();
				CTPopTabView.Dispose();
				CTPopView.Dispose();
			}catch(Exception ex){LogError(ex);}
		}
		
		private void CTPopMobsList_Click(object sender, int row, int col)
		{
			try
			{
				int scroll = CTPopMobsList.ScrollPosition;
				
				CTPopRow = CTPopMobsList[row];
				if(col == 1)
				{
					CTHolder.MobNames.RemoveAll(x => x == ((HudStaticText)CTPopRow[0]).Text);
				}
				
				CTPopMobTxt.Text = String.Empty;
				CTPopMobsList.ClearRows();
				
				foreach(string mob in CTHolder.MobNames)
				{
					CTPopRow = CTPopMobsList.AddRow();
					((HudStaticText)CTPopRow[0]).Text = mob;
					((HudPictureBox)CTPopRow[1]).Image = CorpseRemoveCircle;
				}
				
				CTPopMobsList.ScrollPosition = scroll;
				
			}catch(Exception ex){LogError(ex);}
		}
		
		private void CTPopMobAddButton_Hit(object sender, EventArgs e)
		{
			try
			{
				int scroll = CTPopMobsList.ScrollPosition;
				
				CTHolder.MobNames.Add(CTPopMobTxt.Text);
				CTPopMobTxt.Text = String.Empty;
				CTPopMobsList.ClearRows();
				
				foreach(string mob in CTHolder.MobNames)
				{
					CTPopRow = CTPopMobsList.AddRow();
					((HudStaticText)CTPopRow[0]).Text = mob;
					((HudPictureBox)CTPopRow[1]).Image = CorpseRemoveCircle;
				}
				
				CTPopMobsList.ScrollPosition = scroll;
			}catch(Exception ex){LogError(ex);}
		}
		
		private void CTPopNPCList_Click(object sender, int row, int col)
		{
			try
			{
				int scroll = CTPopNPCList.ScrollPosition;
				
				CTPopRow = CTPopNPCList[row];
				if(col == 1)
				{
					CTHolder.NPCNames.RemoveAll(x => x == ((HudStaticText)CTPopRow[0]).Text);
				}
				
				CTPopNPCTxt.Text = String.Empty;
				CTPopNPCList.ClearRows();
				
				foreach(string npc in CTHolder.NPCNames)
				{
					CTPopRow = CTPopNPCList.AddRow();
					((HudStaticText)CTPopRow[0]).Text = npc;
					((HudPictureBox)CTPopRow[1]).Image = CorpseRemoveCircle;
				}
				
				CTPopNPCList.ScrollPosition = scroll;
			}catch(Exception ex){LogError(ex);}
		}
		
		private void CTPopNPCAddButton_Hit(object sender, EventArgs e)
		{
			try
			{
				int scroll = CTPopNPCList.ScrollPosition;
				
				CTHolder.NPCNames.Add(CTPopNPCTxt.Text);
				
				CTPopNPCTxt.Text = String.Empty;
				CTPopNPCList.ClearRows();
				
				foreach(string npc in CTHolder.NPCNames)
				{
					CTPopRow = CTPopNPCList.AddRow();
					((HudStaticText)CTPopRow[0]).Text = npc;
					((HudPictureBox)CTPopRow[1]).Image = CorpseRemoveCircle;
				}
				
				CTPopNPCList.ScrollPosition = scroll;
				
			}catch(Exception ex){LogError(ex);}
		}
		
			
		private void CTPopView_VisibleChanged(object sender, EventArgs e)
		{
			try
			{				
				CTHolder.TaskName = CTPopTaskName.Text;
				CTHolder.CompleteCount = Convert.ToInt32(CTPopCompleteCount.Text);
				CTHolder.Item = CTPopItemName.Text;
				CTHolder.NPCInfo = CTPopNPCInfo.Text;
				CTHolder.NPCCoords = CTPopNPCCoords.Text;
				CTHolder.NPCYellowFlagText = CTPopNPCFlagTxt.Text;
				CTHolder.NPCYellowCompleteText = CTPopNPCCompleteTxt.Text;			
				
				List<CollectTask> MasterCTList = ReadMasterCTList();
				
				if(MasterCTList.Any(x => x.TaskName == OldCTName))
				{
					MasterCTList.RemoveAll(x => x.TaskName == OldCTName);
				}
				
				MasterCTList.Add(CTHolder);	
				
				WriteMasterCTList(MasterCTList);
				
				if(!mKTSet.MyCollectTasks.Any(x => x.TaskName == CTHolder.TaskName))
				{
					mKTSet.MyCollectTasks.Add(CTHolder);
				}

				if(OldCTName != CTHolder.TaskName)
				{
					mKTSet.MyCollectTasks.RemoveAll(x => x.TaskName == OldCTName);
				}	
				
				ReadWriteGearTaskSettings(false);
				
				
				CTHolder = null;
				OldCTName = String.Empty;
						
				CTPopDispose();
				
				UpdateTaskPanel();
			}catch(Exception ex){LogError(ex);}
		}
		
		
		void WriteKillTaskFailureToFile(string mobname, int numberkilled, int totalnumber)
		{
			try
			{
		
				FileInfo TaskFile = new FileInfo(GearDir + @"\Taskdebugging.txt");
            	if(TaskFile.Exists)
            	{
            		if(TaskFile.Length > 1000000) {TaskFile.Delete();}
            	}
            	
            	using (StreamWriter writer = new StreamWriter(TaskFile.ToString(), true))
                {
                    writer.WriteLine("============================================================================");
                    writer.WriteLine(DateTime.Now.ToString());
                    writer.WriteLine("MobName: " + mobname);
                    writer.WriteLine("NumberKilled: " + numberkilled.ToString());
                    writer.WriteLine("TotalNumber: " + totalnumber.ToString());
                    writer.WriteLine("============================================================================");
                    writer.WriteLine("");
                    writer.Close();
                }
			

             }catch{}
		}
		
		
		
		
		
	
		
		
	}
}
