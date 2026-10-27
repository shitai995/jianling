package com.tencent.webnet;

import android.location.Location;
import android.location.LocationListener;
import android.os.Bundle;

/* JADX INFO: loaded from: classes.dex */
class f implements LocationListener {
    f() {
    }

    @Override // android.location.LocationListener
    public void onLocationChanged(Location location) {
        if (location != null) {
            e.a(location);
        }
    }

    @Override // android.location.LocationListener
    public void onProviderDisabled(String str) {
    }

    @Override // android.location.LocationListener
    public void onProviderEnabled(String str) {
    }

    @Override // android.location.LocationListener
    public void onStatusChanged(String str, int i, Bundle bundle) {
    }
}
