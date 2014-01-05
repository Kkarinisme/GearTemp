using System;
using System.Collections.Generic;
using System.Text;
using VirindiViewService;
using MyClasses.MetaViewWrappers;
using System.Drawing;
using Decal.Adapter;
using Decal.Adapter.Wrappers;
using Decal.Interop.D3DService;
using Decal.Filters;
using System.ComponentModel;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Threading;
using WindowsTimer = System.Windows.Forms.Timer;


// nonlocal variables will be placed here

namespace GearFoundry
{

    public partial class PluginCore : Decal.Adapter.PluginBase
    {
    	static internal FileService fileservice;
        
        //Operational flags.  Not saved across sessions

        private System.Windows.Forms.Timer MasterTimer = new System.Windows.Forms.Timer();
        public static string Log;

        public static PluginCore Instance;
        static PluginHost host;      
        
        //Interop Codes
		private const int NOTIFYLINK_ID = 221112;
		private const int GOARROWLINK_ID = 110011;
	
		//EchoFilter decoding constants
		
		public class EchoConstants
		{
			public const int AC_DESTROY_OBJECT = 0x24;
			public const int AC_ADJUST_STACK = 0x197;
			public const int AC_PLAYER_KILLED = 0x19E;
	        public const int AC_SET_OBJECT_LINK = 0x2DA;
	        public const int AC_GAME_EVENT = 0xF7B0;
	        public const int AC_GAME_ACTION = 0xF7B1;
	        public const int AC_CREATE_OBJECT = 0xF745;
	        public const int AC_SET_OBJECT_DWORD = 0x2CE;
	        public const int AC_CLOSE_CONTAINER = 0x0052;
	        public const int AC_APPLY_VISUALSOUND = 0xF755;
	        public const int AC_MOVE_OBJECT_INTO_INVENTORY = 0xF74A;
	        
	        public const int GE_MESSAGE_BOX = 0x4;
	        
	        public const int GE_SETPACK_CONTENTS = 0x196;
	        public const int GE_IDENTIFY_OBJECT = 0xC9;
	        public const int GE_APPROACH_VENDOR = 0x62;
	        public const int GE_FAILURE_TO_GIVE_ITEM = 0xA0;
	        public const int GE_READY_PREV_ACTION_COMPLETE = 0x1C7;
	        public const int GE_ENTER_TRADE = 0x1FD;
	        public const int GE_EXIT_TRADE = 0x1FF;
	        public const int GE_INSERT_INVENTORY_ITEM = 0x22;
	        public const int GE_WEAR_ITEM = 0x23;
	        public const int GE_UPDATE_HEALTH = 0x01C0;
	        
	        public const int GA_USE_ITEM = 0x0036;
	        
	        
	        //Fellowship Packets
	        public const int GE_ADD_FELLOWMEMBER = 0x2C0;
	        public const int GE_FELLOWSHIP_MEMBER_QUIT = 0x00A3;
	        public const int GE_FELLOWSHIP_MEMBER_DISMISSED = 0x00A4;
			public const int GE_CREATE_FELLOWSHIP = 0x02BE;
			public const int GE_DISBAND_FELLOWSHIP = 0x02BF;
		}
		
		internal class GameData
		{
			internal static string[] RingableKeysArray = {"legendary key", "black marrow key", "directive key", "granite key", "mana forge key", "master key", "marble key", 
					"singularity key",	"skeletal falatacot key", "sturdy iron key", "sturdy steel key"};
//			internal static string[] KeyringMatchingArray = {"burning sands", "black marrow", "directive", "granite", "black coral", "master", "marble", "singularity", 
//					"skeletal falatacot", "sturdy iron", "sturdy steel"};
		}
		        
      //  XDocument xdoc = null;
        XDocument xdocGenSettings = null;
        XDocument xdocSwitchGearSettings = null;
        XDocument xdocMobs = null;
        XDocument xdocTrophies = null;
        XDocument xdocSalvage = null;
        XDocument xdocRules = null;
        private XDocument xdocGenInventory = null;
        private XDocument xdocToonInventory = null;

        //Filenames used in GearFoundry that correspond to xdocuments

        private string GearDir = null;
        private string currDir = null;
        private string toonDir = null;
        private string mobsFilename = null;
        private string trophiesFilename = null;
        private string salvageFilename = null;
        private string rulesFilename = null;
        private string tempFilename = null;
        private string switchGearSettingsFilename = null;
        private string genSettingsFilename = null;
        private string toonSettingsFilename = null;
        private string holdingStatsFilename = null;
        private string statsFilename = null;
        private string allStatsFilename = null;
        private string quickSlotsvFilename = null;
        private string quickSlotshFilename = null;
        private string remoteGearFilename = null;
        private string portalGearFilename = null;
        private string programinv = String.Empty;

