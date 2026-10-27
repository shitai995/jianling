package com.qq.WapGame;

import com.qq.taf.jce.JceDisplayer;
import com.qq.taf.jce.JceInputStream;
import com.qq.taf.jce.JceOutputStream;
import com.qq.taf.jce.JceStruct;
import com.qq.taf.jce.JceUtil;

/* JADX INFO: loaded from: classes.dex */
public final class a extends JceStruct {
    static final /* synthetic */ boolean c;
    public String a = "";
    public String b = "";

    static {
        c = !a.class.desiredAssertionStatus();
    }

    public String a() {
        return "WapGame.AdvInfo";
    }

    public String b() {
        return "com.qq.WapGame.AdvInfo";
    }

    public String c() {
        return this.a;
    }

    public void a(String str) {
        this.a = str;
    }

    public String d() {
        return this.b;
    }

    public void b(String str) {
        this.b = str;
    }

    public a() {
        a(this.a);
        b(this.b);
    }

    public a(String str, String str2) {
        a(str);
        b(str2);
    }

    public boolean equals(Object obj) {
        a aVar = (a) obj;
        return JceUtil.equals(this.a, aVar.a) && JceUtil.equals(this.b, aVar.b);
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
        jceOutputStream.write(this.a, 0);
        jceOutputStream.write(this.b, 1);
    }

    @Override // com.qq.taf.jce.JceStruct
    public void readFrom(JceInputStream jceInputStream) {
        a(jceInputStream.readString(0, true));
        b(jceInputStream.readString(1, true));
    }

    @Override // com.qq.taf.jce.JceStruct
    public void display(StringBuilder sb, int i) {
        JceDisplayer jceDisplayer = new JceDisplayer(sb, i);
        jceDisplayer.display(this.a, "picUrl");
        jceDisplayer.display(this.b, "advUrl");
    }
}
