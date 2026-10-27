package com.qq.WapGame;

import com.qq.taf.jce.JceDisplayer;
import com.qq.taf.jce.JceInputStream;
import com.qq.taf.jce.JceOutputStream;
import com.qq.taf.jce.JceStruct;
import com.qq.taf.jce.JceUtil;
import java.util.HashMap;
import java.util.Map;

/* JADX INFO: loaded from: classes.dex */
public final class ag extends JceStruct {
    static Map av;
    static Map aw;
    static final /* synthetic */ boolean c;
    public short as = 0;
    public Map at = null;
    public Map au = null;

    static {
        c = !ag.class.desiredAssertionStatus();
    }

    public String a() {
        return "WapGame.GetUserSettingSC";
    }

    public String b() {
        return "com.qq.WapGame.GetUserSettingSC";
    }

    public short S() {
        return this.as;
    }

    public void i(short s) {
        this.as = s;
    }

    public Map T() {
        return this.at;
    }

    public void d(Map map) {
        this.at = map;
    }

    public Map U() {
        return this.au;
    }

    public void e(Map map) {
        this.au = map;
    }

    public ag() {
        i(this.as);
        d(this.at);
        e(this.au);
    }

    public ag(short s, Map map, Map map2) {
        i(s);
        d(map);
        e(map2);
    }

    public boolean equals(Object obj) {
        ag agVar = (ag) obj;
        return JceUtil.equals(this.as, agVar.as) && JceUtil.equals(this.at, agVar.at) && JceUtil.equals(this.au, agVar.au);
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
        jceOutputStream.write(this.at, 1);
        if (this.au != null) {
            jceOutputStream.write(this.au, 2);
        }
    }

    @Override // com.qq.taf.jce.JceStruct
    public void readFrom(JceInputStream jceInputStream) {
        i(jceInputStream.read(this.as, 0, true));
        if (av == null) {
            av = new HashMap();
            av.put("", "");
        }
        d((Map) jceInputStream.read((Object) av, 1, true));
        if (aw == null) {
            aw = new HashMap();
            aw.put("", new byte[]{0});
        }
        e((Map) jceInputStream.read((Object) aw, 2, false));
    }

    @Override // com.qq.taf.jce.JceStruct
    public void display(StringBuilder sb, int i) {
        JceDisplayer jceDisplayer = new JceDisplayer(sb, i);
        jceDisplayer.display(this.as, "rst");
        jceDisplayer.display(this.at, "setting");
        jceDisplayer.display(this.au, "b_setting");
    }
}
