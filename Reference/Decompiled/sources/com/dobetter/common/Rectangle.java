package com.dobetter.common;

/* JADX INFO: loaded from: classes.dex */
public class Rectangle {
    public int height;
    public int width;
    public int x;
    public int y;

    public Rectangle() {
    }

    public Rectangle(int xx, int yy, int ww, int hh) {
        this.x = xx;
        this.y = yy;
        this.width = ww;
        this.height = hh;
    }

    public Rectangle(Rectangle rect) {
        this.x = rect.x;
        this.y = rect.y;
        this.width = rect.width;
        this.height = rect.height;
    }
}
