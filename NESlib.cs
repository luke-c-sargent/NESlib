using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace NESlib
{

    public static class GlobalVars{
		public const byte BLACK = 15;
        public static readonly List<byte> blacks = new List<byte> { 13, 14, 29, 30, 31, 46, 47, 62, 63 };//to 15
        public static readonly Dictionary<byte, string> nesHexColorDict = new Dictionary<byte, string>
		  {
     {0,"7C7C7C"},{1,"0000FC"},{2,"0000BC"},{3,"4428BC"},{4,"940084"},{5,"A80020"},
     {6,"A81000"},{7,"881400"},{8,"503000"},{9,"007800"},{10,"006800"},{11,"005800"},
     {12,"004058"},{13,"000000"},{14,"000000"},{15,"000000"},{16,"BCBCBC"},{17,"0078F8"},
     {18,"0058F8"},{19,"6844FC"},{20,"D800CC"},{21,"E40058"},{22,"F83800"},{23,"E45C10"},
     {24,"AC7C00"},{25,"00B800"},{26,"00A800"},{27,"00A844"},{28,"008888"},{29,"000000"},
     {30,"000000"},{31,"000000"},{32,"F8F8F8"},{33,"3CBCFC"},{34,"6888FC"},{35,"9878F8"},
     {36,"F878F8"},{37,"F85898"},{38,"F87858"},{39,"FCA044"},{40,"F8B800"},{41,"B8F818"},
     {42,"58D854"},{43,"58F898"},{44,"00E8D8"},{45,"787878"},{46,"000000"},{47,"000000"},
     {48,"FCFCFC"},{49,"A4E4FC"},{50,"B8B8F8"},{51,"D8B8F8"},{52,"F8B8F8"},{53,"F8A4C0"},
     {54,"F0D0B0"},{55,"FCE0A8"},{56,"F8D878"},{57,"D8F878"},{58,"B8F8B8"},{59,"B8F8D8"},
     {60,"00FCFC"},{61,"F8D8F8"},{62,"000000"},{63,"000000"}		  
		  };

	}
	
	
	public class Palette{
		//16 bytes- NES has 2 palettes: bg and sprite\
		//color sets of 4;
		 private byte[] values;
		 
		 public Palette(byte[] bytesin){
			this.values = bytesin;
		 }

		 public NEScolor byteToColor(int index){
             return new NEScolor(this.values[index]);
		 }
	}
	
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

        //- get a particular point value
        public int getPoint(int x, int y)
        {
            return this.pixelArray[x, y];
        }

        //+ set a particular point value
        public void setPoint(int x,int y,int val){
            this.pixelArray[x,y]=val;
        }
		
		//returns 16-byte chunk of raw binary data
		public byte[] getRaw(){
			byte[] result = new byte[16];
			for(int i = 0;i<8;i++){
			    uint arithmShift=(uint)Convert.ToInt32("10000000",2);
				for(int j = 0;j<8;j++){
					switch(this.pixelArray[i,j]){
					  case 1://first 8 bytes
						result[i] = (byte)((uint)result[i] ^ arithmShift);
					    break;
					  case 2://second 8 bytes
					    result[i+8] = (byte)((uint)result[i+8] ^ arithmShift);
					    break;
					  case 3://first 8 bytes and second 8 bytes
						result[i] = (byte)((uint)result[i] ^ arithmShift);
						result[i+8] = (byte)((uint)result[i+8] ^ arithmShift);
						break;
					  default:
					    break;
					}
				}
			}
			return result;
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
		const int BYTESPERTILE= 16;

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
		
		public CHRfile toCHR(){
            byte[] temp = new byte[(PPUTILES * BYTESPERTILE)];
			for(int i = 0;i<PPUY;i++)
			{
				for(int j=0;j<PPUX;j++)
				{
					(ppumap1[i,j].getRaw()).CopyTo(temp,(i*8 + j)*16);
                    (ppumap1[i, j].getRaw()).CopyTo(temp, (i * 8 + j) * 16);

                    (ppumap2[i, j].getRaw()).CopyTo(temp, (256 + (i * 8 + j) * 16));
                    (ppumap2[i, j].getRaw()).CopyTo(temp, (256 + (i * 8 + j) * 16));
				}
			}
            return new CHRfile(temp);
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

	public class CHRfile
	{
		private byte[] raw;
		
		//fill in file read stuff;
		public CHRfile(byte[] bytesin){
			this.raw = bytesin;
		}
		
		public PPUs toPPUs(){
			return new PPUs(this.raw);
		}
		
		public byte[] getBytes(){
			return this.raw;
		}
	}
	
	//if color not in palette, add 0x0F=15
	public class NEScolor
	{
        private byte color;
		
		public NEScolor(byte val){
			if(GlobalVars.blacks.Contains(val))
				this.color=GlobalVars.BLACK;
			else
				this.color = val;
		}

        public string toHex()
        {
            string result = "";
            GlobalVars.nesHexColorDict.TryGetValue(this.color, out result);
            return result;
        }
	}
}