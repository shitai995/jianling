package com.tencent.webnet;

import android.os.Build;
import java.io.ByteArrayOutputStream;
import java.io.PrintStream;

/* JADX INFO: loaded from: classes.dex */
public class n implements Thread.UncaughtExceptionHandler {
    protected Thread.UncaughtExceptionHandler ez;

    public static void bh() {
        if (!Thread.getDefaultUncaughtExceptionHandler().getClass().getName().equals("com.tencent.webnet.MyUncaughtExceptionHandler")) {
            new n();
        }
    }

    private n() {
        this.ez = null;
        this.ez = Thread.getDefaultUncaughtExceptionHandler();
        Thread.setDefaultUncaughtExceptionHandler(this);
    }

    @Override // java.lang.Thread.UncaughtExceptionHandler
    public void uncaughtException(Thread thread, Throwable th) {
        h hVarB;
        ByteArrayOutputStream byteArrayOutputStream = new ByteArrayOutputStream();
        th.printStackTrace(new PrintStream(byteArrayOutputStream));
        String packageName = "";
        if (b.ax() != null) {
            packageName = b.ax().getPackageName();
        }
        String str = "\n\tSDK:20120920101810phone:" + Build.MODEL + ",SDK:" + Build.VERSION.SDK + ",framwork:" + Build.VERSION.RELEASE + "game:" + packageName + "\n\t" + byteArrayOutputStream.toString() + "-------------------------------------------------------------------------\n\t";
        if (b.cB) {
            l.a("未捕获异常 = " + str, null);
        } else {
            l.a("global uncatch Exception = " + str, null);
        }
        m mVarAZ = m.aZ();
        try {
            com.qq.WapGame.aq aqVar = new com.qq.WapGame.aq();
            String str2 = (mVarAZ == null || mVarAZ.M("uin") == null) ? "" : (String) mVarAZ.M("uin");
            aqVar.h(str2);
            aqVar.c(b.cb);
            aqVar.d(b.cc);
            aqVar.i("V2.0");
            aqVar.j(b.cd);
            aqVar.t(str);
            hVarB = b.B("reportBug");
            if (hVarB != null) {
                try {
                    if (hVarB.l(aqVar.toByteArray())) {
                        hVarB.aN();
                    }
                } catch (Exception e) {
                }
            }
        } catch (Exception e2) {
            hVarB = null;
        }
        if (hVarB != null) {
            hVarB.aO();
        }
        WebNetInterface.Destroy();
        this.ez.uncaughtException(thread, th);
    }
}
