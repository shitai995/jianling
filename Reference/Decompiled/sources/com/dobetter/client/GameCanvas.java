package com.dobetter.client;

import android.graphics.Bitmap;
import android.graphics.Canvas;
import android.graphics.Paint;
import android.support.v4.util.TimeUtils;
import android.support.v4.view.MotionEventCompat;
import android.view.MotionEvent;
import android.view.SurfaceHolder;
import android.view.SurfaceView;
import com.dobetter.action.ActionSet;
import com.dobetter.client.data.AttributeData;
import com.dobetter.client.data.EnemyData;
import com.dobetter.client.data.EquipData;
import com.dobetter.client.data.PropData;
import com.dobetter.client.data.SkillData;
import com.dobetter.client.data.SpriteBaseData;
import com.dobetter.common.Common;
import com.dobetter.common.Font;
import com.dobetter.common.Graphics;
import com.dobetter.common.StyledText;
import com.dobetter.common.Tool;
import com.dobetter.common.XList;
import com.dobetter.map.GameMap;
import com.dobetter.script.Bool;
import com.dobetter.script.DataType;
import com.dobetter.script.Int;
import com.dobetter.script.Interpreter;
import com.dobetter.script.ScriptProcessor;
import com.dobetter.script.Str;
import com.tencent.webnet.PreSMSReturn;
import com.tencent.webnet.WebNetEvent;
import com.tencent.webnet.WebNetInterface;
import com.ty.jl.R;
import java.io.DataInputStream;
import java.io.DataOutputStream;
import java.io.File;
import java.io.FileInputStream;
import java.io.FileOutputStream;
import java.io.InputStream;
import java.lang.reflect.Array;
import java.util.Calendar;
import java.util.Date;
import java.util.Vector;

/* JADX INFO: loaded from: classes.dex */
public class GameCanvas extends SurfaceView implements SurfaceHolder.Callback, ScriptProcessor {
    private static final int AUTO_USE_MEDICINE_SET = 1;
    static final int BAG_VISIBLE_LINE = 4;
    private static final int DROP_INFO_NUM = 3;
    private static final int DROP_POS_H = 3;
    private static final int DROP_POS_INTERVAL = 30;
    private static final int DROP_POS_W = 3;
    public static final byte DRUATION_TIME = 5;
    private static final int FIRE_KEY_HEIGHT = 60;
    static final String FREE_MEMORY = "可用内存：";
    static final int ICON_BG_HEIGHT = 26;
    static final int ICON_BG_WIDTH = 26;
    static final int ICON_HEIGHT = 22;
    static final int ICON_WIDTH = 22;
    private static final String MAIN_RECORD_NAME = "TheSoulOfTheSword";
    static final byte MESSAGE_SCREEN = 0;
    static final byte MESSAGE_WINDOW = 1;
    private static final int MUSIC_SET = 0;
    private static final String RECORD_TITLE = "Title";
    static final int SHOP_VISIBLE_LINE = 5;
    private static final String SMS_RECORD = "SMS";
    private static final byte STATE_ABOUT = 2;
    private static final byte STATE_BLACK_SCREEN_MESSAGE = 24;
    private static final byte STATE_CHOICE = 11;
    private static final byte STATE_EQUIP_PREVIEW = 14;
    private static final byte STATE_EXIT_VALIDATE = 25;
    private static final byte STATE_GAMEEND = 23;
    private static final byte STATE_GAMEOVER = 22;
    private static final byte STATE_GAME_MENU = 18;
    private static final byte STATE_HELP = 1;
    private static final byte STATE_LOAD = 9;
    public static final byte STATE_LOGO = 0;
    private static final byte STATE_MAIN_MENU = 5;
    private static final byte STATE_MAP = 28;
    private static final byte STATE_MESSAGE = 17;
    private static final byte STATE_MINI_MAP = 36;
    private static final byte STATE_MORE = 26;
    private static final byte STATE_MOVING_SPRITE = 20;
    private static final byte STATE_NOTIFY = 16;
    private static final byte STATE_READ_RECORD = 7;
    private static final byte STATE_REINFORCE = 27;
    private static final byte STATE_RETURN_VALIDATE = 37;
    private static final byte STATE_SCENE = 12;
    private static final byte STATE_SCREEN_MOVE = 13;
    private static final byte STATE_SCREEN_STAY = 19;
    private static final byte STATE_SETTING = 3;
    private static final byte STATE_SHOP = 15;
    private static final byte STATE_SMS_CONFIRM = 35;
    private static final byte STATE_SMS_INFO = 32;
    private static final byte STATE_SMS_SHOP = 31;
    private static final byte STATE_SMS_WAITING = 34;
    private static final byte STATE_STARTMUSIC = 4;
    private static final byte STATE_WARNING = 30;
    private static final String UNSAVED = "unsaved";
    public static Bitmap addHPImg;
    public static Bitmap addMPImg;
    public static Bitmap arImg;
    public static ActionSet beatedEffectAS;
    static boolean cdTime;
    public static ActionSet criticalEffectAS;
    static AttributeData[] enemyAttributes;
    static EnemyData[] enemyDatas;
    static EquipData[] equipDatas;
    public static ActionSet faceAS;
    static AttributeData[] famulusDatas;
    public static Font font;
    public static GameCanvas instance;
    public static boolean isHited;
    public static Bitmap loseHPImg0;
    public static Bitmap loseHPImg1;
    public static Bitmap missImg;
    private static byte optionData;
    static PropData[] propDatas;
    static AttributeData roleData;
    static Bitmap signImg;
    static SkillData[] skillDatas;
    static SpriteBaseData[] sprBaseData;
    public static int touchState;
    public static int touchX;
    public static int touchY;
    static boolean turnOn;
    private final int[][] AREA_COORDINATE;
    private final String[] AREA_NAME;
    private ActionSet CGAS;
    private Bitmap[] CGBGImg;
    private int[][] CGBGPos;
    private final int CPID;
    private final int DIR_KEY_HEIGHT;
    private final int[][] DIR_KEY_POSITION;
    private final int DIR_KEY_WIDHT;
    final int DISTANCE;
    final byte DOWN_KEY;
    private int DROP_INFO_TIME;
    final byte DWELLTIME;
    final byte EXP_INCREASE_FRAME_NUM;
    final byte FIRE_KEY;
    private final String GAMEID;
    final byte LEFT_KEY;
    final int OFF_X;
    private int OPTION_HEIGHT;
    private int OPTION_WIDTH;
    final byte RIGHT_KEY;
    private final byte SMS_EQUIP;
    private final byte SMS_EXP;
    private final byte SMS_MONEY;
    private final byte SMS_RELIVE;
    private final byte SMS_SKILL;
    private final byte SMS_STAGE;
    final int STAGE_HEIGHT;
    final int STAGE_WIDTH;
    Bitmap SmsImage;
    private StyledText UIST;
    final byte UP_KEY;
    private int YYY;
    private String aboutStr;
    private DataAccess access;
    private Bitmap arrowImg;
    private Bitmap arrowUpImg;
    private Bitmap attributeUpNumImg;
    private byte bAmount;
    private byte bAreaIndex;
    private byte bCGState;
    private byte bCriticalCounter;
    private byte bCriticalDuration;
    private final byte[] bDistanceX;
    private final byte[] bDistanceY;
    private byte[] bDrawGift;
    byte bEffectCounter;
    private byte bEntryIndex;
    byte bExitCounter;
    private byte bExpCoefficientBySMS;
    private byte bFamulusFaceState;
    byte bFlashCounter;
    private byte bGameSetState;
    public byte bGameState;
    private byte bItemSum;
    byte bKeyPressTipIndex;
    byte bKeyResponsePoint;
    private byte bMenuIndex;
    byte bMiniMapXCount;
    byte bMiniMapYCount;
    private byte bMiscellaneousType;
    private byte bMode;
    private byte bNotifyState;
    private byte bOptionIndex;
    private int bOverCount;
    private int bOverDuration;
    private int bOverIndex;
    private byte bPopUpOptionIndex;
    private byte bPreState;
    private byte bRecordIndex;
    byte bRecoverPercent;
    private byte bSMCount;
    private byte bSMSIndex;
    private byte bSMSShopIndex;
    private byte bSMSType;
    byte[][] bSaveStage;
    private byte bSceneUIOffY;
    byte bShopType;
    private byte bSkillPreviewIndex;
    byte[][][] bStageExit;
    byte bStayCounter;
    private byte bSystemIndex;
    byte bTurtorialState;
    private byte bVibrateType;
    private final byte[] bVx;
    private final byte[] bVy;
    private Bitmap backKey0;
    private Bitmap backKey1;
    Bitmap backgroundImg;
    Bag bag;
    private Bitmap bengKey0;
    private Bitmap bengKey1;
    private int bengX;
    private int bengY;
    String[] blackScreenMessage;
    boolean bool;
    private Bitmap bossHPImg;
    private String branchMissionInfo;
    private String branchMissionTitle;
    public Vector bulletInScreen;
    boolean buyEquip;
    private Vector candidateEquipV;
    private Vector candidateItemV;
    StyledText choiceInfo;
    XList choiceList;
    private Bitmap[] comboImg;
    private Bitmap comboNumImg;
    private Bitmap comboTipImg;
    private final String[] content;
    Bitmap coverImg;
    private int curKey;
    private String currentBGMusic;
    private Byte d;
    private int dirFrame;
    private ActionSet dirSet;
    private int dirX;
    private int dirY;
    int distance_x;
    int distance_y;
    private StyledText[] dropInfo;
    private int[][] dropInfoCount;
    public Vector effectV;
    public Vector enemyInScreen;
    private Equip equipForReinforce;
    Bitmap equipIconImg;
    private Byte f;
    public SpriteBaseData famulusBaseData;
    private Bitmap famulusDurabilityImg;
    private Bitmap famulusExpImg;
    String[] feedTipInfo;
    public Vector fightElement;
    private Bitmap fireKey0;
    private Bitmap fireKey1;
    private int fireX;
    private int fireY;
    Vector fsBaseDataV;
    private Graphics g;
    Bitmap goldImg;
    public Bitmap headImg;
    Bitmap[] headImgs;
    private String helpStr;
    private SurfaceHolder holder;
    private int iArrowX_L;
    private int iArrowX_R;
    private int iArrowY;
    private int iBGColor;
    int iBGImgX1;
    int iBGImgX2;
    final int iBGMoveV;
    public int iBossHP;
    public int iBossMaxHP;
    int iCostOfRepair;
    int iCreateLeavesCounter;
    public int iCurrentStageID;
    public int iEventIndex;
    private int iExpIncrease;
    int iFireCounter;
    private int iGoldBySMS;
    private int iGoldUnsaved;
    int iHPDecrement;
    int iLevelUpExp;
    int iLineIndex;
    int iLineNum;
    private int iLineX1;
    private int iLineX2;
    private int iLoadOverCounter;
    private int iLoadingCounter;
    private int iLogoCounter;
    private int iMainMenuX;
    private int iMainMenuY;
    int iMiniMapHeight;
    int iMiniMapWidth;
    int iMiniMapX;
    int iMiniMapY;
    int iMissionStageIndex;
    private int iPageNum;
    int iPointerX;
    int iPointerY;
    int iRemainingTime;
    private int iRolePreCellX;
    private int iRolePreCellY;
    private int iScreenBackX;
    private int iScreenBackY;
    private int iScreenMoveTimes;
    private int iScreenMoveToX;
    private int iScreenMoveToY;
    private int iScreenMoveTotalTime;
    private int iScreenMoveVX;
    private int iScreenMoveVY;
    private int iScreenStayCounter;
    private int iScreenStayDuration;
    public int iScreenVibrateNums;
    private int iScreenVibrateScope;
    int iShowWidth;
    private int[] iSpecialPropPriceCoefficient;
    private int iStageNameCounter;
    private int iStageNameOffY;
    private int iStageNameWidth;
    int iStopCounter;
    private int iStoryImgY;
    private int iStoryMessageY;
    int iTotalIncreaseExp;
    private int iTotalSum;
    private int iTradeNum;
    private int iTransactionValue;
    int iUIInfoHeight;
    int iUIInfoWidth;
    int iUIInfoX;
    int iUIInfoY;
    private int iVibrateX;
    private int iVibrateY;
    public int iViewMapX;
    public int iViewMapY;
    int iWhiteScreenCounter;
    Bitmap[] iconBGImg;
    private int infoBarCount;
    Bitmap[] infoSMImg;
    final int interval;
    boolean isAllEnemiesKilled;
    private boolean[] isAreaOpened;
    public boolean isAttackable;
    private boolean isBranchMission;
    boolean isChoiceClosable;
    boolean isCountByFrame;
    private boolean isDX;
    boolean isDownReleased;
    public boolean isDrawBossHpBar;
    private boolean isDrawOnLeft;
    private boolean isDropItem;
    boolean isDropVertical;
    boolean isDurabilityNonLoss;
    boolean isDurabilityNonLossForever;
    boolean isDurabilityToZero;
    boolean isExitGameMenu;
    boolean isExpRoom;
    boolean isFalseKeyResponse;
    private boolean isFeed;
    private boolean isFeedOver;
    boolean isFeedTip;
    boolean isFlashEffect;
    boolean isFree;
    boolean isFromMainMenu;
    private boolean isHideShortCutBar;
    private boolean isHideUI;
    private boolean isIntroduct;
    public boolean isKeyLocked;
    boolean isLeftReleased;
    boolean isLoadFinish;
    boolean isLoadingAfterTimeOut;
    private boolean isMaximumPower;
    boolean isOKReleased;
    boolean isOpenAllStage;
    boolean isOpenSound;
    boolean isOtherState;
    boolean isPause;
    private boolean isPopUpOption;
    private boolean isQQ;
    private boolean isReinforceOver;
    private boolean isReturn;
    boolean isRightReleased;
    boolean isRunning;
    private final boolean isSMS;
    private boolean isSale;
    private boolean isScreenMoveWithRole;
    boolean isScreenMoving;
    private boolean isScreenStay;
    boolean isSend;
    private boolean isSendSuccessful;
    private boolean isSetKeyLocked;
    boolean isShowStageInfo;
    private boolean isSkillPreview;
    private boolean isTalking;
    boolean isTimeIn;
    boolean isToLoading;
    public boolean isToURL;
    boolean[] isTurtorialKey;
    boolean isUpReleased;
    boolean isVibrate;
    boolean isWaitforSkillOver;
    private String[] itemOptions;
    int keyCode;
    Vector keyCodeV;
    ActionSet keyPressTipAS;
    Vector keyV;
    private Bitmap killKey;
    private ActionSet killSet;
    private int killX;
    private int killY;
    private Byte l;
    long lMessageTimeMills;
    long lPauseDuration;
    long lPauseTime;
    long lSleepTimeMills;
    long lStageStartTime;
    int lastChoiceIndex;
    private Vector leavesV;
    private String[] loadString;
    private ActionSet loadingAS;
    private Bitmap[] loadingImgs;
    Vector loadingStateV;
    private Bitmap[] logoImg;
    private String mainMissionInfo;
    private String mainMissionTitle;
    private ActionSet mainUIAS;
    public GameMap map;
    ActionSet mapAS;
    Bitmap mapImg;
    private Bitmap mapKey0;
    private Bitmap mapKey1;
    private Vector mapScripts;
    Prop medicine;
    private ActionSet menuAS;
    private int menuButtonX;
    private int menuButtonY;
    private final int menuHeight;
    private Bitmap menuKey0;
    private Bitmap menuKey1;
    private final int menuWidth;
    private final int menuX;
    private final int menuY;
    private StyledText message;
    private Vector messageDurationSet;
    private Vector messageSet;
    private byte messageType;
    private Vector messageTypeSet;
    Bitmap[] miniMapSignImg;
    private int miniMapX;
    private int miniMapY;
    byte minute;
    private int[][] missionHortation;
    private Bitmap moneyImg;
    private int[] movingSpriteDir;
    private int[] movingSpriteDistance;
    private int[] movingSpriteStep;
    private int[] movingSpriteTotalDistance;
    private Bitmap musicSetImg;
    private Bitmap npcHeadImg;
    private Bitmap[] numImg;
    private final String number;
    Bitmap optionArrowImg;
    Bitmap optionFrameImg;
    Bitmap optionImg;
    Bitmap[] optionSMImg;
    private int optionWidth;
    private ActionSet overAS;
    private Paint paint;
    private String port;
    Equip previewEquip;
    Bitmap propIconImg;
    private Byte r;
    Bitmap recommondImg;
    private String[] recordNames;
    String[] recordOptions;
    private String[] recordTitles;
    public Role role;
    public SpriteBaseData roleBaseData;
    private Bitmap roleExpImg0;
    private Bitmap roleExpImg1;
    private Bitmap roleHPImg;
    private Bitmap roleHPImg1;
    private Bitmap roleMPImg;
    private Bitmap roleMPImg1;
    private short sEquipIndex;
    private short sItemIndex;
    short sLevelBeforeFeed;
    String[] sRecordName;
    private short sReinforceItemIndex;
    private short sShopIndex;
    private Vector saleVector;
    String sayName;
    ActionSet sceneUIAS;
    Interpreter script;
    private Vector scriptFileToRunV;
    private byte scriptState;
    private String scrollInfo;
    private final int scrollInfoBarH;
    private final int scrollInfoBarW;
    private int scrollInfoCount;
    private byte scrollInfoState;
    private Vector scrollInfoV;
    private Bitmap scrollSignImg;
    byte second;
    int sendSmsId;
    private Vector shopItems;
    private Bitmap shortCutImg;
    Bitmap skillIconAndroid;
    Bitmap skillIconImg;
    private Bitmap skillKey0;
    private Bitmap skillKey1;
    private int skillX;
    private int skillY;
    SMS sms;
    private int smsId;
    private Bitmap spImg;
    private Vector spriteBelowLayer0;
    private Vector spriteInStage;
    private Vector spriteOnLayer0;
    private Vector spriteOnLayer1;
    private Vector spriteOnLayer2;
    private Sprite[] sprsByScriptControl;
    Vector sprsByScriptControlV;
    private StyledText stHelpAbout;
    int[][][] stageData;
    private String stageName;
    private int stageNameX;
    private int stageNameY;
    Bitmap storyImg;
    private Vector switchV;
    ActionSet talkAS;
    private Bitmap talkImg;
    StyledText talkST;
    private Sprite talkSprite;
    String taskOverScript;
    Vector taskV;
    private MyThread thread;
    String timeOutScript;
    private StyledText tipInfoST;
    Bitmap tipSMImg;
    StyledText tipST;
    private Byte u;
    public Bitmap uiBGImg;
    private Vector unsavedEquipV;
    int weiting;
    private static final byte STATE_CG = -1;
    public static byte curPressedButtonId = STATE_CG;
    public static byte touchProcess = STATE_CG;
    public static int SCREEN_WIDTH = 533;
    public static int SCREEN_HEIGHT = 320;
    public static int FontWidth = Font.fontChinaW;
    public static int FontHeight = Font.fontChinaH;
    public static String IMG_PATH = "/pic/";
    public static String ACTION_PATH = "/action/";
    public static final String[] COLORS = {"ffffff", "00ff00", "00ccff", "ff00ff", "f9a12f", "d70369"};
    private static final int[] value = {8, 4, 2, 1};
    public static final int[][][][] autoDirSet = {new int[][][]{new int[][]{new int[]{0, -1}, new int[]{-1, -1}, new int[]{0, -2}, new int[]{-1, -2}}, new int[][]{new int[]{0, 1}, new int[]{-1, 1}, new int[]{0, 2}, new int[]{-1, 2}}}, new int[][][]{new int[][]{new int[]{0, -1}, new int[]{1, -1}, new int[]{0, -2}, new int[]{1, -2}}, new int[][]{new int[]{0, 1}, new int[]{1, 1}, new int[]{0, 2}, new int[]{1, 2}}}, new int[][][]{new int[][]{new int[]{-1, 0}, new int[]{-1, -1}, new int[]{-2, 0}, new int[]{-2, -1}}, new int[][]{new int[]{1, 0}, new int[]{1, -1}, new int[]{2, 0}, new int[]{2, -1}}}, new int[][][]{new int[][]{new int[]{-1, 0}, new int[]{-1, 1}, new int[]{-2, 0}, new int[]{-2, 1}}, new int[][]{new int[]{1, 0}, new int[]{1, 1}, new int[]{2, 0}, new int[]{2, 1}}}};
    static final byte[] MUSIC_VOLUME = {0, 25, 50, 75, 100};
    public static String[] debugStr = null;
    static String[] SCRIPT_VARIABLES = {"event", "stageID", ""};
    public static int MILLIS_PRE_UPDATE = 80;
    public static int CYCLE_PRE_SECOND = 200;

    public GameCanvas() {
        super(JL01Activity.activity);
        this.spriteInStage = new Vector(80, 10);
        this.spriteBelowLayer0 = new Vector(5, 2);
        this.spriteOnLayer0 = new Vector(5, 2);
        this.spriteOnLayer1 = new Vector(5, 2);
        this.spriteOnLayer2 = new Vector(5, 2);
        this.switchV = new Vector(5);
        this.enemyInScreen = new Vector(10, 5);
        this.bulletInScreen = new Vector(20, 5);
        this.effectV = new Vector(10, 2);
        this.fightElement = new Vector(10, 2);
        this.isSMS = true;
        this.isUpReleased = false;
        this.isDownReleased = false;
        this.isLeftReleased = false;
        this.isRightReleased = false;
        this.isOKReleased = false;
        this.currentBGMusic = "/cover.mid";
        this.scrollInfoBarW = 126;
        this.scrollInfoBarH = Font.fontChinaH + 10;
        this.scrollInfoV = new Vector(10);
        this.EXP_INCREASE_FRAME_NUM = (byte) 15;
        this.keyV = new Vector(5);
        this.UP_KEY = (byte) 0;
        this.DOWN_KEY = (byte) 1;
        this.LEFT_KEY = (byte) 2;
        this.RIGHT_KEY = (byte) 3;
        this.FIRE_KEY = (byte) 4;
        this.u = new Byte((byte) 0);
        this.d = new Byte((byte) 1);
        this.l = new Byte((byte) 2);
        this.r = new Byte((byte) 3);
        this.f = new Byte((byte) 4);
        this.isDX = true;
        this.keyCode = -1;
        this.CGBGPos = (int[][]) Array.newInstance((Class<?>) Integer.TYPE, 2, 2);
        this.bDistanceX = new byte[]{18, -70};
        this.bDistanceY = new byte[]{8, -24};
        this.bVx = new byte[]{1, -4};
        this.bVy = new byte[]{1, -4};
        this.leavesV = new Vector(15);
        this.loadString = new String[]{"普通攻击中按‘后’可使用后跳回避躲开攻击", "强化武器可以使游戏更轻松！", "在展云飞那里可以领取各种支线任务，可能获得珍贵道具哦", "当敌人防御时，对他使用崩击可以破除防御", "去藏剑山庄找古长老可以进入练功场练级，获得双倍经验", "去藏剑山庄找古长老可以进入剑冢打宝，获得强力武器", "铁匠出售的武器数量与主角等级有关，等级越高出售武器越多", "某些武器的技能是随机的，每次获得都不一样", "武器每强化一次，喂给剑灵的经验加1倍", "武器强化到+9喂给剑灵可获12倍经验", "命中与回避比攻击和防御更重要", "等级比敌人低时，命中和防御降低", "练功场最高可享受8倍经验哦！"};
        this.isQQ = true;
        this.recordNames = new String[]{"", ""};
        this.recordTitles = new String[]{"无存档", "存档点"};
        this.sRecordName = new String[]{"record0", "record1"};
        this.YYY = 0;
        this.isOpenSound = true;
        this.bRecoverPercent = (byte) 70;
        this.helpStr = "移动：方向键\n攻击：剑型按钮\n技能：‘技’字按钮\n崩击：‘崩’字按钮\n超杀：‘杀’字按钮\n剑灵合体：点击剑灵头像\n查看地图：右上角指南针按钮\n系统菜单：右上角房屋型按钮\n\n";
        this.aboutStr = "发行商：腾讯\n开发商：北京天娱无限科技有限公司\n客服电话：010-84727011\n邮箱：tianyuwuxian1@163.com\n";
        this.iBGColor = 0;
        this.iBGMoveV = 6;
        this.AREA_COORDINATE = new int[][]{new int[]{129, 279}, new int[]{259, 220}, new int[]{282, 295}, new int[]{216, 144}, new int[]{417, 233}, new int[]{374, 166}, new int[]{348, 99}, new int[]{134, 79}, new int[]{239, 67}, new int[]{88, 202}, new int[]{88, 202}, new int[]{259, 220}, new int[]{134, 79}};
        this.AREA_NAME = new String[]{"藏剑山庄", "无双城", "紫草坞", "清溪村", "元宝湾", "潮音洞", "栖霞镇", "巨鲸帮", "顺风码头", "戾剑境", "剑冢", "无双城", "巨鲸帮"};
        this.isAreaOpened = new boolean[this.AREA_NAME.length];
        this.interval = 8;
        this.STAGE_WIDTH = 45;
        this.STAGE_HEIGHT = DROP_POS_INTERVAL;
        this.distance_x = 12;
        this.distance_y = 12;
        this.stageData = new int[][][]{new int[][]{new int[]{100, 0, 0}, new int[]{101, 1, 0}}, new int[][]{new int[]{334, 0, 0}, new int[]{335, 1, 0}}, new int[][]{new int[]{400, 0, 1}, new int[]{401, 1, 1}, new int[]{402, 2, 1}, new int[]{403, 2, 2}, new int[]{404, 3, 2}, new int[]{405, 4, 2}, new int[]{406, 4, 1}, new int[]{407, 3, 1}, new int[]{408, 3, 0}}, new int[][]{new int[]{500, 0, 0}, new int[]{501, 1, 0}, new int[]{502, 2, 0}, new int[]{503, 3, 0}, new int[]{504, 3, 1}, new int[]{505, 4, 0}, new int[]{506, 2, 1}, new int[]{507, 1, 1}, new int[]{508, 1, 2}, new int[]{509, 2, 2}, new int[]{510, 3, 2}, new int[]{511, 4, 2}, new int[]{512, 4, 1}}, new int[][]{new int[]{600, 4, 2}, new int[]{601, 3, 2}, new int[]{602, 2, 2}, new int[]{603, 1, 2}, new int[]{604, 0, 2}, new int[]{605, 0, 1}, new int[]{606, 1, 1}, new int[]{607, 2, 1}, new int[]{608, 3, 0}, new int[]{609, 2, 0}, new int[]{610, 1, 0}, new int[]{612, 3, 1}, new int[]{611, 0, 0}}, new int[][]{new int[]{700, 0, 2}, new int[]{701, 0, 1}, new int[]{702, 0, 0}, new int[]{703, 1, 0}, new int[]{704, 2, 0}, new int[]{705, 3, 0}, new int[]{706, 3, 1}, new int[]{707, 2, 1}, new int[]{708, 1, 1}, new int[]{709, 1, 2}, new int[]{710, 2, 2}, new int[]{711, 3, 2}, new int[]{712, 4, 2}, new int[]{713, 4, 1}}, new int[][]{new int[]{800, 0, 4}, new int[]{801, 1, 4}, new int[]{802, 1, 5}, new int[]{803, 2, 4}, new int[]{804, 3, 4}, new int[]{805, 3, 3}, new int[]{806, 2, 3}, new int[]{807, 1, 3}, new int[]{808, 0, 3}, new int[]{809, 0, 2}, new int[]{810, 1, 2}, new int[]{811, 2, 2}, new int[]{812, 3, 2}, new int[]{813, 3, 1}, new int[]{814, 2, 1}, new int[]{815, 1, 0}, new int[]{816, 1, 1}, new int[]{817, 0, 1}}, new int[][]{new int[]{900, 0, 1}}, new int[][]{new int[]{WebNetEvent.GotoWeb_Event_OK, 4, 3}, new int[]{WebNetEvent.GotoWeb_Event_Error, 3, 3}, new int[]{1002, 2, 3}, new int[]{1003, 2, 2}, new int[]{1004, 1, 2}, new int[]{1005, 0, 2}, new int[]{1006, 0, 1}, new int[]{1007, 1, 1}, new int[]{1008, 3, 1}, new int[]{1009, 3, 2}, new int[]{1010, 4, 2}, new int[]{1011, 4, 1}, new int[]{1012, 4, 0}, new int[]{1013, 3, 0}, new int[]{1014, 2, 0}, new int[]{1015, 1, 0}}, new int[][]{new int[]{1100, 0, 1}, new int[]{1101, 1, 1}, new int[]{1102, 2, 1}, new int[]{1103, 2, 0}, new int[]{1104, 3, 0}, new int[]{1105, 4, 0}, new int[]{1106, 4, 1}, new int[]{1107, 5, 1}, new int[]{1108, 6, 1}, new int[]{1109, 6, 2}, new int[]{1110, 5, 2}, new int[]{1111, 4, 2}, new int[]{1112, 4, 3}, new int[]{1113, 3, 3}, new int[]{1114, 2, 3}, new int[]{1115, 2, 4}, new int[]{1116, 1, 4}, new int[]{1117, 0, 4}, new int[]{1118, 0, 5}, new int[]{1119, 1, 5}, new int[]{1120, 2, 5}, new int[]{1121, 2, 6}, new int[]{1122, 3, 6}, new int[]{1123, 4, 6}, new int[]{1124, 4, 5}}, new int[][]{new int[]{200, 0, 0}, new int[]{201, 1, 0}, new int[]{202, 2, 0}, new int[]{203, 3, 0}, new int[]{204, 4, 0}}, new int[][]{new int[]{300, 0, 2}, new int[]{301, 1, 2}, new int[]{302, 2, 2}, new int[]{303, 2, 1}, new int[]{304, 1, 1}, new int[]{305, 0, 1}, new int[]{306, 0, 0}, new int[]{307, 1, 0}, new int[]{308, 2, 0}, new int[]{309, 3, 0}, new int[]{310, 4, 0}, new int[]{311, 5, 0}, new int[]{312, 6, 0}, new int[]{313, 6, 1}, new int[]{314, 5, 1}, new int[]{315, 4, 1}, new int[]{316, 4, 2}, new int[]{317, 5, 2}, new int[]{318, 6, 2}, new int[]{319, 6, 3}, new int[]{320, 5, 3}, new int[]{321, 4, 3}, new int[]{322, 4, 4}, new int[]{323, 5, 4}, new int[]{324, 6, 4}, new int[]{325, 6, 5}, new int[]{326, 5, 5}, new int[]{327, 4, 5}, new int[]{328, 3, 5}, new int[]{329, 3, 4}, new int[]{330, 2, 4}, new int[]{331, 1, 4}, new int[]{332, 0, 4}, new int[]{333, 0, 3}, new int[]{334, 1, 3}, new int[]{335, 2, 3}}, new int[][]{new int[]{900, 0, 1}, new int[]{901, 0, 2}, new int[]{902, 1, 1}, new int[]{903, 1, 0}, new int[]{904, 2, 1}, new int[]{905, 2, 2}, new int[]{906, 3, 1}, new int[]{907, 3, 0}, new int[]{908, 4, 1}, new int[]{909, 4, 2}, new int[]{910, 4, 3}, new int[]{911, 3, 3}, new int[]{912, 2, 3}, new int[]{913, 1, 3}, new int[]{914, 0, 3}, new int[]{915, 0, 4}, new int[]{916, 1, 4}, new int[]{917, 2, 4}, new int[]{918, 3, 4}, new int[]{919, 4, 4}}};
        byte[] bArr = new byte[4];
        bArr[3] = 1;
        byte[][] bArr2 = {bArr, new byte[]{0, 0, 1, STATE_CG}};
        byte[][] bArr3 = {new byte[]{0, 0, STATE_CG, 1}, new byte[]{0, 0, 1, STATE_CG}};
        byte[][] bArr4 = {new byte[]{0, 0, STATE_CG, 1}, new byte[]{0, 0, 1, 1}, new byte[]{0, 1, 1, 0}, new byte[]{1, 0, 0, 1}, new byte[]{0, 0, 1, 1}, new byte[]{1, 0, 1, 0}, new byte[]{0, 1, 1, 0}, new byte[]{1, 0, 0, 1}, new byte[]{0, 1, STATE_CG, 0}};
        byte[] bArr5 = new byte[4];
        bArr5[2] = 1;
        byte[][] bArr6 = {new byte[]{STATE_CG, 0, 0, 1}, new byte[]{0, 0, 1, 1}, new byte[]{0, 0, 1, 1}, new byte[]{0, 1, 1, 1}, new byte[]{1, 0, 1, 0}, bArr5, new byte[]{0, 0, 1, 1}, new byte[]{0, 1, 0, 1}, new byte[]{1, 0, 0, 1}, new byte[]{0, 0, 1, 1}, new byte[]{0, 0, 1, 1}, new byte[]{1, 0, 1, 0}, new byte[]{0, 1, 0, STATE_CG}};
        byte[][] bArr7 = {new byte[]{0, STATE_CG, 1, 0}, new byte[]{0, 0, 1, 1}, new byte[]{0, 0, 1, 1}, new byte[]{0, 0, 1, 1}, new byte[]{1, 0, 0, 1}, new byte[]{0, 1, 0, 1}, new byte[]{0, 0, 1, 1}, new byte[]{0, 0, 1, 1}, new byte[]{0, 1, 1, 0}, new byte[]{0, 0, 1, 1}, new byte[]{0, 0, 1, 1}, new byte[]{1, 0, 1, 0}, new byte[]{0, 0, STATE_CG, 1}};
        byte[][] bArr8 = {new byte[]{1, 0, STATE_CG, 0}, new byte[]{1, 1, 0, 0}, new byte[]{0, 1, 0, 1}, new byte[]{0, 0, 1, 1}, new byte[]{0, 0, 1, 1}, new byte[]{0, 1, 1, 0}, new byte[]{1, 0, 1, 0}, new byte[]{0, 0, 1, 1}, new byte[]{0, 1, 0, 1}, new byte[]{1, 0, 0, 1}, new byte[]{0, 0, 1, 1}, new byte[]{0, 0, 1, 1}, new byte[]{1, 0, 1, 0}, new byte[]{0, 1, 0, STATE_CG}};
        byte[] bArr9 = new byte[4];
        bArr9[0] = 2;
        byte[] bArr10 = new byte[4];
        bArr10[1] = 2;
        byte[][] bArr11 = {new byte[]{0, 0, STATE_CG, 1}, new byte[]{0, 1, 1, 0}, bArr9, new byte[]{0, 1, 0, 1}, new byte[]{1, 0, 1, 0}, new byte[]{0, 1, 1, 0}, new byte[]{0, 0, 1, 1}, new byte[]{0, 0, 1, 1}, new byte[]{1, 0, 0, 1}, new byte[]{0, 1, 0, 1}, new byte[]{0, 0, 1, 1}, new byte[]{0, 0, 1, 1}, new byte[]{1, 0, 1, 0}, new byte[]{0, 1, 1, 0}, new byte[]{1, 0, 0, 1}, bArr10, new byte[]{1, 0, 1, 0}, new byte[]{0, 0, STATE_CG, 1}};
        byte[] bArr12 = new byte[4];
        bArr12[2] = STATE_CG;
        byte[][] bArr13 = {bArr12};
        byte[][] bArr14 = {new byte[]{0, 0, 1, STATE_CG}, new byte[]{0, 0, 1, 1}, new byte[]{1, 0, 0, 1}, new byte[]{0, 1, 1, 0}, new byte[]{0, 0, 1, 1}, new byte[]{1, 0, 0, 1}, new byte[]{0, 1, 0, 1}, new byte[]{0, 0, 1, 1}, new byte[]{0, 1, 1, 0}, new byte[]{1, 0, 0, 1}, new byte[]{1, 0, 1, 0}, new byte[]{1, 1, 0, 0}, new byte[]{0, 1, 1, 0}, new byte[]{0, 0, 1, 1}, new byte[]{0, 0, 1, 1}, new byte[]{0, 0, STATE_CG, 1}};
        byte[][] bArr15 = {new byte[]{0, 0, STATE_CG, 1}, new byte[]{0, 0, 1, 1}, new byte[]{1, 0, 1, 0}, new byte[]{0, 1, 0, 1}, new byte[]{0, 0, 1, 1}, new byte[]{0, 1, 1, 0}, new byte[]{1, 0, 0, 1}, new byte[]{0, 0, 1, 1}, new byte[]{0, 1, 1, 0}, new byte[]{1, 0, 1, 0}, new byte[]{0, 0, 1, 1}, new byte[]{0, 1, 0, 1}, new byte[]{1, 0, 1, 0}, new byte[]{0, 0, 1, 1}, new byte[]{0, 1, 0, 1}, new byte[]{1, 0, 1, 0}, new byte[]{0, 0, 1, 1}, new byte[]{0, 1, 0, 1}, new byte[]{1, 0, 0, 1}, new byte[]{0, 0, 1, 1}, new byte[]{0, 1, 1, 0}, new byte[]{1, 0, 0, 1}, new byte[]{0, 0, 1, 1}, new byte[]{1, 0, 1, 0}, new byte[]{0, 1, STATE_CG, 0}};
        byte[] bArr16 = new byte[4];
        bArr16[3] = 1;
        byte[] bArr17 = new byte[4];
        bArr17[2] = 1;
        byte[][] bArr18 = {bArr16, new byte[]{0, 0, 1, 1}, new byte[]{0, 0, 1, 1}, new byte[]{0, 0, 1, 1}, bArr17};
        byte[][] bArr19 = {new byte[]{0, 0, STATE_CG, 1}, new byte[]{0, 0, 1, 1}, new byte[]{1, 0, 1, 0}, new byte[]{0, 1, 1, 0}, new byte[]{0, 0, 1, 1}, new byte[]{1, 0, 0, 1}, new byte[]{0, 1, 0, 1}, new byte[]{0, 0, 1, 1}, new byte[]{0, 0, 1, 1}, new byte[]{0, 0, 1, 1}, new byte[]{0, 0, 1, 1}, new byte[]{0, 0, 1, 1}, new byte[]{0, 1, 1, 0}, new byte[]{1, 0, 1, 0}, new byte[]{0, 0, 1, 1}, new byte[]{0, 1, 0, 1}, new byte[]{1, 0, 0, 1}, new byte[]{0, 0, 1, 1}, new byte[]{0, 1, 1, 0}, new byte[]{1, 0, 1, 0}, new byte[]{0, 0, 1, 1}, new byte[]{0, 1, 0, 1}, new byte[]{1, 0, 0, 1}, new byte[]{0, 0, 1, 1}, new byte[]{0, 1, 1, 0}, new byte[]{1, 0, 1, 0}, new byte[]{0, 0, 1, 1}, new byte[]{0, 0, 1, 1}, new byte[]{1, 0, 0, 1}, new byte[]{0, 1, 1, 0}, new byte[]{0, 0, 1, 1}, new byte[]{0, 0, 1, 1}, new byte[]{1, 0, 0, 1}, new byte[]{0, 1, 0, 1}, new byte[]{0, 0, 1, 1}, new byte[]{0, 0, 1, STATE_CG}};
        byte[] bArr20 = {0, 1, STATE_CG, 0};
        byte[] bArr21 = new byte[4];
        bArr21[0] = 2;
        byte[] bArr22 = new byte[4];
        bArr22[1] = 2;
        byte[] bArr23 = new byte[4];
        bArr23[0] = 2;
        byte[] bArr24 = new byte[4];
        bArr24[1] = 2;
        this.bStageExit = new byte[][][]{bArr2, bArr3, bArr4, bArr6, bArr7, bArr8, bArr11, bArr13, bArr14, bArr15, bArr18, bArr19, new byte[][]{bArr20, bArr21, new byte[]{1, 1, 0, 0}, bArr22, new byte[]{1, 1, 0, 0}, bArr23, new byte[]{1, 1, 0, 0}, bArr24, new byte[]{1, 1, 0, 0}, new byte[]{1, 1, 0, 0}, new byte[]{1, 0, 1, 0}, new byte[]{0, 0, 1, 1}, new byte[]{0, 0, 1, 1}, new byte[]{0, 0, 1, 1}, new byte[]{0, 1, 0, 1}, new byte[]{1, 0, 0, 1}, new byte[]{0, 0, 1, 1}, new byte[]{0, 0, 1, 1}, new byte[]{0, 0, 1, 1}, new byte[]{0, 0, 1, STATE_CG}}};
        byte[] bArr25 = new byte[9];
        bArr25[2] = 1;
        bArr25[7] = 1;
        byte[] bArr26 = new byte[13];
        bArr26[3] = 1;
        bArr26[11] = 1;
        byte[] bArr27 = new byte[13];
        bArr27[11] = 1;
        byte[] bArr28 = new byte[14];
        bArr28[5] = 1;
        bArr28[12] = 1;
        byte[] bArr29 = new byte[18];
        bArr29[8] = 1;
        bArr29[16] = 1;
        byte[] bArr30 = new byte[20];
        bArr30[9] = 1;
        bArr30[18] = 1;
        byte[] bArr31 = new byte[16];
        bArr31[7] = 1;
        bArr31[14] = 1;
        byte[] bArr32 = new byte[25];
        bArr32[11] = 1;
        bArr32[23] = 1;
        byte[] bArr33 = new byte[36];
        bArr33[9] = 1;
        bArr33[21] = 1;
        bArr33[33] = 1;
        byte[] bArr34 = new byte[20];
        bArr34[9] = 1;
        bArr34[18] = 1;
        this.bSaveStage = new byte[][]{new byte[2], new byte[2], bArr25, bArr26, bArr27, bArr28, bArr29, bArr30, bArr31, bArr32, new byte[5], bArr33, bArr34};
        this.mainMissionTitle = "";
        this.mainMissionInfo = "";
        this.missionHortation = (int[][]) Array.newInstance((Class<?>) Integer.TYPE, 2, 3);
        this.branchMissionTitle = "";
        this.branchMissionInfo = "";
        this.iTradeNum = 1;
        this.iSpecialPropPriceCoefficient = new int[7];
        this.scriptFileToRunV = new Vector(3);
        this.loadingStateV = new Vector(3);
        this.iRemainingTime = 0;
        this.messageSet = new Vector();
        this.messageTypeSet = new Vector();
        this.messageDurationSet = new Vector();
        this.mapScripts = new Vector(5);
        this.sprsByScriptControlV = new Vector(5);
        this.keyCodeV = new Vector(4);
        this.iShowWidth = 0;
        this.iLineIndex = 0;
        this.iLineNum = 0;
        this.iPageNum = 0;
        this.fsBaseDataV = new Vector(20);
        this.dropInfo = new StyledText[3];
        this.dropInfoCount = (int[][]) Array.newInstance((Class<?>) Integer.TYPE, 2, 3);
        this.DROP_INFO_TIME = 35;
        this.DWELLTIME = (byte) 20;
        this.sms = new SMS();
        this.port = "";
        this.CPID = 195;
        this.GAMEID = "105";
        this.bExpCoefficientBySMS = (byte) 1;
        this.iGoldBySMS = 10;
        this.iGoldUnsaved = 0;
        this.unsavedEquipV = new Vector();
        this.bDrawGift = new byte[2];
        this.SMS_MONEY = (byte) 0;
        this.SMS_EXP = (byte) 1;
        this.SMS_RELIVE = (byte) 2;
        this.SMS_STAGE = (byte) 3;
        this.SMS_EQUIP = (byte) 4;
        this.SMS_SKILL = (byte) 5;
        this.content = new String[]{"0411C1107311022100713111022100709701MC099506000000000000000000000000", "0211C1107311022100713111022100710001MC099506000000000000000000000000", "0111C1107311022100713111022100710101MC099506000000000000000000000000", "1011C1107311022100713111022100709601MC099506000000000000000000000000", "0111C1107311022100713111022100709801MC099506000000000000000000000000", "0111C1107311022100713111022100709901MC099506000000000000000000000000"};
        this.number = "10659811002";
        this.DIR_KEY_WIDHT = 70;
        this.DIR_KEY_HEIGHT = 40;
        this.DIR_KEY_POSITION = new int[][]{new int[4], new int[4], new int[4], new int[4], new int[4], new int[4], new int[4], new int[4], new int[4], new int[4], new int[4], new int[4], new int[4], new int[4], new int[4], new int[4], new int[4], new int[4], new int[4], new int[4], new int[4], new int[4], new int[4], new int[4]};
        this.dirFrame = -1;
        this.holder = getHolder();
        this.holder.addCallback(this);
        initAndroid();
        instance = this;
        Common.initUIBounds(0, 0, SCREEN_WIDTH, SCREEN_HEIGHT);
        this.DISTANCE = SCREEN_WIDTH >> 1;
        this.OFF_X = (SCREEN_WIDTH - 533) >> 1;
        this.menuWidth = 175;
        this.menuHeight = 187;
        this.menuX = (SCREEN_WIDTH - this.menuWidth) >> 1;
        this.menuY = (SCREEN_HEIGHT - this.menuHeight) >> 1;
        loadLogoRes();
        calcMainMenuPos();
        initButtons();
        loadKeyRes();
        this.holder = getHolder();
        this.holder.addCallback(this);
        this.g = null;
    }

    private void initAndroid() {
        if (this.paint == null) {
            this.paint = new Paint();
            this.paint.setTextSize(18.0f);
        }
        if (font == null) {
            font = Font.getFont(this.paint);
        }
        if (this.access == null) {
            this.access = new DataAccess();
        }
        FontWidth = Font.fontChinaW;
        FontHeight = Font.fontChinaH;
    }

    @Override // android.view.View
    protected void onDraw(Canvas canvas) {
        if (this.g == null) {
            this.g = new Graphics(canvas, this.paint, font);
        }
        canvas.save();
        draw(this.g);
        canvas.restore();
    }

    @Override // android.view.SurfaceHolder.Callback
    public void surfaceChanged(SurfaceHolder holder, int format, int width, int height) {
        setFocusable(true);
        setFocusableInTouchMode(true);
    }

    @Override // android.view.SurfaceHolder.Callback
    public void surfaceCreated(SurfaceHolder holder) {
        this.thread = new MyThread(getHolder());
        this.thread.setFlag(true);
        this.thread.start();
        if (this.bGameState != 4 && this.bGameState != 0) {
            playMusic();
        }
        System.out.println("surfaceCreated-------------------------------");
    }

    @Override // android.view.SurfaceHolder.Callback
    public void surfaceDestroyed(SurfaceHolder holder) {
        boolean retry = true;
        this.thread.setFlag(false);
        while (retry) {
            try {
                this.thread.join();
                retry = false;
            } catch (InterruptedException e) {
            }
        }
        System.out.println("surfaceDestroyed-------------------------------");
    }

    protected void keyPressed(int keyCode) {
        if (!this.isKeyLocked) {
            Common.keyPressed(keyCode);
        }
    }

    protected void keyReleased(int keyCode) {
        Common.clearKeyPressedStates();
        Common.keyReleased(keyCode);
        this.isUpReleased = true;
        this.isDownReleased = true;
        this.isLeftReleased = true;
        this.isRightReleased = true;
        this.isOKReleased = true;
        this.dirFrame = -1;
    }

    protected void pointerPressed(int x, int y) {
        if (!this.isKeyLocked) {
            this.iPointerX = x;
            this.iPointerY = y;
        }
    }

    protected void pointerReleased(int x, int y) {
        this.iPointerX = -1;
        this.iPointerY = -1;
    }

    public void draw(Graphics g) {
        g.setColor(0);
        g.fillRect(0, 0, SCREEN_WIDTH, SCREEN_HEIGHT);
        switch (this.bGameState) {
            case -1:
                drawCG(g);
                break;
            case 0:
                drawLogo(g);
                break;
            case 1:
            case 2:
                if (this.bPreState == 5) {
                    drawMainMenu(g);
                } else {
                    drawScene(g);
                }
                drawHelp(g);
                break;
            case 3:
                if (this.bPreState == 5) {
                    drawMainMenu(g);
                } else {
                    drawScene(g);
                }
                drawGameSet(g);
                break;
            case 4:
                drawStartMusic(g);
                break;
            case 5:
                drawMainMenu(g);
                break;
            case 7:
                if (this.bPreState == 5) {
                    drawMainMenu(g);
                } else {
                    drawScene(g);
                }
                drawRecord(g);
                break;
            case Interpreter.ERR_ARR_OUT /* 9 */:
                drawLoading(g);
                break;
            case 11:
            case 12:
            case 13:
            case 20:
            case 22:
            case 37:
                drawScene(g);
                if (this.bGameState == 11) {
                    drawChoice(g);
                } else if (this.bGameState == 22) {
                    drawGameOver(g);
                } else if (this.bGameState == 37) {
                    drawReturnValidate(g);
                }
                if (this.isTalking) {
                    drawTalk(g);
                }
                break;
            case 14:
                drawScene(g);
                drawEquipPreviewUI(g);
                break;
            case 15:
                drawScene(g);
                drawShopUI(g);
                break;
            case Graphics.TOP /* 16 */:
                drawNotify(g);
                break;
            case Graphics.HT /* 17 */:
                if (this.bPreState == 18) {
                    drawScene(g);
                    drawGameMenu(g);
                } else if (this.bPreState == 27) {
                    drawScene(g);
                    drawReinforceUI(g);
                } else if (this.bPreState == 28) {
                    drawGameMap(g);
                } else if (this.bPreState == 15) {
                    drawScene(g);
                    drawShopUI(g);
                } else {
                    drawScene(g);
                }
                drawMessage(g);
                break;
            case 18:
                drawScene(g);
                drawGameMenu(g);
                break;
            case TimeUtils.HUNDRED_DAY_FIELD_LEN /* 19 */:
                if (this.bPreState == 18) {
                    drawGameMenu(g);
                } else if (this.bPreState == 27) {
                    drawReinforceUI(g);
                } else {
                    drawScene(g);
                }
                break;
            case Graphics.RT /* 24 */:
                drawMessageOnBlackScreen(g);
                break;
            case 25:
                drawExitValidate(g);
                break;
            case 26:
                drawMoreInfo(g);
                break;
            case 27:
                drawScene(g);
                drawReinforceUI(g);
                break;
            case 28:
                drawGameMap(g);
                break;
            case DROP_POS_INTERVAL /* 30 */:
                drawWarning(g);
                break;
            case 31:
                drawScene(g);
                drawSMSShop(g);
                break;
            case Graphics.BOTTOM /* 32 */:
                drawScene(g);
                drawSMSInfo(g);
                break;
            case 34:
                drawScene(g);
                drawSMSWaiting(g);
                break;
            case 35:
                drawScene(g);
                drawSMSConfirm(g);
                break;
            case Graphics.LB /* 36 */:
                drawMiniMap(g);
                break;
        }
        drawKey(g);
        if (this.bTurtorialState == 2) {
            drawTip(g);
        }
    }

    public void cycle() {
        if (this.isDurabilityToZero) {
            if (this.bEffectCounter < this.sceneUIAS.actionDatas[2].frameNum) {
                this.bEffectCounter = (byte) (this.bEffectCounter + 1);
            } else {
                this.isDurabilityToZero = false;
            }
            if (getGameState() == 12) {
                return;
            }
        }
        if (this.bCriticalDuration > 0) {
            if (this.bCriticalCounter < this.bCriticalDuration) {
                this.bCriticalCounter = (byte) (this.bCriticalCounter + 1);
            } else {
                this.bCriticalDuration = (byte) 0;
                this.bCriticalCounter = (byte) 0;
            }
        }
        if (this.iHPDecrement > 0) {
            int velocity = (this.role.iFinalHP / this.roleHPImg.getWidth()) << 1;
            if (this.iHPDecrement >= velocity) {
                this.iHPDecrement -= velocity;
            } else {
                this.iHPDecrement = 0;
            }
        }
        if (this.bTurtorialState == 2 && this.tipST != null) {
            this.tipST.cycle();
        }
        switch (this.bGameState) {
            case -1:
                cycleCG();
                break;
            case 0:
                cycleLogo();
                break;
            case 1:
            case 2:
                cycleHelp();
                break;
            case 4:
                cycleStartMusic();
                break;
            case 5:
                cycleMainMenu();
                break;
            case Interpreter.ERR_ARR_OUT /* 9 */:
                cycleLoad();
                break;
            case 11:
                cycleChoice();
                break;
            case 12:
                if (!this.isTalking) {
                    cycleScene();
                } else {
                    refreshSpriteInScreen();
                }
                break;
            case 13:
                cycleScreenMove();
                break;
            case 14:
            case 15:
            case 27:
                if (this.UIST != null) {
                    this.UIST.cycle();
                }
                break;
            case Graphics.HT /* 17 */:
                cycleMessage();
                break;
            case 18:
                cycleGameMenu();
                break;
            case TimeUtils.HUNDRED_DAY_FIELD_LEN /* 19 */:
                cycleSceneStay();
                break;
            case 20:
                cycleScriptMovingSprite();
                break;
            case 22:
                cycleGameOver();
                break;
            case Graphics.RT /* 24 */:
                cycleBlackScreenMessage();
                break;
            case 34:
                if (GameCenterEvent.isSucceed != 0) {
                    smsSendOver();
                    GameCenterEvent.isSucceed = 0;
                }
                break;
        }
        cycleVibrate();
        cycleScrollInfo();
        cycleDropInfo();
        cycleScreenMoveByScript();
        cycleContorlledSpriteByScript();
        cycleStageInfo();
    }

    public void handleKeyPressed() {
        if (Common.isAnyKeyPressed()) {
            if (isFalseResponse()) {
                this.bTurtorialState = (byte) 0;
                Common.clearKeyPressedStates();
                return;
            }
            switch (this.bGameState) {
                case -1:
                    handleKey_CG();
                    break;
                case 1:
                case 2:
                    handleKey_Help();
                    break;
                case 3:
                    handleKey_GameSet();
                    break;
                case 4:
                    handleKey_StartMusic();
                    break;
                case 5:
                    handleKey_MainMenu();
                    break;
                case 7:
                    handleKey_ReadRecord();
                    break;
                case 11:
                    handleKey_Choice();
                    break;
                case 12:
                    handleKey_Scene();
                    break;
                case 14:
                    handleKey_Preview();
                    break;
                case 15:
                    handleKey_Shop();
                    break;
                case Graphics.HT /* 17 */:
                    handleKey_Message();
                    break;
                case 18:
                    handleKey_GameMenu();
                    break;
                case 22:
                    handleKey_GameOver();
                    break;
                case Graphics.RT /* 24 */:
                    handleKey_BlackScreenMessage();
                    break;
                case 25:
                    if (Common.isKeyPressed(9, true)) {
                        this.bGameState = (byte) 26;
                    }
                    if (Common.isKeyPressed(10, true)) {
                        this.bOptionIndex = (byte) 0;
                        this.bGameState = (byte) 5;
                    }
                    break;
                case 26:
                    if (Common.isKeyPressed(9, true)) {
                        this.isToURL = true;
                        exit();
                    }
                    if (Common.isKeyPressed(10, true)) {
                        exit();
                    }
                    break;
                case 27:
                    handleKey_Reinforce();
                    break;
                case 28:
                    handleKey_map();
                    break;
                case 31:
                    handleKey_SMSShop();
                    break;
                case Graphics.BOTTOM /* 32 */:
                    handlekey_SMSInfo();
                    break;
                case 35:
                    handlekey_SendConfirm();
                    break;
                case Graphics.LB /* 36 */:
                    handleKey_MiniMap();
                    break;
                case 37:
                    if (Common.isKeyPressed(9, true)) {
                        this.bSystemIndex = (byte) 0;
                        releaseGameRes();
                        loadMainMenuRes();
                    }
                    if (Common.isKeyPressed(10, true)) {
                        returnPreState();
                    }
                    break;
            }
            if (this.bTurtorialState == 2) {
                this.bTurtorialState = (byte) 0;
                Common.clearKeyPressedStates();
            }
        }
    }

    public void handleKeyReleased() {
        if (this.bGameState == 12) {
            if (this.isUpReleased) {
                this.isUpReleased = false;
                for (int i = 0; i < this.keyV.size(); i++) {
                    byte keyPoint = ((Byte) this.keyV.elementAt(i)).byteValue();
                    if (keyPoint == 0) {
                        this.keyV.removeElementAt(i);
                    }
                }
            }
            if (this.isDownReleased) {
                this.isDownReleased = false;
                for (int i2 = 0; i2 < this.keyV.size(); i2++) {
                    byte keyPoint2 = ((Byte) this.keyV.elementAt(i2)).byteValue();
                    if (keyPoint2 == 1) {
                        this.keyV.removeElementAt(i2);
                    }
                }
            }
            if (this.isLeftReleased) {
                this.isLeftReleased = false;
                for (int i3 = 0; i3 < this.keyV.size(); i3++) {
                    byte keyPoint3 = ((Byte) this.keyV.elementAt(i3)).byteValue();
                    if (keyPoint3 == 2) {
                        this.keyV.removeElementAt(i3);
                    }
                }
            }
            if (this.isRightReleased) {
                this.isRightReleased = false;
                for (int i4 = 0; i4 < this.keyV.size(); i4++) {
                    byte keyPoint4 = ((Byte) this.keyV.elementAt(i4)).byteValue();
                    if (keyPoint4 == 3) {
                        this.keyV.removeElementAt(i4);
                    }
                }
            }
            if (this.isOKReleased) {
                this.isOKReleased = false;
                for (int i5 = 0; i5 < this.keyV.size(); i5++) {
                    byte keyPoint5 = ((Byte) this.keyV.elementAt(i5)).byteValue();
                    if (keyPoint5 == 4) {
                        this.keyV.removeElementAt(i5);
                    }
                }
            }
            this.isRunning = false;
            for (int i6 = 0; i6 < this.keyV.size(); i6++) {
                byte keyPoint6 = ((Byte) this.keyV.lastElement()).byteValue();
                if (keyPoint6 < 4) {
                    this.isRunning = true;
                    break;
                }
            }
            if (!this.isRunning && this.role.bActID == 1) {
                this.role.act((byte) 0);
            }
        }
    }

    public static void clearUpMemory(Graphics g) {
    }

    public void playMusic() {
        if (this.isOpenSound && optionData > 0) {
            int id = 0;
            if (this.currentBGMusic.equals("/cover.mid")) {
                id = R.raw.cover;
            } else if (this.currentBGMusic.equals("/alone.mid")) {
                id = R.raw.alone;
            } else if (this.currentBGMusic.equals("/0.mid")) {
                id = R.raw.zero;
            } else if (this.currentBGMusic.equals("/1.mid")) {
                id = R.raw.one;
            } else if (this.currentBGMusic.equals("/2.mid") || this.currentBGMusic.equals("/3.mid")) {
                id = R.raw.two;
            } else if (this.currentBGMusic.equals("/4.mid")) {
                id = R.raw.four;
            } else if (this.currentBGMusic.equals("/5.mid")) {
                id = R.raw.two;
            }
            JL01Activity.activity.playBgMusic(id);
        }
    }

    void playSound(int soundIndex) {
        if (this.isOpenSound && optionData > 0) {
            JL01Activity.activity.palyEffSound(soundIndex, 1);
        }
    }

    void playSound() {
        switch (this.role.bActID) {
            case 7:
            case 8:
            case Interpreter.ERR_ARR_OUT /* 9 */:
            case 10:
                if (this.role.iCurFrameIndex == 0) {
                    playSound(0);
                }
                break;
        }
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void startLoading() {
        if (this.bGameState == 28) {
            releaseGameMapRes();
        }
        this.bGameState = (byte) 9;
        this.isLoadFinish = false;
        this.iLoadingCounter = 0;
        this.iLoadOverCounter = 2;
        this.loadingAS.images[this.loadingAS.images.length - 1] = this.loadingImgs[Tool.getNextRnd(0, 4)];
        new LoadingCycle(getHolder()).start();
        this.message = new StyledText(this.loadString[Tool.getNextRnd(0, this.loadString.length)], SCREEN_WIDTH - 30, 100, FontHeight / 3, 0, font);
        this.message.setLineCenter();
    }

    private void newGame() {
        startLoading();
        startGame();
        this.bag = new Bag();
        this.bag.iMoney += this.iGoldUnsaved;
        initRole((short) 1);
        this.bag.addEquip(new Equip(equipDatas[0], 0, true));
        this.role.updateEquipment((Equip) this.bag.equipV.elementAt(0));
        script_call("start.s", false);
    }

    private void startGame() {
        try {
            readRoleAttribute("/role.tbd");
            readFamulusAttribute("/ego.tbd");
            readSpriteBaseData("/action/sprite.tbd");
            readEnemyAttribute("/enemybase.tbd");
            readEnemyData("/enemy.tbd");
            readEquipMentData("/equip.tbd");
            readPropData("/item.tbd");
            readSkillData("/skill.tbd");
            initDatas();
            releaseMainMenuRes();
            loadGameRes();
            readRecordNames("TheSoulOfTheSwordTitle");
        } catch (Exception e) {
            e.printStackTrace();
            Tool.reportException(e);
        }
    }

    private void exit() {
        JL01Activity.activity.exitGame();
    }

    private void initDatas() {
        for (int i = 0; i < 3; i++) {
            this.dropInfoCount[0][i] = 0;
            this.dropInfoCount[1][i] = 0;
        }
        this.isTimeIn = false;
        this.scrollInfoV.removeAllElements();
        this.isAttackable = false;
        this.isDrawBossHpBar = false;
        this.isDropItem = true;
        this.taskV = new Vector(5);
        this.bTurtorialState = (byte) 0;
        this.branchMissionTitle = "";
        this.branchMissionInfo = "";
        for (int i2 = 0; i2 < this.missionHortation.length; i2++) {
            for (int j = 0; j < this.missionHortation[i2].length; j++) {
                this.missionHortation[i2][j] = -1;
            }
        }
    }

    public byte getGameState() {
        return this.bGameState;
    }

    public void returnPreState() {
        this.bGameState = this.bPreState;
    }

    public void setGameState(byte state) {
        if (this.bGameState != state) {
            this.bPreState = this.bGameState;
            this.bGameState = state;
        }
    }

    private void initRole(short level) {
        this.role = new Role(roleData, level);
        this.role.initData();
        addMapElement(this.role);
    }

    private void setRoleLevel(int level) {
        this.role.sLevel = (short) level;
        this.role.initData();
        this.role.updateAttribute(true);
    }

    void addScrollInfo(String scrollInfo) {
        if (this.scrollInfo == null) {
            initScrollInfoBar(scrollInfo);
        } else if (!this.scrollInfo.equals(scrollInfo)) {
            this.scrollInfoV.addElement(scrollInfo);
        }
    }

    private void initScrollInfoBar(String scrollInfo) {
        this.scrollInfoState = (byte) 1;
        this.infoBarCount = 0;
        this.scrollInfoCount = 0;
        this.scrollInfo = new String(scrollInfo);
    }

    void cycleScreenMoveByScript() {
        if (this.isScreenMoving) {
            if (this.iScreenMoveTimes < this.iScreenMoveTotalTime) {
                this.iScreenMoveTimes++;
                this.iViewMapX += this.iScreenMoveVX;
                this.iViewMapY += this.iScreenMoveVY;
                this.map.setViewPosInMap(this.iViewMapX, this.iViewMapY);
                return;
            }
            this.isScreenMoving = false;
        }
    }

    void cycleDropInfo() {
        for (int i = 0; i < 3; i++) {
            if (this.dropInfo[i] != null) {
                if (this.dropInfoCount[0][i] >= (SCREEN_WIDTH - this.dropInfo[i].getWidth()) + 7) {
                    int[] iArr = this.dropInfoCount[0];
                    iArr[i] = iArr[i] - 10;
                } else {
                    this.dropInfoCount[0][i] = (SCREEN_WIDTH - this.dropInfo[i].getWidth()) - 3;
                    if (this.dropInfoCount[1][i] > 0) {
                        int[] iArr2 = this.dropInfoCount[1];
                        iArr2[i] = iArr2[i] - 1;
                    }
                }
            }
        }
    }

    void setCriticalEffect(int duration) {
        this.bCriticalDuration = (byte) duration;
        setVibrate(4, Tool.getNextRnd(0, 1));
        playSound(1);
    }

    void setReducedHP(int reducedHP) {
        this.iHPDecrement = reducedHP;
    }

    void cycleSpriteInStage() {
        if (!this.role.isCriticalKill) {
            this.spriteOnLayer0.removeAllElements();
            this.spriteOnLayer1.removeAllElements();
            this.spriteOnLayer2.removeAllElements();
            this.spriteBelowLayer0.removeAllElements();
            this.enemyInScreen.removeAllElements();
            this.bulletInScreen.removeAllElements();
            this.effectV.removeAllElements();
            this.switchV.removeAllElements();
            this.isAllEnemiesKilled = true;
            int i = 0;
            while (i < this.spriteInStage.size()) {
                Sprite spr = (Sprite) this.spriteInStage.elementAt(i);
                if (spr == null || spr.isDead) {
                    this.spriteInStage.removeElementAt(i);
                } else {
                    if (spr.bType != 0 && spr.bType != 14) {
                        spr.cycle();
                        if (spr instanceof Enemy) {
                            this.isAllEnemiesKilled = false;
                            this.enemyInScreen.addElement(spr);
                        }
                        if (spr.bType == 5) {
                            this.switchV.addElement(spr);
                        }
                    }
                    if (spr.isInScreen(this.iViewMapX, this.iViewMapY, SCREEN_WIDTH, SCREEN_HEIGHT)) {
                        spr.isInScreen = true;
                        if (spr.iLayerIndex == 0) {
                            this.spriteBelowLayer0.addElement(spr);
                        } else if (spr.iLayerIndex == 1) {
                            this.spriteOnLayer0.addElement(spr);
                        } else if (spr.iLayerIndex == 2) {
                            this.spriteOnLayer1.addElement(spr);
                        } else if (spr.iLayerIndex == 3) {
                            this.spriteOnLayer2.addElement(spr);
                        }
                        switch (spr.bType) {
                            case Interpreter.ERR_ARR_OUT /* 9 */:
                            case 10:
                            case 11:
                                Enemy e = (Enemy) spr;
                                this.role.processAttack(e);
                                if (spr.bType == 10) {
                                    this.iBossHP = ((FightSprite) spr).iHP;
                                    if (this.iBossHP > 0) {
                                        this.iBossMaxHP = ((Enemy) spr).iMaxHP;
                                        this.isDrawBossHpBar = true;
                                    }
                                    if (e.isInvisible) {
                                        this.isDrawBossHpBar = false;
                                    }
                                }
                                break;
                            case 15:
                                this.bulletInScreen.addElement(spr);
                                break;
                            case Graphics.TOP /* 16 */:
                                this.effectV.addElement(spr);
                                break;
                        }
                    } else {
                        spr.isInScreen = false;
                        switch (spr.bType) {
                            case 14:
                                if (this.role.famulus != null) {
                                    this.role.famulus.transfer();
                                }
                                break;
                            case 15:
                                spr.isDead = true;
                                break;
                        }
                    }
                    i++;
                }
            }
            if (this.isAllEnemiesKilled) {
                for (int i2 = 0; i2 < this.switchV.size(); i2++) {
                    Sprite me = (Sprite) this.switchV.elementAt(i2);
                    me.isInvisible = false;
                }
                return;
            }
            return;
        }
        if (this.role.bActID == 25 || (this.role.bActID == 24 && this.role.iCurFrameIndex >= 34)) {
            int i3 = 0;
            while (i3 < this.fightElement.size()) {
                Sprite spr2 = (Sprite) this.fightElement.elementAt(i3);
                spr2.cycle();
                if (spr2 instanceof Enemy) {
                    this.role.processAttack((Enemy) spr2);
                    if (spr2.bType == 10) {
                        this.iBossHP = ((FightSprite) spr2).iHP;
                        if (this.iBossHP > 0) {
                            this.iBossMaxHP = ((Enemy) spr2).iMaxHP;
                            this.isDrawBossHpBar = true;
                        }
                    }
                }
                if (spr2.isDead) {
                    this.fightElement.removeElementAt(i3);
                } else {
                    i3++;
                }
            }
        }
    }

    private void refreshSpriteInScreen() {
        this.spriteBelowLayer0.removeAllElements();
        this.spriteOnLayer0.removeAllElements();
        this.spriteOnLayer1.removeAllElements();
        this.spriteOnLayer2.removeAllElements();
        int i = 0;
        while (i < this.spriteInStage.size()) {
            Sprite me = (Sprite) this.spriteInStage.elementAt(i);
            if (me == null || me.isDead) {
                this.spriteInStage.removeElementAt(i);
            } else {
                if (me.ID != -2) {
                    me.cycle();
                }
                if (me.isInvisible) {
                    i++;
                } else {
                    if (me.isInScreen(this.iViewMapX, this.iViewMapY, SCREEN_WIDTH, SCREEN_HEIGHT)) {
                        if (me.iLayerIndex == 0) {
                            this.spriteBelowLayer0.addElement(me);
                        } else if (me.iLayerIndex == 1) {
                            this.spriteOnLayer0.addElement(me);
                        } else if (me.iLayerIndex == 2) {
                            this.spriteOnLayer1.addElement(me);
                        } else if (me.iLayerIndex == 3) {
                            this.spriteOnLayer2.addElement(me);
                        }
                        if (me instanceof Enemy) {
                            Enemy e = (Enemy) me;
                            this.role.processAttack(e);
                        }
                        if ((me instanceof Role) && me.bActState != 0) {
                            moveSpriteByStep(me, 10, 5);
                        }
                    }
                    i++;
                }
            }
        }
    }

    public int calcEnemyExp(Enemy fs) {
        int exp = (fs.sLevel * fs.sLevel * 5) + 10;
        if (exp > fs.sLevel * 100) {
            exp = fs.sLevel * 100;
        }
        int exp2 = (fs.enemyData.sExpCoefficient * exp) / 100;
        if (this.isExpRoom) {
            exp2 <<= 1;
        }
        int exp3 = exp2 * this.bExpCoefficientBySMS;
        int levelCoefficient = ((fs.sLevel - this.role.sLevel) * 10) + 100;
        if (levelCoefficient > 200) {
            levelCoefficient = 200;
        } else if (levelCoefficient < 10) {
            levelCoefficient = 10;
        }
        int exp4 = (exp3 * levelCoefficient) / 100;
        return exp4 + Tool.getNextRnd((exp4 * (-5)) / 100, ((exp4 * 5) / 100) + 1);
    }

    void processEnemyDie(FightSprite fs) {
        if (fs.bType == 10) {
            this.isDrawBossHpBar = false;
            if (!this.role.isCriticalKill) {
                fs.act((byte) 3);
            }
        }
        dropItem((Enemy) fs);
        this.role.addExp(calcEnemyExp((Enemy) fs));
        processTask(fs);
    }

    private void processTask(FightSprite fs) {
        for (int i = 0; i < this.taskV.size(); i++) {
            Task task = (Task) this.taskV.elementAt(i);
            for (int m = 0; m < task.iStageID.length; m++) {
                if (this.iCurrentStageID == task.iStageID[m]) {
                    for (int j = 0; j < task.iEnemyID.length; j++) {
                        if (fs.iSortID == task.iEnemyID[j] && task.iEnemyNum[j] > 0) {
                            int[] iArr = task.iEnemyNum;
                            iArr[j] = iArr[j] - 1;
                        }
                    }
                }
            }
            boolean isAchieve = true;
            for (int m2 = 0; m2 < task.iEnemyNum.length; m2++) {
                if (task.iEnemyNum[m2] > 0) {
                    isAchieve = false;
                }
            }
            if (isAchieve) {
                this.taskV.removeElementAt(i);
                if (this.role.bActState != 0 && this.role.bActState != 2) {
                    this.isWaitforSkillOver = true;
                    this.taskOverScript = task.taskOverScript;
                    this.isToLoading = task.isLoading;
                } else {
                    this.role.initAct();
                    script_call(task.taskOverScript, task.isLoading);
                }
            }
        }
    }

    private void cycleRole() {
        if (!this.role.isInvisible) {
            this.role.cycle();
            this.curKey = -1;
            if (!this.role.isLocked && !this.keyV.isEmpty()) {
                byte bByteValue = ((Byte) this.keyV.lastElement()).byteValue();
                this.curKey = bByteValue;
                switch (bByteValue) {
                    case 0:
                        if (this.role.bActState == 0) {
                            this.role.setDir((byte) 2);
                            this.role.act((byte) 1);
                        }
                        break;
                    case 1:
                        if (this.role.bActState == 0) {
                            this.role.setDir((byte) 3);
                            this.role.act((byte) 1);
                        }
                        break;
                    case 2:
                        if ((this.role.bActID >= 7 && this.role.bActID <= 10) || this.role.bActID == 2) {
                            if (this.role.isFaceToTheRight) {
                                this.role.act((byte) 4);
                                this.keyV.removeElementAt(this.keyV.size() - 1);
                            }
                        } else if (this.role.bActState == 0) {
                            this.role.setDir((byte) 0);
                            this.role.act((byte) 1);
                        }
                        break;
                    case 3:
                        if ((this.role.bActID >= 7 && this.role.bActID <= 10) || this.role.bActID == 2) {
                            if (!this.role.isFaceToTheRight) {
                                this.role.act((byte) 4);
                                this.keyV.removeElementAt(this.keyV.size() - 1);
                            }
                        } else if (this.role.bActState == 0) {
                            this.role.setDir((byte) 1);
                            this.role.act((byte) 1);
                        }
                        break;
                    case 4:
                        if (this.role.bActState == 0 || this.role.bActID < 11) {
                            this.role.fire();
                        }
                        break;
                }
            }
            moveSpriteByStep(this.role, 10, 5);
            basicTurtorialKeyResponse();
            moveScreen();
            if (this.iFireCounter < 6) {
                this.iFireCounter++;
            }
            if (this.iFireCounter >= 3) {
                this.role.iAttackIndex = 0;
            }
        }
    }

    void cycleGameMenu() {
        int frameNum;
        if (this.isExitGameMenu) {
            if (this.bExitCounter > 0) {
                this.bExitCounter = (byte) (this.bExitCounter + STATE_CG);
            } else {
                this.isExitGameMenu = false;
                this.bMenuIndex = (byte) 0;
                this.bGameState = (byte) 12;
                this.UIST = null;
            }
        }
        if (this.UIST != null) {
            this.UIST.cycle();
        }
        cycleScrollInfo();
        if (this.iExpIncrease > 0) {
            int temp = this.role.famulus.iLevelUpExp / 15;
            if (this.iExpIncrease >= temp) {
                if (this.role.famulus.iCurrentExp <= this.role.famulus.iLevelUpExp - temp) {
                    this.iExpIncrease -= temp;
                    this.role.famulus.iCurrentExp += temp;
                } else {
                    this.iExpIncrease -= this.role.famulus.iLevelUpExp - this.role.famulus.iCurrentExp;
                    this.role.famulus.iCurrentExp = this.role.famulus.iLevelUpExp;
                    if (this.role.famulus.isLevelUpContinue()) {
                        this.isFlashEffect = true;
                        this.bFlashCounter = (byte) 0;
                    } else {
                        this.iTotalIncreaseExp -= this.iExpIncrease;
                        this.iExpIncrease = 0;
                    }
                }
            } else if (this.role.famulus.iCurrentExp <= this.role.famulus.iLevelUpExp - this.iExpIncrease) {
                this.role.famulus.iCurrentExp += this.iExpIncrease;
                this.iExpIncrease = 0;
            } else {
                this.iExpIncrease -= this.role.famulus.iLevelUpExp - this.role.famulus.iCurrentExp;
                this.role.famulus.iCurrentExp = this.role.famulus.iLevelUpExp;
                if (this.role.famulus.isLevelUpContinue()) {
                    this.isFlashEffect = true;
                    this.bFlashCounter = (byte) 0;
                } else {
                    this.iTotalIncreaseExp -= this.iExpIncrease;
                    this.iExpIncrease = 0;
                }
            }
            if (this.iExpIncrease == 0) {
                feedOver();
            }
        }
        if (this.isFlashEffect) {
            if (this.bFlashCounter < 4) {
                this.bFlashCounter = (byte) (this.bFlashCounter + 1);
            } else {
                this.bFlashCounter = (byte) 0;
                this.isFlashEffect = false;
            }
        }
        if (this.isSkillPreview) {
            if (this.role.bSkillIndex < 9) {
                frameNum = this.role.actionSet.actionDatas[this.role.bSkillIndex + 12].frameNum - 1;
            } else {
                frameNum = (this.role.miscellaneousAS.actionDatas[this.bMiscellaneousType].frameNum + this.role.actionSet.actionDatas[21].frameNum) - 1;
            }
            if (this.bSkillPreviewIndex < frameNum) {
                this.bSkillPreviewIndex = (byte) (this.bSkillPreviewIndex + 1);
            } else {
                this.bSkillPreviewIndex = (byte) 0;
                this.isSkillPreview = false;
            }
        }
    }

    void cycleMessage() {
        if (this.message != null) {
            this.message.cycle();
        }
        if (this.lSleepTimeMills > 0) {
            this.lMessageTimeMills += (long) MILLIS_PRE_UPDATE;
            if (this.lMessageTimeMills >= this.lSleepTimeMills) {
                this.message = null;
                this.lMessageTimeMills = 0L;
                if (this.messageSet.isEmpty()) {
                    this.lSleepTimeMills = 0L;
                    returnPreState();
                    setScriptState((byte) 3);
                } else {
                    this.message = (StyledText) this.messageSet.firstElement();
                    this.messageSet.removeElementAt(0);
                    this.messageType = ((Byte) this.messageTypeSet.firstElement()).byteValue();
                    this.messageTypeSet.removeElementAt(0);
                    this.lSleepTimeMills = ((Integer) this.messageDurationSet.firstElement()).intValue();
                    this.messageDurationSet.removeElementAt(0);
                }
            }
        }
    }

    private void checkScript() {
        if (this.scriptState == 0) {
            int rlCellX = (this.role.iMapX + (this.role.iFootWidth >> 1)) / this.map.sCellWidth;
            int rlCellY = (this.role.iMapY + (this.role.iFootHeight >> 1)) / this.map.sCellHeight;
            if (rlCellX != this.iRolePreCellX || rlCellY != this.iRolePreCellY) {
                this.iRolePreCellX = rlCellX;
                this.iRolePreCellY = rlCellY;
                String scriptFile = getMapScript(rlCellX, rlCellY);
                if (scriptFile != null) {
                    script_call(scriptFile, false);
                }
            }
        }
    }

    private int getAutoDir() {
        int x = (this.role.iMapX + (this.role.iFootWidth / 2)) / this.map.sCellWidth;
        int y = (this.role.iMapY + (this.role.iFootHeight / 2)) / this.map.sCellHeight;
        int first = 0;
        int second = 0;
        for (int i = 0; i < autoDirSet[this.role.bCurDir][0].length; i++) {
            int temX = x + autoDirSet[this.role.bCurDir][0][i][0];
            int temY = y + autoDirSet[this.role.bCurDir][0][i][1];
            if (temX >= 0 && temY >= 0 && temX < this.map.sCellXCount && temY < this.map.sCellYCount && this.map.canBlockPass(temX, temY)) {
                first += value[i];
            }
        }
        for (int i2 = 0; i2 < autoDirSet[this.role.bCurDir][1].length; i2++) {
            int temX2 = x + autoDirSet[this.role.bCurDir][1][i2][0];
            int temY2 = y + autoDirSet[this.role.bCurDir][1][i2][1];
            if (temX2 >= 0 && temY2 >= 0 && temX2 < this.map.sCellXCount && temY2 < this.map.sCellYCount && this.map.canBlockPass(temX2, temY2)) {
                second += value[i2];
            }
        }
        if (first >= 11 || second >= 11) {
            return first >= second ? -1 : 1;
        }
        return 0;
    }

    void useProp(Prop prop) {
        StringBuffer sb = new StringBuffer();
        switch (prop.data.bID) {
            case 0:
            case 1:
            case 2:
            case 3:
                int recoverHP = prop.data.sHPUp;
                if (this.role.iHitPoint == this.role.iFinalHP) {
                    script_showMessage("无须使用该物品", 0);
                }
                this.role.iHP += recoverHP;
                if (this.role.iHP > this.role.iFinalHP) {
                    recoverHP = (short) ((this.role.iFinalHP + recoverHP) - this.role.iHP);
                    this.role.iHP = this.role.iFinalHP;
                }
                if (recoverHP > 0) {
                    sb.append("使用了" + prop.data.name + ",");
                    sb.append("气血值恢复了" + recoverHP + "点");
                    this.bag.deleteProp(prop, 1);
                }
                if (this.iHPDecrement > 0) {
                    this.iHPDecrement = 0;
                }
                break;
            case 4:
            case 5:
            case 6:
            case 7:
                int recoverMP = prop.data.sMPUp;
                if (this.role.isIntegrate) {
                    if (this.role.famulus.iCurDurability == 0) {
                        script_showMessage("武器已损坏，无法使用该物品", 0);
                        return;
                    }
                    if (this.role.famulus.iCurDurability == this.role.famulus.iFinalDurability) {
                        script_showMessage("无须使用该物品", 0);
                        return;
                    }
                    this.role.famulus.iCurDurability += recoverMP;
                    if (this.role.famulus.iCurDurability > this.role.famulus.iFinalDurability) {
                        recoverMP = (short) (this.role.famulus.iFinalDurability - (this.role.famulus.iCurDurability - recoverMP));
                        this.role.famulus.iCurDurability = this.role.famulus.iFinalDurability;
                    }
                    if (recoverMP > 0) {
                        sb.append("使用了" + prop.data.name + ",");
                        sb.append("武器耐久度恢复了" + recoverMP + "点");
                        this.bag.deleteProp(prop, 1);
                    }
                } else {
                    if (this.role.equip.iCurDurability == this.role.equip.iFinalDurability) {
                        script_showMessage("无须使用该物品", 0);
                        return;
                    }
                    this.role.equip.iCurDurability += recoverMP;
                    if (this.role.equip.iCurDurability > this.role.equip.iFinalDurability) {
                        recoverMP = (short) (this.role.equip.iFinalDurability - (this.role.equip.iCurDurability - recoverMP));
                        this.role.equip.iCurDurability = this.role.equip.iFinalDurability;
                    }
                    if (recoverMP > 0) {
                        sb.append("使用了" + prop.data.name + ",");
                        sb.append("武器耐久度恢复了" + recoverMP + "点");
                        this.bag.deleteProp(prop, 1);
                    }
                }
                break;
            case 8:
                if (this.role.isIntegrate) {
                    if (this.role.famulus.skill != null && this.role.famulus.skill.bLevel < 9) {
                        Skill skill = this.role.famulus.skill;
                        skill.bLevel = (byte) (skill.bLevel + 1);
                        sb.append("使用了" + prop.data.name + ",");
                        sb.append("剑灵的技能等级提升了1级！");
                        this.bag.deleteProp(prop, 1);
                    } else {
                        script_showMessage("剑灵技能等级已达上限", 0);
                    }
                } else if (this.role.equip.skill != null) {
                    if (this.role.equip.skill.bLevel < 9) {
                        Skill skill2 = this.role.equip.skill;
                        skill2.bLevel = (byte) (skill2.bLevel + 1);
                        sb.append("使用了" + prop.data.name + ",");
                        sb.append("当前装备的武器技能等级提升了1级！");
                        this.bag.deleteProp(prop, 1);
                    } else {
                        script_showMessage("主角技能等级已达上限", 0);
                    }
                } else {
                    script_showMessage("剑灵技能等级已达上限", 0);
                }
                break;
            case Interpreter.ERR_ARR_OUT /* 9 */:
                if (this.role.bActID == 3) {
                    sb.append("使用了" + prop.data.name + ",");
                    this.role.reLive();
                    sb.append("气血完全恢复！");
                    this.bag.deleteProp(prop, 1);
                } else {
                    script_showMessage("无须复活!", 0);
                }
                break;
            case 10:
                if (this.role.isIntegrate) {
                    this.role.famulus.iCurDurability = this.role.famulus.iFinalDurability;
                    sb.append("使用了" + prop.data.name + ",");
                    sb.append("剑灵耐久恢复如初！");
                    this.bag.deleteProp(prop, 1);
                } else {
                    this.role.equip.iCurDurability = this.role.equip.iFinalDurability;
                    sb.append("使用了" + prop.data.name + ",");
                    sb.append("武器耐久恢复如初！");
                    this.bag.deleteProp(prop, 1);
                }
                break;
            case 11:
            case 12:
            case 13:
                sb.append("剑灵获得");
                String[] temp = Tool.getStringArray(prop.data.effectStr, ',');
                int value2 = Integer.parseInt(temp[1]) * prop.number * this.bExpCoefficientBySMS;
                sb.append(value2);
                sb.append("点经验");
                this.role.famulus.addExp(value2);
                this.bag.deleteProp(prop, prop.number);
                break;
            case 14:
            case 15:
                String[] temp2 = Tool.getStringArray(prop.data.effectStr, ',');
                int value3 = Integer.parseInt(temp2[1]) * prop.number;
                sb.append(value3);
                script_addMoney(value3, true);
                this.bag.deleteProp(prop, prop.number);
                break;
            case Graphics.TOP /* 16 */:
                String[] temp3 = Tool.getStringArray(prop.data.effectStr, ',');
                this.role.levelUp(Integer.parseInt(temp3[1]));
                this.bag.deleteProp(prop.data.bID, 1);
                break;
            case Graphics.HT /* 17 */:
                String[] temp4 = Tool.getStringArray(prop.data.effectStr, ',');
                int value4 = Integer.parseInt(temp4[1]);
                this.role.iHPUpByProp += value4;
                this.role.updateAttribute(false);
                sb.append("气血上限永久提升" + value4);
                this.bag.deleteProp(prop.data.bID, 1);
                break;
            case 18:
                String[] temp5 = Tool.getStringArray(prop.data.effectStr, ',');
                int value5 = Integer.parseInt(temp5[1]);
                this.role.iAPUpByProp += value5;
                this.role.updateAttribute(false);
                sb.append("基础攻击永久提升" + value5);
                this.bag.deleteProp(prop.data.bID, 1);
                break;
            case TimeUtils.HUNDRED_DAY_FIELD_LEN /* 19 */:
                String[] temp6 = Tool.getStringArray(prop.data.effectStr, ',');
                int value6 = Integer.parseInt(temp6[1]);
                this.role.iDPUpByProp += value6;
                this.role.updateAttribute(false);
                sb.append("基础防御永久提升" + value6);
                this.bag.deleteProp(prop.data.bID, 1);
                break;
            case 20:
                String[] temp7 = Tool.getStringArray(prop.data.effectStr, ',');
                int value7 = Integer.parseInt(temp7[1]);
                this.role.iHitPointUpByProp += value7;
                this.role.updateAttribute(false);
                sb.append("基础命中永久提升" + value7);
                this.bag.deleteProp(prop.data.bID, 1);
                break;
            case 21:
                String[] temp8 = Tool.getStringArray(prop.data.effectStr, ',');
                int value8 = Integer.parseInt(temp8[1]);
                this.role.iDodgePointUpByProp += value8;
                this.role.updateAttribute(false);
                sb.append("基础闪避永久提升" + value8);
                this.bag.deleteProp(prop.data.bID, 1);
                break;
            case 22:
                String[] temp9 = Tool.getStringArray(prop.data.effectStr, ',');
                int value9 = Integer.parseInt(temp9[1]);
                this.role.iCriticalPointUpByProp += value9;
                this.role.updateAttribute(false);
                sb.append("基础暴击永久提升" + value9);
                this.bag.deleteProp(prop.data.bID, 1);
                break;
            case 23:
                sb.append("使用了" + prop.data.name + ",");
                if (this.role.isIntegrate) {
                    this.role.famulus.skill = new Skill(skillDatas[Tool.getNextRnd(5, 22)]);
                    sb.append("剑灵获取了一个新技能！");
                } else {
                    this.role.equip.skill = new Skill(skillDatas[Tool.getNextRnd(5, 22)]);
                    sb.append("当前装备的武器获取了一个新技能！");
                }
                this.role.updateSkill();
                this.bag.deleteProp(prop.data.bID, 1);
                break;
        }
        if (sb.toString().length() > 0) {
            addScrollInfo(sb.toString());
        }
    }

    protected void autoUseMedicament() {
        this.medicine = null;
        if (this.role.equip.iCurDurability == 0) {
            for (int i = 0; i < this.bag.propV.size(); i++) {
                Object obj = this.bag.propV.elementAt(i);
                if (obj instanceof Prop) {
                    Prop prop = (Prop) obj;
                    if (prop.data.bID == 10) {
                        this.medicine = prop;
                        break;
                    }
                }
            }
        } else if (this.role.equip.iCurDurability < (this.bRecoverPercent * this.role.equip.iFinalDurability) / 100) {
            int temp = this.role.equip.iFinalDurability - this.role.equip.iCurDurability;
            Prop[] medicines = new Prop[4];
            for (int i2 = 0; i2 < this.bag.propV.size(); i2++) {
                Object obj2 = this.bag.propV.elementAt(i2);
                if (obj2 instanceof Prop) {
                    Prop prop2 = (Prop) obj2;
                    if (medicines[0] == null && prop2.data.bID == 7) {
                        medicines[0] = prop2;
                    }
                    if (medicines[1] == null && prop2.data.bID == 6) {
                        medicines[1] = prop2;
                    }
                    if (medicines[2] == null && prop2.data.bID == 5) {
                        medicines[2] = prop2;
                    }
                    if (medicines[3] == null && prop2.data.bID == 4) {
                        medicines[3] = prop2;
                    }
                }
            }
            if (temp >= 2000) {
                if (medicines[0] != null) {
                    this.medicine = medicines[0];
                }
            } else if (temp >= 800) {
                if (medicines[1] != null) {
                    this.medicine = medicines[1];
                }
            } else if (temp >= 300) {
                if (medicines[2] != null) {
                    this.medicine = medicines[2];
                }
            } else if (medicines[3] != null) {
                this.medicine = medicines[3];
            }
            if (this.medicine == null) {
                for (int i3 = 0; i3 < medicines.length; i3++) {
                    if (medicines[i3] != null) {
                        this.medicine = medicines[i3];
                        break;
                    }
                }
            }
        }
        if (!this.role.isDead() && this.role.iHP < (this.bRecoverPercent * this.role.iFinalHP) / 100) {
            int offHP = this.role.iFinalHP - this.role.iHP;
            Prop[] medicines2 = new Prop[4];
            for (int i4 = 0; i4 < this.bag.propV.size(); i4++) {
                Object obj3 = this.bag.propV.elementAt(i4);
                if (obj3 instanceof Prop) {
                    Prop prop3 = (Prop) obj3;
                    if (medicines2[0] == null && prop3.data.bID == 3) {
                        medicines2[0] = prop3;
                    }
                    if (medicines2[1] == null && prop3.data.bID == 2) {
                        medicines2[1] = prop3;
                    }
                    if (medicines2[2] == null && prop3.data.bID == 1) {
                        medicines2[2] = prop3;
                    }
                    if (medicines2[3] == null && prop3.data.bID == 0) {
                        medicines2[3] = prop3;
                    }
                }
            }
            if (offHP >= 5000) {
                if (medicines2[0] != null) {
                    this.medicine = medicines2[0];
                }
            } else if (offHP >= 2000) {
                if (medicines2[1] != null) {
                    this.medicine = medicines2[1];
                }
            } else if (offHP >= 1000) {
                if (medicines2[2] != null) {
                    this.medicine = medicines2[2];
                }
            } else if (medicines2[3] != null) {
                this.medicine = medicines2[3];
            }
            if (this.medicine == null) {
                for (int i5 = 0; i5 < medicines2.length; i5++) {
                    if (medicines2[i5] != null) {
                        this.medicine = medicines2[i5];
                        break;
                    }
                }
            }
        }
        if (this.medicine != null) {
            this.role.famulus.act((byte) 3);
        }
    }

    private void moveScreen() {
        if (this.map != null && this.role != null && this.role.actionSet != null && !this.isScreenStay) {
            if (this.map.sMapWidth < SCREEN_WIDTH) {
                this.iViewMapX = (this.map.sMapWidth - SCREEN_WIDTH) >> 1;
                this.iBGImgX1 = 0;
            } else {
                if (!this.role.isFaceToTheRight) {
                    if (this.iViewMapX >= ((this.role.iMapX + (this.role.actionSet.footWidth >> 1)) - (SCREEN_WIDTH - this.DISTANCE)) + 12) {
                        this.iViewMapX -= 12;
                    } else {
                        this.iViewMapX = (this.role.iMapX + (this.role.actionSet.footWidth >> 1)) - (SCREEN_WIDTH - this.DISTANCE);
                    }
                } else if (this.iViewMapX <= ((this.role.iMapX + (this.role.actionSet.footWidth >> 1)) - this.DISTANCE) - 12) {
                    this.iViewMapX += 12;
                } else {
                    this.iViewMapX = (this.role.iMapX + (this.role.actionSet.footWidth >> 1)) - this.DISTANCE;
                }
                if (this.iViewMapX < 0) {
                    this.iViewMapX = 0;
                } else if (this.iViewMapX > this.map.sMapWidth - SCREEN_WIDTH) {
                    this.iViewMapX = this.map.sMapWidth - SCREEN_WIDTH;
                }
                int bgOffX = ((this.iViewMapX << 1) / 3) % (SCREEN_WIDTH << 1);
                if (bgOffX <= SCREEN_WIDTH) {
                    this.iBGImgX1 = -bgOffX;
                } else {
                    this.iBGImgX1 = (SCREEN_WIDTH << 1) - bgOffX;
                }
                this.iBGImgX2 = SCREEN_WIDTH - bgOffX;
            }
            if (this.map.sMapHeight < SCREEN_HEIGHT) {
                this.iViewMapY = (this.map.sMapHeight - SCREEN_HEIGHT) >> 1;
            } else {
                this.iViewMapY = this.role.iMapY - ((SCREEN_HEIGHT << 2) / 5);
                if (this.iViewMapY < 0) {
                    this.iViewMapY = 0;
                } else if (this.iViewMapY > this.map.sMapHeight - SCREEN_HEIGHT) {
                    this.iViewMapY = this.map.sMapHeight - SCREEN_HEIGHT;
                }
            }
            this.map.setViewPosInMap(this.iViewMapX, this.iViewMapY);
        }
    }

    private boolean isUnBlocked(Sprite mapElement, int vx, int vy) {
        if (vx == 0 && vy == 0) {
            return false;
        }
        int footX = mapElement.iMapX + vx;
        int footY = mapElement.iMapY + vy;
        if (mapElement.isCheckMapBlock) {
            int msx = footX / this.map.sCellWidth;
            int msy = footY / this.map.sCellHeight;
            int mex = (mapElement.iFootWidth + footX) / this.map.sCellWidth;
            int mey = (mapElement.iFootHeight + footY) / this.map.sCellHeight;
            for (int i = msx; i <= mex; i++) {
                for (int j = msy; j <= mey; j++) {
                    if (!this.map.canBlockPass(i, j)) {
                        return false;
                    }
                }
            }
        }
        return true;
    }

    protected boolean isUnBlocked(Sprite mapElement, int x) {
        int footY = mapElement.iMapY;
        if (mapElement.isCheckMapBlock) {
            int msx = x / this.map.sCellWidth;
            int msy = footY / this.map.sCellHeight;
            int mex = (mapElement.iFootWidth + x) / this.map.sCellWidth;
            int mey = (mapElement.iFootHeight + footY) / this.map.sCellHeight;
            for (int i = msx; i <= mex; i++) {
                for (int j = msy; j <= mey; j++) {
                    if (!this.map.canBlockPass(i, j)) {
                        return false;
                    }
                }
            }
        }
        return true;
    }

    protected boolean isOutOfMap(Sprite mapElement, int vx, int vy) {
        int footX = mapElement.iMapX + vx;
        int footY = mapElement.iMapY + vy;
        int headY = footY + mapElement.curFrame.edgeRect.y;
        if (footX > 0 && headY > 0 && mapElement.iFootWidth + footX < this.map.sMapWidth && mapElement.iFootHeight + footY < this.map.sMapHeight) {
            return false;
        }
        if (mapElement.bType != 15) {
            return true;
        }
        mapElement.isDead = true;
        return true;
    }

    protected boolean isOutOfMap(Sprite mapElement, int x) {
        return x <= 0 || mapElement.iFootWidth + x >= this.map.sMapWidth;
    }

    boolean isSpriteMoveable(Sprite mapElement) {
        return isSpriteMoveable(mapElement, mapElement.iCurVx, mapElement.iCurVy);
    }

    public boolean isSpriteMoveable(Sprite mapElement, int vx, int vy) {
        if ((vx == 0 && vy == 0) || isOutOfMap(mapElement, vx, vy) || !isUnBlocked(mapElement, vx, vy)) {
            return false;
        }
        int footX = mapElement.iMapX + vx;
        int footY = mapElement.iMapY + vy;
        for (int i = 0; i < this.spriteOnLayer0.size(); i++) {
            Sprite me = (Sprite) this.spriteOnLayer0.elementAt(i);
            if (me != mapElement && !me.isInvisible && (me.bType == 1 || me.bType == 4)) {
                if (me.curFrame.bodyRect != null) {
                    for (int j = 0; j < me.curFrame.bodyRect.length; j++) {
                        if (Tool.isRectIntersected(footX, footY, mapElement.iFootWidth, mapElement.iFootHeight, me.iMapX + me.curFrame.bodyRect[j].x, me.iMapY + me.curFrame.bodyRect[j].y, me.curFrame.bodyRect[j].width, me.curFrame.bodyRect[j].height)) {
                            return false;
                        }
                    }
                }
                if (Tool.isRectIntersected(footX, footY, mapElement.iFootWidth, mapElement.iFootHeight, me.iMapX, me.iMapY, me.iFootWidth, me.iFootHeight)) {
                    return false;
                }
            }
        }
        for (int i2 = 0; i2 < this.spriteOnLayer1.size(); i2++) {
            Sprite me2 = (Sprite) this.spriteOnLayer1.elementAt(i2);
            if (me2 != mapElement && !me2.isInvisible) {
                if (me2.bType == 1 || me2.bType == 4) {
                    if (me2.curFrame.bodyRect != null) {
                        for (int j2 = 0; j2 < me2.curFrame.bodyRect.length; j2++) {
                            if (Tool.isRectIntersected(footX, footY, mapElement.iFootWidth, mapElement.iFootHeight, me2.iMapX + me2.curFrame.bodyRect[j2].x, me2.iMapY + me2.curFrame.bodyRect[j2].y, me2.curFrame.bodyRect[j2].width, me2.curFrame.bodyRect[j2].height)) {
                                return false;
                            }
                        }
                    }
                    if (Tool.isRectIntersected(footX, footY, mapElement.iFootWidth, mapElement.iFootHeight, me2.iMapX, me2.iMapY, me2.iFootWidth, me2.iFootHeight)) {
                        return false;
                    }
                }
                if (me2.bType == 17 && !me2.isDead && mapElement.bType == 0 && Tool.isRectIntersected(this.role.iMapX, this.role.iMapY, this.role.iFootWidth, this.role.iFootHeight, me2.iMapX, me2.iMapY, me2.iFootWidth, me2.iFootHeight)) {
                    me2.isDead = true;
                    getItem(((NPCSprite) me2).item);
                    break;
                }
            }
        }
        return true;
    }

    boolean moveSpriteByStep(Sprite spr, int baseWidth, int baseHeight) {
        int Vx = spr.iCurVx;
        int Vy = spr.iCurVy;
        if (Math.abs(Vx) <= Math.abs(baseWidth) && Math.abs(Vy) <= Math.abs(baseHeight)) {
            if (isSpriteMoveable(spr)) {
                spr.move(Vx, Vy);
                if (spr.bType == 0) {
                    checkScript();
                }
                return true;
            }
            return false;
        }
        int tempVx1 = 0;
        int tempVy1 = 0;
        int tempVx2 = 0;
        int tempVy2 = 0;
        int tempCount = 0;
        int dir = spr.bCurDir;
        if (spr.bActID != 1) {
            if (spr.isFaceToTheRight) {
                dir = 1;
            } else {
                dir = 0;
            }
        }
        switch (dir) {
            case 0:
                tempVx1 = baseWidth * (Vx / Math.abs(Vx));
                tempVy1 = 0;
                tempCount = Vx / tempVx1;
                tempVx2 = Vx % baseWidth;
                tempVy2 = 0;
                break;
            case 1:
                tempVx1 = baseWidth * (Vx / Math.abs(Vx));
                tempVy1 = 0;
                tempCount = Vx / tempVx1;
                tempVx2 = Vx % baseWidth;
                tempVy2 = 0;
                break;
            case 2:
                tempVx1 = 0;
                tempVy1 = baseHeight * (Vy / Math.abs(Vy));
                tempCount = Vy / tempVy1;
                tempVx2 = 0;
                tempVy2 = Vy % baseHeight;
                break;
            case 3:
                tempVx1 = 0;
                tempVy1 = baseHeight * (Vy / Math.abs(Vy));
                tempCount = Vy / tempVy1;
                tempVx2 = 0;
                tempVy2 = Vy % baseHeight;
                break;
        }
        for (int i = 0; i < tempCount; i++) {
            if (isSpriteMoveable(spr, tempVx1, tempVy1)) {
                spr.move(tempVx1, tempVy1);
                if (!specialProcessingSpriteMove(spr)) {
                    return false;
                }
            } else {
                return false;
            }
        }
        if (tempVy2 != 0 || tempVx2 != 0) {
            if (isSpriteMoveable(spr, tempVx2, tempVy2)) {
                spr.move(tempVx2, tempVy2);
                if (!specialProcessingSpriteMove(spr)) {
                    return false;
                }
            } else {
                return false;
            }
        }
        return true;
    }

    private boolean specialProcessingSpriteMove(Sprite spr) {
        if (spr.bType == 0) {
            checkScript();
            for (int i = 0; i < this.spriteOnLayer1.size(); i++) {
                Sprite me = (Sprite) this.spriteOnLayer1.elementAt(i);
                if (me.bType == 17 && !me.isDead && Tool.isRectIntersected(this.role.iMapX, this.role.iMapY, this.role.iFootWidth, this.role.iFootHeight, me.iMapX, me.iMapY, me.iFootWidth, me.iFootHeight)) {
                    me.isDead = true;
                    getItem(((NPCSprite) me).item);
                    return true;
                }
            }
            return true;
        }
        if (spr instanceof Bullet) {
            Bullet b = (Bullet) spr;
            boolean isContinueMove = b.isContinueMoveAfterHitTarget();
            return isContinueMove;
        }
        if (!(spr instanceof Enemy) || spr.bActID != 9 || spr.isInScreen(this.iViewMapX, this.iViewMapY, SCREEN_WIDTH, SCREEN_HEIGHT)) {
            return true;
        }
        spr.act((byte) 10);
        ((Enemy) spr).showHurt();
        if (spr.bCurDir == 0) {
            spr.iMapX = (instance.iViewMapX + SCREEN_WIDTH) - spr.iFootWidth;
            return true;
        }
        spr.iMapX = instance.iViewMapX;
        return true;
    }

    private void cycleScrollInfo() {
        switch (this.scrollInfoState) {
            case 0:
                if (this.scrollInfoV.size() > 0) {
                    initScrollInfoBar((String) this.scrollInfoV.elementAt(0));
                    this.scrollInfoV.removeElementAt(0);
                }
                break;
            case 1:
                if (this.infoBarCount < 53) {
                    this.infoBarCount += 10;
                } else {
                    this.infoBarCount = 63;
                    this.scrollInfoState = (byte) 2;
                }
                break;
            case 2:
                this.scrollInfoCount += 6;
                if (this.scrollInfoCount > (font.stringWidth(this.scrollInfo) + 126) - 8) {
                    this.scrollInfoCount = (font.stringWidth(this.scrollInfo) + 126) - 8;
                    this.scrollInfoState = (byte) 0;
                    this.scrollInfo = null;
                }
                break;
        }
    }

    void cycleScreenMove() {
        boolean bOver = false;
        if (this.map.sMapWidth <= SCREEN_WIDTH) {
            bOver = true;
        } else {
            if (this.iViewMapX < this.iScreenMoveToX) {
                this.iViewMapX += this.iScreenMoveVX;
                if (this.iViewMapX > this.iScreenMoveToX) {
                    this.iViewMapX = this.iScreenMoveToX;
                }
            } else if (this.iViewMapX > this.iScreenMoveToX) {
                this.iViewMapX -= this.iScreenMoveVX;
                if (this.iViewMapX < this.iScreenMoveToX) {
                    this.iViewMapX = this.iScreenMoveToX;
                }
            }
            if (this.iViewMapY < this.iScreenMoveToY) {
                this.iViewMapY += this.iScreenMoveVY;
                if (this.iViewMapY > this.iScreenMoveToY) {
                    this.iViewMapY = this.iScreenMoveToY;
                }
            } else if (this.iViewMapY > this.iScreenMoveToY) {
                this.iViewMapY -= this.iScreenMoveVY;
                if (this.iViewMapY < this.iScreenMoveToY) {
                    this.iViewMapY = this.iScreenMoveToY;
                }
            }
            if (this.iViewMapX == this.iScreenMoveToX && this.iViewMapY == this.iScreenMoveToY) {
                bOver = true;
            }
        }
        this.map.setViewPosInMap(this.iViewMapX, this.iViewMapY);
        refreshSpriteInScreen();
        if (bOver) {
            Common.clearKeyStates();
            this.bGameState = (byte) 12;
            setScriptState((byte) 3);
        }
    }

    void cycleSceneStay() {
        if (this.isCountByFrame) {
            this.iScreenStayCounter++;
        } else {
            this.iScreenStayCounter += MILLIS_PRE_UPDATE;
        }
        refreshSpriteInScreen();
        if (this.iScreenStayCounter > this.iScreenStayDuration) {
            if (this.bPreState == 9 && this.isLoadFinish) {
                setGameState((byte) 12);
            } else {
                returnPreState();
            }
            setScriptState((byte) 3);
            Common.clearKeyStates();
        }
        if ((this.bPreState == 18 || this.bPreState == 27) && this.UIST != null) {
            this.UIST.cycle();
        }
    }

    void cycleContorlledSpriteByScript() {
        int i = 0;
        while (i < this.sprsByScriptControlV.size()) {
            Sprite spr = (Sprite) this.sprsByScriptControlV.elementAt(i);
            spr.curAction = spr.actionSet.actionDatas[spr.iCurActionID];
            int frameNum = spr.curAction.frameNum;
            spr.curFrame = spr.actionSet.frameDatas[spr.curAction.frameID[spr.iCurFrameIndex]];
            if (spr.isSpriteAnimate && spr.isDrawInScreenCenter) {
                spr.iMapX = this.iViewMapX + ((SCREEN_WIDTH - spr.iFootWidth) >> 1);
                spr.iMapY = this.iViewMapY + ((SCREEN_HEIGHT - spr.iFootHeight) >> 1);
            }
            if (spr instanceof FightSprite) {
                ((FightSprite) spr).isCurAttack = spr.curAction.isAttack[spr.iCurFrameIndex];
                ((FightSprite) spr).isCurBeAttack = spr.curAction.isBeAttack[spr.iCurFrameIndex];
            }
            if (spr.bActID == 1 && spr.bCurDir > 1) {
                spr.iCurVx = 0;
            } else {
                spr.iCurVx = spr.curAction.iVx[spr.iCurFrameIndex];
                spr.iCurVy = spr.curAction.iVy[spr.iCurFrameIndex];
            }
            if (spr.iDurationFrame > 0) {
                spr.iDurationFrame--;
                spr.iCurFrameIndex++;
                if (spr.iCurFrameIndex == frameNum) {
                    if (!spr.isKeepFrame) {
                        spr.iCurFrameIndex = 0;
                    } else {
                        spr.iCurFrameIndex = frameNum - 1;
                    }
                }
                if (spr instanceof Role) {
                    playSound();
                }
                i++;
            } else {
                if (!spr.isKeepFrame) {
                    spr.initAct();
                }
                spr.isSpriteAnimate = false;
                this.sprsByScriptControlV.removeElementAt(i);
            }
        }
    }

    void cycleScriptMovingSprite() {
        refreshSpriteInScreen();
        boolean bOver = true;
        for (int i = 0; i < this.sprsByScriptControl.length; i++) {
            if (this.sprsByScriptControl[i] != null) {
                if (this.movingSpriteStep[i] != 0) {
                    if (this.movingSpriteDistance[i] < this.movingSpriteTotalDistance[i] - this.movingSpriteStep[i]) {
                        int[] iArr = this.movingSpriteDistance;
                        iArr[i] = iArr[i] + this.movingSpriteStep[i];
                    } else {
                        this.movingSpriteDistance[i] = this.movingSpriteTotalDistance[i];
                    }
                    if (this.movingSpriteDistance[i] >= this.movingSpriteTotalDistance[i]) {
                        this.sprsByScriptControl[i].act((byte) 0);
                    } else {
                        this.sprsByScriptControl[i].act((byte) 1);
                        switch (this.movingSpriteDir[i]) {
                            case 0:
                                this.sprsByScriptControl[i].move(-this.movingSpriteStep[i], 0);
                                break;
                            case 1:
                                this.sprsByScriptControl[i].move(this.movingSpriteStep[i], 0);
                                break;
                            case 2:
                                this.sprsByScriptControl[i].move(0, -this.movingSpriteStep[i]);
                                break;
                            case 3:
                                this.sprsByScriptControl[i].move(0, this.movingSpriteStep[i]);
                                break;
                        }
                        if (this.sprsByScriptControl[i] == this.role) {
                            if (this.isScreenMoveWithRole) {
                                moveScreen();
                            } else {
                                this.iScreenBackX = this.role.iMapX - (SCREEN_WIDTH / 2);
                                if (this.iScreenBackX < 0) {
                                    this.iScreenBackX = 0;
                                } else if (this.iScreenBackX > this.map.sMapWidth - SCREEN_WIDTH) {
                                    this.iScreenBackX = this.map.sMapWidth - SCREEN_WIDTH;
                                }
                                this.iScreenBackY = this.role.iMapY - (SCREEN_HEIGHT / 2);
                                if (this.iScreenBackY < 0) {
                                    this.iScreenBackY = 0;
                                } else if (this.iScreenBackY > this.map.sMapHeight - SCREEN_HEIGHT) {
                                    this.iScreenBackY = this.map.sMapHeight - SCREEN_HEIGHT;
                                }
                            }
                        }
                        bOver = false;
                    }
                } else if (this.movingSpriteDistance[i] > 0) {
                    int[] iArr2 = this.movingSpriteDistance;
                    iArr2[i] = iArr2[i] - 1;
                    this.sprsByScriptControl[i].act((byte) 1);
                    bOver = false;
                } else {
                    this.sprsByScriptControl[i].act((byte) 0);
                }
            }
        }
        if (bOver) {
            this.sprsByScriptControl = null;
            this.movingSpriteDir = null;
            this.movingSpriteStep = null;
            this.movingSpriteTotalDistance = null;
            this.movingSpriteDistance = null;
            this.bGameState = (byte) 12;
            setScriptState((byte) 3);
            Common.clearKeyStates();
        }
    }

    private void handleKey_Scene() {
        if (this.isTalking) {
            handleKey_Talk();
            return;
        }
        if (!this.role.isLocked && !this.role.isDead()) {
            this.isRunning = false;
            if (Common.isUpPressed(true)) {
                this.isRunning = true;
                this.keyCode = 0;
                this.keyV.removeAllElements();
                this.keyV.addElement(this.u);
            } else if (Common.isDownPressed(true)) {
                this.isRunning = true;
                this.keyCode = 1;
                this.keyV.removeAllElements();
                this.keyV.addElement(this.d);
            } else if (Common.isLeftPressed(true)) {
                this.isRunning = true;
                this.keyCode = 2;
                this.keyV.removeAllElements();
                this.keyV.addElement(this.l);
            } else if (Common.isRightPressed(true)) {
                this.isRunning = true;
                this.keyCode = 3;
                this.keyV.removeAllElements();
                this.keyV.addElement(this.r);
            }
            if (Common.isOKPressed(true)) {
                this.keyCode = 4;
                boolean isCallScript = false;
                if (this.role.bActState == 0) {
                    this.talkSprite = getSpriteTalkTo();
                    if (this.talkSprite != null && this.talkSprite.script != null) {
                        Common.clearKeyStates();
                        if (this.talkSprite.bType == 7 || this.talkSprite.bType == 6 || this.talkSprite.bType == 8) {
                            switch (this.role.bCurDir) {
                                case 0:
                                    this.talkSprite.setDir((byte) 1);
                                    break;
                                case 1:
                                    this.talkSprite.setDir((byte) 0);
                                    break;
                            }
                        }
                        if (this.talkSprite.iMoveDistance > 0) {
                            this.talkSprite.act((byte) 0);
                        }
                        isCallScript = true;
                        script_call(this.talkSprite.script, false);
                    }
                }
                if (!isCallScript && this.isAttackable) {
                    this.keyV.removeAllElements();
                    this.keyV.addElement(this.f);
                }
            }
            if (this.isAttackable) {
                if (Common.isKeyPressed(18, true)) {
                    if (this.role.bActState == 0 || this.role.bActState == 2) {
                        this.role.useSkill();
                    }
                } else if (Common.isKeyPressed(20, true)) {
                    if (this.role.bActState == 0 || this.role.bActState == 2) {
                        breakSkill();
                    }
                } else if (Common.isKeyPressed(11, true)) {
                    if (!this.role.isCriticalKill && this.role.bActID != 22) {
                        short s = this.role.sEP;
                        this.role.getClass();
                        if (s == 2000 && this.role.bActID != 26 && this.role.bActState != 1) {
                            if (this.isDurabilityNonLossForever || this.isDurabilityNonLoss || ((this.role.isIntegrate && this.role.famulus.iCurDurability > 0) || (!this.role.isIntegrate && this.role.equip.iCurDurability > 0))) {
                                this.role.act((byte) 22);
                                if (!this.isMaximumPower) {
                                    this.role.sEP = (short) 0;
                                }
                            } else if (!this.role.isIntegrate) {
                                addDropInfo("武器已损坏！");
                            } else {
                                addDropInfo("剑灵耐久不足！");
                            }
                        }
                    }
                } else if (!Common.isKeyPressed(22, true)) {
                    Common.isKeyPressed(21, true);
                }
            }
            if (Common.isKeyPressed(12, true)) {
                if (this.role.bActState == 0) {
                    integrate();
                }
            } else {
                if (Common.isKeyPressed(14, true)) {
                    loadMiniMap();
                    return;
                }
                if (Common.isKeyPressed(9, true)) {
                    createGameMenu();
                } else if (Common.isKeyPressed(10, true)) {
                    if (this.isOpenAllStage || this.isDX) {
                        createSMSShopMenu();
                    }
                }
            }
        }
    }

    void integrate() {
        if (!this.role.isIntegrate) {
            if (this.role.famulus != null) {
                if (this.isDurabilityNonLossForever || this.isDurabilityNonLoss) {
                    this.role.act((byte) 26);
                    this.role.famulus.act((byte) 4);
                    return;
                } else {
                    if (this.role.famulus.iCurDurability > (this.role.famulus.iFinalDurability * 10) / 100) {
                        this.role.famulus.iCurDurability -= (this.role.famulus.iFinalDurability * 10) / 100;
                        this.role.act((byte) 26);
                        this.role.famulus.act((byte) 4);
                        return;
                    }
                    addDropInfo("剑灵耐久不足！");
                    return;
                }
            }
            return;
        }
        this.role.act((byte) 26);
        this.role.sHoldPoint = 49;
        this.role.famulus.act((byte) 5);
        this.role.famulus.transfer();
    }

    void breakSkill() {
        if (!this.role.isIntegrate) {
            if (this.isDurabilityNonLossForever || this.isDurabilityNonLoss || this.role.equip.iCurDurability > 0) {
                this.role.act((byte) 11);
                return;
            } else {
                addDropInfo("武器已损坏！");
                return;
            }
        }
        if (this.isDurabilityNonLossForever || this.isDurabilityNonLoss || this.role.famulus.iCurDurability > 0) {
            this.role.act((byte) 11);
        } else {
            addDropInfo("剑灵耐久不足！");
        }
    }

    void script_criticalKill(boolean isCriticalKillEnd) {
        this.role.isUseCriticalKillByScript = true;
        this.role.isCriticalKillEnd = isCriticalKillEnd;
        this.role.act((byte) 22);
        if (isCriticalKillEnd) {
            this.script.pause();
        }
    }

    void basicTurtorialKeyResponse() {
        if (this.bTurtorialState == 1) {
            for (int i = 0; i < this.keyCodeV.size(); i++) {
                if (this.keyCode == ((Int) this.keyCodeV.elementAt(i)).getVal()) {
                    this.keyCodeV.removeElementAt(i);
                    if (this.bKeyPressTipIndex == 0 && this.keyCode < 4) {
                        this.isTurtorialKey[this.keyCode] = false;
                    }
                    this.keyCode = -1;
                    break;
                }
            }
            if (this.keyCodeV.size() < 1) {
                this.role.initAct();
                this.bTurtorialState = (byte) 0;
                setScriptState((byte) 3);
            }
        }
    }

    private void setKeyCode() {
        this.keyCode = -1;
        if (Common.isUpPressed(false)) {
            this.keyCode = 0;
            return;
        }
        if (Common.isDownPressed(false)) {
            this.keyCode = 1;
            return;
        }
        if (Common.isLeftPressed(false)) {
            this.keyCode = 2;
            return;
        }
        if (Common.isRightPressed(false)) {
            this.keyCode = 3;
            return;
        }
        if (Common.isOKPressed(false)) {
            this.keyCode = 4;
            return;
        }
        if (Common.isKeyPressed(9, false)) {
            this.keyCode = 5;
            return;
        }
        if (Common.isKeyPressed(10, false)) {
            this.keyCode = 6;
            return;
        }
        if (Common.isKeyPressed(12, false)) {
            this.keyCode = 7;
            return;
        }
        if (Common.isKeyPressed(14, false)) {
            this.keyCode = 8;
            return;
        }
        if (Common.isKeyPressed(18, false)) {
            this.keyCode = 9;
        } else if (Common.isKeyPressed(20, false)) {
            this.keyCode = 10;
        } else if (Common.isKeyPressed(11, false)) {
            this.keyCode = 11;
        }
    }

    boolean isFalseResponse() {
        setKeyCode();
        if (this.bTurtorialState == 2) {
            setScriptState((byte) 3);
            if (this.bKeyResponsePoint != this.keyCode) {
                this.isFalseKeyResponse = true;
            } else {
                this.isFalseKeyResponse = false;
            }
        }
        return this.isFalseKeyResponse;
    }

    private void getEquipInfo() {
        if (this.saleVector.size() > 0) {
            this.iUIInfoX = this.OFF_X + 340;
            this.iUIInfoY = 84;
            this.iUIInfoWidth = 70;
            this.iUIInfoHeight = 156;
            Equip equip = (Equip) this.saleVector.elementAt(this.sEquipIndex);
            this.UIST = new StyledText(equip.data.description, this.iUIInfoWidth, this.iUIInfoHeight, Font.fontChinaH / 10, Font.fontChinaH, font);
            return;
        }
        this.UIST = null;
    }

    private void getPropInfo() {
        if (this.bag.propV.size() > 0) {
            this.iUIInfoX = this.OFF_X + 340;
            this.iUIInfoY = 84;
            this.iUIInfoWidth = 70;
            this.iUIInfoHeight = 156;
            Prop prop = (Prop) this.bag.propV.elementAt(this.sItemIndex);
            this.UIST = new StyledText(prop.data.description, this.iUIInfoWidth, this.iUIInfoHeight, Font.fontChinaH / 10, Font.fontChinaH, font);
            return;
        }
        this.UIST = null;
    }

    private void getPreviewInfo() {
        this.iUIInfoX = (SCREEN_WIDTH >> 1) - 74;
        this.iUIInfoY = ((SCREEN_HEIGHT - 240) >> 1) + 72;
        this.iUIInfoWidth = 106;
        this.iUIInfoHeight = 88;
        this.UIST = new StyledText(this.previewEquip.data.description, this.iUIInfoWidth, this.iUIInfoHeight, Font.fontChinaH / 10, Font.fontChinaH, font);
    }

    /* JADX WARN: Code duplicated, block: B:59:0x0100  */
    /* JADX WARN: Code duplicated, block: B:61:0x0105  */
    /* JADX WARN: Code duplicated, block: B:63:0x010b  */
    /* JADX WARN: Code duplicated, block: B:65:0x0111 A[DONT_INVERT] */
    /* JADX WARN: Code duplicated, block: B:66:0x0113  */
    /* JADX WARN: Code duplicated, block: B:67:0x0116  */
    /* JADX WARN: Code duplicated, block: B:68:0x0119  */
    /* JADX WARN: Code duplicated, block: B:70:0x011f  */
    /* JADX WARN: Code duplicated, block: B:72:0x0125  */
    /* JADX WARN: Code duplicated, block: B:73:0x0128  */
    /* JADX WARN: Code duplicated, block: B:74:0x012e  */
    /* JADX WARN: Code duplicated, block: B:76:0x0134  */
    /* JADX WARN: Code duplicated, block: B:78:0x013a  */
    private void getFamulusInfo(int feedState) {
        int rnd;
        int rnd2;
        int rnd3;
        this.iUIInfoX = ((SCREEN_WIDTH - 320) >> 1) + 18;
        this.iUIInfoY = 204;
        this.iUIInfoWidth = 140;
        this.iUIInfoHeight = 38;
        String str = null;
        if (this.role.famulus.sLevel < (this.role.equip.data.bLevel >> 3)) {
            switch (feedState) {
                case 0:
                    int rnd4 = Tool.getNextRnd(0, 2);
                    if (rnd4 == 0) {
                        str = "嗯？对不起，太久没吃东西了，有点没力气……";
                    } else {
                        str = "最近你好像对我不是很照顾了，有些遗憾……";
                    }
                    break;
                case 1:
                    int rnd5 = Tool.getNextRnd(0, 2);
                    if (rnd5 == 0) {
                        str = "还以为你把我扔在一边了……还好不是。";
                    } else {
                        str = "还好没有成为被遗弃的武器……";
                    }
                    break;
                case 2:
                    str = "如果不想养我，干脆把我卖掉吧……我果然是被遗弃的武器呢。";
                    break;
                case 3:
                    str = "我的力量大不如前了，如果有点武器吃就好了。";
                    break;
            }
            this.bFamulusFaceState = (byte) 3;
        } else if (this.role.famulus.sLevel < (this.role.equip.data.bLevel >> 2)) {
            switch (feedState) {
                case 0:
                    int rnd6 = Tool.getNextRnd(0, 2);
                    if (rnd6 == 0) {
                        str = "不想和你说话，这样下去我都要生锈了。";
                    } else {
                        str = "终于想起我来了吗？没有责任感的主人。";
                    }
                    break;
                case 1:
                    int rnd7 = Tool.getNextRnd(0, 2);
                    if (rnd7 == 0) {
                        str = "终于想起要给我武器吃了么，没有责任感的主人？";
                    } else {
                        str = "还好不是完全没有责任感的主人。";
                    }
                    break;
                case 2:
                    str = "如果不想让我成长，不如把我重新封印如何？不让灵剑得到任何成长，这是对我的侮辱。";
                    break;
                case 3:
                    str = "喂！再不给我武器吃的话，我就不理你了！";
                    break;
            }
            this.bFamulusFaceState = (byte) 2;
        } else if (this.role.famulus.sLevel < (this.role.equip.data.bLevel >> 1)) {
            short s = this.role.famulus.sLevel;
            this.role.famulus.getClass();
            if (s < 40) {
                switch (feedState) {
                    case 0:
                        int rnd8 = Tool.getNextRnd(0, 2);
                        if (rnd8 == 0) {
                            str = "你来啦，这次有没有带好吃的武器给我啊？";
                        } else {
                            str = "喂……我肚子饿了，给我点东西吃吧。";
                        }
                        this.bFamulusFaceState = (byte) 1;
                        break;
                    case 1:
                        int rnd9 = Tool.getNextRnd(0, 2);
                        if (rnd9 == 0) {
                            str = "有武器吃吗？又可以成长了~";
                        } else {
                            str = "会是什么武器呢？好奇……";
                        }
                        this.bFamulusFaceState = (byte) 1;
                        break;
                    case 2:
                        int rnd10 = Tool.getNextRnd(0, 2);
                        if (rnd10 == 0) {
                            str = "虽然很可惜，不过也没办法，下次吧。";
                        } else {
                            str = "我早就猜到了，你这小气鬼。";
                        }
                        this.bFamulusFaceState = (byte) 2;
                        break;
                    case 3:
                        int rnd11 = Tool.getNextRnd(0, 3);
                        if (rnd11 == 0) {
                            str = "嗯，好吧，下次来记得带武器给我吃哦。";
                        } else {
                            str = "好吧，一会儿见";
                        }
                        this.bFamulusFaceState = (byte) 1;
                        break;
                }
            } else {
                switch (feedState) {
                    case 0:
                        rnd3 = Tool.getNextRnd(0, 3);
                        if (rnd3 == 0) {
                            str = "肚子很饱，心情很舒畅。叫我有什么事呢？";
                        } else if (rnd3 == 1) {
                            str = "今天天气不错哦，你身体还好吗？";
                        } else {
                            str = "有没有受伤啊？我可以替你使用药品恢复哦。";
                        }
                        this.bFamulusFaceState = (byte) 0;
                        break;
                    case 1:
                        rnd2 = Tool.getNextRnd(0, 3);
                        if (rnd2 == 0) {
                            str = "哇~要给我吃什么呢？有点小小的期待哦~";
                        } else {
                            str = "经常给我武器吃真是谢谢你哦~";
                        }
                        this.bFamulusFaceState = (byte) 0;
                        break;
                    case 2:
                        str = "不给了吗？那就下次再说吧。";
                        this.bFamulusFaceState = (byte) 1;
                        break;
                    case 3:
                        rnd = Tool.getNextRnd(0, 3);
                        if (rnd == 0) {
                            str = "这样就走了？再陪人家聊一下嘛~";
                        } else {
                            str = "这样就走了？再陪人家聊一下嘛~";
                        }
                        this.bFamulusFaceState = (byte) 0;
                        break;
                }
            }
        } else {
            switch (feedState) {
                case 0:
                    rnd3 = Tool.getNextRnd(0, 3);
                    if (rnd3 == 0) {
                        str = "肚子很饱，心情很舒畅。叫我有什么事呢？";
                    } else if (rnd3 == 1) {
                        str = "今天天气不错哦，你身体还好吗？";
                    } else {
                        str = "有没有受伤啊？我可以替你使用药品恢复哦。";
                    }
                    this.bFamulusFaceState = (byte) 0;
                    break;
                case 1:
                    rnd2 = Tool.getNextRnd(0, 3);
                    if (rnd2 == 0) {
                        str = "哇~要给我吃什么呢？有点小小的期待哦~";
                    } else {
                        str = "经常给我武器吃真是谢谢你哦~";
                    }
                    this.bFamulusFaceState = (byte) 0;
                    break;
                case 2:
                    str = "不给了吗？那就下次再说吧。";
                    this.bFamulusFaceState = (byte) 1;
                    break;
                case 3:
                    rnd = Tool.getNextRnd(0, 3);
                    if (rnd == 0) {
                        str = "这样就走了？再陪人家聊一下嘛~";
                    } else {
                        str = "这样就走了？再陪人家聊一下嘛~";
                    }
                    this.bFamulusFaceState = (byte) 0;
                    break;
            }
        }
        if (str != null) {
            this.UIST = new StyledText(str, this.iUIInfoWidth, this.iUIInfoHeight, Font.fontChinaH / 10, Font.fontChinaH, font);
        } else {
            this.UIST = null;
        }
    }

    private void getShopInfo() {
        this.iUIInfoX = this.OFF_X + 340;
        this.iUIInfoY = 84;
        this.iUIInfoWidth = 70;
        this.iUIInfoHeight = 156;
        if (this.bShopType < 2) {
            Prop prop = (Prop) this.shopItems.elementAt(this.sShopIndex);
            this.UIST = new StyledText(prop.data.description, this.iUIInfoWidth, this.iUIInfoHeight, Font.fontChinaH / 10, Font.fontChinaH, font);
        } else {
            this.UIST = null;
        }
    }

    private void getMissionInfo() {
        String missionInfo;
        this.iUIInfoX = this.OFF_X + 273;
        this.iUIInfoY = 78;
        this.iUIInfoWidth = 130;
        this.iUIInfoHeight = 100;
        if (!this.isBranchMission) {
            missionInfo = this.mainMissionInfo;
        } else {
            missionInfo = this.branchMissionInfo;
        }
        if (missionInfo != null) {
            this.UIST = new StyledText(missionInfo, this.iUIInfoWidth, this.iUIInfoHeight, Font.fontChinaH / 10, Font.fontChinaH, font);
        } else {
            this.UIST = null;
        }
    }

    private void getCandidateEquip() {
        this.candidateEquipV = new Vector(26);
        for (int i = 0; i < this.bag.equipV.size(); i++) {
            Equip equip = (Equip) this.bag.equipV.elementAt(i);
            if (this.role.equip != equip) {
                this.candidateEquipV.addElement(equip);
            }
        }
        if (this.candidateEquipV.size() <= 0) {
            this.iUIInfoX = ((SCREEN_WIDTH - 320) >> 1) + 18;
            this.iUIInfoY = 204;
            this.iUIInfoWidth = 140;
            this.iUIInfoHeight = 38;
            this.bFamulusFaceState = (byte) 2;
            this.UIST = new StyledText("你身上根本没有多余的武器嘛！大骗子！", this.iUIInfoWidth, this.iUIInfoHeight, Font.fontChinaH / 10, Font.fontChinaH, font);
            this.isFeed = false;
            return;
        }
        this.isFeed = true;
        this.isFeedOver = false;
        this.sEquipIndex = (short) 0;
        getFamulusInfo(1);
    }

    private void handleKey_Talk() {
        if (this.talkST != null) {
            this.talkST.handleKey();
            if (this.talkST.isToClosed) {
                this.talkST = null;
                if (this.npcHeadImg != null) {
                    this.npcHeadImg.recycle();
                    this.npcHeadImg = null;
                }
                if (this.talkSprite != null) {
                    this.talkSprite = null;
                }
                this.isTalking = false;
                setScriptState((byte) 3);
                Common.clearKeyStates();
            }
        }
    }

    private void handleKey_Message() {
        if (this.message != null && this.lSleepTimeMills <= 0) {
            if ((this.lSleepTimeMills == -1 && Common.isKeyPressed(10, true)) || (this.lSleepTimeMills == 0 && Common.isOKPressed(true))) {
                Common.clearKeyStates();
                this.isReinforceOver = false;
                this.message = null;
                if (this.messageSet.isEmpty()) {
                    returnPreState();
                    setScriptState((byte) 3);
                    return;
                }
                this.message = (StyledText) this.messageSet.firstElement();
                this.messageSet.removeElementAt(0);
                this.messageType = ((Byte) this.messageTypeSet.firstElement()).byteValue();
                this.messageTypeSet.removeElementAt(0);
                this.lSleepTimeMills = ((Integer) this.messageDurationSet.firstElement()).intValue();
                this.messageDurationSet.removeElementAt(0);
            }
        }
    }

    public void handleKey_Choice() {
        if (this.choiceList != null) {
            this.choiceList.handleKey();
        }
        boolean bClose = false;
        if (Common.isOKPressed(true) || Common.isKeyPressed(9, true)) {
            this.lastChoiceIndex = this.choiceList.getFocusIndex();
            bClose = true;
        } else if (this.isChoiceClosable && Common.isKeyPressed(10, true)) {
            this.lastChoiceIndex = -1;
            bClose = true;
        }
        if (bClose) {
            this.choiceInfo = null;
            this.choiceList = null;
            this.bGameState = (byte) 12;
            setScriptState((byte) 3);
        }
    }

    private void handleKey_GameOver() {
        if (Common.isAnyKeyPressed()) {
            Common.clearKeyPressedStates();
            releaseGameRes();
            loadMainMenuRes();
        }
    }

    private void loadLogoRes() {
        this.logoImg = new Bitmap[2];
        for (int i = 0; i < this.logoImg.length; i++) {
            this.logoImg[i] = Tool.loadPImage(String.valueOf(IMG_PATH) + "logo" + i + ".p");
        }
        this.iLogoCounter = 0;
        this.bGameState = (byte) 0;
    }

    private void releaseLogoRes() {
        for (int i = 0; i < this.logoImg.length; i++) {
            if (this.logoImg[i] != null) {
                this.logoImg[i].recycle();
                this.logoImg[i] = null;
            }
        }
        this.logoImg = null;
    }

    private void drawLogo(Graphics g) {
        if (this.iLogoCounter < DROP_POS_INTERVAL) {
            g.setColor(9786);
            g.fillRect(0, 0, SCREEN_WIDTH, SCREEN_HEIGHT);
            g.drawImage(this.logoImg[0], SCREEN_WIDTH >> 1, SCREEN_HEIGHT >> 1, 3);
        }
    }

    private void cycleLogo() {
        if (this.iLogoCounter < DROP_POS_INTERVAL) {
            this.iLogoCounter++;
        } else {
            releaseLogoRes();
            loadMusicSetRes();
        }
    }

    private void loadMusicSetRes() {
        if (this.musicSetImg == null) {
            this.musicSetImg = Tool.loadPImage(String.valueOf(IMG_PATH) + "soundask.p");
        }
        this.bGameState = (byte) 4;
    }

    private void releaseMusicSetRes() {
    }

    private void drawStartMusic(Graphics g) {
        Common.fillUIRect(g, 0);
        g.drawImage(this.musicSetImg, (SCREEN_WIDTH - this.musicSetImg.getWidth()) >> 1, (SCREEN_HEIGHT - this.musicSetImg.getHeight()) - 25, 0);
    }

    private void handleKey_StartMusic() {
        if (Common.isKeyPressed(10, true)) {
            optionData = (byte) 0;
            releaseMusicSetRes();
            loadCGRes();
        } else if (Common.isOKPressed(true) || Common.isKeyPressed(9, true)) {
            if (optionData == 0) {
                optionData = (byte) 2;
            }
            releaseMusicSetRes();
            loadCGRes();
            this.currentBGMusic = "/cover.mid";
            playMusic();
        }
    }

    private void cycleStartMusic() {
        if (this.iLogoCounter < 20) {
            this.iLogoCounter++;
        }
    }

    void loadCGRes() {
        String[] imgStrs = {"t_04", "t_06", "t_05", "s_34"};
        this.CGAS = Common.getActionSet("t_00", imgStrs, true, false);
        this.CGBGImg = new Bitmap[2];
        this.CGBGImg[0] = Tool.loadPImage(String.valueOf(IMG_PATH) + "t_08.p");
        this.CGBGImg[1] = Tool.loadPImage(String.valueOf(IMG_PATH) + "t_07.p");
        this.coverImg = Tool.loadPImage(String.valueOf(IMG_PATH) + "t_00.p");
        this.bGameState = STATE_CG;
    }

    void releaseCGRes() {
        if (this.CGAS != null) {
            this.CGAS.releasImg();
            this.CGAS = null;
        }
        if (this.CGBGImg != null) {
            for (int i = 0; i < this.CGBGImg.length; i++) {
                if (this.CGBGImg[i] != null) {
                    this.CGBGImg[i].recycle();
                    this.CGBGImg[i] = null;
                }
            }
            this.CGBGImg = null;
        }
    }

    void drawCG(Graphics g) {
        Tool.drawImage(g, this.CGBGImg[1], this.CGBGPos[1][0], this.CGBGPos[1][1], 0);
        Tool.drawImage(g, this.CGBGImg[0], this.CGBGPos[0][0] - 18, this.CGBGPos[0][1] - 8, 0);
        for (int i = 0; i < this.leavesV.size(); i++) {
            StageSprite leaf = (StageSprite) this.leavesV.elementAt(i);
            leaf.draw(g, 0, 0, 0, 0);
        }
        if (this.bCGState == 2) {
            if (this.leavesV.isEmpty()) {
                this.CGAS.drawFrameCycle(g, 2, this.iWhiteScreenCounter, SCREEN_WIDTH >> 1, SCREEN_HEIGHT >> 1);
            }
        } else if (this.bCGState == 3) {
            Tool.drawImage(g, this.coverImg, 0, 0, 0);
            if (this.iStopCounter < 3) {
                this.CGAS.drawFrameCycle(g, 2, this.iWhiteScreenCounter + this.iStopCounter, SCREEN_WIDTH >> 1, SCREEN_HEIGHT >> 1);
            } else if (this.iWhiteScreenCounter > 0) {
                this.CGAS.drawFrameCycle(g, 2, this.CGAS.actionDatas[2].frameNum - this.iWhiteScreenCounter, SCREEN_WIDTH >> 1, SCREEN_HEIGHT >> 1);
            }
        }
        Tool.drawString(g, "跳过", (SCREEN_WIDTH - 3) - font.stringWidth("跳过"), (SCREEN_HEIGHT - 3) - FontHeight, 0, -1);
    }

    void cycleCG() {
        if (this.bCGState == 0) {
            if (this.CGBGPos[0][0] <= this.bDistanceX[0] - this.bVx[0]) {
                int[] iArr = this.CGBGPos[0];
                iArr[0] = iArr[0] + this.bVx[0];
            } else {
                this.CGBGPos[0][0] = this.bDistanceX[0];
            }
            if (this.CGBGPos[1][0] >= this.bDistanceX[1] - this.bVx[1]) {
                int[] iArr2 = this.CGBGPos[1];
                iArr2[0] = iArr2[0] + this.bVx[1];
            } else {
                this.CGBGPos[1][0] = this.bDistanceX[1];
            }
            if (this.iCreateLeavesCounter < 3 && !Tool.isCountTimes2Add()) {
                this.iCreateLeavesCounter++;
                StageSprite spr1 = new StageSprite(this.CGAS, Tool.getNextRnd(0, 40) - 80, Tool.getNextRnd(0, 40) - 120);
                StageSprite spr2 = new StageSprite(this.CGAS, Tool.getNextRnd(0, 40) + 320, Tool.getNextRnd(0, 40) - 120);
                spr1.setDir((byte) 0);
                spr2.setDir((byte) 2);
                this.leavesV.addElement(spr1);
                this.leavesV.addElement(spr2);
            }
            if (this.CGBGPos[0][0] == this.bDistanceX[0] && this.CGBGPos[1][0] == this.bDistanceX[1]) {
                this.bCGState = (byte) (this.bCGState + 1);
            }
        } else if (this.bCGState == 1) {
            if (this.CGBGPos[0][1] <= this.bDistanceY[0] - this.bVy[0]) {
                int[] iArr3 = this.CGBGPos[0];
                iArr3[1] = iArr3[1] + this.bVy[0];
            } else {
                this.CGBGPos[0][1] = this.bDistanceY[0];
            }
            if (this.CGBGPos[1][1] >= this.bDistanceY[1] - this.bVy[1]) {
                int[] iArr4 = this.CGBGPos[1];
                iArr4[1] = iArr4[1] + this.bVy[1];
            } else {
                this.CGBGPos[1][1] = this.bDistanceY[1];
            }
            if (this.CGBGPos[0][1] == this.bDistanceY[0] && this.CGBGPos[1][1] == this.bDistanceY[1]) {
                this.bCGState = (byte) (this.bCGState + 1);
            }
        } else if (this.bCGState == 2) {
            if (this.leavesV.isEmpty()) {
                if (this.iWhiteScreenCounter < 5) {
                    this.iWhiteScreenCounter++;
                } else {
                    this.bCGState = (byte) (this.bCGState + 1);
                }
            }
        } else if (this.bCGState == 3) {
            if (this.iStopCounter < 3) {
                this.iStopCounter++;
            } else if (this.iWhiteScreenCounter > 0) {
                this.iWhiteScreenCounter--;
            } else {
                releaseCGRes();
                loadMainMenuRes();
            }
        }
        int i = 0;
        while (i < this.leavesV.size()) {
            StageSprite leaf = (StageSprite) this.leavesV.elementAt(i);
            if (leaf.isDead) {
                this.leavesV.removeElementAt(i);
            } else {
                leaf.cycle();
                i++;
            }
        }
    }

    private void handleKey_CG() {
        if (Common.isKeyPressed(4, true)) {
            releaseCGRes();
            loadMainMenuRes();
        }
    }

    private void releaseMainMenuRes() {
        if (this.coverImg != null) {
            this.coverImg.recycle();
            this.coverImg = null;
        }
        this.recommondImg = null;
    }

    private void loadMainMenuRes() {
        if (this.coverImg == null) {
            this.coverImg = Tool.loadPImage(String.valueOf(IMG_PATH) + "t_00.p");
        }
        if (this.optionFrameImg == null) {
            this.optionFrameImg = Tool.loadPImage(String.valueOf(IMG_PATH) + "t_01.p");
            this.optionImg = Tool.loadPImage(String.valueOf(IMG_PATH) + "t_02.p");
            this.optionArrowImg = Tool.loadPImage(String.valueOf(IMG_PATH) + "t_03.p");
        }
        if (this.mainUIAS == null) {
            String[] imgStrs = {"u_02", "u_00", "u_43", "u_45"};
            this.mainUIAS = Common.getActionSet("u_02", imgStrs, true, true);
        }
        if (this.uiBGImg == null) {
            this.uiBGImg = Tool.loadPImage(String.valueOf(IMG_PATH) + "u_40.p");
        }
        if (this.arrowImg == null) {
            this.arrowImg = Tool.loadPImage(String.valueOf(IMG_PATH) + "u_44.p");
        }
        if (this.numImg == null) {
            this.numImg = new Bitmap[7];
            for (int i = 0; i < this.numImg.length - 1; i++) {
                this.numImg[i] = Tool.loadPImage(String.valueOf(IMG_PATH) + "u_" + (i + 17) + ".p");
            }
        }
        if (this.loadingImgs == null) {
            this.loadingImgs = new Bitmap[4];
            for (int i2 = 0; i2 < this.loadingImgs.length; i2++) {
                this.loadingImgs[i2] = Tool.loadPImage(String.valueOf(ACTION_PATH) + "o_0" + (i2 + 2) + ".p");
            }
        }
        if (this.loadingAS == null) {
            String[] imgFileStrs = {"l_00", "l_01", "l_02", "l_03", "o_02"};
            this.loadingAS = Common.getActionSet("z_loading", imgFileStrs, true, true);
        }
        if (this.recommondImg == null) {
            this.recommondImg = Tool.loadPImage(String.valueOf(IMG_PATH) + "04.p");
        }
        calcMainMenuPos();
        this.isToURL = false;
        this.bOptionIndex = (byte) 0;
        this.bGameState = (byte) 5;
    }

    void calcMainMenuPos() {
        this.OPTION_WIDTH = 99;
        this.OPTION_HEIGHT = 26;
        this.iMainMenuX = (SCREEN_WIDTH - this.OPTION_WIDTH) >> 1;
        this.iMainMenuY = 204;
        if (this.optionArrowImg != null) {
            this.iArrowX_L = (this.iMainMenuX - this.optionArrowImg.getWidth()) - 12;
            this.iArrowX_R = this.iMainMenuX + this.OPTION_WIDTH + 10;
            this.iArrowY = this.iMainMenuY + 8;
        }
    }

    private void drawMainMenu(Graphics g) {
        g.drawImage(this.coverImg, SCREEN_WIDTH >> 1, SCREEN_HEIGHT >> 1, 3);
        g.drawImage(this.optionFrameImg, this.OFF_X + Tool.UI_RATATE_180, 194, 20);
        Tool.drawClipImage(g, this.optionImg, this.iMainMenuX + 8, this.iMainMenuY, 0, this.OPTION_HEIGHT * this.bOptionIndex, this.OPTION_WIDTH, this.OPTION_HEIGHT);
        Tool.drawRegion(g, this.optionArrowImg, 0, 0, this.optionArrowImg.getWidth(), this.optionArrowImg.getHeight(), 0, ((this.iArrowX_L + 20) - (Tool.countTimes2 % 3)) - 14, this.iArrowY);
        Tool.drawRegion(g, this.optionArrowImg, 0, 0, this.optionArrowImg.getWidth(), this.optionArrowImg.getHeight(), 2, this.iArrowX_R + 20 + (Tool.countTimes2 % 3) + 3, this.iArrowY);
        g.canvas.save();
        g.canvas.scale(0.67f, 0.67f, SCREEN_WIDTH - 10, SCREEN_HEIGHT - 10);
        g.drawImage(this.recommondImg, SCREEN_WIDTH - 10, SCREEN_HEIGHT - 10, 40);
        g.canvas.restore();
    }

    private void cycleMainMenu() {
    }

    private void handleKey_MainMenu() {
        if (Common.isLeftPressed(true)) {
            this.bOptionIndex = (byte) (this.bOptionIndex + STATE_CG);
            if (this.bOptionIndex < 0) {
                this.bOptionIndex = (byte) 5;
            }
            if (!this.isQQ && this.bOptionIndex == 2) {
                this.bOptionIndex = (byte) (this.bOptionIndex + STATE_CG);
            }
        } else if (Common.isRightPressed(true)) {
            this.bOptionIndex = (byte) (this.bOptionIndex + 1);
            if (this.bOptionIndex > 5) {
                this.bOptionIndex = (byte) 0;
            }
            if (!this.isQQ && this.bOptionIndex == 2) {
                this.bOptionIndex = (byte) (this.bOptionIndex + 1);
            }
        }
        if (Common.isOKPressed(true) || Common.isKeyPressed(9, true)) {
            switch (this.bOptionIndex) {
                case 0:
                    readRecordNames("TheSoulOfTheSwordTitle");
                    setGameState((byte) 7);
                    this.isFromMainMenu = true;
                    this.bRecordIndex = (byte) 0;
                    break;
                case 1:
                    this.bGameSetState = (byte) 0;
                    setGameState((byte) 3);
                    break;
                case 2:
                    this.isToURL = true;
                    WebNetInterface.StartWeb(JL01Activity.activity);
                    break;
                case 3:
                    this.stHelpAbout = new StyledText(this.helpStr, 280, this.menuHeight - 40, Font.fontChinaH / 10, Font.fontChinaH, font);
                    setGameState((byte) 1);
                    break;
                case 4:
                    this.stHelpAbout = new StyledText(this.aboutStr, 280, this.menuHeight - 40, Font.fontChinaH / 10, Font.fontChinaH, font);
                    setGameState((byte) 2);
                    break;
                case 5:
                    exit();
                    break;
            }
            return;
        }
        if (Common.isKeyPressed(10, true)) {
            JL01Activity.activity.initMoreGame();
        }
    }

    private void drawRecord(Graphics g) {
        this.mainUIAS.drawFrameCycle(g, this.bRecordIndex, Tool.countTimes, SCREEN_WIDTH >> 1, SCREEN_HEIGHT >> 1);
        int tempX = (SCREEN_WIDTH - 320) >> 1;
        int offY = (38 - FontHeight) - 2;
        int i = 0;
        while (i < 2) {
            int y = 82 + (i * 78);
            if (this.recordNames[i].length() > 1) {
                Tool.drawString(g, this.recordTitles[1], tempX + ((320 - font.stringWidth(this.recordTitles[1])) >> 1), y + offY, MotionEventCompat.ACTION_POINTER_INDEX_MASK, 0);
                Tool.drawString(g, this.recordNames[i], tempX + ((320 - font.stringWidth(this.recordNames[i])) >> 1), y + 38 + 2, this.bRecordIndex == i ? MotionEventCompat.ACTION_POINTER_INDEX_MASK : -1, 0);
            } else {
                Tool.drawString(g, this.recordTitles[0], tempX + ((320 - font.stringWidth(this.recordTitles[0])) >> 1), y + ((76 - FontHeight) >> 1), this.bRecordIndex == i ? MotionEventCompat.ACTION_POINTER_INDEX_MASK : -1, 0);
            }
            i++;
        }
        int optionHeight = FontHeight + 6;
        int optionY = (((this.bRecordIndex * 78) + 82) + 38) - optionHeight;
        this.YYY = optionY;
        if (this.isPopUpOption) {
            drawOption(g, this.recordOptions, this.optionWidth, optionHeight, (SCREEN_WIDTH - this.optionWidth) >> 1, optionY, this.bPopUpOptionIndex, 16739328, -1, 0);
        }
    }

    private void handleKey_ReadRecord() {
        if (Common.isUpPressed(true) || Common.isDownPressed(true)) {
            if (!this.isPopUpOption) {
                this.bRecordIndex = (byte) (this.bRecordIndex != 0 ? 0 : 1);
                return;
            } else {
                this.bPopUpOptionIndex = (byte) (this.bPopUpOptionIndex != 0 ? 0 : 1);
                return;
            }
        }
        if (Common.isKeyPressed(9, true) || Common.isOKPressed(true)) {
            if (this.recordNames[this.bRecordIndex].length() > 1) {
                if (!this.isPopUpOption) {
                    this.isPopUpOption = true;
                    this.recordOptions = new String[2];
                    this.recordOptions[0] = "读取";
                    this.recordOptions[1] = "删除";
                    this.optionWidth = font.stringWidth(this.recordOptions[0]) + 24;
                    this.bPopUpOptionIndex = (byte) 0;
                    return;
                }
                this.isPopUpOption = false;
                this.recordOptions = null;
                if (this.bPopUpOptionIndex == 0) {
                    startLoading();
                    if (this.isFromMainMenu) {
                        startGame();
                    }
                    readRecord(this.bRecordIndex);
                    readUnsavedSMSItem();
                    readSMSRecord();
                    this.iCurrentStageID = Interpreter.getValue(SCRIPT_VARIABLES[1]).getVal();
                    if (this.iCurrentStageID == 0) {
                        script_call("start.s", false);
                        return;
                    } else {
                        script_call(String.valueOf(this.iCurrentStageID) + ".s", false);
                        return;
                    }
                }
                this.recordNames[this.bRecordIndex] = "";
                saveRecordNames("TheSoulOfTheSwordTitle");
                delRecord(this.bRecordIndex);
                return;
            }
            newGame();
            return;
        }
        if (Common.isKeyPressed(10, true)) {
            if (this.isPopUpOption) {
                this.isPopUpOption = false;
            } else {
                returnPreState();
            }
        }
    }

    private void drawGameSet(Graphics g) {
        this.mainUIAS.drawFrame(g, 3, SCREEN_WIDTH >> 1, SCREEN_HEIGHT >> 1);
        this.mainUIAS.drawFrame(g, optionData + 5, SCREEN_WIDTH >> 1, SCREEN_HEIGHT >> 1);
        this.mainUIAS.drawFrame(g, 4, SCREEN_WIDTH >> 1, (SCREEN_HEIGHT >> 1) + (this.bGameSetState * 40));
        Tool.drawNumReturnWidth(g, this.numImg[2], this.bRecoverPercent, (SCREEN_WIDTH / 2) + 35, 211, 3, 1, -2);
        Tool.drawRegion(g, this.arrowImg, 0, 0, this.arrowImg.getWidth(), this.arrowImg.getHeight(), 0, this.menuX + 145 + (Tool.countTimes2 % 3), this.menuY + 102 + (this.bGameSetState * 40));
        Tool.drawRegion(g, this.arrowImg, 0, 0, this.arrowImg.getWidth(), this.arrowImg.getHeight(), 2, (this.menuX + 18) - (Tool.countTimes2 % 3), this.menuY + 102 + (this.bGameSetState * 40));
    }

    void handleKey_GameSet() {
        if (!Common.isUpPressed(true)) {
            if (!Common.isDownPressed(true)) {
                if (Common.isLeftPressed(true)) {
                    switch (this.bGameSetState) {
                        case 0:
                            if (optionData > 0) {
                                optionData = (byte) (optionData + STATE_CG);
                                if (optionData > 0) {
                                    JL01Activity.activity.addVolume(false);
                                } else {
                                    JL01Activity.activity.setMinVolume();
                                }
                            }
                            break;
                        case 1:
                            if (this.bRecoverPercent >= 20) {
                                this.bRecoverPercent = (byte) (this.bRecoverPercent - 10);
                            }
                            break;
                    }
                }
                if (Common.isRightPressed(true)) {
                    switch (this.bGameSetState) {
                        case 0:
                            if (optionData < MUSIC_VOLUME.length - 1) {
                                optionData = (byte) (optionData + 1);
                                if (optionData > 0) {
                                    playMusic();
                                    JL01Activity.activity.addVolume(true);
                                } else {
                                    JL01Activity.activity.stopBgMusic();
                                }
                            } else {
                                JL01Activity.activity.setMaxVolume();
                            }
                            break;
                        case 1:
                            if (this.bRecoverPercent <= 80) {
                                this.bRecoverPercent = (byte) (this.bRecoverPercent + 10);
                            }
                            break;
                    }
                }
                if (Common.isKeyPressed(9, true) || Common.isKeyPressed(10, true)) {
                    returnPreState();
                    return;
                }
                return;
            }
            this.bGameSetState = (byte) (this.bGameSetState != 0 ? 0 : 1);
            return;
        }
        this.bGameSetState = (byte) (this.bGameSetState != 0 ? 0 : 1);
    }

    private void drawHelp(Graphics g) {
        this.mainUIAS.drawFrame(g, 12, SCREEN_WIDTH >> 1, SCREEN_HEIGHT >> 1);
        this.stHelpAbout.draw(g, (SCREEN_WIDTH - 280) >> 1, this.menuY + 20);
    }

    private void cycleHelp() {
        if (this.stHelpAbout != null) {
            this.stHelpAbout.cycle();
        }
    }

    private void handleKey_Help() {
        if (Common.isKeyPressed(9, true) || Common.isKeyPressed(10, true)) {
            this.stHelpAbout = null;
            returnPreState();
        }
    }

    private void drawWarning(Graphics g) {
        Common.fillUIRect(g, 0);
        int width = font.stringWidth("请使用手机下载正版游戏");
        Tool.drawString(g, "请使用手机下载正版游戏", (SCREEN_WIDTH - width) >> 1, (SCREEN_HEIGHT - FontHeight) >> 1, 16711680, -1);
    }

    private void drawMoreInfo(Graphics g) {
        Common.fillUIRect(g, 0);
        int width = font.stringWidth("更多精彩游戏");
        g.setColor(-1);
        g.drawString("更多精彩游戏", (SCREEN_WIDTH - width) >> 1, (SCREEN_HEIGHT - (FontHeight * 3)) >> 1, 0);
        g.drawString("尽在游戏频道", (SCREEN_WIDTH - width) >> 1, (SCREEN_HEIGHT - FontHeight) >> 1, 0);
        g.drawString("wap.xjoys.com", (SCREEN_WIDTH - width) >> 1, (SCREEN_HEIGHT + FontHeight) >> 1, 0);
        g.drawString("确认", 4, SCREEN_HEIGHT - 4, 36);
        g.drawString("退出", SCREEN_WIDTH - 4, SCREEN_HEIGHT - 4, 40);
    }

    private void drawExitValidate(Graphics g) {
        Common.fillUIRect(g, 0);
        int width = font.stringWidth("是否确认退出");
        g.setColor(-1);
        g.drawString("是否确认退出", (SCREEN_WIDTH - width) >> 1, (SCREEN_HEIGHT - FontHeight) >> 1, 0);
        g.drawString("是", 4, SCREEN_HEIGHT - 4, 36);
        g.drawString("否", SCREEN_WIDTH - 4, SCREEN_HEIGHT - 4, 40);
    }

    private void drawReturnValidate(Graphics g) {
        drawRoundRectBGByTile(g, this.uiBGImg, 10, SCREEN_HEIGHT >> 1, this.uiBGImg.getHeight(), this.uiBGImg.getHeight(), SCREEN_WIDTH - 20, 70);
        g.setColor(-1);
        Tool.drawString(g, "是", 20, ((SCREEN_HEIGHT >> 1) + 66) - FontHeight, 16711680, -1, (byte) 1);
        Tool.drawString(g, "否", (SCREEN_WIDTH - 20) - FontHeight, ((SCREEN_HEIGHT >> 1) + 66) - FontHeight, 16711680, -1, (byte) 1);
    }

    private void drawLoading(Graphics g) {
        Common.fillUIRect(g, 0);
        g.setColor(-1);
        this.loadingAS.drawFrameCycle(g, 0, this.iLoadingCounter, SCREEN_WIDTH >> 1, (SCREEN_HEIGHT >> 1) + 40);
        if (this.message != null) {
            this.message.draw(g, 10, 25);
        }
    }

    private void cycleLoad() {
        moveScreen();
        if (this.message != null) {
            this.message.cycle();
        }
        if (this.iLoadOverCounter <= 0) {
            this.message = null;
            if (!this.isOtherState) {
                this.bGameState = (byte) 12;
                if (!this.isOpenAllStage) {
                }
            }
            setScriptState((byte) 3);
        }
    }

    private void drawNotify(Graphics g) {
        Common.fillUIRect(g, 0);
        Tool.drawString(g, "请按键返回", (SCREEN_WIDTH - font.stringWidth("请按键返回")) >> 1, (SCREEN_HEIGHT - FontHeight) >> 1, 16711680, -1);
    }

    private void loadGameRes() {
        if (this.numImg[6] == null) {
            this.numImg[6] = Tool.loadPImage(String.valueOf(IMG_PATH) + "u_31.p");
            this.skillIconImg = Tool.loadPImage("/pic/skill.p");
            this.skillIconAndroid = Tool.loadPImage("/pic/skillandrod.p");
            this.propIconImg = Tool.loadPImage("/pic/item.p");
            this.shortCutImg = Tool.loadPImage(String.valueOf(IMG_PATH) + "sign.p");
            signImg = Tool.loadPImage(String.valueOf(IMG_PATH) + "u_32.p");
            this.scrollSignImg = Tool.loadPImage(String.valueOf(IMG_PATH) + "u_27.p");
            this.talkImg = Tool.loadPImage(String.valueOf(IMG_PATH) + "u_28.p");
            this.goldImg = Tool.loadPImage(String.valueOf(IMG_PATH) + "coin.p");
            this.roleHPImg = Tool.loadPImage(String.valueOf(IMG_PATH) + "u_23.p");
            this.famulusDurabilityImg = Tool.loadPImage(String.valueOf(IMG_PATH) + "u_07.p");
            this.bossHPImg = Tool.loadPImage(String.valueOf(IMG_PATH) + "u_25.p");
            this.roleMPImg = Tool.loadPImage(String.valueOf(IMG_PATH) + "u_16.p");
            this.spImg = Tool.loadPImage(String.valueOf(IMG_PATH) + "u_26.p");
            this.roleExpImg0 = Tool.loadPImage(String.valueOf(IMG_PATH) + "u_08.p");
            loseHPImg0 = Tool.loadPImage(String.valueOf(IMG_PATH) + "u_33.p");
            loseHPImg1 = Tool.loadPImage(String.valueOf(IMG_PATH) + "u_34.p");
            addHPImg = Tool.loadPImage(String.valueOf(IMG_PATH) + "u_35.p");
            addMPImg = Tool.loadPImage(String.valueOf(IMG_PATH) + "u_36.p");
            missImg = Tool.loadPImage(String.valueOf(IMG_PATH) + "miss.p");
            this.headImgs = new Bitmap[2];
            this.headImgs[0] = Tool.loadPImage(String.valueOf(IMG_PATH) + "u_37.p");
            this.headImgs[1] = Tool.loadPImage(String.valueOf(IMG_PATH) + "u_38.p");
            this.comboImg = new Bitmap[3];
            for (int i = 0; i < this.comboImg.length; i++) {
                this.comboImg[i] = Tool.loadPImage(String.valueOf(IMG_PATH) + "u_" + (i + 12) + ".p");
            }
            this.comboTipImg = Tool.loadPImage(String.valueOf(IMG_PATH) + "u_29.p");
            this.comboNumImg = Tool.loadPImage(String.valueOf(IMG_PATH) + "u_30.p");
            for (int i2 = 0; i2 < 3; i2++) {
                this.dropInfo[i2] = new StyledText(SCREEN_WIDTH, 1, FontHeight + 2, font);
            }
            String[] imgStr = {"jiantou"};
            this.keyPressTipAS = Common.getActionSet("jiantou", imgStr, false, false);
            String[] imgs = {"s_26"};
            beatedEffectAS = Common.getActionSet("s_26", imgs, false, false);
            String[] imgs2 = {"s_34"};
            criticalEffectAS = Common.getActionSet("s_32", imgs2, false, false);
            String[] imgs3 = {"u_01", "u_03", "u_05", "u_00", "jiantou", "ui", "help"};
            this.sceneUIAS = Common.getActionSet("u_00", imgs3, false, false);
            String[] imgs4 = {"s_99"};
            faceAS = Common.getActionSet("e_99", imgs4, false, false);
            this.moneyImg = Tool.loadPImage(String.valueOf(IMG_PATH) + "money.p");
            this.iconBGImg = new Bitmap[5];
            for (int i3 = 0; i3 < this.iconBGImg.length; i3++) {
                this.iconBGImg[i3] = Tool.loadPImage(String.valueOf(IMG_PATH) + "i_0" + (i3 + 1) + ".p");
            }
            this.arrowUpImg = Tool.loadPImage(String.valueOf(IMG_PATH) + "u_42.p");
            this.attributeUpNumImg = Tool.loadPImage(String.valueOf(IMG_PATH) + "u_41.p");
            loadSMSRes();
        }
    }

    private void loadSMSRes() {
        this.infoSMImg = new Bitmap[6];
        for (int i = 0; i < this.infoSMImg.length - 1; i++) {
            this.infoSMImg[i] = Tool.loadPImage(String.valueOf(IMG_PATH) + "sminfo_" + i + ".p");
        }
        this.infoSMImg[5] = Tool.loadPImage(String.valueOf(IMG_PATH) + "sminfo_0_1.p");
        this.tipSMImg = Tool.loadPImage(String.valueOf(IMG_PATH) + "smtip.p");
        this.optionSMImg = new Bitmap[12];
        for (int i2 = 0; i2 < this.optionSMImg.length; i2++) {
            if (i2 < 4) {
                this.optionSMImg[i2] = Tool.loadPImage(String.valueOf(IMG_PATH) + "sm" + i2 + ".p");
            } else if (i2 < 6) {
                this.optionSMImg[4] = Tool.loadPImage(String.valueOf(IMG_PATH) + "sm4_1.p");
                this.optionSMImg[5] = Tool.loadPImage(String.valueOf(IMG_PATH) + "sm4_2.p");
            } else if (i2 < 10) {
                this.optionSMImg[i2] = Tool.loadPImage(String.valueOf(IMG_PATH) + "sm" + (i2 - 6) + "_1.p");
            } else {
                this.optionSMImg[10] = Tool.loadPImage(String.valueOf(IMG_PATH) + "sm4_1_1.p");
                this.optionSMImg[11] = Tool.loadPImage(String.valueOf(IMG_PATH) + "sm4_2_1.p");
            }
        }
        this.SmsImage = Tool.loadPImage(String.valueOf(IMG_PATH) + "issend.p");
    }

    private void releaseGameRes() {
        this.map.closeMap();
        this.map = null;
        for (int i = 0; i < this.spriteInStage.size(); i++) {
            Sprite me = (Sprite) this.spriteInStage.elementAt(i);
            if (me != null) {
                me.freeSprite();
            }
        }
        this.loadingStateV.removeAllElements();
        this.scriptFileToRunV.removeAllElements();
        this.mapScripts.removeAllElements();
        this.spriteOnLayer0.removeAllElements();
        this.spriteOnLayer1.removeAllElements();
        this.spriteOnLayer2.removeAllElements();
        this.spriteBelowLayer0.removeAllElements();
        this.enemyInScreen.removeAllElements();
        this.bulletInScreen.removeAllElements();
        this.effectV.removeAllElements();
        this.spriteInStage.removeAllElements();
        this.sprsByScriptControlV.removeAllElements();
        this.fightElement.removeAllElements();
        Common.clearResCache.clear();
    }

    int getBGColor() {
        return this.iBGColor;
    }

    void setBGColor(int color) {
        this.iBGColor = color;
    }

    private void drawScene(Graphics g) {
        g.setColor(0);
        g.fillRect(0, 0, SCREEN_WIDTH, SCREEN_HEIGHT);
        boolean isNormal = false;
        if (this.role.isCriticalKill) {
            g.setColor(0);
            g.fillRect(0, 0, SCREEN_WIDTH, SCREEN_HEIGHT);
            int effectX = this.role.isFaceToTheRight ? ((this.role.iMapX - this.iViewMapX) - 64) + 120 : ((this.role.iMapX - this.iViewMapX) + 64) - 120;
            int effectY = (SCREEN_HEIGHT >> 1) + (((160 - SCREEN_HEIGHT) + this.role.iMapY) - this.iViewMapY);
            if (this.role.bActID == 24) {
                if (this.role.iCurFrameIndex < 6) {
                    this.role.criticalKillAS.drawFrameCycle(g, 0, this.role.iCurFrameIndex, effectX, effectY);
                    this.role.draw(g, this.iViewMapX, this.iViewMapY, 0, 0);
                    drawSpriteDuringSpecialSkill(g);
                } else {
                    this.role.draw(g, this.iViewMapX, this.iViewMapY, 0, 0);
                    if (this.role.isIntegrate) {
                        this.role.criticalKillAS.drawFrameCycle(g, 2, this.role.iCurFrameIndex - 6, effectX, effectY);
                    } else {
                        this.role.criticalKillAS.drawFrameCycle(g, 1, this.role.iCurFrameIndex - 6, effectX, effectY);
                    }
                    drawSpriteDuringSpecialSkill(g);
                    if (this.role.isFaceToTheRight) {
                        this.role.criticalKillAS.drawFrameCycle(g, 3, this.role.iCurFrameIndex - 6, effectX, this.role.iMapY);
                    } else {
                        this.role.criticalKillAS.drawFrameCycle(g, 4, this.role.iCurFrameIndex - 6, effectX, this.role.iMapY);
                    }
                    if (this.role.iCurFrameIndex > 33) {
                        drawComboTip(g);
                    }
                }
            } else if (this.role.bActID == 25) {
                this.role.criticalKillAS.drawFrameCycle(g, 5, this.role.iCurFrameIndex, effectX, this.role.iMapY);
                this.role.draw(g, this.iViewMapX, this.iViewMapY, this.role.isFaceToTheRight ? 0 : -112, 0);
                this.role.criticalKillAS.drawFrameCycle(g, 6, this.role.iCurFrameIndex, effectX, this.role.iMapY);
                if (this.role.iCurFrameIndex > 50 && this.role.iCurFrameIndex < 55) {
                    this.role.criticalKillAS.drawFrameCycle(g, 10, this.role.iCurFrameIndex - 51, effectX + 120, this.role.iMapY);
                }
                drawSpriteDuringSpecialSkill(g);
                if (!this.role.isIntegrate) {
                    this.role.criticalKillAS.drawFrameCycle(g, 7, this.role.iCurFrameIndex, effectX, this.role.iMapY);
                } else {
                    this.role.criticalKillAS.drawFrameCycle(g, 8, this.role.iCurFrameIndex, effectX, this.role.iMapY);
                }
                drawComboTip(g);
            }
        } else if (this.role.enemy != null) {
            if (this.role.enemy.isBossCriticalKill) {
                if (this.role.enemy.bActID == 22) {
                    g.setColor(0);
                    g.fillRect(0, 0, SCREEN_WIDTH, SCREEN_HEIGHT);
                    if (this.role.enemy.iCurFrameIndex < 6) {
                        this.role.criticalKillAS.drawFrameCycle(g, 0, this.role.enemy.iCurFrameIndex, SCREEN_WIDTH >> 1, SCREEN_HEIGHT >> 1);
                    } else {
                        this.role.enemy.actionSet.drawFrameCycle(g, 23, this.role.enemy.iCurFrameIndex, (SCREEN_WIDTH - this.role.enemy.iFootWidth) >> 1, (SCREEN_HEIGHT - this.role.enemy.iFootHeight) >> 1);
                    }
                    this.role.draw(g, this.iViewMapX, this.iViewMapY, 0, 0);
                    this.role.enemy.draw(g, this.iViewMapX, this.iViewMapY, 0, 0);
                } else if (this.role.enemy.iSortID == 241 || this.role.enemy.iSortID == 242) {
                    g.setColor(this.iBGColor);
                    g.fillRect(0, 0, SCREEN_WIDTH, SCREEN_HEIGHT);
                    this.role.draw(g, this.iViewMapX, this.iViewMapY, 0, 0);
                    if (!this.role.enemy.isInvisible) {
                        this.role.enemy.draw(g, this.iViewMapX, this.iViewMapY, 0, 0);
                    }
                    Bullet b = (Bullet) this.bulletInScreen.firstElement();
                    b.draw(g, this.iViewMapX, this.iViewMapY, 0, 0);
                    this.role.enemy.actionSet.drawFrameCycle(g, 26, b.iCurFrameIndex, b.iMapX - this.iViewMapX, b.iMapY - this.iViewMapY);
                } else {
                    isNormal = true;
                }
            } else if (this.role.enemy.bActID == 3) {
                g.setColor(this.iBGColor);
                g.fillRect(0, 0, SCREEN_WIDTH, SCREEN_HEIGHT);
                this.role.draw(g, this.iViewMapX, this.iViewMapY, 0, 0);
                if (!this.role.enemy.isInvisible) {
                    this.role.enemy.draw(g, this.iViewMapX, this.iViewMapY, 0, 0);
                }
            } else {
                isNormal = true;
            }
        } else {
            isNormal = true;
        }
        if (isNormal) {
            this.map.drawMapBackground(g);
            drawMapBackground(g);
            drawSprite(g, 0);
            this.map.drawLayer0(g, this.iVibrateX, this.iVibrateY);
            drawSprite(g, 1);
            this.map.drawLayer1(g, this.iVibrateX, this.iVibrateY);
            drawSprite(g, 2);
            this.map.drawLayer2(g, this.iVibrateX, this.iVibrateY);
            drawSprite(g, 3);
        }
        if (!this.isHideUI) {
            drawComboTip(g);
            drawSceneUI(g);
        }
        if (this.bTurtorialState > 0) {
            drawKeyPressTip(g);
        }
        if (this.bCriticalDuration > 0) {
            criticalEffectAS.drawFrameCycle(g, 0, this.bCriticalCounter + STATE_CG, SCREEN_WIDTH >> 1, SCREEN_HEIGHT >> 1);
        }
        if (this.scrollInfoState > 0) {
            drawScrollInfo(g);
        }
        drawRemainingTime(g, 9, 55);
        drawDropInfo(g);
        drawStageInfo(g);
    }

    void loadMapBackgroundRes(String fileName) {
        if (this.backgroundImg != null) {
            this.backgroundImg.recycle();
            this.backgroundImg = null;
        }
        this.backgroundImg = Tool.loadPImage(String.valueOf(IMG_PATH) + fileName + ".p");
    }

    private void drawMapBackground(Graphics g) {
        if (this.backgroundImg != null) {
            Tool.drawImage(g, this.backgroundImg, 0, 0, 0);
            int w = this.backgroundImg.getWidth();
            if (SCREEN_WIDTH > w) {
                Tool.drawImage(g, this.backgroundImg, w, 0, 0);
            }
        }
    }

    private void drawSceneUI(Graphics g) {
        int x;
        if (this.isIntroduct) {
            g.setColor(0);
            g.fillRect(0, 0, SCREEN_WIDTH, SCREEN_HEIGHT);
        }
        this.sceneUIAS.drawFrameCycle(g, 0, 0, (SCREEN_WIDTH >> 1) - this.OFF_X, SCREEN_HEIGHT >> 1);
        int width = (this.roleHPImg.getWidth() * this.role.iHP) / this.role.iFinalHP;
        Tool.drawClipImage(g, this.roleHPImg, 53, 11, 0, 0, width, this.roleHPImg.getHeight());
        if (this.iHPDecrement > 0) {
            int length = ((this.roleHPImg.getWidth() * (this.role.iHP + this.iHPDecrement)) / this.role.iFinalHP) - width;
            g.setColor(-1);
            g.fillRect(width + 48 + 5, 11, length, this.roleHPImg.getHeight());
        }
        int roleMP = this.role.equip.iCurDurability;
        int roleMaxMP = this.role.equip.iFinalDurability;
        if (roleMP == 0) {
            if (this.isDurabilityToZero) {
                this.sceneUIAS.drawFrameCycle(g, 2, this.bEffectCounter + STATE_CG, (SCREEN_WIDTH >> 1) - this.OFF_X, SCREEN_HEIGHT >> 1);
            } else {
                this.sceneUIAS.drawFrameCycle(g, 4, 0, (SCREEN_WIDTH >> 1) - this.OFF_X, SCREEN_HEIGHT >> 1);
            }
        } else {
            this.sceneUIAS.drawFrameCycle(g, 1, 0, (SCREEN_WIDTH >> 1) - this.OFF_X, SCREEN_HEIGHT >> 1);
            Tool.drawClipImage(g, this.roleMPImg, 55, 25, 0, 0, (this.roleMPImg.getWidth() * roleMP) / roleMaxMP, this.roleMPImg.getHeight());
        }
        this.sceneUIAS.drawFrameCycle(g, 5, 0, (SCREEN_WIDTH >> 1) - this.OFF_X, SCREEN_HEIGHT >> 1);
        Tool.drawClipImage(g, this.headImgs[0], 4, -2, 0, 0, this.headImgs[0].getWidth(), this.headImgs[0].getHeight());
        short s = this.role.sEP;
        this.role.getClass();
        if (s < 2000) {
            Bitmap bitmap = this.spImg;
            int height = this.spImg.getHeight();
            this.role.getClass();
            int i = height * (2000 - this.role.sEP);
            this.role.getClass();
            int i2 = (i / 2000) + 34;
            int height2 = this.spImg.getHeight();
            this.role.getClass();
            int i3 = height2 * (2000 - this.role.sEP);
            this.role.getClass();
            Tool.drawClipImage(g, bitmap, 39, i2, 0, i3 / 2000, this.spImg.getWidth(), this.spImg.getHeight());
        } else if (this.role.isFillEP) {
            this.sceneUIAS.drawFrameCycle(g, 7, this.role.bEPCounter + STATE_CG, (SCREEN_WIDTH >> 1) - this.OFF_X, SCREEN_HEIGHT >> 1);
        } else {
            this.sceneUIAS.drawFrameCycle(g, 8, Tool.countTimes, (SCREEN_WIDTH >> 1) - this.OFF_X, SCREEN_HEIGHT >> 1);
        }
        Tool.drawClipImage(g, this.roleExpImg0, FIRE_KEY_HEIGHT, 38, 0, 0, (this.roleExpImg0.getWidth() * this.role.iCurrentExp) / this.role.iLevelUpExp, this.roleExpImg0.getHeight());
        Tool.drawNumReturnWidth(g, this.numImg[2], this.role.sLevel, 179, 37, 3, 1, -2);
        if (this.isDrawBossHpBar) {
            this.sceneUIAS.drawFrame(g, 8, (SCREEN_WIDTH >> 1) - this.OFF_X, SCREEN_HEIGHT >> 1);
            Tool.drawClipImage(g, this.bossHPImg, 58, 54, 0, 0, (this.bossHPImg.getWidth() * this.iBossHP) / this.iBossMaxHP, this.bossHPImg.getHeight());
        }
        int offX = this.OFF_X + 155;
        if (SCREEN_WIDTH == 533) {
            x = (SCREEN_WIDTH >> 1) - this.OFF_X;
        } else {
            x = SCREEN_WIDTH >> 1;
        }
        this.sceneUIAS.drawFrame(g, 1, (SCREEN_WIDTH >> 1) - this.OFF_X, (SCREEN_HEIGHT >> 1) + this.bSceneUIOffY);
        if (this.role.famulus != null) {
            this.sceneUIAS.drawFrameCycle(g, 11, 0, x, this.bSceneUIOffY + (SCREEN_HEIGHT >> 1));
            if ((this.role.famulus.iCurDurability * 100) / this.role.famulus.iFinalDurability >= 10) {
                this.sceneUIAS.drawFrameCycle(g, 13, Tool.countTimes, x, this.bSceneUIOffY + (SCREEN_HEIGHT >> 1));
            }
            Tool.drawClipImage(g, this.famulusDurabilityImg, offX + 48, ((SCREEN_HEIGHT - 14) + this.bSceneUIOffY) - 3, 0, 0, (this.famulusDurabilityImg.getWidth() * this.role.famulus.iCurDurability) / this.role.famulus.iFinalDurability, this.famulusDurabilityImg.getHeight());
            Tool.drawClipImage(g, this.headImgs[1], offX - 7, (this.bSceneUIOffY + 276) - 3, 0, 0, this.headImgs[1].getWidth(), this.headImgs[1].getHeight());
        }
        Skill skill = null;
        if (this.role.isIntegrate) {
            if (this.role.famulus.skill != null) {
                skill = this.role.famulus.skill;
            }
        } else if (this.role.equip.skill != null) {
            skill = this.role.equip.skill;
        }
        if (skill != null) {
            int iconIndex = skill.data.bID - 5;
            if (iconIndex == 6) {
                iconIndex = 7;
            } else if (iconIndex == 7) {
                iconIndex = 6;
            }
            int yy = (SCREEN_HEIGHT - 38) - 3;
            if (this.role.sCDCount[0] > 0) {
                Tool.drawClipImage(g, this.skillIconAndroid, 330, yy, iconIndex * 38, 0, 38, 38);
                if (this.role.sCDCount[0] < this.role.sCycleTime[0]) {
                    if (this.role.sCDCount[0] < this.role.sCycleTime[0] - 2) {
                        g.setColor(0);
                        g.fillRect(330, ((this.role.sCDCount[0] * 38) / this.role.sCycleTime[0]) + yy, 38, 38 - ((this.role.sCDCount[0] * 38) / this.role.sCycleTime[0]));
                    } else {
                        g.setColor(-1);
                        g.fillRect(330, yy, 38, 38);
                    }
                }
            }
        }
        if (this.isIntroduct) {
            this.sceneUIAS.drawFrame(g, 23, SCREEN_WIDTH >> 1, SCREEN_HEIGHT >> 1);
        }
    }

    private void cycleScene() {
        if (this.isPause) {
            this.isPause = false;
            return;
        }
        if (this.bGameState != 17) {
            getScriptState();
            if (this.isHideShortCutBar) {
                if (this.bSceneUIOffY < 37) {
                    this.bSceneUIOffY = (byte) (this.bSceneUIOffY + 10);
                } else {
                    this.bSceneUIOffY = (byte) 40;
                }
            } else if (this.bSceneUIOffY > 0) {
                this.bSceneUIOffY = (byte) (this.bSceneUIOffY - 8);
            } else {
                this.bSceneUIOffY = (byte) 0;
            }
        }
        cycleSpriteInStage();
        cycleRole();
        checkRoleState();
        cycleTimeCounter();
    }

    public void checkRoleState() {
        if (!this.role.isDead() && this.role.famulus != null && !this.role.isIntegrate && !this.role.famulus.isLocked && this.role.famulus.bActState == 0) {
            autoUseMedicament();
        }
    }

    private void script_showStageName() {
        this.iStageNameWidth = font.stringWidth(this.stageName);
        this.stageNameX = (SCREEN_WIDTH - this.iStageNameWidth) >> 1;
        this.stageNameY = (SCREEN_HEIGHT >> 1) - 50;
        this.iLineX1 = (-10) - ((SCREEN_WIDTH + this.iStageNameWidth) >> 1);
        this.iLineX2 = this.iLineX1 + this.iStageNameWidth + 20;
        this.iStageNameCounter = 0;
        this.iStageNameOffY = 0;
        this.isShowStageInfo = true;
        debugStr = new String[10];
    }

    private void drawStageInfo(Graphics g) {
        if (this.isShowStageInfo) {
            g.setColor(-1);
            g.drawLine(this.iLineX1, this.stageNameY, this.iLineX1 + this.iStageNameWidth + 20, this.stageNameY);
            g.drawLine(this.iLineX2, this.stageNameY, (this.iLineX2 + this.stageNameX) - 10, this.stageNameY);
            Tool.drawString(g, this.stageName, (SCREEN_WIDTH - font.stringWidth(this.stageName)) >> 1, this.stageNameY - this.iStageNameOffY, 0, -1);
        }
    }

    void cycleStageInfo() {
        if (this.isShowStageInfo) {
            if (this.iLineX1 < this.stageNameX - 10) {
                this.iLineX1 += DROP_POS_INTERVAL;
            } else {
                this.iLineX1 = this.stageNameX - 10;
            }
            if (this.iLineX2 < SCREEN_WIDTH) {
                this.iLineX2 += DROP_POS_INTERVAL;
                return;
            }
            this.iLineX2 = SCREEN_WIDTH;
            if (this.iStageNameOffY < FontHeight) {
                this.iStageNameOffY++;
            } else {
                if (this.iStageNameCounter < 24) {
                    this.iStageNameCounter++;
                    return;
                }
                this.iStageNameCounter = 0;
                this.iStageNameOffY = 0;
                this.isShowStageInfo = false;
            }
        }
    }

    private void drawComboTip(Graphics g) {
        if (this.role.iComboNum > 1) {
            if (this.role.isComboNumChange) {
                Tool.drawImage(g, this.comboImg[this.role.bComboChangeEffectCounter], SCREEN_WIDTH - this.comboImg[0].getWidth(), 35, 0);
                Tool.drawNumReturnWidth(g, this.numImg[this.role.bComboChangeEffectCounter + 3], this.role.iComboNum, SCREEN_WIDTH - this.comboImg[0].getWidth(), (this.comboImg[0].getHeight() + 31) - this.numImg[3].getHeight(), 3, 1, -1);
            } else {
                Tool.drawImage(g, this.comboImg[0], SCREEN_WIDTH - this.comboImg[0].getWidth(), 35, 0);
                Tool.drawNumReturnWidth(g, this.numImg[3], this.role.iComboNum, SCREEN_WIDTH - this.comboImg[0].getWidth(), (this.comboImg[0].getHeight() + 31) - this.numImg[3].getHeight(), 3, 1, -1);
            }
            int ap = this.role.iAP + this.role.equip.iFinalAP;
            if (this.role.isIntegrate) {
                ap = this.role.iAP + this.role.famulus.iAP;
            }
            if (this.role.iFinalAP > ap) {
                Tool.drawImage(g, this.comboTipImg, SCREEN_WIDTH - this.comboTipImg.getWidth(), this.comboImg[0].getHeight() + 35 + 2, 0);
                Tool.drawNumReturnWidth(g, this.comboNumImg, this.role.sComboUpgradeAPPercent + 100, SCREEN_WIDTH - this.comboTipImg.getWidth(), this.comboImg[0].getHeight() + 35 + 2, 3, 1, -1);
            }
        }
    }

    void setAreaIndex(int areaIndex) {
        this.bAreaIndex = (byte) areaIndex;
        this.isAreaOpened[this.bAreaIndex] = true;
    }

    public void loadGameMapRes() {
        if (this.mapImg == null) {
            this.mapImg = Tool.loadPImage(String.valueOf(IMG_PATH) + "m_87.p");
            String[] imgs = new String[6];
            for (int i = 0; i < imgs.length; i++) {
                imgs[i] = "m_8" + (i + 1);
            }
            this.mapAS = Common.getActionSet("z_worldmap", imgs, false, false);
        }
    }

    public void releaseGameMapRes() {
        if (this.mapImg != null) {
            this.mapImg.recycle();
            this.mapImg = null;
        }
        if (this.mapAS != null) {
            this.mapAS.releasImg();
            this.mapAS = null;
        }
    }

    public void initGameMap(int areaIndex) {
        Interpreter.setValue(SCRIPT_VARIABLES[1], new Int(1));
        this.iEventIndex = Interpreter.getValue(SCRIPT_VARIABLES[0]).getVal();
        this.bAreaIndex = (byte) areaIndex;
        this.isAreaOpened[this.bAreaIndex] = true;
        loadGameMapRes();
        this.bGameState = STATE_MAP;
        this.isLoadFinish = true;
        this.message = null;
    }

    private void drawGameMap(Graphics g) {
        g.drawImage(this.mapImg, 0, 0, 0);
        this.mapAS.drawFrameCycle(g, this.bAreaIndex == 3 ? 5 : 4, Tool.countTimes, this.AREA_COORDINATE[3][0], this.AREA_COORDINATE[3][1]);
        this.mapAS.drawFrameCycle(g, this.bAreaIndex == 6 ? 7 : 6, Tool.countTimes, this.AREA_COORDINATE[6][0], this.AREA_COORDINATE[6][1]);
        this.mapAS.drawFrameCycle(g, this.bAreaIndex == 5 ? 3 : 2, Tool.countTimes, this.AREA_COORDINATE[5][0], this.AREA_COORDINATE[5][1]);
        this.mapAS.drawFrameCycle(g, this.bAreaIndex == 4 ? 1 : 0, Tool.countTimes, this.AREA_COORDINATE[4][0], this.AREA_COORDINATE[4][1]);
        this.mapAS.drawFrameCycle(g, (this.bAreaIndex == 7 || this.bAreaIndex == 12) ? 5 : 4, Tool.countTimes, this.AREA_COORDINATE[7][0], this.AREA_COORDINATE[7][1]);
        this.mapAS.drawFrameCycle(g, this.bAreaIndex == 8 ? 1 : 0, Tool.countTimes, this.AREA_COORDINATE[8][0], this.AREA_COORDINATE[8][1]);
        this.mapAS.drawFrameCycle(g, (this.bAreaIndex == 9 || this.bAreaIndex == 10) ? 3 : 2, Tool.countTimes, this.AREA_COORDINATE[9][0], this.AREA_COORDINATE[9][1]);
        this.mapAS.drawFrameCycle(g, this.bAreaIndex == 0 ? 7 : 6, Tool.countTimes, this.AREA_COORDINATE[0][0], this.AREA_COORDINATE[0][1]);
        this.mapAS.drawFrameCycle(g, (this.bAreaIndex == 1 || this.bAreaIndex == 11) ? 5 : 4, Tool.countTimes, this.AREA_COORDINATE[1][0], this.AREA_COORDINATE[1][1]);
        this.mapAS.drawFrameCycle(g, this.bAreaIndex == 2 ? 1 : 0, Tool.countTimes, this.AREA_COORDINATE[2][0], this.AREA_COORDINATE[2][1]);
        drawRoundRectBGByTile(g, this.uiBGImg, (SCREEN_WIDTH - 240) >> 1, SCREEN_HEIGHT - 35, this.uiBGImg.getHeight(), this.uiBGImg.getHeight(), 240, 32);
        Tool.drawString(g, this.AREA_NAME[this.bAreaIndex], (((SCREEN_WIDTH - 80) - font.stringWidth(this.AREA_NAME[this.bAreaIndex])) >> 1) + 40, ((22 - FontHeight) >> 1) + (SCREEN_HEIGHT - 28), 0, -1);
    }

    private void handleKey_map() {
        if (Common.isUpPressed(true)) {
            switch (this.bAreaIndex) {
                case 0:
                    this.bAreaIndex = (byte) 9;
                    break;
                case 1:
                case 11:
                    this.bAreaIndex = (byte) 3;
                    break;
                case 2:
                    this.bAreaIndex = (byte) 1;
                    if (this.iEventIndex >= 1050) {
                        this.bAreaIndex = (byte) 11;
                    }
                    break;
                case 3:
                    this.bAreaIndex = (byte) 8;
                    break;
                case 4:
                    this.bAreaIndex = (byte) 5;
                    break;
                case 5:
                    this.bAreaIndex = (byte) 6;
                    break;
            }
            return;
        }
        if (Common.isDownPressed(true)) {
            switch (this.bAreaIndex) {
                case 1:
                case 11:
                    this.bAreaIndex = (byte) 2;
                    break;
                case 3:
                    this.bAreaIndex = (byte) 1;
                    if (this.iEventIndex >= 1050) {
                        this.bAreaIndex = (byte) 11;
                    }
                    break;
                case 5:
                    this.bAreaIndex = (byte) 4;
                    break;
                case 6:
                    this.bAreaIndex = (byte) 5;
                    break;
                case 8:
                    this.bAreaIndex = (byte) 3;
                    break;
                case Interpreter.ERR_ARR_OUT /* 9 */:
                    this.bAreaIndex = (byte) 0;
                    break;
            }
            return;
        }
        if (Common.isLeftPressed(true)) {
            switch (this.bAreaIndex) {
                case 1:
                case 11:
                    this.bAreaIndex = (byte) 9;
                    break;
                case 2:
                    this.bAreaIndex = (byte) 0;
                    break;
                case 4:
                    this.bAreaIndex = (byte) 1;
                    if (this.iEventIndex >= 1050) {
                        this.bAreaIndex = (byte) 11;
                    }
                    break;
                case 5:
                    this.bAreaIndex = (byte) 3;
                    break;
                case 6:
                    this.bAreaIndex = (byte) 8;
                    break;
                case 8:
                    this.bAreaIndex = (byte) 7;
                    if (this.iEventIndex > 1071) {
                        this.bAreaIndex = (byte) 12;
                    }
                    break;
                case 10:
                    this.bAreaIndex = (byte) 2;
                    break;
            }
            return;
        }
        if (Common.isRightPressed(true)) {
            switch (this.bAreaIndex) {
                case 0:
                    this.bAreaIndex = (byte) 2;
                    break;
                case 1:
                case 11:
                    this.bAreaIndex = (byte) 4;
                    break;
                case 3:
                    this.bAreaIndex = (byte) 5;
                    break;
                case 7:
                case 12:
                    this.bAreaIndex = (byte) 8;
                    break;
                case 8:
                    this.bAreaIndex = (byte) 6;
                    break;
                case Interpreter.ERR_ARR_OUT /* 9 */:
                    this.bAreaIndex = (byte) 1;
                    if (this.iEventIndex >= 1050) {
                        this.bAreaIndex = (byte) 11;
                    }
                    break;
            }
            return;
        }
        if (Common.isOKPressed(true) || Common.isKeyPressed(9, true)) {
            if (this.isAreaOpened[this.bAreaIndex]) {
                switch (this.bAreaIndex) {
                    case 0:
                        script_call("101.s", true);
                        break;
                    case 1:
                        script_call("334.s", true);
                        break;
                    case 2:
                        script_call("400.s", true);
                        break;
                    case 3:
                        script_call("500.s", true);
                        break;
                    case 4:
                        script_call("600.s", true);
                        break;
                    case 5:
                        script_call("700.s", true);
                        break;
                    case 6:
                        script_call("800.s", true);
                        break;
                    case 7:
                        script_call("900.s", true);
                        break;
                    case 8:
                        script_call("1000.s", true);
                        break;
                    case Interpreter.ERR_ARR_OUT /* 9 */:
                        script_call("1100.s", true);
                        break;
                    case 10:
                        script_call("200.s", true);
                        break;
                    case 11:
                        script_call("300.s", true);
                        break;
                    case 12:
                        script_call("900.s", true);
                        break;
                }
                return;
            }
            script_showMessage("该区域尚未开放!", 0);
        }
    }

    /* JADX WARN: Code duplicated, block: B:12:0x003d  */
    /* JADX WARN: Code duplicated, block: B:31:0x00f7 A[LOOP:1: B:14:0x005b->B:31:0x00f7, LOOP_END] */
    /* JADX WARN: Code duplicated, block: B:9:0x002a  */
    void loadMiniMap() {
        int i;
        switch (this.bAreaIndex) {
            case 0:
            case 1:
                this.bMiniMapXCount = (byte) 2;
                this.bMiniMapYCount = (byte) 1;
                this.iMiniMapWidth = 98;
                this.iMiniMapHeight = DROP_POS_INTERVAL;
                break;
            case 2:
            case 3:
            case 4:
            case 5:
                this.bMiniMapXCount = (byte) 5;
                this.bMiniMapYCount = (byte) 3;
                this.iMiniMapWidth = 257;
                this.iMiniMapHeight = 106;
                break;
            case 6:
                this.bMiniMapXCount = (byte) 4;
                this.bMiniMapYCount = (byte) 6;
                this.iMiniMapWidth = 220;
                this.iMiniMapHeight = 204;
                break;
            case 7:
                this.bMiniMapXCount = (byte) 1;
                this.bMiniMapYCount = (byte) 1;
                this.iMiniMapWidth = 45;
                this.iMiniMapHeight = DROP_POS_INTERVAL;
                break;
            case 8:
                this.bMiniMapXCount = (byte) 5;
                this.bMiniMapYCount = (byte) 4;
                this.iMiniMapWidth = 257;
                this.iMiniMapHeight = 144;
                break;
            case Interpreter.ERR_ARR_OUT /* 9 */:
                this.bMiniMapXCount = (byte) 7;
                this.bMiniMapYCount = (byte) 7;
                this.iMiniMapWidth = 363;
                this.iMiniMapHeight = 258;
                break;
            case 10:
                this.bMiniMapXCount = (byte) 5;
                this.bMiniMapYCount = (byte) 1;
                this.iMiniMapWidth = 257;
                this.iMiniMapHeight = DROP_POS_INTERVAL;
                break;
            case 11:
                this.bMiniMapXCount = (byte) 7;
                this.bMiniMapYCount = (byte) 6;
                this.iMiniMapWidth = 363;
                this.iMiniMapHeight = 220;
                break;
            case 12:
                this.bMiniMapXCount = (byte) 5;
                this.bMiniMapYCount = (byte) 5;
                this.iMiniMapWidth = 257;
                this.iMiniMapHeight = 182;
                break;
        }
        this.iMiniMapX = ((SCREEN_WIDTH - this.iMiniMapWidth) >> 1) - this.distance_x;
        int temp = 0;
        for (int i2 = 0; i2 < this.bStageExit[this.bAreaIndex].length; i2++) {
            if (this.stageData[this.bAreaIndex][i2][0] == this.iCurrentStageID) {
                temp = this.stageData[this.bAreaIndex][i2][1];
                if (this.iMiniMapX + (temp * 53) < 0) {
                    this.iMiniMapX = (-temp) * 53;
                }
                if (this.iMiniMapX + this.distance_x + (temp * 53) > SCREEN_WIDTH - 45) {
                    this.iMiniMapX = ((SCREEN_WIDTH - 45) - this.distance_x) - (temp * 53);
                }
                this.iMiniMapY = ((SCREEN_HEIGHT - this.iMiniMapHeight) >> 1) - this.distance_y;
                this.miniMapSignImg = new Bitmap[3];
                for (i = 0; i < this.miniMapSignImg.length; i++) {
                    this.miniMapSignImg[i] = Tool.loadPImage(String.valueOf(IMG_PATH) + "mini_" + i + ".p");
                }
                this.bGameState = STATE_MINI_MAP;
            }
        }
        if (this.iMiniMapX + (temp * 53) < 0) {
            this.iMiniMapX = (-temp) * 53;
        }
        if (this.iMiniMapX + this.distance_x + (temp * 53) > SCREEN_WIDTH - 45) {
            this.iMiniMapX = ((SCREEN_WIDTH - 45) - this.distance_x) - (temp * 53);
        }
        this.iMiniMapY = ((SCREEN_HEIGHT - this.iMiniMapHeight) >> 1) - this.distance_y;
        this.miniMapSignImg = new Bitmap[3];
        while (i < this.miniMapSignImg.length) {
            this.miniMapSignImg[i] = Tool.loadPImage(String.valueOf(IMG_PATH) + "mini_" + i + ".p");
        }
        this.bGameState = STATE_MINI_MAP;
    }

    void releaseMiniMap() {
        if (this.miniMapSignImg != null) {
            for (int i = 0; i < this.miniMapSignImg.length; i++) {
                if (this.miniMapSignImg[i] != null) {
                    this.miniMapSignImg[i].recycle();
                    this.miniMapSignImg[i] = null;
                }
            }
        }
        this.bGameState = (byte) 12;
    }

    private void drawMiniMap(Graphics g) {
        g.setColor(0);
        g.fillRect(0, 0, SCREEN_WIDTH, SCREEN_HEIGHT);
        for (int i = 0; i < this.bStageExit[this.bAreaIndex].length; i++) {
            int x = this.iMiniMapX + this.distance_x + (this.stageData[this.bAreaIndex][i][1] * 53);
            int y = this.iMiniMapY + this.distance_y + (this.stageData[this.bAreaIndex][i][2] * 38);
            if (this.bStageExit[this.bAreaIndex][i][0] > 1 || this.bStageExit[this.bAreaIndex][i][1] > 1) {
                x = this.iMiniMapX + this.distance_x + (this.stageData[this.bAreaIndex][i][1] * 53) + 26;
            }
            g.setColor(-1);
            g.drawRect(x, y, 45, DROP_POS_INTERVAL);
            if (this.stageData[this.bAreaIndex][i][0] == this.iCurrentStageID) {
                if (this.iMissionStageIndex == this.iCurrentStageID) {
                    Tool.drawImage(g, this.miniMapSignImg[0], x + 5, y + 7, 0);
                    Tool.drawImage(g, this.miniMapSignImg[1], (x + 45) - 20, y + 7, 0);
                } else if (this.bSaveStage[this.bAreaIndex][i] == 1) {
                    Tool.drawImage(g, this.miniMapSignImg[0], x + 5, y + 7, 0);
                    Tool.drawImage(g, this.miniMapSignImg[2], (x + 45) - 20, y + 7, 0);
                } else {
                    Tool.drawImage(g, this.miniMapSignImg[0], x + 14, y + 7, 0);
                }
            } else if (this.iMissionStageIndex == this.stageData[this.bAreaIndex][i][0]) {
                Tool.drawImage(g, this.miniMapSignImg[1], x + 14, y + 7, 0);
            } else if (this.bSaveStage[this.bAreaIndex][i] == 1) {
                Tool.drawImage(g, this.miniMapSignImg[2], x + 14, y + 7, 0);
            }
            if (this.bStageExit[this.bAreaIndex][i][0] > 0) {
                if (this.bStageExit[this.bAreaIndex][i][0] > 1) {
                    g.setColor(16776960);
                    g.fillRect(x + 5, y - 4, 4, 4);
                    g.fillRect(x + 35, y - 4, 4, 4);
                } else {
                    int temp = 0;
                    for (int j = 0; j < this.bStageExit[this.bAreaIndex].length; j++) {
                        if (this.bStageExit[this.bAreaIndex][j][1] > 1 && this.stageData[this.bAreaIndex][i][2] - 1 == this.stageData[this.bAreaIndex][j][2]) {
                            if (this.stageData[this.bAreaIndex][i][1] == this.stageData[this.bAreaIndex][j][1]) {
                                temp = 1;
                            } else if (this.stageData[this.bAreaIndex][i][1] == this.stageData[this.bAreaIndex][j][1] + 1) {
                                temp = 2;
                            }
                        }
                    }
                    if (temp == 1) {
                        g.setColor(16776960);
                        g.fillRect(x + 31, y - 4, 4, 4);
                    } else if (temp == 2) {
                        g.setColor(16776960);
                        g.fillRect(x + 8, y - 4, 4, 4);
                    } else {
                        g.setColor(16776960);
                        g.fillRect(x + 20, y - 4, 4, 4);
                    }
                }
            } else if (this.bStageExit[this.bAreaIndex][i][0] == -1) {
                g.setColor(0);
                g.fillRect(x + 19, y - 2, 6, 4);
            }
            if (this.bStageExit[this.bAreaIndex][i][1] > 0) {
                if (this.bStageExit[this.bAreaIndex][i][1] > 1) {
                    g.setColor(16776960);
                    g.fillRect(x + 5, y + DROP_POS_INTERVAL, 4, 4);
                    g.fillRect(x + 35, y + DROP_POS_INTERVAL, 4, 4);
                } else {
                    int temp2 = 0;
                    for (int j2 = 0; j2 < this.bStageExit[this.bAreaIndex].length; j2++) {
                        if (this.bStageExit[this.bAreaIndex][j2][0] > 1 && this.stageData[this.bAreaIndex][i][2] + 1 == this.stageData[this.bAreaIndex][j2][2]) {
                            if (this.stageData[this.bAreaIndex][i][1] == this.stageData[this.bAreaIndex][j2][1]) {
                                temp2 = 1;
                            } else if (this.stageData[this.bAreaIndex][i][1] == this.stageData[this.bAreaIndex][j2][1] + 1) {
                                temp2 = 2;
                            }
                        }
                    }
                    if (temp2 == 1) {
                        g.setColor(16776960);
                        g.fillRect(x + 31, y + DROP_POS_INTERVAL, 4, 4);
                    } else if (temp2 == 2) {
                        g.setColor(16776960);
                        g.fillRect(x + 8, y + DROP_POS_INTERVAL, 4, 4);
                    } else {
                        g.setColor(16776960);
                        g.fillRect(x + 20, y + DROP_POS_INTERVAL, 4, 4);
                    }
                }
            } else if (this.bStageExit[this.bAreaIndex][i][1] == -1) {
                g.setColor(0);
                g.fillRect(x + 19, (y + DROP_POS_INTERVAL) - 2, 6, 4);
            }
            if (this.bStageExit[this.bAreaIndex][i][2] > 0) {
                boolean isLeftStageExist = false;
                for (int j3 = 0; j3 < this.bStageExit[this.bAreaIndex].length; j3++) {
                    if (this.stageData[this.bAreaIndex][i][2] == this.stageData[this.bAreaIndex][j3][2] && this.stageData[this.bAreaIndex][i][1] - 1 == this.stageData[this.bAreaIndex][j3][1]) {
                        isLeftStageExist = true;
                    }
                }
                if (!isLeftStageExist) {
                    g.setColor(16776960);
                    g.fillRect((x - 8) - 22, y + 13, DROP_POS_INTERVAL, 4);
                } else {
                    g.setColor(16776960);
                    g.fillRect(x - 4, y + 13, 4, 4);
                }
            } else if (this.bStageExit[this.bAreaIndex][i][2] == -1) {
                g.setColor(0);
                g.fillRect(x - 2, y + 12, 4, 6);
            }
            if (this.bStageExit[this.bAreaIndex][i][3] > 0) {
                boolean isRightStageExist = false;
                for (int j4 = 0; j4 < this.bStageExit[this.bAreaIndex].length; j4++) {
                    if (this.stageData[this.bAreaIndex][i][2] == this.stageData[this.bAreaIndex][j4][2] && this.stageData[this.bAreaIndex][i][1] + 1 == this.stageData[this.bAreaIndex][j4][1]) {
                        isRightStageExist = true;
                    }
                }
                if (!isRightStageExist) {
                    g.setColor(16776960);
                    g.fillRect(x + 45, y + 13, 31, 4);
                } else {
                    g.setColor(16776960);
                    g.fillRect(x + 45, y + 13, 4, 4);
                }
            } else if (this.bStageExit[this.bAreaIndex][i][3] == -1) {
                g.setColor(0);
                g.fillRect((x + 45) - 2, y + 12, 4, 6);
            }
        }
        if (this.iMiniMapWidth > SCREEN_WIDTH) {
            if (this.iMiniMapX > (SCREEN_WIDTH - this.iMiniMapWidth) - this.distance_x) {
                Tool.drawRegion(g, this.arrowImg, 0, 0, this.arrowImg.getWidth(), this.arrowImg.getHeight(), 0, (Tool.countTimes2 % 4) + (SCREEN_WIDTH - 35), SCREEN_HEIGHT - 45);
            }
            if (this.iMiniMapX <= -15) {
                Tool.drawRegion(g, this.arrowImg, 0, 0, this.arrowImg.getWidth(), this.arrowImg.getHeight(), 2, 15 - (Tool.countTimes2 % 4), SCREEN_HEIGHT - 45);
            }
        }
        if (this.iMiniMapHeight > SCREEN_HEIGHT) {
            if (this.iMiniMapY > (SCREEN_HEIGHT - this.iMiniMapHeight) - this.distance_y) {
                Tool.drawRegion(g, this.arrowImg, 0, 0, this.arrowImg.getWidth(), this.arrowImg.getHeight(), 3, (Tool.countTimes2 % 4) + (SCREEN_WIDTH - 35), SCREEN_HEIGHT - 45);
            }
            if (this.iMiniMapY <= -15) {
                Tool.drawRegion(g, this.arrowImg, 0, 0, this.arrowImg.getWidth(), this.arrowImg.getHeight(), 1, 15 - (Tool.countTimes2 % 4), SCREEN_HEIGHT - 45);
            }
        }
    }

    void handleKey_MiniMap() {
        if (Common.isUpPressed(false)) {
            if (this.iMiniMapHeight > SCREEN_HEIGHT) {
                if (this.iMiniMapY <= -15) {
                    this.iMiniMapY += 15;
                } else {
                    this.iMiniMapY = 0;
                }
            }
        } else if (Common.isDownPressed(false)) {
            if (this.iMiniMapHeight > SCREEN_HEIGHT) {
                if (this.iMiniMapY + this.distance_y + this.iMiniMapHeight >= SCREEN_HEIGHT + 15) {
                    this.iMiniMapY -= 15;
                } else {
                    this.iMiniMapY = ((SCREEN_HEIGHT - (this.distance_y << 1)) - this.iMiniMapHeight) - 2;
                }
            }
        } else if (Common.isLeftPressed(false)) {
            if (this.iMiniMapWidth > SCREEN_WIDTH) {
                if (this.iMiniMapX <= -15) {
                    this.iMiniMapX += 15;
                } else {
                    this.iMiniMapX = 0;
                }
            }
        } else if (Common.isRightPressed(false) && this.iMiniMapWidth > SCREEN_WIDTH) {
            if (this.iMiniMapX + this.distance_x + this.iMiniMapWidth >= SCREEN_WIDTH + 15) {
                this.iMiniMapX -= 15;
            } else {
                this.iMiniMapX = (SCREEN_WIDTH - (this.distance_x << 1)) - this.iMiniMapWidth;
            }
        }
        if (Common.isKeyPressed(14, true)) {
            releaseMiniMap();
        }
    }

    void drawTip(Graphics g) {
        if (this.tipST != null) {
            if (this.tipST.getTotalLineNum() < 2) {
                drawRoundRectBGByTile(g, this.uiBGImg, 0, this.menuY - (FontHeight + 10), this.uiBGImg.getHeight(), this.uiBGImg.getHeight(), SCREEN_WIDTH, FontHeight + 10);
                this.tipST.draw(g, 10, (this.menuY - FontHeight) - 5);
            } else {
                drawRoundRectBGByTile(g, this.uiBGImg, 0, this.menuY - ((FontHeight << 1) + 10), this.uiBGImg.getHeight(), this.uiBGImg.getHeight(), SCREEN_WIDTH, (FontHeight << 1) + 10);
                this.tipST.draw(g, 10, (this.menuY - (FontHeight << 1)) - 5);
            }
        }
    }

    public void script_tip(String str) {
        this.tipST = new StyledText(str, SCREEN_WIDTH - 20, (FontHeight << 1) + 2, Font.fontChinaH / 10, Font.fontChinaH, font);
        this.tipST.setLineCenter();
    }

    public void script_clearTip() {
        this.tipST = null;
    }

    void drawKeyPressTip(Graphics g) {
        if (this.bKeyPressTipIndex == 0) {
            for (int i = 0; i < this.isTurtorialKey.length; i++) {
                if (this.isTurtorialKey[i]) {
                    this.keyPressTipAS.drawFrameCycle(g, i, Tool.countTimes, SCREEN_WIDTH >> 1, SCREEN_HEIGHT >> 1);
                }
            }
            return;
        }
        if (this.bKeyPressTipIndex == 1) {
            this.keyPressTipAS.drawFrameCycle(g, 4, Tool.countTimes, SCREEN_WIDTH >> 1, SCREEN_HEIGHT >> 1);
        }
    }

    void debug(Graphics g) {
        if (debugStr != null) {
            g.setColor(16711680);
            for (int i = 0; i < debugStr.length; i++) {
                if (debugStr[i] != null) {
                    g.drawString(debugStr[i], (SCREEN_WIDTH - font.stringWidth(debugStr[i])) >> 1, (FontHeight * i) + 20, 0);
                }
            }
        }
    }

    private void drawScrollInfo(Graphics g) {
        Tool.drawString(g, this.scrollInfo, (SCREEN_WIDTH - 90) - this.scrollInfoCount, ((this.scrollInfoBarH - Font.fontChinaH) >> 1) + 3, -1, 0);
    }

    private void drawDropInfo(Graphics g) {
        for (int i = 0; i < 3 && this.dropInfoCount[1][i] > 0; i++) {
            this.dropInfo[i].draw(g, this.dropInfoCount[0][i], ((SCREEN_HEIGHT - ((i + 1) * Font.fontChinaH)) - this.uiBGImg.getHeight()) - 14);
        }
    }

    public void drawSprite(Graphics g, int layerIndex) {
        int meY2;
        int meY3;
        if (layerIndex == 0) {
            for (int i = 0; i < this.spriteBelowLayer0.size(); i++) {
                Sprite me = (Sprite) this.spriteBelowLayer0.elementAt(i);
                if (me != null && !me.isInvisible) {
                    me.draw(g, this.iViewMapX, this.iViewMapY, this.iVibrateX, this.iVibrateY);
                }
            }
            return;
        }
        if (layerIndex == 1) {
            for (int i2 = 0; i2 < this.spriteOnLayer1.size(); i2++) {
                Sprite me1 = (Sprite) this.spriteOnLayer1.elementAt(i2);
                if (me1 != null && !me1.isInvisible && me1.bType == 11) {
                    ((Enemy) me1).drawSpecialEffectBack(g, this.iViewMapX, this.iViewMapY, this.iVibrateX, this.iVibrateY);
                }
            }
            for (int i3 = 0; i3 < this.spriteOnLayer0.size(); i3++) {
                Sprite me2 = (Sprite) this.spriteOnLayer0.elementAt(i3);
                if (me2 != null && !me2.isInvisible) {
                    int meY1 = me2.iMapY + me2.iFootHeight;
                    int minYIndex = i3;
                    if (i3 < this.spriteOnLayer0.size() - 1) {
                        for (int j = i3 + 1; j < this.spriteOnLayer0.size(); j++) {
                            Sprite me3 = (Sprite) this.spriteOnLayer0.elementAt(j);
                            if (me3 != null && !me3.isInvisible && (meY3 = me3.iMapY + me3.iFootHeight) < meY1) {
                                meY1 = meY3;
                                minYIndex = j;
                            }
                        }
                        if (minYIndex != i3) {
                            Object ob = this.spriteOnLayer0.elementAt(minYIndex);
                            this.spriteOnLayer0.setElementAt(ob, i3);
                            this.spriteOnLayer0.setElementAt(me2, minYIndex);
                        }
                    }
                    Sprite me4 = (Sprite) this.spriteOnLayer0.elementAt(i3);
                    me4.draw(g, this.iViewMapX, this.iViewMapY, this.iVibrateX, this.iVibrateY);
                }
            }
            return;
        }
        if (layerIndex == 2) {
            for (int i4 = 0; i4 < this.spriteOnLayer1.size(); i4++) {
                Sprite me5 = (Sprite) this.spriteOnLayer1.elementAt(i4);
                if (me5 != null && !me5.isInvisible && (me5.bType == 4 || me5.bType == 5)) {
                    me5.draw(g, this.iViewMapX, this.iViewMapY, this.iVibrateX, this.iVibrateY);
                }
            }
            for (int i5 = 0; i5 < this.spriteOnLayer1.size(); i5++) {
                Sprite me6 = (Sprite) this.spriteOnLayer1.elementAt(i5);
                if (me6 != null && !me6.isInvisible && me6.bType != 4 && me6.bType != 5) {
                    int meY4 = me6.iMapY + me6.iFootHeight;
                    if (me6.bType == 0) {
                        meY4 += 6;
                    }
                    int minYIndex2 = i5;
                    if (i5 < this.spriteOnLayer1.size() - 1) {
                        for (int j2 = i5 + 1; j2 < this.spriteOnLayer1.size(); j2++) {
                            Sprite me7 = (Sprite) this.spriteOnLayer1.elementAt(j2);
                            if (me7 != null && !me7.isInvisible && me7.bType != 3 && me7.bType != 12 && me7.bType != 4 && me7.bType != 5) {
                                int meY5 = me7.iMapY + me7.iFootHeight;
                                if (me7.bType == 0) {
                                    meY5 += 6;
                                }
                                if (meY5 < meY4) {
                                    meY4 = meY5;
                                    minYIndex2 = j2;
                                }
                            }
                        }
                        if (minYIndex2 != i5) {
                            Object ob2 = this.spriteOnLayer1.elementAt(minYIndex2);
                            this.spriteOnLayer1.setElementAt(ob2, i5);
                            this.spriteOnLayer1.setElementAt(me6, minYIndex2);
                        }
                    }
                    Sprite me8 = (Sprite) this.spriteOnLayer1.elementAt(i5);
                    me8.draw(g, this.iViewMapX, this.iViewMapY, this.iVibrateX, this.iVibrateY);
                }
            }
            return;
        }
        if (layerIndex == 3) {
            for (int i6 = 0; i6 < this.spriteOnLayer2.size(); i6++) {
                Sprite me9 = (Sprite) this.spriteOnLayer2.elementAt(i6);
                if (me9 != null && !me9.isInvisible) {
                    int meY6 = me9.iMapY + me9.iFootHeight;
                    int minYIndex3 = i6;
                    if (i6 < this.spriteOnLayer2.size() - 1) {
                        for (int j3 = i6 + 1; j3 < this.spriteOnLayer2.size(); j3++) {
                            Sprite me10 = (Sprite) this.spriteOnLayer2.elementAt(j3);
                            if (me10 != null && !me10.isInvisible && (meY2 = me10.iMapY + me10.iFootHeight) < meY6) {
                                meY6 = meY2;
                                minYIndex3 = j3;
                            }
                        }
                        if (minYIndex3 != i6) {
                            Object ob3 = this.spriteOnLayer2.elementAt(minYIndex3);
                            this.spriteOnLayer2.setElementAt(ob3, i6);
                            this.spriteOnLayer2.setElementAt(me9, minYIndex3);
                        }
                    }
                    Sprite me11 = (Sprite) this.spriteOnLayer2.elementAt(i6);
                    me11.draw(g, this.iViewMapX, this.iViewMapY, this.iVibrateX, this.iVibrateY);
                }
            }
        }
    }

    private void drawSpriteDuringSpecialSkill(Graphics g) {
        int elementInScreenNum = this.fightElement.size();
        for (int i = 0; i < elementInScreenNum; i++) {
            Sprite spr = (Sprite) this.fightElement.elementAt(i);
            spr.draw(g, this.iViewMapX, this.iViewMapY, this.iVibrateX, this.iVibrateY);
        }
    }

    public void drawRoundRectBGByTile(Graphics g, Bitmap img, int x, int y, int tileW, int tileH, int w, int h) {
        boolean isVerticalNeedToFill = false;
        boolean isHorizontalNeedToFill = false;
        int m = (w - (tileW << 1)) / tileW;
        int n = (h - (tileH << 1)) / tileH;
        if (w < (tileW << 1)) {
            m = 0;
        } else if (m * tileW < w - (tileW << 1)) {
            isVerticalNeedToFill = true;
        }
        if (h < (tileH << 1)) {
            n = 0;
        } else if (n * tileH < h - (tileH << 1)) {
            isHorizontalNeedToFill = true;
        }
        g.setColor(-2716582);
        g.fillRect(x + 2, y + 2, w - 8, h - 12);
        if (isVerticalNeedToFill) {
            Tool.drawRegion(g, img, tileW << 2, 0, (w - (tileW << 1)) - (m * tileW), tileH, 0, x + ((m + 1) * tileW), y);
            Tool.drawRegion(g, img, tileW * 5, tileH - ((h >> 1) > tileH ? tileH : h >> 1), (w - (tileW << 1)) - (m * tileW), (h >> 1) > tileH ? tileH : h >> 1, 0, x + ((m + 1) * tileW), (h >> 1) > tileH ? (y + h) - tileH : y + (h >> 1));
        }
        if (isHorizontalNeedToFill) {
            Tool.drawRegion(g, img, tileW * 7, 0, tileW, (h - (tileH << 1)) - (n * tileH), 0, x, y + ((n + 1) * tileH));
            Tool.drawRegion(g, img, tileW * 6, 0, tileW, (h - (tileH << 1)) - (n * tileH), 0, (x + w) - tileW, y + ((n + 1) * tileH));
        }
        for (int i = 0; i < m; i++) {
            Tool.drawRegion(g, img, tileW << 2, 0, tileW, (h >> 1) > tileH ? tileH : h >> 1, 0, x + ((i + 1) * tileW), y);
            Tool.drawRegion(g, img, tileW * 5, tileH - ((h >> 1) > tileH ? tileH : h >> 1), tileW, (h >> 1) > tileH ? tileH : h >> 1, 0, x + ((i + 1) * tileW), (h >> 1) > tileH ? (y + h) - tileH : y + (h >> 1));
        }
        for (int i2 = 0; i2 < n; i2++) {
            Tool.drawRegion(g, img, tileW * 7, 0, (w >> 1) > tileW ? tileW : w >> 1, tileH, 0, x, y + ((i2 + 1) * tileH));
            Tool.drawRegion(g, img, tileW * 6, 0, (w >> 1) > tileW ? tileW : w >> 1, tileH, 0, (w >> 1) > tileW ? (x + w) - tileW : x + (w >> 1), y + ((i2 + 1) * tileW));
        }
        Tool.drawRegion(g, img, 0, 0, (w >> 1) > tileW ? tileW : w >> 1, (h >> 1) > tileH ? tileH : h >> 1, 0, x, y);
        Tool.drawRegion(g, img, tileW, 0, (w >> 1) > tileW ? tileW : w >> 1, (h >> 1) > tileH ? tileH : h >> 1, 0, (w >> 1) > tileW ? (x + w) - tileW : x + (w >> 1), y);
        Tool.drawRegion(g, img, tileW << 1, tileH - ((h >> 1) > tileH ? tileH : h >> 1), (w >> 1) > tileW ? tileW : w >> 1, (h >> 1) > tileH ? tileH : h >> 1, 0, x, (h >> 1) > tileH ? (y + h) - tileH : y + (h >> 1));
        Tool.drawRegion(g, img, tileW * 3, tileH - ((h >> 1) > tileH ? tileH : h >> 1), (w >> 1) > tileW ? tileW : w >> 1, (h >> 1) > tileH ? tileH : h >> 1, 0, (w >> 1) > tileW ? (x + w) - tileW : x + (w >> 1), (h >> 1) > tileH ? (y + h) - tileH : y + (h >> 1));
    }

    void createGameMenu() {
        loadGameMenuRes();
        this.bGameState = (byte) 18;
        this.bMenuIndex = (byte) 0;
        this.isFeedOver = false;
        if (this.role.famulus != null) {
            this.role.famulus.initAct();
        }
        timePause();
    }

    void closeGameMenu() {
        releaseGameMenuRes();
        this.bGameState = (byte) 12;
        this.bSystemIndex = (byte) 0;
        this.isPopUpOption = false;
        this.UIST = null;
        timeContinue();
    }

    void loadGameMenuRes() {
        if (this.equipIconImg == null) {
            this.equipIconImg = Tool.loadPImage("/pic/weapon.p");
            this.roleHPImg1 = Tool.loadPImage(String.valueOf(IMG_PATH) + "u_24.p");
            this.roleMPImg1 = Tool.loadPImage(String.valueOf(IMG_PATH) + "u_15.p");
            this.roleExpImg1 = Tool.loadPImage(String.valueOf(IMG_PATH) + "u_10.p");
            this.famulusExpImg = Tool.loadPImage(String.valueOf(IMG_PATH) + "u_09.p");
            this.headImg = Tool.loadPImage(String.valueOf(IMG_PATH) + "u_39.p");
        }
        loadUIAction();
    }

    void releaseGameMenuRes() {
        cleanUpUIAction();
    }

    private void loadUIAction() {
        if (this.menuAS == null) {
            String[] imgs = {"u_04", "u_02", "u_06", "u_11", "s_34", "f_07", "u_00", "u_46"};
            this.menuAS = Common.getActionSet("u_01", imgs, true, false);
        }
    }

    void cleanUpUIAction() {
    }

    private void drawGameMenu(Graphics g) {
        switch (this.bMenuIndex) {
            case 0:
                this.menuAS.drawFrame(g, 1, SCREEN_WIDTH >> 1, SCREEN_HEIGHT >> 1);
                showRoleSkill(g);
                if (!this.isSkillPreview) {
                    this.menuAS.drawFrame(g, 0, SCREEN_WIDTH >> 1, SCREEN_HEIGHT >> 1);
                    Tool.drawClipImage(g, this.headImg, this.OFF_X + 117, 78, 0, 0, this.headImg.getWidth(), this.headImg.getHeight());
                    int xx = this.OFF_X + 226;
                    Tool.drawClipImage(g, this.roleHPImg1, xx, 86, 0, 0, (this.roleHPImg1.getWidth() * this.role.iHP) / this.role.iFinalHP, this.roleHPImg1.getHeight());
                    int legnth = Tool.drawNumReturnWidth(g, this.numImg[6], this.role.iFinalHP, xx + 75, 88, 4, 1, -1);
                    int signWidth = Tool.drawSignReturnWidth(g, signImg, (xx + 75) - legnth, 88, 1, 0, 1);
                    Tool.drawNumReturnWidth(g, this.numImg[6], this.role.iHP, ((xx + 75) - legnth) - signWidth, 88, 4, 1, -1);
                    int yy = 86 + 20;
                    int durability = this.role.isIntegrate ? this.role.famulus.iCurDurability : this.role.equip.iCurDurability;
                    int maxDurability = this.role.isIntegrate ? this.role.famulus.iFinalDurability : this.role.equip.iFinalDurability;
                    Tool.drawClipImage(g, this.roleMPImg1, xx, yy, 0, 0, (this.roleMPImg1.getWidth() * durability) / maxDurability, this.roleMPImg1.getHeight());
                    int legnth2 = Tool.drawNumReturnWidth(g, this.numImg[6], maxDurability, xx + 75, 108, 4, 1, -1);
                    int signWidth2 = Tool.drawSignReturnWidth(g, signImg, (xx + 75) - legnth2, 108, 1, 0, 1);
                    Tool.drawNumReturnWidth(g, this.numImg[6], durability, ((xx + 75) - legnth2) - signWidth2, 108, 4, 1, -1);
                    Tool.drawClipImage(g, this.roleExpImg1, xx, yy + 22, 0, 0, (this.roleExpImg1.getWidth() * this.role.iCurrentExp) / this.role.iLevelUpExp, this.roleExpImg1.getHeight());
                    int legnth3 = Tool.drawNumReturnWidth(g, this.numImg[6], this.role.iLevelUpExp, xx + 75, 130, 4, 1, -1);
                    int signWidth3 = Tool.drawSignReturnWidth(g, signImg, (xx + 75) - legnth3, 130, 1, 0, 1);
                    Tool.drawNumReturnWidth(g, this.numImg[6], this.role.iCurrentExp, ((xx + 75) - legnth3) - signWidth3, 130, 4, 1, -1);
                    int ap = this.role.iAP + this.role.equip.iFinalAP + this.role.iAPUpByProp;
                    if (this.role.isIntegrate) {
                        ap = this.role.iAP + this.role.famulus.iFinalAP + this.role.iAPUpByProp;
                    }
                    int xx2 = this.OFF_X + 377;
                    Tool.drawNumReturnWidth(g, this.numImg[2], ap, xx2, 83, 4, 2, -2);
                    Tool.drawNumReturnWidth(g, this.numImg[2], this.role.iFinalHitPoint, xx2, 107, 4, 2, -2);
                    Tool.drawNumReturnWidth(g, this.numImg[2], this.role.iFinalDodgePoint, xx2, 131, 4, 2, -2);
                    Tool.drawNumReturnWidth(g, this.numImg[2], this.role.iFinalDP, xx2, 155, 4, 2, -2);
                    Tool.drawNumReturnWidth(g, this.numImg[2], this.role.iFinalCriticalPoint, xx2, 179, 4, 2, -2);
                    int xx3 = this.OFF_X + 315;
                    if (this.role.isIntegrate) {
                        if (this.role.famulus.skill != null) {
                            int iconIndex = this.role.famulus.skill.data.bID - 5;
                            if (iconIndex == 6) {
                                iconIndex = 7;
                            } else if (iconIndex == 7) {
                                iconIndex = 6;
                            }
                            Tool.drawClipImage(g, this.skillIconImg, xx3, 214, iconIndex * 22, 0, 22, 22);
                            Tool.drawString(g, this.role.famulus.skill.name, xx3 + 24 + ((65 - font.stringWidth(this.role.famulus.skill.name)) >> 1), 214 + ((18 - FontHeight) >> 1), 0, -1);
                        }
                    } else if (this.role.equip.skill != null) {
                        int iconIndex2 = this.role.equip.skill.data.bID - 5;
                        if (iconIndex2 == 6) {
                            iconIndex2 = 7;
                        } else if (iconIndex2 == 7) {
                            iconIndex2 = 6;
                        }
                        Tool.drawClipImage(g, this.skillIconImg, xx3, 214, iconIndex2 * 22, 0, 22, 22);
                        int xx4 = xx3 + 24;
                        String str = String.valueOf(this.role.equip.skill.name) + ((int) this.role.equip.skill.bLevel) + "级";
                        Tool.drawString(g, this.role.equip.skill.name, xx4 + ((65 - font.stringWidth(str)) >> 1), 214 + ((18 - FontHeight) >> 1), 0, -1);
                        Tool.drawString(g, new StringBuilder(String.valueOf((int) this.role.equip.skill.bLevel)).toString(), ((65 - font.stringWidth(str)) >> 1) + xx4 + font.stringWidth(this.role.equip.skill.name), 214 + ((18 - FontHeight) >> 1), 16711680, -1);
                        Tool.drawString(g, "级", ((65 - font.stringWidth(str)) >> 1) + xx4 + font.stringWidth(String.valueOf(this.role.equip.skill.name) + ((int) this.role.equip.skill.bLevel)), 214 + ((18 - FontHeight) >> 1), 0, -1);
                    }
                } else {
                    return;
                }
                break;
            case 1:
                this.menuAS.drawFrameCycle(g, this.bMenuIndex, Tool.countTimes, SCREEN_WIDTH >> 1, SCREEN_HEIGHT >> 1);
                drawScrollItem(g, this.scrollSignImg, this.OFF_X + 302, 87, 108, this.sEquipIndex, this.bag.equipV.size());
                g.setColor(198, 181, 142);
                int xx5 = this.OFF_X + 188;
                if (this.sEquipIndex < 4) {
                    g.fillRect(xx5, (this.sEquipIndex * 31) + 82, 107, 26);
                } else {
                    g.fillRect(xx5, 175, 107, 26);
                }
                int xx6 = this.OFF_X + 220;
                for (int i = 0; i < this.bag.equipV.size(); i++) {
                    if (this.sEquipIndex < 4) {
                        if (i < 4) {
                            Equip equip = (Equip) this.bag.equipV.elementAt(i);
                            Tool.drawString(g, equip.data.name, xx6, ((26 - FontHeight) >> 1) + 82 + (i * 31), Integer.parseInt(COLORS[equip.data.bGrade], 16), 0);
                            if (equip.data.bID < 26 && equip.bReinforcedLevel > 0) {
                                Tool.drawString(g, " + " + ((int) equip.bReinforcedLevel), xx6 + font.stringWidth(equip.data.name), ((26 - FontHeight) >> 1) + 82 + (i * 31), 0, -1, -1);
                            }
                            Tool.drawClipImage(g, this.iconBGImg[equip.data.bGrade], xx6 - 31, 82 + (i * 31), 0, 0, 26, 26);
                            Tool.drawClipImage(g, this.equipIconImg, xx6 - 29, 82 + (i * 31), Integer.parseInt(equip.data.iconIDInfo[1]) * 22, Integer.parseInt(equip.data.iconIDInfo[0]) * 22, 22, 22);
                        }
                    } else if (i > this.sEquipIndex - 4 && i <= this.sEquipIndex) {
                        Equip equip2 = (Equip) this.bag.equipV.elementAt(i);
                        Tool.drawString(g, equip2.data.name, xx6, ((26 - FontHeight) >> 1) + 82 + ((((i - this.sEquipIndex) + 4) - 1) * 31), Integer.parseInt(COLORS[equip2.data.bGrade], 16), 0);
                        if (equip2.data.bID < 26 && equip2.bReinforcedLevel > 0) {
                            Tool.drawString(g, " + " + ((int) equip2.bReinforcedLevel), xx6 + font.stringWidth(equip2.data.name), ((26 - FontHeight) >> 1) + 82 + ((((i - this.sEquipIndex) + 4) - 1) * 31), 0, -1, -1);
                        }
                        Tool.drawClipImage(g, this.iconBGImg[equip2.data.bGrade], xx6 - 31, 82 + ((((i - this.sEquipIndex) + 4) - 1) * 31), 0, 0, 26, 26);
                        Tool.drawClipImage(g, this.equipIconImg, xx6 - 29, 82 + ((((i - this.sEquipIndex) + 4) - 1) * 31), Integer.parseInt(equip2.data.iconIDInfo[1]) * 22, Integer.parseInt(equip2.data.iconIDInfo[0]) * 22, 22, 22);
                    }
                }
                this.role.actionSet.drawFrameCycle(g, 0, Tool.countTimes, this.OFF_X + 140, 204);
                if (this.bag.equipV.size() > 0) {
                    int xx7 = this.OFF_X + 380;
                    Equip choosedEquip = (Equip) this.bag.equipV.elementAt(this.sEquipIndex);
                    Tool.drawNumReturnWidth(g, this.numImg[2], choosedEquip.iFinalHP, xx7, 80, 4, 2, -2);
                    Tool.drawNumReturnWidth(g, this.numImg[2], choosedEquip.iFinalAP, xx7, 80 + 24, 4, 2, -2);
                    Tool.drawNumReturnWidth(g, this.numImg[2], choosedEquip.iFinalDurability, xx7, 24 + 104, 4, 2, -2);
                    Tool.drawNumReturnWidth(g, this.numImg[2], choosedEquip.iFinalDP, xx7, 24 + 128, 4, 2, -2);
                    Tool.drawNumReturnWidth(g, this.numImg[2], choosedEquip.iHitPoint, xx7, 24 + 152, 4, 2, -2);
                    Tool.drawNumReturnWidth(g, this.numImg[2], choosedEquip.iDodgePoint, xx7, 24 + 176, 4, 2, -2);
                    Tool.drawNumReturnWidth(g, this.numImg[2], choosedEquip.iCriticalPoint, xx7, 24 + 200, 4, 2, -2);
                    int xx8 = this.OFF_X + 224;
                    if (choosedEquip.skill != null) {
                        int iconIndex3 = choosedEquip.skill.data.bID - 5;
                        if (iconIndex3 == 6) {
                            iconIndex3 = 7;
                        } else if (iconIndex3 == 7) {
                            iconIndex3 = 6;
                        }
                        Tool.drawClipImage(g, this.skillIconImg, this.OFF_X + 198, 214, iconIndex3 * 22, 0, 22, 22);
                        String str2 = String.valueOf(choosedEquip.skill.name) + ((int) choosedEquip.skill.bLevel) + "级";
                        Tool.drawString(g, choosedEquip.skill.name, xx8 + ((64 - font.stringWidth(str2)) >> 1), 212 + ((24 - FontHeight) >> 1), 0, -1);
                        Tool.drawString(g, new StringBuilder(String.valueOf((int) choosedEquip.skill.bLevel)).toString(), ((64 - font.stringWidth(str2)) >> 1) + xx8 + font.stringWidth(choosedEquip.skill.name), 212 + ((24 - FontHeight) >> 1), 16711680, -1);
                        Tool.drawString(g, "级", ((64 - font.stringWidth(str2)) >> 1) + xx8 + font.stringWidth(String.valueOf(choosedEquip.skill.name) + ((int) choosedEquip.skill.bLevel)), 212 + ((24 - FontHeight) >> 1), 0, -1);
                    } else {
                        Tool.drawString(g, "无技能", xx8 + ((64 - font.stringWidth("无技能")) >> 1), 212 + ((24 - FontHeight) >> 1), 0, -1);
                    }
                    if (this.role.equip.equals(choosedEquip)) {
                        this.menuAS.drawFrame(g, 5, (SCREEN_WIDTH >> 1) - 191, (SCREEN_HEIGHT >> 1) + 35);
                    }
                    if (choosedEquip.iCurDurability == 0) {
                        this.menuAS.drawFrame(g, 6, ((SCREEN_WIDTH >> 1) - 200) + 9, (SCREEN_HEIGHT >> 1) + 35);
                    }
                }
                break;
            case 2:
                this.menuAS.drawFrameCycle(g, this.bMenuIndex, Tool.countTimes, SCREEN_WIDTH >> 1, SCREEN_HEIGHT >> 1);
                drawScrollItem(g, this.scrollSignImg, this.OFF_X + 322, 94, 140, this.sItemIndex, this.bag.propV.size());
                g.setColor(198, 181, 142);
                int xx9 = this.OFF_X + 128;
                if (this.sItemIndex < 5) {
                    g.fillRect(xx9, (this.sItemIndex * 31) + 88, 187, 26);
                } else {
                    g.fillRect(xx9, 212, 187, 26);
                }
                int xx10 = this.OFF_X + 150;
                for (int i2 = 0; i2 < this.bag.propV.size(); i2++) {
                    Prop prop = (Prop) this.bag.propV.elementAt(i2);
                    if (this.sItemIndex < 5) {
                        if (i2 < 5) {
                            Tool.drawString(g, prop.data.name, xx10 + ((160 - font.stringWidth(prop.data.name)) >> 1), ((26 - FontHeight) >> 1) + 91 + (i2 * 31), Integer.parseInt(COLORS[prop.data.bType], 16), 0);
                            Tool.drawClipImage(g, this.iconBGImg[prop.data.bType], xx10 - 25, (i2 * 31) + 89, 0, 0, 26, 26);
                            Tool.drawClipImage(g, this.propIconImg, xx10 - 23, (i2 * 31) + 89, (prop.data.bID == 23 ? (byte) 8 : prop.data.bID) * 22, 0, 22, 22);
                            Tool.drawNumReturnWidth(g, this.numImg[6], prop.number, xx10 - 8, (i2 * 31) + 106, 2, 0, -1);
                        }
                    } else if (i2 > this.sItemIndex - 5 && i2 <= this.sItemIndex) {
                        Tool.drawString(g, prop.data.name, xx10 + ((160 - font.stringWidth(prop.data.name)) >> 1), ((26 - FontHeight) >> 1) + 91 + ((((i2 - this.sItemIndex) + 5) - 1) * 31), Integer.parseInt(COLORS[prop.data.bType], 16), 0);
                        Tool.drawClipImage(g, this.iconBGImg[prop.data.bType], xx10 - 25, ((((i2 - this.sItemIndex) + 5) - 1) * 31) + 89, 0, 0, 26, 26);
                        Tool.drawClipImage(g, this.propIconImg, xx10 - 23, 91 + ((((i2 - this.sItemIndex) + 5) - 1) * 31), (prop.data.bID == 23 ? (byte) 8 : prop.data.bID) * 22, 0, 22, 22);
                        Tool.drawNumReturnWidth(g, this.numImg[6], prop.number, xx10 - 8, ((((i2 - this.sItemIndex) + 5) - 1) * 31) + 106, 2, 0, -1);
                    }
                }
                if (this.UIST != null) {
                    this.UIST.draw(g, this.iUIInfoX, this.iUIInfoY);
                }
                if (this.isPopUpOption) {
                    this.menuAS.drawFrameCycle(g, this.bPopUpOptionIndex + 15, Tool.countTimes, SCREEN_WIDTH >> 1, SCREEN_HEIGHT >> 1);
                }
                break;
            case 3:
                this.menuAS.drawFrameCycle(g, 23, Tool.countTimes, SCREEN_WIDTH >> 1, SCREEN_HEIGHT >> 1);
                this.menuAS.drawFrameCycle(g, this.bFamulusFaceState + 24, Tool.countTimes, SCREEN_WIDTH >> 1, SCREEN_HEIGHT >> 1);
                int offX = this.OFF_X + 186;
                if (!this.isFeed) {
                    this.menuAS.drawFrameCycle(g, this.bMenuIndex, Tool.countTimes, SCREEN_WIDTH >> 1, SCREEN_HEIGHT >> 1);
                    if (!this.isFeedOver) {
                        Tool.drawNumReturnWidth(g, this.numImg[2], this.role.famulus.iFinalHP, offX + 178, 92, 4, 2, 0);
                        int i3 = 35 + 21;
                        Tool.drawNumReturnWidth(g, this.numImg[2], this.role.famulus.iFinalDurability, offX + 178, 113, 4, 2, 0);
                        int offY = 21 + 56;
                        Tool.drawNumReturnWidth(g, this.numImg[2], this.role.famulus.iFinalAP, offX + 178, 134, 4, 2, 0);
                        int offY2 = 21 + 77;
                        Tool.drawNumReturnWidth(g, this.numImg[2], this.role.famulus.iFinalDP, offX + 178, 155, 4, 2, 0);
                        int offY3 = 21 + 98;
                        Tool.drawNumReturnWidth(g, this.numImg[2], this.role.famulus.iFinalHitPoint, offX + 178, 176, 4, 2, 0);
                        int offY4 = 21 + 119;
                        Tool.drawNumReturnWidth(g, this.numImg[2], this.role.famulus.iFinalDodgePoint, offX + 178, 197, 4, 2, 0);
                        int i4 = 21 + 140;
                        Tool.drawNumReturnWidth(g, this.numImg[2], this.role.famulus.iFinalCriticalPoint, offX + 178, 218, 4, 2, 0);
                    } else {
                        int hpUp = this.role.famulus.iFinalHP - this.role.famulus.iPreMaxHP;
                        int durabilityUp = this.role.famulus.iFinalDurability - this.role.famulus.iPreMaxDurability;
                        int APUp = this.role.famulus.iFinalAP - this.role.famulus.iPreAP;
                        int DPUp = this.role.famulus.iFinalDP - this.role.famulus.iPreDP;
                        int hitUp = this.role.famulus.iFinalHitPoint - this.role.famulus.iPreHitPoint;
                        int dodgeUp = this.role.famulus.iFinalDodgePoint - this.role.famulus.iPreDodgePoint;
                        int criticalUp = this.role.famulus.iFinalCriticalPoint - this.role.famulus.iPreCriticalPoint;
                        Tool.drawNumReturnWidth(g, this.numImg[2], this.role.famulus.iFinalHP, offX + 155, 92, 4, 2, -2);
                        int i5 = 35 + 21;
                        Tool.drawNumReturnWidth(g, this.numImg[2], this.role.famulus.iFinalDurability, offX + 155, 113, 4, 2, -2);
                        int offY5 = 21 + 56;
                        Tool.drawNumReturnWidth(g, this.numImg[2], this.role.famulus.iFinalAP, offX + 155, 134, 4, 2, -2);
                        int offY6 = 21 + 77;
                        Tool.drawNumReturnWidth(g, this.numImg[2], this.role.famulus.iFinalDP, offX + 155, 155, 4, 2, -2);
                        int offY7 = 21 + 98;
                        Tool.drawNumReturnWidth(g, this.numImg[2], this.role.famulus.iFinalHitPoint, offX + 155, 176, 4, 2, -2);
                        int offY8 = 21 + 119;
                        Tool.drawNumReturnWidth(g, this.numImg[2], this.role.famulus.iFinalDodgePoint, offX + 155, 197, 4, 2, -2);
                        int i6 = 21 + 140;
                        Tool.drawNumReturnWidth(g, this.numImg[2], this.role.famulus.iFinalCriticalPoint, offX + 155, 218, 4, 2, -2);
                        if (hpUp > 0) {
                            Tool.drawImage(g, this.arrowUpImg, offX + 175, 92 - (Tool.countTimes2 % 2), 0);
                            Tool.drawNumReturnWidth(g, this.attributeUpNumImg, hpUp, offX + 206, 92, 4, 2, -2);
                        }
                        int i7 = 35 + 21;
                        if (durabilityUp > 0) {
                            Tool.drawImage(g, this.arrowUpImg, offX + 175, 113 - (Tool.countTimes2 % 2), 0);
                            Tool.drawNumReturnWidth(g, this.attributeUpNumImg, durabilityUp, offX + 206, 113, 4, 2, -2);
                        }
                        int offY9 = 21 + 56;
                        if (APUp > 0) {
                            Tool.drawImage(g, this.arrowUpImg, offX + 175, 134 - (Tool.countTimes2 % 2), 0);
                            Tool.drawNumReturnWidth(g, this.attributeUpNumImg, APUp, offX + 206, 134, 4, 2, -2);
                        }
                        int offY10 = 21 + 77;
                        if (DPUp > 0) {
                            Tool.drawImage(g, this.arrowUpImg, offX + 175, 155 - (Tool.countTimes2 % 2), 0);
                            Tool.drawNumReturnWidth(g, this.attributeUpNumImg, DPUp, offX + 206, 155, 4, 2, -2);
                        }
                        int offY11 = 21 + 98;
                        if (hitUp > 0) {
                            Tool.drawImage(g, this.arrowUpImg, offX + 175, 176 - (Tool.countTimes2 % 2), 0);
                            Tool.drawNumReturnWidth(g, this.attributeUpNumImg, hitUp, offX + 206, 176, 4, 2, -2);
                        }
                        int offY12 = 21 + 119;
                        if (dodgeUp > 0) {
                            Tool.drawImage(g, this.arrowUpImg, offX + 175, 197 - (Tool.countTimes2 % 2), 0);
                            Tool.drawNumReturnWidth(g, this.attributeUpNumImg, dodgeUp, offX + 206, 197, 4, 2, -2);
                        }
                        int i8 = 21 + 140;
                        if (criticalUp > 0) {
                            Tool.drawImage(g, this.arrowUpImg, offX + 175, 218 - (Tool.countTimes2 % 2), 0);
                            Tool.drawNumReturnWidth(g, this.attributeUpNumImg, criticalUp, offX + 206, 218, 4, 2, -2);
                        }
                    }
                } else {
                    this.menuAS.drawFrameCycle(g, 6, Tool.countTimes, SCREEN_WIDTH >> 1, SCREEN_HEIGHT >> 1);
                    g.setColor(198, 181, 142);
                    if (this.sEquipIndex < 5) {
                        g.fillRect(offX + 104, (this.sEquipIndex * 31) + 50 + 35, 107, 26);
                    } else {
                        g.fillRect(offX + 104, 209, 107, 26);
                    }
                    for (int i9 = 0; i9 < this.candidateEquipV.size(); i9++) {
                        Equip equip3 = (Equip) this.candidateEquipV.elementAt(i9);
                        if (this.sEquipIndex < 5) {
                            if (i9 < 5) {
                                Tool.drawString(g, equip3.data.name, offX + 132, ((26 - FontHeight) >> 1) + 87 + (i9 * 31), Integer.parseInt(COLORS[equip3.data.bGrade], 16), 0);
                                if (equip3.data.bID < 26 && equip3.bReinforcedLevel > 0) {
                                    Tool.drawString(g, " + " + ((int) equip3.bReinforcedLevel), offX + 132 + font.stringWidth(equip3.data.name), ((26 - FontHeight) >> 1) + 87 + (i9 * 31), 0, -1, -1);
                                }
                                Tool.drawClipImage(g, this.iconBGImg[equip3.data.bGrade], offX + 105, (i9 * 31) + 85, 0, 0, 26, 26);
                                Tool.drawClipImage(g, this.equipIconImg, offX + 107, (i9 * 31) + 87, Integer.parseInt(equip3.data.iconIDInfo[1]) * 22, Integer.parseInt(equip3.data.iconIDInfo[0]) * 22, 22, 22);
                            }
                        } else if (i9 > this.sEquipIndex - 5 && i9 <= this.sEquipIndex) {
                            Tool.drawString(g, equip3.data.name, offX + 132, ((26 - FontHeight) >> 1) + 87 + ((((i9 - this.sEquipIndex) + 5) - 1) * 31), Integer.parseInt(COLORS[equip3.data.bGrade], 16), 0);
                            if (equip3.data.bID < 26 && equip3.bReinforcedLevel > 0) {
                                Tool.drawString(g, " + " + ((int) equip3.bReinforcedLevel), offX + 132 + font.stringWidth(equip3.data.name), ((26 - FontHeight) >> 1) + 87 + ((((i9 - this.sEquipIndex) + 5) - 1) * 31), 0, -1, -1);
                            }
                            Tool.drawClipImage(g, this.iconBGImg[equip3.data.bGrade], offX + 105, ((((i9 - this.sEquipIndex) + 5) - 1) * 31) + 85, 0, 0, 26, 26);
                            Tool.drawClipImage(g, this.equipIconImg, offX + 107, ((((i9 - this.sEquipIndex) + 5) - 1) * 31) + 87, Integer.parseInt(equip3.data.iconIDInfo[1]) * 22, Integer.parseInt(equip3.data.iconIDInfo[0]) * 22, 22, 22);
                        }
                    }
                    drawScrollItem(g, this.scrollSignImg, offX + 216, 91, 139, this.sEquipIndex, this.candidateEquipV.size());
                }
                int xx11 = (SCREEN_WIDTH - 320) >> 1;
                Tool.drawNumReturnWidth(g, this.numImg[2], this.role.famulus.sLevel, xx11 + 14 + ((this.famulusExpImg.getWidth() * 2) / 3), 175, 3, 2, -2);
                Tool.drawClipImage(g, this.famulusExpImg, xx11 + 19, 190, 0, 0, (this.famulusExpImg.getWidth() * this.role.famulus.iCurrentExp) / this.role.famulus.iLevelUpExp, this.famulusExpImg.getHeight());
                if (this.iExpIncrease > 0) {
                    this.menuAS.drawFrameCycle(g, 21, Tool.countTimes, xx11 + 19 + ((this.famulusExpImg.getWidth() * this.role.famulus.iCurrentExp) / this.role.famulus.iLevelUpExp), 200);
                }
                if (this.UIST != null) {
                    this.UIST.draw(g, this.iUIInfoX, this.iUIInfoY);
                }
                if (this.isFlashEffect) {
                    this.menuAS.drawFrameCycle(g, DROP_POS_INTERVAL, this.bFlashCounter + STATE_CG, SCREEN_WIDTH >> 1, SCREEN_HEIGHT >> 1);
                }
                int yy2 = SCREEN_HEIGHT - 69;
                int xx12 = xx11 + 111;
                if (this.role.famulus.skill != null) {
                    int iconIndex4 = this.role.famulus.skill.data.bID - 5;
                    if (iconIndex4 == 6) {
                        iconIndex4 = 7;
                    } else if (iconIndex4 == 7) {
                        iconIndex4 = 6;
                    }
                    Tool.drawClipImage(g, this.skillIconImg, xx12, yy2, iconIndex4 * 22, 0, 22, 22);
                    String str3 = String.valueOf(this.role.famulus.skill.name) + ((int) this.role.famulus.skill.bLevel) + "级";
                    Tool.drawString(g, this.role.famulus.skill.name, xx12 + 24 + ((64 - font.stringWidth(str3)) >> 1), yy2 + ((24 - FontHeight) >> 1), 0, -1);
                    Tool.drawString(g, new StringBuilder(String.valueOf((int) this.role.famulus.skill.bLevel)).toString(), xx12 + 24 + ((64 - font.stringWidth(str3)) >> 1) + font.stringWidth(this.role.famulus.skill.name), yy2 + ((24 - FontHeight) >> 1), 16711680, -1);
                    Tool.drawString(g, "级", xx12 + 24 + ((64 - font.stringWidth(str3)) >> 1) + font.stringWidth(String.valueOf(this.role.famulus.skill.name) + ((int) this.role.famulus.skill.bLevel)), yy2 + ((24 - FontHeight) >> 1), 0, -1);
                } else {
                    Tool.drawString(g, "无技能", xx12 + 24 + ((64 - font.stringWidth("无技能")) >> 1), yy2 + ((24 - FontHeight) >> 1), 0, -1);
                }
                if (this.isFeedTip) {
                    drawRoundRectBGByTile(g, this.uiBGImg, (SCREEN_WIDTH - 300) >> 1, SCREEN_HEIGHT >> 1, this.uiBGImg.getHeight(), this.uiBGImg.getHeight(), 300, 120);
                    for (int i10 = 0; i10 < this.feedTipInfo.length; i10++) {
                        Tool.drawString(g, this.feedTipInfo[i10], ((SCREEN_WIDTH - 320) >> 1) + 14, (SCREEN_HEIGHT >> 1) + 8 + (FontHeight * i10), 0, -1);
                    }
                    Tool.drawString(g, "否", ((SCREEN_WIDTH - 300) >> 1) + 2, (((SCREEN_HEIGHT + 240) >> 1) - FontHeight) - 2, 16711680, -1);
                    Tool.drawString(g, "是", ((SCREEN_WIDTH / 2) + 150) - FontWidth, (((SCREEN_HEIGHT + 240) >> 1) - FontHeight) - 2, 16711680, -1);
                }
                break;
            case 4:
                if (!this.isBranchMission) {
                    this.menuAS.drawFrameCycle(g, this.bMenuIndex, Tool.countTimes, SCREEN_WIDTH >> 1, SCREEN_HEIGHT >> 1);
                } else {
                    this.menuAS.drawFrameCycle(g, 7, Tool.countTimes, SCREEN_WIDTH >> 1, SCREEN_HEIGHT >> 1);
                }
                int xx13 = this.OFF_X + 210;
                if (this.missionHortation[this.isBranchMission ? (char) 1 : (char) 0][0] > 0) {
                    Tool.drawNumReturnWidth(g, this.numImg[0], this.missionHortation[this.isBranchMission ? (char) 1 : (char) 0][0] * this.bExpCoefficientBySMS, xx13, 212, 3, 0, 0);
                }
                int xx14 = xx13 + FIRE_KEY_HEIGHT;
                if (this.missionHortation[this.isBranchMission ? (char) 1 : (char) 0][1] > 0) {
                    Tool.drawClipImage(g, this.moneyImg, xx14, 212, this.moneyImg.getWidth() >> 1, 0, this.moneyImg.getWidth() >> 1, this.moneyImg.getHeight());
                    Tool.drawNumReturnWidth(g, this.numImg[1], this.missionHortation[this.isBranchMission ? (char) 1 : (char) 0][1], xx14 + 20, 212, 3, 0, 0);
                }
                int xx15 = xx14 - 80;
                int yy3 = 212 + 13;
                if (this.missionHortation[this.isBranchMission ? (char) 1 : (char) 0][2] >= 0) {
                    Equip equip4 = new Equip(equipDatas[this.missionHortation[this.isBranchMission ? (char) 1 : (char) 0][2]], 0, true);
                    Tool.drawString(g, equip4.data.name, xx15 + 49, yy3, Integer.parseInt(COLORS[equip4.data.bGrade], 16), 0);
                    Tool.drawClipImage(g, this.iconBGImg[equip4.data.bGrade], xx15, yy3, 0, 0, 26, 26);
                    Tool.drawClipImage(g, this.equipIconImg, xx15 + 2, yy3, Integer.parseInt(equip4.data.iconIDInfo[1]) * 22, Integer.parseInt(equip4.data.iconIDInfo[0]) * 22, 22, 22);
                }
                int xx16 = this.OFF_X + 120;
                if (this.mainMissionTitle != null) {
                    Tool.drawString(g, this.mainMissionTitle, xx16 + ((180 - font.stringWidth(this.mainMissionTitle)) >> 1), 72 + ((50 - FontHeight) >> 1), 0, -1);
                }
                if (this.branchMissionTitle != null) {
                    Tool.drawString(g, this.branchMissionTitle, xx16 + ((180 - font.stringWidth(this.branchMissionTitle)) >> 1), ((50 - FontHeight) >> 1) + 128, 0, -1);
                }
                if (this.UIST != null) {
                    this.UIST.draw(g, this.iUIInfoX, this.iUIInfoY);
                }
                break;
            case 5:
                if (this.bSystemIndex == 0) {
                    this.menuAS.drawFrameCycle(g, this.bMenuIndex, Tool.countTimes, SCREEN_WIDTH >> 1, SCREEN_HEIGHT >> 1);
                } else {
                    this.menuAS.drawFrameCycle(g, this.bSystemIndex + 7, Tool.countTimes, SCREEN_WIDTH >> 1, SCREEN_HEIGHT >> 1);
                }
                break;
        }
        if (this.scrollInfoState > 0) {
            drawScrollInfo(g);
        }
        if (this.bMenuIndex != 3) {
            drawMoney(g);
        }
    }

    private void drawMoney(Graphics g) {
        int xx = this.OFF_X + 264;
        Tool.drawNumReturnWidth(g, this.numImg[0], this.bag.iMoney / 10000, xx, 258, 6, 1, -1);
        Tool.drawNumReturnWidth(g, this.numImg[1], this.bag.iMoney % 10000, xx + 50, 258, 6, 1, -1);
        Tool.drawClipImage(g, this.moneyImg, xx + 2, 258, 0, 0, this.moneyImg.getWidth() >> 1, this.moneyImg.getHeight());
        Tool.drawClipImage(g, this.moneyImg, xx + 52, 258, this.moneyImg.getWidth() >> 1, 0, this.moneyImg.getWidth() >> 1, this.moneyImg.getHeight());
    }

    private void showRoleSkill(Graphics g) {
        int bActionID = 0;
        int xx = this.OFF_X + 155;
        this.bMiscellaneousType = STATE_CG;
        if (this.role.bSkillIndex < 0) {
            if (this.role.isIntegrate) {
                bActionID = 27;
            } else {
                bActionID = 0;
            }
        } else if (this.role.bSkillIndex < 9) {
            if (this.role.isIntegrate) {
                bActionID = this.role.bSkillIndex + 39;
            } else {
                bActionID = this.role.bSkillIndex + 12;
            }
        } else if (this.role.bSkillIndex < 17) {
            if (this.role.isIntegrate) {
                bActionID = 48;
            } else {
                bActionID = 21;
            }
            if (this.role.bSkillIndex < 12) {
                this.bMiscellaneousType = (byte) 1;
            } else if (this.role.bSkillIndex < 15) {
                this.bMiscellaneousType = (byte) 2;
            } else {
                this.bMiscellaneousType = (byte) 0;
            }
        }
        if (!this.isSkillPreview) {
            if (this.role.isIntegrate) {
                this.role.actionSet.drawFrameCycle(g, 27, Tool.countTimes, xx, 233, true);
            } else {
                this.role.actionSet.drawFrameCycle(g, 0, Tool.countTimes, xx, 233, true);
            }
            this.menuAS.drawFrame(g, 2, (SCREEN_WIDTH >> 1) - 70, (SCREEN_HEIGHT >> 1) - 60);
        }
        g.setColor(0);
        g.fillRect(0, 0, SCREEN_WIDTH, SCREEN_HEIGHT);
        if (this.bMiscellaneousType >= 0 && this.bSkillPreviewIndex >= this.role.actionSet.actionDatas[21].frameNum) {
            this.role.actionSet.drawFrameCycle(g, 0, Tool.countTimes, xx, 233, true);
            this.role.miscellaneousAS.drawFrameCycle(g, this.bMiscellaneousType, this.bSkillPreviewIndex - this.role.actionSet.actionDatas[21].frameNum, xx, 233);
            return;
        }
        int actID = bActionID;
        if (this.role.isIntegrate) {
            actID -= 27;
        }
        switch (actID) {
            case 12:
            case 14:
            case Graphics.TOP /* 16 */:
            case 21:
                if (this.role.skillAS != null) {
                    this.role.skillAS.drawFrameCycle(g, 1, this.bSkillPreviewIndex, xx, 233, true);
                }
                break;
        }
        this.role.actionSet.drawFrameCycle(g, bActionID, this.bSkillPreviewIndex, xx, 233, true);
        switch (actID) {
            case 12:
            case 13:
            case 14:
            case 15:
            case Graphics.TOP /* 16 */:
            case Graphics.HT /* 17 */:
            case 18:
            case TimeUtils.HUNDRED_DAY_FIELD_LEN /* 19 */:
            case 21:
                if (this.role.skillAS != null) {
                    this.role.skillAS.drawFrameCycle(g, 0, this.bSkillPreviewIndex, xx, 233, true);
                }
                break;
        }
    }

    void handleKey_GameMenu() {
        if (!this.isExitGameMenu) {
            if (this.iExpIncrease > 0) {
                if (Common.isKeyPressed(10, true)) {
                    short sLevelBeforeFeed = this.role.famulus.sLevel;
                    this.role.famulus.addExp(this.iExpIncrease);
                    this.iExpIncrease = 0;
                    if (this.role.famulus.sLevel > sLevelBeforeFeed) {
                        this.isFlashEffect = true;
                        this.bFlashCounter = (byte) 0;
                        feedOver();
                        return;
                    }
                    return;
                }
                return;
            }
            if (this.isPopUpOption) {
                if (Common.isLeftPressed(true)) {
                    this.bPopUpOptionIndex = (byte) 0;
                    return;
                }
                if (Common.isRightPressed(true)) {
                    this.bPopUpOptionIndex = (byte) 1;
                    return;
                }
                if (Common.isOKPressed(true) || Common.isKeyPressed(9, true)) {
                    Prop prop = (Prop) this.bag.propV.elementAt(this.sItemIndex);
                    if (this.bPopUpOptionIndex == 0) {
                        useProp(prop);
                        if (this.bag.propV.size() > 0 && this.sItemIndex >= this.bag.propV.size()) {
                            this.sItemIndex = (short) (this.bag.propV.size() - 1);
                            getPropInfo();
                        } else {
                            getPropInfo();
                        }
                    } else {
                        this.bag.deleteProp(prop);
                        if (this.bag.propV.size() > 0 && this.sItemIndex >= this.bag.propV.size()) {
                            this.sItemIndex = (short) (this.bag.propV.size() - 1);
                            getPropInfo();
                        } else {
                            getPropInfo();
                        }
                    }
                    this.isPopUpOption = false;
                    return;
                }
                if (Common.isKeyPressed(10, true)) {
                    this.isPopUpOption = false;
                    return;
                }
                return;
            }
            if (Common.isLeftPressed(true)) {
                this.bMenuIndex = (byte) (this.bMenuIndex + STATE_CG);
                if (this.bMenuIndex < 0) {
                    this.bMenuIndex = (byte) 5;
                }
                if (this.role.famulus == null && this.bMenuIndex == 3) {
                    this.bMenuIndex = (byte) 2;
                }
                switch (this.bMenuIndex) {
                    case 1:
                        this.UIST = null;
                        this.sEquipIndex = (short) 0;
                        break;
                    case 2:
                        this.sItemIndex = (short) 0;
                        getPropInfo();
                        break;
                    case 3:
                        this.isFeed = false;
                        this.isFeedOver = false;
                        this.isFeedTip = false;
                        getFamulusInfo(0);
                        break;
                    case 4:
                        getMissionInfo();
                        break;
                    default:
                        this.UIST = null;
                        break;
                }
            }
            if (Common.isRightPressed(true)) {
                this.bMenuIndex = (byte) (this.bMenuIndex + 1);
                if (this.bMenuIndex > 5) {
                    this.bMenuIndex = (byte) 0;
                }
                if (this.role.famulus == null && this.bMenuIndex == 3) {
                    this.bMenuIndex = (byte) 4;
                }
                switch (this.bMenuIndex) {
                    case 1:
                        this.UIST = null;
                        this.sEquipIndex = (short) 0;
                        break;
                    case 2:
                        this.sItemIndex = (short) 0;
                        getPropInfo();
                        break;
                    case 3:
                        this.isFeedOver = false;
                        this.isFeed = false;
                        this.isFeedTip = false;
                        getFamulusInfo(0);
                        break;
                    case 4:
                        getMissionInfo();
                        break;
                    default:
                        this.UIST = null;
                        break;
                }
            }
            if (Common.isUpPressed(true)) {
                switch (this.bMenuIndex) {
                    case 1:
                        if (this.sEquipIndex > 0) {
                            this.sEquipIndex = (short) (this.sEquipIndex - 1);
                        } else {
                            this.sEquipIndex = (short) (this.bag.equipV.size() - 1);
                        }
                        break;
                    case 2:
                        if (this.sItemIndex > 0) {
                            this.sItemIndex = (short) (this.sItemIndex - 1);
                        } else if (this.bag.propV.size() > 0) {
                            this.sItemIndex = (short) (this.bag.propV.size() - 1);
                        }
                        getPropInfo();
                        break;
                    case 3:
                        if (this.isFeed) {
                            if (this.sEquipIndex > 0) {
                                this.sEquipIndex = (short) (this.sEquipIndex - 1);
                            } else {
                                this.sEquipIndex = (short) (this.candidateEquipV.size() - 1);
                            }
                        }
                        break;
                    case 4:
                        this.isBranchMission = false;
                        getMissionInfo();
                        break;
                    case 5:
                        this.bSystemIndex = (byte) (this.bSystemIndex + STATE_CG);
                        if (this.bSystemIndex < 0) {
                            this.bSystemIndex = (byte) 5;
                        }
                        break;
                }
            }
            if (Common.isDownPressed(true)) {
                switch (this.bMenuIndex) {
                    case 1:
                        if (this.sEquipIndex < this.bag.equipV.size() - 1) {
                            this.sEquipIndex = (short) (this.sEquipIndex + 1);
                        } else {
                            this.sEquipIndex = (short) 0;
                        }
                        break;
                    case 2:
                        if (this.sItemIndex < this.bag.propV.size() - 1) {
                            this.sItemIndex = (short) (this.sItemIndex + 1);
                        } else {
                            this.sItemIndex = (short) 0;
                        }
                        getPropInfo();
                        break;
                    case 3:
                        if (this.isFeed) {
                            if (this.sEquipIndex < this.candidateEquipV.size() - 1) {
                                this.sEquipIndex = (short) (this.sEquipIndex + 1);
                            } else {
                                this.sEquipIndex = (short) 0;
                            }
                        }
                        break;
                    case 4:
                        this.isBranchMission = true;
                        getMissionInfo();
                        break;
                    case 5:
                        this.bSystemIndex = (byte) (this.bSystemIndex + 1);
                        if (this.bSystemIndex > 5) {
                            this.bSystemIndex = (byte) 0;
                        }
                        break;
                }
            }
            if (Common.isOKPressed(true)) {
                switch (this.bMenuIndex) {
                    case 1:
                        this.role.updateEquipment((Equip) this.bag.equipV.elementAt(this.sEquipIndex));
                        break;
                    case 2:
                        if (this.bag.propV.size() > 0) {
                            this.isPopUpOption = true;
                            this.bPopUpOptionIndex = (byte) 0;
                        }
                        break;
                    case 3:
                        if (!this.isFeed) {
                            getCandidateEquip();
                        } else {
                            Equip equip = (Equip) this.candidateEquipV.elementAt(this.sEquipIndex);
                            if (!this.isFeedTip) {
                                if (equip.skill != null) {
                                    if (this.role.famulus.skill != null) {
                                        if (equip.skill.data.bID != this.role.famulus.skill.data.bID || equip.skill.bLevel > this.role.famulus.skill.bLevel) {
                                            this.isFeedTip = true;
                                            this.feedTipInfo = Tool.getStringArray("是否继承技能" + equip.skill.name + ((int) equip.skill.bLevel) + "级?\n继承技能将替换灵剑现有的技能");
                                        } else {
                                            feed(equip);
                                        }
                                    } else {
                                        this.role.famulus.skill = equip.skill;
                                        this.role.sCycleTime[0] = this.role.famulus.skill.data.sCD;
                                        this.role.sCDCount[0] = 0;
                                        feed(equip);
                                    }
                                } else {
                                    feed(equip);
                                }
                            }
                        }
                        break;
                    case 5:
                        switch (this.bSystemIndex) {
                            case 0:
                                if (this.isOpenAllStage || this.isDX) {
                                    createSMSShopMenu();
                                } else {
                                    script_showMessage("尚未开启！", 0);
                                }
                                break;
                            case 1:
                                initShop(0);
                                break;
                            case 2:
                                setGameState((byte) 7);
                                this.isFromMainMenu = false;
                                break;
                            case 3:
                                this.bGameSetState = (byte) 0;
                                setGameState((byte) 3);
                                break;
                            case 4:
                                this.stHelpAbout = new StyledText(this.helpStr, 280, this.menuHeight - 40, Font.fontChinaH / 10, Font.fontChinaH, font);
                                setGameState((byte) 1);
                                break;
                            case 5:
                                releaseGameRes();
                                loadMainMenuRes();
                                this.currentBGMusic = "/cover.mid";
                                playMusic();
                                break;
                        }
                        break;
                }
            }
            if (Common.isKeyPressed(9, true)) {
                switch (this.bMenuIndex) {
                    case 1:
                        this.role.updateEquipment((Equip) this.bag.equipV.elementAt(this.sEquipIndex));
                        break;
                    case 2:
                        if (this.bag.propV.size() > 0) {
                            this.isPopUpOption = true;
                            this.bPopUpOptionIndex = (byte) 0;
                        }
                        break;
                    case 3:
                        if (!this.isFeed) {
                            getCandidateEquip();
                        } else {
                            Equip equip2 = (Equip) this.candidateEquipV.elementAt(this.sEquipIndex);
                            if (!this.isFeedTip) {
                                if (equip2.skill != null) {
                                    if (this.role.famulus.skill != null) {
                                        if (equip2.skill.data.bID != this.role.famulus.skill.data.bID || equip2.skill.bLevel > this.role.famulus.skill.bLevel) {
                                            this.isFeedTip = true;
                                            this.feedTipInfo = Tool.getStringArray("是否继承技能" + equip2.skill.name + ((int) equip2.skill.bLevel) + "级?\n继承技能将替换灵剑现有的技能");
                                        } else {
                                            feed(equip2);
                                        }
                                    } else {
                                        this.role.famulus.skill = equip2.skill;
                                        this.role.sCycleTime[0] = this.role.famulus.skill.data.sCD;
                                        this.role.sCDCount[0] = 0;
                                        feed(equip2);
                                    }
                                } else {
                                    feed(equip2);
                                }
                            } else {
                                this.isFeedTip = false;
                                feed(equip2);
                            }
                        }
                        break;
                    case 5:
                        switch (this.bSystemIndex) {
                            case 0:
                                if (this.isOpenAllStage || this.isDX) {
                                    createSMSShopMenu();
                                } else {
                                    script_showMessage("尚未开启！", 0);
                                }
                                break;
                            case 1:
                                initShop(0);
                                break;
                            case 2:
                                setGameState((byte) 7);
                                this.isFromMainMenu = false;
                                break;
                            case 3:
                                this.bGameSetState = (byte) 0;
                                setGameState((byte) 3);
                                break;
                            case 4:
                                this.stHelpAbout = new StyledText(this.helpStr, 280, this.menuHeight - 40, Font.fontChinaH / 10, Font.fontChinaH, font);
                                setGameState((byte) 1);
                                break;
                            case 5:
                                releaseGameRes();
                                loadMainMenuRes();
                                break;
                        }
                        break;
                }
            }
            if (Common.isKeyPressed(10, true)) {
                if (!this.isFeed) {
                    if (this.bMenuIndex == 3) {
                        getFamulusInfo(3);
                        this.bExitCounter = (byte) 20;
                        this.isExitGameMenu = true;
                        return;
                    }
                    closeGameMenu();
                    return;
                }
                if (this.bMenuIndex == 3) {
                    if (!this.isFeedTip) {
                        this.isFeed = false;
                        getFamulusInfo(2);
                        return;
                    }
                    Equip equip3 = (Equip) this.candidateEquipV.elementAt(this.sEquipIndex);
                    this.role.famulus.skill = equip3.skill;
                    this.role.updateSkill();
                    feed(equip3);
                    this.isFeedTip = false;
                    return;
                }
                closeGameMenu();
                return;
            }
            if (Common.isKeyPressed(18, true) && this.bMenuIndex == 0 && !this.isSkillPreview) {
                this.isSkillPreview = true;
                this.bSkillPreviewIndex = (byte) 0;
            }
        }
    }

    private void feed(Equip equip) {
        this.isFeed = false;
        this.UIST = null;
        this.bFamulusFaceState = (byte) 4;
        this.sLevelBeforeFeed = this.role.famulus.sLevel;
        this.iLevelUpExp = this.role.famulus.iLevelUpExp;
        this.role.famulus.iPreMaxHP = this.role.famulus.iFinalHP;
        this.role.famulus.iPreMaxDurability = this.role.famulus.iFinalDurability;
        this.role.famulus.iPreAP = this.role.famulus.iFinalAP;
        this.role.famulus.iPreDP = this.role.famulus.iFinalDP;
        this.role.famulus.iPreHitPoint = this.role.famulus.iFinalHitPoint;
        this.role.famulus.iPreDodgePoint = this.role.famulus.iFinalDodgePoint;
        this.role.famulus.iPreCriticalPoint = this.role.famulus.iFinalCriticalPoint;
        short s = this.role.famulus.sLevel;
        this.role.famulus.getClass();
        if (s < 40) {
            this.iExpIncrease = equip.data.bLevel * (equip.bReinforcedLevel + 1 + ((equip.bReinforcedLevel / 9) * 2)) * 5;
            if (this.iExpIncrease > 10000) {
                this.iExpIncrease = equip.data.bLevel * 10000;
            } else {
                this.iExpIncrease *= equip.data.bLevel;
            }
            this.iExpIncrease *= this.bExpCoefficientBySMS;
            this.iTotalIncreaseExp = this.iExpIncrease;
        } else {
            this.iExpIncrease = 0;
            this.iTotalIncreaseExp = 0;
            feedOver();
        }
        this.role.famulus.updateAttributeByEatEquip(equip);
        this.bag.deleteEquip(equip);
    }

    private void feedOver() {
        String str;
        this.isFeedOver = true;
        short s = this.role.famulus.sLevel;
        this.role.famulus.getClass();
        if (s == 40) {
            str = "虽然我已经恢复到最强状态了，不过享用一下武器也是不错的感觉哦~谢谢你啦。";
        } else if (this.iTotalIncreaseExp < (this.iLevelUpExp * 5) / 100) {
            this.bFamulusFaceState = (byte) 3;
            int rnd = Tool.getNextRnd(0, 3);
            if (rnd == 0) {
                str = "不行哦~这样的武器对我帮助不大。";
            } else if (rnd == 1) {
                str = "提升很微弱，还是给我别的武器吧。";
            } else {
                str = "几乎没有成长，下次能给我更好的武器么？";
            }
        } else if (this.iTotalIncreaseExp < (this.iLevelUpExp * 10) / 100) {
            this.bFamulusFaceState = (byte) 1;
            if (Tool.getNextRnd(0, 2) == 0) {
                str = "灵气有点少，不过有些成长。";
            } else {
                str = "成长了一些，下次还会更多吧？";
            }
        } else if (this.iTotalIncreaseExp < (this.iLevelUpExp * 20) / 100) {
            this.bFamulusFaceState = (byte) 0;
            if (Tool.getNextRnd(0, 2) == 0) {
                str = "成长了很多~多亏你啦~";
            } else {
                str = "提高了不少能力，这样很快就能帮到你了。";
            }
        } else {
            this.bFamulusFaceState = (byte) 5;
            if (this.role.famulus.sLevel > this.sLevelBeforeFeed) {
                str = "升级了~能力大幅提高，这下应该可以帮到你了。";
            } else if (Tool.getNextRnd(0, 2) == 0) {
                str = "灵气十分充足~再给我来一份吧~";
            } else {
                str = "吃到好东西了~能力大幅提高！";
            }
        }
        this.UIST = new StyledText(str, this.iUIInfoWidth, this.iUIInfoHeight, Font.fontChinaH / 10, Font.fontChinaH, font);
    }

    public void drawScrollItem(Graphics g, Bitmap scrollItemImg, int x, int y, int height, int curLineIndex, int totalLineNum) {
        scrollItemImg.getWidth();
        int h = scrollItemImg.getHeight();
        int offY = 0;
        if (totalLineNum > 1) {
            offY = ((height - h) * curLineIndex) / (totalLineNum - 1);
        }
        g.drawImage(scrollItemImg, x, y + offY, 0);
    }

    void drawOption(Graphics g, String[] options, int optionWidth, int optionHeight) {
        int x = (SCREEN_WIDTH - optionWidth) >> 1;
        int y = (SCREEN_HEIGHT - (options.length * optionHeight)) >> 1;
        drawOption(g, options, optionWidth, optionHeight, x, y, this.bPopUpOptionIndex, 16739328, -1, 0);
    }

    private void drawOption(Graphics g, String[] options, int optionWidth, int optionHeight, int x, int y, int selectIndex, int choosedColor, int fColor, int bgColor) {
        int i = 0;
        while (i < options.length) {
            drawRoundRectBGByTile(g, this.uiBGImg, x, (y - 2) + (i * optionHeight), this.uiBGImg.getHeight(), this.uiBGImg.getHeight(), optionWidth, optionHeight);
            Tool.drawString(g, options[i], x + ((optionWidth - font.stringWidth(options[i])) >> 1), ((optionHeight - Font.fontChinaH) >> 1) + (i * optionHeight) + y, selectIndex == i ? choosedColor : fColor, bgColor);
            i++;
        }
    }

    private void initEquipPreview(int equipID, int reinForceLevel, boolean isFree) {
        this.previewEquip = new Equip(equipDatas[equipID], reinForceLevel, false);
        loadUIAction();
        String[] temp = Tool.getStringArray(this.previewEquip.data.resStr, ',');
        this.role.reload(temp, 4);
        getPreviewInfo();
        if (this.equipIconImg == null) {
            this.equipIconImg = Tool.loadPImage("/pic/weapon.p");
        }
        this.bGameState = (byte) 14;
        this.isFree = isFree;
        this.script.pause();
    }

    void initReinforce() {
        this.candidateItemV = new Vector(this.bag.equipV.size());
        for (int i = 0; i < this.bag.equipV.size(); i++) {
            Equip equip = (Equip) this.bag.equipV.elementAt(i);
            if (equip.data.bID < 26 && equip.bReinforcedLevel < 9) {
                this.candidateItemV.addElement(equip);
            }
        }
        if (this.candidateItemV.size() > 0) {
            loadUIAction();
            if (this.equipIconImg == null) {
                this.equipIconImg = Tool.loadPImage("/pic/weapon.p");
            }
            this.sReinforceItemIndex = (short) 0;
            this.bGameState = STATE_REINFORCE;
            return;
        }
        showMessage("无可强化的武器!", 0, (byte) 1);
    }

    void releaseReinforceRes() {
        cleanUpUIAction();
        this.equipIconImg = null;
    }

    void drawReinforceUI(Graphics g) {
        int xx = SCREEN_WIDTH >> 1;
        int yy = (SCREEN_HEIGHT - 240) >> 1;
        this.menuAS.drawFrameCycle(g, 31, Tool.countTimes, xx, SCREEN_HEIGHT >> 1);
        drawScrollItem(g, this.scrollSignImg, xx + 36, yy + 48, 108, this.sReinforceItemIndex, this.candidateItemV.size());
        g.setColor(198, 181, 142);
        int xx2 = ((SCREEN_WIDTH - 320) >> 1) + 84;
        int yy2 = yy + 42;
        if (this.sReinforceItemIndex < 4) {
            g.fillRect(xx2, (this.sReinforceItemIndex * 31) + yy2, 107, 26);
        } else {
            g.fillRect(xx2, yy2 + 93, 107, 26);
        }
        if (this.candidateItemV.size() > 0) {
            for (int i = 0; i < this.candidateItemV.size(); i++) {
                if (this.sReinforceItemIndex < 4) {
                    if (i < 4) {
                        Equip equip = (Equip) this.candidateItemV.elementAt(i);
                        Tool.drawString(g, equip.data.name, xx2 + DROP_POS_INTERVAL, ((26 - FontHeight) >> 1) + yy2 + (i * 31), Integer.parseInt(COLORS[equip.data.bGrade], 16), 0);
                        if (equip.data.bID < 26 && equip.bReinforcedLevel > 0) {
                            Tool.drawString(g, " + " + ((int) equip.bReinforcedLevel), xx2 + 32 + font.stringWidth(equip.data.name), ((26 - FontHeight) >> 1) + yy2 + (i * 31), 0, -1, -1);
                        }
                        Tool.drawClipImage(g, this.iconBGImg[equip.data.bGrade], xx2, yy2 + (i * 31), 0, 0, 26, 26);
                        Tool.drawClipImage(g, this.equipIconImg, xx2 + 2, yy2 + (i * 31), Integer.parseInt(equip.data.iconIDInfo[1]) * 22, Integer.parseInt(equip.data.iconIDInfo[0]) * 22, 22, 22);
                    }
                } else if (i > this.sReinforceItemIndex - 4 && i <= this.sReinforceItemIndex) {
                    Equip equip2 = (Equip) this.candidateItemV.elementAt(i);
                    Tool.drawString(g, equip2.data.name, xx2 + DROP_POS_INTERVAL, ((26 - FontHeight) >> 1) + yy2 + ((((i - this.sReinforceItemIndex) + 4) - 1) * 31), Integer.parseInt(COLORS[equip2.data.bGrade], 16), 0);
                    if (equip2.data.bID < 26 && equip2.bReinforcedLevel > 0) {
                        Tool.drawString(g, " + " + ((int) equip2.bReinforcedLevel), xx2 + 32 + font.stringWidth(equip2.data.name), ((26 - FontHeight) >> 1) + yy2 + ((((i - this.sReinforceItemIndex) + 4) - 1) * 31), 0, -1, -1);
                    }
                    Tool.drawClipImage(g, this.iconBGImg[equip2.data.bGrade], xx2, yy2 + ((((i - this.sReinforceItemIndex) + 4) - 1) * 31), 0, 0, 26, 26);
                    Tool.drawClipImage(g, this.equipIconImg, xx2 + 2, yy2 + ((((i - this.sReinforceItemIndex) + 4) - 1) * 31), Integer.parseInt(equip2.data.iconIDInfo[1]) * 22, Integer.parseInt(equip2.data.iconIDInfo[0]) * 22, 22, 22);
                }
            }
            this.role.actionSet.drawFrameCycle(g, 0, Tool.countTimes, ((SCREEN_WIDTH - 320) >> 1) + DROP_POS_INTERVAL, 210);
            int xx3 = (SCREEN_WIDTH >> 1) + 110;
            Equip choosedEquip = (Equip) this.candidateItemV.elementAt(this.sReinforceItemIndex);
            if (!this.isReinforceOver) {
                Tool.drawNumReturnWidth(g, this.numImg[2], choosedEquip.iFinalHP, xx3, yy2, 4, 2, -2);
                int yy3 = yy2 + 24;
                Tool.drawNumReturnWidth(g, this.numImg[2], choosedEquip.iFinalAP, xx3, yy3, 4, 2, -2);
                int yy4 = yy3 + 24;
                Tool.drawNumReturnWidth(g, this.numImg[2], choosedEquip.iFinalDurability, xx3, yy4, 4, 2, -2);
                int yy5 = yy4 + 24;
                Tool.drawNumReturnWidth(g, this.numImg[2], choosedEquip.iFinalDP, xx3, yy5, 4, 2, -2);
                int yy6 = yy5 + 24;
                Tool.drawNumReturnWidth(g, this.numImg[2], choosedEquip.iHitPoint, xx3, yy6, 4, 2, -2);
                int yy7 = yy6 + 24;
                Tool.drawNumReturnWidth(g, this.numImg[2], choosedEquip.iDodgePoint, xx3, yy7, 4, 2, -2);
                Tool.drawNumReturnWidth(g, this.numImg[2], choosedEquip.iCriticalPoint, xx3, yy7 + 24, 4, 2, -2);
            } else {
                int hpUp = choosedEquip.iFinalHP - choosedEquip.iPreMaxHP;
                int durabilityUp = choosedEquip.iFinalDurability - choosedEquip.iPreDurability;
                int APUp = choosedEquip.iFinalAP - choosedEquip.iPreAP;
                int DPUp = choosedEquip.iFinalDP - choosedEquip.iPreDP;
                if (hpUp > 0) {
                    Tool.drawImage(g, this.arrowUpImg, xx3, yy2 - (Tool.countTimes2 % 2), 0);
                    Tool.drawNumReturnWidth(g, this.attributeUpNumImg, hpUp, xx3 + 18, yy2, 4, 0, -2);
                }
                Tool.drawNumReturnWidth(g, this.numImg[2], choosedEquip.iFinalHP, xx3 - 14, yy2, 4, 2, -2);
                int yy8 = yy2 + 24;
                if (APUp > 0) {
                    Tool.drawImage(g, this.arrowUpImg, xx3, yy8 - (Tool.countTimes2 % 2), 0);
                    Tool.drawNumReturnWidth(g, this.attributeUpNumImg, durabilityUp, xx3 + 18, yy8, 4, 0, -2);
                }
                Tool.drawNumReturnWidth(g, this.numImg[2], choosedEquip.iFinalAP, xx3 - 14, yy8, 4, 2, -2);
                int yy9 = yy8 + 24;
                if (durabilityUp > 0) {
                    Tool.drawImage(g, this.arrowUpImg, xx3, yy9 - (Tool.countTimes2 % 2), 0);
                    Tool.drawNumReturnWidth(g, this.attributeUpNumImg, APUp, xx3 + 18, yy9, 4, 0, -2);
                }
                Tool.drawNumReturnWidth(g, this.numImg[2], choosedEquip.iFinalDurability, xx3 - 14, yy9, 4, 2, -2);
                int yy10 = yy9 + 24;
                if (DPUp > 0) {
                    Tool.drawImage(g, this.arrowUpImg, xx3, yy10 - (Tool.countTimes2 % 2), 0);
                    Tool.drawNumReturnWidth(g, this.attributeUpNumImg, DPUp, xx3 + 18, yy10, 4, 0, -2);
                }
                Tool.drawNumReturnWidth(g, this.numImg[2], choosedEquip.iFinalDP, xx3 - 14, yy10, 4, 2, -2);
                int yy11 = yy10 + 24;
                Tool.drawNumReturnWidth(g, this.numImg[2], choosedEquip.iHitPoint, xx3 - 14, yy11, 4, 2, -2);
                int yy12 = yy11 + 24;
                Tool.drawNumReturnWidth(g, this.numImg[2], choosedEquip.iDodgePoint, xx3 - 14, yy12, 4, 2, -2);
                Tool.drawNumReturnWidth(g, this.numImg[2], choosedEquip.iCriticalPoint, xx3 - 14, yy12 + 24, 4, 2, -2);
            }
            int offX = ((SCREEN_WIDTH >> 1) - 68) - 127;
            int offY = ((SCREEN_HEIGHT >> 1) + 52) - 256;
            if (choosedEquip.skill != null) {
                int iconIndex = choosedEquip.skill.data.bID - 5;
                if (iconIndex == 6) {
                    iconIndex = 7;
                } else if (iconIndex == 7) {
                    iconIndex = 6;
                }
                Tool.drawClipImage(g, this.skillIconImg, offX + 127, offY + 258, iconIndex * 22, 0, 22, 22);
                String str = String.valueOf(choosedEquip.skill.name) + ((int) choosedEquip.skill.bLevel) + "级";
                Tool.drawString(g, choosedEquip.skill.name, offX + 152 + ((64 - font.stringWidth(str)) >> 1), ((offY + 265) + ((24 - FontHeight) >> 1)) - 11, 0, -1);
                Tool.drawString(g, new StringBuilder(String.valueOf((int) choosedEquip.skill.bLevel)).toString(), offX + 152 + ((64 - font.stringWidth(str)) >> 1) + font.stringWidth(choosedEquip.skill.name), ((offY + 265) + ((24 - FontHeight) >> 1)) - 11, 16711680, -1);
                Tool.drawString(g, "级", offX + 152 + ((64 - font.stringWidth(str)) >> 1) + font.stringWidth(String.valueOf(choosedEquip.skill.name) + ((int) choosedEquip.skill.bLevel)), ((offY + 265) + ((24 - FontHeight) >> 1)) - 11, 0, -1);
            } else {
                Tool.drawString(g, "无技能", offX + 152 + ((64 - font.stringWidth("无技能")) >> 1), ((offY + 265) + ((24 - FontHeight) >> 1)) - 11, 0, -1);
            }
            int xx4 = (SCREEN_WIDTH - 320) >> 1;
            int yy13 = (SCREEN_HEIGHT - 240) >> 1;
            if (this.role.equip == choosedEquip) {
                this.menuAS.drawFrame(g, 5, (SCREEN_WIDTH >> 1) - 191, (SCREEN_HEIGHT >> 1) + 35);
            }
            if (choosedEquip.iCurDurability == 0) {
                this.menuAS.drawFrame(g, 6, (SCREEN_WIDTH >> 1) - 191, (SCREEN_HEIGHT >> 1) + 35);
            }
        }
        if (this.isPopUpOption) {
            this.menuAS.drawFrameCycle(g, this.bPopUpOptionIndex + 32, Tool.countTimes, SCREEN_WIDTH >> 1, SCREEN_HEIGHT >> 1);
            int x = ((SCREEN_WIDTH - font.stringWidth(String.valueOf("强化费用：") + this.equipForReinforce.iReinforceMoney)) - (this.moneyImg.getWidth() >> 1)) >> 1;
            Tool.drawString(g, "强化费用：", x, 110, 0, -1);
            drawPrice(g, x + font.stringWidth("强化费用："), 110, this.equipForReinforce.iReinforceMoney, (byte) 0);
            String str2 = "成功率：" + ((int) this.equipForReinforce.bOdds[this.equipForReinforce.bReinforcedLevel]) + "%";
            Tool.drawString(g, str2, (SCREEN_WIDTH - font.stringWidth(str2)) >> 1, FontHeight + 85 + DROP_POS_INTERVAL, 0, -1);
        }
        drawMoney(g);
    }

    private void handleKey_Reinforce() {
        if (Common.isUpPressed(true)) {
            if (!this.isPopUpOption) {
                if (this.sReinforceItemIndex > 0) {
                    this.sReinforceItemIndex = (short) (this.sReinforceItemIndex - 1);
                    return;
                } else {
                    if (this.candidateItemV.size() > 0) {
                        this.sReinforceItemIndex = (short) (this.candidateItemV.size() - 1);
                        return;
                    }
                    return;
                }
            }
            return;
        }
        if (Common.isDownPressed(true)) {
            if (!this.isPopUpOption) {
                if (this.sReinforceItemIndex < this.candidateItemV.size() - 1) {
                    this.sReinforceItemIndex = (short) (this.sReinforceItemIndex + 1);
                    return;
                } else {
                    this.sReinforceItemIndex = (short) 0;
                    return;
                }
            }
            return;
        }
        if (Common.isLeftPressed(true)) {
            if (this.isPopUpOption) {
                this.bPopUpOptionIndex = (byte) (this.bPopUpOptionIndex == 0 ? 1 : 0);
                return;
            }
            return;
        }
        if (Common.isRightPressed(true)) {
            if (this.isPopUpOption) {
                this.bPopUpOptionIndex = (byte) (this.bPopUpOptionIndex != 0 ? 0 : 1);
                return;
            }
            return;
        }
        if (Common.isKeyPressed(9, true) || Common.isOKPressed(true)) {
            if (!this.isPopUpOption) {
                this.equipForReinforce = (Equip) this.candidateItemV.elementAt(this.sReinforceItemIndex);
                if (this.equipForReinforce.bReinforcedLevel == 9) {
                    script_showMessage("强化等级已达上限！", 0);
                    return;
                }
                if (this.bag.iMoney >= this.equipForReinforce.iReinforceMoney) {
                    this.isPopUpOption = true;
                    this.bPopUpOptionIndex = (byte) 0;
                    return;
                } else if (this.isOpenAllStage || this.isDX) {
                    script_showMessage("银两不足，可到商城购买。", 0);
                    return;
                } else {
                    script_showMessage("银两不足", 0);
                    return;
                }
            }
            if (this.bPopUpOptionIndex == 0) {
                this.equipForReinforce.reinforce();
                if (this.equipForReinforce.isReinforceSuccess) {
                    this.bag.iMoney -= this.equipForReinforce.iReinforceMoney;
                    this.isReinforceOver = true;
                    script_showMessage("强化成功！", 0);
                } else {
                    script_showMessage("强化失败！强化等级降为“+" + ((int) this.equipForReinforce.bReinforcedLevel) + "”", 0);
                }
            }
            this.isPopUpOption = false;
            return;
        }
        if (Common.isKeyPressed(10, true)) {
            if (this.isPopUpOption) {
                this.isPopUpOption = false;
            } else {
                releaseReinforceRes();
                this.bGameState = (byte) 12;
            }
        }
    }

    void initShop(int index) {
        this.shopItems = new Vector(DROP_POS_INTERVAL);
        if (index < 2) {
            for (int i = 0; i < propDatas.length; i++) {
                if (propDatas[i].bID != 14 && propDatas[i].bID != 15 && (this.role.famulus == null || this.role.famulus.sLevel != 40 || (propDatas[i].bID != 11 && propDatas[i].bID != 12 && propDatas[i].bID != 13))) {
                    this.shopItems.addElement(new Prop(propDatas[i], 1));
                }
            }
            this.sItemIndex = (short) 0;
        } else if (index == 2) {
            this.saleVector = new Vector(DROP_POS_INTERVAL);
            for (int i2 = 0; i2 < equipDatas.length; i2++) {
                if (equipDatas[i2].bGrade < 3 && equipDatas[i2].bLevel <= this.role.sLevel) {
                    this.shopItems.addElement(new Equip(equipDatas[i2], 0, false));
                }
            }
            for (int i3 = 0; i3 < this.bag.equipV.size(); i3++) {
                Equip equip = (Equip) this.bag.equipV.elementAt(i3);
                if (equip != this.role.equip) {
                    this.saleVector.addElement(equip);
                }
            }
            this.sItemIndex = (short) 0;
            this.sEquipIndex = (short) 0;
        }
        this.bShopType = (byte) index;
        loadUIAction();
        if (this.equipIconImg == null) {
            this.equipIconImg = Tool.loadPImage("/pic/weapon.p");
        }
        this.isPopUpOption = false;
        this.sShopIndex = (short) 0;
        getShopInfo();
        setGameState((byte) 15);
    }

    void releaseShopRes() {
        cleanUpUIAction();
        this.equipIconImg = null;
    }

    void drawShopUI(Graphics g) {
        if (this.isSale) {
            this.menuAS.drawFrameCycle(g, 14, Tool.countTimes, SCREEN_WIDTH >> 1, SCREEN_HEIGHT >> 1);
        } else {
            this.menuAS.drawFrameCycle(g, 13, Tool.countTimes, SCREEN_WIDTH >> 1, SCREEN_HEIGHT >> 1);
        }
        int offX = (SCREEN_WIDTH - 320) >> 1;
        int offY = (SCREEN_HEIGHT - 240) >> 1;
        g.setColor(198, 181, 142);
        if (this.isSale) {
            if (this.bShopType < 2) {
                if (this.sItemIndex < 5) {
                    g.fillRect(offX + 22, offY + 48 + (this.sItemIndex * 31), 187, 26);
                } else {
                    g.fillRect(offX + 22, offY + 48 + 124, 187, 26);
                }
                for (int i = 0; i < this.bag.propV.size(); i++) {
                    Prop prop = (Prop) this.bag.propV.elementAt(i);
                    if (this.sItemIndex < 5) {
                        if (i < 5) {
                            Tool.drawString(g, prop.data.name, offX + 52, (i * 31) + offY + 50 + ((26 - FontHeight) >> 1), Integer.parseInt(COLORS[prop.data.bType], 16), 0);
                            drawPrice(g, offX + 208, (i * 31) + offY + 50 + ((26 - FontHeight) >> 1), prop.data.iSellPrice, (byte) 1);
                            Tool.drawClipImage(g, this.iconBGImg[prop.data.bType], offX + 23, (i * 31) + offY + 48, 0, 0, 26, 26);
                            Tool.drawClipImage(g, this.propIconImg, offX + 25, (i * 31) + offY + 50, (prop.data.bID == 23 ? (byte) 8 : prop.data.bID) * 22, 0, 22, 22);
                            Tool.drawNumReturnWidth(g, this.numImg[6], prop.number, offX + 40, (i * 31) + offY + 65, 2, 0, -1);
                        }
                    } else if (i > this.sItemIndex - 5 && i <= this.sItemIndex) {
                        Tool.drawString(g, prop.data.name, offX + 52, ((((i - this.sItemIndex) + 5) - 1) * 31) + offY + 50 + ((26 - FontHeight) >> 1), Integer.parseInt(COLORS[prop.data.bType], 16), 0);
                        drawPrice(g, offX + 208, ((((i - this.sItemIndex) + 5) - 1) * 31) + offY + 50 + ((26 - FontHeight) >> 1), prop.data.iSellPrice, (byte) 1);
                        Tool.drawClipImage(g, this.iconBGImg[prop.data.bType], offX + 23, ((((i - this.sItemIndex) + 5) - 1) * 31) + offY + 48, 0, 0, 26, 26);
                        Tool.drawClipImage(g, this.propIconImg, offX + 25, ((((i - this.sItemIndex) + 5) - 1) * 31) + offY + 50, (prop.data.bID == 23 ? (byte) 8 : prop.data.bID) * 22, 0, 22, 22);
                        Tool.drawNumReturnWidth(g, this.numImg[6], prop.number, offX + 40, ((((i - this.sItemIndex) + 5) - 1) * 31) + offY + 65, 2, 0, -1);
                    }
                }
            } else {
                if (this.sEquipIndex < 5) {
                    g.fillRect(offX + 22, offY + 48 + (this.sEquipIndex * 31), 187, 26);
                } else {
                    g.fillRect(offX + 22, offY + 48 + 124, 187, 26);
                }
                for (int i2 = 0; i2 < this.saleVector.size(); i2++) {
                    Equip equip = (Equip) this.saleVector.elementAt(i2);
                    if (this.sEquipIndex < 5) {
                        if (i2 < 5) {
                            Tool.drawString(g, equip.data.name, offX + 52, (i2 * 31) + offY + 50 + ((26 - FontHeight) >> 1), Integer.parseInt(COLORS[equip.data.bGrade], 16), 0);
                            drawPrice(g, offX + 208, (i2 * 31) + offY + 50 + ((26 - FontHeight) >> 1), equip.data.iSellPrice, (byte) 1);
                            Tool.drawClipImage(g, this.iconBGImg[equip.data.bGrade], offX + 23, (i2 * 31) + offY + 48, 0, 0, 26, 26);
                            Tool.drawClipImage(g, this.equipIconImg, offX + 25, (i2 * 31) + offY + 50, Integer.parseInt(equip.data.iconIDInfo[1]) * 22, Integer.parseInt(equip.data.iconIDInfo[0]) * 22, 22, 22);
                        }
                    } else if (i2 > this.sEquipIndex - 5 && i2 <= this.sEquipIndex) {
                        Tool.drawString(g, equip.data.name, offX + 52, ((((i2 - this.sEquipIndex) + 5) - 1) * 31) + offY + 50 + ((26 - FontHeight) >> 1), Integer.parseInt(COLORS[equip.data.bGrade], 16), 0);
                        drawPrice(g, offX + 208, ((((i2 - this.sEquipIndex) + 5) - 1) * 31) + offY + 50 + ((26 - FontHeight) >> 1), equip.data.iSellPrice, (byte) 1);
                        Tool.drawClipImage(g, this.iconBGImg[equip.data.bGrade], offX + 23, ((((i2 - this.sEquipIndex) + 5) - 1) * 31) + offY + 48, 0, 0, 26, 26);
                        Tool.drawClipImage(g, this.equipIconImg, offX + 25, ((((i2 - this.sEquipIndex) + 5) - 1) * 31) + offY + 50, Integer.parseInt(equip.data.iconIDInfo[1]) * 22, Integer.parseInt(equip.data.iconIDInfo[0]) * 22, 22, 22);
                    }
                }
            }
            drawScrollItem(g, this.scrollSignImg, offX + 216, offY + 54, 139, this.bShopType < 2 ? this.sItemIndex : this.sEquipIndex, this.bShopType < 2 ? this.bag.propV.size() : this.saleVector.size());
        } else {
            if (this.sShopIndex < 5) {
                g.fillRect(offX + 22, offY + 48 + (this.sShopIndex * 31), 187, 26);
            } else {
                g.fillRect(offX + 22, offY + 48 + 124, 187, 26);
            }
            for (int i3 = 0; i3 < this.shopItems.size(); i3++) {
                if (this.bShopType < 2) {
                    Prop prop2 = (Prop) this.shopItems.elementAt(i3);
                    int price = prop2.data.iPurchasePrice;
                    if (prop2.data.bID > 15 && prop2.data.bID < 23 && (price = price + (this.iSpecialPropPriceCoefficient[prop2.data.bID - 16] * WebNetEvent.GotoWeb_Event_OK)) > 20000) {
                        price = 20000;
                    }
                    if (this.bShopType == 0) {
                        price <<= 1;
                    }
                    if (this.sShopIndex < 5) {
                        if (i3 < 5) {
                            Tool.drawString(g, prop2.data.name, offX + 52, (i3 * 31) + offY + 50 + ((26 - FontHeight) >> 1), Integer.parseInt(COLORS[prop2.data.bType], 16), 0);
                            drawPrice(g, offX + 208, (i3 * 31) + offY + 50 + ((26 - FontHeight) >> 1), price, (byte) 1);
                            Tool.drawClipImage(g, this.iconBGImg[prop2.data.bType], offX + 23, (i3 * 31) + offY + 48, 0, 0, 26, 26);
                            Tool.drawClipImage(g, this.propIconImg, offX + 25, (i3 * 31) + offY + 50, (prop2.data.bID == 23 ? (byte) 8 : prop2.data.bID) * 22, 0, 22, 22);
                        }
                    } else if (i3 > this.sShopIndex - 5 && i3 <= this.sShopIndex) {
                        Tool.drawString(g, prop2.data.name, offX + 52, ((((i3 - this.sShopIndex) + 5) - 1) * 31) + offY + 50 + ((26 - FontHeight) >> 1), Integer.parseInt(COLORS[prop2.data.bType], 16), 0);
                        drawPrice(g, offX + 208, ((((i3 - this.sShopIndex) + 5) - 1) * 31) + offY + 50 + ((26 - FontHeight) >> 1), price, (byte) 1);
                        Tool.drawClipImage(g, this.iconBGImg[prop2.data.bType], offX + 23, ((((i3 - this.sShopIndex) + 5) - 1) * 31) + offY + 48, 0, 0, 26, 26);
                        Tool.drawClipImage(g, this.propIconImg, offX + 25, ((((i3 - this.sShopIndex) + 5) - 1) * 31) + offY + 50, (prop2.data.bID == 23 ? (byte) 8 : prop2.data.bID) * 22, 0, 22, 22);
                    }
                } else {
                    if (Tool.countTimes % 10 < 6) {
                        this.menuAS.drawFrame(g, 2, (SCREEN_WIDTH >> 1) + 40, (SCREEN_HEIGHT >> 1) - 100);
                    }
                    Equip equip2 = (Equip) this.shopItems.elementAt(i3);
                    if (this.sShopIndex < 5) {
                        if (i3 < 5) {
                            Tool.drawString(g, equip2.data.name, offX + 52, (i3 * 31) + offY + 50 + ((26 - FontHeight) >> 1), Integer.parseInt(COLORS[equip2.data.bGrade], 16), 0);
                            drawPrice(g, offX + 208, (i3 * 31) + offY + 50 + ((26 - FontHeight) >> 1), equip2.data.iPurchasePrice, (byte) 1);
                            Tool.drawClipImage(g, this.iconBGImg[equip2.data.bGrade], offX + 23, (i3 * 31) + offY + 48, 0, 0, 26, 26);
                            Tool.drawClipImage(g, this.equipIconImg, offX + 25, (i3 * 31) + offY + 50, Integer.parseInt(equip2.data.iconIDInfo[1]) * 22, Integer.parseInt(equip2.data.iconIDInfo[0]) * 22, 22, 22);
                        }
                    } else if (i3 > this.sShopIndex - 5 && i3 <= this.sShopIndex) {
                        Tool.drawString(g, equip2.data.name, offX + 52, ((((i3 - this.sShopIndex) + 5) - 1) * 31) + offY + 50 + ((26 - FontHeight) >> 1), Integer.parseInt(COLORS[equip2.data.bGrade], 16), 0);
                        drawPrice(g, offX + 208, ((((i3 - this.sShopIndex) + 5) - 1) * 31) + offY + 50 + ((26 - FontHeight) >> 1), equip2.data.iPurchasePrice, (byte) 1);
                        Tool.drawClipImage(g, this.iconBGImg[equip2.data.bGrade], offX + 23, ((((i3 - this.sShopIndex) + 5) - 1) * 31) + offY + 48, 0, 0, 26, 26);
                        Tool.drawClipImage(g, this.equipIconImg, offX + 25, ((((i3 - this.sShopIndex) + 5) - 1) * 31) + offY + 50, Integer.parseInt(equip2.data.iconIDInfo[1]) * 22, Integer.parseInt(equip2.data.iconIDInfo[0]) * 22, 22, 22);
                    }
                }
            }
            drawScrollItem(g, this.scrollSignImg, offX + 216, offY + 54, 139, this.sShopIndex, this.shopItems.size());
        }
        drawMoney(g);
        if (this.UIST != null) {
            this.UIST.draw(g, this.iUIInfoX, this.iUIInfoY);
        }
        if (this.isPopUpOption) {
            if (this.isSale) {
                this.menuAS.drawFrameCycle(g, this.bPopUpOptionIndex + 19, Tool.countTimes, SCREEN_WIDTH >> 1, SCREEN_HEIGHT >> 1);
            } else {
                this.menuAS.drawFrameCycle(g, this.bPopUpOptionIndex + 17, Tool.countTimes, SCREEN_WIDTH >> 1, SCREEN_HEIGHT >> 1);
            }
            if (this.bShopType < 2) {
                if (((Prop) this.shopItems.elementAt(this.sShopIndex)).data.bID < 16) {
                    Tool.drawString(g, "数量：", (SCREEN_WIDTH >> 1) - font.stringWidth("数量："), this.arrowUpImg.getHeight() + 75 + 20, -1, 0);
                    Tool.drawString(g, new StringBuilder().append(this.iTradeNum).toString(), (SCREEN_WIDTH >> 1) + 5, this.arrowUpImg.getHeight() + 75 + 20, 0, -1);
                    Tool.drawImage(g, this.arrowUpImg, (SCREEN_WIDTH >> 1) + 5 + ((font.stringWidth(new StringBuilder().append(this.iTradeNum).toString()) - this.arrowUpImg.getWidth()) >> 1), (75 - (Tool.countTimes2 % 2)) + 20, 0);
                    Tool.drawImage(g, this.arrowUpImg, (SCREEN_WIDTH >> 1) + 5 + ((font.stringWidth(new StringBuilder().append(this.iTradeNum).toString()) - this.arrowUpImg.getWidth()) >> 1), FontHeight + 75 + this.arrowUpImg.getHeight() + (Tool.countTimes2 % 2) + 20, 1);
                } else {
                    Tool.drawString(g, "每次限购1个，是否购买？", (SCREEN_WIDTH - font.stringWidth("每次限购1个，是否购买？")) >> 1, (this.arrowUpImg.getHeight() << 1) + 75 + 20, -1, 0);
                }
                Tool.drawString(g, "总额：", (SCREEN_WIDTH >> 1) - font.stringWidth("总额："), (this.arrowUpImg.getHeight() << 1) + 75 + FontHeight + 3 + 20, -1, 0);
                drawPrice(g, (SCREEN_WIDTH >> 1) + 5, (this.arrowUpImg.getHeight() << 1) + 75 + FontHeight + 3 + 20, this.iTransactionValue, (byte) 0);
            } else {
                Tool.drawString(g, "是否购买武器？", (SCREEN_WIDTH - font.stringWidth("是否购买武器？")) >> 1, (this.arrowUpImg.getHeight() << 1) + 75 + 20, -1, 0);
                Tool.drawString(g, "总额：", (SCREEN_WIDTH >> 1) - font.stringWidth("总额："), (this.arrowUpImg.getHeight() << 1) + 75 + FontHeight + 3 + 20, -1, 0);
                drawPrice(g, (SCREEN_WIDTH >> 1) + 5, (this.arrowUpImg.getHeight() << 1) + 75 + FontHeight + 3 + 20, this.iTransactionValue, (byte) 0);
            }
        }
        if (this.scrollInfoState > 0) {
            drawScrollInfo(g);
        }
    }

    private void drawPrice(Graphics g, int x, int y1, int iTransactionValue, byte alignment) {
        int gold = iTransactionValue / 10000;
        int silver = iTransactionValue % 10000;
        int unitWidth = this.moneyImg.getWidth() >> 1;
        int y = y1 + ((FontHeight - this.numImg[0].getHeight()) >> 1);
        if (alignment == 0) {
            if (gold > 0) {
                int goldNumWidth = Tool.drawNumReturnWidth(g, this.numImg[0], gold, x, y, 4, 0, -1);
                Tool.drawClipImage(g, this.moneyImg, x + goldNumWidth + 2, y, 0, 0, unitWidth, this.moneyImg.getHeight());
                if (silver > 0) {
                    Tool.drawClipImage(g, this.moneyImg, x + goldNumWidth + Tool.drawNumReturnWidth(g, this.numImg[1], silver, x + goldNumWidth + 4 + unitWidth, y, 4, 0, -1) + 6 + unitWidth, y, unitWidth, 0, unitWidth, this.moneyImg.getHeight());
                    return;
                }
                return;
            }
            Tool.drawClipImage(g, this.moneyImg, x + Tool.drawNumReturnWidth(g, this.numImg[1], silver, x, y, 4, 0, -1) + 2, y, unitWidth, 0, unitWidth, this.moneyImg.getHeight());
            return;
        }
        if (alignment == 1) {
            if (silver > 0) {
                Tool.drawClipImage(g, this.moneyImg, (x - 2) - unitWidth, y, unitWidth, 0, unitWidth, this.moneyImg.getHeight());
                int silverNumWidth = Tool.drawNumReturnWidth(g, this.numImg[1], silver, (x - 4) - unitWidth, y, 4, 1, -1);
                if (gold > 0) {
                    Tool.drawClipImage(g, this.moneyImg, ((x - silverNumWidth) - 6) - (unitWidth << 1), y, 0, 0, unitWidth, this.moneyImg.getHeight());
                    Tool.drawNumReturnWidth(g, this.numImg[0], gold, ((x - silverNumWidth) - 8) - (unitWidth << 1), y, 4, 1, -1);
                    return;
                }
                return;
            }
            if (gold > 0) {
                Tool.drawClipImage(g, this.moneyImg, (x - 2) - unitWidth, y, 0, 0, unitWidth, this.moneyImg.getHeight());
                Tool.drawNumReturnWidth(g, this.numImg[0], gold, (x - 4) - unitWidth, y, 4, 1, -1);
            }
        }
    }

    private void handleKey_Shop() {
        if (!this.isPopUpOption) {
            if (Common.isLeftPressed(true)) {
                if (this.isSale) {
                    this.isSale = false;
                    getShopInfo();
                    return;
                }
                return;
            }
            if (Common.isRightPressed(true)) {
                if (!this.isSale) {
                    if (this.bShopType < 2) {
                        if (this.bag.propV.size() > 0) {
                            this.isSale = true;
                            getPropInfo();
                            return;
                        } else {
                            script_showMessage("无可出售物品", 0);
                            return;
                        }
                    }
                    if (this.saleVector.size() > 0) {
                        this.isSale = true;
                        getEquipInfo();
                        return;
                    } else {
                        script_showMessage("无可出售物品", 0);
                        return;
                    }
                }
                return;
            }
            if (Common.isUpPressed(true)) {
                if (!this.isSale) {
                    if (this.sShopIndex > 0) {
                        this.sShopIndex = (short) (this.sShopIndex - 1);
                    } else {
                        this.sShopIndex = (short) (this.shopItems.size() - 1);
                    }
                    getShopInfo();
                    return;
                }
                if (this.bShopType < 2) {
                    this.sItemIndex = (short) (this.sItemIndex - 1);
                    if (this.sItemIndex < 0) {
                        this.sItemIndex = (short) (this.bag.propV.size() - 1);
                    }
                    getPropInfo();
                    return;
                }
                this.sEquipIndex = (short) (this.sEquipIndex - 1);
                if (this.sEquipIndex < 0) {
                    this.sEquipIndex = (short) (this.saleVector.size() - 1);
                }
                getEquipInfo();
                return;
            }
            if (Common.isDownPressed(true)) {
                if (!this.isSale) {
                    if (this.sShopIndex < this.shopItems.size() - 1) {
                        this.sShopIndex = (short) (this.sShopIndex + 1);
                    } else {
                        this.sShopIndex = (short) 0;
                    }
                    getShopInfo();
                    return;
                }
                if (this.bShopType < 2) {
                    if (this.sItemIndex < this.bag.propV.size() - 1) {
                        this.sItemIndex = (short) (this.sItemIndex + 1);
                    } else {
                        this.sItemIndex = (short) 0;
                    }
                    getPropInfo();
                    return;
                }
                this.sEquipIndex = (short) (this.sEquipIndex + 1);
                if (this.sEquipIndex == this.saleVector.size()) {
                    this.sEquipIndex = (short) 0;
                }
                getEquipInfo();
                return;
            }
            if (Common.isKeyPressed(9, true) || Common.isOKPressed(true)) {
                if (!this.isSale) {
                    this.isPopUpOption = true;
                    this.bPopUpOptionIndex = (byte) 0;
                    if (this.bShopType < 2) {
                        Prop tradeProp = (Prop) this.shopItems.elementAt(this.sShopIndex);
                        this.iTradeNum = 1;
                        this.iTransactionValue = tradeProp.data.iPurchasePrice;
                        if (tradeProp.data.bID > 15 && tradeProp.data.bID < 23) {
                            this.iTransactionValue += this.iSpecialPropPriceCoefficient[tradeProp.data.bID - 16] * WebNetEvent.GotoWeb_Event_OK;
                        }
                        if (this.bShopType == 0) {
                            this.iTransactionValue <<= 1;
                            return;
                        }
                        return;
                    }
                    Equip tradeEquip = (Equip) this.shopItems.elementAt(this.sShopIndex);
                    this.iTransactionValue = tradeEquip.data.iPurchasePrice;
                    return;
                }
                if (this.bShopType < 2) {
                    if (this.bag.propV.size() > 0) {
                        this.isPopUpOption = true;
                        this.bPopUpOptionIndex = (byte) 0;
                        this.iTradeNum = 1;
                        this.iTransactionValue = ((Prop) this.bag.propV.elementAt(this.sItemIndex)).data.iSellPrice;
                        return;
                    }
                    return;
                }
                if (this.saleVector.size() > 0) {
                    this.isPopUpOption = true;
                    this.bPopUpOptionIndex = (byte) 0;
                    this.iTransactionValue = ((Equip) this.saleVector.elementAt(this.sEquipIndex)).data.iSellPrice;
                    return;
                }
                return;
            }
            if (Common.isKeyPressed(10, true)) {
                this.isSale = false;
                this.isPopUpOption = false;
                this.UIST = null;
                if (this.bShopType != 0) {
                    releaseShopRes();
                    this.bGameState = (byte) 12;
                    return;
                } else {
                    this.bGameState = (byte) 18;
                    return;
                }
            }
            if (Common.isKeyPressed(18, true)) {
                this.previewEquip = (Equip) this.shopItems.elementAt(this.sShopIndex);
                String[] temp = Tool.getStringArray(this.previewEquip.data.resStr, ',');
                this.role.reload(temp, 4);
                getPreviewInfo();
                this.bGameState = (byte) 14;
                this.bEntryIndex = (byte) 1;
                return;
            }
            return;
        }
        if (Common.isUpPressed(false)) {
            if (this.bShopType < 2) {
                if (this.isSale) {
                    this.iTradeNum--;
                    Prop tradeProp2 = (Prop) this.bag.propV.elementAt(this.sItemIndex);
                    if (this.iTradeNum < 1) {
                        this.iTradeNum = tradeProp2.number;
                    }
                    this.iTransactionValue = this.iTradeNum * tradeProp2.data.iSellPrice;
                    return;
                }
                Prop tradeProp3 = (Prop) this.shopItems.elementAt(this.sShopIndex);
                if (tradeProp3.data.bID < 16) {
                    this.iTradeNum--;
                    if (this.iTradeNum < 1) {
                        this.iTradeNum = 99;
                    }
                    this.iTransactionValue = this.iTradeNum * tradeProp3.data.iPurchasePrice;
                    if (this.bShopType == 0) {
                        this.iTransactionValue <<= 1;
                        return;
                    }
                    return;
                }
                return;
            }
            return;
        }
        if (Common.isDownPressed(false)) {
            if (this.bShopType < 2) {
                if (this.isSale) {
                    Prop tradeProp4 = (Prop) this.bag.propV.elementAt(this.sItemIndex);
                    this.iTradeNum++;
                    if (this.iTradeNum > tradeProp4.number) {
                        this.iTradeNum = 1;
                    }
                    this.iTransactionValue = this.iTradeNum * tradeProp4.data.iSellPrice;
                    return;
                }
                Prop tradeProp5 = (Prop) this.shopItems.elementAt(this.sShopIndex);
                if (tradeProp5.data.bID < 16) {
                    this.iTradeNum++;
                    if (this.iTradeNum > 99) {
                        this.iTradeNum = 1;
                    }
                    this.iTransactionValue = this.iTradeNum * tradeProp5.data.iPurchasePrice;
                    if (this.bShopType == 0) {
                        this.iTransactionValue <<= 1;
                        return;
                    }
                    return;
                }
                return;
            }
            return;
        }
        if (!Common.isLeftPressed(true)) {
            if (!Common.isRightPressed(true)) {
                if (Common.isKeyPressed(9, true) || Common.isOKPressed(true)) {
                    if (this.bPopUpOptionIndex == 0) {
                        if (this.isSale) {
                            if (this.bShopType < 2) {
                                this.bag.deleteProp((Prop) this.bag.propV.elementAt(this.sItemIndex), this.iTradeNum);
                                getMoney(this.iTransactionValue);
                                if (this.sItemIndex > this.bag.propV.size() - 1 && this.sItemIndex > 0) {
                                    this.sItemIndex = (short) (this.bag.propV.size() - 1);
                                    getPropInfo();
                                }
                            } else {
                                Equip equip = (Equip) this.saleVector.elementAt(this.sEquipIndex);
                                this.bag.deleteEquip(equip);
                                getMoney(equip.data.iSellPrice);
                                this.saleVector.removeElement(equip);
                                if (this.sEquipIndex > this.saleVector.size() - 1 && this.sEquipIndex > 0) {
                                    this.sEquipIndex = (short) (this.saleVector.size() - 1);
                                    getEquipInfo();
                                }
                            }
                        } else if (this.bShopType < 2) {
                            Prop tradeProp6 = (Prop) this.shopItems.elementAt(this.sShopIndex);
                            if (this.bag.iMoney > this.iTransactionValue) {
                                if (tradeProp6.data.bID < 11 || tradeProp6.data.bID == 23) {
                                    this.bag.addProp(tradeProp6.data, this.iTradeNum);
                                    this.bag.iMoney -= this.iTransactionValue;
                                } else if (tradeProp6.data.bID > 15 && tradeProp6.data.bID < 23) {
                                    this.bag.addProp(tradeProp6.data, 1);
                                    useProp(tradeProp6);
                                    if (this.iSpecialPropPriceCoefficient[tradeProp6.data.bID - 16] < 19) {
                                        int[] iArr = this.iSpecialPropPriceCoefficient;
                                        int i = tradeProp6.data.bID - 16;
                                        iArr[i] = iArr[i] + 1;
                                    }
                                    this.bag.iMoney -= this.iTransactionValue;
                                } else if (this.role.famulus == null) {
                                    script_showMessage("获得剑灵后开启!", 0);
                                } else {
                                    short s = this.role.famulus.sLevel;
                                    this.role.famulus.getClass();
                                    if (s < 40) {
                                        Prop prop = new Prop(tradeProp6.data, this.iTradeNum);
                                        useProp(prop);
                                        this.bag.iMoney -= this.iTransactionValue;
                                    } else {
                                        script_showMessage("剑灵等级已达上限!", 0);
                                    }
                                }
                            } else if (this.isOpenAllStage || this.isDX) {
                                script_showMessage("银两不足，可到商城购买", 0);
                            } else {
                                script_showMessage("银两不足！", 0);
                            }
                        } else {
                            Equip equip2 = new Equip(((Equip) this.shopItems.elementAt(this.sShopIndex)).data, 0, false);
                            if (this.bag.iMoney > equip2.data.iPurchasePrice) {
                                this.bag.iMoney -= equip2.data.iPurchasePrice;
                                equip2.skill = equip2.getSkill();
                                this.bag.addEquip(equip2);
                                this.saleVector.addElement(equip2);
                            } else if (this.isOpenAllStage || this.isDX) {
                                script_showMessage("银两不足，可到商城购买", 0);
                            } else {
                                script_showMessage("银两不足！", 0);
                            }
                        }
                    }
                    this.isPopUpOption = false;
                    return;
                }
                if (Common.isKeyPressed(10, true)) {
                    this.isPopUpOption = false;
                    return;
                }
                return;
            }
            this.bPopUpOptionIndex = (byte) (this.bPopUpOptionIndex != 0 ? 0 : 1);
            return;
        }
        this.bPopUpOptionIndex = (byte) (this.bPopUpOptionIndex == 0 ? 1 : 0);
    }

    void drawEquipPreviewUI(Graphics g) {
        int xx = SCREEN_WIDTH >> 1;
        int yy = (SCREEN_HEIGHT - 240) >> 1;
        this.menuAS.drawFrameCycle(g, 34, Tool.countTimes, SCREEN_WIDTH >> 1, SCREEN_HEIGHT >> 1);
        g.setColor(8873815);
        int xx2 = xx - 73;
        int yy2 = yy + 43;
        g.fillRect(xx2, yy2, 107, 26);
        int xx3 = xx2 + DROP_POS_INTERVAL;
        int yy3 = yy2 + 2;
        Tool.drawString(g, this.previewEquip.data.name, xx3, yy3 + ((26 - FontHeight) >> 1), Integer.parseInt(COLORS[this.previewEquip.data.bGrade], 16), 0);
        if (this.previewEquip.data.bID < 26 && this.previewEquip.bReinforcedLevel > 0) {
            Tool.drawString(g, " + " + ((int) this.previewEquip.bReinforcedLevel), xx3 + font.stringWidth(this.previewEquip.data.name), yy3 + ((26 - FontHeight) >> 1), 0, -1, -1);
        }
        Tool.drawClipImage(g, this.iconBGImg[this.previewEquip.data.bGrade], xx3 - 30, yy3 - 2, 0, 0, 26, 26);
        Tool.drawClipImage(g, this.equipIconImg, xx3 - 28, yy3, Integer.parseInt(this.previewEquip.data.iconIDInfo[1]) * 22, Integer.parseInt(this.previewEquip.data.iconIDInfo[0]) * 22, 22, 22);
        int xx4 = (SCREEN_WIDTH >> 1) + 110;
        int yy4 = (SCREEN_HEIGHT - 240) >> 1;
        int yy5 = yy4 + 42;
        Tool.drawNumReturnWidth(g, this.numImg[2], this.previewEquip.iFinalHP, xx4, yy5, 4, 2, -2);
        int yy6 = yy5 + 24;
        Tool.drawNumReturnWidth(g, this.numImg[2], this.previewEquip.iFinalAP, xx4, yy6, 4, 2, -2);
        int yy7 = yy6 + 24;
        Tool.drawNumReturnWidth(g, this.numImg[2], this.previewEquip.iFinalDurability, xx4, yy7, 4, 2, -2);
        int yy8 = yy7 + 24;
        Tool.drawNumReturnWidth(g, this.numImg[2], this.previewEquip.iFinalDP, xx4, yy8, 4, 2, -2);
        int yy9 = yy8 + 24;
        Tool.drawNumReturnWidth(g, this.numImg[2], this.previewEquip.iHitPoint, xx4, yy9, 4, 2, -2);
        int yy10 = yy9 + 24;
        Tool.drawNumReturnWidth(g, this.numImg[2], this.previewEquip.iDodgePoint, xx4, yy10, 4, 2, -2);
        Tool.drawNumReturnWidth(g, this.numImg[2], this.previewEquip.iCriticalPoint, xx4, yy10 + 24, 4, 2, -2);
        int xx5 = SCREEN_WIDTH >> 1;
        int yy11 = SCREEN_HEIGHT >> 1;
        this.role.actionSet.drawFrameCycle(g, 0, Tool.countTimes, xx5 - 120, yy11 + DROP_POS_INTERVAL);
        int xx6 = xx5 - 67;
        int yy12 = yy11 + 53;
        if (this.previewEquip.data.bSkillIndex >= 0) {
            int iconIndex = this.previewEquip.data.bSkillIndex - 5;
            if (iconIndex == 6) {
                iconIndex = 7;
            } else if (iconIndex == 7) {
                iconIndex = 6;
            }
            Tool.drawClipImage(g, this.skillIconImg, xx6, yy12, iconIndex * 22, 0, 22, 22);
            Skill skill = new Skill(skillDatas[this.previewEquip.data.bSkillIndex], this.previewEquip.data.bLevel);
            String str = String.valueOf(skill.name) + ((int) skill.bLevel) + "级";
            Tool.drawString(g, skill.name, xx6 + 25 + ((64 - font.stringWidth(str)) >> 1), yy12 + ((24 - FontHeight) >> 1), 0, -1);
            Tool.drawString(g, new StringBuilder(String.valueOf((int) skill.bLevel)).toString(), xx6 + 25 + ((64 - font.stringWidth(str)) >> 1) + font.stringWidth(skill.name), yy12 + ((24 - FontHeight) >> 1), 16711680, -1);
            Tool.drawString(g, "级", xx6 + 25 + ((64 - font.stringWidth(str)) >> 1) + font.stringWidth(String.valueOf(skill.name) + ((int) skill.bLevel)), yy12 + ((24 - FontHeight) >> 1), 0, -1);
        } else if (this.previewEquip.data.bSkillIndex == -1) {
            Tool.drawClipImage(g, this.skillIconImg, xx6, yy12, (Tool.countTimes2 % 17) * 22, 0, 22, 22);
            Tool.drawString(g, "随机技能", xx6 + 25 + ((64 - font.stringWidth("随机技能")) >> 1), yy12 + ((24 - FontHeight) >> 1), 0, -1);
        } else {
            Tool.drawString(g, "无技能", xx6 + 25 + ((64 - font.stringWidth("无技能")) >> 1), yy12 + ((24 - FontHeight) >> 1), 0, -1);
        }
        if (this.UIST != null) {
            this.UIST.draw(g, this.iUIInfoX, this.iUIInfoY);
        }
        drawMoney(g);
    }

    void handleKey_Preview() {
        if (Common.isKeyPressed(9, true) || Common.isOKPressed(true)) {
            if (this.bEntryIndex == 1) {
                this.bEntryIndex = (byte) 0;
                this.previewEquip = null;
                if (this.isAttackable) {
                    this.role.reload(this.role.weaponStr, 4);
                } else {
                    this.role.actionSet.images[4] = null;
                }
                getShopInfo();
                this.bGameState = (byte) 15;
                return;
            }
            if (!this.isFree) {
                initSMS(4);
                return;
            } else {
                exitEquipPreview();
                return;
            }
        }
        if (Common.isKeyPressed(10, true)) {
            if (this.bEntryIndex == 1) {
                this.bEntryIndex = (byte) 0;
                this.previewEquip = null;
                if (this.isAttackable) {
                    this.role.reload(this.role.weaponStr, 4);
                } else {
                    this.role.actionSet.images[4] = null;
                }
                getShopInfo();
                this.bGameState = (byte) 15;
                return;
            }
            exitEquipPreview();
        }
    }

    void exitEquipPreview() {
        setScriptState((byte) 3);
        this.previewEquip = null;
        if (this.isAttackable) {
            this.role.reload(this.role.weaponStr, 4);
        } else {
            this.role.actionSet.images[4] = null;
        }
        if (this.bEntryIndex == 2) {
            this.bEntryIndex = (byte) 0;
            this.bGameState = (byte) 31;
        } else {
            this.bGameState = (byte) 12;
        }
        cleanUpUIAction();
        this.equipIconImg = null;
    }

    void drawTalk(Graphics g) {
        if (this.talkST != null) {
            int sh = this.talkST.getHeight() + 8;
            int sy = SCREEN_HEIGHT - sh;
            int imgW = this.talkImg.getWidth();
            int imgH = this.talkImg.getHeight();
            int sx = (SCREEN_WIDTH - imgW) >> 1;
            System.out.println("SCREEN_WIDTH = " + SCREEN_WIDTH + "-----------------");
            g.setColor(-1);
            if (this.npcHeadImg != null) {
                if (this.isDrawOnLeft) {
                    g.drawRegion(this.talkImg, 0, 0, imgW, imgH, 2, sx, SCREEN_HEIGHT - imgH, 0);
                    g.drawString(this.sayName, (SCREEN_WIDTH - sx) - 42, (SCREEN_HEIGHT - imgH) + 6, 17);
                    g.drawImage(this.npcHeadImg, sx, sy - 8, 36);
                } else {
                    g.drawImage(this.talkImg, sx, SCREEN_HEIGHT, 36);
                    g.drawString(this.sayName, sx + 42, (SCREEN_HEIGHT - imgH) + 6, 17);
                    g.drawImage(this.npcHeadImg, SCREEN_WIDTH - sx, sy - 8, 40);
                }
            } else {
                g.drawImage(this.talkImg, sx, SCREEN_HEIGHT, 36);
                g.drawString(this.sayName, sx + 42, (SCREEN_HEIGHT - imgH) + 6, 17);
            }
            this.talkST.draw(g, sx + 12, sy);
        }
    }

    private Bitmap loadHeadImage(String name, String pathName) {
        String imgPath = null;
        if (pathName != null && !pathName.equals("")) {
            imgPath = String.valueOf(pathName) + ".p";
        } else if (name.equals("古小天")) {
            imgPath = "f_00.p";
        } else if (name.equals("灵儿")) {
            imgPath = "f_01.p";
        } else if (name.equals("秦天虎")) {
            imgPath = "f_02.p";
        } else if (name.equals("叶秋心")) {
            imgPath = "f_03.p";
        } else if (name.equals("楚山南")) {
            imgPath = "f_04.p";
        } else if (name.equals("天草")) {
            imgPath = "f_05.p";
        }
        if (imgPath == null) {
            return null;
        }
        try {
            Bitmap img = Tool.loadPImage(String.valueOf(IMG_PATH) + imgPath);
            return img;
        } catch (Exception e) {
            e.printStackTrace();
            return null;
        }
    }

    public void script_talk(String name, String str, String pathName, boolean isLeft) {
        Common.clearKeyStates();
        this.sayName = name;
        this.npcHeadImg = loadHeadImage(name, pathName);
        this.talkST = new StyledText(str, (SCREEN_WIDTH / 2) - 30, 2, font, Font.fontChinaH, this.talkAS, 16);
        this.isDrawOnLeft = isLeft;
        this.isTalking = true;
        this.bGameState = (byte) 12;
        this.script.pause();
    }

    @Override // com.dobetter.script.ScriptProcessor
    public void setScriptState(byte st) {
        this.scriptState = st;
    }

    @Override // com.dobetter.script.ScriptProcessor
    public byte getScriptState() {
        return this.scriptState;
    }

    @Override // com.dobetter.script.ScriptProcessor
    public DataType call(Vector para, int methodID) throws Exception {
        switch (methodID) {
            case Graphics.BOTTOM /* 32 */:
                script_call(Interpreter.vPopStringBack(para), Interpreter.vPopBooleanBack(para));
                return null;
            case Graphics.HB /* 33 */:
                script_loadStart(Interpreter.vPopStringBack(para));
                return null;
            case 34:
                script_loadFinish(Interpreter.vPopBooleanBack(para));
                return null;
            case 35:
                script_loadMap(Interpreter.vPopStringBack(para));
                return null;
            case Graphics.LB /* 36 */:
                script_setMapScript(Interpreter.vPopIntBack(para), Interpreter.vPopIntBack(para), Interpreter.vPopStringBack(para));
                return null;
            case 37:
                script_delMapScript(Interpreter.vPopStringBack(para));
                return null;
            case 38:
                script_initRole(Interpreter.vPopIntBack(para), Interpreter.vPopIntBack(para), getDir(Interpreter.vPopStringBack(para)));
                return null;
            case 39:
                script_talk(Interpreter.vPopStringBack(para), Interpreter.vPopStringBack(para), Interpreter.vPopStringBack(para), Interpreter.vPopBooleanBack(para));
                return null;
            case Graphics.RB /* 40 */:
                script_showMessage(Interpreter.vPopStringBack(para), Interpreter.vPopIntBack(para));
                return null;
            case 41:
                script_showText(Interpreter.vPopStringBack(para));
                return null;
            case 42:
                String musicPath = Interpreter.vPopStringBack(para);
                if (!musicPath.equals(this.currentBGMusic)) {
                    this.currentBGMusic = musicPath;
                    playMusic();
                }
                return null;
            case 43:
                script_stopMusic();
                return null;
            case 44:
                script_setMapFlag(Interpreter.vPopIntBack(para), Interpreter.vPopIntBack(para), Interpreter.vPopIntBack(para), Interpreter.vPopBooleanBack(para));
                return null;
            case 45:
                script_showChoice(Interpreter.vPopStringBack(para), (com.dobetter.script.Array) Interpreter.vPopBack(para), Interpreter.vPopBooleanBack(para));
                return null;
            case 46:
                return new Int(getLastChoice());
            case 47:
                script_initFamulus(Interpreter.vPopIntBack(para), Interpreter.vPopIntBack(para), getDir(Interpreter.vPopStringBack(para)));
                return null;
            case 48:
                script_addSprite(Interpreter.vPopIntBack(para), Interpreter.vPopIntBack(para), getDir(Interpreter.vPopStringBack(para)), Interpreter.vPopIntBack(para), Interpreter.vPopIntBack(para), Interpreter.vPopIntBack(para));
                return null;
            case 49:
                script_delSprite(Interpreter.vPopIntBack(para));
                return null;
            case 50:
                script_setDir(Interpreter.vPopIntBack(para), getDir(Interpreter.vPopStringBack(para)));
                return null;
            case 51:
                script_moveRole(getDir(Interpreter.vPopStringBack(para)), Interpreter.vPopIntBack(para), Interpreter.vPopIntBack(para), Interpreter.vPopBooleanBack(para));
                return null;
            case 52:
                script_moveSprite(Interpreter.vPopIntBack(para), getDir(Interpreter.vPopStringBack(para)), Interpreter.vPopIntBack(para), Interpreter.vPopIntBack(para));
                return null;
            case 53:
                script_moveSprtes((com.dobetter.script.Array) Interpreter.vPopBack(para), getDir(Interpreter.vPopStringBack(para)), Interpreter.vPopIntBack(para), Interpreter.vPopIntBack(para));
                return null;
            case 54:
                script_moveSpritesEach((com.dobetter.script.Array) Interpreter.vPopBack(para), (com.dobetter.script.Array) Interpreter.vPopBack(para), (com.dobetter.script.Array) Interpreter.vPopBack(para), (com.dobetter.script.Array) Interpreter.vPopBack(para));
                return null;
            case 55:
                script_setSpriteScript(Interpreter.vPopIntBack(para), Interpreter.vPopStringBack(para), Interpreter.vPopIntBack(para));
                return null;
            case 56:
                script_delSpriteScript(Interpreter.vPopIntBack(para), Interpreter.vPopIntBack(para));
                return null;
            case 57:
                script_setSpriteDisappear(Interpreter.vPopIntBack(para), Interpreter.vPopBooleanBack(para));
                return null;
            case 58:
                script_setSpriteAction(Interpreter.vPopIntBack(para), Interpreter.vPopIntBack(para), Interpreter.vPopIntBack(para), Interpreter.vPopBooleanBack(para), Interpreter.vPopBooleanBack(para), Interpreter.vPopIntBack(para));
                return null;
            case 59:
                script_initSpriteAction(Interpreter.vPopIntBack(para));
                return null;
            case FIRE_KEY_HEIGHT /* 60 */:
                setSpriteMove(Interpreter.vPopIntBack(para), Interpreter.vPopIntBack(para));
                return null;
            case 61:
                script_setSpriteFace(Interpreter.vPopIntBack(para), Interpreter.vPopIntBack(para), Interpreter.vPopIntBack(para));
                return null;
            case 62:
                script_setEnemyLevel(Interpreter.vPopIntBack(para), Interpreter.vPopIntBack(para));
                return null;
            case 63:
                script_lockSprite(Interpreter.vPopIntBack(para), Interpreter.vPopBooleanBack(para));
                return null;
            case Graphics.BASELINE /* 64 */:
                setScreenMove(Interpreter.vPopIntBack(para), Interpreter.vPopIntBack(para), Interpreter.vPopIntBack(para), Interpreter.vPopBooleanBack(para));
                return null;
            case 65:
                script_screenMoveBack(Interpreter.vPopIntBack(para), Interpreter.vPopBooleanBack(para));
                return null;
            case 66:
                script_screenMoveTo(Interpreter.vPopIntBack(para), Interpreter.vPopIntBack(para), Interpreter.vPopIntBack(para), Interpreter.vPopBooleanBack(para));
                return null;
            case 67:
                setScreenPos(Interpreter.vPopIntBack(para), Interpreter.vPopIntBack(para));
                return null;
            case 68:
                setScreenStay(Interpreter.vPopBooleanBack(para));
                return null;
            case 69:
                setVibrate(Interpreter.vPopIntBack(para), Interpreter.vPopIntBack(para));
                return null;
            case 70:
                stopVibrate();
                return null;
            case 71:
            case 88:
            case 98:
            case 112:
            case 113:
            case 115:
            case 116:
            case 117:
            case 138:
            case 139:
            case 140:
            case 141:
            case 142:
            case 143:
            default:
                throw new Exception("No Such Func In Lib: " + Integer.toHexString(methodID));
            case 72:
                initGameMap(Interpreter.vPopIntBack(para));
                return null;
            case 73:
                script_tip(Interpreter.vPopStringBack(para));
                return null;
            case 74:
                return new Int(getPropNum(Interpreter.vPopIntBack(para)));
            case 75:
                script_delProp(Interpreter.vPopIntBack(para), Interpreter.vPopIntBack(para));
                return null;
            case 76:
                script_showStageName();
                return null;
            case 77:
                setRoleLevel(Interpreter.vPopIntBack(para));
                return null;
            case 78:
                this.isExpRoom = true;
                return null;
            case 79:
                script_useSkill(Interpreter.vPopIntBack(para), Interpreter.vPopIntBack(para));
                return null;
            case 80:
                script_addMoney(Interpreter.vPopIntBack(para), Interpreter.vPopBooleanBack(para));
                return null;
            case 81:
                script_addProp(Interpreter.vPopIntBack(para), Interpreter.vPopIntBack(para), Interpreter.vPopBooleanBack(para));
                return null;
            case 82:
                this.isDropItem = Interpreter.vPopBooleanBack(para);
                return null;
            case 83:
                script_addEquip(Interpreter.vPopIntBack(para), Interpreter.vPopIntBack(para), Interpreter.vPopBooleanBack(para));
                return null;
            case 84:
                script_delEquip(Interpreter.vPopIntBack(para));
                return null;
            case 85:
                script_putOnEquip(Interpreter.vPopIntBack(para));
                return null;
            case 86:
                script_takeOffEquip();
                return null;
            case 87:
                initReinforce();
                return null;
            case 89:
                script_setTask((com.dobetter.script.Array) Interpreter.vPopBack(para), (com.dobetter.script.Array) Interpreter.vPopBack(para), (com.dobetter.script.Array) Interpreter.vPopBack(para), Interpreter.vPopStringBack(para), Interpreter.vPopBooleanBack(para));
                return null;
            case Tool.UI_RATATE_90 /* 90 */:
                script_setMissionText(Interpreter.vPopStringBack(para), Interpreter.vPopStringBack(para), (com.dobetter.script.Array) Interpreter.vPopBack(para), Interpreter.vPopBooleanBack(para));
                return null;
            case 91:
                addDropInfo(Interpreter.vPopStringBack(para));
                return null;
            case 92:
                addScrollInfo(Interpreter.vPopStringBack(para));
                return null;
            case 93:
                script_blackScreenMessage(Interpreter.vPopStringBack(para), Interpreter.vPopBooleanBack(para), Interpreter.vPopIntBack(para), Interpreter.vPopIntBack(para), Interpreter.vPopStringBack(para), Interpreter.vPopIntBack(para));
                return null;
            case 94:
                setScreenStayDuration(Interpreter.vPopIntBack(para));
                return null;
            case 95:
                sleep(Interpreter.vPopIntBack(para));
                return null;
            case 96:
                closeGameMenu();
                return null;
            case 97:
                script_hideUI(Interpreter.vPopBooleanBack(para));
                return null;
            case 99:
                initShop(Interpreter.vPopIntBack(para));
                return null;
            case Tool.P_CHUNK_SIZE /* 100 */:
                script_addFamulus(Interpreter.vPopIntBack(para));
                return null;
            case 101:
                this.isKeyLocked = Interpreter.vPopBooleanBack(para);
                this.role.initAct();
                return null;
            case 102:
                Common.setKeyPressed(Interpreter.vPopIntBack(para));
                handleKeyPressed();
                Common.clearKeyPressedStates();
                return null;
            case 103:
                int[] btnKeys0 = {9, 9, 10};
                int btnid = Interpreter.vPopIntBack(para);
                Common.setKeyPressed(btnKeys0[btnid]);
                handleKeyPressed();
                Common.clearKeyPressedStates();
                return null;
            case 104:
                this.recordNames[this.bRecordIndex] = this.stageName;
                saveRecordNames("TheSoulOfTheSwordTitle");
                saveRecord(this.bRecordIndex);
                addScrollInfo("已保存当前进度。");
                return null;
            case 105:
                script_gameEnd();
                return null;
            case 106:
                script_checkKeyCode(Interpreter.vPopIntBack(para));
                return null;
            case 107:
                return new Bool(this.isFalseKeyResponse);
            case 108:
                script_checkKeyResponse((com.dobetter.script.Array) Interpreter.vPopBack(para), Interpreter.vPopIntBack(para));
                return null;
            case 109:
                loadMapBackgroundRes(Interpreter.vPopStringBack(para));
                return null;
            case 110:
                script_setGuideArrow(Interpreter.vPopIntBack(para));
                return null;
            case 111:
                return new Bool(this.isAllEnemiesKilled);
            case 114:
                script_removeFamulus();
                return null;
            case 118:
                script_setMissionSign(Interpreter.vPopIntBack(para), Interpreter.vPopIntBack(para));
                return null;
            case 119:
                return new Int(this.role.sLevel);
            case 120:
                int index = Interpreter.vPopIntBack(para);
                if (index == 3) {
                    if (!this.isOpenAllStage) {
                        initSMS(3);
                        this.script.pause();
                    } else {
                        this.isOpenAllStage = true;
                        if (this.script != null) {
                            setScriptState((byte) 3);
                        }
                    }
                } else if (index == 1 && this.bExpCoefficientBySMS < 4) {
                    script_showMessage("升级过慢，可到商城购买多倍经验。", 0);
                }
                return null;
            case 121:
                return new Bool(isSendSMSSuccess());
            case 122:
                script_loadSkillRes(Interpreter.vPopIntBack(para));
                return null;
            case 123:
                script_swordDestroyed();
                return null;
            case 124:
                script_setDurabilityNonLoss(Interpreter.vPopBooleanBack(para));
                return null;
            case 125:
                this.isMaximumPower = Interpreter.vPopBooleanBack(para);
                this.role.sEP = (short) 1000;
                return null;
            case 126:
                script_bossCriticalKill(Interpreter.vPopIntBack(para));
                return null;
            case 127:
                integrate();
                return null;
            case 128:
                script_criticalKill(Interpreter.vPopBooleanBack(para));
                return null;
            case 129:
                breakSkill();
                return null;
            case 130:
                return new Bool(isDurabilityToZero());
            case 131:
                timeIn(Interpreter.vPopIntBack(para), Interpreter.vPopStringBack(para), Interpreter.vPopBooleanBack(para));
                return null;
            case 132:
                timePause();
                return null;
            case 133:
                timeContinue();
                return null;
            case 134:
                timeOut();
                return null;
            case 135:
                repair();
                return null;
            case 136:
                recover();
                return null;
            case 137:
                return new Int(getCostOfRepair());
            case 144:
                return new Bool(this.role.iHP < this.role.iFinalHP);
            case 145:
                return new Bool(this.role.isIntegrate);
            case 146:
                script_addExp(Interpreter.vPopIntBack(para), Interpreter.vPopBooleanBack(para));
                return null;
            case 147:
                script_setFamulusLevel(Interpreter.vPopIntBack(para));
                return null;
            case 148:
                initEquipPreview(Interpreter.vPopIntBack(para), Interpreter.vPopIntBack(para), Interpreter.vPopBooleanBack(para));
                return null;
            case 149:
                this.iMissionStageIndex = Interpreter.vPopIntBack(para);
                return null;
            case 150:
                setAreaIndex(Interpreter.vPopIntBack(para));
                return null;
            case 151:
                short s = this.role.sEP;
                this.role.getClass();
                return new Bool(s == 2000);
            case 152:
                Role role = this.role;
                this.role.getClass();
                role.sEP = (short) 2000;
                return null;
            case 153:
                return new Int(this.role.famulus.sLevel);
            case 154:
                this.isIntroduct = Interpreter.vPopBooleanBack(para);
                return null;
            case 155:
                return new Int(this.bag.iMoney);
            case 156:
                bloodControl(Interpreter.vPopBooleanBack(para));
                return null;
            case 157:
                cdTimeControl(Interpreter.vPopBooleanBack(para));
                return null;
        }
    }

    void bloodControl(boolean isTurnOn) {
        turnOn = isTurnOn;
    }

    void cdTimeControl(boolean isCdTime) {
        cdTime = isCdTime;
    }

    void script_setDurabilityNonLoss(boolean isNonLoss) {
        this.isDurabilityNonLoss = isNonLoss;
        if (this.role.isIntegrate) {
            this.role.famulus.iCurDurability = this.role.famulus.iFinalDurability;
        } else {
            this.role.equip.iCurDurability = this.role.equip.iFinalDurability;
        }
    }

    void recover() {
        this.role.iHP = this.role.iFinalHP;
        addScrollInfo("气血恢复为满值。");
    }

    int getCostOfRepair() {
        int durabilityVariance = 0;
        int destroyedNum = 0;
        for (int i = 0; i < this.bag.equipV.size(); i++) {
            Equip equip = (Equip) this.bag.equipV.elementAt(i);
            int temp = equip.iFinalDurability - equip.iCurDurability;
            if (temp > 0) {
                durabilityVariance += temp;
                destroyedNum++;
            }
        }
        this.iCostOfRepair = (durabilityVariance / 10) + (destroyedNum * 500);
        return this.iCostOfRepair;
    }

    String getRepairInfo() {
        StringBuffer sb = new StringBuffer();
        sb.append("是否花费");
        if (this.iCostOfRepair / 10000 > 0) {
            sb.append(this.iCostOfRepair / 10000);
            sb.append("金");
            if (this.iCostOfRepair % 10000 > 0) {
                sb.append(this.iCostOfRepair % 10000);
                sb.append("银");
            }
        } else {
            sb.append(this.iCostOfRepair);
            sb.append("银");
        }
        sb.append("修理全部武器？");
        return sb.toString();
    }

    void repair() {
        if (this.bag.iMoney >= this.iCostOfRepair) {
            this.bag.iMoney -= this.iCostOfRepair;
            for (int i = 0; i < this.bag.equipV.size(); i++) {
                Equip equip = (Equip) this.bag.equipV.elementAt(i);
                equip.iCurDurability = equip.iFinalDurability;
            }
            script_showMessage("修理成功", 0);
            return;
        }
        script_showMessage("银两不足", 0);
    }

    public boolean isDurabilityToZero() {
        if (this.role.isIntegrate) {
            if (this.role.famulus.iCurDurability == 0) {
                return true;
            }
        } else if (this.role.equip.iCurDurability == 0) {
            return true;
        }
        return false;
    }

    public void script_bossCriticalKill(int id) {
        Enemy e = (Enemy) getSprite(id);
        this.role.isLocked = true;
        this.role.enemy = e;
        e.act((byte) 22);
        e.isBossCriticalKill = true;
    }

    public void script_loadSkillRes(int skillIndex) {
        this.role.loadSkillRes(skillIndex);
    }

    public void script_useSkill(int skillIndex, int level) {
        if (skillIndex < 14) {
            this.role.act((byte) ((skillIndex + 12) - 5));
        } else if (skillIndex < 22) {
            this.role.act((byte) 21);
        }
        this.role.isUseSkillByScript = true;
        this.role.bSkillLevelByScript = (byte) level;
    }

    private boolean isSendSMSSuccess() {
        return this.isOpenAllStage;
    }

    private byte getDir(String str) {
        String str2 = str.toLowerCase();
        if (str2.compareTo("l") == 0) {
            return (byte) 0;
        }
        if (str2.compareTo("r") == 0) {
            return (byte) 1;
        }
        if (str2.compareTo("u") == 0) {
            return (byte) 2;
        }
        return (byte) 3;
    }

    private void cycleTimeCounter() {
        if (this.isTimeIn) {
            long currentTime = ((long) this.iRemainingTime) - (((System.currentTimeMillis() - this.lPauseDuration) - this.lStageStartTime) / 1000);
            this.second = (byte) (currentTime % 60);
            this.minute = (byte) (currentTime / 60);
            if (currentTime <= 0) {
                this.isTimeIn = false;
                script_call(this.timeOutScript, this.isLoadingAfterTimeOut);
            }
        }
    }

    private void timeIn(int totalTime, String script, boolean isToLoading) {
        this.iRemainingTime = totalTime;
        this.timeOutScript = script;
        this.isLoadingAfterTimeOut = isToLoading;
        this.lStageStartTime = System.currentTimeMillis();
        this.lPauseDuration = 0L;
        this.isTimeIn = true;
        int currentTime = ((int) ((System.currentTimeMillis() - this.lPauseDuration) - this.lStageStartTime)) / WebNetEvent.GotoWeb_Event_OK;
        int remainingTime = this.iRemainingTime - currentTime;
        this.second = (byte) (remainingTime % FIRE_KEY_HEIGHT == 0 ? 0 : 60 - (remainingTime % FIRE_KEY_HEIGHT));
        this.minute = (byte) (remainingTime / FIRE_KEY_HEIGHT);
    }

    private void timeOut() {
        this.isTimeIn = false;
    }

    private void timePause() {
        this.lPauseTime = System.currentTimeMillis();
    }

    private void timeContinue() {
        this.lPauseDuration += System.currentTimeMillis() - this.lPauseTime;
    }

    public void drawRemainingTime(Graphics g, int x, int y) {
        if (this.isTimeIn) {
            int width = font.stringWidth("剩余时间 ");
            Tool.drawString(g, "剩余时间 ", x, y, 16711680, -1);
            StringBuffer sb = new StringBuffer("");
            sb.append((int) this.minute);
            sb.append(":");
            if (this.second < 10) {
                sb.append("0");
            }
            sb.append((int) this.second);
            Tool.drawString(g, sb.toString(), x + width, y, 0, -1);
        }
    }

    public void script_call(String sFile, boolean isLoading) {
        this.scriptFileToRunV.addElement(sFile);
        if (isLoading) {
            this.loadingStateV.addElement(new Byte((byte) 1));
        } else {
            this.loadingStateV.addElement(new Byte((byte) 0));
        }
    }

    public void script_loadStart(String stageName) {
        Common.clearKeyStates();
        this.stageName = stageName;
        try {
            initMapElement();
        } catch (Exception e) {
            e.printStackTrace();
        }
        this.iRolePreCellX = -1;
        this.iRolePreCellY = -1;
        this.isExpRoom = false;
    }

    public void script_loadFinish(boolean isOtherState) {
        Common.clearKeyStates();
        this.iCurrentStageID = Interpreter.getValue(SCRIPT_VARIABLES[1]).getVal();
        this.isLoadFinish = true;
        this.isOtherState = isOtherState;
        if (this.scriptState == 1) {
            this.script.pause();
        }
    }

    public void script_loadMap(String mFile) {
        this.map = new GameMap(SCREEN_WIDTH, SCREEN_HEIGHT);
        this.map.loadData(mFile);
    }

    void script_setTask(com.dobetter.script.Array arg0, com.dobetter.script.Array arg1, com.dobetter.script.Array arg2, String script, boolean isLoading) {
        int[] stageID = new int[arg0.getLength()];
        for (int i = 0; i < stageID.length; i++) {
            stageID[i] = ((Int) arg0.getValue(i)).getVal();
        }
        int[] enemyID = new int[arg1.getLength()];
        for (int i2 = 0; i2 < enemyID.length; i2++) {
            enemyID[i2] = ((Int) arg1.getValue(i2)).getVal();
        }
        int[] enemyNum = new int[arg2.getLength()];
        for (int i3 = 0; i3 < enemyNum.length; i3++) {
            enemyNum[i3] = ((Int) arg2.getValue(i3)).getVal();
        }
        Task task = new Task(stageID, enemyID, enemyNum, script, isLoading);
        this.taskV.addElement(task);
    }

    public int script_getRnd(int min, int max) {
        return Tool.getNextRnd(min, max);
    }

    public void script_showText(String str) {
        showMessage(str, 0, (byte) 0);
        this.script.pause();
    }

    public void script_showMessage(String str, int time) {
        showMessage(str, time, (byte) 1);
        this.script.pause();
    }

    public void script_setMissionText(String title, String info, com.dobetter.script.Array args, boolean isBranch) {
        if (!isBranch) {
            this.mainMissionTitle = title;
            this.mainMissionInfo = info;
        } else {
            this.branchMissionTitle = title;
            this.branchMissionInfo = info;
            if (title == null || title.trim().length() < 1) {
                this.taskV.removeAllElements();
            }
        }
        for (int i = 0; i < 3; i++) {
            if (!isBranch) {
                if (i < args.getLength()) {
                    this.missionHortation[0][i] = ((Int) args.getValue(i)).getVal();
                } else {
                    this.missionHortation[0][i] = -1;
                }
            } else if (i < args.getLength()) {
                this.missionHortation[1][i] = ((Int) args.getValue(i)).getVal();
            } else {
                this.missionHortation[1][i] = -1;
            }
        }
        addScrollInfo("当前任务已更新。请在系统菜单查看");
    }

    public void script_showChoice(String info, com.dobetter.script.Array args, boolean bClose) {
        String[] items = new String[args.getLength()];
        for (int i = 0; i < items.length; i++) {
            items[i] = ((Str) args.getValue(i)).getVal();
        }
        showChoice(info, items, bClose);
        this.script.pause();
    }

    public int getLastChoice() {
        return this.lastChoiceIndex;
    }

    public void showChoice(String info, String[] items, boolean bClose) {
        if (info == null || info.length() == 0) {
            this.choiceInfo = null;
        } else {
            this.choiceInfo = new StyledText(info, Tool.UI_RATATE_180, 80, Font.fontChinaH / 10, Font.fontChinaH, font);
        }
        int ih = FontHeight + 5;
        this.choiceList = new XList(items, 120, ih, 3, font, true);
        this.isChoiceClosable = bClose;
        this.lastChoiceIndex = -1;
        this.bGameState = (byte) 11;
    }

    public void drawChoice(Graphics g) {
        int listY;
        if (this.choiceInfo != null) {
            int tw = this.choiceInfo.getWidth();
            int th = this.choiceInfo.getHeight();
            int tx = (SCREEN_WIDTH - tw) >> 1;
            listY = 50 + th + DROP_POS_INTERVAL;
            drawRoundRectBGByTile(g, this.uiBGImg, tx - 10, 40, this.uiBGImg.getHeight(), this.uiBGImg.getHeight(), tw + 20, th + 20);
            this.choiceInfo.draw(g, tx, 50);
        } else {
            listY = SCREEN_HEIGHT / 4;
        }
        if (this.choiceList != null) {
            int listW = this.choiceList.getWidth();
            int listH = this.choiceList.getHeight();
            int listX = (SCREEN_WIDTH - listW) >> 1;
            drawRoundRectBGByTile(g, this.uiBGImg, listX - 30, listY - 15, this.uiBGImg.getHeight(), this.uiBGImg.getHeight(), listW + FIRE_KEY_HEIGHT, listH + DROP_POS_INTERVAL);
            drawXList(g, this.choiceList, listX, listY, (byte) 0);
        }
    }

    public void drawXList(Graphics g, XList xList, int dx, int dy, byte bType) {
        if (xList.getItemNum() > 0) {
            int[] fomat = {13947468, 0, 2};
            int[] focusFomat = {16777215, 0, 2};
            int listW = xList.getWidth();
            int listH = xList.getHeight();
            g.setColor(8612255);
            int ih = xList.getItemHeight();
            int focusY = dy + (xList.getFocusIndexInPage() * ih);
            g.drawRect(dx - 5, focusY, listW + 9, ih - 1);
            g.fillRect(dx - 3, focusY + 2, listW + 6, ih - 4);
            xList.draw(g, dx, dy, fomat, focusFomat, bType);
            drawListBar(g, xList, dx, dy, listW, listH);
        }
    }

    private void drawListBar(Graphics g, XList xList, int listX, int listY, int listW, int listH) {
        int sclX = listX + listW + 7;
        Tool.fillRect(g, sclX, listY, 6, listH, 13817549, -1, true);
        int blockTotalH = listH - 2;
        int blockH = blockTotalH / xList.getItemNum();
        if (blockH < 3) {
            blockH = 3;
        }
        int blockY = (xList.getFocusIndex() * blockTotalH) / xList.getItemNum();
        if (blockY + blockH > blockTotalH) {
            blockY = blockTotalH - blockH;
        }
        Tool.fillRect(g, sclX + 1, listY + blockY + 1, 4, blockH, 5725285, -1, true);
    }

    private void cycleChoice() {
        if (this.choiceInfo != null) {
            this.choiceInfo.cycle();
        }
        this.choiceList.cycle();
    }

    void showMessage(String str, int time, byte type) {
        Common.clearKeyStates();
        this.lSleepTimeMills = time;
        this.lMessageTimeMills = 0L;
        StyledText msg = null;
        if (type == 0) {
            msg = new StyledText(str, SCREEN_WIDTH - 20, SCREEN_HEIGHT - 50, Font.fontChinaH / 10, Font.fontChinaH, font);
        } else if (type == 1) {
            msg = new StyledText(str, SCREEN_WIDTH - 20, SCREEN_HEIGHT / 3, Font.fontChinaH / 10, Font.fontChinaH, font);
            msg.setLineCenter();
        }
        if (this.message == null) {
            this.message = msg;
            this.messageType = type;
        } else {
            this.messageSet.addElement(msg);
            this.messageTypeSet.addElement(new Byte(type));
            this.messageDurationSet.addElement(new Integer(time));
        }
        setGameState((byte) 17);
    }

    void drawMessage(Graphics g) {
        if (this.message != null) {
            if (this.messageType == 0) {
                Common.fillUIRect(g, 0);
                this.message.draw(g, 10, 50);
            } else if (this.messageType == 1) {
                int my = ((SCREEN_HEIGHT - this.message.getHeight()) >> 1) - 10;
                int height = this.message.iVisibleHeight < this.message.getHeight() ? this.message.iVisibleHeight : this.message.getHeight();
                drawRoundRectBGByTile(g, this.uiBGImg, 0, my - 10, this.uiBGImg.getHeight(), this.uiBGImg.getHeight(), SCREEN_WIDTH, height + 20);
                this.message.draw(g, 10, my);
                if (this.lSleepTimeMills == 0) {
                    this.keyPressTipAS.drawFrameCycle(g, 4, Tool.countTimes, SCREEN_WIDTH - 10, my + height);
                }
            }
        }
    }

    public void script_stopMusic() {
    }

    public void script_setMapFlag(int cx, int cy, int layerIndex, boolean bPass) {
        if (this.map != null) {
            this.map.setMapFlag(cx, cy, layerIndex, bPass);
        }
    }

    public void script_addMoney(int num, boolean isShowMessage) {
        if (num > 0) {
            this.bag.iMoney += num;
            if (isShowMessage) {
                showMessage("获得  <c=" + COLORS[3] + ",0,1>" + num + "银两", 0, (byte) 1);
            }
        } else if (Math.abs(num) > this.bag.iMoney) {
            if (isShowMessage) {
                showMessage("你没有足够的银两！", 0, (byte) 1);
            }
        } else {
            this.bag.iMoney += num;
            if (isShowMessage) {
                showMessage("失去  <c=" + COLORS[3] + ",0,1>" + Math.abs(num) + "银两", 0, (byte) 1);
            }
        }
        if (isShowMessage) {
            this.script.pause();
        }
    }

    public void script_addEquip(int id, int reinforceLevel, boolean isShowMessage) {
        Equip equip = new Equip(equipDatas[id], reinforceLevel, true);
        this.bag.addEquip(equip);
        if (isShowMessage) {
            showMessage("得到" + equip.nameInfo(), 0, (byte) 1);
            this.script.pause();
        }
    }

    public void script_delEquip(int id) {
        this.bag.deleteEquip(equipDatas[id]);
    }

    public void script_putOnEquip(int id) {
        if (id >= 0) {
            for (int i = this.bag.equipV.size() - 1; i >= 0; i--) {
                Equip weapon = (Equip) this.bag.equipV.elementAt(i);
                if (weapon.data.bID == id) {
                    this.role.updateEquipment(weapon);
                    return;
                }
            }
            return;
        }
        this.role.reload(this.role.weaponStr, 4);
        this.isAttackable = true;
    }

    public void script_takeOffEquip() {
        this.role.actionSet.images[4] = null;
        this.isAttackable = false;
    }

    public void script_swordDestroyed() {
        if (this.role.isIntegrate) {
            this.role.famulus.iCurDurability = 0;
        } else if (this.role.equip.iCurDurability > 0) {
            destroySword();
        }
    }

    void destroySword() {
        this.role.equip.iCurDurability = 0;
        this.isDurabilityToZero = true;
        this.bEffectCounter = (byte) 0;
        this.role.sCDCount[0] = 0;
        this.role.sCDCount[1] = 0;
        if (this.role.famulus != null && this.role.famulus.bActID == 3) {
            this.role.famulus.initAct();
        }
    }

    public void script_addProp(int propID, int num, boolean isShowMessage) {
        this.bag.addProp(propDatas[propID], num);
        Prop prop = new Prop(propDatas[propID], num);
        if (isShowMessage) {
            showMessage("得到 <c=" + COLORS[3] + ">" + num + "</c>个" + prop.nameInfo() + "!", 0, (byte) 1);
            this.script.pause();
        }
    }

    public void script_delProp(int propID, int num) {
        this.bag.deleteProp(propDatas[propID].bID, num);
    }

    public int getPropNum(int propID) {
        for (int i = 0; i < this.bag.propV.size(); i++) {
            Prop prop = (Prop) this.bag.propV.elementAt(i);
            if (prop.data.bID == propID) {
                return prop.number;
            }
        }
        return 0;
    }

    public void script_addExp(int exp, boolean isShowMessage) {
        this.role.addExp(this.bExpCoefficientBySMS * exp);
        if (isShowMessage) {
            showMessage("得到 <c=" + COLORS[1] + ">" + (this.bExpCoefficientBySMS * exp) + "</c>点经验!", 0, (byte) 1);
        }
    }

    public void setSpriteMove(int id, int distance) {
        Sprite spr = getSprite(id);
        if (spr != null) {
            spr.iMoveDistance = distance;
        }
    }

    public void script_screenMoveTo(int mx, int my, int step, boolean isSaveCurrentPos) {
        this.iScreenMoveVX = step;
        this.iScreenMoveVY = step;
        int x = mx - (SCREEN_WIDTH >> 1);
        int y = my - (SCREEN_WIDTH >> 1);
        if (x < 0) {
            x = 0;
        }
        if (x > this.map.sMapWidth - SCREEN_WIDTH) {
            x = this.map.sMapWidth - SCREEN_WIDTH;
        }
        if (y < 0) {
            y = 0;
        }
        if (y > this.map.sMapHeight - SCREEN_HEIGHT) {
            y = this.map.sMapHeight - SCREEN_HEIGHT;
        }
        this.iScreenMoveToX = x;
        this.iScreenMoveToY = y;
        if (isSaveCurrentPos) {
            this.iScreenBackX = this.iViewMapX;
            this.iScreenBackY = this.iViewMapY;
        }
        this.bGameState = (byte) 13;
        this.script.pause();
    }

    public void script_screenMoveBack(int step, boolean isSaveCurrentPos) {
        this.iScreenMoveToX = this.iScreenBackX;
        this.iScreenMoveToY = this.iScreenBackY;
        this.iScreenMoveVX = step;
        this.iScreenMoveVY = step;
        if (isSaveCurrentPos) {
            this.iScreenBackX = this.iViewMapX;
            this.iScreenBackY = this.iViewMapY;
        }
        this.bGameState = (byte) 13;
        this.script.pause();
    }

    public void setScreenMove(int offX, int offY, int step, boolean isSaveCurrentPos) {
        this.isScreenMoving = true;
        this.iScreenMoveVX = offX > 0 ? step : -step;
        if (offX == 0) {
            this.iScreenMoveVX = 0;
        }
        this.iScreenMoveVY = offY > 0 ? step : -step;
        if (offY == 0) {
            this.iScreenMoveVY = 0;
        }
        this.iScreenMoveTotalTime = Math.abs(offX) > Math.abs(offY) ? Math.abs(offX) / step : Math.abs(offY) / step;
        this.iScreenMoveTimes = 0;
        if (isSaveCurrentPos) {
            this.iScreenBackX = this.iViewMapX;
            this.iScreenBackY = this.iViewMapY;
        }
    }

    public void setScreenStayDuration(int duration) {
        this.isCountByFrame = true;
        this.iScreenStayCounter = 0;
        this.iScreenStayDuration = duration;
        this.bPreState = this.bGameState;
        this.bGameState = (byte) 19;
        this.script.pause();
    }

    public void sleep(int duration) {
        this.isCountByFrame = false;
        this.iScreenStayCounter = 0;
        this.iScreenStayDuration = duration;
        this.bPreState = this.bGameState;
        this.bGameState = (byte) 19;
        this.script.pause();
    }

    public void setScreenPos(int mx, int my) {
        this.iScreenBackX = this.iViewMapX;
        this.iScreenBackY = this.iViewMapY;
        this.iViewMapX = mx - (SCREEN_WIDTH >> 1);
        this.iViewMapY = my - (SCREEN_HEIGHT >> 1);
        this.map.setViewPosInMap(this.iViewMapX, this.iViewMapY);
    }

    public void setScreenStay(boolean isStay) {
        this.isScreenStay = isStay;
    }

    public void setVibrate(int times, int vibrateType) {
        this.isVibrate = true;
        if (times == -1) {
            this.iScreenVibrateNums = 1000000000;
        } else {
            this.iScreenVibrateNums = times;
        }
        this.bVibrateType = (byte) vibrateType;
    }

    public void stopVibrate() {
        this.iVibrateX = 0;
        this.iVibrateY = 0;
        this.isVibrate = false;
    }

    void cycleVibrate() {
        if (this.isVibrate && this.iScreenVibrateNums > 0) {
            if (this.iScreenVibrateNums % 4 == 0) {
                if (this.bVibrateType == 0) {
                    this.iVibrateX = 2;
                    this.iVibrateY = 2;
                } else if (this.bVibrateType == 1) {
                    this.iVibrateX = 0;
                    this.iVibrateY = 2;
                }
            } else if (this.iScreenVibrateNums % 4 == 1) {
                if (this.bVibrateType == 0) {
                    this.iVibrateX = 4;
                    this.iVibrateY = 0;
                } else if (this.bVibrateType == 1) {
                    this.iVibrateX = 0;
                    this.iVibrateY = -2;
                }
            } else if (this.iScreenVibrateNums % 4 == 2) {
                if (this.bVibrateType == 0) {
                    this.iVibrateX = 2;
                    this.iVibrateY = 2;
                } else if (this.bVibrateType == 1) {
                    this.iVibrateX = 0;
                    this.iVibrateY = 1;
                }
            } else if (this.bVibrateType == 0) {
                this.iVibrateX = 0;
                this.iVibrateY = 0;
            } else if (this.bVibrateType == 1) {
                this.iVibrateX = 0;
                this.iVibrateY = -1;
            }
            this.iScreenVibrateNums--;
            if (this.iScreenVibrateNums == 0) {
                stopVibrate();
            }
        }
    }

    public void script_setMapScript(int cx, int cy, String str) {
        boolean isFind = false;
        if (str == null) {
            str = "";
        }
        for (int i = 0; i < this.mapScripts.size(); i++) {
            MapScript ms = (MapScript) this.mapScripts.elementAt(i);
            if (ms.cellX == cx && ms.cellY == cy) {
                isFind = true;
                if (str.length() == 0) {
                    this.mapScripts.removeElementAt(i);
                    break;
                } else {
                    ms.scriptFile = str;
                    break;
                }
            }
        }
        if (!isFind && str.length() > 0) {
            this.mapScripts.addElement(new MapScript(cx, cy, str));
        }
    }

    public void script_delMapScript(String str) {
        int i = 0;
        while (i < this.mapScripts.size()) {
            MapScript ms = (MapScript) this.mapScripts.elementAt(i);
            if (ms.scriptFile.equals(str)) {
                this.mapScripts.removeElementAt(i);
                System.out.println("mapScripts=" + str);
            } else {
                i++;
            }
        }
    }

    public String getMapScript(int cx, int cy) {
        for (int i = 0; i < this.mapScripts.size(); i++) {
            MapScript ms = (MapScript) this.mapScripts.elementAt(i);
            if (ms.cellX == cx && ms.cellY == cy) {
                return ms.scriptFile;
            }
        }
        return null;
    }

    void script_setSpriteAction(int sprID, int actionID, int duration, boolean isKeepFrame, boolean isCenter, int attackModeID) {
        Sprite spr = getSprite(sprID);
        if (spr != null) {
            boolean isSpriteExist = false;
            for (int i = 0; i < this.sprsByScriptControlV.size(); i++) {
                Sprite spr1 = (Sprite) this.sprsByScriptControlV.elementAt(i);
                if (sprID == spr1.ID) {
                    isSpriteExist = true;
                    break;
                }
            }
            if (!isSpriteExist) {
                this.sprsByScriptControlV.addElement(spr);
            }
            spr.iCurActionID = actionID;
            spr.iCurFrameIndex = 0;
            if (duration > 0) {
                spr.iDurationFrame = duration;
            } else {
                spr.iDurationFrame = spr.actionSet.actionDatas[actionID].frameNum - 1;
            }
            spr.isKeepFrame = isKeepFrame;
            spr.isSpriteAnimate = true;
            spr.isDrawInScreenCenter = isCenter;
            spr.curAction = spr.actionSet.actionDatas[spr.iCurActionID];
            spr.curFrame = spr.actionSet.frameDatas[spr.curAction.frameID[spr.iCurFrameIndex]];
            if (attackModeID > 0) {
                if (spr.bType == 10 || spr.bType == 9) {
                    ((Enemy) spr).setAttackPoint(attackModeID - 1);
                }
            }
        }
    }

    void script_setSpritesAction(com.dobetter.script.Array sprID, com.dobetter.script.Array actionID, com.dobetter.script.Array startFrame, com.dobetter.script.Array endFrame, com.dobetter.script.Array isKeepFrame, com.dobetter.script.Array attackModeID, int pauseDuration) {
        int num = sprID.getLength();
        this.sprsByScriptControl = new Sprite[num];
        for (int i = 0; i < num; i++) {
            this.sprsByScriptControl[i] = getSprite(((Int) sprID.getValue(i)).getVal());
            if (this.sprsByScriptControl[i] != null) {
                this.sprsByScriptControl[i].iCurActionID = ((Int) actionID.getValue(i)).getVal();
                this.sprsByScriptControl[i].iCurFrameIndex = ((Int) endFrame.getValue(i)).getVal();
                this.sprsByScriptControl[i].iDurationFrame = ((Int) endFrame.getValue(i)).getVal();
                this.sprsByScriptControl[i].isKeepFrame = ((Bool) isKeepFrame.getValue(i)).getVal();
                this.sprsByScriptControl[i].isSpriteAnimate = true;
                int atkModeID = ((Int) attackModeID.getValue(i)).getVal();
                if (atkModeID >= 0 && (this.sprsByScriptControl[i].bType == 10 || this.sprsByScriptControl[i].bType == 9)) {
                    ((Enemy) this.sprsByScriptControl[i]).setAttackPoint(atkModeID);
                }
            }
        }
        this.script.pause();
    }

    void script_initSpriteAction(int sprID) {
        Sprite me = getSprite(sprID);
        if (me != null) {
            me.isKeepFrame = false;
            me.isSpriteAnimate = false;
            me.initAct();
            this.sprsByScriptControlV.removeElement(me);
        }
    }

    void script_lockSprite(int sprID, boolean isLocked) {
        Sprite me = getSprite(sprID);
        if (me != null) {
            me.isLocked = isLocked;
            if (isLocked) {
                me.initAct();
            }
        }
    }

    void script_addFamulus(int level) {
        this.role.famulus = new Famulus(this.role, (short) level);
        this.role.famulus.initData();
        addMapElement(this.role.famulus);
    }

    void script_setFamulusLevel(int level) {
        this.role.famulus.sLevel = (short) level;
        this.role.famulus.getLevelUpExp();
        this.role.famulus.initData();
    }

    void script_removeFamulus() {
        if (this.role.famulus != null) {
            this.role.famulus.freeSprite();
            this.role.famulus = null;
        }
    }

    void script_setSpriteFace(int npcID, int faceID, int num) {
        Sprite me = getSprite(npcID);
        if (me != null) {
            me.iFaceIndex = faceID;
            me.iFaceFrameIndex = 0;
            me.iFaceDuration = (faceAS.actionDatas[faceID].frameNum * num) - 1;
        }
    }

    private void script_setMissionSign(int npcID, int signIndex) {
        Sprite me = getSprite(npcID);
        if (me != null) {
            me.iSignIndex = signIndex;
        }
    }

    /* JADX WARN: Code duplicated, block: B:20:0x004c  */
    /* JADX WARN: Code duplicated, block: B:25:0x006c  */
    public void script_addSprite(int id, int sortID, byte dir, int mx, int my, int layerIndex) throws Exception {
        Sprite spr;
        SpriteBaseData data = null;
        for (int i = 0; i < sprBaseData.length; i++) {
            data = sprBaseData[i];
            if (data.sortID == sortID) {
                break;
            }
        }
        if (data == null) {
            throw new Exception("No Such SortID: " + sortID);
        }
        switch (data.bType) {
            case 1:
            case 2:
            case 3:
            case 4:
            case 5:
                spr = new StageSprite(id, data, mx, my);
                break;
            case 6:
            case 7:
            case 8:
            default:
                spr = new NPCSprite(id, data, mx, my);
                ((NPCSprite) spr).setInitDir(dir);
                break;
            case Interpreter.ERR_ARR_OUT /* 9 */:
            case 10:
            case 11:
                EnemyData eData = null;
                for (int i2 = 0; i2 < enemyDatas.length; i2++) {
                    eData = enemyDatas[i2];
                    if (eData.sortID == sortID) {
                        if (eData == null) {
                            throw new Exception("No Such SortID: " + sortID);
                        }
                        spr = new Enemy(id, eData, mx, my);
                    }
                    break;
                }
                if (eData == null) {
                    throw new Exception("No Such SortID: " + sortID);
                }
                spr = new Enemy(id, eData, mx, my);
                break;
                break;
        }
        spr.setDir(dir);
        spr.iLayerIndex = layerIndex;
        addMapElement(spr);
    }

    void script_setEnemyLevel(int sortID, int level) {
        for (int i = 0; i < this.spriteInStage.size(); i++) {
            Sprite spr = (Sprite) this.spriteInStage.elementAt(i);
            if (spr != null && spr.iSortID == sortID) {
                ((Enemy) spr).sLevel = (short) (level > 150 ? 150 : level);
                ((Enemy) spr).initData();
            }
        }
    }

    public void script_initRole(int mx, int my, byte dir) {
        this.role.iMapX = mx;
        this.role.iMapY = my;
        this.role.setDir(dir);
        this.role.initAct();
        if (this.role.famulus != null) {
            this.role.famulus.iMapX = mx;
            this.role.famulus.iMapY = my;
            this.role.famulus.setDir(dir);
        }
        moveScreen();
    }

    public void script_initFamulus(int mx, int my, byte dir) {
        this.role.famulus.iMapX = mx;
        this.role.famulus.iMapY = my;
        this.role.famulus.setDir(dir);
    }

    public void script_setDir(int id, byte dir) {
        Sprite npc = getSprite(id);
        if (npc != null) {
            npc.setDir(dir);
        }
    }

    private void script_setGuideArrow(int id) {
        if (id < 0) {
            this.role.target = null;
            return;
        }
        this.role.target = getSprite(id);
        this.role.iTargetStageID = this.iCurrentStageID;
    }

    public void script_checkKeyCode(int keyCode) {
        this.bKeyResponsePoint = (byte) keyCode;
        this.bKeyPressTipIndex = STATE_CG;
        this.bTurtorialState = (byte) 2;
        this.script.pause();
    }

    public void script_checkKeyResponse(com.dobetter.script.Array arg, int index) {
        this.bKeyPressTipIndex = (byte) index;
        if (index == 0) {
            this.isTurtorialKey = new boolean[4];
            for (int i = 0; i < this.isTurtorialKey.length; i++) {
                this.isTurtorialKey[i] = true;
            }
        }
        this.keyCodeV.removeAllElements();
        int num = arg.getLength();
        for (int i2 = 0; i2 < num; i2++) {
            this.keyCodeV.addElement((Int) arg.getValue(i2));
        }
        this.bTurtorialState = (byte) 1;
        this.script.pause();
    }

    private void script_hideUI(boolean hide) {
        this.isHideUI = hide;
    }

    private void initMoveNPCs(int num) {
        this.sprsByScriptControl = new Sprite[num];
        this.movingSpriteDir = new int[num];
        this.movingSpriteStep = new int[num];
        this.movingSpriteTotalDistance = new int[num];
        this.movingSpriteDistance = new int[num];
    }

    public void script_moveSprite(int id, int dir, int step, int distance) {
        initMoveNPCs(1);
        this.sprsByScriptControl[0] = getSprite(id);
        this.movingSpriteDir[0] = dir;
        this.movingSpriteStep[0] = step;
        this.movingSpriteTotalDistance[0] = distance;
        this.movingSpriteDistance[0] = 0;
        this.bGameState = (byte) 20;
        this.script.pause();
    }

    public void script_moveSpritesEach(com.dobetter.script.Array args, com.dobetter.script.Array vxs, com.dobetter.script.Array vys, com.dobetter.script.Array tms) {
        int num = args.getLength();
        initMoveNPCs(num);
        for (int i = 0; i < num; i++) {
            this.sprsByScriptControl[i] = getSprite(((Int) args.getValue(i)).getVal());
            this.movingSpriteDir[i] = getDir(((Str) vxs.getValue(i)).getVal());
            this.movingSpriteStep[i] = ((Int) vys.getValue(i)).getVal();
            this.movingSpriteTotalDistance[i] = ((Int) tms.getValue(i)).getVal();
            this.movingSpriteDistance[i] = 0;
        }
        this.bGameState = (byte) 20;
        this.script.pause();
    }

    public void script_moveSprtes(com.dobetter.script.Array args, int dir, int step, int distance) {
        int num = args.getLength();
        initMoveNPCs(num);
        for (int i = 0; i < num; i++) {
            this.sprsByScriptControl[i] = getSprite(((Int) args.getValue(i)).getVal());
            this.movingSpriteDir[i] = dir;
            this.movingSpriteStep[i] = step;
            this.movingSpriteTotalDistance[i] = distance;
            this.movingSpriteDistance[i] = 0;
        }
        this.bGameState = (byte) 20;
        this.script.pause();
    }

    private void moveRole(int dir, int step, int distance, boolean isWithScreen) {
        initMoveNPCs(1);
        this.sprsByScriptControl[0] = this.role;
        this.movingSpriteDir[0] = dir;
        this.movingSpriteStep[0] = step;
        this.movingSpriteTotalDistance[0] = distance;
        this.movingSpriteDistance[0] = 0;
        this.isScreenMoveWithRole = isWithScreen;
        this.bGameState = (byte) 20;
    }

    public void script_moveRole(int dir, int step, int distance, boolean isWithScreen) {
        moveRole(dir, step, distance, isWithScreen);
        this.script.pause();
    }

    private Sprite getSprite(int id) {
        for (int i = this.spriteInStage.size() - 1; i >= 0; i--) {
            Sprite spr = (Sprite) this.spriteInStage.elementAt(i);
            if (spr != null && spr.ID == id) {
                return spr;
            }
        }
        return null;
    }

    private void script_setSpriteDisappear(int id, boolean isDisappear) {
        Sprite me = getSprite(id);
        if (me != null) {
            me.isInvisible = isDisappear;
        }
    }

    private void script_delSprite(int id) {
        for (int i = this.spriteInStage.size() - 1; i >= 0; i--) {
            Sprite me = (Sprite) this.spriteInStage.elementAt(i);
            if (me != null && me.ID == id) {
                this.spriteInStage.removeElementAt(i);
                return;
            }
        }
    }

    public void script_setSpriteScript(int id, String str, int type) {
        Sprite spr = getSprite(id);
        if (spr != null) {
            if (str != null && str.length() == 0) {
                str = null;
            }
            if (type == 0) {
                spr.script = str;
            } else if (type == 1) {
                spr.overScript = str;
            } else {
                spr.specialScript = str;
            }
        }
    }

    public void script_delSpriteScript(int id, int type) {
        Sprite spr = getSprite(id);
        if (spr != null) {
            if (type == 0) {
                spr.script = null;
            } else if (type == 1) {
                spr.overScript = null;
            } else {
                spr.specialScript = null;
            }
        }
    }

    void script_dropItem(int spriteID, boolean isDropVertical) {
        Enemy fs = (Enemy) getSprite(spriteID);
        if (fs != null) {
            this.isDropVertical = isDropVertical;
            dropItem(fs);
        }
    }

    void script_gameEnd() {
        releaseGameRes();
        loadMainMenuRes();
    }

    void script_blackScreenMessage(String message, boolean isReturn, int messageY, int lineNum, String imgPath, int imgY) {
        this.blackScreenMessage = Tool.getStringArray(message, SCREEN_WIDTH - 20, font);
        if (messageY == 0) {
            this.iStoryMessageY = (SCREEN_HEIGHT - (this.blackScreenMessage.length * FontHeight)) >> 1;
            if (this.iStoryMessageY > FIRE_KEY_HEIGHT) {
                this.iStoryMessageY = FIRE_KEY_HEIGHT;
            }
        } else {
            this.iStoryMessageY = messageY;
        }
        this.iLineNum = lineNum;
        if (imgPath != null && imgPath.length() > 0) {
            this.storyImg = Tool.loadPImage(String.valueOf(IMG_PATH) + imgPath + ".p");
            this.iStoryImgY = imgY;
        }
        this.isReturn = isReturn;
        this.bGameState = (byte) 24;
        this.iShowWidth = 0;
        this.iLineIndex = 0;
        this.script.pause();
    }

    public int script_getYear() {
        Calendar cal = Calendar.getInstance();
        Date date = new Date();
        cal.setTime(date);
        return cal.get(1);
    }

    public int script_getMonth() {
        Calendar cal = Calendar.getInstance();
        Date date = new Date();
        cal.setTime(date);
        return cal.get(2) + 1;
    }

    public int script_getDay() {
        Calendar cal = Calendar.getInstance();
        Date date = new Date();
        cal.setTime(date);
        return cal.get(5);
    }

    public int script_getHour() {
        Calendar cal = Calendar.getInstance();
        Date date = new Date();
        cal.setTime(date);
        return cal.get(11);
    }

    public int script_getMinute() {
        Calendar cal = Calendar.getInstance();
        Date date = new Date();
        cal.setTime(date);
        return cal.get(12);
    }

    private class DataAccess {
        private final String FILE_PATH = "/data/data/com.ty.jl/";
        private final byte RECORD_COUNTS = 2;
        private File nameFile = new File("/data/data/com.ty.jl/", "TheSoulOfTheSwordTitle");
        private File[] recordFile = new File[2];
        private File smsFile;
        private File unSaveFile;

        public DataAccess() {
            for (int i = 0; i < 2; i++) {
                this.recordFile[i] = new File("/data/data/com.ty.jl/", GameCanvas.MAIN_RECORD_NAME + GameCanvas.this.sRecordName[i]);
            }
            this.smsFile = new File("/data/data/com.ty.jl/", "TheSoulOfTheSwordSMS");
            this.unSaveFile = new File("/data/data/com.ty.jl/", "TheSoulOfTheSwordunsaved");
        }

        /* JADX INFO: Access modifiers changed from: private */
        public void readRecordNames() {
            try {
                if (this.nameFile.exists()) {
                    FileInputStream fis = new FileInputStream(this.nameFile);
                    try {
                        DataInputStream dis = new DataInputStream(fis);
                        for (int i = 0; i < 2; i++) {
                            try {
                                GameCanvas.this.recordNames[i] = dis.readUTF();
                            } catch (Exception e) {
                                e = e;
                            }
                        }
                        dis.close();
                        return;
                    } catch (Exception e2) {
                        e = e2;
                    }
                } else {
                    return;
                }
            } catch (Exception e3) {
                e = e3;
            }
            e.printStackTrace();
        }

        /* JADX INFO: Access modifiers changed from: private */
        public void saveRecordNames() {
            try {
                if (!this.nameFile.exists()) {
                    this.nameFile.createNewFile();
                }
                FileOutputStream fos = new FileOutputStream(this.nameFile);
                try {
                    DataOutputStream dos = new DataOutputStream(fos);
                    for (int i = 0; i < 2; i++) {
                        try {
                            dos.writeUTF(GameCanvas.this.recordNames[i]);
                        } catch (Exception e) {
                            e = e;
                            e.printStackTrace();
                            return;
                        }
                    }
                    dos.close();
                } catch (Exception e2) {
                    e = e2;
                }
            } catch (Exception e3) {
                e = e3;
            }
        }

        /* JADX INFO: Access modifiers changed from: private */
        public void readRecord(int index) {
            try {
                if (this.recordFile[index].exists()) {
                    System.out.println("-------------readRecord----------------");
                    FileInputStream fis = new FileInputStream(this.recordFile[index]);
                    try {
                        DataInputStream dis = new DataInputStream(fis);
                        try {
                            GameCanvas.this.role = Role.readRole(dis);
                            GameCanvas.this.bag = Bag.readBagData(dis);
                            int temp = dis.readInt();
                            Equip equip = (Equip) GameCanvas.this.bag.equipV.elementAt(temp);
                            GameCanvas.this.role.updateEquipment(equip);
                            if (GameCanvas.this.role.isIntegrate) {
                                GameCanvas.this.role.integrate(true);
                                GameCanvas.this.role.sCDCount[1] = 0;
                                GameCanvas.this.role.updateSkill();
                            }
                            GameCanvas.this.role.iHP = dis.readInt();
                            for (int i = 0; i < GameCanvas.this.iSpecialPropPriceCoefficient.length; i++) {
                                GameCanvas.this.iSpecialPropPriceCoefficient[i] = dis.readInt();
                            }
                            int valueNum = dis.readInt();
                            for (int i2 = 0; i2 < valueNum; i2++) {
                                String key = dis.readUTF();
                                Int value = new Int(dis.readInt());
                                Interpreter.setValue(key, value);
                            }
                            GameCanvas.this.mainMissionTitle = dis.readUTF();
                            GameCanvas.this.mainMissionInfo = dis.readUTF();
                            GameCanvas.this.branchMissionTitle = dis.readUTF();
                            GameCanvas.this.branchMissionInfo = dis.readUTF();
                            for (int i3 = 0; i3 < GameCanvas.this.missionHortation.length; i3++) {
                                for (int j = 0; j < GameCanvas.this.missionHortation[i3].length; j++) {
                                    GameCanvas.this.missionHortation[i3][j] = dis.readInt();
                                }
                            }
                            int size = dis.readInt();
                            for (int i4 = 0; i4 < size; i4++) {
                                int[] stageID = new int[dis.readInt()];
                                for (int j2 = 0; j2 < stageID.length; j2++) {
                                    stageID[j2] = dis.readInt();
                                }
                                int length = dis.readInt();
                                int[] enemyID = new int[length];
                                int[] enemyNum = new int[length];
                                for (int j3 = 0; j3 < enemyID.length; j3++) {
                                    enemyID[j3] = dis.readInt();
                                    enemyNum[j3] = dis.readInt();
                                }
                                String taskOverScript = dis.readUTF();
                                boolean isLoading = dis.readBoolean();
                                Task task = new Task(stageID, enemyID, enemyNum, taskOverScript, isLoading);
                                GameCanvas.this.taskV.addElement(task);
                            }
                            for (int i5 = 0; i5 < GameCanvas.this.isAreaOpened.length; i5++) {
                                GameCanvas.this.isAreaOpened[i5] = dis.readBoolean();
                            }
                            GameCanvas.this.iMissionStageIndex = dis.readInt();
                            GameCanvas.this.bDrawGift[GameCanvas.this.bRecordIndex] = dis.readByte();
                            dis.close();
                            return;
                        } catch (Exception e) {
                            e = e;
                        }
                    } catch (Exception e2) {
                        e = e2;
                    }
                } else {
                    return;
                }
            } catch (Exception e3) {
                e = e3;
            }
            e.printStackTrace();
        }

        /* JADX INFO: Access modifiers changed from: private */
        public void saveRecord(int index) {
            try {
                if (!this.recordFile[index].exists()) {
                    this.recordFile[index].createNewFile();
                }
                System.out.println("------------saveRecord----------------");
                FileOutputStream fos = new FileOutputStream(this.recordFile[index]);
                try {
                    DataOutputStream dos = new DataOutputStream(fos);
                    try {
                        GameCanvas.this.role.saveRoleData(dos);
                        GameCanvas.this.bag.saveBagData(dos);
                        for (int i = 0; i < GameCanvas.this.bag.equipV.size(); i++) {
                            Equip equip = (Equip) GameCanvas.this.bag.equipV.elementAt(i);
                            if (GameCanvas.this.role.equip == equip) {
                                dos.writeInt(i);
                                break;
                            }
                        }
                        dos.writeInt(GameCanvas.this.role.iHP);
                        for (int i2 = 0; i2 < GameCanvas.this.iSpecialPropPriceCoefficient.length; i2++) {
                            dos.writeInt(GameCanvas.this.iSpecialPropPriceCoefficient[i2]);
                        }
                        Object[] obs = Interpreter.getAllValues();
                        int valueNum = obs.length / 2;
                        dos.writeInt(valueNum);
                        for (int i3 = 0; i3 < valueNum; i3++) {
                            dos.writeUTF((String) obs[i3 * 2]);
                            dos.writeInt(((Int) obs[(i3 * 2) + 1]).getVal());
                        }
                        dos.writeUTF(GameCanvas.this.mainMissionTitle);
                        dos.writeUTF(GameCanvas.this.mainMissionInfo);
                        dos.writeUTF(GameCanvas.this.branchMissionTitle);
                        dos.writeUTF(GameCanvas.this.branchMissionInfo);
                        for (int i4 = 0; i4 < GameCanvas.this.missionHortation.length; i4++) {
                            for (int j = 0; j < GameCanvas.this.missionHortation[i4].length; j++) {
                                dos.writeInt(GameCanvas.this.missionHortation[i4][j]);
                            }
                        }
                        dos.writeInt(GameCanvas.this.taskV.size());
                        for (int i5 = 0; i5 < GameCanvas.this.taskV.size(); i5++) {
                            Task task = (Task) GameCanvas.this.taskV.elementAt(i5);
                            dos.writeInt(task.iStageID.length);
                            for (int j2 = 0; j2 < task.iStageID.length; j2++) {
                                dos.writeInt(task.iStageID[j2]);
                            }
                            dos.writeInt(task.iEnemyID.length);
                            for (int j3 = 0; j3 < task.iEnemyID.length; j3++) {
                                dos.writeInt(task.iEnemyID[j3]);
                                dos.writeInt(task.iEnemyNum[j3]);
                            }
                            dos.writeUTF(task.taskOverScript);
                            dos.writeBoolean(task.isLoading);
                        }
                        for (int i6 = 0; i6 < GameCanvas.this.isAreaOpened.length; i6++) {
                            dos.writeBoolean(GameCanvas.this.isAreaOpened[i6]);
                        }
                        dos.writeInt(GameCanvas.this.iMissionStageIndex);
                        dos.writeByte(GameCanvas.this.bDrawGift[GameCanvas.this.bRecordIndex]);
                        dos.close();
                    } catch (Exception e) {
                        e = e;
                        e.printStackTrace();
                    }
                } catch (Exception e2) {
                    e = e2;
                }
            } catch (Exception e3) {
                e = e3;
            }
        }

        /* JADX INFO: Access modifiers changed from: private */
        public void delRecord(int index) {
            if (this.recordFile[index].exists()) {
                this.recordFile[index].delete();
            }
        }

        void saveSMSRecord() {
            try {
                if (!this.smsFile.exists()) {
                    this.smsFile.createNewFile();
                }
                System.out.println("-------------saveSMSRecord-------------");
                FileOutputStream fos = new FileOutputStream(this.smsFile);
                try {
                    DataOutputStream dos = new DataOutputStream(fos);
                    try {
                        dos.writeBoolean(GameCanvas.this.isOpenAllStage);
                        dos.writeBoolean(GameCanvas.this.isDurabilityNonLossForever);
                        dos.writeInt(GameCanvas.this.iGoldBySMS);
                        dos.writeByte(GameCanvas.this.bExpCoefficientBySMS);
                        dos.writeInt(GameCanvas.this.iTotalSum);
                        dos.writeInt(GameCanvas.this.bag.iMoney);
                        dos.close();
                    } catch (Exception e) {
                        e = e;
                        e.printStackTrace();
                    }
                } catch (Exception e2) {
                    e = e2;
                }
            } catch (Exception e3) {
                e = e3;
            }
        }

        /* JADX INFO: Access modifiers changed from: private */
        public void readSMSRecord() {
            try {
                FileInputStream fis = new FileInputStream(this.smsFile);
                try {
                    DataInputStream dis = new DataInputStream(fis);
                    try {
                        System.out.println("-------------readSMSRecord-------------");
                        GameCanvas.this.isOpenAllStage = dis.readBoolean();
                        GameCanvas.this.isDurabilityNonLossForever = dis.readBoolean();
                        GameCanvas.this.iGoldBySMS = dis.readInt();
                        GameCanvas.this.bExpCoefficientBySMS = dis.readByte();
                        GameCanvas.this.iTotalSum = dis.readInt();
                        GameCanvas.this.bag.iMoney = dis.readInt();
                        dis.close();
                    } catch (Exception e) {
                        e = e;
                        e.printStackTrace();
                    }
                } catch (Exception e2) {
                    e = e2;
                }
            } catch (Exception e3) {
                e = e3;
            }
        }

        void saveUnsavedSMSItem() {
            try {
                if (!this.unSaveFile.exists()) {
                    this.unSaveFile.createNewFile();
                }
                System.out.println("-----------------saveUnsavedSMSItem------------------");
                FileOutputStream fos = new FileOutputStream(this.unSaveFile);
                try {
                    DataOutputStream dos = new DataOutputStream(fos);
                    try {
                        dos.writeInt(GameCanvas.this.iGoldUnsaved);
                        dos.writeInt(GameCanvas.this.unsavedEquipV.size());
                        for (int i = 0; i < GameCanvas.this.unsavedEquipV.size(); i++) {
                            Equip equip = (Equip) GameCanvas.this.unsavedEquipV.elementAt(i);
                            equip.saveEquipData(dos);
                        }
                        dos.close();
                    } catch (Exception e) {
                        e = e;
                        e.printStackTrace();
                    }
                } catch (Exception e2) {
                    e = e2;
                }
            } catch (Exception e3) {
                e = e3;
            }
        }

        /* JADX INFO: Access modifiers changed from: private */
        public void readUnsavedSMSItem() {
            try {
                if (this.unSaveFile.exists()) {
                    System.out.println("---------------readUnsavedSMSItem-----------------");
                    FileInputStream fis = new FileInputStream(this.unSaveFile);
                    try {
                        DataInputStream dis = new DataInputStream(fis);
                        try {
                            GameCanvas.this.iGoldUnsaved = dis.readInt();
                            int size = dis.readInt();
                            for (int i = 0; i < size; i++) {
                                Equip equip = Equip.readEquipData(dis);
                                GameCanvas.this.unsavedEquipV.addElement(equip);
                            }
                            dis.close();
                            return;
                        } catch (Exception e) {
                            e = e;
                        }
                    } catch (Exception e2) {
                        e = e2;
                    }
                } else {
                    return;
                }
            } catch (Exception e3) {
                e = e3;
            }
            e.printStackTrace();
        }
    }

    private void readRecordNames(String name) {
        this.access.readRecordNames();
    }

    private void delRecord(int index) {
        this.access.delRecord(index);
    }

    private void saveRecordNames(String name) {
        this.access.saveRecordNames();
    }

    private void readRecord(int index) {
        this.isTimeIn = false;
        this.scrollInfoV.removeAllElements();
        this.access.readRecord(index);
    }

    private void saveRecord(int index) {
        this.access.saveRecord(index);
    }

    private void readSMSRecord() {
        this.access.readSMSRecord();
    }

    private void saveSMSRecord() {
        this.access.saveSMSRecord();
    }

    private void readUnsavedSMSItem() {
        this.access.readUnsavedSMSItem();
    }

    private void saveUnsavedSMSItem() {
        this.access.saveUnsavedSMSItem();
    }

    private Sprite getSpriteTalkTo() {
        for (int i = 0; i < this.spriteOnLayer0.size(); i++) {
            Sprite me = (Sprite) this.spriteOnLayer0.elementAt(i);
            if ((me.bType == 18 || me.bType == 7 || me.bType == 6 || me.bType == 8) && !me.isInvisible && this.role.isInTalkRangeOf(me)) {
                return me;
            }
        }
        for (int i2 = 0; i2 < this.spriteOnLayer1.size(); i2++) {
            Sprite me2 = (Sprite) this.spriteOnLayer1.elementAt(i2);
            if ((me2.bType == 18 || me2.bType == 7 || me2.bType == 6 || me2.bType == 8) && !me2.isInvisible && this.role.isInTalkRangeOf(me2)) {
                return me2;
            }
        }
        return null;
    }

    private void readSpriteBaseData(String path) throws Exception {
        InputStream in = Tool.getResourceAsStream(path);
        DataInputStream din = new DataInputStream(in);
        int num = din.readInt();
        sprBaseData = new SpriteBaseData[num];
        for (short i = 0; i < num; i = (short) (i + 1)) {
            sprBaseData[i] = new SpriteBaseData();
            sprBaseData[i].sortID = din.readShort();
            sprBaseData[i].name = din.readUTF();
            sprBaseData[i].actionFile = din.readUTF();
            String[] picFileName = Tool.getStringArray(din.readUTF(), ';');
            sprBaseData[i].imageFiles = new String[picFileName.length];
            sprBaseData[i].imagePtFiles = new String[picFileName.length];
            for (int j = 0; j < picFileName.length; j++) {
                String[] picFileName2 = Tool.getStringArray(picFileName[j], ',');
                sprBaseData[i].imageFiles[j] = picFileName2[0];
                if (picFileName2.length > 1) {
                    sprBaseData[i].imagePtFiles[j] = picFileName2[1];
                } else {
                    sprBaseData[i].imagePtFiles[j] = "0";
                }
            }
            sprBaseData[i].bType = din.readByte();
            if (sprBaseData[i].bType == 0) {
                this.roleBaseData = sprBaseData[i];
            } else if (sprBaseData[i].bType == 14) {
                this.famulusBaseData = sprBaseData[i];
            }
            if (sprBaseData[i].bType == 10 || sprBaseData[i].bType == 9 || sprBaseData[i].bType == 11) {
                this.fsBaseDataV.addElement(sprBaseData[i]);
            }
        }
        din.close();
        in.close();
    }

    private void readSkillData(String file) throws Exception {
        InputStream in = Tool.getResourceAsStream(file);
        DataInputStream din = new DataInputStream(in);
        int num = din.readInt();
        skillDatas = new SkillData[num];
        for (int i = 0; i < num; i++) {
            skillDatas[i] = new SkillData();
            skillDatas[i].bID = din.readByte();
            skillDatas[i].name = din.readUTF();
            skillDatas[i].description = din.readUTF();
            skillDatas[i].bRank = din.readByte();
            skillDatas[i].damageStr = Tool.getStringArray(din.readUTF(), ',');
            skillDatas[i].amendStr = Tool.getStringArray(din.readUTF(), ',');
            String[] tempStr = Tool.getStringArray(din.readUTF(), ',');
            skillDatas[i].sReducedMP = Short.parseShort(tempStr[0]);
            if (tempStr.length > 1) {
                skillDatas[i].sUpgradeReducedMP = Short.parseShort(tempStr[1]);
            }
            skillDatas[i].sCD = din.readShort();
            skillDatas[i].sDestroyPointForHold = din.readShort();
        }
        din.close();
        in.close();
    }

    private void readRoleAttribute(String path) throws Exception {
        InputStream in = Tool.getResourceAsStream(path);
        DataInputStream din = new DataInputStream(in);
        din.readInt();
        roleData = new AttributeData();
        String[] tempStr = Tool.getStringArray(din.readUTF(), ',');
        roleData.iHP = Integer.parseInt(tempStr[0]);
        roleData.iUpgradeHP = Integer.parseInt(tempStr[1]);
        String[] tempStr2 = Tool.getStringArray(din.readUTF(), ',');
        roleData.sAP = Short.parseShort(tempStr2[0]);
        roleData.sUpgradeAP = Short.parseShort(tempStr2[1]);
        String[] tempStr3 = Tool.getStringArray(din.readUTF(), ',');
        roleData.sDP = Short.parseShort(tempStr3[0]);
        roleData.sUpgradeDP = Short.parseShort(tempStr3[1]);
        String[] tempStr4 = Tool.getStringArray(din.readUTF(), ',');
        roleData.sHitPoint = Short.parseShort(tempStr4[0]);
        roleData.sUpgradeHit = Short.parseShort(tempStr4[1]);
        String[] tempStr5 = Tool.getStringArray(din.readUTF(), ',');
        roleData.sDodgePoint = Short.parseShort(tempStr5[0]);
        roleData.sUpgradeDodge = Short.parseShort(tempStr5[1]);
        roleData.sCriticalPoint = Integer.parseInt(tempStr5[0]);
        roleData.sUpgradeCritical = Short.parseShort(tempStr5[1]);
        din.close();
        in.close();
    }

    private void readFamulusAttribute(String path) throws Exception {
        InputStream in = Tool.getResourceAsStream(path);
        DataInputStream din = new DataInputStream(in);
        int num = din.readInt();
        famulusDatas = new AttributeData[num];
        for (int i = 0; i < num; i++) {
            famulusDatas[i] = new AttributeData();
            String[] tempStr = Tool.getStringArray(din.readUTF(), ',');
            famulusDatas[i].bLowerLimitLevel = Byte.parseByte(tempStr[0]);
            famulusDatas[i].bUpperLimitLevel = Byte.parseByte(tempStr[1]);
            String[] tempStr2 = Tool.getStringArray(din.readUTF(), ',');
            famulusDatas[i].sDurability = Short.parseShort(tempStr2[0]);
            if (tempStr2.length > 1) {
                famulusDatas[i].sUpgradeDurability = Short.parseShort(tempStr2[1]);
            } else {
                famulusDatas[i].sUpgradeDurability = (short) 0;
            }
            String[] tempStr3 = Tool.getStringArray(din.readUTF(), ',');
            famulusDatas[i].iHP = Integer.parseInt(tempStr3[0]);
            if (tempStr3.length > 1) {
                famulusDatas[i].iUpgradeHP = Integer.parseInt(tempStr3[1]);
            } else {
                famulusDatas[i].iUpgradeHP = 0;
            }
            String[] tempStr4 = Tool.getStringArray(din.readUTF(), ',');
            famulusDatas[i].sAP = Short.parseShort(tempStr4[0]);
            if (tempStr4.length > 1) {
                famulusDatas[i].sUpgradeAP = Short.parseShort(tempStr4[1]);
            } else {
                famulusDatas[i].sUpgradeAP = (short) 0;
            }
            String[] tempStr5 = Tool.getStringArray(din.readUTF(), ',');
            famulusDatas[i].sDP = Short.parseShort(tempStr5[0]);
            if (tempStr5.length > 1) {
                famulusDatas[i].sUpgradeDP = Short.parseShort(tempStr5[1]);
            } else {
                famulusDatas[i].sUpgradeDP = (short) 0;
            }
            String[] tempStr6 = Tool.getStringArray(din.readUTF(), ',');
            famulusDatas[i].sHitPoint = Short.parseShort(tempStr6[0]);
            if (tempStr6.length > 1) {
                famulusDatas[i].sUpgradeHit = Short.parseShort(tempStr6[1]);
            } else {
                famulusDatas[i].sUpgradeHit = (short) 0;
            }
            String[] tempStr7 = Tool.getStringArray(din.readUTF(), ',');
            famulusDatas[i].sDodgePoint = Short.parseShort(tempStr7[0]);
            if (tempStr7.length > 1) {
                famulusDatas[i].sUpgradeDodge = Short.parseShort(tempStr7[1]);
            } else {
                famulusDatas[i].sUpgradeDodge = (short) 0;
            }
            String[] tempStr8 = Tool.getStringArray(din.readUTF(), ',');
            famulusDatas[i].sCriticalPoint = Short.parseShort(tempStr8[0]);
            if (tempStr8.length > 1) {
                famulusDatas[i].sUpgradeCritical = Short.parseShort(tempStr8[1]);
            } else {
                famulusDatas[i].sUpgradeCritical = (short) 0;
            }
        }
        din.close();
        in.close();
    }

    private void readEnemyData(String path) throws Exception {
        InputStream in = Tool.getResourceAsStream(path);
        DataInputStream din = new DataInputStream(in);
        int num = din.readInt();
        enemyDatas = new EnemyData[num];
        for (int i = 0; i < num; i++) {
            enemyDatas[i] = new EnemyData();
            enemyDatas[i].sortID = din.readShort();
            enemyDatas[i].sHoldPoint = din.readShort();
            enemyDatas[i].sHPCoefficient = din.readShort();
            enemyDatas[i].sAPCoefficient = din.readShort();
            enemyDatas[i].sHitPoint = din.readByte();
            enemyDatas[i].sDodgePoint = din.readByte();
            enemyDatas[i].sKen = din.readShort();
            String[] tempStr = Tool.getStringArray(din.readUTF(), ';');
            enemyDatas[i].sAttackRange = (short[][]) Array.newInstance((Class<?>) Short.TYPE, tempStr.length, 3);
            enemyDatas[i].bAttackOdds = new byte[tempStr.length];
            enemyDatas[i].bAttackType = new byte[tempStr.length];
            for (int j = 0; j < tempStr.length; j++) {
                if (tempStr[j].length() > 0) {
                    String[] sTem = Tool.getStringArray(tempStr[j], ',');
                    enemyDatas[i].sAttackRange[j][0] = Short.parseShort(sTem[0]);
                    enemyDatas[i].sAttackRange[j][1] = Short.parseShort(sTem[1]);
                    enemyDatas[i].sAttackRange[j][2] = Short.parseShort(sTem[2]);
                    enemyDatas[i].bAttackOdds[j] = Byte.parseByte(sTem[3]);
                    enemyDatas[i].bAttackType[j] = Byte.parseByte(sTem[4]);
                }
            }
            String[] tempStr2 = Tool.getStringArray(din.readUTF(), ',');
            enemyDatas[i].sDamageCoefficient = new short[tempStr2.length];
            for (int j2 = 0; j2 < tempStr2.length; j2++) {
                enemyDatas[i].sDamageCoefficient[j2] = Short.parseShort(tempStr2[j2]);
            }
            String[] tempStr3 = Tool.getStringArray(din.readUTF(), ',');
            enemyDatas[i].sDestroyedHoldPoint = new short[tempStr3.length];
            for (int j3 = 0; j3 < tempStr3.length; j3++) {
                enemyDatas[i].sDestroyedHoldPoint[j3] = Short.parseShort(tempStr3[j3]);
            }
            String[] tempStr4 = Tool.getStringArray(din.readUTF(), ',');
            enemyDatas[i].sAttackInterval = new short[tempStr4.length];
            for (int j4 = 0; j4 < tempStr4.length; j4++) {
                enemyDatas[i].sAttackInterval[j4] = Short.parseShort(tempStr4[j4]);
            }
            enemyDatas[i].sExpCoefficient = din.readShort();
            enemyDatas[i].sDropMoneyCoefficient = din.readShort();
            String[] tempStr5 = Tool.getStringArray(din.readUTF(), ';');
            enemyDatas[i].bDropedEquipOdds = new byte[tempStr5.length];
            enemyDatas[i].bDropedEquipIndex = new byte[tempStr5.length];
            for (int j5 = 0; j5 < tempStr5.length; j5++) {
                if (tempStr5[j5].length() > 0) {
                    String[] sTem2 = Tool.getStringArray(tempStr5[j5], ',');
                    enemyDatas[i].bDropedEquipOdds[j5] = Byte.parseByte(sTem2[0]);
                    if (enemyDatas[i].bDropedEquipOdds[j5] > 0) {
                        enemyDatas[i].bDropedEquipIndex[j5] = Byte.parseByte(sTem2[1]);
                    }
                }
            }
            String[] tempStr6 = Tool.getStringArray(din.readUTF(), ';');
            enemyDatas[i].bDropedItemOdds = new byte[tempStr6.length];
            enemyDatas[i].bDropedItemIndex = new byte[tempStr6.length];
            for (int j6 = 0; j6 < tempStr6.length; j6++) {
                if (tempStr6[j6].length() > 0) {
                    String[] sTem3 = Tool.getStringArray(tempStr6[j6], ',');
                    enemyDatas[i].bDropedItemOdds[j6] = Byte.parseByte(sTem3[0]);
                    if (enemyDatas[i].bDropedItemOdds[j6] > 0) {
                        enemyDatas[i].bDropedItemIndex[j6] = Byte.parseByte(sTem3[1]);
                    }
                }
            }
            for (byte n = 0; n < this.fsBaseDataV.size(); n = (byte) (n + 1)) {
                SpriteBaseData sprBD = (SpriteBaseData) this.fsBaseDataV.elementAt(n);
                if (sprBD.sortID == enemyDatas[i].sortID) {
                    enemyDatas[i].name = sprBD.name;
                    enemyDatas[i].actionFile = sprBD.actionFile;
                    enemyDatas[i].imageFiles = sprBD.imageFiles;
                    enemyDatas[i].imagePtFiles = sprBD.imagePtFiles;
                    enemyDatas[i].bType = sprBD.bType;
                    this.fsBaseDataV.removeElementAt(n);
                    break;
                }
            }
        }
        din.close();
        in.close();
    }

    private void readEnemyAttribute(String path) throws Exception {
        InputStream in = Tool.getResourceAsStream(path);
        DataInputStream din = new DataInputStream(in);
        int num = din.readInt();
        enemyAttributes = new AttributeData[num];
        for (int i = 0; i < num; i++) {
            enemyAttributes[i] = new AttributeData();
            String[] tempStr = Tool.getStringArray(din.readUTF(), ',');
            enemyAttributes[i].iHP = Integer.parseInt(tempStr[0]);
            if (tempStr.length > 1) {
                enemyAttributes[i].iUpgradeHP = Integer.parseInt(tempStr[1]);
            }
            String[] tempStr2 = Tool.getStringArray(din.readUTF(), ',');
            enemyAttributes[i].sAP = Short.parseShort(tempStr2[0]);
            if (tempStr2.length > 1) {
                enemyAttributes[i].sUpgradeAP = Short.parseShort(tempStr2[1]);
            }
            String[] tempStr3 = Tool.getStringArray(din.readUTF(), ',');
            enemyAttributes[i].sDP = Short.parseShort(tempStr3[0]);
            if (tempStr3.length > 1) {
                enemyAttributes[i].sUpgradeDP = Short.parseShort(tempStr3[1]);
            }
            String[] tempStr4 = Tool.getStringArray(din.readUTF(), ',');
            enemyAttributes[i].sHitPoint = Short.parseShort(tempStr4[0]);
            if (tempStr4.length > 1) {
                enemyAttributes[i].sUpgradeHit = Short.parseShort(tempStr4[1]);
            }
            String[] tempStr5 = Tool.getStringArray(din.readUTF(), ',');
            enemyAttributes[i].sDodgePoint = Short.parseShort(tempStr5[0]);
            if (tempStr5.length > 1) {
                enemyAttributes[i].sUpgradeDodge = Short.parseShort(tempStr5[1]);
            }
            enemyAttributes[i].bBeinjuredCoefficient = din.readByte();
        }
        din.close();
        in.close();
    }

    private void readEquipMentData(String file) throws Exception {
        InputStream in = Tool.getResourceAsStream(file);
        DataInputStream din = new DataInputStream(in);
        int num = din.readInt();
        equipDatas = new EquipData[num];
        for (int i = 0; i < num; i++) {
            equipDatas[i] = new EquipData();
            equipDatas[i].bID = din.readByte();
            equipDatas[i].name = din.readUTF();
            equipDatas[i].bGrade = din.readByte();
            equipDatas[i].description = din.readUTF();
            equipDatas[i].resStr = din.readUTF();
            equipDatas[i].bLevel = din.readByte();
            equipDatas[i].bSkillIndex = din.readByte();
            equipDatas[i].sDurability = din.readShort();
            equipDatas[i].sAddHP = din.readShort();
            equipDatas[i].sAddAP = din.readShort();
            equipDatas[i].sAddDP = din.readShort();
            equipDatas[i].sAddHitP = din.readShort();
            equipDatas[i].sAddDodgeP = din.readShort();
            equipDatas[i].sAddCriticalP = din.readShort();
            equipDatas[i].iPurchasePrice = din.readInt();
            equipDatas[i].iSellPrice = din.readInt();
            String[] addtiveEffectStr = Tool.getStringArray(din.readUTF(), ';');
            equipDatas[i].bAddtiveEffectType = new byte[addtiveEffectStr.length];
            equipDatas[i].sAddtiveEffectPoint = new short[addtiveEffectStr.length];
            for (int j = 0; j < addtiveEffectStr.length; j++) {
                if (addtiveEffectStr[j].length() > 0) {
                    String[] sTem = Tool.getStringArray(addtiveEffectStr[j], ',');
                    equipDatas[i].bAddtiveEffectType[j] = Byte.parseByte(sTem[0]);
                    if (sTem.length > 1) {
                        equipDatas[i].sAddtiveEffectPoint[j] = Short.parseShort(sTem[1]);
                    }
                }
            }
            equipDatas[i].iconIDInfo = Tool.getStringArray(din.readUTF(), ',');
        }
        din.close();
        in.close();
    }

    private void readPropData(String file) throws Exception {
        InputStream in = Tool.getResourceAsStream(file);
        DataInputStream din = new DataInputStream(in);
        int num = din.readInt();
        propDatas = new PropData[num];
        for (int i = 0; i < num; i++) {
            propDatas[i] = new PropData();
            propDatas[i].bID = din.readByte();
            propDatas[i].name = din.readUTF();
            propDatas[i].description = din.readUTF();
            propDatas[i].iPurchasePrice = din.readInt();
            propDatas[i].iSellPrice = din.readInt();
            propDatas[i].sHPUp = din.readShort();
            propDatas[i].sMPUp = din.readShort();
            propDatas[i].effectStr = din.readUTF();
            propDatas[i].bType = din.readByte();
        }
        din.close();
        in.close();
    }

    private void initMapElement() throws Exception {
        this.mapScripts.removeAllElements();
        this.spriteBelowLayer0.removeAllElements();
        this.spriteOnLayer0.removeAllElements();
        this.spriteOnLayer1.removeAllElements();
        this.spriteOnLayer2.removeAllElements();
        this.enemyInScreen.removeAllElements();
        this.bulletInScreen.removeAllElements();
        this.effectV.removeAllElements();
        this.sprsByScriptControlV.removeAllElements();
        this.fightElement.removeAllElements();
        for (int i = 0; i < this.spriteInStage.size(); i++) {
            Sprite me = (Sprite) this.spriteInStage.elementAt(i);
            if (me != null) {
                me.freeSprite();
            }
        }
        Common.clearResCache.clear();
        this.spriteInStage.removeAllElements();
        this.role.loadRes();
        this.role.clear();
        addMapElement(this.role);
        if (this.role.famulus != null) {
            this.role.famulus.loadRes();
            addMapElement(this.role.famulus);
        }
        this.isAttackable = true;
        this.isDrawBossHpBar = false;
        this.isDurabilityNonLoss = false;
        this.isMaximumPower = false;
    }

    public void addFightElement(Sprite me) {
        this.fightElement.addElement(me);
    }

    public void addMapElement(Sprite me) {
        this.spriteInStage.addElement(me);
    }

    public void addDropInfo(String str) {
        if (str != null && str.length() > 0) {
            for (int i = 1; i >= 0; i--) {
                if (this.dropInfoCount[1][i] > 0) {
                    StyledText stTem = this.dropInfo[i + 1];
                    this.dropInfo[i + 1] = this.dropInfo[i];
                    this.dropInfo[i] = stTem;
                    this.dropInfoCount[0][i + 1] = this.dropInfoCount[0][i];
                    this.dropInfoCount[1][i + 1] = this.dropInfoCount[1][i];
                }
            }
            this.dropInfo[0].formatText(str);
            this.dropInfoCount[0][0] = SCREEN_WIDTH;
            this.dropInfoCount[1][0] = this.DROP_INFO_TIME;
        }
    }

    public int dropMoney(Enemy e) {
        int money = ((e.sLevel + 20) * 20) / 20;
        if (money > 100) {
            money = 100;
        }
        int money2 = (e.enemyData.sDropMoneyCoefficient * money) / 100;
        int money3 = money2 + Tool.getNextRnd((money2 * (-5)) / 100, (money2 * 5) / 100);
        if (this.role.sLevel > e.sLevel + 9) {
            return 10;
        }
        return money3;
    }

    void dropItem(Enemy e) {
        NPCSprite spr2;
        NPCSprite spr3;
        if (this.isDropItem) {
            Vector dropItemV = new Vector(9);
            if (Tool.getNextRnd(0, 100) < (this.isExpRoom ? 80 : 40)) {
                dropItemV.addElement(new Integer(dropMoney(e)));
            }
            if (e.sLevel > this.role.sLevel - 10) {
                for (int i = 0; i < e.enemyData.bDropedEquipOdds.length; i++) {
                    if (Tool.getNextRnd(0, 100) < (this.isExpRoom ? e.enemyData.bDropedEquipOdds[i] << 1 : e.enemyData.bDropedEquipOdds[i])) {
                        Equip equip = new Equip(equipDatas[e.enemyData.bDropedEquipIndex[i]], 0, true);
                        dropItemV.addElement(equip);
                        break;
                    }
                }
                for (int i2 = 0; i2 < e.enemyData.bDropedItemOdds.length; i2++) {
                    if (Tool.getNextRnd(0, 100) < (this.isExpRoom ? e.enemyData.bDropedItemOdds[i2] << 1 : e.enemyData.bDropedItemOdds[i2])) {
                        Prop prop = new Prop(propDatas[e.enemyData.bDropedItemIndex[i2]], 1);
                        dropItemV.addElement(prop);
                    }
                }
            }
            int num = dropItemV.size();
            if (num > 0) {
                if (num == 1) {
                    Object obj = dropItemV.elementAt(0);
                    if (obj instanceof Equip) {
                        SpriteBaseData equipData = new SpriteBaseData();
                        equipData.bType = (byte) 17;
                        equipData.actionFile = "z_weapondrop";
                        String[] temp = Tool.getStringArray(((Equip) obj).data.resStr, ',');
                        equipData.imageFiles = new String[2];
                        equipData.imageFiles[0] = temp[0];
                        equipData.imageFiles[1] = "i_00";
                        equipData.imagePtFiles = new String[2];
                        if (temp.length > 1 && temp[1] != null) {
                            equipData.imagePtFiles[0] = temp[1];
                        } else {
                            equipData.imagePtFiles[0] = "0";
                        }
                        equipData.imagePtFiles[1] = "0";
                        spr3 = new NPCSprite((-100) - num, equipData, e.iMapX, e.iMapY);
                        spr3.item = obj;
                    } else {
                        spr3 = new NPCSprite((-100) - num, (byte) 13, e.iMapX + ((e.iFootWidth - 22) >> 1), e.iMapY + ((e.iFootHeight - 22) >> 1), (byte) 15, (byte) 10, 300, this.isDropVertical ? (byte) 2 : (byte) Tool.getNextRnd(0, 2), obj);
                    }
                    spr3.iLayerIndex = 2;
                    addMapElement(spr3);
                    return;
                }
                Vector v = new Vector(9);
                for (int i3 = 0; i3 < 9; i3++) {
                    v.addElement(new Integer(i3));
                }
                for (int i4 = 0; i4 < 9; i4++) {
                    int rNum = Tool.getNextRnd(0, v.size());
                    Integer itg = (Integer) v.elementAt(rNum);
                    v.removeElementAt(rNum);
                    int posX = e.iMapX + ((e.iFootWidth - 22) >> 1) + (((itg.intValue() % 3) - 1) * DROP_POS_INTERVAL);
                    int posY = e.iMapY + ((e.iFootHeight - 22) >> 1) + (((itg.intValue() / 3) - 1) * DROP_POS_INTERVAL);
                    if (posX < 15) {
                        posX = 15;
                    }
                    if (posX > this.map.sMapWidth - 15) {
                        posX = this.map.sMapWidth - 15;
                    }
                    if (posY < 15) {
                        posY = 15;
                    }
                    if (posY > this.map.sMapHeight - 15) {
                        posY = this.map.sMapHeight - 15;
                    }
                    num--;
                    Object obj2 = dropItemV.elementAt(num);
                    if (obj2 instanceof Equip) {
                        SpriteBaseData equipData2 = new SpriteBaseData();
                        equipData2.bType = (byte) 17;
                        equipData2.actionFile = "z_weapondrop";
                        equipData2.imageFiles = new String[2];
                        equipData2.imageFiles[0] = this.role.weaponStr[0];
                        equipData2.imageFiles[1] = "i_00";
                        equipData2.imagePtFiles = new String[2];
                        if (this.role.weaponStr.length > 1 && this.role.weaponStr[1] != null) {
                            equipData2.imagePtFiles[0] = this.role.weaponStr[1];
                        } else {
                            equipData2.imagePtFiles[0] = "0";
                        }
                        equipData2.imagePtFiles[1] = "0";
                        spr2 = new NPCSprite((-100) - num, equipData2, e.iMapX, e.iMapY);
                        spr2.item = obj2;
                    } else {
                        spr2 = new NPCSprite((-100) - num, (byte) 13, posX, posY, (byte) 15, (byte) 10, 300, this.isDropVertical ? (byte) 2 : (byte) Tool.getNextRnd(0, 2), obj2);
                    }
                    spr2.iLayerIndex = 2;
                    addMapElement(spr2);
                    if (num <= 0) {
                        return;
                    }
                }
            }
        }
    }

    public void getMoney(int num) {
        this.bag.iMoney += num;
    }

    void getItem(Object item) {
        if (item instanceof Integer) {
            int iDropMoney = ((Integer) item).intValue();
            getMoney(iDropMoney);
            addDropInfo("银两+" + iDropMoney);
            return;
        }
        String itemName = null;
        if (item instanceof Equip) {
            this.bag.addEquip((Equip) item);
            itemName = ((Equip) item).nameInfo();
        } else if (item instanceof Prop) {
            this.bag.addProp(((Prop) item).data, 1);
            itemName = ((Prop) item).nameInfo();
        }
        addDropInfo("获得 " + itemName);
    }

    void gameOver() {
        if (this.role.overScript != null) {
            this.role.initAct();
            if (this.role.specialScript != null) {
                script_call(this.role.specialScript, false);
                return;
            } else {
                script_call(this.role.overScript, false);
                return;
            }
        }
        boolean isHaveMedicine = false;
        for (int i = 0; i < this.bag.propV.size(); i++) {
            Prop prop = (Prop) this.bag.propV.elementAt(i);
            if (prop.data.bID == 9) {
                isHaveMedicine = true;
                useProp(prop);
                break;
            }
        }
        if (!isHaveMedicine) {
            if (this.isOpenAllStage || this.isDX) {
                initSMS(2);
                return;
            }
            if (Interpreter.getValue(SCRIPT_VARIABLES[2]).getVal() == 1) {
                this.role.reLive();
                return;
            }
            Bitmap[] imgs = {Tool.loadPImage(String.valueOf(ACTION_PATH) + "gameover.p")};
            this.overAS = ActionSet.createActionSet(String.valueOf(ACTION_PATH) + "gameover.n", imgs);
            this.bOverCount = 0;
            this.bOverDuration = (byte) (this.overAS.actionDatas[0].frameNum - 1);
            this.bGameState = (byte) 22;
        }
    }

    void drawMessageOnBlackScreen(Graphics g) {
        g.setColor(0);
        g.fillRect(0, 0, SCREEN_WIDTH, SCREEN_HEIGHT);
        if (this.storyImg != null) {
            g.setClip(0, this.iStoryImgY, this.storyImg.getWidth(), this.storyImg.getHeight());
            g.drawImage(this.storyImg, 0, this.iStoryImgY, 0);
            g.restore();
        }
        if (this.blackScreenMessage != null) {
            for (int i = 0; i <= this.iLineIndex; i++) {
                boolean isSave = false;
                if (i == this.iLineIndex) {
                    if (this.iLineIndex < this.iLineNum) {
                        isSave = true;
                        g.setClip(10, this.iStoryMessageY + (FontHeight * i), this.iShowWidth, FontHeight);
                    } else {
                        isSave = true;
                        g.setClip(10, this.iStoryMessageY + (FontHeight * (i - (this.iLineNum * this.iPageNum))), this.iShowWidth, FontHeight);
                    }
                } else if (i < this.iLineIndex) {
                    if (this.iLineIndex < this.iLineNum) {
                        isSave = true;
                        g.setClip(10, this.iStoryMessageY + (FontHeight * i), SCREEN_WIDTH - 20, FontHeight);
                    } else if (i >= this.iLineNum * this.iPageNum) {
                        isSave = true;
                        g.setClip(10, this.iStoryMessageY + (FontHeight * (i - (this.iLineNum * this.iPageNum))), SCREEN_WIDTH - 20, FontHeight);
                    }
                }
                g.setColor(-1);
                if (i >= this.iLineNum * this.iPageNum && i < this.blackScreenMessage.length) {
                    g.drawString(this.blackScreenMessage[i], 10, this.iStoryMessageY + (FontHeight * (i - (this.iLineNum * this.iPageNum))), 0);
                }
                if (isSave) {
                    g.restore();
                }
            }
        }
    }

    void cycleBlackScreenMessage() {
        if (this.blackScreenMessage != null) {
            if (this.iLineIndex < this.blackScreenMessage.length) {
                if (this.iShowWidth < font.stringWidth(this.blackScreenMessage[this.iLineIndex]) - 2) {
                    this.iShowWidth += 2;
                    return;
                }
                this.iLineIndex++;
                this.iShowWidth = 0;
                if (this.iLineNum > 0) {
                    this.iPageNum = this.iLineIndex / this.iLineNum;
                    if (this.iLineIndex % this.iLineNum == this.iLineNum - 1 || this.iLineIndex == this.blackScreenMessage.length - 1) {
                        this.bStayCounter = (byte) 0;
                        return;
                    }
                    return;
                }
                return;
            }
            if (this.bStayCounter < 20) {
                this.bStayCounter = (byte) (this.bStayCounter + 1);
                return;
            }
            this.bStayCounter = (byte) 0;
            this.blackScreenMessage = null;
            this.iPageNum = 0;
            if (this.storyImg != null) {
                this.storyImg = null;
            }
            if (this.isReturn) {
                this.bGameState = (byte) 12;
            } else {
                setScriptState((byte) 3);
            }
        }
    }

    void handleKey_BlackScreenMessage() {
        if (Common.isOKPressed(true) && this.blackScreenMessage != null && this.iLineIndex < this.blackScreenMessage.length) {
            if ((this.iLineNum > 0 && this.iLineIndex % this.iLineNum == this.iLineNum - 1) || this.iLineIndex == this.blackScreenMessage.length - 1) {
                this.bStayCounter = (byte) 0;
                this.iShowWidth = font.stringWidth(this.blackScreenMessage[this.iLineIndex]);
            } else {
                this.iLineIndex++;
                if (this.iLineNum > 0) {
                    this.iPageNum = this.iLineIndex / this.iLineNum;
                }
                this.iShowWidth = 0;
            }
        }
    }

    private void drawGameOver(Graphics g) {
        this.overAS.drawFrameCycle(g, this.bOverIndex, this.bOverCount, SCREEN_WIDTH >> 1, SCREEN_HEIGHT >> 1);
    }

    private void cycleGameOver() {
        if (this.bOverIndex == 0) {
            if (this.bOverCount < this.bOverDuration) {
                this.bOverCount++;
            } else {
                this.bOverIndex = 1;
                this.bOverCount = 0;
            }
        }
    }

    private class LoadingCycle extends Thread {
        private boolean flag;
        private SurfaceHolder surfaceHolder;

        public LoadingCycle(SurfaceHolder surfaceHolder) {
            this.surfaceHolder = surfaceHolder;
        }

        @Override // java.lang.Thread, java.lang.Runnable
        public void run() {
            while (true) {
                GameCanvas.this.iLoadingCounter++;
                Canvas canvas = null;
                try {
                    canvas = this.surfaceHolder.lockCanvas();
                    synchronized (this.surfaceHolder) {
                        try {
                            GameCanvas.this.onDraw(canvas);
                        } catch (Throwable th) {
                            throw th;
                        }
                    }
                    try {
                        Thread.sleep(GameCanvas.MILLIS_PRE_UPDATE);
                    } catch (InterruptedException e) {
                        e.printStackTrace();
                    }
                    if (canvas != null) {
                        this.surfaceHolder.unlockCanvasAndPost(canvas);
                    }
                    if (GameCanvas.this.isLoadFinish) {
                        GameCanvas.this.cycleSpriteInStage();
                        if (GameCanvas.this.iLoadOverCounter > 0) {
                            GameCanvas.this.iLoadOverCounter--;
                        } else {
                            return;
                        }
                    }
                } catch (Throwable th2) {
                    if (canvas != null) {
                        this.surfaceHolder.unlockCanvasAndPost(canvas);
                    }
                    if (GameCanvas.this.isLoadFinish) {
                        GameCanvas.this.cycleSpriteInStage();
                        if (GameCanvas.this.iLoadOverCounter > 0) {
                            GameCanvas.this.iLoadOverCounter--;
                        } else {
                            return;
                        }
                    }
                    throw th2;
                }
            }
        }
    }

    private void sendSms() {
        this.sms.sendMessage(this.content[this.smsId], "10659811002");
        smsSendOver();
    }

    private void smsSendOver() {
        if (GameCenterEvent.isSucceed == 1) {
            this.isSendSuccessful = true;
            getItemBySMS(this.sendSmsId - 1);
            this.tipInfoST = new StyledText(280, SCREEN_HEIGHT - 90, FontHeight, font);
            StringBuffer sb = new StringBuffer("\n        购买成功，请注意保存，祝您游戏愉快。\n");
            if (this.bDrawGift[this.bRecordIndex] == 0) {
                if (this.iTotalSum < 6) {
                    sb.append("        还差<c=ff0000>" + (6 - this.iTotalSum) + "</c>元就可以领取白银礼包了哦！");
                } else {
                    sb.append("        <c=00ff00>您已可以领取白银礼包了哦</c>！");
                }
            } else if (this.bDrawGift[this.bRecordIndex] == 1) {
                if (this.iTotalSum < 12) {
                    sb.append("        还差<c=ff0000>" + (12 - this.iTotalSum) + "</c>元就可以领取黄金礼包了哦！");
                } else {
                    sb.append("        <c=00ff00>您已可以领取黄金礼包了哦</c>！");
                }
            } else if (this.bDrawGift[this.bRecordIndex] == 2) {
                if (this.iTotalSum < 20) {
                    sb.append("        还差<c=ff0000>" + (20 - this.iTotalSum) + "</c>元就可以领取钻石大礼包了哦！");
                } else {
                    sb.append("        <c=00ff00>您已可以领取钻石大礼包了哦</c>！");
                }
            }
            this.tipInfoST.formatText(sb.toString());
        } else {
            this.isSendSuccessful = false;
            this.tipInfoST = new StyledText(SCREEN_WIDTH - 40, SCREEN_HEIGHT - 90, FontHeight, font);
            this.tipInfoST.formatText("\n        发送失败。");
        }
        if (this.bGameState == 16) {
            playMusic();
        }
        this.bGameState = STATE_SMS_CONFIRM;
    }

    private void createSMSShopMenu() {
        this.isSend = false;
        switch (this.bSMSShopIndex) {
            case 0:
                this.bAmount = (byte) 1;
                this.bSMSIndex = (byte) 0;
                this.bSMSType = (byte) 6;
                this.bItemSum = (byte) 2;
                break;
            case 1:
                this.bSMSType = (byte) 3;
                this.bItemSum = (byte) 1;
                this.bAmount = (byte) 1;
                break;
            case 2:
                this.bSMSIndex = (byte) 2;
                this.bSMSType = (byte) 2;
                this.bItemSum = (byte) 1;
                this.bAmount = (byte) 1;
                break;
            case 3:
                if (this.bExpCoefficientBySMS < 4) {
                    this.bSMSIndex = (byte) 1;
                    this.bSMSType = (byte) 13;
                    this.bItemSum = (byte) 2;
                    this.bAmount = (byte) 1;
                }
                break;
        }
        this.bGameState = (byte) 31;
    }

    private void drawSMSShop(Graphics g) {
        int menuX = (SCREEN_WIDTH - 320) >> 1;
        int menuY = (SCREEN_HEIGHT - 240) >> 1;
        this.mainUIAS.drawFrame(g, 16, SCREEN_WIDTH >> 1, SCREEN_HEIGHT >> 1);
        int frame_top = menuY + 52;
        int frame_left = menuX + 165;
        int option_top = frame_top + 22;
        int option_left = menuX + 50;
        int optionNum = 3;
        if (this.bExpCoefficientBySMS < 4 && this.bDrawGift[this.bRecordIndex] < 3) {
            optionNum = 5;
            option_top = frame_top - 9;
        } else if (this.bDrawGift[this.bRecordIndex] < 3 || this.bExpCoefficientBySMS < 4) {
            optionNum = 4;
            option_top = frame_top + 6;
        }
        int i = 0;
        while (i < optionNum) {
            drawRoundRectBGByTile(g, this.uiBGImg, option_left, option_top + (i * 31), this.uiBGImg.getHeight(), this.uiBGImg.getHeight(), 80, 28);
            if (i < 3) {
                Tool.drawImage(g, this.bSMSShopIndex == i ? this.optionSMImg[i + 6] : this.optionSMImg[i], option_left + 15, (i * 31) + option_top + 9, 0);
            } else if (this.bExpCoefficientBySMS < 4) {
                Tool.drawImage(g, this.bSMSShopIndex == 3 ? this.optionSMImg[9] : this.optionSMImg[3], option_left + 15, option_top + 93 + 9, 0);
                if (this.bDrawGift[this.bRecordIndex] < 3) {
                    Tool.drawClipImage(g, this.bSMSShopIndex == 4 ? this.optionSMImg[10] : this.optionSMImg[4], option_left + 15, option_top + 124 + 9, this.bDrawGift[this.bRecordIndex] * 25, 0, 25, 13);
                    Tool.drawImage(g, this.bSMSShopIndex == 4 ? this.optionSMImg[11] : this.optionSMImg[5], option_left + 40, option_top + 124 + 9, 0);
                }
            } else if (this.bDrawGift[this.bRecordIndex] < 3) {
                Tool.drawClipImage(g, this.bSMSShopIndex == 3 ? this.optionSMImg[10] : this.optionSMImg[4], option_left + 15, option_top + 93 + 9, this.bDrawGift[this.bRecordIndex] * 25, 0, 25, 13);
                Tool.drawImage(g, this.bSMSShopIndex == 3 ? this.optionSMImg[11] : this.optionSMImg[5], option_left + 40, option_top + 93 + 9, 0);
            }
            i++;
        }
        if (this.bSMSShopIndex < 3) {
            Tool.drawImage(g, this.infoSMImg[this.bSMSShopIndex], frame_left + 8, frame_top + 5, 0);
            if (this.bSMSShopIndex == 0) {
                if (this.iGoldBySMS < DROP_POS_INTERVAL) {
                    Tool.drawImage(g, this.infoSMImg[5], frame_left + 8, frame_top + 12 + this.infoSMImg[0].getHeight(), 0);
                }
                Tool.drawNumReturnWidth(g, this.numImg[1], this.iGoldBySMS * 10000, menuX + 221, menuY + 57, 7, 0, -1);
                Tool.drawNumReturnWidth(g, this.numImg[0], this.iGoldBySMS, menuX + Tool.UI_RATATE_180, menuY + 76, 7, 0, -1);
            }
        } else if (this.bSMSShopIndex == 3) {
            if (this.bExpCoefficientBySMS < 4) {
                Tool.drawImage(g, this.infoSMImg[3], frame_left + 8, frame_top + 12, 0);
                Tool.drawNumReturnWidth(g, this.numImg[1], (this.bExpCoefficientBySMS + 1) * 100, menuX + 221, menuY + 64, 3, 0, -1);
            } else {
                Tool.drawImage(g, this.infoSMImg[4], frame_left + 8, frame_top + 25, 0);
                if (this.bDrawGift[this.bRecordIndex] == 0) {
                    Tool.drawNumReturnWidth(g, this.numImg[0], 6L, menuX + 218, menuY + 77, 3, 2, -1);
                } else if (this.bDrawGift[this.bRecordIndex] == 1) {
                    Tool.drawNumReturnWidth(g, this.numImg[0], 12L, menuX + 218, menuY + 77, 3, 2, -1);
                } else {
                    Tool.drawNumReturnWidth(g, this.numImg[0], 20L, menuX + 218, menuY + 77, 3, 2, -1);
                }
            }
        } else {
            Tool.drawImage(g, this.infoSMImg[4], frame_left + 8, frame_top + 25, 0);
            if (this.bDrawGift[this.bRecordIndex] == 0) {
                Tool.drawNumReturnWidth(g, this.numImg[0], 6L, menuX + 218, menuY + 77, 3, 2, -1);
            } else if (this.bDrawGift[this.bRecordIndex] == 1) {
                Tool.drawNumReturnWidth(g, this.numImg[0], 12L, menuX + 218, menuY + 77, 3, 2, -1);
            } else {
                Tool.drawNumReturnWidth(g, this.numImg[0], 20L, menuX + 218, menuY + 77, 3, 2, -1);
            }
        }
        if (this.bSMSShopIndex < 3 || (this.bSMSShopIndex == 3 && this.bExpCoefficientBySMS < 4)) {
            Tool.drawImage(g, this.tipSMImg, frame_left + 7, frame_top + 78, 0);
            Tool.drawNumReturnWidth(g, this.numImg[1], this.bItemSum, menuX + 262, menuY + 132, 3, 2, -1);
            Tool.drawNumReturnWidth(g, this.numImg[1], this.bAmount, menuX + 214, menuY + 149, 3, 2, -1);
            Tool.drawNumReturnWidth(g, this.numImg[1], this.bItemSum / this.bAmount, menuX + 268, menuY + 149, 3, 2, -1);
        }
        Tool.drawNumReturnWidth(g, this.numImg[0], this.bag.iMoney / 10000, menuX + 140, menuY + 218, 6, 1, -1);
        Tool.drawNumReturnWidth(g, this.numImg[1], this.bag.iMoney % 10000, menuX + 187, menuY + 218, 6, 1, -1);
        Tool.drawClipImage(g, this.moneyImg, menuX + 142, menuY + 218, 0, 0, this.moneyImg.getWidth() >> 1, this.moneyImg.getHeight());
        Tool.drawClipImage(g, this.moneyImg, menuX + 189, menuY + 218, this.moneyImg.getWidth() >> 1, 0, this.moneyImg.getWidth() >> 1, this.moneyImg.getHeight());
        if (this.isSend) {
            Tool.drawClipImage(g, this.SmsImage, (SCREEN_WIDTH - this.SmsImage.getWidth()) >> 1, ((SCREEN_HEIGHT - this.SmsImage.getHeight()) >> 1) + 70, 0, 0, this.SmsImage.getWidth(), this.SmsImage.getHeight());
        }
    }

    private void handleKey_SMSShop() {
        byte b;
        byte b2;
        byte optionNum = 3;
        if (this.bExpCoefficientBySMS < 4 && this.bDrawGift[this.bRecordIndex] < 3) {
            optionNum = 5;
        } else if (this.bDrawGift[this.bRecordIndex] < 3 || this.bExpCoefficientBySMS < 4) {
            optionNum = 4;
        }
        if (Common.isUpPressed(true)) {
            if (this.bSMSShopIndex == 0) {
                b2 = (byte) (optionNum + STATE_CG);
            } else {
                b2 = (byte) (this.bSMSShopIndex + STATE_CG);
                this.bSMSShopIndex = b2;
            }
            this.bSMSShopIndex = b2;
        }
        if (Common.isDownPressed(true)) {
            if (this.bSMSShopIndex == ((byte) (optionNum + STATE_CG))) {
                b = 0;
            } else {
                b = (byte) (this.bSMSShopIndex + 1);
                this.bSMSShopIndex = b;
            }
            this.bSMSShopIndex = b;
        }
        switch (this.bSMSShopIndex) {
            case 0:
                this.bAmount = (byte) 1;
                this.bSMSIndex = (byte) 0;
                this.bSMSType = (byte) 6;
                this.bItemSum = (byte) 2;
                break;
            case 1:
                this.bSMSType = (byte) 3;
                this.bItemSum = (byte) 1;
                this.bAmount = (byte) 1;
                break;
            case 2:
                this.bSMSIndex = (byte) 2;
                this.bSMSType = (byte) 2;
                this.bItemSum = (byte) 1;
                this.bAmount = (byte) 1;
                break;
            case 3:
                if (this.bExpCoefficientBySMS < 4) {
                    this.bSMSIndex = (byte) 1;
                    this.bSMSType = (byte) 13;
                    this.bItemSum = (byte) 2;
                    this.bAmount = (byte) 1;
                }
                break;
        }
        if (Common.isKeyPressed(9, true)) {
            this.bool = false;
            switch (this.bSMSShopIndex) {
                case 0:
                    this.bool = true;
                    this.sendSmsId = 5;
                    break;
                case 1:
                    if (this.role.sLevel < 21) {
                        initEquipPreview(5, 9, false);
                    } else if (this.role.sLevel < 31) {
                        initEquipPreview(8, 9, false);
                    } else if (this.role.sLevel < 41) {
                        initEquipPreview(11, 9, false);
                    } else if (this.role.sLevel < 51) {
                        initEquipPreview(14, 9, false);
                    } else if (this.role.sLevel < 61) {
                        initEquipPreview(17, 9, false);
                    } else {
                        initEquipPreview(20, 9, false);
                    }
                    this.bEntryIndex = (byte) 2;
                    break;
                case 2:
                    this.bool = true;
                    this.sendSmsId = 1;
                    break;
                case 3:
                    if (this.bExpCoefficientBySMS < 4) {
                        this.bool = true;
                        this.sendSmsId = 3;
                        break;
                    } else {
                        switch (this.bDrawGift[this.bRecordIndex]) {
                            case 0:
                                initSMS(5);
                                break;
                            case 1:
                                initSMS(6);
                                break;
                            case 2:
                                initSMS(7);
                                break;
                        }
                    }
                    break;
                case 4:
                    switch (this.bDrawGift[this.bRecordIndex]) {
                        case 0:
                            initSMS(5);
                            break;
                        case 1:
                            initSMS(6);
                            break;
                        case 2:
                            initSMS(7);
                            break;
                    }
                    break;
            }
            if (this.isSend) {
                this.isSend = false;
                WebNetInterface.SMSBillingPoint(this.sendSmsId, "1" + System.currentTimeMillis());
                this.tipInfoST = new StyledText(SCREEN_WIDTH - 40, SCREEN_HEIGHT - 90, FontHeight, font);
                this.tipInfoST.formatText("\n        短信发送中...请稍侯");
                setGameState(STATE_SMS_WAITING);
                return;
            }
            if (this.bool) {
                this.isSend = true;
                return;
            }
            return;
        }
        if (Common.isKeyPressed(10, true)) {
            this.bGameState = (byte) 12;
        }
    }

    private void initSMS(int index) {
        this.bSMSIndex = (byte) index;
        StringBuffer sb = new StringBuffer();
        boolean bool = false;
        switch (this.bSMSIndex) {
            case 0:
                sb.append("\n        钱不够用？立刻获得<c=ffff00>");
                sb.append(this.iGoldBySMS);
                sb.append("</c>金（");
                sb.append(this.iGoldBySMS * 10000);
                sb.append("银），金山银海等你来挥霍！");
                if (this.iGoldBySMS < DROP_POS_INTERVAL) {
                    sb.append("<c=00ff00>下次购买额外再送<c=00ff00>");
                    sb.append(2);
                    sb.append("</c>金哦，亲！</c>");
                }
                bool = true;
                this.bSMSType = (byte) 6;
                this.bItemSum = (byte) 2;
                this.sendSmsId = 5;
                this.bAmount = (byte) 1;
                break;
            case 1:
                sb.append("\n        立刻获得");
                if (this.bExpCoefficientBySMS == 1) {
                    sb.append("<c=00ff00>200</c>");
                } else if (this.bExpCoefficientBySMS == 2) {
                    sb.append("<c=00ff00>300</c>");
                } else if (this.bExpCoefficientBySMS == 3) {
                    sb.append("<c=00ff00>400</c>");
                }
                sb.append("%的升级速度，对主角与剑灵均有效哦！");
                bool = true;
                this.bSMSType = (byte) 13;
                this.bItemSum = (byte) 2;
                this.bAmount = (byte) 1;
                this.sendSmsId = 3;
                break;
            case 2:
                if (this.role.bActID == 3) {
                    sb.append("        倒下不是你的错，但让敌人嚣张就是你的不对啦！立刻满状态原地复活。\n");
                    sb.append("\n        <c=00ff00>当前场景内怒气值始终为满值且使用任何技能（<c=ffff00>包括必杀和崩击</c>）均不消耗武器耐久</c>！");
                } else {
                    sb.append("\n        立即开启无限超杀，气血完全恢复！\n");
                    sb.append("\n        <c=00ff00>当前场景内怒气值始终为满值且使用任何技能（<c=ffff00>包括必杀和崩击</c>）均不消耗武器耐久</c>！");
                }
                bool = true;
                this.sendSmsId = 1;
                this.bSMSType = (byte) 2;
                this.bItemSum = (byte) 1;
                this.bAmount = (byte) 1;
                break;
            case 3:
                if (JL01Activity.activity.smsType == 1) {
                    this.bItemSum = (byte) 4;
                    this.bAmount = (byte) 2;
                    sb.append("\n        <c=00ff00>藏剑门最强绝招的奥秘究竟是什么？古小天和剑灵的冒险才刚刚开始！立刻开启后续游戏流程，只需支付信息费4元，需发送2条短信，2元/条（不含通信费）");
                } else {
                    this.bItemSum = (byte) 4;
                    this.bAmount = (byte) 1;
                    sb.append("\n        <c=00ff00>藏剑门最强绝招的奥秘究竟是什么？古小天和剑灵的冒险才刚刚开始！立刻开启后续游戏流程，只需支付信息费4元，需发送1条短信，4元/条（不含通信费）");
                }
                if (this.script != null) {
                    this.script.pause();
                }
                bool = true;
                this.bSMSType = (byte) 1;
                this.sendSmsId = 4;
                break;
            case 4:
                sb.append("\n        立刻获得您手中的神兵利器，用它奋勇杀敌吧！");
                bool = true;
                this.bSMSType = (byte) 3;
                this.bItemSum = (byte) 1;
                this.bAmount = (byte) 1;
                this.sendSmsId = 2;
                break;
            case 5:
                if (this.iTotalSum >= 6) {
                    sb.append("    您可免费领取以下礼品：\n\n    ");
                } else {
                    sb.append("    您再消费<c=00ff00>" + (6 - this.iTotalSum) + "</c>元，即可免费获得以下礼品：\n\n    ");
                }
                sb.append("隐藏武器 <c=" + COLORS[equipDatas[27].bGrade] + ">" + equipDatas[27].name + "</c>，");
                sb.append("<c=" + COLORS[propDatas[3].bType] + ">" + propDatas[3].name + "</c>");
                sb.append("<c=ffff00>20</c>个、");
                sb.append(" <c=" + COLORS[propDatas[7].bType] + ">" + propDatas[7].name + "</c>");
                sb.append("<c=ffff00>20</c>个、");
                sb.append(" <c=" + COLORS[propDatas[16].bType] + ">" + propDatas[16].name + "</c>");
                sb.append("<c=ffff00>3</c>个、");
                sb.append(" <c=" + COLORS[propDatas[17].bType] + ">" + propDatas[17].name + "</c>");
                sb.append("<c=ffff00>3</c>个、");
                sb.append(" <c=" + COLORS[propDatas[18].bType] + ">" + propDatas[18].name + "</c>");
                sb.append("<c=ffff00>3</c>个，");
                sb.append("并赠送<c=ffff00>5</c>金（50000银）。\n\n    感谢您的支持！");
                break;
            case 6:
                if (this.iTotalSum >= 12) {
                    sb.append("    您可免费领取以下礼品：\n\n    ");
                } else {
                    sb.append("    您再消费<c=00ff00>" + (12 - this.iTotalSum) + "</c>元，即可免费获得以下礼品：\n\n    ");
                }
                sb.append("隐藏武器 <c=" + COLORS[equipDatas[29].bGrade] + ">" + equipDatas[29].name + "</c>，");
                sb.append(" <c=" + COLORS[propDatas[9].bType] + ">" + propDatas[9].name + "</c>");
                sb.append("<c=ff0000>2</c>个、");
                sb.append(" <c=" + COLORS[propDatas[10].bType] + ">" + propDatas[10].name + "</c>");
                sb.append("<c=ff0000>5</c>个、");
                sb.append(" <c=" + COLORS[propDatas[17].bType] + ">" + propDatas[17].name + "</c>");
                sb.append("<c=ff0000>2</c>个、");
                sb.append(" <c=" + COLORS[propDatas[18].bType] + ">" + propDatas[18].name + "</c>");
                sb.append("<c=ff0000>2</c>个、");
                sb.append(" <c=" + COLORS[propDatas[19].bType] + ">" + propDatas[19].name + "</c>");
                sb.append("<c=ff0000>2</c>个、");
                sb.append(" <c=" + COLORS[propDatas[20].bType] + ">" + propDatas[20].name + "</c>");
                sb.append("<c=ff0000>2</c>个、");
                sb.append(" <c=" + COLORS[propDatas[21].bType] + ">" + propDatas[21].name + "</c>");
                sb.append("<c=ff0000>2</c>个、");
                sb.append(" <c=" + COLORS[propDatas[22].bType] + ">" + propDatas[22].name + "</c>");
                sb.append("<c=ff0000>2</c>个，");
                sb.append("并赠送<c=ffff00>15</c>金（150000银）。");
                break;
            case 7:
                if (this.iTotalSum >= 20) {
                    sb.append("    您可免费领取以下礼品：\n\n    ");
                } else {
                    sb.append("    您再消费<c=00ff00>" + (20 - this.iTotalSum) + "</c>元，即可免费获得以下礼品：\n\n    ");
                }
                sb.append("<c=00ff00>使用任何技能（<c=ff0000>包括必杀和崩击</c>）均不消耗武器耐久</c>,");
                sb.append("并赠送<c=ffff00>35</c>金（350000银）。\n\n    感谢您的支持！");
                break;
        }
        int lineNum = (Tool.UI_RATATE_180 / FontHeight) - 1;
        this.tipInfoST = new StyledText(280, lineNum, FontHeight, font);
        if (bool) {
            PreSMSReturn ret = WebNetInterface.PreSMSBillingPoint(this.sendSmsId);
            if (ret.m_bSuccess) {
                if (this.bSMSIndex != 3) {
                    sb.append(ret.m_contents);
                }
                this.tipInfoST.formatText(sb.toString());
            } else {
                this.tipInfoST.formatText(ret.m_contents);
            }
            setGameState((byte) 32);
            return;
        }
        this.tipInfoST.formatText(sb.toString());
        setGameState((byte) 32);
    }

    private void drawSMSInfo(Graphics g) {
        this.mainUIAS.drawFrame(g, 13, SCREEN_WIDTH >> 1, SCREEN_HEIGHT >> 1);
        if (this.tipInfoST.getPageNum() > 0) {
            this.mainUIAS.drawFrame(g, 15, (SCREEN_WIDTH >> 1) + 44, SCREEN_HEIGHT >> 1);
            drawScrollItem(g, this.scrollSignImg, ((SCREEN_WIDTH / 2) + 160) - 12, 49, 224, this.tipInfoST.getPageIndex(), this.tipInfoST.getPageNum() + 1);
        }
        if (this.tipInfoST.getPageIndex() == this.tipInfoST.getPageNum()) {
            this.mainUIAS.drawFrame(g, 14, SCREEN_WIDTH >> 1, SCREEN_HEIGHT >> 1);
        } else {
            Tool.drawImage(g, this.arrowUpImg, (SCREEN_WIDTH - this.arrowUpImg.getWidth()) >> 1, (Tool.countTimes2 % 2) + 265, 1);
        }
        this.tipInfoST.draw(g, (SCREEN_WIDTH - 280) >> 1, 85);
    }

    private void handlekey_SMSInfo() {
        if (Common.isUpPressed(true)) {
            this.tipInfoST.prePage();
            return;
        }
        if (Common.isDownPressed(true)) {
            this.tipInfoST.nextPage();
            return;
        }
        if (Common.isKeyPressed(9, true)) {
            if (this.tipInfoST.getPageIndex() == this.tipInfoST.getPageNum()) {
                if (this.bSMSIndex < 5) {
                    this.tipInfoST = new StyledText(280, this.menuHeight - 90, FontHeight, font);
                    this.tipInfoST.formatText("\n        短信发送中...请稍侯");
                    this.bGameState = STATE_SMS_WAITING;
                    WebNetInterface.SMSBillingPoint(this.sendSmsId, "1" + System.currentTimeMillis());
                    return;
                }
                this.isSendSuccessful = true;
                this.tipInfoST = new StyledText(280, this.menuHeight - 90, FontHeight, font);
                if (this.bSMSIndex == 5) {
                    if (this.iTotalSum >= 6) {
                        getItemBySMS(this.bSMSIndex);
                        this.tipInfoST.formatText("\n        已成功领取礼包，请注意保存，祝您游戏愉快。");
                    } else {
                        this.tipInfoST.formatText("\n        不满足领取条件，请再接再厉。");
                    }
                } else if (this.bSMSIndex == 6) {
                    if (this.iTotalSum >= 12) {
                        getItemBySMS(this.bSMSIndex);
                        this.tipInfoST.formatText("\n        已成功领取礼包，请注意保存，祝您游戏愉快。");
                    } else {
                        this.tipInfoST.formatText("\n        不满足领取条件，请再接再厉。");
                    }
                } else if (this.iTotalSum >= 20) {
                    if (this.bDrawGift[this.bRecordIndex] < 3) {
                        getItemBySMS(this.bSMSIndex);
                        this.bSMSShopIndex = (byte) 0;
                        this.tipInfoST.formatText("\n        已成功领取礼包，请注意保存，祝您游戏愉快。");
                    } else {
                        this.tipInfoST.formatText("\n        已成功领取礼包，无法再次领取。");
                    }
                } else {
                    this.tipInfoST.formatText("\n        不满足领取条件，请再接再厉!");
                }
                this.bGameState = STATE_SMS_CONFIRM;
                return;
            }
            return;
        }
        if (Common.isKeyPressed(10, true)) {
            cancelSend();
        }
    }

    private void drawSMSWaiting(Graphics g) {
        this.mainUIAS.drawFrame(g, 12, SCREEN_WIDTH >> 1, SCREEN_HEIGHT >> 1);
        this.tipInfoST.draw(g, (SCREEN_WIDTH - 280) >> 1, FIRE_KEY_HEIGHT);
    }

    private void drawSMSConfirm(Graphics g) {
        this.mainUIAS.drawFrame(g, 12, SCREEN_WIDTH >> 1, SCREEN_HEIGHT >> 1);
        this.tipInfoST.draw(g, (SCREEN_WIDTH - 280) >> 1, FIRE_KEY_HEIGHT);
    }

    private void handlekey_SendConfirm() {
        if (Common.isUpPressed(true)) {
            this.tipInfoST.prePage();
            return;
        }
        if (Common.isDownPressed(true)) {
            this.tipInfoST.nextPage();
            return;
        }
        if (Common.isOKPressed(true) || Common.isKeyPressed(9, true)) {
            if (this.bSMSIndex == 3 && this.isOpenAllStage) {
                if (this.script != null) {
                    setScriptState((byte) 3);
                }
            } else if (!this.isSendSuccessful) {
                cancelSend();
            } else if (this.bSMSIndex == 4) {
                exitEquipPreview();
            } else {
                returnPreState();
            }
            if (this.bGameState == 31) {
                createSMSShopMenu();
                return;
            }
            return;
        }
        if (Common.isKeyPressed(10, true)) {
            if (this.bSMSIndex == 3 && this.isOpenAllStage) {
                if (this.script != null) {
                    setScriptState((byte) 3);
                }
            } else if (!this.isSendSuccessful) {
                cancelSend();
            } else if (this.bSMSIndex == 4) {
                exitEquipPreview();
            } else {
                returnPreState();
            }
            if (this.bGameState == 31) {
                createSMSShopMenu();
            }
        }
    }

    public void getItemBySMS(int smsIndex) {
        switch (smsIndex) {
            case 0:
                if (this.role.bActID == 3) {
                    this.role.reLive();
                    this.role.wudi(100);
                } else {
                    this.role.iHP = this.role.iFinalHP;
                }
                this.isDurabilityNonLoss = true;
                this.isMaximumPower = true;
                Role role = this.role;
                this.role.getClass();
                role.sEP = (short) 2000;
                if (this.role.isIntegrate) {
                    this.role.famulus.iCurDurability = this.role.famulus.iFinalDurability;
                } else {
                    this.role.equip.iCurDurability = this.role.equip.iFinalDurability;
                }
                break;
            case 1:
                this.previewEquip.skill = this.previewEquip.getSkill();
                this.bag.addEquip(this.previewEquip);
                this.unsavedEquipV.addElement(this.previewEquip);
                saveUnsavedSMSItem();
                break;
            case 2:
                this.bExpCoefficientBySMS = (byte) (this.bExpCoefficientBySMS + 1);
                if (this.bExpCoefficientBySMS > 3) {
                    this.bSMSShopIndex = (byte) 0;
                }
                break;
            case 3:
                this.isOpenAllStage = true;
                break;
            case 4:
                this.bag.iMoney += this.iGoldBySMS * 10000;
                this.iGoldUnsaved += this.iGoldBySMS * 10000;
                saveUnsavedSMSItem();
                if (this.iGoldBySMS < DROP_POS_INTERVAL) {
                    this.iGoldBySMS += 2;
                } else {
                    this.iGoldBySMS = DROP_POS_INTERVAL;
                }
                break;
            case 5:
                this.bDrawGift[this.bRecordIndex] = 1;
                this.bag.iMoney += 50000;
                this.bag.addProp(propDatas[3], 20);
                this.bag.addProp(propDatas[7], 20);
                this.bag.addProp(propDatas[16], 3);
                this.bag.addProp(propDatas[17], 3);
                this.bag.addProp(propDatas[18], 3);
                this.bag.addEquip(new Equip(equipDatas[27], 0, true));
                break;
            case 6:
                this.bDrawGift[this.bRecordIndex] = 2;
                this.bag.iMoney += 150000;
                this.bag.addEquip(new Equip(equipDatas[29], 0, true));
                this.bag.addProp(propDatas[9], 2);
                this.bag.addProp(propDatas[10], 5);
                this.bag.addProp(propDatas[17], 2);
                this.bag.addProp(propDatas[18], 2);
                this.bag.addProp(propDatas[19], 2);
                this.bag.addProp(propDatas[20], 2);
                this.bag.addProp(propDatas[21], 2);
                this.bag.addProp(propDatas[22], 2);
                break;
            case 7:
                this.bDrawGift[this.bRecordIndex] = 3;
                this.bag.iMoney += 350000;
                this.isDurabilityNonLossForever = true;
                this.bSMSShopIndex = (byte) (this.bSMSShopIndex + STATE_CG);
                break;
        }
        this.iTotalSum += this.bItemSum;
        saveSMSRecord();
    }

    void cancelSend() {
        if (this.bSMSIndex == 2) {
            if (this.role.bActID == 3) {
                script_gameEnd();
                return;
            } else {
                returnPreState();
                return;
            }
        }
        returnPreState();
        if (this.bSMSIndex == 3) {
            this.isOpenAllStage = false;
            if (this.script != null) {
                setScriptState((byte) 3);
            }
        }
    }

    private String[] getSMS(int money) {
        String[] s = {"", ""};
        return s;
    }

    public void setQQSMS(int id, int type, int totalPrice) {
    }

    private class MyThread extends Thread {
        private boolean flag;
        private SurfaceHolder surfaceHolder;

        public MyThread(SurfaceHolder surfaceHolder) {
            this.surfaceHolder = surfaceHolder;
        }

        public void setFlag(boolean flag) {
            this.flag = flag;
        }

        @Override // java.lang.Thread, java.lang.Runnable
        public void run() {
            while (this.flag) {
                System.currentTimeMillis();
                if (!JL01Activity.isBack) {
                    if (GameCanvas.this.scriptState == 4) {
                        GameCanvas.this.setScriptState((byte) 0);
                    }
                    if (GameCanvas.this.scriptState != 0 || GameCanvas.this.scriptFileToRunV.isEmpty()) {
                        if (GameCanvas.this.scriptState == 3 && GameCanvas.this.script != null) {
                            GameCanvas.this.script.continueProcess();
                        }
                        long currTime = System.currentTimeMillis();
                        GameCanvas.this.handleKeyPressed();
                        GameCanvas.this.handleKeyReleased();
                        Tool.cycle();
                        GameCanvas.this.cycle();
                        Canvas canvas = null;
                        try {
                            canvas = this.surfaceHolder.lockCanvas();
                            synchronized (this.surfaceHolder) {
                                GameCanvas.this.onDraw(canvas);
                            }
                            if (canvas != null) {
                                this.surfaceHolder.unlockCanvasAndPost(canvas);
                            }
                            long currTime2 = System.currentTimeMillis() - currTime;
                            if (currTime2 < GameCanvas.MILLIS_PRE_UPDATE) {
                                try {
                                    Thread.sleep(((long) GameCanvas.MILLIS_PRE_UPDATE) - currTime2);
                                } catch (Exception e) {
                                    System.out.println(e);
                                }
                            }
                        } catch (Throwable th) {
                            if (canvas != null) {
                                this.surfaceHolder.unlockCanvasAndPost(canvas);
                            }
                            long currTime3 = System.currentTimeMillis() - currTime;
                            if (currTime3 < GameCanvas.MILLIS_PRE_UPDATE) {
                                try {
                                    Thread.sleep(((long) GameCanvas.MILLIS_PRE_UPDATE) - currTime3);
                                } catch (Exception e2) {
                                    System.out.println(e2);
                                }
                            }
                            throw th;
                        }
                    } else {
                        if (((Byte) GameCanvas.this.loadingStateV.elementAt(0)).byteValue() == 1) {
                            GameCanvas.this.startLoading();
                        }
                        String runningScript = (String) GameCanvas.this.scriptFileToRunV.elementAt(0);
                        GameCanvas.this.loadingStateV.removeElementAt(0);
                        GameCanvas.this.scriptFileToRunV.removeElementAt(0);
                        System.out.println("Run Script: " + runningScript);
                        if (GameCanvas.this.script == null) {
                            GameCanvas.this.script = new Interpreter(GameCanvas.this);
                        }
                        GameCanvas.this.script.startScript("/script/" + runningScript);
                    }
                } else {
                    try {
                        Thread.sleep(1000L);
                    } catch (InterruptedException e3) {
                        e3.printStackTrace();
                    }
                }
            }
        }
    }

    @Override // android.view.View
    public boolean onTouchEvent(MotionEvent event) {
        int keyCode;
        touchX = (int) event.getX();
        touchY = (int) event.getY();
        this.dirFrame = -1;
        int eventAction = event.getAction();
        switch (eventAction) {
            case 0:
                keyPressed(tranlateKeyCode(true));
                return true;
            case 1:
                keyReleased(tranlateKeyCode(false));
                return true;
            case 2:
                if (this.bGameState == 12 && (keyCode = tranlateKeyCode(true)) < 4) {
                    keyPressed(keyCode);
                }
                break;
        }
        return super.onTouchEvent(event);
    }

    private int tranlateKeyCode(boolean isPress) {
        int index = -1;
        switch (this.bGameState) {
            case 3:
                if (Tool.isPointInsideRect(touchX, touchY, this.DIR_KEY_POSITION[20][0], this.DIR_KEY_POSITION[20][1], this.DIR_KEY_POSITION[20][2], this.DIR_KEY_POSITION[20][3])) {
                    index = 2;
                } else if (Tool.isPointInsideRect(touchX, touchY, this.DIR_KEY_POSITION[21][0], this.DIR_KEY_POSITION[21][1], this.DIR_KEY_POSITION[21][2], this.DIR_KEY_POSITION[21][3])) {
                    index = 3;
                } else if (Tool.isPointInsideRect(touchX, touchY, this.DIR_KEY_POSITION[22][0], this.DIR_KEY_POSITION[22][1], this.DIR_KEY_POSITION[22][2], this.DIR_KEY_POSITION[22][3])) {
                    if (isPress) {
                        this.bGameSetState = (byte) 0;
                    }
                } else if (Tool.isPointInsideRect(touchX, touchY, this.DIR_KEY_POSITION[23][0], this.DIR_KEY_POSITION[23][1], this.DIR_KEY_POSITION[23][2], this.DIR_KEY_POSITION[23][3]) && isPress) {
                    this.bGameSetState = (byte) 1;
                }
                if (Tool.isPointInsideRect(touchX, touchY, this.DIR_KEY_POSITION[11][0], this.DIR_KEY_POSITION[11][1], this.DIR_KEY_POSITION[11][2], this.DIR_KEY_POSITION[11][3])) {
                    index = 9;
                } else if (Tool.isPointInsideRect(touchX, touchY, this.DIR_KEY_POSITION[12][0], this.DIR_KEY_POSITION[12][1], this.DIR_KEY_POSITION[12][2], this.DIR_KEY_POSITION[12][3])) {
                    index = 10;
                }
                if (this.bPreState == 18) {
                    if (Tool.isPointInsideRect(touchX, touchY, this.DIR_KEY_POSITION[0][0], this.DIR_KEY_POSITION[0][1], this.DIR_KEY_POSITION[0][2], this.DIR_KEY_POSITION[0][3])) {
                        this.dirFrame = 0;
                        return 0;
                    }
                    if (Tool.isPointInsideRect(touchX, touchY, this.DIR_KEY_POSITION[1][0], this.DIR_KEY_POSITION[1][1], this.DIR_KEY_POSITION[1][2], this.DIR_KEY_POSITION[1][3])) {
                        this.dirFrame = 1;
                        return 1;
                    }
                    if (Tool.isPointInsideRect(touchX, touchY, this.DIR_KEY_POSITION[2][0], this.DIR_KEY_POSITION[2][1], this.DIR_KEY_POSITION[2][2], this.DIR_KEY_POSITION[2][3])) {
                        this.dirFrame = 2;
                        return 2;
                    }
                    if (Tool.isPointInsideRect(touchX, touchY, this.DIR_KEY_POSITION[3][0], this.DIR_KEY_POSITION[3][1], this.DIR_KEY_POSITION[3][2], this.DIR_KEY_POSITION[3][3])) {
                        this.dirFrame = 3;
                        return 3;
                    }
                    if (Tool.isPointInsideRect(touchX, touchY, this.DIR_KEY_POSITION[4][0], this.DIR_KEY_POSITION[4][1], this.DIR_KEY_POSITION[4][2], this.DIR_KEY_POSITION[4][3])) {
                        return 4;
                    }
                    return index;
                }
                return index;
            case 4:
                if (Tool.isPointInsideRect(touchX, touchY, this.DIR_KEY_POSITION[13][0], this.DIR_KEY_POSITION[13][1], this.DIR_KEY_POSITION[13][2], this.DIR_KEY_POSITION[13][3])) {
                    return 9;
                }
                if (!Tool.isPointInsideRect(touchX, touchY, this.DIR_KEY_POSITION[14][0], this.DIR_KEY_POSITION[14][1], this.DIR_KEY_POSITION[14][2], this.DIR_KEY_POSITION[14][3]) || this.bGameState != 4) {
                    return -1;
                }
                return 10;
            case 5:
                if (Tool.isPointInsideRect(touchX, touchY, this.DIR_KEY_POSITION[15][0], this.DIR_KEY_POSITION[15][1], this.DIR_KEY_POSITION[15][2], this.DIR_KEY_POSITION[15][3])) {
                    return 4;
                }
                if (Tool.isPointInsideRect(touchX, touchY, this.DIR_KEY_POSITION[16][0], this.DIR_KEY_POSITION[16][1], this.DIR_KEY_POSITION[16][2], this.DIR_KEY_POSITION[16][3])) {
                    return 2;
                }
                if (Tool.isPointInsideRect(touchX, touchY, this.DIR_KEY_POSITION[17][0], this.DIR_KEY_POSITION[17][1], this.DIR_KEY_POSITION[17][2], this.DIR_KEY_POSITION[17][3])) {
                    return 3;
                }
                if (!Tool.isPointInsideRect(touchX, touchY, SCREEN_WIDTH - 100, SCREEN_HEIGHT - 100, 100, 100)) {
                    return -1;
                }
                System.out.println("index = Common.BUTTON_BACK_PRESSED;");
                return 10;
            case 6:
            default:
                if (Tool.isPointInsideRect(touchX, touchY, this.DIR_KEY_POSITION[0][0], this.DIR_KEY_POSITION[0][1], this.DIR_KEY_POSITION[0][2], this.DIR_KEY_POSITION[0][3])) {
                    this.dirFrame = 0;
                    return 0;
                }
                if (Tool.isPointInsideRect(touchX, touchY, this.DIR_KEY_POSITION[1][0], this.DIR_KEY_POSITION[1][1], this.DIR_KEY_POSITION[1][2], this.DIR_KEY_POSITION[1][3])) {
                    this.dirFrame = 1;
                    return 1;
                }
                if (Tool.isPointInsideRect(touchX, touchY, this.DIR_KEY_POSITION[2][0], this.DIR_KEY_POSITION[2][1], this.DIR_KEY_POSITION[2][2], this.DIR_KEY_POSITION[2][3])) {
                    this.dirFrame = 2;
                    return 2;
                }
                if (Tool.isPointInsideRect(touchX, touchY, this.DIR_KEY_POSITION[3][0], this.DIR_KEY_POSITION[3][1], this.DIR_KEY_POSITION[3][2], this.DIR_KEY_POSITION[3][3])) {
                    this.dirFrame = 3;
                    return 3;
                }
                if (Tool.isPointInsideRect(touchX, touchY, this.DIR_KEY_POSITION[4][0], this.DIR_KEY_POSITION[4][1], this.DIR_KEY_POSITION[4][2], this.DIR_KEY_POSITION[4][3])) {
                    return 4;
                }
                if (Tool.isPointInsideRect(touchX, touchY, this.DIR_KEY_POSITION[5][0], this.DIR_KEY_POSITION[5][1], this.DIR_KEY_POSITION[5][2], this.DIR_KEY_POSITION[5][3])) {
                    return 18;
                }
                if (Tool.isPointInsideRect(touchX, touchY, this.DIR_KEY_POSITION[6][0], this.DIR_KEY_POSITION[6][1], this.DIR_KEY_POSITION[6][2], this.DIR_KEY_POSITION[6][3])) {
                    return 20;
                }
                if (Tool.isPointInsideRect(touchX, touchY, this.DIR_KEY_POSITION[7][0], this.DIR_KEY_POSITION[7][1], this.DIR_KEY_POSITION[7][2], this.DIR_KEY_POSITION[7][3])) {
                    return 11;
                }
                if (Tool.isPointInsideRect(touchX, touchY, this.DIR_KEY_POSITION[8][0], this.DIR_KEY_POSITION[8][1], this.DIR_KEY_POSITION[8][2], this.DIR_KEY_POSITION[8][3])) {
                    return 9;
                }
                if (Tool.isPointInsideRect(touchX, touchY, this.DIR_KEY_POSITION[9][0], this.DIR_KEY_POSITION[9][1], this.DIR_KEY_POSITION[9][2], this.DIR_KEY_POSITION[9][3])) {
                    if (this.bGameState == 12 || this.bGameState == 36) {
                        return 14;
                    }
                    return 10;
                }
                if (Tool.isPointInsideRect(touchX, touchY, this.DIR_KEY_POSITION[10][0], this.DIR_KEY_POSITION[10][1], this.DIR_KEY_POSITION[10][2], this.DIR_KEY_POSITION[10][3])) {
                    if (this.bGameState != 12) {
                        return -1;
                    }
                    return 12;
                }
                if (Tool.isPointInsideRect(touchX, touchY, this.DIR_KEY_POSITION[11][0], this.DIR_KEY_POSITION[11][1], this.DIR_KEY_POSITION[11][2], this.DIR_KEY_POSITION[11][3])) {
                    if (this.bGameState == 12) {
                        return -1;
                    }
                    return 9;
                }
                if (!Tool.isPointInsideRect(touchX, touchY, this.DIR_KEY_POSITION[12][0], this.DIR_KEY_POSITION[12][1], this.DIR_KEY_POSITION[12][2], this.DIR_KEY_POSITION[12][3]) || this.bGameState == 12) {
                    return -1;
                }
                return 10;
            case 7:
                if (!this.isPopUpOption) {
                    if (Tool.isPointInsideRect(touchX, touchY, this.DIR_KEY_POSITION[18][0], this.DIR_KEY_POSITION[18][1], this.DIR_KEY_POSITION[18][2], this.DIR_KEY_POSITION[18][3])) {
                        if (isPress) {
                            this.bRecordIndex = (byte) 0;
                            index = 4;
                        }
                    } else if (Tool.isPointInsideRect(touchX, touchY, this.DIR_KEY_POSITION[19][0], this.DIR_KEY_POSITION[19][1], this.DIR_KEY_POSITION[19][2], this.DIR_KEY_POSITION[19][3]) && isPress) {
                        this.bRecordIndex = (byte) 1;
                        index = 4;
                    }
                } else {
                    int h = FontHeight + 6;
                    int x = (SCREEN_WIDTH - this.optionWidth) >> 1;
                    int y = this.YYY;
                    if (Tool.isPointInsideRect(touchX, touchY, x, y, this.optionWidth, h)) {
                        index = this.bPopUpOptionIndex == 0 ? 4 : 0;
                    } else if (Tool.isPointInsideRect(touchX, touchY, x, y + h, this.optionWidth, h)) {
                        index = this.bPopUpOptionIndex == 0 ? 1 : 4;
                    }
                }
                if (Tool.isPointInsideRect(touchX, touchY, this.DIR_KEY_POSITION[11][0], this.DIR_KEY_POSITION[11][1], this.DIR_KEY_POSITION[11][2], this.DIR_KEY_POSITION[11][3])) {
                    index = 9;
                } else if (Tool.isPointInsideRect(touchX, touchY, this.DIR_KEY_POSITION[12][0], this.DIR_KEY_POSITION[12][1], this.DIR_KEY_POSITION[12][2], this.DIR_KEY_POSITION[12][3])) {
                    index = 10;
                }
                if (this.bPreState == 18) {
                    if (Tool.isPointInsideRect(touchX, touchY, this.DIR_KEY_POSITION[0][0], this.DIR_KEY_POSITION[0][1], this.DIR_KEY_POSITION[0][2], this.DIR_KEY_POSITION[0][3])) {
                        this.dirFrame = 0;
                        return 0;
                    }
                    if (Tool.isPointInsideRect(touchX, touchY, this.DIR_KEY_POSITION[1][0], this.DIR_KEY_POSITION[1][1], this.DIR_KEY_POSITION[1][2], this.DIR_KEY_POSITION[1][3])) {
                        this.dirFrame = 1;
                        return 1;
                    }
                    if (Tool.isPointInsideRect(touchX, touchY, this.DIR_KEY_POSITION[2][0], this.DIR_KEY_POSITION[2][1], this.DIR_KEY_POSITION[2][2], this.DIR_KEY_POSITION[2][3])) {
                        this.dirFrame = 2;
                        return 2;
                    }
                    if (Tool.isPointInsideRect(touchX, touchY, this.DIR_KEY_POSITION[3][0], this.DIR_KEY_POSITION[3][1], this.DIR_KEY_POSITION[3][2], this.DIR_KEY_POSITION[3][3])) {
                        this.dirFrame = 3;
                        return 3;
                    }
                    if (Tool.isPointInsideRect(touchX, touchY, this.DIR_KEY_POSITION[4][0], this.DIR_KEY_POSITION[4][1], this.DIR_KEY_POSITION[4][2], this.DIR_KEY_POSITION[4][3])) {
                        return 4;
                    }
                    return index;
                }
                return index;
        }
    }

    private void initButtons() {
        this.DIR_KEY_POSITION[0][0] = 26;
        this.DIR_KEY_POSITION[0][1] = (SCREEN_HEIGHT - 120) - 1;
        this.DIR_KEY_POSITION[0][2] = 70;
        this.DIR_KEY_POSITION[0][3] = 40;
        this.DIR_KEY_POSITION[1][0] = 26;
        this.DIR_KEY_POSITION[1][1] = (SCREEN_HEIGHT - 40) - 1;
        this.DIR_KEY_POSITION[1][2] = 70;
        this.DIR_KEY_POSITION[1][3] = 40;
        this.DIR_KEY_POSITION[2][0] = 1;
        this.DIR_KEY_POSITION[2][1] = (SCREEN_HEIGHT - 80) - 1;
        this.DIR_KEY_POSITION[2][2] = 50;
        this.DIR_KEY_POSITION[2][3] = 40;
        this.DIR_KEY_POSITION[3][0] = 71;
        this.DIR_KEY_POSITION[3][1] = (SCREEN_HEIGHT - 80) - 1;
        this.DIR_KEY_POSITION[3][2] = 50;
        this.DIR_KEY_POSITION[3][3] = 40;
        this.dirX = this.DIR_KEY_POSITION[0][0] + (this.DIR_KEY_POSITION[0][2] / 2);
        this.dirY = this.DIR_KEY_POSITION[0][1] + ((this.DIR_KEY_POSITION[0][3] * 3) / 2);
        this.DIR_KEY_POSITION[4][0] = (SCREEN_WIDTH - 1) - 70;
        this.DIR_KEY_POSITION[4][1] = ((SCREEN_HEIGHT - 1) - 60) - 5;
        this.DIR_KEY_POSITION[4][2] = 70;
        this.DIR_KEY_POSITION[4][3] = FIRE_KEY_HEIGHT;
        this.fireX = this.DIR_KEY_POSITION[4][0] + (this.DIR_KEY_POSITION[4][2] / 2);
        this.fireY = this.DIR_KEY_POSITION[4][1] + (this.DIR_KEY_POSITION[4][3] / 2);
        this.DIR_KEY_POSITION[5][0] = (((SCREEN_WIDTH - 1) - 1) - 70) - 40;
        this.DIR_KEY_POSITION[5][1] = (SCREEN_HEIGHT - 1) - 40;
        this.DIR_KEY_POSITION[5][2] = 40;
        this.DIR_KEY_POSITION[5][3] = 40;
        this.skillX = this.DIR_KEY_POSITION[5][0] + 20;
        this.skillY = this.DIR_KEY_POSITION[5][1] + 20;
        this.DIR_KEY_POSITION[6][0] = ((SCREEN_WIDTH - 3) - 70) - 30;
        this.DIR_KEY_POSITION[6][1] = (((SCREEN_HEIGHT - 2) - 60) - 40) - 5;
        this.DIR_KEY_POSITION[6][2] = 40;
        this.DIR_KEY_POSITION[6][3] = 40;
        this.bengX = this.DIR_KEY_POSITION[6][0] + 20;
        this.bengY = this.DIR_KEY_POSITION[6][1] + 20;
        this.DIR_KEY_POSITION[7][0] = (SCREEN_WIDTH - 40) - 1;
        this.DIR_KEY_POSITION[7][1] = (((SCREEN_HEIGHT - 3) - 60) - 40) - 5;
        this.DIR_KEY_POSITION[7][2] = 40;
        this.DIR_KEY_POSITION[7][3] = 40;
        this.killX = this.DIR_KEY_POSITION[7][0] + 20;
        this.killY = this.DIR_KEY_POSITION[7][1] + 20;
        this.DIR_KEY_POSITION[8][0] = (SCREEN_WIDTH - 100) - 2;
        this.DIR_KEY_POSITION[8][1] = 1;
        this.DIR_KEY_POSITION[8][2] = 50;
        this.DIR_KEY_POSITION[8][3] = 50;
        this.menuButtonX = this.DIR_KEY_POSITION[8][0] + 25;
        this.menuButtonY = this.DIR_KEY_POSITION[8][1] + 25;
        this.DIR_KEY_POSITION[9][0] = (SCREEN_WIDTH - 50) - 1;
        this.DIR_KEY_POSITION[9][1] = 1;
        this.DIR_KEY_POSITION[9][2] = 50;
        this.DIR_KEY_POSITION[9][3] = 50;
        this.miniMapX = this.DIR_KEY_POSITION[9][0] + 25;
        this.miniMapY = this.DIR_KEY_POSITION[9][1] + 25;
        this.DIR_KEY_POSITION[10][0] = 140;
        this.DIR_KEY_POSITION[10][1] = SCREEN_HEIGHT - 40;
        this.DIR_KEY_POSITION[10][2] = 120;
        this.DIR_KEY_POSITION[10][3] = 40;
        this.DIR_KEY_POSITION[11][0] = (SCREEN_WIDTH - 320) >> 1;
        this.DIR_KEY_POSITION[11][1] = (((SCREEN_HEIGHT / 2) + 120) - 40) + 10;
        this.DIR_KEY_POSITION[11][2] = FIRE_KEY_HEIGHT;
        this.DIR_KEY_POSITION[11][3] = DROP_POS_INTERVAL;
        this.DIR_KEY_POSITION[12][0] = ((this.DIR_KEY_POSITION[11][0] + 320) - 40) - 20;
        this.DIR_KEY_POSITION[12][1] = (((SCREEN_HEIGHT / 2) + 120) - 40) + 10;
        this.DIR_KEY_POSITION[12][2] = FIRE_KEY_HEIGHT;
        this.DIR_KEY_POSITION[12][3] = DROP_POS_INTERVAL;
        this.DIR_KEY_POSITION[13][0] = (SCREEN_WIDTH - 240) >> 1;
        this.DIR_KEY_POSITION[13][1] = SCREEN_HEIGHT - 70;
        this.DIR_KEY_POSITION[13][2] = 100;
        this.DIR_KEY_POSITION[13][3] = 50;
        this.DIR_KEY_POSITION[14][0] = ((((SCREEN_WIDTH - 240) >> 1) + 240) - 50) - 50;
        this.DIR_KEY_POSITION[14][1] = SCREEN_HEIGHT - 70;
        this.DIR_KEY_POSITION[14][2] = 100;
        this.DIR_KEY_POSITION[14][3] = 50;
        this.iMainMenuX += 10;
        this.DIR_KEY_POSITION[15][0] = this.iMainMenuX;
        this.DIR_KEY_POSITION[15][1] = this.iMainMenuY - 10;
        this.DIR_KEY_POSITION[15][2] = this.OPTION_WIDTH;
        this.DIR_KEY_POSITION[15][3] = this.OPTION_HEIGHT + 20;
        this.DIR_KEY_POSITION[16][0] = this.iMainMenuX - 70;
        this.DIR_KEY_POSITION[16][1] = this.iMainMenuY - 20;
        this.DIR_KEY_POSITION[16][2] = 70;
        this.DIR_KEY_POSITION[16][3] = this.OPTION_HEIGHT + 40;
        this.DIR_KEY_POSITION[17][0] = this.iMainMenuX + this.OPTION_WIDTH;
        this.DIR_KEY_POSITION[17][1] = this.iMainMenuY - 20;
        this.DIR_KEY_POSITION[17][2] = 70;
        this.DIR_KEY_POSITION[17][3] = this.OPTION_HEIGHT + 40;
        this.DIR_KEY_POSITION[18][0] = (SCREEN_WIDTH - 320) >> 1;
        this.DIR_KEY_POSITION[18][1] = 82;
        this.DIR_KEY_POSITION[18][2] = 320;
        this.DIR_KEY_POSITION[18][3] = 76;
        this.DIR_KEY_POSITION[19][0] = (SCREEN_WIDTH - 320) >> 1;
        this.DIR_KEY_POSITION[19][1] = 162;
        this.DIR_KEY_POSITION[19][2] = 320;
        this.DIR_KEY_POSITION[19][3] = 76;
        this.DIR_KEY_POSITION[20][0] = ((SCREEN_WIDTH - 100) >> 1) - 80;
        this.DIR_KEY_POSITION[20][1] = SCREEN_HEIGHT >> 1;
        this.DIR_KEY_POSITION[20][2] = 80;
        this.DIR_KEY_POSITION[20][3] = 70;
        this.DIR_KEY_POSITION[21][0] = (SCREEN_WIDTH + 100) >> 1;
        this.DIR_KEY_POSITION[21][1] = SCREEN_HEIGHT >> 1;
        this.DIR_KEY_POSITION[21][2] = 80;
        this.DIR_KEY_POSITION[21][3] = 70;
        this.DIR_KEY_POSITION[22][0] = (SCREEN_WIDTH - 100) >> 1;
        this.DIR_KEY_POSITION[22][1] = SCREEN_HEIGHT >> 1;
        this.DIR_KEY_POSITION[22][2] = 100;
        this.DIR_KEY_POSITION[22][3] = 40;
        this.DIR_KEY_POSITION[23][0] = (SCREEN_WIDTH - 100) >> 1;
        this.DIR_KEY_POSITION[23][1] = (SCREEN_HEIGHT + 80) >> 1;
        this.DIR_KEY_POSITION[23][2] = 100;
        this.DIR_KEY_POSITION[23][3] = 50;
    }

    private void drawKey(Graphics g) {
        if (this.bGameState != 0 && this.bGameState != 9 && this.bGameState != -1 && this.bGameState != 4 && this.bGameState != 5 && ((this.bGameState != 7 || this.bPreState != 5) && this.bGameState != 1 && this.bGameState != 2 && (this.bGameState != 3 || this.bPreState != 5))) {
            this.dirSet.drawFrame(g, this.dirFrame < 0 ? 0 : this.dirFrame + 1, this.dirX, this.dirY);
            g.drawImage((this.curKey == 4 || Common.isOKPressed(false)) ? this.fireKey1 : this.fireKey0, this.fireX, this.fireY, 3);
        }
        if ((this.bGameState == 12 || this.bGameState == 18 || this.bGameState == 17) && this.scriptState == 0) {
            g.drawImage(Common.isKeyPressed(18, false) ? this.skillKey1 : this.skillKey0, this.skillX, this.skillY, 3);
            g.drawImage(Common.isKeyPressed(20, false) ? this.bengKey1 : this.bengKey0, this.bengX, this.bengY, 3);
            g.drawImage(Common.isKeyPressed(14, false) ? this.mapKey1 : this.mapKey0, this.miniMapX, this.miniMapY, 3);
            g.drawImage(Common.isKeyPressed(9, false) ? this.menuKey1 : this.menuKey0, this.menuButtonX, this.menuButtonY, 3);
            g.drawImage(this.killKey, this.killX, this.killY, 3);
            short s = this.role.sEP;
            this.role.getClass();
            if (s == 2000) {
                this.killSet.drawFrameCycle(g, 0, Tool.countTimes, this.killX, this.killY);
            }
        }
        if (this.bGameState != 12) {
            if (this.bGameState == 36 || this.bGameState == 2 || this.bGameState == 11 || this.bGameState == 1 || this.bGameState == 18 || this.bGameState == 7 || this.bGameState == 27 || this.bGameState == 3 || this.bGameState == 15 || this.bGameState == 23 || this.bGameState == 22) {
                g.drawImage(Common.isKeyPressed(10, false) ? this.backKey1 : this.backKey0, this.miniMapX, this.miniMapY, 3);
            }
        }
    }

    private void loadKeyRes() {
        if (this.fireKey0 == null) {
            this.fireKey0 = Tool.loadPImage("key/c_02.p");
            this.fireKey1 = Tool.loadPImage("key/c_12.p");
            this.menuKey0 = Tool.loadPImage("key/c_06.p");
            this.menuKey1 = Tool.loadPImage("key/c_16.p");
            this.mapKey0 = Tool.loadPImage("key/c_07.p");
            this.mapKey1 = Tool.loadPImage("key/c_17.p");
            this.backKey0 = Tool.loadPImage("key/c_08.p");
            this.backKey1 = Tool.loadPImage("key/c_18.p");
            this.skillKey0 = Tool.loadPImage("key/c_03.p");
            this.skillKey1 = Tool.loadPImage("key/c_13.p");
            this.bengKey0 = Tool.loadPImage("key/c_04.p");
            this.bengKey1 = Tool.loadPImage("key/c_14.p");
            this.killKey = Tool.loadPImage("key/c_05.p");
            this.dirSet = ActionSet.createActionSet("key/c_00.n", new Bitmap[]{Tool.loadPImage("key/c_00.p"), Tool.loadPImage("key/c_01.p")});
            this.killSet = ActionSet.createActionSet("key/c_15.n", new Bitmap[]{Tool.loadPImage("key/c_15.p")});
        }
    }
}
