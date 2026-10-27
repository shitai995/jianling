package com.dobetter.client;

import com.dobetter.action.ActionSet;
import com.dobetter.common.Graphics;
import com.dobetter.script.Interpreter;

/* JADX INFO: loaded from: classes.dex */
public class Bullet extends FightSprite {
    public static final byte DIR1 = 1;
    public static final byte DIR10 = 10;
    public static final byte DIR11 = 11;
    public static final byte DIR12 = 12;
    public static final byte DIR2 = 2;
    public static final byte DIR3 = 3;
    public static final byte DIR4 = 4;
    public static final byte DIR5 = 5;
    public static final byte DIR6 = 6;
    public static final byte DIR7 = 7;
    public static final byte DIR8 = 8;
    public static final byte DIR9 = 9;
    public static final byte DIR_DL = 21;
    public static final byte DIR_DR = 22;
    public static final byte DIR_LD = 32;
    public static final byte DIR_LU = 31;
    public static final byte DIR_RD = 42;
    public static final byte DIR_RU = 41;
    public static final byte DIR_UL = 11;
    public static final byte DIR_UR = 12;
    public static final byte TYPE_LINE = 0;
    public static final byte TYPE_TRACK = 1;
    public byte bBulletType;
    public byte bChangeDirCount;
    public byte bChangeDirFrequency;
    public byte bEndDir;
    public byte bFrequency;
    public byte bFrequencyCount;
    public byte bNowDir;
    public byte bSpeed;
    public byte bVx;
    public byte bVy;
    public int iDamagePoint;
    public int iDestroyedHoldPoint;
    int iLoseHPCounter;
    int iLoseHPPerHit;
    public boolean isActionOverDisappear;
    public boolean isCanDodge;
    public boolean isUnDisappearAfterHit;
    FightSprite owner;
    FightSprite target;

    public Bullet(FightSprite fs, int mx, int my, byte dir, byte speed, byte bFrequency, byte bChangeDirFrequency, ActionSet as, int actionID, FightSprite target) {
        this(mx, my);
        this.owner = fs;
        this.bBulletType = (byte) 1;
        this.iLayerIndex = 2;
        this.bNowDir = dir;
        this.bFrequency = bFrequency;
        this.bChangeDirFrequency = bChangeDirFrequency;
        this.bSpeed = speed;
        this.target = target;
        this.actionSet = as;
        this.iFootWidth = this.actionSet.footWidth;
        this.iFootHeight = this.actionSet.footHeight;
        this.iCurActionID = actionID;
        getCurData();
        changeXYMove();
    }

    public Bullet(FightSprite fs, int mx, int my, byte dir, byte speed, ActionSet as, int actionID) {
        this(mx, my);
        this.owner = fs;
        this.bBulletType = (byte) 0;
        this.iLayerIndex = 2;
        this.bCurDir = dir;
        switch (this.bCurDir) {
            case 0:
                this.iCurVx = -speed;
                break;
            case 1:
                this.iCurVx = speed;
                break;
            case 2:
                this.iCurVy = -speed;
                break;
            case 3:
                this.iCurVy = speed;
                break;
            case 11:
                this.iCurVy = -speed;
                this.iCurVx = (-speed) >> 1;
                break;
            case 12:
                this.iCurVy = -speed;
                this.iCurVx = speed >> 1;
                break;
            case 21:
                this.iCurVy = speed;
                this.iCurVx = (-speed) >> 1;
                break;
            case 22:
                this.iCurVy = speed;
                this.iCurVx = speed >> 1;
                break;
            case 31:
                this.iCurVy = (-speed) >> 1;
                this.iCurVx = -speed;
                break;
            case Graphics.BOTTOM /* 32 */:
                this.iCurVy = speed >> 1;
                this.iCurVx = -speed;
                break;
            case 41:
                this.iCurVy = (-speed) >> 1;
                this.iCurVx = speed;
                break;
            case 42:
                this.iCurVy = speed >> 1;
                this.iCurVx = speed;
                break;
        }
        this.actionSet = as;
        this.iFootWidth = this.actionSet.footWidth;
        this.iFootHeight = this.actionSet.footHeight;
        this.iCurActionID = actionID;
        getCurData();
    }

