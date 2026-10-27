package com.qq.taf.jce;

import android.support.v4.view.accessibility.AccessibilityEventCompat;
import com.dobetter.script.Interpreter;
import java.io.UnsupportedEncodingException;
import java.lang.reflect.Array;
import java.nio.BufferUnderflowException;
import java.nio.ByteBuffer;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

/* JADX INFO: loaded from: classes.dex */
public final class JceInputStream {
    private ByteBuffer bs;
    protected String sServerEncoding = "GBK";

    public class HeadData {
        public int tag;
        public byte type;

        public void clear() {
            this.type = (byte) 0;
            this.tag = 0;
        }
    }

    public JceInputStream() {
    }

    public JceInputStream(ByteBuffer byteBuffer) {
        this.bs = byteBuffer;
    }

    public JceInputStream(byte[] bArr) {
        this.bs = ByteBuffer.wrap(bArr);
    }

    public void warp(byte[] bArr) {
        if (this.bs != null) {
            this.bs.clear();
        }
        this.bs = ByteBuffer.wrap(bArr);
    }

    public static int readHead(HeadData headData, ByteBuffer byteBuffer) {
        byte b = byteBuffer.get();
        headData.type = (byte) (b & 15);
        headData.tag = (b & 240) >> 4;
        if (headData.tag != 15) {
            return 1;
        }
        headData.tag = byteBuffer.get();
        return 2;
    }

    public void readHead(HeadData headData) {
        readHead(headData, this.bs);
    }

    private int peakHead(HeadData headData) {
        return readHead(headData, this.bs.duplicate());
    }

    private void skip(int i) {
        this.bs.position(this.bs.position() + i);
    }

    public boolean skipToTag(int i) {
        try {
            HeadData headData = new HeadData();
            while (true) {
                int iPeakHead = peakHead(headData);
                if (i <= headData.tag || headData.type == 11) {
                    break;
                }
                skip(iPeakHead);
                skipField(headData.type);
            }
            return i == headData.tag;
        } catch (JceDecodeException e) {
            return false;
        } catch (BufferUnderflowException e2) {
            return false;
        }
    }

    public void skipToStructEnd() {
        HeadData headData = new HeadData();
        do {
            readHead(headData);
            skipField(headData.type);
        } while (headData.type != 11);
    }

    private void skipField() {
        HeadData headData = new HeadData();
        readHead(headData);
        skipField(headData.type);
    }

    private void skipField(byte b) {
        int i = 0;
        switch (b) {
            case 0:
                skip(1);
                return;
            case 1:
                skip(2);
                return;
            case 2:
                skip(4);
                return;
            case 3:
                skip(8);
                return;
            case 4:
                skip(4);
                return;
            case 5:
                skip(8);
                return;
            case 6:
                int i2 = this.bs.get();
                if (i2 < 0) {
                    i2 += AccessibilityEventCompat.TYPE_VIEW_HOVER_EXIT;
                }
                skip(i2);
                return;
            case 7:
                skip(this.bs.getInt());
                return;
            case 8:
                int i3 = read(0, 0, true);
                while (i < i3 * 2) {
                    skipField();
                    i++;
                }
                return;
            case Interpreter.ERR_ARR_OUT /* 9 */:
                int i4 = read(0, 0, true);
                while (i < i4) {
                    skipField();
                    i++;
                }
                return;
            case 10:
                skipToStructEnd();
                return;
            case 11:
            case 12:
                return;
            case 13:
                HeadData headData = new HeadData();
                readHead(headData);
                if (headData.type != 0) {
                    throw new JceDecodeException("skipField with invalid type, type value: " + ((int) b) + ", " + ((int) headData.type));
                }
                skip(read(0, 0, true));
                return;
            default:
                throw new JceDecodeException("invalid type.");
        }
    }

    public boolean read(boolean z, int i, boolean z2) {
        return read((byte) 0, i, z2) != 0;
    }

    public byte read(byte b, int i, boolean z) {
        if (skipToTag(i)) {
            HeadData headData = new HeadData();
            readHead(headData);
            switch (headData.type) {
                case 0:
                    return this.bs.get();
                case 12:
                    return (byte) 0;
                default:
                    throw new JceDecodeException("type mismatch.");
            }
        }
        if (z) {
            throw new JceDecodeException("require field not exist.");
        }
        return b;
    }

