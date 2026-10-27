package com.qq.taf.jce;

import java.io.UnsupportedEncodingException;
import java.nio.ByteBuffer;
import java.util.Arrays;
import java.util.Collection;
import java.util.Iterator;
import java.util.List;
import java.util.Map;

/* JADX INFO: loaded from: classes.dex */
public class JceOutputStream {
    private ByteBuffer bs;
    protected String sServerEncoding;

    public JceOutputStream(ByteBuffer byteBuffer) {
        this.sServerEncoding = "GBK";
        this.bs = byteBuffer;
    }

    public JceOutputStream(int i) {
        this.sServerEncoding = "GBK";
        this.bs = ByteBuffer.allocate(i);
    }

    public JceOutputStream() {
        this(128);
    }

    public ByteBuffer getByteBuffer() {
        return this.bs;
    }

    public byte[] toByteArray() {
        byte[] bArr = new byte[this.bs.position()];
        System.arraycopy(this.bs.array(), 0, bArr, 0, this.bs.position());
        return bArr;
    }

    public void reserve(int i) {
        if (this.bs.remaining() < i) {
            ByteBuffer byteBufferAllocate = ByteBuffer.allocate((this.bs.capacity() + i) * 2);
            byteBufferAllocate.put(this.bs.array(), 0, this.bs.position());
            this.bs = byteBufferAllocate;
        }
    }

    public void writeHead(byte b, int i) {
        if (i < 15) {
            this.bs.put((byte) ((i << 4) | b));
        } else {
            if (i < 256) {
                this.bs.put((byte) (b | 240));
                this.bs.put((byte) i);
                return;
            }
            throw new JceEncodeException("tag is too large: " + i);
        }
    }

    public void write(boolean z, int i) {
        write((byte) (z ? 1 : 0), i);
    }

    public void write(byte b, int i) {
        reserve(3);
        if (b == 0) {
            writeHead((byte) 12, i);
        } else {
            writeHead((byte) 0, i);
            this.bs.put(b);
        }
    }

    public void write(short s, int i) {
        reserve(4);
        if (s >= -128 && s <= 127) {
            write((byte) s, i);
        } else {
            writeHead((byte) 1, i);
            this.bs.putShort(s);
        }
    }

    public void write(int i, int i2) {
        reserve(6);
        if (i >= -32768 && i <= 32767) {
            write((short) i, i2);
        } else {
            writeHead((byte) 2, i2);
            this.bs.putInt(i);
        }
    }

    public void write(long j, int i) {
        reserve(10);
        if (j >= -2147483648L && j <= 2147483647L) {
            write((int) j, i);
        } else {
            writeHead((byte) 3, i);
            this.bs.putLong(j);
        }
    }

    public void write(float f, int i) {
        reserve(6);
        writeHead((byte) 4, i);
        this.bs.putFloat(f);
    }

    public void write(double d, int i) {
        reserve(10);
        writeHead((byte) 5, i);
        this.bs.putDouble(d);
    }

    public void writeStringByte(String str, int i) {
        byte[] bArrHexStr2Bytes = HexUtil.hexStr2Bytes(str);
        reserve(bArrHexStr2Bytes.length + 10);
        if (bArrHexStr2Bytes.length > 255) {
            writeHead((byte) 7, i);
            this.bs.putInt(bArrHexStr2Bytes.length);
            this.bs.put(bArrHexStr2Bytes);
        } else {
            writeHead((byte) 6, i);
            this.bs.put((byte) bArrHexStr2Bytes.length);
            this.bs.put(bArrHexStr2Bytes);
        }
    }

    public void writeByteString(String str, int i) {
        reserve(str.length() + 10);
        byte[] bArrHexStr2Bytes = HexUtil.hexStr2Bytes(str);
        if (bArrHexStr2Bytes.length > 255) {
            writeHead((byte) 7, i);
            this.bs.putInt(bArrHexStr2Bytes.length);
            this.bs.put(bArrHexStr2Bytes);
        } else {
            writeHead((byte) 6, i);
            this.bs.put((byte) bArrHexStr2Bytes.length);
            this.bs.put(bArrHexStr2Bytes);
        }
    }

    public void write(String str, int i) {
        byte[] bytes;
        try {
            bytes = str.getBytes(this.sServerEncoding);
        } catch (UnsupportedEncodingException e) {
            bytes = str.getBytes();
        }
        reserve(bytes.length + 10);
        if (bytes.length > 255) {
            writeHead((byte) 7, i);
            this.bs.putInt(bytes.length);
            this.bs.put(bytes);
        } else {
            writeHead((byte) 6, i);
            this.bs.put((byte) bytes.length);
            this.bs.put(bytes);
        }
    }

    public void write(Map map, int i) {
        reserve(8);
        writeHead((byte) 8, i);
        write(map == null ? 0 : map.size(), 0);
        if (map != null) {
            for (Map.Entry entry : map.entrySet()) {
                write(entry.getKey(), 0);
                write(entry.getValue(), 1);
            }
        }
    }

    public void write(boolean[] zArr, int i) {
        reserve(8);
        writeHead((byte) 9, i);
        write(zArr.length, 0);
        for (boolean z : zArr) {
            write(z, 0);
        }
    }

