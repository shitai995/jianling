package com.tencent.webnet;

import android.content.Context;
import android.database.Cursor;
import android.net.ConnectivityManager;
import android.net.NetworkInfo;
import android.net.Uri;
import android.support.v4.view.accessibility.AccessibilityEventCompat;
import java.io.InputStream;
import java.io.OutputStream;
import java.net.HttpURLConnection;
import java.net.SocketTimeoutException;
import java.net.URL;
import java.util.List;
import org.apache.http.HttpResponse;
import org.apache.http.client.HttpClient;
import org.apache.http.client.entity.UrlEncodedFormEntity;
import org.apache.http.client.methods.HttpPost;
import org.apache.http.impl.client.DefaultHttpClient;

/* JADX INFO: loaded from: classes.dex */
class h {
    private static final int da = 10000;
    protected static final int dc = 0;
    protected static final int dd = 1;
    protected static final int de = 2;
    protected static final int df = 3;
    protected static final int dg = 4;
    protected static final int dh = 5;
    protected static final int di = 6;
    protected static final int dj = 7;
    protected static final int dk = 8;
    protected static final int dl = 9;
    protected static final int dm = 10;
    protected static final int dn = 11;
    private HttpClient dp;
    private String ds = "";
    private URL dt = null;
    private HttpURLConnection du = null;
    private static Uri db = Uri.parse("content://telephony/carriers/preferapn");

    /* JADX INFO: renamed from: do, reason: not valid java name */
    private static boolean f0do = false;
    private static String dq = null;
    private static String dr = null;

    protected h() {
        this.dp = null;
        this.dp = new DefaultHttpClient();
    }

    protected boolean F(String str) {
        String strSubstring;
        String strSubstring2;
        this.ds = str;
        try {
            if (f0do) {
                int length = "http://".length();
                int iIndexOf = this.ds.indexOf(47, length);
                if (iIndexOf < 0) {
                    strSubstring = this.ds.substring(length);
                    strSubstring2 = "";
                } else {
                    strSubstring = this.ds.substring(length, iIndexOf);
                    strSubstring2 = this.ds.substring(iIndexOf);
                }
                this.dt = new URL("http://" + dq + ":" + dr + strSubstring2);
                this.du = (HttpURLConnection) this.dt.openConnection();
                this.du.setRequestProperty("X-Online-Host", strSubstring);
            } else {
                this.dt = new URL(this.ds);
                this.du = (HttpURLConnection) this.dt.openConnection();
            }
            this.du.setConnectTimeout(da);
            this.du.setReadTimeout(da);
            this.du.setDoInput(true);
            this.du.setDoOutput(true);
            this.du.connect();
            return true;
        } catch (Exception e) {
            if (b.cB) {
                l.a("网络连接失败, 是否忘加INTERNET权限! ", e);
            } else {
                l.a("net connect error, uses permission INTERNET", e);
            }
            return false;
        }
    }

    protected boolean l(byte[] bArr) {
        try {
            OutputStream outputStream = this.du.getOutputStream();
            outputStream.write(bArr);
            outputStream.flush();
            outputStream.close();
            return true;
        } catch (Exception e) {
            l.a("HttpNet Send" + this.ds + "false!", e);
            return false;
        }
    }

    protected byte[] aN() {
        byte[] bArr;
        try {
            InputStream inputStream = this.du.getInputStream();
            if (inputStream == null) {
                return null;
            }
            byte[] bArr2 = new byte[AccessibilityEventCompat.TYPE_TOUCH_EXPLORATION_GESTURE_END];
            int i = 0;
            while (true) {
                if (i >= bArr2.length) {
                    byte[] bArr3 = new byte[bArr2.length * 2];
                    System.arraycopy(bArr2, 0, bArr3, 0, bArr2.length);
                    bArr2 = bArr3;
                }
                int i2 = inputStream.read(bArr2, i, bArr2.length - i);
                if (-1 == i2) {
                    break;
                }
                i = i2 + i;
            }
            if (i == 0) {
                bArr = (byte[]) null;
            } else {
                bArr = new byte[i];
                System.arraycopy(bArr2, 0, bArr, 0, i);
            }
            inputStream.close();
            System.gc();
            return bArr;
        } catch (SocketTimeoutException e) {
            return null;
        } catch (Exception e2) {
            l.a("HttpNet Recv " + this.ds + " false! ", e2);
            return null;
        }
    }

