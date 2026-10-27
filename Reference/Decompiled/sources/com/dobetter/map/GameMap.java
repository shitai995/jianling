package com.dobetter.map;

import android.graphics.Bitmap;
import com.dobetter.common.GZIP;
import com.dobetter.common.Graphics;
import com.dobetter.common.Tool;
import java.io.ByteArrayInputStream;
import java.io.DataInputStream;
import java.io.IOException;
import java.lang.reflect.Array;

/* JADX INFO: loaded from: classes.dex */
public class GameMap {
    public static final byte IMAGE_MAP = 1;
    public static final byte LAYER_0 = 0;
    public static final byte LAYER_1 = 1;
    public static final byte LAYER_2 = 2;
    public static final byte TILE_MAP = 0;
    private byte[][] bCellImageID;
    private byte[][] bCellLandformID;
    private byte[][] bLayer0ImageID;
    private byte[] bLayer0PartData;
    private short[] bLayer0PartPosX;
    private short[] bLayer0PartPosY;
    private byte[][] bLayer0TileID;
    private byte[][] bLayer0TileTransformType;
    private byte[][] bLayer1ImageID;
    private byte[] bLayer1PartData;
    private short[] bLayer1PartPosX;
    private short[] bLayer1PartPosY;
    private byte[][] bLayer1TileID;
    private byte[][] bLayer1TileTransformType;
    private byte[][] bLayer2ImageID;
    private byte[] bLayer2PartData;
    private short[] bLayer2PartPosX;
    private short[] bLayer2PartPosY;
    private byte[][] bLayer2TileID;
    private byte[][] bLayer2TileTransformType;
    public int iBGColor;
    private boolean[][] isLayer0Flag;
    private boolean[][] isLayer1Flag;
    private boolean[][] isLayer2Flag;
    Bitmap[] mapImages;
    public byte mapType;
    public short sCellHeight;
    public short sCellWidth;
    public short sCellXCount;
    public short sCellYCount;
    public short sMapHeight;
    public short sMapWidth;
    private short sViewHeight;
    private short sViewMapX;
    private short sViewMapY;
    private short sViewWidth;

    public GameMap(int vWidth, int vHeight) {
        this.sViewWidth = (short) vWidth;
        this.sViewHeight = (short) vHeight;
    }

