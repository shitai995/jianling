package com.tencent.webnet;

import java.io.ByteArrayInputStream;
import java.io.InputStream;
import java.io.UnsupportedEncodingException;

/* JADX INFO: loaded from: classes.dex */
public class i {
    static final int dA = 9;
    static final int dB = 14;
    static final int dC = 20;
    static final int dD = 4;
    static final int dE = 11;
    static final int dF = 16;
    static final int dG = 23;
    static final int dH = 6;
    static final int dI = 10;
    static final int dJ = 15;
    static final int dK = 21;
    static final byte[] dL;
    private static final char[] dQ;
    static final int dv = 7;
    static final int dw = 12;
    static final int dx = 17;
    static final int dy = 22;
    static final int dz = 5;
    public String dO;
    private long[] dM = new long[4];
    private long[] dN = new long[2];
    private byte[] buffer = new byte[64];
    private byte[] dP = new byte[16];

    static {
        byte[] bArr = new byte[64];
        bArr[0] = -128;
        dL = bArr;
        dQ = new char[]{'0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f'};
    }

    public byte[] G(String str) {
        return m(str.getBytes());
    }

    public byte[] m(byte[] bArr) {
        aS();
        b(new ByteArrayInputStream(bArr), bArr.length);
        aT();
        return this.dP;
    }

    public byte[] a(InputStream inputStream, long j) {
        aS();
        if (!b(inputStream, j)) {
            return new byte[16];
        }
        aT();
        return this.dP;
    }

    public i() {
        aS();
    }

    private void aS() {
        this.dN[0] = 0;
        this.dN[1] = 0;
        this.dM[0] = 1732584193;
        this.dM[1] = 4023233417L;
        this.dM[2] = 2562383102L;
        this.dM[3] = 271733878;
    }

    private long a(long j, long j2, long j3) {
        return (j & j2) | (((-1) ^ j) & j3);
    }

    private long b(long j, long j2, long j3) {
        return (j & j3) | (((-1) ^ j3) & j2);
    }

    private long c(long j, long j2, long j3) {
        return (j ^ j2) ^ j3;
    }

    private long d(long j, long j2, long j3) {
        return (((-1) ^ j3) | j) ^ j2;
    }

    private long a(long j, long j2, long j3, long j4, long j5, long j6, long j7) {
        long jA = a(j2, j3, j4) + j5 + j7 + j;
        return ((long) ((((int) jA) >>> ((int) (32 - j6))) | (((int) jA) << ((int) j6)))) + j2;
    }

    private long b(long j, long j2, long j3, long j4, long j5, long j6, long j7) {
        long jB = b(j2, j3, j4) + j5 + j7 + j;
        return ((long) ((((int) jB) >>> ((int) (32 - j6))) | (((int) jB) << ((int) j6)))) + j2;
    }

    private long c(long j, long j2, long j3, long j4, long j5, long j6, long j7) {
        long jC = c(j2, j3, j4) + j5 + j7 + j;
        return ((long) ((((int) jC) >>> ((int) (32 - j6))) | (((int) jC) << ((int) j6)))) + j2;
    }

    private long d(long j, long j2, long j3, long j4, long j5, long j6, long j7) {
        long jD = d(j2, j3, j4) + j5 + j7 + j;
        return ((long) ((((int) jD) >>> ((int) (32 - j6))) | (((int) jD) << ((int) j6)))) + j2;
    }

    private boolean b(InputStream inputStream, long j) {
        int i;
        byte[] bArr = new byte[64];
        int i2 = ((int) (this.dN[0] >>> 3)) & 63;
        long[] jArr = this.dN;
        long j2 = jArr[0] + (j << 3);
        jArr[0] = j2;
        if (j2 < (j << 3)) {
            long[] jArr2 = this.dN;
            jArr2[1] = jArr2[1] + 1;
        }
        long[] jArr3 = this.dN;
        jArr3[1] = jArr3[1] + (j >>> 29);
        int i3 = 64 - i2;
        if (j >= i3) {
            byte[] bArr2 = new byte[i3];
            try {
                inputStream.read(bArr2, 0, i3);
                a(this.buffer, bArr2, i2, 0, i3);
                n(this.buffer);
                i = i3;
                while (i + 63 < j) {
                    try {
                        inputStream.read(bArr);
                        n(bArr);
                        i += 64;
                    } catch (Exception e) {
                        return false;
                    }
                }
                i2 = 0;
            } catch (Exception e2) {
                return false;
            }
        } else {
            i = 0;
        }
        byte[] bArr3 = new byte[(int) (j - ((long) i))];
        try {
            inputStream.read(bArr3);
            a(this.buffer, bArr3, i2, 0, bArr3.length);
            return true;
        } catch (Exception e3) {
            return false;
        }
    }

