package com.dobetter.action;

/* JADX INFO: loaded from: classes.dex */
public class ActionData {
    public byte[] bAttackType;
    public short[] frameID;
    public short frameNum;
    public short[] iVx;
    public short[] iVy;
    public boolean[] isAttack;
    public boolean[] isBeAttack;

    public ActionData(short fn) {
        this.frameNum = fn;
        this.frameID = new short[fn];
        this.iVx = new short[fn];
        this.iVy = new short[fn];
        this.isAttack = new boolean[fn];
        this.isBeAttack = new boolean[fn];
        this.bAttackType = new byte[fn];
    }
}
