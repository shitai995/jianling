package com.qq.taf.jce;

import java.util.Collection;
import java.util.List;
import java.util.Map;

/* JADX INFO: loaded from: classes.dex */
public final class JceDisplayer {
    private int _level;
    private StringBuilder sb;

    private void ps(String str) {
        for (int i = 0; i < this._level; i++) {
            this.sb.append('\t');
        }
        if (str != null) {
            this.sb.append(str).append(": ");
        }
    }

    public JceDisplayer(StringBuilder sb, int i) {
        this._level = 0;
        this.sb = sb;
        this._level = i;
    }

    public JceDisplayer(StringBuilder sb) {
        this._level = 0;
        this.sb = sb;
    }

    public JceDisplayer display(boolean z, String str) {
        ps(str);
        this.sb.append(z ? 'T' : 'F').append('\n');
        return this;
    }

    public JceDisplayer display(byte b, String str) {
        ps(str);
        this.sb.append((int) b).append('\n');
        return this;
    }

    public JceDisplayer display(char c, String str) {
        ps(str);
        this.sb.append(c).append('\n');
        return this;
    }

    public JceDisplayer display(short s, String str) {
        ps(str);
        this.sb.append((int) s).append('\n');
        return this;
    }

    public JceDisplayer display(int i, String str) {
        ps(str);
        this.sb.append(i).append('\n');
        return this;
    }

    public JceDisplayer display(long j, String str) {
        ps(str);
        this.sb.append(j).append('\n');
        return this;
    }

    public JceDisplayer display(float f, String str) {
        ps(str);
        this.sb.append(f).append('\n');
        return this;
    }

    public JceDisplayer display(double d, String str) {
        ps(str);
        this.sb.append(d).append('\n');
        return this;
    }

    public JceDisplayer display(String str, String str2) {
        ps(str2);
        this.sb.append(str).append('\n');
        return this;
    }

    public JceDisplayer display(byte[] bArr, String str) {
        ps(str);
        if (bArr.length == 0) {
            this.sb.append(bArr.length).append(", []").append('\n');
        } else {
            this.sb.append(bArr.length).append(", [").append('\n');
            JceDisplayer jceDisplayer = new JceDisplayer(this.sb, this._level + 1);
            for (byte b : bArr) {
                jceDisplayer.display(b, (String) null);
            }
            display(']', (String) null);
        }
        return this;
    }

    public JceDisplayer display(char[] cArr, String str) {
        ps(str);
        if (cArr.length == 0) {
            this.sb.append(cArr.length).append(", []").append('\n');
        } else {
            this.sb.append(cArr.length).append(", [").append('\n');
            JceDisplayer jceDisplayer = new JceDisplayer(this.sb, this._level + 1);
            for (char c : cArr) {
                jceDisplayer.display(c, (String) null);
            }
            display(']', (String) null);
        }
        return this;
    }

    public JceDisplayer display(short[] sArr, String str) {
        ps(str);
        if (sArr.length == 0) {
            this.sb.append(sArr.length).append(", []").append('\n');
        } else {
            this.sb.append(sArr.length).append(", [").append('\n');
            JceDisplayer jceDisplayer = new JceDisplayer(this.sb, this._level + 1);
            for (short s : sArr) {
                jceDisplayer.display(s, (String) null);
            }
            display(']', (String) null);
        }
        return this;
    }

    public JceDisplayer display(int[] iArr, String str) {
        ps(str);
        if (iArr.length == 0) {
            this.sb.append(iArr.length).append(", []").append('\n');
        } else {
            this.sb.append(iArr.length).append(", [").append('\n');
            JceDisplayer jceDisplayer = new JceDisplayer(this.sb, this._level + 1);
            for (int i : iArr) {
                jceDisplayer.display(i, (String) null);
            }
            display(']', (String) null);
        }
        return this;
    }

    public JceDisplayer display(long[] jArr, String str) {
        ps(str);
        if (jArr.length == 0) {
            this.sb.append(jArr.length).append(", []").append('\n');
        } else {
            this.sb.append(jArr.length).append(", [").append('\n');
            JceDisplayer jceDisplayer = new JceDisplayer(this.sb, this._level + 1);
            for (long j : jArr) {
                jceDisplayer.display(j, (String) null);
            }
            display(']', (String) null);
        }
        return this;
    }

