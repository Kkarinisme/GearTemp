/*
 * Created by SharpDevelop.
 * User: Paul
 * Date: 12/27/2012
 * Time: 8:06 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Drawing;
using Decal.Adapter;
using Decal.Filters;
using Decal.Adapter.Wrappers;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace GearFoundry
{

	public partial class PluginCore
	{
		
		//ToMish:  Need to add the following to Settings!
		//ToMish:  Standard treatment with bEnableTextFiltering.  Call SubscribeChatEvents() on startup if enabled.  Call SubscribeChatEvents when enabled
		//ToMish:  Call UnsubscribeChatEvents() when disabled.
		
		private bool bEnableTextFiltering = false;	//Enable/Disable the module
		private bool bTextFilterAllStatus = false;  //Removes all red status messages from the display area (overrides Busy and Casting filters)
		private bool bTextFilterBusyStatus = false;  //Removes only the busy status message from the display area
		private bool bTextFilterCastingStatus = false;  //Removes the "you cast" red message from the display area	
		private bool bTextFilterMyDefenseMessages = false;  //You evade, resist, dodge, etc.
		private bool bTextFilterMobDefenseMessages = false;  //The mob evades, resists, ect.
		private bool bTextFilterMyKillMessages = false;  //Kill spam for things killed by the player
		private bool bTextFilterPKFails = false;  //Filter Spell Fails from PK/NPK status errors
		private bool bTextFilterDirtyFighting = false;  //Dirty Fighting, Sneak Attack, etc.
		private bool bTextFilterMySpellCasting = false;
		private bool bTextFilterOthersSpellCasting = false;
		private bool bTextFilterSpellExpirations = false;
		private bool bTextFilterManaStoneMessages = false;
		private bool bTextFilterHealingMessages = false;
		private bool bTextFilterSalvageMessages = false;
		private bool bTextFilterBotSpam = false;
		private bool bTextFilterIdentFailures = false;
		private bool bTextFilterKillTaskComplete = false;
		private bool bTextFilterVendorTells = false;
		private bool bTextFilterMonsterTells = false;
		private bool bTextFilterNPCChatter = false;
		

		private void SubscribeChatEvents()
		{
			try
			{
				BuildTextCollections();
				Core.ChatBoxMessage += new EventHandler<ChatTextInterceptEventArgs>(ChatBoxTextMessage);
				Host.Underlying.Hooks.StatusTextIntercept += new Decal.Interop.Core.IACHooksEvents_StatusTextInterceptEventHandler(StatusTextMessage);			
			}catch(Exception ex){LogError(ex);}
		}

		private void UnsubscribeChatEvents()
		{
			try
			{
				Core.ChatBoxMessage -= new EventHandler<ChatTextInterceptEventArgs>(ChatBoxTextMessage);
				Host.Underlying.Hooks.StatusTextIntercept -= new Decal.Interop.Core.IACHooksEvents_StatusTextInterceptEventHandler(StatusTextMessage);	
			}catch(Exception ex){LogError(ex);}
		}
		
		// Text filter sourced from Mag-Tools.
		// Thanks to Mag-nus for the excellent chat filtering using RegEx. His code definitions are used heavily below.
		// Sorry Mag-nus.  I just can't leave anything alone.....I heavily repurposed your Regex expressions.
		// For original Mag-Tools source code, go to http://http://magtools.codeplex.com/
			
		private static Collection<Regex> ChatTypes = new Collection<Regex>();	
		private static List<string> CastWords = new List<string>();
		private static List<int> ChatChannelPass = new List<int>();

		private void BuildTextCollections()
		{
			ChatTypes.Add(new Regex("^You say, \"(?<msg>.*)\"$"));
			ChatTypes.Add(new Regex("^<Tell:IIDString:[0-9]+:(?<name>[\\w\\s'-]+)>[\\w\\s'-]+<\\\\Tell> says, \"(?<msg>.*)\"$"));
			ChatTypes.Add(new Regex("^(?<name>[\\w\\s'-]+) says, \"(?<msg>.*)\"$"));
			ChatTypes.Add(new Regex("^\\[(?<channel>.+)]+ <Tell:IIDString:[0-9]+:(?<name>[\\w\\s'-]+)>[\\w\\s'-]+<\\\\Tell> says, \"(?<msg>.*)\"$"));
			ChatTypes.Add(new Regex("^You tell .+, \"(?<msg>.*)\"$"));
			ChatTypes.Add(new Regex("^<Tell:IIDString:[0-9]+:(?<name>[\\w\\s'-]+)>[\\w\\s'-]+<\\\\Tell> tells you, \"(?<msg>.*)\"$"));
			ChatTypes.Add(new Regex("^(?<name>[\\w\\s'-]+) tells you, \"(?<msg>.*)\"$"));	
			
			CastWords.Add(", \"Zojak");
			CastWords.Add(", \"Malar");
			CastWords.Add(", \"Puish");
			CastWords.Add(", \"Curath");
			CastWords.Add(", \"Volae");
			CastWords.Add(", \"Quavosh");
			CastWords.Add(", \"Shurov");
			CastWords.Add(", \"Boquar");
			CastWords.Add(", \"Helkas");
			CastWords.Add(", \"Equin");
			CastWords.Add(", \"Roiga");
			CastWords.Add(", \"Malar");
			CastWords.Add(", \"Jevak");
			CastWords.Add(", \"Tugak");
			CastWords.Add(", \"Slavu");
			CastWords.Add(", \"Drostu");
			CastWords.Add(", \"Traku");
			CastWords.Add(", \"Yanoi");
			CastWords.Add(", \"Drosta");
			CastWords.Add(", \"Feazh");	

			ChatChannelPass.Add(0);
			ChatChannelPass.Add(1);
			ChatChannelPass.Add(2);
			ChatChannelPass.Add(3);
			ChatChannelPass.Add(4);
			ChatChannelPass.Add(5);
			ChatChannelPass.Add(8);
			ChatChannelPass.Add(9);
			ChatChannelPass.Add(10);
			ChatChannelPass.Add(11);
			ChatChannelPass.Add(12);
			ChatChannelPass.Add(18);
			ChatChannelPass.Add(19);
			ChatChannelPass.Add(24);
			ChatChannelPass.Add(27);
			ChatChannelPass.Add(28);
			ChatChannelPass.Add(29);
			ChatChannelPass.Add(30);
			
			
			//0: Green: (Gameplay) Many system messages including death, quest, motd, friends, etc. 
			//1: Green: (Mandatory) 
			//2: White: (Area Chat) Local chat 
			//3: Yellow: (Tells) Tell to someone 
			//4: Dim Yellow: (Tells) Tell from someone 
			//5: Purple: (Gameplay) 
			//6: Red: (Gameplay/Combat) Impact damage 
			//7: Blue: (Magic) Spell cast, fizzle, resist, spell burn, vitae, cast recall text, attuning to lifestone 
			//8: Peach: (Mandatory) 
			//9: Peach: (Mandatory) 
			//10: Yellow: (Allegiance) Alleg broadcasts, To/From Co-Vassals 
			//11: Dim Yellow: (Allegiance) To vassals 
			//12: Grey: (Area Chat) Emotes 
			//13: Turquoise: (Gameplay) Stat/Attribute raises, level up related 
			//14: Light Blue: (Mandatory) 
			//15: Red: (Mandatory) 
			//16: Green: (Gameplay) Failed to assess 
			//17: Blue: (Magic) Spell words 
			//18: Orange: (Allegiance) Allegiance channel 
			//19: Yellow: (Fellowship) Fellow To/From, Other Fellow Messages 
			//20: Green: (Gameplay) House Maintenance 
			//21: Red: (Combat) Melee/Missle messages to you from attacker 
			//22: Pink: (Combat) Melee/Missle messages from you attacking 
			//23: Green: (Gameplay) Commandline Recalls 
			//24: Green: (Gameplay) Tinker attempts 
			//25: Green: (Gameplay) 
			//26: Red: (Errors) Errors, Status text 
			//27: Light Blue: (General Channel) General channel 
			//28: Light Blue: (Trade Channel) Trade channel 
			//29: Light Blue: (LFG Channel) LFG channel 
			//30: Light Blue: (Roleplay Channel) Roleplay channel 
		}

		private void ChatBoxTextMessage(object sender, ChatTextInterceptEventArgs e)
		{
			try
			{
				if(ChatChannelPass.Contains(e.Color)) {return;}
				else if(bEnableTextFiltering)
				{
					e.Eat = true;
					return;
				}

			}catch(Exception ex){LogError(ex);}
		}
			
		private void StatusTextMessage(string StatusText, ref bool bEat)
		{
			try
			{
				if(!bEnableTextFiltering) {return;}
				else
				{
					bEat = true;
					return;
				}
			}catch(Exception ex){LogError(ex);}
			
		}
			
	}
}
