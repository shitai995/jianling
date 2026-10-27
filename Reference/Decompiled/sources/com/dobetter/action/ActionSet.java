package com.dobetter.action;

import android.graphics.Bitmap;
import com.dobetter.common.GZIP;
import com.dobetter.common.Graphics;
import com.dobetter.common.Rectangle;
import com.dobetter.common.Tool;
import java.io.ByteArrayInputStream;
import java.io.DataInputStream;
import java.io.InputStream;

/* JADX INFO: loaded from: classes.dex */
public class ActionSet {
    public ActionData[] actionDatas;
    public short actionNum;
    public int footHeight;
    public int footWidth;
    public FrameData[] frameDatas;
    public short frameNum;
    public byte imageNum;
    public Rectangle[][] imageRects;
    public Bitmap[] images;
    public short[] rectNum;

    public void releasImg() {
        if (this.images != null) {
            for (int i = 0; i < this.imageNum; i++) {
                if (this.images[i] != null) {
                    this.images[i].recycle();
                    this.images[i] = null;
                }
            }
            this.images = null;
        }
    }

    public static ActionSet createActionSet(byte[] gdata, Bitmap[] imgs) {
        try {
            byte[] data = GZIP.inflate(gdata);
            ByteArrayInputStream bin = new ByteArrayInputStream(data);
            return createActionSet(bin, imgs);
        } catch (Exception e) {
            Tool.reportException(e);
            return null;
        }
    }

    public static ActionSet createActionSet(String actionFile, Bitmap[] imgs) {
        try {
            System.out.println("loading actionFile = " + actionFile);
            InputStream in = Tool.getResourceAsStream(actionFile);
            byte[] gdata = Tool.getBytesFromInput(in);
            return createActionSet(gdata, imgs);
        } catch (Exception e) {
            Tool.reportException(e);
            return null;
        }
    }

    private static ActionSet createActionSet(InputStream in, Bitmap[] imgs) throws Exception {
        ActionSet actset = new ActionSet();
        DataInputStream din = new DataInputStream(in);
        actset.imageNum = din.readByte();
        if (actset.imageNum != imgs.length) {
            throw new Exception("Action Bitmap Num Error");
        }
        actset.rectNum = new short[actset.imageNum];
        actset.imageRects = new Rectangle[actset.imageNum][];
        for (int i = 0; i < actset.imageNum; i++) {
            actset.rectNum[i] = din.readShort();
            actset.imageRects[i] = new Rectangle[actset.rectNum[i]];
            for (int j = 0; j < actset.rectNum[i]; j++) {
                actset.imageRects[i][j] = new Rectangle();
                actset.imageRects[i][j].x = din.readShort();
                actset.imageRects[i][j].y = din.readShort();
                actset.imageRects[i][j].width = din.readByte() & 255;
                actset.imageRects[i][j].height = din.readByte() & 255;
            }
        }
        actset.frameNum = din.readShort();
        actset.frameDatas = new FrameData[actset.frameNum];
        for (int i2 = 0; i2 < actset.frameNum; i2++) {
            short frameRectNum = (short) (din.readByte() & 255);
            actset.frameDatas[i2] = new FrameData(frameRectNum);
            FrameData fd = actset.frameDatas[i2];
            for (int j2 = 0; j2 < fd.rectNum; j2++) {
                fd.imgID[j2] = din.readByte();
                fd.rectID[j2] = din.readShort();
                fd.drawType[j2] = din.readByte();
                fd.rectDX[j2] = din.readShort();
                fd.rectDY[j2] = din.readShort();
                if (fd.imgID[j2] < 0) {
                    fd.rectWidth[j2] = din.readShort();
                    fd.rectHeight[j2] = din.readShort();
                    fd.color[j2] = din.readInt();
                }
            }
            if (din.readBoolean()) {
                int length = din.readInt();
                fd.atkRect = new Rectangle[length];
                for (int n = 0; n < length; n++) {
                    fd.atkRect[n] = new Rectangle();
                    fd.atkRect[n].x = din.readShort();
                    fd.atkRect[n].y = din.readShort();
                    fd.atkRect[n].width = din.readShort();
                    fd.atkRect[n].height = din.readShort();
                }
            }
            if (din.readBoolean()) {
                int length2 = din.readInt();
                fd.bodyRect = new Rectangle[length2];
                for (int n2 = 0; n2 < length2; n2++) {
                    fd.bodyRect[n2] = new Rectangle();
                    fd.bodyRect[n2].x = din.readShort();
                    fd.bodyRect[n2].y = din.readShort();
                    fd.bodyRect[n2].width = din.readShort();
                    fd.bodyRect[n2].height = din.readShort();
                }
            }
            fd.edgeRect.x = din.readShort();
            fd.edgeRect.y = din.readShort();
            fd.edgeRect.width = din.readShort();
            fd.edgeRect.height = din.readShort();
        }
        actset.footWidth = din.readByte() & 255;
        actset.footHeight = din.readByte() & 255;
        actset.actionNum = din.readShort();
        actset.actionDatas = new ActionData[actset.actionNum];
        for (int i3 = 0; i3 < actset.actionNum; i3++) {
            short fn = din.readShort();
            actset.actionDatas[i3] = new ActionData(fn);
            ActionData ad = actset.actionDatas[i3];
            for (int j3 = 0; j3 < ad.frameNum; j3++) {
                ad.frameID[j3] = din.readShort();
                ad.iVx[j3] = din.readShort();
                ad.iVy[j3] = din.readShort();
                ad.isAttack[j3] = din.readBoolean();
                ad.isBeAttack[j3] = din.readBoolean();
                ad.bAttackType[j3] = din.readByte();
            }
        }
        din.close();
        in.close();
        actset.images = imgs;
        return actset;
    }

