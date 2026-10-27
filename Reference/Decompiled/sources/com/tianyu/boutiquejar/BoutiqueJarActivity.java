package com.tianyu.boutiquejar;

import android.app.Activity;
import android.content.Context;
import android.content.Intent;
import android.content.SharedPreferences;
import android.content.res.AssetManager;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.net.ConnectivityManager;
import android.net.NetworkInfo;
import android.net.Uri;
import android.os.Bundle;
import android.os.Environment;
import android.os.Handler;
import android.os.Message;
import android.support.v4.view.accessibility.AccessibilityEventCompat;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.BaseAdapter;
import android.widget.ImageView;
import android.widget.ListAdapter;
import android.widget.ListView;
import android.widget.RelativeLayout;
import android.widget.TextView;
import android.widget.Toast;
import com.tianyu.domain.Game;
import com.tianyu.service.GameService;
import java.io.File;
import java.io.IOException;
import java.io.InputStream;
import java.util.List;

/* JADX INFO: loaded from: classes.dex */
public class BoutiqueJarActivity extends Activity {
    public static final int GAMEINFO = 11;
    public static final int LOADBANNER = 14;
    public static final int NETTOAST = 13;
    private Integer R_id_b_downinfo;
    private Integer R_id_b_gamename;
    private Integer R_id_b_intro;
    private Integer R_id_b_iv_icon;
    private Integer _b_banner;
    private Integer _b_ic_launcher;
    private Integer _b_imageview;
    private Integer _b_listview;
    private Integer _chiyudaren;
    private Integer _dandantang;
    private Integer _fengkuanglaotou;
    private Integer _gujian;
    private ImageView b_imageview;
    private Integer b_item_layout;
    private ListView b_listview;
    private String channelcode;
    private RelativeLayout chiyudaren;
    private RelativeLayout dandantang;
    private RelativeLayout fengkuanglaotou;
    private Game game;
    private GameAdapter gameadapter;
    private List<Game> games;
    protected GameService gameservice;
    private RelativeLayout gujian;
    private LayoutInflater mInflater;
    private Integer main_layout;
    private String productid;
    private SharedPreferences sp;
    private Bitmap image = null;
    private Handler handler = new Handler() { // from class: com.tianyu.boutiquejar.BoutiqueJarActivity.1
        @Override // android.os.Handler
        public void handleMessage(Message msg) {
            super.handleMessage(msg);
            switch (msg.what) {
                case 11:
                    BoutiqueJarActivity.this.games = (List) msg.obj;
                    if (BoutiqueJarActivity.this.games != null) {
                        System.out.println("子线程向主线程发送适配消息");
                        BoutiqueJarActivity.this.listgame(BoutiqueJarActivity.this.games);
                    }
                    break;
                case 13:
                    BoutiqueJarActivity.this.b_imageview.setImageBitmap(BoutiqueJarActivity.this.getImageFromAssetsFile("b_banner_geezer.png"));
                    BoutiqueJarActivity.this.showtoast();
                    break;
                case 14:
                    final Game game = (Game) msg.obj;
                    String path = game.getImage();
                    System.out.println("主线程，对banner进行图片适配");
                    System.out.println("主线程适配图片的地址为：" + path);
                    BoutiqueJarActivity.this.b_imageview.setOnClickListener(new View.OnClickListener() { // from class: com.tianyu.boutiquejar.BoutiqueJarActivity.1.1
                        @Override // android.view.View.OnClickListener
                        public void onClick(View v) {
                            String path2 = ConstantPort.redirectservlet + game.getDowngameurl() + "?channelcode=" + BoutiqueJarActivity.this.channelcode + "&productid=" + BoutiqueJarActivity.this.productid;
                            System.out.println("这里有没有数据" + game.getDowngameurl());
                            Intent bannerIntent = new Intent("android.intent.action.VIEW", Uri.parse(path2));
                            BoutiqueJarActivity.this.startActivity(bannerIntent);
                        }
                    });
                    LoadImageAsyncTask task = new LoadImageAsyncTask(new LoadImageAsyncTask.LoadImageCallBack() { // from class: com.tianyu.boutiquejar.BoutiqueJarActivity.1.2
                        @Override // com.tianyu.boutiquejar.LoadImageAsyncTask.LoadImageCallBack
                        public void beforeImageLoad() {
                            BoutiqueJarActivity.this.b_imageview.setImageResource(BoutiqueJarActivity.this._b_banner.intValue());
                        }

                        @Override // com.tianyu.boutiquejar.LoadImageAsyncTask.LoadImageCallBack
                        public void afterImageLoad(Bitmap result) {
                            if (result != null) {
                                BoutiqueJarActivity.this.b_imageview.setImageBitmap(result);
                            } else {
                                BoutiqueJarActivity.this.b_imageview.setImageResource(BoutiqueJarActivity.this._b_banner.intValue());
                            }
                        }
                    });
                    task.execute(path);
                    break;
            }
        }
    };

