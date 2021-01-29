using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using SD = System.Drawing;
using P = System.Drawing.Point;
using B = System.Drawing.Brushes;
using static System.Math;
//static class W{
//[STAThread]
//static void Main(){
//Application.Run(new A());}
//}

class Game : ApplicationContext, IMessageFilter
{
    [DllImport("user32.dll")]
    static extern bool RegisterHotKey(IntPtr h, int i, int f, int v);
    NotifyIcon I;
    Timer Q;
    //_ = SPACE
    int L = -1, R = 1, U = 3, D = 4, _ = 5, W = 16, H = 14, Z = 15, i, j, k, x, y;
    int mode, t = 0, dx = 1, dy, spd = 5, X, Y, game_selection, bx, by;
    string[] games = { "BRK", "SNK", "CRS", "ARK" };
    int BRK = 0, SNK = 1, CRS = 2, ARK = 3, MENU = 9, OVR = 8;
    Random r = new Random();
    int[][] M;
    List<P> snake;
    P f;
    Font font;

    public Game()
    {
        I = new NotifyIcon()
        {
            ContextMenu = new ContextMenu(new MenuItem[]
        {new MenuItem("Exit",(s,e)=>Application.Exit())}),
            Visible = true
        };
        //register global hotkeys for controls
        Action<int, Keys> E = (k, K) => RegisterHotKey((IntPtr)0, k, 0, (int)K);
        E(L, Keys.Left);
        E(R, Keys.Right);
        E(U, Keys.Up);
        E(D, Keys.Down);
        E(_, Keys.Space);
        Application.AddMessageFilter(this);
        mode = MENU;
        //read the custom font for legible text at 16x16 pixels
        var c = new SD.Text.PrivateFontCollection();
        c.AddFontFile("f.ttf");
        font = new Font(c.Families[0], 5, GraphicsUnit.Pixel);
        Q = new Timer() { Interval = W };
        Q.Tick += (e, a) => T();
        Q.Start();
    }

    void Reset()
    {
        //reset all games
        GenLevel();
        snake = new List<P>();
        if (mode == SNK)
        {
            //generate initial snake and food
            f = new P(8, 8);
            for (i = 4; i >= 0; i--) snake.Add(new P(i, 4));
        }
        if (mode == ARK)
        {
            f = new P(9, 15); X = 5;
            //make level
            for (y = 3; y < 7; y++)
                for (x = 3; x < 13; x++)
                {
                    snake.Add(new P(x, y));
                }
        }
        if (mode == CRS) { Y = 1; X = 1; SpawnCars(); }
        dx = 1; dy = 0;
        bx = 1; by = -1;
    }

    void T()
    {
        t++;
        if (mode == BRK)
        {
            //move bricks down
            if (t % 15 == 0)
            {
                for (y = H - 1; y > 0; y--)
                    M[y] = M[y - 1];
                M[0] = MkR();
            }
            //shoot the cannon
            for (y = H - 1; y >= 0; y--)
                if (M[y][X + 2] > 0)
                {
                    M[y][X + 2] = 0;
                    break;
                }
        }
        if (mode == SNK)
        {
            if (t % spd == 0)
            {
                //remove the tail
                snake.RemoveAt(snake.Count - 1);
                //respawn it in front of the head in snake's direction
                var dst = snake[0] + new Size(dx, dy);
                snake.Insert(0, dst);
                //ate the food - spawn one more head
                if (dst == f)
                {
                    snake.Insert(0, snake[0] + new Size(dx, dy));
                    f = new P(r.Next(W), r.Next(W));
                }
                //head hit the wall, end
                var h = snake[0];
                if (h.X > W || h.X < 0 || h.Y < 0 || h.Y > W) mode = OVR;
            }
        }
        if (mode == CRS)
        {
            if (t % spd == 0)
            {
                //move cars downward, respawn cars if they leave off screen
                for (i = 0; i < 3; i++) { M[0][i]--; if (M[0][i] < -3) SpawnCars(); }
                //if a car in the same lane touches the player, game over
                if (M[0][X] < 5) mode = OVR;
            }
        }
        if (mode == ARK)
        {
            if (t % spd == 0)
            {
                foreach (P p in snake)
                {
                    //check if we have a neighbor left or right - if yes, invert x and destroy it
                    if (Abs(p.X - f.X) == 1 && p.Y == f.Y)
                    {
                        bx *= -1;
                        snake.Remove(p);
                        break;
                    }
                    //check if we have a neighbor above or below - if yes, invert y and destroy it
                    if (Abs(p.Y - f.Y) == 1 && p.X == f.X)
                    {
                        by *= -1;
                        snake.Remove(p);
                        break;
                    }
                }
                //update ball position - it's always going in 45 degrees
                f.X += bx;
                f.Y += by;
                //hit paddle?
                if (f.Y == 14 && f.X >= X && f.X < X + 5) by *= -1;
                //ball hit the top - invert dy
                if (f.Y < 1) by *= -1;
                //ball hit the right wall or left wall - invert dx; corners - dx and dy
                if (f.X > 13 || f.X < 2) bx *= -1;
                //ball hit the bottom - game over
                if (f.Y > 15) mode = OVR;
            }
        }
        Draw();
    }

