package com.qq.taf.jce.dynamic;

import android.support.v4.view.accessibility.AccessibilityEventCompat;
import com.dobetter.script.Interpreter;
import com.qq.taf.jce.JceDecodeException;
import com.qq.taf.jce.JceInputStream;
import java.io.UnsupportedEncodingException;
import java.nio.BufferUnderflowException;
import java.nio.ByteBuffer;
import java.util.ArrayList;

/* JADX INFO: loaded from: classes.dex */
public final class DynamicInputStream {
    private ByteBuffer bs;
    private String sServerEncoding = "GBK";

    public DynamicInputStream(ByteBuffer byteBuffer) {
        this.bs = byteBuffer;
    }

    public DynamicInputStream(byte[] bArr) {
        this.bs = ByteBuffer.wrap(bArr);
    }

    public int setServerEncoding(String str) {
        this.sServerEncoding = str;
        return 0;
    }

    public JceField read() {
        try {
            JceInputStream.HeadData headData = new JceInputStream.HeadData();
            JceInputStream.readHead(headData, this.bs);
            switch (headData.type) {
                case 0:
                    return JceField.create(this.bs.get(), headData.tag);
                case 1:
                    return JceField.create(this.bs.getShort(), headData.tag);
                case 2:
                    return JceField.create(this.bs.getInt(), headData.tag);
                case 3:
                    return JceField.create(this.bs.getLong(), headData.tag);
                case 4:
                    return JceField.create(this.bs.getFloat(), headData.tag);
                case 5:
                    return JceField.create(this.bs.getDouble(), headData.tag);
                case 6:
                    int i = this.bs.get();
                    if (i < 0) {
                        i += AccessibilityEventCompat.TYPE_VIEW_HOVER_EXIT;
                    }
                    return readString(headData, i);
                case 7:
                    return readString(headData, this.bs.getInt());
                case 8:
                    int iIntValue = ((NumberField) read()).intValue();
                    JceField[] jceFieldArr = new JceField[iIntValue];
                    JceField[] jceFieldArr2 = new JceField[iIntValue];
                    for (int i2 = 0; i2 < iIntValue; i2++) {
                        jceFieldArr[i2] = read();
                        jceFieldArr2[i2] = read();
                    }
                    return JceField.createMap(jceFieldArr, jceFieldArr2, headData.tag);
                case Interpreter.ERR_ARR_OUT /* 9 */:
                    int iIntValue2 = ((NumberField) read()).intValue();
                    JceField[] jceFieldArr3 = new JceField[iIntValue2];
                    for (int i3 = 0; i3 < iIntValue2; i3++) {
                        jceFieldArr3[i3] = read();
                    }
                    return JceField.createList(jceFieldArr3, headData.tag);
                case 10:
                    ArrayList arrayList = new ArrayList();
                    while (true) {
                        JceField jceField = read();
                        if (jceField != null) {
                            arrayList.add(jceField);
                        } else {
                            return JceField.createStruct((JceField[]) arrayList.toArray(new JceField[0]), headData.tag);
                        }
                    }
                    break;
                case 11:
                    return null;
                case 12:
                    return JceField.createZero(headData.tag);
                case 13:
                    int i4 = headData.tag;
                    JceInputStream.readHead(headData, this.bs);
                    if (headData.type != 0) {
                        throw new JceDecodeException("type mismatch, simple_list only support byte, tag: " + i4 + ", type: " + ((int) headData.type));
                    }
                    byte[] bArr = new byte[((NumberField) read()).intValue()];
                    this.bs.get(bArr);
                    return JceField.create(bArr, i4);
                default:
                    return null;
            }
        } catch (BufferUnderflowException e) {
            return null;
        }
    }

    private JceField readString(JceInputStream.HeadData headData, int i) {
        String str;
        byte[] bArr = new byte[i];
        this.bs.get(bArr);
        try {
            str = new String(bArr, this.sServerEncoding);
        } catch (UnsupportedEncodingException e) {
            str = new String(bArr);
        }
        return JceField.create(str, headData.tag);
    }
}
