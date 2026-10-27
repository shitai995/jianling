package com.dobetter.client;

import android.graphics.Bitmap;
import com.dobetter.action.ActionSet;
import com.dobetter.client.data.AttributeData;
import com.dobetter.common.Graphics;
import com.dobetter.common.Tool;
import com.tencent.webnet.WebNetEvent;

/* JADX INFO: loaded from: classes.dex */
public class FightSprite extends Sprite {
    public static final byte ACT_DIE = 3;
    public static final byte ACT_HURTED = 2;
    public static final int SCREEN_CHECK_X = -5;
    public static final int SCREEN_CHECK_Y = -30;
    public static final int SHOW_NUMBER_DURATION = 10;
    public static final int SHOW_SPECIAL_STATE_TIMES = 10;
    public static final int TALK_DX = 20;
    public static final int TALK_DY = 10;
    public AttributeData attributeData;
    public byte bAttackType;
    int iAP;
    int iCriticalPoint;
    int iCurrentExp;
    int iDP;
    int iDodgePoint;
    int iHP;
    int iHitPoint;
    int iLevelUpExp;
    int iMaxHP;
    boolean isCheckCollision;
    public boolean isCurAttack;
    public boolean isCurBeAttack;
    boolean isLockTarget;
    int sDamageCoefficient;
    int sHoldPoint;
    short sLevel;
    public int wudiTimes;
    public int[] iNumberShowedCounter = new int[5];
    public int[] iNumericalValue = new int[5];
    public byte[] bNumberType = new byte[5];
    public int[] showTimes = new int[2];

    public void wudi(int time) {
        this.wudiTimes = time;
    }

    public FightSprite() {
        this.isCheckMapBlock = false;
        this.isCheckCollision = false;
    }

    @Override // com.dobetter.client.Sprite
    public void initAct() {
        this.iCurFrameIndex = 0;
        this.bActID = (byte) 0;
        this.bActState = (byte) 0;
        getCurActionData();
    }

    @Override // com.dobetter.client.Sprite
    public void act(byte act) {
    }

    @Override // com.dobetter.client.Sprite
    public void getCurActionData() {
        if (this.bCurDir < 2) {
            this.iCurActionID = (this.bActID << 1) + this.bCurDir;
        } else {
            this.iCurActionID = (this.bActID << 1) + this.bPreDir;
        }
        getCurData();
        if (this.bCurDir > 1 && this.bActID == 1) {
            this.iCurVx = 0;
        } else {
            this.iCurVx = this.curAction.iVx[this.iCurFrameIndex];
            this.iCurVy = this.curAction.iVy[this.iCurFrameIndex];
        }
    }

