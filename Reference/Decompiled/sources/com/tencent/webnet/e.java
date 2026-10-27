package com.tencent.webnet;

import android.app.Activity;
import android.location.Location;
import android.location.LocationListener;
import android.location.LocationManager;
import android.net.wifi.WifiManager;
import android.telephony.TelephonyManager;
import android.telephony.gsm.GsmCellLocation;

/* JADX INFO: loaded from: classes.dex */
class e {
    private static LocationManager cN;
    private static double cO;
    private static double cP;
    private static boolean cQ;
    private static int cX;
    private static String cY;
    private boolean cZ = false;
    protected static final LocationListener cM = new f();
    private static String cR = null;
    private static int cS = -1;
    private static int cT = -1;
    private static int cU = -1;
    private static int cV = -1;
    private static String cW = null;

    protected e(LocationManager locationManager) {
        cN = locationManager;
        if (cN != null) {
            aL();
        }
        aM();
    }

    protected void Destroy() {
        cN.removeUpdates(cM);
    }

    protected void az() {
        if (cN != null) {
            this.cZ = true;
            cN.removeUpdates(cM);
        }
    }

    protected void aA() {
        boolean z;
        if (this.cZ) {
            this.cZ = false;
            try {
                if (cN.isProviderEnabled("gps")) {
                    cN.requestLocationUpdates("gps", 180000L, 0.0f, cM);
                    Location lastKnownLocation = cN.getLastKnownLocation("gps");
                    if (lastKnownLocation != null) {
                        cO = lastKnownLocation.getLatitude();
                        cP = lastKnownLocation.getLongitude();
                        cQ = true;
                    }
                    z = true;
                } else {
                    z = false;
                }
            } catch (Exception e) {
                if (b.cB) {
                    l.a("gps Continue 失败, 是否忘加ACCESS_FINE_LOCATION权限 ", e);
                    z = false;
                } else {
                    l.a("gps continue error, uses permission ACCESS_FINE_LOCATION", e);
                    z = false;
                }
            }
            if (!z) {
                try {
                    if (cN.isProviderEnabled("network")) {
                        cN.requestLocationUpdates("network", 180000L, 0.0f, cM);
                        Location lastKnownLocation2 = cN.getLastKnownLocation("network");
                        if (lastKnownLocation2 != null) {
                            cO = lastKnownLocation2.getLatitude();
                            cP = lastKnownLocation2.getLongitude();
                            cQ = true;
                        }
                    }
                } catch (Exception e2) {
                    if (b.cB) {
                        l.a("net location 失败, 是否忘加ACCESS_FINE_LOCATION权限 ", e2);
                    } else {
                        l.a("net location error, uses permission ACCESS_FINE_LOCATION", e2);
                    }
                }
            }
        }
    }

    protected static boolean aB() {
        return cQ;
    }

    protected static double aC() {
        return cO;
    }

    protected static double aD() {
        return cP;
    }

    protected static int aE() {
        return cS;
    }

    protected static int aF() {
        return -1;
    }

    protected static int aG() {
        return cU;
    }

    protected static int aH() {
        return cV;
    }

    protected static String aI() {
        return cW;
    }

    protected static int aJ() {
        return cX;
    }

    protected static String aK() {
        return cY == null ? "" : cY;
    }

    /* JADX INFO: Access modifiers changed from: private */
    public static void a(Location location) {
        cO = location.getLatitude();
        cP = location.getLongitude();
        cQ = true;
        Activity activityAx = b.ax();
        if (activityAx != null) {
            TelephonyManager telephonyManager = (TelephonyManager) activityAx.getSystemService("phone");
            if (telephonyManager != null) {
                try {
                    GsmCellLocation gsmCellLocation = (GsmCellLocation) telephonyManager.getCellLocation();
                    if (gsmCellLocation != null) {
                        cU = gsmCellLocation.getCid();
                        cV = gsmCellLocation.getLac();
                    }
                } catch (Exception e) {
                    if (b.cB) {
                        l.a("获取基站信息 失败, 是否忘加ACCESS_FINE_LOCATION权限 ", e);
                    } else {
                        l.a("get cell error, uses permission ACCESS_FINE_LOCATION", e);
                    }
                }
            }
            WifiManager wifiManager = (WifiManager) activityAx.getSystemService("wifi");
            if (wifiManager != null && wifiManager.getConnectionInfo() != null) {
                cW = wifiManager.getConnectionInfo().getBSSID();
            }
        }
    }

