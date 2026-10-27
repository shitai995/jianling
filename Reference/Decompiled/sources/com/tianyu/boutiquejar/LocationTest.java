package com.tianyu.boutiquejar;

import android.app.Activity;
import android.content.Intent;
import android.os.Bundle;
import android.view.View;
import android.widget.Button;

/* JADX INFO: loaded from: classes.dex */
public class LocationTest extends Activity {
    private Button bt;

    @Override // android.app.Activity
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(com.ty.jl.R.drawable.line);
        this.bt = (Button) findViewById(R.id.bt);
        this.bt.setOnClickListener(new View.OnClickListener() { // from class: com.tianyu.boutiquejar.LocationTest.1
            @Override // android.view.View.OnClickListener
            public void onClick(View v) {
                Intent intent = new Intent(LocationTest.this, (Class<?>) BoutiqueJarActivity.class);
                Bundle bundle = new Bundle();
                bundle.putString("channelcode", "1");
                bundle.putString("productid", "1");
                bundle.putInt("main_layout", com.ty.jl.R.drawable.boutique_jar);
                System.out.println(com.ty.jl.R.drawable.boutique_jar);
                bundle.putInt("b_item_layout", com.ty.jl.R.drawable.b_item);
                bundle.putInt("R_id_b_iv_icon", R.id.b_iv_icon);
                bundle.putInt("R_id_b_gamename", R.id.b_gamename);
                bundle.putInt("R_id_b_intro", R.id.b_intro);
                bundle.putInt("R_id_b_downinfo", R.id.b_downinfo);
                bundle.putInt("_gujian", R.id.b_lv_dandantang);
                bundle.putInt("_gujian", R.id.b_lv_gujian);
                bundle.putInt("_dandantang", R.id.b_lv_dandantang);
                bundle.putInt("_b_imageview", R.id.b_imageview);
                bundle.putInt("_b_listview", R.id.b_listview);
                bundle.putInt("_b_ic_launcher", com.ty.jl.R.drawable.ic_launcher);
                bundle.putInt("_b_banner", com.ty.jl.R.drawable.b_banner);
                intent.putExtras(bundle);
                LocationTest.this.startActivity(intent);
            }
        });
    }
}
