package com.qq.WapGame;

import com.qq.taf.jce.JceDisplayer;
import com.qq.taf.jce.JceInputStream;
import com.qq.taf.jce.JceOutputStream;
import com.qq.taf.jce.JceStruct;
import com.qq.taf.jce.JceUtil;

/* JADX INFO: loaded from: classes.dex */
public final class g extends JceStruct {
    static final /* synthetic */ boolean c;
    public short n = 0;
    public int o = 0;
    public int f = 0;

    static {
        c = !g.class.desiredAssertionStatus();
    }

    public String a() {
        return "WapGame.CScoreInfo";
    }

    public String b() {
        return "com.qq.WapGame.CScoreInfo";
    }

    public short k() {
        return this.n;
    }

    public void d(short s) {
        this.n = s;
    }

    public int l() {
        return this.o;
    }

    public void b(int i) {
        this.o = i;
    }

    public int f() {
        return this.f;
    }

    public void a(int i) {
        this.f = i;
    }

    public g() {
        d(this.n);
        b(this.o);
        a(this.f);
    }

    public g(short s, int i, int i2) {
        d(s);
        b(i);
        a(i2);
    }

    public boolean equals(Object obj) {
        g gVar = (g) obj;
        return JceUtil.equals(this.n, gVar.n) && JceUtil.equals(this.o, gVar.o) && JceUtil.equals(this.f, gVar.f);
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
        jceOutputStream.write(this.n, 0);
        jceOutputStream.write(this.o, 1);
        jceOutputStream.write(this.f, 2);
    }

    @Override // com.qq.taf.jce.JceStruct
    public void readFrom(JceInputStream jceInputStream) {
        d(jceInputStream.read(this.n, 0, true));
        b(jceInputStream.read(this.o, 1, true));
        a(jceInputStream.read(this.f, 2, true));
    }

    @Override // com.qq.taf.jce.JceStruct
    public void display(StringBuilder sb, int i) {
        JceDisplayer jceDisplayer = new JceDisplayer(sb, i);
        jceDisplayer.display(this.n, "tag");
        jceDisplayer.display(this.o, "score");
        jceDisplayer.display(this.f, "getTime");
    }
}
