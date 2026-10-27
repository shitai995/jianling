package com.qq.taf.jce.dynamic;

/* JADX INFO: loaded from: classes.dex */
public final class ZeroField extends NumberField {
    ZeroField(int i) {
        super(i);
    }

    @Override // com.qq.taf.jce.dynamic.NumberField
    public Number getNumber() {
        return 0;
    }

    @Override // com.qq.taf.jce.dynamic.NumberField
    public byte byteValue() {
        return (byte) 0;
    }

    @Override // com.qq.taf.jce.dynamic.NumberField
    public double doubleValue() {
        return 0.0d;
    }

    @Override // com.qq.taf.jce.dynamic.NumberField
    public float floatValue() {
        return 0.0f;
    }

    @Override // com.qq.taf.jce.dynamic.NumberField
    public int intValue() {
        return 0;
    }

    @Override // com.qq.taf.jce.dynamic.NumberField
    public long longValue() {
        return 0L;
    }

    @Override // com.qq.taf.jce.dynamic.NumberField
    public short shortValue() {
        return (short) 0;
    }
}
