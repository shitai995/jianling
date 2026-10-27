package com.qq.WapGame;

import com.qq.taf.jce.JceDisplayer;
import com.qq.taf.jce.JceInputStream;
import com.qq.taf.jce.JceOutputStream;
import com.qq.taf.jce.JceStruct;
import com.qq.taf.jce.JceUtil;
import java.util.ArrayList;
import java.util.Collection;

/* JADX INFO: loaded from: classes.dex */
public final class bs extends JceStruct {
    static ArrayList bB;
    static byte[] bC;
    static byte[] bD;
    static ArrayList bE;
    static final /* synthetic */ boolean c;
    public String C = "";
    public String bp = "";
    public String bq = "";
    public String z = "";
    public String br = "";
    public String bt = "";
    public int status = 0;
    public String bu = "";
    public String bv = "";
    public ArrayList bw = null;
    public byte[] bx = null;
    public byte[] by = null;
    public ArrayList bz = null;
    public int bA = 0;

    static {
        c = !bs.class.desiredAssertionStatus();
    }

    public String a() {
        return "WapGame.UpdateSC";
    }

    public String b() {
        return "com.qq.WapGame.UpdateSC";
    }

    public String x() {
        return this.C;
    }

    public void i(String str) {
        this.C = str;
    }

    public String getUrl() {
        return this.bp;
    }

    public void setUrl(String str) {
        this.bp = str;
    }

    public String al() {
        return this.bq;
    }

    public void w(String str) {
        this.bq = str;
    }

    public String u() {
        return this.z;
    }

    public void h(String str) {
        this.z = str;
    }

    public String am() {
        return this.br;
    }

    public void x(String str) {
        this.br = str;
    }

    public String an() {
        return this.bt;
    }

    public void y(String str) {
        this.bt = str;
    }

    public int getStatus() {
        return this.status;
    }

    public void f(int i) {
        this.status = i;
    }

    public String ao() {
        return this.bu;
    }

    public void z(String str) {
        this.bu = str;
    }

    public String ap() {
        return this.bv;
    }

    public void A(String str) {
        this.bv = str;
    }

    public ArrayList aq() {
        return this.bw;
    }

    public void i(ArrayList arrayList) {
        this.bw = arrayList;
    }

    public byte[] ar() {
        return this.bx;
    }

    public void g(byte[] bArr) {
        this.bx = bArr;
    }

    public byte[] as() {
        return this.by;
    }

    public void h(byte[] bArr) {
        this.by = bArr;
    }

    public ArrayList at() {
        return this.bz;
    }

    public void j(ArrayList arrayList) {
        this.bz = arrayList;
    }

    public int au() {
        return this.bA;
    }

    public void k(int i) {
        this.bA = i;
    }

    public bs() {
        i(this.C);
        setUrl(this.bp);
        w(this.bq);
        h(this.z);
        x(this.br);
        y(this.bt);
        f(this.status);
        z(this.bu);
        A(this.bv);
        i(this.bw);
        g(this.bx);
        h(this.by);
        j(this.bz);
        k(this.bA);
    }

    public bs(String str, String str2, String str3, String str4, String str5, String str6, int i, String str7, String str8, ArrayList arrayList, byte[] bArr, byte[] bArr2, ArrayList arrayList2, int i2) {
        i(str);
        setUrl(str2);
        w(str3);
        h(str4);
        x(str5);
        y(str6);
        f(i);
        z(str7);
        A(str8);
        i(arrayList);
        g(bArr);
        h(bArr2);
        j(arrayList2);
        k(i2);
    }

    public boolean equals(Object obj) {
        bs bsVar = (bs) obj;
        return JceUtil.equals(this.C, bsVar.C) && JceUtil.equals(this.bp, bsVar.bp) && JceUtil.equals(this.bq, bsVar.bq) && JceUtil.equals(this.z, bsVar.z) && JceUtil.equals(this.br, bsVar.br) && JceUtil.equals(this.bt, bsVar.bt) && JceUtil.equals(this.status, bsVar.status) && JceUtil.equals(this.bu, bsVar.bu) && JceUtil.equals(this.bv, bsVar.bv) && JceUtil.equals(this.bw, bsVar.bw) && JceUtil.equals(this.bx, bsVar.bx) && JceUtil.equals(this.by, bsVar.by) && JceUtil.equals(this.bz, bsVar.bz) && JceUtil.equals(this.bA, bsVar.bA);
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
        jceOutputStream.write(this.C, 0);
        jceOutputStream.write(this.bp, 1);
        jceOutputStream.write(this.bq, 2);
        jceOutputStream.write(this.z, 3);
        jceOutputStream.write(this.br, 4);
        if (this.bt != null) {
            jceOutputStream.write(this.bt, 5);
        }
        jceOutputStream.write(this.status, 6);
        if (this.bu != null) {
            jceOutputStream.write(this.bu, 7);
        }
        if (this.bv != null) {
            jceOutputStream.write(this.bv, 8);
        }
        if (this.bw != null) {
            jceOutputStream.write((Collection) this.bw, 9);
        }
        if (this.bx != null) {
            jceOutputStream.write(this.bx, 10);
        }
        if (this.by != null) {
            jceOutputStream.write(this.by, 11);
        }
        if (this.bz != null) {
            jceOutputStream.write((Collection) this.bz, 12);
        }
        jceOutputStream.write(this.bA, 13);
    }

    @Override // com.qq.taf.jce.JceStruct
    public void readFrom(JceInputStream jceInputStream) {
        i(jceInputStream.readString(0, true));
        setUrl(jceInputStream.readString(1, true));
        w(jceInputStream.readString(2, true));
        h(jceInputStream.readString(3, true));
        x(jceInputStream.readString(4, true));
        y(jceInputStream.readString(5, false));
        f(jceInputStream.read(this.status, 6, false));
        z(jceInputStream.readString(7, false));
        A(jceInputStream.readString(8, false));
        if (bB == null) {
            bB = new ArrayList();
            bB.add("");
        }
        i((ArrayList) jceInputStream.read((Object) bB, 9, false));
        if (bC == null) {
            bC = new byte[1];
            bC[0] = 0;
        }
        g(jceInputStream.read(bC, 10, false));
        if (bD == null) {
            bD = new byte[1];
            bD[0] = 0;
        }
        h(jceInputStream.read(bD, 11, false));
        if (bE == null) {
            bE = new ArrayList();
            bE.add(new byte[]{0});
        }
        j((ArrayList) jceInputStream.read((Object) bE, 12, false));
        k(jceInputStream.read(this.bA, 13, false));
    }

    @Override // com.qq.taf.jce.JceStruct
    public void display(StringBuilder sb, int i) {
        JceDisplayer jceDisplayer = new JceDisplayer(sb, i);
        jceDisplayer.display(this.C, "version");
        jceDisplayer.display(this.bp, "url");
        jceDisplayer.display(this.bq, "sid");
        jceDisplayer.display(this.z, "uin");
        jceDisplayer.display(this.br, "nickName");
        jceDisplayer.display(this.bt, "welcomeMsg");
        jceDisplayer.display(this.status, "status");
        jceDisplayer.display(this.bu, "softVersion");
        jceDisplayer.display(this.bv, "softUrl");
        jceDisplayer.display((Collection) this.bw, "softInfo");
        jceDisplayer.display(this.bx, "b_nickName");
        jceDisplayer.display(this.by, "b_welcomeMsg");
        jceDisplayer.display((Collection) this.bz, "b_softInfo");
        jceDisplayer.display(this.bA, "usrType");
    }
}