    private static void aL() {
        boolean z;
        cO = 0.0d;
        cP = 0.0d;
        cQ = false;
        try {
            if (cN.isProviderEnabled("gps")) {
                cN.requestLocationUpdates("gps", 180000L, 0.0f, cM);
                Location lastKnownLocation = cN.getLastKnownLocation("gps");
                if (lastKnownLocation != null) {
                    cO = lastKnownLocation.getLatitude();
                    cP = lastKnownLocation.getLongitude();
                    cQ = true;
                }
                z = true;
            } else {
                z = false;
            }
        } catch (Exception e) {
            if (b.cB) {
                l.a("gps InitLocation 失败, 是否忘加ACCESS_FINE_LOCATION权限 ", e);
                z = false;
            } else {
                l.a("gps InitLocation error, uses permission ACCESS_FINE_LOCATION", e);
                z = false;
            }
        }
        if (!z) {
            try {
                if (cN.isProviderEnabled("network")) {
                    cN.requestLocationUpdates("network", 180000L, 0.0f, cM);
                    Location lastKnownLocation2 = cN.getLastKnownLocation("network");
                    if (lastKnownLocation2 != null) {
                        cO = lastKnownLocation2.getLatitude();
                        cP = lastKnownLocation2.getLongitude();
                        cQ = true;
                    }
                }
            } catch (Exception e2) {
                if (b.cB) {
                    l.a("InitLocation net locatio 失败, 是否忘加ACCESS_FINE_LOCATION权限 ", e2);
                } else {
                    l.a("InitLocation net locatio error, uses permission ACCESS_FINE_LOCATION", e2);
                }
            }
        }
    }

    protected void aM() {
        Activity activityAx = b.ax();
        if (activityAx != null) {
            TelephonyManager telephonyManager = (TelephonyManager) activityAx.getSystemService("phone");
            if (telephonyManager != null) {
                try {
                    if (telephonyManager.getPhoneType() == 1) {
                        cR = telephonyManager.getSubscriberId();
                    } else if (telephonyManager.getPhoneType() != 0) {
                        cR = telephonyManager.getSimOperator();
                    }
                    if (cR != null && cR.length() >= 3) {
                        cS = Integer.parseInt(cR.substring(0, 3));
                    }
                    if (cR != null && cR.length() >= 5) {
                        cT = Integer.parseInt(cR.substring(3, 5));
                    }
                } catch (Exception e) {
                    if (b.cB) {
                        l.a("获取运营商 失败, 是否忘加ACCESS_FINE_LOCATION权限 ", e);
                    } else {
                        l.a("get operators error, uses permission ACCESS_FINE_LOCATION", e);
                    }
                }
                try {
                    GsmCellLocation gsmCellLocation = (GsmCellLocation) telephonyManager.getCellLocation();
                    if (gsmCellLocation != null) {
                        cU = gsmCellLocation.getCid();
                        cV = gsmCellLocation.getLac();
                    }
                } catch (Exception e2) {
                    if (b.cB) {
                        l.a("GetNetworkInfo获取基站信息  失败, 是否忘加ACCESS_FINE_LOCATION权限 ", e2);
                    } else {
                        l.a("GetNetworkInfo get cell error, uses permission ACCESS_FINE_LOCATION", e2);
                    }
                }
            }
            WifiManager wifiManager = (WifiManager) activityAx.getSystemService("wifi");
            if (wifiManager != null) {
                try {
                    if (wifiManager.getConnectionInfo() != null) {
                        cX = wifiManager.getConnectionInfo().getIpAddress();
                        cW = wifiManager.getConnectionInfo().getBSSID();
                        cY = wifiManager.getConnectionInfo().getMacAddress();
                        if (b.cl == null) {
                            b.cl = cY;
                        }
                    }
                } catch (Exception e3) {
                    if (b.cB) {
                        l.a("获取网络参数失败  失败, 是否忘加ACCESS_FINE_LOCATION权限 ", e3);
                    } else {
                        l.a("get net message error, uses permission ACCESS_FINE_LOCATION", e3);
                    }
                }
            }
        }
    }
}
