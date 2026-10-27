package com.tencent.webnet;

import android.location.LocationManager;
import android.os.Message;
import com.qq.WapGame.au;
import com.qq.WapGame.aw;
import com.qq.WapGame.ay;
import com.qq.WapGame.bc;
import com.qq.WapGame.bi;
import com.qq.WapGame.bk;
import com.qq.WapGame.bm;
import com.qq.WapGame.bo;
import com.qq.WapGame.bq;
import com.qq.WapGame.bs;
import com.qq.taf.jce.JceInputStream;
import java.util.ArrayList;
import java.util.Vector;

/* JADX INFO: loaded from: classes.dex */
class t extends Thread {
    protected static final int eN = 0;
    protected static final int eO = 1;
    protected static final int eP = 2;
    protected static final int eQ = 3;
    protected static final int eR = 4;
    protected static final int eS = 5;
    protected static final int eT = 7;
    protected static final int eU = 8;
    protected static final int eV = 9;
    protected static final int eW = 10;
    protected static final int eX = 11;
    protected static final int eY = 12;
    protected static final int eZ = 13;
    protected static final int fa = 15;
    protected static final int fb = 16;
    private h fc = null;
    private boolean fd = false;
    private Vector fe = new Vector();
    private u ff = null;

    protected t() {
        setName("TaskThread");
        if (b.ax() != null) {
            b.ce = new e((LocationManager) b.ax().getSystemService("location"));
            h.a(b.ax());
        }
    }

    /* JADX WARN: Can't fix incorrect switch cases order, some code will duplicate */
    @Override // java.lang.Thread, java.lang.Runnable
    public void run() {
        boolean z;
        boolean z2;
        boolean z3 = false;
        while (this.fd && b.bX != null) {
            if (WebNetInterface.bq()) {
                b.ay();
                z3 = true;
            } else {
                if (this.ff == null) {
                    b.ay();
                    z2 = z3;
                } else {
                    if (z3) {
                        h.a(b.ax());
                        z = false;
                    } else {
                        z = z3;
                    }
                    switch (this.ff.fg) {
                        case 0:
                            b.ay();
                            z2 = z;
                            break;
                        case 1:
                            this.fd = false;
                            z2 = z;
                            break;
                        case 2:
                            a(((Integer) this.ff.p(0)).intValue(), ((Integer) this.ff.p(1)).intValue(), (String) this.ff.p(2));
                            a(o(9));
                            z2 = z;
                            break;
                        case 3:
                            a(((Integer) this.ff.p(0)).intValue(), (String) this.ff.p(1));
                            a(o(9));
                            z2 = z;
                            break;
                        case 4:
                            c(((Integer) this.ff.p(0)).intValue(), (String) this.ff.p(1));
                            a(o(9));
                            z2 = z;
                            break;
                        case 5:
                            b(((Integer) this.ff.p(0)).intValue(), (String) this.ff.p(1));
                            a(o(9));
                            z2 = z;
                            break;
                        case 6:
                        case 14:
                        default:
                            z2 = z;
                            break;
                        case 7:
                            b((String) this.ff.p(0), (String) this.ff.p(1));
                            a(o(9));
                            z2 = z;
                            break;
                        case 8:
                            P((String) this.ff.p(0));
                            a(o(9));
                            z2 = z;
                            break;
                        case 9:
                            bi();
                            z2 = z;
                            break;
                        case 10:
                            bj();
                            a(o(9));
                            z2 = z;
                            break;
                        case 11:
                            bk();
                            a(o(9));
                            z2 = z;
                            break;
                        case eY /* 12 */:
                            bl();
                            a(o(9));
                            z2 = z;
                            break;
                        case 13:
                            a((String) this.ff.p(0), (byte[]) this.ff.p(1), (String) this.ff.p(2));
                            z2 = z;
                            break;
                        case fa /* 15 */:
                            bm();
                            a(o(9));
                            z2 = z;
                            break;
                        case 16:
                            bn();
                            a(o(9));
                            z2 = z;
                            break;
                    }
                }
                this.ff = null;
                synchronized (this) {
                    if (!this.fe.isEmpty()) {
                        this.ff = (u) this.fe.get(0);
                        this.fe.remove(0);
                    }
                }
                z3 = z2;
            }
        }
        if (b.ce != null) {
            b.ce.Destroy();
        }
        b.ce = null;
        System.gc();
    }

