package com.tencent.webnet;

import android.os.Handler;
import android.os.Looper;
import android.os.Message;
import android.support.v4.util.TimeUtils;
import com.dobetter.common.Graphics;
import com.dobetter.script.Interpreter;

/* JADX INFO: loaded from: classes.dex */
class z extends Handler {
    final /* synthetic */ y fX;

    /* JADX WARN: 'super' call moved to the top of the method (can break code semantics) */
    z(y yVar, Looper looper) {
        super(looper);
        this.fX = yVar;
    }

    @Override // android.os.Handler
    public void handleMessage(Message message) {
        switch (message.what) {
            case 0:
            case 4:
            case 8:
                this.fX.a("QQ游戏中心", "正在上传数据，请稍候。", null, null, null);
                break;
            case 1:
                r rVar = (r) message.obj;
                if (b.cu) {
                    b.bW.c(rVar);
                }
                switch (rVar.as) {
                    case WebNetEvent.NOT_LOGIN /* -102 */:
                    case -2:
                        this.fX.R("您没有登录！将在下次登录后上传积分");
                        break;
                    case WebNetEvent.NET_NOT_AVAILABLE /* -101 */:
                        this.fX.R("网络没有链接！将在下次登录后上传积分");
                        break;
                    case 0:
                        this.fX.R("上传积分成功");
                        break;
                    default:
                        this.fX.R("网络拥堵！请稍候上传积分");
                        break;
                }
                break;
            case 2:
            case 6:
            case 10:
                this.fX.a("QQ游戏中心", "正在获取数据，请稍候。", null, null, null);
                break;
            case 3:
                int i = message.arg1;
                if (b.cu) {
                }
                switch (i) {
                    case WebNetEvent.NOT_LOGIN /* -102 */:
                    case -2:
                        this.fX.R("您没有登录！将在下次登录后获取积分");
                        break;
                    case WebNetEvent.NET_NOT_AVAILABLE /* -101 */:
                        this.fX.R("网络没有链接！将在下次登录后获取积分");
                        break;
                    case 0:
                        this.fX.R("获取积分成功");
                        break;
                    default:
                        this.fX.R("网络拥堵！请稍候获取积分");
                        break;
                }
                break;
            case 5:
                r rVar2 = (r) message.obj;
                if (b.cu) {
                    b.bW.d(rVar2);
                }
                switch (rVar2.as) {
                    case WebNetEvent.NOT_LOGIN /* -102 */:
                    case -2:
                        this.fX.R("您没有登录！将在下次登录后上传成就");
                        break;
                    case WebNetEvent.NET_NOT_AVAILABLE /* -101 */:
                        this.fX.R("网络没有链接！将在下次登录后上传成就");
                        break;
                    case 0:
                        this.fX.R("上传成就成功");
                        break;
                    default:
                        this.fX.R("网络拥堵！请稍候上传成就");
                        break;
                }
                break;
            case 7:
                int i2 = message.arg1;
                if (b.cu) {
                }
                switch (i2) {
                    case WebNetEvent.NOT_LOGIN /* -102 */:
                    case -2:
                        this.fX.R("您没有登录！将在下次登录后获取成就");
                        break;
                    case WebNetEvent.NET_NOT_AVAILABLE /* -101 */:
                        this.fX.R("网络没有链接！将在下次登录后获取成就");
                        break;
                    case 0:
                        this.fX.R("获取成就成功");
                        break;
                    default:
                        this.fX.R("网络拥堵！请稍候获取成就");
                        break;
                }
                break;
            case Interpreter.ERR_ARR_OUT /* 9 */:
                int i3 = message.arg1;
                if (b.cu) {
                }
                switch (i3) {
                    case WebNetEvent.NOT_LOGIN /* -102 */:
                    case -2:
                        this.fX.R("您没有登录！将在下次登录后上传数据");
                        break;
                    case WebNetEvent.NET_NOT_AVAILABLE /* -101 */:
                        this.fX.R("网络没有链接！将在下次登录后上传数据");
                        break;
                    case 0:
                        this.fX.R("上传数据成功");
                        break;
                    default:
                        this.fX.R("网络拥堵！请稍候上传数据");
                        break;
                }
                break;
            case 11:
                int i4 = message.arg1;
                if (b.cu) {
                }
                switch (i4) {
                    case WebNetEvent.NOT_LOGIN /* -102 */:
                    case -2:
                        this.fX.R("您没有登录！将在下次登录后获取数据");
                        break;
                    case WebNetEvent.NET_NOT_AVAILABLE /* -101 */:
                        this.fX.R("网络没有链接！将在下次登录后获取数据");
                        break;
                    case 0:
                        this.fX.R("获取数据成功");
                        break;
                    default:
                        this.fX.R("网络拥堵！请稍候获取数据");
                        break;
                }
                break;
            case 12:
            case Graphics.TOP /* 16 */:
                this.fX.a("QQ游戏中心", "正在进入游戏中心，请稍候。", null, null, null);
                break;
            case 13:
                this.fX.bo();
                if (b.cu) {
                }
                switch (message.arg1) {
                    case WebNetEvent.NET_NOT_AVAILABLE /* -101 */:
                        this.fX.R("没有网络链接，您必须联网进行支付！");
                        break;
                    case 0:
                        this.fX.R("支付成功");
                        break;
                    default:
                        this.fX.R("破解成功，爱吾游戏提供破解，更多游戏访问25az.com");
                        break;
                }
                break;
            case 14:
                this.fX.a("QQ游戏中心", "正在发送付费短信...", null, null, null);
                break;
            case 15:
                this.fX.bo();
                if (b.cu) {
                    b.bV.SendSMSCB(message.arg1, (String) message.obj);
                }
                switch (message.arg1) {
                    case WebNetEvent.SendSMS_Event_OK /* 1040 */:
                        this.fX.R("支付成功");
                        break;
                    case WebNetEvent.SendSMS_Event_Error /* 1041 */:
                    default:
                        this.fX.R("破解成功，爱吾游戏提供破解，更多游戏访问25az.com");
                        break;
                    case WebNetEvent.SendSMS_Event_Generic_Failure /* 1042 */:
                        this.fX.R("手机信号不稳定，请稍后再试！");
                        break;
                    case WebNetEvent.SendSMS_Event_Radio_Off /* 1043 */:
                        this.fX.R("手机网络没有链接！");
                        break;
                    case WebNetEvent.SendSMS_Event_NULL_PDU /* 1044 */:
                        this.fX.R("短信格式错误！");
                        break;
                }
                break;
            case Graphics.HT /* 17 */:
                this.fX.bo();
                switch (message.arg1) {
                    case WebNetEvent.NET_NOT_AVAILABLE /* -101 */:
                        this.fX.R("没有网络链接，您只有链接网络后才能访问游戏中心！");
                        break;
                    default:
                        this.fX.R("连接游戏中心失败");
                        break;
                }
                break;
            case 18:
                this.fX.R((String) message.obj);
                break;
            case TimeUtils.HUNDRED_DAY_FIELD_LEN /* 19 */:
                this.fX.bo();
                if (b.cu) {
                    b.bV.SyncMicBlogCB(message.arg1, (String) message.obj);
                }
                switch (message.arg1) {
                    case WebNetEvent.SyncMicBlog_OK /* 1070 */:
                        this.fX.R("同步微博成功");
                        break;
                    case WebNetEvent.SyncMicBlog_SERVER_RET_IMAGE_ERR /* 1071 */:
                        this.fX.R("无法识别的图片格式");
                        break;
                    case WebNetEvent.SyncMicBlog_NOT_LOGIN /* 1072 */:
                        this.fX.R("您还没有登录，请登录后同步微博");
                        break;
                    case WebNetEvent.SyncMicBlog_NOT_ID /* 1073 */:
                        this.fX.R("您还没有开通微博");
                        break;
                    default:
                        this.fX.R("同步微博失败");
                        break;
                }
                break;
        }
    }
}
