package com.tencent.webnet;

import java.util.Vector;

/* JADX INFO: loaded from: classes.dex */
public class u {
    protected int fg;
    protected Vector fh = new Vector();
    final /* synthetic */ t fi;

    protected u(t tVar) {
        this.fi = tVar;
    }

    protected void a(Object obj) {
        this.fh.add(obj);
    }

    protected Object p(int i) {
        if (i < 0 || i > this.fh.size()) {
            return null;
        }
        return this.fh.get(i);
    }
}
