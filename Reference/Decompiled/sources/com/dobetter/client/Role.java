package com.dobetter.client;

import android.support.v4.util.TimeUtils;
import com.dobetter.action.ActionSet;
import com.dobetter.client.data.AttributeData;
import com.dobetter.common.Common;
import com.dobetter.common.Graphics;
import com.dobetter.common.Tool;
import com.dobetter.script.Interpreter;
import com.tencent.webnet.WebNetEvent;
import java.io.DataInputStream;
import java.io.DataOutputStream;

/* JADX INFO: loaded from: classes.dex */
public class Role extends FightSprite {
    public static final byte ACT_ATTACK1 = 7;
    public static final byte ACT_ATTACK2 = 8;
    public static final byte ACT_ATTACK3 = 9;
    public static final byte ACT_ATTACK4 = 10;
    public static final byte ACT_BREAK = 11;
    public static final byte ACT_CRITICAL_KILL = 25;
    public static final byte ACT_CRITICAL_KILL_CHECK = 22;
    public static final byte ACT_CRITICAL_KILL_HIT = 24;
    public static final byte ACT_CRITICAL_KILL_MISS = 23;
    public static final byte ACT_DIE = 3;
    public static final byte ACT_DODGE = 4;
    public static final byte ACT_FLYING_AFTER_BLOW = 5;
    public static final byte ACT_HURTED = 2;
    public static final byte ACT_HURTED_BY_BREAK = 6;
    public static final byte ACT_INTEGRATE = 26;
    public static final byte ACT_RUN = 1;
    public static final byte ACT_SKILL1 = 12;
    public static final byte ACT_SKILL10 = 21;
    public static final byte ACT_SKILL2 = 13;
    public static final byte ACT_SKILL3 = 14;
    public static final byte ACT_SKILL4 = 15;
    public static final byte ACT_SKILL5 = 16;
    public static final byte ACT_SKILL6 = 17;
    public static final byte ACT_SKILL7 = 18;
    public static final byte ACT_SKILL8 = 19;
    public static final byte ACT_SKILL9 = 20;
    public static final byte ACT_STAND = 0;
    static final int AP = 1;
    static final int ATTACK_TYPE_NUM = 4;
    static final int ATTRIBUTE_NUM = 6;
    static final int CRITICAL = 5;
    static final int DODGE = 4;
    static final int DP = 2;
    static final int HIT = 3;
    static final int HP = 0;
    byte bComboChangeEffectCounter;
    byte bEPCounter;
    public byte bMiscellaneousIndex;
    public byte bSkillIndex;
    public byte bSkillLevelByScript;
    byte bWeaponIndex;
    ActionSet criticalKillAS;
    public AttributeData data;
    Enemy enemy;
    public Equip equip;
    Famulus famulus;
    ActionSet guideAS;
    int iAPUpByProp;
    public int iBreakNum;
    public int iComboCoffecient;
    public int iComboNum;
    int iCriticalPointUpByProp;
    int iDPUpByProp;
    int iDodgePointUpByProp;
    int iFinalAP;
    int iFinalCriticalPoint;
    int iFinalDP;
    int iFinalDodgePoint;
    int iFinalHP;
    int iFinalHitPoint;
    public int iGuideArrowIndex;
    int iHPUpByProp;
    int iHitPointUpByProp;
    int iImmuneInjuryCount;
    int iImmuneInjuryDuration;
    int iLevelUpCounter;
    int iLevelUpDuration;
    int iRecoverPoint;
    int iTargetStageID;
    public int iTotalKilledNum;
    ActionSet integrateAS;
    private boolean isBreakHit;
    boolean isComboNumChange;
    boolean isContinueAttack;
    boolean isCriticalKill;
    boolean isCriticalKillEnd;
    boolean isFillEP;
    public boolean isIntegrate;
    boolean isUseCriticalKillByScript;
    public boolean isUseMiscellaneous;
    public boolean isUseSkillByScript;
    ActionSet levelUpAS;
    ActionSet miscellaneousAS;
    short sComboUpgradeAPPercent;
    short sRoleID;
    ActionSet skillAS;
    public Sprite target;
    int iAttackIndex = 0;
    short sEP = 0;
    final short MAX_EP = 2000;
    int[] attributeUpByProp = new int[6];
    String[] weaponStr = null;
    String[] swordSoulStr = null;
    public short[] sCycleTime = new short[2];
    public short[] sCDCount = new short[2];
    final short sMaxLevel = 999;
    final int tan225 = 41421;
    final int tan675 = 241421;

    public Role(AttributeData roleData, short level) {
        this.bType = (byte) 0;
        this.isCheckMapBlock = true;
        this.ID = -1;
        this.iSortID = -1;
        this.bType = (byte) 0;
        this.sLevel = level;
        this.iLayerIndex = 2;
        this.iMoveSpeed = 6;
        this.data = roleData;
        init();
    }

    public void loadRes() {
        loadRes(GameCanvas.instance.roleBaseData);
        initAct();
        loadSpecialEffectRes();
        String[] imgStr = {"levelup"};
        this.levelUpAS = Common.getActionSet("z_levelup", imgStr, false, false);
        reload(this.weaponStr, 4);
        reload(this.swordSoulStr, 1);
    }

    public void loadSpecialEffectRes() {
        String[] imgStr = {"s_17", "s_18", "s_19", "s_20", "s_23", "s_24", "s_26", "o_00", "s_25", "s_27", "s_28", "s_242"};
        this.criticalKillAS = Common.getActionSet("z_finalslash", imgStr, false, true);
        String[] imgStr2 = {"h_04"};
        this.integrateAS = Common.getActionSet("skill_24", imgStr2, false, true);
        String[] imgStr3 = {"s_41", "h_04"};
        this.miscellaneousAS = Common.getActionSet("s_41", imgStr3, false, true);
        String[] imgStr4 = {"s_35"};
        this.guideAS = Common.getActionSet("z_arrow", imgStr4, false, false);
    }

