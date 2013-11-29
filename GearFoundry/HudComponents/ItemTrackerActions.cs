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
		private List<int> CombineSalvageWOList = new List<int>();
		private List<WorldObject> InventorySalvage = new List<WorldObject>();
		private List<PendingActions> InspectorActionList = new List<PendingActions>();
		
		private System.Windows.Forms.Timer InspectorActionTimer = new System.Windows.Forms.Timer();
	
		
		public class PendingActions
		{
			public bool fireaction = false;
			public bool pending = false;
			public DateTime StartAction = DateTime.MinValue;
		}
		
		public enum IAction
		{
			None,
			Desiccate,
			Ring,
			Salvage,
			Combine,
			Read,
			ManaStone
		}
		
		private void ToggleInspectorActions(int toggle)
		{
			try
			{
				switch(toggle)
				{
					//Disable all
					case 0:
						for(int i = 0; i < InspectorActionList.Count; i++)
						{
							InspectorActionList[i].fireaction = false;
						}
						return;
					//Peace then Move
					case 1:
						InspectorActionList[0].fireaction = true;
						InspectorActionList[2].fireaction = true;
						return;
					//Peace, then process
					case 2:
						InspectorActionList[0].fireaction = true;
						if(LOList.Any(x => x.ProcessAction == IAction.Read && x.Process))
						{
							InspectorActionList[3].fireaction = true;
						}
						if(LOList.Any(x => x.ProcessAction == IAction.Desiccate && x.Process))
						{
							InspectorActionList[4].fireaction = true;
						}
						if(LOList.Any(x => x.ProcessAction == IAction.ManaStone && x.Process))
						{
							InspectorActionList[5].fireaction = true;
						}
						if(LOList.Any(x => x.ProcessAction == IAction.Ring && x.Process))
						{
							InspectorActionList[6].fireaction = true;
						}
						if(LOList.Any(x => x.ProcessAction == IAction.Salvage && x.Process))
						{
							InspectorActionList[7].fireaction = true;
						}
						if(LOList.Any(x => x.ProcessAction == IAction.Combine && x.Process))
						{
							InspectorActionList[8].fireaction = true;
						}
				
						return;
						
						
				}
			}catch(Exception ex){LogError(ex);}

		}
				
		private bool ActionsPending = false;
		private void InitiateInspectorActionSequence()
		{
			try
			{
				if(!ActionsPending)
				{
					ActionsPending = true;
					InspectorActionTimer.Interval = 150;
					InspectorActionTimer.Start();
					
					InspectorActionTimer.Tick += InspectorActionInitiator;
					return;
				}			
			}catch(Exception ex){LogError(ex);}
		}
		
		private void InspectorActionInitiator(object sender, EventArgs e)
		{
			try
			{
				FireInspectorActions();
			}catch(Exception ex){LogError(ex);}
		}
			
		private void FireInspectorActions()
		{
			try
			{
				//Peace Mode
				if(InspectorActionList[0].fireaction)
				{
					if(InspectorActionList[0].pending && (DateTime.Now - InspectorActionList[0].StartAction).TotalMilliseconds < 750)
					{
						return;
					}
					else if(Core.Actions.CombatMode != CombatState.Peace)
					{
						InspectorActionList[0].pending = true;
						InspectorActionList[0].StartAction = DateTime.Now;
						Core.Actions.SetCombatMode(CombatState.Peace);
						return;
					}
					else
					{
						InspectorActionList[0].pending = false;
						InspectorActionList[0].StartAction = DateTime.MinValue;
						InspectorActionList[0].fireaction = false;
					}
				}
				//OpenContainer
				else if(InspectorActionList[1].fireaction)
				{
					if(InspectorActionList[1].pending && (DateTime.Now - InspectorActionList[1].StartAction).TotalMilliseconds < 1000)
					{
						return;
					}
					else if(LOList.Any(x => x.Open))
					{
						LootObject lo = LOList.Find(x => x.Open);
						InspectorActionList[1].pending = true;
						InspectorActionList[1].StartAction = DateTime.Now;
						
						
						lo.ActionTarget = true;
						lo.LastActionTime = DateTime.Now;
						Core.Actions.UseItem(lo.Id,0);
						
						return;
					}
					else
					{
						InspectorActionList[1].pending = false;
						InspectorActionList[1].StartAction = DateTime.MinValue;
						InspectorActionList[1].fireaction = false;
					}
					
				}
				//MoveObject
				else if(InspectorActionList[2].fireaction)
				{
					//Quit Actions if backpack is full
					if(Core.WorldFilter.GetByContainer(Core.CharacterFilter.Id).Where(x => x.Values(LongValueKey.EquippedSlots) == 0 && x.Values(LongValueKey.Unknown10) != 56).Count() == 100)
					{
						InspectorActionList[2].fireaction = false;
						WriteToChat("You are out of space in your main pack.  Looting stopped.");
						return;
					}
					
					//Delay next action check until 450ms has passed
					if(InspectorActionList[2].pending && (DateTime.Now - InspectorActionList[2].StartAction).TotalMilliseconds < 450)
					{
						return;
					}
					//Check to see if an item is being moved from the open container
					else if(Core.Actions.OpenedContainer != 0 && LOList.Any(x => x.Move && x.Container == Core.Actions.OpenedContainer))
					{
						LootObject lo = LOList.Find(x => x.Move && x.Container == Core.Actions.OpenedContainer);	
						
						InspectorActionList[2].pending = true;
						InspectorActionList[2].StartAction = DateTime.Now;
							
						lo.ActionTarget = true;
						lo.LastActionTime = DateTime.Now;
						Core.Actions.UseItem(lo.Id,0);							
						return;
					}
					//Hold Chests until closed. or empty
					else if(Core.Actions.OpenedContainer != 0 && ChestCheck(Core.WorldFilter[Core.Actions.OpenedContainer].Name) && Core.WorldFilter.GetByContainer(Core.Actions.OpenedContainer).Count() > 0)
					{
						return;
					}
					//Check to see if the open container has any more items pending
					else if(Core.Actions.OpenedContainer != 0 && LOList.Any(x => x.Container == Core.Actions.OpenedContainer && x.InspectList))
					{
						return;
					}
					//Check to see if items need looting from other containers
					else if(LOList.Any(x => x.Move && x.Container != Core.Actions.OpenedContainer))
					{
						LootObject lo = LOList.Find(x => x.Move && x.Container != Core.Actions.OpenedContainer);	
				
						if(Core.WorldFilter.GetInventory().Where(x => x.Id == lo.Id).Count() > 0)
						{
							lo.Move = false;
							lo.ActionTarget = false;
							lo.InspectList = false;
							
							if(lo.ProcessAction != IAction.None) 
							{
								lo.ProcessList = true;
								if(GISettings.AutoProcess) 
								{
									lo.Process = true;
									ToggleInspectorActions(2);
									InitiateInspectorActionSequence();
								}
							}
							InspectorActionList[2].pending = false;
							UpdateItemHud();
							return;
						}
						//TODO:  Put in code to keep from attempting to re-open chests here.  Continue testing and see if it is needed.
//						if(!ChestCheck(Core.WorldFilter[lo.Container].Name))
//						{
//							LOList.Find(x => x.Id == lo.Container).Open = true;
//							InspectorActionList[1].fireaction = true;
//							return;
//						}
					}
					else
					{	
						InspectorActionList[2].pending = false;
						InspectorActionList[2].StartAction = DateTime.MinValue;
						InspectorActionList[2].fireaction = false;
					}
				}
//				//Read
				else if(InspectorActionList[3].fireaction)
				{
					if(InspectorActionList[3].pending && (DateTime.Now - InspectorActionList[3].StartAction).TotalMilliseconds < 450)
					{
						return;
					}
					else if(LOList.Any(x => x.Process && x.ProcessAction == IAction.Read))
					{
						InspectorActionList[3].pending = true;
						InspectorActionList[3].StartAction = DateTime.Now;
						Core.Actions.UseItem(LOList.Find(x => x.Process && x.ProcessAction == IAction.Read).Id,0);
						return;
					}
					else
					{
						InspectorActionList[3].pending = false;
						InspectorActionList[3].StartAction = DateTime.MinValue;
						InspectorActionList[3].fireaction = false;
					}
					
				}
//				//Desiccate
				else if(InspectorActionList[4].fireaction)
				{
					if(InspectorActionList[4].pending && (DateTime.Now - InspectorActionList[4].StartAction).TotalMilliseconds < 450)
					{
						return;
					}
					else if(LOList.Any(x => x.Process && x.ProcessAction == IAction.Desiccate))
					{
						if(Core.WorldFilter.GetInventory().Where(x => x.Name == "Aetheria Desiccant").Count() == 0) 
						{
							WriteToChat("No Aetheria Desiccant found.");
							InspectorActionList[4].fireaction = false;
							LOList.Find(x => x.Process && x.ProcessAction == IAction.Desiccate).Process = false;
							return;
						}
						
						InspectorActionList[4].pending = true;
						InspectorActionList[4].StartAction = DateTime.Now;
						Core.Actions.SelectItem(LOList.Find(x => x.Process && x.ProcessAction == IAction.Desiccate).Id);
						Core.RenderFrame += InspectorDesiccate;
						return;
					}
					else
					{
						InspectorActionList[4].pending = false;
						InspectorActionList[4].StartAction = DateTime.MinValue;
						InspectorActionList[4].fireaction = false;
					}
				}
//				//ManaStone
				else if(InspectorActionList[5].fireaction)
				{
					if(InspectorActionList[5].pending && (DateTime.Now - InspectorActionList[5].StartAction).TotalMilliseconds < 450)
					{
						return;
					}
					else if(LOList.Any(x => x.Process && x.ProcessAction == IAction.ManaStone))
					{
						if(Core.WorldFilter.GetInventory().Where(x => x.ObjectClass == ObjectClass.ManaStone && x.Values(LongValueKey.IconOutline) == 0).Count() == 0) 
						{
							WriteToChat("No empty mana stones available.");
							InspectorActionList[5].fireaction = false;
							LOList.Find(x => x.Process && x.ProcessAction == IAction.ManaStone).Process = false;
							return;
						}
						
						InspectorActionList[5].pending = true;
						InspectorActionList[5].StartAction = DateTime.Now;
						Core.Actions.SelectItem(LOList.Find(x => x.Process && x.ProcessAction == IAction.ManaStone).Id);
						Core.RenderFrame += InspectorDrainMana;
						return;
					}
					else
					{
						InspectorActionList[5].pending = false;
						InspectorActionList[5].StartAction = DateTime.MinValue;
						InspectorActionList[5].fireaction = false;
					}
					
				}
//				//RingKeys
				else if(InspectorActionList[6].fireaction)
				{
					if(InspectorActionList[6].pending && (DateTime.Now - InspectorActionList[6].StartAction).TotalMilliseconds < 450)
					{
						return;
					}
					else if(LOList.Any(x => x.Process && x.ProcessAction == IAction.Ring))
					{
						if(MatchKey(LOList.Find(x => x.Process && x.ProcessAction == IAction.Ring).Name) == 0)
						{
							WriteToChat("No matching, empty keyrings found.");
							InspectorActionList[6].fireaction = false;
							LOList.Find(x => x.Process && x.ProcessAction == IAction.Ring).Process = false;
							return;
						}
						
						InspectorActionList[6].pending = true;
						InspectorActionList[6].StartAction = DateTime.Now;
						Core.Actions.SelectItem(LOList.Find(x => x.Process && x.ProcessAction == IAction.Ring).Id);
						Core.RenderFrame += InspectorRingKey;
						return;
					}
					else
					{
						InspectorActionList[6].pending = false;
						InspectorActionList[6].StartAction = DateTime.MinValue;
						InspectorActionList[6].fireaction = false;
					}	
				}
				//Salvage
				else if(InspectorActionList[7].fireaction)
				{		
					if(InspectorActionList[7].pending && (DateTime.Now - InspectorActionList[7].StartAction).TotalMilliseconds < 300)
					{
						return;
					}
					else if(LOList.Any(x => x.Process && x.ProcessAction == IAction.Salvage))
					{
						InspectorActionList[7].pending = true;
						InspectorActionList[7].StartAction = DateTime.Now;
						InspectorUseUst();
						Core.RenderFrame += InspectorSalvageAction;
						return;
					}
					else
					{
						InspectorActionList[7].pending = false;
						InspectorActionList[7].StartAction = DateTime.MinValue;
						InspectorActionList[7].fireaction = false;
					}	
				}
				//SalvageCombine
				else if(InspectorActionList[8].fireaction)
				{
					if(InspectorActionList[8].pending && (DateTime.Now - InspectorActionList[7].StartAction).TotalMilliseconds < 300)
					{
						return;
					}
					else if(LOList.Any(x => x.Process && x.ProcessAction == IAction.Combine))
					{
						InspectorActionList[8].pending = true;
						InspectorActionList[8].StartAction = DateTime.Now;
						InspectorCombineAction();
						return;
					}
					else
					{
						InspectorActionList[8].pending = false;
						InspectorActionList[8].StartAction = DateTime.MinValue;
						InspectorActionList[8].fireaction = false;
					}
					
				}
				//Stop
				else
				{
					InspectorActionTimer.Tick -= InspectorActionInitiator;
					ActionsPending = false;
					return;
				}				
			}catch(Exception ex){LogError(ex);}
		}
		
		private void InspectorMoveCheckBack(object sender, EventArgs e)
		{
			try
			{
				if(!LOList.Any(x => x.ActionTarget && !x.IsContainer))
				{
					Core.RenderFrame -= InspectorMoveCheckBack;
					return;
				}
				else if((DateTime.Now - LOList.Find(x => x.ActionTarget && !x.IsContainer).LastActionTime).TotalMilliseconds < 100) {return;}
				else
				{
					Core.RenderFrame -= InspectorMoveCheckBack;
				}
				
				LootObject lo = LOList.Find(x => x.ActionTarget && !x.IsContainer);
				
				if(Core.WorldFilter.GetInventory().Where(x => x.Id == lo.Id).Count() > 0)
				{
					lo.Move = false;
					lo.ActionTarget = false;
					lo.InspectList = false;
					
					InspectorActionList[2].pending = false;
					
					if(lo.ProcessAction != IAction.None) 
					{
						lo.ProcessList = true;
						if(GISettings.AutoProcess) 
						{
							lo.Process = true;
							ToggleInspectorActions(2);
							InitiateInspectorActionSequence();
						}
					}
					UpdateItemHud();
				}
									
			}catch(Exception ex){Core.RenderFrame -= InspectorMoveCheckBack; LogError(ex);}
		}
		
		private void OpenContainerCheckback(object sender, EventArgs  e)
		{
			try
			{
				if(!LOList.Any(x => x.ActionTarget && x.IsContainer))
				{
					Core.RenderFrame -= OpenContainerCheckback;
					return;
				}
				else if((DateTime.Now - LOList.Find(x => x.ActionTarget && x.IsContainer).LastActionTime).TotalMilliseconds < 200) {return;}
				else
				{
					Core.RenderFrame -= OpenContainerCheckback;
				}
				
				LootObject lo = LOList.Find(x => x.ActionTarget && x.IsContainer);
				
				if(Core.Actions.OpenedContainer == lo.Id)
				{
					InspectorActionList[1].fireaction = false;
					InspectorActionList[1].pending = false;
					
					lo.Open = false;
					lo.ActionTarget = false;	
				}		
			}catch(Exception ex){Core.RenderFrame -= OpenContainerCheckback;LogError(ex);}
		}
		
		private void InspectorUseUst()
		{
			try
			{				
				WorldObject ust = Core.WorldFilter.GetInventory().Where(x => x.Name == "Ust").First();
				
				if(ust == null)
				{
					ToggleInspectorActions(0);
					WriteToChat("Character Has no Ust. Actions disabled.");
				}
				
				Core.Actions.UseItem(ust.Id,0);					
				
			}catch(Exception ex){LogError(ex);}		
		}
		
		private void InspectorSalvageAction(object sender, EventArgs e)
		{
			try
			{
				if((DateTime.Now - InspectorActionList[7].StartAction).TotalMilliseconds < 50) {return;}
				else
				{
					Core.RenderFrame -= InspectorSalvageAction;
				}
				
				LootObject so = LOList.Find(x => x.Process && x.ProcessAction == IAction.Salvage);
						
				Core.Actions.SalvagePanelAdd(so.Id);
				Core.Actions.SalvagePanelSalvage();
			
			}catch(Exception ex){Core.RenderFrame -= InspectorSalvageAction;LogError(ex);}	
		}		
		
		private void SalvageCreated(object sender, CreateObjectEventArgs e)
		{
			try
			{
				if(e.New.ObjectClass != ObjectClass.Salvage || e.New.Container != Core.CharacterFilter.Id) {return;}
				if(e.New.Values(LongValueKey.UsesRemaining) ==  100) {return;}
				if(!LOList.Any(x => x.Id == e.New.Id))
				{
					LootObject so = new LootObject(e.New);
					so.ProcessAction = IAction.Combine;
					so.Process = true;
					LOList.Add(so);
				}
				
				ToggleInspectorActions(2);
				InitiateInspectorActionSequence();
				
			}catch(Exception ex){LogError(ex);}
		}
		
		private void InspectorCombineAction()
		{
			try
			{
				LootObject co = LOList.Find(x => x.Process && x.ProcessAction == IAction.Combine);
								
				ScanInventoryForSalvageBags();
				
				//Find an applicable material rule.
				var materialrules = from allrules in SalvageRulesList
					where (allrules.material == co.LValue(LongValueKey.Material)) &&
					       (co.DValue(DoubleValueKey.SalvageWorkmanship) >= allrules.minwork) && 
						   (co.DValue(DoubleValueKey.SalvageWorkmanship) <= (allrules.maxwork +0.99))
						   select allrules;		
					
				if(materialrules.Count() > 0)
				{
					var sr = materialrules.First();
					
					var partbagslinq = from bags in InventorySalvage
								  where bags.Values(LongValueKey.UsesRemaining) < 100  &&
							      	bags.Values(LongValueKey.Material) == sr.material  &&
								  	bags.Values(DoubleValueKey.SalvageWorkmanship) >= sr.minwork &&
								 	 bags.Values(DoubleValueKey.SalvageWorkmanship) <= (sr.maxwork + 0.99)
								  select new PartialBags{ SalvBagID = bags.Id, SalvBagUses = bags.Values(LongValueKey.UsesRemaining), 
					             	 SalvBagWork = bags.Values(DoubleValueKey.SalvageWorkmanship), SalvBagMat = bags.Values(LongValueKey.Material)};
				
					PartialBags[] partbags = partbagslinq.ToArray();
					
					CombineSalvageWOList.Clear();
					
					int salvagesum = 0;
					salvagesum += co.LValue(LongValueKey.UsesRemaining);
					CombineSalvageWOList.Add(co.Id);
				
					for(int i = 0; i < partbags.Count(); i++)
					{
						if(salvagesum < 100)
						{
							if(salvagesum + partbags[i].SalvBagUses < 125)
							{
								if(!CombineSalvageWOList.Contains(partbags[i].SalvBagID))
							    {
									salvagesum += partbags[i].SalvBagUses;
									CombineSalvageWOList.Add(partbags[i].SalvBagID);
								}
							}
						}		
					}
					if(CombineSalvageWOList.Count() > 1)
					{						
						foreach(int salvageid in CombineSalvageWOList)
						{
							Core.Actions.SalvagePanelAdd(salvageid);
						}
						Core.Actions.SalvagePanelSalvage();
						CombineSalvageWOList.Clear();
						return;
					}
					else
					{
						co.ProcessAction = IAction.None;
						co.Process = false;
						return;
					}
				}
				else
				{
					co.ProcessAction = IAction.None;
					co.Process = false;
					return;
				}
				
			}catch(Exception ex){LogError(ex);}
		}
		
		private void InspectorDesiccate(object sender, EventArgs e)
		{
			try
			{
				if((DateTime.Now - InspectorActionList[4].StartAction).TotalMilliseconds < 100){return;}
				else
				{

					Core.RenderFrame -= InspectorDesiccate;
				}
								
				Core.Actions.UseItem(Core.WorldFilter.GetInventory().Where(x => x.Name == "Aetheria Desiccant").First().Id, 1);
								                     
			}catch(Exception ex){LogError(ex);}	
		}
		
		private void InspectorDrainMana(object sender, System.EventArgs e)
		{
			try
			{
				if((DateTime.Now - InspectorActionList[5].StartAction).TotalMilliseconds < 100){return;}
				else
				{
					Core.RenderFrame -= InspectorDrainMana;		
				}
				
				Core.Actions.UseItem(Core.WorldFilter.GetInventory().Where(x => x.ObjectClass == ObjectClass.ManaStone && x.Values(LongValueKey.IconOutline) == 0).First().Id,1);	
			
			
			}catch(Exception ex){Core.RenderFrame -= InspectorDrainMana;LogError(ex);}		
		}
		
		private void InspectorRingKey(object sender, EventArgs e)
		{
			try
			{
				if((DateTime.Now - InspectorActionList[6].StartAction).TotalMilliseconds < 100){return;}
				else
				{
					Core.RenderFrame -= InspectorRingKey;
				}
				
				Core.Actions.UseItem(MatchKey(LOList.Find(x => x.Process && x.ProcessAction == IAction.Ring).Name),1);
				
				//Listen in ObjectRelease (or destroyed?) to fire the next action if needed.
			
				
			}catch(Exception ex){LogError(ex);}
		}
			
		private int MatchKey(string keyname)
		{
			try
			{
					WorldObject matchedkeyring = null;
					switch(keyname.ToLower())
					{
						case "legendary key":
							matchedkeyring = MaidKeyRings.FirstOrDefault(x => x.Name.ToLower().Contains("burning sands") && 
							                x.Values(LongValueKey.UsesRemaining) < 0 && x.Values(LongValueKey.KeysHeld) < 24);
							if(matchedkeyring != null) {return matchedkeyring.Id;}
							else{goto default;}
						case "black marrow key":
							matchedkeyring = MaidKeyRings.FirstOrDefault(x => x.Name.ToLower().Contains("black marrow") &&
							                 x.Values(LongValueKey.UsesRemaining) < 0 && x.Values(LongValueKey.KeysHeld) < 24);
							if(matchedkeyring != null) {return matchedkeyring.Id;}
							else{goto default;}
						case "directive key":
							matchedkeyring = MaidKeyRings.FirstOrDefault(x => x.Name.ToLower().Contains("directive") &&
							                 x.Values(LongValueKey.UsesRemaining) < 0 && x.Values(LongValueKey.KeysHeld) < 24);
							if(matchedkeyring != null) {return matchedkeyring.Id;}
							else{goto default;}
						case "granite key":
							matchedkeyring = MaidKeyRings.FirstOrDefault(x => x.Name.ToLower().Contains("granite") &&
							                 x.Values(LongValueKey.UsesRemaining) < 0 && x.Values(LongValueKey.KeysHeld) < 24);
							if(matchedkeyring != null) {return matchedkeyring.Id;}
							else{goto default;}
						case "mana forge key":
							matchedkeyring = MaidKeyRings.FirstOrDefault(x => x.Name.ToLower().Contains("black coral") &&
							                 x.Values(LongValueKey.UsesRemaining) < 0 && x.Values(LongValueKey.KeysHeld) < 24);
							if(matchedkeyring != null) {return matchedkeyring.Id;}
							else{goto default;}
						case "master key":
							matchedkeyring = MaidKeyRings.FirstOrDefault(x => x.Name.ToLower().Contains("master") &&
							                 x.Values(LongValueKey.UsesRemaining) < 0 && x.Values(LongValueKey.KeysHeld) < 24);
							if(matchedkeyring != null) {return matchedkeyring.Id;}
							else{goto default;}
						case "marble key":
							matchedkeyring = MaidKeyRings.FirstOrDefault(x => x.Name.ToLower().Contains("marble") &&
							                 x.Values(LongValueKey.UsesRemaining) < 0 && x.Values(LongValueKey.KeysHeld) < 24);
							if(matchedkeyring != null) {return matchedkeyring.Id;}
							else{goto default;}
						case "singularity key":
							matchedkeyring = MaidKeyRings.FirstOrDefault(x => x.Name.ToLower().Contains("singularity") &&
							                 x.Values(LongValueKey.UsesRemaining) < 0 && x.Values(LongValueKey.KeysHeld) < 24);
							if(matchedkeyring != null) {return matchedkeyring.Id;}
							else{goto default;}
						case "skeletal falatacot key":
							matchedkeyring = MaidKeyRings.FirstOrDefault(x => x.Name.ToLower().Contains("skeletal falatacot") &&
							                 x.Values(LongValueKey.UsesRemaining) < 0 && x.Values(LongValueKey.KeysHeld) < 24);
							if(matchedkeyring != null) {return matchedkeyring.Id;}
							else{goto default;}
						case "sturdy iron key":
							matchedkeyring = MaidKeyRings.FirstOrDefault(x => x.Name.ToLower().Contains("sturdy iron") &&
							                 x.Values(LongValueKey.UsesRemaining) < 0 && x.Values(LongValueKey.KeysHeld) < 24);
							if(matchedkeyring != null) {return matchedkeyring.Id;}
							else{goto default;}
						case "sturdy steel key":
							matchedkeyring = MaidKeyRings.FirstOrDefault(x => x.Name.ToLower().Contains("sturdy steel") &&
							                x.Values(LongValueKey.UsesRemaining) < 0 && x.Values(LongValueKey.KeysHeld) < 24);
							if(matchedkeyring != null) {return matchedkeyring.Id;}
							else{goto default;}
						default:
							return 0;
					}		
			}catch(Exception ex)
			{
				LogError(ex);
				return 0;
			}
		}
		
		private bool ChestCheck(string containername)
		{
			try
			{
				if(containername.Contains("Chest") || containername.Contains("Vault") ||  containername.Contains("Reliquary")) {return true;}
				else{ return false;}
				
			}catch(Exception ex){LogError(ex);return false;}
		}
		
		private bool CheckTheNull()
		{
			try
			{

				foreach(var item in LOList.Where(x => x.InspectList))
				{
					WriteToChat(item.Name);
				}
				return false;
//					else if(LOList.Any(x => x.InspectList && x.Container == Core.Actions.OpenedContainer))
			}catch(Exception ex){LogError(ex); return false;}
		}
		
		
		private void ScanInventoryForSalvageBags()
		{
			try
			{
				InventorySalvage.Clear();
		 		InventorySalvage = Core.WorldFilter.GetInventory().Where(x => x.ObjectClass == ObjectClass.Salvage).OrderBy(x => x.Values(LongValueKey.Material)).ToList();
			}catch{}
		}
	}
}