    public short read(short s, int i, boolean z) {
        if (skipToTag(i)) {
            HeadData headData = new HeadData();
            readHead(headData);
            switch (headData.type) {
                case 0:
                    return this.bs.get();
                case 1:
                    return this.bs.getShort();
                case 12:
                    return (short) 0;
                default:
                    throw new JceDecodeException("type mismatch.");
            }
        }
        if (z) {
            throw new JceDecodeException("require field not exist.");
        }
        return s;
    }

    public int read(int i, int i2, boolean z) {
        if (skipToTag(i2)) {
            HeadData headData = new HeadData();
            readHead(headData);
            switch (headData.type) {
                case 0:
                    return this.bs.get();
                case 1:
                    return this.bs.getShort();
                case 2:
                    return this.bs.getInt();
                case 12:
                    return 0;
                default:
                    throw new JceDecodeException("type mismatch.");
            }
        }
        if (z) {
            throw new JceDecodeException("require field not exist.");
        }
        return i;
    }

    public long read(long j, int i, boolean z) {
        if (skipToTag(i)) {
            HeadData headData = new HeadData();
            readHead(headData);
            switch (headData.type) {
                case 0:
                    return this.bs.get();
                case 1:
                    return this.bs.getShort();
                case 2:
                    return this.bs.getInt();
                case 3:
                    return this.bs.getLong();
                case 12:
                    return 0L;
                default:
                    throw new JceDecodeException("type mismatch.");
            }
        }
        if (z) {
            throw new JceDecodeException("require field not exist.");
        }
        return j;
    }

    public float read(float f, int i, boolean z) {
        if (skipToTag(i)) {
            HeadData headData = new HeadData();
            readHead(headData);
            switch (headData.type) {
                case 4:
                    return this.bs.getFloat();
                case 12:
                    return 0.0f;
                default:
                    throw new JceDecodeException("type mismatch.");
            }
        }
        if (z) {
            throw new JceDecodeException("require field not exist.");
        }
        return f;
    }

    public double read(double d, int i, boolean z) {
        if (skipToTag(i)) {
            HeadData headData = new HeadData();
            readHead(headData);
            switch (headData.type) {
                case 4:
                    return this.bs.getFloat();
                case 5:
                    return this.bs.getDouble();
                case 12:
                    return 0.0d;
                default:
                    throw new JceDecodeException("type mismatch.");
            }
        }
        if (z) {
            throw new JceDecodeException("require field not exist.");
        }
        return d;
    }

    public String readByteString(String str, int i, boolean z) {
        if (skipToTag(i)) {
            HeadData headData = new HeadData();
            readHead(headData);
            switch (headData.type) {
                case 6:
                    int i2 = this.bs.get();
                    if (i2 < 0) {
                        i2 += AccessibilityEventCompat.TYPE_VIEW_HOVER_EXIT;
                    }
                    byte[] bArr = new byte[i2];
                    this.bs.get(bArr);
                    return HexUtil.bytes2HexStr(bArr);
                case 7:
                    int i3 = this.bs.getInt();
                    if (i3 > 104857600 || i3 < 0) {
                        throw new JceDecodeException("String too long: " + i3);
                    }
                    byte[] bArr2 = new byte[i3];
                    this.bs.get(bArr2);
                    return HexUtil.bytes2HexStr(bArr2);
                default:
                    throw new JceDecodeException("type mismatch.");
            }
        }
        if (z) {
            throw new JceDecodeException("require field not exist.");
        }
        return str;
    }

    public String read(String str, int i, boolean z) {
        if (skipToTag(i)) {
            HeadData headData = new HeadData();
            readHead(headData);
            switch (headData.type) {
                case 6:
                    int i2 = this.bs.get();
                    if (i2 < 0) {
                        i2 += AccessibilityEventCompat.TYPE_VIEW_HOVER_EXIT;
                    }
                    byte[] bArr = new byte[i2];
                    this.bs.get(bArr);
                    try {
                        return new String(bArr, this.sServerEncoding);
                    } catch (UnsupportedEncodingException e) {
                        return new String(bArr);
                    }
                case 7:
                    int i3 = this.bs.getInt();
                    if (i3 > 104857600 || i3 < 0) {
                        throw new JceDecodeException("String too long: " + i3);
                    }
                    byte[] bArr2 = new byte[i3];
                    this.bs.get(bArr2);
                    try {
                        return new String(bArr2, this.sServerEncoding);
                    } catch (UnsupportedEncodingException e2) {
                        return new String(bArr2);
                    }
                default:
                    throw new JceDecodeException("type mismatch.");
            }
        }
        if (z) {
            throw new JceDecodeException("require field not exist.");
        }
        return str;
    }

