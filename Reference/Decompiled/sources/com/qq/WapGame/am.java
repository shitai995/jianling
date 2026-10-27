package com.qq.WapGame;

import com.qq.taf.jce.JceDisplayer;
import com.qq.taf.jce.JceInputStream;
import com.qq.taf.jce.JceOutputStream;
import com.qq.taf.jce.JceStruct;
import com.qq.taf.jce.JceUtil;

/* JADX INFO: loaded from: classes.dex */
public final class am extends JceStruct {
    static final /* synthetic */ boolean c;
    public int aA = 0;
    public String aB = "";
    public String aC = "";
    public int aD = 0;

    static {
        c = !am.class.desiredAssertionStatus();
    }

    public String a() {
        return "WapGame.MobileSMInfo";
    }

    public String b() {
        return "com.qq.WapGame.MobileSMInfo";
    }

    public int V() {
        return this.aA;
    }

    public void i(int i) {
        this.aA = i;
    }

    public String getCommand() {
        return this.aB;
    }

    public void setCommand(String str) {
        this.aB = str;
    }

    public String W() {
        return this.aC;
    }

    public void r(String str) {
        this.aC = str;
    }

    public int X() {
        return this.aD;
    }

    public void j(int i) {
        this.aD = i;
    }

    public am() {
        i(this.aA);
        setCommand(this.aB);
        r(this.aC);
        j(this.aD);
    }

    public am(int i, String str, String str2, int i2) {
        i(i);
        setCommand(str);
        r(str2);
        j(i2);
    }

    public boolean equals(Object obj) {
        am amVar = (am) obj;
        return JceUtil.equals(this.aA, amVar.aA) && JceUtil.equals(this.aB, amVar.aB) && JceUtil.equals(this.aC, amVar.aC) && JceUtil.equals(this.aD, amVar.aD);
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
        jceOutputStream.write(this.aA, 0);
        jceOutputStream.write(this.aB, 1);
        jceOutputStream.write(this.aC, 2);
        jceOutputStream.write(this.aD, 3);
    }

    @Override // com.qq.taf.jce.JceStruct
    public void readFrom(JceInputStream jceInputStream) {
        i(jceInputStream.read(this.aA, 0, true));
        setCommand(jceInputStream.readString(1, true));
        r(jceInputStream.readString(2, true));
        j(jceInputStream.read(this.aD, 3, true));
    }

    @Override // com.qq.taf.jce.JceStruct
    public void display(StringBuilder sb, int i) {
        JceDisplayer jceDisplayer = new JceDisplayer(sb, i);
        jceDisplayer.display(this.aA, "payId");
        jceDisplayer.display(this.aB, "command");
        jceDisplayer.display(this.aC, "destNo");
        jceDisplayer.display(this.aD, "cost");
    }
}
