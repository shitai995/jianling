package com.tencent.webnet.a;

import java.io.IOException;
import java.io.Writer;
import java.util.HashMap;
import java.util.Map;

/* JADX INFO: loaded from: classes.dex */
public class c extends HashMap implements b, d, Map {
    private static final long serialVersionUID = 1;

    public static void a(Map map, Writer writer) throws IOException {
        boolean z;
        if (map == null) {
            writer.write("null");
            return;
        }
        boolean z2 = true;
        writer.write(123);
        for (Map.Entry entry : map.entrySet()) {
            if (z2) {
                z = false;
            } else {
                writer.write(44);
                z = z2;
            }
            writer.write(34);
            writer.write(U(String.valueOf(entry.getKey())));
            writer.write(34);
            writer.write(58);
            e.a(entry.getValue(), writer);
            z2 = z;
        }
        writer.write(125);
    }

    @Override // com.tencent.webnet.a.d
    public void a(Writer writer) throws IOException {
        a(this, writer);
    }

    public static String f(Map map) {
        boolean z;
        if (map == null) {
            return "null";
        }
        StringBuffer stringBuffer = new StringBuffer();
        boolean z2 = true;
        stringBuffer.append('{');
        for (Map.Entry entry : map.entrySet()) {
            if (z2) {
                z = false;
            } else {
                stringBuffer.append(',');
                z = z2;
            }
            a(String.valueOf(entry.getKey()), entry.getValue(), stringBuffer);
            z2 = z;
        }
        stringBuffer.append('}');
        return stringBuffer.toString();
    }

    @Override // com.tencent.webnet.a.b
    public String by() {
        return f(this);
    }

    private static String a(String str, Object obj, StringBuffer stringBuffer) {
        stringBuffer.append('\"');
        if (str == null) {
            stringBuffer.append("null");
        } else {
            e.a(str, stringBuffer);
        }
        stringBuffer.append('\"').append(':');
        stringBuffer.append(e.b(obj));
        return stringBuffer.toString();
    }

    @Override // java.util.AbstractMap
    public String toString() {
        return by();
    }

    public static String c(String str, Object obj) {
        StringBuffer stringBuffer = new StringBuffer();
        a(str, obj, stringBuffer);
        return stringBuffer.toString();
    }

    public static String U(String str) {
        return e.U(str);
    }
}
