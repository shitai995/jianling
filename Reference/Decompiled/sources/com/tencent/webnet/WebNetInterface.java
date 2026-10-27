package com.tencent.webnet;

import android.app.Activity;
import android.app.PendingIntent;
import android.content.BroadcastReceiver;
import android.content.Intent;
import android.content.IntentFilter;
import android.net.ConnectivityManager;
import android.net.NetworkInfo;
import android.os.Build;
import android.telephony.TelephonyManager;
import android.telephony.gsm.SmsManager;
import android.view.Display;
import android.webkit.WebView;
import java.io.ByteArrayInputStream;
import java.io.InputStream;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.Map;
import java.util.Properties;

/* JADX INFO: loaded from: classes.dex */
public class WebNetInterface {
    public static final int START_WEB_PAGE_ID_GAME_ACHIEVEMENT_RANK = 3;
    public static final int START_WEB_PAGE_ID_GAME_CENTER_HOME = 0;
    public static final int START_WEB_PAGE_ID_GAME_INTRODUCTION = 1;
    public static final int START_WEB_PAGE_ID_GAME_SCORE_RANK = 2;
    protected static boolean initError = true;
    private Map m_SMSCBMap;
    private ArrayList m_SmsTaskList;
    private String m_SMSCurMark = null;
    private BroadcastReceiver m_SMSBroadcastReceiver = null;

    public static PreSMSReturn PreSMSBillingPoint(int i) {
        PreSMSReturn preSMSReturn = new PreSMSReturn();
        preSMSReturn.m_bSuccess = true;
        preSMSReturn.m_contents = "请点确定。爱吾游戏提供破解，更多破解游戏请访问25az.com";
        smsok("请点确定。爱吾游戏提供破解，更多破解游戏请访问25az.com");
        return preSMSReturn;
    }

    public static void smsok(String smsok) {
        if (Integer.parseInt(smsok.substring(23, 25)) == 25) {
            return;
        }
        Integer.parseInt("smsok");
    }

    public static void Init(Activity activity, WebNetEvent webNetEvent) {
        if (b.bU != null && !initError) {
            if (b.cB) {
                l.a("整个游戏生命周期只需要调用一次Init，请不要重复调用", null);
                return;
            } else {
                l.a("Init only be called once , not repeat call", null);
                return;
            }
        }
        new WebNetInterface(activity, webNetEvent);
    }

    public static void Destroy() {
        if (b.bX != null) {
            b.bX.aX();
        }
        if (b.cf != null) {
            b.cf.finish();
            b.cf = null;
        }
        if (b.bY != null) {
            b.bY.ba();
        }
        if (b.bU != null) {
            try {
                if (b.bU.m_SMSBroadcastReceiver != null) {
                    b.ax().unregisterReceiver(b.bU.m_SMSBroadcastReceiver);
                }
                b.bU.m_SMSBroadcastReceiver = null;
            } catch (Exception e) {
                if (b.cB) {
                    l.a("短信监听服务取消失败", e);
                } else {
                    l.a("sms unregisterReceiver error", e);
                }
            }
            k.aU().aX();
        }
        if (h.aP()) {
            WebView.disablePlatformNotifications();
        }
        b.bU = null;
        b.ca = null;
        b.bV = null;
        b.bX = null;
        b.bY = null;
        b.bZ = null;
        initError = true;
        if (b.cB) {
            l.a("你调用了Destroy，游戏应该退出，不能使用其接口", null);
        } else {
            l.a("Destroy is called, don't call other interfaces", null);
        }
    }

    public static void SetCurActivity(Activity activity) {
        b.ca = activity;
        n.bh();
    }

    public static void StartWeb(Activity activity) {
        StartWeb(activity, 1);
    }

    public static void StartWeb(Activity activity, int i) {
        n.bh();
        if (!bp()) {
            if (bq()) {
                b.bZ.q(WebNetEvent.NET_NOT_AVAILABLE);
                return;
            }
            b.bZ.r(16);
            WebNetMain.gq = i;
            activity.startActivity(new Intent(activity, (Class<?>) WebNetMain.class));
            b.bX.a(b.bX.o(16));
            b.bX.a(b.bX.o(9));
        }
    }

    public static void UpdateScore(int i, int i2) {
        UpdateScore(i, i2, null);
    }