    /* JADX WARN: Type inference failed for: r2v84, types: [com.tianyu.boutiquejar.BoutiqueJarActivity$3] */
    /* JADX WARN: Type inference failed for: r2v98, types: [com.tianyu.boutiquejar.BoutiqueJarActivity$7] */
    @Override // android.app.Activity
    public void onCreate(Bundle savedInstanceState) {
        getWindow().setFlags(AccessibilityEventCompat.TYPE_TOUCH_EXPLORATION_GESTURE_END, AccessibilityEventCompat.TYPE_TOUCH_EXPLORATION_GESTURE_END);
        setRequestedOrientation(1);
        requestWindowFeature(1);
        super.onCreate(savedInstanceState);
        Bundle bundle = getIntent().getExtras();
        this.channelcode = bundle.getString("channelcode");
        this.productid = bundle.getString("productid");
        this.main_layout = Integer.valueOf(bundle.getInt("main_layout"));
        this.b_item_layout = Integer.valueOf(bundle.getInt("b_item_layout"));
        this.R_id_b_iv_icon = Integer.valueOf(bundle.getInt("R_id_b_iv_icon"));
        this.R_id_b_gamename = Integer.valueOf(bundle.getInt("R_id_b_gamename"));
        this.R_id_b_intro = Integer.valueOf(bundle.getInt("R_id_b_intro"));
        this.R_id_b_downinfo = Integer.valueOf(bundle.getInt("R_id_b_downinfo"));
        this._gujian = Integer.valueOf(bundle.getInt("_gujian"));
        this._dandantang = Integer.valueOf(bundle.getInt("_dandantang"));
        this._fengkuanglaotou = Integer.valueOf(bundle.getInt("_fengkuanglaotou"));
        this._b_imageview = Integer.valueOf(bundle.getInt("_b_imageview"));
        this._b_listview = Integer.valueOf(bundle.getInt("_b_listview"));
        this._b_ic_launcher = Integer.valueOf(bundle.getInt("_b_ic_launcher"));
        this._b_banner = Integer.valueOf(bundle.getInt("_b_banner"));
        System.out.println("从外界获取的数据是 +" + this.channelcode + this.productid);
        this.sp = getSharedPreferences("SP", 0);
        SharedPreferences.Editor editor = this.sp.edit();
        editor.putString("channelcode", this.channelcode);
        editor.putString("productid", this.productid);
        editor.putInt("main_layout", this.main_layout.intValue());
        editor.putInt("b_item_layout", this.b_item_layout.intValue());
        editor.putInt("R_id_b_gamename", this.R_id_b_gamename.intValue());
        editor.putInt("R_id_b_intro", this.R_id_b_intro.intValue());
        editor.putInt("R_id_b_downinfo", this.R_id_b_downinfo.intValue());
        editor.putInt("_gujian", this._gujian.intValue());
        editor.putInt("_dandantang", this._dandantang.intValue());
        editor.putInt("_fengkuanglaotou", this._fengkuanglaotou.intValue());
        editor.putInt("_b_imageview", this._b_imageview.intValue());
        editor.putInt("_b_listview", this._b_listview.intValue());
        editor.putInt("_b_ic_launcher", this._b_ic_launcher.intValue());
        editor.putInt("_b_banner", this._b_banner.intValue());
        editor.commit();
        setContentView(this.main_layout.intValue());
        this.gujian = (RelativeLayout) findViewById(this._gujian.intValue());
        System.out.println("这里输出的值是多少");
        System.out.println(this._gujian);
        this.fengkuanglaotou = (RelativeLayout) findViewById(this._fengkuanglaotou.intValue());
        this.dandantang = (RelativeLayout) findViewById(this._dandantang.intValue());
        this.b_imageview = (ImageView) findViewById(this._b_imageview.intValue());
        this.b_imageview.setOnClickListener(new View.OnClickListener() { // from class: com.tianyu.boutiquejar.BoutiqueJarActivity.2
            @Override // android.view.View.OnClickListener
            public void onClick(View v) {
                String path = ConstantPort.redirectservlet + BoutiqueJarActivity.this.game.getDowngameurl() + "?channelcode=" + BoutiqueJarActivity.this.channelcode + "&productid=" + BoutiqueJarActivity.this.productid;
                System.out.println("这里有没有数据" + BoutiqueJarActivity.this.game.getDowngameurl());
                Intent bannerIntent = new Intent("android.intent.action.VIEW", Uri.parse(path));
                BoutiqueJarActivity.this.startActivity(bannerIntent);
            }
        });
        new Thread() { // from class: com.tianyu.boutiquejar.BoutiqueJarActivity.3
            @Override // java.lang.Thread, java.lang.Runnable
            public void run() {
                super.run();
                System.out.println("新建一个线程，判断联网");
                if (BoutiqueJarActivity.this.isNetworkAvaiable()) {
                    System.out.println("联网成功");
                    BoutiqueJarActivity.this.b_imageview.setOnClickListener(new View.OnClickListener() { // from class: com.tianyu.boutiquejar.BoutiqueJarActivity.3.1
                        @Override // android.view.View.OnClickListener
                        public void onClick(View v) {
                            Intent bannerIntent = new Intent("android.intent.action.VIEW", Uri.parse("http://gtouch.3g.qq.com/gamedetail.action?id=225593&sid=00"));
                            BoutiqueJarActivity.this.startActivity(bannerIntent);
                        }
                    });
                    String path1 = ConstantPort.bannerpath + BoutiqueJarActivity.this.channelcode + "&productid=" + BoutiqueJarActivity.this.productid;
                    System.out.println("banner的请求路径为" + path1);
                    try {
                        new Game();
                        Game gamebanner = BoutiqueJarActivity.this.gameservice.getGamesFromJson1(path1);
                        System.out.println("gamebanner是否为空");
                        System.out.println(gamebanner == null);
                        Message msg = new Message();
                        msg.obj = gamebanner;
                        msg.what = 14;
                        BoutiqueJarActivity.this.handler.sendMessage(msg);
                        return;
                    } catch (Exception e) {
                        e.printStackTrace();
                        return;
                    }
                }
                System.out.println("联网失败");
                BoutiqueJarActivity.this.b_imageview.setOnClickListener(new View.OnClickListener() { // from class: com.tianyu.boutiquejar.BoutiqueJarActivity.3.2
                    @Override // android.view.View.OnClickListener
                    public void onClick(View v) {
                        System.out.println("给banner设置的点击事件");
                        Message msg2 = new Message();
                        msg2.what = 13;
                        BoutiqueJarActivity.this.handler.sendMessage(msg2);
                    }
                });
                BoutiqueJarActivity.this.gujian.setOnClickListener(new View.OnClickListener() { // from class: com.tianyu.boutiquejar.BoutiqueJarActivity.3.3
                    @Override // android.view.View.OnClickListener
                    public void onClick(View v) {
                        System.out.println("给listview设置的点击事件");
                        Message msg2 = new Message();
                        msg2.what = 13;
                        BoutiqueJarActivity.this.handler.sendMessage(msg2);
                    }
                });
                BoutiqueJarActivity.this.dandantang.setOnClickListener(new View.OnClickListener() { // from class: com.tianyu.boutiquejar.BoutiqueJarActivity.3.4
                    @Override // android.view.View.OnClickListener
                    public void onClick(View v) {
                        Message msg2 = new Message();
                        msg2.what = 13;
                        BoutiqueJarActivity.this.handler.sendMessage(msg2);
                    }
                });
                BoutiqueJarActivity.this.fengkuanglaotou.setOnClickListener(new View.OnClickListener() { // from class: com.tianyu.boutiquejar.BoutiqueJarActivity.3.5
                    @Override // android.view.View.OnClickListener
                    public void onClick(View v) {
                        Message msg2 = new Message();
                        msg2.what = 13;
                        BoutiqueJarActivity.this.handler.sendMessage(msg2);
                    }
                });
            }
        }.start();
        this.gujian.setOnClickListener(new View.OnClickListener() { // from class: com.tianyu.boutiquejar.BoutiqueJarActivity.4
            @Override // android.view.View.OnClickListener
            public void onClick(View v) {
                Toast.makeText(BoutiqueJarActivity.this, "网络不给力哦~~", 0).show();
                Intent bannerIntent = new Intent("android.intent.action.VIEW", Uri.parse("http://tygames.cn:8090/Game/RedirectServlet"));
                BoutiqueJarActivity.this.startActivity(bannerIntent);
            }
        });
        this.dandantang.setOnClickListener(new View.OnClickListener() { // from class: com.tianyu.boutiquejar.BoutiqueJarActivity.5
            @Override // android.view.View.OnClickListener
            public void onClick(View v) {
                Toast.makeText(BoutiqueJarActivity.this, "网络不给力哦~~", 0).show();
                Intent bannerIntent = new Intent("android.intent.action.VIEW", Uri.parse("http://tygames.cn:8090/Game/RedirectServlet"));
                BoutiqueJarActivity.this.startActivity(bannerIntent);
            }
        });
        this.fengkuanglaotou.setOnClickListener(new View.OnClickListener() { // from class: com.tianyu.boutiquejar.BoutiqueJarActivity.6
            @Override // android.view.View.OnClickListener
            public void onClick(View v) {
                Toast.makeText(BoutiqueJarActivity.this, "网络不给力哦~~", 0).show();
                Intent bannerIntent = new Intent("android.intent.action.VIEW", Uri.parse("http://tygames.cn:8090/Game/RedirectServlet"));
                BoutiqueJarActivity.this.startActivity(bannerIntent);
            }
        });
        this.b_listview = (ListView) findViewById(this._b_listview.intValue());
        this.gameservice = new GameService();
        this.mInflater = LayoutInflater.from(this);
        System.out.println("这里开启一个新的线程,给listview解析数据");
        if (isNetworkAvaiable()) {
            System.out.println("判断是否联网，联网新启一个线程，从网络下载listview数据");
            new Thread() { // from class: com.tianyu.boutiquejar.BoutiqueJarActivity.7
                @Override // java.lang.Thread, java.lang.Runnable
                public void run() {
                    super.run();
                    BoutiqueJarActivity.this.gujian.setOnClickListener(new View.OnClickListener() { // from class: com.tianyu.boutiquejar.BoutiqueJarActivity.7.1
                        @Override // android.view.View.OnClickListener
                        public void onClick(View v) {
                            Intent bannerIntent = new Intent("android.intent.action.VIEW", Uri.parse("http://gtouch.3g.qq.com/gamedetail.action?id=225593&sid=00"));
                            BoutiqueJarActivity.this.startActivity(bannerIntent);
                        }
                    });
                    BoutiqueJarActivity.this.dandantang.setOnClickListener(new View.OnClickListener() { // from class: com.tianyu.boutiquejar.BoutiqueJarActivity.7.2
                        @Override // android.view.View.OnClickListener
                        public void onClick(View v) {
                            Intent bannerIntent = new Intent("android.intent.action.VIEW", Uri.parse("http://gtouch.3g.qq.com/gamedetail.action?id=1170645&sid=00"));
                            BoutiqueJarActivity.this.startActivity(bannerIntent);
                        }
                    });
                    BoutiqueJarActivity.this.fengkuanglaotou.setOnClickListener(new View.OnClickListener() { // from class: com.tianyu.boutiquejar.BoutiqueJarActivity.7.3
                        @Override // android.view.View.OnClickListener
                        public void onClick(View v) {
                            Intent bannerIntent = new Intent("android.intent.action.VIEW", Uri.parse("http://gtouch.3g.qq.com/gamedetail.action?id=1169977&sid=00"));
                            BoutiqueJarActivity.this.startActivity(bannerIntent);
                        }
                    });
                    String path = ConstantPort.listviewpath + BoutiqueJarActivity.this.channelcode + "&productid=" + BoutiqueJarActivity.this.productid;
                    System.out.println("listview请求路径为" + path);
                    try {
                        System.out.println("接下来发送请求，获取资源，解析数据");
                        BoutiqueJarActivity.this.games = BoutiqueJarActivity.this.gameservice.getGamesFromJson(path);
                    } catch (Exception e) {
                        e.printStackTrace();
                    }
                    Message msg = new Message();
                    msg.obj = BoutiqueJarActivity.this.games;
                    msg.what = 11;
                    BoutiqueJarActivity.this.handler.sendMessage(msg);
                }
            }.start();
        } else {
            System.out.println("联网失败，无法从网络获取listview数据");
        }
    }