    public void loadSkillRes(int skillIndex) {
        switch (skillIndex) {
            case 5:
                String[] imgStr = {"s_20"};
                this.skillAS = Common.getActionSet("skill_05", imgStr, false, false);
                break;
            case 6:
                String[] imgStr2 = {"s_19"};
                this.skillAS = Common.getActionSet("skill_06", imgStr2, false, false);
                break;
            case 7:
                String[] imgStr3 = {"s_21"};
                this.skillAS = Common.getActionSet("skill_07", imgStr3, false, false);
                break;
            case 8:
                String[] imgStr4 = {"s_22"};
                this.skillAS = Common.getActionSet("skill_08", imgStr4, false, false);
                break;
            case Interpreter.ERR_ARR_OUT /* 9 */:
                String[] imgStr5 = {"s_20"};
                this.skillAS = Common.getActionSet("skill_09", imgStr5, false, false);
                break;
            case 10:
                String[] imgStr6 = {"s_11", "s_19", "s_08"};
                this.skillAS = Common.getActionSet("skill_10", imgStr6, false, false);
                break;
            case 11:
                String[] imgStr7 = {"s_21"};
                this.skillAS = Common.getActionSet("skill_11", imgStr7, false, false);
                break;
            case 12:
                String[] imgStr8 = {"s_22"};
                this.skillAS = Common.getActionSet("skill_12", imgStr8, false, false);
                break;
            case 14:
            case 15:
            case Graphics.TOP /* 16 */:
            case Graphics.HT /* 17 */:
            case 18:
            case TimeUtils.HUNDRED_DAY_FIELD_LEN /* 19 */:
            case 20:
            case 21:
                String[] imgStr9 = {"s_24"};
                this.skillAS = Common.getActionSet("skill_14", imgStr9, false, false);
                break;
        }
    }

    public void init() {
        this.isCheckMapBlock = true;
        getLevelUpExp();
        this.iComboNum = 0;
        this.sCycleTime[1] = 3;
        this.sCDCount[1] = 3;
    }

    void addExp(int value) {
        if (this.sLevel < 999) {
            GameCanvas.instance.addDropInfo("经验+<c=ff0000,-1,0>" + value + "</c>");
            this.iCurrentExp += value;
            int tempLevel = this.sLevel;
            while (this.iCurrentExp >= this.iLevelUpExp) {
                this.iCurrentExp -= this.iLevelUpExp;
                this.sLevel = (short) (this.sLevel + 1);
                this.iLevelUpCounter = 0;
                this.iLevelUpDuration = this.levelUpAS.actionDatas[0].frameNum - 1;
                getLevelUpExp();
                if (this.sLevel == 999) {
                    this.iCurrentExp = this.iLevelUpExp;
                }
            }
            if (this.sLevel > tempLevel) {
                initData();
                if (this.isIntegrate) {
                    integrate(true);
                } else {
                    updateAttribute(true);
                }
                StringBuffer sb = new StringBuffer("等级提升为");
                sb.append((int) this.sLevel);
                GameCanvas.instance.addScrollInfo(sb.toString());
            }
        }
    }

    void levelUp(int num) {
        if (this.sLevel <= 999 - num) {
            this.iLevelUpCounter = 0;
            this.iLevelUpDuration = this.levelUpAS.actionDatas[0].frameNum - 1;
            this.sLevel = (short) (this.sLevel + num);
            getLevelUpExp();
            initData();
            if (this.isIntegrate) {
                integrate(true);
            } else {
                updateAttribute(true);
            }
            StringBuffer sb = new StringBuffer("等级提升为");
            sb.append((int) this.sLevel);
            GameCanvas.instance.addScrollInfo(sb.toString());
        }
    }

    void initData() {
        int i = this.data.iHP + ((this.sLevel - 1) * this.data.iUpgradeHP);
        this.iMaxHP = i;
        this.iHP = i;
        this.iFinalHP = i;
        int i2 = this.data.sAP + ((this.sLevel - 1) * this.data.sUpgradeAP);
        this.iAP = i2;
        this.iFinalAP = i2;
        int i3 = this.data.sDP + ((this.sLevel - 1) * this.data.sUpgradeDP);
        this.iDP = i3;
        this.iFinalDP = i3;
        int i4 = this.data.sHitPoint + ((this.sLevel - 1) * this.data.sUpgradeHit);
        this.iHitPoint = i4;
        this.iFinalHitPoint = i4;
        int i5 = this.data.sDodgePoint + ((this.sLevel - 1) * this.data.sUpgradeDodge);
        this.iDodgePoint = i5;
        this.iFinalDodgePoint = i5;
        int i6 = this.data.sCriticalPoint + ((this.sLevel - 1) * this.data.sUpgradeCritical);
        this.iCriticalPoint = i6;
        this.iFinalCriticalPoint = i6;
    }

    void updateAttribute(boolean isLevelUp) {
        int tempHP = 1;
        int tempMaxHP = 1;
        if (!isLevelUp) {
            tempHP = this.iHP;
            tempMaxHP = this.iFinalHP;
        }
        this.iFinalHP = (this.equip.iCurDurability == 0 ? 0 : this.equip.iFinalHP) + this.iMaxHP + this.iHPUpByProp;
        this.iFinalDP = (this.equip.iCurDurability == 0 ? 0 : this.equip.iFinalDP) + this.iDP + this.iDPUpByProp;
        this.iFinalDodgePoint = (this.equip.iCurDurability == 0 ? 0 : this.equip.iDodgePoint) + this.iDodgePoint + this.iDodgePointUpByProp;
        this.iFinalCriticalPoint = (this.equip.iCurDurability == 0 ? 0 : this.equip.iCriticalPoint) + this.iCriticalPoint + this.iCriticalPointUpByProp;
        if (this.iFinalCriticalPoint > 1000) {
            this.iFinalCriticalPoint = WebNetEvent.GotoWeb_Event_OK;
        }
        this.iFinalHitPoint = this.iHitPoint + (this.equip.iCurDurability != 0 ? this.equip.iHitPoint : 0) + this.iHitPointUpByProp;
        this.iHP = (int) ((((long) this.iFinalHP) * ((long) tempHP)) / ((long) tempMaxHP));
        int temp = (int) ((((long) ((this.iAP + this.equip.iFinalAP) + this.iAPUpByProp)) * ((long) this.sComboUpgradeAPPercent)) / 100);
        if (this.iComboNum > 9 && temp < 1) {
            temp = 1;
        }
        this.iFinalAP = this.iAP + this.equip.iFinalAP + temp + this.iAPUpByProp;
    }

