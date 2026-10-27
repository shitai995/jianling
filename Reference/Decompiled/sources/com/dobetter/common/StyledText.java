package com.dobetter.common;

import com.dobetter.action.ActionSet;
import java.util.Stack;
import java.util.Vector;

/* JADX INFO: loaded from: classes.dex */
public class StyledText {
    private byte bPageIndex;
    private byte bPageNum;
    public Vector content;
    private Style defaultStyle;
    private Font font;
    private int iIconWidth;
    private int iLineHeight;
    private int iLineWidth;
    public int iOffY;
    public int iScrollStep;
    public int iStartLineIndex;
    private int iVisibleContentWidth;
    public int iVisibleHeight;
    private int iVisibleLineNum;
    private ActionSet iconAS;
    private boolean isScroll;
    public boolean isToClosed;
    private int offX;

    public StyledText(String str, int contentWidth, Font fn, Style style, ActionSet as, int iconW) {
        this.iVisibleContentWidth = contentWidth;
        this.font = fn;
        this.defaultStyle = style;
        this.iconAS = as;
        if (this.iconAS == null) {
            this.iIconWidth = 0;
        } else {
            this.iIconWidth = iconW;
        }
        formatText(str);
    }

    public StyledText(String str, int contentWidth, int drawHeight, int scrollStep, int startPos, Font font) {
        this.iVisibleContentWidth = contentWidth;
        this.font = font;
        this.iLineHeight = Font.fontChinaH;
        this.iVisibleHeight = drawHeight;
        this.iScrollStep = scrollStep;
        this.iOffY = 0;
        formatText(str);
        this.iVisibleLineNum = this.content.size();
        if (this.content.size() * this.iLineHeight > drawHeight) {
            this.isScroll = true;
            this.iOffY = startPos;
        }
    }

    public StyledText(int contentWidth, int drawLineNum, int lineHeight, Font font) {
        this.iLineHeight = lineHeight;
        this.iVisibleLineNum = drawLineNum;
        this.iVisibleContentWidth = contentWidth;
        this.iVisibleHeight = drawLineNum * lineHeight;
        this.font = font;
    }

    public StyledText(String str, int contentWidth, int drawLineNum, Font font, int lineHeight, ActionSet as, int iconWidth) {
        this.iLineHeight = lineHeight;
        this.iVisibleLineNum = drawLineNum;
        this.iVisibleContentWidth = contentWidth;
        this.iVisibleHeight = drawLineNum * lineHeight;
        this.iconAS = as;
        this.font = font;
        if (this.iconAS != null) {
            this.iIconWidth = iconWidth;
        }
        formatText(str);
    }

    public void formatText(String str) {
        if (this.iVisibleLineNum == 1) {
            this.iVisibleContentWidth = this.font.stringWidth(str);
        }
        Stack styleStack = new Stack();
        if (this.defaultStyle == null) {
            this.defaultStyle = new Style();
        }
        styleStack.push(this.defaultStyle);
        this.content = new Vector();
        boolean isSwitchLine = true;
        int curLineW = 0;
        Vector curLine = null;
        Object itemToAdded = null;
        StringBuffer curSB = null;
        int i = 0;
        while (true) {
            Object obj = itemToAdded;
            if (i >= str.length()) {
                break;
            }
            if (isSwitchLine) {
                curLine = new Vector();
                curLine.addElement(styleStack.peek());
                curLineW = 0;
                curSB = new StringBuffer();
                if (obj != null) {
                    if (obj instanceof Character) {
                        curSB.append(((Character) obj).charValue());
                    } else if ((obj instanceof Style) && ((Style) obj).type == 2 && this.iconAS != null) {
                        curLine.addElement(obj);
                        curLineW = 0 + this.iIconWidth;
                    }
                    itemToAdded = null;
                } else {
                    itemToAdded = obj;
                }
                this.content.addElement(curLine);
                isSwitchLine = false;
            } else {
                itemToAdded = obj;
            }
            char ch = str.charAt(i);
            boolean isChar = false;
            if (ch == '\n') {
                if (curSB.length() > 0) {
                    curLine.addElement(curSB.toString());
                }
                i++;
                isSwitchLine = true;
            } else if (ch == '<') {
                char ch1 = str.charAt(i + 1);
                int flagID = Style.getFlagID(ch1);
                if (flagID >= 0) {
                    Style sty = null;
                    int fendi = str.indexOf(62, i);
                    if (fendi >= 0) {
                        String flagStr = str.substring(i + 1, fendi);
                        sty = Style.createStyle(flagStr);
                    }
                    if (sty == null) {
                        isChar = true;
                    } else if (sty.type == 0) {
                        if (curSB.length() > 0) {
                            curLine.addElement(curSB.toString());
                            curLineW += this.font.stringWidth(curSB.toString());
                            curSB = new StringBuffer();
                        }
                        if (styleStack.size() > 1) {
                            styleStack.pop();
                        }
                        curLine.addElement(styleStack.peek());
                        i = fendi + 1;
                    } else if (sty.type == 1) {
                        if (curSB.length() > 0) {
                            curLine.addElement(curSB.toString());
                            curLineW += this.font.stringWidth(curSB.toString());
                            curSB = new StringBuffer();
                        }
                        curLine.addElement(styleStack.push(sty));
                        i = fendi + 1;
                    } else if (sty.type == 2) {
                        if (this.iconAS != null) {
                            if (curSB.length() > 0) {
                                curLine.addElement(curSB.toString());
                                curLineW += this.font.stringWidth(curSB.toString());
                                curSB = new StringBuffer();
                            }
                            if (curLineW + this.iIconWidth > this.iVisibleContentWidth) {
                                isSwitchLine = true;
                                itemToAdded = sty;
                            } else {
                                curLine.addElement(sty);
                                curLineW += this.iIconWidth;
                            }
                        }
                        i = fendi + 1;
                    }
                } else {
                    isChar = true;
                }
            } else {
                isChar = true;
            }
            if (isChar) {
                int newLineW = this.font.stringWidth(curSB.toString()) + curLineW + Font.fontChinaW;
                if (newLineW > this.iVisibleContentWidth) {
                    if (curSB.length() > 0) {
                        curLine.addElement(curSB.toString());
                    }
                    if (i != str.length() - 1) {
                        itemToAdded = new Character(ch);
                        isSwitchLine = true;
                    } else {
                        curLine = new Vector();
                        curLine.addElement(styleStack.peek());
                        curLineW = Font.fontChinaW;
                        curSB = new StringBuffer();
                        curSB.append(ch);
                        curLine.addElement(curSB.toString());
                        this.content.addElement(curLine);
                    }
                } else {
                    curSB.append(ch);
                    if (i == str.length() - 1) {
                        curLine.addElement(curSB.toString());
                        curLineW = newLineW;
                        break;
                    }
                }
                i++;
            }
        }
        if (curLineW <= 0) {
            curLineW = this.iVisibleContentWidth;
        }
        this.iLineWidth = curLineW;
        if (this.iVisibleLineNum > 0) {
            this.bPageNum = (byte) (getTotalLineNum() / this.iVisibleLineNum);
            if (getTotalLineNum() % this.iVisibleLineNum == 0) {
                this.bPageNum = (byte) (this.bPageNum - 1);
            }
        }
        this.bPageIndex = (byte) 0;
    }

