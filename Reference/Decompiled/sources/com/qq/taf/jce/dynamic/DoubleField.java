package com.qq.taf.jce.dynamic;

/* JADX INFO: loaded from: classes.dex */
public class DoubleField extends NumberField {
    private double data;

    DoubleField(double d, int i) {
        super(i);
        this.data = d;
    }

    @Override // com.qq.taf.jce.dynamic.NumberField
    public Number getNumber() {
        return Double.valueOf(this.data);
    }

    public double get() {
        return this.data;
    }

    public void set(double d) {
        this.data = d;
    }
}
