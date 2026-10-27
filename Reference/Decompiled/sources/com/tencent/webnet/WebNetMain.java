package com.tencent.webnet;

import android.R;
import android.app.Activity;
import android.content.Context;
import android.content.res.Configuration;
import android.graphics.Bitmap;
import android.graphics.Canvas;
import android.graphics.Paint;
import android.graphics.Rect;
import android.graphics.drawable.BitmapDrawable;
import android.os.Bundle;
import android.os.Handler;
import android.os.Looper;
import android.view.KeyEvent;
import android.view.Menu;
import android.view.MenuItem;
import android.view.View;
import android.view.animation.AnimationUtils;
import android.webkit.WebSettings;
import android.webkit.WebView;
import android.widget.Button;
import android.widget.ImageView;
import android.widget.LinearLayout;
import android.widget.RelativeLayout;
import java.lang.reflect.Method;

/* JADX INFO: loaded from: classes.dex */
public class WebNetMain extends Activity {
    protected static final int gf = 1;
    protected static final int gg = 2;
    private static final int gh = 0;
    private static final int gi = 0;
    private static final int gj = 1;
    private static final int gk = 2;
    private static final int gl = 3;
    private static final String gm = "帮助信息";
    private static final String gn = "关于";
    private static final String go = "切换帐号";
    private static final String gp = "退出";
    protected static int gq = 0;
    protected static Handler eC = null;
    protected WebView gr = null;
    protected String gs = "";
    private RelativeLayout gt = null;
    protected MyButton gu = null;
    private MyButton gv = null;
    private MyButton gw = null;
    protected MessageButton gx = null;
    private MyButton gy = null;
    private MyButton gz = null;
    private MyButton gA = null;

    public class MyButton extends Button {
        protected static final int ALIGN_BOTTOM = 1;
        protected static final int ALIGN_TOP = 0;
        protected static final int dR = 2;
        protected static final int dS = 3;
        protected static final int dT = 4;
        protected static final int dU = 5;
        private Bitmap dV;
        private Bitmap dW;
        private Bitmap dX;
        private Bitmap ea;
        private int ed;
        private boolean ee;

        protected MyButton(Context context, String str, String str2, String str3, boolean z) {
            super(context);
            this.dV = null;
            this.dW = null;
            this.dX = null;
            this.ea = null;
            this.ed = 4;
            this.ee = true;
            this.dV = WebNetMain.this.T(str);
            this.dW = WebNetMain.this.T(str2);
            this.dX = WebNetMain.this.T(str3);
            this.ea = this.dW;
            setBackgroundDrawable(null);
            if (z) {
                setWidth(this.dW.getWidth() > this.dX.getWidth() ? this.dW.getWidth() : this.dX.getWidth());
                setHeight(this.dW.getHeight() > this.dX.getHeight() ? this.dW.getHeight() : this.dX.getHeight());
            }
            setOnTouchListener(new ap(this));
        }

        protected void l(int i) {
            this.ed = i;
        }

        @Override // android.widget.TextView, android.view.View
        public void setEnabled(boolean z) {
            this.ee = z;
            if (!z) {
                this.ea = this.dV;
            } else {
                this.ea = this.dW;
            }
            super.setEnabled(z);
            invalidate();
        }

        @Override // android.widget.TextView, android.view.View
        protected void onDraw(Canvas canvas) {
            int width;
            int height = 0;
            if (this.ea != null) {
                switch (this.ed) {
                    case 0:
                        width = 0;
                        break;
                    case 1:
                        width = 0;
                        height = getHeight() - this.ea.getHeight();
                        break;
                    case 2:
                        width = (getWidth() - this.ea.getWidth()) >> 1;
                        break;
                    case 3:
                        width = 0;
                        height = (getHeight() - this.ea.getHeight()) >> 1;
                        break;
                    case 4:
                        width = (getWidth() - this.ea.getWidth()) >> 1;
                        height = (getHeight() - this.ea.getHeight()) >> 1;
                        break;
                    case 5:
                        width = (getWidth() - this.ea.getWidth()) >> 1;
                        height = getHeight() - this.ea.getHeight();
                        break;
                    default:
                        width = 0;
                        break;
                }
                canvas.drawBitmap(this.ea, width, height, (Paint) null);
            }
            super.onDraw(canvas);
        }
    }

