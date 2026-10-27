package com.qq.WapGame;

import com.qq.taf.jce.JceDisplayer;
import com.qq.taf.jce.JceInputStream;
import com.qq.taf.jce.JceOutputStream;
import com.qq.taf.jce.JceStruct;
import com.qq.taf.jce.JceUtil;
import java.util.ArrayList;
import java.util.Collection;

/* JADX INFO: loaded from: classes.dex */
public final class bk extends JceStruct {
    static i aR;
    static ArrayList aZ;
    static ArrayList ba;
    static final /* synthetic */ boolean c;
    public String z = "";
    public int A = 0;
    public int B = 0;
    public String C = "";
    public String D = "";
    public ArrayList aW = null;
    public ArrayList aX = null;
    public String aY = "";
    public i aQ = null;
    public boolean K = true;

    static {
        c = !bk.class.desiredAssertionStatus();
    }

    public String a() {
        return "WapGame.UpdateDataCS";
    }

    public String b() {
        return "com.qq.WapGame.UpdateDataCS";
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

    public ArrayList ac() {
        return this.aW;
    }

    public void e(ArrayList arrayList) {
        this.aW = arrayList;
    }

    public ArrayList ad() {
        return this.aX;
    }

    public void f(ArrayList arrayList) {
        this.aX = arrayList;
    }

    public String ae() {
        return this.aY;
    }

    public void u(String str) {
        this.aY = str;
    }

    public i ab() {
        return this.aQ;
    }

    public void a(i iVar) {
        this.aQ = iVar;
    }

    public boolean B() {
        return this.K;
    }

    public void a(boolean z) {
        this.K = z;
    }

    public bk() {
        h(this.z);
        c(this.A);
        d(this.B);
        i(this.C);
        j(this.D);
        e(this.aW);
        f(this.aX);
        u(this.aY);
        a(this.aQ);
        a(this.K);
    }

    public bk(String str, int i, int i2, String str2, String str3, ArrayList arrayList, ArrayList arrayList2, String str4, i iVar, boolean z) {
        h(str);
        c(i);
        d(i2);
        i(str2);
        j(str3);
        e(arrayList);
        f(arrayList2);
        u(str4);
        a(iVar);
        a(z);
    }

    public boolean equals(Object obj) {
        bk bkVar = (bk) obj;
        return JceUtil.equals(this.z, bkVar.z) && JceUtil.equals(this.A, bkVar.A) && JceUtil.equals(this.B, bkVar.B) && JceUtil.equals(this.C, bkVar.C) && JceUtil.equals(this.D, bkVar.D) && JceUtil.equals(this.aW, bkVar.aW) && JceUtil.equals(this.aX, bkVar.aX) && JceUtil.equals(this.aY, bkVar.aY) && JceUtil.equals(this.aQ, bkVar.aQ) && JceUtil.equals(this.K, bkVar.K);
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
        jceOutputStream.write((Collection) this.aW, 5);
        jceOutputStream.write((Collection) this.aX, 6);
        jceOutputStream.write(this.aY, 7);
        jceOutputStream.write((JceStruct) this.aQ, 8);
        jceOutputStream.write(this.K, 9);
    }

    @Override // com.qq.taf.jce.JceStruct
    public void readFrom(JceInputStream jceInputStream) {
        h(jceInputStream.readString(0, true));
        c(jceInputStream.read(this.A, 1, true));
        d(jceInputStream.read(this.B, 2, true));
        i(jceInputStream.readString(3, true));
        j(jceInputStream.readString(4, true));
        if (aZ == null) {
            aZ = new ArrayList();
            aZ.add(new g());
        }
        e((ArrayList) jceInputStream.read((Object) aZ, 5, true));
        if (ba == null) {
            ba = new ArrayList();
            ba.add(new c());
        }
        f((ArrayList) jceInputStream.read((Object) ba, 6, true));
        u(jceInputStream.readString(7, true));
        if (aR == null) {
            aR = new i();
        }
        a((i) jceInputStream.read((JceStruct) aR, 8, true));
        a(jceInputStream.read(this.K, 9, false));
    }

    @Override // com.qq.taf.jce.JceStruct
    public void display(StringBuilder sb, int i) {
        JceDisplayer jceDisplayer = new JceDisplayer(sb, i);
        jceDisplayer.display(this.z, "uin");
        jceDisplayer.display(this.A, "cpId");
        jceDisplayer.display(this.B, "gameId");
        jceDisplayer.display(this.C, "version");
        jceDisplayer.display(this.D, "gameKey");
        jceDisplayer.display((Collection) this.aW, "scoreVector");
        jceDisplayer.display((Collection) this.aX, "achVector");
        jceDisplayer.display(this.aY, "privateData");
        jceDisplayer.display((JceStruct) this.aQ, "gpsInfo");
        jceDisplayer.display(this.K, "disableStrByte");
    }
}
