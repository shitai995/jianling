package com.dobetter.common;

import android.content.res.AssetManager;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.support.v4.view.MotionEventCompat;
import com.dobetter.client.Bullet;
import com.dobetter.client.JL01Activity;
import com.tencent.webnet.WebNetEvent;
import java.io.ByteArrayOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.lang.reflect.Array;
import java.util.Hashtable;
import java.util.Random;
import java.util.Vector;

/* JADX INFO: loaded from: classes.dex */
public class Tool implements Transfer {
    public static final byte FONT_TYPE_EDGE = 1;
    public static final byte FONT_TYPE_NULL = 0;
    public static final byte FONT_TYPE_SHADOW = 2;
    public static final byte H_CENTER = 0;
    public static final byte H_LEFT = 1;
    public static final byte H_RIGHT = 2;
    public static final int P_CHUNK_SIZE = 100;
    public static final int UI_FLIP_HORIZONTAL = 8192;
    public static final int UI_FLIP_VERTICAL = 16384;
    public static final int UI_RATATE_180 = 180;
    public static final int UI_RATATE_270 = 270;
    public static final int UI_RATATE_90 = 90;
    private static int[] alphaColors;
    public static int imageCreatedColor;
    public static int CENTER = 3;
    public static int TOP_LEFT = 20;
    public static final byte[] PNG_HEAD = {-119, 80, 78, 71, 13, 10, 26, 10, 0, 0, 0, 13, 73, 72, 68, 82};
    public static final byte[] PNG_END = {0, 0, 0, 0, 73, 69, 78, 68, -82, 66, 96, -126};
    public static int countTimes = 0;
    public static int countTimes2 = 0;
    public static int countTimes2Bak = 0;
    public static Random rnd = new Random(System.currentTimeMillis());
    private static final int[] TAN_DATA_X10000 = {0, 174, 349, 524, 699, 874, WebNetEvent.SavePrivateData_Event_Error, 1227, 1405, 1583, 1763, 1943, 2125, 2308, 2493, 2679, 2867, 3057, 3249, 3443, 3639, 3838, 4040, 4244, 4452, 4663, 4877, 5095, 5317, 5543, 5773, 6008, 6248, 6494, 6745, 7002, 7265, 7535, 7812, 8097, 8390, 8692, 9004, 9325, 9656, 10000, 10355, 10723, 11106, 11503, 11917, 12348, 12799, 13270, 13763, 14281, 14825, 15398, 16003, 16642, 17320, 18040, 18807, 19626, 20503, 21445, 22460, 23558, 24750, 26050, 27474, 29042, 30776, 32708, 34874, 37320, 40107, 43314, 47046, 51445, 56712, 63137, 71153, 81443, 95143, 114300, 143006, 190811, 286362, 572899};
    private static final short[] SIN_DATA_X10000 = {0, 174, 349, 524, 698, 872, 1045, 1219, 1392, 1564, 1736, 1908, 2079, 2249, 2419, 2588, 2756, 2924, 3090, 3255, 3420, 3584, 3746, 3907, 4067, 4226, 4384, 4540, 4695, 4848, 5000, 5150, 5299, 5446, 5592, 5736, 5878, 6018, 6156, 6293, 6428, 6561, 6691, 6820, 6947, 7071, 7193, 7314, 7431, 7547, 7660, 7771, 7880, 7986, 8090, 8192, 8290, 8387, 8480, 8572, 8660, 8746, 8829, 8910, 8988, 9063, 9135, 9205, 9272, 9336, 9397, 9455, 9511, 9563, 9612, 9659, 9703, 9744, 9781, 9816, 9848, 9877, 9902, 9925, 9945, 9962, 9976, 9986, 9994, 9998, 10000};

    static {
        init();
    }

    public static void init() {
        Bitmap img = Bitmap.createBitmap(1, 1, Bitmap.Config.ARGB_8888);
        int[] rgbData = getImageRGB(img, 0, 0, 1, 1);
        imageCreatedColor = rgbData[0];
    }

    public static void cycle() {
        countTimes++;
        countTimes2Bak = countTimes2;
        if (countTimes % 2 == 0) {
            countTimes2++;
        }
    }

