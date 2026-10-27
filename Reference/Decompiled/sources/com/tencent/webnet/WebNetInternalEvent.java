package com.tencent.webnet;

/* JADX INFO: loaded from: classes.dex */
public class WebNetInternalEvent implements WebNetEvent {
    public static final int gb = 1010;
    public static final int gc = 1011;
    public static final int gd = 1030;
    public static final int ge = 1031;

    @Override // com.tencent.webnet.WebNetEvent
    public native boolean SendSMSCB(int i, String str);

    @Override // com.tencent.webnet.WebNetEvent
    public native boolean SyncMicBlogCB(int i, String str);

    protected boolean c(r rVar) {
        return false;
    }

    protected boolean d(r rVar) {
        return false;
    }
}
