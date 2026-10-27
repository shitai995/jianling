package com.tencent.webnet;

import android.view.View;

/* JADX INFO: loaded from: classes.dex */
class ai implements View.OnClickListener {
    final /* synthetic */ WebNetMain gB;

    ai(WebNetMain webNetMain) {
        this.gB = webNetMain;
    }

    @Override // android.view.View.OnClickListener
    public void onClick(View view) {
        this.gB.finish();
    }
}