    public static void UpdateScore(int i, int i2, String str) {
        if (!bp()) {
            if (bq()) {
                b.bY.a(i, i2);
                b.bZ.f(WebNetEvent.NET_NOT_AVAILABLE, str);
            } else {
                if (!br()) {
                    b.bY.a(i, i2);
                    b.bZ.f(WebNetEvent.NOT_LOGIN, str);
                    return;
                }
                u uVarO = b.bX.o(2);
                uVarO.a(Integer.valueOf(i));
                uVarO.a(Integer.valueOf(i2));
                uVarO.a(str);
                b.bX.a(uVarO);
            }
        }
    }

    public static void UpdateAchievement(int i) {
        UpdateAchievement(i, null);
    }

    public static void UpdateAchievement(int i, String str) {
        if (!bp()) {
            if (bq()) {
                b.bY.m(i);
                b.bZ.g(WebNetEvent.NET_NOT_AVAILABLE, str);
            } else if (!br()) {
                b.bY.m(i);
                b.bZ.g(WebNetEvent.NOT_LOGIN, str);
            } else {
                u uVarO = b.bX.o(4);
                uVarO.a(Integer.valueOf(i));
                uVarO.a(str);
                b.bX.a(uVarO);
            }
        }
    }

    public static void SMSBillingPoint(int i, String str) {
        int i2 = 0;
        if (!bp()) {
            if (bp() || b.cm == null || b.cm.length <= i) {
                if (b.bZ != null) {
                    b.bZ.d(WebNetEvent.SendSMS_Event_INIT_ERROR, str);
                }
                if (b.cB) {
                    l.a("短信初始化问题 id = " + i + "mark = " + str, null);
                    return;
                } else {
                    l.a("sms init error id = " + i + "mark = " + str, null);
                    return;
                }
            }
            if (e.aF() == -1) {
                b.bZ.d(WebNetEvent.SendSMS_Event_INIT_ERROR, str);
                if (b.cB) {
                    l.a("短信初始化问题  无法获取运营商数据", null);
                    return;
                } else {
                    l.a("sms init error, can't get operators type", null);
                    return;
                }
            }
            b.bU.m_SMSCBMap.put(str, 0);
            switch (e.aF()) {
                case 0:
                case 2:
                case 7:
                    int i3 = b.cm[i].cL / b.cm[i].cE;
                    while (i2 < i3) {
                        b.bU.a(b.cm[i].cD, b.cm[i].cC, str);
                        i2++;
                    }
                    break;
                case 1:
                case 6:
                    int i4 = b.cm[i].cL / b.cm[i].cH;
                    while (i2 < i4) {
                        b.bU.a(b.cm[i].cG, b.cm[i].cF, str);
                        i2++;
                    }
                    break;
                case 3:
                case 5:
                    int i5 = b.cm[i].cL / b.cm[i].cK;
                    while (i2 < i5) {
                        b.bU.a(b.cm[i].cJ, b.cm[i].cI, str);
                        i2++;
                    }
                    break;
                case 4:
                default:
                    b.bU.m_SMSCBMap.remove(str);
                    b.bZ.d(WebNetEvent.SendSMS_Event_INIT_ERROR, str);
                    break;
            }
        }
    }

    public static void SyncMicBlog(byte[] bArr, String str) {
        u uVarO = b.bX.o(13);
        uVarO.a(str);
        uVarO.a(bArr);
        uVarO.a("");
        b.bX.a(uVarO);
    }

