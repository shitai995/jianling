package com.dobetter.script;

import java.util.Vector;

/* JADX INFO: loaded from: classes.dex */
public interface ScriptProcessor {
    public static final byte SCRIPT_CONTINUE = 3;
    public static final byte SCRIPT_FINISHED = 4;
    public static final byte SCRIPT_NULL = 0;
    public static final byte SCRIPT_PAUSE = 2;
    public static final byte SCRIPT_RUNNING = 1;

    DataType call(Vector vector, int i) throws Exception;

    byte getScriptState();

    void setScriptState(byte b);
}