    private void c(byte[] bArr, int i) {
        int i2 = 0;
        byte[] bArr2 = new byte[64];
        int i3 = ((int) (this.dN[0] >>> 3)) & 63;
        long[] jArr = this.dN;
        long j = jArr[0] + ((long) (i << 3));
        jArr[0] = j;
        if (j < (i << 3)) {
            long[] jArr2 = this.dN;
            jArr2[1] = jArr2[1] + 1;
        }
        long[] jArr3 = this.dN;
        jArr3[1] = jArr3[1] + ((long) (i >>> 29));
        int i4 = 64 - i3;
        if (i >= i4) {
            a(this.buffer, bArr, i3, 0, i4);
            n(this.buffer);
            while (i4 + 63 < i) {
                a(bArr2, bArr, 0, i4, 64);
                n(bArr2);
                i4 += 64;
            }
            i3 = 0;
            i2 = i4;
        }
        a(this.buffer, bArr, i3, i2, i - i2);
    }

    private void aT() {
        byte[] bArr = new byte[8];
        a(bArr, this.dN, 8);
        int i = ((int) (this.dN[0] >>> 3)) & 63;
        c(dL, i < 56 ? 56 - i : 120 - i);
        c(bArr, 8);
        a(this.dP, this.dM, 16);
    }

    private void a(byte[] bArr, byte[] bArr2, int i, int i2, int i3) {
        for (int i4 = 0; i4 < i3; i4++) {
            bArr[i + i4] = bArr2[i2 + i4];
        }
    }

