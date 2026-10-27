package com.qq.WapGame;

import com.qq.taf.jce.JceDisplayer;
import com.qq.taf.jce.JceInputStream;
import com.qq.taf.jce.JceOutputStream;
import com.qq.taf.jce.JceStruct;
import com.qq.taf.jce.JceUtil;

/* JADX INFO: loaded from: classes.dex */
public final class c extends JceStruct {
    static final /* synthetic */ boolean c;
    public short e = 0;
    public int f = 0;

    static {
        c = !c.class.desiredAssertionStatus();
    }

    public String a() {
        return "WapGame.CAchInfo";
    }

    public String b() {
        return "com.qq.WapGame.CAchInfo";
    }

    public short e() {
        return this.e;
    }

    public void a(short s) {
        this.e = s;
    }

    public int f() {
        return this.f;
    }

    public void a(int i) {
        this.f = i;
    }

    public c() {
        a(this.e);
        a(this.f);
    }

    public c(short s, int i) {
        a(s);
        a(i);
    }

    public boolean equals(Object obj) {
        c cVar = (c) obj;
        return JceUtil.equals(this.e, cVar.e) && JceUtil.equals(this.f, cVar.f);
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
        jceOutputStream.write(this.e, 0);
        jceOutputStream.write(this.f, 1);
    }

    @Override // com.qq.taf.jce.JceStruct
    public void readFrom(JceInputStream jceInputStream) {
        a(jceInputStream.read(this.e, 0, true));
        a(jceInputStream.read(this.f, 1, true));
    }

    @Override // com.qq.taf.jce.JceStruct
    public void display(StringBuilder sb, int i) {
        JceDisplayer jceDisplayer = new JceDisplayer(sb, i);
        jceDisplayer.display(this.e, "achId");
        jceDisplayer.display(this.f, "getTime");
    }
}