    public static int getMirrorTransType(int dt) {
        switch (dt) {
            case 0:
                return 2;
            case 1:
                return 3;
            case 2:
                return 0;
            case 3:
                return 1;
            default:
                return 0;
        }
    }

    public static Bitmap getBitmap(int resId, int size) {
        BitmapFactory.Options opt = new BitmapFactory.Options();
        opt.inPreferredConfig = Bitmap.Config.RGB_565;
        opt.inSampleSize = size;
        InputStream is = JL01Activity.activity.getResources().openRawResource(resId);
        return BitmapFactory.decodeStream(is, null, opt);
    }

    public static boolean isCountTimes2Add() {
        return countTimes2Bak != countTimes2;
    }

    public static void drawRegion(Graphics g, Bitmap image, int x_src, int y_src, int width, int height, int transType, int x_dest, int y_dest) {
        g.drawRegion(image, x_src, y_src, width, height, transType, x_dest, y_dest, TOP_LEFT);
    }

    public static void fillRect(Graphics g, int x, int y, int width, int height, int transType) {
        switch (transType) {
            case 5:
            case 6:
                g.fillRect(x, y, height, width);
                break;
            default:
                g.fillRect(x, y, width, height);
                break;
        }
    }

    public static int[] getImageRGB(Bitmap image, int x_src, int y_src, int width, int height) {
        int length = width * height;
        int[] rgbData = new int[length];
        image.getPixels(rgbData, 0, width, x_src, y_src, width, height);
        return rgbData;
    }

    public static void drawRegionAlphaColor(Graphics g, Bitmap image, int x_src, int y_src, int width, int height, int transType, int x_dest, int y_dest, int alphaColor) {
        int length = width * height;
        int[] rgbData = new int[length];
        image.getPixels(rgbData, 0, width, x_src, y_src, width, height);
        int alpha = alphaColor >>> 24;
        int alphaR = (alphaColor >>> 16) & MotionEventCompat.ACTION_MASK;
        int alphaG = (alphaColor >>> 8) & MotionEventCompat.ACTION_MASK;
        int alphaB = alphaColor & MotionEventCompat.ACTION_MASK;
        Hashtable colorHT = new Hashtable();
        int count = 0;
        for (int i = 0; i < length; i++) {
            if (rgbData[i] == imageCreatedColor) {
                rgbData[i] = 0;
            } else if ((rgbData[i] >>> 24) != 0) {
                String rgbKey = Integer.toString(rgbData[i]);
                if (colorHT.containsKey(rgbKey)) {
                    rgbData[i] = Integer.parseInt((String) colorHT.get(rgbKey));
                } else {
                    int red = (rgbData[i] >>> 16) & MotionEventCompat.ACTION_MASK;
                    int green = (rgbData[i] >>> 8) & MotionEventCompat.ACTION_MASK;
                    int blue = rgbData[i] & MotionEventCompat.ACTION_MASK;
                    int red2 = (((255 - alpha) * red) / MotionEventCompat.ACTION_MASK) + ((alphaR * alpha) / MotionEventCompat.ACTION_MASK);
                    int green2 = (((255 - alpha) * green) / MotionEventCompat.ACTION_MASK) + ((alphaG * alpha) / MotionEventCompat.ACTION_MASK);
                    rgbData[i] = (red2 << 16) | (green2 << 8) | ((((255 - alpha) * blue) / MotionEventCompat.ACTION_MASK) + ((alphaB * alpha) / MotionEventCompat.ACTION_MASK)) | (-16777216);
                    colorHT.put(rgbKey, Integer.toString(rgbData[i]));
                    count++;
                }
            }
        }
        drawRGB(g, rgbData, x_dest, y_dest, width, height, transType, true);
    }

    public static void drawImageAlphaColor(Graphics g, Bitmap i, int x, int y, int transType, int alphaColor) {
        drawRegionAlphaColor(g, i, 0, 0, i.getWidth(), i.getHeight(), transType, x, y, alphaColor);
    }

