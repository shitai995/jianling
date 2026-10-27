package com.dobetter.script;

/* JADX INFO: loaded from: classes.dex */
public class Int extends DataType {
    private int value;

    public Int(String s) {
        this.value = 0;
        if (s.length() > 1) {
            if (s.startsWith("0x")) {
                this.value = Integer.parseInt(s.substring(2), 16);
                return;
            } else if (s.startsWith("0")) {
                this.value = Integer.parseInt(s.substring(1), 8);
                return;
            } else {
                this.value = Integer.parseInt(s);
                return;
            }
        }
        this.value = Integer.parseInt(s);
    }

    public Int(int i) {
        this.value = 0;
        this.value = i;
    }

    public int getVal() {
        return this.value;
    }

    public void setVal(int i) {
        this.value = i;
    }

    @Override // com.dobetter.script.DataType
    public String getString() {
        return Integer.toString(this.value);
    }
}
