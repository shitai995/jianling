package com.tencent.webnet;

import android.content.Context;
import android.graphics.Bitmap;
import android.graphics.Canvas;
import android.graphics.Paint;
import android.graphics.Typeface;
import android.graphics.drawable.BitmapDrawable;
import android.widget.Button;

/* JADX INFO: loaded from: classes.dex */
public class MessageButton extends Button {
    protected static final int ALIGN_BOTTOM = 1;
    protected static final int ALIGN_TOP = 0;
    protected static final int dR = 2;
    protected static final int dS = 3;
    protected static final int dT = 4;
    protected static final int dU = 5;
    private Bitmap dV;
    private Bitmap dW;
    private Bitmap dX;
    private Bitmap dY;
    private Bitmap dZ;
    private Bitmap ea;
    private Bitmap eb;
    private Bitmap ec;
    private int ed;
    private boolean ee;

    protected MessageButton(Context context, Bitmap bitmap, Bitmap bitmap2, Bitmap bitmap3, boolean z) {
        super(context);
        this.dV = null;
        this.dW = null;
        this.dX = null;
        this.dY = null;
        this.dZ = null;
        this.ea = null;
        this.eb = null;
        this.ec = null;
        this.ed = 4;
        this.ee = true;
        try {
            this.eb = new BitmapDrawable(context.getAssets().open("QQGameCenter/messagebutton.png")).getBitmap();
            this.ec = new BitmapDrawable(context.getAssets().open("QQGameCenter/messagebutton_big.png")).getBitmap();
        } catch (Exception e) {
            if (b.cB) {
                l.a("数字按钮资源访问失败是否没有正确使用sdk，请不要将assets目录迁移到其他地方", e);
            } else {
                l.a("number button get res error, don't move assets dir", e);
            }
        }
        this.dV = bitmap;
        this.dY = bitmap2;
        this.dW = bitmap2;
        this.dZ = bitmap3;
        this.dX = bitmap3;
        this.ea = this.dW;
        setBackgroundDrawable(null);
        if (z) {
            setWidth(this.dW.getWidth() > this.dX.getWidth() ? this.dW.getWidth() : this.dX.getWidth());
            setHeight(this.dW.getHeight() > this.dX.getHeight() ? this.dW.getHeight() : this.dX.getHeight());
        }
        setOnTouchListener(new j(this));
    }

    protected void l(int i) {
        this.ed = i;
    }

    protected void setMessage(String str) {
        int width;
        int height;
        Bitmap.Config config;
        if (str == null) {
            if (this.ea == this.dY) {
                this.ea = this.dW;
            } else if (this.ea == this.dZ) {
                this.ea = this.dX;
            }
            this.dY = this.dW;
            this.dZ = this.dX;
            invalidate();
            return;
        }
        Bitmap bitmap = str.length() > 2 ? this.ec : this.eb;
        if (bitmap != null) {
            int width2 = bitmap.getWidth();
            int height2 = bitmap.getHeight();
            Bitmap bitmapCreateBitmap = Bitmap.createBitmap(width2, height2, bitmap.getConfig());
            Canvas canvas = new Canvas(bitmapCreateBitmap);
            Paint paint = new Paint();
            paint.setDither(true);
            paint.setFilterBitmap(true);
            canvas.drawBitmap(bitmap, 0.0f, 0.0f, paint);
            Paint paint2 = new Paint(257);
            paint2.setColor(-1);
            paint2.setTextSize(10.0f);
            paint2.setTextAlign(Paint.Align.CENTER);
            paint2.setTypeface(Typeface.DEFAULT_BOLD);
            canvas.drawText(str, (width2 / 2) + 2, (height2 / 2) + 3, paint2);
            canvas.save(31);
            canvas.restore();
            Bitmap[] bitmapArr = new Bitmap[2];
            for (int i = 0; i < 2; i++) {
                if (i == 0) {
                    width = this.dW.getWidth();
                    height = this.dW.getHeight();
                    config = this.dW.getConfig();
                } else {
                    width = this.dX.getWidth();
                    height = this.dX.getHeight();
                    config = this.dX.getConfig();
                }
                bitmapArr[i] = Bitmap.createBitmap(width, height, config);
                Canvas canvas2 = new Canvas(bitmapArr[i]);
                if (i == 0) {
                    canvas2.drawBitmap(this.dW, 0.0f, 0.0f, paint);
                } else {
                    canvas2.drawBitmap(this.dX, 0.0f, 0.0f, paint);
                }
                canvas2.drawBitmap(bitmapCreateBitmap, (width - bitmapCreateBitmap.getWidth()) - 15, 5.0f, paint);
                canvas2.save(31);
                canvas2.restore();
            }
            if (this.ea == this.dY) {
                this.ea = bitmapArr[0];
            } else if (this.ea == this.dZ) {
                this.ea = bitmapArr[1];
            }
            this.dY = bitmapArr[0];
            this.dZ = bitmapArr[1];
            invalidate();
        }
    }

    @Override // android.widget.TextView, android.view.View
    public void setEnabled(boolean z) {
        this.ee = z;
        if (!z) {
            this.ea = this.dV;
        } else {
            this.ea = this.dY;
        }
        super.setEnabled(z);
        invalidate();
    }

    @Override // android.widget.TextView, android.view.View
    protected void onDraw(Canvas canvas) {
        int width;
        int height = 0;
        if (this.ea != null) {
            switch (this.ed) {
                case 0:
                    width = 0;
                    break;
                case 1:
                    width = 0;
                    height = getHeight() - this.ea.getHeight();
                    break;
                case 2:
                    width = (getWidth() - this.ea.getWidth()) >> 1;
                    break;
                case 3:
                    width = 0;
                    height = (getHeight() - this.ea.getHeight()) >> 1;
                    break;
                case 4:
                    width = (getWidth() - this.ea.getWidth()) >> 1;
                    height = (getHeight() - this.ea.getHeight()) >> 1;
                    break;
                case 5:
                    width = (getWidth() - this.ea.getWidth()) >> 1;
                    height = getHeight() - this.ea.getHeight();
                    break;
                default:
                    width = 0;
                    break;
            }
            canvas.drawBitmap(this.ea, width, height, (Paint) null);
        }
        super.onDraw(canvas);
    }
}
