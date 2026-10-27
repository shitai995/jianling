package com.tencent.webnet;

import android.os.Handler;
import android.os.Looper;
import android.os.Message;
import android.webkit.WebView;

/* JADX INFO: loaded from: classes.dex */
class o extends Thread {
    private static final int eA = 0;
    protected static o eB = null;
    private Handler eC;
    private int eD = 0;
    private WebView eE = null;
    private String eF = null;
    private boolean eG = false;

    protected o() {
        this.eC = null;
        if (!WebNetInterface.bq()) {
            setName("PrePull");
            eB = this;
            this.eC = new p(this, Looper.getMainLooper());
            Message message = new Message();
            message.what = 0;
            this.eC.sendMessage(message);
        }
    }

    protected static void aX() {
        if (eB != null && eB.isAlive()) {
            try {
                eB.stop();
            } catch (Exception e) {
            }
        }
    }

    protected static void c(boolean z) {
        if (eB != null && eB.eD <= 6 && eB.eG != z) {
            eB.eG = z;
            if (eB.eG && eB.eE != null) {
                eB.eE.stopLoading();
                if (eB.eD > 0) {
                    eB.eD--;
                    return;
                }
                return;
            }
            if (eB.eE != null) {
                switch (eB.eD) {
                    case 0:
                        eB.eE.loadUrl(String.valueOf(eB.eF) + "game-item-self.html?cpId=" + b.cb + "&gameId=" + b.cc + "&isPreload=1");
                        eB.eD++;
                        break;
                    case 1:
                        eB.eE.loadUrl(String.valueOf(eB.eF) + "game-ranking.html?cpId=" + b.cb + "&gameId=" + b.cc + "&isPreload=1");
                        eB.eD++;
                        break;
                    case 2:
                        eB.eE.loadUrl(String.valueOf(eB.eF) + "game-achievement.html?cpId=" + b.cb + "&gameId=" + b.cc + "&isPreload=1");
                        eB.eD++;
                        break;
                    case 3:
                        eB.eE.loadUrl(String.valueOf(eB.eF) + "center.html&isPreload=1");
                        eB.eD++;
                        break;
                    case 4:
                        eB.eE.loadUrl(String.valueOf(eB.eF) + "profile-self.html&isPreload=1");
                        eB.eD++;
                        break;
                    case 5:
                        eB.eE.loadUrl(String.valueOf(eB.eF) + "friends-list.html&isPreload=1");
                        eB.eD++;
                        break;
                    case 6:
                        eB.eE.loadUrl(String.valueOf(eB.eF) + "profile-mygame.html&isPreload=1");
                        eB.eD++;
                        break;
                }
            }
        }
    }

    @Override // java.lang.Thread, java.lang.Runnable
    public void run() {
        if (!aq.V(b.ca.getFilesDir() + "/html/")) {
            this.eF = "file:///android_asset/html/";
        } else {
            this.eF = "file:///" + b.ca.getFilesDir() + "/html/";
        }
        this.eE.setWebChromeClient(new q(this));
        this.eE.loadUrl(String.valueOf(this.eF) + "game-item-self.html?cpId=" + b.cb + "&gameId=" + b.cc + "&isPreload=1");
        this.eD++;
        while (this.eD <= 6) {
            b.ay();
        }
        eB = null;
    }
}
