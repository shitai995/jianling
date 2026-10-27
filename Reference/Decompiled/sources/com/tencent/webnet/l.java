package com.tencent.webnet;

import android.os.Build;
import android.util.Log;
import java.io.ByteArrayOutputStream;
import java.io.PrintStream;

/* JADX INFO: loaded from: classes.dex */
class l {
    private static final String ep = "QQWebNet";

    l() {
    }

    protected static void a(String str, Exception exc) {
        if (exc != null) {
            ByteArrayOutputStream byteArrayOutputStream = new ByteArrayOutputStream();
            exc.printStackTrace(new PrintStream(byteArrayOutputStream));
            str = String.valueOf(str) + "\n\t" + byteArrayOutputStream.toString();
        }
        Log.e(ep, str);
    }

    protected static void b(String str, Exception exc) {
        String str2;
        h hVarB;
        String packageName = "";
        if (b.ax() != null) {
            packageName = b.ax().getPackageName();
        }
        String str3 = "\n\tSDK:" + b.cj + "phone:" + Build.MODEL + ",SDK:" + Build.VERSION.SDK + ",framwork:" + Build.VERSION.RELEASE + "game:" + packageName + "运行时错误！ " + str;
        if (exc != null) {
            ByteArrayOutputStream byteArrayOutputStream = new ByteArrayOutputStream();
            exc.printStackTrace(new PrintStream(byteArrayOutputStream));
            str2 = String.valueOf(str3) + "\n\t" + byteArrayOutputStream.toString() + "-------------------------------------------------------------------------\n\t";
        } else {
            str2 = String.valueOf(str3) + "\n\t-------------------------------------------------------------------------\n\t";
        }
        m mVarAZ = m.aZ();
        try {
            com.qq.WapGame.aq aqVar = new com.qq.WapGame.aq();
            String str4 = (mVarAZ == null || mVarAZ.M("uin") == null) ? "" : (String) mVarAZ.M("uin");
            aqVar.h(str4);
            aqVar.c(b.cb);
            aqVar.d(b.cc);
            aqVar.i("V2.0");
            aqVar.j(b.cd);
            aqVar.t(str2);
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
    }
}