    void setDamagePoint(int damagePoint, int destroyHoldPoint) {
        this.iDamagePoint = damagePoint;
        this.iDestroyedHoldPoint = destroyHoldPoint;
    }

    void setAttribute(boolean isUnDisappear, boolean isActionOverDisappear, boolean isCanDodge) {
        this.isUnDisappearAfterHit = isUnDisappear;
        this.isActionOverDisappear = isActionOverDisappear;
        this.isCanDodge = isCanDodge;
    }

    private Bullet(int mx, int my) {
        this.iMapX = mx;
        this.iMapY = my;
        this.bType = (byte) 15;
    }

    public void bulletAttack(FightSprite spr) {
        if (this.isCanDodge && spr.isDodge(this.owner)) {
            spr.setSpecialStateDuration(0);
            return;
        }
        if (isCritical()) {
            this.iDamagePoint <<= 1;
            spr.loseHP(this.iDamagePoint, true);
        } else {
            spr.loseHP(this.iDamagePoint, false);
        }
        if (spr.sHoldPoint > this.iDestroyedHoldPoint) {
            spr.sHoldPoint -= this.iDestroyedHoldPoint;
        } else {
            spr.sHoldPoint = 0;
        }
        if (spr.bType == 0) {
            if (!spr.isDead() && (!this.isCanDodge || (spr.isCanStopAct() && ((Role) spr).iImmuneInjuryDuration == 0))) {
                switch (this.bAttackType) {
                    case 0:
                        spr.act((byte) 2);
                        ((Role) spr).comboOver();
                        break;
                    case 1:
                        spr.act((byte) 5);
                        ((Role) spr).comboOver();
                        break;
                    case 2:
                        spr.act((byte) 6);
                        ((Role) spr).comboOver();
                        break;
                }
            }
            spr.showHurt();
        }
    }

    public static byte getFaceTo(int dir) {
        int dir2;
        switch (dir) {
            case 1:
            case 11:
            case 12:
                dir2 = 0;
                break;
            case 2:
            case 3:
            case 4:
                dir2 = 3;
                break;
            case 5:
            case 6:
            case 7:
                dir2 = 1;
                break;
            case 8:
            case Interpreter.ERR_ARR_OUT /* 9 */:
            case 10:
                dir2 = 2;
                break;
            default:
                dir2 = 0;
                break;
        }
        return dir2 % 2 == 0 ? (byte) (dir2 + 1) : (byte) (dir2 - 1);
    }

    public static byte getCurDir(int dir) {
        int dir2;
        switch (dir) {
            case 1:
            case 11:
            case 12:
                dir2 = 0;
                break;
            case 2:
            case 3:
            case 4:
                dir2 = 3;
                break;
            case 5:
            case 6:
            case 7:
                dir2 = 1;
                break;
            case 8:
            case Interpreter.ERR_ARR_OUT /* 9 */:
            case 10:
                dir2 = 2;
                break;
            default:
                dir2 = 0;
                break;
        }
        return (byte) dir2;
    }

    private void setEndDir(byte bDir) {
        this.bEndDir = bDir;
    }

    public void changeDir(byte bDir) {
        this.bNowDir = (byte) (this.bNowDir + bDir);
        if (this.bNowDir > 12) {
            this.bNowDir = (byte) 1;
        } else if (this.bNowDir < 1) {
            this.bNowDir = (byte) 12;
        }
        changeXYMove();
    }

    private byte getToDir() {
        if (this.bNowDir == this.bEndDir) {
            return (byte) 0;
        }
        if (this.bNowDir > this.bEndDir) {
            return this.bNowDir - this.bEndDir <= 6 ? (byte) -1 : (byte) 1;
        }
        return this.bEndDir - this.bNowDir <= 6 ? (byte) 1 : (byte) -1;
    }

