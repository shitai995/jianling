package com.qq.WapGame;

import com.qq.taf.jce.JceDisplayer;
import com.qq.taf.jce.JceInputStream;
import com.qq.taf.jce.JceOutputStream;
import com.qq.taf.jce.JceStruct;
import com.qq.taf.jce.JceUtil;

/* JADX INFO: loaded from: classes.dex */
public final class au extends JceStruct {
    static final /* synthetic */ boolean c;
    public String z = "";
    public int A = 0;
    public int B = 0;
    public String C = "";
    public String D = "";
    public String E = "";

    static {
        c = !au.class.desiredAssertionStatus();
    }

    public String a() {
        return "WapGame.ReportInstallCS";
    }

    public String b() {
        return "com.qq.WapGame.ReportInstallCS";
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

    public String getDeviceId() {
        return this.E;
    }

    public void k(String str) {
        this.E = str;
    }

    public au() {
        h(this.z);
        c(this.A);
        d(this.B);
        i(this.C);
        j(this.D);
        k(this.E);
    }

    public au(String str, int i, int i2, String str2, String str3, String str4) {
        h(str);
        c(i);
        d(i2);
        i(str2);
        j(str3);
        k(str4);
    }

    public boolean equals(Object obj) {
        au auVar = (au) obj;
        return JceUtil.equals(this.z, auVar.z) && JceUtil.equals(this.A, auVar.A) && JceUtil.equals(this.B, auVar.B) && JceUtil.equals(this.C, auVar.C) && JceUtil.equals(this.D, auVar.D) && JceUtil.equals(this.E, auVar.E);
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
        jceOutputStream.write(this.E, 5);
    }

    @Override // com.qq.taf.jce.JceStruct
    public void readFrom(JceInputStream jceInputStream) {
        h(jceInputStream.readString(0, true));
        c(jceInputStream.read(this.A, 1, true));
        d(jceInputStream.read(this.B, 2, true));
        i(jceInputStream.readString(3, true));
        j(jceInputStream.readString(4, true));
        k(jceInputStream.readString(5, true));
    }

    @Override // com.qq.taf.jce.JceStruct
    public void display(StringBuilder sb, int i) {
        JceDisplayer jceDisplayer = new JceDisplayer(sb, i);
        jceDisplayer.display(this.z, "uin");
        jceDisplayer.display(this.A, "cpId");
        jceDisplayer.display(this.B, "gameId");
        jceDisplayer.display(this.C, "version");
        jceDisplayer.display(this.D, "gameKey");
        jceDisplayer.display(this.E, "deviceId");
    }
}
