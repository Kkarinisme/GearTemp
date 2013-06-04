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
	public partial class PluginCore
	{
		

		

	 	
		private void Inspector_ActionComplete(object sender, System.EventArgs e)
		{
			try
			{

				
			}catch(Exception ex){LogError(ex);}
		}
		
		//This is one of the few crossovers.
		private void AutoLootStarter(object sender, System.EventArgs e)
		{
			try
			{
				
			}catch(Exception ex){LogError(ex);}
		}
		
		
		
		//Callbacks are made using CoreManager.Current.RenderFrame event.  It checks every time a new frame is rendered.  sweet.....	
		private void AutoPickUp(int ItemId)
		{
			try
			{

			}catch(Exception ex){LogError(ex);}
		}
		
		private void GearInspectorReadWriteSettings(bool read)
		{
			try
			{
				FileInfo GearInspectorSettingsFile = new FileInfo(toonDir + @"\GearInspector.xml");
								
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

	}
}