    public String readString(int i, boolean z) {
        if (skipToTag(i)) {
            HeadData headData = new HeadData();
            readHead(headData);
            switch (headData.type) {
                case 6:
                    int i2 = this.bs.get();
                    if (i2 < 0) {
                        i2 += AccessibilityEventCompat.TYPE_VIEW_HOVER_EXIT;
                    }
                    byte[] bArr = new byte[i2];
                    this.bs.get(bArr);
                    try {
                        return new String(bArr, this.sServerEncoding);
                    } catch (UnsupportedEncodingException e) {
                        return new String(bArr);
                    }
                case 7:
                    int i3 = this.bs.getInt();
                    if (i3 > 104857600 || i3 < 0) {
                        throw new JceDecodeException("String too long: " + i3);
                    }
                    byte[] bArr2 = new byte[i3];
                    this.bs.get(bArr2);
                    try {
                        return new String(bArr2, this.sServerEncoding);
                    } catch (UnsupportedEncodingException e2) {
                        return new String(bArr2);
                    }
                default:
                    throw new JceDecodeException("type mismatch.");
            }
        }
        if (!z) {
            return null;
        }
        throw new JceDecodeException("require field not exist.");
    }

    public String[] read(String[] strArr, int i, boolean z) {
        return (String[]) readArray(strArr, i, z);
    }

    public Map readStringMap(int i, boolean z) {
        HashMap map = new HashMap();
        if (skipToTag(i)) {
            HeadData headData = new HeadData();
            readHead(headData);
            switch (headData.type) {
                case 8:
                    int i2 = read(0, 0, true);
                    if (i2 < 0) {
                        throw new JceDecodeException("size invalid: " + i2);
                    }
                    for (int i3 = 0; i3 < i2; i3++) {
                        map.put(readString(0, true), readString(1, true));
                    }
                    break;
                    break;
                default:
                    throw new JceDecodeException("type mismatch.");
            }
        } else if (z) {
            throw new JceDecodeException("require field not exist.");
        }
        return map;
    }

    public HashMap readMap(Map map, int i, boolean z) {
        return (HashMap) readMap(new HashMap(), map, i, z);
    }

    private Map readMap(Map map, Map map2, int i, boolean z) {
        if (map2 == null || map2.isEmpty()) {
            return new HashMap();
        }
        Map.Entry entry = (Map.Entry) map2.entrySet().iterator().next();
        Object key = entry.getKey();
        Object value = entry.getValue();
        if (skipToTag(i)) {
            HeadData headData = new HeadData();
            readHead(headData);
            switch (headData.type) {
                case 8:
                    int i2 = read(0, 0, true);
                    if (i2 < 0) {
                        throw new JceDecodeException("size invalid: " + i2);
                    }
                    for (int i3 = 0; i3 < i2; i3++) {
                        map.put(read(key, 0, true), read(value, 1, true));
                    }
                    return map;
                default:
                    throw new JceDecodeException("type mismatch.");
            }
        }
        if (z) {
            throw new JceDecodeException("require field not exist.");
        }
        return map;
    }

