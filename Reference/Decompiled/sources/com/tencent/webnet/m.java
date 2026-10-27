package com.tencent.webnet;

import java.io.Serializable;
import java.util.HashMap;
import java.util.Map;

/* JADX INFO: loaded from: classes.dex */
class m implements Serializable {
    private static final String ek = "mysql";
    private static m eq = null;
    protected static final String eu = "sid";
    protected static final String ev = "uin";
    protected static final String ew = "nickName";
    protected static final String ex = "isUpdate";
    protected static final String ey = "deviceid";
    private static final long serialVersionUID = 1;
    private Map er;
    private v es;
    private boolean et = false;

    protected static m aZ() {
        if (eq == null) {
            m mVar = (m) d.C(ek);
            eq = mVar;
            if (mVar == null) {
                eq = new m();
            }
        }
        if (eq == null) {
            l.a("失败是否忘记WRITE_EXTERNAL_STORAGE权限!", null);
            if (b.cB) {
                l.a("sql 创建私有数据文件 错误, 是否忘加WRITE_EXTERNAL_STORAGE权限! ", null);
            } else {
                l.a("create mysql file error, uses permission WRITE_EXTERNAL_STORAGE", null);
            }
        }
        return eq;
    }

    protected void ba() {
        bb();
        eq = null;
    }

    protected void b(String str, Object obj) {
        synchronized (this) {
            if (this.er == null) {
                this.er = new HashMap();
            }
            this.er.put(str, obj);
        }
        bb();
    }

    protected Object M(String str) {
        synchronized (this) {
            if (this.er == null || !this.er.containsKey(str)) {
                return null;
            }
            return this.er.get(str);
        }
    }

    protected boolean bb() {
        boolean zA;
        synchronized (this) {
            zA = d.a(ek, this);
        }
        return zA;
    }

    protected boolean bc() {
        return d.E(ek);
    }

    protected v bd() {
        v vVar;
        synchronized (this) {
            vVar = b.cw ? this.es : null;
        }
        return vVar;
    }

    protected void be() {
        synchronized (this) {
            if (b.cw) {
                this.es = new v();
                bb();
            }
        }
    }

    protected void a(int i, int i2) {
        synchronized (this) {
            if (b.cw) {
                if (this.es == null) {
                    this.es = new v();
                }
                if (this.es.a((short) i, i2)) {
                    bb();
                }
            }
        }
    }

    protected void m(int i) {
        synchronized (this) {
            if (b.cw) {
                if (this.es == null) {
                    this.es = new v();
                }
                if (this.es.k((short) i)) {
                    bb();
                }
            }
        }
    }

    protected void N(String str) {
        synchronized (this) {
            if (b.cw) {
                if (this.es == null) {
                    this.es = new v();
                }
                if (this.es.Q(str)) {
                    bb();
                }
            }
        }
    }

    protected boolean bf() {
        boolean z;
        synchronized (this) {
            z = this.et;
        }
        return z;
    }

    protected void b(boolean z) {
        synchronized (this) {
            this.et = z;
        }
    }

    protected boolean bg() {
        Object objM = M(eu);
        return (objM == null || ((String) objM).equals("")) ? false : true;
    }

    private m() {
        this.er = null;
        this.es = null;
        this.er = new HashMap();
        if (b.cw) {
            this.es = new v();
        }
        b(ex, "false");
        bb();
    }
}
