package com.tianyu.service;

import android.os.Environment;
import android.support.v4.view.accessibility.AccessibilityEventCompat;
import com.tianyu.domain.Game;
import java.io.ByteArrayOutputStream;
import java.io.File;
import java.io.FileOutputStream;
import java.io.InputStream;
import java.net.HttpURLConnection;
import java.net.URL;
import java.util.ArrayList;
import java.util.List;
import org.json.JSONArray;
import org.json.JSONObject;

/* JADX INFO: loaded from: classes.dex */
public class GameService {
    public static File getBitmap(String path) throws Exception {
        URL url = new URL(path);
        System.out.println(path);
        HttpURLConnection conn = (HttpURLConnection) url.openConnection();
        conn.setConnectTimeout(5000);
        conn.setRequestMethod("GET");
        System.out.println("向服务器发送请求");
        if (conn.getResponseCode() == 200) {
            System.out.println("服务器成功响应");
            InputStream is = conn.getInputStream();
            String name = path.substring(path.lastIndexOf("/") + 1);
            File file = new File(Environment.getExternalStorageDirectory(), name);
            FileOutputStream fos = new FileOutputStream(file);
            byte[] buffer = new byte[AccessibilityEventCompat.TYPE_TOUCH_EXPLORATION_GESTURE_END];
            while (true) {
                int len = is.read(buffer);
                if (len == -1) {
                    break;
                }
                fos.write(buffer, 0, len);
            }
            fos.close();
            System.out.println("读取服务器上的图片资源完毕");
            System.out.println(file == null);
            return file;
        }
        return null;
    }

    public List<Game> getGamesFromJson(String path) throws Exception {
        List<Game> games = new ArrayList<>();
        System.out.println("listview获取服务器上的数据的路径为" + path);
        URL url = new URL(path);
        HttpURLConnection conn = (HttpURLConnection) url.openConnection();
        conn.setConnectTimeout(10000);
        conn.setRequestMethod("GET");
        if (conn.getResponseCode() == 200) {
            InputStream is = conn.getInputStream();
            ByteArrayOutputStream bos = new ByteArrayOutputStream();
            byte[] buffer = new byte[AccessibilityEventCompat.TYPE_TOUCH_EXPLORATION_GESTURE_END];
            while (true) {
                int len = is.read(buffer);
                if (len == -1) {
                    break;
                }
                bos.write(buffer, 0, len);
            }
            String json = bos.toString();
            bos.close();
            is.close();
            JSONArray jsonArray = new JSONArray(json);
            for (int i = 0; i < jsonArray.length(); i++) {
                JSONObject jsonObject = jsonArray.getJSONObject(i);
                Game game = new Game();
                game.setImage(jsonObject.getString("promotegamepicurl"));
                game.setGamename(jsonObject.getString("gamename"));
                game.setPromotedesc(jsonObject.getString("promotedesc"));
                game.setChannelcode("下载量：" + jsonObject.getString("channelcode"));
                game.setPromotedowloaddesc(jsonObject.getString("promotedowloaddesc"));
                game.setDowngameurl(jsonObject.getString("downloadurl"));
                System.out.println(jsonObject.getString("downloadurl"));
                games.add(game);
                System.out.println(game);
            }
        }
        return games;
    }

    public Game getGamesFromJson1(String path) throws Exception {
        Game game = new Game();
        URL url = new URL(path);
        System.out.println("banner部分json解析的地址为" + path);
        HttpURLConnection conn = (HttpURLConnection) url.openConnection();
        conn.setConnectTimeout(5000);
        conn.setRequestMethod("GET");
        if (conn.getResponseCode() == 200) {
            System.out.println("banner部分服务器访问正常");
            InputStream is = conn.getInputStream();
            ByteArrayOutputStream bos = new ByteArrayOutputStream();
            byte[] buffer = new byte[AccessibilityEventCompat.TYPE_TOUCH_EXPLORATION_GESTURE_END];
            while (true) {
                int len = is.read(buffer);
                if (len == -1) {
                    break;
                }
                bos.write(buffer, 0, len);
            }
            String json = bos.toString();
            bos.close();
            is.close();
            System.out.println("开始进行json解析");
            JSONArray jsonArray = new JSONArray(json);
            System.out.println("++++++++++++++++++++++");
            for (int i = 0; i < jsonArray.length(); i++) {
                System.out.println("json开始");
                JSONObject jsonObject = jsonArray.getJSONObject(i);
                game.setImage(jsonObject.getString("promotegamepicurl"));
                game.setGamename(jsonObject.getString("gamename"));
                game.setPromotedesc(jsonObject.getString("promotedesc"));
                game.setChannelcode("下载量：" + jsonObject.getString("channelcode"));
                game.setPromotedowloaddesc(jsonObject.getString("promotedowloaddesc"));
                game.setDowngameurl(jsonObject.getString("downloadurl"));
                System.out.println("获取到的downurl为" + jsonObject.getString("downloadurl"));
            }
        }
        return game;
    }
}
