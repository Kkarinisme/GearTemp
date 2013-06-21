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
			
		private static Collection<Regex> MyDefenseMessages = new Collection<Regex>();
		private static Collection<Regex> MobDefenseMessages = new Collection<Regex>();
		private static Collection<Regex> MyAttackMessages = new Collection<Regex>();
		private static Collection<Regex> MobAttackMessages = new Collection<Regex>();
		private static Collection<Regex> TargetKilledByMe = new Collection<Regex>();
		private static Collection<Regex> ChatTypes = new Collection<Regex>();	
		private static List<string> CastWords = new List<string>();

		private void BuildTextCollections()
		{
			MyDefenseMessages.Add(new Regex("^You evaded (?<targetname>.+)!$"));
			MyDefenseMessages.Add(new Regex("^You resist the spell cast by (?<targetname>.+)$"));
			
			MobDefenseMessages.Add(new Regex("^(?<targetname>.+) evaded your attack.$"));
			MobDefenseMessages.Add(new Regex("^(?<targetname>.+) resists your spell!$"));
			MobDefenseMessages.Add(new Regex("^(?<targetname>.+) resists your spell$"));
	
			MyAttackMessages.Add(new Regex("^Critical hit!  You [\\w]+ (?<targetname>.*) for (?<points>.+) point.* of .+ damage.*$"));
			MyAttackMessages.Add(new Regex("^You [\\w]+ (?<targetname>.*) for (?<points>.+) point.* of .+ damage.*$"));
			MyAttackMessages.Add(new Regex("^Critical hit! You [\\w]+ (?<targetname>.+) for (?<points>.+) point.* with .+$"));
			MyAttackMessages.Add(new Regex("^You [\\w]+ (?<targetname>.+) for (?<points>.+) point.* with .+$"));

			MobAttackMessages.Add(new Regex("^Critical hit! (?<targetname>.+) [\\w]+ you for (?<points>.+) point.* with .+$"));
			MobAttackMessages.Add(new Regex("^(?<targetname>.+) [\\w]+ you for (?<points>.+) point.* with .+$"));
			MobAttackMessages.Add(new Regex("^Magical energies lose (?<points>.+) point.* of health due to (?<targetname>.+) casting .+$"));
			MobAttackMessages.Add(new Regex("^You lose (?<points>.+) point.* of health due to (?<targetname>.+) casting .+$"));
			MobAttackMessages.Add(new Regex("^(?<targetname>.+) casts .+ and drains (?<points>.+) point.* .+$"));
			MobAttackMessages.Add(new Regex("^Critical hit! (?<targetname>.+) [\\w]+ your .+ for (?<points>.*) point.* of .+ damage.*$"));
			MobAttackMessages.Add(new Regex("^(?<targetname>.+) [\\w]+ your .+ for (?<points>.+) point.* of .+ damage.*$"));

			TargetKilledByMe.Add(new Regex("^You flatten (?<targetname>.+)'s body with the force of your assault!$"));
			TargetKilledByMe.Add(new Regex("^You bring (?<targetname>.+) to a fiery end!$"));
			TargetKilledByMe.Add(new Regex("^You beat (?<targetname>.+) to a lifeless pulp!$"));
			TargetKilledByMe.Add(new Regex("^You smite (?<targetname>.+) mightily!$"));
			TargetKilledByMe.Add(new Regex("^You obliterate (?<targetname>.+)!$"));
			TargetKilledByMe.Add(new Regex("^You run (?<targetname>.+) through!$"));
			TargetKilledByMe.Add(new Regex("^You reduce (?<targetname>.+) to a sizzling, oozing mass!$"));
			TargetKilledByMe.Add(new Regex("^You knock (?<targetname>.+) into next Morningthaw!$"));
			TargetKilledByMe.Add(new Regex("^You split (?<targetname>.+) apart!$"));
			TargetKilledByMe.Add(new Regex("^You cleave (?<targetname>.+) in twain!$"));
			TargetKilledByMe.Add(new Regex("^You slay (?<targetname>.+) viciously enough to impart death several times over!$"));
			TargetKilledByMe.Add(new Regex("^You reduce (?<targetname>.+) to a drained, twisted corpse!$"));
			TargetKilledByMe.Add(new Regex("^Your killing blow nearly turns (?<targetname>.+) inside-out!$"));
			TargetKilledByMe.Add(new Regex("^Your attack stops (?<targetname>.+) cold!$"));
			TargetKilledByMe.Add(new Regex("^Your lightning coruscates over (?<targetname>.+)'s mortal remains!$"));
			TargetKilledByMe.Add(new Regex("^Your assault sends (?<targetname>.+) to an icy death!$"));
			TargetKilledByMe.Add(new Regex("^You killed (?<targetname>.+)!$"));
			TargetKilledByMe.Add(new Regex("^The thunder of crushing (?<targetname>.+) is followed by the deafening silence of death!$"));
			TargetKilledByMe.Add(new Regex("^The deadly force of your attack is so strong that (?<targetname>.+)'s ancestors feel it!$"));
			TargetKilledByMe.Add(new Regex("^(?<targetname>.+)'s seared corpse smolders before you!$"));
			TargetKilledByMe.Add(new Regex("^(?<targetname>.+) is reduced to cinders!$"));
			TargetKilledByMe.Add(new Regex("^(?<targetname>.+) is shattered by your assault!$"));
			TargetKilledByMe.Add(new Regex("^(?<targetname>.+) catches your attack, with dire consequences!$"));
			TargetKilledByMe.Add(new Regex("^(?<targetname>.+) is utterly destroyed by your attack!$"));
			TargetKilledByMe.Add(new Regex("^(?<targetname>.+) suffers a frozen fate!$"));
			TargetKilledByMe.Add(new Regex("^(?<targetname>.+)'s perforated corpse falls before you!$"));
			TargetKilledByMe.Add(new Regex("^(?<targetname>.+) is fatally punctured!$"));
			TargetKilledByMe.Add(new Regex("^(?<targetname>.+)'s death is preceded by a sharp, stabbing pain!$"));
			TargetKilledByMe.Add(new Regex("^(?<targetname>.+) is torn to ribbons by your assault!$"));
			TargetKilledByMe.Add(new Regex("^(?<targetname>.+) is liquified by your attack!$"));
			TargetKilledByMe.Add(new Regex("^(?<targetname>.+)'s last strength dissolves before you!$"));
			TargetKilledByMe.Add(new Regex("^Electricity tears (?<targetname>.+) apart!$"));
			TargetKilledByMe.Add(new Regex("^Blistered by lightning, (?<targetname>.+) falls!$"));
			TargetKilledByMe.Add(new Regex("^(?<targetname>.+)'s last strength withers before you!$"));
			TargetKilledByMe.Add(new Regex("^(?<targetname>.+) is dessicated by your attack!$"));
			TargetKilledByMe.Add(new Regex("^(?<targetname>.+) is incinerated by your assault!$"));
					
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
		}

		private void ChatBoxTextMessage(object sender, ChatTextInterceptEventArgs e)
		{
			try
			{
				if(e.Eat || string.IsNullOrEmpty(e.Text)) {return;}
				if(!bEnableTextFiltering) {return;}
				
				if(ChatTypes.Any(x => x.IsMatch(e.Text)))
				{
					if(bTextFilterMySpellCasting && e.Text.StartsWith("You say, "))
					{
						if(CastWords.Any(x => e.Text.Contains(x))){e.Eat = true;}
					}
					if (bTextFilterOthersSpellCasting && e.Text.Contains("says, "))
					{
					    if(CastWords.Any(x => e.Text.Contains(x))){e.Eat = true;}
					}
					if(bTextFilterBotSpam)
					{
						if(e.Text.Trim().EndsWith("-t-\"") || e.Text.Trim().EndsWith("-b-\"")) {e.Eat = true;}
					}
					if(bTextFilterVendorTells)
					{
						if(Core.WorldFilter.GetByObjectClass(ObjectClass.Vendor).ToList().Any(x => e.Text.StartsWith(x.Name))) {e.Eat = true;}
					}
					if (bTextFilterMonsterTells)
					{
						if(Core.WorldFilter.GetByObjectClass(ObjectClass.Monster).ToList().Any(x => e.Text.StartsWith(x.Name))) {e.Eat = true;}
					}
					if(bTextFilterNPCChatter)
					{
						if(Core.WorldFilter.GetByObjectClass(ObjectClass.Npc).ToList().Any(x => e.Text.StartsWith(x.Name))) {e.Eat = true;}
					}		
				}
				else
				{
					if(bTextFilterMobDefenseMessages)
					{
						if(MobDefenseMessages.Any(x => x.IsMatch(e.Text))) {e.Eat = true;}
					}
					if(bTextFilterMyDefenseMessages)
					{
						if(MyDefenseMessages.Any(x => x.IsMatch(e.Text))) {e.Eat = true;}
					}
					if(bTextFilterMyKillMessages)
					{
						if(TargetKilledByMe.Any(x => x.IsMatch(e.Text))) {e.Eat = true;}
					}
					if(bTextFilterPKFails)
					{
						if(e.Text.StartsWith("You fail to affect ") && e.Text.Contains(" you are not a player killer!")){e.Eat = true;}
						if(e.Text.Contains("fails to affect you") && e.Text.Contains(" is not a player killer!")) {e.Eat = true;}
					}
					if(bTextFilterDirtyFighting)
					{
						if(e.Text.StartsWith("Dirty Fighting! ") && e.Text.Contains(" delivers a ")) {e.Eat = true;}
					}
					if(bTextFilterMySpellCasting)
					{
						if(e.Text.StartsWith("Your spell fizzled.")) {e.Eat = true;}
						if (e.Text.StartsWith("The spell consumed the following components")) {e.Eat = true;}
					}
					if(bTextFilterSpellExpirations)
					{
						if (!e.Text.Contains("Brilliance") && !e.Text.Contains("Prodigal") && !e.Text.Contains("Spectral"))
						{
							if (e.Text.Contains("has expired.") || e.Text.Contains("have expired.")) {e.Eat = true;}
						}
					}
					if(bTextFilterHealingMessages)
					{
						if(e.Text.StartsWith("You ") && e.Text.Contains(" heal yourself for ")) {e.Eat = true;}
						if(e.Text.StartsWith("You fail to heal yourself. ")) {e.Eat = true;}
					}
					if(bTextFilterSalvageMessages)
					{
						if(e.Text.StartsWith("You obtain ") && e.Text.Contains(" using your knowledge of ")) {e.Eat = true;}
						if (e.Text.StartsWith("Salvaging Failed!")) {e.Eat = true;}
						if (e.Text.Contains("The following were not suitable for salvaging: ")) {e.Eat = true;}
					}
					if(bTextFilterManaStoneMessages)
					{
						if(e.Text.StartsWith("The Mana Stone gives ")) {e.Eat = true;}
						if(e.Text.StartsWith("You need ") && e.Text.Trim().EndsWith(" more mana to fully charge your items.")) {e.Eat = true;}
						if(e.Text.StartsWith("The Mana Stone drains ")) {e.Eat = true;}
						if(e.Text.StartsWith("The ") && e.Text.Trim().EndsWith(" is destroyed.")) {e.Eat = true;}
					}
					if(bTextFilterBotSpam)
					{
						if(e.Text.Trim().EndsWith("-t-") || e.Text.Trim().EndsWith("-b-")) {e.Eat = true;}
					}
					if(bTextFilterIdentFailures)
					{
						if(e.Text.Trim().EndsWith("tried and failed to assess you!")) {e.Eat = true;}
					}
					if(bTextFilterKillTaskComplete)
					{
						if (e.Text.StartsWith("You have killed ") && e.Text.Trim().EndsWith("Your task is complete!")) {e.Eat = true;}
					}	
				}	
			}catch(Exception ex){LogError(ex);}
		}
		
		private void StatusTextMessage(string StatusText, ref bool bEat)
		{
			try
			{
				if(!bEnableTextFiltering || StatusText == String.Empty) {return;}
				if (bTextFilterAllStatus) {bEat = true;}
				if (bTextFilterBusyStatus && !bEat && StatusText == "You're too busy!") {bEat = true;}
				if (bTextFilterCastingStatus && !bEat && StatusText.StartsWith("Casting ")){bEat = true;}
			}catch(Exception ex){LogError(ex);}
			
		}
			
	}
}
