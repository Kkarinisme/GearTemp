using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Decal;
using Decal.Adapter;
using Decal.Adapter.Wrappers;
using Decal.Filters;
using Decal.Adapter.NetParser;
using Decal.Adapter.Messages;
using System.Xml.Serialization;
using VirindiViewService;
using MyClasses.MetaViewWrappers;
using WindowsTimer = System.Windows.Forms.Timer;


namespace AlincoVVS
{
    public partial class PluginCore : PluginBase
    {

        public static class Util
        {
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
        }
    }
}