    void SpawnCars()
    {
        //produce pattern 001,010,011,100,101,110
        k = r.Next(6) + 1;
        //spawn enemy car close, if its bit was 1, or really far away
        M[0][0] = (k & 1) > 0 ? H : 99;
        M[0][1] = (k & 2) > 0 ? H : 99;
        M[0][2] = (k & 4) > 0 ? H : 99;
    }

    int[] MkR()
    {
        var I = new int[W];
        for (int x = 0; x < W; x++)
            if (r.Next(10) > 7)
                I[x] = 1;
        return I;
    }

    void GenLevel()
    {
        M = new int[H][];
        for (int y = 0; y < H; y++) M[y] = y < 6 ? MkR() : new int[W];
    }

    void Draw()
    {
        var bmp = new Bitmap(W, W);
        var g = Graphics.FromImage(bmp);
        var ds = SD.Drawing2D.DashStyle.Dash;
        Pen BP = new Pen(Color.White) { DashStyle = ds, DashOffset = t };
        g.Clear(Color.DarkBlue);
        Action<string, int> S = (s, i) => g.DrawString(s, font, B.White, 0, i);
        Action<Brush, int, int> FR = (b, x, y) => g.FillRectangle(b, x, y, 1, 1);
        if (mode == MENU)
        {
            //mode selector
            S("PLAY", 1);
            string game = games[game_selection];
            S(game + spd, 8);
        }
        if (mode == OVR)
        {
            S("GAME", 1);
            S("OVER", 8);
        }
        if (mode == BRK)
        {
            //draw player
            g.Clear(Color.Black);
            g.DrawLine(Pens.Magenta, X, 15, X + 5, 15);
            //draw blocks
            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                    if (M[y][x] > 0)
                        FR(B.Cyan, x, y);
            //draw bullets
            g.DrawLine(BP, X + 2, 15, X + 2, 0);
        }
        if (mode == SNK)
        {
            foreach (P p in snake)
                FR(B.Yellow, p.X, p.Y);
            FR(B.Red, f.X, f.Y);
        }
        if (mode == CRS)
        {
            //draw the player, we can recycle X,Y
            g.FillRectangle(B.White, X * 6, Y, 3, 4);
            FR(B.Red, X * 6, Y + 2);
            FR(B.Blue, X * 6 + 2, Y + 2);
            //draw enemies - they move from M
            // we keep the enemies in M
            for (x = 0; x < 3; x++)
                g.FillRectangle(B.Tan, x * 6, M[0][x], 3, 4);
            //draw the lines
            g.DrawLine(BP, 4, 0, 4, 16);
            g.DrawLine(BP, 10, 0, 10, 16);
        }
        if (mode == ARK)
        {
            g.Clear(Color.White);
            //blocks
            foreach (P p in snake)
                FR(B.RoyalBlue, p.X, p.Y);
            //paddle
            g.DrawLine(Pens.Green, X, 15, X + 5, 15);
            //ball
            FR(B.Red, f.X, f.Y);
            //walls
            g.DrawRectangle(Pens.Blue, 0.5f, -1, 15, 18);
        }
        I.Icon = Icon.FromHandle(bmp.GetHicon());
    }

    public bool PreFilterMessage(ref Message m)
    {
        if (m.Msg == 786) Hotkey((int)m.WParam);
        return false;
    }

    void Hotkey(int k)
    {
        //create a direction vector from arrow keys
        dx = 0; dy = 0;
        if (k == U) { dy = -1; }
        if (k == D) { dy = 1; }
        if (k == L) { dx = -1; }
        if (k == R) { dx = 1; }
        //car-specific movement in 3 lanes vs the rest
        X = mode == CRS ? Max(0, Min(2, X + dx)) : Max(-2, Min(W - 3, X + dx));
        //arrow keys change the speed in the menu
        if (mode == MENU) spd = Max(1, Min(spd + dx, 9));
        game_selection = Max(0, Min(game_selection + dy, 3));
        if (mode == MENU && k == _) { mode = game_selection; Reset(); }
        if (mode == OVR && k == _) { mode = MENU; }
    }
}