    void integrate(boolean isLevelUp) {
        int tempHP = 1;
        int tempMaxHP = 1;
        if (!isLevelUp) {
            tempHP = this.iHP;
            tempMaxHP = this.iFinalHP;
        }
        this.iFinalHP = this.iMaxHP + this.famulus.iFinalHP + this.iHPUpByProp;
        this.iFinalDP = this.iDP + this.famulus.iFinalDP + this.iDPUpByProp;
        this.iFinalDodgePoint = this.iDodgePoint + this.famulus.iFinalDodgePoint + this.iDodgePointUpByProp;
        this.iFinalCriticalPoint = this.iCriticalPoint + this.famulus.iFinalCriticalPoint + this.iCriticalPointUpByProp;
        if (this.iFinalCriticalPoint > 1000) {
            this.iFinalCriticalPoint = WebNetEvent.GotoWeb_Event_OK;
        }
        this.iFinalHitPoint = this.iHitPoint + this.famulus.iFinalHitPoint + this.iHitPointUpByProp;
        this.iHP = (int) ((((long) this.iFinalHP) * ((long) tempHP)) / ((long) tempMaxHP));
        int temp = (int) ((((long) ((this.iAP + this.famulus.iFinalAP) + this.iAPUpByProp)) * ((long) this.sComboUpgradeAPPercent)) / 100);
        if (this.iComboNum > 9 && temp < 1) {
            temp = 1;
        }
        this.iFinalAP = this.iAP + this.famulus.iFinalAP + temp + this.iAPUpByProp;
        this.sHoldPoint = 99;
    }

    void reLive() {
        initAct();
        this.iHP = this.iFinalHP;
    }

    public void fire() {
        if (this.bActState == 0) {
            if (GameCanvas.instance.iFireCounter < 3 && this.iAttackIndex < 3) {
                this.iAttackIndex++;
            } else {
                this.iAttackIndex = 0;
            }
            act((byte) (this.iAttackIndex + 7));
            return;
        }
        if (this.bActID < 11 && this.iCurFrameIndex >= this.curAction.frameNum - 3) {
            this.isContinueAttack = true;
        }
    }

    void comboCounter() {
        this.isComboNumChange = true;
        this.bComboChangeEffectCounter = (byte) 0;
        this.sComboUpgradeAPPercent = (short) ((this.iComboNum / 10) + (this.iBreakNum * 5));
        int temp = (((this.isIntegrate ? this.famulus.iAP : this.equip.iFinalAP) + this.iAP) * this.sComboUpgradeAPPercent) / 100;
        if (this.iComboNum > 9 && temp < 1) {
            temp = 1;
        }
        if (this.isIntegrate) {
            this.iFinalAP = this.iAP + this.famulus.iFinalAP + temp;
        } else {
            this.iFinalAP = this.iAP + this.equip.iFinalAP + temp;
        }
    }

    void comboOver() {
        this.iComboNum = 0;
        this.iBreakNum = 0;
        this.sComboUpgradeAPPercent = (short) 0;
        if (this.isIntegrate) {
            this.iFinalAP = this.iAP + this.famulus.iFinalAP;
        } else {
            this.iFinalAP = this.iAP + this.equip.iFinalAP;
        }
    }

    public void processAttack(Enemy spr) {
        if (!this.isInvisible && !spr.isInvisible && this.bActID != 22) {
            boolean isHited = false;
            if (this.isCriticalKill) {
                if (this.isCurAttack) {
                    this.iComboNum++;
                    comboCounter();
                    int damagePoint = 0;
                    if (this.bActID == 24) {
                        damagePoint = getDamagePoint(spr, (byte) 25);
                        if (this.iCurFrameIndex == 34) {
                            spr.faceTo((FightSprite) this);
                            spr.act((byte) 11);
                        }
                    } else if (this.bActID == 25) {
                        damagePoint = this.isIntegrate ? getDamagePoint(spr, (byte) 23) : getDamagePoint(spr, (byte) 22);
                    }
                    if (isCritical()) {
                        spr.loseHP(damagePoint << 1, true);
                        GameCanvas.instance.setCriticalEffect(5);
                    } else {
                        spr.loseHP(damagePoint, false);
                        isHited = true;
                    }
                }
            } else if (isCanAttack(spr) && isHit(spr)) {
                if (spr.isDodge(this)) {
                    spr.setSpecialStateDuration(0);
                } else {
                    this.iComboNum++;
                    comboCounter();
                    if (this.sEP < 1995) {
                        this.sEP = (short) (this.sEP + 5);
                    } else {
                        if (this.sEP != 2000) {
                            this.isFillEP = true;
                        }
                        this.sEP = (short) 2000;
                        this.bEPCounter = (byte) 0;
                    }
                    spr.faceTo((FightSprite) this);
                    int damagePoint2 = 0;
                    if (this.bActID < 21) {
                        if (this.bActID == 11) {
                            GameCanvas.instance.isPause = true;
                        }
                        damagePoint2 = getDamagePoint(spr, (byte) (this.bActID - 7));
                    } else if (this.bActID == 26 && !this.isIntegrate) {
                        damagePoint2 = getDamagePoint(spr, (byte) 24);
                    }
                    if (isCritical()) {
                        spr.loseHP(damagePoint2 << 1, true);
                        GameCanvas.instance.setCriticalEffect(5);
                    } else {
                        spr.loseHP(damagePoint2, false);
                        isHited = true;
                    }
                    if (!spr.isDead() || spr.bType != 10) {
                        this.isBreakHit = false;
                        if (spr.isCanStopAct()) {
                            switch (this.bAttackType) {
                                case 0:
                                    if (spr.bActID == 4 || spr.bActID == 5) {
                                        spr.act((byte) 5);
                                    } else {
                                        spr.act((byte) 2);
                                    }
                                    break;
                                case 1:
                                    if (spr.bActID == 4 || spr.bActID == 5) {
                                        spr.act((byte) 8);
                                    } else {
                                        spr.act((byte) 6);
                                    }
                                    GameCanvas.instance.setVibrate(4, 1);
                                    break;
                                case 2:
                                    if (spr.bActID == 4 || spr.bActID == 5) {
                                        spr.act((byte) 8);
                                    } else if (spr.bActID == 14) {
                                        spr.act((byte) 16);
                                    } else {
                                        spr.act((byte) 7);
                                    }
                                    this.isBreakHit = true;
                                    GameCanvas.instance.setVibrate(4, 0);
                                    break;
                                case 3:
                                    if (spr.bActID == 4 || spr.bActID == 5) {
                                        spr.act((byte) 5);
                                    } else {
                                        spr.act((byte) 4);
                                    }
                                    break;
                            }
                        } else if (spr.isDead()) {
                            spr.act((byte) 6);
                        } else if (spr.bActID == 14) {
                            if (this.bAttackType == 2) {
                                spr.act((byte) 16);
                                spr.sHoldPoint = 0;
                                this.isBreakHit = true;
                            } else {
                                spr.act((byte) 15);
                            }
                            spr.showHurt();
                        }
                    } else {
                        return;
                    }
                }
            }
            if (isHited) {
                GameCanvas.instance.playSound(2);
            }
        }
    }

