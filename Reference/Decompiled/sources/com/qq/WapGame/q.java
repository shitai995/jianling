package com.qq.WapGame;

import com.qq.taf.jce.JceDisplayer;
import com.qq.taf.jce.JceInputStream;
import com.qq.taf.jce.JceOutputStream;
import com.qq.taf.jce.JceStruct;
import com.qq.taf.jce.JceUtil;

/* JADX INFO: loaded from: classes.dex */
public final class q extends JceStruct {
    static byte[] P;
    static byte[] Q;
    static final /* synthetic */ boolean c;
    public int status = 0;
    public String value = "";
    public String M = "";
    public byte[] N = null;
    public byte[] O = null;

    static {
        c = !q.class.desiredAssertionStatus();
    }

    public String a() {
        return "WapGame.GetGamePrivateDataSC";
    }

    public String b() {
        return "com.qq.WapGame.GetGamePrivateDataSC";
    }

    public int getStatus() {
        return this.status;
    }

    public void f(int i) {
        this.status = i;
    }

    public String getValue() {
        return this.value;
    }

    public void setValue(String str) {
        this.value = str;
    }

    public String C() {
        return this.M;
    }

    public void l(String str) {
        this.M = str;
    }

    public byte[] D() {
        return this.N;
    }

    public void b(byte[] bArr) {
        this.N = bArr;
    }

    public byte[] E() {
        return this.O;
    }

    public void c(byte[] bArr) {
        this.O = bArr;
    }

    public q() {
        f(this.status);
        setValue(this.value);
        l(this.M);
        b(this.N);
        c(this.O);
    }

    public q(int i, String str, String str2, byte[] bArr, byte[] bArr2) {
        f(i);
        setValue(str);
        l(str2);
        b(bArr);
        c(bArr2);
    }

    public boolean equals(Object obj) {
        q qVar = (q) obj;
        return JceUtil.equals(this.status, qVar.status) && JceUtil.equals(this.value, qVar.value) && JceUtil.equals(this.M, qVar.M) && JceUtil.equals(this.N, qVar.N) && JceUtil.equals(this.O, qVar.O);
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
        jceOutputStream.write(this.value, 1);
        jceOutputStream.write(this.M, 2);
        if (this.N != null) {
            jceOutputStream.write(this.N, 3);
        }
        if (this.O != null) {
            jceOutputStream.write(this.O, 4);
        }
    }

    @Override // com.qq.taf.jce.JceStruct
    public void readFrom(JceInputStream jceInputStream) {
        f(jceInputStream.read(this.status, 0, true));
        setValue(jceInputStream.readString(1, true));
        l(jceInputStream.readString(2, true));
        if (P == null) {
            P = new byte[1];
            P[0] = 0;
        }
        b(jceInputStream.read(P, 3, false));
        if (Q == null) {
            Q = new byte[1];
            Q[0] = 0;
        }
        c(jceInputStream.read(Q, 4, false));
    }

    @Override // com.qq.taf.jce.JceStruct
    public void display(StringBuilder sb, int i) {
        JceDisplayer jceDisplayer = new JceDisplayer(sb, i);
        jceDisplayer.display(this.status, "status");
        jceDisplayer.display(this.value, "value");
        jceDisplayer.display(this.M, "msg");
        jceDisplayer.display(this.N, "b_value");
        jceDisplayer.display(this.O, "b_msg");
    }
}
