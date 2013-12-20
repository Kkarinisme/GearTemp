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
		private const int AC_DESTROY_OBJECT = 0x24;
		private const int AC_ADJUST_STACK = 0x197;
		private const int AC_PLAYER_KILLED = 0x19E;
        private const int AC_SET_OBJECT_LINK = 0x2DA;
        private const int AC_GAME_EVENT = 0xF7B0;
        private const int AC_GAME_ACTION = 0xF7B1;
        private const int AC_CREATE_OBJECT = 0xF745;
        private const int AC_SET_OBJECT_DWORD = 0x2CE;
        private const int AC_CLOSE_CONTAINER = 0x0052;
        private const int AC_APPLY_VISUALSOUND = 0xF755;
        private const int AC_MOVE_OBJECT_INTO_INVENTORY = 0xF74A;
        
        private const int GE_MESSAGE_BOX = 0x4;
        
        private const int GE_SETPACK_CONTENTS = 0x196;
        private const int GE_IDENTIFY_OBJECT = 0xC9;
        private const int GE_APPROACH_VENDOR = 0x62;
        private const int GE_FAILURE_TO_GIVE_ITEM = 0xA0;
        private const int GE_READY_PREV_ACTION_COMPLETE = 0x1C7;
        private const int GE_ENTER_TRADE = 0x1FD;
        private const int GE_EXIT_TRADE = 0x1FF;
        private const int GE_INSERT_INVENTORY_ITEM = 0x22;
        private const int GE_WEAR_ITEM = 0x23;
        private const int GE_UPDATE_HEALTH = 0x01C0;
        
        private const int GA_USE_ITEM = 0x0036;
        
        
        //Fellowship Packets
        private const int GE_ADD_FELLOWMEMBER = 0x2C0;
        private const int GE_FELLOWSHIP_MEMBER_QUIT = 0x00A3;
        private const int GE_FELLOWSHIP_MEMBER_DISMISSED = 0x00A4;
		private const int GE_CREATE_FELLOWSHIP = 0x02BE;
		private const int GE_DISBAND_FELLOWSHIP = 0x02BF;
		        
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
        
        public class GearGraphics 
        {
        	
        	public const int RemoveCircle = 0x60011F8;
        	
        	public const int ItemUstIcon = 0x60026BA;
			public const int ItemManaStoneIcon = 0x60032D4;
			public const int ItemDesiccantIcon = 0x6006C0D;
			
			public const int RemoteGearIcon = 0x6006E0A;
			public const int GearBulterIcon = 0x6006533;
			public const int GearVisectionIcon = 0x6001070;
			public const int GearInspectorIcon = 0x600218D;
			public const int GearSenseIcon = 0x6001355;
			public const int GearTacticianIcon = 0x6004D06;
			public const int GearInventoryIcon = 0x600127E;
			public const int GearTaskerIcon = 0x60067EC;
			public const int GearPortalIcon = 0x60022BE;
			public const int GearArmorIcon = 0x6001EE2;
			public const int HoriSwitchGearIcon = 0;
			public const int VertSwitchGearIcon = 0;
	
			
        }
    }    
}
        


