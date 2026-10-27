package com.dobetter.common;

import android.graphics.Paint;

/* JADX INFO: loaded from: classes.dex */
public class Font {
    private static Font font;
    public static int fontChinaH;
    public static int fontChinaW;
    public static int fontEnglishH;
    public static int fontEnglishW;
    Paint paint;

    public static Font getFont(Paint paint) {
        if (font == null) {
            font = new Font(paint);
        }
        return font;
    }

    private Font(Paint paint) {
        this.paint = paint;
        fontChinaW = charWidth((char) 20320);
        fontChinaH = getFontHeight();
        fontEnglishW = charWidth('a');
        fontEnglishH = fontChinaH;
        System.out.println("字体宽度====" + fontChinaW);
        System.out.println("字体高度====" + fontChinaH);
    }

    private int charWidth(char c) {
        return (int) this.paint.measureText(new StringBuilder(String.valueOf(c)).toString());
    }

    private int getFontHeight() {
        Paint.FontMetrics fm = this.paint.getFontMetrics();
        return ((int) Math.ceil(fm.descent - fm.ascent)) + 2;
    }

    public static int charChinaWidth() {
        return fontChinaW;
    }

    public static int charEnglishWidth() {
        return fontEnglishW;
    }

    public static int charChinaHeight() {
        return fontChinaH;
    }

    public static int charEnglishHeight() {
        return fontEnglishH;
    }

    public int stringWidth(String str) {
        return (int) this.paint.measureText(str);
    }

    public int substringWidth(String str, int offset, int len) {
        String temp = str.substring(offset, len + offset);
        return stringWidth(temp);
    }
}
