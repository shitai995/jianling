package com.qq.taf.jce.dynamic;

import java.util.Arrays;
import java.util.Comparator;

/* JADX INFO: loaded from: classes.dex */
public class StructField extends JceField {
    private static final Comparator tagComp = new Comparator() { // from class: com.qq.taf.jce.dynamic.StructField.1
        @Override // java.util.Comparator
        public int compare(JceField jceField, JceField jceField2) {
            return jceField.getTag() - jceField2.getTag();
        }
    };
    private JceField[] data;

    StructField(JceField[] jceFieldArr, int i) {
        super(i);
        this.data = jceFieldArr;
    }

    public JceField[] get() {
        return this.data;
    }

    public boolean setByTag(int i, JceField jceField) {
        int iBinarySearch = Arrays.binarySearch(this.data, JceField.createZero(i), tagComp);
        if (iBinarySearch >= 0) {
            this.data[iBinarySearch] = jceField;
            return true;
        }
        int i2 = (-iBinarySearch) - 1;
        JceField[] jceFieldArr = new JceField[this.data.length + 1];
        for (int i3 = 0; i3 < i2; i3++) {
            jceFieldArr[i3] = this.data[i3];
        }
        jceFieldArr[i2] = jceField;
        while (i2 < this.data.length) {
            jceFieldArr[i2 + 1] = this.data[i2];
            i2++;
        }
        return false;
    }

    public JceField getFieldByTag(int i) {
        int iBinarySearch = Arrays.binarySearch(this.data, JceField.createZero(i), tagComp);
        if (iBinarySearch >= 0) {
            return this.data[iBinarySearch];
        }
        return null;
    }
}
