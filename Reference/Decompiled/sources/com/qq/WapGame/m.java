package com.qq.WapGame;

import com.qq.taf.jce.JceDisplayer;
import com.qq.taf.jce.JceInputStream;
import com.qq.taf.jce.JceOutputStream;
import com.qq.taf.jce.JceStruct;
import com.qq.taf.jce.JceUtil;
import java.util.ArrayList;
import java.util.Collection;

/* JADX INFO: loaded from: classes.dex */
public final class m extends JceStruct {
    static ArrayList I;
    static final /* synthetic */ boolean c;
    public int status = 0;
    public int F = 0;
    public ArrayList H = null;

    static {
        c = !m.class.desiredAssertionStatus();
    }

    public String a() {
        return "WapGame.GetAdvListSC";
    }

    public String b() {
        return "com.qq.WapGame.GetAdvListSC";
    }

    public int getStatus() {
        return this.status;
    }

    public void f(int i) {
        this.status = i;
    }

    public int z() {
        return this.F;
    }

    public void e(int i) {
        this.F = i;
    }

    public ArrayList A() {
        return this.H;
    }

    public void a(ArrayList arrayList) {
        this.H = arrayList;
    }

    public m() {
        f(this.status);
        e(this.F);
        a(this.H);
    }

    public m(int i, int i2, ArrayList arrayList) {
        f(i);
        e(i2);
        a(arrayList);
    }

    public boolean equals(Object obj) {
        m mVar = (m) obj;
        return JceUtil.equals(this.status, mVar.status) && JceUtil.equals(this.F, mVar.F) && JceUtil.equals(this.H, mVar.H);
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
        jceOutputStream.write(this.F, 1);
        jceOutputStream.write((Collection) this.H, 2);
    }

    @Override // com.qq.taf.jce.JceStruct
    public void readFrom(JceInputStream jceInputStream) {
        f(jceInputStream.read(this.status, 0, true));
        e(jceInputStream.read(this.F, 1, true));
        if (I == null) {
            I = new ArrayList();
            I.add(new a());
        }
        a((ArrayList) jceInputStream.read((Object) I, 2, true));
    }

    @Override // com.qq.taf.jce.JceStruct
    public void display(StringBuilder sb, int i) {
        JceDisplayer jceDisplayer = new JceDisplayer(sb, i);
        jceDisplayer.display(this.status, "status");
        jceDisplayer.display(this.F, "advVersion");
        jceDisplayer.display((Collection) this.H, "advInfo");
    }
}