    public static void drawRegionAlpha(Graphics g, Bitmap image, int x_src, int y_src, int width, int height, int transType, int x_dest, int y_dest, int alphaValue) {
        if (alphaValue > 0) {
            if (alphaValue >= 255) {
                drawRegion(g, image, x_src, y_src, width, height, transType, x_dest, y_dest);
                return;
            }
            int alpha = alphaValue << 24;
            int length = width * height;
            int[] rgbData = new int[length];
            image.getPixels(rgbData, 0, width, x_src, y_src, width, height);
            for (int i = 0; i < length; i++) {
                if (rgbData[i] == imageCreatedColor) {
                    rgbData[i] = 0;
                } else if ((rgbData[i] >>> 24) != 0) {
                    rgbData[i] = (rgbData[i] & 16777215) | alpha;
                }
            }
            drawRGB(g, rgbData, x_dest, y_dest, width, height, transType, true);
        }
    }

    public static void drawImageAlpha(Graphics g, Bitmap i, int x, int y, int transType, int alphaValue) {
        drawRegionAlpha(g, i, 0, 0, i.getWidth(), i.getHeight(), transType, x, y, alphaValue);
    }

    public static void drawRGB(Graphics g, int[] rgbData, int x_dest, int y_dest, int width, int height, int transType, boolean isProcessAlpha) {
        int length = width * height;
        switch (transType) {
            case 1:
                int[] rgb = new int[length];
                for (int i = 0; i < height; i++) {
                    System.arraycopy(rgbData, i * width, rgb, ((height - i) - 1) * width, width);
                }
                g.drawRGB(rgb, 0, width, x_dest, y_dest, width, height, isProcessAlpha);
                break;
            case 2:
                int[] rgb2 = new int[length];
                for (int i2 = 0; i2 < height; i2++) {
                    for (int j = 0; j < width; j++) {
                        rgb2[(i2 * width) + j] = rgbData[(((i2 * width) + width) - j) - 1];
                    }
                }
                g.drawRGB(rgb2, 0, width, x_dest, y_dest, width, height, isProcessAlpha);
                break;
            case 3:
                int[] rgb3 = new int[length];
                for (int i3 = 0; i3 < height; i3++) {
                    for (int j2 = 0; j2 < width; j2++) {
                        rgb3[(i3 * width) + j2] = rgbData[(((((height - i3) - 1) * width) + width) - j2) - 1];
                    }
                }
                g.drawRGB(rgb3, 0, width, x_dest, y_dest, width, height, isProcessAlpha);
                break;
            case 4:
            default:
                g.drawRGB(rgbData, 0, width, x_dest, y_dest, width, height, isProcessAlpha);
                break;
            case 5:
                int[] rgb4 = new int[length];
                for (int i4 = 0; i4 < width; i4++) {
                    for (int j3 = 0; j3 < height; j3++) {
                        rgb4[(i4 * height) + j3] = rgbData[(((height - j3) - 1) * width) + i4];
                    }
                }
                g.drawRGB(rgb4, 0, height, x_dest, y_dest, height, width, isProcessAlpha);
                break;
            case 6:
                int[] rgb5 = new int[length];
                for (int i5 = 0; i5 < width; i5++) {
                    for (int j4 = 0; j4 < height; j4++) {
                        rgb5[(i5 * height) + j4] = rgbData[(((j4 * width) + width) - i5) - 1];
                    }
                }
                g.drawRGB(rgb5, 0, height, x_dest, y_dest, height, width, isProcessAlpha);
                break;
        }
    }

    public static void drawTile(Graphics g, Bitmap image, int id, int fwidth, int fheight, int type, int dx, int dy) {
        int cols = image.getWidth() / fwidth;
        drawRegion(g, image, (id % cols) * fwidth, (id / cols) * fheight, fwidth, fheight, type, dx, dy);
    }

    public static void drawImage(Graphics g, Bitmap i, int x, int y, int transType) {
        if (transType == 0) {
            g.drawImage(i, x, y, TOP_LEFT);
        } else {
            drawRegion(g, i, 0, 0, i.getWidth(), i.getHeight(), transType, x, y);
        }
    }

    public static void drawClipImage(Graphics g, Bitmap image, int destX, int destY, int srcX, int srcY, int width, int height) {
        g.setClip(destX, destY, width, height);
        g.drawImage(image, destX - srcX, destY - srcY, 20);
        g.restore();
    }

