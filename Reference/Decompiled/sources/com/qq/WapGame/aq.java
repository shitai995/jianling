package com.qq.WapGame;

import com.qq.taf.jce.JceDisplayer;
import com.qq.taf.jce.JceInputStream;
import com.qq.taf.jce.JceOutputStream;
import com.qq.taf.jce.JceStruct;
import com.qq.taf.jce.JceUtil;

/* JADX INFO: loaded from: classes.dex */
public final class aq extends JceStruct {
    static byte[] aJ;
    static final /* synthetic */ boolean c;
    public String z = "";
    public int A = 0;
    public int B = 0;
    public String C = "";
    public String D = "";
    public String aH = "";
    public boolean K = true;
    public byte[] aI = null;

    static {
        c = !aq.class.desiredAssertionStatus();
    }

    public String a() {
        return "WapGame.ReportBugDataCS";
    }

    public String b() {
        return "com.qq.WapGame.ReportBugDataCS";
    }

    public String u() {
        return this.z;
    }

    public void h(String str) {
        this.z = str;
    }

    public int v() {
        return this.A;
    }

    public void c(int i) {
        this.A = i;
    }

    public int w() {
        return this.B;
    }

    public void d(int i) {
        this.B = i;
    }

    public String x() {
        return this.C;
    }

    public void i(String str) {
        this.C = str;
    }

    public String y() {
        return this.D;
    }

    public void j(String str) {
        this.D = str;
    }

    public String Z() {
        return this.aH;
    }

    public void t(String str) {
        this.aH = str;
    }

    public boolean B() {
        return this.K;
    }

    public void a(boolean z) {
        this.K = z;
    }

    public byte[] aa() {
        return this.aI;
    }

    public void d(byte[] bArr) {
        this.aI = bArr;
    }

    public aq() {
        h(this.z);
        c(this.A);
        d(this.B);
        i(this.C);
        j(this.D);
        t(this.aH);
        a(this.K);
        d(this.aI);
    }

    public aq(String str, int i, int i2, String str2, String str3, String str4, boolean z, byte[] bArr) {
        h(str);
        c(i);
        d(i2);
        i(str2);
        j(str3);
        t(str4);
        a(z);
        d(bArr);
    }

    public boolean equals(Object obj) {
        aq aqVar = (aq) obj;
        return JceUtil.equals(this.z, aqVar.z) && JceUtil.equals(this.A, aqVar.A) && JceUtil.equals(this.B, aqVar.B) && JceUtil.equals(this.C, aqVar.C) && JceUtil.equals(this.D, aqVar.D) && JceUtil.equals(this.aH, aqVar.aH) && JceUtil.equals(this.K, aqVar.K) && JceUtil.equals(this.aI, aqVar.aI);
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
        jceOutputStream.write(this.z, 0);
        jceOutputStream.write(this.A, 1);
        jceOutputStream.write(this.B, 2);
        jceOutputStream.write(this.C, 3);
        jceOutputStream.write(this.D, 4);
        jceOutputStream.write(this.aH, 5);
        jceOutputStream.write(this.K, 6);
        if (this.aI != null) {
            jceOutputStream.write(this.aI, 7);
        }
    }

    @Override // com.qq.taf.jce.JceStruct
    public void readFrom(JceInputStream jceInputStream) {
        h(jceInputStream.readString(0, true));
        c(jceInputStream.read(this.A, 1, true));
        d(jceInputStream.read(this.B, 2, true));
        i(jceInputStream.readString(3, true));
        j(jceInputStream.readString(4, true));
        t(jceInputStream.readString(5, true));
        a(jceInputStream.read(this.K, 6, false));
        if (aJ == null) {
            aJ = new byte[1];
            aJ[0] = 0;
        }
        d(jceInputStream.read(aJ, 7, false));
    }

    @Override // com.qq.taf.jce.JceStruct
    public void display(StringBuilder sb, int i) {
        JceDisplayer jceDisplayer = new JceDisplayer(sb, i);
        jceDisplayer.display(this.z, "uin");
        jceDisplayer.display(this.A, "cpId");
        jceDisplayer.display(this.B, "gameId");
        jceDisplayer.display(this.C, "version");
        jceDisplayer.display(this.D, "gameKey");
        jceDisplayer.display(this.aH, "bugInfo");
        jceDisplayer.display(this.K, "disableStrByte");
        jceDisplayer.display(this.aI, "b_bugInfo");
    }
}
