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
using VirindiViewService.Themes;
using System.Xml.Serialization;
using System.Xml;

namespace GearFoundry
{
	/// <summary>
	/// Description of ItemTrackerComponents.
	/// </summary>
	public partial class PluginCore
	{
		
		public class OpenContainer
		{
			public bool ContainerIsLooting = false;
			public int ContainerGUID = 0;
			public DateTime LastCheck;
			public DateTime LootingStarted;
			public List<IdentifiedObject> ContainerIOs = new List<PluginCore.IdentifiedObject>();
		}
		
		private void GearInspectorReadWriteSettings(bool read)
		{
			try
			{
                FileInfo GearInspectorSettingsFile = new FileInfo(GearDir + @"\GearInspector.xml");
								
				if (read)
				{
					
					try
					{
						if (!GearInspectorSettingsFile.Exists)
		                {
		                    try
		                    {
		                    	string filedefaults = GetResourceTextFile("GearInspector.xml");
		                    	using (StreamWriter writedefaults = new StreamWriter(GearInspectorSettingsFile.ToString(), true))
								{
									writedefaults.Write(filedefaults);
									writedefaults.Close();
								}
		                    }
		                    catch (Exception ex) { LogError(ex); }
		                }
						
						using (XmlReader reader = XmlReader.Create(GearInspectorSettingsFile.ToString()))
						{	
							XmlSerializer serializer = new XmlSerializer(typeof(GearInspectorSettings));
							GISettings = (GearInspectorSettings)serializer.Deserialize(reader);
							reader.Close();
						}
					}
					catch
					{
						GISettings = new GearInspectorSettings();
					}
				}
				
				
				if(!read)
				{
					if(GearInspectorSettingsFile.Exists)
					{
						GearInspectorSettingsFile.Delete();
					}
					
					using (XmlWriter writer = XmlWriter.Create(GearInspectorSettingsFile.ToString()))
					{
			   			XmlSerializer serializer2 = new XmlSerializer(typeof(GearInspectorSettings));
			   			serializer2.Serialize(writer, GISettings);
			   			writer.Close();
					}
				}
			}catch(Exception ex){LogError(ex);}
		}
		
		private void ItemHud_ChangeObject(object sender, ChangeObjectEventArgs e)
	 	{
	 		try
	 		{
	 			if(e.Changed.Id == ItemHudMoveId)
	 			{
	 				IHRenderTime150 = DateTime.Now;
//	 				Core.RenderFrame += new EventHandler<EventArgs>(IHRenderFrame150ms);
	 			}
	 			
	 		}catch(Exception ex){LogError(ex);}
	 	}
		
		private void IHRenderFrame150ms(object sender, System.EventArgs e)
	 	{
	 		try
	 		{
	 			if((DateTime.Now - IHRenderTime150).TotalMilliseconds < 150) {return;}
	 			else
	 			{
	 				ItemHudMoveId = 0;
	 				Core.RenderFrame -= new EventHandler<EventArgs>(IHRenderFrame150ms);
	 			}
	 		}catch(Exception ex){LogError(ex);}
	 	}
	 	
		private void Inspector_ActionComplete(object sender, System.EventArgs e)
		{
			try
			{
				if(GISettings.AutoLoot && bCorpseHudEnabled && CorpseTrackingList.Count > 0) 
				{
					AutoLootDelayStart = DateTime.Now;
//					Core.RenderFrame += new EventHandler<EventArgs>(AutoLootStarter);
				}
				
			}catch(Exception ex){LogError(ex);}
		}
		
		//This is one of the few crossovers.
		private void AutoLootStarter(object sender, System.EventArgs e)
		{
			try
			{
				if((DateTime.Now - AutoLootDelayStart).TotalSeconds < 5 || mOpenContainer.ContainerIsLooting) {return;}
				else if(CorpseTrackingList.Count > 0)
				{
					Core.Actions.UseItem(CorpseTrackingList.First().Id, 0);
					CorpseTrackingList.RemoveAt(0);
					UpdateCorpseHud();
					Core.RenderFrame -= new EventHandler<EventArgs>(AutoLootStarter);
				}
			}catch(Exception ex){LogError(ex);}
		}
		
