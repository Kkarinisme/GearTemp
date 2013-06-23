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
		private DateTime dtInspectorLastAction;		
		private List<int> CombineSalvageWOList = new List<int>();
		
		private void FireInspectorActions()
		{
			try
			{		
				dtInspectorLastAction = DateTime.Now;
				
				if(Core.Actions.CombatMode != CombatState.Peace)
				{
					Core.RenderFrame += RenderFrame_PeaceMode;
					return;
				}
				else if(ItemHudMoveQueue.Count > 0)
				{
					if(!Host.Underlying.Hooks.IsValidObject(ItemHudMoveQueue.First().Id) || 
					   Core.WorldFilter.GetInventory().Any(x => x.Id == ItemHudMoveQueue.First().Id) ||
					   AutoDeQueue.Contains(ItemHudMoveQueue.First().Id))
					{
						ItemHudMoveQueue.Dequeue();
						QueueMisFiredAction();
						return;
					}

					Core.RenderFrame += RenderFrame_InspectorMoveAction;
					return;
				}
				else if(DesiccateItemsQueue.Count > 0)
				{
					Core.RenderFrame += RenderFrame_DesiccateItem;
					return;
				}
				else if(ManaTankQueue.Count > 0)
				{
					Core.RenderFrame += RenderFrame_DrainManaTank;
					return;
				}
				else if(KeyItemsQueue.Count > 0)
				{
					Core.RenderFrame += RenderFrame_RingKeys;
					return;
				}
				else if(SalvageItemsQueue.Count > 0)
				{
					Core.RenderFrame += RenderFrame_InspectorUseUst;
					return;
				}
				else if(SalvageCreatedQueue.Count > 0)
				{
					Core.RenderFrame += RenderFrame_InspectorCombineAction;
					return;
				}
				else
				{
					
					return;
				}
			}catch(Exception ex){LogError(ex);}
		}
		
		private void QueueMisFiredAction()
		{
			try
			{
				WriteToChat("Misfired");
			}catch(Exception ex){LogError(ex);}
			
		}
		
		private bool ChangingCombatMode = false;
		private void RenderFrame_PeaceMode(object sender, EventArgs e)
		{
			try
			{	
				if((DateTime.Now - dtInspectorLastAction).TotalMilliseconds < 100){return;}
				else
				{
					Core.RenderFrame -= RenderFrame_PeaceMode; 
					dtInspectorLastAction = DateTime.Now;
				}
				
				ChangingCombatMode = true;
				Core.Actions.SetCombatMode(CombatState.Peace);
				
			}catch(Exception ex){LogError(ex);}
		}
		
		private void ItemTracker_ActionComplete(object sender, EventArgs e)
		{
			try
			{
				if(ChangingCombatMode)
				{
					if(Core.Actions.CombatMode == CombatState.Peace)
					{
						ChangingCombatMode = false;
						FireInspectorActions();
					}
				}	
			}catch(Exception ex){LogError(ex);}
		}
			
		private void RenderFrame_InspectorMoveAction(object sender, System.EventArgs e)
		{
			try
			{
				if((DateTime.Now - dtInspectorLastAction).TotalMilliseconds < 100){return;}
				else
				{
					Core.RenderFrame -= RenderFrame_InspectorMoveAction;
					dtInspectorLastAction = DateTime.Now;
				}
				
				Core.Actions.UseItem(ItemHudMoveQueue.First().Id, 0);
				
				//Listens in Change object for itemchanged, dequeues, then fires inspector action
				
			}catch(Exception ex){LogError(ex);}			
		}
		
		private int ListenForContainerID = 0;
		private void RenderFrame_ReopenContainer(object sender, EventArgs e)
		{
			try
			{
				if((DateTime.Now - dtInspectorLastAction).TotalMilliseconds < 100) {return;}
				else
				{
					Core.RenderFrame -= RenderFrame_ReopenContainer;
					dtInspectorLastAction = DateTime.Now;
				}
				ListenForContainerID = ItemHudMoveQueue.First().Container;
				Core.Actions.UseItem(ItemHudMoveQueue.First().Container, 0);		
			}catch(Exception ex){LogError(ex);}
		}
		
		//Processing of Salvage			
		private void RenderFrame_InspectorUseUst(object sender, EventArgs e)
		{
			try
			{
				if((DateTime.Now - dtInspectorLastAction).TotalMilliseconds < 100) {return;}
				else
				{
					dtInspectorLastAction = DateTime.Now;
					Core.RenderFrame -= RenderFrame_InspectorUseUst;		
				}
						
				Core.Actions.UseItem(Core.WorldFilter.GetInventory().Where(x => x.Name == "Ust").First().Id,0);
				
				dtInspectorLastAction = DateTime.Now;
				Core.RenderFrame += RenderFrame_InspectorSalvageAction;	
				
			}catch(Exception ex){LogError(ex);}		
		}
		
		private void RenderFrame_InspectorSalvageAction(object sender, EventArgs e)
		{
			try
			{
				if((DateTime.Now - dtInspectorLastAction).TotalMilliseconds < 200) {return;}
				else
				{
					Core.RenderFrame -= RenderFrame_InspectorSalvageAction;	
					dtInspectorLastAction = DateTime.Now;
				}
				if(SalvageItemsQueue.Count == 0){return;}
				
				Core.Actions.SalvagePanelAdd(SalvageItemsQueue.Dequeue().Id);
				Core.Actions.SalvagePanelSalvage();
			
				//Listens in ObjectCreated for the creation of salvage, then fires InspectorAction
				//Process Items List should remove from ItemDestroyed
			
			}catch(Exception ex){LogError(ex);}	
		}		
		
		private void ItemTrackerActions_ObjectCreated(object sender, CreateObjectEventArgs e)
		{
			try
			{
				if(e.New.ObjectClass == ObjectClass.Salvage)
				{
					if(!SalvageCreatedQueue.Any(x => x.Id == e.New.Id))
					{
						if(SalvageCreatedQueue.Count == 0)
						{
							SalvageCreatedQueue.Enqueue(e.New);
							FireInspectorActions();
						}
						else
						{
							SalvageCreatedQueue.Enqueue(e.New);
						}
						return;	
					}
				}
			}catch(Exception ex){LogError(ex);}
		}
		
		private void RenderFrame_InspectorCombineAction(object sender, EventArgs e)
		{
			try
			{
				if((DateTime.Now - dtInspectorLastAction).TotalMilliseconds < 100) {return;}
				else
				{
					dtInspectorLastAction = DateTime.Now;
					Core.RenderFrame -= RenderFrame_InspectorCombineAction;
				}
				
				WriteToChat("Combine Fired");
				
				ScanInventoryForSalvageBags();
				
				WorldObject CurrentSalvage = SalvageCreatedQueue.Dequeue();
				
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
						foreach(int salvageid in CombineSalvageWOList)
						{
							Core.Actions.SalvagePanelAdd(salvageid);
						}
						Core.Actions.SalvagePanelSalvage();
					}
					CombineSalvageWOList.Clear();	
				}
				
				FireInspectorActions();
				
			}catch(Exception ex){LogError(ex);}
		}
		
		//Processing of ManaTankQueue		
		private void RenderFrame_DrainManaTank(object sender, System.EventArgs e)
		{
			try
			{
				if((DateTime.Now - dtInspectorLastAction).TotalMilliseconds < 100) {return;}
				else
				{
					dtInspectorLastAction = DateTime.Now;
					Core.RenderFrame -= RenderFrame_DrainManaTank;		
				}
				
				if(Core.WorldFilter.GetInventory().Where(x => x.ObjectClass == ObjectClass.ManaStone && x.Values(LongValueKey.IconOutline) == 0).Count() > 0)
				{
					WriteToChat("mana tank selected");
					Core.Actions.SelectItem(ManaTankQueue.Dequeue().Id);
					dtInspectorLastAction = DateTime.Now;
					Core.RenderFrame += RenderFrame_UseManaStone;	
				}
				else
				{
					WriteToChat("No empty manastones available!");
					ManaTankQueue.Dequeue();
					FireInspectorActions();
				}				
			}catch(Exception ex){LogError(ex);}
			
		}
		
		private void RenderFrame_UseManaStone(object sender, System.EventArgs e)
		{
			try
			{
				if((DateTime.Now - dtInspectorLastAction).TotalMilliseconds < 100) {return;}
				else
				{
					dtInspectorLastAction = DateTime.Now;
					Core.RenderFrame -= RenderFrame_UseManaStone;		
				}
				
				Core.Actions.UseItem(Core.WorldFilter.GetInventory().Where(x => x.ObjectClass == ObjectClass.ManaStone && x.Values(LongValueKey.IconOutline) == 0).First().Id,1);
				


				FireInspectorActions();
			
			}catch(Exception ex){LogError(ex);}		
		}
		
		//Processing of Desiccate Queue.
		private void RenderFrame_DesiccateItem(object sender, EventArgs e)
		{
			try
			{
				if((DateTime.Now - dtInspectorLastAction).TotalMilliseconds < 100) {return;}
				else
				{
					dtInspectorLastAction = DateTime.Now;
					Core.RenderFrame -= RenderFrame_DesiccateItem;
				}
				
				if(Core.WorldFilter.GetInventory().Where(x => x.Name == "Aetheria Desiccant").Count() > 0)
				{
					Core.Actions.SelectItem(DesiccateItemsQueue.Dequeue().Id);
					dtInspectorLastAction = DateTime.Now;
					Core.RenderFrame += RenderFrame_ApplyDesiccant;
				}				
				else
				{
					WriteToChat("You are out of Aetheria Desiccant!");
					DesiccateItemsQueue.Dequeue();
	
						FireInspectorActions();
					
				}	
			}catch(Exception ex){LogError(ex);}
		}
		
		
		private void RenderFrame_ApplyDesiccant(object sender, EventArgs e)
		{
			try
			{
				if((DateTime.Now - dtInspectorLastAction).TotalMilliseconds < 100) {return;}
				else
				{
					dtInspectorLastAction = DateTime.Now;
					Core.RenderFrame -= RenderFrame_ApplyDesiccant;
				}
				
				Core.Actions.UseItem(Core.WorldFilter.GetInventory().Where(x => x.Name == "Aetheria Desiccant").First().Id, 1);
				
	
					FireInspectorActions();
				
								                     
			}catch(Exception ex){LogError(ex);}
			
		}

		private int KeyRingMatchId = 0;
		private void RenderFrame_RingKeys(object sender, EventArgs e)
		{
			try
			{
				if((DateTime.Now - dtInspectorLastAction).TotalMilliseconds < 100) {return;}
				else
				{
					dtInspectorLastAction = DateTime.Now;
					Core.RenderFrame -= RenderFrame_RingKeys;
				}
				
				LootObject currentkey = KeyItemsQueue.Dequeue();
				KeyRingMatchId = MatchKey(currentkey.Name);
				if(MatchedKeyRingId != 0)
				{
					Core.Actions.SelectItem(currentkey.Id);
					dtInspectorLastAction = DateTime.Now;
					Core.RenderFrame += RenderFrame_RingIt;
				}
				else
				{
					WriteToChat("No empty keyring matches.");
	
						FireInspectorActions();
					
					
				}	
			}catch(Exception ex){LogError(ex);}
		}
		
		private void RenderFrame_RingIt(object sender, EventArgs e)
		{
			try
			{
				if((DateTime.Now - dtInspectorLastAction).TotalMilliseconds < 100) {return;}
				else
				{
					dtInspectorLastAction = DateTime.Now;
					Core.RenderFrame -= RenderFrame_RingIt;
				}
				Core.Actions.UseItem(KeyRingMatchId,1);
				

					FireInspectorActions();
				
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
