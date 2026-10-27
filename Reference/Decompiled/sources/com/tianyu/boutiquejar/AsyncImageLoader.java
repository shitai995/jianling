package com.tianyu.boutiquejar;

import android.graphics.drawable.Drawable;
import android.os.Handler;
import java.lang.ref.SoftReference;
import java.net.URL;
import java.util.HashMap;
import java.util.Map;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;

/* JADX INFO: loaded from: classes.dex */
public class AsyncImageLoader {
    public Map<String, SoftReference<Drawable>> imageCache = new HashMap();
    private ExecutorService executorService = Executors.newFixedThreadPool(5);
    private final Handler handler = new Handler();

    public interface ImageCallback {
        void imageLoaded(Drawable drawable);
    }

    public Drawable loadDrawable(final String imageUrl, final ImageCallback callback) {
        if (this.imageCache.containsKey(imageUrl)) {
            System.out.println("如果缓存过就从缓存中取出数据");
            SoftReference<Drawable> softReference = this.imageCache.get(imageUrl);
            if (softReference.get() != null) {
                return softReference.get();
            }
        }
        this.executorService.submit(new Runnable() { // from class: com.tianyu.boutiquejar.AsyncImageLoader.1
            @Override // java.lang.Runnable
            public void run() {
                try {
                    System.out.println("要把网络上的数据缓存到内存中");
                    final Drawable drawable = AsyncImageLoader.this.loadImageFromUrl(imageUrl);
                    AsyncImageLoader.this.imageCache.put(imageUrl, new SoftReference<>(drawable));
                    Handler handler = AsyncImageLoader.this.handler;
                    final ImageCallback imageCallback = callback;
                    handler.post(new Runnable() { // from class: com.tianyu.boutiquejar.AsyncImageLoader.1.1
                        @Override // java.lang.Runnable
                        public void run() {
                            imageCallback.imageLoaded(drawable);
                        }
                    });
                } catch (Exception e) {
                    throw new RuntimeException(e);
                }
            }
        });
        return null;
    }

    protected Drawable loadImageFromUrl(String imageUrl) {
        try {
            return Drawable.createFromStream(new URL(imageUrl).openStream(), "iamgeSync");
        } catch (Exception e) {
            throw new RuntimeException(e);
        }
    }
}
