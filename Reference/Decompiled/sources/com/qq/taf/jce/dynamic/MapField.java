package com.qq.taf.jce.dynamic;

/* JADX INFO: loaded from: classes.dex */
public final class MapField extends JceField {
    private JceField[] keys;
    private JceField[] values;

    MapField(JceField[] jceFieldArr, JceField[] jceFieldArr2, int i) {
        super(i);
        this.keys = jceFieldArr;
        this.values = jceFieldArr2;
    }

    public JceField[] getKeys() {
        return this.keys;
    }

    public JceField[] getValues() {
        return this.values;
    }

    public int size() {
        return this.keys.length;
    }

    public JceField getKey(int i) {
        return this.keys[i];
    }

    public JceField getValue(int i) {
        return this.values[i];
    }

    public void setKey(int i, JceField jceField) {
        this.keys[i] = jceField;
    }

    public void setValue(int i, JceField jceField) {
        this.values[i] = jceField;
    }
}
