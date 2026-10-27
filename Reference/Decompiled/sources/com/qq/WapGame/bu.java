package com.qq.WapGame;

import com.qq.taf.jce.JceDisplayer;
import com.qq.taf.jce.JceInputStream;
import com.qq.taf.jce.JceOutputStream;
import com.qq.taf.jce.JceStruct;
import com.qq.taf.jce.JceUtil;
import java.util.HashMap;
import java.util.Map;

/* JADX INFO: loaded from: classes.dex */
public final class bu extends JceStruct {
    static Map av;
    static Map aw;
    static final /* synthetic */ boolean c;
    public String z = "";
    public int A = 0;
    public int B = 0;
    public String C = "";
    public String D = "";
    public String E = "";
    public Map at = null;
    public boolean K = true;
    public Map au = null;

    static {
        c = !bu.class.desiredAssertionStatus();
    }

    public String a() {
        return "WapGame.UpdateUserSettingCS";
    }

    public String b() {
        return "com.qq.WapGame.UpdateUserSettingCS";
    }

    public String u() {
        return this.z;
    }

    public void h(String str) {
        this.z = str;
    }

    public int v() {
        return this.A;
    }

    public void c(int i) {
        this.A = i;
    }

    public int w() {
        return this.B;
    }

    public void d(int i) {
        this.B = i;
    }

    public String x() {
        return this.C;
    }

    public void i(String str) {
        this.C = str;
    }

    public String y() {
        return this.D;
    }

    public void j(String str) {
        this.D = str;
    }

    public String getDeviceId() {
        return this.E;
    }

    public void k(String str) {
        this.E = str;
    }

    public Map T() {
        return this.at;
    }

    public void d(Map map) {
        this.at = map;
    }

    public boolean B() {
        return this.K;
    }

    public void a(boolean z) {
        this.K = z;
    }

    public Map U() {
        return this.au;
    }

    public void e(Map map) {
        this.au = map;
    }

    public bu() {
        h(this.z);
        c(this.A);
        d(this.B);
        i(this.C);
        j(this.D);
        k(this.E);
        d(this.at);
        a(this.K);
        e(this.au);
    }

    public bu(String str, int i, int i2, String str2, String str3, String str4, Map map, boolean z, Map map2) {
        h(str);
        c(i);
        d(i2);
        i(str2);
        j(str3);
        k(str4);
        d(map);
        a(z);
        e(map2);
    }

    public boolean equals(Object obj) {
        bu buVar = (bu) obj;
        return JceUtil.equals(this.z, buVar.z) && JceUtil.equals(this.A, buVar.A) && JceUtil.equals(this.B, buVar.B) && JceUtil.equals(this.C, buVar.C) && JceUtil.equals(this.D, buVar.D) && JceUtil.equals(this.E, buVar.E) && JceUtil.equals(this.at, buVar.at) && JceUtil.equals(this.K, buVar.K) && JceUtil.equals(this.au, buVar.au);
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
        jceOutputStream.write(this.z, 0);
        jceOutputStream.write(this.A, 1);
        jceOutputStream.write(this.B, 2);
        jceOutputStream.write(this.C, 3);
        jceOutputStream.write(this.D, 4);
        jceOutputStream.write(this.E, 5);
        jceOutputStream.write(this.at, 6);
        jceOutputStream.write(this.K, 7);
        if (this.au != null) {
            jceOutputStream.write(this.au, 8);
        }
    }

    @Override // com.qq.taf.jce.JceStruct
    public void readFrom(JceInputStream jceInputStream) {
        h(jceInputStream.readString(0, true));
        c(jceInputStream.read(this.A, 1, true));
        d(jceInputStream.read(this.B, 2, true));
        i(jceInputStream.readString(3, true));
        j(jceInputStream.readString(4, true));
        k(jceInputStream.readString(5, true));
        if (av == null) {
            av = new HashMap();
            av.put("", "");
        }
        d((Map) jceInputStream.read((Object) av, 6, true));
        a(jceInputStream.read(this.K, 7, false));
        if (aw == null) {
            aw = new HashMap();
            aw.put("", new byte[]{0});
        }
        e((Map) jceInputStream.read((Object) aw, 8, false));
    }

    @Override // com.qq.taf.jce.JceStruct
    public void display(StringBuilder sb, int i) {
        JceDisplayer jceDisplayer = new JceDisplayer(sb, i);
        jceDisplayer.display(this.z, "uin");
        jceDisplayer.display(this.A, "cpId");
        jceDisplayer.display(this.B, "gameId");
        jceDisplayer.display(this.C, "version");
        jceDisplayer.display(this.D, "gameKey");
        jceDisplayer.display(this.E, "deviceId");
        jceDisplayer.display(this.at, "setting");
        jceDisplayer.display(this.K, "disableStrByte");
        jceDisplayer.display(this.au, "b_setting");
    }
}
