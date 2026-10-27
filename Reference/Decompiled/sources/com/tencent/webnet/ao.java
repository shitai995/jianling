package com.tencent.webnet;

import android.view.View;

/* JADX INFO: loaded from: classes.dex */
class ao implements View.OnClickListener {
    final /* synthetic */ WebNetMain gB;

    ao(WebNetMain webNetMain) {
        this.gB = webNetMain;
    }

    @Override // android.view.View.OnClickListener
    public void onClick(View view) {
        this.gB.gr.stopLoading();
        o.c(true);
        this.gB.gr.loadUrl(String.valueOf(this.gB.gs) + "friends-list.html");
    }
}
