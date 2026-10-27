package com.tencent.webnet;

import android.webkit.WebChromeClient;
import android.webkit.WebView;

/* JADX INFO: loaded from: classes.dex */
class q extends WebChromeClient {
    final /* synthetic */ o eH;

    q(o oVar) {
        this.eH = oVar;
    }

    @Override // android.webkit.WebChromeClient
    public void onProgressChanged(WebView webView, int i) {
        super.onProgressChanged(webView, i);
        if (i != 100 || this.eH.eG) {
            return;
        }
        switch (this.eH.eD) {
            case 1:
                this.eH.eE.loadUrl(String.valueOf(this.eH.eF) + "game-ranking.html?cpId=" + b.cb + "&gameId=" + b.cc + "&isPreload=1");
                this.eH.eD++;
                break;
            case 2:
                this.eH.eE.loadUrl(String.valueOf(this.eH.eF) + "game-achievement.html?cpId=" + b.cb + "&gameId=" + b.cc + "&isPreload=1");
                this.eH.eD++;
                break;
            case 3:
                this.eH.eE.loadUrl(String.valueOf(this.eH.eF) + "center.html&isPreload=1");
                this.eH.eD++;
                break;
            case 4:
                this.eH.eE.loadUrl(String.valueOf(this.eH.eF) + "profile-self.html&isPreload=1");
                this.eH.eD++;
                break;
            case 5:
                this.eH.eE.loadUrl(String.valueOf(this.eH.eF) + "friends-list.html&isPreload=1");
                this.eH.eD++;
                break;
            case 6:
                this.eH.eE.loadUrl(String.valueOf(this.eH.eF) + "profile-mygame.html&isPreload=1");
                this.eH.eD++;
                break;
        }
    }
}
