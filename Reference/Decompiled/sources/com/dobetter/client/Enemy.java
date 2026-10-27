package com.dobetter.client;

import android.support.v4.util.TimeUtils;
import com.dobetter.action.ActionSet;
import com.dobetter.client.data.EnemyData;
import com.dobetter.common.Common;
import com.dobetter.common.Graphics;
import com.dobetter.common.Tool;
import com.dobetter.script.Interpreter;

/* JADX INFO: loaded from: classes.dex */
public class Enemy extends FightSprite {
    public static final byte ACT_ATTACK1 = 17;
    public static final byte ACT_ATTACK2 = 18;
    public static final byte ACT_ATTACK3 = 19;
    public static final byte ACT_CRITICAL_HURTED0 = 11;
    public static final byte ACT_CRITICAL_HURTED1 = 12;
    public static final byte ACT_CRITICAL_HURTED2 = 13;
    public static final byte ACT_DEFENSE = 14;
    public static final byte ACT_DEFENSE_BREAKED = 16;
    public static final byte ACT_DEFENSING_HURTED = 15;
    public static final byte ACT_FLYING_AFTER_BLOW = 6;
    public static final byte ACT_FLYING_AFTER_BREAK = 9;
    public static final byte ACT_FLYING_AFTER_HURTED = 4;
    public static final byte ACT_FLYING_HURTED_AGAIN = 5;
    public static final byte ACT_FLYING_HURTED_BY_BREAK = 8;
    public static final byte ACT_HURTED_BY_BREAK = 7;
    public static final byte ACT_REBOUND = 10;
    public static final byte ACT_SELF_PROTECTION = 20;
    public static final byte BOSS2_ACT_SPECIAL_SKILL_END = 26;
    public static final byte BOSS2_ACT_SPECIAL_SKILL_START = 25;
    public static final byte BOSS_ACT_CRITICAL_KILL_CHECK = 21;
    public static final byte BOSS_ACT_CRITICAL_KILL_END = 24;
    public static final byte BOSS_ACT_CRITICAL_KILL_NAME = 23;
    public static final byte BOSS_ACT_CRITICAL_KILL_START = 22;
    public static final int hpBarH = 4;
    public static final int hpBarW = 20;
    public byte bDefenseCounter;
    public byte bPursueCounter;
    public EnemyData enemyData;
    public int[] iAttackIntervalCounter;
    int iBoss2SpecialSkillCounter;
    int iBossCriticalSkillCounter;
    int iDesPosX;
    int iDesPosY;
    boolean isBoss2CriticalKillHit;
    boolean isBossCriticalKill;
    public boolean[] isCanAttack;
    public boolean isCanDefecse;
    short sRefreshCounter;
    ActionSet specialAS;
    Role target;
    int iAttackModeID = -1;
    boolean isBossCanUSeCriticalSkill = true;
    byte bRefreshNum = 1;
    final short REFRESH_DURATION = 60;

    public Enemy() {
        this.isCheckMapBlock = false;
        this.isCheckCollision = false;
    }

    public Enemy(int id, EnemyData enemyData, int mx, int my) {
        this.iMoveSpeed = 2;
        this.ID = id;
        this.bType = enemyData.bType;
        this.enemyData = enemyData;
        this.iSortID = enemyData.sortID;
        loadRes(enemyData);
        this.isCheckCollision = false;
        this.iBornMapX = mx;
        this.iMapX = mx;
        this.iBornMapY = my;
        this.iMapY = my;
        this.isCheckMapBlock = true;
        this.iAttackIntervalCounter = new int[enemyData.sAttackInterval.length];
        this.isCanAttack = new boolean[enemyData.sAttackInterval.length];
        for (int i = 0; i < this.isCanAttack.length; i++) {
            this.isCanAttack[i] = true;
        }
        initAct();
        loadSkillRes();
        this.target = GameCanvas.instance.role;
    }

    void loadSkillRes() {
        if (this.bType == 11) {
            String[] imgs = {"s_36"};
            this.specialAS = Common.getActionSet("s_36", imgs, false, true);
        }
    }

    public void initData() {
        int temp;
        this.sHoldPoint = this.enemyData.sHoldPoint;
        this.isCanDefecse = true;
        if (this.sLevel < 100) {
            temp = this.sLevel % 10;
            if (this.bType == 9) {
                this.attributeData = GameCanvas.enemyAttributes[(this.sLevel / 10) - 1];
            } else if (this.bType == 11) {
                this.attributeData = GameCanvas.enemyAttributes[(this.sLevel / 10) + 8];
            } else if (this.bType == 10) {
                this.attributeData = GameCanvas.enemyAttributes[(this.sLevel / 10) + 17];
            }
        } else {
            temp = this.sLevel - 90;
            if (this.bType == 9) {
                this.attributeData = GameCanvas.enemyAttributes[8];
            } else if (this.bType == 11) {
                this.attributeData = GameCanvas.enemyAttributes[17];
            } else if (this.bType == 10) {
                this.attributeData = GameCanvas.enemyAttributes[26];
            }
        }
        int i = (int) (((long) ((this.attributeData.iHP + (this.attributeData.iUpgradeHP * temp)) * this.enemyData.sHPCoefficient)) / 100);
        this.iMaxHP = i;
        this.iHP = i;
        this.iAP = ((this.attributeData.sAP + (this.attributeData.sUpgradeAP * temp)) * this.enemyData.sAPCoefficient) / 100;
        this.iDP = this.attributeData.sDP + (this.attributeData.sUpgradeDP * temp);
        this.iHitPoint = this.attributeData.sHitPoint + (this.attributeData.sUpgradeHit * temp) + this.enemyData.sHitPoint;
        this.iDodgePoint = this.attributeData.sDodgePoint + (this.attributeData.sUpgradeDodge * temp) + this.enemyData.sDodgePoint;
    }

