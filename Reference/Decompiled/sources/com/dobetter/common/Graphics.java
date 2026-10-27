package com.dobetter.common;

import android.graphics.Bitmap;
import android.graphics.Canvas;
import android.graphics.Matrix;
import android.graphics.Paint;

/* JADX INFO: loaded from: classes.dex */
public class Graphics implements Transfer {
    public static final int BASELINE = 64;
    public static final int BOTTOM = 32;
    public static final int HB = 33;
    public static final int HCENTER = 1;
    public static final int HT = 17;
    public static final int HV = 3;
    public static final int LB = 36;
    public static final int LEFT = 4;
    public static final int LT = 20;
    public static final int RB = 40;
    public static final int RIGHT = 8;
    public static final int RT = 24;
    public static final int TOP = 16;
    public static final int VCENTER = 2;
    private static Graphics graphics;
    public Canvas canvas;
    private int clipHeight;
    private int clipWidth;
    private int clipX;
    private int clipY;
    Font font;
    Paint paint;

    public Graphics(Canvas canvas, Paint paint, Font font) {
        this.canvas = canvas;
        this.paint = paint;
        this.font = font;
    }

    public void setClip(int x, int y, int width, int height) {
        this.clipX = x;
        this.clipY = y;
        this.clipWidth = width;
        this.clipHeight = height;
        this.canvas.save();
        this.canvas.clipRect(x, y, x + width, y + height);
    }

    public void restore() {
        this.canvas.restore();
    }

    public int getColor() {
        return this.paint.getColor();
    }

    public void setColor(int red, int green, int blue) {
        this.paint.setColor((-16777216) + (red << 16) + (green << 8) + blue);
    }

    public void setColor(int color) {
        if (color == 0) {
            this.paint.setColor(-16777216);
        } else if (color == -1) {
            this.paint.setColor(-1);
        } else {
            this.paint.setColor((-16777216) + color);
        }
    }

    public Font getFont() {
        return this.font;
    }

    public void setFont(Font font) {
        this.font = font;
    }

    public int getClipX() {
        return this.clipX;
    }

    public int getClipY() {
        return this.clipY;
    }

    public int getClipWidth() {
        return this.clipWidth;
    }

    public int getClipHeight() {
        return this.clipHeight;
    }

    public void drawLine(int x1, int y1, int x2, int y2) {
        this.canvas.drawLine(x1, y1, x2, y2, this.paint);
    }

    public void fillRect(int x, int y, int width, int height) {
        this.canvas.drawRect(x, y, x + width, y + height, this.paint);
    }

    public void drawRect(int x, int y, int width, int height) {
        this.canvas.drawLine(x, y, x + width, y, this.paint);
        this.canvas.drawLine(x + width, y, x + width, y + height, this.paint);
        this.canvas.drawLine(x, y + height, x + width, y + height, this.paint);
        this.canvas.drawLine(x, y, x, y + height, this.paint);
    }

    public void drawString(String str, int x, int y, int anchor) {
        int strW = this.font.stringWidth(str);
        int strH = Font.fontChinaH - 3;
        int strHalfW = strW >> 1;
        switch (anchor) {
            case 0:
            case 20:
                this.canvas.drawText(str, x, y + strH, this.paint);
                break;
            case HT /* 17 */:
                this.canvas.drawText(str, x - strHalfW, y + strH, this.paint);
                break;
            case RT /* 24 */:
                this.canvas.drawText(str, x - strW, y + strH, this.paint);
                break;
            case HB /* 33 */:
                this.canvas.drawText(str, x - strHalfW, y, this.paint);
                break;
            case LB /* 36 */:
                this.canvas.drawText(str, x, y, this.paint);
                break;
            case RB /* 40 */:
                this.canvas.drawText(str, x - strW, y, this.paint);
                break;
        }
    }

    public void drawString(String str, int x, int y, int anchor, int color) {
        setColor(color);
        drawString(str, x, y, anchor);
    }

