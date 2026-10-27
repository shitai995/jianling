package com.qq.WapGame;

import com.qq.taf.jce.JceDisplayer;
import com.qq.taf.jce.JceInputStream;
import com.qq.taf.jce.JceOutputStream;
import com.qq.taf.jce.JceStruct;
import com.qq.taf.jce.JceUtil;

/* JADX INFO: loaded from: classes.dex */
public final class bg extends JceStruct {
    static final /* synthetic */ boolean c;
    public int aD = 0;
    public String aC = "";
    public String aB = "";

    static {
        c = !bg.class.desiredAssertionStatus();
    }

    public String a() {
        return "WapGame.UnicomSMInfo";
    }

    public String b() {
        return "com.qq.WapGame.UnicomSMInfo";
    }

    public int X() {
        return this.aD;
    }

    public void j(int i) {
        this.aD = i;
    }

    public String W() {
        return this.aC;
    }

    public void r(String str) {
        this.aC = str;
    }

    public String getCommand() {
        return this.aB;
    }

    public void setCommand(String str) {
        this.aB = str;
    }

    public bg() {
        j(this.aD);
        r(this.aC);
        setCommand(this.aB);
    }

    public bg(int i, String str, String str2) {
        j(i);
        r(str);
        setCommand(str2);
    }

    public boolean equals(Object obj) {
        bg bgVar = (bg) obj;
        return JceUtil.equals(this.aD, bgVar.aD) && JceUtil.equals(this.aC, bgVar.aC) && JceUtil.equals(this.aB, bgVar.aB);
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
        jceOutputStream.write(this.aD, 0);
        jceOutputStream.write(this.aC, 1);
        jceOutputStream.write(this.aB, 2);
    }

    @Override // com.qq.taf.jce.JceStruct
    public void readFrom(JceInputStream jceInputStream) {
        j(jceInputStream.read(this.aD, 0, true));
        r(jceInputStream.readString(1, true));
        setCommand(jceInputStream.readString(2, true));
    }

    @Override // com.qq.taf.jce.JceStruct
    public void display(StringBuilder sb, int i) {
        JceDisplayer jceDisplayer = new JceDisplayer(sb, i);
        jceDisplayer.display(this.aD, "cost");
        jceDisplayer.display(this.aC, "destNo");
        jceDisplayer.display(this.aB, "command");
    }
}
