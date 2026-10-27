package com.tencent.webnet;

import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;

/* JADX INFO: loaded from: classes.dex */
class ac extends BroadcastReceiver {
    ac() {
    }

    @Override // android.content.BroadcastReceiver
    public void onReceive(Context context, Intent intent) {
        String str = b.bU.m_SMSCurMark;
        if (b.bU.m_SMSCBMap.get(str) != null) {
            int resultCode = getResultCode();
            synchronized (b.bU.m_SmsTaskList) {
                try {
                    switch (resultCode) {
                        case -1:
                            if (((Integer) b.bU.m_SMSCBMap.get(str)).intValue() != 1) {
                                if (b.cB) {
                                    l.a("发送短信成功 mark= " + str, null);
                                } else {
                                    l.a("sms send ok mark= " + str, null);
                                }
                                b.bZ.d(WebNetEvent.SendSMS_Event_OK, str);
                                b.bU.m_SMSCBMap.put(str, 1);
                            }
                            if (b.bU.m_SmsTaskList.size() > 0) {
                                b.bU.m_SmsTaskList.remove(0);
                            }
                            b.bU.bt();
                            return;
                        case 0:
                        default:
                            if (b.cB) {
                                l.a("未知短信错误代码mark= " + str + "code = " + resultCode, null);
                            } else {
                                l.a("sms send error unknow error mark= " + str + "code = " + resultCode, null);
                            }
                            l.b("SMSReceiver get code = " + resultCode, null);
                            if (b.bU.m_SmsTaskList.size() > 0) {
                                b.bU.m_SmsTaskList.remove(0);
                            }
                            b.bU.bt();
                            return;
                        case 1:
                        case 2:
                        case 3:
                        case 4:
                            if (b.cB) {
                                l.a("发送短信失败 mark= " + str + "code = " + resultCode, null);
                            } else {
                                l.a("sms send error mark= " + str + "code = " + resultCode, null);
                            }
                            if (b.bU.m_SmsTaskList.size() > 0) {
                                b.bU.m_SmsTaskList.remove(0);
                            }
                            b.bU.bt();
                            return;
                    }
                } catch (Throwable th) {
                    throw th;
                }
            }
        }
    }
}
