using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace NESLib
{
    public class Tile
    {
        
        //8x8 tile used in backgrounds and sprites
        private const int TILEX = 8;
        private const int TILEY = 8;
        private int[,] pixelArray= new int[TILEX,TILEY];

        //+ create a tile via [8,8] pixel array with values ranged 0-3
        public Tile(int[,] arr)
        {
            this.pixelArray = arr;
        }

        //+ create a tile via 16 byte representation as in NES .chr files
        public Tile(byte[] pix)
        {
            for(int i =0; i< pix.Length/2;i++)
            {
                uint arithmShift=(uint)Convert.ToInt32("10000000",2);
                for(int j=0;j<8;j++){
                    if( (pix[i] & arithmShift)!=0)
                        this.pixelArray[i,j] +=1;
                    if( (pix[i+pix.Length/2] & arithmShift)!=0)
                        this.pixelArray[i,j] +=2;
                    arithmShift= arithmShift >> 1;
                }
            }
        }

        //+ set a particular point value
        public void setPoint(int x,int y,int val){
            this.pixelArray[x,y]=val;
        }
    }

    public class PPUs
    {
        private Tile[,] ppumap1= new Tile[16,16];
        private Tile[,] ppumap2 = new Tile[16, 16];
        //ppu tiles: 16*16 * 2
        const int PPUTILES = 512;
        const int PPUX=16;
        const int PPUY = 16;

        public PPUs(List<Tile> tiles)
        {
            if (tiles.Count == PPUTILES)
            {
                System.Diagnostics.Debug.Write("test");
                for(int i = 0; i <PPUX;i++){
                    for (int j = 0; j < PPUY; j++){
                        this.ppumap1[i, j] = tiles[i * PPUY + j];
                        this.ppumap2[i, j] = tiles[i * PPUY + j + PPUX * PPUY];
                    }
                }
            }
        }

        public PPUs(byte[] raw)
        {
            //8192 = 16 bytes per sprite, 16x16 sprite pages, 2 pages
            int size = 8192/2; // the /2 to signify doing 2 pages at once
            if (raw.Length == size*2)
            {
                //8192/16=512
                for (int i = 0; i < size/16; i++)
                {
                    int x = i%16;
                    int y = i/16;
                    byte[] temp= new byte[16];
                    Array.Copy(raw,(byte)i*16,temp,0,16);
                    this.ppumap1[y,x]=new Tile(temp);
                    Array.Copy(raw,(byte)i*16+16*16,temp,0,16);
                    this.ppumap2[y,x]=new Tile(temp);
                }
            }
        }
    }

    public class SpriteObj
    {
        private int objX;
        private int objY;
        private Tile[,] sprites;
        private int[,] palette;

        public SpriteObj(int x, int y, Tile[,] sprites, int[,]colors)
        {
            this.objX = x;
            this.objY = y;
            this.sprites = sprites;
            this.palette = colors;
        }

    }

    public class BGObj 
    {
        private int objX;
        private int objY;
        private Tile[,] sprites;
        private int[,] palette;

        public BGObj(int x, int y, Tile[,] sprites, int[,] colors)
        {
            this.objX = x;
            this.objY = y;
            this.sprites = sprites;
            this.palette = colors;
        }
    }

    public class Level
    {
        private List<BGObj> objectlist;
        private List<int[]> objectloclist;
        private List<int[]> triggers;
    }
}