    public List readList(int i, boolean z) {
        ArrayList arrayList = new ArrayList();
        if (skipToTag(i)) {
            HeadData headData = new HeadData();
            readHead(headData);
            switch (headData.type) {
                case Interpreter.ERR_ARR_OUT /* 9 */:
                    int i2 = read(0, 0, true);
                    if (i2 < 0) {
                        throw new JceDecodeException("size invalid: " + i2);
                    }
                    for (int i3 = 0; i3 < i2; i3++) {
                        HeadData headData2 = new HeadData();
                        readHead(headData2);
                        switch (headData2.type) {
                            case 0:
                                skip(1);
                                break;
                            case 1:
                                skip(2);
                                break;
                            case 2:
                                skip(4);
                                break;
                            case 3:
                                skip(8);
                                break;
                            case 4:
                                skip(4);
                                break;
                            case 5:
                                skip(8);
                                break;
                            case 6:
                                int i4 = this.bs.get();
                                if (i4 < 0) {
                                    i4 += AccessibilityEventCompat.TYPE_VIEW_HOVER_EXIT;
                                }
                                skip(i4);
                                break;
                            case 7:
                                skip(this.bs.getInt());
                                break;
                            case 8:
                            case Interpreter.ERR_ARR_OUT /* 9 */:
                                break;
                            case 10:
                                try {
                                    JceStruct jceStruct = (JceStruct) Class.forName(JceStruct.class.getName()).getConstructor(new Class[0]).newInstance(new Object[0]);
                                    jceStruct.readFrom(this);
                                    skipToStructEnd();
                                    arrayList.add(jceStruct);
                                } catch (Exception e) {
                                    e.printStackTrace();
                                    throw new JceDecodeException("type mismatch." + e);
                                }
                                break;
                            case 11:
                            default:
                                throw new JceDecodeException("type mismatch.");
                            case 12:
                                arrayList.add(new Integer(0));
                                break;
                        }
                    }
                    break;
                    break;
                default:
                    throw new JceDecodeException("type mismatch.");
            }
        } else if (z) {
            throw new JceDecodeException("require field not exist.");
        }
        return arrayList;
    }

    public boolean[] read(boolean[] zArr, int i, boolean z) {
        boolean[] zArr2 = (boolean[]) null;
        if (skipToTag(i)) {
            HeadData headData = new HeadData();
            readHead(headData);
            switch (headData.type) {
                case Interpreter.ERR_ARR_OUT /* 9 */:
                    int i2 = read(0, 0, true);
                    if (i2 < 0) {
                        throw new JceDecodeException("size invalid: " + i2);
                    }
                    zArr2 = new boolean[i2];
                    for (int i3 = 0; i3 < i2; i3++) {
                        zArr2[i3] = read(zArr2[0], 0, true);
                    }
                    break;
                    break;
                default:
                    throw new JceDecodeException("type mismatch.");
            }
        } else if (z) {
            throw new JceDecodeException("require field not exist.");
        }
        return zArr2;
    }

    public byte[] read(byte[] bArr, int i, boolean z) {
        byte[] bArr2 = (byte[]) null;
        if (skipToTag(i)) {
            HeadData headData = new HeadData();
            readHead(headData);
            switch (headData.type) {
                case Interpreter.ERR_ARR_OUT /* 9 */:
                    int i2 = read(0, 0, true);
                    if (i2 < 0) {
                        throw new JceDecodeException("size invalid: " + i2);
                    }
                    byte[] bArr3 = new byte[i2];
                    for (int i3 = 0; i3 < i2; i3++) {
                        bArr3[i3] = read(bArr3[0], 0, true);
                    }
                    return bArr3;
                case 13:
                    HeadData headData2 = new HeadData();
                    readHead(headData2);
                    if (headData2.type != 0) {
                        throw new JceDecodeException("type mismatch, tag: " + i + ", type: " + ((int) headData.type) + ", " + ((int) headData2.type));
                    }
                    int i4 = read(0, 0, true);
                    if (i4 < 0) {
                        throw new JceDecodeException("invalid size, tag: " + i + ", type: " + ((int) headData.type) + ", " + ((int) headData2.type) + ", size: " + i4);
                    }
                    byte[] bArr4 = new byte[i4];
                    this.bs.get(bArr4);
                    return bArr4;
                default:
                    throw new JceDecodeException("type mismatch.");
            }
        }
        if (z) {
            throw new JceDecodeException("require field not exist.");
        }
        return bArr2;
    }

    public short[] read(short[] sArr, int i, boolean z) {
        short[] sArr2 = (short[]) null;
        if (skipToTag(i)) {
            HeadData headData = new HeadData();
            readHead(headData);
            switch (headData.type) {
                case Interpreter.ERR_ARR_OUT /* 9 */:
                    int i2 = read(0, 0, true);
                    if (i2 < 0) {
                        throw new JceDecodeException("size invalid: " + i2);
                    }
                    sArr2 = new short[i2];
                    for (int i3 = 0; i3 < i2; i3++) {
                        sArr2[i3] = read(sArr2[0], 0, true);
                    }
                    break;
                    break;
                default:
                    throw new JceDecodeException("type mismatch.");
            }
        } else if (z) {
            throw new JceDecodeException("require field not exist.");
        }
        return sArr2;
    }

