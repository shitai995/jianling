package com.tencent.webnet;

import android.view.MotionEvent;
import android.view.View;

/* JADX INFO: loaded from: classes.dex */
class ap implements View.OnTouchListener {
    final /* synthetic */ WebNetMain.MyButton gC;

    ap(WebNetMain.MyButton myButton) {
        this.gC = myButton;
    }

    @Override // android.view.View.OnTouchListener
    public boolean onTouch(View view, MotionEvent motionEvent) {
        if (!this.gC.ee) {
            this.gC.ea = this.gC.dV;
        } else if (motionEvent.getAction() == 0) {
            this.gC.ea = this.gC.dX;
            this.gC.invalidate();
        } else if (motionEvent.getAction() == 1 || motionEvent.getAction() == 3 || motionEvent.getAction() == 4) {
            this.gC.ea = this.gC.dW;
            this.gC.invalidate();
        } else if (this.gC.isPressed() && this.gC.ea != this.gC.dX) {
            this.gC.ea = this.gC.dX;
            this.gC.invalidate();
        } else if (!this.gC.isPressed() && this.gC.ea != this.gC.dW) {
            this.gC.ea = this.gC.dW;
            this.gC.invalidate();
        }
        return false;
    }
}
