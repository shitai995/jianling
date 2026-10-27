package com.dobetter.common;

import android.support.v4.view.MotionEventCompat;

/* JADX INFO: loaded from: classes.dex */
public class XList {
    public static final byte V_BOTTOM = 2;
    public static final byte V_CENTER = 0;
    public static final byte V_TOP = 1;
    private boolean bFocusChanged;
    private int focusIndex;
    private Font font;
    private int itemHeight;
    private int itemShowFirst;
    private int itemShowNum;
    private MovingString[] itemStrs;
    private int itemWidth;
    private Object[] objects;

    public XList(Object[] os, int iw, int ih, int sn, Font fn) {
        this.objects = os;
        this.itemWidth = iw;
        this.itemHeight = ih;
        this.itemShowNum = sn;
        this.font = fn;
        this.itemShowFirst = 0;
        this.focusIndex = 0;
        this.itemStrs = new MovingString[this.objects.length];
        for (int i = 0; i < this.itemStrs.length; i++) {
            if (this.objects[i] != null) {
                this.itemStrs[i] = new MovingString(this.objects[i].toString(), this.itemWidth, 2);
            } else {
                this.itemStrs[i] = new MovingString("Null", this.itemWidth, 2);
            }
        }
        this.itemStrs[this.focusIndex].start();
    }

    public XList(Object[] os, int iw, int ih, int sn, Font fn, boolean bAutoHeight) {
        this(os, iw, ih, sn, fn);
        if (bAutoHeight && this.itemShowNum > os.length) {
            this.itemShowNum = os.length;
        }
    }

    public int getWidth() {
        return this.itemWidth;
    }

    public int getHeight() {
        return this.itemHeight * this.itemShowNum;
    }

    public int getItemHeight() {
        return this.itemHeight;
    }

    public boolean focusChanged() {
        return this.bFocusChanged;
    }

    public Object getFocusItem() {
        return this.objects[this.focusIndex];
    }

    public int getFocusIndex() {
        return this.focusIndex;
    }

    public void setFocusIndex(int index) {
        if (index >= 0 && index < this.objects.length) {
            this.focusIndex = index;
        }
    }

    public int getFocusIndexInPage() {
        return this.focusIndex - this.itemShowFirst;
    }

    public int getFirstIndexShown() {
        return this.itemShowFirst;
    }

    public int getItemNum() {
        return this.objects.length;
    }

    public Object getItemAt(int index) {
        if (index < 0 || index >= this.objects.length) {
            return null;
        }
        return this.objects[index];
    }

    public MovingString getitemStrsAt(int index) {
        if (index < 0 || index >= this.itemStrs.length) {
            return null;
        }
        return this.itemStrs[index];
    }

    public Object getItemShownAt(int index) {
        return getItemAt(this.itemShowFirst + index);
    }

    public int getItemNumShown() {
        return this.itemShowNum > this.objects.length ? this.objects.length : this.itemShowNum;
    }

    public void handleKey() {
        this.bFocusChanged = false;
        if (Common.isAnyKeyPressed()) {
            int lastFocused = this.focusIndex;
            if (Common.isUpPressed(true)) {
                this.focusIndex--;
                if (this.focusIndex < 0) {
                    this.focusIndex = 0;
                }
                if (this.itemShowFirst > this.focusIndex) {
                    this.itemShowFirst = this.focusIndex;
                }
            } else if (Common.isDownPressed(true)) {
                this.focusIndex++;
                if (this.focusIndex >= this.objects.length) {
                    this.focusIndex = this.objects.length - 1;
                }
                if (this.itemShowFirst < (this.focusIndex - this.itemShowNum) + 1) {
                    this.itemShowFirst = (this.focusIndex - this.itemShowNum) + 1;
                }
            }
            if (lastFocused != this.focusIndex) {
                this.itemStrs[lastFocused].stop();
                this.itemStrs[this.focusIndex].start();
                this.bFocusChanged = true;
            }
        }
    }

    public void handleTouch() {
        this.bFocusChanged = false;
        if (Common.isAnyKeyPressed()) {
            int lastFocused = this.focusIndex;
            if (Common.isKeyPressed(0, true)) {
                this.focusIndex--;
                if (this.focusIndex < 0) {
                    this.focusIndex = 0;
                }
                if (this.itemShowFirst > this.focusIndex) {
                    this.itemShowFirst = this.focusIndex;
                }
            } else if (Common.isKeyPressed(1, true)) {
                this.focusIndex++;
                if (this.focusIndex >= this.objects.length) {
                    this.focusIndex = this.objects.length - 1;
                }
                if (this.itemShowFirst < (this.focusIndex - this.itemShowNum) + 1) {
                    this.itemShowFirst = (this.focusIndex - this.itemShowNum) + 1;
                }
            }
            if (lastFocused != this.focusIndex) {
                this.itemStrs[lastFocused].stop();
                this.itemStrs[this.focusIndex].start();
                this.bFocusChanged = true;
            }
        }
    }

    public void cycle() {
        this.itemStrs[this.focusIndex].cycle();
    }

    public void draw(Graphics g, int listX, int listY) {
        int index;
        g.setFont(this.font);
        for (int i = 0; i < this.itemShowNum && (index = i + this.itemShowFirst) < this.objects.length; i++) {
            if (this.focusIndex == index) {
                this.itemStrs[index].draw(g, listX, listY + (this.itemHeight * i), 16777215, MotionEventCompat.ACTION_MASK, (byte) 1);
            } else {
                this.itemStrs[index].draw(g, listX, listY + (this.itemHeight * i), 16777215, -1, (byte) 1);
            }
        }
    }

    public void draw(Graphics g, int listX, int listY, int[] format, int[] focusFormat, byte vType) {
        int index;
        g.setFont(this.font);
        int dh = 0;
        if (vType == 0) {
            dh = (this.itemHeight - Font.fontChinaH) >> 1;
        } else if (vType == 2) {
            dh = this.itemHeight - Font.fontChinaH;
        }
        for (int i = 0; i < this.itemShowNum && (index = i + this.itemShowFirst) < this.objects.length; i++) {
            if (this.focusIndex == index) {
                this.itemStrs[index].draw(g, listX, (this.itemHeight * i) + listY + dh, focusFormat[0], focusFormat[1], (byte) focusFormat[2]);
            } else {
                this.itemStrs[index].draw(g, listX, (this.itemHeight * i) + listY + dh, format[0], format[1], (byte) format[2]);
            }
        }
    }
}
