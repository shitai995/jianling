package com.qq.WapGame;

import com.qq.taf.jce.JceDisplayer;
import com.qq.taf.jce.JceInputStream;
import com.qq.taf.jce.JceOutputStream;
import com.qq.taf.jce.JceStruct;
import com.qq.taf.jce.JceUtil;

/* JADX INFO: loaded from: classes.dex */
public final class bq extends JceStruct {
    static byte[] bn;
    static final /* synthetic */ boolean c;
    public int status = 0;
    public String ac = "";
    public byte[] bm = null;

    static {
        c = !bq.class.desiredAssertionStatus();
    }

    public String a() {
        return "WapGame.UpdateImgSC";
    }

    public String b() {
        return "com.qq.WapGame.UpdateImgSC";
    }

    public int getStatus() {
        return this.status;
    }

    public void f(int i) {
        this.status = i;
    }

    public String J() {
        return this.ac;
    }

    public void m(String str) {
        this.ac = str;
    }

    public byte[] ak() {
        return this.bm;
    }

    public void f(byte[] bArr) {
        this.bm = bArr;
    }

    public bq() {
        f(this.status);
        m(this.ac);
        f(this.bm);
    }

    public bq(int i, String str, byte[] bArr) {
        f(i);
        m(str);
        f(bArr);
    }

    public boolean equals(Object obj) {
        bq bqVar = (bq) obj;
        return JceUtil.equals(this.status, bqVar.status) && JceUtil.equals(this.ac, bqVar.ac) && JceUtil.equals(this.bm, bqVar.bm);
    }

    public Object clone() {
        try {
            return super.clone();
        } catch (CloneNotSupportedException e) {
            if (c) {
                return null;
            }
            throw new AssertionError();
        }
    }

    @Override // com.qq.taf.jce.JceStruct
    public void writeTo(JceOutputStream jceOutputStream) {
        jceOutputStream.write(this.status, 0);
        if (this.ac != null) {
            jceOutputStream.write(this.ac, 1);
        }
        if (this.bm != null) {
            jceOutputStream.write(this.bm, 2);
        }
    }

    @Override // com.qq.taf.jce.JceStruct
    public void readFrom(JceInputStream jceInputStream) {
        f(jceInputStream.read(this.status, 0, true));
        m(jceInputStream.readString(1, false));
        if (bn == null) {
            bn = new byte[1];
            bn[0] = 0;
        }
        f(jceInputStream.read(bn, 2, false));
    }

    @Override // com.qq.taf.jce.JceStruct
    public void display(StringBuilder sb, int i) {
        JceDisplayer jceDisplayer = new JceDisplayer(sb, i);
        jceDisplayer.display(this.status, "status");
        jceDisplayer.display(this.ac, "errMsg");
        jceDisplayer.display(this.bm, "b_errMsg");
    }
}
