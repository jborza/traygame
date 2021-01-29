using System;using System.Collections.Generic;using System.Drawing;using System.Runtime.InteropServices;using System.Windows.Forms;using SD=System.Drawing;using P=System.Drawing.Point;using B=System.Drawing.Brushes;using static System.Math;

class A:ApplicationContext,IMessageFilter{
[STAThread]
static void Main(){
Application.Run(new A());}
[DllImport("user32.dll")]
static extern bool RegisterHotKey(IntPtr h,int i,int f,int v);
NotifyIcon I;
Timer Q;
int L=-1,R=1,U=3,D=4,_=5,W=16,H=14,Z=15,i,k,x,y;
int m,t=0,dx=1,dy,d=5,X,Y,G,bx,by;
Random r=new Random();
int[][]b;
List<P>s;
P f;
Font F;

public A(){
I=new NotifyIcon(){Visible=true};
Action<int,Keys>E=(k,K)=>RegisterHotKey((IntPtr)0,k,0,(int)K);
E(L,Keys.Left);E(R,Keys.Right);E(U,Keys.Up);E(D,Keys.Down);E(_,Keys.Space);
Application.AddMessageFilter(this);
m=9;
var c=new SD.Text.PrivateFontCollection();
c.AddFontFile("f.ttf");
F=new Font(c.Families[0],5,GraphicsUnit.Pixel);
Q=new Timer(){Interval=W};
Q.Tick+=(e,a)=>T();
Q.Start();
}

void u()
{
b=new int[H][];
for(y=0;y<H;y++)b[y]=y<6?c():new int[W];
s=new List<P>();
if(m==1){
f=new P(8,8);
for(i=4;i>=0;i--)s.Add(new P(i,4));}
if(m==3){f=new P(9,Z);X=5;
for(y=3;y<7;y++)
for(x=3;x<13;x++){
s.Add(new P(x,y));
}}
if(m==2){Y=1;X=1;z();}
dx=1;dy=0;
bx=1;by=L;
}

void q(){Console.Beep(2088,50);}

void T(){
t++;
if(m==0){
if(t%Z==0){
for(y=H-1;y>0;y--)
b[y]=b[y-1];
b[0]=c();
}
for(y=H-1;y>=0;y--)
if(b[y][X+2]>0){
b[y][X+2]=0;
break;
}
}
if(m==1){
if(t%d==0){
s.RemoveAt(s.Count-1);
var D=s[0]+new Size(dx,dy);s.Insert(0,D);
if(D==f){s.Insert(0,s[0]+new Size(dx,dy));f=new P(r.Next(W),r.Next(W));q();}
var h=s[0];
if(h.X>W||h.X<0||h.Y<0||h.Y>W)m=8;
}
}
if(m==2){
if(t%d==0){        
for(i=0;i<3;i++){b[0][i]--;if(b[0][i]<-3)z();}
if(b[0][X]<5)m=8;
}  
}
if(m==3)
{
if(t%d==0){
foreach (P p in s)
{
if(Abs(p.X-f.X)==1&&p.Y==f.Y){bx*=L;s.Remove(p);q();break;}
if(Abs(p.Y-f.Y)==1&&p.X==f.X){by*=L;s.Remove(p);q();break;}
}
f.X+=bx;f.Y+=by;
if(f.Y==14&&f.X>=X&&f.X<X+5)by*=L;
if(f.Y<1)by*=L;
if(f.X>13||f.X<2)bx*=L;
if(f.Y>Z)m=8;
}}
g();
}

void z()
{
k=r.Next(6)+1;b[0][0]=(k&1)>0?H:99;b[0][1]=(k&2)>0?H:99;b[0][2]=(k&4)>0?H:99;
}

int[] c(){
var I=new int[W];
for(x=0;x<W;x++)if(r.Next(10)>7)
I[x]=1;
return I;
}

void g(){
var bmp=new Bitmap(W,W);
var g=Graphics.FromImage(bmp);
Pen BP=new Pen(Color.White){DashStyle=SD.Drawing2D.DashStyle.Dash,DashOffset=t};
g.Clear(Color.DarkBlue);
Action<string,int> S=(s,i)=>g.DrawString(s,F,B.White,0,i);
Action<Brush,int,int> FR=(b,x,y)=>g.FillRectangle(b,x,y,1,1);
Action<Pen,int,int,int,int> J=g.DrawLine;
if(m==9){
S("PLAY",1);
S(new string[]{"BRK","SNK","CRS","ARK"}[G]+d,8);
}
if(m==8){S("GAME",1);S("OVER",8);}
if(m==0){
J(Pens.Magenta,X,Z,X+5,Z);
for(y=0;y<H;y++)
for(x=0;x<W;x++)
if(b[y][x]>0)
FR(B.Cyan,x,y);
J(BP,X+2,Z,X+2,0);
}
if(m==1){foreach(P p in s)FR(B.Yellow,p.X,p.Y);FR(B.Red,f.X,f.Y);}
if(m==2) {
g.FillRectangle(B.White,X*6,Y,3,4);
FR(B.Red,X*6,Y+2);
FR(B.Blue,X*6+2,Y+2);
for(x=0;x<3;x++)
g.FillRectangle(B.Tan,x*6,b[0][x],3,4);
J(BP,4,0,4,W);
J(BP,10,0,10,W);
}
if(m==3){
g.Clear(Color.White);
foreach(P p in s)FR(B.Blue,p.X,p.Y);
J(Pens.Green,X,Z,X+5,Z);
FR(B.Red,f.X,f.Y);
g.DrawRectangle(Pens.Blue,0.5f,L,Z,18);
}
I.Icon=Icon.FromHandle(bmp.GetHicon());
}

public bool PreFilterMessage(ref Message n){
if(n.Msg==786){
k=(int)n.WParam;
dx=0;dy=0;
if(k==U){dy=L;}if(k==D){dy=1;}if(k==L){dx=L;}if(k==R){dx=1;}
X=m==2?Max(0,Min(2,X+dx)):Max(-2,Min(W-3,X+dx));
if(m==9)d=Max(1,Min(d+dx,9));
G=Max(0,Min(G+dy,3));
if(m==9&&k==_){m=G;u();}
if(m==8&&k==_){m=9;}
}
return false;
}    
}