		private void LootContainerOpened(object sender, ContainerOpenedEventArgs e)
		{
			//Patterned off Mag-Tools Looter
			try
			{					
				
				WorldObject container = Core.WorldFilter[e.ItemGuid];
				
				if(container.Name.Contains("Storage")) {return;}
				if(container == null) {return;}

				if(container.ObjectClass == ObjectClass.Corpse)
				{
					if(container.Name.Contains(Core.CharacterFilter.Name))
					{
						ghSettings.DeadMeList.RemoveAll(x => x.GUID == container.Id);
						return;
					}
					//Don't loot out permitted corpses.....
//					else if(PermittedCorpsesList.Count() > 0 && container.Name
//					{
//
//					}
					else
					{
						CheckContainer(container);
					}
				}
				if(container.Name.Contains("Chest") || container.Name.Contains("Vault") || container.Name.Contains("Reliquary"))
				{
					CheckContainer(container);	
				}
			}
			catch{}
		}
		
				//Callbacks are made using CoreManager.Current.RenderFrame event.  It checks every time a new frame is rendered.  sweet.....
		
		private void CheckContainer(WorldObject container)
		{
			try
			{
				mOpenContainer.ContainerGUID = container.Id;
				mOpenContainer.LastCheck = DateTime.Now;
				mOpenContainer.LootingStarted = DateTime.Now;
				WorldObject[] ContainerContents = Core.WorldFilter.GetByContainer(container.Id).ToArray();
				foreach(WorldObject wo in ContainerContents)
				{
					if(!ItemExclusionList.Any(x => x == wo.Id))
					{
						mOpenContainer.ContainerIOs.Add(new IdentifiedObject(wo));
					}
				}
				
				if(mOpenContainer.ContainerIOs.Count() == 0)
				{
					mOpenContainer.ContainerIsLooting = false;
				}
				else
				{
					foreach(IdentifiedObject IOItem in mOpenContainer.ContainerIOs)
					{
						SeparateItemsToID(IOItem);
					}
					LockContainerOpen();
				}
			}catch{}
		}
		
		private void LockContainerOpen()
		{
			try
			{
				mOpenContainer.ContainerIsLooting = true;
				CoreManager.Current.RenderFrame += new EventHandler<EventArgs>(RenderFrame_LootingCheck);					
			}catch(Exception ex){LogError(ex);}
		}
		
		private void RenderFrame_LootingCheck(object sender, System.EventArgs e)
		{
			try
			{
				if((DateTime.Now - mOpenContainer.LastCheck).TotalMilliseconds < 300) {return;}	
				if((DateTime.Now - mOpenContainer.LastCheck).TotalSeconds > 5) {UnlockContainer();}
        		

				if(GISettings.AutoLoot)
    			{
//    				if(mOpenContainer.ContainerIOs.Any(x => x.IOR != IOResult.unknown) && ItemHudMoveId == 0)
//    				{
////    					if(mOpenContainer.ContainerGUID != Core.Actions.OpenedContainer)
////    					{
////    						Core.Actions.UseItem(mOpenContainer.ContainerGUID, 0);
////    					}
//    					
////    					Core.Actions.MoveItem(mOpenContainer.ContainerIOs.First(x => x.IOR != IOResult.unknown).Id,Core.CharacterFilter.Id,0,true);
////    					ItemHudMoveId = mOpenContainer.ContainerIOs.First(x => x.IOR != IOResult.unknown).Id;
//    					return;
//    			  	}
//    				else if(mOpenContainer.ContainerIOs.Any(x => x.IOR == IOResult.unknown))
//    				{
////    					if(mOpenContainer.ContainerGUID != Core.Actions.OpenedContainer)
////    					{
////    						Core.Actions.UseItem(mOpenContainer.ContainerGUID, 0);
////    					}
//    					return;
//    				}
//    				else
//    				{
//    					mOpenContainer.ContainerIsLooting = false;
//        				UnlockContainer();
//    				}
    			}
					
				if(mOpenContainer.ContainerIOs.Count > 0 && mOpenContainer.ContainerIOs.Any(x => x.IOR == IOResult.unknown))
				{
					if(mOpenContainer.ContainerGUID != Core.Actions.OpenedContainer)
    				{
    					Core.Actions.UseItem(mOpenContainer.ContainerGUID, 0);
    				}
				}
				else
        		{
        			UnlockContainer();
        		}
        		
        		
			}catch(Exception ex){LogError(ex);}
		}
		
		private void UnlockContainer()
		{
			try
			{
				mOpenContainer.ContainerIsLooting = false;
				CoreManager.Current.RenderFrame -= new EventHandler<EventArgs>(RenderFrame_LootingCheck);					
			}catch(Exception ex){LogError(ex);}
		}
		
		private void AutoPickUp(int ItemId)
		{
			try
			{
				Core.Actions.MoveItem(ItemId, Core.CharacterFilter.Id, 0, true);
			}catch(Exception ex){LogError(ex);}
		}

	}
}
