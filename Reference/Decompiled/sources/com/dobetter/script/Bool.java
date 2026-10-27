package com.dobetter.script;

/* JADX INFO: loaded from: classes.dex */
public class Bool extends DataType {
    private boolean value;

    public Bool(String s) {
        this.value = false;
        if (s.toLowerCase().equals("true")) {
            this.value = true;
        } else {
            this.value = false;
        }
    }

    public Bool(boolean b) {
        this.value = false;
        this.value = b;
    }

    public void setVal(boolean b) {
        this.value = b;
    }

    public boolean getVal() {
        return this.value;
    }

    @Override // com.dobetter.script.DataType
    public String getString() {
        return new Boolean(this.value).toString();
    }

    public static boolean isBool(String s) {
        return s.toLowerCase().equals("true") || s.toLowerCase().equals("false");
    }
}