    public static int drawNumReturnWidth(Graphics g, Bitmap I_Num, long num, int x, int y, int maxbit, int alignment, int interval) {
        int startX = 0;
        if (num < 0) {
            num = 0;
        }
        int width = I_Num.getWidth() / 10;
        int height = I_Num.getHeight();
        int bit = 1;
        long[] tempNum = {num, 1};
        while (tempNum[0] > 9) {
            bit++;
            tempNum[0] = tempNum[0] / 10;
            tempNum[1] = tempNum[1] * 10;
        }
        for (int i = 0; i < bit; i++) {
            if (i == 0) {
                switch (alignment) {
                    case 0:
                        startX = x;
                        break;
                    case 1:
                        startX = x - ((width + interval) * bit);
                        break;
                    case 2:
                        startX = x - (((width + interval) * bit) >> 1);
                        break;
                }
                drawClipImage(g, I_Num, startX, y, (int) (tempNum[0] * ((long) width)), 0, width, height);
            } else {
                tempNum[1] = tempNum[1] / 10;
                switch (alignment) {
                    case 0:
                        startX = x + ((width + interval) * i);
                        break;
                    case 1:
                        startX = x + ((width + interval) * (i - bit));
                        break;
                    case 2:
                        startX = (x - (((width + interval) * bit) >> 1)) + ((width + interval) * i);
                        break;
                }
                drawClipImage(g, I_Num, startX, y, (int) (((num / tempNum[1]) % 10) * ((long) width)), 0, width, height);
            }
        }
        int numWidth = bit * (width + interval);
        return numWidth;
    }

    public static int drawSignReturnWidth(Graphics g, Bitmap signImg, int x, int y, int num, int index, int alignment) {
        int width = signImg.getWidth() / num;
        int height = signImg.getHeight();
        int startX = 0;
        switch (alignment) {
            case 0:
                startX = x;
                break;
            case 1:
                startX = x - width;
                break;
            case 2:
                startX = x - (width >> 1);
                break;
        }
        drawClipImage(g, signImg, startX, y, index * width, 0, width, height);
        return width;
    }

    void drawFraction(Graphics g, Bitmap numImg, Bitmap signImg, int num1, int num2, int x, int y, int maxBit, int alignment, int inteval) {
        if (alignment == 0) {
            int num1Width = drawNumReturnWidth(g, numImg, num1, x, y, maxBit, alignment, inteval);
            drawNumReturnWidth(g, numImg, num2, ((x + num1Width) + drawSignReturnWidth(g, signImg, (x + num1Width) - 4, y, 4, 1, alignment)) - 5, y, maxBit, alignment, inteval);
        } else if (alignment == 1) {
            int num1Width2 = drawNumReturnWidth(g, numImg, num2, x, y, maxBit, alignment, inteval);
            drawNumReturnWidth(g, numImg, num1, ((x - num1Width2) - drawSignReturnWidth(g, signImg, (x - num1Width2) + 4, y, 4, 1, alignment)) + 5, y, maxBit, alignment, inteval);
        } else {
            int signWidth = drawSignReturnWidth(g, signImg, x, y, 4, 1, alignment);
            drawNumReturnWidth(g, numImg, num1, x - (signWidth >> 1), y, maxBit, 1, inteval);
            drawNumReturnWidth(g, numImg, num2, x + (signWidth >> 1), y, maxBit, 0, inteval);
        }
    }

    void drawPercent(Graphics g, Bitmap numImg, Bitmap signImg, long num1, long num2, int x, int y, int bit, int alignment, int inteval) {
        String[] tempArray = floatPercentCounter(num1, num2, bit);
        int num1Width = drawNumReturnWidth(g, numImg, Integer.parseInt(tempArray[0]), x, y, 3, alignment, inteval);
        int signWidth = tempArray.length == 1 ? 0 : drawSignReturnWidth(g, signImg, (x + num1Width) - 1, y, 4, 3, alignment);
        int num2Width = tempArray.length == 1 ? 0 : drawNumReturnWidth(g, numImg, Integer.parseInt(tempArray[1]), ((x + num1Width) + signWidth) - 2, y, bit, alignment, inteval);
        drawSignReturnWidth(g, signImg, (((x + num1Width) + signWidth) + num2Width) - 2, y, 4, 0, alignment);
    }

