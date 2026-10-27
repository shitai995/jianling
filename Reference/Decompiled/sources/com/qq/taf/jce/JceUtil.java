package com.qq.taf.jce;

import android.support.v4.view.accessibility.AccessibilityEventCompat;
import java.nio.ByteBuffer;
import java.util.Iterator;
import java.util.List;

/* JADX INFO: loaded from: classes.dex */
public final class JceUtil {
    private static final byte[] highDigits;
    private static final byte[] lowDigits;

    public static boolean equals(boolean z, boolean z2) {
        return z == z2;
    }

    public static boolean equals(byte b, byte b2) {
        return b == b2;
    }

    public static boolean equals(char c, char c2) {
        return c == c2;
    }

    public static boolean equals(short s, short s2) {
        return s == s2;
    }

    public static boolean equals(int i, int i2) {
        return i == i2;
    }

    public static boolean equals(long j, long j2) {
        return j == j2;
    }

    public static boolean equals(float f, float f2) {
        return f == f2;
    }

    public static boolean equals(double d, double d2) {
        return d == d2;
    }

    public static boolean equals(Object obj, Object obj2) {
        return obj.equals(obj2);
    }

    public static int compareTo(boolean z, boolean z2) {
        return (z ? 1 : 0) - (z2 ? 1 : 0);
    }

    public static int compareTo(byte b, byte b2) {
        if (b < b2) {
            return -1;
        }
        return b > b2 ? 1 : 0;
    }

    public static int compareTo(char c, char c2) {
        if (c < c2) {
            return -1;
        }
        return c > c2 ? 1 : 0;
    }

    public static int compareTo(short s, short s2) {
        if (s < s2) {
            return -1;
        }
        return s > s2 ? 1 : 0;
    }

    public static int compareTo(int i, int i2) {
        if (i < i2) {
            return -1;
        }
        return i > i2 ? 1 : 0;
    }

    public static int compareTo(long j, long j2) {
        if (j < j2) {
            return -1;
        }
        return j > j2 ? 1 : 0;
    }

    public static int compareTo(float f, float f2) {
        if (f < f2) {
            return -1;
        }
        return f > f2 ? 1 : 0;
    }

    public static int compareTo(double d, double d2) {
        if (d < d2) {
            return -1;
        }
        return d > d2 ? 1 : 0;
    }

    public static int compareTo(Comparable comparable, Comparable comparable2) {
        return comparable.compareTo(comparable2);
    }

    public static int compareTo(List list, List list2) {
        Iterator it = list.iterator();
        Iterator it2 = list2.iterator();
        while (it.hasNext() && it2.hasNext()) {
            int iCompareTo = ((Comparable) it.next()).compareTo((Comparable) it2.next());
            if (iCompareTo != 0) {
                return iCompareTo;
            }
        }
        return compareTo(it.hasNext(), it2.hasNext());
    }

    public static int compareTo(Comparable[] comparableArr, Comparable[] comparableArr2) {
        int i = 0;
        for (int i2 = 0; i < comparableArr.length && i2 < comparableArr2.length; i2++) {
            int iCompareTo = comparableArr[i].compareTo(comparableArr2[i2]);
            if (iCompareTo != 0) {
                return iCompareTo;
            }
            i++;
        }
        return compareTo(comparableArr.length, comparableArr2.length);
    }

    public static int compareTo(boolean[] zArr, boolean[] zArr2) {
        int i = 0;
        for (int i2 = 0; i < zArr.length && i2 < zArr2.length; i2++) {
            int iCompareTo = compareTo(zArr[i], zArr2[i2]);
            if (iCompareTo != 0) {
                return iCompareTo;
            }
            i++;
        }
        return compareTo(zArr.length, zArr2.length);
    }

    public static int compareTo(byte[] bArr, byte[] bArr2) {
        int i = 0;
        for (int i2 = 0; i < bArr.length && i2 < bArr2.length; i2++) {
            int iCompareTo = compareTo(bArr[i], bArr2[i2]);
            if (iCompareTo != 0) {
                return iCompareTo;
            }
            i++;
        }
        return compareTo(bArr.length, bArr2.length);
    }

    public static int compareTo(char[] cArr, char[] cArr2) {
        int i = 0;
        for (int i2 = 0; i < cArr.length && i2 < cArr2.length; i2++) {
            int iCompareTo = compareTo(cArr[i], cArr2[i2]);
            if (iCompareTo != 0) {
                return iCompareTo;
            }
            i++;
        }
        return compareTo(cArr.length, cArr2.length);
    }

