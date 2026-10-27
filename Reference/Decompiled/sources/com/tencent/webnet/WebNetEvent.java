package com.tencent.webnet;

/* JADX INFO: loaded from: classes.dex */
public interface WebNetEvent {
    public static final int BillingPoint_Event_Error = 1031;
    public static final int BillingPoint_Event_OK = 0;
    public static final int Error_Type_Connect = -104;
    public static final int Error_Type_Rec = -106;
    public static final int Error_Type_Send = -105;
    public static final int Error_Type_Sys = -107;
    public static final int FUNCTION_NA = -103;
    public static final int GAMEKEY_ERROR = -3;
    public static final int GetAchievement_Event_Error = 1021;
    public static final int GetAchievement_Event_OK = 1020;
    public static final int GetPrivateData_Event_Error = 1061;
    public static final int GetPrivateData_Event_OK = 1060;
    public static final int GetScore_Event_Error = 1011;
    public static final int GetScore_Event_OK = 1010;
    public static final int GotoWeb_Event_Error = 1001;
    public static final int GotoWeb_Event_OK = 1000;
    public static final int NET_NOT_AVAILABLE = -101;
    public static final int NOT_LOGIN = -102;
    public static final int SERVER_RET_NOT_LOGIN = -2;
    public static final int SavePrivateData_Event_Error = 1051;
    public static final int SavePrivateData_Event_Illegal_Data = 1052;
    public static final int SavePrivateData_Event_OK = 1050;
    public static final int SendSMS_Event_Error = 1041;
    public static final int SendSMS_Event_Generic_Failure = 1042;
    public static final int SendSMS_Event_INIT_ERROR = 1045;
    public static final int SendSMS_Event_NULL_PDU = 1044;
    public static final int SendSMS_Event_OK = 1040;
    public static final int SendSMS_Event_Radio_Off = 1043;
    public static final int SyncMicBlog_Error = 1074;
    public static final int SyncMicBlog_NOT_ID = 1073;
    public static final int SyncMicBlog_NOT_LOGIN = 1072;
    public static final int SyncMicBlog_OK = 1070;
    public static final int SyncMicBlog_SERVER_RET_IMAGE_ERR = 1071;

    boolean SendSMSCB(int i, String str);

    boolean SyncMicBlogCB(int i, String str);
}
