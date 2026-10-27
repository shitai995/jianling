package com.dobetter.common;

/* JADX INFO: loaded from: classes.dex */
public class Style {
    private static final String ERROR_SYNTAX = "syntax error: ";
    public static final char STYLE_BEGIN_CHAR = '<';
    public static final char STYLE_END_CHAR = '>';
    public static final byte TYPE_COLOR = 1;
    public static final byte TYPE_END = 0;
    public static final byte TYPE_ICON = 2;
    public int backColor;
    public int fontColor;
    public byte fontType;
    public int iconID;
    public byte type;
    public static final char[] STYLE_FLAGS = {'/', 'c', 'i'};
    public static int DefaultFontColor = 16777215;
    public static int DefaultBackColor = 0;
    public static byte DefaultFontType = 1;

    public Style() {
        this.type = (byte) 1;
        this.fontColor = DefaultFontColor;
        this.backColor = DefaultBackColor;
        this.fontType = DefaultFontType;
    }

    public Style(int fc, int bc, byte fnType) {
        this.type = (byte) 1;
        this.fontColor = fc;
        this.backColor = bc;
        this.fontType = fnType;
    }

    public Style(int icID) {
        this.type = (byte) 2;
        this.iconID = icID;
    }

    public static void setDefault(int clr0, int clr1, byte fType) {
        DefaultFontColor = clr0;
        DefaultBackColor = clr1;
        DefaultFontType = fType;
    }

    public static Style createStyle(String str) {
        try {
            Style style = new Style();
            String str2 = str.toLowerCase();
            char fistChar = str2.charAt(0);
            if (fistChar == STYLE_FLAGS[0]) {
                style.type = (byte) 0;
                if (str2.length() != 2) {
                    throw new Exception(ERROR_SYNTAX + str2);
                }
                if (str2.charAt(1) != STYLE_FLAGS[1]) {
                    throw new Exception(ERROR_SYNTAX + str2);
                }
                return style;
            }
            if (fistChar == STYLE_FLAGS[1]) {
                style.type = (byte) 1;
                int index = str2.indexOf(61);
                if (index < 0) {
                    throw new Exception("syntax error: without '='");
                }
                String[] cStrs = Tool.getStringArray(str2.substring(index + 1), ',');
                if (cStrs == null) {
                    throw new Exception(ERROR_SYNTAX + str2);
                }
                if (cStrs.length == 0 || cStrs.length > 3) {
                    throw new Exception(ERROR_SYNTAX + str2);
                }
                if (cStrs[0].trim().length() > 0) {
                    style.fontColor = Integer.parseInt(cStrs[0], 16);
                }
                if (cStrs.length > 1 && cStrs[1].trim().length() > 0) {
                    style.backColor = Integer.parseInt(cStrs[1], 16);
                }
                if (cStrs.length <= 2 || cStrs[2].trim().length() <= 0) {
                    return style;
                }
                style.fontType = Byte.parseByte(cStrs[2]);
                return style;
            }
            if (fistChar == STYLE_FLAGS[2]) {
                style.type = (byte) 2;
                int index2 = str2.indexOf(61);
                if (index2 < 0) {
                    throw new Exception("syntax error: without '='");
                }
                String cStr = str2.substring(index2 + 1);
                if (cStr.trim().length() <= 0) {
                    return style;
                }
                style.iconID = Integer.parseInt(cStr);
                return style;
            }
            return null;
        } catch (Exception e) {
            e.printStackTrace();
            return null;
        }
    }

    public static boolean isFlag(char ch) {
        for (int i = 0; i < STYLE_FLAGS.length; i++) {
            if (ch == STYLE_FLAGS[i]) {
                return true;
            }
        }
        return false;
    }

    public static int getFlagID(char ch) {
        for (int i = 0; i < STYLE_FLAGS.length; i++) {
            if (ch == STYLE_FLAGS[i]) {
                return i;
            }
        }
        return -1;
    }

    public static String getText(Style style, String str) {
        StringBuffer strB = new StringBuffer();
        strB.append(STYLE_BEGIN_CHAR);
        strB.append(STYLE_FLAGS[style.type]);
        strB.append('=');
        if (style.type == 1) {
            strB.append(Integer.toHexString(style.fontColor));
            strB.append(',');
            strB.append(Integer.toHexString(style.backColor));
            strB.append(',');
            strB.append((int) style.fontType);
        } else if (style.type == 2) {
            strB.append(style.iconID);
        }
        strB.append(STYLE_END_CHAR);
        if (style.type == 1) {
            strB.append(str);
            strB.append(STYLE_BEGIN_CHAR);
            strB.append(STYLE_FLAGS[0]);
            strB.append(STYLE_FLAGS[1]);
            strB.append(STYLE_END_CHAR);
        }
        return strB.toString();
    }

    public String toString() {
        StringBuffer strB = new StringBuffer();
        strB.append(STYLE_BEGIN_CHAR);
        strB.append(STYLE_FLAGS[this.type]);
        if (this.type == 0) {
            strB.append(STYLE_FLAGS[1]);
        } else if (this.type == 1) {
            strB.append('=');
            strB.append(Integer.toHexString(this.fontColor));
            strB.append(',');
            strB.append(Integer.toHexString(this.backColor));
            strB.append(',');
            strB.append((int) this.fontType);
        } else if (this.type == 2) {
            strB.append('=');
            strB.append(this.iconID);
        }
        strB.append(STYLE_END_CHAR);
        return strB.toString();
    }
}
