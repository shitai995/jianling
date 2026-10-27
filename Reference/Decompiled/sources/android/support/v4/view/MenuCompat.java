package android.support.v4.view;

import android.os.Build;
import android.view.MenuItem;

/* JADX INFO: loaded from: classes.dex */
public class MenuCompat {
    static final MenuVersionImpl IMPL;

    interface MenuVersionImpl {
        boolean setShowAsAction(MenuItem menuItem, int i);
    }

    static class BaseMenuVersionImpl implements MenuVersionImpl {
        BaseMenuVersionImpl() {
        }

        @Override // android.support.v4.view.MenuCompat.MenuVersionImpl
        public boolean setShowAsAction(MenuItem item, int actionEnum) {
            return false;
        }
    }

    static class HoneycombMenuVersionImpl implements MenuVersionImpl {
        HoneycombMenuVersionImpl() {
        }

        @Override // android.support.v4.view.MenuCompat.MenuVersionImpl
        public boolean setShowAsAction(MenuItem item, int actionEnum) {
            MenuItemCompatHoneycomb.setShowAsAction(item, actionEnum);
            return true;
        }
    }

    static {
        if (Build.VERSION.SDK_INT >= 11) {
            IMPL = new HoneycombMenuVersionImpl();
        } else {
            IMPL = new BaseMenuVersionImpl();
        }
    }

    public static boolean setShowAsAction(MenuItem item, int actionEnum) {
        return IMPL.setShowAsAction(item, actionEnum);
    }
}