    protected void aX() {
        if (this.fd) {
            a(o(1));
            o.aX();
        }
    }

    protected void a(u uVar) {
        synchronized (this) {
            if (!this.fe.add(uVar)) {
                if (b.cB) {
                    l.a("加入任务队列失败!", null);
                } else {
                    l.a("add task error", null);
                }
            }
            if (!this.fd) {
                this.fd = true;
                start();
            }
        }
    }

    private r a(v vVar) {
        Exception exc;
        bm bmVar;
        bm bmVar2;
        r rVar = new r();
        ArrayList arrayList = new ArrayList();
        if (vVar.fj != null) {
            for (int i = 0; i < vVar.fj.size(); i++) {
                x xVar = (x) vVar.fj.get(i);
                arrayList.add(new com.qq.WapGame.g(xVar.n, xVar.o, xVar.f));
            }
        }
        ArrayList arrayList2 = new ArrayList();
        if (vVar.fk != null) {
            for (int i2 = 0; i2 < vVar.fk.size(); i2++) {
                w wVar = (w) vVar.fk.get(i2);
                arrayList2.add(new com.qq.WapGame.c(wVar.e, wVar.f));
            }
        }
        com.qq.WapGame.i iVar = new com.qq.WapGame.i();
        if (b.ce != null) {
            try {
                iVar.d(Double.toString(e.aD()));
                iVar.e(Double.toString(e.aC()));
                iVar.e((short) e.aE());
                iVar.f((short) e.aF());
                iVar.g((short) e.aH());
                iVar.h((short) e.aG());
                iVar.f(Integer.toString(e.aJ()));
                iVar.g(e.aK());
            } catch (Exception e) {
                if (b.cB) {
                    l.a("获取GPS信息失败 ", e);
                } else {
                    l.a("task get GPS error", null);
                }
            }
        }
        m mVarAZ = m.aZ();
        try {
            byte[] bArrAN = (byte[]) null;
            bk bkVar = new bk();
            String str = (mVarAZ == null || mVarAZ.M("uin") == null) ? "" : (String) mVarAZ.M("uin");
            bkVar.h(str);
            bkVar.c(b.cb);
            bkVar.d(b.cc);
            bkVar.j(b.cd);
            bkVar.i("V2.0");
            bkVar.e(arrayList);
            bkVar.f(arrayList2);
            bkVar.u(vVar.aY == null ? "" : vVar.aY);
            bkVar.a(iVar);
            this.fc = b.B("updateData");
            if (this.fc == null) {
                rVar.as = (short) -104;
            } else if (!this.fc.l(bkVar.toByteArray())) {
                rVar.as = (short) -105;
            } else {
                bArrAN = this.fc.aN();
                if (bArrAN == null) {
                    rVar.as = (short) -106;
                }
            }
            if (bArrAN != null) {
                bmVar2 = new bm();
                try {
                    bmVar2.readFrom(new JceInputStream(bArrAN));
                } catch (Exception e2) {
                    bmVar = bmVar2;
                    exc = e2;
                    if (b.cB) {
                        l.a("上传积分成就信息失败是否gamekey不合法或没有添加INTERNET权限 ", exc);
                    } else {
                        l.a("task update score error, uses permission INTERNET", exc);
                    }
                    rVar.as = (short) -107;
                    bmVar2 = bmVar;
                }
            } else {
                bmVar2 = null;
            }
        } catch (Exception e3) {
            exc = e3;
            bmVar = null;
        }
        if (rVar.as == 0 && bmVar2 != null) {
            rVar.as = bmVar2.S();
            ArrayList arrayListAf = bmVar2.af();
            if (arrayListAf != null) {
                for (int i3 = 0; i3 < arrayListAf.size(); i3++) {
                    com.qq.WapGame.e eVar = (com.qq.WapGame.e) arrayListAf.get(i3);
                    rVar.a(eVar.g(), eVar.h(), eVar.i());
                }
            }
            ArrayList arrayListAg = bmVar2.ag();
            if (arrayListAg != null) {
                for (int i4 = 0; i4 < arrayListAg.size(); i4++) {
                    com.qq.WapGame.e eVar2 = (com.qq.WapGame.e) arrayListAg.get(i4);
                    rVar.b(eVar2.g(), eVar2.h(), eVar2.i());
                }
            }
            rVar.eK = bmVar2.ah();
        }
        if (bmVar2 != null && bmVar2.S() != 0) {
            n(rVar.as);
        }
        if (this.fc != null) {
            this.fc.aO();
        }
        this.fc = null;
        return rVar;
    }

