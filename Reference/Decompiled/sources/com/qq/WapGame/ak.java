package com.qq.WapGame;

import com.qq.taf.jce.JceDisplayer;
import com.qq.taf.jce.JceInputStream;
import com.qq.taf.jce.JceOutputStream;
import com.qq.taf.jce.JceStruct;
import com.qq.taf.jce.JceUtil;

/* JADX INFO: loaded from: classes.dex */
public final class ak extends JceStruct {
    static final /* synthetic */ boolean c;
    public short as = 0;

    static {
        c = !ak.class.desiredAssertionStatus();
    }

    public String a() {
        return "WapGame.LogoutSC";
    }

    public String b() {
        return "com.qq.WapGame.LogoutSC";
    }

    public short S() {
        return this.as;
    }

    public void i(short s) {
        this.as = s;
    }

    public ak() {
        i(this.as);
    }

    public ak(short s) {
        i(s);
    }

    public boolean equals(Object obj) {
        return JceUtil.equals(this.as, ((ak) obj).as);
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
        jceOutputStream.write(this.as, 0);
    }

    @Override // com.qq.taf.jce.JceStruct
    public void readFrom(JceInputStream jceInputStream) {
        i(jceInputStream.read(this.as, 0, true));
    }

    @Override // com.qq.taf.jce.JceStruct
    public void display(StringBuilder sb, int i) {
        new JceDisplayer(sb, i).display(this.as, "rst");
    }
}
