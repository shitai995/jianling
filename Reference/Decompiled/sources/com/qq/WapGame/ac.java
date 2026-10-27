package com.qq.WapGame;

import com.qq.taf.jce.JceDisplayer;
import com.qq.taf.jce.JceInputStream;
import com.qq.taf.jce.JceOutputStream;
import com.qq.taf.jce.JceStruct;
import com.qq.taf.jce.JceUtil;
import java.util.ArrayList;
import java.util.Collection;

/* JADX INFO: loaded from: classes.dex */
public final class ac extends JceStruct {
    static ArrayList ao;
    static ArrayList ap;
    static final /* synthetic */ boolean c;
    public int status = 0;
    public ArrayList ai = null;
    public ArrayList aj = null;
    public String ak = "";
    public String al = "";
    public String am = "";
    public String an = "";

    static {
        c = !ac.class.desiredAssertionStatus();
    }

    public String a() {
        return "WapGame.GetSMSDataSC";
    }

    public String b() {
        return "com.qq.WapGame.GetSMSDataSC";
    }

    public int getStatus() {
        return this.status;
    }

    public void f(int i) {
        this.status = i;
    }

    public ArrayList M() {
        return this.ai;
    }

    public void c(ArrayList arrayList) {
        this.ai = arrayList;
    }

    public ArrayList N() {
        return this.aj;
    }

    public void d(ArrayList arrayList) {
        this.aj = arrayList;
    }

    public String O() {
        return this.ak;
    }

    public void n(String str) {
        this.ak = str;
    }

    public String P() {
        return this.al;
    }

    public void o(String str) {
        this.al = str;
    }

    public String Q() {
        return this.am;
    }

    public void p(String str) {
        this.am = str;
    }

    public String R() {
        return this.an;
    }

    public void q(String str) {
        this.an = str;
    }

    public ac() {
        f(this.status);
        c(this.ai);
        d(this.aj);
        n(this.ak);
        o(this.al);
        p(this.am);
        q(this.an);
    }

    public ac(int i, ArrayList arrayList, ArrayList arrayList2, String str, String str2, String str3, String str4) {
        f(i);
        c(arrayList);
        d(arrayList2);
        n(str);
        o(str2);
        p(str3);
        q(str4);
    }

    public boolean equals(Object obj) {
        ac acVar = (ac) obj;
        return JceUtil.equals(this.status, acVar.status) && JceUtil.equals(this.ai, acVar.ai) && JceUtil.equals(this.aj, acVar.aj) && JceUtil.equals(this.ak, acVar.ak) && JceUtil.equals(this.al, acVar.al) && JceUtil.equals(this.am, acVar.am) && JceUtil.equals(this.an, acVar.an);
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
        jceOutputStream.write((Collection) this.ai, 1);
        jceOutputStream.write((Collection) this.aj, 2);
        jceOutputStream.write(this.ak, 3);
        jceOutputStream.write(this.al, 4);
        jceOutputStream.write(this.am, 5);
        jceOutputStream.write(this.an, 6);
    }

    @Override // com.qq.taf.jce.JceStruct
    public void readFrom(JceInputStream jceInputStream) {
        f(jceInputStream.read(this.status, 0, true));
        if (ao == null) {
            ao = new ArrayList();
            ao.add(new am());
        }
        c((ArrayList) jceInputStream.read((Object) ao, 1, true));
        if (ap == null) {
            ap = new ArrayList();
            ap.add(new bg());
        }
        d((ArrayList) jceInputStream.read((Object) ap, 2, true));
        n(jceInputStream.readString(3, true));
        o(jceInputStream.readString(4, true));
        p(jceInputStream.readString(5, true));
        q(jceInputStream.readString(6, true));
    }

    @Override // com.qq.taf.jce.JceStruct
    public void display(StringBuilder sb, int i) {
        JceDisplayer jceDisplayer = new JceDisplayer(sb, i);
        jceDisplayer.display(this.status, "status");
        jceDisplayer.display((Collection) this.ai, "mobileSMInfo");
        jceDisplayer.display((Collection) this.aj, "unicomSMInfo");
        jceDisplayer.display(this.ak, "levelChannelID");
        jceDisplayer.display(this.al, "leve2ChannelID");
        jceDisplayer.display(this.am, "insideChannelID");
        jceDisplayer.display(this.an, "channelNumber");
    }
}
