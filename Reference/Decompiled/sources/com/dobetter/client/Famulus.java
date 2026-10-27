package com.dobetter.client;

import com.dobetter.common.Graphics;
import com.dobetter.common.Tool;
import java.io.DataInputStream;
import java.io.DataOutputStream;

/* JADX INFO: loaded from: classes.dex */
public class Famulus extends FightSprite {
    public static final byte ACT_EAT = 5;
    public static final byte ACT_INTEGRATE = 4;
    public static final byte ACT_INTEGRATE_OVER = 5;
    public static final byte ACT_SCRAM = 2;
    public static final byte ACT_USING_PROP = 3;
    static final int ATTACK = 2;
    static final int CRITICAL = 6;
    static final int DEFENSE = 3;
    static final int DODGE = 5;
    static final int DURABILITY = 1;
    static final int HEALTH = 0;
    static final int HIT = 4;
    byte bDurabilityCounter;
    int iCurDurability;
    public int iFinalAP;
    public int iFinalCriticalPoint;
    public int iFinalDP;
    public int iFinalDodgePoint;
    public int iFinalDurability;
    public int iFinalHP;
    public int iFinalHitPoint;
    int iMaxDurability;
    int iPreAP;
    int iPreCriticalPoint;
    int iPreDP;
    int iPreDodgePoint;
    int iPreHitPoint;
    int iPreMaxDurability;
    int iPreMaxHP;
    Role owner;
    Skill skill;
    public int[] iReinforceAttribute = new int[7];
    final short sMaxLevel = 40;

    public Famulus(Role owner, short level) {
        this.owner = owner;
        this.ID = -2;
        this.iLayerIndex = 2;
        this.bType = (byte) 14;
        this.sLevel = level;
        loadRes();
        this.iMoveSpeed = 6;
        getLevelUpExp();
        this.skill = new Skill(GameCanvas.skillDatas[13]);
    }

    public void loadRes() {
        loadRes(GameCanvas.instance.famulusBaseData);
        initAct();
    }

    public void initData() {
        for (int i = 0; i < GameCanvas.famulusDatas.length; i++) {
            if (this.sLevel >= GameCanvas.famulusDatas[i].bLowerLimitLevel && this.sLevel <= GameCanvas.famulusDatas[i].bUpperLimitLevel) {
                this.attributeData = GameCanvas.famulusDatas[i];
                break;
            }
        }
        int temp1 = this.iCurDurability;
        int temp2 = this.iFinalDurability;
        this.iMaxHP = this.attributeData.iHP + ((this.sLevel - this.attributeData.bLowerLimitLevel) * this.attributeData.iUpgradeHP);
        this.iMaxDurability = (short) (this.attributeData.sDurability + ((this.sLevel - this.attributeData.bLowerLimitLevel) * this.attributeData.sUpgradeDurability));
        this.iAP = this.attributeData.sAP + ((this.sLevel - this.attributeData.bLowerLimitLevel) * this.attributeData.sUpgradeAP);
        this.iDP = this.attributeData.sDP + ((this.sLevel - this.attributeData.bLowerLimitLevel) * this.attributeData.sUpgradeDP);
        this.iHitPoint = this.attributeData.sHitPoint + ((this.sLevel - this.attributeData.bLowerLimitLevel) * this.attributeData.sUpgradeHit);
        this.iDodgePoint = this.attributeData.sDodgePoint + ((this.sLevel - this.attributeData.bLowerLimitLevel) * this.attributeData.sUpgradeDodge);
        this.iCriticalPoint = this.attributeData.sCriticalPoint + ((this.sLevel - this.attributeData.bLowerLimitLevel) * this.attributeData.sUpgradeCritical);
        this.iFinalHP = this.iMaxHP + this.iReinforceAttribute[0];
        this.iFinalDurability = this.iMaxDurability + this.iReinforceAttribute[1];
        this.iFinalAP = this.iAP + this.iReinforceAttribute[2];
        this.iFinalDP = this.iDP + this.iReinforceAttribute[3];
        this.iFinalHitPoint = this.iHitPoint + this.iReinforceAttribute[4];
        this.iFinalDodgePoint = this.iDodgePoint + this.iReinforceAttribute[5];
        this.iFinalCriticalPoint = this.iCriticalPoint + this.iReinforceAttribute[6];
        if (temp2 > 0) {
            this.iCurDurability = (this.iFinalDurability * temp1) / temp2;
        } else {
            this.iCurDurability = 0;
        }
        this.owner.swordSoulStr = new String[1];
        if (this.sLevel < 11) {
            this.owner.swordSoulStr[0] = "w_97";
        } else if (this.sLevel < 21) {
            this.owner.swordSoulStr[0] = "w_98";
        } else if (this.sLevel < 31) {
            this.owner.swordSoulStr[0] = "w_99";
        } else {
            this.owner.swordSoulStr[0] = "w_00";
        }
        this.owner.reload(this.owner.swordSoulStr, 1);
    }

