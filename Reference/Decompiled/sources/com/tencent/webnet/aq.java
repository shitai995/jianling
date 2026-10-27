package com.tencent.webnet;

import android.app.Activity;
import android.support.v4.view.accessibility.AccessibilityEventCompat;
import java.io.File;
import java.io.FileInputStream;
import java.io.FileOutputStream;
import java.io.InputStream;
import java.util.zip.ZipEntry;
import java.util.zip.ZipInputStream;

/* JADX INFO: loaded from: classes.dex */
class aq {
    aq() {
    }

    protected static boolean V(String str) {
        return new File(str).exists();
    }

    protected static boolean W(String str) {
        File file = new File(str);
        if (file.exists()) {
            return false;
        }
        try {
            return file.mkdirs();
        } catch (Exception e) {
            l.a("create path error ", e);
            return false;
        }
    }

    protected static boolean c(String str, String str2) {
        File file = new File(str2);
        if (file.exists()) {
            return false;
        }
        try {
            return W(str) && file.createNewFile();
        } catch (Exception e) {
            l.a("create file error ", e);
            return false;
        }
    }

    protected static boolean X(String str) {
        File file = new File(str);
        if (file.exists()) {
            return false;
        }
        try {
            return file.createNewFile();
        } catch (Exception e) {
            l.a("create file error ", e);
            return false;
        }
    }

    protected static void a(Activity activity, String str, String str2) {
        File file = new File(str);
        if (activity != null && file.exists()) {
            try {
                a(new FileInputStream(str), str2);
            } catch (Exception e) {
                if (b.cB) {
                    l.a("UnZip 文件操作失败是否没有申明WRITE_EXTERNAL_STORAGE权限 ", e);
                } else {
                    l.a("UnZip error, uses permission WRITE_EXTERNAL_STORAGE", e);
                }
            }
        }
    }

    protected static void a(InputStream inputStream, String str) {
        try {
            File file = new File(str);
            if (!file.exists()) {
                file.mkdirs();
            }
            ZipInputStream zipInputStream = new ZipInputStream(inputStream);
            for (ZipEntry nextEntry = zipInputStream.getNextEntry(); nextEntry != null; nextEntry = zipInputStream.getNextEntry()) {
                if (nextEntry.isDirectory()) {
                    String name = nextEntry.getName();
                    new File(String.valueOf(str) + File.separator + name.substring(0, name.length() - 1)).mkdirs();
                } else {
                    File file2 = new File(String.valueOf(str) + File.separator + nextEntry.getName());
                    file2.createNewFile();
                    FileOutputStream fileOutputStream = new FileOutputStream(file2);
                    byte[] bArr = new byte[AccessibilityEventCompat.TYPE_TOUCH_EXPLORATION_GESTURE_END];
                    while (true) {
                        int i = zipInputStream.read(bArr);
                        if (i <= 0) {
                            break;
                        } else {
                            fileOutputStream.write(bArr, 0, i);
                        }
                    }
                    fileOutputStream.close();
                }
            }
            zipInputStream.close();
        } catch (Exception e) {
            if (b.cB) {
                l.a("UnZip2 文件操作失败是否没有申明WRITE_EXTERNAL_STORAGE权限 ", e);
            } else {
                l.a("UnZip2 error, uses permission WRITE_EXTERNAL_STORAGE", e);
            }
        }
    }

    protected static void Y(String str) {
        File file;
        if (str != null && (file = new File(str)) != null && file.exists()) {
            if (file.isDirectory()) {
                for (File file2 : file.listFiles()) {
                    Y(file2.toString());
                }
            }
            file.delete();
        }
    }

    protected static void Z(String str) {
        File file;
        if (str != null && (file = new File(str)) != null && file.exists() && file.isDirectory()) {
            File[] fileArrListFiles = file.listFiles();
            for (File file2 : fileArrListFiles) {
                Y(file2.toString());
            }
        }
    }

    protected static void d(String str, String str2) {
        if (str != null && str2 != null) {
            File file = new File(str);
            if (file.exists()) {
                file.renameTo(new File(str2));
            }
        }
    }

    protected static boolean a(String str, String str2, boolean z) {
        if (str == null || str2 == null || !new File(str).exists()) {
            return false;
        }
        try {
            File file = new File(str2);
            if (!z && file.exists()) {
                return false;
            }
            if (!file.exists() && !file.createNewFile()) {
                return false;
            }
            FileInputStream fileInputStream = new FileInputStream(file);
            byte[] bArr = new byte[fileInputStream.available()];
            fileInputStream.read(bArr);
            fileInputStream.close();
            FileOutputStream fileOutputStream = new FileOutputStream(file);
            fileOutputStream.write(bArr);
            fileOutputStream.close();
            return true;
        } catch (Exception e) {
            if (b.cB) {
                l.a("copyFile 文件操作失败是否没有申明WRITE_EXTERNAL_STORAGE权限 ", e);
                return false;
            }
            l.a("copyFile error, uses permission WRITE_EXTERNAL_STORAGE", e);
            return false;
        }
    }

    protected static boolean a(InputStream inputStream, String str, boolean z) {
        if (inputStream == null || str == null) {
            return false;
        }
        try {
            File file = new File(str);
            if (!z && file.exists()) {
                return false;
            }
            if (!file.exists() && !file.createNewFile()) {
                return false;
            }
            byte[] bArr = new byte[inputStream.available()];
            inputStream.read(bArr);
            inputStream.close();
            FileOutputStream fileOutputStream = new FileOutputStream(file);
            fileOutputStream.write(bArr);
            fileOutputStream.close();
            return true;
        } catch (Exception e) {
            if (b.cB) {
                l.a("copyFile2 文件操作失败是否没有申明WRITE_EXTERNAL_STORAGE权限 ", e);
                return false;
            }
            l.a("copyFile2 error, uses permission WRITE_EXTERNAL_STORAGE", e);
            return false;
        }
    }

    protected static boolean a(byte[] bArr, String str, boolean z) {
        if (bArr == null || str == null) {
            return false;
        }
        try {
            File file = new File(str);
            if (!z && file.exists()) {
                return false;
            }
            if (!file.exists() && !file.createNewFile()) {
                return false;
            }
            FileOutputStream fileOutputStream = new FileOutputStream(file);
            fileOutputStream.write(bArr);
            fileOutputStream.close();
            return true;
        } catch (Exception e) {
            if (b.cB) {
                l.a("copyFile3 文件操作失败是否没有申明WRITE_EXTERNAL_STORAGE权限 ", e);
                return false;
            }
            l.a("copyFile3 error, uses permission WRITE_EXTERNAL_STORAGE", e);
            return false;
        }
    }
}
