package com.dobetter.client;

import com.dobetter.action.ActionSet;
import com.dobetter.common.Graphics;
import com.dobetter.common.Tool;

/* JADX INFO: loaded from: classes.dex */
public class EffectSprite extends Sprite {
    public EffectSprite(ActionSet as, int curActionID) {
        this.actionSet = as;
        this.curAction = as.actionDatas[curActionID];
        this.iLayerIndex = 3;
        this.bType = (byte) 16;
    }

    @Override // com.dobetter.client.Sprite
    public void cycle() {
        if (this.iCurFrameIndex < this.curAction.frameNum) {
            this.iCurFrameIndex++;
            getCurActionData();
        } else {
            this.isDead = true;
        }
    }

    @Override // com.dobetter.client.Sprite
    public void draw(Graphics g, int viewMapX, int viewMapY, int dx, int dy) {
        int drawX = (this.iMapX - viewMapX) + dx;
        int drawY = (this.iMapY - viewMapY) + dy;
        try {
            this.actionSet.drawFrame(g, this.curFrame, drawX, drawY);
        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    @Override // com.dobetter.client.Sprite
    public boolean isInScreen(int viewMapX, int viewMapY, int viewWidth, int viewHeight) {
        if (this.actionSet == null) {
            return false;
        }
        return Tool.isRectIntersected(this.iMapX + this.curFrame.edgeRect.x, this.iMapY + this.curFrame.edgeRect.y, this.curFrame.edgeRect.width, this.curFrame.edgeRect.height, viewMapX, viewMapY, viewWidth, viewHeight);
    }

    @Override // com.dobetter.client.Sprite
    public void setDir(byte dir) {
    }

    @Override // com.dobetter.client.Sprite
    public void move() {
    }

    @Override // com.dobetter.client.Sprite
    public void move(int curVx, int curVy) {
    }

    @Override // com.dobetter.client.Sprite
    public void initAct() {
    }

    @Override // com.dobetter.client.Sprite
    public void act(byte act) {
    }

    @Override // com.dobetter.client.Sprite
    public void getCurActionData() {
        this.curFrame = this.actionSet.frameDatas[this.curAction.frameID[this.iCurFrameIndex - 1]];
    }

    @Override // com.dobetter.client.Sprite
    public void freeSprite() {
    }
}