    public void drawFrame(Graphics g, int frameID, int footX, int footY) {
        drawFrame(g, this.frameDatas[frameID], footX, footY);
    }

    public void drawFrame(Graphics g, FrameData fd, int footX, int footY) {
        if (fd != null) {
            for (int i = 0; i < fd.rectNum; i++) {
                int iId = fd.imgID[i];
                int drawType = fd.drawType[i];
                short s = fd.rectDX[i];
                short s2 = fd.rectDY[i];
                if (iId < 0) {
                    int color = fd.color[i];
                    g.setColor(color);
                    Tool.fillRect(g, footX + s, footY + s2, fd.rectWidth[i], fd.rectHeight[i], drawType);
                } else {
                    short s3 = fd.rectID[i];
                    if (s3 < this.rectNum[iId]) {
                        try {
                            Rectangle rct = this.imageRects[iId][s3];
                            if (this.images[iId] != null) {
                                Tool.drawRegion(g, this.images[iId], rct.x, rct.y, rct.width, rct.height, drawType, footX + s, footY + s2);
                            }
                        } catch (Exception e) {
                            e.printStackTrace();
                            System.out.println("iId=" + iId);
                            System.out.println("rId=" + ((int) s3));
                        }
                    }
                }
            }
        }
    }

    public void drawFrameCycle(Graphics g, int actIndex, int drawTimes, int footX, int footY) {
        if (actIndex < 0) {
            actIndex = 0;
        } else if (actIndex >= this.actionDatas.length) {
            actIndex = this.actionDatas.length - 1;
        }
        FrameData fd = this.frameDatas[this.actionDatas[actIndex].frameID[Math.abs(drawTimes) % this.actionDatas[actIndex].frameID.length]];
        drawFrame(g, fd, footX, footY);
    }

    public void drawFrame(Graphics graphics, FrameData frameData, int i, int i2, boolean z) {
        int i3;
        if (frameData != null) {
            for (int i4 = 0; i4 < frameData.rectNum; i4++) {
                byte b = frameData.imgID[i4];
                int mirrorTransType = frameData.drawType[i4];
                short s = frameData.rectDX[i4];
                short s2 = frameData.rectDY[i4];
                if (b < 0) {
                    graphics.setColor(frameData.color[i4]);
                    short s3 = frameData.rectWidth[i4];
                    short s4 = frameData.rectHeight[i4];
                    if (z) {
                        i3 = s;
                        int i5 = ((-s) - s3) + this.footWidth;
                        mirrorTransType = Tool.getMirrorTransType(frameData.drawType[i4]);
                        i3 = i5;
                    }
                    i3 = s;
                    Tool.fillRect(graphics, i + i3, i2 + s2, s3, s4, mirrorTransType);
                } else {
                    short s5 = frameData.rectID[i4];
                    if (s5 < this.rectNum[b]) {
                        try {
                            Rectangle rectangle = this.imageRects[b][s5];
                            int i6 = s;
                            if (z) {
                                int i7 = ((-s) - rectangle.width) + this.footWidth;
                                mirrorTransType = Tool.getMirrorTransType(frameData.drawType[i4]);
                                i6 = i7;
                            }
                            if (this.images[b] != null) {
                                Tool.drawRegion(graphics, this.images[b], rectangle.x, rectangle.y, rectangle.width, rectangle.height, mirrorTransType == true ? 1 : 0, i + i6, i2 + s2);
                            }
                        } catch (Exception e) {
                            e.printStackTrace();
                            System.out.println("iId=" + ((int) b));
                            System.out.println("rId=" + ((int) s5));
                        }
                    }
                }
            }
        }
    }

    public void drawFrame(Graphics g, int frameID, int footX, int footY, boolean bMirror) {
        FrameData fd = this.frameDatas[frameID];
        drawFrame(g, fd, footX, footY, bMirror);
    }

    public void drawFrame(Graphics g, int actIndex, int frameIndex, int footX, int footY, boolean bMirror) {
        if (actIndex < 0) {
            actIndex = 0;
        } else if (actIndex >= this.actionDatas.length) {
            actIndex = this.actionDatas.length - 1;
        }
        if (frameIndex < 0) {
            frameIndex = 0;
        } else if (frameIndex >= this.actionDatas[actIndex].frameID.length) {
            frameIndex = this.actionDatas[actIndex].frameID.length - 1;
        }
        FrameData fd = this.frameDatas[this.actionDatas[actIndex].frameID[frameIndex]];
        drawFrame(g, fd, footX, footY, bMirror);
    }

    public void drawFrameCycle(Graphics g, int actIndex, int drawTimes, int footX, int footY, boolean bMirror) {
        if (actIndex < 0) {
            actIndex = 0;
        } else if (actIndex >= this.actionDatas.length) {
            actIndex = this.actionDatas.length - 1;
        }
        FrameData fd = this.frameDatas[this.actionDatas[actIndex].frameID[Math.abs(drawTimes) % this.actionDatas[actIndex].frameID.length]];
        drawFrame(g, fd, footX, footY, bMirror);
    }
}
