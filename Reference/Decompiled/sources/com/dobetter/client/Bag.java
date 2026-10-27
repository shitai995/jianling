package com.dobetter.client;

import com.dobetter.client.data.EquipData;
import com.dobetter.client.data.PropData;
import java.io.DataInputStream;
import java.io.DataOutputStream;
import java.util.Vector;

/* JADX INFO: loaded from: classes.dex */
public class Bag {
    public int iMoney = 0;
    public final short sMoneyChangeCofficient = 10000;
    public int cellCount = 40;
    public int MAX = 99;
    public Vector propV = new Vector();
    public Vector equipV = new Vector();

    public boolean addProp(PropData propData, int num) {
        int temp = num;
        for (int i = 0; i < this.propV.size(); i++) {
            if (this.propV.elementAt(i) instanceof Prop) {
                Prop bagProp = (Prop) this.propV.elementAt(i);
                if (bagProp.data.bID != propData.bID) {
                    continue;
                } else {
                    if (bagProp.number <= this.MAX - num) {
                        bagProp.number += num;
                        return true;
                    }
                    temp = (bagProp.number + num) - this.MAX;
                    bagProp.number = this.MAX;
                }
            }
        }
        while (temp > this.MAX) {
            this.propV.addElement(new Prop(propData, this.MAX));
            temp -= this.MAX;
        }
        if (temp > 0) {
            this.propV.addElement(new Prop(propData, temp));
            return true;
        }
        return false;
    }

    public void deleteProp(Prop bagProp) {
        this.propV.removeElement(bagProp);
    }

    public void deleteProp(Prop bagProp, int num) {
        bagProp.number -= num;
        if (bagProp.number <= 0) {
            this.propV.removeElement(bagProp);
        }
    }

    public void deleteProp(int propID, int num) {
        int temp = num;
        while (temp > 0) {
            for (int i = 0; i < this.propV.size(); i++) {
                if (this.propV.elementAt(i) instanceof Prop) {
                    Prop bagProp = (Prop) this.propV.elementAt(i);
                    if (bagProp.data.bID != propID) {
                        continue;
                    } else {
                        if (bagProp.number >= temp) {
                            bagProp.number -= temp;
                            temp = 0;
                            if (bagProp.number != 0) {
                                break;
                            }
                            this.propV.removeElement(bagProp);
                            break;
                        }
                        temp -= bagProp.number;
                        this.propV.removeElement(bagProp);
                    }
                }
            }
        }
    }

    public void deletePropByPropID(int propID) {
        for (int i = 0; i < this.propV.size(); i++) {
            Object obj = this.propV.elementAt(i);
            Prop prop = (Prop) obj;
            if (prop.data.bID == propID) {
                prop.number--;
                if (prop.number <= 0) {
                    this.propV.removeElementAt(i);
                    return;
                }
                return;
            }
        }
    }

    public boolean addEquip(Equip equip) {
        if (equip == null) {
            return false;
        }
        this.equipV.addElement(equip);
        return true;
    }

    public void deleteEquip(Equip equip) {
        this.equipV.removeElement(equip);
    }

    public void deleteEquip(EquipData equipdata) {
        for (int i = 0; i < this.equipV.size(); i++) {
            Object obj = this.equipV.elementAt(i);
            if (obj instanceof Equip) {
                Equip equip = (Equip) obj;
                if (equip.data.bID == equipdata.bID) {
                    this.equipV.removeElementAt(i);
                    return;
                }
            }
        }
    }

    public void deleteItem(int index) {
        this.equipV.removeElementAt(index);
    }

    public Object getItem(Object obj) {
        for (int i = 0; i < this.equipV.size(); i++) {
            Object item = this.equipV.elementAt(i);
            if ((item instanceof Prop) && ((Prop) item).data.equals(obj)) {
                return item;
            }
            if ((item instanceof Equip) && ((Equip) item).data.equals(obj)) {
                return item;
            }
        }
        return null;
    }

    public static Bag readBagData(DataInputStream dis) {
        Bag bag = new Bag();
        try {
            bag.iMoney = dis.readInt();
            int size = dis.readInt();
            for (int i = 0; i < size; i++) {
                byte id = dis.readByte();
                byte num = dis.readByte();
                Prop prop = new Prop(GameCanvas.propDatas[id], num);
                bag.propV.addElement(prop);
            }
            int size2 = dis.readInt();
            for (int i2 = 0; i2 < size2; i2++) {
                Equip equip = Equip.readEquipData(dis);
                bag.equipV.addElement(equip);
            }
        } catch (Exception e) {
            e.printStackTrace();
        }
        return bag;
    }

    public void saveBagData(DataOutputStream dos) {
        try {
            dos.writeInt(this.iMoney);
            dos.writeInt(this.propV.size());
            for (int i = 0; i < this.propV.size(); i++) {
                Prop prop = (Prop) this.propV.elementAt(i);
                dos.writeByte(prop.data.bID);
                dos.writeByte((byte) prop.number);
            }
            dos.writeInt(this.equipV.size());
            for (int i2 = 0; i2 < this.equipV.size(); i2++) {
                Equip equip = (Equip) this.equipV.elementAt(i2);
                equip.saveEquipData(dos);
            }
        } catch (Exception e) {
            e.printStackTrace();
        }
    }
}
