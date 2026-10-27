package com.qq.WapGame;

import com.qq.taf.jce.JceDisplayer;
import com.qq.taf.jce.JceInputStream;
import com.qq.taf.jce.JceOutputStream;
import com.qq.taf.jce.JceStruct;
import com.qq.taf.jce.JceUtil;
import java.util.ArrayList;
import java.util.Collection;

/* JADX INFO: loaded from: classes.dex */
public final class bm extends JceStruct {
    static ArrayList bf;
    static ArrayList bg;
    static final /* synthetic */ boolean c;
    public short as = 0;
    public ArrayList bc = null;
    public ArrayList bd = null;
    public short be = 0;

    static {
        c = !bm.class.desiredAssertionStatus();
    }

    public String a() {
        return "WapGame.UpdateDataSC";
    }

    public String b() {
        return "com.qq.WapGame.UpdateDataSC";
    }

    public short S() {
        return this.as;
    }

    public void i(short s) {
        this.as = s;
    }

    public ArrayList af() {
        return this.bc;
    }

    public void g(ArrayList arrayList) {
        this.bc = arrayList;
    }

    public ArrayList ag() {
        return this.bd;
    }

    public void h(ArrayList arrayList) {
        this.bd = arrayList;
    }

    public short ah() {
        return this.be;
    }

    public void j(short s) {
        this.be = s;
    }

    public bm() {
        i(this.as);
        g(this.bc);
        h(this.bd);
        j(this.be);
    }

    public bm(short s, ArrayList arrayList, ArrayList arrayList2, short s2) {
        i(s);
        g(arrayList);
        h(arrayList2);
        j(s2);
    }

    public boolean equals(Object obj) {
        bm bmVar = (bm) obj;
        return JceUtil.equals(this.as, bmVar.as) && JceUtil.equals(this.bc, bmVar.bc) && JceUtil.equals(this.bd, bmVar.bd) && JceUtil.equals(this.be, bmVar.be);
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
        jceOutputStream.write((Collection) this.bc, 1);
        jceOutputStream.write((Collection) this.bd, 2);
        jceOutputStream.write(this.be, 3);
    }

    @Override // com.qq.taf.jce.JceStruct
    public void readFrom(JceInputStream jceInputStream) {
        i(jceInputStream.read(this.as, 0, true));
        if (bf == null) {
            bf = new ArrayList();
            bf.add(new e());
        }
        g((ArrayList) jceInputStream.read((Object) bf, 1, true));
        if (bg == null) {
            bg = new ArrayList();
            bg.add(new e());
        }
        h((ArrayList) jceInputStream.read((Object) bg, 2, true));
        j(jceInputStream.read(this.be, 3, true));
    }

    @Override // com.qq.taf.jce.JceStruct
    public void display(StringBuilder sb, int i) {
        JceDisplayer jceDisplayer = new JceDisplayer(sb, i);
        jceDisplayer.display(this.as, "rst");
        jceDisplayer.display((Collection) this.bc, "scoreRstVector");
        jceDisplayer.display((Collection) this.bd, "achRstVector");
        jceDisplayer.display(this.be, "privateDataRst");
    }
}
