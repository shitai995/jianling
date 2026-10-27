package com.dobetter.script;

/* JADX INFO: loaded from: classes.dex */
public class Array extends DataType {
    byte deepth = 0;
    private DataType[] elements;

    public Array(int[] dimSize) {
        init(dimSize);
    }

    public Array(Object ob, boolean isValue) {
        if (isValue) {
            if (ob instanceof int[]) {
                int[] args = (int[]) ob;
                int[] dimSize = {args.length};
                init(dimSize);
                for (int i = 0; i < args.length; i++) {
                    this.elements[i] = new Int(args[i]);
                }
                return;
            }
            if (ob instanceof boolean[]) {
                boolean[] args2 = (boolean[]) ob;
                int[] dimSize2 = {args2.length};
                init(dimSize2);
                for (int i2 = 0; i2 < args2.length; i2++) {
                    this.elements[i2] = new Bool(args2[i2]);
                }
                return;
            }
            if (ob instanceof String[]) {
                String[] args3 = (String[]) ob;
                int[] dimSize3 = {args3.length};
                init(dimSize3);
                for (int i3 = 0; i3 < args3.length; i3++) {
                    this.elements[i3] = new Str(args3[i3]);
                }
                return;
            }
            return;
        }
        if (ob instanceof int[]) {
            init((int[]) ob);
        }
    }

    private void init(int[] dimSize) {
        this.deepth = (byte) dimSize.length;
        int len = dimSize[0];
        if (this.deepth > 1) {
            int[] newDimSize = new int[this.deepth - 1];
            System.arraycopy(dimSize, 1, newDimSize, 0, this.deepth - 1);
            this.elements = new Array[len];
            for (int i = 0; i < ((Array[]) this.elements).length; i++) {
                this.elements[i] = new Array(newDimSize);
            }
            return;
        }
        this.elements = new DataType[len];
        for (int i2 = 0; i2 < len; i2++) {
            this.elements[i2] = new Int(0);
        }
    }

    public Object getValue(int[] pos) {
        Array arr = this;
        for (int i = 0; i < pos.length; i++) {
            if (arr.deepth == 1) {
                return arr.elements[pos[i]];
            }
            arr = (Array) arr.elements[pos[i]];
        }
        return arr;
    }

    public Object getValue(int pos) {
        return this.elements[pos];
    }

    public int getLength() {
        return this.elements.length;
    }

    public void setValue(int[] pos, Object value) {
        Array arr = this;
        for (int i = 0; i < pos.length; i++) {
            if (arr.deepth == 1) {
                arr.elements[pos[i]] = (DataType) value;
            } else {
                arr = (Array) arr.elements[pos[i]];
            }
        }
    }

    @Override // com.dobetter.script.DataType
    public String getString() {
        return new StringBuilder("Array[").append(this.elements).toString() == null ? "" : String.valueOf(this.elements.length) + "]";
    }
}
