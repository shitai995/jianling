package com.qq.WapGame;

import com.qq.taf.jce.JceDisplayer;
import com.qq.taf.jce.JceInputStream;
import com.qq.taf.jce.JceOutputStream;
import com.qq.taf.jce.JceStruct;
import com.qq.taf.jce.JceUtil;
import java.util.ArrayList;
import java.util.Collection;

/* JADX INFO: loaded from: classes.dex */
public final class y extends JceStruct {
    static ArrayList af;
    static final /* synthetic */ boolean c;
    public int status = 0;
    public String ac = "";
    public ArrayList ad = null;
    public int ae = 0;

    static {
        c = !y.class.desiredAssertionStatus();
    }

    public String a() {
        return "WapGame.GetPkgInfoListSC";
    }

    public String b() {
        return "com.qq.WapGame.GetPkgInfoListSC";
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

    public ArrayList K() {
        return this.ad;
    }

    public void b(ArrayList arrayList) {
        this.ad = arrayList;
    }

    public int L() {
        return this.ae;
    }

    public void h(int i) {
        this.ae = i;
    }

    public y() {
        f(this.status);
        m(this.ac);
        b(this.ad);
        h(this.ae);
    }

    public y(int i, String str, ArrayList arrayList, int i2) {
        f(i);
        m(str);
        b(arrayList);
        h(i2);
    }

    public boolean equals(Object obj) {
        y yVar = (y) obj;
        return JceUtil.equals(this.status, yVar.status) && JceUtil.equals(this.ac, yVar.ac) && JceUtil.equals(this.ad, yVar.ad) && JceUtil.equals(this.ae, yVar.ae);
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
        jceOutputStream.write(this.ac, 1);
        jceOutputStream.write((Collection) this.ad, 2);
        jceOutputStream.write(this.ae, 3);
    }

    @Override // com.qq.taf.jce.JceStruct
    public void readFrom(JceInputStream jceInputStream) {
        f(jceInputStream.read(this.status, 0, true));
        m(jceInputStream.readString(1, true));
        if (af == null) {
            af = new ArrayList();
            af.add(new ao());
        }
        b((ArrayList) jceInputStream.read((Object) af, 2, true));
        h(jceInputStream.read(this.ae, 3, true));
    }

    @Override // com.qq.taf.jce.JceStruct
    public void display(StringBuilder sb, int i) {
        JceDisplayer jceDisplayer = new JceDisplayer(sb, i);
        jceDisplayer.display(this.status, "status");
        jceDisplayer.display(this.ac, "errMsg");
        jceDisplayer.display((Collection) this.ad, "pkgInfo");
        jceDisplayer.display(this.ae, "systemTime");
    }
}
