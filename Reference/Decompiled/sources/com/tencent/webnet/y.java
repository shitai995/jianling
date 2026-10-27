package com.tencent.webnet;

import android.app.ProgressDialog;
import android.content.DialogInterface;
import android.os.Handler;
import android.os.Looper;
import android.os.Message;
import android.view.View;
import android.widget.Toast;

/* JADX INFO: loaded from: classes.dex */
class y {
    protected static final int fA = 13;
    protected static final int fB = 14;
    protected static final int fC = 15;
    protected static final int fD = 16;
    protected static final int fE = 17;
    protected static final int fF = 18;
    protected static final int fG = 19;
    private static final String fH = "QQ游戏中心";
    private static final String fI = "网络没有链接！将在下次登录后";
    private static final String fJ = "您没有登录！将在下次登录后";
    private static final String fK = "正在进入游戏中心，请稍候。";
    private static final String fL = "正在上传数据，请稍候。";
    private static final String fM = "正在获取数据，请稍候。";
    private static final String fN = "连接游戏中心";
    private static final String fO = "上传积分";
    private static final String fP = "获取积分";
    private static final String fQ = "上传成就";
    private static final String fR = "获取成就";
    private static final String fS = "上传数据";
    private static final String fT = "获取数据";
    private static final String fU = "支付";
    private static final String fV = "成功";
    private static final String fW = "网络拥堵！请稍候";
    protected static final int fn = 0;
    protected static final int fo = 1;
    protected static final int fp = 2;
    protected static final int fq = 3;
    protected static final int fr = 4;
    protected static final int fs = 5;
    protected static final int ft = 6;
    protected static final int fu = 7;
    protected static final int fv = 8;
    protected static final int fw = 9;
    protected static final int fx = 10;
    protected static final int fy = 11;
    protected static final int fz = 12;
    private Handler eC = new z(this, Looper.getMainLooper());
    private ProgressDialog fm;

    protected y() {
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void a(String str, String str2, String str3, String str4, DialogInterface.OnClickListener onClickListener) {
        this.fm = ProgressDialog.show(b.ax(), str, str2, true);
    }

    protected void bo() {
        if (this.fm != null) {
            this.fm.dismiss();
            this.fm = null;
        }
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void R(String str) {
        bo();
        Toast toastMakeText = Toast.makeText(b.ax(), str, 0);
        View view = toastMakeText.getView();
        view.getBackground().setAlpha(178);
        toastMakeText.setView(view);
        toastMakeText.show();
    }

    protected void a(int i, int i2, int i3, String str) {
        if (!b.cu) {
        }
        ab abVar = new ab(this);
        abVar.o = i3;
        abVar.eL = str;
        Message message = new Message();
        message.what = 3;
        message.arg1 = i;
        message.arg2 = i2;
        message.obj = abVar;
        this.eC.sendMessage(message);
    }

    protected void b(int i, int i2, int i3, String str) {
        if (!b.cu) {
        }
        aa aaVar = new aa(this);
        aaVar.fY = i3;
        aaVar.eL = str;
        Message message = new Message();
        message.what = 7;
        message.arg1 = i;
        message.arg2 = i2;
        message.obj = aaVar;
        this.eC.sendMessage(message);
    }

    protected void b(int i, int i2, String str) {
        if (!b.cu) {
        }
        Message message = new Message();
        message.what = 13;
        message.arg1 = i;
        message.arg2 = i2;
        message.obj = str;
        this.eC.sendMessage(message);
    }

    protected void d(int i, String str) {
        if (!b.cu) {
            b.bV.SendSMSCB(i, str);
        }
        Message message = new Message();
        message.what = fC;
        message.arg1 = i;
        message.obj = str;
        this.eC.sendMessage(message);
    }

    protected void e(int i, String str) {
        Message message = new Message();
        message.what = 9;
        message.arg1 = i;
        message.obj = str;
        this.eC.sendMessage(message);
        if (!b.cu) {
        }
    }

    protected void a(int i, String str, String str2) {
        Message message = new Message();
        message.what = 11;
        message.arg1 = i;
        message.obj = str;
        this.eC.sendMessage(message);
        if (!b.cu) {
        }
    }

    protected void f(int i, String str) {
        r rVar = new r();
        rVar.O(str);
        rVar.as = (short) i;
        a(rVar);
    }

    protected void a(r rVar) {
        Message message = new Message();
        message.what = 1;
        message.obj = rVar;
        this.eC.sendMessage(message);
        if (!b.cu) {
            b.bW.c(rVar);
        }
    }

    protected void g(int i, String str) {
        r rVar = new r();
        rVar.O(str);
        rVar.as = (short) i;
        b(rVar);
    }

    protected void b(r rVar) {
        Message message = new Message();
        message.what = 5;
        message.obj = rVar;
        this.eC.sendMessage(message);
        if (!b.cu) {
            b.bW.d(rVar);
        }
    }

    protected void q(int i) {
        Message message = new Message();
        message.what = 17;
        message.arg1 = i;
        this.eC.sendMessage(message);
    }

    protected void h(int i, String str) {
        if (!b.cu) {
            b.bV.SyncMicBlogCB(i, str);
        }
        Message message = new Message();
        message.what = 19;
        message.arg1 = i;
        message.obj = str;
        this.eC.sendMessage(message);
    }

    protected void r(int i) {
        Message message = new Message();
        message.what = i;
        this.eC.sendMessage(message);
    }

    protected void ShowMessage(String str) {
        Message message = new Message();
        message.what = fF;
        message.obj = str;
        this.eC.sendMessage(message);
    }
}