    public static int compareTo(short[] sArr, short[] sArr2) {
        int i = 0;
        for (int i2 = 0; i < sArr.length && i2 < sArr2.length; i2++) {
            int iCompareTo = compareTo(sArr[i], sArr2[i2]);
            if (iCompareTo != 0) {
                return iCompareTo;
            }
            i++;
        }
        return compareTo(sArr.length, sArr2.length);
    }

    public static int compareTo(int[] iArr, int[] iArr2) {
        int i = 0;
        for (int i2 = 0; i < iArr.length && i2 < iArr2.length; i2++) {
            int iCompareTo = compareTo(iArr[i], iArr2[i2]);
            if (iCompareTo != 0) {
                return iCompareTo;
            }
            i++;
        }
        return compareTo(iArr.length, iArr2.length);
    }

    public static int compareTo(long[] jArr, long[] jArr2) {
        int i = 0;
        for (int i2 = 0; i < jArr.length && i2 < jArr2.length; i2++) {
            int iCompareTo = compareTo(jArr[i], jArr2[i2]);
            if (iCompareTo != 0) {
                return iCompareTo;
            }
            i++;
        }
        return compareTo(jArr.length, jArr2.length);
    }

    public static int compareTo(float[] fArr, float[] fArr2) {
        int i = 0;
        for (int i2 = 0; i < fArr.length && i2 < fArr2.length; i2++) {
            int iCompareTo = compareTo(fArr[i], fArr2[i2]);
            if (iCompareTo != 0) {
                return iCompareTo;
            }
            i++;
        }
        return compareTo(fArr.length, fArr2.length);
    }

    public static int compareTo(double[] dArr, double[] dArr2) {
        int i = 0;
        for (int i2 = 0; i < dArr.length && i2 < dArr2.length; i2++) {
            int iCompareTo = compareTo(dArr[i], dArr2[i2]);
            if (iCompareTo != 0) {
                return iCompareTo;
            }
            i++;
        }
        return compareTo(dArr.length, dArr2.length);
    }

    public static int hashCode(boolean z) {
        return z ? 1 : 0;
    }

    public static int hashCode(byte b) {
        return b;
    }

    public static int hashCode(char c) {
        return c;
    }

    public static int hashCode(short s) {
        return s;
    }

    public static int hashCode(int i) {
        return i;
    }

    public static int hashCode(long j) {
        return (int) j;
    }

    public static int hashCode(float f) {
        return new Float(f).hashCode();
    }

    public static int hashCode(double d) {
        return new Double(d).hashCode();
    }

    public static int hashCode(Object obj) {
        return obj.hashCode();
    }

    public static byte[] getJceBufArray(ByteBuffer byteBuffer) {
        byte[] bArr = new byte[byteBuffer.position()];
        System.arraycopy(byteBuffer.array(), 0, bArr, 0, bArr.length);
        return bArr;
    }

    static {
        byte[] bArr = {48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 65, 66, 67, 68, 69, 70};
        byte[] bArr2 = new byte[AccessibilityEventCompat.TYPE_VIEW_HOVER_EXIT];
        byte[] bArr3 = new byte[AccessibilityEventCompat.TYPE_VIEW_HOVER_EXIT];
        for (int i = 0; i < 256; i++) {
            bArr2[i] = bArr[i >>> 4];
            bArr3[i] = bArr[i & 15];
        }
        highDigits = bArr2;
        lowDigits = bArr3;
    }

    public static String getHexdump(byte[] bArr) {
        return getHexdump(ByteBuffer.wrap(bArr));
    }

    public static String getHexdump(ByteBuffer byteBuffer) {
        int iRemaining = byteBuffer.remaining();
        if (iRemaining == 0) {
            return "empty";
        }
        StringBuffer stringBuffer = new StringBuffer((byteBuffer.remaining() * 3) - 1);
        int iPosition = byteBuffer.position();
        int i = byteBuffer.get() & 255;
        stringBuffer.append((char) highDigits[i]);
        stringBuffer.append((char) lowDigits[i]);
        for (int i2 = iRemaining - 1; i2 > 0; i2--) {
            stringBuffer.append(' ');
            int i3 = byteBuffer.get() & 255;
            stringBuffer.append((char) highDigits[i3]);
            stringBuffer.append((char) lowDigits[i3]);
        }
        byteBuffer.position(iPosition);
        return stringBuffer.toString();
    }
}
