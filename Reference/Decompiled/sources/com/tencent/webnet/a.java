package com.tencent.webnet;

import android.support.v4.view.MotionEventCompat;
import java.io.ByteArrayOutputStream;
import java.io.DataOutputStream;
import java.io.IOException;
import java.util.Random;

/* JADX INFO: loaded from: classes.dex */
public class a {
    public static final int bI = 79764919;
    public static final int bJ = 2;
    public static final int bK = 7;
    private byte[] bL;
    private byte[] bM;
    private byte[] bN;
    private int bO;
    private int bP;
    private byte[] bQ;
    private int bS;
    private int padding;
    private int pos;
    private boolean bR = true;
    private Random bT = new Random();

    public static byte[] a(byte[] bArr, int i) {
        return new byte[2];
    }

    public static int i(byte[] bArr) {
        int length = bArr.length;
        int i = -1;
        int i2 = 0;
        while (i2 < length) {
            int i3 = i;
            byte b = bArr[i2];
            for (int i4 = 0; i4 < 8; i4++) {
                if (((b ^ i3) >>> 31) == 1) {
                    i3 = (i3 << 1) ^ bI;
                } else {
                    i3 <<= 1;
                }
                b = (byte) (b << 1);
            }
            i2++;
            i = i3;
        }
        return i ^ (-1);
    }

    public static byte[] a(int i, byte[] bArr) {
        short[] sArr = {(short) (65535 & i), (short) (i >>> 16)};
        short[] sArr2 = {(short) ((bArr[1] << 8) | bArr[0]), (short) ((bArr[3] << 8) | bArr[2]), (short) ((bArr[5] << 8) | bArr[4]), (short) ((bArr[7] << 8) | bArr[6])};
        short s = sArr[0];
        short s2 = sArr[1];
        short s3 = 32;
        short s4 = s2;
        short s5 = s;
        short s6 = 0;
        while (true) {
            short s7 = (short) (s3 - 1);
            if (s3 > 0) {
                short s8 = (short) (s6 + 12895);
                s5 = (short) (((((s4 << 4) + sArr2[0]) ^ (s4 + s8)) ^ ((s4 >> 5) + sArr2[1])) + s5);
                s4 = (short) (((((s5 << 4) + sArr2[2]) ^ (s5 + s8)) ^ ((s5 >> 5) + sArr2[3])) + s4);
                s6 = s8;
                s3 = s7;
            } else {
                return new byte[]{(byte) (s4 >> 8), (byte) (s4 & 255), (byte) (s5 >> 8), (byte) (s5 & 255)};
            }
        }
    }

    public static int a(long j, byte[] bArr) {
        short[] sArr = {(short) (65535 & j), (short) (j >> 16)};
        short[] sArr2 = {(short) ((bArr[1] << 8) | bArr[0]), (short) ((bArr[3] << 8) | bArr[2]), (short) ((bArr[5] << 8) | bArr[4]), (short) ((bArr[7] << 8) | bArr[6])};
        short s = 32;
        short s2 = sArr[0];
        short s3 = sArr[1];
        short s4 = (short) 412640;
        while (true) {
            short s5 = (short) (s - 1);
            if (s > 0) {
                s3 = (short) (s3 - ((((s2 << 4) + sArr2[2]) ^ (s2 + s4)) ^ ((s2 >> 5) + sArr2[3])));
                s2 = (short) (s2 - ((((s3 << 4) + sArr2[0]) ^ (s3 + s4)) ^ ((s3 >> 5) + sArr2[1])));
                s4 = (short) (s4 - 12895);
                s = s5;
            } else {
                sArr[0] = s2;
                sArr[1] = s3;
                return (sArr[1] << 16) | (sArr[0] & 65535);
            }
        }
    }

    public static long a(byte[] bArr, int i, int i2) {
        int i3;
        long j = 0;
        if (i2 > 8) {
            i3 = i + 8;
        } else {
            i3 = i + i2;
        }
        while (i < i3) {
            j = (j << 8) | ((long) (bArr[i] & 255));
            i++;
        }
        return (j >>> 32) | (4294967295L & j);
    }

