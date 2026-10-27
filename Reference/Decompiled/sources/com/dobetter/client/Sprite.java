package com.dobetter.client;

import android.graphics.Bitmap;
import com.dobetter.action.ActionData;
import com.dobetter.action.ActionSet;
import com.dobetter.action.FrameData;
import com.dobetter.client.data.SpriteBaseData;
import com.dobetter.common.Common;
import com.dobetter.common.Graphics;

/* JADX INFO: loaded from: classes.dex */
public abstract class Sprite {
    public static final byte ACT_RUN = 1;
    public static final byte ACT_STAND = 0;
    public static final byte DIR_DOWN = 3;
    public static final byte DIR_LEFT = 0;
    public static final byte DIR_RIGHT = 1;
    public static final byte DIR_UP = 2;
    public static final byte STATE_ATTACK = 2;
    public static final byte STATE_HURTED = 1;
    public static final byte STATE_NORMAL = 0;
    public static final byte STATE_OTHER = 4;
    public static final byte STATE_SKILL = 3;
    public static final byte TYPE_BOSS = 10;
    public static final byte TYPE_BULLET = 15;
    public static final byte TYPE_DROP_EQUIP = 17;
    public static final byte TYPE_DROP_ITEM = 13;
    public static final byte TYPE_ELITE = 11;
    public static final byte TYPE_ENEMY = 9;
    public static final byte TYPE_FAMULUS = 14;
    public static final byte TYPE_NPC_MAIN = 6;
    public static final byte TYPE_NPC_TALK = 7;
    public static final byte TYPE_RECORD = 18;
    public static final byte TYPE_ROLE = 0;
    public static final byte TYPE_SALER = 8;
    public static final byte TYPE_SCENE = 1;
    public static final byte TYPE_SCENE_NON_COLLISION = 2;
    public static final byte TYPE_SPELL = 16;
    public static final byte TYPE_SPRITE_FLY = 12;
    public static final byte TYPE_SPRITE_GROUND = 4;
    public static final byte TYPE_SPRITE_SKY = 3;
    public static final byte TYPE_SWITCH = 5;
    public int ID;
    public ActionSet actionSet;
    public byte bActID;
    public byte bActState;
    public byte bCurDir;
    public byte bPreDir;
    public byte bType;
    public ActionData curAction;
    public FrameData curFrame;
    public int iBornMapX;
    public int iBornMapY;
    public int iCurActionID;
    public int iCurFrameIndex;
    public int iCurVx;
    public int iCurVy;
    public int iDurationFrame;
    public int iFaceDuration;
    public int iFaceFrameIndex;
    public int iFootHeight;
    public int iFootWidth;
    public int iLayerIndex;
    public int iMapX;
    public int iMapY;
    public int iMoveDistance;
    public int iMoveSpeed;
    public int iNextFrameIndex;
    public int iSortID;
    public boolean isCheckMapBlock;
    public boolean isDead;
    public boolean isDrawInScreenCenter;
    public boolean isFaceToTheRight;
    public boolean isHasResInCache;
    public boolean isInScreen;
    public boolean isInvisible;
    public boolean isKeepFrame;
    public boolean isLocked;
    public boolean isSpriteAnimate;
    public String overScript;
    public String script;
    public String specialScript;
    public int iFaceIndex = -1;
    public int iSignIndex = -1;

    public abstract void act(byte b);

    public abstract void cycle();

    public abstract void draw(Graphics graphics, int i, int i2, int i3, int i4);

    public abstract void freeSprite();

    public abstract void getCurActionData();

    public abstract void initAct();

    public abstract boolean isInScreen(int i, int i2, int i3, int i4);

    public abstract void move();

    public abstract void move(int i, int i2);

    public abstract void setDir(byte b);

    public boolean isOutOfMoveDistance() {
        return Math.abs(this.iMapX - this.iBornMapX) > (this.iMoveDistance >> 1) || Math.abs(this.iMapY - this.iBornMapY) > (this.iMoveDistance >> 1);
    }

    public void loadRes(SpriteBaseData sprBaseData) {
        String pt = "";
        Bitmap[] imgs = new Bitmap[sprBaseData.imageFiles.length];
        for (int i = 0; i < sprBaseData.imageFiles.length; i++) {
            if (sprBaseData.imagePtFiles != null && !sprBaseData.imagePtFiles[i].equals("0")) {
                pt = String.valueOf(pt) + sprBaseData.imagePtFiles[i];
                imgs[i] = Common.getImageFromCache("/action/" + sprBaseData.imageFiles[i] + ".p", "/action/" + sprBaseData.imagePtFiles[i] + ".pt", false);
            } else {
                imgs[i] = Common.getImageFromCache("/action/" + sprBaseData.imageFiles[i] + ".p", false, true);
            }
        }
        String actionFile = "/action/" + sprBaseData.actionFile + ".n";
        if (Common.clearResCache.containsKey(String.valueOf(actionFile) + pt)) {
            this.actionSet = (ActionSet) Common.clearResCache.get(String.valueOf(actionFile) + pt);
            this.isHasResInCache = true;
        } else {
            this.actionSet = ActionSet.createActionSet(actionFile, imgs);
            Common.clearResCache.put(String.valueOf(actionFile) + pt, this.actionSet);
            this.isHasResInCache = false;
        }
        this.curAction = this.actionSet.actionDatas[0];
        this.iFootWidth = this.actionSet.footWidth;
        this.iFootHeight = this.actionSet.footHeight;
    }
}
