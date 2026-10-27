package com.dobetter.client;

/* JADX INFO: loaded from: classes.dex */
public class Task {
    public int[] iEnemyID;
    public int[] iEnemyNum;
    public int[] iStageID;
    boolean isLoading;
    String taskOverScript;

    public Task(int[] stageID, int[] enemyID, int[] enemyNum, String script, boolean isLoading) {
        this.iStageID = stageID;
        this.iEnemyID = enemyID;
        this.iEnemyNum = enemyNum;
        this.taskOverScript = script;
        this.isLoading = isLoading;
    }
}