    private int a(int i, int i2, String str) {
        v vVar = new v();
        vVar.a((short) i, i2);
        r rVarA = a(vVar);
        rVarA.O(str);
        if (rVarA.as != 0) {
            b.bY.a(i, i2);
        } else {
            m.aZ().b("isUpdate", "true");
        }
        b.bZ.a(rVarA);
        return rVarA.as;
    }

    private int a(int i, String str) {
        b.bZ.a(WebNetEvent.FUNCTION_NA, i, 0, str);
        return 0;
    }

    private int b(int i, String str) {
        b.bZ.b(WebNetEvent.FUNCTION_NA, i, 0, str);
        return 0;
    }

    private int c(int i, String str) {
        v vVar = new v();
        vVar.k((short) i);
        r rVarA = a(vVar);
        rVarA.O(str);
        if (rVarA.as != 0) {
            b.bY.m(i);
        } else {
            m.aZ().b("isUpdate", "true");
        }
        b.bZ.b(rVarA);
        return rVarA.as;
    }

    private int b(String str, String str2) {
        v vVar = new v();
        vVar.Q(str);
        r rVarA = a(vVar);
        rVarA.O(str2);
        if (rVarA.as != 0) {
            b.bY.N(str);
        }
        b.bZ.e(rVarA.as, str2);
        return rVarA.as;
    }

    private int P(String str) {
        com.qq.WapGame.q qVar;
        int i;
        com.qq.WapGame.q qVar2;
        int status;
        String value;
        int i2 = 0;
        try {
            byte[] bArrAN = (byte[]) null;
            com.qq.WapGame.o oVar = new com.qq.WapGame.o();
            oVar.d(b.cc);
            oVar.c(b.cb);
            this.fc = b.B("updateData");
            if (this.fc == null) {
                i2 = WebNetEvent.Error_Type_Connect;
            } else if (!this.fc.l(oVar.toByteArray())) {
                i2 = WebNetEvent.Error_Type_Send;
            } else {
                bArrAN = this.fc.aN();
                if (bArrAN == null) {
                    i2 = WebNetEvent.Error_Type_Rec;
                }
            }
            if (bArrAN != null) {
                qVar = new com.qq.WapGame.q();
                try {
                    qVar.readFrom(new JceInputStream(bArrAN));
                    i = i2;
                    qVar2 = qVar;
                } catch (Exception e) {
                    e = e;
                    if (b.cB) {
                        l.a("GetData error ", e);
                    } else {
                        l.a("task GetDate error", e);
                    }
                    i = WebNetEvent.Error_Type_Sys;
                    qVar2 = qVar;
                }
            } else {
                i = i2;
                qVar2 = null;
            }
        } catch (Exception e2) {
            e = e2;
            qVar = null;
        }
        if (i != 0 || qVar2 == null) {
            status = i;
            value = null;
        } else {
            status = qVar2.getStatus();
            value = qVar2.getValue();
        }
        if (this.fc != null) {
            this.fc.aO();
        }
        this.fc = null;
        b.bZ.a(status, value, str);
        return status;
    }