    public int[] read(int[] iArr, int i, boolean z) {
        int[] iArr2 = (int[]) null;
        if (skipToTag(i)) {
            HeadData headData = new HeadData();
            readHead(headData);
            switch (headData.type) {
                case Interpreter.ERR_ARR_OUT /* 9 */:
                    int i2 = read(0, 0, true);
                    if (i2 < 0) {
                        throw new JceDecodeException("size invalid: " + i2);
                    }
                    iArr2 = new int[i2];
                    for (int i3 = 0; i3 < i2; i3++) {
                        iArr2[i3] = read(iArr2[0], 0, true);
                    }
                    break;
                    break;
                default:
                    throw new JceDecodeException("type mismatch.");
            }
        } else if (z) {
            throw new JceDecodeException("require field not exist.");
        }
        return iArr2;
    }

    public long[] read(long[] jArr, int i, boolean z) {
        long[] jArr2 = (long[]) null;
        if (skipToTag(i)) {
            HeadData headData = new HeadData();
            readHead(headData);
            switch (headData.type) {
                case Interpreter.ERR_ARR_OUT /* 9 */:
                    int i2 = read(0, 0, true);
                    if (i2 < 0) {
                        throw new JceDecodeException("size invalid: " + i2);
                    }
                    jArr2 = new long[i2];
                    for (int i3 = 0; i3 < i2; i3++) {
                        jArr2[i3] = read(jArr2[0], 0, true);
                    }
                    break;
                    break;
                default:
                    throw new JceDecodeException("type mismatch.");
            }
        } else if (z) {
            throw new JceDecodeException("require field not exist.");
        }
        return jArr2;
    }

    public float[] read(float[] fArr, int i, boolean z) {
        float[] fArr2 = (float[]) null;
        if (skipToTag(i)) {
            HeadData headData = new HeadData();
            readHead(headData);
            switch (headData.type) {
                case Interpreter.ERR_ARR_OUT /* 9 */:
                    int i2 = read(0, 0, true);
                    if (i2 < 0) {
                        throw new JceDecodeException("size invalid: " + i2);
                    }
                    fArr2 = new float[i2];
                    for (int i3 = 0; i3 < i2; i3++) {
                        fArr2[i3] = read(fArr2[0], 0, true);
                    }
                    break;
                    break;
                default:
                    throw new JceDecodeException("type mismatch.");
            }
        } else if (z) {
            throw new JceDecodeException("require field not exist.");
        }
        return fArr2;
    }

    public double[] read(double[] dArr, int i, boolean z) {
        double[] dArr2 = (double[]) null;
        if (skipToTag(i)) {
            HeadData headData = new HeadData();
            readHead(headData);
            switch (headData.type) {
                case Interpreter.ERR_ARR_OUT /* 9 */:
                    int i2 = read(0, 0, true);
                    if (i2 < 0) {
                        throw new JceDecodeException("size invalid: " + i2);
                    }
                    dArr2 = new double[i2];
                    for (int i3 = 0; i3 < i2; i3++) {
                        dArr2[i3] = read(dArr2[0], 0, true);
                    }
                    break;
                    break;
                default:
                    throw new JceDecodeException("type mismatch.");
            }
        } else if (z) {
            throw new JceDecodeException("require field not exist.");
        }
        return dArr2;
    }

    public Object[] readArray(Object[] objArr, int i, boolean z) {
        if (objArr == null || objArr.length == 0) {
            throw new JceDecodeException("unable to get type of key and value.");
        }
        return readArrayImpl(objArr[0], i, z);
    }

    public List readArray(List list, int i, boolean z) {
        if (list == null || list.isEmpty()) {
            return new ArrayList();
        }
        Object[] arrayImpl = readArrayImpl(list.get(0), i, z);
        if (arrayImpl == null) {
            return null;
        }
        ArrayList arrayList = new ArrayList();
        for (Object obj : arrayImpl) {
            arrayList.add(obj);
        }
        return arrayList;
    }