    private void n(byte[] bArr) {
        long j = this.dM[0];
        long j2 = this.dM[1];
        long j3 = this.dM[2];
        long j4 = this.dM[3];
        long[] jArr = new long[16];
        a(jArr, bArr, 64);
        long jA = a(j, j2, j3, j4, jArr[0], 7L, 3614090360L);
        long jA2 = a(j4, jA, j2, j3, jArr[1], 12L, 3905402710L);
        long jA3 = a(j3, jA2, jA, j2, jArr[2], 17L, 606105819L);
        long jA4 = a(j2, jA3, jA2, jA, jArr[3], 22L, 3250441966L);
        long jA5 = a(jA, jA4, jA3, jA2, jArr[4], 7L, 4118548399L);
        long jA6 = a(jA2, jA5, jA4, jA3, jArr[5], 12L, 1200080426L);
        long jA7 = a(jA3, jA6, jA5, jA4, jArr[6], 17L, 2821735955L);
        long jA8 = a(jA4, jA7, jA6, jA5, jArr[7], 22L, 4249261313L);
        long jA9 = a(jA5, jA8, jA7, jA6, jArr[8], 7L, 1770035416L);
        long jA10 = a(jA6, jA9, jA8, jA7, jArr[9], 12L, 2336552879L);
        long jA11 = a(jA7, jA10, jA9, jA8, jArr[10], 17L, 4294925233L);
        long jA12 = a(jA8, jA11, jA10, jA9, jArr[11], 22L, 2304563134L);
        long jA13 = a(jA9, jA12, jA11, jA10, jArr[dw], 7L, 1804603682L);
        long jA14 = a(jA10, jA13, jA12, jA11, jArr[13], 12L, 4254626195L);
        long jA15 = a(jA11, jA14, jA13, jA12, jArr[14], 17L, 2792965006L);
        long jA16 = a(jA12, jA15, jA14, jA13, jArr[dJ], 22L, 1236535329L);
        long jB = b(jA13, jA16, jA15, jA14, jArr[1], 5L, 4129170786L);
        long jB2 = b(jA14, jB, jA16, jA15, jArr[6], 9L, 3225465664L);
        long jB3 = b(jA15, jB2, jB, jA16, jArr[11], 14L, 643717713L);
        long jB4 = b(jA16, jB3, jB2, jB, jArr[0], 20L, 3921069994L);
        long jB5 = b(jB, jB4, jB3, jB2, jArr[5], 5L, 3593408605L);
        long jB6 = b(jB2, jB5, jB4, jB3, jArr[10], 9L, 38016083L);
        long jB7 = b(jB3, jB6, jB5, jB4, jArr[dJ], 14L, 3634488961L);
        long jB8 = b(jB4, jB7, jB6, jB5, jArr[4], 20L, 3889429448L);
        long jB9 = b(jB5, jB8, jB7, jB6, jArr[9], 5L, 568446438L);
        long jB10 = b(jB6, jB9, jB8, jB7, jArr[14], 9L, 3275163606L);
        long jB11 = b(jB7, jB10, jB9, jB8, jArr[3], 14L, 4107603335L);
        long jB12 = b(jB8, jB11, jB10, jB9, jArr[8], 20L, 1163531501L);
        long jB13 = b(jB9, jB12, jB11, jB10, jArr[13], 5L, 2850285829L);
        long jB14 = b(jB10, jB13, jB12, jB11, jArr[2], 9L, 4243563512L);
        long jB15 = b(jB11, jB14, jB13, jB12, jArr[7], 14L, 1735328473L);
        long jB16 = b(jB12, jB15, jB14, jB13, jArr[dw], 20L, 2368359562L);
        long jC = c(jB13, jB16, jB15, jB14, jArr[5], 4L, 4294588738L);
        long jC2 = c(jB14, jC, jB16, jB15, jArr[8], 11L, 2272392833L);
        long jC3 = c(jB15, jC2, jC, jB16, jArr[11], 16L, 1839030562L);
        long jC4 = c(jB16, jC3, jC2, jC, jArr[14], 23L, 4259657740L);
        long jC5 = c(jC, jC4, jC3, jC2, jArr[1], 4L, 2763975236L);
        long jC6 = c(jC2, jC5, jC4, jC3, jArr[4], 11L, 1272893353L);
        long jC7 = c(jC3, jC6, jC5, jC4, jArr[7], 16L, 4139469664L);
        long jC8 = c(jC4, jC7, jC6, jC5, jArr[10], 23L, 3200236656L);
        long jC9 = c(jC5, jC8, jC7, jC6, jArr[13], 4L, 681279174L);
        long jC10 = c(jC6, jC9, jC8, jC7, jArr[0], 11L, 3936430074L);
        long jC11 = c(jC7, jC10, jC9, jC8, jArr[3], 16L, 3572445317L);
        long jC12 = c(jC8, jC11, jC10, jC9, jArr[6], 23L, 76029189L);
        long jC13 = c(jC9, jC12, jC11, jC10, jArr[9], 4L, 3654602809L);
        long jC14 = c(jC10, jC13, jC12, jC11, jArr[dw], 11L, 3873151461L);
        long jC15 = c(jC11, jC14, jC13, jC12, jArr[dJ], 16L, 530742520L);
        long jC16 = c(jC12, jC15, jC14, jC13, jArr[2], 23L, 3299628645L);
        long jD = d(jC13, jC16, jC15, jC14, jArr[0], 6L, 4096336452L);
        long jD2 = d(jC14, jD, jC16, jC15, jArr[7], 10L, 1126891415L);
        long jD3 = d(jC15, jD2, jD, jC16, jArr[14], 15L, 2878612391L);
        long jD4 = d(jC16, jD3, jD2, jD, jArr[5], 21L, 4237533241L);
        long jD5 = d(jD, jD4, jD3, jD2, jArr[dw], 6L, 1700485571L);
        long jD6 = d(jD2, jD5, jD4, jD3, jArr[3], 10L, 2399980690L);
        long jD7 = d(jD3, jD6, jD5, jD4, jArr[10], 15L, 4293915773L);
        long jD8 = d(jD4, jD7, jD6, jD5, jArr[1], 21L, 2240044497L);
        long jD9 = d(jD5, jD8, jD7, jD6, jArr[8], 6L, 1873313359L);
        long jD10 = d(jD6, jD9, jD8, jD7, jArr[dJ], 10L, 4264355552L);
        long jD11 = d(jD7, jD10, jD9, jD8, jArr[6], 15L, 2734768916L);
        long jD12 = d(jD8, jD11, jD10, jD9, jArr[13], 21L, 1309151649L);
        long jD13 = d(jD9, jD12, jD11, jD10, jArr[4], 6L, 4149444226L);
        long jD14 = d(jD10, jD13, jD12, jD11, jArr[11], 10L, 3174756917L);
        long jD15 = d(jD11, jD14, jD13, jD12, jArr[2], 15L, 718787259L);
        long jD16 = d(jD12, jD15, jD14, jD13, jArr[9], 21L, 3951481745L);
        long[] jArr2 = this.dM;
        jArr2[0] = jArr2[0] + jD13;
        long[] jArr3 = this.dM;
        jArr3[1] = jD16 + jArr3[1];
        long[] jArr4 = this.dM;
        jArr4[2] = jArr4[2] + jD15;
        long[] jArr5 = this.dM;
        jArr5[3] = jArr5[3] + jD14;
    }

