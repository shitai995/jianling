package com.dobetter.client;

import com.dobetter.client.data.SkillData;
import java.io.DataInputStream;
import java.io.DataOutputStream;

/* JADX INFO: loaded from: classes.dex */
public class Skill {
    public byte bLevel;
    public byte bSkillType;
    public SkillData data;
    public String name;
    public short sDamagePoint;

    public Skill(SkillData data) {
        this.data = data;
        this.name = data.name;
        this.bLevel = (byte) 1;
    }

    public Skill(SkillData data, int equipLevel) {
        this(data);
        this.name = data.name;
        int temp = ((equipLevel - data.bRank) / 10) + 1;
        if (temp > 9) {
            temp = 9;
        } else if (temp < 1) {
            temp = 1;
        }
        this.bLevel = (byte) temp;
    }

    public String getSkillInfo() {
        if (this.bSkillType != 0) {
            return null;
        }
        String description = this.data.description;
        return description;
    }

    public void saveSkill(DataOutputStream dos) {
        try {
            dos.writeByte(this.data.bID);
            dos.writeByte(this.bLevel);
        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    public static Skill readSkill(DataInputStream dis) {
        Skill skill = null;
        try {
            byte id = dis.readByte();
            byte level = dis.readByte();
            Skill skill2 = new Skill(GameCanvas.skillDatas[id]);
            try {
                skill2.bLevel = level;
                return skill2;
            } catch (Exception e) {
                e = e;
                skill = skill2;
                e.printStackTrace();
                return skill;
            }
        } catch (Exception e2) {
            e = e2;
        }
    }
}
