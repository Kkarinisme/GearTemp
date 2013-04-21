using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Diagnostics;
using System.Net;
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
                using (StreamWriter writer = new StreamWriter(Environment.GetFolderPath(Environment.SpecialFolder.Personal) + @"\Decal Plugins\" + Globals.PluginName + " errors.txt", true))
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
                Globals.Host.Actions.AddChatText("<{" + Globals.PluginName + "}>: " + message, 5);
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
  
	
	}}

