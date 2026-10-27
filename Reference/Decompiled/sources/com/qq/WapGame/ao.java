package com.qq.WapGame;

import com.qq.taf.jce.JceDisplayer;
import com.qq.taf.jce.JceInputStream;
import com.qq.taf.jce.JceOutputStream;
import com.qq.taf.jce.JceStruct;
import com.qq.taf.jce.JceUtil;

/* JADX INFO: loaded from: classes.dex */
public final class ao extends JceStruct {
    static final /* synthetic */ boolean c;
    public String aF = "";

    static {
        c = !ao.class.desiredAssertionStatus();
    }

    public String a() {
        return "WapGame.PkgInfo";
    }

    public String b() {
        return "com.qq.WapGame.PkgInfo";
    }

    public String Y() {
        return this.aF;
    }

    public void s(String str) {
        this.aF = str;
    }

    public ao() {
        s(this.aF);
    }

    public ao(String str) {
        s(str);
    }

    public boolean equals(Object obj) {
        return JceUtil.equals(this.aF, ((ao) obj).aF);
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
        jceOutputStream.write(this.aF, 0);
    }

    @Override // com.qq.taf.jce.JceStruct
    public void readFrom(JceInputStream jceInputStream) {
        s(jceInputStream.readString(0, true));
    }

    @Override // com.qq.taf.jce.JceStruct
    public void display(StringBuilder sb, int i) {
        new JceDisplayer(sb, i).display(this.aF, "pkgName");
    }
}