    public byte[] a(byte[] bArr, int i, int i2, byte[] bArr2) {
        this.bP = 0;
        this.bO = 0;
        this.bQ = bArr2;
        byte[] bArr3 = new byte[i + 8];
        if (i2 % 8 != 0 || i2 < 16) {
            return null;
        }
        this.bM = b(bArr, i);
        this.pos = this.bM[0] & 7;
        int i3 = (i2 - this.pos) - 10;
        if (i3 < 0) {
            return null;
        }
        for (int i4 = i; i4 < bArr3.length; i4++) {
            bArr3[i4] = 0;
        }
        this.bN = new byte[i3];
        this.bP = 0;
        this.bO = 8;
        this.bS = 8;
        this.pos++;
        this.padding = 1;
        byte[] bArr4 = bArr3;
        while (this.padding <= 2) {
            if (this.pos < 8) {
                this.pos++;
                this.padding++;
            }
            if (this.pos == 8) {
                if (!b(bArr, i, i2)) {
                    return null;
                }
                bArr4 = bArr;
            }
        }
        int i5 = i3;
        byte[] bArr5 = bArr4;
        int i6 = 0;
        byte[] bArr6 = bArr5;
        while (i5 != 0) {
            if (this.pos < 8) {
                this.bN[i6] = (byte) (bArr6[(this.bP + i) + this.pos] ^ this.bM[this.pos]);
                i6++;
                i5--;
                this.pos++;
            }
            if (this.pos == 8) {
                this.bP = this.bO - 8;
                if (!b(bArr, i, i2)) {
                    return null;
                }
                bArr6 = bArr;
            }
        }
        this.padding = 1;
        byte[] bArr7 = bArr6;
        while (this.padding < 8) {
            if (this.pos < 8) {
                if ((bArr7[(this.bP + i) + this.pos] ^ this.bM[this.pos]) != 0) {
                    return null;
                }
                this.pos++;
            }
            if (this.pos == 8) {
                this.bP = this.bO;
                if (!b(bArr, i, i2)) {
                    return null;
                }
                bArr7 = bArr;
            }
            this.padding++;
        }
        return this.bN;
    }

    public byte[] a(byte[] bArr, byte[] bArr2) {
        return a(bArr, 0, bArr.length, bArr2);
    }

    public byte[] b(byte[] bArr, int i, int i2, byte[] bArr2) {
        int i3;
        int i4;
        this.bL = new byte[8];
        this.bM = new byte[8];
        this.pos = 1;
        this.padding = 0;
        this.bP = 0;
        this.bO = 0;
        this.bQ = bArr2;
        this.bR = true;
        this.pos = (i2 + 10) % 8;
        if (this.pos != 0) {
            this.pos = 8 - this.pos;
        }
        this.bN = new byte[this.pos + i2 + 10];
        this.bL[0] = (byte) ((aw() & 248) | this.pos);
        for (int i5 = 1; i5 <= this.pos; i5++) {
            this.bL[i5] = (byte) (aw() & MotionEventCompat.ACTION_MASK);
        }
        this.pos++;
        for (int i6 = 0; i6 < 8; i6++) {
            this.bM[i6] = 0;
        }
        this.padding = 1;
        while (this.padding <= 2) {
            if (this.pos < 8) {
                byte[] bArr3 = this.bL;
                int i7 = this.pos;
                this.pos = i7 + 1;
                bArr3[i7] = (byte) (aw() & MotionEventCompat.ACTION_MASK);
                this.padding++;
            }
            if (this.pos == 8) {
                av();
            }
        }
        int i8 = i;
        int i9 = i2;
        while (i9 > 0) {
            if (this.pos < 8) {
                byte[] bArr4 = this.bL;
                int i10 = this.pos;
                this.pos = i10 + 1;
                i3 = i8 + 1;
                bArr4[i10] = bArr[i8];
                i4 = i9 - 1;
            } else {
                i3 = i8;
                i4 = i9;
            }
            if (this.pos == 8) {
                av();
            }
            i9 = i4;
            i8 = i3;
        }
        this.padding = 1;
        while (this.padding <= 7) {
            if (this.pos < 8) {
                byte[] bArr5 = this.bL;
                int i11 = this.pos;
                this.pos = i11 + 1;
                bArr5[i11] = 0;
                this.padding++;
            }
            if (this.pos == 8) {
                av();
            }
        }
        return this.bN;
    }

    public byte[] b(byte[] bArr, byte[] bArr2) {
        return b(bArr, 0, bArr.length, bArr2);
    }

