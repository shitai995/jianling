package com.tencent.webnet;

import android.content.Intent;
import android.net.Uri;
import android.webkit.DownloadListener;

/* JADX INFO: loaded from: classes.dex */
class al implements DownloadListener {
    final /* synthetic */ WebNetMain gB;

    al(WebNetMain webNetMain) {
        this.gB = webNetMain;
    }

    @Override // android.webkit.DownloadListener
    public void onDownloadStart(String str, String str2, String str3, String str4, long j) {
        this.gB.startActivity(new Intent("android.intent.action.VIEW", Uri.parse(str)));
    }
}
