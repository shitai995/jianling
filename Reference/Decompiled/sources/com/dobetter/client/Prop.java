package com.dobetter.client;

import com.dobetter.client.data.PropData;

/* JADX INFO: loaded from: classes.dex */
public class Prop {
    public PropData data;
    public int number;

    public Prop(PropData dt, int num) {
        this.data = dt;
        this.number = num;
    }

    public String nameInfo() {
        return " <c=" + GameCanvas.COLORS[this.data.bType] + ">" + this.data.name + "</c>";
    }
}
