package com.dobetter.script;

/* JADX INFO: loaded from: classes.dex */
public class Str extends DataType {
    private String value;

    public Str(String s) {
        this.value = null;
        this.value = s;
    }

    public void setVal(String s) {
        this.value = s;
    }

    public String getVal() {
        return this.value;
    }

    @Override // com.dobetter.script.DataType
    public String getString() {
        return this.value;
    }
}
