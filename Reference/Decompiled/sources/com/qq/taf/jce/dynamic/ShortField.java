package com.qq.taf.jce.dynamic;

/* JADX INFO: loaded from: classes.dex */
public final class ShortField extends NumberField {
    private short data;

    ShortField(short s, int i) {
        super(i);
        this.data = s;
    }

    @Override // com.qq.taf.jce.dynamic.NumberField
    public Number getNumber() {
        return Short.valueOf(this.data);
    }

    public short get() {
        return this.data;
    }

    public void set(short s) {
        this.data = s;
    }
}
