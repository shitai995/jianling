package com.tencent.webnet;

import android.os.Environment;
import android.support.v4.view.accessibility.AccessibilityEventCompat;
import java.io.BufferedInputStream;
import java.io.File;
import java.io.IOException;
import java.io.RandomAccessFile;
import java.io.Serializable;
import java.net.HttpURLConnection;
import java.net.URL;

/* JADX INFO: loaded from: classes.dex */
class k extends Thread implements Serializable {
    protected static k eg = null;
    private static final long serialVersionUID = 1;
    protected int eh = 17;
    protected int ei = -1;
    protected String ej = null;
    protected String ek = null;
    protected long el = 0;
    protected long em = 0;
    private boolean en = false;
    private boolean eo = false;

    k() {
    }

    protected static k aU() {
        if (eg == null) {
            k kVar = (k) d.C("MyDownLoadConfig");
            eg = kVar;
            if (kVar == null) {
                eg = new k();
            }
            if (eg == null) {
                if (b.cB) {
                    l.a("创建更新数据文件失败是否忘记申明WRITE_EXTERNAL_STORAGE权限!", null);
                } else {
                    l.a("creat html file error, uses permission WRITE_EXTERNAL_STORAGE", null);
                }
            } else {
                eg.setName("UpdateDownload");
            }
        }
        return eg;
    }

    protected boolean aV() {
        if (this.ei <= this.eh) {
            return false;
        }
        if (this.em >= this.el && this.em > 0 && this.el > 0) {
            if (Environment.getExternalStorageState().equals("mounted")) {
                String str = String.valueOf(Environment.getExternalStorageDirectory().getPath()) + "/Tencent/GameCenter/Update/html";
                aq.W(str);
                String str2 = String.valueOf(str) + this.ei + ".zip";
                if (aq.V(str2)) {
                    aq.Z(String.valueOf(Environment.getExternalStorageDirectory().getPath()) + "/Tencent/GameCenter/Update/html");
                    aq.a(this.ek, str2, true);
                }
            }
            aq.a(b.ax(), this.ek, b.ax().getFilesDir() + "/htmltemp/");
            aq.Y(this.ek);
            synchronized (this) {
                this.eo = true;
                this.ej = null;
                this.el = 0L;
                this.em = 0L;
                this.ek = null;
                this.eh = this.ei;
                this.ei = -1;
            }
            aY();
            return true;
        }
        if (Environment.getExternalStorageState().equals("mounted") && aq.V(String.valueOf(Environment.getExternalStorageDirectory().getPath()) + "/Tencent/GameCenter/Update/html" + this.ei + ".zip") && aq.a(String.valueOf(Environment.getExternalStorageDirectory().getPath()) + "/Tencent/GameCenter/Update/html" + this.ei + ".zip", this.ek, true)) {
            aq.a(b.ax(), this.ek, b.ax().getFilesDir() + "/htmltemp/");
            aq.Y(this.ek);
            synchronized (this) {
                this.eo = true;
                this.ej = null;
                this.el = 0L;
                this.em = 0L;
                this.ek = null;
                this.eh = this.ei;
                this.ei = -1;
            }
            aY();
            return true;
        }
        if (this.en) {
            return true;
        }
        this.en = true;
        start();
        return true;
    }

    protected boolean a(String str, int i) {
        if (i <= this.eh || str == null || str.length() <= 0) {
            return false;
        }
        if (str != null && i > this.ei) {
            if (this.en) {
                this.en = false;
                try {
                    stop();
                } catch (Exception e) {
                }
                while (eg != null) {
                    b.ay();
                }
                eg = this;
            }
            if (Environment.getExternalStorageState().equals("mounted")) {
                if (this.ej != null && this.ek != null) {
                    aq.Y(this.ek);
                }
                String str2 = String.valueOf(Environment.getExternalStorageDirectory().getPath()) + "/Tencent/GameCenter/Update/html" + i + ".zip";
                if (aq.V(str2)) {
                    String str3 = b.ax().getFilesDir() + str.substring(str.lastIndexOf("/"));
                    if (aq.a(str2, str3, true)) {
                        aq.a(b.ax(), str3, b.ax().getFilesDir() + "/htmltemp/");
                        aq.Y(str3);
                        synchronized (this) {
                            this.eo = true;
                            this.ej = null;
                            this.el = 0L;
                            this.em = 0L;
                            this.ek = null;
                            this.eh = i;
                            this.ei = -1;
                        }
                        aY();
                        return true;
                    }
                }
            }
            long jL = L(str);
            if (jL <= 0) {
                return false;
            }
            if (this.ej != null && this.ek != null) {
                aq.Y(this.ek);
            }
            synchronized (this) {
                this.eo = false;
                this.ej = str;
                this.el = jL;
                this.em = 0L;
                this.ek = str.substring(str.lastIndexOf("/"));
                this.ek = b.ax().getFilesDir() + this.ek;
                this.ei = i;
            }
            aY();
        }
        if (this.en) {
            return true;
        }
        this.en = true;
        start();
        return true;
    }

