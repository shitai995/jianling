package com.tencent.webnet;

import android.view.MotionEvent;
import android.view.View;

/* JADX INFO: loaded from: classes.dex */
class j implements View.OnTouchListener {
    final /* synthetic */ MessageButton ef;

    j(MessageButton messageButton) {
        this.ef = messageButton;
    }

    @Override // android.view.View.OnTouchListener
    public boolean onTouch(View view, MotionEvent motionEvent) {
        if (!this.ef.ee) {
            this.ef.ea = this.ef.dV;
        } else if (motionEvent.getAction() == 0) {
            this.ef.ea = this.ef.dZ;
            this.ef.invalidate();
        } else if (motionEvent.getAction() == 1 || motionEvent.getAction() == 3 || motionEvent.getAction() == 4) {
            this.ef.ea = this.ef.dY;
            this.ef.invalidate();
        } else if (this.ef.isPressed() && this.ef.ea != this.ef.dY) {
            this.ef.ea = this.ef.dZ;
            this.ef.invalidate();
        } else if (!this.ef.isPressed() && this.ef.ea != this.ef.dY) {
            this.ef.ea = this.ef.dY;
            this.ef.invalidate();
        }
        return false;
    }
}