    private void changeXYMove() {
        this.bVx = (byte) ((this.bSpeed / 3) * returnXMove(this.bNowDir));
        this.bVy = (byte) ((this.bSpeed / 3) * returnYMove(this.bNowDir));
    }

    public static byte returnXMove(byte dir) {
        switch (dir) {
            case 1:
            case 5:
                return (byte) 1;
            case 2:
            case 4:
                return (byte) 2;
            case 3:
                return (byte) 3;
            case 6:
            case 12:
                return (byte) 0;
            case 7:
            case 11:
                return (byte) -1;
            case 8:
            case 10:
                return (byte) -2;
            case Interpreter.ERR_ARR_OUT /* 9 */:
                return (byte) -3;
            default:
                return (byte) 0;
        }
    }

    public static byte returnYMove(byte dir) {
        switch (dir) {
            case 1:
            case 11:
                return (byte) -2;
            case 2:
            case 10:
                return (byte) -1;
            case 3:
            case Interpreter.ERR_ARR_OUT /* 9 */:
                return (byte) 0;
            case 4:
            case 8:
                return (byte) 1;
            case 5:
            case 7:
                return (byte) 2;
            case 6:
                return (byte) 3;
            case 12:
                return (byte) -3;
            default:
                return (byte) 0;
        }
    }

    public static byte getRelativeDir(int x, int y, Sprite me) {
        int rltX = me.iMapX - x;
        int rltY = me.iMapY - y;
        if (rltX == 0) {
            if (rltY > 0) {
                return (byte) 6;
            }
            return (byte) 12;
        }
        if (rltY == 0) {
            if (rltX > 0) {
                return (byte) 3;
            }
            return (byte) 9;
        }
        if (rltX < 0) {
            if (rltY > 0) {
                if (rltX + rltY >= 0) {
                    return (byte) 7;
                }
                return (byte) 8;
            }
            if (rltX - rltY >= 0) {
                return (byte) 11;
            }
            return (byte) 10;
        }
        if (rltY > 0) {
            if (rltX - rltY > 0) {
                return (byte) 4;
            }
            return (byte) 5;
        }
        if (rltX + rltY > 0) {
            return (byte) 2;
        }
        return (byte) 1;
    }

    public boolean isContinueMoveAfterHitTarget() {
        if (this.owner instanceof Enemy) {
            Enemy e = (Enemy) this.owner;
            if (e.isBossCriticalKill && (e.iSortID == 241 || e.iSortID == 242)) {
                if (this.iCurActionID == 24) {
                    if (isCanAttack(e.target) && isHit(e.target)) {
                        this.iCurActionID = 25;
                        this.iCurFrameIndex = 0;
                        getCurData();
                        GameCanvas.instance.setScreenStay(true);
                        GameCanvas.instance.setScreenMove(((this.iMapX - GameCanvas.instance.iViewMapX) + (this.iFootWidth >> 1)) - (GameCanvas.SCREEN_WIDTH >> 1), 0, 10, true);
                        e.target.act((byte) 6);
                        this.iLoseHPPerHit = e.target.iHP / 11;
                        this.iLoseHPCounter = 0;
                        e.target.iMapX = this.iMapX + ((this.iFootWidth - e.target.iFootWidth) >> 1);
                        e.target.iMapY = this.iMapY + ((this.iFootHeight - e.target.iFootHeight) >> 1);
                        e.isInvisible = true;
                        return false;
                    }
                } else if (this.iCurActionID == 25 && isCanAttack(e.target)) {
                    GameCanvas.instance.setBGColor(6710886);
                    if (this.iLoseHPCounter < 10) {
                        this.iLoseHPCounter++;
                        e.target.loseHP(this.iLoseHPPerHit, false);
                        e.target.act((byte) 6);
                        GameCanvas.instance.setReducedHP(this.iLoseHPPerHit);
                    } else {
                        GameCanvas.instance.setReducedHP(e.target.iHP);
                        e.target.loseHP(e.target.iHP, e.target.iHP > this.iLoseHPPerHit);
                        GameCanvas.instance.setScreenStay(false);
                        e.isInvisible = false;
                    }
                }
            } else if (isCanAttack(e.target) && isHit(e.target)) {
                bulletAttack(e.target);
                if (!this.isUnDisappearAfterHit) {
                    this.isDead = true;
                    return false;
                }
            }
        } else {
            for (int i = 0; i < GameCanvas.instance.enemyInScreen.size(); i++) {
                Enemy e2 = (Enemy) GameCanvas.instance.enemyInScreen.elementAt(i);
                if (isCanAttack(e2) && isHit(e2)) {
                    bulletAttack(e2);
                    if (!this.isUnDisappearAfterHit) {
                        this.isDead = true;
                        return false;
                    }
                }
            }
        }
        return true;
    }