    public void updateAttributeByEatEquip(Equip equip) {
        int[] iArr = this.iReinforceAttribute;
        iArr[0] = iArr[0] + (equip.iFinalHP / 50);
        int temp1 = this.iCurDurability;
        int temp2 = this.iFinalDurability;
        int[] iArr2 = this.iReinforceAttribute;
        iArr2[1] = iArr2[1] + (equip.iCurDurability / 50);
        int[] iArr3 = this.iReinforceAttribute;
        iArr3[2] = iArr3[2] + (equip.iFinalAP / 50);
        int[] iArr4 = this.iReinforceAttribute;
        iArr4[3] = iArr4[3] + (equip.iFinalDP / 200);
        int[] iArr5 = this.iReinforceAttribute;
        iArr5[4] = iArr5[4] + (equip.iHitPoint / 20);
        int[] iArr6 = this.iReinforceAttribute;
        iArr6[5] = iArr6[5] + (equip.iDodgePoint / 20);
        int[] iArr7 = this.iReinforceAttribute;
        iArr7[6] = iArr7[6] + (equip.iCriticalPoint / 20);
        if (equip.bReinforcedLevel == 9) {
            for (int j = 0; j < equip.data.bAddtiveEffectType.length; j++) {
                int type = equip.data.bAddtiveEffectType[j];
                short s = equip.data.sAddtiveEffectPoint[j];
                switch (type) {
                    case 0:
                        int[] iArr8 = this.iReinforceAttribute;
                        iArr8[0] = iArr8[0] + s;
                        break;
                    case 1:
                        int[] iArr9 = this.iReinforceAttribute;
                        iArr9[1] = iArr9[1] + s;
                        break;
                    case 2:
                        int[] iArr10 = this.iReinforceAttribute;
                        iArr10[2] = iArr10[2] + s;
                        break;
                    case 3:
                        int[] iArr11 = this.iReinforceAttribute;
                        iArr11[3] = iArr11[3] + s;
                        break;
                    case 4:
                        int[] iArr12 = this.iReinforceAttribute;
                        iArr12[4] = iArr12[4] + s;
                        break;
                    case 5:
                        int[] iArr13 = this.iReinforceAttribute;
                        iArr13[5] = iArr13[5] + s;
                        break;
                    case 6:
                        int[] iArr14 = this.iReinforceAttribute;
                        iArr14[6] = iArr14[6] + s;
                        break;
                }
            }
        }
        this.iFinalHP = this.iMaxHP + this.iReinforceAttribute[0];
        this.iFinalDurability = this.iMaxDurability + this.iReinforceAttribute[1];
        this.iFinalAP = this.iAP + this.iReinforceAttribute[2];
        this.iFinalDP = this.iDP + this.iReinforceAttribute[3];
        this.iFinalHitPoint = this.iHitPoint + this.iReinforceAttribute[4];
        this.iFinalDodgePoint = this.iDodgePoint + this.iReinforceAttribute[5];
        this.iFinalCriticalPoint = this.iCriticalPoint + this.iReinforceAttribute[6];
        this.iCurDurability = (this.iFinalDurability * temp1) / temp2;
        if (this.owner.isIntegrate) {
            this.owner.integrate(false);
        }
    }