        private int nitemFontHeight = 0;
        private int nmenuFontHeight = 0;



        int mitem;
        Int32 mMaxLoot;
        
        private string sname = String.Empty;
        private string sinput = String.Empty;
        private bool mexact = false;
        private bool mchecked = false;
        private bool mgoon;
        bool mgoonInv;

		XElement mSelectedRule = null;
		XElement mSelectedSalvage = null;		
        
        int nusearrowid;
        int nTrophyRow;
        int nMobRow;
        string snameorig;




        //  private string elname;


        // Name of toon currently playing
        private string toonName;
        // Name of toon in inventory program whose inventory item is being identified
        private string toonInvName;
        private string world;
        
        private MainSettings mMainSettings = new MainSettings();

        public class MainSettings
        {
        	//Individual Huds
        	public bool bquickSlotsvEnabled = false;
	        public bool bquickSlotshEnabled = false;
	        public bool bGearVisection =  false;
	        public bool bGearInspectorEnabled = false;
	        public bool bGearButlerEnabled = false;
	        public bool bGearTacticianEnabled = false;	
	        public bool bRemoteGearEnabled = false;
			public bool bPortalGearEnabled = false;
			public bool bGearTaskerEnabled = false;
			public bool bGearSenseHudEnabled = false; 
	
			//Inventory flags
	        public bool bArmorHudEnabled = false;
	        public bool binventoryHudEnabled = false;
	        public bool btoonStatsEnabled = false;
	        public bool binventoryEnabled = false;
	        public bool binventoryBurdenEnabled = false;
	        public bool binventoryCompleteEnabled= false;
	        public bool binventoryWaitingEnabled = false;

        }
        
        internal class GearGraphics 
        {
        	
        	internal const int RemoveCircle = 0x60011F8;
        	
        	internal const int ItemUstIcon = 0x60026BA;
			internal const int ItemManaStoneIcon = 0x60032D4;
			internal const int ItemDesiccantIcon = 0x6006C0D;
			
			internal const int RemoteGearIcon = 0x6006E0A;
			internal const int GearBulterIcon = 0x6006533;
			internal const int GearVisectionIcon = 0x6001070;
			internal const int GearInspectorIcon = 0x600218D;
			internal const int GearSenseIcon = 0x6001355;
			internal const int GearTacticianIcon = 0x6004D06;
			internal const int GearInventoryIcon = 0x600127E;
			internal const int GearTaskerIcon = 0x60067EC;
			internal const int GearPortalIcon = 0x60022BE;
			internal const int GearArmorIcon = 0x6001EE2;
			internal const int HoriSwitchGearIcon = 0;
			internal const int VertSwitchGearIcon = 0;
			
			internal const int GB_USE_ICON = 0x6000FB7;
			internal const int GB_GIVE_ICON = 0x60011F7;
			internal const int GB_TRADEVENDOR_ICON = 0x6001080;
			internal const int GB_EQUIPPED_ICON = 0x600136F;
			internal const int GB_UNEQUIPPED_ICON = 0x600127E;
			internal const int GB_MELEE_ICON = 0x60010BC;
			internal const int GB_MISSILE_ICON = 0x6001302;
			internal const int GB_ARMOR_ICON = 0x6000FC7;
			internal const int GB_CASTER_ICON = 0x6001066;
			internal const int GB_KEY_ICON = 0x6001ED3;
			internal const int GB_KEYRING_ICON = 0x6002C3F;
			internal const int GB_LOCKPICK_ICON = 0x6001D6E;
			internal const int GB_MANASTONE_ICON = 0x60032D4;
			internal const int GB_HEALKIT_ICON = 0X60032F3;
			internal const int GB_POTION_ICON = 0x60019FD;
			
			internal const int GB_SELECT = 0x60011F9;
			internal const int GB_BACKGROUND = 0x60011D1;
			
			internal const int GR_LifestoneRecall_ICON = 0x60024E1;
			internal const int GR_HouseRecall_ICON = 0x6001A2A;
			internal const int GR_MansionRecall_ICON = 0x60022DE;
			internal const int GR_AHRecall_ICON = 0x60024DD;
			internal const int GR_Market_ICON = 0x600107E;
	
			
        }
    }    
}
        


