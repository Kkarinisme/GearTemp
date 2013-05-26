/*
 * Created by SharpDevelop.
 * User: Paul
 * Date: 5/25/2013
 * Time: 8:00 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace GearFoundry.VTInterop
{
	/// <summary>
	/// Description of VTInterop.
	/// </summary>
	public class VTInterop
	{
//	class VirindiTankLootRuleProcessor : ILootRuleProcessor
//	{
//		struct ItemWaitingForCallback
//		{
//			public readonly ItemInfoIdentArgs ItemInfoIdentArgs;
//			public readonly ItemInfoCallback ItemInfoCallBack;
//
//			public ItemWaitingForCallback(ItemInfoIdentArgs itemInfoIdentArgs, ItemInfoCallback itemInfoCallBack)
//			{
//				ItemInfoIdentArgs = itemInfoIdentArgs;
//				ItemInfoCallBack = itemInfoCallBack;
//			}
//		}
//
//		readonly List<ItemWaitingForCallback> itemsWaitingForCallback = new List<ItemWaitingForCallback>();
//
//		public bool GetLootRuleInfoFromItemInfo(ItemInfoIdentArgs itemInfoIdentArgs, ItemInfoCallback itemInfoCallback)
//		{
//			try
//			{
//				if (uTank2.PluginCore.PC.FLootPluginQueryNeedsID(itemInfoIdentArgs.IdentifiedItem.Id))
//				{
//					// public delegate void delFLootPluginClassifyCallback(int obj, LootAction result, bool getsuccess);
//					//uTank2.PluginCore.delFLootPluginClassifyCallback callback = new uTank2.PluginCore.delFLootPluginClassifyCallback(uTankCallBack);
//					//uTank2.PluginCore.PC.FLootPluginClassifyCallback(itemInfoIdentArgs.IdentifiedItem.Id, callback);
//
//					ItemWaitingForCallback itemWaitingForCallback = new ItemWaitingForCallback(itemInfoIdentArgs, itemInfoCallback);
//					itemsWaitingForCallback.Add(itemWaitingForCallback);
//
//					uTank2.PluginCore.PC.FLootPluginClassifyCallback(itemInfoIdentArgs.IdentifiedItem.Id, uTankCallBack);
//				}
//				else
//				{
//					uTank2.LootPlugins.LootAction result = uTank2.PluginCore.PC.FLootPluginClassifyImmediate(itemInfoIdentArgs.IdentifiedItem.Id);
//
//					itemInfoCallback(itemInfoIdentArgs, !result.IsNoLoot, result.IsSalvage, result.RuleName);
//				}
//			}
//			catch (NullReferenceException) // Virindi tank probably not loaded.
//			{
//				return false;
//			}
//
//			return true;
//		}
//
//		void uTankCallBack(int obj, uTank2.LootPlugins.LootAction result, bool getsuccess)
//		{
//			try
//			{
//				if (!getsuccess)
//					return;
//
//				for (int i = 0 ; i < itemsWaitingForCallback.Count ; i++)
//				{
//					ItemWaitingForCallback itemWaitingForCallback = itemsWaitingForCallback[i];
//
//					if (itemWaitingForCallback.ItemInfoIdentArgs.IdentifiedItem.Id == obj)
//					{
//						itemsWaitingForCallback.RemoveAt(i);
//
//						itemWaitingForCallback.ItemInfoCallBack(itemWaitingForCallback.ItemInfoIdentArgs, !result.IsNoLoot, result.IsSalvage, result.RuleName);
//
//						break;
//					}
//				}
//			}
//			catch (Exception ex) { Debug.LogException(ex); }
//		}
//	}
	}
}
