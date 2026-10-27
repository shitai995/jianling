package com.tencent.webnet;

import android.content.Intent;
import android.content.pm.PackageManager;
import android.content.pm.ResolveInfo;
import android.os.Message;
import android.view.View;
import android.widget.Toast;
import java.util.LinkedHashMap;
import java.util.List;

/* JADX INFO: loaded from: classes.dex */
final class JavaScript {
    private boolean m_bShow;

    protected JavaScript(boolean z) {
        this.m_bShow = true;
        this.m_bShow = z;
    }

    public void SetData(String str, String str2) {
        if (m.aZ() != null) {
            m.aZ().b(str, str2);
        }
    }

    public String GetData(String str) {
        Object objM;
        if (m.aZ() == null || (objM = m.aZ().M(str)) == null) {
            return "";
        }
        return objM.toString();
    }

    public String CheckApk(String str) {
        if (str == null) {
            return "0";
        }
        try {
            b.ca.getPackageManager().getApplicationInfo(str, 8192);
            return "1";
        } catch (PackageManager.NameNotFoundException e) {
            return "0";
        } catch (Exception e2) {
            return "0";
        }
    }

    public String RunApk(String str) {
        Intent launchIntentForPackage;
        List<ResolveInfo> listQueryIntentActivities = null;
        if (b.cf == null) {
            return "false";
        }
        if (str == b.cf.getPackageName()) {
            b.cf.finish();
            return "true";
        }
        try {
            launchIntentForPackage = b.cf.getPackageManager().getLaunchIntentForPackage(str);
            launchIntentForPackage.setAction("android.intent.action.MAIN");
            launchIntentForPackage.addCategory("android.intent.category.LAUNCHER");
            launchIntentForPackage.setFlags(268435456);
            listQueryIntentActivities = b.cf.getPackageManager().queryIntentActivities(launchIntentForPackage, 0);
        } catch (Exception e) {
            launchIntentForPackage = null;
        }
        if (listQueryIntentActivities == null || listQueryIntentActivities.size() <= 0) {
            return "false";
        }
        b.cf.startActivity(launchIntentForPackage);
        return "true";
    }

    public String GetGpsInfo() {
        LinkedHashMap linkedHashMap = new LinkedHashMap();
        if (b.ce != null) {
            linkedHashMap.put("iX", Double.toString(e.aD()));
            linkedHashMap.put("iY", Double.toString(e.aC()));
            linkedHashMap.put("iMCC", Integer.valueOf(e.aE()));
            linkedHashMap.put("iMNC", Integer.valueOf(e.aF()));
            linkedHashMap.put("iLAC", Integer.valueOf(e.aH()));
            linkedHashMap.put("iCellId", Integer.valueOf(e.aG()));
            linkedHashMap.put("clientIp", Integer.toString(e.aJ()));
            linkedHashMap.put("wifiMac", e.aK());
        } else {
            linkedHashMap.put("iX", "0");
            linkedHashMap.put("iY", "0");
            linkedHashMap.put("iMCC", 0);
            linkedHashMap.put("iMNC", 0);
            linkedHashMap.put("iLAC", 0);
            linkedHashMap.put("iCellId", 0);
            linkedHashMap.put("clientIp", "");
            linkedHashMap.put("wifiMac", "");
        }
        return com.tencent.webnet.a.c.f(linkedHashMap);
    }

    public String GetTermInfo() {
        LinkedHashMap linkedHashMap = new LinkedHashMap();
        linkedHashMap.put("OSName", b.ci);
        linkedHashMap.put("OSVer", b.ck);
        return com.tencent.webnet.a.c.f(linkedHashMap);
    }

    public String PayCB(String str, String str2) {
        return "true";
    }

    public String RunSelf() {
        if (b.cf == null) {
            return "false";
        }
        b.cf.finish();
        return "true";
    }

    public String GetApkVersion(String str) {
        try {
            return new StringBuilder().append(b.ax().getPackageManager().getPackageInfo(str, 0).versionCode).toString();
        } catch (Exception e) {
            return "0";
        }
    }

    public String ToPage(String str) {
        if (!this.m_bShow || b.cf == null) {
            return "false";
        }
        b.cf.gr.stopLoading();
        o.c(true);
        if (str.startsWith("file:///")) {
            b.cf.gr.loadUrl(str);
        } else {
            b.cf.gr.loadUrl(String.valueOf(b.cf.gs) + str);
        }
        return "true";
    }

    public String IsAndriodSDK() {
        return "true";
    }

    public void ShowMessage(String str) {
        if (this.m_bShow && b.cf != null && str != null) {
            Toast toastMakeText = Toast.makeText(b.cf, str, 0);
            View view = toastMakeText.getView();
            view.getBackground().setAlpha(178);
            toastMakeText.setView(view);
            toastMakeText.show();
        }
    }

    public void ClearHistory() {
        if (this.m_bShow && b.cf != null) {
            b.cf.gr.clearHistory();
            if (b.cf.gu != null) {
                Message message = new Message();
                message.what = 1;
                message.arg1 = 0;
                WebNetMain.eC.sendMessage(message);
            }
        }
    }

    public void HadDownload(String str, String str2) {
        m mVarAZ = m.aZ();
        String str3 = (mVarAZ == null || mVarAZ.M("sid") == null) ? "" : (String) mVarAZ.M("sid");
        new h().a("http://andsngame.3g.qq.com/g/ad/j_down.g?sid=" + str3 + "&cpId=" + str + "&gameId=" + str2, null);
    }

    public void Login() {
        if (b.bX != null) {
            b.bX.a(b.bX.o(12));
            b.bX.a(b.bX.o(9));
        }
        m.aZ().b("uin", "");
    }

    public void SyncMicroblogging(String str, String str2) {
        if (b.cf != null) {
            byte[] bArrD = d.D("screenshots.jpeg");
            if (bArrD == null) {
                Toast toastMakeText = Toast.makeText(b.cf, "同步微博失败", 0);
                View view = toastMakeText.getView();
                view.getBackground().setAlpha(178);
                toastMakeText.setView(view);
                toastMakeText.show();
                return;
            }
            u uVarO = b.bX.o(13);
            uVarO.a(str);
            uVarO.a(bArrD);
            uVarO.a(str2);
            b.bX.a(uVarO);
        }
    }

    public String GetBuildVersion() {
        return "V2.0_20120920101810";
    }

    public String HaveCame() {
        return (b.cf != null && b.a(b.cf, "MediaStore.ACTION_IMAGE_CAPTURE")) ? "true" : "false";
    }

    public void SetMessageNum(String str) {
        if (str != null) {
            b.cn = Integer.valueOf(str).intValue();
            if (b.cf != null) {
                Message message = new Message();
                message.what = 2;
                WebNetMain.eC.sendMessage(message);
            }
        }
    }
}
