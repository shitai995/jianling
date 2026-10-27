package com.dobetter.common;

import android.graphics.Bitmap;
import com.dobetter.action.ActionSet;
import java.util.Hashtable;

/* JADX INFO: loaded from: classes.dex */
public class Common {
    public static final byte BUTTON_BACK_PRESSED = 10;
    public static final byte BUTTON_MENU_PRESSED = 9;
    public static final byte BUTTON_OK_PRESSED = 9;
    public static final int DOWN = 1;
    public static final int FIRE = 4;
    public static final byte KEY_NUM0_PRESSED = 11;
    public static final byte KEY_NUM1_PRESSED = 12;
    public static final byte KEY_NUM2_PRESSED = 13;
    public static final byte KEY_NUM3_PRESSED = 14;
    public static final byte KEY_NUM4_PRESSED = 15;
    public static final byte KEY_NUM5_PRESSED = 16;
    public static final byte KEY_NUM6_PRESSED = 17;
    public static final byte KEY_NUM7_PRESSED = 18;
    public static final byte KEY_NUM8_PRESSED = 19;
    public static final byte KEY_NUM9_PRESSED = 20;
    public static final byte KEY_POUND_PRESSED = 21;
    public static final byte KEY_STAR_PRESSED = 22;
    public static final int LEFT = 2;
    public static final int RIGHT = 3;
    private static final byte SOFT_FIRST_PRESSED = 9;
    private static final byte SOFT_LAST_PRESSED = 10;
    public static final int UP = 0;
    public static int uiHeight;
    public static int uiWidth;
    public static int uiX;
    public static int uiY;
    public static int keyPressed = -1;
    public static int keyReleased = -1;
    public static Hashtable clearResCache = new Hashtable();
    public static Hashtable unClearResCache = new Hashtable();

    public static void initUIBounds(int x, int y, int width, int height) {
        uiX = x;
        uiY = y;
        uiWidth = width;
        uiHeight = height;
    }

    public static void setKeyPressed(int key) {
        keyPressed = key;
    }

    public static void setKeyReleased(int key) {
        keyReleased = key;
    }

    public static void keyPressed(int keyCode) {
        keyPressed = keyCode;
    }

    public static void keyReleased(int keyCode) {
        keyReleased = keyCode;
    }

    public static final boolean isAnyKeyPressed() {
        return keyPressed >= 0;
    }

    public static final boolean isAnyKeyReleased() {
        return keyReleased >= 0;
    }

    public static final boolean isKeyPressed(int key, boolean clear) {
        boolean ret = keyPressed == key;
        if (clear && ret) {
            keyPressed = -1;
        }
        return ret;
    }

    public static final boolean isKeyReleased(int key, boolean clear) {
        boolean ret = keyReleased == key;
        if (clear && ret) {
            keyReleased = -1;
        }
        return ret;
    }

    public static void clearKeyStates() {
        keyPressed = -1;
        keyReleased = -1;
    }

    public static void clearKeyPressedStates() {
        keyPressed = -1;
    }

    public static void clearKeyReleasedStates() {
        keyReleased = -1;
    }

    public static boolean isOKPressed(boolean isClear) {
        return isKeyPressed(4, isClear);
    }

    public static boolean isUpPressed(boolean isClear) {
        return isKeyPressed(0, isClear);
    }

    public static boolean isDownPressed(boolean isClear) {
        return isKeyPressed(1, isClear);
    }

    public static boolean isLeftPressed(boolean isClear) {
        return isKeyPressed(2, isClear);
    }

    public static boolean isRightPressed(boolean isClear) {
        return isKeyPressed(3, isClear);
    }

    public static boolean isOKReleased(boolean isClear) {
        return isKeyReleased(4, isClear);
    }

    public static boolean isUpReleased() {
        return isKeyReleased(0, true);
    }

    public static boolean isDownReleased() {
        return isKeyReleased(1, true);
    }

    public static boolean isLeftReleased() {
        return isKeyReleased(2, true);
    }

    public static boolean isRightReleased() {
        return isKeyReleased(3, true);
    }

    public static void setUIClip(Graphics g) {
        g.setClip(uiX, uiY, uiWidth, uiHeight);
    }

    public static void fillUIRect(Graphics g, int color) {
        g.setColor(color);
        g.fillRect(uiX, uiY, uiWidth, uiHeight);
    }

    public static Bitmap getImageFromCache(String file, boolean isUIRes, boolean isPut) {
        String filePath = file.toLowerCase();
        if (unClearResCache.containsKey(filePath)) {
            return (Bitmap) unClearResCache.get(filePath);
        }
        if (clearResCache.containsKey(filePath)) {
            return (Bitmap) clearResCache.get(filePath);
        }
        try {
            Bitmap img = Tool.loadPImage(filePath);
            if (isPut) {
                if (isUIRes) {
                    unClearResCache.put(filePath, img);
                } else {
                    clearResCache.put(filePath, img);
                }
            }
            return img;
        } catch (Exception e) {
            e.printStackTrace();
            return null;
        }
    }

    public static Bitmap getImageFromCache(String file, String pt, boolean isUIRes) {
        String str = String.valueOf(file.toLowerCase()) + pt.toLowerCase();
        if (unClearResCache.containsKey(str)) {
            return (Bitmap) unClearResCache.get(str);
        }
        if (clearResCache.containsKey(str)) {
            return (Bitmap) clearResCache.get(str);
        }
        try {
            Bitmap img = Tool.loadPPImage(file.toLowerCase(), pt.toLowerCase());
            if (isUIRes) {
                unClearResCache.put(str, img);
            } else {
                clearResCache.put(str, img);
            }
            return img;
        } catch (Exception e) {
            e.printStackTrace();
            return null;
        }
    }

    public static ActionSet getActionSet(String actionFile, String[] imageFiles, boolean isUIRes, boolean isPut) {
        Bitmap[] imgs = new Bitmap[imageFiles.length];
        for (int i = 0; i < imageFiles.length; i++) {
            imgs[i] = getImageFromCache("/action/" + imageFiles[i] + ".p", isUIRes, isPut);
        }
        String actionFile2 = "/action/" + actionFile + ".n";
        if (unClearResCache.containsKey(String.valueOf(actionFile2) + "")) {
            return (ActionSet) unClearResCache.get(String.valueOf(actionFile2) + "");
        }
        if (clearResCache.containsKey(String.valueOf(actionFile2) + "")) {
            return (ActionSet) clearResCache.get(String.valueOf(actionFile2) + "");
        }
        ActionSet as = ActionSet.createActionSet(actionFile2, imgs);
        if (isPut) {
            if (isUIRes) {
                unClearResCache.put(actionFile2, as);
                return as;
            }
            clearResCache.put(actionFile2, as);
            return as;
        }
        return as;
    }
}