    @Override // com.dobetter.client.FightSprite, com.dobetter.client.Sprite
    public void cycle() {
        if (!this.isDead && isContinueMoveAfterHitTarget()) {
            switch (this.bBulletType) {
                case 1:
                    if (this.target.isDead || this.target.isInvisible || this.owner.isDead()) {
                        this.isDead = true;
                        return;
                    }
                    this.bFrequencyCount = (byte) (this.bFrequencyCount + 1);
                    if (this.bFrequencyCount > this.bFrequency) {
                        this.bFrequencyCount = (byte) 0;
                        setEndDir(getRelativeDir(this.iMapX, this.iMapY, this.target));
                    }
                    this.bChangeDirCount = (byte) (this.bChangeDirCount + 1);
                    if (this.bChangeDirCount > this.bChangeDirFrequency) {
                        this.bChangeDirCount = (byte) 0;
                        changeDir(getToDir());
                    }
                    break;
                    break;
            }
            GameCanvas.instance.moveSpriteByStep(this, 20, 10);
            cycleAction();
        }
    }

    public void cycleAction() {
        this.iCurFrameIndex++;
        if (GameCanvas.instance.getBGColor() == 6710886) {
            GameCanvas.instance.setBGColor(3355443);
        } else if (GameCanvas.instance.getBGColor() == 3355443) {
            GameCanvas.instance.setBGColor(0);
        } else {
            Enemy e = (Enemy) this.owner;
            if (e.isBossCriticalKill && ((e.iSortID == 241 || e.iSortID == 242) && this.iLoseHPCounter == 10)) {
                GameCanvas.instance.setBGColor(6710886);
            }
        }
        if (this.iCurFrameIndex >= this.curAction.frameNum) {
            this.iCurFrameIndex = 0;
            if (this.isActionOverDisappear) {
                this.isDead = true;
                Enemy e2 = (Enemy) this.owner;
                if (e2.isBossCriticalKill && (e2.iSortID == 241 || e2.iSortID == 242)) {
                    e2.isBossCriticalKill = false;
                    e2.isBossCanUSeCriticalSkill = false;
                }
            }
        }
        getCurData();
    }

    @Override // com.dobetter.client.FightSprite
    public void getCurData() {
        this.curAction = this.actionSet.actionDatas[this.iCurActionID];
        this.curFrame = this.actionSet.frameDatas[this.curAction.frameID[this.iCurFrameIndex]];
        this.isCurAttack = this.curAction.isAttack[this.iCurFrameIndex];
        this.isCurBeAttack = this.curAction.isBeAttack[this.iCurFrameIndex];
        this.bAttackType = this.curAction.bAttackType[this.iCurFrameIndex];
    }

    @Override // com.dobetter.client.FightSprite, com.dobetter.client.Sprite
    public void draw(Graphics g, int viewMapX, int viewMapY, int dx, int dy) {
        if (!this.isDead) {
            int drawX = (this.iMapX - viewMapX) + dx;
            int drawY = (this.iMapY - viewMapY) + dy;
            this.actionSet.drawFrame(g, this.curFrame, drawX, drawY);
        }
    }

    @Override // com.dobetter.client.FightSprite, com.dobetter.client.Sprite
    public void freeSprite() {
        if (this.actionSet != null) {
            this.actionSet.releasImg();
            this.actionSet = null;
        }
    }
}