    public static String[] floatPercentCounter(long numerator, long denominator, int bit) {
        long temp0 = numerator * 100;
        int temp1 = 1;
        int temp2 = bit;
        while (temp2 > 0) {
            temp2--;
            temp1 *= 10;
        }
        long num1 = denominator == 0 ? 0L : temp0 / denominator;
        long num2 = denominator == 0 ? 0L : ((temp0 % denominator) * ((long) temp1)) / denominator;
        if (num2 > 0) {
            String[] arg = new String[2];
            arg[0] = Long.toString(num1);
            arg[1] = num2 < 10 ? "0" + Long.toString(num2) : Long.toString(num2);
            return arg;
        }
        return new String[]{Long.toString(num1)};
    }

    public static boolean rectIntersect(int x1, int y1, int w1, int h1, int x2, int y2, int w2, int h2) {
        return y1 < y2 + h2 && y2 < y1 + h1 && x1 < x2 + w2 && x2 < x1 + w1;
    }

    public static int getNextRnd(int min, int max) {
        if (max <= min) {
            return -1;
        }
        return (Math.abs(rnd.nextInt()) % (max - min)) + min;
    }

    public static int getNextRnd() {
        return rnd.nextInt();
    }

    public static int getDistanceBetweenTwoPoints(int x1, int y1, int x2, int y2) {
        int oppositeSideLength = Math.abs(y1 - y2);
        int nearSideLength = Math.abs(x1 - x2);
        int angle = 0;
        if (oppositeSideLength == 0) {
            return nearSideLength;
        }
        if (nearSideLength == 0) {
            return oppositeSideLength;
        }
        while (angle < TAN_DATA_X10000.length && (oppositeSideLength * 10000) / nearSideLength > TAN_DATA_X10000[angle]) {
            angle++;
        }
        int radius = (oppositeSideLength * 10000) / SIN_DATA_X10000[angle];
        return radius;
    }

    public static int power(int base, int exp) {
        if (exp < 0) {
            return 0;
        }
        int result = 1;
        for (int i = 0; i < exp; i++) {
            result *= base;
        }
        return result;
    }

    public static String[] getStringArray(String s, int drawWidth, Font font) {
        int begin = 0;
        Vector vS = new Vector();
        String s2 = String.valueOf(s) + '\n';
        for (int i = 0; i < s2.length(); i++) {
            boolean blineEnd = false;
            if (s2.charAt(i) == '\n') {
                blineEnd = true;
            } else {
                int lineWidth = font.substringWidth(s2, begin, (i - begin) + 1);
                if (lineWidth >= drawWidth) {
                    blineEnd = true;
                }
            }
            if (blineEnd) {
                vS.addElement(s2.substring(begin, i));
                begin = i;
                if (s2.charAt(i) == '\n') {
                    begin++;
                }
            }
        }
        String[] strs = new String[vS.size()];
        vS.copyInto(strs);
        return strs;
    }

    public static String[] getStringArray(String s, char ch) {
        int startIndex = 0;
        Vector vS = new Vector();
        while (true) {
            int endIndex = s.indexOf(ch, startIndex);
            if (endIndex == -1) {
                vS.addElement(s.substring(startIndex));
                String[] strs = new String[vS.size()];
                vS.copyInto(strs);
                return strs;
            }
            vS.addElement(s.substring(startIndex, endIndex));
            startIndex = endIndex + 1;
        }
    }

    public static String[] getStringArray(String s) {
        return getStringArray(s, '\n');
    }

    public static String[] getStringArrayFrn(String s, int drawWidth, Font fnt) {
        int sl = s.length();
        int beginID = 0;
        int endID = 0;
        int spaceID = 0;
        Vector vS = new Vector();
        for (int i = 0; i < sl; i++) {
            boolean bLineEnd = false;
            boolean bLink = false;
            char ch = s.charAt(i);
            if (i == sl - 1) {
                endID = i + 1;
                bLineEnd = true;
            } else {
                if (ch == ' ') {
                    spaceID = i;
                }
                int lineWidth = fnt.substringWidth(s, beginID, (i - beginID) + 1);
                if (lineWidth >= drawWidth) {
                    if (ch != ' ') {
                        if (spaceID <= beginID) {
                            endID = i;
                            bLink = true;
                        } else {
                            endID = spaceID + 1;
                        }
                    } else {
                        endID = i + 1;
                    }
                    bLineEnd = true;
                }
            }
            if (bLineEnd) {
                String tstr = s.substring(beginID, endID);
                if (bLink) {
                    tstr = String.valueOf(tstr) + "-";
                }
                vS.addElement(tstr);
                beginID = endID;
            }
        }
        String[] strs = new String[vS.size()];
        vS.copyInto(strs);
        return strs;
    }

