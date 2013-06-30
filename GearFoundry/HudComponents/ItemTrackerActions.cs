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
		private List<int> CombineSalvageWOList = new List<int>();
		private Queue<PendingActions> InspectorActionQueue = new Queue<PendingActions>();
		private System.Windows.Forms.Timer InspectorActionTimer = new System.Windows.Forms.Timer();
	
		
		public class PendingActions
		{
			public IAction Action = IAction.Nothing;
			public LootObject LootItem = null;
			public bool pending = false;
			public DateTime StartAction = DateTime.MinValue;
		}
		
			
		public enum IAction
		{
			Nothing,
			PeaceMode,
			OpenContainer,
			SelectItem,
			MoveItem,
			SalvageItem,			
			CombineSalvage,
			ManaStone,
			Desiccate,
			RingKey,
			DeQueue
		}
		
		
		//Entry points should be:
		//1.  Item queued for move
		//2.  Loot Object enters inventory which is an autoprocess type.
		
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
				
				if(Core.WorldFilter.GetByContainer(Core.CharacterFilter.Id).Where(x => x.Values(LongValueKey.EquippedSlots) == 0 && x.Values(LongValueKey.Unknown10) != 56).Count() == 102)
				{
					ActionsPending = false;
					InspectorActionTimer.Tick -= InspectorActionInitiator;
					InspectorActionTimer.Stop();
					InspectorActionQueue.Clear();
					WriteToChat("You are out of space in your main pack.  Looting disabled.");
					return;
				}
				
				//this will restart the queue if it fails.
				if(InspectorActionQueue.First().pending)
				{
					if((DateTime.Now - InspectorActionQueue.First().StartAction).TotalSeconds < 3)	{return;}
				}
				
				InspectorActionQueue.First().StartAction = DateTime.Now;
				InspectorActionQueue.First().pending = true;
				WriteToChat("Time: " + DateTime.Now);
				WriteToChat("Inspectoractionqueue.count = " + InspectorActionQueue.Count);	
				WriteToChat("Pending Action " + InspectorActionQueue.First().Action.ToString());
				if(InspectorActionQueue.First().LootItem != null)
				{
					WriteToChat(InspectorActionQueue.First().LootItem.Name);
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
						Core.RenderFrame += RenderFrame_InspectorUseUst;
						Core.RenderFrame += RenderFrame_InspectorSalvageAction;
						return;
					case IAction.CombineSalvage:
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
					InspectorActionQueue.Dequeue();
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
				
				if(mOpenContainer.ContainerGUID != InspectorActionQueue.First().LootItem.Container)
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
				if((DateTime.Now - InspectorActionQueue.First().StartAction).TotalMilliseconds < 300){return;}
				else
				{
					Core.RenderFrame -= RenderFrame_InspectorSalvageAction;	
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
				
				if(InspectorActionQueue.Count > 0)
				{
					if(InspectorActionQueue.First().Action == IAction.SalvageItem || InspectorActionQueue.First().Action == IAction.CombineSalvage)
					{
						InspectorActionQueue.Dequeue();
					}
				}
				
				if(!InspectorActionQueue.Any(x => x.LootItem.Id == e.New.Id))
				{
					PendingActions nextaction = new PendingActions();
					nextaction.Action = IAction.CombineSalvage;
					nextaction.LootItem = new LootObject(e.New);
					InspectorActionQueue.Enqueue(nextaction);
				}	
				
			}catch(Exception ex){LogError(ex);}
		}
		
		private void RenderFrame_InspectorCombineAction(object sender, EventArgs e)
		{
			try
			{
				if((DateTime.Now - InspectorActionQueue.First().StartAction).TotalMilliseconds < 300){return;}
				else
				{
					Core.RenderFrame -= RenderFrame_InspectorCombineAction;
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
						InspectorActionQueue.Dequeue();
						return;
					}
				}
				else
				{
					InspectorActionQueue.Dequeue();
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
				
				if(Core.WorldFilter.GetInventory().Where(x => x.ObjectClass == ObjectClass.ManaStone && x.Values(LongValueKey.IconOutline) == 0).Count() == 0)
				{
					WriteToChat("No empty manastones available!");
					InspectorActionQueue.Dequeue();
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
				
				if(Core.WorldFilter.GetInventory().Where(x => x.Name == "Aetheria Desiccant").Count() == 0) 
				{
					WriteToChat("No Aetheria Desiccant found.");
					InspectorActionQueue.Dequeue();
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
				
				LootObject currentkey = InspectorActionQueue.First().LootItem;
				
				KeyRingMatchId = MatchKey(currentkey.Name);
				if(MatchedKeyRingId == 0)
				{
					WriteToChat("No matching, empty keyrings found.");
					InspectorActionQueue.Dequeue();
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

//Green Garnet 9-9) {NO ID} Heavy Bracelet
//(Trophy) Lesser Corrupted Essence (102.0S, 102.0W)
//(Steel 10-10) {GS 94 1J} Katar, Damage Score: 53, Skill Modifiers: 41, Major Quickness, Light Weapons 400 to Wield
//#GearFoundry#: Time: 6/28/2013 2:33:58 PM
//#GearFoundry#: Inspectoractionqueue.count = 2
//#GearFoundry#: Pending Action PeaceMode
//(Silver 1-10) {GS 85} Lightning Assagai, Damage Score: 45, Skill Modifiers: 40, Two Handed Combat 400 to Wield
//#GearFoundry#: Time: 6/28/2013 2:33:59 PM
//#GearFoundry#: Inspectoractionqueue.count = 1
//#GearFoundry#: Pending Action MoveItem
//#GearFoundry#: Lesser Corrupted Essence
//#GearFoundry#: Time: 6/28/2013 2:34:03 PM
//#GearFoundry#: Inspectoractionqueue.count = 1
//#GearFoundry#: Pending Action MoveItem
//#GearFoundry#: Heavy Bracelet
//#GearFoundry#: Time: 6/28/2013 2:34:08 PM
//#GearFoundry#: Inspectoractionqueue.count = 2
//#GearFoundry#: Pending Action SalvageItem
//#GearFoundry#: Heavy Bracelet
//You obtain 12 Green Garnet (ws 9.00) using your knowledge of Salvaging.
//#GearFoundry#: Time: 6/28/2013 2:34:13 PM
//#GearFoundry#: Inspectoractionqueue.count = 3
//#GearFoundry#: Pending Action MoveItem
//#GearFoundry#: Katar
//(Silver 1-10) {NO ID} Bracelet
//(Trophy) Glyph of Item Enchantment (102.0S, 102.0W)
//#GearFoundry#: Time: 6/28/2013 2:34:20 PM
//#GearFoundry#: Inspectoractionqueue.count = 4
//#GearFoundry#: Pending Action CombineSalvage
//#GearFoundry#: Salvage (12)
//#GearFoundry#: Time: 6/28/2013 2:34:20 PM
//#GearFoundry#: Inspectoractionqueue.count = 3
//#GearFoundry#: Pending Action MoveItem
//#GearFoundry#: Lightning Assagai
//#GearFoundry#: Time: 6/28/2013 2:34:21 PM
//#GearFoundry#: Inspectoractionqueue.count = 4
//#GearFoundry#: Pending Action OpenContainer
//#GearFoundry#: Corpse of Degenerate Shadow Commander
//#GearFoundry#: Time: 6/28/2013 2:34:22 PM
//#GearFoundry#: Inspectoractionqueue.count = 4
//#GearFoundry#: Pending Action MoveItem
//#GearFoundry#: Lightning Assagai



