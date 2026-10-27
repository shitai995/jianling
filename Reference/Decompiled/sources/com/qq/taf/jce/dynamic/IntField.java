package com.qq.taf.jce.dynamic;

/* JADX INFO: loaded from: classes.dex */
public class IntField extends NumberField {
    private int data;

    IntField(int i, int i2) {
        super(i2);
        this.data = i;
    }

    public int get() {
        return this.data;
    }

    public void set(int i) {
        this.data = i;
    }

    @Override // com.qq.taf.jce.dynamic.NumberField
    public Number getNumber() {
        return Integer.valueOf(this.data);
    }
}