    @Override // android.app.Activity
    public void onCreate(Bundle bundle) {
        super.onCreate(bundle);
        requestWindowFeature(1);
        setRequestedOrientation(1);
        getWindow().setSoftInputMode(32);
        this.gt = new RelativeLayout(this);
        setContentView(this.gt);
        this.gt.startAnimation(AnimationUtils.loadAnimation(this, R.anim.slide_in_left));
        n.bh();
        b.cf = this;
        boolean zAW = k.aU().aW();
        if (b.bX != null && m.aZ().M("sid") != null && !((String) m.aZ().M("sid")).equals("")) {
            b.bX.a(b.bX.o(11));
        }
        d(false);
        bu();
        if (zAW) {
            try {
                this.gr.clearHistory();
                this.gr.clearCache(true);
                this.gr.clearFormData();
                deleteDatabase("webview.db");
                deleteDatabase("webviewCache.db");
            } catch (Exception e) {
                l.b("清除webview缓存失败", e);
            }
        }
        bv();
        eC = new ae(this, Looper.getMainLooper());
        b.bZ.bo();
        getWindow().getDecorView().setDrawingCacheEnabled(true);
    }

    private void d(boolean z) {
        RelativeLayout relativeLayout = new RelativeLayout(this);
        relativeLayout.setId(1);
        relativeLayout.setBackgroundDrawable(S("QQGameCenter/topback.png"));
        RelativeLayout.LayoutParams layoutParams = new RelativeLayout.LayoutParams(-2, -2);
        if (!z) {
            layoutParams.addRule(9, -1);
            if (Integer.parseInt(b.cj) > 3) {
                layoutParams.setMargins(13, 0, 0, 0);
                layoutParams.addRule(15, -1);
            } else {
                layoutParams.setMargins(13, 5, 0, 0);
            }
            this.gu = new MyButton(this, null, "QQGameCenter/back.png", "QQGameCenter/backdown.png", true);
            this.gu.setOnClickListener(new ah(this));
            relativeLayout.addView(this.gu, layoutParams);
            this.gu.setEnabled(false);
        }
        RelativeLayout.LayoutParams layoutParams2 = new RelativeLayout.LayoutParams(-2, -2);
        layoutParams2.addRule(11, -1);
        if (Integer.parseInt(b.cj) > 3) {
            layoutParams2.setMargins(0, 0, 13, 0);
            layoutParams2.addRule(15, -1);
        } else {
            layoutParams2.setMargins(0, 5, 13, 0);
        }
        this.gv = new MyButton(this, "QQGameCenter/backgame.png", "QQGameCenter/backgame.png", "QQGameCenter/backgamedown.png", true);
        this.gv.setOnClickListener(new ai(this));
        relativeLayout.addView(this.gv, layoutParams2);
        ImageView imageView = new ImageView(this);
        imageView.setImageBitmap(T("QQGameCenter/logo.png"));
        RelativeLayout.LayoutParams layoutParams3 = new RelativeLayout.LayoutParams(-2, -2);
        if (Integer.parseInt(b.cj) > 3) {
            layoutParams3.addRule(13, -1);
        } else {
            layoutParams3.addRule(14, -1);
            layoutParams3.setMargins(0, 5, 0, 0);
        }
        relativeLayout.addView(imageView, layoutParams3);
        RelativeLayout.LayoutParams layoutParams4 = new RelativeLayout.LayoutParams(-1, -2);
        layoutParams4.addRule(10, -1);
        this.gt.addView(relativeLayout, layoutParams4);
    }

    private void bu() {
        RelativeLayout.LayoutParams layoutParams = new RelativeLayout.LayoutParams(-1, -1);
        layoutParams.addRule(3, 1);
        this.gr = new WebView(this);
        this.gt.addView(this.gr, layoutParams);
        RelativeLayout.LayoutParams layoutParams2 = new RelativeLayout.LayoutParams(-1, -2);
        layoutParams2.addRule(3, 1);
        RelativeLayout relativeLayout = new RelativeLayout(this);
        relativeLayout.setBackgroundDrawable(S("QQGameCenter/topcoverback.png"));
        this.gt.addView(relativeLayout, layoutParams2);
        a(this.gr, true);
        this.gr.setWebViewClient(new aj(this));
        this.gr.setWebChromeClient(new ak(this));
        this.gr.setDownloadListener(new al(this));
        h.a(this);
        if (h.aP()) {
            WebView.enablePlatformNotifications();
        }
        this.gr.requestFocus();
        bw();
        o.c(true);
        switch (gq) {
            case 0:
                this.gr.loadUrl(String.valueOf(this.gs) + "center.html");
                break;
            case 1:
                this.gr.loadUrl(String.valueOf(this.gs) + "game-item-self.html?cpId=" + b.cb + "&gameId=" + b.cc);
                break;
            case 2:
                this.gr.loadUrl(String.valueOf(this.gs) + "game-ranking.html?cpId=" + b.cb + "&gameId=" + b.cc);
                break;
            case 3:
                this.gr.loadUrl(String.valueOf(this.gs) + "game-achievement.html?cpId=" + b.cb + "&gameId=" + b.cc);
                break;
            default:
                o.c(false);
                break;
        }
        gq = 0;
    }