    void shorHurt() {
        int dir;
        int bActionID;
        if (0 < 2) {
            dir = this.bCurDir;
        } else {
            dir = this.bPreDir;
        }
        switch (this.bActID) {
            case 5:
                bActionID = dir + 2;
                break;
            case 6:
                bActionID = dir + 4;
                break;
            default:
                bActionID = dir;
                break;
        }
        EffectSprite be = new EffectSprite(GameCanvas.beatedEffectAS, bActionID);
        be.iMapX = this.iMapX + ((this.iFootWidth - be.iFootWidth) >> 1) + Tool.getNextRnd(-4, 5);
        be.iMapY = this.iMapY + ((this.iFootHeight - be.iFootHeight) >> 1) + Tool.getNextRnd(-4, 5);
        GameCanvas.instance.addMapElement(be);
    }

    void useSkill() {
        int skillIndex;
        if ((this.isIntegrate && this.famulus.skill != null) || (!this.isIntegrate && this.equip.skill != null)) {
            if (isCanUseSkill()) {
                if (this.isIntegrate) {
                    skillIndex = this.famulus.skill.data.bID;
                } else {
                    skillIndex = this.equip.skill.data.bID;
                }
                if (skillIndex < 14) {
                    act((byte) ((skillIndex + 12) - 5));
                    return;
                } else {
                    if (skillIndex < 22) {
                        act((byte) 21);
                        this.iRecoverPoint = getRecoverPoint(this.bSkillIndex + 5);
                        return;
                    }
                    return;
                }
            }
            return;
        }
        GameCanvas.instance.addDropInfo("无可用技能！");
    }

    int getDamagePoint(FightSprite spr, byte skillIndex) {
        Skill curSkill;
        int skillCoefficient;
        int skillAmend;
        int damagePoint;
        if (skillIndex < 5 || skillIndex > 21 || this.isUseSkillByScript) {
            curSkill = new Skill(GameCanvas.skillDatas[skillIndex]);
            if (this.isUseSkillByScript) {
                curSkill.bLevel = this.bSkillLevelByScript;
            }
        } else {
            curSkill = this.isIntegrate ? this.famulus.skill : this.equip.skill;
        }
        if (curSkill.data.damageStr.length > 1) {
            short[] temp = new short[curSkill.data.damageStr.length];
            for (int i = 0; i < temp.length; i++) {
                temp[i] = Short.parseShort(curSkill.data.damageStr[i]);
            }
            if (curSkill.bLevel < 9) {
                skillCoefficient = temp[0] + (curSkill.bLevel * temp[1]);
            } else {
                skillCoefficient = temp[0] + (curSkill.bLevel * temp[1]) + temp[3];
            }
        } else {
            skillCoefficient = Short.parseShort(curSkill.data.damageStr[0]);
        }
        if (curSkill.data.amendStr.length > 1) {
            short[] temp2 = new short[curSkill.data.amendStr.length];
            for (int i2 = 0; i2 < temp2.length; i2++) {
                temp2[i2] = Short.parseShort(curSkill.data.amendStr[i2]);
            }
            skillAmend = temp2[0] + (curSkill.bLevel * temp2[1]) + ((curSkill.bLevel / temp2[2]) * temp2[3]);
        } else {
            skillAmend = Short.parseShort(curSkill.data.amendStr[0]);
        }
        int damagePoint2 = (int) ((((((long) this.iFinalAP) * ((long) skillCoefficient)) / 100) + ((long) skillAmend)) - ((long) spr.iDP));
        int levelCoefficient = ((this.sLevel - spr.sLevel) * 5) + 100;
        if (levelCoefficient < 50) {
            levelCoefficient = 50;
        }
        if (levelCoefficient > 150) {
            levelCoefficient = 150;
        }
        int damagePoint3 = (int) (((((long) damagePoint2) * ((long) levelCoefficient)) * ((long) ((spr.bActID != 14 || skillIndex == 4) ? spr.attributeData.bBeinjuredCoefficient : (byte) 1))) / 10000);
        if (skillIndex == 4) {
            damagePoint = damagePoint3 + (((this.equip.iFinalDurability * 30) * 10) / 100);
        } else {
            damagePoint = damagePoint3 + Tool.getNextRnd(((-damagePoint3) * 10) / 100, (damagePoint3 * 10) / 100);
        }
        if (damagePoint < 10) {
            damagePoint = 10;
        }
        if (spr.sHoldPoint > curSkill.data.sDestroyPointForHold) {
            spr.sHoldPoint -= curSkill.data.sDestroyPointForHold;
        } else {
            spr.sHoldPoint = 0;
        }
        return damagePoint;
    }

    private int getRecoverPoint(int i) {
        int i2;
        Skill skill = new Skill(GameCanvas.skillDatas[i]);
        if (skill.data.damageStr.length > 1) {
            short[] sArr = new short[skill.data.damageStr.length];
            for (int i3 = 0; i3 < sArr.length; i3++) {
                sArr[i3] = Short.parseShort(skill.data.damageStr[i3]);
            }
            if (sArr.length == 1) {
                i2 = sArr[0];
            } else {
                i2 = sArr[0] + (skill.bLevel * sArr[1]);
            }
        } else {
            i2 = Short.parseShort(skill.data.damageStr[0]);
        }
        if (i < 17) {
            if (i <= 13) {
                return 0;
            }
            return ((this.iFinalHP * i2) / 100) + Short.parseShort(skill.data.amendStr[0]) + (skill.bLevel * Short.parseShort(skill.data.amendStr[1])) + ((skill.bLevel / Short.parseShort(skill.data.amendStr[2])) * Short.parseShort(skill.data.amendStr[3]));
        }
        return Short.parseShort(skill.data.amendStr[0]) + (skill.bLevel * Short.parseShort(skill.data.amendStr[1]));
    }

    @Override // com.dobetter.client.FightSprite, com.dobetter.client.Sprite
    public void act(byte act) {
        if (this.bActID == act && this.bActState == 0) {
            getCurVxVy();
            return;
        }
        this.bActID = act;
        this.iCurFrameIndex = 0;
        switch (this.bActID) {
            case 0:
            case 1:
                this.bActState = (byte) 0;
                break;
            case 2:
            case 5:
            case 6:
                this.bActState = (byte) 1;
                break;
            case 3:
            case 4:
            case 11:
            default:
                this.bActState = (byte) 4;
                break;
            case 7:
            case 8:
            case Interpreter.ERR_ARR_OUT /* 9 */:
            case 10:
                this.bActState = (byte) 2;
                break;
            case 12:
            case 13:
            case 14:
            case 15:
            case Graphics.TOP /* 16 */:
            case Graphics.HT /* 17 */:
            case 18:
            case TimeUtils.HUNDRED_DAY_FIELD_LEN /* 19 */:
            case 20:
            case 21:
                this.bActState = (byte) 3;
                break;
        }
        GameCanvas.instance.playSound();
        getCurActionData();
    }

