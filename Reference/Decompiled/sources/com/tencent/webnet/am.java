package com.tencent.webnet;

import android.view.View;

/* JADX INFO: loaded from: classes.dex */
class am implements View.OnClickListener {
    final /* synthetic */ WebNetMain gB;

    am(WebNetMain webNetMain) {
        this.gB = webNetMain;
    }

    @Override // android.view.View.OnClickListener
    public void onClick(View view) {
        this.gB.gr.stopLoading();
        o.c(true);
        this.gB.gr.loadUrl(String.valueOf(this.gB.gs) + "center.html");
    }
}