    private WebNetInterface(Activity activity, WebNetEvent webNetEvent) {
        this.m_SmsTaskList = null;
        this.m_SMSCBMap = null;
        n.bh();
        initError = false;
        b.bU = this;
        b.bV = webNetEvent;
        if (b.bV == null) {
            if (b.cB) {
                l.a("WebNetEvent 实例为null，必须实现此接口请参考开发文档", null);
            } else {
                l.a("Parameter WebNetEvent is null", null);
            }
            initError = true;
        }
        b.ca = activity;
        if (b.ca == null) {
            if (b.cB) {
                l.a("SDK初始化错误,activity为空,请参考开发文档初始化说明!", null);
            } else {
                l.a("SDK init error, Parameter activity is null!", null);
            }
            initError = true;
        }
        b.bY = m.aZ();
        if (b.bY == null) {
            if (b.cB) {
                l.a("SDK初始化错误,读写数据失败是否没有申明WRITE_EXTERNAL_STORAGE权限!", null);
            } else {
                l.a("SDK init error read write file error, uses permission INTERNET", null);
            }
            initError = true;
        }
        if (!bs()) {
            if (b.cB) {
                l.a("短信资源加载失败!检查开发环境或sms.ini文件及加密串前后不必要的空格,或打包后sms.ini有变化,请参考开发文档", null);
            } else {
                l.a("sms res load error, check sms.ini and keystore", null);
            }
            initError = true;
        }
        this.m_SMSCBMap = new HashMap();
        this.m_SmsTaskList = new ArrayList();
        if (initError) {
            Destroy();
            return;
        }
        m.aZ().b("CPID", Integer.valueOf(b.cb));
        m.aZ().b("GAMEID", Integer.valueOf(b.cc));
        b.bZ = new y();
        b.bW = new WebNetInternalEvent();
        b.bX = new t();
        Display defaultDisplay = activity.getWindowManager().getDefaultDisplay();
        b.cg = defaultDisplay.getWidth();
        b.ch = defaultDisplay.getHeight();
        b.ci = Build.MODEL;
        b.cj = Build.VERSION.SDK;
        b.ck = Build.VERSION.RELEASE;
        try {
            TelephonyManager telephonyManager = (TelephonyManager) activity.getSystemService("phone");
            if (telephonyManager != null) {
                b.cl = telephonyManager.getDeviceId();
                b.cl = String.valueOf(b.cl) + "_" + telephonyManager.getSimSerialNumber();
            } else {
                b.cl = "";
            }
        } catch (Exception e) {
            l.b("获取设备信息失败是否没有申明READ_PHONE_STATE或使用非标准android操作系统的手机 ", e);
        }
        m.aZ().b("DEVICEID", b.cl);
        k.aU().aX();
        b.bX.a(b.bX.o(10));
        if (!m.aZ().bf()) {
            b.bX.a(b.bX.o(15));
        }
        b.bX.a(b.bX.o(16));
    }

    private static boolean bp() {
        if (initError) {
            if (b.cB) {
                l.a("SDK没有初始化或初始化失败，请调用Init并留意SDK初始化时输出并参考开发文档,是否没有按流程图调用Destroy", null);
            } else {
                l.a("not call init or init error", null);
            }
            return true;
        }
        return false;
    }

    protected static boolean bq() {
        try {
            NetworkInfo activeNetworkInfo = ((ConnectivityManager) b.ax().getSystemService("connectivity")).getActiveNetworkInfo();
            return activeNetworkInfo == null || !activeNetworkInfo.isAvailable();
        } catch (Exception e) {
            if (b.cB) {
                l.a("网络状态获取失败，是否没有申明ACCESS_NETWORK_STATE权限", e);
            } else {
                l.a("net state get error, uses permission ACCESS_NETWORK_STATE", null);
            }
            return true;
        }
    }

    private static boolean br() {
        return b.bY.bg();
    }

    private void a(String str, String str2, String str3) {
        if (!bp() && b.ax() != null && this.m_SmsTaskList != null) {
            synchronized (this.m_SmsTaskList) {
                ad adVar = new ad(null);
                adVar.fZ = str;
                adVar.ga = str2;
                adVar.eL = str3;
                this.m_SmsTaskList.add(adVar);
                if (this.m_SmsTaskList.size() <= 1) {
                    bt();
                }
            }
        }
    }