    protected boolean aW() {
        if (!this.eo) {
            return false;
        }
        aq.d(b.ax().getFilesDir() + "/html/", b.ax().getFilesDir() + "/temp");
        aq.d(b.ax().getFilesDir() + "/htmltemp/", b.ax().getFilesDir() + "/html/");
        aq.Y(b.ax().getFilesDir() + "/temp");
        this.eo = false;
        aY();
        b.bZ.ShowMessage("QQ游戏平台更新完成");
        return true;
    }

    private HttpURLConnection K(String str) {
        String strSubstring;
        String strSubstring2;
        try {
            if (h.aP()) {
                int length = "http://".length();
                int iIndexOf = str.indexOf(47, length);
                if (iIndexOf < 0) {
                    strSubstring = str.substring(length);
                    strSubstring2 = "";
                } else {
                    strSubstring = str.substring(length, iIndexOf);
                    strSubstring2 = str.substring(iIndexOf);
                }
                HttpURLConnection httpURLConnection = (HttpURLConnection) new URL("http://" + h.aQ() + ":" + h.aR() + strSubstring2).openConnection();
                httpURLConnection.setRequestProperty("X-Online-Host", strSubstring);
                return httpURLConnection;
            }
            return (HttpURLConnection) new URL(str).openConnection();
        } catch (IOException e) {
            if (b.cB) {
                l.a("网络连接异常是否忘加INTERNET权限 ", e);
            } else {
                l.a("update html connect error, uses permission INTERNET", e);
            }
            return null;
        }
    }

    private boolean a(HttpURLConnection httpURLConnection) {
        if (httpURLConnection != null) {
            try {
                httpURLConnection.disconnect();
            } catch (Exception e) {
                if (b.cB) {
                    l.a("关闭网络失败 ", e);
                } else {
                    l.a("close html net error", e);
                }
                return false;
            }
        }
        return true;
    }

    private long L(String str) {
        HttpURLConnection httpURLConnectionK = K(str);
        long contentLength = httpURLConnectionK.getContentLength();
        a(httpURLConnectionK);
        return contentLength;
    }

    private BufferedInputStream b(HttpURLConnection httpURLConnection) {
        httpURLConnection.setDoInput(true);
        httpURLConnection.setAllowUserInteraction(true);
        httpURLConnection.setRequestProperty("Range", "bytes=" + this.em + "-" + this.el);
        try {
            httpURLConnection.connect();
            return new BufferedInputStream(httpURLConnection.getInputStream(), 8192);
        } catch (Exception e) {
            if (b.cB) {
                l.a("设置断点续传连接异常是否忘加INTERNET权限", e);
            } else {
                l.a("set Breakpoints error, uses permission INTERNET", e);
            }
            return null;
        }
    }

    protected void aX() {
        this.en = false;
    }

    private void aY() {
        synchronized (this) {
            d.a("MyDownLoadConfig", this);
        }
    }

