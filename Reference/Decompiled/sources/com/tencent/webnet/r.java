package com.tencent.webnet;

import java.util.Vector;

/* JADX INFO: loaded from: classes.dex */
class r {
    protected short as = 0;
    protected Vector eI = null;
    protected Vector eJ = null;
    protected short eK = 0;
    protected String eL = null;

    r() {
    }

    protected boolean a(short s, short s2, String str) {
        if (this.eI == null) {
            this.eI = new Vector();
        }
        return this.eI.add(new s(this, s, s2, str));
    }

    protected boolean b(short s, short s2, String str) {
        if (this.eJ == null) {
            this.eJ = new Vector();
        }
        return this.eJ.add(new s(this, s, s2, str));
    }

    protected boolean O(String str) {
        this.eL = str;
        return true;
    }
}