    private void bv() {
        LinearLayout.LayoutParams layoutParams = new LinearLayout.LayoutParams(-2, -1);
        layoutParams.weight = 1.0f;
        LinearLayout linearLayout = new LinearLayout(this);
        linearLayout.setOrientation(0);
        linearLayout.setPadding(0, 0, 0, 0);
        linearLayout.setBackgroundDrawable(S("QQGameCenter/downback.png"));
        this.gw = new MyButton(this, "QQGameCenter/home.png", "QQGameCenter/home.png", "QQGameCenter/homedown.png", false);
        this.gw.l(5);
        this.gw.setOnClickListener(new am(this));
        linearLayout.addView(this.gw, layoutParams);
        this.gx = new MessageButton(this, T("QQGameCenter/selfmessage.png"), T("QQGameCenter/selfmessage.png"), T("QQGameCenter/selfmessagedown.png"), false);
        this.gx.l(5);
        if (b.cn > 0) {
            if (b.cn > 99) {
                this.gx.setMessage("99+");
            } else {
                this.gx.setMessage(new StringBuilder().append(b.cn).toString());
            }
        } else {
            this.gx.setMessage(null);
        }
        this.gx.setOnClickListener(new an(this));
        linearLayout.addView(this.gx, layoutParams);
        this.gy = new MyButton(this, "QQGameCenter/friend.png", "QQGameCenter/friend.png", "QQGameCenter/frienddown.png", false);
        this.gy.l(5);
        this.gy.setOnClickListener(new ao(this));
        linearLayout.addView(this.gy, layoutParams);
        this.gz = new MyButton(this, "QQGameCenter/game.png", "QQGameCenter/game.png", "QQGameCenter/gamedown.png", false);
        this.gz.l(5);
        this.gz.setOnClickListener(new af(this));
        linearLayout.addView(this.gz, layoutParams);
        this.gA = new MyButton(this, "QQGameCenter/screenshotenable.png", "QQGameCenter/screenshot.png", "QQGameCenter/screenshotpass.png", false);
        this.gA.l(5);
        this.gA.setOnClickListener(new ag(this));
        linearLayout.addView(this.gA, layoutParams);
        RelativeLayout.LayoutParams layoutParams2 = new RelativeLayout.LayoutParams(-1, -2);
        layoutParams2.addRule(12, -1);
        this.gt.addView(linearLayout, layoutParams2);
    }

    protected void bw() {
        if (!aq.V(getFilesDir() + "/html/")) {
            this.gs = "file:///android_asset/html/";
        } else {
            this.gs = "file:///" + getFilesDir() + "/html/";
        }
    }

    protected BitmapDrawable S(String str) {
        try {
            return new BitmapDrawable(getAssets().open(str));
        } catch (Exception e) {
            if (b.cB) {
                l.a("获取资源失败，是否移动了assets目录的资源", e);
            } else {
                l.a("read drawable res error, don't move assets dir", e);
            }
            return null;
        }
    }

    protected Bitmap T(String str) {
        if (str == null) {
            return null;
        }
        try {
            return new BitmapDrawable(getAssets().open(str)).getBitmap();
        } catch (Exception e) {
            if (b.cB) {
                l.a("获取资源失败，是否移动了assets目录的资源", e);
                return null;
            }
            l.a("read bitmap res error, don't move assets dir", e);
            return null;
        }
    }

