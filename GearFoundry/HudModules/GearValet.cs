
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
using System.Xml.Serialization;
using System.Xml;

namespace GearFoundry
{
	/// <summary>
	/// Description of GearValet.
	/// </summary>
	public partial class PluginCore
	{	
		
		private int ValetCurrentSuit = 0;
		
		public class ValetTicket
		{
			public int ItemId = 0;
			public int SlotId = 0;
		}
		
		public class ValetSuit
		{
			public int TicketStub = 0;
			public string SuitName = String.Empty;
			public int Icon = 0;
			public List<ValetTicket> SuitPieces = new List<ValetTicket>();
		}
					
		private void ValetSuitList_Click(object sender, int row, int col)
		{
			try
			{
				ValetRow = ValetSuitList[row];
				if(col == 0 || col == 1)
				{
					
					ValetCurrentSuit = Convert.ToInt32(((HudStaticText)ValetRow[3]).Text);
					UpdateValetHud();
				}
				if(col == 2)
				{
					GearButlerSettings.ValetSuitList.RemoveAll(x => x.TicketStub == Convert.ToInt32(((HudStaticText)ValetRow[3]).Text));
					if(GearButlerSettings.ValetSuitList.Count > 0 && !GearButlerSettings.ValetSuitList.Any(x => x.TicketStub == ValetCurrentSuit))
					{
						ValetCurrentSuit = GearButlerSettings.ValetSuitList.First().TicketStub;
					}
					else
					{
						ValetCurrentSuit = 0;
					}
					UpdateValetHud();
				}				
			}catch(Exception ex){LogError(ex);}
		}
		
		private void ValetSuitPiecesList_Click(object sender, int row, int col)
		{
			try
			{
				ValetRow = ValetSuitPiecesList[row];
				if(col == 0)
				{
					Core.Actions.SelectItem(Convert.ToInt32(((HudStaticText)ValetRow[3]).Text));
				}
				if(col == 1)
				{
					LootObject tLootObj = new LootObject(Core.WorldFilter[Convert.ToInt32(((HudStaticText)ValetRow[3]).Text)]);
					if(GISettings.GSStrings){tLootObj.GSReportString();}
					if(GISettings.AlincoStrings){tLootObj.LinkString();}
				}
				if(col == 2)
				{
					GearButlerSettings.ValetSuitList[ValetCurrentSuit].SuitPieces.RemoveAll(x => x.ItemId == Convert.ToInt32(((HudStaticText)ValetRow[3]).Text));
					if(GearButlerSettings.ValetSuitList[ValetCurrentSuit].SuitPieces.Count == 0)
					{
						GearButlerSettings.ValetSuitList.RemoveAll(x => x.TicketStub == ValetCurrentSuit);
					}
					if(GearButlerSettings.ValetSuitList.Count > 0 && !GearButlerSettings.ValetSuitList.Any(x => x.TicketStub == ValetCurrentSuit))
					{
						ValetCurrentSuit = GearButlerSettings.ValetSuitList.First().TicketStub;
					}
					else
					{
						ValetCurrentSuit = 0;
					}
				}
				UpdateValetHud();
				
			}catch(Exception ex){LogError(ex);}		
		}
				
		private void UpdateValetHud()
		{
			try
			{
				ValetSuitList.ClearRows();
				
				if(GearButlerSettings.ValetSuitList.Count == 0){return;}
				
				foreach(ValetSuit vs in GearButlerSettings.ValetSuitList)
				{
					ValetRow = ValetSuitList.AddRow();
					((HudPictureBox)ValetRow[0]).Image = vs.Icon + 0x6000000;
                    ((HudStaticText)ValetRow[1]).Text = vs.SuitName;
                    if(vs.TicketStub == ValetCurrentSuit) {((HudStaticText)ValetRow[1]).TextColor = Color.Gold;}
                    ((HudPictureBox)ValetRow[2]).Image = GearGraphics.RemoveCircle;
                    ((HudStaticText)ValetRow[3]).Text = vs.TicketStub.ToString();
				}
				
				ValetSuitPiecesList.ClearRows();
				
				foreach(ValetTicket vt in GearButlerSettings.ValetSuitList[ValetCurrentSuit].SuitPieces)
				{
					ValetRow = ValetSuitPiecesList.AddRow();
					((HudPictureBox)ValetRow[0]).Image = Core.WorldFilter[vt.ItemId].Icon + 0x6000000;
					((HudStaticText)ValetRow[1]).Text = Core.WorldFilter[vt.ItemId].Name;
					if(vt.ItemId == Core.Actions.CurrentSelection){((HudStaticText)ValetRow[1]).TextColor = Color.Gold;}
                    ((HudPictureBox)ValetRow[2]).Image = GearGraphics.RemoveCircle;
                    ((HudStaticText)ValetRow[3]).Text = vt.ItemId.ToString();
				}
			}catch(Exception ex){LogError(ex);}
		}
		
				
		private void ValetDisrobe_Hit(object sender, System.EventArgs e)
		{
			try
			{
				int UsedInvCount = Core.WorldFilter.GetByContainer(Core.CharacterFilter.Id).Where(x => x.Values(LongValueKey.EquippedSlots) == 0 && x.Values(LongValueKey.Unknown10) != 56).Count();
				int UnEquipItemsCount = Core.WorldFilter.GetInventory().Where(x => x.Values(LongValueKey.EquippedSlots) !=  0).Count();
				int SlotsLeftOver = 102 - UnEquipItemsCount - UsedInvCount;
				
				if(SlotsLeftOver < 0)
				{
					WriteToChat("You need to clear " + SlotsLeftOver + " slots in your main pack.");
					return;
				}
				
				ValetRemoveList = Core.WorldFilter.GetInventory().Where(x => x.Values(LongValueKey.EquippedSlots) !=  0).OrderBy(x => x.Name).ToList();	
				UpdateValetHud();
			}catch(Exception ex){LogError(ex);}
		}
		
