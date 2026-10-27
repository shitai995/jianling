package com.dobetter.client;

import android.util.Log;
import com.tencent.webnet.WebNetEvent;

/* JADX INFO: loaded from: classes.dex */
public class GameCenterEvent implements WebNetEvent {
    public static int isSucceed;

    /* JADX WARN: Unreachable blocks removed: 1, instructions: 10 */
    @Override // com.tencent.webnet.WebNetEvent
    public boolean SendSMSCB(int event, String mark) {
        switch (event) {
            case WebNetEvent.SendSMS_Event_OK /* 1040 */:
            case WebNetEvent.SendSMS_Event_Error /* 1041 */:
            case WebNetEvent.SendSMS_Event_Generic_Failure /* 1042 */:
            case WebNetEvent.SendSMS_Event_Radio_Off /* 1043 */:
            case WebNetEvent.SendSMS_Event_NULL_PDU /* 1044 */:
            case WebNetEvent.SendSMS_Event_INIT_ERROR /* 1045 */:
                if (mark.equals("计费点1第一次发送")) {
                    Log.i("计费log", "计费点1第一次发送收费成功");
                } else if (mark.equals("计费点2第二次发送")) {
                    Log.i("计费log", "计费点2第二次发送收费成功");
                } else {
                    Log.i("计费log", "我好像没有要发送计费，这不可能发生，难道是上次游戏启动的计费请求？！！！");
                }
                isSucceed = 1;
                break;
        }
        return false;
    }

    @Override // com.tencent.webnet.WebNetEvent
    public boolean SyncMicBlogCB(int arg0, String arg1) {
        return false;
    }
}
