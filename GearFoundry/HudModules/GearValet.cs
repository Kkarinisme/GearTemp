
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
		private HudFixedLayout ValetTabLayout = null;
		private HudButton ValetDisrobe = null;
		private HudButton ValetEquipSuit = null;		
		private HudButton ValetCreateSuit = null;
		private HudList ValetSuitList = null;
		private HudList ValetSuitPiecesList = null;
		private HudStaticText ValetTextBoxLabel = null;
		private HudStaticText ValetSuitListLabel = null;
		private HudStaticText ValetSuitPiecesListLabel = null;
		
		private HudList.HudListRowAccessor ValetRow = null;
		private HudTextBox ValetNameBox = null;
		private const int GearValetRemoveCircle = 0x60011F8;
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
		
		private void RenderButlerHudValetTab()
		{
			try
			{   
				int split3horizontal = Convert.ToInt32((double)GearButlerSettings.ButlerHudWidth /(double)3);
				int splithalf = Convert.ToInt32((double)GearButlerSettings.ButlerHudWidth/(double)2);
				int halfsplit3horizontal = Convert.ToInt32((double)split3horizontal/(double)2);
				int splitbottomvertical = Convert.ToInt32(((double)100 - GearButlerSettings.ButlerHudHeight) /2);
				
				
				ValetDisrobe = new HudButton();
				ValetDisrobe.Text = "Disrobe";
				ValetTabLayout.AddControl(ValetDisrobe, new Rectangle(10,5,split3horizontal-20,20));
				
				ValetEquipSuit = new HudButton();
				ValetEquipSuit.Text = "Equip";
				ValetTabLayout.AddControl(ValetEquipSuit, new Rectangle(splithalf - halfsplit3horizontal ,5,split3horizontal-20,20));
				
				ValetCreateSuit = new HudButton();
				ValetCreateSuit.Text = "Create";
				ValetTabLayout.AddControl(ValetCreateSuit, new Rectangle(splithalf + halfsplit3horizontal,5,split3horizontal-20,20));
				
				ValetTextBoxLabel = new HudStaticText();
				ValetTextBoxLabel.Text = "Suit Label:";
				ValetTabLayout.AddControl(ValetTextBoxLabel, new Rectangle(0,30,50,16));
				
				ValetNameBox = new HudTextBox();
				ValetNameBox.Text = String.Empty;
				ValetTabLayout.AddControl(ValetNameBox, new Rectangle(10,55,GearButlerSettings.ButlerHudWidth -20, 20));
				
				ValetSuitListLabel = new HudStaticText();
				ValetSuitListLabel.Text = "Suits:";
				ValetTabLayout.AddControl(ValetSuitListLabel, new Rectangle(0,80,50,16));			
	
				ValetSuitList = new HudList();
				ValetSuitList.AddColumn(typeof(HudPictureBox), 16, null);
				ValetSuitList.AddColumn(typeof(HudStaticText), GearButlerSettings.ButlerHudWidth - 80, null);
				ValetSuitList.AddColumn(typeof(HudPictureBox), 16, null);
				ValetSuitList.AddColumn(typeof(HudStaticText), 1, null);
				ValetTabLayout.AddControl(ValetSuitList, new Rectangle(0,100,GearButlerSettings.ButlerHudWidth - 20,100));
				
				ValetSuitPiecesListLabel = new HudStaticText();
				ValetSuitPiecesListLabel.Text = "Pieces:";
				ValetTabLayout.AddControl(ValetSuitPiecesListLabel, new Rectangle(0,210,50,16));	
				
				ValetSuitPiecesList = new HudList();
				ValetSuitPiecesList.AddColumn(typeof(HudPictureBox), 16, null);
				ValetSuitPiecesList.AddColumn(typeof(HudStaticText), GearButlerSettings.ButlerHudWidth - 80, null);
				ValetSuitPiecesList.AddColumn(typeof(HudPictureBox), 16, null);
				ValetSuitPiecesList.AddColumn(typeof(HudStaticText), 1, null);
				ValetTabLayout.AddControl(ValetSuitPiecesList, new Rectangle(0, 230 ,GearButlerSettings.ButlerHudWidth - 20,100));
				
				ValetDisrobe.Hit += ValetDisrobe_Hit;
				ValetEquipSuit.Hit += ValetEquipSuit_Hit;
				ValetCreateSuit.Hit += ValetCreateSuit_Hit;
				ValetSuitList.Click += ValetSuitList_Click;
				ValetSuitPiecesList.Click += ValetSuitPiecesList_Click;
				
				ValetTab = true;
				
				UpdateValetHud();
				
				
			}catch(Exception ex){LogError(ex);}
		}
			
		private void DisposeValetTabLayout()
		{
			try
			{
				if(!ValetTab) { return;}
				
				ValetDisrobe.Hit -= ValetDisrobe_Hit;
				ValetEquipSuit.Hit -= ValetEquipSuit_Hit;
				ValetCreateSuit.Hit -= ValetCreateSuit_Hit;
				ValetSuitList.Click -= ValetSuitList_Click;
				ValetSuitPiecesList.Click -= ValetSuitPiecesList_Click;
				
				ValetDisrobe.Dispose();
				ValetEquipSuit.Dispose();
				ValetCreateSuit.Dispose();
				ValetTextBoxLabel.Dispose();
				ValetNameBox.Dispose();				
				ValetSuitListLabel.Dispose();			
				ValetSuitList.Dispose();				
				ValetSuitPiecesListLabel.Dispose();				
				ValetSuitPiecesList.Dispose();
				
				ValetTab = false;
				
			}catch(Exception ex){LogError(ex);}
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
					tLootObj.GSReportString();
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
                    ((HudPictureBox)ValetRow[2]).Image = GearValetRemoveCircle;
                    ((HudStaticText)ValetRow[3]).Text = vs.TicketStub.ToString();
				}
				
				foreach(ValetTicket vt in GearButlerSettings.ValetSuitList[ValetCurrentSuit].SuitPieces)
				{
					ValetRow = ValetSuitPiecesList.AddRow();
					((HudPictureBox)ValetRow[0]).Image = Core.WorldFilter[vt.ItemId].Icon + 0x6000000;
					((HudStaticText)ValetRow[1]).Text = Core.WorldFilter[vt.ItemId].Name;
					if(vt.ItemId == Core.Actions.CurrentSelection){((HudStaticText)ValetRow[1]).TextColor = Color.Gold;}
                    ((HudPictureBox)ValetRow[2]).Image = GearValetRemoveCircle;
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
				
				List<WorldObject> newsuitobjects = Core.WorldFilter.GetInventory().Where(x => x.Values(LongValueKey.EquippedSlots) !=  0).OrderBy(x => x.Name).ToList();

				foreach(WorldObject wo in newsuitobjects)
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
		
//		private void ValetSuit3_Hit(object sender, System.EventArgs e)
//		{
//			try
//			{
//				GearButlerSettings.SuitList3.Clear();
//				
//				List<WorldObject> vs = Core.WorldFilter.GetInventory().Where(x => x.Values(LongValueKey.EquippedSlots) !=  0).OrderBy(x => x.Name).ToList();	
//				foreach(WorldObject wo in vs)
//				{
//					ValetTicket vt = new ValetTicket();
//					vt.ItemId = wo.Id;
//					vt.SlotId = wo.Values(LongValueKey.EquippedSlots);
//					GearButlerSettings.SuitList3.Add(vt);
//				}
//				GearButlerReadWriteSettings(false);
//				UpdateValetHud();
//				
//			}catch(Exception ex){LogError(ex);}
//		}
//		
//		private void ValetSuit0_Hit(object sender, System.EventArgs e)
//		{
//			try
//			{
//				GearButlerSettings.SuitList0.Clear();
//				
//				List<WorldObject> vs = Core.WorldFilter.GetInventory().Where(x => x.Values(LongValueKey.EquippedSlots) !=  0).OrderBy(x => x.Name).ToList();	
//				foreach(WorldObject wo in vs)
//				{
//					ValetTicket vt = new ValetTicket();
//					vt.ItemId = wo.Id;
//					vt.SlotId = wo.Values(LongValueKey.EquippedSlots);
//					GearButlerSettings.SuitList0.Add(vt);
//				}
//				GearButlerReadWriteSettings(false);
//				UpdateValetHud();
//				
//			}catch(Exception ex){LogError(ex);}
//		}
//		
//		private void ValetClearSuit1_Hit(object sender, System.EventArgs e)
//		{
//			try
//			{
//				GearButlerSettings.SuitList1.Clear();
//				GearButlerReadWriteSettings(false);
//				UpdateValetHud();
//			}catch(Exception ex){LogError(ex);}
//		}
//		
//		private void ValetClearSuit2_Hit(object sender, System.EventArgs e)
//		{
//			try
//			{
//				GearButlerSettings.SuitList2.Clear();
//				GearButlerReadWriteSettings(false);
//				UpdateValetHud();
//			}catch(Exception ex){LogError(ex);}
//		}
//				
//		private void ValetClearSuit3_Hit(object sender, System.EventArgs e)
//		{
//			try
//			{
//				GearButlerSettings.SuitList3.Clear();
//				GearButlerReadWriteSettings(false);
//				UpdateValetHud();
//			}catch(Exception ex){LogError(ex);}
//		}
//						
//		private void ValetClearSuit0_Hit(object sender, System.EventArgs e)
//		{
//			try
//			{
//				GearButlerSettings.SuitList0.Clear();
//				GearButlerReadWriteSettings(false);
//				UpdateValetHud();
//			}catch(Exception ex){LogError(ex);}
//		}
//		
//		private void ValetEquipSuit1_Hit(object sender, System.EventArgs e)
//		{
//			try
//			{
//				ValetRemoveList = Core.WorldFilter.GetInventory().Where(x => x.Values(LongValueKey.EquippedSlots) !=  0).OrderBy(x => x.Name).ToList();
//				ValetEquipList = GearButlerSettings.SuitList1.ToList();
//			}catch(Exception ex){LogError(ex);}
//		}
//		
//		private void ValetEquipSuit2_Hit(object sender, System.EventArgs e)
//		{
//			try
//			{
//				ValetRemoveList = Core.WorldFilter.GetInventory().Where(x => x.Values(LongValueKey.EquippedSlots) !=  0).OrderBy(x => x.Name).ToList();
//				ValetEquipList = GearButlerSettings.SuitList2.ToList();
//			}catch(Exception ex){LogError(ex);}
//		}
//		
//		private void ValetEquipSuit3_Hit(object sender, System.EventArgs e)
//		{
//			try
//			{
//				ValetRemoveList = Core.WorldFilter.GetInventory().Where(x => x.Values(LongValueKey.EquippedSlots) !=  0).OrderBy(x => x.Name).ToList();
//				ValetEquipList = GearButlerSettings.SuitList3.ToList();
//			}catch(Exception ex){LogError(ex);}
//		}
//		
//		private void ValetEquipSuit0_Hit(object sender, System.EventArgs e)
//		{
//			try
//			{
//				ValetRemoveList = Core.WorldFilter.GetInventory().Where(x => x.Values(LongValueKey.EquippedSlots) !=  0).OrderBy(x => x.Name).ToList();
//				ValetEquipList = GearButlerSettings.SuitList0.ToList();
//			}catch(Exception ex){LogError(ex);}
//		}
			
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
