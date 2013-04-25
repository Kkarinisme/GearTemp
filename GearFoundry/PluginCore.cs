/////////////////////////////////////////////////////////////////////////////////
////This file is Copyright (c) 2011 VirindiPlugins
////
////Permission is hereby granted, free of charge, to any person obtaining a copy
////  of this software and associated documentation files (the "Software"), to deal
////  in the Software without restriction, including without limitation the rights
////  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
////  copies of the Software, and to permit persons to whom the Software is
////  furnished to do so, subject to the following conditions:
////
////The above copyright notice and this permission notice shall be included in
////  all copies or substantial portions of the Software.
////
////THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
////  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
////  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
////  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
////  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
////  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
////  THE SOFTWARE.//
/////////////////////////////////////////////////////////////////////////////////

// This entire file is being used -- Karin 4/16/13

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
using Decal.Interop;
using System.Runtime.InteropServices;
using Decal.Adapter.Wrappers;
using System.Drawing;
using System.Xml.Linq;
using System.ComponentModel;
using VirindiViewService;
using MyClasses.MetaViewWrappers;

namespace GearFoundry
{
   [WireUpBaseEvents]
   
    [FriendlyName("GearFoundry")]
    public partial class PluginCore : PluginBase
    {

        protected override void Startup()
        {
            try
            {
                Globals.Init("GearFoundry", Host, Core);
                ViewInit();
                Instance = this;
                host = Host;
                InitEvents();
            }
            catch (Exception ex) { LogError(ex); }
        }

        protected override void Shutdown()
        {
            try
            {
            	
            	DisposeOnShutdown();
                if (quickiesvHud != null)
                {
                    DisposeVerticalQuickSlots();
                }


                if (quickieshHud != null)
                {
                    DisposeHorizontalQuickSlots();
                }
                
                if(CorpseHudView != null) {DisposeCorpseHud();}
                if(LandscapeHudView != null) {DisposeLandscapeHud();}
                if(ItemHudView != null) {DisposeItemHud();}

                //Destroy the view.
                MVWireupHelper.WireupEnd(this);
                View.Dispose();
                EndEvents();
                Core.CharacterFilter.LoginComplete -= new EventHandler(OnCharacterFilterLoginCompleted);
            }
            catch (Exception ex) { LogError(ex); }
        }
        
        private void DisposeOnShutdown()
        {
       		if(CorpseHudView != null) {DisposeCorpseHud();}
            if(LandscapeHudView != null) {DisposeLandscapeHud();}
            if(ItemHudView != null) {DisposeItemHud();}
            if(ButlerHudView != null){DisposeButlerHud();}
        }
        
        
        public void InitEvents()
		{

			try 
			{
				FileService = Core.Filter<FileService>();				
				Core.CharacterFilter.LoginComplete += new EventHandler(OnCharacterFilterLoginCompleted);			
				MasterTimer = new System.Windows.Forms.Timer();	
				
				RenderItemHud();
                SubscribeLootEvents();
                
                RenderButlerHud();
                SubscribeButlerEvents();
				
			} catch (Exception ex) {
				LogError(ex);
			}
		}


		public void EndEvents()
		{
			try {

				if (MasterTimer != null) {
					MasterTimer.Stop();
					MasterTimer.Tick -= LandscapeTimerTick;
					MasterTimer.Tick -= CorpseCheckerTick;
				}
				
				UnsubscribeLootEvents();
				
				DisposeItemHud();

				GC.Collect();
				GC.WaitForPendingFinalizers();
	
				host = null;

			} catch (Exception ex) {
				LogError(ex);
			}

		}


        private void OnCharacterFilterLoginCompleted(object sender, System.EventArgs e)
        {
            WriteToChat("Plugin now online. Server population: " + Core.CharacterFilter.ServerPopulation);
            try
            {
            	InitPaths();
            	LoadSettingsFiles();
            	
				InitListBuilder();
               
                InitFilenames();
                loadFiles();
                loadLists();
                populateRulesListBox();
                populateRuleSpellEnabledListBox();
                startRoutines();

                Decal.Adapter.CoreManager.Current.ItemSelected += new EventHandler<ItemSelectedEventArgs>(Current_ItemSelected);

                WriteToChat("Plugin now online. Server population: " + Core.CharacterFilter.ServerPopulation);
                if(CloakSpellList != null)
                    WriteToChat("CloakSpellList count: " + CloakSpellList.Count);
                else
                    WriteToChat("CloakSpellList does not exist." );

                //ToMish:  I am. 
                MasterTimer.Interval = 1000;
                MasterTimer.Start();
                
               

                mCharacterLoginComplete = true;
                
                
  

            }
            catch (Exception ex) { LogError(ex); }
        }

        private void LoadSettingsFiles()
        {
        	try
        	{
        		
        	}
        	catch(Exception ex){LogError(ex);}
        }
        
        

        

    }
}

