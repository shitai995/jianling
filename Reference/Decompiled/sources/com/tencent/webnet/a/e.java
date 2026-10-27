package com.tencent.webnet.a;

import com.dobetter.script.Interpreter;
import java.io.Writer;
import java.util.List;
import java.util.Map;

/* JADX INFO: loaded from: classes.dex */
public class e {
    public static void a(Object obj, Writer writer) {
        if (obj == null) {
            writer.write("null");
            return;
        }
        if (obj instanceof String) {
            writer.write(34);
            writer.write(U((String) obj));
            writer.write(34);
            return;
        }
        if (obj instanceof Double) {
            if (((Double) obj).isInfinite() || ((Double) obj).isNaN()) {
                writer.write("null");
                return;
            } else {
                writer.write(obj.toString());
                return;
            }
        }
        if (obj instanceof Float) {
            if (((Float) obj).isInfinite() || ((Float) obj).isNaN()) {
                writer.write("null");
                return;
            } else {
                writer.write(obj.toString());
                return;
            }
        }
        if (obj instanceof Number) {
            writer.write(obj.toString());
            return;
        }
        if (obj instanceof Boolean) {
            writer.write(obj.toString());
            return;
        }
        if (obj instanceof d) {
            ((d) obj).a(writer);
            return;
        }
        if (obj instanceof b) {
            writer.write(((b) obj).by());
            return;
        }
        if (obj instanceof Map) {
            c.a((Map) obj, writer);
        } else if (obj instanceof List) {
            a.a((List) obj, writer);
        } else {
            writer.write(obj.toString());
        }
    }

    public static String b(Object obj) {
        if (obj == null) {
            return "null";
        }
        if (obj instanceof String) {
            return "\"" + U((String) obj) + "\"";
        }
        if (obj instanceof Double) {
            if (((Double) obj).isInfinite() || ((Double) obj).isNaN()) {
                return "null";
            }
            return obj.toString();
        }
        if (obj instanceof Float) {
            if (((Float) obj).isInfinite() || ((Float) obj).isNaN()) {
                return "null";
            }
            return obj.toString();
        }
        if (obj instanceof Number) {
            return obj.toString();
        }
        if (obj instanceof Boolean) {
            return obj.toString();
        }
        if (obj instanceof b) {
            return ((b) obj).by();
        }
        if (obj instanceof Map) {
            return c.f((Map) obj);
        }
        if (obj instanceof List) {
            return a.a((List) obj);
        }
        return obj.toString();
    }

    public static String U(String str) {
        if (str == null) {
            return null;
        }
        StringBuffer stringBuffer = new StringBuffer();
        a(str, stringBuffer);
        return stringBuffer.toString();
    }

    static void a(String str, StringBuffer stringBuffer) {
        for (int i = 0; i < str.length(); i++) {
            char cCharAt = str.charAt(i);
            switch (cCharAt) {
                case '\b':
                    stringBuffer.append("\\b");
                    break;
                case Interpreter.ERR_ARR_OUT /* 9 */:
                    stringBuffer.append("\\t");
                    break;
                case '\n':
                    stringBuffer.append("\\n");
                    break;
                case '\f':
                    stringBuffer.append("\\f");
                    break;
                case '\r':
                    stringBuffer.append("\\r");
                    break;
                case '\"':
                    stringBuffer.append("\\\"");
                    break;
                case '/':
                    stringBuffer.append("\\/");
                    break;
                case '\\':
                    stringBuffer.append("\\\\");
                    break;
                default:
                    if ((cCharAt >= 0 && cCharAt <= 31) || ((cCharAt >= 127 && cCharAt <= 159) || (cCharAt >= 8192 && cCharAt <= 8447))) {
                        String hexString = Integer.toHexString(cCharAt);
                        stringBuffer.append("\\u");
                        for (int i2 = 0; i2 < 4 - hexString.length(); i2++) {
                            stringBuffer.append('0');
                        }
                        stringBuffer.append(hexString.toUpperCase());
                    } else {
                        stringBuffer.append(cCharAt);
                    }
                    break;
            }
        }
    }
}
