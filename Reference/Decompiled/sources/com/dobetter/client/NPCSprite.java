package com.dobetter.client;

import com.dobetter.client.data.SpriteBaseData;
import com.dobetter.common.Graphics;
import com.dobetter.common.Tool;

/* JADX INFO: loaded from: classes.dex */
public class NPCSprite extends Sprite {
    byte bDropDir;
    private byte bDropSpeed;
    public byte bInitDir;
    private byte bMaxDropSpeed;
    private byte bMoveSpeed;
    SpriteBaseData data;
    public int iPateId = -1;
    int iShowTimeCount;
    boolean isDrop;
    boolean isDropOver;
    Object item;

    public NPCSprite(int id, SpriteBaseData npcData, int mx, int my) {
        this.isCheckMapBlock = true;
        this.ID = id;
        initData(npcData, mx, my);
        loadRes(npcData);
        this.curAction = this.actionSet.actionDatas[0];
        this.iFootWidth = this.actionSet.footWidth;
        this.iFootHeight = this.actionSet.footHeight;
        initAct();
    }

    public NPCSprite(int id, byte type, int mx, int my, byte moveSpeed, byte dropSpeed, int showTime, byte dropDir, Object item) {
        this.isCheckMapBlock = false;
        this.ID = id;
        this.bType = type;
        this.iLayerIndex = 2;
        this.iMapX = mx;
        this.iMapY = my;
        this.iShowTimeCount = showTime;
        this.item = item;
        this.bMoveSpeed = moveSpeed;
        this.bDropSpeed = dropSpeed;
        this.bMaxDropSpeed = dropSpeed;
        this.bDropDir = dropDir;
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

    public void setInitDir(byte dir) {
        this.bInitDir = dir;
    }

    public void setPateId(int id) {
        this.iPateId = id;
    }

    @Override // com.dobetter.client.Sprite
    public void getCurActionData() {
        this.iCurActionID = this.bActID;
        getCurData();
        getCurVxVy();
    }

    public void getCurData() {
        try {
            this.curAction = this.actionSet.actionDatas[this.iCurActionID];
            this.curFrame = this.actionSet.frameDatas[this.curAction.frameID[this.iCurFrameIndex]];
        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    public void getCurVxVy() {
        if (this.bCurDir == 2 && this.bActID == 1) {
            this.iCurVx = 0;
            this.iCurVy = -this.iMoveSpeed;
        } else if (this.bCurDir == 3 && this.bActID == 1) {
            this.iCurVx = 0;
            this.iCurVy = this.iMoveSpeed;
        } else {
            this.iCurVx = (this.isFaceToTheRight ? (short) -1 : (short) 1) * this.curAction.iVx[this.iCurFrameIndex];
            this.iCurVy = this.curAction.iVy[this.iCurFrameIndex];
        }
    }

    public void cycleAction() {
        if (!this.isSpriteAnimate) {
            if (this.isKeepFrame) {
                getCurData();
                return;
            }
            if (this.iCurFrameIndex < this.curAction.frameNum - 1) {
                this.iCurFrameIndex++;
                getCurActionData();
                return;
            }
            if (this.bType == 17) {
                if (this.bActID == 0) {
                    this.bActID = (byte) 1;
                    this.iCurFrameIndex = 0;
                    getCurActionData();
                    return;
                } else {
                    this.iCurFrameIndex = 0;
                    getCurActionData();
                    return;
                }
            }
            if (this.bActState == 0) {
                this.iCurFrameIndex = 0;
                getCurActionData();
            } else {
                initAct();
            }
        }
    }

    @Override // com.dobetter.client.Sprite
    public void cycle() {
        if (!this.isInvisible) {
            if (this.iFaceDuration > 0) {
                if (this.iFaceFrameIndex < this.iFaceDuration) {
                    this.iFaceFrameIndex++;
                } else {
                    this.iFaceFrameIndex = 0;
                    this.iFaceDuration = 0;
                }
            }
            if (this.bType == 13) {
                if (this.iShowTimeCount > 0) {
                    if (this.iShowTimeCount < 260) {
                        boolean isMoveOnX = false;
                        boolean isMoveOnY = false;
                        if (this.iMapX <= GameCanvas.instance.role.iMapX - this.bMoveSpeed) {
                            this.iMapX += this.bMoveSpeed;
                        } else if (this.iMapX > GameCanvas.instance.role.iMapX + GameCanvas.instance.role.iFootWidth + this.bMoveSpeed) {
                            this.iMapX -= this.bMoveSpeed;
                        } else {
                            isMoveOnX = true;
                        }
                        if (this.iMapY <= GameCanvas.instance.role.iMapY - this.bMoveSpeed) {
                            this.iMapY += this.bMoveSpeed;
                        } else if (this.iMapY > GameCanvas.instance.role.iMapY + GameCanvas.instance.role.iFootHeight + this.bMoveSpeed) {
                            this.iMapY -= this.bMoveSpeed;
                        } else {
                            isMoveOnY = true;
                        }
                        if (isMoveOnX && isMoveOnY) {
                            this.isDead = true;
                            GameCanvas.instance.getItem(this.item);
                        }
                    } else if (!this.isDropOver) {
                        if (!this.isDrop) {
                            if (this.bDropSpeed > 1) {
                                if (this.iMapY >= this.bDropSpeed) {
                                    this.iMapY -= this.bDropSpeed;
                                }
                                this.bDropSpeed = (byte) (this.bDropSpeed - 2);
                            } else {
                                this.isDrop = true;
                            }
                        } else if (this.bDropSpeed <= this.bMaxDropSpeed - 2) {
                            this.bDropSpeed = (byte) (this.bDropSpeed + 2);
                            if (this.iMapY <= GameCanvas.instance.map.sMapHeight - this.bDropSpeed) {
                                this.iMapY += this.bDropSpeed;
                            }
                        } else {
                            this.isDropOver = true;
                        }
                        if (this.bDropDir == 0) {
                            if (this.iMapX > 1) {
                                this.iMapX -= 2;
                            }
                        } else if (this.bDropDir == 1 && this.iMapX < GameCanvas.instance.map.sMapWidth - 1) {
                            this.iMapX += 2;
                        }
                    }
                    this.iShowTimeCount--;
                    return;
                }
                this.isDead = true;
                return;
            }
            cycleAction();
        }
    }

    @Override // com.dobetter.client.Sprite
    public void act(byte act) {
        if (this.bActID != act) {
            this.bActID = act;
            this.iCurFrameIndex = 0;
            if (this.bActID == 0 || this.bActID == 1) {
                this.bActState = (byte) 0;
            } else {
                this.bActState = (byte) 4;
            }
            getCurActionData();
        }
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
                System.out.println("NPCID=" + this.ID);
            }
            if (this.iFaceDuration > 0) {
                int bx = drawX + ((this.iFootWidth - GameCanvas.faceAS.footWidth) >> 1);
                int by = (this.curFrame.edgeRect.y + drawY) - (GameCanvas.faceAS.footHeight << 1);
                GameCanvas.faceAS.drawFrameCycle(g, this.iFaceIndex, this.iFaceFrameIndex, bx, by);
                return;
            }
            return;
        }
        if (this.bType == 13) {
            int drawX2 = (this.iMapX - viewMapX) + dx;
            int drawY2 = (this.iMapY - viewMapY) + dy;
            if (this.item instanceof Prop) {
                Prop prop = (Prop) this.item;
                Tool.drawClipImage(g, GameCanvas.instance.iconBGImg[prop.data.bType], drawX2 - 2, drawY2 - 2, 0, 0, 26, 26);
                Tool.drawRegion(g, GameCanvas.instance.propIconImg, prop.data.bID * 22, 0, 22, 22, 0, drawX2, drawY2);
                return;
            }
            Tool.drawRegion(g, GameCanvas.instance.goldImg, 0, 0, GameCanvas.instance.goldImg.getWidth(), GameCanvas.instance.goldImg.getHeight(), 0, drawX2, drawY2);
        }
    }

    @Override // com.dobetter.client.Sprite
    public boolean isInScreen(int viewMapX, int viewMapY, int viewWidth, int viewHeight) {
        if (this.bType == 13) {
            return Tool.isRectIntersected(this.iMapX, this.iMapY, 22, 22, viewMapX, viewMapY, viewWidth, viewHeight);
        }
        int x = this.curFrame.edgeRect.x;
        if (this.isFaceToTheRight) {
            x = ((-this.curFrame.edgeRect.x) - this.curFrame.edgeRect.width) + this.iFootWidth;
        }
        return Tool.isRectIntersected(this.iMapX + x, this.iMapY + this.curFrame.edgeRect.y, this.curFrame.edgeRect.width, this.curFrame.edgeRect.height, viewMapX, viewMapY, viewWidth, viewHeight);
    }

    public void setDir(int mx, int my) {
        int fcx = this.iMapX + (this.iFootWidth >> 1);
        int fcy = this.iMapY + (this.iFootHeight >> 1);
        if (fcx > mx) {
            if (Math.abs(mx - fcx) > Math.abs(my - fcy)) {
                setDir((byte) 0);
            }
        } else if (Math.abs(mx - fcx) > Math.abs(my - fcy)) {
            setDir((byte) 1);
        }
    }
}
