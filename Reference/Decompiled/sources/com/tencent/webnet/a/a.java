package com.tencent.webnet.a;

import java.io.IOException;
import java.io.Writer;
import java.util.ArrayList;
import java.util.List;

/* JADX INFO: loaded from: classes.dex */
public class a extends ArrayList implements b, d, List {
    private static final long serialVersionUID = 1;

    public static void a(List list, Writer writer) throws IOException {
        if (list == null) {
            writer.write("null");
            return;
        }
        boolean z = true;
        writer.write(91);
        for (Object obj : list) {
            if (z) {
                z = false;
            } else {
                writer.write(44);
            }
            if (obj == null) {
                writer.write("null");
            } else {
                e.a(obj, writer);
            }
        }
        writer.write(93);
    }

    @Override // com.tencent.webnet.a.d
    public void a(Writer writer) throws IOException {
        a(this, writer);
    }

    public static String a(List list) {
        if (list == null) {
            return "null";
        }
        boolean z = true;
        StringBuffer stringBuffer = new StringBuffer();
        stringBuffer.append('[');
        for (Object obj : list) {
            if (z) {
                z = false;
            } else {
                stringBuffer.append(',');
            }
            if (obj == null) {
                stringBuffer.append("null");
            } else {
                stringBuffer.append(e.b(obj));
            }
        }
        stringBuffer.append(']');
        return stringBuffer.toString();
    }

    @Override // com.tencent.webnet.a.b
    public String by() {
        return a(this);
    }

    @Override // java.util.AbstractCollection
    public String toString() {
        return by();
    }
}