    public static void drawString(Graphics g, String s, int x, int y, int frontColor, int bgColor, byte type) {
        g.setColor(bgColor);
        if (type == 1) {
            g.drawString(s, x, y - 1, TOP_LEFT);
            g.drawString(s, x, y + 1, TOP_LEFT);
            g.drawString(s, x - 1, y, TOP_LEFT);
            g.drawString(s, x + 1, y, TOP_LEFT);
        } else if (type == 2) {
            g.drawString(s, x - 1, y - 1, TOP_LEFT);
        }
        g.setColor(frontColor);
        g.drawString(s, x, y, TOP_LEFT);
    }

    public static void drawString(Graphics g, String s, int x, int y, int frontColor, int bgColor) {
        drawString(g, s, x, y, frontColor, bgColor, (byte) 1);
    }

    public static void drawString(Graphics g, String s, int x, int y, int frontColor, int bgColor, int jrange, byte type) {
        if (jrange == 0) {
            drawString(g, s, x, y, frontColor, bgColor, type);
            return;
        }
        Font font = g.getFont();
        int[] f = {-jrange, 0, jrange, 0};
        for (int i = 0; i < s.length(); i++) {
            int w = font.substringWidth(s, 0, i);
            drawString(g, s.substring(i, i + 1), x + w, y + f[((countTimes & 3) + i) & 3], frontColor, bgColor, type);
        }
    }

    public static void drawString(Graphics g, String s, int x, int y, int frontColor, int bgColor, int jrange) {
        drawString(g, s, x, y, frontColor, bgColor, jrange, (byte) 1);
    }

    public static void drawImageNum(Graphics g, Bitmap img, String numStr, int dx, int dy, int fontWidth, int fontOffSet) {
        for (int i = 0; i < numStr.length(); i++) {
            String s = numStr.substring(i, i + 1);
            int asc = s.hashCode();
            int x = ((fontWidth + fontOffSet) * i) + dx;
            int offset = asc - 47;
            g.setClip(x, dy, fontWidth, img.getHeight());
            g.drawImage(img, x - (offset * fontWidth), dy, TOP_LEFT);
            g.restore();
        }
    }

    public static void drawImageRect(Graphics g, Bitmap rImage, int rx, int ry, int rw, int rh, int bgColor) {
        int fw = rImage.getWidth() / 3;
        int fh = rImage.getHeight() / 3;
        if (rh < fh) {
            ry += (rh - (fh * 2)) >> 1;
            rh = fh * 2;
        }
        int rhb = rh;
        int w = rw / fw;
        int h = rh / fh;
        if (w * fw < rw) {
            rw = fw * (w + 1);
            rx += (rw - rw) >> 1;
        }
        if (h * fh < rh) {
            rh = fh * (h + 1);
            ry += (rhb - rh) >> 1;
        }
        if (bgColor >= 0) {
            g.setColor(bgColor);
            g.fillRect(rx + fw, ry + fh, rw - (fw * 2), rh - (fh * 2));
        }
        for (int i = 0; i < (rw / fw) - 2; i++) {
            drawTile(g, rImage, 1, fw, fh, 0, rx + ((i + 1) * fw), ry);
            drawTile(g, rImage, 7, fw, fh, 0, rx + ((i + 1) * fw), (ry + rh) - fh);
        }
        for (int i2 = 0; i2 < (rh / fh) - 2; i2++) {
            drawTile(g, rImage, 3, fw, fh, 0, rx, ry + ((i2 + 1) * fh));
            drawTile(g, rImage, 5, fw, fh, 0, (rx + rw) - fw, ry + ((i2 + 1) * fh));
        }
        drawTile(g, rImage, 0, fw, fh, 0, rx, ry);
        drawTile(g, rImage, 2, fw, fh, 0, (rx + rw) - fw, ry);
        drawTile(g, rImage, 6, fw, fh, 0, rx, (ry + rh) - fh);
        drawTile(g, rImage, 8, fw, fh, 0, (rx + rw) - fw, (ry + rh) - fh);
    }