    protected void showtoast() {
        Toast.makeText(this, "网络不给力哦~~", 0).show();
        Intent bannerIntent = new Intent("android.intent.action.VIEW", Uri.parse("http://tygames.cn:8090/Game/RedirectServlet"));
        startActivity(bannerIntent);
    }

    /* JADX INFO: Access modifiers changed from: private */
    public boolean isNetworkAvaiable() {
        ConnectivityManager cm = (ConnectivityManager) getSystemService("connectivity");
        NetworkInfo info = cm.getActiveNetworkInfo();
        return info != null && info.isConnected();
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void listgame(List<Game> games2) {
        try {
            System.out.println("主线程开始适配");
            this.gameadapter = new GameAdapter(this, this.games);
            this.gujian.setVisibility(4);
            this.dandantang.setVisibility(4);
            this.fengkuanglaotou.setVisibility(4);
            this.b_listview.setAdapter((ListAdapter) this.gameadapter);
        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    /* JADX INFO: Access modifiers changed from: private */
    public Bitmap getImageFromAssetsFile(String string) {
        AssetManager am = getResources().getAssets();
        try {
            InputStream is = am.open(string);
            this.image = BitmapFactory.decodeStream(is);
            is.close();
        } catch (IOException e) {
            e.printStackTrace();
        }
        return this.image;
    }

    public class GameAdapter extends BaseAdapter {
        private List<Game> games;

        public GameAdapter(Context context, List<Game> games) {
            this.games = games;
        }

        @Override // android.widget.Adapter
        public int getCount() {
            System.out.println(this.games.size());
            return this.games.size();
        }

        @Override // android.widget.Adapter
        public Object getItem(int position) {
            return null;
        }

        @Override // android.widget.Adapter
        public long getItemId(int position) {
            return 0L;
        }

        @Override // android.widget.Adapter
        public View getView(int position, View converView, ViewGroup parent) {
            System.out.println("listview 适配前，先判断converView是否为空");
            System.out.println(converView == null);
            if (converView == null) {
                converView = BoutiqueJarActivity.this.mInflater.inflate(BoutiqueJarActivity.this.b_item_layout.intValue(), (ViewGroup) null);
            }
            System.out.println("listview中加载item");
            final ImageView b_iv_icon = (ImageView) converView.findViewById(BoutiqueJarActivity.this.R_id_b_iv_icon.intValue());
            TextView b_gamename = (TextView) converView.findViewById(BoutiqueJarActivity.this.R_id_b_gamename.intValue());
            TextView b_intro = (TextView) converView.findViewById(BoutiqueJarActivity.this.R_id_b_intro.intValue());
            TextView b_downinfo = (TextView) converView.findViewById(BoutiqueJarActivity.this.R_id_b_downinfo.intValue());
            final Game game = this.games.get(position);
            try {
                String path = game.getImage();
                System.out.println("listview图片的地址是" + path);
                String name = path.substring(path.lastIndexOf("/") + 1);
                System.out.println("图片的名字是" + name);
                File imageFile = new File(Environment.getExternalStorageDirectory(), name);
                System.out.println("444444444444444444444444444444444给listview适配，图片资源是否存在");
                System.out.println(imageFile.exists());
                if (!imageFile.exists()) {
                    String iconpath = game.getImage();
                    System.out.println("这里执行了异步任务");
                    LoadImageAsyncTask task = new LoadImageAsyncTask(new LoadImageAsyncTask.LoadImageCallBack() { // from class: com.tianyu.boutiquejar.BoutiqueJarActivity.GameAdapter.1
                        @Override // com.tianyu.boutiquejar.LoadImageAsyncTask.LoadImageCallBack
                        public void beforeImageLoad() {
                            System.out.println("执行异步任务之前");
                            System.out.println(BoutiqueJarActivity.this._b_ic_launcher);
                            b_iv_icon.setImageResource(com.ty.jl.R.drawable.ic_launcher);
                            System.out.println("这是给添加默认的图片");
                        }

                        @Override // com.tianyu.boutiquejar.LoadImageAsyncTask.LoadImageCallBack
                        public void afterImageLoad(Bitmap result) {
                            if (result == null) {
                                b_iv_icon.setImageResource(BoutiqueJarActivity.this._b_ic_launcher.intValue());
                            } else {
                                System.out.println("执行异步任务之后");
                                b_iv_icon.setImageBitmap(result);
                            }
                        }
                    });
                    task.execute(iconpath);
                    b_iv_icon.setImageURI(Uri.fromFile(imageFile));
                } else {
                    String imagepath = new StringBuilder().append(Uri.fromFile(imageFile)).toString().substring(7);
                    System.out.println("截取后的图片地址为：" + imagepath);
                    BitmapFactory.Options options = new BitmapFactory.Options();
                    options.inSampleSize = 0;
                    Bitmap bm = BitmapFactory.decodeFile(imagepath, options);
                    b_iv_icon.setImageBitmap(bm);
                }
            } catch (Exception e) {
                e.printStackTrace();
            }
            b_gamename.setText(game.getGamename());
            b_intro.setText(game.getPromotedesc());
            b_downinfo.setText(game.getPromotedowloaddesc());
            converView.setOnClickListener(new View.OnClickListener() { // from class: com.tianyu.boutiquejar.BoutiqueJarActivity.GameAdapter.2
                @Override // android.view.View.OnClickListener
                public void onClick(View paramView) {
                    String channelcode = BoutiqueJarActivity.this.sp.getString("channelcode", "none");
                    String productid = BoutiqueJarActivity.this.sp.getString("productid", "none");
                    String path2 = ConstantPort.redirectservlet + game.getDowngameurl() + "?channelcode=" + channelcode + "&productid=" + productid;
                    Intent browserIntent = new Intent("android.intent.action.VIEW", Uri.parse(path2));
                    BoutiqueJarActivity.this.startActivity(browserIntent);
                }
            });
            return converView;
        }
    }
}