    private byte[] j(byte[] bArr) {
        int i = 16;
        try {
            long jA = a(bArr, 0, 4);
            long jA2 = a(bArr, 4, 4);
            long jA3 = a(this.bQ, 0, 4);
            long jA4 = a(this.bQ, 4, 4);
            long jA5 = a(this.bQ, 8, 4);
            long jA6 = a(this.bQ, 12, 4);
            long j = 0;
            long j2 = (-1640531527) & 4294967295L;
            while (true) {
                int i2 = i - 1;
                if (i > 0) {
                    j = (j + j2) & 4294967295L;
                    jA = (jA + ((((jA2 << 4) + jA3) ^ (jA2 + j)) ^ ((jA2 >>> 5) + jA4))) & 4294967295L;
                    jA2 = (jA2 + ((((jA << 4) + jA5) ^ (jA + j)) ^ ((jA >>> 5) + jA6))) & 4294967295L;
                    i = i2;
                } else {
                    ByteArrayOutputStream byteArrayOutputStream = new ByteArrayOutputStream(8);
                    DataOutputStream dataOutputStream = new DataOutputStream(byteArrayOutputStream);
                    dataOutputStream.writeInt((int) jA);
                    dataOutputStream.writeInt((int) jA2);
                    dataOutputStream.close();
                    return byteArrayOutputStream.toByteArray();
                }
            }
        } catch (IOException e) {
            return null;
        }
    }

    private byte[] b(byte[] bArr, int i) {
        int i2 = 16;
        try {
            long jA = a(bArr, i, 4);
            long jA2 = a(bArr, i + 4, 4);
            long jA3 = a(this.bQ, 0, 4);
            long jA4 = a(this.bQ, 4, 4);
            long jA5 = a(this.bQ, 8, 4);
            long jA6 = a(this.bQ, 12, 4);
            long j = (-478700656) & 4294967295L;
            long j2 = (-1640531527) & 4294967295L;
            while (true) {
                int i3 = i2 - 1;
                if (i2 > 0) {
                    jA2 = (jA2 - ((((jA << 4) + jA5) ^ (jA + j)) ^ ((jA >>> 5) + jA6))) & 4294967295L;
                    jA = (jA - ((((jA2 << 4) + jA3) ^ (jA2 + j)) ^ ((jA2 >>> 5) + jA4))) & 4294967295L;
                    j = (j - j2) & 4294967295L;
                    i2 = i3;
                } else {
                    ByteArrayOutputStream byteArrayOutputStream = new ByteArrayOutputStream(8);
                    DataOutputStream dataOutputStream = new DataOutputStream(byteArrayOutputStream);
                    dataOutputStream.writeInt((int) jA);
                    dataOutputStream.writeInt((int) jA2);
                    dataOutputStream.close();
                    return byteArrayOutputStream.toByteArray();
                }
            }
        } catch (IOException e) {
            return null;
        }
    }

    private byte[] k(byte[] bArr) {
        return b(bArr, 0);
    }

    private void av() {
        this.pos = 0;
        while (this.pos < 8) {
            if (this.bR) {
                byte[] bArr = this.bL;
                int i = this.pos;
                bArr[i] = (byte) (bArr[i] ^ this.bM[this.pos]);
            } else {
                byte[] bArr2 = this.bL;
                int i2 = this.pos;
                bArr2[i2] = (byte) (bArr2[i2] ^ this.bN[this.bP + this.pos]);
            }
            this.pos++;
        }
        System.arraycopy(j(this.bL), 0, this.bN, this.bO, 8);
        this.pos = 0;
        while (this.pos < 8) {
            byte[] bArr3 = this.bN;
            int i3 = this.bO + this.pos;
            bArr3[i3] = (byte) (bArr3[i3] ^ this.bM[this.pos]);
            this.pos++;
        }
        System.arraycopy(this.bL, 0, this.bM, 0, 8);
        this.bP = this.bO;
        this.bO += 8;
        this.pos = 0;
        this.bR = false;
    }

    private boolean b(byte[] bArr, int i, int i2) {
        this.pos = 0;
        while (this.pos < 8) {
            if (this.bS + this.pos >= i2) {
                return true;
            }
            byte[] bArr2 = this.bM;
            int i3 = this.pos;
            bArr2[i3] = (byte) (bArr2[i3] ^ bArr[(this.bO + i) + this.pos]);
            this.pos++;
        }
        this.bM = k(this.bM);
        if (this.bM == null) {
            return false;
        }
        this.bS += 8;
        this.bO += 8;
        this.pos = 0;
        return true;
    }

    private int aw() {
        return this.bT.nextInt();
    }
}