    public static void fillAlphaRect(Graphics g, int argb, int x, int y, int width, int height) {
        int alpha = argb >>> 24;
        if (alpha != 0) {
            if (alpha == 255) {
                g.setColor(16777215 & argb);
                g.fillRect(x, y, width, height);
                return;
            }
            boolean bnew = false;
            if (alphaColors == null || width != alphaColors.length || argb != alphaColors[0]) {
                bnew = true;
            }
            if (bnew) {
                alphaColors = new int[width];
                for (int i = 0; i < width; i++) {
                    alphaColors[i] = argb;
                }
            }
            g.drawRGB(alphaColors, 0, 0, x, y, width, height, true);
        }
    }

    public static boolean isRectIntersected(int ax, int ay, int aw, int ah, int bx, int by, int bw, int bh) {
        return by + bh >= ay && by <= ay + ah && bx + bw >= ax && bx <= ax + aw;
    }

    public static boolean isRectIntersected(int ax, int ay, Rectangle aRect, int bx, int by, Rectangle bRect) {
        return isRectIntersected(aRect.x + ax, aRect.y + ay, aRect.width, aRect.height, bRect.x + bx, bRect.y + by, bRect.width, bRect.height);
    }

    public static boolean isPointInsideRect(int ax, int ay, int bx, int by, int bw, int bh) {
        return ax >= bx && ax <= bx + bw && ay >= by && ay <= by + bh;
    }

    public static byte[] getBytesFromInput(InputStream in) throws IOException {
        ByteArrayOutputStream out = new ByteArrayOutputStream();
        int len = 0;
        byte[] buf = new byte[64];
        while (true) {
            int rd = in.read(buf);
            if (rd != -1) {
                len += rd;
                out.write(buf, 0, rd);
            } else {
                byte[] rt = out.toByteArray();
                out.close();
                return rt;
            }
        }
    }

    public static String toUTF(byte[] data, int off, int len) {
        int charCount;
        char ch;
        char[] infoStr = new char[len * 2];
        int i = 0;
        int charCount2 = 0;
        while (i < len) {
            try {
                if ((data[i + off] & 128) != 0) {
                    if ((data[i + off] & Bullet.DIR_LD) != 0) {
                        char ch2 = (char) ((data[i + off] & 15) << 12);
                        ch = (char) ((data[i + 2 + off] & 63) + ((char) ((((char) (data[(i + 1) + off] & 63)) << 6) + ch2)));
                        i += 3;
                    } else {
                        char ch3 = (char) ((data[i + off] & Bullet.DIR_LU) << 6);
                        ch = (char) ((data[i + 1 + off] & 63) + ch3);
                        i += 2;
                    }
                } else {
                    ch = (char) data[i + off];
                    i++;
                }
                charCount = charCount2 + 1;
                try {
                    infoStr[charCount2] = ch;
                    charCount2 = charCount;
                } catch (Exception e) {
                    charCount2 = charCount;
                }
            } catch (Exception e2) {
                charCount = charCount2;
            }
        }
        return new String(infoStr, 0, charCount2);
    }

    public static void reportException(Exception e) {
        e.printStackTrace();
    }

    public static final InputStream getResourceAsStream(String path) {
        AssetManager manager = JL01Activity.activity.getAssets();
        try {
            if (path.charAt(0) == '/') {
                path = path.substring(1, path.length());
            }
            return manager.open(path);
        } catch (IOException e) {
            e.printStackTrace();
            return null;
        }
    }

