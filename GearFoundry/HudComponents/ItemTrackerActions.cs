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
		private Queue<PendingActions> InspectorActionQueue = new Queue<PendingActions>();
		private System.Windows.Forms.Timer InspectorActionTimer = new System.Windows.Forms.Timer();
	
		
		public class PendingActions
		{
			public IAction Action = IAction.DeQueue;
			public LootObject LootItem = null;
			public bool pending = false;
			public DateTime StartAction = DateTime.MinValue;
		}
		
			
		public enum IAction
		{
			PeaceMode,
			OpenContainer,
			MoveItem,
			SalvageItem,			
			CombineSalvage,
			ManaStone,
			Desiccate,
			RingKey,
			DeQueue
		}
		
		private bool ActionsPending = false;
		private void InitiateInspectorActionSequence()
		{
			try
			{
				if(!ActionsPending)
				{
					ActionsPending = true;
					InspectorActionTimer.Interval = 100;
					InspectorActionTimer.Start();
					
					InspectorActionTimer.Tick += InspectorActionInitiator;
					return;
				}
				else
				{
					if(InspectorActionQueue.Count > 0){return;}
					else
					{
						ActionsPending = false;
						InspectorActionTimer.Tick -= InspectorActionInitiator;
						InspectorActionTimer.Stop();
						return;
					}
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

				if(ActionsPending && InspectorActionQueue.Count == 0)
				{
					ActionsPending = false;
					InspectorActionTimer.Tick -= InspectorActionInitiator;
					InspectorActionTimer.Stop();				
					return;
				}
				
				if(Core.WorldFilter.GetByContainer(Core.CharacterFilter.Id).Where(x => x.Values(LongValueKey.EquippedSlots) == 0 && x.Values(LongValueKey.Unknown10) != 56).Count() == 101)
				{
					ActionsPending = false;
					InspectorActionTimer.Tick -= InspectorActionInitiator;
					InspectorActionTimer.Stop();
					InspectorActionQueue.Clear();
					WriteToChat("You are out of space in your main pack.  Looting disabled.");
					return;
				}
								
				//this will restart the action attempt after 2 seconds if it fails.
				if(InspectorActionQueue.First().pending && InspectorActionQueue.First().Action != IAction.DeQueue)
				{
					if((DateTime.Now - InspectorActionQueue.First().StartAction).TotalSeconds < 2)	{return;}
				}

				InspectorActionQueue.First().StartAction = DateTime.Now;
				InspectorActionQueue.First().pending = true;
				
				if((InspectorActionQueue.First().Action == IAction.SalvageItem || InspectorActionQueue.First().Action == IAction.CombineSalvage ||
				   InspectorActionQueue.First().Action == IAction.Desiccate || InspectorActionQueue.First().Action == IAction.RingKey ||
				   InspectorActionQueue.First().Action == IAction.ManaStone) && ItemTrackingList != null)
				{
				
					if(ItemTrackingList.Any(x => x.Container == Core.Actions.OpenedContainer))
					{
						if(InspectorActionQueue.Any(x => x.Action == IAction.MoveItem && x.LootItem.Container == Core.Actions.OpenedContainer))
						{
							InspectorActionQueue.First().StartAction = DateTime.MinValue;
							InspectorActionQueue.First().pending = false;
							
							List<PendingActions> TempPendingActionsHolder = InspectorActionQueue.ToList();
							int nextactioninex = TempPendingActionsHolder.FindIndex(x => x.Action == IAction.MoveItem && x.LootItem.Container == Core.Actions.OpenedContainer);
							
							InspectorActionQueue.Clear();
													
							InspectorActionQueue.Enqueue(TempPendingActionsHolder.ElementAt(nextactioninex));
							TempPendingActionsHolder.RemoveAt(nextactioninex);
							
							foreach(PendingActions pa in TempPendingActionsHolder)
							{
								InspectorActionQueue.Enqueue(pa);
							}
						}
						return;
					}
				}
				
				switch(InspectorActionQueue.First().Action)
				{
					case IAction.DeQueue:
						InspectorActionQueue.Dequeue();
						return;
					case IAction.PeaceMode:
						Core.RenderFrame += RenderFrame_PeaceMode;
						return;
					case IAction.OpenContainer:
						Core.RenderFrame += RenderFrame_OpenContainer;
						return;
					case IAction.MoveItem:
						Core.RenderFrame += RenderFrame_InspectorMoveAction;
						return;
					case IAction.Desiccate:
						Core.RenderFrame += RenderFrame_DesiccateItem;
						return;
					case  IAction.ManaStone:
						Core.RenderFrame += RenderFrame_DrainManaTank;
						return;
					case IAction.RingKey:
						Core.RenderFrame += RenderFrame_RingKeys;
						return;						
					case IAction.SalvageItem:
						if(Core.WorldFilter.GetInventory().Where(x => x.Name == "Ust").Count() == 0)
						{
							WriteToChat("Character has no Ust.");
							InspectorActionQueue.First().Action = IAction.DeQueue;
							return;
						}
						Core.RenderFrame += RenderFrame_InspectorUseUst;
						Core.RenderFrame += RenderFrame_InspectorSalvageAction;
						return;
					case IAction.CombineSalvage:
						if(Core.WorldFilter.GetInventory().Where(x => x.Id == InspectorActionQueue.First().LootItem.Id).Count() == 0 ||
						   InspectorActionQueue.First().LootItem.LValue(LongValueKey.UsesRemaining) == 100)
						{
							InspectorActionQueue.First().Action = IAction.DeQueue;
							return;
						}
						
						if(Core.WorldFilter.GetInventory().Where(x => x.Name == "Ust").Count() == 0)
						{
							WriteToChat("Character has no Ust.");
							InspectorActionQueue.First().Action = IAction.DeQueue;
							return;
						}
						Core.RenderFrame += RenderFrame_InspectorUseUst;
						Core.RenderFrame += RenderFrame_InspectorCombineAction;
						return;					
				}
			}catch(Exception ex){LogError(ex);}
		}
		
		private void RenderFrame_PeaceMode(object sender, EventArgs e)
		{
			try
			{	
				if((DateTime.Now - InspectorActionQueue.First().StartAction).TotalMilliseconds < 100){return;}
				else
				{
					Core.RenderFrame -= RenderFrame_PeaceMode; 
				}
				
				if(Core.Actions.CombatMode == CombatState.Peace)
				{
					InspectorActionQueue.First().Action = IAction.DeQueue;
					return;
				}
				
				Core.Actions.SetCombatMode(CombatState.Peace);	
				InspectorActionQueue.First().StartAction = DateTime.Now;
				Core.RenderFrame += RenderFrame_SwitchCombatWait;				
				
			}catch(Exception ex){LogError(ex);}
		}
		
		private void RenderFrame_SwitchCombatWait(object sender, EventArgs e)
		{
			try
			{	
				if(Core.Actions.CombatMode == CombatState.Peace && (DateTime.Now - InspectorActionQueue.First().StartAction).TotalMilliseconds > 1000)
				{
					Core.RenderFrame -= RenderFrame_SwitchCombatWait;
					InspectorActionQueue.First().Action = IAction.DeQueue;
					return;
				}
				else
				{
					return;
				}	
			}catch(Exception ex){LogError(ex);}
		}
		
		private void RenderFrame_OpenContainer(object sender, EventArgs e)
		{
			try
			{
				if((DateTime.Now - InspectorActionQueue.First().StartAction).TotalMilliseconds < 100){return;}
				else
				{
					Core.RenderFrame -= RenderFrame_OpenContainer;
				}
				
				if(Core.Actions.OpenedContainer == InspectorActionQueue.First().LootItem.Id || InspectorActionQueue.First().LootItem.Id == Core.CharacterFilter.Id)
				{
					InspectorActionQueue.First().Action = IAction.DeQueue;
					return;
				}
				
				Core.Actions.UseItem(InspectorActionQueue.First().LootItem.Id,0);
				
				return;
				
			}catch(Exception ex){LogError(ex);}
		}
			
		private void RenderFrame_InspectorMoveAction(object sender, System.EventArgs e)
		{
			try
			{
				if((DateTime.Now - InspectorActionQueue.First().StartAction).TotalMilliseconds < 100){return;}
				else
				{
					Core.RenderFrame -= RenderFrame_InspectorMoveAction;
				}
				
				if(Core.WorldFilter.GetInventory().Where(x => x.Id == InspectorActionQueue.First().LootItem.Id).Count() > 0 || !InspectorActionQueue.First().LootItem.isvalid)
				{
					InspectorActionQueue.First().Action = IAction.DeQueue;
					return;
				}
				
				if(Core.Actions.OpenedContainer != InspectorActionQueue.First().LootItem.Container)
				{
					InspectorActionQueue.First().StartAction = DateTime.MinValue;
					InspectorActionQueue.First().pending = false;
					
					List<PendingActions> TempPendingActionsHolder = InspectorActionQueue.ToList();
					
					PendingActions nextaction = new PendingActions();
					nextaction.Action = IAction.OpenContainer;
					nextaction.LootItem = new LootObject(Core.WorldFilter[InspectorActionQueue.First().LootItem.Container]);
					InspectorActionQueue.Enqueue(nextaction);
					
					TempPendingActionsHolder.Insert(0, nextaction);				
					InspectorActionQueue.Clear();
					
					foreach(PendingActions pa in TempPendingActionsHolder)
					{
						InspectorActionQueue.Enqueue(pa);
					}					
					TempPendingActionsHolder.Clear();
					
					return;
				}
				else
				{				
					Core.Actions.UseItem(InspectorActionQueue.First().LootItem.Id, 0);
				}
				
				//Listens in Change object for itemchanged, dequeues, then fires inspector action
				
			}catch(Exception ex){LogError(ex);}			
		}
		
		//Processing of Salvage			
		private void RenderFrame_InspectorUseUst(object sender, EventArgs e)
		{
			try
			{
				if((DateTime.Now - InspectorActionQueue.First().StartAction).TotalMilliseconds < 20){return;}
				else
				{
					Core.RenderFrame -= RenderFrame_InspectorUseUst;		
				}
				
				Core.Actions.UseItem(Core.WorldFilter.GetInventory().Where(x => x.Name == "Ust").First().Id,0);	
				
				//this could listen in change view, but why bother.....
				
			}catch(Exception ex){LogError(ex);}		
		}
		
		private void RenderFrame_InspectorSalvageAction(object sender, EventArgs e)
		{
			try
			{
				if((DateTime.Now - InspectorActionQueue.First().StartAction).TotalMilliseconds < 150){return;}
				else
				{
					Core.RenderFrame -= RenderFrame_InspectorSalvageAction;	
				}
				
				if(Core.WorldFilter.GetInventory().Where(x => x.Id == InspectorActionQueue.First().LootItem.Id).Count() == 0 || !InspectorActionQueue.First().LootItem.isvalid)
				{
					InspectorActionQueue.First().Action = IAction.DeQueue;
					return;
				}
						
				Core.Actions.SalvagePanelAdd(InspectorActionQueue.First().LootItem.Id);
				Core.Actions.SalvagePanelSalvage();
			
				//Listens in ObjectCreated for the creation of salvage, then fires InspectorAction
				//Process Items List should remove from ItemDestroyed
			
			}catch(Exception ex){LogError(ex);}	
		}		
		
		private void ItemTrackerActions_ObjectCreated(object sender, CreateObjectEventArgs e)
		{
			try
			{
				if(e.New.ObjectClass != ObjectClass.Salvage) {return;}
				{
					PendingActions nextaction = new PendingActions();
					nextaction.Action = IAction.CombineSalvage;
					nextaction.LootItem = new LootObject(e.New);
					InspectorActionQueue.Enqueue(nextaction);	
				}
				
				if(!ActionsPending) {InitiateInspectorActionSequence();}
				
			}catch(Exception ex){LogError(ex);}
		}
		
		private void RenderFrame_InspectorCombineAction(object sender, EventArgs e)
		{
			try
			{
				if((DateTime.Now - InspectorActionQueue.First().StartAction).TotalMilliseconds < 150){return;}
				else
				{
					Core.RenderFrame -= RenderFrame_InspectorCombineAction;
				}
				
				if(Core.WorldFilter.GetInventory().Where(x => x.Id == InspectorActionQueue.First().LootItem.Id).Count() == 0 || !InspectorActionQueue.First().LootItem.isvalid)
				{
					InspectorActionQueue.First().Action = IAction.DeQueue;
					return;
				}
				
				ScanInventoryForSalvageBags();
				
				WorldObject CurrentSalvage = Core.WorldFilter[InspectorActionQueue.First().LootItem.Id];
				
				//Find an applicable material rule.
				var materialrules = from allrules in SalvageRulesList
					where (allrules.material == CurrentSalvage.Values(LongValueKey.Material)) &&
					       (CurrentSalvage.Values(DoubleValueKey.SalvageWorkmanship) >= allrules.minwork) && 
						   (CurrentSalvage.Values(DoubleValueKey.SalvageWorkmanship) <= (allrules.maxwork +0.99))
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
					salvagesum += CurrentSalvage.Values(LongValueKey.UsesRemaining);
					CombineSalvageWOList.Add(CurrentSalvage.Id);
				
					for(int i = 0; i < partbags.Count(); i++)
					{
						if(salvagesum < 100)
						{
							if(salvagesum + partbags[i].SalvBagUses < 110)
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
						//Now that it's committed to salvage something, check for the ust, otherwise, why bother?
						
						foreach(int salvageid in CombineSalvageWOList)
						{
							Core.Actions.SalvagePanelAdd(salvageid);
						}
						Core.Actions.SalvagePanelSalvage();
						CombineSalvageWOList.Clear();
						return;
						//If it combines, it will listen in ObjectCreated and FIA.  Else the FIA below will kick off the FIA again.
					}
					else
					{
						InspectorActionQueue.First().Action = IAction.DeQueue;
						return;
					}
				}
				else
				{
					InspectorActionQueue.First().Action = IAction.DeQueue;
					return;
				}
				
			}catch(Exception ex){LogError(ex);}
		}
		
		//Processing of ManaTankQueue		
		private void RenderFrame_DrainManaTank(object sender, System.EventArgs e)
		{
			try
			{
				if((DateTime.Now - InspectorActionQueue.First().StartAction).TotalMilliseconds < 100){return;}
				else
				{
					Core.RenderFrame -= RenderFrame_DrainManaTank;		
				}
				
				if(Core.WorldFilter.GetInventory().Where(x => x.Id == InspectorActionQueue.First().LootItem.Id).Count() == 0  || !InspectorActionQueue.First().LootItem.isvalid)
				{
					InspectorActionQueue.First().Action = IAction.DeQueue;
					return;
				}
				
				if(Core.WorldFilter.GetInventory().Where(x => x.ObjectClass == ObjectClass.ManaStone && x.Values(LongValueKey.IconOutline) == 0).Count() == 0)
				{
					WriteToChat("No empty manastones available!");
					InspectorActionQueue.First().Action = IAction.DeQueue;
					return;
				}
				else
				{
					if(Core.Actions.CombatMode != CombatState.Peace)
					{
						InspectorActionQueue.First().StartAction = DateTime.MinValue;
						InspectorActionQueue.First().pending = false;
						
						List<PendingActions> TempPendingActionsHolder = InspectorActionQueue.ToList();
						InspectorActionQueue.Clear();
						
						PendingActions nextaction = new PendingActions();
						nextaction.Action = IAction.PeaceMode;
						InspectorActionQueue.Enqueue(nextaction);
						
						foreach(PendingActions pa in TempPendingActionsHolder)
						{
							InspectorActionQueue.Enqueue(pa);
						}					
						TempPendingActionsHolder.Clear();
	
						return;					
					}
					Core.Actions.SelectItem(InspectorActionQueue.First().LootItem.Id);
					InspectorActionQueue.First().StartAction = DateTime.Now;
					Core.RenderFrame += RenderFrame_UseManaStone;
				}				
			}catch(Exception ex){LogError(ex);}
			
		}
		
		private void RenderFrame_UseManaStone(object sender, System.EventArgs e)
		{
			try
			{
				if((DateTime.Now - InspectorActionQueue.First().StartAction).TotalMilliseconds < 100){return;}
				else
				{
					Core.RenderFrame -= RenderFrame_UseManaStone;		
				}
				
				Core.Actions.UseItem(Core.WorldFilter.GetInventory().Where(x => x.ObjectClass == ObjectClass.ManaStone && x.Values(LongValueKey.IconOutline) == 0).First().Id,1);	

				//Listen in ItemDestroyed to DeQueue and FireInspectorAction() if needed (done)				
			
			}catch(Exception ex){LogError(ex);}		
		}
		
		//Processing of Desiccate Queue.
		private void RenderFrame_DesiccateItem(object sender, EventArgs e)
		{
			try
			{
				if((DateTime.Now - InspectorActionQueue.First().StartAction).TotalMilliseconds < 100){return;}
				else
				{
					Core.RenderFrame -= RenderFrame_DesiccateItem;
				}
				
				if(Core.WorldFilter.GetInventory().Where(x => x.Id == InspectorActionQueue.First().LootItem.Id).Count() == 0  || !InspectorActionQueue.First().LootItem.isvalid)
				{
					InspectorActionQueue.First().Action = IAction.DeQueue;
					return;
				}
				
				if(Core.WorldFilter.GetInventory().Where(x => x.Name == "Aetheria Desiccant").Count() == 0) 
				{
					WriteToChat("No Aetheria Desiccant found.");
					InspectorActionQueue.First().Action = IAction.DeQueue;
					return;
				}
				else	
				{
					if(Core.Actions.CombatMode != CombatState.Peace)
					{
						

						InspectorActionQueue.First().StartAction = DateTime.MinValue;
						InspectorActionQueue.First().pending = false;
						
						List<PendingActions> TempPendingActionsHolder = InspectorActionQueue.ToList();
						
						PendingActions nextaction = new PendingActions();
						nextaction.Action = IAction.PeaceMode;
						InspectorActionQueue.Enqueue(nextaction);
						
						TempPendingActionsHolder.Insert(0, nextaction);				
						InspectorActionQueue.Clear();
						
						foreach(PendingActions pa in TempPendingActionsHolder)
						{
							InspectorActionQueue.Enqueue(pa);
						}					
						TempPendingActionsHolder.Clear();
	
						return;				
					}
					
					Core.Actions.SelectItem(InspectorActionQueue.First().LootItem.Id);
					InspectorActionQueue.First().StartAction = DateTime.Now;
					Core.RenderFrame += RenderFrame_ApplyDesiccant;
				}				
	
			}catch(Exception ex){LogError(ex);}
		}
		
		
		private void RenderFrame_ApplyDesiccant(object sender, EventArgs e)
		{
			try
			{
				if((DateTime.Now - InspectorActionQueue.First().StartAction).TotalMilliseconds < 100){return;}
				else
				{

					Core.RenderFrame -= RenderFrame_ApplyDesiccant;
				}
				
				
				Core.Actions.UseItem(Core.WorldFilter.GetInventory().Where(x => x.Name == "Aetheria Desiccant").First().Id, 1);
	
				//Listen in Item Destroyed then FIA if needed
								                     
			}catch(Exception ex){LogError(ex);}	
		}

		private int KeyRingMatchId = 0;
		private void RenderFrame_RingKeys(object sender, EventArgs e)
		{
			try
			{
				if((DateTime.Now - InspectorActionQueue.First().StartAction).TotalMilliseconds < 100){return;}
				else
				{
					Core.RenderFrame -= RenderFrame_RingKeys;
				}
				
				if(Core.WorldFilter.GetInventory().Where(x => x.Id == InspectorActionQueue.First().LootItem.Id).Count() == 0  || !InspectorActionQueue.First().LootItem.isvalid)
				{
					InspectorActionQueue.First().Action = IAction.DeQueue;
					return;
				}
				
				LootObject currentkey = InspectorActionQueue.First().LootItem;
				
				KeyRingMatchId = MatchKey(currentkey.Name);
				if(MatchedKeyRingId == 0)
				{
					WriteToChat("No matching, empty keyrings found.");
					InspectorActionQueue.First().Action = IAction.DeQueue;
					return;
				}
				else
				{	
					if(Core.Actions.CombatMode != CombatState.Peace)
					{
						InspectorActionQueue.First().StartAction = DateTime.MinValue;
						InspectorActionQueue.First().pending = false;
						
						List<PendingActions> TempPendingActionsHolder = InspectorActionQueue.ToList();
						InspectorActionQueue.Clear();
						
						PendingActions nextaction = new PendingActions();
						nextaction.Action = IAction.PeaceMode;
						InspectorActionQueue.Enqueue(nextaction);
						
						foreach(PendingActions pa in TempPendingActionsHolder)
						{
							InspectorActionQueue.Enqueue(pa);
						}					
						TempPendingActionsHolder.Clear();
	
						return;					
					}	
					
					Core.Actions.SelectItem(currentkey.Id);
					InspectorActionQueue.First().StartAction = DateTime.Now;
					Core.RenderFrame += RenderFrame_RingIt;
				}
	
			}catch(Exception ex){LogError(ex);}
		}
		
		private void RenderFrame_RingIt(object sender, EventArgs e)
		{
			try
			{
				if((DateTime.Now - InspectorActionQueue.First().StartAction).TotalMilliseconds < 100){return;}
				else
				{
					Core.RenderFrame -= RenderFrame_RingIt;
				}
				
				Core.Actions.UseItem(KeyRingMatchId,1);
				
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
							matchedkeyring = MaidKeyRings.FirstOrDefault(x => x.Name.ToLower().Contains("burning sands"));
							if(matchedkeyring != null) {return matchedkeyring.Id;}
							else{goto default;}
						case "black marrow key":
							matchedkeyring = MaidKeyRings.FirstOrDefault(x => x.Name.ToLower().Contains("black marrow"));
							if(matchedkeyring != null) {return matchedkeyring.Id;}
							else{goto default;}
						case "directive key":
							matchedkeyring = MaidKeyRings.FirstOrDefault(x => x.Name.ToLower().Contains("directive"));
							if(matchedkeyring != null) {return matchedkeyring.Id;}
							else{goto default;}
						case "granite key":
							matchedkeyring = MaidKeyRings.FirstOrDefault(x => x.Name.ToLower().Contains("granite"));
							if(matchedkeyring != null) {return matchedkeyring.Id;}
							else{goto default;}
						case "mana forge key":
							matchedkeyring = MaidKeyRings.FirstOrDefault(x => x.Name.ToLower().Contains("black coral"));
							if(matchedkeyring != null) {return matchedkeyring.Id;}
							else{goto default;}
						case "master key":
							matchedkeyring = MaidKeyRings.FirstOrDefault(x => x.Name.ToLower().Contains("master"));
							if(matchedkeyring != null) {return matchedkeyring.Id;}
							else{goto default;}
						case "marble key":
							matchedkeyring = MaidKeyRings.FirstOrDefault(x => x.Name.ToLower().Contains("marble"));
							if(matchedkeyring != null) {return matchedkeyring.Id;}
							else{goto default;}
						case "singularity key":
							matchedkeyring = MaidKeyRings.FirstOrDefault(x => x.Name.ToLower().Contains("singularity"));
							if(matchedkeyring != null) {return matchedkeyring.Id;}
							else{goto default;}
						case "skeletal falatacot key":
							matchedkeyring = MaidKeyRings.FirstOrDefault(x => x.Name.ToLower().Contains("skeletal falatacot"));
							if(matchedkeyring != null) {return matchedkeyring.Id;}
							else{goto default;}
						case "sturdy iron key":
							matchedkeyring = MaidKeyRings.FirstOrDefault(x => x.Name.ToLower().Contains("sturdy iron"));
							if(matchedkeyring != null) {return matchedkeyring.Id;}
							else{goto default;}
						case "sturdy steel key":
							matchedkeyring = MaidKeyRings.FirstOrDefault(x => x.Name.ToLower().Contains("sturdy steel"));
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

	}
}