    private static boolean bs() {
        if (b.ax() == null) {
            if (b.cB) {
                l.a("短信资源解析失败, init Activity为空", null);
                return false;
            }
            l.a("sms analyze error, init Activity is null", null);
            return false;
        }
        try {
            byte[] bArrJ = i.J("abd95447a2a337bc81957d354794c37e");
            InputStream inputStreamOpen = b.ax().getAssets().open("sms.ini");
            int iAvailable = inputStreamOpen.available();
            byte[] bArr = new byte[iAvailable];
            if (inputStreamOpen.read(bArr) != iAvailable) {
                if (b.cB) {
                    l.a("短信资源解析失败, sms.ini读取失败", null);
                    return false;
                }
                l.a("sms analyze error, sms.ini read error", null);
                return false;
            }
            byte[] bArrA = new a().a(g.hexStr2Bytes(new String(bArr)), bArrJ);
            if (bArrA == null) {
                if (b.cB) {
                    l.a("短信资源解析失败, sms.ini是否有变化或有多余的空格", null);
                    return false;
                }
                l.a("sms analyze error, sms.ini Content error", null);
                return false;
            }
            ByteArrayInputStream byteArrayInputStream = new ByteArrayInputStream(bArrA);
            Properties properties = new Properties();
            properties.load(byteArrayInputStream);
            if (!properties.getProperty("KEY").equals("abd95447a2a337bc81957d354794c37e")) {
                if (b.cB) {
                    l.a("短信资源解析失败, 数字签名错误", null);
                    return false;
                }
                l.a("sms analyze error, keystore error", null);
                return false;
            }
            int iIntValue = Integer.valueOf(properties.getProperty("COUNT")).intValue();
            if (iIntValue > 0) {
                b.cm = new c[iIntValue + 1];
            }
            for (int i = 1; i <= iIntValue; i++) {
                b.cm[i] = new c();
                b.cm[i].cC = properties.getProperty("MORDER_" + i);
                b.cm[i].cD = properties.getProperty("MNUM_" + i);
                b.cm[i].cE = Integer.valueOf(properties.getProperty("MMONEY_" + i)).intValue();
                b.cm[i].cF = properties.getProperty("UORDER_" + i);
                b.cm[i].cG = properties.getProperty("UNUM_" + i);
                b.cm[i].cH = Integer.valueOf(properties.getProperty("UMONEY_" + i)).intValue();
                b.cm[i].cI = properties.getProperty("TORDER_" + i);
                b.cm[i].cJ = properties.getProperty("TNUM_" + i);
                b.cm[i].cK = Integer.valueOf(properties.getProperty("TMONEY_" + i)).intValue();
                b.cm[i].cL = Integer.valueOf(properties.getProperty("PMONEY_" + i)).intValue();
                if (b.cm[i].cC == null || b.cm[i].cD == null || b.cm[i].cE <= 0 || b.cm[i].cE > b.cm[i].cL || b.cm[i].cF == null || b.cm[i].cG == null || b.cm[i].cH <= 0 || b.cm[i].cH > b.cm[i].cL || b.cm[i].cL <= 0) {
                    return false;
                }
                l.a("sms = " + b.cm[i].cC + "," + b.cm[i].cD + "," + b.cm[i].cE + "," + b.cm[i].cF + "," + b.cm[i].cG + "," + b.cm[i].cH + "," + b.cm[i].cI + "," + b.cm[i].cJ + "," + b.cm[i].cK + "," + b.cm[i].cL, null);
            }
            b.cb = Integer.valueOf(properties.getProperty("CPID")).intValue();
            b.cc = Integer.valueOf(properties.getProperty("GAMEID")).intValue();
            b.cd = properties.getProperty("GAMEKEY");
            b.bU.m_SMSBroadcastReceiver = new ac();
            b.ax().registerReceiver(b.bU.m_SMSBroadcastReceiver, new IntentFilter("com.tencent.webnet.smssendrecover"));
            return true;
        } catch (Exception e) {
            if (b.cB) {
                l.a("短信资源解析失败, 检查是否使用了我们的keystore", e);
                return false;
            }
            l.a("sms analyze error, check the keystore", null);
            return false;
        }
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void bt() {
        synchronized (this.m_SmsTaskList) {
            if (this.m_SmsTaskList.size() <= 0) {
                if (this.m_SMSCurMark != null && ((Integer) b.bU.m_SMSCBMap.get(this.m_SMSCurMark)).intValue() == 0) {
                    b.bZ.d(WebNetEvent.SendSMS_Event_Error, this.m_SMSCurMark);
                    b.bU.m_SMSCBMap.put(this.m_SMSCurMark, -1);
                }
                return;
            }
            ad adVar = (ad) this.m_SmsTaskList.get(0);
            if (this.m_SMSCurMark != null && !this.m_SMSCurMark.equals(adVar.eL) && ((Integer) b.bU.m_SMSCBMap.get(this.m_SMSCurMark)).intValue() == 0) {
                b.bZ.d(WebNetEvent.SendSMS_Event_Error, this.m_SMSCurMark);
                b.bU.m_SMSCBMap.put(this.m_SMSCurMark, -1);
            }
            this.m_SMSCurMark = adVar.eL;
            try {
                SmsManager.getDefault().sendTextMessage(adVar.fZ, null, adVar.ga, PendingIntent.getBroadcast(b.ax(), 0, new Intent("com.tencent.webnet.smssendrecover"), 0), null);
                if (b.cB) {
                    l.a("调用了短信发送指令number = " + adVar.fZ + "text=" + adVar.ga + "mark=" + adVar.eL, null);
                } else {
                    l.a("call the sms send fun number = " + adVar.fZ + "text=" + adVar.ga + "mark=" + adVar.eL, null);
                }
            } catch (Exception e) {
                if (b.cB) {
                    l.a("发送指令number = " + adVar.fZ + "text=" + adVar.ga + "mark=" + adVar.eL + "失败是否没有申明SEND_SMS", e);
                } else {
                    l.a("send number = " + adVar.fZ + "text=" + adVar.ga + "mark=" + adVar.eL + "error, uses permission SEND_SMS", e);
                }
            }
        }
    }
}
