package com.qq.taf.jce.dynamic;

/* JADX INFO: loaded from: classes.dex */
public final class ByteField extends NumberField {
    private byte data;

    ByteField(byte b, int i) {
        super(i);
        this.data = b;
    }

    @Override // com.qq.taf.jce.dynamic.NumberField
    public Number getNumber() {
        return Byte.valueOf(this.data);
    }

    public byte get() {
        return this.data;
    }

    public void set(byte b) {
        this.data = b;
    }
}