    public void cycleAction() {
        if (!this.isSpriteAnimate && this.actionSet != null) {
            if (this.isKeepFrame) {
                getCurData();
                return;
            }
            if (this.iCurFrameIndex < this.curAction.frameNum - 1) {
                this.iCurFrameIndex++;
                getCurActionData();
                return;
            }
            switch (this.bActID) {
                case 0:
                case 1:
                    this.iCurFrameIndex = 0;
                    getCurActionData();
                    break;
                case 2:
                case 5:
                case 6:
                    if (this.isIntegrate) {
                        this.sHoldPoint = 99;
                    } else {
                        this.sHoldPoint = 49;
                    }
                    initAct();
                    break;
                case 3:
                    GameCanvas.instance.gameOver();
                    break;
                case 4:
                case 11:
                case 12:
                case 13:
                case 14:
                case 15:
                case Graphics.TOP /* 16 */:
                case Graphics.HT /* 17 */:
                case 18:
                case TimeUtils.HUNDRED_DAY_FIELD_LEN /* 19 */:
                case 20:
                case 23:
                default:
                    initAct();
                    break;
                case 7:
                case 8:
                case Interpreter.ERR_ARR_OUT /* 9 */:
                    if (this.isContinueAttack) {
                        this.isContinueAttack = false;
                        act((byte) (this.bActID + 1));
                    } else {
                        GameCanvas.instance.iFireCounter = 0;
                        initAct();
                    }
                    break;
                case 10:
                    if (this.isContinueAttack) {
                        this.isContinueAttack = false;
                        act((byte) 7);
                    } else {
                        GameCanvas.instance.iFireCounter = 0;
                        initAct();
                    }
                    break;
                case 21:
                    if (this.bSkillIndex != 15 && this.bSkillIndex != 16) {
                        this.isUseMiscellaneous = true;
                        this.bMiscellaneousIndex = (byte) 0;
                    }
                    initAct();
                    miscellaneousEffect();
                    break;
                case 22:
                    if (enemyNumInCheckRange() > 0) {
                        act((byte) 24);
                        this.isCriticalKill = true;
                    } else {
                        act((byte) 23);
                    }
                    break;
                case Graphics.RT /* 24 */:
                    act((byte) 25);
                    for (int i = 0; i < GameCanvas.instance.fightElement.size(); i++) {
                        Sprite spr = (Sprite) GameCanvas.instance.fightElement.elementAt(i);
                        spr.iMapX = (this.isFaceToTheRight ? (this.iMapX - 64) + 120 : (this.iMapX + 64) - 120) - (spr.iFootWidth >> 1);
                        if (spr.iMapX <= 0) {
                            spr.iMapX = 1;
                        }
                        if (spr.iMapX >= (GameCanvas.instance.iViewMapX + GameCanvas.SCREEN_WIDTH) - spr.iFootWidth) {
                            spr.iMapX = ((GameCanvas.instance.iViewMapX + GameCanvas.SCREEN_WIDTH) - spr.iFootWidth) - 1;
                        }
                    }
                    break;
                case 25:
                    this.isCriticalKill = false;
                    GameCanvas.instance.fightElement.removeAllElements();
                    initAct();
                    if (this.isUseCriticalKillByScript) {
                        GameCanvas.instance.setScriptState((byte) 3);
                        this.isUseCriticalKillByScript = false;
                    }
                    break;
                case 26:
                    if (!this.isIntegrate) {
                        integrate(false);
                    } else {
                        updateAttribute(false);
                    }
                    this.isIntegrate = !this.isIntegrate;
                    updateSkill();
                    this.sCDCount[1] = 0;
                    initAct();
                    break;
            }
            GameCanvas.instance.basicTurtorialKeyResponse();
        }
    }

    @Override // com.dobetter.client.FightSprite, com.dobetter.client.Sprite
    public void initAct() {
        super.initAct();
        if (GameCanvas.instance.isWaitforSkillOver) {
            GameCanvas.instance.isWaitforSkillOver = false;
            GameCanvas.instance.script_call(GameCanvas.instance.taskOverScript, GameCanvas.instance.isToLoading);
            GameCanvas.instance.taskOverScript = null;
        }
    }

    @Override // com.dobetter.client.FightSprite, com.dobetter.client.Sprite
    public void getCurActionData() {
        this.iCurActionID = (this.isIntegrate ? (byte) 27 : (byte) 0) + this.bActID;
        getCurData();
        getCurVxVy();
        if (this.bActID == 25) {
            if (this.iCurFrameIndex == 98 && !GameCanvas.instance.isDurabilityNonLossForever && !GameCanvas.instance.isDurabilityNonLoss) {
                if (this.isIntegrate) {
                    this.sCDCount[1] = 0;
                    this.famulus.iCurDurability = 0;
                    return;
                } else {
                    this.sCDCount[0] = 0;
                    this.sCDCount[1] = 0;
                    GameCanvas.instance.destroySword();
                    return;
                }
            }
            return;
        }
        if (this.bActID == 11) {
            if (this.isBreakHit) {
                this.sCDCount[1] = 0;
                this.isBreakHit = false;
                if (this.iComboNum > 1) {
                    this.iBreakNum++;
                }
                comboCounter();
                if (!GameCanvas.instance.isDurabilityNonLossForever && !GameCanvas.instance.isDurabilityNonLoss) {
                    if (this.isIntegrate) {
                        if (this.famulus.iCurDurability > this.famulus.iFinalDurability / 3) {
                            this.famulus.iCurDurability -= this.famulus.iFinalDurability / 3;
                            return;
                        } else {
                            this.famulus.iCurDurability = 0;
                            return;
                        }
                    }
                    if (this.equip.iCurDurability > this.equip.iFinalDurability / 3) {
                        this.equip.iCurDurability -= this.equip.iFinalDurability / 3;
                        return;
                    } else {
                        this.sCDCount[0] = 0;
                        GameCanvas.instance.destroySword();
                        return;
                    }
                }
                return;
            }
            return;
        }
        if (this.bActID == 24 && this.isUseCriticalKillByScript && !this.isCriticalKillEnd && this.iCurFrameIndex == 33) {
            this.isCriticalKill = false;
            this.isKeepFrame = true;
            GameCanvas.instance.fightElement.removeAllElements();
            this.isUseCriticalKillByScript = false;
        }
    }

