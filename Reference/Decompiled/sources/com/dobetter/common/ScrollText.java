package com.dobetter.common;

/* JADX INFO: loaded from: classes.dex */
public class ScrollText {
    public boolean bScroll;
    int drawDy;
    Font font;
    int height;
    int lineHeight;
    int lineWidth;
    int scrollStep;
    String[] text;
    int width;

    public ScrollText(String str, int w, int h, int lineHeight, int step, int startPos, Font fn) {
        if (str != null) {
            this.width = w;
            this.lineHeight = lineHeight;
            this.scrollStep = step;
            this.font = fn;
            this.text = Tool.getStringArray(str, this.width - 5, this.font);
            this.drawDy = 0;
            for (int i = 0; i < this.text.length; i++) {
                if (this.font.stringWidth(this.text[i]) > this.lineWidth) {
                    this.lineWidth = this.font.stringWidth(this.text[i]);
                }
            }
            this.height = this.text.length * lineHeight;
            if (this.height > h) {
                this.height = h;
                this.bScroll = true;
                this.drawDy = startPos;
            }
        }
    }

    public void cycle() {
        if (this.bScroll) {
            this.drawDy -= this.scrollStep;
            if (this.drawDy <= (-this.lineHeight) * this.text.length) {
                this.drawDy = this.height;
            }
        }
    }

    public void start() {
        this.bScroll = true;
    }

    public void stop() {
        this.drawDy = 0;
        this.bScroll = false;
    }

    public void pause() {
        this.bScroll = false;
    }

    public int getWidth() {
        return this.width;
    }

    public int getHeight() {
        return this.height;
    }

    public void draw(Graphics g, int x, int y, int frontColor, int bgColor, byte type) {
        g.setClip(x, y, this.width, this.height);
        g.setFont(this.font);
        int startIndex = (-this.drawDy) / this.lineHeight;
        if (startIndex < 0) {
            startIndex = 0;
        }
        int endIndex = ((this.height - this.drawDy) / this.lineHeight) + 1;
        if (endIndex > this.text.length - 1) {
            endIndex = this.text.length - 1;
        }
        for (int i = startIndex; i <= endIndex; i++) {
            Tool.drawString(g, this.text[i], x + ((this.width - this.lineWidth) >> 1), this.drawDy + (this.lineHeight * i) + y, frontColor, bgColor, type);
        }
        g.restore();
    }
}
