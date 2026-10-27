package com.tencent.webnet;

import android.view.View;

/* JADX INFO: loaded from: classes.dex */
class ah implements View.OnClickListener {
    final /* synthetic */ WebNetMain gB;

    ah(WebNetMain webNetMain) {
        this.gB = webNetMain;
    }

    @Override // android.view.View.OnClickListener
    public void onClick(View view) {
        o.c(true);
        this.gB.gr.stopLoading();
        this.gB.gr.goBack();
        if (this.gB.gr.canGoBack()) {
            this.gB.gu.setEnabled(true);
        } else {
            this.gB.gu.setEnabled(false);
        }
    }
}