    protected static void a(WebView webView, boolean z) {
        WebSettings settings = webView.getSettings();
        settings.setJavaScriptEnabled(true);
        settings.setNeedInitialFocus(true);
        settings.setSaveFormData(false);
        settings.setSavePassword(false);
        settings.setPluginsEnabled(true);
        settings.setBuiltInZoomControls(false);
        settings.setSupportZoom(true);
        settings.setCacheMode(1);
        try {
            Method declaredMethod = settings.getClass().getDeclaredMethod("setPageCacheCapacity", Integer.TYPE);
            declaredMethod.setAccessible(true);
            declaredMethod.invoke(settings, 20);
        } catch (Exception e) {
            if (b.cB) {
                l.a("设置webview的页面缓存失败!!", e);
            } else {
                l.a("set webview page cache capacity error!!", e);
            }
        }
        webView.addJavascriptInterface(new JavaScript(z), "comtencentwebnetJavaScript");
        webView.setHorizontalScrollBarEnabled(false);
        webView.setVerticalScrollBarEnabled(false);
    }

    protected Bitmap bx() {
        View decorView = getWindow().getDecorView();
        Rect rect = new Rect();
        decorView.getWindowVisibleDisplayFrame(rect);
        decorView.buildDrawingCache();
        Bitmap drawingCache = decorView.getDrawingCache();
        if (drawingCache == null) {
            return null;
        }
        int width = drawingCache.getWidth();
        int height = drawingCache.getHeight();
        if (rect.top < 0 || drawingCache.getHeight() - rect.top < 0 || height <= this.gt.getBottom() - this.gt.getTop()) {
            rect.top = 0;
        }
        int i = height - rect.top;
        Bitmap bitmapCreateBitmap = Bitmap.createBitmap(drawingCache, 0, rect.top, width, i);
        decorView.destroyDrawingCache();
        float f = width > i ? width / 460.0f : i / 460.0f;
        if (f < 1.000001f) {
            f = 1.0f;
        }
        return Bitmap.createScaledBitmap(bitmapCreateBitmap, (int) (width / f), (int) (i / f), true);
    }

    @Override // android.app.Activity
    public boolean onCreateOptionsMenu(Menu menu) {
        super.onCreateOptionsMenu(menu);
        menu.add(0, 0, 0, gm).setIcon(S("QQGameCenter/menuhelp.png"));
        menu.add(0, 1, 1, gn).setIcon(S("QQGameCenter/menuinfor.png"));
        menu.add(0, 2, 2, go).setIcon(S("QQGameCenter/menulogout.png"));
        menu.add(0, 3, 3, gp).setIcon(S("QQGameCenter/menuexit.png"));
        return true;
    }

    @Override // android.app.Activity
    public boolean onPrepareOptionsMenu(Menu menu) {
        super.onPrepareOptionsMenu(menu);
        return true;
    }

    @Override // android.app.Activity
    public boolean onOptionsItemSelected(MenuItem menuItem) {
        o.c(true);
        switch (menuItem.getItemId()) {
            case 0:
                this.gr.stopLoading();
                this.gr.loadUrl(String.valueOf(this.gs) + "help.html");
                break;
            case 1:
                this.gr.stopLoading();
                this.gr.loadUrl(String.valueOf(this.gs) + "game-version.html");
                break;
            case 2:
                this.gr.stopLoading();
                this.gr.loadUrl(String.valueOf(this.gs) + "logout.html");
                break;
            case 3:
                this.gr.stopLoading();
                finish();
                break;
            default:
                o.c(false);
                break;
        }
        return super.onOptionsItemSelected(menuItem);
    }

    @Override // android.app.Activity, android.content.ComponentCallbacks
    public void onConfigurationChanged(Configuration configuration) {
        super.onConfigurationChanged(configuration);
    }

    @Override // android.app.Activity, android.view.KeyEvent.Callback
    public boolean onKeyDown(int i, KeyEvent keyEvent) {
        if (i != 4 || !this.gr.canGoBack()) {
            return super.onKeyDown(i, keyEvent);
        }
        this.gr.goBack();
        return true;
    }

    protected void Destroy() {
        finish();
    }

    @Override // android.app.Activity
    protected void onStart() {
        super.onStart();
    }

    @Override // android.app.Activity
    protected void onPause() {
        if (b.ce != null) {
            b.ce.az();
        }
        super.onPause();
    }

    @Override // android.app.Activity
    protected void onResume() {
        if (b.ce != null) {
            b.ce.aA();
        }
        super.onResume();
    }

    @Override // android.app.Activity
    protected void onStop() {
        super.onStop();
    }

    @Override // android.app.Activity
    protected void onDestroy() {
        if (b.bZ != null) {
            b.bZ.bo();
        }
        b.cf = null;
        if (this.gr != null && h.aP()) {
            WebView.disablePlatformNotifications();
        }
        if (this.gr != null) {
            this.gr.destroy();
        }
        super.onDestroy();
    }
}