    public void drawLine(Graphics g, int index, int dx, int dy) {
        if (index >= 0 && index < this.content.size()) {
            Vector line = (Vector) this.content.elementAt(index);
            drawLine(g, line, dx, dy);
        }
    }

    public void drawLine(Graphics g, Vector line, int dx, int dy) {
        Style style = new Style();
        for (int i = 0; i < line.size(); i++) {
            Object ob = line.elementAt(i);
            if (ob instanceof Style) {
                Style sty = (Style) ob;
                if (sty.type == 1) {
                    style = sty;
                } else if (sty.type == 2 && this.iconAS != null) {
                    int actIndex = sty.iconID;
                    if (sty.iconID >= this.iconAS.actionNum) {
                        actIndex = 0;
                    }
                    this.iconAS.drawFrameCycle(g, actIndex, Tool.countTimes, dx, dy, false);
                    Common.setUIClip(g);
                    dx += this.iIconWidth;
                }
            } else if (ob instanceof String) {
                String str = (String) ob;
                Tool.drawString(g, str, dx, dy, style.fontColor, style.backColor, style.fontType);
                dx += this.font.stringWidth(str);
            }
        }
    }

    public void draw(Graphics g, int x, int y) {
        g.setClip(x, y, this.iVisibleContentWidth + 1, this.iVisibleHeight + 1);
        for (int i = 0; i < this.iVisibleLineNum; i++) {
            if (this.iStartLineIndex + i < this.content.size()) {
                drawLine(g, this.iStartLineIndex + i, this.offX + x, this.iOffY + y);
                y += this.iLineHeight;
            }
        }
        g.restore();
    }

    public void cycle() {
        this.isToClosed = false;
        if (this.isScroll) {
            this.iOffY -= this.iScrollStep;
            if (this.iOffY <= (-this.content.size()) * this.iLineHeight) {
                this.iOffY = this.iVisibleHeight;
                this.isToClosed = true;
            }
        }
    }

    public void handleKey() {
        if (Common.isOKPressed(true)) {
            nextPage();
        }
    }

    public void prePage() {
        this.isToClosed = false;
        if (this.bPageIndex > 0) {
            this.bPageIndex = (byte) (this.bPageIndex - 1);
            this.iStartLineIndex -= this.iVisibleLineNum;
        }
    }

    public void nextPage() {
        this.isToClosed = false;
        if (this.bPageIndex < this.bPageNum) {
            this.bPageIndex = (byte) (this.bPageIndex + 1);
            this.iStartLineIndex += this.iVisibleLineNum;
        } else {
            this.isToClosed = true;
        }
    }

    public void start() {
        this.isScroll = true;
    }

    public void stop() {
        this.iOffY = 0;
        this.isScroll = false;
    }

    public void pause() {
        this.isScroll = false;
    }

    public int getLineWidth() {
        return this.iLineWidth;
    }

    public int getLineHeight() {
        return this.iLineHeight;
    }

    public int getTotalLineNum() {
        return this.content.size();
    }

    public byte getPageNum() {
        return this.bPageNum;
    }

    public byte getPageIndex() {
        return this.bPageIndex;
    }

    public int getWidth() {
        return this.iVisibleContentWidth;
    }

    public int getHeight() {
        return this.iLineHeight * this.iVisibleLineNum;
    }

    public void setLineCenter() {
        if (this.iVisibleLineNum <= 1) {
            this.offX = (this.iVisibleContentWidth - this.iLineWidth) >> 1;
        }
    }

    public void close() {
        this.content = null;
        this.defaultStyle = null;
        this.font = null;
        if (this.iconAS != null) {
            this.iconAS.releasImg();
            this.iconAS = null;
        }
    }
}
