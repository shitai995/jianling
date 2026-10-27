package com.dobetter.action;

import com.dobetter.common.Rectangle;

/* JADX INFO: loaded from: classes.dex */
public class FrameData {
    public Rectangle[] atkRect;
    public Rectangle[] bodyRect;
    public int[] color;
    public byte[] drawType;
    public Rectangle edgeRect = new Rectangle();
    public byte[] imgID;
    public short[] rectDX;
    public short[] rectDY;
    public short[] rectHeight;
    public short[] rectID;
    public short rectNum;
    public short[] rectWidth;

    public FrameData(short rn) {
        this.rectNum = rn;
        this.imgID = new byte[rn];
        this.rectID = new short[rn];
        this.drawType = new byte[rn];
        this.color = new int[rn];
        this.rectDX = new short[rn];
        this.rectDY = new short[rn];
        this.rectWidth = new short[rn];
        this.rectHeight = new short[rn];
    }
}
