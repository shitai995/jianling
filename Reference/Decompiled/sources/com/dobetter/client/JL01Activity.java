package com.dobetter.client;

import android.app.Activity;
import android.app.AlertDialog;
import android.content.DialogInterface;
import android.content.Intent;
import android.media.AudioManager;
import android.media.MediaPlayer;
import android.os.Bundle;
import android.os.Process;
import android.support.v4.view.accessibility.AccessibilityEventCompat;
import android.telephony.TelephonyManager;
import android.util.DisplayMetrics;
import android.view.KeyEvent;
import com.tencent.webnet.WebNetInterface;
import com.tianyu.boutiquejar.BoutiqueJarActivity;
import com.ty.jl.R;
import dalvik.system.VMRuntime;

/* JADX INFO: loaded from: classes.dex */
public class JL01Activity extends Activity {
    public static final byte SOUND_CRITICAL = 1;
    public static final byte SOUND_HIT = 2;
    public static final byte SOUND_MISS = 0;
    private static final float TARGET_HEAP_UTILIZATION = 0.75f;
    public static JL01Activity activity;
    static boolean isBack;
    private AudioManager audioManager;
    private BgMusic bgMusic;
    DisplayMetrics dm;
    private EffectSound effectSound;
    GameCanvas gameCanvas;
    private float maxVulume;
    public int smsType;
    private TelephonyManager telMgr;
    private float volumeValue;
    public boolean isSound = true;
    private final byte SOUND_COUNTS = 3;

    @Override // android.app.Activity
    public void onCreate(Bundle savedInstanceState) {
        System.out.println("on create -----------------------------");
        super.onCreate(savedInstanceState);
        activity = this;
        initScreenData(true);
        initGame();
        setContentView(this.gameCanvas);
        GameCenterEvent webEvent = new GameCenterEvent();
        WebNetInterface.Init(this, webEvent);
        this.telMgr = (TelephonyManager) getSystemService("phone");
        if (this.telMgr.getNetworkOperator().equals("46001")) {
            this.smsType = 1;
        }
    }

    public void initMoreGame() {
        Intent intent = new Intent(this, (Class<?>) BoutiqueJarActivity.class);
        Bundle bundle = new Bundle();
        bundle.putString("channelcode", "1");
        bundle.putString("productid", "1");
        bundle.putInt("main_layout", R.drawable.boutique_jar);
        bundle.putInt("b_item_layout", R.drawable.b_item);
        bundle.putInt("R_id_b_iv_icon", R.id.b_iv_icon);
        bundle.putInt("R_id_b_gamename", R.id.b_gamename);
        bundle.putInt("R_id_b_intro", R.id.b_intro);
        bundle.putInt("R_id_b_downinfo", R.id.b_downinfo);
        bundle.putInt("_fengkuanglaotou", R.id.b_lv_fengkuanglaotou);
        bundle.putInt("_gujian", R.id.b_lv_gujian);
        bundle.putInt("_dandantang", R.id.b_lv_dandantang);
        bundle.putInt("_b_imageview", R.id.b_imageview);
        bundle.putInt("_b_listview", R.id.b_listview);
        bundle.putInt("_b_ic_launcher", R.drawable.ic_launcher);
        bundle.putInt("_b_banner", R.drawable.b_banner);
        intent.putExtras(bundle);
        startActivity(intent);
    }

    private void initGame() {
        if (this.gameCanvas == null) {
            this.gameCanvas = new GameCanvas();
        }
        if (this.bgMusic == null) {
            this.bgMusic = new BgMusic();
        }
        if (this.effectSound == null) {
            this.effectSound = new EffectSound();
        }
    }

    private void initScreenData(boolean isLandscape) {
        getWindow().setFlags(128, 128);
        VMRuntime.getRuntime().setTargetHeapUtilization(TARGET_HEAP_UTILIZATION);
        if (this.audioManager == null) {
            this.audioManager = (AudioManager) getSystemService("audio");
        }
        setVolumeControlStream(3);
        requestWindowFeature(1);
        getWindow().setFlags(AccessibilityEventCompat.TYPE_TOUCH_EXPLORATION_GESTURE_END, AccessibilityEventCompat.TYPE_TOUCH_EXPLORATION_GESTURE_END);
        setRequestedOrientation(isLandscape ? 0 : 1);
        this.volumeValue = this.audioManager.getStreamVolume(3);
        this.maxVulume = this.audioManager.getStreamMaxVolume(3);
        if (this.dm == null) {
            this.dm = new DisplayMetrics();
        }
        getWindowManager().getDefaultDisplay().getMetrics(this.dm);
        GameCanvas.SCREEN_WIDTH = (short) this.dm.widthPixels;
        GameCanvas.SCREEN_HEIGHT = (short) this.dm.heightPixels;
        System.out.println("screen_W======" + GameCanvas.SCREEN_WIDTH);
        System.out.println("screen_H======" + GameCanvas.SCREEN_HEIGHT);
    }

