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
    int left = -1, right = 1, up = 3, down = 4, _ = 5, width = 16, height = 14, Z = 15, i, j, k, x, y;
    int mode, t = 0, dx = 1, dy, speed = 5, X, Y, game_selection, bx, by;
    string[] games = { "BRK", "SNK", "CRS", "ARK" };
    int BRK = 0, SNK = 1, CRS = 2, ARK = 3, MENU = 9, OVR = 8;
    Random r = new Random();
    int[][] blocks;
    List<P> snake;
    P ball;
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
        E(left, Keys.Left);
        E(right, Keys.Right);
        E(up, Keys.Up);
        E(down, Keys.Down);
        E(_, Keys.Space);
        Application.AddMessageFilter(this);
        mode = MENU;
        //read the custom font for legible text at 16x16 pixels
        var c = new SD.Text.PrivateFontCollection();
        c.AddFontFile("f.ttf");
        font = new Font(c.Families[0], 5, GraphicsUnit.Pixel);
        Q = new Timer() { Interval = width };
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
            ball = new P(8, 8);
            for (i = 4; i >= 0; i--) snake.Add(new P(i, 4));
        }
        if (mode == ARK)
        {
            ball = new P(9, 15); X = 5;
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
                for (y = height - 1; y > 0; y--)
                    blocks[y] = blocks[y - 1];
                blocks[0] = MkR();
            }
            //shoot the cannon
            for (y = height - 1; y >= 0; y--)
                if (blocks[y][X + 2] > 0)
                {
                    blocks[y][X + 2] = 0;
                    break;
                }
        }
        if (mode == SNK)
        {
            if (t % speed == 0)
            {
                //remove the tail
                snake.RemoveAt(snake.Count - 1);
                //respawn it in front of the head in snake's direction
                var dst = snake[0] + new Size(dx, dy);
                snake.Insert(0, dst);
                //ate the food - spawn one more head
                if (dst == ball)
                {
                    snake.Insert(0, snake[0] + new Size(dx, dy));
                    ball = new P(r.Next(width), r.Next(width));
                }
                //head hit the wall, end
                var h = snake[0];
                if (h.X > width || h.X < 0 || h.Y < 0 || h.Y > width) mode = OVR;
            }
        }
        if (mode == CRS)
        {
            if (t % speed == 0)
            {
                //move cars downward, respawn cars if they leave off screen
                for (i = 0; i < 3; i++) { blocks[0][i]--; if (blocks[0][i] < -3) SpawnCars(); }
                //if a car in the same lane touches the player, game over
                if (blocks[0][X] < 5) mode = OVR;
            }
        }
        if (mode == ARK)
        {
            if (t % speed == 0)
            {
                foreach (P p in snake)
                {
                    //check if we have a neighbor left or right - if yes, invert x and destroy it
                    if (Abs(p.X - ball.X) == 1 && p.Y == ball.Y)
                    {
                        bx *= -1;
                        snake.Remove(p);
                        break;
                    }
                    //check if we have a neighbor above or below - if yes, invert y and destroy it
                    if (Abs(p.Y - ball.Y) == 1 && p.X == ball.X)
                    {
                        by *= -1;
                        snake.Remove(p);
                        break;
                    }
                }
                //update ball position - it's always going in 45 degrees
                ball.X += bx;
                ball.Y += by;
                //hit paddle?
                if (ball.Y == 14 && ball.X >= X && ball.X < X + 5) by *= -1;
                //ball hit the top - invert dy
                if (ball.Y < 1) by *= -1;
                //ball hit the right wall or left wall - invert dx; corners - dx and dy
                if (ball.X > 13 || ball.X < 2) bx *= -1;
                //ball hit the bottom - game over
                if (ball.Y > 15) mode = OVR;
            }
        }
        Draw();
    }

    void SpawnCars()
    {
        //produce pattern 001,010,011,100,101,110
        k = r.Next(6) + 1;
        //spawn enemy car close, if its bit was 1, or really far away
        blocks[0][0] = (k & 1) > 0 ? height : 99;
        blocks[0][1] = (k & 2) > 0 ? height : 99;
        blocks[0][2] = (k & 4) > 0 ? height : 99;
    }

    int[] MkR()
    {
        var I = new int[width];
        for (int x = 0; x < width; x++)
            if (r.Next(10) > 7)
                I[x] = 1;
        return I;
    }

    void GenLevel()
    {
        blocks = new int[height][];
        for (int y = 0; y < height; y++) blocks[y] = y < 6 ? MkR() : new int[width];
    }

    void Draw()
    {
        var bmp = new Bitmap(width, width);
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
            S(game + speed, 8);
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
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    if (blocks[y][x] > 0)
                        FR(B.Cyan, x, y);
            //draw bullets
            g.DrawLine(BP, X + 2, 15, X + 2, 0);
        }
        if (mode == SNK)
        {
            foreach (P p in snake)
                FR(B.Yellow, p.X, p.Y);
            FR(B.Red, ball.X, ball.Y);
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
                g.FillRectangle(B.Tan, x * 6, blocks[0][x], 3, 4);
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
            FR(B.Red, ball.X, ball.Y);
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
        if (k == up) { dy = -1; }
        if (k == down) { dy = 1; }
        if (k == left) { dx = -1; }
        if (k == right) { dx = 1; }
        //car-specific movement in 3 lanes vs the rest
        X = mode == CRS ? Max(0, Min(2, X + dx)) : Max(-2, Min(width - 3, X + dx));
        //arrow keys change the speed in the menu
        if (mode == MENU) speed = Max(1, Min(speed + dx, 9));
        game_selection = Max(0, Min(game_selection + dy, 3));
        if (mode == MENU && k == _) { mode = game_selection; Reset(); }
        if (mode == OVR && k == _) { mode = MENU; }
    }
}