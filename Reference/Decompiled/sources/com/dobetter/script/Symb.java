package com.dobetter.script;

/* JADX INFO: loaded from: classes.dex */
public class Symb extends DataType {
    public static final byte ADD = 1;
    public static final byte AND = 12;
    public static final byte DIV = 4;
    public static final byte EQU = 7;
    public static final byte GE = 8;
    public static final byte GRE = 5;
    public static final byte LE = 9;
    public static final byte LES = 6;
    public static final byte LP = 14;
    public static final byte MUL = 3;
    public static final byte NE = 10;
    public static final byte NONE = 0;
    public static final byte NOT = 11;
    public static final byte OR = 13;
    public static final byte RP = 15;
    public static final byte SUB = 2;
    private String s_value;
    private byte value;

    public Symb(String opSymbol) {
        this.value = (byte) 0;
        this.s_value = null;
        this.s_value = opSymbol;
        if (opSymbol.equals("+")) {
            this.value = (byte) 1;
            return;
        }
        if (opSymbol.equals("-")) {
            this.value = (byte) 2;
            return;
        }
        if (opSymbol.equals("*")) {
            this.value = (byte) 3;
            return;
        }
        if (opSymbol.equals("/")) {
            this.value = (byte) 4;
            return;
        }
        if (opSymbol.equals(">")) {
            this.value = (byte) 5;
            return;
        }
        if (opSymbol.equals("<")) {
            this.value = (byte) 6;
            return;
        }
        if (opSymbol.equals("=")) {
            this.value = (byte) 7;
            return;
        }
        if (opSymbol.equals(">=")) {
            this.value = (byte) 8;
            return;
        }
        if (opSymbol.equals("<=")) {
            this.value = (byte) 9;
            return;
        }
        if (opSymbol.equals("<>")) {
            this.value = (byte) 10;
            return;
        }
        if (opSymbol.equals("!")) {
            this.value = (byte) 11;
            return;
        }
        if (opSymbol.equals("(")) {
            this.value = (byte) 14;
            return;
        }
        if (opSymbol.equals(")")) {
            this.value = (byte) 15;
            return;
        }
        if (opSymbol.equals("&")) {
            this.value = (byte) 12;
        } else if (opSymbol.equals("|")) {
            this.value = (byte) 13;
        } else {
            this.value = (byte) 0;
        }
    }

    public byte getVal() {
        return this.value;
    }

    @Override // com.dobetter.script.DataType
    public String getString() {
        return this.s_value;
    }

    public boolean isLogicOp() {
        byte ot = this.value;
        return ot == 5 || ot == 6 || ot == 7 || ot == 8 || ot == 9 || ot == 10 || ot == 11 || ot == 12 || ot == 13;
    }

    public boolean isCompOp() {
        byte ot = this.value;
        return ot == 5 || ot == 6 || ot == 7 || ot == 8 || ot == 9 || ot == 10;
    }
}