    public void addExp(int exp) {
        if (this.sLevel < 40) {
            this.iCurrentExp += exp;
        }
        while (this.iCurrentExp >= this.iLevelUpExp && isLevelUpContinue()) {
        }
    }

    public boolean isLevelUpContinue() {
        if (this.sLevel >= 40) {
            return false;
        }
        this.iCurrentExp -= this.iLevelUpExp;
        this.sLevel = (short) (this.sLevel + 1);
        initData();
        getLevelUpExp();
        if (this.sLevel == 40) {
            this.iCurrentExp = this.iLevelUpExp;
        }
        return true;
    }

    @Override // com.dobetter.client.FightSprite, com.dobetter.client.Sprite
    public void cycle() {
        if (this.owner.bActID < 22 || this.owner.bActID >= 26) {
            if (this.owner.isIntegrate) {
                if (!GameCanvas.instance.isDurabilityNonLoss && !GameCanvas.instance.isDurabilityNonLossForever) {
                    if (this.bDurabilityCounter < 10) {
                        this.bDurabilityCounter = (byte) (this.bDurabilityCounter + 1);
                        return;
                    }
                    this.bDurabilityCounter = (byte) 0;
                    if (this.iCurDurability > this.iFinalDurability / 100) {
                        this.iCurDurability -= this.iFinalDurability / 100;
                        return;
                    }
                    this.iCurDurability = 0;
                    this.owner.act((byte) 26);
                    transfer();
                    act((byte) 5);
                    return;
                }
                return;
            }
            if (this.iCurDurability < this.iFinalDurability) {
                if (this.bDurabilityCounter < 10) {
                    this.bDurabilityCounter = (byte) (this.bDurabilityCounter + 1);
                } else {
                    this.bDurabilityCounter = (byte) 0;
                    if (this.iCurDurability <= this.iFinalDurability - (this.iFinalDurability / 100)) {
                        this.iCurDurability += this.iFinalDurability / 100;
                    } else {
                        this.iCurDurability = this.iFinalDurability;
                    }
                }
            }
            super.cycle();
            cycleAction();
        }
    }

    @Override // com.dobetter.client.FightSprite, com.dobetter.client.Sprite
    public void act(byte act) {
        if (this.bActID == act && this.bActState == 0) {
            getCurVxVy();
            return;
        }
        this.bActID = act;
        this.iCurFrameIndex = 0;
        switch (this.bActID) {
            case 0:
            case 1:
                this.bActState = (byte) 0;
                break;
            default:
                this.bActState = (byte) 4;
                break;
        }
        getCurActionData();
    }

    @Override // com.dobetter.client.FightSprite, com.dobetter.client.Sprite
    public void getCurActionData() {
        this.iCurActionID = this.bActID;
        getCurData();
        getCurVxVy();
    }

    public void cycleAction() {
        if (!this.isSpriteAnimate) {
            if (this.isKeepFrame) {
                getCurData();
            }
            if (this.iCurFrameIndex < this.curAction.frameNum - 1) {
                getCurActionData();
                this.iCurFrameIndex++;
                return;
            }
            switch (this.bActID) {
                case 0:
                case 1:
                    getCurActionData();
                    this.iCurFrameIndex = 0;
                    break;
                case 2:
                    act((byte) 0);
                    break;
                case 3:
                    if (!this.owner.isDead() && GameCanvas.instance.medicine != null) {
                        GameCanvas.instance.useProp(GameCanvas.instance.medicine);
                    }
                    initAct();
                    break;
                case 4:
                    break;
                default:
                    initAct();
                    break;
            }
        }
    }