		private void ValetEquipSuit_Hit(object sender, System.EventArgs e)
		{
			try
			{
				int UsedInvCount = Core.WorldFilter.GetByContainer(Core.CharacterFilter.Id).Where(x => x.Values(LongValueKey.EquippedSlots) == 0 && x.Values(LongValueKey.Unknown10) != 56).Count();
				int UnEquipItemsCount = Core.WorldFilter.GetInventory().Where(x => x.Values(LongValueKey.EquippedSlots) !=  0).Count();
				int SlotsLeftOver = 102 - UnEquipItemsCount - UsedInvCount;
				
				if( GearButlerSettings.ValetSuitList.Count == 0)
				{
					WriteToChat("First create some suits.");
					return;
				}
				
				if(SlotsLeftOver < 0)
				{
					WriteToChat("You need to clear " + SlotsLeftOver + " slots in your main pack.");
					return;
				}
								
				ValetRemoveList = Core.WorldFilter.GetInventory().Where(x => x.Values(LongValueKey.EquippedSlots) !=  0).OrderBy(x => x.Name).ToList();
				ValetEquipList = GearButlerSettings.ValetSuitList.Find(x => x.TicketStub == ValetCurrentSuit).SuitPieces.ToList();
				
				UpdateValetHud();
				
			}catch(Exception ex){LogError(ex);}
		}
		
		private void ValetCreateSuit_Hit(object sender, System.EventArgs e)
		{
			try
			{
				if(ValetNameBox.Text == String.Empty)
				{
					WriteToChat("The suit needs a name.  Put a name in the box.");
					return;
				}
				
				if(Core.Actions.CurrentSelection == 0)
				{
					WriteToChat("Select an item to use as the icon for the suit.");
					return;
				}
				
				int nextsuitid = 0;
				
				for(int i = 0; i < GearButlerSettings.ValetSuitList.Count; i++)
				{
					if(GearButlerSettings.ValetSuitList[i].TicketStub == nextsuitid)
					{
						nextsuitid = i + 1;
					}	
				}
				
				ValetSuit nValetSuit = new ValetSuit();
				nValetSuit.TicketStub = nextsuitid;
				nValetSuit.SuitName = ValetNameBox.Text;
				nValetSuit.Icon = Core.WorldFilter[Core.Actions.CurrentSelection].Icon;				

				foreach(WorldObject wo in Core.WorldFilter.GetInventory().Where(x => x.Values(LongValueKey.EquippedSlots) !=  0).OrderBy(x => x.Name).ToList())
				{
					ValetTicket vt = new ValetTicket();
					vt.ItemId = wo.Id;
					vt.SlotId = wo.Values(LongValueKey.EquippedSlots);
					nValetSuit.SuitPieces.Add(vt);
				}
				
				GearButlerSettings.ValetSuitList.Add(nValetSuit);
				
				GearButlerReadWriteSettings(false);
				UpdateValetHud();
				
			}catch(Exception ex){LogError(ex);}
		}
					
			private void ValetProcessRemove()
			{
				try
				{
					if(ValetRemoveList.Count > 0)
					{
						if(ValetRemoveList.First().Values(LongValueKey.EquippedSlots) == 0)
						{
							ValetRemoveList.RemoveAt(0);
						}
						Core.Actions.MoveItem(ValetRemoveList.First().Id, Core.CharacterFilter.Id,0, false);
						return;
					}
						
				}catch(Exception ex){LogError(ex);}
			}
			
			private void ValetProcessEquip()
			{
				try
				{
					if(ValetEquipList.Count > 0)
					{	
						if(Core.WorldFilter[ValetEquipList.First().ItemId].Values(LongValueKey.EquippedSlots) > 0)
						{
								ValetEquipList.RemoveAt(0);
						}
						if(ValetEquipList.First().SlotId == 0x200000 && Core.WorldFilter[ValetEquipList.First().ItemId].ObjectClass == ObjectClass.MeleeWeapon)
						{
							Core.Actions.AutoWield(ValetEquipList.First().ItemId, 1, 0, 1, 0, 0);
						}
						else
						{
							Core.Actions.UseItem(ValetEquipList.First().ItemId,0);
						}
						return;
					}
				}catch(Exception ex){LogError(ex);}
			}
		
	}
}