    public void write(byte[] bArr, int i) {
        reserve(bArr.length + 8);
        writeHead((byte) 13, i);
        writeHead((byte) 0, 0);
        write(bArr.length, 0);
        this.bs.put(bArr);
    }

    public void write(short[] sArr, int i) {
        reserve(8);
        writeHead((byte) 9, i);
        write(sArr.length, 0);
        for (short s : sArr) {
            write(s, 0);
        }
    }

    public void write(int[] iArr, int i) {
        reserve(8);
        writeHead((byte) 9, i);
        write(iArr.length, 0);
        for (int i2 : iArr) {
            write(i2, 0);
        }
    }

    public void write(long[] jArr, int i) {
        reserve(8);
        writeHead((byte) 9, i);
        write(jArr.length, 0);
        for (long j : jArr) {
            write(j, 0);
        }
    }

    public void write(float[] fArr, int i) {
        reserve(8);
        writeHead((byte) 9, i);
        write(fArr.length, 0);
        for (float f : fArr) {
            write(f, 0);
        }
    }

    public void write(double[] dArr, int i) {
        reserve(8);
        writeHead((byte) 9, i);
        write(dArr.length, 0);
        for (double d : dArr) {
            write(d, 0);
        }
    }

    public void write(Object[] objArr, int i) {
        writeArray(objArr, i);
    }

    private void writeArray(Object[] objArr, int i) {
        reserve(8);
        writeHead((byte) 9, i);
        write(objArr.length, 0);
        for (Object obj : objArr) {
            write(obj, 0);
        }
    }

    public void write(Collection collection, int i) {
        reserve(8);
        writeHead((byte) 9, i);
        write(collection == null ? 0 : collection.size(), 0);
        if (collection != null) {
            Iterator it = collection.iterator();
            while (it.hasNext()) {
                write(it.next(), 0);
            }
        }
    }

    public void write(JceStruct jceStruct, int i) {
        reserve(2);
        writeHead((byte) 10, i);
        jceStruct.writeTo(this);
        reserve(2);
        writeHead((byte) 11, 0);
    }

    public void write(Byte b, int i) {
        write(b.byteValue(), i);
    }

    public void write(Boolean bool, int i) {
        write(bool.booleanValue(), i);
    }

    public void write(Short sh, int i) {
        write(sh.shortValue(), i);
    }

    public void write(Integer num, int i) {
        write(num.intValue(), i);
    }

    public void write(Long l, int i) {
        write(l.longValue(), i);
    }

    public void write(Float f, int i) {
        write(f.floatValue(), i);
    }

    public void write(Double d, int i) {
        write(d.doubleValue(), i);
    }

    public void write(Object obj, int i) {
        if (obj instanceof Byte) {
            write(((Byte) obj).byteValue(), i);
            return;
        }
        if (obj instanceof Boolean) {
            write(((Boolean) obj).booleanValue(), i);
            return;
        }
        if (obj instanceof Short) {
            write(((Short) obj).shortValue(), i);
            return;
        }
        if (obj instanceof Integer) {
            write(((Integer) obj).intValue(), i);
            return;
        }
        if (obj instanceof Long) {
            write(((Long) obj).longValue(), i);
            return;
        }
        if (obj instanceof Float) {
            write(((Float) obj).floatValue(), i);
            return;
        }
        if (obj instanceof Double) {
            write(((Double) obj).doubleValue(), i);
            return;
        }
        if (obj instanceof String) {
            write((String) obj, i);
            return;
        }
        if (obj instanceof Map) {
            write((Map) obj, i);
            return;
        }
        if (obj instanceof List) {
            write((Collection) obj, i);
            return;
        }
        if (obj instanceof JceStruct) {
            write((JceStruct) obj, i);
            return;
        }
        if (obj instanceof byte[]) {
            write((byte[]) obj, i);
            return;
        }
        if (obj instanceof boolean[]) {
            write((boolean[]) obj, i);
            return;
        }
        if (obj instanceof short[]) {
            write((short[]) obj, i);
            return;
        }
        if (obj instanceof int[]) {
            write((int[]) obj, i);
            return;
        }
        if (obj instanceof long[]) {
            write((long[]) obj, i);
            return;
        }
        if (obj instanceof float[]) {
            write((float[]) obj, i);
            return;
        }
        if (obj instanceof double[]) {
            write((double[]) obj, i);
        } else if (obj.getClass().isArray()) {
            writeArray((Object[]) obj, i);
        } else {
            if (obj instanceof Collection) {
                write((Collection) obj, i);
                return;
            }
            throw new JceEncodeException("write object error: unsupport type. " + obj.getClass());
        }
    }

    public int setServerEncoding(String str) {
        this.sServerEncoding = str;
        return 0;
    }

    public static void main(String[] strArr) {
        JceOutputStream jceOutputStream = new JceOutputStream();
        jceOutputStream.write(1311768467283714885L, 0);
        System.out.println(HexUtil.bytes2HexStr(jceOutputStream.getByteBuffer().array()));
        System.out.println(Arrays.toString(jceOutputStream.toByteArray()));
    }
}