    public void processMove(int targetX, int targetY, int offX, int offY1, int offY2) {
        int desPosX;
        int desPosY;
        if (this.bActState == 0) {
            int fcx = this.iMapX + (this.iFootWidth >> 1);
            int fcy = this.iMapY + this.iFootHeight;
            if (fcx < targetX - offX) {
                desPosX = targetX - offX;
            } else if (fcx > targetX + offX) {
                desPosX = targetX + offX;
            } else {
                desPosX = fcx;
            }
            if (fcy < targetY - offY2) {
                desPosY = targetY - offY2;
            } else if (fcy > targetY + offY1) {
                desPosY = targetY + offY1;
            } else {
                desPosY = fcy;
            }
            int temp = Tool.getNextRnd(0, 2);
            if (temp == 0) {
                if (Math.abs(fcx - desPosX) > Math.abs(this.iCurVx)) {
                    if (fcx > desPosX) {
                        setDir((byte) 0);
                    } else {
                        setDir((byte) 1);
                    }
                    act((byte) 1);
                } else if (Math.abs(fcy - desPosY) > this.iMoveSpeed) {
                    if (fcy > desPosY) {
                        this.iCurVx = 0;
                        setDir((byte) 2);
                    } else {
                        setDir((byte) 3);
                    }
                    act((byte) 1);
                } else if (this.bActID == 1) {
                    act((byte) 2);
                }
            } else if (Math.abs(fcy - desPosY) > this.iMoveSpeed) {
                if (fcy > desPosY) {
                    setDir((byte) 2);
                } else {
                    setDir((byte) 3);
                }
                act((byte) 1);
            } else if (Math.abs(fcx - desPosX) > Math.abs(this.iCurVx)) {
                if (fcx > desPosX) {
                    setDir((byte) 0);
                } else {
                    setDir((byte) 1);
                }
                act((byte) 1);
            } else if (this.bActID == 1) {
                act((byte) 2);
            }
            GameCanvas.instance.moveSpriteByStep(this, 11, 5);
        }
    }

    public void transfer() {
        if (!this.isLocked) {
            this.iMapX = this.owner.iMapX;
            this.iMapY = this.owner.iMapY;
        }
    }

    @Override // com.dobetter.client.FightSprite, com.dobetter.client.Sprite
    public void draw(Graphics g, int viewMapX, int viewMapY, int dx, int dy) {
        if (this.actionSet != null && !this.owner.isIntegrate) {
            int drawX = (this.iMapX - viewMapX) + dx;
            int drawY = (this.iMapY - viewMapY) + dy;
            try {
                this.actionSet.drawFrame(g, this.curFrame, drawX, drawY, this.isFaceToTheRight);
            } catch (Exception e) {
                e.printStackTrace();
                System.out.println("ID=" + this.ID);
            }
            drawFace(g, drawX, drawY);
        }
    }

    public static Famulus readFamulus(Role role, DataInputStream dis) {
        Famulus famulus = null;
        try {
            short level = dis.readShort();
            Famulus famulus2 = new Famulus(role, level);
            try {
                famulus2.iCurrentExp = dis.readInt();
                famulus2.iLevelUpExp = dis.readInt();
                famulus2.iMapX = dis.readInt();
                famulus2.iMapY = dis.readInt();
                famulus2.setDir(dis.readByte());
                for (int i = 0; i < famulus2.iReinforceAttribute.length; i++) {
                    famulus2.iReinforceAttribute[i] = dis.readInt();
                }
                famulus2.initData();
                famulus2.iCurDurability = dis.readInt();
                if (!dis.readBoolean()) {
                    return famulus2;
                }
                famulus2.skill = Skill.readSkill(dis);
                return famulus2;
            } catch (Exception e) {
                e = e;
                famulus = famulus2;
                e.printStackTrace();
                return famulus;
            }
        } catch (Exception e2) {
            e = e2;
        }
    }

    public void saveFamulus(DataOutputStream dos) {
        try {
            dos.writeShort(this.sLevel);
            dos.writeInt(this.iCurrentExp);
            dos.writeInt(this.iLevelUpExp);
            dos.writeInt(this.iMapX);
            dos.writeInt(this.iMapY);
            dos.writeByte(this.bCurDir);
            for (int i = 0; i < this.iReinforceAttribute.length; i++) {
                dos.writeInt(this.iReinforceAttribute[i]);
            }
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

    @Override // com.dobetter.client.FightSprite, com.dobetter.client.Sprite
    public void freeSprite() {
        if (this.actionSet != null) {
            this.actionSet.releasImg();
            this.actionSet = null;
        }
    }
}
