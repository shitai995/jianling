package com.qq.WapGame;

import com.qq.taf.jce.JceDisplayer;
import com.qq.taf.jce.JceInputStream;
import com.qq.taf.jce.JceOutputStream;
import com.qq.taf.jce.JceStruct;
import com.qq.taf.jce.JceUtil;
import java.util.HashMap;
import java.util.Map;

/* JADX INFO: loaded from: classes.dex */
public final class u extends JceStruct {
    static Map W;
    static Map X;
    static Map Y;
    static final /* synthetic */ boolean c;
    public Map T = null;
    public Map U = null;
    public int status = 0;
    public Map V = null;

    static {
        c = !u.class.desiredAssertionStatus();
    }

    public String a() {
        return "WapGame.GetMsgSC";
    }

    public String b() {
        return "com.qq.WapGame.GetMsgSC";
    }

    public Map F() {
        return this.T;
    }

    public void a(Map map) {
        this.T = map;
    }

    public Map G() {
        return this.U;
    }

    public void b(Map map) {
        this.U = map;
    }

    public int getStatus() {
        return this.status;
    }

    public void f(int i) {
        this.status = i;
    }

    public Map H() {
        return this.V;
    }

    public void c(Map map) {
        this.V = map;
    }

    public u() {
        a(this.T);
        b(this.U);
        f(this.status);
        c(this.V);
    }

    public u(Map map, Map map2, int i, Map map3) {
        a(map);
        b(map2);
        f(i);
        c(map3);
    }

    public boolean equals(Object obj) {
        u uVar = (u) obj;
        return JceUtil.equals(this.T, uVar.T) && JceUtil.equals(this.U, uVar.U) && JceUtil.equals(this.status, uVar.status) && JceUtil.equals(this.V, uVar.V);
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
        jceOutputStream.write(this.T, 0);
        jceOutputStream.write(this.U, 1);
        jceOutputStream.write(this.status, 2);
        if (this.V != null) {
            jceOutputStream.write(this.V, 3);
        }
    }

    @Override // com.qq.taf.jce.JceStruct
    public void readFrom(JceInputStream jceInputStream) {
        if (W == null) {
            W = new HashMap();
            W.put("", 0);
        }
        a((Map) jceInputStream.read((Object) W, 0, true));
        if (X == null) {
            X = new HashMap();
            X.put("", "");
        }
        b((Map) jceInputStream.read((Object) X, 1, true));
        f(jceInputStream.read(this.status, 2, true));
        if (Y == null) {
            Y = new HashMap();
            Y.put("", new byte[]{0});
        }
        c((Map) jceInputStream.read((Object) Y, 3, false));
    }

    @Override // com.qq.taf.jce.JceStruct
    public void display(StringBuilder sb, int i) {
        JceDisplayer jceDisplayer = new JceDisplayer(sb, i);
        jceDisplayer.display(this.T, "iValues");
        jceDisplayer.display(this.U, "sValues");
        jceDisplayer.display(this.status, "status");
        jceDisplayer.display(this.V, "b_sValues");
    }
}