    private int bi() {
        v vVarBd;
        if (b.bY == null || b.bY.M("sid") == null || (vVarBd = b.bY.bd()) == null) {
            return 0;
        }
        if (vVarBd.fj == null && vVarBd.fk == null && (vVarBd.aY == null || vVarBd.aY.equals(""))) {
            return 0;
        }
        r rVarA = a(vVarBd);
        rVarA.O("upload local data");
        if (rVarA.as == 0) {
            b.bY.be();
            m.aZ().b("isUpdate", "true");
        }
        return rVarA.as;
    }

    private void bj() {
        bs bsVar;
        if (o.eB == null) {
            new o();
        }
        k kVarAU = k.aU();
        m mVarAZ = m.aZ();
        try {
            byte[] bArrAN = (byte[]) null;
            bi biVar = new bi();
            String str = (mVarAZ == null || mVarAZ.M("uin") == null) ? "" : (String) mVarAZ.M("uin");
            biVar.h(str);
            biVar.c(b.cb);
            biVar.d(b.cc);
            biVar.i("V2.0");
            biVar.j(b.cd);
            biVar.k(b.cl != null ? b.cl : "");
            this.fc = b.B("update");
            if (this.fc != null && this.fc.l(biVar.toByteArray()) && (bArrAN = this.fc.aN()) == null) {
            }
            if (bArrAN != null) {
                bsVar = new bs();
                try {
                    bsVar.readFrom(new JceInputStream(bArrAN));
                } catch (Exception e) {
                    e = e;
                    if (b.cB) {
                        l.a("获取登录态失败是否gameky错误或没有申请INTERNET权限 ", e);
                    } else {
                        l.a("task get login state error, uses permission INTERNET", e);
                    }
                }
            } else {
                bsVar = null;
            }
        } catch (Exception e2) {
            e = e2;
            bsVar = null;
        }
        if (bsVar != null && bsVar.bq != null && !bsVar.bq.equals("")) {
            if ((mVarAZ.M("sid") == null || !((String) mVarAZ.M("sid")).equals(bsVar.bq)) && b.bX != null) {
                b.bX.a(b.bX.o(eY));
                b.bX.a(b.bX.o(9));
            }
            mVarAZ.b("sid", bsVar.bq);
            mVarAZ.b("uin", bsVar.z);
            mVarAZ.b("nickName", bsVar.br);
        } else if (bsVar != null && (bsVar.bq == null || bsVar.bq.equals(""))) {
            mVarAZ.b("sid", null);
            mVarAZ.b("uin", null);
        }
        if (bsVar != null && !bsVar.bt.equals("") && b.bZ != null) {
            b.bZ.ShowMessage(bsVar.bt);
        }
        if (bsVar != null && bsVar.C != null && bsVar.C.length() > 0) {
            int i = Integer.parseInt(bsVar.C);
            if (i > kVarAU.eh && i > kVarAU.ei) {
                kVarAU.a(bsVar.bp, i);
            } else {
                kVarAU.aV();
            }
        }
        if (bsVar != null && bsVar.status != 0) {
            n(bsVar.status);
        }
        if (this.fc != null) {
            this.fc.aO();
        }
        this.fc = null;
    }

    private void bk() {
        m mVarAZ = m.aZ();
        try {
            bc bcVar = new bc();
            String str = (mVarAZ == null || mVarAZ.M("uin") == null) ? "" : (String) mVarAZ.M("uin");
            bcVar.h(str);
            bcVar.c(b.cb);
            bcVar.d(b.cc);
            bcVar.i("V2.0");
            bcVar.j(b.cd);
            com.qq.WapGame.i iVar = new com.qq.WapGame.i();
            if (b.ce != null) {
                try {
                    iVar.d(Double.toString(e.aD()));
                    iVar.e(Double.toString(e.aC()));
                    iVar.e((short) e.aE());
                    iVar.f((short) e.aF());
                    iVar.g((short) e.aH());
                    iVar.h((short) e.aG());
                    iVar.f(Integer.toString(e.aJ()));
                    iVar.g(e.aK());
                } catch (Exception e) {
                    if (b.cB) {
                        l.a("UpdateGPS 获取gps错误 ", e);
                    } else {
                        l.a("task UpdateGPS error", e);
                    }
                }
                bcVar.a(iVar);
                this.fc = b.B("ReportPlayerGps");
                if (this.fc != null && this.fc.l(bcVar.toByteArray())) {
                    this.fc.aN();
                }
            } else {
                bcVar.a(iVar);
                this.fc = b.B("ReportPlayerGps");
                if (this.fc != null) {
                    this.fc.aN();
                }
            }
        } catch (Exception e2) {
            if (b.cB) {
                l.a("上传GPS信息失败是否gamekey错误或没有申明INTERNET权限 ", e2);
            } else {
                l.a("task UpdateGPS http error, uses permission INTERNET", e2);
            }
        }
        if (this.fc != null) {
            this.fc.aO();
        }
        this.fc = null;
    }

