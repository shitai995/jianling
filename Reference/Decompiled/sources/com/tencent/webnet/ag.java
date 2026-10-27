package com.tencent.webnet;

import android.graphics.Bitmap;
import android.view.View;
import java.io.ByteArrayOutputStream;
import java.io.File;

/* JADX INFO: loaded from: classes.dex */
class ag implements View.OnClickListener {
    final /* synthetic */ WebNetMain gB;

    ag(WebNetMain webNetMain) {
        this.gB = webNetMain;
    }

    @Override // android.view.View.OnClickListener
    public void onClick(View view) {
        Bitmap bitmapBx = this.gB.bx();
        if (bitmapBx != null) {
            ByteArrayOutputStream byteArrayOutputStream = new ByteArrayOutputStream();
            bitmapBx.compress(Bitmap.CompressFormat.JPEG, 80, byteArrayOutputStream);
            if (d.a("screenshots.jpeg", byteArrayOutputStream.toByteArray())) {
                this.gB.gr.stopLoading();
                o.c(true);
                this.gB.gr.loadUrl(String.valueOf(this.gB.gs) + "screenCapture.html?file=file:///" + this.gB.getFilesDir() + File.separator + "screenshots.jpeg");
            }
        }
    }
}