    private Object[] readArrayImpl(Object obj, int i, boolean z) {
        if (skipToTag(i)) {
            HeadData headData = new HeadData();
            readHead(headData);
            switch (headData.type) {
                case Interpreter.ERR_ARR_OUT /* 9 */:
                    int i2 = read(0, 0, true);
                    if (i2 < 0) {
                        throw new JceDecodeException("size invalid: " + i2);
                    }
                    Object[] objArr = (Object[]) Array.newInstance(obj.getClass(), i2);
                    for (int i3 = 0; i3 < i2; i3++) {
                        objArr[i3] = read(obj, 0, true);
                    }
                    return objArr;
                default:
                    throw new JceDecodeException("type mismatch.");
            }
        }
        if (z) {
            throw new JceDecodeException("require field not exist.");
        }
        return null;
    }

    public JceStruct directRead(JceStruct jceStruct, int i, boolean z) {
        JceStruct jceStructNewInit = null;
        if (skipToTag(i)) {
            try {
                jceStructNewInit = jceStruct.newInit();
                HeadData headData = new HeadData();
                readHead(headData);
                if (headData.type != 10) {
                    throw new JceDecodeException("type mismatch.");
                }
                jceStructNewInit.readFrom(this);
                skipToStructEnd();
            } catch (Exception e) {
                throw new JceDecodeException(e.getMessage());
            }
        } else if (z) {
            throw new JceDecodeException("require field not exist.");
        }
        return jceStructNewInit;
    }

    public JceStruct read(JceStruct jceStruct, int i, boolean z) {
        JceStruct jceStruct2 = null;
        if (skipToTag(i)) {
            try {
                jceStruct2 = (JceStruct) jceStruct.getClass().newInstance();
                HeadData headData = new HeadData();
                readHead(headData);
                if (headData.type != 10) {
                    throw new JceDecodeException("type mismatch.");
                }
                jceStruct2.readFrom(this);
                skipToStructEnd();
            } catch (Exception e) {
                throw new JceDecodeException(e.getMessage());
            }
        } else if (z) {
            throw new JceDecodeException("require field not exist.");
        }
        return jceStruct2;
    }

    public JceStruct[] read(JceStruct[] jceStructArr, int i, boolean z) {
        return (JceStruct[]) readArray(jceStructArr, i, z);
    }

    public Object read(Object obj, int i, boolean z) {
        if (obj instanceof Byte) {
            return Byte.valueOf(read((byte) 0, i, z));
        }
        if (obj instanceof Boolean) {
            return Boolean.valueOf(read(false, i, z));
        }
        if (obj instanceof Short) {
            return Short.valueOf(read((short) 0, i, z));
        }
        if (obj instanceof Integer) {
            return Integer.valueOf(read(0, i, z));
        }
        if (obj instanceof Long) {
            return Long.valueOf(read(0L, i, z));
        }
        if (obj instanceof Float) {
            return Float.valueOf(read(0.0f, i, z));
        }
        if (obj instanceof Double) {
            return Double.valueOf(read(0.0d, i, z));
        }
        if (obj instanceof String) {
            return String.valueOf(readString(i, z));
        }
        if (obj instanceof Map) {
            return readMap((Map) obj, i, z);
        }
        if (obj instanceof List) {
            return readArray((List) obj, i, z);
        }
        if (obj instanceof JceStruct) {
            return read((JceStruct) obj, i, z);
        }
        if (obj.getClass().isArray()) {
            if ((obj instanceof byte[]) || (obj instanceof Byte[])) {
                return read((byte[]) null, i, z);
            }
            if (obj instanceof boolean[]) {
                return read((boolean[]) null, i, z);
            }
            if (obj instanceof short[]) {
                return read((short[]) null, i, z);
            }
            if (obj instanceof int[]) {
                return read((int[]) null, i, z);
            }
            if (obj instanceof long[]) {
                return read((long[]) null, i, z);
            }
            if (obj instanceof float[]) {
                return read((float[]) null, i, z);
            }
            if (obj instanceof double[]) {
                return read((double[]) null, i, z);
            }
            return readArray((Object[]) obj, i, z);
        }
        throw new JceDecodeException("read object error: unsupport type.");
    }

    public int setServerEncoding(String str) {
        this.sServerEncoding = str;
        return 0;
    }

    public static void main(String[] strArr) {
    }

    public ByteBuffer getBs() {
        return this.bs;
    }
}