    private void bl() {
        m mVarAZ = m.aZ();
        try {
            ay ayVar = new ay();
            String str = (mVarAZ == null || mVarAZ.M("uin") == null) ? "" : (String) mVarAZ.M("uin");
            ayVar.h(str);
            ayVar.c(b.cb);
            ayVar.d(b.cc);
            ayVar.i("V2.0");
            ayVar.j(b.cd);
            ayVar.k(b.cl != null ? b.cl : "");
            this.fc = b.B("reportPlayerGame");
            if (this.fc != null && this.fc.l(ayVar.toByteArray())) {
                this.fc.aN();
            }
        } catch (Exception e) {
            if (b.cB) {
                l.a("报告使用次数错误是否没有申明INTERNET权限或gamekey错误 ", e);
            } else {
                l.a("task ReportPlay http error, uses permission INTERNET or gamekey error", e);
            }
        }
        if (this.fc != null) {
            this.fc.aO();
        }
        this.fc = null;
    }

    private void a(String str, byte[] bArr, String str2) {
        bq bqVar;
        m mVarAZ = m.aZ();
        try {
            byte[] bArrAN = (byte[]) null;
            bo boVar = new bo();
            String str3 = (mVarAZ == null || mVarAZ.M("uin") == null) ? "" : (String) mVarAZ.M("uin");
            boVar.h(str3);
            boVar.c(b.cb);
            boVar.d(b.cc);
            boVar.i("V2.0");
            boVar.j(b.cd);
            boVar.e(bArr);
            boVar.v("jpeg");
            boVar.l(str);
            this.fc = b.a("updateImg", str2);
            if (this.fc != null && this.fc.l(boVar.toByteArray()) && (bArrAN = this.fc.aN()) == null) {
            }
            if (bArrAN != null) {
                bqVar = new bq();
                try {
                    bqVar.readFrom(new JceInputStream(bArrAN));
                } catch (Exception e) {
                    e = e;
                    if (b.cB) {
                        l.a("同步微博失败是否没有申明INTERNET权限", e);
                    } else {
                        l.a("task SyncMicBlog error, uses permission INTERNET", e);
                    }
                }
            } else {
                bqVar = null;
            }
        } catch (Exception e2) {
            e = e2;
            bqVar = null;
        }
        if (bqVar != null) {
            switch (bqVar.status) {
                case -2:
                    b.bZ.h(WebNetEvent.SyncMicBlog_NOT_LOGIN, "您还没有登录，请登录后同步微博!");
                    break;
                case -1:
                case 1:
                case 2:
                default:
                    b.bZ.h(WebNetEvent.SyncMicBlog_Error, "同步微博失败！");
                    break;
                case 0:
                    b.bZ.h(WebNetEvent.SyncMicBlog_OK, "同步微博成功");
                    break;
                case 3:
                    b.bZ.h(WebNetEvent.SyncMicBlog_NOT_ID, "您还没有开通微博！");
                    break;
            }
        }
        if (this.fc != null) {
            this.fc.aO();
        }
        this.fc = null;
    }

