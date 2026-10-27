package com.qq.WapGame;

import com.qq.taf.jce.JceDisplayer;
import com.qq.taf.jce.JceInputStream;
import com.qq.taf.jce.JceOutputStream;
import com.qq.taf.jce.JceStruct;
import com.qq.taf.jce.JceUtil;

/* JADX INFO: loaded from: classes.dex */
public final class e extends JceStruct {
    static final /* synthetic */ boolean c;
    static byte[] l;
    public short h = 0;
    public short i = 0;
    public String j = "";
    public byte[] k = null;

    static {
        c = !e.class.desiredAssertionStatus();
    }

    public String a() {
        return "WapGame.CRst";
    }

    public String b() {
        return "com.qq.WapGame.CRst";
    }

    public short g() {
        return this.h;
    }

    public void b(short s) {
        this.h = s;
    }

    public short h() {
        return this.i;
    }

    public void c(short s) {
        this.i = s;
    }

    public String i() {
        return this.j;
    }

    public void c(String str) {
        this.j = str;
    }

    public byte[] j() {
        return this.k;
    }

    public void a(byte[] bArr) {
        this.k = bArr;
    }

    public e() {
        b(this.h);
        c(this.i);
        c(this.j);
        a(this.k);
    }

    public e(short s, short s2, String str, byte[] bArr) {
        b(s);
        c(s2);
        c(str);
        a(bArr);
    }

    public boolean equals(Object obj) {
        e eVar = (e) obj;
        return JceUtil.equals(this.h, eVar.h) && JceUtil.equals(this.i, eVar.i) && JceUtil.equals(this.j, eVar.j) && JceUtil.equals(this.k, eVar.k);
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
        jceOutputStream.write(this.h, 0);
        jceOutputStream.write(this.i, 1);
        jceOutputStream.write(this.j, 2);
        if (this.k != null) {
            jceOutputStream.write(this.k, 3);
        }
    }

    @Override // com.qq.taf.jce.JceStruct
    public void readFrom(JceInputStream jceInputStream) {
        b(jceInputStream.read(this.h, 0, true));
        c(jceInputStream.read(this.i, 1, true));
        c(jceInputStream.readString(2, true));
        if (l == null) {
            l = new byte[1];
            l[0] = 0;
        }
        a(jceInputStream.read(l, 3, false));
    }

    @Override // com.qq.taf.jce.JceStruct
    public void display(StringBuilder sb, int i) {
        JceDisplayer jceDisplayer = new JceDisplayer(sb, i);
        jceDisplayer.display(this.h, "id");
        jceDisplayer.display(this.i, "stat");
        jceDisplayer.display(this.j, "mgs");
        jceDisplayer.display(this.k, "b_mgs");
    }
}
