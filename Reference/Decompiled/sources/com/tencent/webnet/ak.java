package com.tencent.webnet;

import android.webkit.WebChromeClient;
import android.webkit.WebView;

/* JADX INFO: loaded from: classes.dex */
class ak extends WebChromeClient {
    final /* synthetic */ WebNetMain gB;

    ak(WebNetMain webNetMain) {
        this.gB = webNetMain;
    }

    @Override // android.webkit.WebChromeClient
    public void onProgressChanged(WebView webView, int i) {
        super.onProgressChanged(webView, i);
        if (this.gB.gr.canGoBack() && this.gB.gu != null) {
            this.gB.gu.setEnabled(true);
        } else if (this.gB.gu != null) {
            this.gB.gu.setEnabled(false);
        }
        if (this.gB.gA == null || webView.getUrl() == null || webView.getUrl().indexOf("screenCapture") == -1) {
            if (this.gB.gA != null) {
                this.gB.gA.setEnabled(true);
            }
        } else {
            this.gB.gA.setEnabled(false);
        }
        if (i < 100) {
            o.c(true);
        } else {
            o.c(false);
        }
    }
}
