package com.tencent.webnet;

import android.app.Activity;
import android.content.Context;
import android.content.Intent;
import java.util.Locale;

/* JADX INFO: loaded from: classes.dex */
class b {
    protected static final String cA = "com.tencent.webnet.smssendokrecover";
    protected static final String co = "http://andsngame.3g.qq.com/g/zxcvb/ad/";
    protected static final String cp = "http://andsngame.3g.qq.com/g/android/payForm.g?";
    protected static final String cq = "http://andsngame.3g.qq.com/g/ad/j_down.g?";
    protected static final String cr = "/html/";
    protected static final String cs = "/htmltemp/";
    protected static final String ct = "MyDownLoadConfig";
    protected static final boolean cv = true;
    protected static final String cx = "V2.0";
    protected static final String cy = "20120920101810";
    protected static final String cz = "com.tencent.webnet.smssendrecover";
    protected static WebNetInterface bU = null;
    protected static WebNetEvent bV = null;
    protected static WebNetInternalEvent bW = null;
    protected static t bX = null;
    protected static m bY = null;
    protected static y bZ = null;
    protected static Activity ca = null;
    protected static int cb = 0;
    protected static int cc = 0;
    protected static String cd = null;
    protected static e ce = null;
    protected static WebNetMain cf = null;
    protected static int cg = 0;
    protected static int ch = 0;
    protected static String ci = null;
    protected static String cj = null;
    protected static String ck = null;
    protected static String cl = null;
    protected static c[] cm = null;
    protected static int cn = -1;
    protected static boolean cu = true;
    protected static boolean cw = true;
    protected static final boolean cB = Locale.getDefault().getLanguage().equals("zh-CN");

    b() {
    }

    protected static Activity ax() {
        return cf != null ? cf : ca;
    }

    protected static int ay() {
        try {
            Thread.sleep(100L);
            return 100;
        } catch (Exception e) {
            if (cB) {
                l.a("调用sleep失败 ", e);
            } else {
                l.a("call sleep error ", e);
            }
            return 0;
        }
    }

    protected static h B(String str) {
        return a(str, "");
    }

    protected static h a(String str, String str2) {
        h hVar = new h();
        m mVarAZ = m.aZ();
        String str3 = (mVarAZ == null || mVarAZ.M("sid") == null) ? "" : (String) mVarAZ.M("sid");
        if (hVar.F(co + str + ".g?sid=" + str3 + str2)) {
            return hVar;
        }
        return null;
    }

    protected static boolean a(Context context, String str) {
        return !context.getPackageManager().queryIntentActivities(new Intent(str), 65536).isEmpty();
    }
}
