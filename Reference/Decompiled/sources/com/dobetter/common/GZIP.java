package com.dobetter.common;

import android.support.v4.app.FragmentTransaction;
import android.support.v4.view.MotionEventCompat;
import java.io.IOException;

/* JADX INFO: loaded from: classes.dex */
public class GZIP {
    private static final int BTYPE_DYNAMIC = 2;
    private static final int BTYPE_FIXED = 1;
    private static final int BTYPE_NONE = 0;
    private static final int BTYPE_RESERVED = 3;
    private static final int EOB_CODE = 256;
    private static final int FCOMMENT_MASK = 16;
    private static final int FEXTRA_MASK = 4;
    private static final int FHCRC_MASK = 2;
    private static final int FNAME_MASK = 8;
    private static final int FTEXT_MASK = 1;
    private static final int MAX_BITS = 16;
    private static final int MAX_CODE_LITERALS = 287;
    private static int gzipBit;
    private static int gzipByte;
    private static int gzipIndex;
    private static final int[] LENGTH_EXTRA_BITS = {0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 4, 4, 4, 4, 5, 5, 5, 5, 0, 99, 99};
    private static final int MAX_CODE_DISTANCES = 31;
    private static final int[] LENGTH_VALUES = {3, 4, 5, 6, 7, 8, 9, 10, 11, 13, 15, 17, 19, 23, 27, MAX_CODE_DISTANCES, 35, 43, 51, 59, 67, 83, 99, 115, 131, 163, 195, 227, 258, 0, 0};
    private static final int[] DISTANCE_EXTRA_BITS = {0, 0, 0, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 7, 7, 8, 8, 9, 9, 10, 10, 11, 11, 12, 12, 13, 13};
    private static final int[] DISTANCE_VALUES = {1, 2, 3, 4, 5, 7, 9, 13, 17, 25, 33, 49, 65, 97, 129, 193, 257, 385, 513, 769, 1025, 1537, 2049, 3073, FragmentTransaction.TRANSIT_FRAGMENT_OPEN, 6145, 8193, 12289, 16385, 24577};
    private static final int MAX_CODE_LENGTHS = 18;
    private static final int[] DYNAMIC_LENGTH_ORDER = {16, 17, MAX_CODE_LENGTHS, 0, 8, 7, 9, 6, 10, 5, 11, 4, 12, 3, 13, 2, 14, 1, 15};

    public static byte[] inflate(byte[] gzip) throws IOException {
        int bfinal;
        int[] literalTree;
        int[] distanceTree;
        int i;
        int i2;
        gzipBit = 0;
        gzipByte = 0;
        gzipIndex = 0;
        if (readBits(gzip, 16) != 35615 || readBits(gzip, 8) != 8) {
            throw new IOException("Invalid GZIP format");
        }
        int flg = readBits(gzip, 8);
        gzipIndex += 6;
        if ((flg & 4) != 0) {
            gzipIndex += readBits(gzip, 16);
        }
        if ((flg & 8) != 0) {
            do {
                i2 = gzipIndex;
                gzipIndex = i2 + 1;
            } while (gzip[i2] != 0);
        }
        if ((flg & 16) != 0) {
            do {
                i = gzipIndex;
                gzipIndex = i + 1;
            } while (gzip[i] != 0);
        }
        if ((flg & 2) != 0) {
            gzipIndex += 2;
        }
        int index = gzipIndex;
        gzipIndex = gzip.length - 4;
        byte[] uncompressed = new byte[readBits(gzip, 16) | (readBits(gzip, 16) << 16)];
        int uncompressedIndex = 0;
        gzipIndex = index;
        do {
            bfinal = readBits(gzip, 1);
            int btype = readBits(gzip, 2);
            if (btype == 0) {
                gzipBit = 0;
                int len = readBits(gzip, 16);
                readBits(gzip, 16);
                System.arraycopy(gzip, gzipIndex, uncompressed, uncompressedIndex, len);
                gzipIndex += len;
                uncompressedIndex += len;
            } else {
                if (btype == 2) {
                    int hlit = readBits(gzip, 5) + 257;
                    int hdist = readBits(gzip, 5) + 1;
                    int hclen = readBits(gzip, 4) + 4;
                    byte[] lengthBits = new byte[19];
                    for (int i3 = 0; i3 < hclen; i3++) {
                        lengthBits[DYNAMIC_LENGTH_ORDER[i3]] = (byte) readBits(gzip, 3);
                    }
                    int[] lengthTree = createHuffmanTree(lengthBits, MAX_CODE_LENGTHS);
                    literalTree = createHuffmanTree(decodeCodeLengths(gzip, lengthTree, hlit), hlit - 1);
                    distanceTree = createHuffmanTree(decodeCodeLengths(gzip, lengthTree, hdist), hdist - 1);
                } else {
                    byte[] literalBits = new byte[288];
                    for (int i4 = 0; i4 < 144; i4++) {
                        literalBits[i4] = 8;
                    }
                    for (int i5 = 144; i5 < 256; i5++) {
                        literalBits[i5] = 9;
                    }
                    for (int i6 = 256; i6 < 280; i6++) {
                        literalBits[i6] = 7;
                    }
                    for (int i7 = 280; i7 < 288; i7++) {
                        literalBits[i7] = 8;
                    }
                    literalTree = createHuffmanTree(literalBits, MAX_CODE_LITERALS);
                    byte[] distanceBits = new byte[32];
                    for (int i8 = 0; i8 < distanceBits.length; i8++) {
                        distanceBits[i8] = 5;
                    }
                    distanceTree = createHuffmanTree(distanceBits, MAX_CODE_DISTANCES);
                }
                while (true) {
                    int code = readCode(gzip, literalTree);
                    if (code == 256) {
                        break;
                    }
                    if (code > 256) {
                        int code2 = code - 257;
                        int length = LENGTH_VALUES[code2];
                        int leb = LENGTH_EXTRA_BITS[code2];
                        if (leb > 0) {
                            length += readBits(gzip, leb);
                        }
                        int code3 = readCode(gzip, distanceTree);
                        int distance = DISTANCE_VALUES[code3];
                        int deb = DISTANCE_EXTRA_BITS[code3];
                        if (deb > 0) {
                            distance += readBits(gzip, deb);
                        }
                        int offset = uncompressedIndex - distance;
                        while (distance < length) {
                            System.arraycopy(uncompressed, offset, uncompressed, uncompressedIndex, distance);
                            uncompressedIndex += distance;
                            length -= distance;
                            distance <<= 1;
                        }
                        System.arraycopy(uncompressed, offset, uncompressed, uncompressedIndex, length);
                        uncompressedIndex += length;
                    } else {
                        uncompressed[uncompressedIndex] = (byte) code;
                        uncompressedIndex++;
                    }
                }
            }
        } while (bfinal == 0);
        return uncompressed;
    }

