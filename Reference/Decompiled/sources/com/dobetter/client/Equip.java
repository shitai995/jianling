package com.dobetter.client;

import com.dobetter.client.data.EquipData;
import com.dobetter.client.data.SkillData;
import com.dobetter.common.Tool;
import java.io.DataInputStream;
import java.io.DataOutputStream;
import java.util.Vector;

/* JADX INFO: loaded from: classes.dex */
public class Equip {
    public byte[] bOdds;
    public byte bReinforcedLevel;
    EquipData data;
    public int iAP;
    public int iCriticalPoint;
    public int iCurDurability;
    public int iDP;
    public int iDodgePoint;
    public int iDurability;
    public int iFinalAP;
    public int iFinalDP;
    public int iFinalDurability;
    public int iFinalHP;
    public int iHP;
    public int iHitPoint;
    public int iPreAP;
    public int iPreDP;
    public int iPreDurability;
    public int iPreMaxHP;
    public int iReinforceAP;
    public int iReinforceDP;
    public int iReinforceDurability;
    public int iReinforceHP;
    public int iReinforceMoney = 0;
    boolean isReinforceSuccess;
    Skill skill;

    public Equip(EquipData data, int reinforceLevel, boolean isCreateSkill) {
        this.data = data;
        init(reinforceLevel);
        if (isCreateSkill) {
            this.skill = getSkill();
        }
    }

    void init(int reinforceLevel) {
        this.iReinforceMoney = (this.data.iPurchasePrice * 10) / 100;
        this.bOdds = new byte[9];
        for (int i = 0; i < this.bOdds.length; i++) {
            this.bOdds[i] = 100;
        }
        this.iAP = this.data.sAddAP;
        this.iDP = this.data.sAddDP;
        this.iHP = this.data.sAddHP;
        this.iDurability = this.data.sDurability;
        this.iHitPoint = this.data.sAddHitP;
        this.iDodgePoint = this.data.sAddDodgeP;
        this.iCriticalPoint = this.data.sAddCriticalP;
        this.bReinforcedLevel = (byte) reinforceLevel;
        updateAttribute(true);
    }

    Skill getSkill() {
        if (this.data.bSkillIndex >= 0) {
            Skill skill = new Skill(GameCanvas.skillDatas[this.data.bSkillIndex], this.data.bLevel);
            return skill;
        }
        if (this.data.bSkillIndex != -1) {
            return null;
        }
        Vector skillDataV = new Vector();
        for (int i = 5; i < GameCanvas.skillDatas.length - 3; i++) {
            if (GameCanvas.skillDatas[i].bRank <= this.data.bLevel) {
                skillDataV.addElement(GameCanvas.skillDatas[i]);
            }
        }
        if (skillDataV.size() <= 0) {
            return null;
        }
        int rnd = Tool.getNextRnd(0, skillDataV.size());
        SkillData skillData = (SkillData) skillDataV.elementAt(rnd);
        Skill skill2 = new Skill(skillData, this.data.bLevel);
        return skill2;
    }

    public String nameInfo() {
        return " <c=" + GameCanvas.COLORS[this.data.bGrade] + ">" + this.data.name + "</c>";
    }

    public String getDescription() {
        StringBuffer sb = new StringBuffer(this.data.description);
        sb.append("\n装备后");
        if (this.iFinalAP > 0) {
            sb.append("攻击增加" + this.iFinalAP);
        }
        if (this.iFinalDP > 0) {
            sb.append("防御增加" + this.iFinalDP);
        }
        if (this.iFinalHP > 0) {
            sb.append(",气血增加" + this.iFinalHP);
        }
        if (this.iFinalDurability > 0) {
            sb.append(",耐久增加" + this.iFinalDurability);
        }
        if (this.iDodgePoint > 0) {
            sb.append(",闪避增加" + (this.iDodgePoint / 100));
        }
        if (this.iHitPoint > 0) {
            sb.append(",命中增加" + (this.iHitPoint / 100));
        }
        if (this.iCriticalPoint > 0) {
            sb.append(",暴击增加" + (this.iCriticalPoint / 100));
        }
        String description = sb.toString();
        return description;
    }

    public void reinforce() {
        this.iPreMaxHP = this.iFinalHP;
        this.iPreDurability = this.iFinalDurability;
        this.iPreAP = this.iFinalAP;
        this.iPreDP = this.iFinalDP;
        if (Tool.getNextRnd(0, 100) < 100) {
            this.bReinforcedLevel = (byte) (this.bReinforcedLevel + 1);
            if (this.skill != null && ((this.bReinforcedLevel == 5 || this.bReinforcedLevel == 9) && this.skill.bLevel < 9)) {
                Skill skill = this.skill;
                skill.bLevel = (byte) (skill.bLevel + 1);
            }
            this.isReinforceSuccess = true;
        } else {
            if (this.bReinforcedLevel == 8) {
                this.bReinforcedLevel = (byte) (this.bReinforcedLevel - 2);
            } else {
                this.bReinforcedLevel = (byte) (this.bReinforcedLevel - 1);
            }
            this.isReinforceSuccess = false;
        }
        updateAttribute(false);
    }

    public void reinforceLevelDown() {
    }

    public void reinforcedlevelUp() {
    }

    void updateAttribute(boolean isInit) {
        if (this.bReinforcedLevel < 9) {
            this.iReinforceHP = ((this.iHP * this.bReinforcedLevel) * 10) / 100;
            this.iReinforceDurability = ((this.iDurability * this.bReinforcedLevel) * 10) / 100;
            this.iReinforceAP = ((this.iAP * this.bReinforcedLevel) * 10) / 100;
            this.iReinforceDP = ((this.iDP * this.bReinforcedLevel) * 10) / 100;
        } else {
            this.iReinforceHP = this.iHP;
            this.iReinforceDurability = this.iDurability;
            this.iReinforceAP = this.iAP;
            this.iReinforceDP = this.iDP;
        }
        int temp1 = this.iCurDurability;
        int temp2 = this.iFinalDurability;
        this.iFinalHP = this.iHP + this.iReinforceHP;
        this.iFinalDurability = this.iDurability + this.iReinforceDurability;
        this.iFinalAP = this.iAP + this.iReinforceAP;
        this.iFinalDP = this.iDP + this.iReinforceDP;
        if (!isInit) {
            this.iCurDurability = (this.iFinalDurability * temp1) / temp2;
        } else {
            this.iCurDurability = this.iFinalDurability;
        }
    }

    static Equip readEquipData(DataInputStream dis) {
        Equip equip = null;
        try {
            byte id = dis.readByte();
            byte reinforcedLevel = dis.readByte();
            Equip equip2 = new Equip(GameCanvas.equipDatas[id], reinforcedLevel, false);
            try {
                equip2.updateAttribute(false);
                equip2.iCurDurability = dis.readInt();
                if (!dis.readBoolean()) {
                    return equip2;
                }
                equip2.skill = Skill.readSkill(dis);
                return equip2;
            } catch (Exception e) {
                e = e;
                equip = equip2;
                e.printStackTrace();
                return equip;
            }
        } catch (Exception e2) {
            e = e2;
        }
    }

    void saveEquipData(DataOutputStream dos) {
        try {
            dos.writeByte(this.data.bID);
            dos.writeByte(this.bReinforcedLevel);
            dos.writeInt(this.iCurDurability);
            if (this.skill != null) {
                dos.writeBoolean(true);
                this.skill.saveSkill(dos);
            } else {
                dos.writeBoolean(false);
            }
        } catch (Exception e) {
            e.printStackTrace();
        }
    }
}
