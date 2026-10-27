package com.tencent.webnet;

import android.os.Handler;
import android.os.Looper;
import android.os.Message;
import android.webkit.WebView;

/* JADX INFO: loaded from: classes.dex */
class p extends Handler {
    final /* synthetic */ o eH;

    /* JADX WARN: 'super' call moved to the top of the method (can break code semantics) */
    p(o oVar, Looper looper) {
        super(looper);
        this.eH = oVar;
    }

    @Override // android.os.Handler
    public void handleMessage(Message message) {
        switch (message.what) {
            case 0:
                h.a(b.ca);
                if (h.aP()) {
                    WebView.enablePlatformNotifications();
                }
                this.eH.eE = new WebView(b.ca);
                WebNetMain.a(this.eH.eE, false);
                this.eH.start();
                break;
        }
    }
}