    private static int readBits(byte[] gzip, int n) {
        int data;
        if (gzipBit == 0) {
            int i = gzipIndex;
            gzipIndex = i + 1;
            data = gzip[i] & MotionEventCompat.ACTION_MASK;
            gzipByte = data;
        } else {
            data = gzipByte >> gzipBit;
        }
        for (int i2 = 8 - gzipBit; i2 < n; i2 += 8) {
            int i3 = gzipIndex;
            gzipIndex = i3 + 1;
            gzipByte = gzip[i3] & 255;
            data |= gzipByte << i2;
        }
        gzipBit = (gzipBit + n) & 7;
        return ((1 << n) - 1) & data;
    }

    private static int readCode(byte[] gzip, int[] tree) {
        int node = tree[0];
        while (node >= 0) {
            if (gzipBit == 0) {
                int i = gzipIndex;
                gzipIndex = i + 1;
                gzipByte = gzip[i] & 255;
            }
            node = (gzipByte & (1 << gzipBit)) == 0 ? tree[node >> 16] : tree[node & 65535];
            gzipBit = (gzipBit + 1) & 7;
        }
        return node & 65535;
    }

    private static byte[] decodeCodeLengths(byte[] gzip, int[] lengthTree, int count) {
        int i;
        int repeat;
        int repeat2;
        byte[] bits = new byte[count];
        int last = 0;
        int i2 = 0;
        while (i2 < count) {
            int code = readCode(gzip, lengthTree);
            if (code >= 16) {
                if (code == 16) {
                    int repeat3 = readBits(gzip, 2) + 3;
                    code = last;
                    repeat2 = repeat3;
                } else {
                    if (code == 17) {
                        repeat = readBits(gzip, 3) + 3;
                    } else {
                        repeat = readBits(gzip, 7) + 11;
                    }
                    code = 0;
                    repeat2 = repeat;
                }
                while (true) {
                    int repeat4 = repeat2 - 1;
                    if (repeat2 <= 0) {
                        break;
                    }
                    bits[i2] = (byte) code;
                    repeat2 = repeat4;
                    i2++;
                }
                i = i2;
            } else {
                i = i2 + 1;
                bits[i2] = (byte) code;
            }
            last = code;
            i2 = i;
        }
        return bits;
    }

    private static int[] createHuffmanTree(byte[] bits, int maxCode) {
        int treeInsert;
        int[] bl_count = new int[17];
        for (byte b : bits) {
            bl_count[b] = bl_count[b] + 1;
        }
        int code = 0;
        bl_count[0] = 0;
        int[] next_code = new int[17];
        for (int i = 1; i <= 16; i++) {
            code = (bl_count[i - 1] + code) << 1;
            next_code[i] = code;
        }
        int[] tree = new int[(maxCode << 1) + 16];
        int treeInsert2 = 1;
        for (int i2 = 0; i2 <= maxCode; i2++) {
            int len = bits[i2];
            if (len != 0) {
                int code2 = next_code[len];
                next_code[len] = code2 + 1;
                int node = 0;
                int bit = len - 1;
                int treeInsert3 = treeInsert2;
                while (bit >= 0) {
                    int value = code2 & (1 << bit);
                    if (value == 0) {
                        int left = tree[node] >> 16;
                        if (left == 0) {
                            tree[node] = tree[node] | (treeInsert3 << 16);
                            treeInsert = treeInsert3 + 1;
                            node = treeInsert3;
                        } else {
                            node = left;
                            treeInsert = treeInsert3;
                        }
                    } else {
                        int right = tree[node] & 65535;
                        if (right == 0) {
                            tree[node] = tree[node] | treeInsert3;
                            treeInsert = treeInsert3 + 1;
                            node = treeInsert3;
                        } else {
                            node = right;
                            treeInsert = treeInsert3;
                        }
                    }
                    bit--;
                    treeInsert3 = treeInsert;
                }
                tree[node] = Integer.MIN_VALUE | i2;
                treeInsert2 = treeInsert3;
            }
        }
        return tree;
    }
}