    public void loadData(String mFile) {
        String mFile2 = "map" + mFile;
        System.out.println("Load Map = " + mFile2);
        try {
            byte[] data = GZIP.inflate(Tool.getBytesFromInput(Tool.getResourceAsStream(mFile2)));
            loadData(data);
        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    public void loadData(byte[] data) {
        ByteArrayInputStream bin = new ByteArrayInputStream(data);
        DataInputStream din = new DataInputStream(bin);
        try {
            this.mapType = din.readByte();
            int imageCount = din.readByte();
            this.mapImages = new Bitmap[imageCount];
            for (int i = 0; i < imageCount; i++) {
                String resStr = din.readUTF();
                String pathStr = resStr.substring(0, resStr.indexOf(46) + 2);
                try {
                    this.mapImages[i] = Tool.loadPImage("map" + pathStr);
                } catch (Exception e) {
                    e.printStackTrace();
                    System.out.println("Can't find map" + pathStr);
                }
            }
            this.sCellWidth = din.readShort();
            this.sCellHeight = din.readShort();
            this.sCellXCount = din.readShort();
            this.sCellYCount = din.readShort();
            this.iBGColor = din.readInt();
            if (this.mapType == 0) {
                this.bCellImageID = (byte[][]) Array.newInstance((Class<?>) Byte.TYPE, this.sCellXCount, this.sCellYCount);
                this.bLayer0TileID = (byte[][]) Array.newInstance((Class<?>) Byte.TYPE, this.sCellXCount, this.sCellYCount);
                this.bLayer1TileID = (byte[][]) Array.newInstance((Class<?>) Byte.TYPE, this.sCellXCount, this.sCellYCount);
                this.bLayer2TileID = (byte[][]) Array.newInstance((Class<?>) Byte.TYPE, this.sCellXCount, this.sCellYCount);
                byte[][] sCellData = (byte[][]) Array.newInstance((Class<?>) Byte.TYPE, this.sCellXCount, this.sCellYCount);
                this.bLayer0ImageID = (byte[][]) Array.newInstance((Class<?>) Byte.TYPE, this.sCellXCount, this.sCellYCount);
                this.bLayer1ImageID = (byte[][]) Array.newInstance((Class<?>) Byte.TYPE, this.sCellXCount, this.sCellYCount);
                this.bLayer2ImageID = (byte[][]) Array.newInstance((Class<?>) Byte.TYPE, this.sCellXCount, this.sCellYCount);
                this.bLayer0TileTransformType = (byte[][]) Array.newInstance((Class<?>) Byte.TYPE, this.sCellXCount, this.sCellYCount);
                this.bLayer1TileTransformType = (byte[][]) Array.newInstance((Class<?>) Byte.TYPE, this.sCellXCount, this.sCellYCount);
                this.bLayer2TileTransformType = (byte[][]) Array.newInstance((Class<?>) Byte.TYPE, this.sCellXCount, this.sCellYCount);
                this.isLayer0Flag = (boolean[][]) Array.newInstance((Class<?>) Boolean.TYPE, this.sCellXCount, this.sCellYCount);
                this.isLayer1Flag = (boolean[][]) Array.newInstance((Class<?>) Boolean.TYPE, this.sCellXCount, this.sCellYCount);
                this.isLayer2Flag = (boolean[][]) Array.newInstance((Class<?>) Boolean.TYPE, this.sCellXCount, this.sCellYCount);
                this.bCellLandformID = (byte[][]) Array.newInstance((Class<?>) Byte.TYPE, this.sCellXCount, this.sCellYCount);
                for (int i2 = 0; i2 < this.sCellXCount; i2++) {
                    for (int j = 0; j < this.sCellYCount; j++) {
                        this.bCellImageID[i2][j] = din.readByte();
                        this.bLayer0ImageID[i2][j] = (byte) (this.bCellImageID[i2][j] & 15);
                        this.bLayer1ImageID[i2][j] = (byte) ((this.bCellImageID[i2][j] & 255) >> 4);
                        this.bLayer2ImageID[i2][j] = din.readByte();
                        this.bLayer0TileID[i2][j] = din.readByte();
                        this.bLayer1TileID[i2][j] = din.readByte();
                        this.bLayer2TileID[i2][j] = din.readByte();
                        sCellData[i2][j] = din.readByte();
                        byte temp = din.readByte();
                        this.bLayer0TileTransformType[i2][j] = (byte) getCellTransType(sCellData[i2][j], (byte) 0);
                        this.bLayer1TileTransformType[i2][j] = (byte) getCellTransType(sCellData[i2][j], (byte) 1);
                        this.bLayer2TileTransformType[i2][j] = (byte) ((temp & 7) >> 1);
                        this.isLayer0Flag[i2][j] = getCellFlag(sCellData[i2][j], (byte) 0);
                        this.isLayer1Flag[i2][j] = getCellFlag(sCellData[i2][j], (byte) 1);
                        this.isLayer2Flag[i2][j] = ((byte) (temp >> 3)) == 1;
                        this.bCellLandformID[i2][j] = din.readByte();
                    }
                }
            } else if (this.mapType == 1) {
                byte[][] sCellData2 = (byte[][]) Array.newInstance((Class<?>) Byte.TYPE, this.sCellXCount, this.sCellYCount);
                this.isLayer0Flag = (boolean[][]) Array.newInstance((Class<?>) Boolean.TYPE, this.sCellXCount, this.sCellYCount);
                this.isLayer1Flag = (boolean[][]) Array.newInstance((Class<?>) Boolean.TYPE, this.sCellXCount, this.sCellYCount);
                this.isLayer2Flag = (boolean[][]) Array.newInstance((Class<?>) Boolean.TYPE, this.sCellXCount, this.sCellYCount);
                this.bCellLandformID = (byte[][]) Array.newInstance((Class<?>) Byte.TYPE, this.sCellXCount, this.sCellYCount);
                for (int i3 = 0; i3 < this.sCellXCount; i3++) {
                    for (int j2 = 0; j2 < this.sCellYCount; j2++) {
                        sCellData2[i3][j2] = din.readByte();
                        this.bCellLandformID[i3][j2] = din.readByte();
                        this.isLayer0Flag[i3][j2] = ((byte) (sCellData2[i3][j2] & 1)) == 1;
                        this.isLayer1Flag[i3][j2] = ((byte) (sCellData2[i3][j2] >> 1)) == 1;
                        this.isLayer2Flag[i3][j2] = ((byte) (sCellData2[i3][j2] >> 2)) == 1;
                    }
                }
                int partN = din.readByte() & 255;
                this.bLayer0PartData = new byte[partN];
                this.bLayer0PartPosX = new short[partN];
                this.bLayer0PartPosY = new short[partN];
                for (int i4 = 0; i4 < partN; i4++) {
                    this.bLayer0PartData[i4] = din.readByte();
                    this.bLayer0PartPosX[i4] = din.readShort();
                    this.bLayer0PartPosY[i4] = din.readShort();
                }
                int partN2 = din.readByte() & 255;
                this.bLayer1PartData = new byte[partN2];
                this.bLayer1PartPosX = new short[partN2];
                this.bLayer1PartPosY = new short[partN2];
                for (int i5 = 0; i5 < partN2; i5++) {
                    this.bLayer1PartData[i5] = din.readByte();
                    this.bLayer1PartPosX[i5] = din.readShort();
                    this.bLayer1PartPosY[i5] = din.readShort();
                }
                int partN3 = din.readByte() & 255;
                this.bLayer2PartData = new byte[partN3];
                this.bLayer2PartPosX = new short[partN3];
                this.bLayer2PartPosY = new short[partN3];
                for (int i6 = 0; i6 < partN3; i6++) {
                    this.bLayer2PartData[i6] = din.readByte();
                    this.bLayer2PartPosX[i6] = din.readShort();
                    this.bLayer2PartPosY[i6] = din.readShort();
                }
            }
            this.sMapWidth = (short) (this.sCellWidth * this.sCellXCount);
            this.sMapHeight = (short) (this.sCellHeight * this.sCellYCount);
            bin.close();
            din.close();
        } catch (IOException e2) {
            e2.printStackTrace();
        }
    }

    public void setViewPosInMap(int px, int py) {
        this.sViewMapX = (short) px;
        this.sViewMapY = (short) py;
    }

    public short getTileX(short x) {
        return (short) (x / this.sCellWidth);
    }

    public short getTileY(short y) {
        return (short) (y / this.sCellHeight);
    }

    private int getPartImageID(byte partData) {
        return (partData & 255) >> 4;
    }

    private int getPartTransType(byte partData) {
        return partData & 15;
    }

    public void drawMapBackground(Graphics g) {
        g.setColor(0);
        g.fillRect(0, 0, this.sViewWidth, this.sViewHeight);
    }

    public void drawLayer0(Graphics g, int dx, int dy) {
        g.setClip(dx, dy, this.sViewWidth, this.sViewHeight);
        int curViewMapX = this.sViewMapX;
        int curViewMapY = this.sViewMapY;
        if (this.mapType == 1) {
            for (int i = 0; i < this.bLayer0PartData.length; i++) {
                Bitmap img = this.mapImages[getPartImageID(this.bLayer0PartData[i])];
                if (Tool.isRectIntersected(curViewMapX, curViewMapY, this.sViewWidth, this.sViewHeight, this.bLayer0PartPosX[i], this.bLayer0PartPosY[i], img.getWidth(), img.getHeight())) {
                    Tool.drawImage(g, img, (this.bLayer0PartPosX[i] - curViewMapX) + dx, (this.bLayer0PartPosY[i] - curViewMapY) + dy, getPartTransType(this.bLayer0PartData[i]));
                }
            }
        } else {
            drawMapNoBuffer(g, dx, dy, (byte) 0);
        }
        g.restore();
    }

    public void drawLayer1(Graphics g, int dx, int dy) {
        if (this.mapType == 1) {
            for (int i = 0; i < this.bLayer1PartData.length; i++) {
                Bitmap img = this.mapImages[getPartImageID(this.bLayer1PartData[i])];
                if (Tool.isRectIntersected(this.sViewMapX, this.sViewMapY, this.sViewWidth, this.sViewHeight, this.bLayer1PartPosX[i], this.bLayer1PartPosY[i], img.getWidth(), img.getHeight())) {
                    Tool.drawImage(g, img, (this.bLayer1PartPosX[i] - this.sViewMapX) + dx, (this.bLayer1PartPosY[i] - this.sViewMapY) + dy, getPartTransType(this.bLayer1PartData[i]));
                }
            }
            return;
        }
        drawMapNoBuffer(g, dx, dy, (byte) 1);
    }

    public void drawLayer2(Graphics g, int dx, int dy) {
        if (this.mapType == 1) {
            for (int i = 0; i < this.bLayer2PartData.length; i++) {
                Bitmap img = this.mapImages[getPartImageID(this.bLayer2PartData[i])];
                if (Tool.isRectIntersected(this.sViewMapX, this.sViewMapY, this.sViewWidth, this.sViewHeight, this.bLayer2PartPosX[i], this.bLayer2PartPosY[i], img.getWidth(), img.getHeight())) {
                    Tool.drawImage(g, img, (this.bLayer2PartPosX[i] - this.sViewMapX) + dx, (this.bLayer2PartPosY[i] - this.sViewMapY) + dy, getPartTransType(this.bLayer2PartData[i]));
                }
            }
            return;
        }
        drawMapNoBuffer(g, dx, dy, (byte) 2);
    }

    public void drawMapNoBuffer(Graphics g, int dx, int dy, byte layerIndex) {
        int startX = getTileX(this.sViewMapX);
        int startY = getTileY(this.sViewMapY);
        if (startX < 0) {
            startX = 0;
        }
        if (startY < 0) {
            startY = 0;
        }
        int endX = Math.min((int) this.sCellXCount, getTileX((short) (this.sViewMapX + this.sViewWidth)) + 1);
        int endY = Math.min((int) this.sCellYCount, getTileY((short) (this.sViewMapY + this.sViewHeight)) + 1);
        for (int i = startX; i < endX; i++) {
            for (int j = startY; j < endY; j++) {
                if (i >= 0 && j >= 0 && i < this.sCellXCount && j < this.sCellYCount) {
                    int x = ((this.sCellWidth * i) - this.sViewMapX) + dx;
                    int y = ((this.sCellHeight * j) - this.sViewMapY) + dy;
                    if (layerIndex == 1) {
                        int id = (this.bLayer1TileID[i][j] & 255) - 1;
                        if (id >= 0 && this.isLayer1Flag[i][j]) {
                            Tool.drawTile(g, this.mapImages[this.bLayer1ImageID[i][j]], id, this.sCellWidth, this.sCellHeight, this.bLayer1TileTransformType[i][j], x, y);
                        }
                    } else if (layerIndex == 2) {
                        int id2 = (this.bLayer2TileID[i][j] & 255) - 1;
                        if (id2 >= 0 && this.isLayer2Flag[i][j]) {
                            Tool.drawTile(g, this.mapImages[this.bLayer2ImageID[i][j]], id2, this.sCellWidth, this.sCellHeight, this.bLayer2TileTransformType[i][j], x, y);
                        }
                    } else {
                        int id3 = (this.bLayer0TileID[i][j] & 255) - 1;
                        if (id3 >= 0) {
                            Tool.drawTile(g, this.mapImages[this.bLayer0ImageID[i][j]], id3, this.sCellWidth, this.sCellHeight, this.bLayer0TileTransformType[i][j], x, y);
                        }
                        int id4 = (this.bLayer1TileID[i][j] & 255) - 1;
                        if (id4 >= 0 && !this.isLayer1Flag[i][j]) {
                            Tool.drawTile(g, this.mapImages[this.bLayer1ImageID[i][j]], id4, this.sCellWidth, this.sCellHeight, this.bLayer1TileTransformType[i][j], x, y);
                        }
                        int id5 = (this.bLayer2TileID[i][j] & 255) - 1;
                        if (id5 >= 0 && !this.isLayer2Flag[i][j]) {
                            Tool.drawTile(g, this.mapImages[this.bLayer2ImageID[i][j]], id5, this.sCellWidth, this.sCellHeight, this.bLayer2TileTransformType[i][j], x, y);
                        }
                    }
                }
            }
        }
    }

    private boolean getCellFlag(short data, byte layer) {
        if (layer == 0) {
            return (((data & 255) >> 3) & 1) == 1;
        }
        return (((data & 255) >> 7) & 1) == 1;
    }

    private int getCellTransType(short data, byte layer) {
        return layer == 0 ? data & 255 & 7 : ((data & 255) >> 4) & 7;
    }

    public int getCellLandform(int x, int y) {
        return this.bCellLandformID[x][y];
    }

    public boolean canPointPass(int mapX, int mapY) {
        return canBlockPass(mapX / this.sCellWidth, mapY / this.sCellHeight);
    }

    public boolean canBlockPass(int x, int y) {
        return !this.isLayer0Flag[x][y];
    }

    public void setMapFlag(int x, int y, int layerIndex, boolean isSetFlag) {
        if (layerIndex == 0) {
            this.isLayer0Flag[x][y] = isSetFlag;
        } else if (layerIndex == 0) {
            this.isLayer1Flag[x][y] = isSetFlag;
        } else {
            this.isLayer2Flag[x][y] = isSetFlag;
        }
    }

    public void setMapCell(int layer, int mx, int my, int realid, int type) {
        if (layer == 0) {
            this.bLayer0TileID[mx][my] = (byte) realid;
            this.bLayer0TileTransformType[mx][my] = (byte) type;
        } else if (layer == 1) {
            this.bLayer1TileID[mx][my] = (byte) realid;
            this.bLayer1TileTransformType[mx][my] = (byte) type;
        } else {
            this.bLayer2TileID[mx][my] = (byte) realid;
            this.bLayer2TileTransformType[mx][my] = (byte) type;
        }
    }

    public void closeMap() {
        for (int i = 0; i < this.mapImages.length; i++) {
            if (this.mapImages[i] != null) {
                this.mapImages[i].recycle();
                this.mapImages[i] = null;
            }
        }
        this.mapImages = null;
    }
}