    private void bm() {
        aw awVar;
        m mVarAZ = m.aZ();
        if (mVarAZ != null) {
            try {
                byte[] bArrAN = (byte[]) null;
                au auVar = new au();
                String str = (mVarAZ == null || mVarAZ.M("uin") == null) ? "" : (String) mVarAZ.M("uin");
                auVar.h(str);
                auVar.c(b.cb);
                auVar.d(b.cc);
                auVar.i("V2.0");
                auVar.j(b.cd);
                auVar.k(b.cl != null ? b.cl : "");
                this.fc = b.B("reportInstall");
                if (this.fc != null && this.fc.l(auVar.toByteArray())) {
                    bArrAN = this.fc.aN();
                }
                if (bArrAN != null) {
                    awVar = new aw();
                    try {
                        awVar.readFrom(new JceInputStream(bArrAN));
                    } catch (Exception e) {
                        e = e;
                        if (b.cB) {
                            l.a("报告安装失败是否没有申明INTERNET权限 ", e);
                        } else {
                            l.a("task HadInstall error, uses permission INTERNET", e);
                        }
                    }
                } else {
                    awVar = null;
                }
            } catch (Exception e2) {
                e = e2;
                awVar = null;
            }
            if (awVar != null) {
                switch (awVar.status) {
                    case 0:
                        mVarAZ.b(true);
                        break;
                }
            }
            if (this.fc != null) {
                this.fc.aO();
            }
            this.fc = null;
        }
    }

    private void bn() {
        m mVarAZ = m.aZ();
        if (mVarAZ != null) {
            try {
                byte[] bArrAN = (byte[]) null;
                com.qq.WapGame.s sVar = new com.qq.WapGame.s();
                String str = (mVarAZ == null || mVarAZ.M("uin") == null) ? "" : (String) mVarAZ.M("uin");
                sVar.h(str);
                sVar.c(b.cb);
                sVar.d(b.cc);
                sVar.i("V2.0");
                sVar.j(b.cd);
                sVar.k(b.cl != null ? b.cl : "");
                this.fc = b.B("getMsg");
                if (this.fc != null && this.fc.l(sVar.toByteArray())) {
                    bArrAN = this.fc.aN();
                }
                if (bArrAN != null) {
                    com.qq.WapGame.u uVar = new com.qq.WapGame.u();
                    uVar.readFrom(new JceInputStream(bArrAN));
                    if (uVar.status == 0) {
                        b.cn = ((Integer) uVar.T.get(com.qq.WapGame.a.a.value)).intValue();
                        if (b.cf != null && WebNetMain.eC != null) {
                            Message message = new Message();
                            message.what = 2;
                            WebNetMain.eC.sendMessage(message);
                        }
                    }
                } else {
                    b.cn = 0;
                    if (b.cf != null && WebNetMain.eC != null) {
                        Message message2 = new Message();
                        message2.what = 2;
                        WebNetMain.eC.sendMessage(message2);
                    }
                }
            } catch (Exception e) {
                if (b.cB) {
                    l.a("获取玩家数据失败是否gamekey失败或没有申明INTERNET权限 ", e);
                } else {
                    l.a("task GetMessage error, uses permission INTERNET", e);
                }
            }
            if (this.fc != null) {
                this.fc.aO();
            }
            this.fc = null;
        }
    }

    protected static void n(int i) {
        if (b.cB) {
            switch (i) {
                case -3:
                    l.a("GameKey非法或没有生效或初始化失败？或游戏没有正式提交？或cpid gameid错误", null);
                    break;
                case -2:
                    l.a("登录数据错误 sid非法，请重新登录", null);
                    break;
                case -1:
                    l.a("服务器返回-1错误 ", null);
                    break;
                default:
                    l.a("服务器返回未知错误 = " + i, null);
                    break;
            }
        }
        switch (i) {
            case -3:
                l.a("GameKey error or init error", null);
                break;
            case -2:
                l.a("login sid error ", null);
                break;
            case -1:
                l.a("task server return -1", null);
                break;
            default:
                l.a("task server return unknow error code = " + i, null);
                break;
        }
    }

    protected u o(int i) {
        u uVar = new u(this);
        uVar.fg = i;
        return uVar;
    }
}
