using System;
using System.Xml;
using System.Xml.Serialization;
using System.Globalization;
using System.IO;
using System.Collections.ObjectModel;
using System.Text;
using Decal.Filters;
using Decal.Adapter;
using Decal.Adapter.Wrappers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GearFoundry
{
	public partial class PluginCore
	{
		public class MonsterObject
		{
			private WorldObject wo;		
			public MonsterObject(WorldObject obj)
			{
				wo = obj;
			}		
			public MonsterObject()
			{
			}
			public int Id 
			{
				get { return wo.Id; }
			}
			public bool HasIdData 
			{
				get { return wo.HasIdData; }
			}
			public int Icon 
			{
				get { return wo.Icon; }
			}	
			public string Name
			{
				get { return wo.Name; }
			}
			public Decal.Adapter.Wrappers.ObjectClass ObjectClass 
			{
				get { return wo.ObjectClass; }
			}
			public bool isvalid
			{
				get
				{
					if(wo != null) {return true;}
					else{return false;}
				}
			}
			
			public double DistanceAway;
			
			public List<DebuffSpell> DebuffSpellList = new List<DebuffSpell>();
						
			public class DebuffSpell
			{
				public int SpellId = 0;
				public DateTime SpellCastTime = DateTime.Now;
				public double SecondsRemaining = 0;
			}
			
			private int mHealthMax = 0;
			private int mHealthCurrent= 0;
			private int mHealthRemaining = 100;
			
			public int HealthRemaining
			{
				get {return mHealthRemaining;}
				set {mHealthRemaining = value;}
			}
			
			public int HealthMax {
				get { return mHealthMax; }
				set { mHealthMax = value; }
			}		
			public int HealthCurrent {
				get { return mHealthCurrent; }
				set { mHealthCurrent = value; }
			}
			
				public string TruncateName()
			{
				try
				{	
					if(wo.Name.Length > 10)
					{
						string ReturnString = wo.Name;
						if(ReturnString.Contains("of "))
						{
							ReturnString = ReturnString.Replace("of ","");
						}
						if(ReturnString.Length > 10)
						{
							if(ReturnString.Contains("a"))
							{
								ReturnString = ReturnString.Replace("a", "");
							}
							if(ReturnString.Contains("e"))
							{
								ReturnString = ReturnString.Replace("e", "");
							}
							if(ReturnString.Contains("i"))
							{
								ReturnString = ReturnString.Replace("i", "");
							}
							if(ReturnString.Contains("o"))
							{
								ReturnString = ReturnString.Replace("o", "");
							}
							if(ReturnString.Contains("u"))
							{
								ReturnString = ReturnString.Replace("u", "");
							}
							if(ReturnString.Length > 10)
							{
								if(ReturnString.Contains(" "))
								{
									
									string[] splitstring = ReturnString.Split(' ');
									ReturnString = String.Empty;
									foreach(string piece in splitstring)
									{
										if(piece.Length > 3)
										{
											ReturnString += piece.Substring(0,3) + ". ";
										}
										else
										{
											ReturnString += piece + " ";
										}
									}
									return ReturnString;
								}
								else
								{
									return ReturnString.Substring(0,10) + ".";
								}
							}
							else
							{
								return ReturnString;
							}	
						}
						else
						{
							return ReturnString;
						}
					}
					else
					{
						return wo.Name;
					}
				}catch(Exception ex){LogError(ex); return String.Empty;}
			}
			

		}
	}
}
//			These could be included if ever needed.  Skeleton code for reading them from filter is in CombatHud.cs
//			private int mStaminaMax;
//			private int mStaminaCurrent;
//			private int mManaMax;
//			private int mManaCurrent;
//			public int StaminaMax{
//				get { return mStaminaMax; }
//				set { mStaminaMax = value; }
//			}
//			public int StaminaCurrent{
//				get { return mStaminaCurrent; }
//				set { mStaminaCurrent = value; }
//			}
//			public int ManaMax{
//				get { return mManaMax; }
//				set { mManaMax = value; }
//			}
//			public int ManaCurrent{
//				get { return mManaCurrent; }
//				set { mManaCurrent = value; }
//			}
