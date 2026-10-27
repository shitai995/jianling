package com.qq.WapGame;

import com.qq.taf.jce.JceDisplayer;
import com.qq.taf.jce.JceInputStream;
import com.qq.taf.jce.JceOutputStream;
import com.qq.taf.jce.JceStruct;
import com.qq.taf.jce.JceUtil;

/* JADX INFO: loaded from: classes.dex */
public final class bo extends JceStruct {
    static byte[] Q;
    static byte[] bk;
    static final /* synthetic */ boolean c;
    public String z = "";
    public int A = 0;
    public int B = 0;
    public String C = "";
    public String D = "";
    public byte[] bi = null;
    public String bj = "";
    public String M = "";
    public boolean K = true;
    public byte[] O = null;

    static {
        c = !bo.class.desiredAssertionStatus();
    }

    public String a() {
        return "WapGame.UpdateImgCS";
    }

    public String b() {
        return "com.qq.WapGame.UpdateImgCS";
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

    public byte[] ai() {
        return this.bi;
    }

    public void e(byte[] bArr) {
        this.bi = bArr;
    }

    public String aj() {
        return this.bj;
    }

    public void v(String str) {
        this.bj = str;
    }

    public String C() {
        return this.M;
    }

    public void l(String str) {
        this.M = str;
    }

    public boolean B() {
        return this.K;
    }

    public void a(boolean z) {
        this.K = z;
    }

    public byte[] E() {
        return this.O;
    }

    public void c(byte[] bArr) {
        this.O = bArr;
    }

    public bo() {
        h(this.z);
        c(this.A);
        d(this.B);
        i(this.C);
        j(this.D);
        e(this.bi);
        v(this.bj);
        l(this.M);
        a(this.K);
        c(this.O);
    }

    public bo(String str, int i, int i2, String str2, String str3, byte[] bArr, String str4, String str5, boolean z, byte[] bArr2) {
        h(str);
        c(i);
        d(i2);
        i(str2);
        j(str3);
        e(bArr);
        v(str4);
        l(str5);
        a(z);
        c(bArr2);
    }

    public boolean equals(Object obj) {
        bo boVar = (bo) obj;
        return JceUtil.equals(this.z, boVar.z) && JceUtil.equals(this.A, boVar.A) && JceUtil.equals(this.B, boVar.B) && JceUtil.equals(this.C, boVar.C) && JceUtil.equals(this.D, boVar.D) && JceUtil.equals(this.bi, boVar.bi) && JceUtil.equals(this.bj, boVar.bj) && JceUtil.equals(this.M, boVar.M) && JceUtil.equals(this.K, boVar.K) && JceUtil.equals(this.O, boVar.O);
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
        jceOutputStream.write(this.bi, 5);
        jceOutputStream.write(this.bj, 6);
        jceOutputStream.write(this.M, 7);
        jceOutputStream.write(this.K, 8);
        if (this.O != null) {
            jceOutputStream.write(this.O, 9);
        }
    }

    @Override // com.qq.taf.jce.JceStruct
    public void readFrom(JceInputStream jceInputStream) {
        h(jceInputStream.readString(0, true));
        c(jceInputStream.read(this.A, 1, true));
        d(jceInputStream.read(this.B, 2, true));
        i(jceInputStream.readString(3, true));
        j(jceInputStream.readString(4, true));
        if (bk == null) {
            bk = new byte[1];
            bk[0] = 0;
        }
        e(jceInputStream.read(bk, 5, true));
        v(jceInputStream.readString(6, true));
        l(jceInputStream.readString(7, true));
        a(jceInputStream.read(this.K, 8, false));
        if (Q == null) {
            Q = new byte[1];
            Q[0] = 0;
        }
        c(jceInputStream.read(Q, 9, false));
    }

    @Override // com.qq.taf.jce.JceStruct
    public void display(StringBuilder sb, int i) {
        JceDisplayer jceDisplayer = new JceDisplayer(sb, i);
        jceDisplayer.display(this.z, "uin");
        jceDisplayer.display(this.A, "cpId");
        jceDisplayer.display(this.B, "gameId");
        jceDisplayer.display(this.C, "version");
        jceDisplayer.display(this.D, "gameKey");
        jceDisplayer.display(this.bi, "imgData");
        jceDisplayer.display(this.bj, "imgType");
        jceDisplayer.display(this.M, "msg");
        jceDisplayer.display(this.K, "disableStrByte");
        jceDisplayer.display(this.O, "b_msg");
    }
}