    /* JADX WARN: Code duplicated, block: B:11:0x0020 A[Catch: Exception -> 0x0031, TryCatch #5 {Exception -> 0x0031, blocks: (B:9:0x001b, B:11:0x0020, B:13:0x0025), top: B:117:0x001b }] */
    /* JADX WARN: Code duplicated, block: B:13:0x0025 A[Catch: Exception -> 0x0031, TRY_LEAVE, TryCatch #5 {Exception -> 0x0031, blocks: (B:9:0x001b, B:11:0x0020, B:13:0x0025), top: B:117:0x001b }] */
    /* JADX WARN: Code duplicated, block: B:32:0x006e A[Catch: Exception -> 0x0159, TryCatch #9 {Exception -> 0x0159, blocks: (B:30:0x0069, B:32:0x006e, B:34:0x0073), top: B:119:0x0069 }] */
    /* JADX WARN: Code duplicated, block: B:34:0x0073 A[Catch: Exception -> 0x0159, TRY_LEAVE, TryCatch #9 {Exception -> 0x0159, blocks: (B:30:0x0069, B:32:0x006e, B:34:0x0073), top: B:119:0x0069 }] */
    /* JADX WARN: Code duplicated, block: B:47:0x00ba A[Catch: Exception -> 0x00cc, TryCatch #0 {Exception -> 0x00cc, blocks: (B:45:0x00b5, B:47:0x00ba, B:49:0x00bf), top: B:111:0x00b5 }] */
    /* JADX WARN: Code duplicated, block: B:49:0x00bf A[Catch: Exception -> 0x00cc, TRY_LEAVE, TryCatch #0 {Exception -> 0x00cc, blocks: (B:45:0x00b5, B:47:0x00ba, B:49:0x00bf), top: B:111:0x00b5 }] */
    /* JADX WARN: Code duplicated, block: B:68:0x0104 A[Catch: Exception -> 0x0137, TryCatch #2 {Exception -> 0x0137, blocks: (B:66:0x00ff, B:68:0x0104, B:70:0x0109), top: B:113:0x00ff }] */
    /* JADX WARN: Code duplicated, block: B:70:0x0109 A[Catch: Exception -> 0x0137, TRY_LEAVE, TryCatch #2 {Exception -> 0x0137, blocks: (B:66:0x00ff, B:68:0x0104, B:70:0x0109), top: B:113:0x00ff }] */
    /* JADX WARN: Code duplicated, block: B:79:0x0126 A[Catch: Exception -> 0x0148, TryCatch #10 {Exception -> 0x0148, blocks: (B:77:0x0121, B:79:0x0126, B:81:0x012b), top: B:121:0x0121 }] */
    /* JADX WARN: Code duplicated, block: B:81:0x012b A[Catch: Exception -> 0x0148, TRY_LEAVE, TryCatch #10 {Exception -> 0x0148, blocks: (B:77:0x0121, B:79:0x0126, B:81:0x012b), top: B:121:0x0121 }] */
    @Override // java.lang.Thread, java.lang.Runnable
    public void run() throws Throwable {
        RandomAccessFile randomAccessFile;
        HttpURLConnection httpURLConnectionK;
        BufferedInputStream bufferedInputStreamB;
        RandomAccessFile randomAccessFile2 = null;
        byte[] bArr = new byte[AccessibilityEventCompat.TYPE_TOUCH_EXPLORATION_GESTURE_END];
        if (!this.en) {
            return;
        }
        try {
            httpURLConnectionK = K(this.ej);
            try {
                bufferedInputStreamB = b(httpURLConnectionK);
                if (bufferedInputStreamB == null) {
                    this.en = false;
                    if (bufferedInputStreamB != null) {
                        try {
                            bufferedInputStreamB.close();
                            if (httpURLConnectionK != null) {
                                a(httpURLConnectionK);
                            }
                            if (0 != 0) {
                                randomAccessFile2.close();
                            }
                        } catch (Exception e) {
                            if (b.cB) {
                                l.a("关闭更新系统资源失败，下次启动继续下载", e);
                            } else {
                                l.a("html update close http error ", e);
                            }
                        }
                    } else {
                        if (httpURLConnectionK != null) {
                            a(httpURLConnectionK);
                        }
                        if (0 != 0) {
                            randomAccessFile2.close();
                        }
                    }
                    aY();
                    eg = null;
                    System.gc();
                    return;
                }
                try {
                    randomAccessFile = new RandomAccessFile(new File(this.ek), "rw");
                    try {
                        try {
                            randomAccessFile.seek(this.em);
                            long jCurrentTimeMillis = System.currentTimeMillis();
                            while (this.en && this.em < this.el) {
                                int i = bufferedInputStreamB.read(bArr, 0, bArr.length);
                                if (i == -1) {
                                    httpURLConnectionK = K(this.ej);
                                    bufferedInputStreamB = b(httpURLConnectionK);
                                } else {
                                    randomAccessFile.write(bArr, 0, i);
                                    this.em += (long) i;
                                    if (this.em >= this.el) {
                                        randomAccessFile.close();
                                        bufferedInputStreamB.close();
                                        a(httpURLConnectionK);
                                        aV();
                                        this.en = false;
                                        if (bufferedInputStreamB != null) {
                                            try {
                                                bufferedInputStreamB.close();
                                                if (httpURLConnectionK != null) {
                                                    a(httpURLConnectionK);
                                                }
                                                if (randomAccessFile != null) {
                                                    randomAccessFile.close();
                                                }
                                            } catch (Exception e2) {
                                                if (b.cB) {
                                                    l.a("关闭更新系统资源失败，下次启动继续下载", e2);
                                                } else {
                                                    l.a("html update close http error ", e2);
                                                }
                                            }
                                        } else {
                                            if (httpURLConnectionK != null) {
                                                a(httpURLConnectionK);
                                            }
                                            if (randomAccessFile != null) {
                                                randomAccessFile.close();
                                            }
                                        }
                                        aY();
                                        eg = null;
                                        System.gc();
                                        return;
                                    }
                                    if (System.currentTimeMillis() - jCurrentTimeMillis > 1000) {
                                        jCurrentTimeMillis = System.currentTimeMillis();
                                        aY();
                                    }
                                }
                            }
                            this.en = false;
                            if (bufferedInputStreamB != null) {
                                try {
                                    bufferedInputStreamB.close();
                                    if (httpURLConnectionK != null) {
                                        a(httpURLConnectionK);
                                    }
                                    if (randomAccessFile != null) {
                                        randomAccessFile.close();
                                    }
                                } catch (Exception e3) {
                                    if (b.cB) {
                                        l.a("关闭更新系统资源失败，下次启动继续下载", e3);
                                    } else {
                                        l.a("html update close http error ", e3);
                                    }
                                }
                            } else {
                                if (httpURLConnectionK != null) {
                                    a(httpURLConnectionK);
                                }
                                if (randomAccessFile != null) {
                                    randomAccessFile.close();
                                }
                            }
                            aY();
                            eg = null;
                            System.gc();
                        } catch (Exception e4) {
                            e = e4;
                            if (b.cB) {
                                l.a("html下载更新遇到异常，下次启动继续下载", e);
                            } else {
                                l.a("update html error, Next time continue", e);
                            }
                            this.en = false;
                            if (bufferedInputStreamB != null) {
                                try {
                                    bufferedInputStreamB.close();
                                    if (httpURLConnectionK != null) {
                                        a(httpURLConnectionK);
                                    }
                                    if (randomAccessFile != null) {
                                        randomAccessFile.close();
                                    }
                                } catch (Exception e5) {
                                    if (b.cB) {
                                        l.a("关闭更新系统资源失败，下次启动继续下载", e5);
                                    } else {
                                        l.a("html update close http error ", e5);
                                    }
                                    aY();
                                    eg = null;
                                    System.gc();
                                }
                            } else {
                                if (httpURLConnectionK != null) {
                                    a(httpURLConnectionK);
                                }
                                if (randomAccessFile != null) {
                                    randomAccessFile.close();
                                }
                            }
                            aY();
                            eg = null;
                            System.gc();
                        }
                    } catch (Throwable th) {
                        th = th;
                        this.en = false;
                        if (bufferedInputStreamB != null) {
                            try {
                                bufferedInputStreamB.close();
                                if (httpURLConnectionK != null) {
                                    a(httpURLConnectionK);
                                }
                                if (randomAccessFile != null) {
                                    randomAccessFile.close();
                                }
                            } catch (Exception e6) {
                                if (b.cB) {
                                    l.a("关闭更新系统资源失败，下次启动继续下载", e6);
                                } else {
                                    l.a("html update close http error ", e6);
                                }
                                aY();
                                eg = null;
                                System.gc();
                                throw th;
                            }
                        } else {
                            if (httpURLConnectionK != null) {
                                a(httpURLConnectionK);
                            }
                            if (randomAccessFile != null) {
                                randomAccessFile.close();
                            }
                        }
                        aY();
                        eg = null;
                        System.gc();
                        throw th;
                    }
                } catch (Exception e7) {
                    e = e7;
                    randomAccessFile = null;
                } catch (Throwable th2) {
                    th = th2;
                    randomAccessFile = null;
                }
            } catch (Exception e8) {
                e = e8;
                randomAccessFile = null;
                bufferedInputStreamB = null;
            } catch (Throwable th3) {
                th = th3;
                randomAccessFile = null;
                bufferedInputStreamB = null;
            }
        } catch (Exception e9) {
            e = e9;
            randomAccessFile = null;
            httpURLConnectionK = null;
            bufferedInputStreamB = null;
        } catch (Throwable th4) {
            th = th4;
            randomAccessFile = null;
            httpURLConnectionK = null;
            bufferedInputStreamB = null;
        }
    }
}
