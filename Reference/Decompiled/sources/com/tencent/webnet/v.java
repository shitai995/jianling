package com.tencent.webnet;

import java.io.Serializable;
import java.util.Vector;

/* JADX INFO: loaded from: classes.dex */
class v implements Serializable {
    private static final long serialVersionUID = 1;
    protected Vector fj = null;
    protected Vector fk = null;
    protected String aY = "";

    v() {
    }

    protected boolean a(short s, int i) {
        if (this.fj == null) {
            this.fj = new Vector();
        }
        for (int i2 = 0; i2 < this.fj.size(); i2++) {
            x xVar = (x) this.fj.get(i2);
            if (xVar.n == s) {
                if (i <= xVar.o) {
                    return false;
                }
                this.fj.set(i2, new x(this, s, i));
                return true;
            }
        }
        return this.fj.add(new x(this, s, i));
    }

    protected boolean k(short s) {
        if (this.fk == null) {
            this.fk = new Vector();
        }
        for (int i = 0; i < this.fk.size(); i++) {
            if (((w) this.fk.get(i)).e == s) {
                return false;
            }
        }
        return this.fk.add(new w(this, s));
    }

    protected boolean Q(String str) {
        this.aY = str;
        return true;
    }
}
