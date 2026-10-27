package com.dobetter.common;

/* JADX INFO: loaded from: classes.dex */
public class MovingString {
    private String content;
    private int step;
    private int width;
    private int movingTimes = 0;
    private boolean bMove = false;
    private boolean bEnd = false;

    public MovingString(String str, int drawWidth, int moveStep) {
        this.content = str;
        this.width = drawWidth;
        this.step = moveStep;
    }

    public void start() {
        this.bMove = true;
    }

    public void stop() {
        this.movingTimes = 0;
        this.bMove = false;
        this.bEnd = false;
    }

    public void pause() {
        this.bMove = false;
    }

    public boolean isEnd() {
        return this.bEnd;
    }

    public void setContent(String str) {
        this.content = str;
        this.movingTimes = 0;
        this.bMove = false;
        this.bEnd = false;
    }

    public String getContent() {
        return this.content;
    }

    public void setPositionLeft() {
        this.movingTimes = -((this.width / this.step) + 1);
    }

    public void cycle() {
        if (this.content != null && this.bMove) {
            this.movingTimes++;
        }
    }

    public void draw(Graphics g, int x, int y, int frontColor, int bgColor) {
        if (this.content != null) {
            Font font = g.getFont();
            g.setClip(x, y, this.width, Font.fontChinaH);
            int sw = font.stringWidth(this.content);
            if (sw <= this.width) {
                Tool.drawString(g, this.content, x, y, frontColor, bgColor);
            } else {
                int dx = this.movingTimes * this.step;
                if (dx >= sw) {
                    this.bEnd = true;
                    this.movingTimes = -((this.width / this.step) + 1);
                }
                Tool.drawString(g, this.content, x - dx, y, frontColor, bgColor);
            }
            g.restore();
        }
    }

    public void draw(Graphics g, int x, int y, int frontColor, int bgColor, byte type) {
        if (this.content != null) {
            Font font = g.getFont();
            g.setClip(x, y, this.width, Font.fontChinaH);
            int sw = font.stringWidth(this.content);
            if (sw <= this.width) {
                Tool.drawString(g, this.content, x, y, frontColor, bgColor, type);
            } else {
                int dx = this.movingTimes * this.step;
                if (dx >= sw) {
                    this.bEnd = true;
                    this.movingTimes = -((this.width / this.step) + 1);
                }
                Tool.drawString(g, this.content, x - dx, y, frontColor, bgColor, type);
            }
            g.restore();
        }
    }
}
