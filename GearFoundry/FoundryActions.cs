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
		private List<FoundryAction> MasterActionList = new List<FoundryAction>();
		private List<LootObject> MasterObjectList = new List<LootObject>();
		
		internal class FoundryAction
		{
			internal bool Pending = false;
			internal FoundryActionTypes Action = FoundryActionTypes.None;
			internal DateTime StartAction = DateTime.MinValue;
			internal int ActionTarget = 0; 
			internal List<int> WaitingTargets = new List<int>();
		}
				
		internal enum FoundryActionTypes
		{
			None,
			CombatState,
			Portal,
			Open,
			Move,
			Pick,
			Desiccate,
			Ring,
			Cut,
			Salvage,
			SalvageCombine,
			Read,
			ManaStone,
			Reveal,
			MoteCombine,
			Stack,
			Trade,
			Sell
		}

		private void SubscribeFoundryActions()
		{
			try
			{
				foreach(FoundryActionTypes act in Enum.GetValues(typeof(FoundryActionTypes)))
				{
					FoundryAction fa = new FoundryAction();
					fa.Action = act;
					MasterActionList.Add(fa);
				}
			}catch(Exception ex){LogError(ex);}
		}
		
		private void InitiateFoundryActions(object sender, EventArgs e)
		{
			try
			{

			}catch(Exception ex){LogError(ex);}
		}
		

		
	}
}