    public static Bitmap loadPImage(String file) {
        Bitmap img = null;
        try {
            InputStream in = getResourceAsStream(file);
            byte[] lens = new byte[4];
            in.read(lens);
            int len = ((lens[0] & MotionEventCompat.ACTION_MASK) << 24) + ((lens[1] & MotionEventCompat.ACTION_MASK) << 16) + ((lens[2] & MotionEventCompat.ACTION_MASK) << 8) + (lens[3] & MotionEventCompat.ACTION_MASK);
            byte[] pngData = new byte[PNG_HEAD.length + len + PNG_END.length];
            System.arraycopy(PNG_HEAD, 0, pngData, 0, PNG_HEAD.length);
            System.arraycopy(PNG_END, 0, pngData, pngData.length - PNG_END.length, PNG_END.length);
            int blockNum = len / 100;
            int oddSize = len % 100;
            int off = PNG_HEAD.length + len;
            if (oddSize != 0) {
                off -= oddSize;
                in.read(pngData, off, oddSize);
            }
            for (int i = blockNum - 1; i >= 0; i--) {
                off -= 100;
                in.read(pngData, off, 100);
            }
            img = BitmapFactory.decodeByteArray(pngData, 0, pngData.length);
            in.close();
            return img;
        } catch (Exception e) {
            System.out.println("loadPImage Error :" + file);
            e.printStackTrace();
            return img;
        }
    }

    public static Bitmap loadPPImage(String file, String palette) {
        Bitmap img = null;
        try {
            InputStream in = getResourceAsStream(file);
            byte[] lens = new byte[4];
            in.read(lens);
            int len = ((lens[0] & MotionEventCompat.ACTION_MASK) << 24) + ((lens[1] & MotionEventCompat.ACTION_MASK) << 16) + ((lens[2] & MotionEventCompat.ACTION_MASK) << 8) + (lens[3] & MotionEventCompat.ACTION_MASK);
            byte[] pngData = new byte[PNG_HEAD.length + len + PNG_END.length];
            System.arraycopy(PNG_HEAD, 0, pngData, 0, PNG_HEAD.length);
            System.arraycopy(PNG_END, 0, pngData, pngData.length - PNG_END.length, PNG_END.length);
            int blockNum = len / 100;
            int oddSize = len % 100;
            int off = PNG_HEAD.length + len;
            if (oddSize != 0) {
                off -= oddSize;
                in.read(pngData, off, oddSize);
            }
            for (int i = blockNum - 1; i >= 0; i--) {
                off -= 100;
                in.read(pngData, off, 100);
            }
            in.close();
            img = BitmapFactory.decodeByteArray(pngData, 0, pngData.length);
            return img;
        } catch (Exception e) {
            System.out.println("loadPtImage Error :" + file);
            e.printStackTrace();
            return img;
        }
    }

    public static String decryptString(String srcStr, int version) {
        if (version != 1) {
            return srcStr;
        }
        int length = srcStr.length() / 2;
        char[] chs = new char[length];
        for (int i = 0; i < length; i++) {
            char ch0 = srcStr.charAt(i * 2);
            char ch1 = srcStr.charAt((i * 2) + 1);
            chs[i] = (char) ((((ch0 - 'A') / 8) * 24) + (ch1 - 'a'));
        }
        String desStr = new String(chs);
        return desStr;
    }

    public static void fillRect(Graphics g, int x, int y, int width, int height, int bgClr, int edgeClr, boolean hasVertex) {
        if (edgeClr < 0) {
            g.setColor(bgClr);
            g.fillRect(x, y, width, height);
            return;
        }
        g.setColor(bgClr);
        g.fillRect(x + 1, y + 1, width - 2, height - 2);
        g.setColor(edgeClr);
        if (hasVertex) {
            g.drawRect(x, y, width - 1, height - 1);
            return;
        }
        g.fillRect(x + 1, y, width - 2, 1);
        g.fillRect(x + 1, (y + height) - 1, width - 2, 1);
        g.fillRect(x, y + 1, 1, height - 2);
        g.fillRect((x + width) - 1, y + 1, 1, height - 2);
    }

    public static short[][] stringToShorts(String str) {
        String[] strs = getStringArray(str, ';');
        short[][] shorts = (short[][]) Array.newInstance((Class<?>) Short.TYPE, getStringArray(strs[0], ',').length, strs.length);
        for (int i = 0; i < strs.length; i++) {
            String[] ss = getStringArray(strs[i], ',');
            for (int j = 0; j < ss.length; j++) {
                shorts[j][i] = Short.parseShort(ss[j]);
            }
        }
        return shorts;
    }
}