    public void setAttackPoint(int i) {
        this.iAttackModeID = i;
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
            case 4:
            case 5:
            case 6:
            case 7:
            case 8:
            case Interpreter.ERR_ARR_OUT /* 9 */:
            case 10:
            case 11:
            case 12:
            case 13:
                this.bActState = (byte) 1;
                showHurt();
                break;
            case 3:
                if (this.bType == 10) {
                    GameCanvas.instance.role.enemy = this;
                    GameCanvas.instance.setBGColor(15132390);
                }
                this.bActState = (byte) 4;
                break;
            case 14:
            case 15:
            case Graphics.TOP /* 16 */:
            case 21:
            case 22:
            case 23:
            default:
                this.bActState = (byte) 4;
                break;
            case Graphics.HT /* 17 */:
            case 18:
            case TimeUtils.HUNDRED_DAY_FIELD_LEN /* 19 */:
            case 20:
            case Graphics.RT /* 24 */:
                this.bActState = (byte) 2;
                break;
        }
        getCurActionData();
    }

    public boolean isCanUseSkill(byte actID) {
        return true;
    }

    @Override // com.dobetter.client.FightSprite, com.dobetter.client.Sprite
    public void getCurActionData() {
        this.iCurActionID = this.bActID;
        getCurData();
        if (this.bActID != 9) {
            getCurVxVy();
        }
        if (this.bActID == 8) {
            if (this.iCurFrameIndex == 5) {
                showHurt();
                return;
            }
            return;
        }
        if (this.bActID == 11) {
            if (this.iCurFrameIndex == 9) {
                showHurt();
                return;
            }
            return;
        }
        if (this.bActID == 18) {
            if ((this.iSortID == 241 || this.iSortID == 242 || this.iSortID == 240) && this.iCurFrameIndex == 9) {
                addBullet(this, this.actionSet, 27, this.target.iMapX, this.target.iMapY, (byte) 0, (byte) 0, getDamagePoint(this.target), this.enemyData.sDestroyedHoldPoint[this.iAttackModeID], true, true, true);
                return;
            }
            return;
        }
        if (this.bActID == 19) {
            if (this.iSortID == 210 || this.iSortID == 211 || this.iSortID == 212 || this.iSortID == 219 || this.iSortID == 221) {
                if (this.iCurFrameIndex == 4 || this.iCurFrameIndex == 10) {
                    addBullet(this, this.actionSet, 21, this.iMapX, this.iMapY, this.isFaceToTheRight ? (byte) 1 : (byte) 0, (byte) 30, getDamagePoint(this.target), this.enemyData.sDestroyedHoldPoint[this.iAttackModeID], false, false, true);
                    return;
                }
                return;
            }
            if (this.iSortID == 241 || this.iSortID == 242 || this.iSortID == 240) {
                if (this.iCurFrameIndex == 9) {
                    addBullet(this, this.actionSet, 28, this.target.iMapX, this.target.iMapY, (byte) 0, (byte) 0, getDamagePoint(this.target), this.enemyData.sDestroyedHoldPoint[this.iAttackModeID], true, true, true);
                    return;
                }
                return;
            } else {
                if ((this.iSortID == 238 || this.iSortID == 239) && this.iCurFrameIndex == 9) {
                    this.iMapX = this.target.iMapX + ((this.target.iFootWidth - this.iFootWidth) >> 1);
                    this.iMapY = this.target.iMapY + ((this.target.iFootHeight - this.iFootHeight) >> 1);
                    return;
                }
                return;
            }
        }
        if (this.bActID == 3) {
            if (this.iCurFrameIndex == 1) {
                GameCanvas.instance.setBGColor(-1);
                return;
            }
            if (this.iCurFrameIndex == 2) {
                GameCanvas.instance.setBGColor(15132390);
            } else if (this.iCurFrameIndex == 3) {
                GameCanvas.instance.setBGColor(10066329);
            } else if (this.iCurFrameIndex > 3) {
                GameCanvas.instance.role.enemy = null;
            }
        }
    }

    public void cycleAction() {
        int targetX1;
        int targetX2;
        int targetX3;
        int targetX4;
        if (!this.isSpriteAnimate) {
            if (this.isKeepFrame) {
                getCurData();
            }
            if (isDead()) {
                if (this.iCurFrameIndex < this.curAction.frameNum - 1) {
                    this.iCurFrameIndex++;
                    getCurActionData();
                    return;
                }
                switch (this.bActID) {
                    case 2:
                        act((byte) 6);
                        break;
                    case 3:
                        die();
                        break;
                    case 4:
                    case 5:
                    case 6:
                    case 8:
                    case 10:
                        if (this.bType != 10) {
                            act((byte) 3);
                        }
                        break;
                    case 7:
                        if (this.bType != 10) {
                            act((byte) 9);
                            if (!this.isFaceToTheRight) {
                                this.iCurVx = 30;
                                this.iCurVy = 0;
                            } else {
                                this.iCurVx = -30;
                                this.iCurVy = 0;
                            }
                        }
                        break;
                    case Interpreter.ERR_ARR_OUT /* 9 */:
                        if (this.bType != 10) {
                            getCurActionData();
                            if (GameCanvas.instance.moveSpriteByStep(this, 11, 5)) {
                                if (isInScreen(GameCanvas.instance.iViewMapX, GameCanvas.instance.iViewMapY, GameCanvas.SCREEN_WIDTH, GameCanvas.SCREEN_HEIGHT)) {
                                    if (!this.isFaceToTheRight) {
                                        this.iCurVx += 10;
                                    } else {
                                        this.iCurVx -= 10;
                                    }
                                }
                            } else {
                                act((byte) 10);
                                showHurt();
                            }
                        }
                        break;
                    case 11:
                        act((byte) 12);
                        break;
                    case 12:
                        this.iMapX = GameCanvas.instance.role.isFaceToTheRight ? (GameCanvas.instance.role.iMapX - 64) + 240 : (GameCanvas.instance.role.iMapX + 64) - this.iFootWidth;
                        if (this.iMapX >= (GameCanvas.instance.iViewMapX + GameCanvas.SCREEN_WIDTH) - this.iFootWidth) {
                            this.iMapX = ((GameCanvas.instance.iViewMapX + GameCanvas.SCREEN_WIDTH) - this.iFootWidth) - 1;
                        }
                        act((byte) 13);
                        break;
                    case 13:
                        act((byte) 3);
                        break;
                }
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
                    processHurtedOver();
                    break;
                case 3:
                case Graphics.TOP /* 16 */:
                case Graphics.HT /* 17 */:
                case 18:
                case TimeUtils.HUNDRED_DAY_FIELD_LEN /* 19 */:
                case 20:
                case 23:
                default:
                    initAct();
                    break;
                case 4:
                case 5:
                case 6:
                case 8:
                case 10:
                    processEnemyStand();
                    break;
                case 7:
                    act((byte) 9);
                    if (!this.isFaceToTheRight) {
                        this.iCurVx = 30;
                        this.iCurVy = 0;
                    } else {
                        this.iCurVx = -30;
                        this.iCurVy = 0;
                    }
                    break;
                case Interpreter.ERR_ARR_OUT /* 9 */:
                    getCurActionData();
                    if (GameCanvas.instance.moveSpriteByStep(this, 11, 5)) {
                        if (isInScreen(GameCanvas.instance.iViewMapX, GameCanvas.instance.iViewMapY, GameCanvas.SCREEN_WIDTH, GameCanvas.SCREEN_HEIGHT)) {
                            if (!this.isFaceToTheRight) {
                                this.iCurVx += 10;
                            } else {
                                this.iCurVx -= 10;
                            }
                        }
                    } else {
                        act((byte) 10);
                        showHurt();
                    }
                    break;
                case 11:
                    act((byte) 12);
                    break;
                case 12:
                    this.iMapX = GameCanvas.instance.role.isFaceToTheRight ? (GameCanvas.instance.role.iMapX - 64) + 240 : (GameCanvas.instance.role.iMapX + 64) - this.iFootWidth;
                    if (this.iMapX >= (GameCanvas.instance.iViewMapX + GameCanvas.SCREEN_WIDTH) - this.iFootWidth) {
                        this.iMapX = ((GameCanvas.instance.iViewMapX + GameCanvas.SCREEN_WIDTH) - this.iFootWidth) - 1;
                    }
                    act((byte) 13);
                    break;
                case 13:
                    this.sHoldPoint = this.enemyData.sHoldPoint;
                    this.isCanDefecse = true;
                    initAct();
                    break;
                case 14:
                    getCurActionData();
                    if (this.bDefenseCounter > 0) {
                        this.bDefenseCounter = (byte) (this.bDefenseCounter - 1);
                    } else {
                        act((byte) 20);
                    }
                    break;
                case 15:
                    processBossDefenseHurtedOver();
                    break;
                case 21:
                    if (isHit(this.target) && this.target.isCurBeAttack) {
                        this.target.isLocked = true;
                        this.target.enemy = this;
                        act((byte) 22);
                        this.isBossCriticalKill = true;
                    } else {
                        initAct();
                    }
                    break;
                case 22:
                    this.target.isLocked = false;
                    if (this.iSortID == 232 || this.iSortID == 233) {
                        GameCanvas.instance.setCriticalEffect(5);
                        act((byte) 24);
                    } else if (this.iSortID == 235 || this.iSortID == 236) {
                        if (this.target.isFaceToTheRight) {
                            targetX3 = ((this.target.iMapX + (this.target.iFootWidth >> 1)) - 64) - (this.iFootWidth >> 1);
                            targetX4 = ((this.target.iMapX + (this.target.iFootWidth >> 1)) + 64) - (this.iFootWidth >> 1);
                        } else {
                            targetX3 = ((this.target.iMapX + (this.target.iFootWidth >> 1)) + 64) - (this.iFootWidth >> 1);
                            targetX4 = ((this.target.iMapX + (this.target.iFootWidth >> 1)) - 64) - (this.iFootWidth >> 1);
                        }
                        if (!GameCanvas.instance.isOutOfMap(this, targetX3) && GameCanvas.instance.isUnBlocked(this, targetX3)) {
                            this.iMapX = targetX3;
                        } else if (GameCanvas.instance.isUnBlocked(this, targetX4) && !GameCanvas.instance.isOutOfMap(this, targetX4)) {
                            this.iMapX = targetX4;
                        } else {
                            this.iMapX = this.target.iMapX;
                        }
                        faceTo((FightSprite) this.target);
                        act((byte) 24);
                    } else if (this.iSortID == 241 || this.iSortID == 242) {
                        initAct();
                        addBullet(this, this.actionSet, 24, this.target.iMapX + ((this.target.iFootWidth - this.iFootWidth) >> 1), (this.target.iMapY + this.target.iFootHeight) - this.iFootHeight, (byte) 0, (byte) 0, getDamagePoint(this.target), this.iAttackModeID > -1 ? this.enemyData.sDestroyedHoldPoint[this.iAttackModeID] : (short) 0, true, true, false);
                    } else {
                        act((byte) 24);
                    }
                    break;
                case Graphics.RT /* 24 */:
                    this.isBossCanUSeCriticalSkill = false;
                    this.isBoss2CriticalKillHit = false;
                    this.isBossCriticalKill = false;
                    GameCanvas.instance.role.enemy = null;
                    initAct();
                    break;
                case 25:
                    if (this.target.isFaceToTheRight) {
                        targetX1 = ((this.target.iMapX + (this.target.iFootWidth >> 1)) - 64) - (this.iFootWidth >> 1);
                        targetX2 = ((this.target.iMapX + (this.target.iFootWidth >> 1)) + 64) - (this.iFootWidth >> 1);
                    } else {
                        targetX1 = ((this.target.iMapX + (this.target.iFootWidth >> 1)) + 64) - (this.iFootWidth >> 1);
                        targetX2 = ((this.target.iMapX + (this.target.iFootWidth >> 1)) - 64) - (this.iFootWidth >> 1);
                    }
                    if (!GameCanvas.instance.isOutOfMap(this, targetX1) && GameCanvas.instance.isUnBlocked(this, targetX1)) {
                        this.iMapX = targetX1;
                    } else if (!GameCanvas.instance.isOutOfMap(this, targetX2) && GameCanvas.instance.isUnBlocked(this, targetX2)) {
                        this.iMapX = targetX2;
                    } else {
                        this.iMapX = this.target.iMapX;
                    }
                    faceTo((FightSprite) this.target);
                    act((byte) 26);
                    break;
            }
        }
    }

    public void processHurtedOver() {
        if (this.bType == 10) {
            switch (this.iSortID) {
                case 231:
                case 234:
                case 237:
                case 240:
                case 243:
                    this.bDefenseCounter = (byte) 60;
                    break;
                case 232:
                case 235:
                case 238:
                case 241:
                case 244:
                    this.bDefenseCounter = (byte) 24;
                    break;
                case 233:
                case 236:
                case 239:
                case 242:
                case 245:
                    this.bDefenseCounter = (byte) 12;
                    break;
            }
            act((byte) 14);
        } else {
            initAct();
        }
        this.sHoldPoint = this.enemyData.sHoldPoint;
        this.isCanDefecse = true;
    }

    public void processEnemyStand() {
        this.sHoldPoint = this.enemyData.sHoldPoint;
        this.isCanDefecse = true;
        if (this.bType == 10) {
            boolean isCriticalSkill = false;
            if (this.iSortID == 232) {
                if (this.iHP < (this.iMaxHP * 30) / 100 && this.isBossCanUSeCriticalSkill) {
                    isCriticalSkill = true;
                }
            } else if (this.iSortID == 233 && this.iHP < (this.iMaxHP * 50) / 100 && this.isBossCanUSeCriticalSkill) {
                isCriticalSkill = true;
            }
            if (isCriticalSkill) {
                faceTo((FightSprite) this.target);
                act((byte) 21);
            } else {
                act((byte) 20);
                return;
            }
        }
        switch (this.iSortID) {
            case 210:
            case 211:
            case 212:
            case 219:
                setAttackPoint(2);
                act((byte) 19);
                break;
            default:
                initAct();
                break;
        }
    }

    private void processBossDefenseHurtedOver() {
        this.sHoldPoint = this.enemyData.sHoldPoint;
        if (this.iSortID == 231) {
            this.bDefenseCounter = (byte) 36;
            act((byte) 14);
            return;
        }
        if (this.iSortID == 232) {
            this.bDefenseCounter = (byte) 36;
            act((byte) 14);
            return;
        }
        if (this.iSortID == 233) {
            this.bDefenseCounter = (byte) 12;
            act((byte) 14);
            return;
        }
        if (this.iSortID == 234) {
            this.bDefenseCounter = (byte) 36;
            act((byte) 14);
            return;
        }
        if (this.iSortID == 235) {
            this.bDefenseCounter = (byte) 36;
            act((byte) 14);
            return;
        }
        if (this.iSortID == 236) {
            this.bDefenseCounter = (byte) 12;
            act((byte) 14);
            return;
        }
        if (this.iSortID == 237) {
            this.bDefenseCounter = (byte) 36;
            act((byte) 14);
            return;
        }
        if (this.iSortID == 238) {
            this.bDefenseCounter = (byte) 36;
            act((byte) 14);
            return;
        }
        if (this.iSortID == 239) {
            this.bDefenseCounter = (byte) 12;
            act((byte) 14);
            return;
        }
        if (this.iSortID == 240) {
            this.bDefenseCounter = (byte) 36;
            act((byte) 14);
            return;
        }
        if (this.iSortID == 241) {
            this.bDefenseCounter = (byte) 36;
            act((byte) 14);
            return;
        }
        if (this.iSortID == 242) {
            this.bDefenseCounter = (byte) 12;
            act((byte) 14);
            return;
        }
        if (this.iSortID == 243) {
            this.bDefenseCounter = (byte) 36;
            act((byte) 14);
        } else if (this.iSortID == 244) {
            this.bDefenseCounter = (byte) 36;
            act((byte) 14);
        } else if (this.iSortID == 245) {
            this.bDefenseCounter = (byte) 12;
            act((byte) 14);
        }
    }

    void processAI() {
        if (!this.isInvisible && !this.target.isDead() && !this.target.isInvisible) {
            if (this.target.isCriticalKill) {
                if (this.bActID == 13) {
                    GameCanvas.instance.moveSpriteByStep(this, 10, 5);
                    return;
                }
                return;
            }
            if (!isDead()) {
                int targetX = this.target.iMapX + (this.target.iFootWidth >> 1);
                int spriteX = this.iMapX + (this.iFootWidth >> 1);
                int disX = Math.abs(spriteX - targetX);
                if (disX > this.enemyData.sKen) {
                    act((byte) 0);
                    return;
                }
                if (this.iSortID == 235 || this.iSortID == 236) {
                    if (disX >= 80) {
                        this.iBoss2SpecialSkillCounter++;
                    } else {
                        this.iBoss2SpecialSkillCounter = 0;
                    }
                }
                if (GameCanvas.instance.getScriptState() == 0 && !this.isLocked && this.bActState == 0 && !this.isBossCriticalKill) {
                    if ((this.iSortID == 235 || this.iSortID == 236) && this.iBoss2SpecialSkillCounter >= 6) {
                        this.iBoss2SpecialSkillCounter = 0;
                        act((byte) 25);
                    }
                    int targetY = this.target.iMapY + this.target.iFootHeight;
                    int spriteY = this.iMapY + this.iFootHeight;
                    int disY = Math.abs(spriteY - targetY);
                    this.iAttackModeID = -1;
                    if (this.isBossCanUSeCriticalSkill && disX <= 64 && disY <= 5) {
                        if (this.iSortID == 238 || this.iSortID == 239) {
                            if (this.iHP < (this.iMaxHP * 50) / 100) {
                                faceTo((FightSprite) this.target);
                                act((byte) 21);
                                return;
                            }
                        } else if (this.iSortID == 235) {
                            if (this.iHP < (this.iMaxHP * 50) / 100) {
                                faceTo((FightSprite) this.target);
                                act((byte) 21);
                                return;
                            }
                        } else if (this.iSortID == 236) {
                            if (this.iHP < (this.iMaxHP * 70) / 100) {
                                faceTo((FightSprite) this.target);
                                act((byte) 21);
                                return;
                            }
                        } else if (this.iSortID == 241) {
                            if (this.iHP < (this.iMaxHP * 20) / 100) {
                                faceTo((FightSprite) this.target);
                                act((byte) 21);
                                return;
                            }
                        } else if (this.iSortID == 242) {
                            if (this.iHP < (this.iMaxHP * 50) / 100) {
                                faceTo((FightSprite) this.target);
                                act((byte) 21);
                                return;
                            }
                        } else if (this.iSortID == 244) {
                            faceTo((FightSprite) this.target);
                            act((byte) 21);
                            return;
                        } else if (this.iSortID == 245) {
                            faceTo((FightSprite) this.target);
                            act((byte) 21);
                            return;
                        }
                    }
                    for (int i = 0; i < this.enemyData.sAttackRange.length; i++) {
                        if (this.isCanAttack[i] && disX <= this.enemyData.sAttackRange[i][0] && (targetY == spriteY || ((targetY < spriteY && disY <= this.enemyData.sAttackRange[i][1]) || (targetY > spriteY && disY <= this.enemyData.sAttackRange[i][2])))) {
                            int rnd = Tool.getNextRnd(0, 100);
                            if (rnd < this.enemyData.bAttackOdds[i] && isCanUseSkill((byte) (i + 17))) {
                                this.isCanAttack[i] = false;
                                this.iAttackIntervalCounter[i] = 0;
                                setAttackPoint(i);
                                if (spriteX < targetX) {
                                    setDir((byte) 1);
                                } else {
                                    setDir((byte) 0);
                                }
                                act((byte) (i + 17));
                                return;
                            }
                        }
                    }
                    processMove(targetX, targetY);
                }
                processAttack();
            }
            if (GameCanvas.instance.getScriptState() == 0 || this.bActState != 0) {
                GameCanvas.instance.moveSpriteByStep(this, 10, 5);
            }
        }
    }

    public void processMove(int targetX, int targetY) {
        int selfPosX = this.iMapX + (this.iFootWidth >> 1);
        int selfPosY = this.iMapY + this.iFootHeight;
        if (selfPosX > this.enemyData.sAttackRange[0][0] + targetX) {
            this.iDesPosX = this.enemyData.sAttackRange[0][0] + targetX;
        } else if (selfPosX < targetX - this.enemyData.sAttackRange[0][0]) {
            this.iDesPosX = targetX - this.enemyData.sAttackRange[0][0];
        } else {
            this.iDesPosX = selfPosX;
        }
        this.iDesPosY = targetY;
        int Vx = Math.abs((int) this.actionSet.actionDatas[1].iVx[0]);
        int Vy = (Vx << 1) / 3;
        int temp = Tool.getNextRnd(0, 2);
        if (this.bType == 10) {
            temp = 1;
        }
        if (temp == 0) {
            if (Math.abs(selfPosX - this.iDesPosX) > Vx) {
                if (selfPosX > this.iDesPosX) {
                    setDir((byte) 0);
                } else {
                    setDir((byte) 1);
                }
                act((byte) 1);
                return;
            }
            if (Math.abs(selfPosY - this.iDesPosY) > Vy) {
                if (selfPosY > this.iDesPosY) {
                    setDir((byte) 2);
                } else {
                    setDir((byte) 3);
                }
                act((byte) 1);
                return;
            }
            if (selfPosX > targetX) {
                setDir((byte) 0);
            } else {
                setDir((byte) 1);
            }
            act((byte) 0);
            return;
        }
        if (Math.abs(selfPosY - this.iDesPosY) > Vy) {
            if (selfPosY > this.iDesPosY) {
                setDir((byte) 2);
            } else {
                setDir((byte) 3);
            }
            act((byte) 1);
            return;
        }
        if (Math.abs(selfPosX - this.iDesPosX) > Vx) {
            if (selfPosX > this.iDesPosX) {
                setDir((byte) 0);
            } else {
                setDir((byte) 1);
            }
            act((byte) 1);
            return;
        }
        if (selfPosX > targetX) {
            setDir((byte) 0);
        } else {
            setDir((byte) 1);
        }
        act((byte) 0);
    }

    void processAttack() {
        if (this.bActID != 21) {
            if ((this.iSortID == 235 || this.iSortID == 236) && this.bActID == 24 && this.iCurFrameIndex == 33) {
                if (!this.isBoss2CriticalKillHit) {
                    GameCanvas.instance.script_setSpriteFace(this.ID, this.isFaceToTheRight ? 9 : 8, 1);
                } else {
                    GameCanvas.instance.script_setSpriteFace(this.ID, this.isFaceToTheRight ? 10 : 11, 1);
                }
            }
            if (isCanAttack(this.target) && isHit(this.target)) {
                boolean isCanDodge = true;
                if (this.bActID == 24) {
                    isCanDodge = false;
                    if (this.iSortID == 232 || this.iSortID == 233) {
                        int reduceHP = this.target.iFinalHP >> 1;
                        this.target.loseHP(reduceHP, true);
                        GameCanvas.instance.setReducedHP(reduceHP);
                        if (GameCanvas.turnOn && this.target.iHP < this.target.iFinalHP / 10) {
                            this.target.iHP = this.target.iFinalHP / 10;
                        }
                        if (!this.target.isDead()) {
                            this.target.act((byte) 5);
                            return;
                        }
                        return;
                    }
                    if (this.iSortID == 235 || this.iSortID == 236) {
                        this.isBoss2CriticalKillHit = true;
                        if (this.iCurFrameIndex == 26) {
                            GameCanvas.instance.setCriticalEffect(5);
                        }
                    }
                    if (this.iSortID == 238 || this.iSortID == 239) {
                        if (this.target.isIntegrate) {
                            this.target.isIntegrate = false;
                            this.target.famulus.iCurDurability = 0;
                        } else {
                            GameCanvas.instance.destroySword();
                        }
                    }
                }
                if (this.target.isDodge(this) && isCanDodge) {
                    this.target.setSpecialStateDuration(0);
                    return;
                }
                int damagePoint = getDamagePoint(this.target);
                if (this.bActID == 24 && (this.iSortID == 235 || this.iSortID == 236)) {
                    damagePoint = (damagePoint * 20) / 100;
                    if (damagePoint < 1) {
                        damagePoint = 1;
                    }
                    GameCanvas.instance.setReducedHP(damagePoint);
                    int iRecoverPoint = (this.iMaxHP * damagePoint) / this.target.iFinalHP;
                    if (iRecoverPoint < 1) {
                        iRecoverPoint = 1;
                    }
                    addHP(iRecoverPoint);
                    setRaisingNumber(iRecoverPoint, 2);
                }
                short s = 100;
                if (this.iAttackModeID > -1) {
                    s = this.enemyData.sDestroyedHoldPoint[this.iAttackModeID];
                }
                if (this.target.sHoldPoint > s) {
                    this.target.sHoldPoint -= s;
                } else {
                    this.target.sHoldPoint = 0;
                }
                if ((this.iSortID == 243 || this.iSortID == 244 || this.iSortID == 245) && this.bActID == 17) {
                    if (this.target.isIntegrate) {
                        if (this.target.famulus.iCurDurability > (this.target.famulus.iMaxDurability * 30) / 100) {
                            this.target.famulus.iCurDurability -= (this.target.famulus.iMaxDurability * 30) / 100;
                        } else {
                            this.target.famulus.iCurDurability = 0;
                        }
                    } else if (this.target.equip.iCurDurability > (this.target.equip.iFinalDurability * 30) / 100) {
                        this.target.equip.iCurDurability -= (this.target.equip.iFinalDurability * 30) / 100;
                    } else {
                        GameCanvas.instance.destroySword();
                    }
                }
                if (isCritical()) {
                    this.target.loseHP(damagePoint << 1, true);
                } else {
                    this.target.loseHP(damagePoint, false);
                }
                this.target.faceTo((FightSprite) this);
                if (!this.target.isDead() && (!isCanDodge || (this.target.isCanStopAct() && this.target.iImmuneInjuryDuration == 0))) {
                    switch (this.bAttackType) {
                        case 0:
                            this.target.act((byte) 2);
                            this.target.comboOver();
                            break;
                        case 1:
                            this.target.act((byte) 5);
                            this.target.comboOver();
                            break;
                        case 2:
                            this.target.act((byte) 6);
                            this.target.comboOver();
                            break;
                    }
                }
                this.target.showHurt();
            }
        }
    }

    public int getDamagePoint(Role role) {
        int levelCoffecient = ((this.sLevel - role.sLevel) * 5) + 100;
        if (levelCoffecient < 50) {
            levelCoffecient = 50;
        } else if (levelCoffecient > 150) {
            levelCoffecient = 150;
        }
        int damagePoint = ((this.iAP - role.iFinalDP) * levelCoffecient) / 100;
        if (this.iAttackModeID > -1) {
            damagePoint = (this.enemyData.sDamageCoefficient[this.iAttackModeID] * damagePoint) / 100;
        } else if (this.bActID == 24 && (this.iSortID == 244 || this.iSortID == 245)) {
            damagePoint = (damagePoint * 60) / 100;
        }
        int damagePoint2 = damagePoint + Tool.getNextRnd(((-damagePoint) * 10) / 100, (damagePoint * 10) / 100);
        if (damagePoint2 < 1) {
            return 1;
        }
        return damagePoint2;
    }

    public void addHP(int value) {
        this.iHP += value;
        if (this.iHP > this.iMaxHP) {
            this.iHP = this.iMaxHP;
        } else if (this.iHP < 0) {
            this.iHP = 0;
        }
    }

    public void die() {
        if (this.bType == 10) {
            this.isDead = true;
            if (this.overScript != null) {
                GameCanvas.instance.script_call(this.overScript, false);
                return;
            }
            return;
        }
        if (this.bRefreshNum > 0 || GameCanvas.instance.isExpRoom) {
            this.iHP = 0;
            this.isInvisible = true;
            this.isLocked = false;
            this.iMapX = this.iBornMapX;
            this.iMapY = this.iBornMapY;
            for (int i = 0; i < this.enemyData.sAttackInterval.length; i++) {
                this.iAttackIntervalCounter[i] = 0;
                this.isCanAttack[i] = false;
            }
            clear();
            initAct();
            return;
        }
        this.isDead = true;
    }

    public void refresh() {
        if (isDead() && this.isInvisible) {
            if (this.bRefreshNum > 0 || GameCanvas.instance.isExpRoom) {
                if (this.sRefreshCounter < (GameCanvas.instance.isExpRoom ? (short) 6 : (short) 60)) {
                    this.sRefreshCounter = (short) (this.sRefreshCounter + 1);
                    return;
                }
                this.sRefreshCounter = (short) 0;
                if (this.bRefreshNum > 0) {
                    this.bRefreshNum = (byte) (this.bRefreshNum - 1);
                }
                this.iHP = this.iMaxHP;
                this.isInvisible = false;
            }
        }
    }

    @Override // com.dobetter.client.FightSprite, com.dobetter.client.Sprite
    public void cycle() {
        super.cycle();
        refresh();
        processAI();
        cycleAction();
        if (!isDead()) {
            processBoss();
            if (this.bActState == 0) {
                for (int i = 0; i < this.enemyData.sAttackInterval.length; i++) {
                    if (!this.isCanAttack[i]) {
                        if (this.iAttackIntervalCounter[i] < this.enemyData.sAttackInterval[i]) {
                            int[] iArr = this.iAttackIntervalCounter;
                            iArr[i] = iArr[i] + 1;
                        } else {
                            this.isCanAttack[i] = true;
                        }
                    }
                }
            }
        }
    }

    private void processBoss() {
        int rnd;
        byte defenseDuration;
        int skillInterval;
        if (this.bType == 10 && !this.target.isCriticalKill) {
            switch (this.iSortID) {
                case 232:
                    rnd = 20;
                    defenseDuration = 10;
                    skillInterval = 3600;
                    break;
                case 233:
                    rnd = 40;
                    defenseDuration = 5;
                    skillInterval = 1800;
                    break;
                case 234:
                case 237:
                case 240:
                case 243:
                default:
                    return;
                case 235:
                    rnd = 20;
                    defenseDuration = 10;
                    skillInterval = 1800;
                    break;
                case 236:
                    rnd = 40;
                    defenseDuration = 5;
                    skillInterval = 1080;
                    break;
                case 238:
                    rnd = 20;
                    defenseDuration = 10;
                    skillInterval = 1440;
                    break;
                case 239:
                    rnd = 40;
                    defenseDuration = 5;
                    skillInterval = 720;
                    break;
                case 241:
                    rnd = 20;
                    defenseDuration = 10;
                    skillInterval = 3600;
                    break;
                case 242:
                    rnd = 40;
                    defenseDuration = 5;
                    skillInterval = 1800;
                    break;
                case 244:
                    rnd = 20;
                    defenseDuration = 10;
                    skillInterval = 1080;
                    break;
                case 245:
                    rnd = 40;
                    defenseDuration = 5;
                    skillInterval = 720;
                    break;
            }
            if (this.isCanDefecse && this.sHoldPoint < 100) {
                this.isCanDefecse = false;
                if (Tool.getNextRnd(0, 100) < rnd) {
                    act((byte) 14);
                    this.bDefenseCounter = defenseDuration;
                }
            }
            if (!this.isBossCanUSeCriticalSkill) {
                this.iBossCriticalSkillCounter++;
                if (this.iBossCriticalSkillCounter == skillInterval) {
                    this.isBossCanUSeCriticalSkill = true;
                    this.iBossCriticalSkillCounter = 0;
                }
            }
        }
    }

    @Override // com.dobetter.client.FightSprite, com.dobetter.client.Sprite
    public void draw(Graphics g, int viewMapX, int viewMapY, int dx, int dy) {
        int drawX = (this.iMapX - viewMapX) + dx;
        int drawY = (this.iMapY - viewMapY) + dy;
        if (this.actionSet != null) {
            try {
                this.actionSet.drawFrame(g, this.curFrame, drawX, drawY, (!this.isFaceToTheRight || this.bActID == 12 || this.bActID == 13) ? false : true);
            } catch (Exception e) {
                e.printStackTrace();
                System.out.println("ID=" + this.iSortID);
            }
            drawFace(g, drawX, drawY);
            if ((this.bType == 9 || this.bType == 11) && this.bActState == 1) {
                int hpBarX = drawX + ((this.iFootWidth - 20) / 2);
                int hpBarY = (this.curFrame.edgeRect.y + drawY) - 8;
                g.setClip(hpBarX, hpBarY, 21, 5);
                g.setColor(4013368);
                g.fillRect(hpBarX + 1, hpBarY + 1, 19, 3);
                g.setColor(16776960);
                g.fillRect(hpBarX + 1, hpBarY + 1, (this.iHP * 19) / this.iMaxHP, 3);
                g.setColor(15976556);
                g.drawLine(hpBarX + 1, hpBarY + 1, ((this.iHP * 19) / this.iMaxHP) + hpBarX, hpBarY + 1);
                g.drawLine(hpBarX + 1, (hpBarY + 4) - 1, ((this.iHP * 19) / this.iMaxHP) + hpBarX, (hpBarY + 4) - 1);
                g.setColor(15960172);
                g.drawLine(hpBarX + 1, hpBarY, (hpBarX + 20) - 1, hpBarY);
                g.drawLine(hpBarX + 1, hpBarY + 4, (hpBarX + 20) - 1, hpBarY + 4);
                g.drawLine(hpBarX, hpBarY + 1, hpBarX, (hpBarY + 4) - 1);
                g.drawLine(hpBarX + 20, hpBarY + 1, hpBarX + 20, (hpBarY + 4) - 1);
                g.restore();
            }
            for (int i = 0; i < 2; i++) {
                if (this.showTimes[i] > 0) {
                    int bx = ((this.iFootWidth >> 1) + drawX) - (GameCanvas.missImg.getWidth() >> 1);
                    int by = (((drawY - this.curFrame.edgeRect.height) - GameCanvas.missImg.getHeight()) - 10) + this.showTimes[i];
                    Tool.drawTile(g, GameCanvas.missImg, i, GameCanvas.missImg.getWidth(), GameCanvas.missImg.getHeight(), 0, bx, by);
                }
            }
            drawRaisingNumber(g, drawX, drawY);
            drawLogicRect(g, drawX, drawY);
        }
    }

    public void drawSpecialEffectBack(Graphics g, int viewX, int viewY, int dX, int dY) {
        if (this.bType == 11 && !GameCanvas.instance.role.isCriticalKill) {
            int drawX = (this.iMapX - viewX) + dX + ((this.iFootWidth - this.specialAS.footWidth) >> 1);
            int drawY = (this.iMapY - viewY) + dY + ((this.iFootHeight - this.specialAS.footHeight) >> 1);
            this.specialAS.drawFrameCycle(g, 0, Tool.countTimes, drawX, drawY);
        }
    }

    public boolean isBoss() {
        return this.bType == 10;
    }

    @Override // com.dobetter.client.FightSprite
    public void showHurt() {
        int dir;
        int bActionID;
        if (0 < 2) {
            dir = this.bCurDir;
        } else {
            dir = this.bPreDir;
        }
        switch (this.bActID) {
            case 5:
            case 6:
                bActionID = dir + 2;
                break;
            case 7:
                bActionID = dir + 4;
                break;
            case 8:
                if (this.iCurFrameIndex < 5) {
                    bActionID = dir + 8;
                } else {
                    bActionID = dir + 10;
                }
                break;
            case Interpreter.ERR_ARR_OUT /* 9 */:
            default:
                bActionID = dir;
                break;
            case 10:
                bActionID = dir + 6;
                break;
            case 11:
                bActionID = dir + 12;
                break;
        }
        EffectSprite be = new EffectSprite(GameCanvas.beatedEffectAS, bActionID);
        be.iMapX = this.iMapX + ((this.iFootWidth - be.iFootWidth) >> 1) + Tool.getNextRnd(-4, 5);
        be.iMapY = this.iMapY + ((this.iFootHeight - be.iFootHeight) >> 1) + Tool.getNextRnd(-4, 5);
        if (GameCanvas.instance.role.isCriticalKill) {
            GameCanvas.instance.addFightElement(be);
        } else {
            GameCanvas.instance.addMapElement(be);
        }
    }

    @Override // com.dobetter.client.FightSprite, com.dobetter.client.Sprite
    public void freeSprite() {
        super.freeSprite();
        if (this.specialAS != null) {
            this.specialAS.releasImg();
            this.specialAS = null;
        }
    }
}