    public int enemyNumInCheckRange() {
        GameCanvas.instance.fightElement.removeAllElements();
        for (int i = 0; i < GameCanvas.instance.enemyInScreen.size(); i++) {
            Enemy e = (Enemy) GameCanvas.instance.enemyInScreen.elementAt(i);
            if (!e.isInvisible && isHit(e) && e.isCurBeAttack) {
                GameCanvas.instance.fightElement.addElement(e);
            }
        }
        return GameCanvas.instance.fightElement.size();
    }

    @Override // com.dobetter.client.FightSprite, com.dobetter.client.Sprite
    public void cycle() {
        super.cycle();
        if (this.iLevelUpDuration > 0) {
            if (this.iLevelUpCounter < this.iLevelUpDuration) {
                this.iLevelUpCounter++;
            } else {
                this.iLevelUpCounter = 0;
                this.iLevelUpDuration = 0;
            }
        }
        for (int i = 0; i < this.sCycleTime.length && (this.isIntegrate || this.equip.iCurDurability != 0); i++) {
            if (this.sCycleTime[i] > 0 && this.sCDCount[i] < this.sCycleTime[i]) {
                short[] sArr = this.sCDCount;
                sArr[i] = (short) (sArr[i] + 1);
            }
        }
        if (this.isFillEP) {
            if (this.bEPCounter < GameCanvas.instance.sceneUIAS.actionDatas[7].frameNum) {
                this.bEPCounter = (byte) (this.bEPCounter + 1);
            } else {
                this.bEPCounter = (byte) 0;
                this.isFillEP = false;
            }
        }
        if (this.isComboNumChange) {
            if (this.bComboChangeEffectCounter < 2) {
                this.bComboChangeEffectCounter = (byte) (this.bComboChangeEffectCounter + 1);
            } else {
                this.bComboChangeEffectCounter = (byte) 0;
                this.isComboNumChange = false;
            }
        }
        if (this.iImmuneInjuryDuration > 0) {
            if (this.iImmuneInjuryCount < this.iImmuneInjuryDuration) {
                this.iImmuneInjuryCount++;
            } else {
                this.iImmuneInjuryCount = 0;
                this.iImmuneInjuryDuration = 0;
            }
        }
        if (this.isUseMiscellaneous) {
            int frameNum = 0;
            if (this.bSkillIndex >= 9 && this.bSkillIndex < 12) {
                frameNum = this.miscellaneousAS.actionDatas[1].frameNum;
            } else if (this.bSkillIndex >= 12 || this.bSkillIndex < 15) {
                frameNum = this.miscellaneousAS.actionDatas[2].frameNum;
            }
            if (this.bMiscellaneousIndex < frameNum) {
                this.bMiscellaneousIndex = (byte) (this.bMiscellaneousIndex + 1);
            } else {
                this.bMiscellaneousIndex = (byte) 0;
                this.isUseMiscellaneous = false;
            }
        }
        cycleAction();
        if (this.famulus != null) {
            this.famulus.cycle();
            this.famulus.processMove(this.iMapX + (this.iFootWidth >> 1), this.iMapY + (this.iFootHeight >> 1), 42, 20, 20);
        }
        if (this.target != null && this.iTargetStageID == GameCanvas.instance.iCurrentStageID) {
            int targetX = this.target.iMapX + (this.target.iFootWidth >> 1);
            int targetY = this.target.iMapY + (this.target.iFootHeight >> 1);
            int curX = this.iMapX + (this.iFootWidth >> 1);
            int curY = this.iMapY + (this.iFootHeight >> 1);
            int disX = Math.abs(targetX - curX);
            int disY = Math.abs(targetY - curY);
            this.iGuideArrowIndex = -1;
            if (disX > 80 || disY > 60) {
                if (disX == 0) {
                    if (curY > targetY) {
                        this.iGuideArrowIndex = 0;
                        return;
                    } else {
                        this.iGuideArrowIndex = 4;
                        return;
                    }
                }
                if (disY == 0) {
                    if (curX > targetX) {
                        this.iGuideArrowIndex = 2;
                        return;
                    } else {
                        this.iGuideArrowIndex = 6;
                        return;
                    }
                }
                int tan = (100000 * disY) / disX;
                if (tan > 241421) {
                    if (curY > targetY) {
                        this.iGuideArrowIndex = 0;
                        return;
                    } else {
                        this.iGuideArrowIndex = 4;
                        return;
                    }
                }
                if (tan < 41421) {
                    if (curX > targetX) {
                        this.iGuideArrowIndex = 2;
                        return;
                    } else {
                        this.iGuideArrowIndex = 6;
                        return;
                    }
                }
                if (curX > targetX) {
                    if (curY > targetY) {
                        this.iGuideArrowIndex = 1;
                        return;
                    } else {
                        this.iGuideArrowIndex = 3;
                        return;
                    }
                }
                if (curY > targetY) {
                    this.iGuideArrowIndex = 7;
                } else {
                    this.iGuideArrowIndex = 5;
                }
            }
        }
    }

    private void miscellaneousEffect() {
        if (this.bSkillIndex >= 9 && this.bSkillIndex < 12) {
            addHP(this.iRecoverPoint);
            setRaisingNumber(this.iRecoverPoint, 2);
            return;
        }
        if (this.bSkillIndex >= 12 && this.bSkillIndex < 15) {
            if (this.isIntegrate) {
                this.famulus.iCurDurability += this.iRecoverPoint;
                if (this.famulus.iCurDurability > this.famulus.iFinalDurability) {
                    this.famulus.iCurDurability = this.famulus.iFinalDurability;
                }
            } else {
                this.equip.iCurDurability += this.iRecoverPoint;
                if (this.equip.iCurDurability > this.equip.iFinalDurability) {
                    this.equip.iCurDurability = this.equip.iFinalDurability;
                }
            }
            setRaisingNumber(this.iRecoverPoint, 3);
            return;
        }
        if (this.bSkillIndex == 15 || this.bSkillIndex == 16) {
            this.iImmuneInjuryDuration = this.iRecoverPoint;
        }
    }

    public void addHP(int value) {
        this.iHP += value;
        if (this.iHP > this.iFinalHP) {
            this.iHP = this.iFinalHP;
        } else if (this.iHP < 0) {
            this.iHP = 0;
        }
    }

