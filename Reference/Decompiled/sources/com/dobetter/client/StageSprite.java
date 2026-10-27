package com.dobetter.client;

import com.dobetter.action.ActionSet;
import com.dobetter.client.data.SpriteBaseData;
import com.dobetter.common.Graphics;
import com.dobetter.common.Tool;

/* JADX INFO: loaded from: classes.dex */
public class StageSprite extends Sprite {
    SpriteBaseData data;
    boolean isDieAfterActionOver;

    public StageSprite(int id, SpriteBaseData npcData, int mx, int my) {
        this.ID = id;
        initData(npcData, mx, my);
        loadRes(npcData);
        initAct();
    }

    public StageSprite(ActionSet as, int mx, int my) {
        this.isDieAfterActionOver = true;
        this.actionSet = as;
        initAct();
        this.iMapX = mx;
        this.iMapY = my;
    }

    public void initData(SpriteBaseData npcData, int mx, int my) {
        this.data = npcData;
        this.bType = npcData.bType;
        this.iSortID = this.data.sortID;
        this.iMapX = mx;
        this.iMapY = my;
        this.iBornMapX = mx;
        this.iBornMapY = my;
    }

    @Override // com.dobetter.client.Sprite
    public void freeSprite() {
        if (this.actionSet != null) {
            this.actionSet.releasImg();
            this.actionSet = null;
        }
    }

    @Override // com.dobetter.client.Sprite
    public void initAct() {
        this.iCurFrameIndex = 0;
        this.bActID = (byte) 0;
        this.bActState = (byte) 0;
        getCurActionData();
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
        this.bCurDir = dir;
        this.isFaceToTheRight = this.bCurDir % 2 == 1;
    }

    @Override // com.dobetter.client.Sprite
    public void getCurActionData() {
        if (this.bCurDir < 2) {
            this.iCurActionID = this.bActID << 1;
        } else {
            this.iCurActionID = (this.bActID << 1) + 1;
        }
        getCurData();
    }

    public void getCurData() {
        try {
            this.curAction = this.actionSet.actionDatas[this.iCurActionID];
            this.curFrame = this.actionSet.frameDatas[this.curAction.frameID[this.iCurFrameIndex]];
            this.iCurVx = this.curAction.iVx[this.iCurFrameIndex];
            this.iCurVy = this.curAction.iVy[this.iCurFrameIndex];
        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    public void cycleAction() {
        if (this.iCurFrameIndex < this.curAction.frameNum - 1) {
            this.iCurFrameIndex++;
            getCurActionData();
        } else if (this.isDieAfterActionOver) {
            this.isDead = true;
        } else if (this.bActState == 0) {
            this.iCurFrameIndex = 0;
            getCurActionData();
        } else {
            initAct();
        }
    }

    @Override // com.dobetter.client.Sprite
    public void cycle() {
        if (!this.isInvisible) {
            if (this.isDieAfterActionOver) {
                move();
            }
            cycleAction();
        }
    }

    @Override // com.dobetter.client.Sprite
    public void act(byte act) {
        this.bActID = act;
        this.iCurFrameIndex = 0;
        if (this.bActID == 0 || this.bActID == 1) {
            this.bActState = (byte) 0;
        }
        getCurActionData();
    }

    @Override // com.dobetter.client.Sprite
    public void draw(Graphics g, int viewMapX, int viewMapY, int dx, int dy) {
        if (this.actionSet != null) {
            int drawX = (this.iMapX - viewMapX) + dx;
            int drawY = (this.iMapY - viewMapY) + dy;
            try {
                this.actionSet.drawFrame(g, this.curFrame, drawX, drawY, this.isFaceToTheRight);
            } catch (Exception e) {
                e.printStackTrace();
            }
        }
    }

    @Override // com.dobetter.client.Sprite
    public boolean isInScreen(int viewMapX, int viewMapY, int viewWidth, int viewHeight) {
        int x = this.curFrame.edgeRect.x;
        if (this.isFaceToTheRight) {
            x = ((-this.curFrame.edgeRect.x) - this.curFrame.edgeRect.width) + this.iFootWidth;
        }
        return Tool.isRectIntersected(this.iMapX + x, this.iMapY + this.curFrame.edgeRect.y, this.curFrame.edgeRect.width, this.curFrame.edgeRect.height, viewMapX, viewMapY, viewWidth, viewHeight);
    }
}
