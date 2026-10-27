package com.tianyu.boutiquejar;

import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.os.AsyncTask;
import java.io.InputStream;
import java.net.HttpURLConnection;
import java.net.URL;

/* JADX INFO: loaded from: classes.dex */
public class LoadImageAsyncTask extends AsyncTask<String, Void, Bitmap> {
    private LoadImageCallBack mLoadImageCallBack;

    public interface LoadImageCallBack {
        void afterImageLoad(Bitmap bitmap);

        void beforeImageLoad();
    }

    public LoadImageAsyncTask(LoadImageCallBack mLoadImageCallBack) {
        this.mLoadImageCallBack = mLoadImageCallBack;
    }

    @Override // android.os.AsyncTask
    protected void onPreExecute() {
        super.onPreExecute();
        System.out.println("给imageview设置一个默认的图片");
        this.mLoadImageCallBack.beforeImageLoad();
    }

    /* JADX INFO: Access modifiers changed from: protected */
    @Override // android.os.AsyncTask
    public void onPostExecute(Bitmap result) {
        super.onPostExecute(result);
        System.out.println("给imageview设置下载好的图片");
        this.mLoadImageCallBack.afterImageLoad(result);
    }

    /* JADX INFO: Access modifiers changed from: protected */
    @Override // android.os.AsyncTask
    public Bitmap doInBackground(String... params) {
        try {
            System.out.println("***************************************");
            String iconpath = params[0];
            System.out.println(iconpath);
            URL url = new URL(iconpath);
            HttpURLConnection conn = (HttpURLConnection) url.openConnection();
            conn.setRequestMethod("GET");
            conn.setConnectTimeout(5000);
            InputStream is = conn.getInputStream();
            return BitmapFactory.decodeStream(is);
        } catch (Exception e) {
            e.printStackTrace();
            return null;
        }
    }
}
