package com.tencent.webnet;

import android.webkit.WebView;
import android.webkit.WebViewClient;
import android.widget.Toast;

/* JADX INFO: loaded from: classes.dex */
class aj extends WebViewClient {
    final /* synthetic */ WebNetMain gB;

    aj(WebNetMain webNetMain) {
        this.gB = webNetMain;
    }

    @Override // android.webkit.WebViewClient
    public boolean shouldOverrideUrlLoading(WebView webView, String str) {
        webView.loadUrl(str);
        return true;
    }

    @Override // android.webkit.WebViewClient
    public void onReceivedError(WebView webView, int i, String str, String str2) {
        Toast.makeText(this.gB, "Sorry! " + str, 0).show();
    }
}
