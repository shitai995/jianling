package com.qq.taf.jce.dynamic;

/* JADX INFO: loaded from: classes.dex */
public class StringField extends JceField {
    private String data;

    StringField(String str, int i) {
        super(i);
        this.data = str;
    }

    public String get() {
        return this.data;
    }

    public void set(String str) {
        this.data = str;
    }
}