    public void drawSubstring(String scrStr, int offset, int len, int x, int y, int anchor) {
        String str = scrStr.substring(offset, offset + len);
        drawString(str, x, y, anchor);
    }

    public void drawChar(char character, int x, int y, int anchor) {
        String str = new StringBuilder(String.valueOf(character)).toString();
        drawString(str, x, y, anchor);
    }

    public void drawImage(Bitmap bitmap, int x, int y, int anchor) {
        int imgW = bitmap.getWidth();
        int imgH = bitmap.getHeight();
        int imgHalfW = imgW >> 1;
        int imgHalfH = imgH >> 1;
        switch (anchor) {
            case 0:
            case 20:
                this.canvas.drawBitmap(bitmap, x, y, (Paint) null);
                break;
            case 3:
                this.canvas.drawBitmap(bitmap, x - imgHalfW, y - imgHalfH, (Paint) null);
                break;
            case HT /* 17 */:
                this.canvas.drawBitmap(bitmap, x - imgHalfW, y, (Paint) null);
                break;
            case RT /* 24 */:
                this.canvas.drawBitmap(bitmap, x - imgW, y, (Paint) null);
                break;
            case HB /* 33 */:
                this.canvas.drawBitmap(bitmap, x - imgHalfW, y - imgH, (Paint) null);
                break;
            case LB /* 36 */:
                this.canvas.drawBitmap(bitmap, x, y - imgH, (Paint) null);
                break;
            case RB /* 40 */:
                this.canvas.drawBitmap(bitmap, x - imgW, y - imgH, (Paint) null);
                break;
        }
    }

    public void drawRegion(Bitmap srcImg, int x_src, int y_src, int width, int height, int transform, int x_dest, int y_dest, int anchor) {
        if (width > 0 && height > 0) {
            if (transform == 0) {
                drawImgRegion(srcImg, x_dest, y_dest, x_src, y_src, width, height);
                return;
            }
            float[] mirrorY = {-1.0f, 0.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f, 0.0f, 1.0f};
            Matrix mMatrix = new Matrix();
            Matrix temp = new Matrix();
            temp.setValues(mirrorY);
            switch (transform) {
                case 1:
                    mMatrix.setRotate(180.0f);
                    mMatrix.postConcat(temp);
                    break;
                case 2:
                    mMatrix.postConcat(temp);
                    break;
                case 3:
                    mMatrix.setRotate(180.0f);
                    break;
                case 4:
                    mMatrix.setRotate(270.0f);
                    mMatrix.postConcat(temp);
                    break;
                case 5:
                    mMatrix.setRotate(90.0f);
                    break;
                case 6:
                    mMatrix.setRotate(270.0f);
                    break;
                case 7:
                    mMatrix.setRotate(90.0f);
                    mMatrix.postConcat(temp);
                    break;
            }
            Bitmap bitmap = Bitmap.createBitmap(srcImg, x_src, y_src, width, height, mMatrix, true);
            mMatrix.setTranslate(x_dest, y_dest);
            this.canvas.drawBitmap(bitmap, mMatrix, null);
        }
    }

    public void drawImgRegion(Bitmap bitmap, int destX, int destY, int srcX, int srcY, int width, int height) {
        this.canvas.save();
        this.canvas.clipRect(destX, destY, destX + width, destY + height);
        this.canvas.drawBitmap(bitmap, destX - srcX, destY - srcY, (Paint) null);
        this.canvas.restore();
    }

    public void drawRGB(int[] rgbData, int offset, int scanlength, int x, int y, int width, int height, boolean processAlpha) {
        Bitmap b = Bitmap.createBitmap(width, height, processAlpha ? Bitmap.Config.ARGB_8888 : Bitmap.Config.RGB_565);
        b.setPixels(rgbData, offset, scanlength, 0, 0, width, height);
        this.canvas.drawBitmap(b, x, y, (Paint) null);
    }

    public void drawImage(Bitmap bitmap, Matrix matrix) {
        this.canvas.drawBitmap(bitmap, matrix, null);
    }
}
