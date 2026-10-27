package com.qq.WapGame;

import com.qq.taf.jce.JceDisplayer;
import com.qq.taf.jce.JceInputStream;
import com.qq.taf.jce.JceOutputStream;
import com.qq.taf.jce.JceStruct;
import com.qq.taf.jce.JceUtil;

/* JADX INFO: loaded from: classes.dex */
public final class bc extends JceStruct {
    static i aR;
    static final /* synthetic */ boolean c;
    public String z = "";
    public int A = 0;
    public int B = 0;
    public String C = "";
    public String D = "";
    public i aQ = null;

    static {
        c = !bc.class.desiredAssertionStatus();
    }

    public String a() {
        return "WapGame.ReportPlayerGpsCS";
    }

    public String b() {
        return "com.qq.WapGame.ReportPlayerGpsCS";
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

    public i ab() {
        return this.aQ;
    }

    public void a(i iVar) {
        this.aQ = iVar;
    }

    public bc() {
        h(this.z);
        c(this.A);
        d(this.B);
        i(this.C);
        j(this.D);
        a(this.aQ);
    }

    public bc(String str, int i, int i2, String str2, String str3, i iVar) {
        h(str);
        c(i);
        d(i2);
        i(str2);
        j(str3);
        a(iVar);
    }

    public boolean equals(Object obj) {
        bc bcVar = (bc) obj;
        return JceUtil.equals(this.z, bcVar.z) && JceUtil.equals(this.A, bcVar.A) && JceUtil.equals(this.B, bcVar.B) && JceUtil.equals(this.C, bcVar.C) && JceUtil.equals(this.D, bcVar.D) && JceUtil.equals(this.aQ, bcVar.aQ);
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
        jceOutputStream.write((JceStruct) this.aQ, 5);
    }

    @Override // com.qq.taf.jce.JceStruct
    public void readFrom(JceInputStream jceInputStream) {
        h(jceInputStream.readString(0, true));
        c(jceInputStream.read(this.A, 1, true));
        d(jceInputStream.read(this.B, 2, true));
        i(jceInputStream.readString(3, true));
        j(jceInputStream.readString(4, true));
        if (aR == null) {
            aR = new i();
        }
        a((i) jceInputStream.read((JceStruct) aR, 5, true));
    }

    @Override // com.qq.taf.jce.JceStruct
    public void display(StringBuilder sb, int i) {
        JceDisplayer jceDisplayer = new JceDisplayer(sb, i);
        jceDisplayer.display(this.z, "uin");
        jceDisplayer.display(this.A, "cpId");
        jceDisplayer.display(this.B, "gameId");
        jceDisplayer.display(this.C, "version");
        jceDisplayer.display(this.D, "gameKey");
        jceDisplayer.display((JceStruct) this.aQ, "gpsInfo");
    }
}
