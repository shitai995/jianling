package com.qq.WapGame;

import com.qq.taf.jce.JceDisplayer;
import com.qq.taf.jce.JceInputStream;
import com.qq.taf.jce.JceOutputStream;
import com.qq.taf.jce.JceStruct;
import com.qq.taf.jce.JceUtil;

/* JADX INFO: loaded from: classes.dex */
public final class i extends JceStruct {
    static final /* synthetic */ boolean c;
    public String q = "";
    public String r = "";
    public short s = 0;
    public short t = 0;
    public short u = 0;
    public short v = 0;
    public String w = "";
    public String x = "";

    static {
        c = !i.class.desiredAssertionStatus();
    }

    public String a() {
        return "WapGame.GPSInfo";
    }

    public String b() {
        return "com.qq.WapGame.GPSInfo";
    }

    public String m() {
        return this.q;
    }

    public void d(String str) {
        this.q = str;
    }

    public String n() {
        return this.r;
    }

    public void e(String str) {
        this.r = str;
    }

    public short o() {
        return this.s;
    }

    public void e(short s) {
        this.s = s;
    }

    public short p() {
        return this.t;
    }

    public void f(short s) {
        this.t = s;
    }

    public short q() {
        return this.u;
    }

    public void g(short s) {
        this.u = s;
    }

    public short r() {
        return this.v;
    }

    public void h(short s) {
        this.v = s;
    }

    public String s() {
        return this.w;
    }

    public void f(String str) {
        this.w = str;
    }

    public String t() {
        return this.x;
    }

    public void g(String str) {
        this.x = str;
    }

    public i() {
        d(this.q);
        e(this.r);
        e(this.s);
        f(this.t);
        g(this.u);
        h(this.v);
        f(this.w);
        g(this.x);
    }

    public i(String str, String str2, short s, short s2, short s3, short s4, String str3, String str4) {
        d(str);
        e(str2);
        e(s);
        f(s2);
        g(s3);
        h(s4);
        f(str3);
        g(str4);
    }

    public boolean equals(Object obj) {
        i iVar = (i) obj;
        return JceUtil.equals(this.q, iVar.q) && JceUtil.equals(this.r, iVar.r) && JceUtil.equals(this.s, iVar.s) && JceUtil.equals(this.t, iVar.t) && JceUtil.equals(this.u, iVar.u) && JceUtil.equals(this.v, iVar.v) && JceUtil.equals(this.w, iVar.w) && JceUtil.equals(this.x, iVar.x);
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
        jceOutputStream.write(this.q, 0);
        jceOutputStream.write(this.r, 1);
        jceOutputStream.write(this.s, 2);
        jceOutputStream.write(this.t, 3);
        jceOutputStream.write(this.u, 4);
        jceOutputStream.write(this.v, 5);
        jceOutputStream.write(this.w, 6);
        jceOutputStream.write(this.x, 7);
    }

    @Override // com.qq.taf.jce.JceStruct
    public void readFrom(JceInputStream jceInputStream) {
        d(jceInputStream.readString(0, true));
        e(jceInputStream.readString(1, true));
        e(jceInputStream.read(this.s, 2, true));
        f(jceInputStream.read(this.t, 3, true));
        g(jceInputStream.read(this.u, 4, true));
        h(jceInputStream.read(this.v, 5, true));
        f(jceInputStream.readString(6, true));
        g(jceInputStream.readString(7, true));
    }

    @Override // com.qq.taf.jce.JceStruct
    public void display(StringBuilder sb, int i) {
        JceDisplayer jceDisplayer = new JceDisplayer(sb, i);
        jceDisplayer.display(this.q, "iX");
        jceDisplayer.display(this.r, "iY");
        jceDisplayer.display(this.s, "iMCC");
        jceDisplayer.display(this.t, "iMNC");
        jceDisplayer.display(this.u, "iLAC");
        jceDisplayer.display(this.v, "iCellId");
        jceDisplayer.display(this.w, "clientIp");
        jceDisplayer.display(this.x, "wifiMac");
    }
}
