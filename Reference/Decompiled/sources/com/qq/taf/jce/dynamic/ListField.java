package com.qq.taf.jce.dynamic;

/* JADX INFO: loaded from: classes.dex */
public final class ListField extends JceField {
    private JceField[] data;

    ListField(JceField[] jceFieldArr, int i) {
        super(i);
        this.data = jceFieldArr;
    }

    public JceField[] get() {
        return this.data;
    }

    public JceField get(int i) {
        return this.data[i];
    }

    public void set(int i, JceField jceField) {
        this.data[i] = jceField;
    }

    public void set(JceField[] jceFieldArr) {
        this.data = jceFieldArr;
    }

    public int size() {
        return this.data.length;
    }
}