    private void a(byte[] bArr, long[] jArr, int i) {
        int i2 = 0;
        for (int i3 = 0; i3 < i; i3 += 4) {
            bArr[i3] = (byte) (jArr[i2] & 255);
            bArr[i3 + 1] = (byte) ((jArr[i2] >>> 8) & 255);
            bArr[i3 + 2] = (byte) ((jArr[i2] >>> 16) & 255);
            bArr[i3 + 3] = (byte) ((jArr[i2] >>> 24) & 255);
            i2++;
        }
    }

    private void a(long[] jArr, byte[] bArr, int i) {
        int i2 = 0;
        for (int i3 = 0; i3 < i; i3 += 4) {
            jArr[i2] = a(bArr[i3]) | (a(bArr[i3 + 1]) << 8) | (a(bArr[i3 + 2]) << 16) | (a(bArr[i3 + 3]) << 24);
            i2++;
        }
    }

    public static long a(byte b) {
        int i = b;
        if (b < 0) {
            i = b & 255;
        }
        return i;
    }

    public static String b(byte b) {
        char[] cArr = {'0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F'};
        return new String(new char[]{cArr[(b >>> 4) & dJ], cArr[b & 15]});
    }

    public static byte[] o(byte[] bArr) {
        return new i().m(bArr);
    }

    public static byte[] H(String str) {
        byte[] bytes;
        try {
            bytes = str.getBytes("ISO8859_1");
        } catch (UnsupportedEncodingException e) {
            bytes = str.getBytes();
        }
        return new i().m(bytes);
    }

    public static byte[] c(InputStream inputStream, long j) {
        return new i().a(inputStream, j);
    }

    public static String p(byte[] bArr) {
        byte[] bArrM = new i().m(bArr);
        String str = "";
        for (int i = 0; i < 16; i++) {
            str = String.valueOf(str) + b(bArrM[i]);
        }
        return str;
    }

    public static String I(String str) {
        byte[] bytes;
        try {
            bytes = str.getBytes("ISO8859_1");
        } catch (UnsupportedEncodingException e) {
            bytes = str.getBytes();
        }
        byte[] bArrM = new i().m(bytes);
        String str2 = "";
        for (int i = 0; i < 16; i++) {
            str2 = String.valueOf(str2) + b(bArrM[i]);
        }
        return str2;
    }

    public static final byte[] J(String str) {
        if (str != null) {
            int length = str.length() / 2;
            byte[] bArr = new byte[length];
            for (int i = 0; i < length; i++) {
                bArr[i] = (byte) Integer.parseInt(str.substring(i * 2, (i * 2) + 2), 16);
            }
            return bArr;
        }
        return null;
    }

    public static final String q(byte[] bArr) {
        if (bArr != null) {
            char[] cArr = new char[bArr.length * 2];
            int i = 0;
            for (byte b : bArr) {
                int i2 = i + 1;
                cArr[i] = dQ[(b >> 4) & dJ];
                i = i2 + 1;
                cArr[i2] = dQ[b & 15];
            }
            return new String(cArr);
        }
        return null;
    }
}
