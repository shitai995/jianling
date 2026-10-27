package com.tencent.webnet;

import android.app.Activity;
import java.io.ByteArrayInputStream;
import java.io.ByteArrayOutputStream;
import java.io.File;
import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.FileOutputStream;
import java.io.ObjectInputStream;
import java.io.ObjectOutputStream;

/* JADX INFO: loaded from: classes.dex */
class d {
    d() {
    }

    protected static boolean a(String str, byte[] bArr) {
        if (str == null || str.length() <= 0 || bArr == null) {
            return false;
        }
        try {
            Activity activityAx = b.ax();
            if (activityAx != null) {
                File file = new File(activityAx.getFilesDir() + File.separator + str);
                if (!file.exists()) {
                    file.createNewFile();
                }
                FileOutputStream fileOutputStream = new FileOutputStream(file);
                fileOutputStream.write(bArr);
                fileOutputStream.close();
            }
            return true;
        } catch (Exception e) {
            if (b.cB) {
                l.a("FileStorage Write byte[] 写文件错误, 是否忘加WRITE_EXTERNAL_STORAGE权限! ", e);
                return false;
            }
            l.a("FileStorage Write byte[] error, uses permission WRITE_EXTERNAL_STORAGE", e);
            return false;
        }
    }

    protected static boolean a(String str, Object obj) {
        if (str == null || str.length() <= 0 || obj == null) {
            return false;
        }
        try {
            Activity activityAx = b.ax();
            if (activityAx != null) {
                ByteArrayOutputStream byteArrayOutputStream = new ByteArrayOutputStream();
                new ObjectOutputStream(byteArrayOutputStream).writeObject(obj);
                File file = new File(activityAx.getFilesDir() + File.separator + str);
                if (!file.exists()) {
                    file.createNewFile();
                }
                FileOutputStream fileOutputStream = new FileOutputStream(file);
                fileOutputStream.write(byteArrayOutputStream.toByteArray());
                fileOutputStream.close();
            }
            return true;
        } catch (Exception e) {
            if (b.cB) {
                l.a("FileStorage Write object 写文件错误, 是否忘加WRITE_EXTERNAL_STORAGE权限! ", e);
                return false;
            }
            l.a("FileStorage Write object error, uses permission WRITE_EXTERNAL_STORAGE", e);
            return false;
        }
    }

    protected static Object C(String str) {
        if (str == null || str.length() <= 0) {
            return null;
        }
        try {
            Activity activityAx = b.ax();
            if (activityAx == null || !new File(activityAx.getFilesDir() + File.separator + str).exists()) {
                return null;
            }
            FileInputStream fileInputStream = new FileInputStream(activityAx.getFilesDir() + File.separator + str);
            byte[] bArr = new byte[fileInputStream.available()];
            fileInputStream.read(bArr);
            fileInputStream.close();
            return new ObjectInputStream(new ByteArrayInputStream(bArr)).readObject();
        } catch (FileNotFoundException e) {
            return null;
        } catch (Exception e2) {
            if (b.cB) {
                l.a("FileStorage Read object 写文件错误, 是否忘加WRITE_EXTERNAL_STORAGE权限! ", e2);
                return null;
            }
            l.a("FileStorage Read object error, uses permission WRITE_EXTERNAL_STORAGE", e2);
            return null;
        }
    }

    protected static byte[] D(String str) {
        if (str == null || str.length() <= 0) {
            return null;
        }
        try {
            Activity activityAx = b.ax();
            if (activityAx == null || !new File(activityAx.getFilesDir() + File.separator + str).exists()) {
                return null;
            }
            FileInputStream fileInputStream = new FileInputStream(activityAx.getFilesDir() + File.separator + str);
            byte[] bArr = new byte[fileInputStream.available()];
            fileInputStream.read(bArr);
            fileInputStream.close();
            return bArr;
        } catch (FileNotFoundException e) {
            return null;
        } catch (Exception e2) {
            if (b.cB) {
                l.a("FileStorage Read byte[] 写文件错误, 是否忘加WRITE_EXTERNAL_STORAGE权限! ", e2);
                return null;
            }
            l.a("FileStorage Read byte[] error, uses permission WRITE_EXTERNAL_STORAGE", e2);
            return null;
        }
    }

    protected static boolean E(String str) {
        Activity activityAx;
        if (str == null || str.length() <= 0 || (activityAx = b.ax()) == null) {
            return false;
        }
        File file = new File(activityAx.getFilesDir() + File.separator + str);
        if (file.exists()) {
            return file.delete();
        }
        return false;
    }
}