    @Override // com.dobetter.client.FightSprite, com.dobetter.client.Sprite
    public void draw(Graphics g, int viewMapX, int viewMapY, int offX, int offY) {
        if (this.actionSet != null) {
            int drawX = (this.iMapX - viewMapX) + offX;
            int drawY = (this.iMapY - viewMapY) + offY;
            int i = drawX + 15;
            int i2 = drawY - 30;
            drawSkillEffectBackOfRole(g, drawX, drawY);
            if (this.target != null && this.iGuideArrowIndex >= 0) {
                this.guideAS.drawFrameCycle(g, this.iGuideArrowIndex, Tool.countTimes, drawX, drawY);
            }
            this.actionSet.drawFrame(g, this.curFrame, drawX, drawY, this.isFaceToTheRight && this.bActID != 25);
            drawSkillEffectInFrontOfRole(g, drawX, drawY);
            drawFace(g, drawX, drawY);
            for (int i3 = 0; i3 < this.showTimes.length; i3++) {
                if (this.showTimes[i3] > 0) {
                    int bx = drawX + ((this.iFootWidth - GameCanvas.missImg.getWidth()) >> 1);
                    int by = (((drawY - this.curFrame.edgeRect.height) - GameCanvas.missImg.getHeight()) - 10) + this.showTimes[i3];
                    Tool.drawTile(g, GameCanvas.missImg, i3, GameCanvas.missImg.getWidth(), GameCanvas.missImg.getHeight(), 0, bx, by);
                }
            }
            drawRaisingNumber(g, drawX, drawY);
            drawLogicRect(g, drawX, drawY);
            if (this.iLevelUpDuration > 0) {
                this.levelUpAS.drawFrameCycle(g, 0, this.iLevelUpCounter, drawX, drawY);
            }
        }
    }

    protected void drawSkillEffectBackOfRole(Graphics g, int roleX, int roleY) {
        switch (this.bActID) {
            case 12:
            case 14:
            case Graphics.TOP /* 16 */:
            case 21:
                if (this.skillAS != null && !this.isCriticalKill) {
                    this.skillAS.drawFrameCycle(g, 1, this.iCurFrameIndex, roleX, roleY, this.isFaceToTheRight);
                    break;
                }
                break;
            case 26:
                if (!this.isIntegrate) {
                    this.integrateAS.drawFrameCycle(g, 1, this.iCurFrameIndex, roleX, roleY, this.isFaceToTheRight);
                } else {
                    this.integrateAS.drawFrameCycle(g, 3, this.iCurFrameIndex, roleX, roleY, this.isFaceToTheRight);
                }
                break;
        }
    }

    protected void drawSkillEffectInFrontOfRole(Graphics g, int roleX, int roleY) {
        switch (this.bActID) {
            case 12:
            case 13:
            case 14:
            case 15:
            case Graphics.TOP /* 16 */:
            case Graphics.HT /* 17 */:
            case 18:
            case TimeUtils.HUNDRED_DAY_FIELD_LEN /* 19 */:
            case 21:
                if (this.skillAS != null && !this.isCriticalKill) {
                    this.skillAS.drawFrameCycle(g, 0, this.iCurFrameIndex, roleX, roleY, this.isFaceToTheRight);
                }
                break;
            case 26:
                if (!this.isIntegrate) {
                    this.integrateAS.drawFrameCycle(g, 0, this.iCurFrameIndex, roleX, roleY, this.isFaceToTheRight);
                } else {
                    this.integrateAS.drawFrameCycle(g, 2, this.iCurFrameIndex, roleX, roleY, this.isFaceToTheRight);
                }
                break;
        }
        if (this.isUseMiscellaneous) {
            int actIndex = 0;
            if (this.bSkillIndex >= 9 && this.bSkillIndex < 12) {
                actIndex = 1;
            } else if (this.bSkillIndex >= 12 || this.bSkillIndex < 15) {
                actIndex = 2;
            }
            this.miscellaneousAS.drawFrameCycle(g, actIndex, this.bMiscellaneousIndex - 1, roleX, roleY);
        }
        if (this.iImmuneInjuryDuration > 0) {
            this.miscellaneousAS.drawFrameCycle(g, 0, this.iImmuneInjuryCount, roleX, roleY);
        }
    }

    public void updateEquipment(Equip newEquip) {
        if (newEquip != null) {
            this.equip = newEquip;
            switchWeapon(newEquip.data.resStr, 4);
            if (!this.isIntegrate) {
                this.sCDCount[1] = 0;
                updateSkill();
                updateAttribute(false);
            }
        }
    }

    public void updateSkill() {
        this.sCDCount[0] = 0;
        if (!this.isIntegrate) {
            if (this.equip.skill != null) {
                if (GameCanvas.cdTime) {
                    this.sCycleTime[0] = 1;
                } else {
                    this.sCycleTime[0] = this.equip.skill.data.sCD;
                }
                loadSkillRes(this.equip.skill.data.bID);
                this.bSkillIndex = (byte) (this.equip.skill.data.bID - 5);
                return;
            }
            this.bSkillIndex = (byte) -1;
            this.sCycleTime[0] = 0;
            return;
        }
        if (this.famulus.skill != null) {
            if (GameCanvas.cdTime) {
                this.sCycleTime[0] = 1;
            } else {
                this.sCycleTime[0] = this.famulus.skill.data.sCD;
            }
            loadSkillRes(this.famulus.skill.data.bID);
            this.bSkillIndex = (byte) (this.famulus.skill.data.bID - 5);
            return;
        }
        this.bSkillIndex = (byte) -1;
        this.sCycleTime[0] = 0;
    }