    @Override // android.app.Activity, android.view.KeyEvent.Callback
    public boolean onKeyDown(int keyCode, KeyEvent event) {
        if (keyCode == 4) {
            isBack = isBack ? false : true;
            new AlertDialog.Builder(this).setTitle("提示").setMessage("确定退出吗？").setPositiveButton("确定", new DialogInterface.OnClickListener() { // from class: com.dobetter.client.JL01Activity.1
                @Override // android.content.DialogInterface.OnClickListener
                public void onClick(DialogInterface arg0, int arg1) {
                    JL01Activity.this.exitGame();
                }
            }).setNegativeButton("取消", new DialogInterface.OnClickListener() { // from class: com.dobetter.client.JL01Activity.2
                @Override // android.content.DialogInterface.OnClickListener
                public void onClick(DialogInterface arg0, int arg1) {
                    JL01Activity.isBack = false;
                }
            }).show();
        } else if (keyCode == 25) {
            this.volumeValue = this.audioManager.getStreamVolume(3);
            addVolume(false);
        } else if (keyCode == 24) {
            this.volumeValue = this.audioManager.getStreamVolume(3);
            addVolume(true);
        }
        return true;
    }

    public void addVolume(boolean isAdd) {
        if (!isAdd) {
            this.audioManager.setStreamVolume(3, ((int) this.volumeValue) - 1, 4);
        } else {
            this.audioManager.setStreamVolume(3, ((int) this.volumeValue) + 1, 4);
        }
    }

    @Override // android.app.Activity, android.view.KeyEvent.Callback
    public boolean onKeyUp(int keyCode, KeyEvent event) {
        return true;
    }

    private class BgMusic {
        private int curPlayMusicResId;
        public boolean m_bgMusicOn = true;
        private MediaPlayer player;

        public BgMusic() {
        }

        public void play(int resId) {
            if (this.m_bgMusicOn) {
                if (resId == this.curPlayMusicResId) {
                    if (this.player != null && this.player.isPlaying()) {
                        return;
                    }
                } else if (this.player != null && this.player.isPlaying()) {
                    stop();
                }
                if (this.player == null) {
                    this.player = MediaPlayer.create(JL01Activity.this, resId);
                    this.player.setLooping(true);
                } else {
                    this.player.seekTo(0);
                }
                this.player.start();
                this.curPlayMusicResId = resId;
            }
        }

        public void stop() {
            if (this.player != null) {
                if (this.player.isPlaying()) {
                    this.player.stop();
                }
                this.player.release();
            }
            this.player = null;
        }

        public void onPause() {
            stop();
        }

        public void onResume() {
            JL01Activity.this.gameCanvas.playMusic();
        }

        public void release() {
            stop();
        }
    }

    private class EffectSound {
        public boolean m_effSoundOn = true;
        MediaPlayer[] players;

        public EffectSound() {
            init();
        }

        public void init() {
            loadAll();
        }

        private void loadAll() {
            this.players = new MediaPlayer[3];
            this.players[0] = MediaPlayer.create(JL01Activity.this, R.raw.miss);
            this.players[1] = MediaPlayer.create(JL01Activity.this, R.raw.critical);
            this.players[2] = MediaPlayer.create(JL01Activity.this, R.raw.hit);
        }

        public void play(int soundId, int loopCounts) {
            if (this.m_effSoundOn && this.players[soundId] != null && !this.players[soundId].isPlaying()) {
                this.players[soundId].start();
            }
        }

        public void release() {
            for (int i = 0; i < 3; i++) {
                this.players[i].release();
                this.players[i] = null;
            }
        }
    }

    public void playBgMusic(int resId) {
        this.bgMusic.play(resId);
    }

    public void stopBgMusic() {
        this.bgMusic.stop();
    }

    public void setMaxVolume() {
        this.audioManager.setStreamVolume(3, (int) this.maxVulume, 4);
    }

    public void setMinVolume() {
        this.audioManager.setStreamVolume(3, 0, 4);
    }

    public void releaseBgMusic() {
        this.bgMusic.release();
    }

    public void palyEffSound(int soundId, int loopCounts) {
        this.effectSound.play(soundId, loopCounts);
    }

    public void releaseEffSound() {
        this.effectSound.release();
    }

    public void exitGame() {
        this.bgMusic.release();
        finish();
        this.gameCanvas = null;
        this.bgMusic = null;
        this.effectSound = null;
        Process.killProcess(Process.myPid());
        System.exit(0);
    }

    private void pauseGame() {
        this.bgMusic.onPause();
    }

    @Override // android.app.Activity
    protected void onPause() {
        super.onPause();
    }

    @Override // android.app.Activity
    protected void onResume() {
        if (getRequestedOrientation() != 0) {
            setRequestedOrientation(0);
        }
        WebNetInterface.SetCurActivity(this);
        super.onResume();
    }

    private void resumeGame() {
        this.bgMusic.onResume();
    }

    @Override // android.app.Activity
    protected void onStop() {
        super.onStop();
    }

    @Override // android.app.Activity
    protected void onStart() {
        super.onStart();
    }

    @Override // android.app.Activity
    protected void onDestroy() {
        WebNetInterface.Destroy();
        super.onDestroy();
    }

    @Override // android.app.Activity
    protected void onRestoreInstanceState(Bundle savedInstanceState) {
        System.out.println("onRestoreInstanceState------------------------");
        super.onRestoreInstanceState(savedInstanceState);
    }

    @Override // android.app.Activity
    protected void onSaveInstanceState(Bundle outState) {
        System.out.println("onSaveInstanceState------------------------");
        super.onSaveInstanceState(outState);
    }

    @Override // android.app.Activity, android.view.Window.Callback
    public void onWindowFocusChanged(boolean hasFocus) {
        if (hasFocus) {
            if (this.gameCanvas.bGameState != 0) {
                resumeGame();
            }
            System.out.println("解锁");
        } else {
            pauseGame();
            System.out.println("锁定");
        }
        super.onWindowFocusChanged(hasFocus);
    }
}
