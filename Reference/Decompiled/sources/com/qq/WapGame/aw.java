package com.qq.WapGame;

import com.qq.taf.jce.JceDisplayer;
import com.qq.taf.jce.JceInputStream;
import com.qq.taf.jce.JceOutputStream;
import com.qq.taf.jce.JceStruct;
import com.qq.taf.jce.JceUtil;

/* JADX INFO: loaded from: classes.dex */
public final class aw extends JceStruct {
    static final /* synthetic */ boolean c;
    public int status = 0;

    static {
        c = !aw.class.desiredAssertionStatus();
    }

    public String a() {
        return "WapGame.ReportInstallSC";
    }

    public String b() {
        return "com.qq.WapGame.ReportInstallSC";
    }

    public int getStatus() {
        return this.status;
    }

    public void f(int i) {
        this.status = i;
    }

    public aw() {
        f(this.status);
    }

    public aw(int i) {
        f(i);
    }

    public boolean equals(Object obj) {
        return JceUtil.equals(this.status, ((aw) obj).status);
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
    }

    @Override // com.qq.taf.jce.JceStruct
    public void readFrom(JceInputStream jceInputStream) {
        f(jceInputStream.read(this.status, 0, true));
    }

    @Override // com.qq.taf.jce.JceStruct
    public void display(StringBuilder sb, int i) {
        new JceDisplayer(sb, i).display(this.status, "status");
    }
}