    protected boolean aO() {
        if (this.du != null) {
            this.du.disconnect();
        }
        this.ds = "";
        this.dt = null;
        this.du = null;
        return true;
    }

    protected InputStream a(String str, List list) {
        HttpPost httpPost = new HttpPost(str);
        if (list != null) {
            try {
                httpPost.setEntity(new UrlEncodedFormEntity(list));
            } catch (Exception e) {
                if (b.cB) {
                    l.a("post数据 失败, 是否忘加INTERNET权限! ", e);
                } else {
                    l.a("post data error, uses permission INTERNET", e);
                }
            }
        }
        HttpResponse httpResponseExecute = this.dp.execute(httpPost);
        if (httpResponseExecute.getStatusLine().getStatusCode() == 200) {
            return httpResponseExecute.getEntity().getContent();
        }
        return null;
    }

    protected static void a(Context context) {
        int iB = b(context);
        if (iB == 2 || iB == 4 || iB == 6 || iB == 8 || iB == 10) {
            f0do = true;
        } else {
            f0do = false;
        }
        if (f0do) {
            Cursor cursorQuery = context.getContentResolver().query(db, null, null, null, null);
            cursorQuery.moveToFirst();
            if (cursorQuery.isAfterLast()) {
                dq = null;
                dr = null;
            } else {
                dq = cursorQuery.getString(cursorQuery.getColumnIndex("proxy"));
                dr = cursorQuery.getString(cursorQuery.getColumnIndex("port"));
            }
        }
    }

    protected static boolean aP() {
        return f0do;
    }

    protected static String aQ() {
        return dq;
    }

    protected static String aR() {
        return dr;
    }

    protected static int b(Context context) {
        try {
            NetworkInfo activeNetworkInfo = ((ConnectivityManager) context.getSystemService("connectivity")).getActiveNetworkInfo();
            if (activeNetworkInfo == null) {
                return 0;
            }
            if (activeNetworkInfo.getTypeName().toUpperCase().equals("WIFI")) {
                return 1;
            }
            String lowerCase = activeNetworkInfo.getExtraInfo().toLowerCase();
            if (lowerCase.startsWith("cmwap")) {
                return 2;
            }
            if (lowerCase.startsWith("cmnet") || lowerCase.startsWith("epc.tmobile.com")) {
                return 3;
            }
            if (lowerCase.startsWith("uniwap")) {
                return 4;
            }
            if (lowerCase.startsWith("uninet")) {
                return 5;
            }
            if (lowerCase.startsWith("wap")) {
                return 6;
            }
            if (lowerCase.startsWith("net")) {
                return 7;
            }
            if (lowerCase.startsWith("3gwap")) {
                return 10;
            }
            if (lowerCase.startsWith("3gnet")) {
                return 11;
            }
            if (lowerCase.startsWith("#777")) {
                Cursor cursorQuery = context.getContentResolver().query(db, null, null, null, null);
                cursorQuery.moveToFirst();
                if (cursorQuery.isAfterLast()) {
                    return 9;
                }
                String string = cursorQuery.getString(cursorQuery.getColumnIndex("proxy"));
                return (string == null || string.length() <= 0) ? 9 : 8;
            }
        } catch (Exception e) {
            if (b.cB) {
                l.a("获取网络类型 失败, 是否忘加INTERNET权限! ", e);
            } else {
                l.a("get net type error, uses permission INTERNET", e);
            }
        }
        return 0;
    }
}