    public boolean isCanUseSkill() {
        int reduceDurability;
        int reduceDurability2;
        if (this.sCDCount[0] < this.sCycleTime[0]) {
            GameCanvas.instance.addDropInfo("<c=" + GameCanvas.COLORS[0] + ",0,1>技能尚未冷却！");
            return false;
        }
        if (this.isIntegrate) {
            if (GameCanvas.instance.isDurabilityNonLossForever || GameCanvas.instance.isDurabilityNonLoss) {
                reduceDurability2 = 0;
            } else if (this.famulus.skill.data.bID >= 14 && this.famulus.skill.data.bID <= 21) {
                reduceDurability2 = 0;
            } else {
                reduceDurability2 = this.famulus.skill.data.sReducedMP + (this.famulus.skill.data.sUpgradeReducedMP * this.famulus.skill.bLevel);
            }
            if (this.famulus.iCurDurability >= reduceDurability2) {
                this.famulus.iCurDurability -= reduceDurability2;
                this.sCDCount[0] = 0;
                if (this.famulus.iCurDurability == 0) {
                    this.sCDCount[1] = 0;
                }
                return true;
            }
            if (this.famulus.iCurDurability > 0) {
                this.famulus.iCurDurability = 0;
                this.sCDCount[0] = 0;
                this.sCDCount[1] = 0;
                return true;
            }
            GameCanvas.instance.addDropInfo("<c=" + GameCanvas.COLORS[0] + ",0,1>灵剑耐久不足！");
            return false;
        }
        if (GameCanvas.instance.isDurabilityNonLossForever || GameCanvas.instance.isDurabilityNonLoss) {
            reduceDurability = 0;
        } else if (this.equip.skill.data.bID >= 14 && this.equip.skill.data.bID <= 21) {
            reduceDurability = 0;
        } else {
            reduceDurability = this.equip.skill.data.sReducedMP + (this.equip.skill.data.sUpgradeReducedMP * this.equip.skill.bLevel);
        }
        if (this.equip.iCurDurability >= reduceDurability) {
            this.equip.iCurDurability -= reduceDurability;
            this.sCDCount[0] = 0;
            if (this.equip.iCurDurability == 0) {
                updateAttribute(false);
                GameCanvas.instance.destroySword();
                this.sCDCount[1] = 0;
            }
            return true;
        }
        if (this.equip.iCurDurability > 0) {
            GameCanvas.instance.destroySword();
            this.sCDCount[0] = 0;
            this.sCDCount[1] = 0;
            return true;
        }
        GameCanvas.instance.addDropInfo("<c=" + GameCanvas.COLORS[0] + ",0,1>武器耐久不足！");
        return false;
    }

    @Override // com.dobetter.client.FightSprite
    public void clear() {
        super.clear();
        if (this.enemy != null) {
            this.enemy.freeSprite();
            this.enemy = null;
        }
        GameCanvas.instance.isKeyLocked = false;
        stopAttack(false);
    }

    public void stopAttack(boolean isFightOver) {
        initAct();
        this.isContinueAttack = false;
        this.iAttackIndex = 0;
        this.isCriticalKill = false;
    }

    void saveRoleData(DataOutputStream dos) {
        try {
            dos.writeShort(this.sLevel);
            dos.writeInt(this.iCurrentExp);
            dos.writeInt(this.iLevelUpExp);
            dos.writeInt(this.iMapX);
            dos.writeInt(this.iMapY);
            dos.writeByte(this.bCurDir);
            dos.writeInt(this.iHPUpByProp);
            dos.writeInt(this.iAPUpByProp);
            dos.writeInt(this.iDPUpByProp);
            dos.writeInt(this.iHitPointUpByProp);
            dos.writeInt(this.iDodgePointUpByProp);
            dos.writeInt(this.iCriticalPointUpByProp);
            dos.writeShort(this.sEP);
            if (this.sCycleTime[0] > 0) {
                dos.writeBoolean(true);
                dos.writeByte(this.bSkillIndex);
            } else {
                dos.writeBoolean(false);
            }
            if (this.famulus == null) {
                dos.writeBoolean(false);
            } else {
                dos.writeBoolean(true);
                this.famulus.saveFamulus(dos);
            }
            if (this.weaponStr != null) {
                dos.writeBoolean(true);
                dos.writeInt(this.weaponStr.length);
                for (int i = 0; i < this.weaponStr.length; i++) {
                    dos.writeUTF(this.weaponStr[i]);
                }
            } else {
                dos.writeBoolean(false);
            }
            if (this.swordSoulStr != null) {
                dos.writeBoolean(true);
                dos.writeInt(this.swordSoulStr.length);
                for (int i2 = 0; i2 < this.swordSoulStr.length; i2++) {
                    dos.writeUTF(this.swordSoulStr[i2]);
                }
            } else {
                dos.writeBoolean(false);
            }
            dos.writeBoolean(this.isIntegrate);
        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    static Role readRole(DataInputStream dis) {
        Role role = null;
        try {
            short level = dis.readShort();
            Role role2 = new Role(GameCanvas.roleData, level);
            try {
                role2.initData();
                role2.iCurrentExp = dis.readInt();
                role2.iLevelUpExp = dis.readInt();
                role2.iMapX = dis.readInt();
                role2.iMapY = dis.readInt();
                role2.setDir(dis.readByte());
                role2.iHPUpByProp = dis.readInt();
                role2.iAPUpByProp = dis.readInt();
                role2.iDPUpByProp = dis.readInt();
                role2.iHitPointUpByProp = dis.readInt();
                role2.iDodgePointUpByProp = dis.readInt();
                role2.iCriticalPointUpByProp = dis.readInt();
                role2.sEP = dis.readShort();
                if (dis.readBoolean()) {
                    role2.bSkillIndex = dis.readByte();
                }
                if (dis.readBoolean()) {
                    role2.famulus = Famulus.readFamulus(role2, dis);
                }
                if (dis.readBoolean()) {
                    int length = dis.readInt();
                    role2.weaponStr = new String[length];
                    for (int i = 0; i < length; i++) {
                        role2.weaponStr[i] = dis.readUTF();
                    }
                }
                if (dis.readBoolean()) {
                    int length2 = dis.readInt();
                    role2.swordSoulStr = new String[length2];
                    for (int i2 = 0; i2 < length2; i2++) {
                        role2.swordSoulStr[i2] = dis.readUTF();
                    }
                }
                role2.isIntegrate = dis.readBoolean();
                return role2;
            } catch (Exception e) {
                e = e;
                role = role2;
                e.printStackTrace();
                return role;
            }
        } catch (Exception e2) {
            e = e2;
        }
    }

    void switchWeapon(String resStr, int index) {
        if (this.actionSet != null && resStr != null) {
            this.weaponStr = Tool.getStringArray(resStr, ',');
            if (GameCanvas.instance.isAttackable) {
                reload(this.weaponStr, index);
            }
        }
    }

    void reload(String[] resStr, int index) {
        if (this.actionSet != null && resStr != null) {
            if (this.actionSet.images[index] != null) {
                this.actionSet.images[index].recycle();
                this.actionSet.images[index] = null;
            }
            if (resStr.length > 1) {
                this.actionSet.images[index] = Tool.loadPPImage("/action/" + resStr[0] + ".p", "/action/" + resStr[1] + ".pt");
            } else {
                this.actionSet.images[index] = Tool.loadPImage("/action/" + resStr[0] + ".p");
            }
        }
    }
}
