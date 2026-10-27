package com.tencent.webnet;

import android.os.Handler;
import android.os.Looper;
import android.os.Message;

/* JADX INFO: loaded from: classes.dex */
class ae extends Handler {
    final /* synthetic */ WebNetMain gB;

    /* JADX WARN: 'super' call moved to the top of the method (can break code semantics) */
    ae(WebNetMain webNetMain, Looper looper) {
        super(looper);
        this.gB = webNetMain;
    }

    @Override // android.os.Handler
    public void handleMessage(Message message) {
        switch (message.what) {
            case 1:
                this.gB.gu.setEnabled(message.arg1 != 0);
                break;
            case 2:
                if (this.gB.gx != null) {
                    if (b.cn > 99) {
                        this.gB.gx.setMessage("99+");
                    } else if (b.cn > 0) {
                        this.gB.gx.setMessage(new StringBuilder().append(b.cn).toString());
                    } else {
                        this.gB.gx.setMessage(null);
                    }
                }
                break;
        }
    }
}