    public void getCurData() {
        try {
            this.curAction = this.actionSet.actionDatas[this.iCurActionID];
            this.curFrame = this.actionSet.frameDatas[this.curAction.frameID[this.iCurFrameIndex]];
            this.isCurAttack = this.curAction.isAttack[this.iCurFrameIndex];
            this.isCurBeAttack = this.curAction.isBeAttack[this.iCurFrameIndex];
            this.bAttackType = this.curAction.bAttackType[this.iCurFrameIndex];
        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    public void getCurVxVy() {
        int temp = 1;
        if (this.bType != 0 && this.bType != 14) {
            if (this.isFaceToTheRight && this.bActID != 12 && this.bActID != 13) {
                temp = -1;
            }
        } else if (this.isFaceToTheRight) {
            temp = -1;
        }
        this.iCurVx = this.curAction.iVx[this.iCurFrameIndex] * temp;
        this.iCurVy = this.curAction.iVy[this.iCurFrameIndex];
        if (this.bActID == 1) {
            if (this.bCurDir == 2) {
                this.iCurVy = (-(Math.abs(this.iCurVx) << 1)) / 3;
                this.iCurVx = 0;
            } else if (this.bCurDir == 3) {
                this.iCurVy = (Math.abs(this.iCurVx) << 1) / 3;
                this.iCurVx = 0;
            }
        }
    }

    @Override // com.dobetter.client.Sprite
    public void move() {
        this.iMapX += this.iCurVx;
        this.iMapY += this.iCurVy;
    }

    @Override // com.dobetter.client.Sprite
    public void move(int vx, int vy) {
        this.iMapX += vx;
        this.iMapY += vy;
    }

    @Override // com.dobetter.client.Sprite
    public void setDir(byte dir) {
        if (this.bCurDir < 2) {
            this.bPreDir = this.bCurDir;
        }
        this.bCurDir = dir;
        if (this.bCurDir < 2) {
            this.isFaceToTheRight = this.bCurDir == 1;
        } else {
            this.isFaceToTheRight = this.bPreDir == 1;
        }
    }

    boolean isCanAttack(FightSprite spr) {
        return spr.wudiTimes <= 0 && this.isCurAttack && spr.isCurBeAttack;
    }

    public boolean isHit(FightSprite target) {
        if (this.curFrame.atkRect != null && target.curFrame.bodyRect != null) {
            for (int i = 0; i < this.curFrame.atkRect.length; i++) {
                int rect1X = !this.isFaceToTheRight ? this.curFrame.atkRect[i].x : ((-this.curFrame.atkRect[i].x) - this.curFrame.atkRect[i].width) + this.iFootWidth;
                for (int j = 0; j < target.curFrame.bodyRect.length; j++) {
                    int rect2X = !target.isFaceToTheRight ? target.curFrame.bodyRect[j].x : ((-target.curFrame.bodyRect[j].x) - target.curFrame.bodyRect[j].width) + target.iFootWidth;
                    if (Tool.isRectIntersected(this.iMapX + rect1X, this.iMapY + this.curFrame.atkRect[i].y, this.curFrame.atkRect[i].width, this.curFrame.atkRect[i].height, target.iMapX + rect2X, target.iMapY + target.curFrame.bodyRect[j].y, target.curFrame.bodyRect[j].width, target.curFrame.bodyRect[j].height)) {
                        GameCanvas.isHited = true;
                        return true;
                    }
                }
            }
        }
        return false;
    }

    public boolean isDodge(FightSprite fs) {
        int dodgePoint = this.iDodgePoint;
        int hitPoint = fs.iHitPoint;
        if (fs.bType == 0) {
            hitPoint = ((Role) fs).iFinalHitPoint;
        }
        if (this.bType == 0) {
            dodgePoint = ((Role) this).iFinalDodgePoint;
        }
        int dodge = 100 - ((hitPoint - dodgePoint) + ((fs.sLevel - this.sLevel) * 3));
        if (dodge < 10 && this.bType == 0) {
            dodge = 10;
        } else if (dodge > 90) {
            dodge = 90;
        }
        return Tool.getNextRnd(0, 100) < dodge;
    }

    public boolean isCritical() {
        int criticalPoint = this.iCriticalPoint;
        if (this.bType == 0) {
            criticalPoint = ((Role) this).iFinalCriticalPoint;
        }
        return Tool.getNextRnd(0, WebNetEvent.GotoWeb_Event_OK) < criticalPoint;
    }

    public void loseHP(int value, boolean isCritical) {
        this.isCurBeAttack = false;
        setRaisingNumber(value, isCritical ? 1 : 0);
        if (!isDead()) {
            if (this.iHP > value) {
                this.iHP -= value;
            } else {
                this.iHP = 0;
            }
            if (this.bType == 0) {
                if (this.iHP <= ((Role) this).iFinalHP / 3) {
                    if (this.specialScript != null) {
                        if (GameCanvas.instance.role.bActState != 0 && GameCanvas.instance.role.bActState != 2) {
                            GameCanvas.instance.isWaitforSkillOver = true;
                            GameCanvas.instance.taskOverScript = this.specialScript;
                            GameCanvas.instance.isToLoading = false;
                            return;
                        }
                        GameCanvas.instance.script_call(this.specialScript, false);
                        return;
                    }
                    if (GameCanvas.turnOn) {
                        if (this.iHP < ((Role) this).iFinalHP / 10) {
                            this.iHP = ((Role) this).iFinalHP / 10;
                            return;
                        }
                        return;
                    } else {
                        if (this.iHP == 0) {
                            act((byte) 3);
                            return;
                        }
                        return;
                    }
                }
                return;
            }
            if (this.bType == 10) {
                if (this.iHP <= this.iMaxHP / 3) {
                    if (this.specialScript != null) {
                        if (GameCanvas.instance.role.bActState != 0 && GameCanvas.instance.role.bActState != 2) {
                            GameCanvas.instance.isWaitforSkillOver = true;
                            GameCanvas.instance.taskOverScript = this.specialScript;
                            GameCanvas.instance.isToLoading = false;
                            return;
                        }
                        GameCanvas.instance.script_call(this.specialScript, false);
                        return;
                    }
                    if (this.iHP == 0) {
                        GameCanvas.instance.processEnemyDie(this);
                        return;
                    }
                    return;
                }
                return;
            }
            if (this.iHP == 0) {
                GameCanvas.instance.processEnemyDie(this);
            }
        }
    }

    boolean isDead() {
        return this.iHP <= 0;
    }

    public void setSpecialStateDuration(int state) {
        this.showTimes[state] = 10;
    }

    protected void setRaisingNumber(int value, int type) {
        for (int i = this.iNumberShowedCounter.length - 2; i >= 0; i--) {
            if (this.iNumberShowedCounter[i] > 0) {
                this.iNumberShowedCounter[i + 1] = this.iNumberShowedCounter[i];
                this.iNumericalValue[i + 1] = this.iNumericalValue[i];
                this.bNumberType[i + 1] = this.bNumberType[i];
            }
        }
        this.iNumberShowedCounter[0] = 10;
        this.iNumericalValue[0] = value;
        this.bNumberType[0] = (byte) type;
    }

    void showHurt() {
    }

    @Override // com.dobetter.client.Sprite
    public void cycle() {
        if (this.iFaceDuration > 0) {
            if (this.iFaceFrameIndex < this.iFaceDuration) {
                this.iFaceFrameIndex++;
            } else {
                this.iFaceFrameIndex = 0;
                this.iFaceDuration = 0;
            }
        }
        if (this.wudiTimes > 0) {
            this.wudiTimes--;
            System.out.println("无敌时间：" + this.wudiTimes);
        }
        for (int i = 0; i < this.iNumberShowedCounter.length; i++) {
            if (this.iNumberShowedCounter[i] > 0) {
                int[] iArr = this.iNumberShowedCounter;
                iArr[i] = iArr[i] - 1;
            }
        }
        for (int i2 = 0; i2 < this.showTimes.length; i2++) {
            if (this.showTimes[i2] > 0) {
                int[] iArr2 = this.showTimes;
                iArr2[i2] = iArr2[i2] - 1;
            }
        }
    }

    @Override // com.dobetter.client.Sprite
    public void draw(Graphics g, int viewMapX, int viewMapY, int dx, int dy) {
    }

    public void drawFace(Graphics g, int drawX, int drawY) {
        if (this.iFaceDuration > 0) {
            int bx = drawX + ((this.iFootWidth - GameCanvas.faceAS.footWidth) >> 1);
            int by = ((this.curFrame.edgeRect.y + drawY) - (GameCanvas.faceAS.footHeight << 1)) - 16;
            if (this.ID == -1) {
                by += 24;
            } else if (this.ID == -2) {
                by += 16;
            }
            GameCanvas.faceAS.drawFrameCycle(g, this.iFaceIndex, this.iFaceFrameIndex, bx, by);
        }
    }

    public void drawRaisingNumber(Graphics g, int drawX, int drawY) {
        for (int i = this.iNumberShowedCounter.length - 1; i >= 0; i--) {
            if (this.iNumberShowedCounter[i] > 0 && this.iNumericalValue[i] > 0) {
                Bitmap img = GameCanvas.loseHPImg0;
                if (this.bNumberType[i] == 1) {
                    img = GameCanvas.loseHPImg1;
                } else if (this.bNumberType[i] == 2) {
                    img = GameCanvas.addHPImg;
                } else if (this.bNumberType[i] == 3) {
                    img = GameCanvas.addMPImg;
                }
                int hx = drawX + (this.iFootWidth >> 1);
                int hy = (this.curFrame.edgeRect.y + drawY) - ((10 - this.iNumberShowedCounter[i]) * 6);
                Tool.drawNumReturnWidth(g, img, this.iNumericalValue[i], hx, hy, 8, 2, -1);
            }
        }
    }

    public void drawLogicRect(Graphics g, int drawX, int drawY) {
    }

    @Override // com.dobetter.client.Sprite
    public boolean isInScreen(int viewMapX, int viewMapY, int viewWidth, int viewHeight) {
        if (this.actionSet == null) {
            return false;
        }
        int x = this.curFrame.edgeRect.x;
        if (this.isFaceToTheRight) {
            x = ((-this.curFrame.edgeRect.x) - this.curFrame.edgeRect.width) + this.iFootWidth;
        }
        return Tool.isRectIntersected(this.iMapX + x, this.iMapY + this.curFrame.edgeRect.y, this.curFrame.edgeRect.width, this.curFrame.edgeRect.height, viewMapX, viewMapY, viewWidth, viewHeight);
    }

    public boolean isInTalkRangeOf(Sprite spr) {
        int fcx = this.iMapX + (this.iFootWidth >> 1);
        int fcy = this.iMapY + (this.iFootHeight >> 1);
        int spx = spr.iMapX + (spr.iFootWidth >> 1);
        int spy = spr.iMapY + (spr.iFootHeight >> 1);
        int adx = Math.abs(spx - fcx);
        int ady = Math.abs(spy - fcy);
        return adx < ((this.iFootWidth >> 1) + 20) + (spr.iFootWidth >> 1) && ady < ((this.iFootHeight >> 1) + 10) + (spr.iFootHeight >> 1);
    }

    public void faceTo(Sprite spr) {
        int fcx = this.iMapX + (this.iFootWidth >> 1);
        int fcy = this.iMapY + (this.iFootHeight >> 1);
        int spx = spr.iMapX + (spr.iFootWidth >> 1);
        int spy = spr.iMapY + (spr.iFootHeight >> 1);
        if (spy > fcy) {
            if (spx > fcx) {
                setDir((byte) 1);
                return;
            } else {
                setDir((byte) 0);
                return;
            }
        }
        if (spx > fcx) {
            setDir((byte) 1);
        } else {
            setDir((byte) 0);
        }
    }

    public void faceTo(FightSprite spr) {
        if (spr.iMapX + (spr.iFootWidth >> 1) > this.iMapX + (this.iFootWidth >> 1)) {
            setDir((byte) 1);
            return;
        }
        if (spr.iMapX + (spr.iFootWidth >> 1) == this.iMapX + (this.iFootWidth >> 1)) {
            if (spr.isFaceToTheRight) {
                setDir((byte) 0);
                return;
            } else {
                setDir((byte) 1);
                return;
            }
        }
        setDir((byte) 0);
    }

    public boolean isCanStopAct() {
        return this.sHoldPoint <= 0;
    }

    public void clear() {
        for (int i = 0; i < this.iNumberShowedCounter.length; i++) {
            this.iNumberShowedCounter[i] = 0;
        }
        for (int i2 = 0; i2 < this.showTimes.length; i2++) {
            this.showTimes[i2] = 0;
        }
        this.isLockTarget = false;
        initAct();
    }

    @Override // com.dobetter.client.Sprite
    public void freeSprite() {
        if (this.actionSet != null) {
            this.actionSet.releasImg();
            this.actionSet = null;
        }
    }

    void getLevelUpExp() {
        this.iLevelUpExp = this.sLevel * this.sLevel * this.sLevel * 25;
    }

    public void addBullet(FightSprite spr, ActionSet as, int actionIndex, int mx, int my, byte dir, byte speed, int damagePoint, int destroyedHoldPoint, boolean isUnDisappearAfterHit, boolean isDisapperActionOver, boolean isCanDodge) {
        Bullet bt = new Bullet(spr, mx, my, dir, speed, as, actionIndex);
        bt.setDamagePoint(damagePoint, destroyedHoldPoint);
        bt.setAttribute(isUnDisappearAfterHit, isDisapperActionOver, isCanDodge);
        GameCanvas.instance.addMapElement(bt);
    }
}
