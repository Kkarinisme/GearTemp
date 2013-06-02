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
    	static internal FileService FileService;
        
        //Operational flags.  Not saved across sessions
        private bool mCharacterLoginComplete;

        private System.Windows.Forms.Timer MasterTimer;
        public static string Log;

        static PluginCore Instance;
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
        
        
        //Fellowship Packets
        private const int GE_ADD_FELLOWMEMBER = 0x2C0;
        private const int GE_FELLOWSHIP_MEMBER_QUIT = 0x00A3;
        private const int GE_FELLOWSHIP_MEMBER_DISMISSED = 0x00A4;
		private const int GE_CREATE_FELLOWSHIP = 0x02BE;
		private const int GE_DISBAND_FELLOWSHIP = 0x02BF;
		        
        XDocument xdoc = null;
        XDocument xdocToonSettings = null;
        XDocument xdocGenSettings = null;
        XDocument xdocSwitchGearSettings = null;
        XDocument xdocMobs = null;
        XDocument xdocTrophies = null;
        XDocument xdocSalvage = null;
        XDocument xdocRules = null;
        private XDocument xdocGenInventory = null;
        private XDocument xdocToonInventory = null;

        XElement el = null;

        //Filenames used in GearFoundry that correspond to xdocuments

        private string GearDir = null;
        private string currDir = null;
        private string pathToToon = null;
        private string toonDir = null;
        private string mobsFilename = null;
        private string trophiesFilename = null;
        private string salvageFilename = null;
        private string rulesFilename = null;
        private string tempFilename = null;
        private string spellsFilename = null;
        private string switchGearSettingsFilename = null;
        private string genSettingsFilename = null;
        private string toonSettingsFilename = null;
        private string inventoryFilename = null;
        private string armorFilename = null;
        private string genArmorFilename = null;
        private string holdingArmorFilename = null;
        private string genInventoryFilename = null;
        private string holdingInventoryFilename = null;
        private string holdingStatsFilename = null;
        private string statsFilename = null;
        private string allStatsFilename = null;
        private string quickSlotsvFilename = null;
        private string quickSlotshFilename = null;



        int mitem;
        Int32 mMaxLoot;
        private int mintvalue;
        private string sname;
        private string sinput;
        private bool mexact = false;
        private bool mchecked = false;
        private bool menabled = false;
        bool mgoon;
        bool mgoonInv;

        int nNextRuleNum;
        bool bRuleEnabled = false;
        int nRuleNum;
        int nRulePriority = 0;
        string sRuleAppliesTo = "";
        string sRuleName = "";
        string sRuleDescr = "";
        string sRuleKeyWords = "";
        string sRuleKeyWordsNot = "";
        int nRuleArcaneLore = 0;
        int nRuleBurden = 0;
        int nRuleValue = 0;
        double nRuleWork = 0;
        int nRuleWieldReqValue = 0;
        int nRuleWieldLevel = 0;
        int nRuleItemLevel = 0;
        int nRuleMinArmorLevel = 0;
        //bool bRuleTradeBotOnly = false;
        //bool bRuleTradeBot = false;
        int nRuleWieldAttribute = 0;
        int nRuleMasteryType = 0;
        string sRuleDamageTypes = "";
        int nRuleMcModAttack = 0;
        int nRuleMeleeD = 0;
        int nRuleMagicD = 0;
        string sRuleReqSkill = "";
        string sRuleReqSkilla = "";
        string sRuleReqSkillb = "";
        string sRuleReqSkillc = "";
        string sRuleReqSkilld = "";
        string sRuleMinMax = "";
        string sRuleMinMaxa = "";
        string sRuleMinMaxb = "";
        string sRuleMinMaxc = "";
        string sRuleMinMaxd = "";
        string sRuleWeapons = "";
        string sRuleWeaponsa = "false";
        string sRuleWeaponsb = "false";
        string sRuleWeaponsc = "false";
        string sRuleWeaponsd = "false";
        string sRuleMSCleave = "";
        string sRuleMSCleavea = "false";
        string sRuleMSCleaveb = "false";
        string sRuleMSCleavec = "false";
        string sRuleMSCleaved = "false";
        string myvara;
        string myvarab;
        string myvarac;
        string myvarad;
        string sRuleArmorType = "";
        string sRuleArmorSet = "";
        string sRuleArmorCoverage = "";
        string sRuleCloakSets = "";
        string sRuleCloakSpells = "";
        bool bRuleMustBeUnEnchantable = false;
        bool bRuleCloakMustHaveSpell = false;
        bool bRuleRed = false;
        bool bRuleYellow = false;
        bool bRuleBlue = false;
        int nRuleEssMastery = 0;
        string sRuleEssElements = "";
        int nRuleEssLevel = 0;
        int nRuleEssDamageLevel = 0;
        int nRuleEssCDLevel = 0;
        int nRuleEssCRLevel = 0;
        int nRuleEssDRLevel = 0;
        int nRuleEssCritLevel = 0;
        int nRuleEssCritDamResLevel = 0;
        bool bRuleFilterLegend = true;
        bool bRuleFilterEpic = true;
        bool bRuleFilterMajor = true;
        bool bRuleFilterlvl8 = true;
        bool bRuleFilterlvl7 = true;
        bool bRuleFilterlvl6 = true;
        int nRuleMustHaveSpell = 0;
        string sRuleSpells = "";
        int nRuleNumSpells = 0;
        int nid;
        int nusearrowid;
        int arrowtimer;
        int nspells;




        //  private string elname;


        //used in inventory functions
        private XDocument newDoc = new XDocument();

        private static bool getBurden = false;

        //used in inventory functions
        private string inventorySelect;

        // Name of toon currently playing
        private string toonName;
        // Name of toon in inventory program whose inventory item is being identified
        private string toonInvName;
        private string world;

        private static XElement element = null;
        private static IEnumerable<XElement> childElements = null;
        private static IEnumerable<XElement> elements = null;



        //used by both the inventory and armor programs to hold current object being processed
        private WorldObject currentobj;

        private string fn;
        private List<string> moldObjsID = new List<string>();
        private List<WorldObject> mWaitingForID;
        private List<WorldObject> mWaitingForArmorID;

        private List<WorldObject> mIdNotNeeded = new List<WorldObject>();
        private List<long> mwaitingforChangedEvent = new List<long>();
        private List<string> mCurrID = new List<string>();
        private List<string> mIcons = new List<string>();

        private static WindowsTimer mWaitingForIDTimer = new WindowsTimer();
        private int m = 500;
        //ToMish:  Are you using these?  If not can they be removed?
        private int k = 0;
        private int n = 0;
        private int mcount = 0;

        //Used in inventory functions
        private static string objSpellXml = null;
        private static string message = null;
        private static string mySelect = null;
        private static string objSalvWork = "None";
        private static string objMatName = null;
        private static long objEmbueTypeInt = 0;
        private static string objEmbueTypeStr = null;
        private static long objWieldAttrInt = 0;
        private static long objDamageTypeInt = 0;
        private static long objLevelInt = 1;
        private static long objCovers = 0;
        private static string objCoversName = null;
        private static string objSpells = null;
        private static Int32 objIcon;
        private static long objArmorLevel = 1;
        private static long objArmorSet = 0;
        private static string objArmorSetName = null;
        private static long objMat = 0;
        private static long objMagicDamageInt = 0;
        private static string objDamageType = null;
        private static double objDVar = 0;
        private static long objMaxDamLong = 0;
        private static string objMinDam = null;

        private static string objClassName = "None";
        private static int objClass = 0;
        private string objName = null;
        private static int objID = 0;
        string objProts;
        string objAl;
        string objWork;
        string objTinks;
        string objLevel;
        string objMissD;
        string objManaC;
        string objMagicD;
        string objMelD;
        string objElemvsMons;
        string objMaxDam;
        string objAttack;
        string objVar;
        string objBurden;
        string objStack;

        
        //variables used to hold settings
        private static bool bquickSlotsEnabled;
        private static bool bquickSlotsvEnabled;
        private static bool bquickSlotshEnabled;
        

      private static bool bvulnedIconsEnabled;
        
        private static bool binventoryEnabled;
        private static bool binventoryBurdenEnabled;
        private static bool binventoryCompleteEnabled;
        private static bool binventoryWaitingEnabled;
        //private static bool bsalvageCombEnabled;
        private static bool btoonStatsEnabled;
        private static bool btoonArmorEnabled;
        private static bool bCorpseHudEnabled;
 
        //Loot Flags
        private static bool bGearInspectorEnabled;

        //Butler Flags
        private static bool bGearButlerEnabled;
        
        private static bool bCombatHudEnabled;

        //Misc flags
        private static bool bArmorEnabled;
  
		//Landscape Tracker Flags
        private static bool bLandscapeHudEnabled;   
        
        //ItemTrackerFlags            
        private static bool bItemHudEnabled = true;
		private static bool bReportItemStrings = true;
		private static int mLootManaMinimum = 5000;
		private static int mLootValMinimum = 100000;
		private static double mLootValBurdenRatioMinimum = 20;


        //variables used in toon statistics program
        private XDocument xDocStats = new XDocument();
        private XDocument xDocAllStats = new XDocument();

        //gearmisc variables
        private static bool bMuteSounds = false;
        private static Decal.Interop.D3DService.CD3DObj mMarkObject;
        private Decal.Interop.D3DService.ID3DService mD3DService;

        private static Decal.Interop.D3DService.eShape mArrow;

        //used with switch in util for determining sound to play
        private static int mSound = 0;
        private static System.IO.Stream s;


    }    
}
        


