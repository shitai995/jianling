package com.dobetter.client;

import android.telephony.SmsManager;

/* JADX INFO: loaded from: classes.dex */
public class SMS {
    private String content;
    private boolean isOver;
    private boolean isSuccessful;
    private String number;
    private SmsManager smsManager = SmsManager.getDefault();

    /* JADX WARN: Type inference failed for: r0v1, types: [com.dobetter.client.SMS$1] */
    public void sendMessage(String content, String number) {
        this.content = content;
        this.number = number;
        this.isSuccessful = false;
        this.isOver = false;
        new Thread() { // from class: com.dobetter.client.SMS.1
            @Override // java.lang.Thread, java.lang.Runnable
            public void run() {
                try {
                    SMS.this.smsManager.sendTextMessage(SMS.this.number, null, SMS.this.content, null, null);
                    SMS.this.isSuccessful = true;
                } catch (Exception e) {
                    SMS.this.isSuccessful = false;
                }
                SMS.this.isOver = true;
            }
        }.start();
    }

    public boolean isSuccessful() {
        return this.isSuccessful;
    }

    public boolean isOver() {
        return this.isOver;
    }
}