    public JceDisplayer display(float[] fArr, String str) {
        ps(str);
        if (fArr.length == 0) {
            this.sb.append(fArr.length).append(", []").append('\n');
        } else {
            this.sb.append(fArr.length).append(", [").append('\n');
            JceDisplayer jceDisplayer = new JceDisplayer(this.sb, this._level + 1);
            for (float f : fArr) {
                jceDisplayer.display(f, (String) null);
            }
            display(']', (String) null);
        }
        return this;
    }

    public JceDisplayer display(double[] dArr, String str) {
        ps(str);
        if (dArr.length == 0) {
            this.sb.append(dArr.length).append(", []").append('\n');
        } else {
            this.sb.append(dArr.length).append(", [").append('\n');
            JceDisplayer jceDisplayer = new JceDisplayer(this.sb, this._level + 1);
            for (double d : dArr) {
                jceDisplayer.display(d, (String) null);
            }
            display(']', (String) null);
        }
        return this;
    }

    public JceDisplayer display(Map map, String str) {
        ps(str);
        if (map.isEmpty()) {
            this.sb.append(map.size()).append(", {}").append('\n');
        } else {
            this.sb.append(map.size()).append(", {").append('\n');
            JceDisplayer jceDisplayer = new JceDisplayer(this.sb, this._level + 1);
            JceDisplayer jceDisplayer2 = new JceDisplayer(this.sb, this._level + 2);
            for (Map.Entry entry : map.entrySet()) {
                jceDisplayer.display('(', (String) null);
                jceDisplayer2.display(entry.getKey(), (String) null);
                jceDisplayer2.display(entry.getValue(), (String) null);
                jceDisplayer.display(')', (String) null);
            }
            display('}', (String) null);
        }
        return this;
    }

    public JceDisplayer display(Object[] objArr, String str) {
        ps(str);
        if (objArr.length == 0) {
            this.sb.append(objArr.length).append(", []").append('\n');
        } else {
            this.sb.append(objArr.length).append(", [").append('\n');
            JceDisplayer jceDisplayer = new JceDisplayer(this.sb, this._level + 1);
            for (Object obj : objArr) {
                jceDisplayer.display(obj, (String) null);
            }
            display(']', (String) null);
        }
        return this;
    }

    public JceDisplayer display(Collection collection, String str) {
        return display(collection.toArray(), str);
    }

    public JceDisplayer display(Object obj, String str) {
        if (obj instanceof Byte) {
            display(((Byte) obj).byteValue(), str);
        } else if (obj instanceof Boolean) {
            display(((Boolean) obj).booleanValue(), str);
        } else if (obj instanceof Short) {
            display(((Short) obj).shortValue(), str);
        } else if (obj instanceof Integer) {
            display(((Integer) obj).intValue(), str);
        } else if (obj instanceof Long) {
            display(((Long) obj).longValue(), str);
        } else if (obj instanceof Float) {
            display(((Float) obj).floatValue(), str);
        } else if (obj instanceof Double) {
            display(((Double) obj).doubleValue(), str);
        } else if (obj instanceof String) {
            display((String) obj, str);
        } else if (obj instanceof Map) {
            display((Map) obj, str);
        } else if (obj instanceof List) {
            display((Collection) obj, str);
        } else if (obj instanceof JceStruct) {
            display((JceStruct) obj, str);
        } else if (obj instanceof byte[]) {
            display((byte[]) obj, str);
        } else if (obj instanceof boolean[]) {
            display((boolean[]) obj, str);
        } else if (obj instanceof short[]) {
            display((short[]) obj, str);
        } else if (obj instanceof int[]) {
            display((int[]) obj, str);
        } else if (obj instanceof long[]) {
            display((long[]) obj, str);
        } else if (obj instanceof float[]) {
            display((float[]) obj, str);
        } else if (obj instanceof double[]) {
            display((double[]) obj, str);
        } else if (obj.getClass().isArray()) {
            display((Object[]) obj, str);
        } else {
            throw new JceEncodeException("write object error: unsupport type.");
        }
        return this;
    }

    public JceDisplayer display(JceStruct jceStruct, String str) {
        display('{', str);
        jceStruct.display(this.sb, this._level + 1);
        display('}', (String) null);
        return this;
    }

    public static void main(String[] strArr) {
        StringBuilder sb = new StringBuilder();
        sb.append(1.2d);
        System.out.println(sb.toString());
    }
}
