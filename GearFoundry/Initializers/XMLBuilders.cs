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
		
	
		private void BuildKillTaskList()
		{
			List<KillTask> NewKillTasks = new List<PluginCore.KillTask>();
			
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
			NewKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Drudge Stalker Kill Task";
			t.MobNames.Add("Drudge Stalker");
			t.CompleteCount = 100;
			t.NPCNames.Add("Jarvis Hammerstone");
			t.NPCInfo = "Cragstone";
			t.NPCCoords = "25.6N, 49.4E";
			t.NPCYellowFlagText = "In the meantime I need to rid the surrounding area of some of these Drudges.";
			t.NPCYellowCompleteText = "Stalker Stalker";
			NewKillTasks.Add(t);

			t = new KillTask();
			t.TaskName = "Drudge Ravener Kill Task";
			t.MobNames.Add("Drudge Ravener");
			t.CompleteCount = 100;
			t.NPCNames.Add("Jarvis Hammerstone");
			t.NPCInfo = "Cragstone";
			t.NPCCoords = "25.6N, 49.4E";
			t.NPCYellowFlagText = "In the meantime I need to rid the surrounding area of some of these Drudges.";
			t.NPCYellowCompleteText = "Ravenous";
			NewKillTasks.Add(t);
						
			t = new KillTask();
			t.TaskName = "Altered Drudge Kill Task";
			t.MobNames.Add("Altered Drudge");
			t.CompleteCount = 40;
			t.NPCNames.Add("Jarvis Hammerstone");
			t.NPCInfo = "Cragstone";
			t.NPCCoords = "25.6N, 49.4E";
			t.NPCYellowFlagText = "In the meantime I need to rid the surrounding area of some of these Drudges.";
			t.NPCYellowCompleteText = "Altered Hunter";
			NewKillTasks.Add(t);	

			t = new KillTask();
			t.TaskName = "Augmented Drudge Kill Task";
			t.MobNames.Add("Augmented Drudge");
			t.CompleteCount = 40;
			t.NPCNames.Add("Jarvis Hammerstone");
			t.NPCInfo = "Cragstone";
			t.NPCCoords = "25.6N, 49.4E";
			t.NPCYellowFlagText = "In the meantime I need to rid the surrounding area of some of these Drudges.";
			t.NPCYellowCompleteText = "Augmented Hunter";
			NewKillTasks.Add(t);	

			
			//Neftet Quests		
			t = new KillTask();
			t.TaskName = "Armoredillo Hunting (Neftet)";
			t.MobNames.Add("Guardian Armoredillo");
			t.MobNames.Add("Tamed Armoredillo");
			t.MobNames.Add("War Armoredillo");			
			t.CompleteCount = 30;
			t.NPCNames.Add("Dame Tularin");
			t.NPCInfo = "Neftet";
			t.NPCCoords = "22.1S, 6.3E";
			t.NPCYellowFlagText = "If you will kill 30 of the armoredillos within the canyon walls or up on the plateaus, I will reward you for your help.";
			t.NPCYellowCompleteText = "Well done.";
			NewKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Desert Cactus Kill Task (Neftet)";
			t.MobNames.Add("Desert Cactus");
			t.CompleteCount = 6;
			t.NPCNames.Add("Sir Unell bin Rakke");
			t.NPCInfo = "Neftet Encampment";
			t.NPCCoords = "22.2S, 6.1E";
			t.NPCYellowFlagText = "Destroy 6 of the Desert Cactus and I will reward your efforts.";
			t.NPCYellowCompleteText = "The less insects around here, the better!";
			NewKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Golem Hunting (Neftet)";
			t.MobNames.Add("Burning Sands Golem");
			t.MobNames.Add("Dust Golem");
			t.CompleteCount = 15;
			t.NPCNames.Add("Sir Ibreh bin Kassim");
			t.NPCInfo = "Encampment near Neftet";
			t.NPCCoords = "22.2S, 6.2E";
			t.NPCYellowFlagText = "If you will kill 15 of the golems within the canyon walls or up on the plateaus, I will reward you for your assistance.";
			t.NPCYellowCompleteText = "Well done.";
			NewKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Mumiyah Hunting (Neftet)";
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
			NewKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Reedshark Hunting (Neftet)";
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
			NewKillTasks.Add(t);
			
			//Frozen Valley Tasks	

			t = new KillTask();
			t.TaskName = "Frost Golem Kill Task (Frozen Valley)";
			t.MobNames.Add("Frost Golem");
			t.CompleteCount = 20;
			t.NPCNames.Add("George");
			t.NPCInfo = "Frozen Valley";
			t.NPCCoords = "83.8N, 4.3W";
			t.NPCYellowFlagText = "Watch your step and kill 20 Frost Golems";
			t.NPCYellowCompleteText = "Those giant beasts of ice fall all the more magnificently.";
			NewKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Frozen Crystal Kill Task (Frozen Valley)";
			t.MobNames.Add("Frozen Crystal");
			t.CompleteCount = 4;
			t.NPCNames.Add("Boone");
			t.NPCInfo = "Frozen Valley";
			t.NPCCoords = "83.7N, 4.3W";
			t.NPCYellowFlagText = "Destroy 4 of these Frozen Crystals and you will be rewarded.";
			t.NPCYellowCompleteText = "That was some fine destruction, my friend!";
			NewKillTasks.Add(t);			
			t = new KillTask();
			t.TaskName = "Gurog Soldier Kill Task (Frozen Valley)";
			t.MobNames.Add("Gurog Soldier");
			t.CompleteCount = 20;
			t.NPCNames.Add("Gregoria, Gurog Destroyer");
			t.NPCInfo = "Frozen Valley";
			t.NPCCoords = "83.8N 4.3W";
			t.NPCYellowFlagText = "Kill 20 Gurog Soldiers and I will reward you for your efforts.";
			t.NPCYellowCompleteText = "Well done! Always a pleasure to meet someone who shares my hatred of these beasts.";
			NewKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Gurog Minion Kill Task (Frozen Valley)";
			t.MobNames.Add("Gurog Minion");
			t.CompleteCount = 20;
			t.NPCNames.Add("Gregoria, Gurog Destroyer");
			t.NPCInfo = "Frozen Valley";
			t.NPCCoords = "83.8N 4.3W";
			t.NPCYellowFlagText = "Kill 20 Gurog Minions and I will reward you for your efforts.";
			t.NPCYellowCompleteText = "Well done! Ugly creatures aren't they? Glad to be rid of them.";
			NewKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Gurog Henchman Kill Task (Frozen Valley)";
			t.MobNames.Add("Gurog Henchmen");
			t.CompleteCount = 20;
			t.NPCNames.Add("Gregoria, Gurog Destroyer");
			t.NPCInfo = "Frozen Valley";
			t.NPCCoords = "83.8N 4.3W";
			t.NPCYellowFlagText = "Kill 20 Gurog Henchmen and I will reward you for your efforts.";
			t.NPCYellowCompleteText = "Well done! Hardly a scratch on you as well, you truly are a great warrior.";
			NewKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Snow Tusker Kill Task (Frozen Valley)";
			t.MobNames.Add("Snow Tusker");
			t.CompleteCount = 20;
			t.NPCNames.Add("Hunter");
			t.NPCInfo = "Frozen Valley";
			t.NPCCoords = "83.8N 4.3W";
			t.NPCYellowFlagText = "Kill 20 Snow Tuskers and I will reward you for your efforts.";
			t.NPCYellowCompleteText = "Fantastic job! Those mutated beasts need to be put down, every kill helps.";	
			NewKillTasks.Add(t);

			t = new KillTask();
			t.TaskName = "Snow Tusker Warrior Kill Task (Frozen Valley)";
			t.MobNames.Add("Snow Tusker Warrior");
			t.CompleteCount = 20;
			t.NPCNames.Add("Hunter");
			t.NPCInfo = "Frozen Valley";
			t.NPCCoords = "83.8N 4.3W";
			t.NPCYellowFlagText = "Kill 20 Snow Tusker Warriors and I will reward you for your efforts.";
			t.NPCYellowCompleteText = "Well done! Their coats remind me of the creature know as a wolf back on Ispar.";
			NewKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Snow Tusker Leader Kill Task (Frozen Valley)";
			t.MobNames.Add("Snow Tusker Leader");
			t.CompleteCount = 10;
			t.NPCNames.Add("Hunter");
			t.NPCInfo = "Frozen Valley";
			t.NPCCoords = "83.8N 4.3W";
			t.NPCYellowFlagText = "Kill 10 Snow Tusker Leaders and I will reward you for your efforts.";
			t.NPCYellowCompleteText = "Amazing that you survived, those tusks can spear a man all the way through.";
			NewKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Frozen Dread Kill Task (Frozen Valley)";
			t.MobNames.Add("Frozen Dread");
			t.CompleteCount = 10;
			t.NPCNames.Add("Kumiko");
			t.NPCInfo = "Frozen Valley";
			t.NPCCoords = "83.8N 4.3W";
			t.NPCYellowFlagText = "Kill 10 of these Frozen Dreads and I will reward you for your efforts.";
			t.NPCYellowCompleteText = "Those creatures haunt my dreams.";
			NewKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Frozen Wight Kill Task (Frozen Valley)";
			t.MobNames.Add("Frozen Wight");
			t.MobNames.Add("Frozen Wight Captain");
			t.MobNames.Add("Frozen Wight Sorcerer");
			t.MobNames.Add("Frozen Wight Archer");
			t.CompleteCount = 20;
			t.NPCNames.Add("Leilah");
			t.NPCInfo = "Frozen Valley";
			t.NPCCoords = "83.8N 4.3W";
			t.NPCYellowFlagText = "Kill 20 Frozen Wights and I will reward you for your efforts.";
			t.NPCYellowCompleteText = "Those creatures send chills down my spine.";
			NewKillTasks.Add(t);
			
			//Tou-Tou Kill Tasks
			t = new KillTask();
			t.TaskName = "Shadow Flyer Kill Task (Tou-Tou)";
			t.MobNames.Add("Shadow Flyer");
			t.CompleteCount = 15;
			t.NPCNames.Add(" Umbral Guard");
			t.NPCInfo = "Tou-Tou";
			t.NPCCoords = "30.3S, 94.8E";
			t.NPCYellowFlagText = "Kill 15 Shadow Flyers and I will reward you for your efforts.";
			t.NPCYellowCompleteText = "Well done! That should at least keep the corruption from spreading any further.";
			NewKillTasks.Add(t);
			
			//TODO:  Names of proper shadows for this task
			t = new KillTask();
			t.TaskName = "Shadow Kill Task (Tou-Tou)";
			t.MobNames.Add("Shadow");
			t.CompleteCount = 25;
			t.NPCNames.Add("Umbral Guard");
			t.NPCInfo = "Tou-Tou";
			t.NPCCoords = "30.3S, 94.8E";
			t.NPCYellowFlagText = "Kill 25 of the shadows to fight back this corruption.";
			t.NPCYellowCompleteText = "Well done! If you keep this up, Tou-Tou may be ours once again.";
			NewKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Devourer Margul Kill Task (Tou-Tou)";
			t.MobNames.Add("Devourer Margul");
			t.CompleteCount = 15;
			t.NPCNames.Add("Umbral Guard");
			t.NPCInfo = "Tou-Tou";
			t.NPCCoords = "30.3S, 94.8E";
			t.NPCYellowFlagText = "Kill 15 Devourer Marguls and I will reward you for your efforts.";
			t.NPCYellowCompleteText = "Well done! The flapping of those leathery wings is quieter already.";
			NewKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Grievver Shredder Kill Task (Tou-Tou)";
			t.MobNames.Add("Grievver Shredder");
			t.CompleteCount = 15;
			t.NPCNames.Add("Royal Guard");
			t.NPCInfo = "Tou-Tou";
			t.NPCCoords = "30.3S, 94.8E";
			t.NPCYellowFlagText = "Kill 15 Grievver Shredders and I will reward you for your efforts.";
			t.NPCYellowCompleteText = "Well done! The clicking of those ";
			NewKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Void Lord Kill Task (Tou-Tou)";
			t.MobNames.Add("Void Lord");
			t.CompleteCount = 15;
			t.NPCNames.Add("Royal Guard");
			t.NPCInfo = "Tou-Tou";
			t.NPCCoords = "30.3S, 94.8E";
			t.NPCYellowFlagText = "Kill 15 Void Lords and I will reward you for your efforts.";
			t.NPCYellowCompleteText = "Well done! That should help me sleep at night.";
			NewKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Mud Golem Sludge Lord (Aun Golem Hunters)";
			t.MobNames.Add("Mud Golem Sludge Lord");
			t.CompleteCount = 5;
			t.NPCNames.Add("Aun Akuarea");
			t.NPCInfo = "(near Samsur)";
			t.NPCCoords = "2.6S, 20.0E";
			t.NPCYellowFlagText = "Kill five of these golems, return to me and I will see that your battles are rewarded!";
			t.NPCYellowCompleteText = "Excellent! Your keh burns brightly with victory!";
			NewKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Copper Golem Kingpin (Aun Golem Hunters)";
			t.MobNames.Add("Copper Golem Kingpin");
			t.CompleteCount = 5;
			t.NPCNames.Add("Aun Tiulerea");
			t.NPCInfo = "Eastham";
			t.NPCCoords = "16.5N, 63.6E";
			t.NPCYellowFlagText = "Kill five of these golems, return to me and I will see that your battles are rewarded!";
			t.NPCYellowCompleteText = "Excellent! Your keh burns brightly with victory!";
			NewKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Glacial Golem Margrave (Aun Golem Hunters)";
			t.MobNames.Add("Glacial Golem Margrave");
			t.CompleteCount = 5;
			t.NPCNames.Add("Aun Maerirea");
			t.NPCInfo = "Qalaba'r";
			t.NPCCoords = "74.5S, 19.3E";
			t.NPCYellowFlagText = "Kill five of these golems, return to me and I will see that your battles are rewarded!";
			t.NPCYellowCompleteText = "Excellent! Your keh burns brightly with victory!";
			NewKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Magma Golem Exarch (Aun Golem Hunters)";
			t.MobNames.Add("Magma Golem Exarch");
			t.CompleteCount = 5;
			t.NPCNames.Add("Aun Autuorea");
			t.NPCInfo = "Candeth Keep";
			t.NPCCoords = "87.5S, 67.0W";
			t.NPCYellowFlagText = "Kill five of these golems, return to me and I will see that your battles are rewarded!";
			t.NPCYellowCompleteText = "Excellent! Your keh burns brightly with victory!";
			NewKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Coral Golem Viceroy (Aun Golem Hunters)";
			t.MobNames.Add("Coral Golem Viceroy");
			t.CompleteCount = 5;
			t.NPCNames.Add("Aun Aukherea");
			t.NPCInfo = "Ayan Baqur";
			t.NPCCoords = "60.8S, 88.0W";
			t.NPCYellowFlagText = "Kill five of these golems, return to me and I will see that your battles are rewarded!";
			t.NPCYellowCompleteText = "Excellent! Your keh burns brightly with victory!";
			NewKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Platinum Golem Mountain King (Aun Golem Hunters)";
			t.MobNames.Add("Platinum Golem Mountain King");
			t.CompleteCount = 5;
			t.NPCNames.Add("Aun Khekierea");
			t.NPCInfo = "Ayan Baqur";
			t.NPCCoords = "60.8S, 88.0W";
			t.NPCYellowFlagText = "Kill five of these golems, return to me and I will see that your battles are rewarded!";
			t.NPCYellowCompleteText = "Excellent! Your keh burns brightly with victory!";
			NewKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Crystal Lord (Aun Golem Hunters)";
			t.MobNames.Add("Crystal Lord");
			t.CompleteCount = 1;
			t.NPCNames.Add("Aun Tahuirea");
			t.NPCInfo = "Camp";
			t.NPCCoords = "24.0N, 72.0W";
			t.NPCYellowFlagText = "Also, if you and your fellows succeed in defeating one, I will be pleased to share with you the bounties I have recovered from my previous victories.";
			t.NPCYellowCompleteText = "Your tale was truly one of triumph!";
			NewKillTasks.Add(t);
						
			t = new KillTask();
			t.TaskName = "Moarsmen Prisoners (Freebooter)";
			t.MobNames.Add("Moarsman Prisoner");
			t.CompleteCount = 20;  
			t.NPCNames.Add("Avarin");
			t.NPCInfo = "Freebooter Isle";
			t.NPCCoords = "56.4S 96.9E";
			t.NPCYellowFlagText = "I'm authorized to pay a bounty for culling the population of escaped moarsmen prisoners by twenty.";
			t.NPCYellowCompleteText = "For culling the moarsman prisoner population here's the bounty you're owed.";
			NewKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Moarsmen Blessed Leader (Freebooter)";
			t.MobNames.Add("Large Blessed Moarsman");
			t.CompleteCount = 1;
			t.NPCNames.Add("Avarin");
			t.NPCInfo = "Freebooter Isle";
			t.NPCCoords = "56.4S 96.9E";
			t.NPCYellowFlagText = "I'm authorized to pay a bounty for the death of the Blessed moarsman leader.";
			t.NPCYellowCompleteText = "For putting down the Blessed leader here's the bounty you're owed.";
			NewKillTasks.Add(t);

			//Freebooter
			
			t = new KillTask();
			t.TaskName = "Moarsmen Ardent Leader (Freebooter)";
			t.MobNames.Add("Large Ardent Moarsman");
			t.CompleteCount = 1;  
			t.NPCNames.Add("Avarin");
			t.NPCInfo = "Freebooter Isle";
			t.NPCCoords = "56.4S 96.9E";
			t.NPCYellowFlagText = "I'm authorized to pay a bounty for the death of the Ardent moarsman leader.";
			t.NPCYellowCompleteText = "For putting down the Ardent leader here's the bounty you're owed.";
			NewKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Moarsmen Verdant Leader (Freebooter)";
			t.MobNames.Add("Large Verdant Moarsman");
			t.CompleteCount = 1; 
			t.NPCNames.Add("Avarin");
			t.NPCInfo = "Freebooter Isle";
			t.NPCCoords = "56.4S 96.9E";
			t.NPCYellowFlagText = "I'm authorized to pay a bounty for the death of the Verdant moarsman leader.";
			t.NPCYellowCompleteText = "For putting down the Verdant leader here's the bounty you're owed.";
			NewKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Weeding of the Deru Tree (Freebooter)";
			t.MobNames.Add("Eyestalk of T'thuun");
			t.MobNames.Add("Tendril of T'thuun");
			t.MobNames.Add("Tentacle of T'thuun");
			t.CompleteCount = 50;
			t.NPCNames.Add("Valerian McGreggor");
			t.NPCInfo = "Freebooter Keep Black Market";
			t.NPCCoords = "64.0S 97.5E";
			t.NPCYellowFlagText = "I'll tell ye what. If ye go out there and kill 50 of those tentacles for me, just the ones on Freebooter, mind ye, I'll make it worth ye while.";
			t.NPCYellowCompleteText = "That should do it! The Mana flows around the Ruins of Degar'Alesh are moving much better now, thank ye. Here's a little something for yer efforts."; 
			NewKillTasks.Add(t);
			
			//TODO:  Complete Text
			t = new KillTask();
			t.TaskName = "Nyr'leha Sclavus Kill Task (Freebooter)";
			t.MobNames.Add("Afessa Sclavus Guardian");
			t.MobNames.Add("Afessa Sclavus Soldier");
			t.MobNames.Add("Illu Sclavus Soldier");
			t.MobNames.Add("Sclavus Attacker");
			t.MobNames.Add("Siessa Sclavus Soldier");
			t.CompleteCount = 20;
			t.NPCNames.Add("Bayani");
			t.NPCInfo = "Freebooter Keep Black Market";
			t.NPCCoords = "64.0S, 97.5E";
			t.NPCYellowFlagText = "I wish you to destroy 20 of the Sclavi who roam this isle";
			t.NPCYellowCompleteText = "";
			NewKillTasks.Add(t);
			
			//TODO:  Complete Text
			t = new KillTask();
			t.TaskName = "Nyr'leha Moarsman Kill Task (Freebooter)";
			t.MobNames.Add("Blighted Ardent Moarsman"); 
			t.MobNames.Add("Blighted Verdant Moarsman"); 
			t.MobNames.Add("Brood Mother"); 
			t.MobNames.Add("Icthis Moarsman"); 
			t.MobNames.Add("Magshuth Moarsman"); 
			t.MobNames.Add("Maguth Moarsman"); 
			t.MobNames.Add("Mithmog Moarsman"); 
			t.MobNames.Add("Moarsman Adherent of T'thuun"); 
			t.MobNames.Add("Moarsman Attacker"); 
			t.MobNames.Add("Moarsman Blight-caller"); 
			t.MobNames.Add("Moarsman Priest of T'thuun"); 
			t.MobNames.Add("Moarsman Prior"); 
			t.MobNames.Add("Mogshuth Moarsman"); 
			t.MobNames.Add("Moguth Moarsman"); 
			t.MobNames.Add("Shoguth Moarsman"); 
			t.MobNames.Add("Shuthis Moarsman"); 
			t.MobNames.Add("Spawn Watcher"); 
			t.MobNames.Add("Spawnling"); 
			t.MobNames.Add("Spawn"); 
			t.CompleteCount = 20;
			t.NPCNames.Add("Kagani");
			t.NPCInfo = "Freebooter Keep Black Market";
			t.NPCCoords = "64.0S, 97.5E";
			t.NPCYellowFlagText = "If you would rid us of say 20 of those beasts";
			t.NPCYellowCompleteText = "";
			NewKillTasks.Add(t);
			
			//Northwatch
			
			t = new KillTask();
			t.TaskName = "Black Coral Golem Kill Task (Northwatch)";
			t.MobNames.Add("Black Coral Golem");
			t.CompleteCount = 100;
			t.NPCNames.Add("Chiriko");
			t.NPCInfo = "Northwatch Castle Black Market";
			t.NPCCoords = "81.5N 25.0E";
			t.NPCYellowFlagText = "If you would be willing to go there and prove your prowess by destroying 100 of these strange golems, I will reward you handsomely for your actions.";
			t.NPCYellowCompleteText = "Well done, well done indeed. You have proven your skill and honored my task. I thank you. Here is the reward promised.";
			NewKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Blighted Coral Golem Kill Task";
			t.MobNames.Add("Blighted Coral Golem (Northwatch)");
			t.CompleteCount = 100;
			t.NPCNames.Add("Hanzo");
			t.NPCInfo = "Northwatch Castle Black Market";
			t.NPCCoords = "81.5N 25.0E";
			t.NPCYellowFlagText = "I wish you to destroy 100 of the foul, Blighted Coral Golems upon the isle.";
			t.NPCYellowCompleteText = "Impressively done, Honored Master. You have accomplished all I have wished from you. Now, for your promised reward.";
			NewKillTasks.Add(t);	
			
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
			NewKillTasks.Add(t);
			
			
			//Society 
			
			t = new KillTask();
			t.TaskName = "Blessed Moarsman Kill Task (Society)";
			t.MobNames.Add("Blessed Moarsman");
			t.CompleteCount = 50;
			t.NPCNames.Add("Kieran Stronghammer");
			t.NPCNames.Add("Alexander Bowspeaker");
			t.NPCNames.Add("Ian Foefinder");
			t.NPCInfo = "Society Stronghold";
			t.NPCCoords = "Unknown";
			t.NPCYellowFlagText = "Survive, kill 50 Blessed Moarsmen, and I'll make sure you are recognized for your valorous service to our Society.";
			t.NPCYellowCompleteText = "Congratulations, you survived and succeeded. Here, allow me to reward you for your assistance to our Society.";
			NewKillTasks.Add(t);			
			
			t = new KillTask();
			t.TaskName = "Killer Phyntos Wasp Kill Task (Society)";
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
			NewKillTasks.Add(t);
			
			
				
			t = new KillTask();
			t.TaskName = "Moguth Moarsman Kill Task (Society)";
			t.MobNames.Add("Moguth Moarsman");
			t.CompleteCount = 60;
			t.NPCNames.Add("Marconi di Bellenesse");
			t.NPCNames.Add("Ricaldo di Alduressa");
			t.NPCNames.Add("Berrando Piatelli");
			t.NPCInfo = "Society Stronghold";
			t.NPCCoords = "Unknown";
			t.NPCYellowFlagText = "Go to this city and kill 60 of the 'Moguth Moarsmen'.";
			t.NPCYellowCompleteText = "Well done! Here, allow me to reward you for your assistance to our Society.";
			NewKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Phyntos Larva Kill Task (Society)";
			t.MobNames.Add("Phyntos Larva");
			t.CompleteCount = 20;
			t.NPCNames.Add("Alderic");
			t.NPCNames.Add("Haruki");
			t.NPCNames.Add("Ghali al-Fariq");
			t.NPCInfo = "Society Stronghold";
			t.NPCCoords = "Unknown";
			t.NPCYellowFlagText = "In order to control the population of hyper aggressive Phyntos I'm contracting adventurers to kill their larvae.";
			t.NPCYellowCompleteText = "Excellent work infiltrating the Phyntos hive and killing their larvae.";
			NewKillTasks.Add(t);			
			
			t = new KillTask();
			t.TaskName = "Shoguth Moarsman Kill Task (Society)";
			t.MobNames.Add("Shoguth Moarsman");
			t.CompleteCount = 40;
			t.NPCNames.Add("Gavin Hammerstone");
			t.NPCNames.Add("Dorn Bowspeaker");
			t.NPCNames.Add("Kylos Hunterson");
			t.NPCInfo = "Society Stronghold";
			t.NPCCoords = "Unknown";
			t.NPCYellowFlagText = "Go to this city and kill 40 of the 'Shoguth Moarsmen'.";
			t.NPCYellowCompleteText = "Well done! Here, allow me to reward you for your assistance to our Society.";
			NewKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Wight Blade Sorcerer Kill Task (Society)";
			t.MobNames.Add("Wight Blade Sorcerer");
			t.CompleteCount = 12;
			t.NPCNames.Add("Mashira bint Tamur");
			t.NPCNames.Add("Zumaq al-Jaluzi");
			t.NPCNames.Add("Idaris bint Qumal");
			t.NPCInfo = "Society Stronghold";
			t.NPCCoords = "Unknown";
			t.NPCYellowFlagText = "Just concern yourself with killing 12 Wight Blade Sorcerers, and report back to me when you're done.";
			t.NPCYellowCompleteText = "Congratulations! Twelve dead Wight Blade Sorcerers. Our field researchers will be quite pleased. I can reward you now.";
			NewKillTasks.Add(t);
			
			//Mhoire
			
			t = new KillTask();
			t.TaskName = "Guardian Statue Kill Task (Mhoire)";
			t.MobNames.Add("Guardian Statue");
			t.CompleteCount = 10;
			t.NPCNames.Add("Shade of Fordroth");
			t.NPCInfo = "Mhoire Castle Northeast Tower";
			t.NPCCoords = "64.7S 45.2W";
			t.NPCYellowFlagText = "Destroy 10 of these corrupted gargoyles to ease the pain of those that wander these halls and I will reward you.";
			t.NPCYellowCompleteText = "Well done, champion. You must be skilled indeed. Allow me to reward you.";
			NewKillTasks.Add(t);

			//Graveyard			
			
			
			//TODO:  Completion Text
			t = new KillTask();
			t.TaskName = "Grave Rat Kill Task (Graveyard)";
			t.MobNames.Add("Grave Rat");
			t.CompleteCount = 100;
			t.NPCNames.Add("Lo Shoen");
			t.NPCInfo = "Graveyard";
			t.NPCCoords = "65.3S, 43.4W";
			t.NPCYellowFlagText = "If you want to help me, kill 100 of these Grave Rats. Maybe then I will have more work for you.";
			t.NPCYellowCompleteText = "";
			NewKillTasks.Add(t);

			//Gear Knight Kill Tasks
			//UNDONE:  complete messages may be reversed.
			t = new KillTask();
			t.TaskName = "Gear Knight Squire Kill Task (Resistance)";
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
			NewKillTasks.Add(t);			
			
			t = new KillTask();
			t.TaskName = "Gear Knight Knight Kill Task (Resistance)";
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
			NewKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Iron Blade Commander (Resistance)";
			t.MobNames.Add("Invading Iron Blade Commander");
			t.CompleteCount = 1;
			t.NPCNames.Add("Dame Trielle");
			t.NPCInfo = "Direlands Gear Knight Resistance Camp";
			t.NPCCoords = "12.3S 75.0W";
			t.NPCYellowFlagText = "Near the center of these invading forces, you'll find a Gear Knight called the Iron Blade Commander.";
			t.NPCYellowCompleteText = "I am pleased to see that you have been successful. Allow me to reward you for your assistance to the Crown.";
			NewKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Gold Gear Trooper Kill Task (Ijaniya)";
			t.MobNames.Add("Gold Gear Trooper");
			t.CompleteCount = 25;
			t.NPCNames.Add("Sir Stavitor");
			t.NPCInfo = "Yaraq to Ijaniya (22.4S, 0.2E)";
			t.NPCCoords = "33.4S, 6.3E";
			t.NPCYellowFlagText = "Return to me with anything you've learned after destroying 25 Gold Gear Troopers.";
			t.NPCYellowCompleteText = "Congratulations, you survived and succeeded. ";
			NewKillTasks.Add(t);
			
			
			t = new KillTask();
			t.TaskName = "Bronze Gauntlet Trooper Kill Task (Ijaniya)";
			t.MobNames.Add("Bronze Gauntlet Trooper");
			t.CompleteCount = 25;
			t.NPCNames.Add("Lieutenant Grenlin");
			t.NPCInfo = "Yaraq to Ijaniya (22.4S, 0.2E)";
			t.NPCCoords = "33.4S, 6.3E";
			t.NPCYellowFlagText = "Return to me with anything you've learned after destroying 25 Bronze Gauntlet Troopers.";
			t.NPCYellowCompleteText = "Congratulations, you survived and succeeded.";
			NewKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Copper Cog Trooper Kill Task (Ijaniya)";
			t.MobNames.Add("Copper Cog Trooper");
			t.CompleteCount = 25;
			t.NPCNames.Add("Lieutenant Zin");
			t.NPCInfo = "Yaraq to Ijaniya (22.4S, 0.2E)";
			t.NPCCoords = "33.4S, 6.3E";
			t.NPCYellowFlagText = "Return to me with anything you've learned after destroying 25 Copper Cog Troopers.";
			t.NPCYellowCompleteText = "Congratulations, you survived and succeeded.";
			NewKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Iron Blade Trooper Kill Task (Ijaniya)";
			t.MobNames.Add("Iron Blade Trooper");
			t.CompleteCount = 25;
			t.NPCNames.Add("Lieutenant Micham");
			t.NPCInfo = "Yaraq to Ijaniya (22.4S, 0.2E)";
			t.NPCCoords = "33.4S, 6.3E";
			t.NPCYellowFlagText = "Return to me with anything you've learned after destroying 25 Iron Blade Troopers.";
			t.NPCYellowCompleteText = "Congratulations, you survived and succeeded.";
			NewKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Silver Scope Trooper Kill Task (Ijaniya)";
			t.MobNames.Add("Silver Scope Trooper");
			t.CompleteCount = 25;
			t.NPCNames.Add("Lieutenant Faen");
			t.NPCInfo = "Yaraq to Ijaniya (22.4S, 0.2E)";
			t.NPCCoords = "33.4S, 6.3E";
			t.NPCYellowFlagText = "Return to me with anything you've learned after destroying 25 Silver Scope Troopers.";
			t.NPCYellowCompleteText = "Congratulations, you survived and succeeded.";
			NewKillTasks.Add(t);
			
			
			t = new KillTask();
			t.TaskName = "Three Eyed Snowman Kill Task";
			t.MobNames.Add("Three Eyed Snowman");
			t.CompleteCount = 5;
			t.NPCNames.Add("Blind Snowman");
			t.NPCInfo = "Mountains (north of Holtburg)";
			t.NPCCoords = "46.7N, 48.9E";
			t.NPCYellowFlagText = "Kill him five times and maybe he will learn not to go around stealing other peoples dreams.";
			t.NPCYellowCompleteText = "Excellent, I hope that teaches ol' Three Eye a lesson.";
			NewKillTasks.Add(t);
			
			//Eastwatch Tasks
			
			t = new KillTask();
			t.TaskName = "Repugnant Eater Kill Task (Eastwatch)";
			t.MobNames.Add("Repugnant Eater");
			t.CompleteCount = 50;
			t.NPCNames.Add("Xun Yu");
			t.NPCInfo = "Eastwatch";
			t.NPCCoords = "90.2N 43.1W";
			t.NPCYellowFlagText = "Track down and slay 50 of the terrible Repugnant Eaters for me, and I will reward you appropriately.";
			t.NPCYellowCompleteText = "Excellent work, friend!";
			NewKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Olthoi Ripper Kill Task (Eastwatch)";
			t.MobNames.Add("Olthoi Ripper");
			t.CompleteCount = 250;
			t.NPCNames.Add("Marcello");
			t.NPCInfo = "Eastwatch";
			t.NPCCoords = "90.2N 43.1W";
			t.NPCYellowFlagText = "Track down and slay 250 of the terrible Olthoi Rippers for me, and I will reward you appropriately.";
			t.NPCYellowCompleteText = "Excellent work, friend!";
			NewKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Naughty Skeleton Kill Task (Eastwatch)";
			t.MobNames.Add("Naughty Skeleton");
			t.CompleteCount = 100;
			t.NPCNames.Add("Taku Yukio");
			t.NPCInfo = "Eastwatch";
			t.NPCCoords = "90.3N 43.1W";
			t.NPCYellowFlagText = "Track down and slay 100 of the terrible Naughty Skeletons for me, and I will reward you appropriately.";
			t.NPCYellowCompleteText = "Excellent work, friend!";
			NewKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Mosswart Worshipper Kill Task (Eastwatch)";
			t.MobNames.Add("Mosswart Worshipper");
			t.CompleteCount = 175;
			t.NPCNames.Add("Orfeo Orlando");
			t.NPCInfo = "Eastwatch";
			t.NPCCoords = "90.2N, 43.1W";
			t.NPCYellowFlagText = "Track down and slay 175 of the terrible Mosswart Worshippers for me, and I will reward you appropriately.";
			t.NPCYellowCompleteText = "Excellent work, friend!";
			NewKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Floeshark Kill Task (Eastwatch)";
			t.MobNames.Add("Floeshark");
			t.CompleteCount = 50;
			t.NPCNames.Add("Ryuichi Tai");
			t.NPCInfo = "Eastwatch";
			t.NPCCoords = "90.3N, 43.0W";
			t.NPCYellowFlagText = "Track down and slay 50 of the terrible Floesharks for me, and I will reward you appropriately.";
			t.NPCYellowCompleteText = "Excellent work, friend!";
			NewKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Wicked Skeleton Kill Task (Eastwatch)";
			t.MobNames.Add("Wicked Skeleton");
			t.CompleteCount = 100;
			t.NPCNames.Add("Ruqaya al Mubarak");
			t.NPCInfo = "Eastwatch";
			t.NPCCoords = "90.3N 43.1W";
			t.NPCYellowFlagText = "Track down and slay 100 of the terrible Wicked Skeletons for me, and I will reward you appropriately.";
			t.NPCYellowCompleteText = "Excellent work, friend! You have slain many of the terrible beasts! Allow me to reward you!";
			NewKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Grievver Violator Kill Task (Eastwatch)";
			t.MobNames.Add("Grievver Violator");
			t.CompleteCount = 100;
			t.NPCNames.Add("Moina");
			t.NPCInfo = "Eastwatch";
			t.NPCCoords = "90.4N 43.1W";
			t.NPCYellowFlagText = "Track down and slay 100 of the terrible Grievver Violators for me, and I will reward you appropriately.";
			t.NPCYellowCompleteText = "Excellent work, friend!";
			NewKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Tukora Lieutenant Kill Task (Eastwatch)";
			t.MobNames.Add("Tukora Lieutenant");
			t.CompleteCount = 250;
			t.NPCNames.Add("Claire Artmad");
			t.NPCInfo = "Eastwatch";
			t.NPCCoords = "90.2N, 43.1W";
			t.NPCYellowFlagText = "Track down and slay 250 of the terrible Tukora Lieutenants for me, and I will reward you appropriately.";
			t.NPCYellowCompleteText = "Excellent work, friend!";
			NewKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Tumerok Gladiator Kill Task";
			t.MobNames.Add("Tumerok Gladiator");
			t.CompleteCount = 25;
			t.NPCNames.Add("Lieutenant Rothe");
			t.NPCInfo = "Dryreach";
			t.NPCCoords = "8.2S 73.1E";
			t.NPCYellowFlagText = "Track down and slay 25 of the Tumerok Gladiators for me, and I will reward you for your aid in the defense of Dryreach.";
			t.NPCYellowCompleteText = "Excellent work, friend!";
			NewKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Tusker Guard Kill Task";
			t.MobNames.Add("Tusker Guard");
			t.CompleteCount = 500;
			t.NPCNames.Add("Shoichi");
			t.NPCInfo = "Lin";
			t.NPCCoords = "54.4S 72.9E";
			t.NPCYellowFlagText = "Return to me after you have killed 500 Tusker Guards and I will reward you.";
			t.NPCYellowCompleteText = "Excellent, now go contemplate what you have learned from fighting the Tusker Guards.";
			NewKillTasks.Add(t);
			
			//Caul
			
			t = new KillTask();
			t.TaskName = "Umbral Rift Kill Task (Caul)";
			t.MobNames.Add("Umbral Rift");
			t.CompleteCount = 30;
			t.NPCNames.Add("Solange");
			t.NPCInfo = "Singularity Caul";
			t.NPCCoords = "97.4S 94.6W";
			t.NPCYellowFlagText = "Kill 30 Umbral Rifts and let me know of your adventures.";
			t.NPCYellowCompleteText = "Truly amazing isn't it. You are one of us, you are a Rift Walker.";
			NewKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Tenebrous Rift Kill Task (Caul)";
			t.MobNames.Add("Tenebrous Rift");
			t.CompleteCount = 350;
			t.NPCNames.Add("Solange");
			t.NPCInfo = "Singularity Caul";
			t.NPCCoords = "97.4S 94.6W";
			t.NPCYellowFlagText = "If you seek to truely explor your own essence I'd suggest you kill 350 or more Tenebrous Rifts, it's truly amazing.";
			t.NPCYellowCompleteText = "To have experienced that many rifts you must truly be chosen.";
			NewKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Viamontian Man-at-Arms Kill Task";
			t.MobNames.Add("Viamontian Man-at-Arm");
			t.CompleteCount = 50;
			t.NPCNames.Add("Robert Gutsmasher");
			t.NPCInfo = "Rebel Hideout";
			t.NPCCoords = "43.9N 73.9W";
			t.NPCYellowFlagText = "Kill 50 of them for me, and I will reward you for your efforts.";
			t.NPCYellowCompleteText = "You do the Carenzi a great service.";
			NewKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Virindi Paradox Kill Task";
			t.MobNames.Add("Virindi Paradox");
			t.CompleteCount = 75;
			t.NPCNames.Add("Guard Taziq");
			t.NPCInfo = "Qalaba'r";
			t.NPCCoords = "74.3S, 19.1E";
			t.NPCYellowFlagText = "If you slay 75 of these strange beings, come to me.";
			t.NPCYellowCompleteText = "As the hero Yaziq al-Tazar returned triumphant, so do you return to me now.";
			NewKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Virindi Quidiox Kill Task";
			t.MobNames.Add("Virindi Quidiox");
			t.CompleteCount = 75;
			t.NPCNames.Add("Guard Q'alia");
			t.NPCInfo = "Ayan Baqur";
			t.NPCCoords = "60.0S, 88.0W";
			t.NPCYellowFlagText = "Go there now, and slay 75 Virindi Quidioxes.";
			t.NPCYellowCompleteText = "It is done!";
			NewKillTasks.Add(t);			
			
			t = new KillTask();
			t.TaskName = "Voracious Eater Kill Task";
			t.MobNames.Add("Voracious Eater");
			t.CompleteCount = 50;
			t.NPCNames.Add("Aun Ruperea");
			t.NPCInfo = "Timaru";
			t.NPCCoords = "44.3N, 77.9W";
			t.NPCYellowFlagText = "Slay 50 of the Voracious Eater and I will sing your name to the elders of my xuta.";
			t.NPCYellowCompleteText = "Ah, buhdi, you do my xuta a grand service. I thank you.";
			NewKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Crystalline Killer (Crystalline Crag)";
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
			t.NPCYellowFlagText = "Go out and kill 100 Wisps in this area and I will reward you.";
			t.NPCYellowCompleteText = "Excellent work in your hunt. We'll find your reports very useful.";
			t.NPCCoords = "90.3N 43.1W";
			NewKillTasks.Add(t);

			//Royal Tent Kill Tasks			

			t = new KillTask();
			t.TaskName = "Golem Samurai Kill Task (Royal Tent)";
			t.MobNames.Add("Bronze Golem Samurai");
			t.MobNames.Add("Iron Golem Samurai");
			t.MobNames.Add("Clay Golem Samurai");
			t.CompleteCount = 10;
			t.NPCNames.Add("Lieutenant Aurin");
			t.NPCInfo = "Royal Tent";
			t.NPCCoords = "80.7N 43.0W";
			t.NPCYellowFlagText = "If you will do me the honor of killing 5 of the Golem Samurai within the towns or up within the walled fortress, I will reward you for your efforts.";
			t.NPCYellowCompleteText = "Well done, well done indeed.";
			NewKillTasks.Add(t);	
			
			t = new KillTask();
			t.TaskName = "Spectral Archer Kill Task (Royal Tent)";
			t.MobNames.Add("Spectral Archer");
			t.CompleteCount = 10;
			t.NPCNames.Add("Sergeant Trebuus");
			t.NPCInfo = "Royal Tent";
			t.NPCCoords = "80.7N 43.0W";
			t.NPCYellowFlagText = "If you will do me the honor of killing 15 of the Spectral Archers within the towns or up within the walled fortress, I will reward you for your assistance.";
			t.NPCYellowCompleteText = "Well done.";
			NewKillTasks.Add(t);

			t = new KillTask();
			t.TaskName = "Spectral Bushi Kill Task (Royal Tent)";
			t.MobNames.Add("Spectral Bushi");
			t.CompleteCount = 10;
			t.NPCNames.Add("Corporal Irashi");
			t.NPCInfo = "Royal Tent";
			t.NPCCoords = "80.7N 43.0W";
			t.NPCYellowFlagText = "If you will assist me by killing 10 of the Spectral Bushi within the towns or up within the walled fortress, I will reward you for your efforts.";
			t.NPCYellowCompleteText = "Thank you for your assistance.";
			NewKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Spectral Blade and Claw Kill Task (Royal Tent)";
			t.MobNames.Add("Spectral Blade Adept");
			t.MobNames.Add("Spectral Blade Master");
			t.MobNames.Add("Spectral Claw Adept");
			t.MobNames.Add("Spectral Claw Master");
			t.CompleteCount = 10;
			t.NPCNames.Add("Griffon");
			t.NPCInfo = "Royal Tent";
			t.NPCCoords = "80.7N 43.0W";
			t.NPCYellowFlagText = "To that end, if you aid me in hunting 10 of the Spectral Claw Adepts, Claw Masters, Blade Adepts or Blade Masters, I'll happily reward you for your help.";
			t.NPCYellowCompleteText = "Your skill is quite remarkable.";
			NewKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Spectral Mage Kill Task (Royal Tent)";
			t.MobNames.Add("Spectral Bloodmage");
			t.MobNames.Add("Spectral Voidmage");
			t.CompleteCount = 10;
			t.NPCNames.Add("Lord Eorlinde");
			t.NPCInfo = "Royal Tent";
			t.NPCCoords = "80.7N 43.0W";
			t.NPCYellowFlagText = "While these two types of spirits continue to exist, they pose a tremendous threat to the kingdom.";
			t.NPCYellowCompleteText = "Your skill is remarkable.";
			NewKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Spectral Minion Kill Task (Royal Tent)";
			t.MobNames.Add("Spectral Minion");
			t.CompleteCount = 15;
			t.NPCNames.Add("Aun Kirtal");
			t.NPCInfo = "Royal Tent";
			t.NPCCoords = "80.7N 43.0W";
			t.NPCYellowFlagText = "If you will aid me by killing 15 of the Spectral Minions within the towns or up within the walled fortress, I will reward your good work.";
			t.NPCYellowCompleteText = "Your hunt for today is complete.";
			NewKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Spectral Nanjou Shou-jen Kill Task (Royal Tent)";
			t.MobNames.Add("Spectral Nanjou Shou-jen");
			t.CompleteCount = 5;
			t.NPCNames.Add("Hanamoto Aki'ko");
			t.NPCInfo = "Royal Tent";
			t.NPCCoords = "80.7N 43.0W";
			t.NPCYellowFlagText = "To that end, if you will do me the honor of killing 5 of the Spectral Nanjou Shou-jen within the towns or up within the walled fortress, I will reward you for your efforts.";
			t.NPCYellowCompleteText = "Thank you for your aid my assigned task.";
			NewKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Spectral Samurai Kill Task (Royal Tent)";
			t.MobNames.Add("Spectral Samurai");
			t.CompleteCount = 10;
			t.NPCNames.Add("Lieutenant Takamaki");
			t.NPCInfo = "Royal Tent";
			t.NPCCoords = "80.7N 43.0W";
			t.NPCYellowFlagText = "If you will do me the honor of killing 10 of the Spectral Samurai within the towns or up within the walled fortress, I will reward you for your efforts.";
			t.NPCYellowCompleteText = "Your skill is exceptional.";
			NewKillTasks.Add(t);						
			
			t = new KillTask();
			t.TaskName = "Eye of T'thuun Quest";
			t.MobNames.Add("Tentacle of T'thuun");
			t.CompleteCount = 50;
			t.NPCNames.Add("Tamara du Cinghalle");
			t.NPCInfo = "Greenspire";
			t.NPCCoords = "43.2N 67.1W";
			t.NPCYellowFlagText = "Killing 50 should be enough to get an eye off of the larger, eye covered one.";
			t.NPCYellowCompleteText = "Ahh, success. The researchers will be very pleased.";
			NewKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Arctic Mattekar Kill Task";
			t.MobNames.Add("Arctic Mattekar");
			t.CompleteCount = 25;
			t.NPCNames.Add("Enzo Ilario");
			t.NPCInfo = "Silyun";
			t.NPCCoords = "87.4N 70.5W";
			t.NPCYellowFlagText = "Well then. Track down and slay 25 of the terrible Arctic Mattekars for me, and I will reward you appropriately.";
			t.NPCYellowCompleteText = "Excellent work, friend!";
			NewKillTasks.Add(t);
			
			//Stonehold
			
			t = new KillTask();
			t.TaskName = "Rare Game Kill Task (Stonehold)";
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
			t.NPCNames.Add("Belinda du Loc");
			t.NPCInfo = "Stonehold";
			t.NPCCoords = "68.9N 21.6W";
			t.NPCYellowFlagText = "Use that list as a reference. Kill 50 of the creatures";
			t.NPCYellowCompleteText = "Excellent, you are now an experienced hunter of the most elusive creatures.";
			NewKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Banished Creature Kill Task (Stonehold)";
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
			t.NPCYellowFlagText = "I would like you to experience the hunting of the elusive banished creatures.";
			t.NPCYellowCompleteText = "Excellent, you are now an experienced hunted of banished creatures.";
			NewKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Fallen Creature Kill Task (Stonehold)";
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
			t.NPCYellowFlagText = "I would like you to experience the hunting of the elusive fallen creatures.";
			t.NPCYellowCompleteText = "Excellent, you are now an experienced hunted of fallen creatures.";
			NewKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Elemental Kill Task";
			t.MobNames.Add("Caustic");
			t.MobNames.Add("Synnast");
			t.MobNames.Add("Inferno");
			t.MobNames.Add("Hyem");
			t.CompleteCount = 25;
			t.NPCNames.Add("Zahir");
			t.NPCInfo = "Stonehold";
			t.NPCCoords = "68.7N, 21.5W";
			t.NPCYellowFlagText = "Fight twenty-five of these creatures and then return to me and tell me everything you learned about them.";
			t.NPCYellowCompleteText = "Very interesting, I hope I can use this information to further my research.";
			NewKillTasks.Add(t);
			
			//Vissidal 
			
			t = new KillTask();
			t.TaskName = "Shallows Gorger Kill Task (Vissidal)";
			t.MobNames.Add("Shallows Gorger");
			t.CompleteCount = 50;
			t.NPCNames.Add("Dayla Bint Kazm");
			t.NPCInfo = "The Deep (Vissidal)";
			t.NPCCoords = "77.8N 67.1E";
			t.NPCYellowFlagText = "It bids me to tell you to kill 50 of the Shallows Gorgers.";
			t.NPCYellowCompleteText = "It has chosen to reward you for completing the task it set before you.";
			NewKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Remoran Sea Raptor Kill Task (Vissidal)";
			t.MobNames.Add("Remoran Sea Raptor");
			t.CompleteCount = 50;
			t.NPCNames.Add("Peng-Ya");
			t.NPCInfo = "The Deep (Vissidal)";
			t.NPCCoords = "77.8N 67.1E";
			t.NPCYellowFlagText = "Kill 50 Remoran Sea Raptors so that they will know their place.";
			t.NPCYellowCompleteText = "Enough! No more Remoran Sea Raptors need die.";
			NewKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Mucky Moarsman Kill Task (Vissidal)";
			t.MobNames.Add("Mucky Moarsman");
			t.CompleteCount = 50;
			t.NPCNames.Add("Algar Oreksun");
			t.NPCInfo = "The Deep (Vissidal)";
			t.NPCCoords = "77.8N 67.1E";
			t.NPCYellowFlagText = "A sacrifice of 50 Mucky Moarsmen must be made.";
			t.NPCYellowCompleteText = "It is pleased.";
			NewKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Benek Niffis Kill Task (Vissidal)";
			t.MobNames.Add("Benek Niffis");
			t.CompleteCount = 50;
			t.NPCNames.Add("Colista Fluress");
			t.NPCInfo = "The Deep (Vissidal)";
			t.NPCCoords = "77.8N 67.1E";
			t.NPCYellowFlagText = "Kill 50 Benek Niffis and rewards shall be yours.";
			t.NPCYellowCompleteText = "It is pleased. Rewards unto thee.";
			NewKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Blood Shreth Kill Task";
			t.MobNames.Add("Blood Shreth");
			t.CompleteCount = 10;
			t.NPCNames.Add("San Ming");
			t.NPCInfo = "Shoushi";
			t.NPCCoords = "33.5S, 72.8E";
			t.NPCYellowFlagText = "Track down and slay 10 of the terrible Blood Shreth for me, and I will reward you appropriately.";
			t.NPCYellowCompleteText = "Excellent work, friend!";
			NewKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Coral Golem Kill Task";
			t.MobNames.Add("Coral Golem");
			t.CompleteCount = 50;
			t.NPCNames.Add("Malrin");
			t.NPCInfo = "Sanamar";
			t.NPCCoords = "71.8N, 60.8W";
			t.NPCYellowFlagText = "It won't make them go away but I'll reward you for every 50 Coral Golems that you kill.";
			t.NPCYellowCompleteText = "Those Coral Golems won't be scratching my armor again.";
			NewKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Deathcap Thrungus Kill Task";
			t.MobNames.Add("Deathcap Thrungus");
			t.CompleteCount = 25;
			t.NPCNames.Add("Jiang Li");
			t.NPCInfo = "Westwatch";
			t.NPCCoords = "72.7N 73.3W";
			t.NPCYellowFlagText = "Track down and slay 25 of the terrible Deathcap Thrungum for me, and I will reward you appropriately.";
			t.NPCYellowCompleteText = "Excellent work, friend!";
			NewKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Dire Mattekar Kill Task";
			t.MobNames.Add("Dire Mattekar");
			t.CompleteCount = 10;
			t.NPCNames.Add("Fergal the Dire");
			t.NPCInfo = "Baishi";
			t.NPCCoords = "49.4S, 62.4E";
			t.NPCYellowFlagText = "Track down and slay 10 of the terrible Dire Mattekars for me, and I will reward you appropriately.";
			t.NPCYellowCompleteText = "Excellent work, friend!";
			NewKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Ebon Gromnie Kill Task";
			t.MobNames.Add("Ebon Gromnie");
			t.CompleteCount = 25;
			t.NPCNames.Add("Afra bint Abbas");
			t.NPCInfo = "Redspire";
			t.NPCCoords = "40.8N, 83.0W";
			t.NPCYellowFlagText = "Track down and slay 25 of the terrible Ebon Gromnies for me, and I will reward you appropriately.";
			t.NPCYellowCompleteText = "Excellent work, friend!";
			NewKillTasks.Add(t);
			
			//Bur
			
			t = new KillTask();
			t.TaskName = "Putrid Moar Kill Task (Kor-Gursha)";
			t.MobNames.Add("Putrid Moar");
			t.CompleteCount = 25;
			t.NPCNames.Add("Tibik");
			t.NPCInfo = "Kor-Gursha";
			t.NPCCoords = "Bur";
			t.NPCYellowFlagText = "You kill 25 Moar, and Tibik reward Strong Traveler from Faraway.";
			t.NPCYellowCompleteText = "Excellent! Tibik be sneaky now.";
			NewKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Guruk Basher Kill Task (Kor-Gursha)";
			t.MobNames.Add("Guruk Basher");
			t.CompleteCount = 40;
			t.NPCNames.Add("Shiruuk");
			t.NPCInfo = "Kor-Gursha";
			t.NPCCoords = "Bur";
			t.NPCYellowFlagText = "If you want to prove you're a real hunter track down and slay 40 of those lumbering Guruk Bashers and I'll reward you.";
			t.NPCYellowCompleteText = "Well done!";
			NewKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Guruk Colossus Kill Task (Kor-Gursha)";
			t.MobNames.Add("Guruk Colossi");
			t.MobNames.Add("Guruk Colossus");
			t.CompleteCount = 30;
			t.NPCNames.Add("Brogosh");
			t.NPCInfo = "Kor-Gursha";
			t.NPCCoords = "Bur";
			t.NPCYellowFlagText = "Search the Southern Catacombs and kill 30 of those colossal brutes!";
			t.NPCYellowCompleteText = "Good job!";
			NewKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Guruk Fiend Kill Task (Kor-Gursha)";
			t.MobNames.Add("Guruk Fiend");
			t.CompleteCount = 30;
			t.NPCNames.Add("Mohor");
			t.NPCInfo = "Kor-Gursha";
			t.NPCCoords = "Bur";
			t.NPCYellowFlagText = "Go and kill 30 of the cunning fiends lurking in the lower Southern Catacombs and I'll reward you for your help.";
			t.NPCYellowCompleteText = "By the fungal roots, you humans can hunt better than anyone I've ever seen!";
			NewKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Guruk Marauder Kill Task (Kor-Gursha)";
			t.MobNames.Add("Guruk Marauder");
			t.CompleteCount = 40;
			t.NPCNames.Add("Kurket");
			t.NPCInfo = "Kor-Gursha";
			t.NPCCoords = "Bur";
			t.NPCYellowFlagText = "If you want to prove you're a real hunter, track down and slay 40 of those despicable Guruk Marauders and I will reward you appropriately.";
			t.NPCYellowCompleteText = "I'm impressed. I didn't think a human could hunt that well.";
			NewKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Guruk Monstrosity Kill Task (Kor-Gursha)";
			t.MobNames.Add("Guruk Monstrosity");
			t.CompleteCount = 10;
			t.NPCNames.Add("Borsh");
			t.NPCInfo = "Kor-Gursha";
			t.NPCCoords = "Bur";
			t.NPCYellowFlagText = "Take your weapon and hunt down 10 of the Guruk Monstrosities and I will reward you.";
			t.NPCYellowCompleteText = "Thank you, I can sleep soundly for a time.";
			NewKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Guruk Smasher Kill Task (Kor-Gursha)";
			t.MobNames.Add("Guruk Smasher");
			t.CompleteCount = 40;
			t.NPCNames.Add("Kushuk");
			t.NPCInfo = "Kor-Gursha";
			t.NPCCoords = "Bur";
			t.NPCYellowFlagText = "Track down and slay 40 of those infuriating Guruk Smashers and I'll reward you well.";
			t.NPCYellowCompleteText = "Yes! Those vandals got what was coming to them!";
			NewKillTasks.Add(t);
						
			t = new KillTask();
			t.TaskName = "Hea Windreave Kill Task";
			t.MobNames.Add("Hea Windreave");
			t.CompleteCount = 25;
			t.NPCNames.Add("Susana du Loc");
			t.NPCInfo = "Redspire";
			t.NPCCoords = "40.7N, 83.2W";
			t.NPCYellowFlagText = "Do me a favor, friend, and kill 25 of these Hea Windreaves for me.";
			t.NPCYellowCompleteText = "You do my heart much good.";
			NewKillTasks.Add(t);
					
			t = new KillTask();
			t.TaskName = "Iron Spined Chittick Kill Task";
			t.MobNames.Add("Iron Spined Chittick");
			t.CompleteCount = 50;
			t.NPCNames.Add("Aidene");
			t.NPCInfo = "Oolatanga's Refuge";
			t.NPCCoords = "2.0N 95.6E";
			t.NPCYellowFlagText = " Track down and slay 50 of the terrible Iron-Spined Chitticks for me, and I will reward you appropriately.";
			t.NPCYellowCompleteText = "Excellent work, friend!";
			NewKillTasks.Add(t);

			t = new KillTask();
			t.TaskName = "Kilif Zefir Kill Task";
			t.MobNames.Add("Kilif Zefir");
			t.CompleteCount = 35;
			t.NPCNames.Add("Hadiya bint Anan");
			t.NPCInfo = "Shoushi";
			t.NPCCoords = "33.7S 73.1E";
			t.NPCYellowFlagText = "Track down and slay 35 of the cunning Kilif Zefir for me, and I will reward you appropriately.";
			t.NPCYellowCompleteText = "Excellent work, friend!";
			NewKillTasks.Add(t);	
			
			t = new KillTask();
			t.TaskName = "K'nath An'dras Kill Task";
			t.MobNames.Add("K'nath An'dra");
			t.CompleteCount = 25;
			t.NPCNames.Add("Nona");
			t.NPCInfo = "Wai Jhou";
			t.NPCCoords = "61.8S 51.3W";
			t.NPCYellowFlagText = "Track down and slay 25 of the terrible K'nath An'drases for me, and I will reward you appropriately.";
			t.NPCYellowCompleteText = "Excellent work, friend!";
			NewKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Littoral Siraluun Kill Task";
			t.MobNames.Add("Littoral Siraluun");
			t.CompleteCount = 25;
			t.NPCNames.Add(" Rico Cellini");
			t.NPCInfo = "Greenspire";
			t.NPCCoords = "43.2N 67.1W";
			t.NPCYellowFlagText = "If you would slay 25 of the beasts, I would reward you as a hunter whose prowess is equal to mine own.";
			t.NPCYellowCompleteText = "Excellent work, friend!";
			NewKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Mottled Carenzi Kill Task";
			t.MobNames.Add("Mottled Carenzi");
			t.CompleteCount = 50;
			t.NPCNames.Add("Grania the Bold");
			t.NPCInfo = "Candeth Keep";
			t.NPCCoords = "87.6S 67.4W";
			t.NPCYellowFlagText = "Track down and slay 50 of the terrible Mottled Carenzi for me, and I will reward you appropriately.";
			t.NPCYellowCompleteText = "Excellent work, friend!";
			NewKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Mosswart Townsfolk Kill Task";
			t.MobNames.Add("Mosswart Townsfolk");
			t.CompleteCount = 40;
			t.NPCNames.Add("Corporal Massein");
			t.NPCInfo = "Kryst";
			t.NPCCoords = "74.4S 84.6E";
			t.NPCYellowFlagText = "Slay forty of the Mosswart Townsfolk who live in the Mosswart holding, then return back to me.";
			t.NPCYellowCompleteText = "Well done, warrior.";
			NewKillTasks.Add(t);
			
			//Olthoi 
			
			t = new KillTask();
			t.TaskName = "Paradox-touched Grub Kill Task (Olthoi)";
			t.MobNames.Add("Paradox-touched Olthoi Noble Grub");
			t.CompleteCount = 50;
			t.NPCNames.Add("Alicia Swiftblade");
			t.NPCInfo = "Olthoi North";
			t.NPCCoords = "43.8N 54.9E";
			t.NPCYellowFlagText = "Kill, let's say, 50 of these Paradox-touched Grubs, and I'll reward you for your aid in this.";
			t.NPCYellowCompleteText = "You've done it!";
			NewKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Paradox-touched Nymph Kill Task (Olthoi)";
			t.MobNames.Add("Paradox-touched Olthoi Warrior Nymph");
			t.CompleteCount = 50;
			t.NPCNames.Add("Tomihino");
			t.NPCInfo = "Olthoi North";
			t.NPCCoords = "43.8N 54.9E";
			t.NPCYellowFlagText = "Slay them, 50 should do for now, and return to me.";
			t.NPCYellowCompleteText = "You have done well.";
			NewKillTasks.Add(t);
						
			t = new KillTask();
			t.TaskName = "Olthoi Drone Kill Task (Arwic)";
			t.MobNames.Add("Olthoi Drone");
			t.CompleteCount = 20;
			t.NPCNames.Add("Olthoi Hunter");
			t.NPCInfo = "Arwic (South)";
			t.NPCCoords = "30.9N 56.3E";
			t.NPCYellowFlagText = "Track down and slay 20 Olthoi Drones and I will reward you appropriately.";
			t.NPCYellowCompleteText = "You have slain many Olthoi Drones!";
			NewKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Olthoi Nettler Kill Task (Arwic)";
			t.MobNames.Add("Olthoi Nettler");
			t.CompleteCount = 10;
			t.NPCNames.Add("Olthoi Hunter");
			t.NPCInfo = "Arwic (South)";
			t.NPCCoords = "30.9N 56.3E";
			t.NPCYellowFlagText = "Track down and slay 10 Olthoi Nettlers and I will reward you appropriately.";
			t.NPCYellowCompleteText = "You have slain many Olthoi Nettlers!";
			NewKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Olthoi Nymph Kill Task (Arwic)";
			t.MobNames.Add("Olthoi Nymph");
			t.CompleteCount = 20;
			t.NPCNames.Add("Olthoi Hunter");
			t.NPCInfo = "Arwic (South)";
			t.NPCCoords = "30.9N 56.3E";
			t.NPCYellowFlagText = "Track down and slay 20 Olthoi Nymphs and I will reward you appropriately.";
			t.NPCYellowCompleteText = "You have slain many Olthoi Nymphs!";
			NewKillTasks.Add(t);
				
			t = new KillTask();
			t.TaskName = "Plate Armoredillo Kill Task";
			t.MobNames.Add("Plate Armoredillo");
			t.CompleteCount = 25;
			t.NPCNames.Add("Saqr");
			t.NPCInfo = "Fort Tethana";
			t.NPCCoords = "1.5N 71.8W";
			t.NPCYellowFlagText = "Well then. Track down and slay 25 of the terrible Plate Armoredillos for me, and I will reward you appropriately.";
			t.NPCYellowCompleteText = "Excellent work, friend!";
			NewKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Polardillo Kill Task";
			t.MobNames.Add("Polardillo");
			t.CompleteCount = 10;
			t.NPCNames.Add("Alessandro Mardor");
			t.NPCInfo = "Sanamar";
			t.NPCCoords = "72.0N 61.2W";
			t.NPCYellowFlagText = "Track down and slay 10 of the terrible Polardillos for me, and I will reward you appropriately.";
			t.NPCYellowCompleteText = "Excellent work, friend!";
			NewKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Polar Ursuin Kill Task";
			t.MobNames.Add("Polar Ursuin");
			t.CompleteCount = 25;
			t.NPCNames.Add("Mariabella Varanese");
			t.NPCInfo = "Fiun Outpost";
			t.NPCCoords = "95.6N, 56.3W";
			t.NPCYellowFlagText = "Track down and slay 25 of the terrible Polar Ursuine for me, and I will reward you appropriately.";
			t.NPCYellowCompleteText = "Excellent work, friend!";
			NewKillTasks.Add(t);
								
			t = new KillTask();
			t.TaskName = "Ruschk Kill Task";
			t.MobNames.Add("Ruschk Draktehn");
			t.MobNames.Add("Ruschk Laktar");
			t.CompleteCount = 30;
			t.NPCNames.Add("Commander Rylane di Cinghalle");
			t.NPCInfo = "Shattered Outlands";
			t.NPCCoords = "93.2N 48.2W";
			t.NPCYellowFlagText = "30 of the Ruschk in the valley, and then acquire one of their smaller Ice Totems from the encampment";
			t.NPCYellowCompleteText = "Ahh, the Ice Totem.";
			NewKillTasks.Add(t);
						
			t = new KillTask();
			t.TaskName = "Shadow-touched Virindi Paradox Kill Task";
			t.MobNames.Add("Shadow-touched Virindi Paradox");
			t.CompleteCount = 75;
			t.NPCNames.Add("Guard Li");
			t.NPCInfo = "Wai Jhou";
			t.NPCCoords = "61.8S, 51.3W";
			t.NPCYellowFlagText = "Slay 75 of these things and return to me, and I shall reward you.";
			t.NPCYellowCompleteText = "Your task was effectively done.";
			NewKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Shadow-touched Virindi Quidiox Kill Task";
			t.MobNames.Add("Shadow-touched Virindi Quidiox");
			t.CompleteCount = 75;
			t.NPCNames.Add("Guard Alfric");
			t.NPCInfo = "Candeth Keep";
			t.NPCCoords = "87.9S, 67.4W";
			t.NPCYellowFlagText = "Kill 75 and come back ta me.";
			t.NPCYellowCompleteText = "Why, ye've done exactly the task I asked o' ye!";
			NewKillTasks.Add(t);
				
			t = new KillTask();
			t.TaskName = "Sishalti Slithis Kill Task";
			t.MobNames.Add("Sishalti Tentacle");
			t.MobNames.Add("Sishalti Tendril");
			t.MobNames.Add("Sishalti Eye Stalk");
			t.CompleteCount = 150;
			t.NPCNames.Add("Zava bint Laurma");
			t.NPCInfo = "Zaikhal";
			t.NPCCoords = "13.9N, 0.6E";
			t.NPCYellowFlagText = "Track down and slay 150 of those lurking Sishalti Tentacles, Tendrils and Eye Stalks for me and I will reward you appropriately.";
			t.NPCYellowCompleteText = "Excellent work, friend!";
			NewKillTasks.Add(t);
			
			t = new KillTask();
			t.TaskName = "Small Fledgling Mukkir Kill Task";
			t.MobNames.Add("Small Fledgling Mukkir");
			t.CompleteCount = 15;
			t.NPCNames.Add("Royal Guard");
			t.NPCInfo = "Holtburg, Shoushi, or Yaraq";
			t.NPCCoords = "Unknown";
			t.NPCYellowFlagText = "If you will go thin out their number, say, kill 15 of the Small Mukkir Fledglings, I will reward you for your efforts.";
			t.NPCYellowCompleteText = "Excellent! The Queen will be most pleased.";
			NewKillTasks.Add(t);
			//Jump
			
			
			
			
			
			
			
			
//			//TODO:  Fillout mob list
//			t = new KillTask();
//			t.TaskName = "Glenden Wood Invaders	Invaders";
//			t.MobNames.Add("Invader");
//			t.CompleteCount = 20;
//			t.NPCNames.Add("Londigul Ellic the Armorer";
//			t.NPCInfo = "Glenden Wood";
//			t.NPCCoords = "29.9N, 27.1E";
//			NewKillTasks.Add(t);	
			
//			Flags based only on green text.  Unable to fit into the model.	
//			//TODO:  Viamontian knight types
//			t = new KillTask();
//			t.TaskName = "Torgash's Tasks";
//			t.MobNames.Add("Royal Inquisitor");
//			t.MobNames.Add("Viamontian Hand");
//			t.MobNames.Add("Viamontian Lord");
//			t.CompleteCount = 30;
//			t.NPCNames.Add("Torgash");
//			t.NPCInfo = "Shattered Outlands";
//			t.NPCCoords = "94.0N 45.9W";
//			t.NPCYellowFlagText = "";
//			t.NPCYellowCompleteText = "";
//			NewKillTasks.Add(t);
			
			//Pumpkin Lord Kill Task
			
			//Harvest Reaper Kill Task
			
			FileInfo TaskFile = new FileInfo(GearDir + @"\Kill.xml");
			if(TaskFile.Exists)
			{
				TaskFile.Delete();
			}
					
			XmlWriterSettings settings = new XmlWriterSettings();
			settings.Indent = true;
			settings.NewLineOnAttributes = true;
			
			XmlWriter writer = XmlWriter.Create(TaskFile.ToString(), settings);
			
   			XmlSerializer serializer2 = new XmlSerializer(typeof(List<KillTask>));
   			serializer2.Serialize(writer, NewKillTasks);
   			writer.Close();
			
			
			
		}
		
		private void BuildCollectionTaskList()
		{
			
			List<CollectTask> NewCollectTasks = new List<PluginCore.CollectTask>();
			
			CollectTask t;
			
			t = new CollectTask();
			t.TaskName = "Prickly Pear Collecting (Neftet)";
			t.Item = "Prickly Pear";
			t.MobNames.Add("Prickly Pear");
			t.CompleteCount = 15;
			t.NPCNames.Add("Hammah al Rundik");
			t.NPCInfo = "Neftet Encampment";
			t.NPCCoords = "22.2S, 6.2E";
			t.NPCYellowFlagText = "If you'll bring me 15 Prickly Pears, I'll happily reward you.";
			t.NPCYellowCompleteText = "Ahh, a full bushel of Prickly Pears, just what every gourmet needs.";
			NewCollectTasks.Add(t);
			
			t = new CollectTask();
			t.TaskName = "Stone Tablet Collecting (Neftet)";
			t.Item = "Broken Stone Tablet";
			t.MobNames.Add("Cracked Stone Tablet");
			t.CompleteCount = 15;
			t.NPCNames.Add("Taylarn bint Tulani");
			t.NPCInfo = "Neftet Encampment";
			t.NPCCoords = "22.2S, 6.2E";
			t.NPCYellowFlagText = "If you bring me any tablets you find, I'm prepared to reward any of sufficient experience for bringing me 15 stone tablets.";
			t.NPCYellowCompleteText = "Ahh, a full stack of 15 stone tablets.";
			NewCollectTasks.Add(t);
			
			t = new CollectTask();
			t.TaskName = "A'nekshay Bracer Collecting (Neftet)";
			t.Item = "Engraved A'nekshay Bracer";
			t.MobNames.Add("A'nekshay");
			t.CompleteCount = 15;
			t.NPCNames.Add("T'ing Setsuko");
			t.NPCInfo = "Neftet Encampment";
			t.NPCCoords = "22.2S, 6.2E";
			t.NPCYellowFlagText = "I am prepared to reward Adventurers of sufficient experience for their efforts in collecting 15 of these A'nekshay Bracers.";
			t.NPCYellowCompleteText = "Ahh, a full stack of 15 A'nekshay Bracers.";
			NewCollectTasks.Add(t);
			
			t = new CollectTask();
			t.TaskName = "Snow Tusker Blood Collection (Frozen Valley)";
			t.Item = "Snow Tusker Blood Sample";
			t.MobNames.Add("Snow Tusker");
			t.CompleteCount = 10;
			t.NPCNames.Add("Archmage Ichihiri");
			t.NPCInfo = "Frozen Valley";
			t.NPCCoords = "83.8N 4.3W";
			t.NPCYellowFlagText = "If you could bring me 10 samples of Snow Tusker Blood";
			t.NPCYellowCompleteText = "I see that you have gathered 10 vials of blood from these Snow Tuskers.";
			NewCollectTasks.Add(t);
			
			t = new CollectTask();
			t.TaskName = "Undead Jaw Collection (Society)";
			t.Item = "Pyre Skeleton Jaw";
			t.MobNames.Add("Pyre Minion");
			t.MobNames.Add("Pyre Skeleton");
			t.MobNames.Add("Pyre Champion");
			t.CompleteCount = 8;
			t.NPCNames.Add("Balon Strongarm");
			t.NPCNames.Add("Hador the Vengeful");
			t.NPCNames.Add("Cullum of Celdon");
			t.NPCInfo = "Society";
			t.NPCCoords = "Unknown";
			t.NPCYellowFlagText = "Be on your way, and come talk to me when you have eight!";
			t.NPCYellowCompleteText = "Well done. Let me take those from you";
			NewCollectTasks.Add(t);
			
			t = new CollectTask();
			t.TaskName = "Collect Gear Knight Parts (Society)";
			t.Item = "Pile of Gearknight Parts";
			t.MobNames.Add("Invading");
			t.CompleteCount = 10;
			t.NPCNames.Add("Trathium");
			t.NPCNames.Add("Dark Reshan");
			t.NPCNames.Add("Drocogst");
			t.NPCInfo = "Society";
			t.NPCCoords = "Unknown";
			t.NPCYellowFlagText = "Bring back parts from 10 of the Gearknights and I shall reward you.";
			t.NPCYellowCompleteText = "A solid blow to their forces.";
			NewCollectTasks.Add(t);
			
			t = new CollectTask();
			t.TaskName = "Falatacot Report Collector (Society)";
			t.Item = "Falatacot Battle Report";
			t.MobNames.Add("Falatacot Blood Prophetess");
			t.CompleteCount = 10;
			t.NPCNames.Add("Boroth Bearhand");
			t.NPCNames.Add("Turvald Snorborgson");
			t.NPCNames.Add("Agbeart");
			t.NPCInfo = "Society";
			t.NPCCoords = "Unknown";
			t.NPCYellowFlagText = "Go to Dark Isle and collect the Falatacot reports.";
			t.NPCYellowCompleteText = "Ah, here we go.";
			NewCollectTasks.Add(t);
			
			t = new CollectTask();
			t.TaskName = "Black Coral Collection (Society)";
			t.Item = "Black Coral";
			t.MobNames.Add("");
			t.CompleteCount = 10;
			t.NPCNames.Add("Hidoshi Kawara");
			t.NPCNames.Add("Manto Sakara");
			t.NPCNames.Add("Daisei Chirana");
			t.NPCInfo = "Society";
			t.NPCCoords = "Unknown";
			t.NPCYellowFlagText = "Bring exactly ten of this black coral to me.";
			t.NPCYellowCompleteText = "This is sufficient.";
			NewCollectTasks.Add(t);
			
			t = new CollectTask();
			t.TaskName = "Glowing Jungle Lily Collector (Society)";
			t.Item = "Glowing Jungle Lily";
			t.MobNames.Add("Blessed Moarsman");
			t.MobNames.Add("Blessed Moar");
			t.MobNames.Add("Ashris Niffis");
			t.CompleteCount = 20;
			t.NPCNames.Add("Kojina");
			t.NPCNames.Add("Satsuki");
			t.NPCNames.Add("Atsuko");
			t.NPCInfo = "Society";
			t.NPCCoords = "Unknown";
			t.NPCYellowFlagText = "If you get me at least 20 Glowing Jungle Lilies";
			t.NPCYellowCompleteText = "Ahh, the flowers, here, let me take those from you";
			NewCollectTasks.Add(t);
			
			t = new CollectTask();
			t.TaskName = "Glowing Moar Gland Collector (Society)";
			t.Item = "Glowing Moar Gland";
			t.MobNames.Add("Blessed Moar");
			t.CompleteCount = 30;
			t.NPCNames.Add("Aurellia du Cinghalle");
			t.NPCNames.Add("Elloisa du Cinghalle");
			t.NPCNames.Add("Pia du Cinghalle");
			t.NPCInfo = "Society";
			t.NPCCoords = "Unknown";
			t.NPCYellowFlagText = "kill enough Blessed Moars to collect at least 30 Glowing Moar Glands";
			t.NPCYellowCompleteText = "Perfect, you have the glands";
			NewCollectTasks.Add(t);
			
			t = new CollectTask();
			t.TaskName = "Mana-Infused Jungle Flower Collector (Society)";
			t.Item = "Mana-Infused Jungle Flower";
			t.MobNames.Add("");
			t.CompleteCount = 20;
			t.NPCNames.Add("Giri bint Ashud");
			t.NPCNames.Add("Leisall bint Jumadd");
			t.NPCNames.Add("Ti'alla bint Ashud");
			t.NPCInfo = "Society";
			t.NPCCoords = "Unknown";
			t.NPCYellowFlagText = "If you get me at least 20 Mana-Infused Jungle Flowers";
			t.NPCYellowCompleteText = "Ahh, the flowers, here, let me take those from you";
			NewCollectTasks.Add(t);
			
			t = new CollectTask();
			t.TaskName = "Phyntos Honey Collector (Society)";
			t.Item = "Phyntos Honey";
			t.MobNames.Add("Giant Jungle Phyntos Wasp");
			t.MobNames.Add("Killer Phyntos Hive");
			t.MobNames.Add("Killer Phyntos Swarm");
			t.CompleteCount = 10;
			t.NPCNames.Add("Narris");
			t.NPCNames.Add("Zahid al-Din");
			t.NPCNames.Add("Kenji");
			t.NPCInfo = "Society";
			t.NPCCoords = "Unknown";
			t.NPCYellowFlagText = "Bring me anything phyntos related you find";
			t.NPCYellowCompleteText = "We're studying their honey to see if it will reveal anything about their aggressive nature";
			NewCollectTasks.Add(t);
			
			t = new CollectTask();
			t.TaskName = "Phyntos Splinter Collector (Society)";
			t.Item = "Hive Splinters";
			t.MobNames.Add("Killer Phyntos Hive");
			t.CompleteCount = 10;
			t.NPCNames.Add("Narris");
			t.NPCNames.Add("Zahid al-Din");
			t.NPCNames.Add("Kenji");
			t.NPCInfo = "Society";
			t.NPCCoords = "Unknown";
			t.NPCYellowFlagText = " I'll determine what we can use";
			t.NPCYellowCompleteText = "These Phyntos Hive Splinters are proof of destroyed hives";
			NewCollectTasks.Add(t);
			
			t = new CollectTask();
			t.TaskName = "Noble Remains Kill Task (Mhoire)";
			t.Item = "Mhoire Signet Ring";
			t.MobNames.Add("Noble Remain");
			t.CompleteCount = 10;
			t.NPCNames.Add("Shade of Ormend");
			t.NPCInfo = "Mhoire Castle Northeast Tower";
			t.NPCCoords = "64.7S 45.2W";
			t.NPCYellowFlagText = "Destroy these corrupted remains and gather the signet rings from the bones. Return them to me and I will reward you.";
			t.NPCYellowCompleteText = "I see that you have recovered 10 signet rings of House Mhoire.";
			NewCollectTasks.Add(t);
			
			t = new CollectTask();
			t.TaskName = "High Priest of T'thuun (Society)";
			t.Item = "Holy Symbol";
			t.MobNames.Add("High Priest of T'thuun");
			t.CompleteCount = 1;
			t.NPCNames.Add("Kaymor ibn Dumandi");
			t.NPCNames.Add("Milos ibn Ashud");
			t.NPCNames.Add("Lunbal Dolicci");
			t.NPCInfo = "Society Stronghold";
			t.NPCCoords = "Unknown";
			t.NPCYellowFlagText = "Kill this Moarsman High Priest and bring back the Holy Symbol he wields as proof of your kill.";
			t.NPCYellowCompleteText = "Excellent, you were able to defeat the High Priest!";
			NewCollectTasks.Add(t);		

			
//			t = new CollectTask();
//			t.TaskName = "";
//			t.Item = "";
//			t.MobNames.Add("");
//			t.CompleteCount = ;
//			t.NPCNames.Add("");
//			t.NPCNames.Add("");
//			t.NPCNames.Add("");
//			t.NPCInfo = "Society";
//			t.NPCCoords = "Unknown";
//			t.NPCYellowFlagText = "";
//			t.NPCYellowCompleteText = "";
//			mKTSet.MyCollectTasks.Add(t);
			

			
			
			FileInfo TaskFile = new FileInfo(GearDir + @"\Collect.xml");
			if(TaskFile.Exists)
			{
				TaskFile.Delete();
			}
					
			XmlWriterSettings settings = new XmlWriterSettings();
			settings.Indent = true;
			settings.NewLineOnAttributes = true;
			
			XmlWriter writer = XmlWriter.Create(TaskFile.ToString(), settings);
			
   			XmlSerializer serializer2 = new XmlSerializer(typeof(List<CollectTask>));
   			serializer2.Serialize(writer, NewCollectTasks);
   			writer.Close();
		}
		
		
	}
}
