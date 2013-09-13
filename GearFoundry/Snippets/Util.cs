using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Diagnostics;
using System.Net;
using Decal;
using Decal.Adapter;
using Decal.Adapter.Wrappers;
using Decal.Interop.D3DService;
using Decal.Interop;
using System.Media;
namespace GearFoundry
{
	public partial class PluginCore
	{

		public const int ERRORLINK_ID = 221113;
		public static string docPath = string.Empty;
		public static string dllversion = string.Empty;
		public static string wavPath = string.Empty;
	
		public static int TotalErrors;
		
        public static void LogError(Exception ex)
        {
            try
            {
            	FileInfo ErrorLog = new FileInfo(Environment.GetFolderPath(Environment.SpecialFolder.Personal) + @"\Decal Plugins\" + Globals.PluginName + @"\" + Globals.PluginName + "errors.txt");
            	if(ErrorLog.Exists)
            	{
            		if(ErrorLog.Length > 1000000) {ErrorLog.Delete();}
            	}
            	
                using (StreamWriter writer = new StreamWriter(Environment.GetFolderPath(Environment.SpecialFolder.Personal) + @"\Decal Plugins\" + Globals.PluginName + @"\" + Globals.PluginName + "errors.txt", true))
                {
                    writer.WriteLine("============================================================================");
                    writer.WriteLine(DateTime.Now.ToString());
                    writer.WriteLine("Error: " + ex.Message);
                    writer.WriteLine("Source: " + ex.Source);
                    writer.WriteLine("Stack: " + ex.StackTrace);
                    if (ex.InnerException != null)
                    {
                        writer.WriteLine("Inner: " + ex.InnerException.Message);
                        writer.WriteLine("Inner Stack: " + ex.InnerException.StackTrace);
                    }
                    writer.WriteLine("============================================================================");
                    writer.WriteLine("");
                    writer.Close();
                }
            }
            catch
            {
            }
        }


        public static void WriteToChat(string message)
        {
            try
            {
                Globals.Host.Actions.AddChatText("#" + Globals.PluginName + "#: " + message, 2);
            }
            catch (Exception ex) { LogError(ex); }
        }
        
        public static void HudToChat(string report, int color)
        {
            try
            {
            	Globals.Host.Actions.AddChatText(report, color); 	
            }
            catch (Exception ex) { LogError(ex); }
        }
        
        public static void ReportStringToChat(string report)
        {
        	try
            {
                Globals.Host.Actions.AddChatText(report, 2);
            }
            catch (Exception ex) { LogError(ex); }
        }
       	
        //Used to read embeeded .xml files
       	public string GetResourceTextFile(string filename)
		{
		    string result = string.Empty;
		
		    using (Stream stream = this.GetType().Assembly.
		               GetManifestResourceStream(filename))
		    {
		        using (StreamReader sr = new StreamReader(stream))
		        {
		            result = sr.ReadToEnd();
		        }
		    }
		    return result;
		}
       	
       	//Irq:  Added to replace the loss of the old IdqueueAdd routine.
		private void IdqueueAdd(int id)
		{
			Core.Actions.RequestId(id);	
		}


        DateTime ArrowKill = DateTime.MinValue;


        private void ArrowInitiator()
        {
            ArrowKill = DateTime.Now;
            Core.RenderFrame += new EventHandler<EventArgs>(FireTheArrow);

        }

        void FireTheArrow(object sender, EventArgs e)
        {
            if ((DateTime.Now - ArrowKill).TotalSeconds > 3)
            {
                Core.RenderFrame -= FireTheArrow;
            }
            else
            {
                DoShowArrow();
            }
        }
        private void DoShowArrow()
        {
            Decal.Adapter.Wrappers.D3DObj mMarkObject;
            mMarkObject = Core.D3DService.PointToObject(nusearrowid, (unchecked((int)0xFFBB0000)));
        }
        

        
        private void playSoundFromResource(int SoundFileId)
		{
            if (bMuteSounds) { return; }
            else
            {
                
                System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();
                switch (mSound)
                {
                    case 1:
                        s = a.GetManifestResourceStream("<GearFoundry>.Sounds.blip.wav");
                        break;
                    case 2:
                        s = a.GetManifestResourceStream("<GearFoundry>.Sounds.chime.wav");
                        break;
                    case 3:
                        s = a.GetManifestResourceStream("<GearFoundry>.Sounds.click.wav");
                        break;
                    case 4:
                        s = a.GetManifestResourceStream("<GearFoundry>.Sounds.click2.wav");
                        break;
                    case 5:
                        s = a.GetManifestResourceStream("<GearFoundry>.Sounds.oop.wav");
                        break;
                    case 6:
                        s = a.GetManifestResourceStream("<GearFoundry>.Sounds.pluck.wav");
                        break;
                    case 7:
                        s = a.GetManifestResourceStream("<GearFoundry>.Sounds.splooge.wav");
                        break;
                    case 8:
                        s = a.GetManifestResourceStream("<GearFoundry>.Sounds.till.wav");
                        break;
                    case 9:
                        s = a.GetManifestResourceStream("<GearFoundry>.Sounds.womp.wav");
                        break;
                    case 10:
                        s = a.GetManifestResourceStream("<GearFoundry>.Sounds.womp2.wav");
                        break;
                    default:
                        s = a.GetManifestResourceStream("<GearFoundry>.Sounds.blip.wav");
                        break;

                }
                SoundPlayer player = new SoundPlayer(s);
                player.Play();
            }
		}
        
        private List<int> _ConvertCommaStringToIntList(string BaseString)
        {
        	try
        	{
        		List<int> Ids = new List<int>();
            	if(BaseString != String.Empty && BaseString != "")
            	{
            		string[] SplitString = BaseString.Split(',');
            		foreach(string str in SplitString)
            		{
            			Ids.Add(Convert.ToInt32(str));
            		}
            	}

            	return Ids;        		
        	}catch(Exception ex){LogError(ex); return new List<int>();}
        }
        

        
        private string _ConvertIntListToCommaString(List<int> BaseList)
        {
        	try
        	{
        		string result = String.Empty;
        		if(BaseList.Count != 0)
        		{
	        		BaseList.Sort();
	        	
	        		for(int i = 0; i < BaseList.Count; i++)
	        		{
	        			result += BaseList[i].ToString();
	        			if(i != BaseList.Count -1) {result += ",";}
	        		}
        		}
        		return result;
        		
        	}catch(Exception ex){LogError(ex); return String.Empty;}
        }
        
//        private void playSimpleSound()
//		{
//		    SoundPlayer simpleSound = new SoundPlayer(@"c:\Windows\Media\chimes.wav");
//		    simpleSound.Play();
//		}
	
	}}

