package com.tianyu.domain;

import android.graphics.drawable.Drawable;

/* JADX INFO: loaded from: classes.dex */
public class Game {
    private String channelcode;
    private String downgameurl;
    private Drawable drawable;
    private String gamename;
    private String image;
    private String promotedesc;
    private String promotedowloaddesc;

    public Game() {
    }

    public String getDowngameurl() {
        return this.downgameurl;
    }

    public void setDowngameurl(String downgameurl) {
        this.downgameurl = downgameurl;
    }

    public String getImage() {
        return this.image;
    }

    public void setImage(String image) {
        this.image = image;
    }

    public Game(Drawable drawable, String gamename, String promotedesc, String channelcode, String promotedowloaddesc) {
        this.drawable = drawable;
        this.gamename = gamename;
        this.promotedesc = promotedesc;
        this.channelcode = channelcode;
        this.promotedowloaddesc = promotedowloaddesc;
    }

    public Drawable getDrawable() {
        return this.drawable;
    }

    public void setDrawable(Drawable drawable) {
        this.drawable = drawable;
    }

    public String getGamename() {
        return this.gamename;
    }

    public void setGamename(String gamename) {
        this.gamename = gamename;
    }

    public String getPromotedesc() {
        return this.promotedesc;
    }

    public void setPromotedesc(String promotedesc) {
        this.promotedesc = promotedesc;
    }

    public String getChannelcode() {
        return this.channelcode;
    }

    public void setChannelcode(String channelcode) {
        this.channelcode = channelcode;
    }

    public String getPromotedowloaddesc() {
        return this.promotedowloaddesc;
    }

    public void setPromotedowloaddesc(String promotedowloaddesc) {
        this.promotedowloaddesc = promotedowloaddesc;
    }
}
